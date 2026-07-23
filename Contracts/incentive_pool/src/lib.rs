#![no_std]

use soroban_sdk::{contract, contractimpl, contracttype, symbol_short, Address, Env, IntoVal, String, Symbol, Vec};

#[contract]
pub struct IncentivePool;

#[contracttype]
#[derive(Clone)]
pub struct Incentive {
    pub id: u64,
    pub name: String,
    pub description: String,
    pub price: i128,
    pub code: String,
    pub provider: String,
    pub available: bool,
    pub redeemed_by: Option<Address>,
}

fn eco_token_sym(env: &Env) -> Symbol {
    Symbol::new(env, "eco_token")
}

fn inc_count_sym(env: &Env) -> Symbol {
    Symbol::new(env, "inc_count")
}

fn seed_incentives(env: &Env) {
    let count: u64 = env.storage().instance().get(&inc_count_sym(env)).unwrap_or(0);
    if count > 0 {
        return;
    }

    let incentives: [(u64, &str, &str, i128, &str, &str); 3] = [
        (
            1,
            "Desconto 10% Loja Verde",
            "Cupom de 10% de desconto em produtos sustentaveis na Loja Verde.",
            50,
            "VERDE10-A1B2C3",
            "Loja Verde",
        ),
        (
            2,
            "Cafe Gratis Padaria Sustentavel",
            "Um cafe coado gratuito na Padaria Sustentavel, valido de segunda a sexta.",
            30,
            "CAFE30-D4E5F6",
            "Padaria Sustentavel",
        ),
        (
            3,
            "Ingresso Cinema com Desconto",
            "50% de desconto em um ingresso no Cine Ecoa, valido para sessoes de segunda a quinta.",
            80,
            "CINE80-G7H8I9",
            "Cine Ecoa",
        ),
    ];

    for (id, name, description, price, code, provider) in incentives {
        let key = (symbol_short!("incentive"), id);
        let incentive = Incentive {
            id,
            name: String::from_str(env, name),
            description: String::from_str(env, description),
            price,
            code: String::from_str(env, code),
            provider: String::from_str(env, provider),
            available: true,
            redeemed_by: None,
        };
        env.storage().persistent().set(&key, &incentive);
    }

    env.storage().instance().set(&inc_count_sym(env), &3u64);
}

#[contractimpl]
impl IncentivePool {
    pub fn initialize(env: Env, admin: Address, ecoa_token: Address) {
        if env.storage().instance().has(&symbol_short!("admin")) {
            panic!("already initialized");
        }
        env.storage().instance().set(&symbol_short!("admin"), &admin);
        env.storage().instance().set(&eco_token_sym(&env), &ecoa_token);
        env.storage().instance().set(&inc_count_sym(&env), &0u64);

        seed_incentives(&env);
    }

    pub fn add_incentive(
        env: Env,
        name: String,
        description: String,
        price: i128,
        code: String,
        provider: String,
    ) -> u64 {
        let admin: Address = env.storage().instance().get(&symbol_short!("admin")).unwrap();
        admin.require_auth();

        let count: u64 = env.storage().instance().get(&inc_count_sym(&env)).unwrap_or(0);
        let new_id = count + 1;

        let key = (symbol_short!("incentive"), new_id);
        let incentive = Incentive {
            id: new_id,
            name: name.clone(),
            description: description.clone(),
            price,
            code: code.clone(),
            provider: provider.clone(),
            available: true,
            redeemed_by: None,
        };
        env.storage().persistent().set(&key, &incentive);
        env.storage().instance().set(&inc_count_sym(&env), &new_id);

        env.events().publish(
            (symbol_short!("incentive"), symbol_short!("added")),
            (new_id, name, price, provider),
        );

        new_id
    }

    pub fn redeem_incentive(env: Env, user: Address, incentive_id: u64) -> String {
        user.require_auth();

        let key = (symbol_short!("incentive"), incentive_id);
        let mut incentive: Incentive = env
            .storage()
            .persistent()
            .get(&key)
            .unwrap_or_else(|| panic!("incentive not found"));

        if !incentive.available {
            panic!("incentive already redeemed");
        }

        let ecoa_token_addr: Address = env
            .storage()
            .instance()
            .get(&eco_token_sym(&env))
            .unwrap();

        let mut args = Vec::new(&env);
        args.push_back(user.clone().into_val(&env));
        args.push_back(incentive.price.into_val(&env));

        env.invoke_contract::<()>(&ecoa_token_addr, &symbol_short!("burn"), args);

        incentive.available = false;
        incentive.redeemed_by = Some(user.clone());
        env.storage().persistent().set(&key, &incentive);

        env.events().publish(
            (symbol_short!("incentive"), symbol_short!("redeemed")),
            (incentive_id, user, incentive.code.clone()),
        );

        incentive.code
    }

    pub fn get_incentive(env: Env, incentive_id: u64) -> Option<Incentive> {
        let key = (symbol_short!("incentive"), incentive_id);
        env.storage().persistent().get(&key)
    }

    pub fn get_all_incentives(env: Env) -> Vec<Incentive> {
        let count: u64 = env.storage().instance().get(&inc_count_sym(&env)).unwrap_or(0);
        let mut result = Vec::new(&env);

        for id in 1..=count {
            let key = (symbol_short!("incentive"), id);
            if let Some(incentive) = env.storage().persistent().get::<_, Incentive>(&key) {
                result.push_back(incentive);
            }
        }

        result
    }

    pub fn get_available_incentives(env: Env) -> Vec<Incentive> {
        let count: u64 = env.storage().instance().get(&inc_count_sym(&env)).unwrap_or(0);
        let mut result = Vec::new(&env);

        for id in 1..=count {
            let key = (symbol_short!("incentive"), id);
            if let Some(incentive) = env.storage().persistent().get::<_, Incentive>(&key) {
                if incentive.available {
                    result.push_back(incentive);
                }
            }
        }

        result
    }

    pub fn get_incentive_count(env: Env) -> u64 {
        env.storage().instance().get(&inc_count_sym(&env)).unwrap_or(0)
    }
}

#[cfg(test)]
mod test {
    use super::*;
    use soroban_sdk::testutils::Address as _;
    use soroban_sdk::{Address, Env};

    fn create_token_contract(env: &Env, admin: &Address) -> Address {
        let token_id = env.register(ecoa_token::EcoaToken, ());
        let token_client = ecoa_token::EcoaTokenClient::new(env, &token_id);
        token_client.initialize(
            admin,
            &String::from_str(env, "ECOA Token"),
            &7u32,
        );
        token_id
    }

    #[test]
    fn test_initialize_with_seed_incentives() {
        let env = Env::default();
        let admin = Address::generate(&env);
        let token_id = create_token_contract(&env, &admin);

        let pool_id = env.register(IncentivePool, ());
        let pool_client = IncentivePoolClient::new(&env, &pool_id);
        pool_client.initialize(&admin, &token_id);

        let count = pool_client.get_incentive_count();
        assert_eq!(count, 3);

        let all = pool_client.get_all_incentives();
        assert_eq!(all.len(), 3);

        let available = pool_client.get_available_incentives();
        assert_eq!(available.len(), 3);

        let first = pool_client.get_incentive(&1u64).unwrap();
        assert_eq!(first.name, String::from_str(&env, "Desconto 10% Loja Verde"));
        assert_eq!(first.price, 50);
        assert!(first.available);
    }

    #[test]
    fn test_add_incentive() {
        let env = Env::default();
        let admin = Address::generate(&env);
        let token_id = create_token_contract(&env, &admin);

        let pool_id = env.register(IncentivePool, ());
        let pool_client = IncentivePoolClient::new(&env, &pool_id);
        pool_client.initialize(&admin, &token_id);

        let new_id = pool_client.add_incentive(
            &String::from_str(&env, "Desconto 20% Restaurante"),
            &String::from_str(&env, "20% de desconto no Restaurante Eco."),
            &100i128,
            &String::from_str(&env, "REST20-J0K1L2"),
            &String::from_str(&env, "Restaurante Eco"),
        );

        assert_eq!(new_id, 4);

        let count = pool_client.get_incentive_count();
        assert_eq!(count, 4);

        let incentive = pool_client.get_incentive(&4u64).unwrap();
        assert_eq!(incentive.name, String::from_str(&env, "Desconto 20% Restaurante"));
        assert_eq!(incentive.price, 100);
        assert!(incentive.available);
    }

    #[test]
    fn test_redeem_incentive_success() {
        let env = Env::default();
        let admin = Address::generate(&env);
        let user = Address::generate(&env);
        let token_id = create_token_contract(&env, &admin);

        let token_client = ecoa_token::EcoaTokenClient::new(&env, &token_id);
        token_client.mint(&user, &200i128);

        let pool_id = env.register(IncentivePool, ());
        let pool_client = IncentivePoolClient::new(&env, &pool_id);
        pool_client.initialize(&admin, &token_id);

        let code = pool_client.redeem_incentive(&user, &1u64);
        assert_eq!(code, String::from_str(&env, "VERDE10-A1B2C3"));

        let incentive = pool_client.get_incentive(&1u64).unwrap();
        assert!(!incentive.available);
        assert_eq!(incentive.redeemed_by, Some(user.clone()));

        let available = pool_client.get_available_incentives();
        assert_eq!(available.len(), 2);

        let balance = token_client.balance(&user);
        assert_eq!(balance, 150);
    }

    #[test]
    fn test_redeem_incentive_insufficient_balance() {
        let env = Env::default();
        let admin = Address::generate(&env);
        let user = Address::generate(&env);
        let token_id = create_token_contract(&env, &admin);

        let token_client = ecoa_token::EcoaTokenClient::new(&env, &token_id);
        token_client.mint(&user, &10i128);

        let pool_id = env.register(IncentivePool, ());
        let pool_client = IncentivePoolClient::new(&env, &pool_id);
        pool_client.initialize(&admin, &token_id);

        let result = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| {
            pool_client.redeem_incentive(&user, &1u64);
        }));
        assert!(result.is_err());
    }

    #[test]
    fn test_redeem_incentive_already_redeemed() {
        let env = Env::default();
        let admin = Address::generate(&env);
        let user1 = Address::generate(&env);
        let user2 = Address::generate(&env);
        let token_id = create_token_contract(&env, &admin);

        let token_client = ecoa_token::EcoaTokenClient::new(&env, &token_id);
        token_client.mint(&user1, &200i128);
        token_client.mint(&user2, &200i128);

        let pool_id = env.register(IncentivePool, ());
        let pool_client = IncentivePoolClient::new(&env, &pool_id);
        pool_client.initialize(&admin, &token_id);

        pool_client.redeem_incentive(&user1, &1u64);

        let result = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| {
            pool_client.redeem_incentive(&user2, &1u64);
        }));
        assert!(result.is_err());
    }

    #[test]
    fn test_get_available_incentives() {
        let env = Env::default();
        let admin = Address::generate(&env);
        let user = Address::generate(&env);
        let token_id = create_token_contract(&env, &admin);

        let token_client = ecoa_token::EcoaTokenClient::new(&env, &token_id);
        token_client.mint(&user, &200i128);

        let pool_id = env.register(IncentivePool, ());
        let pool_client = IncentivePoolClient::new(&env, &pool_id);
        pool_client.initialize(&admin, &token_id);

        let available_before = pool_client.get_available_incentives();
        assert_eq!(available_before.len(), 3);

        pool_client.redeem_incentive(&user, &1u64);

        let available_after = pool_client.get_available_incentives();
        assert_eq!(available_after.len(), 2);

        let all = pool_client.get_all_incentives();
        assert_eq!(all.len(), 3);
    }
}
