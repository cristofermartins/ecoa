#![no_std]

use soroban_sdk::{contract, contractimpl, symbol_short, Address, Env, String, Symbol};

const ECOA_SYMBOL: &str = "ECOA";

#[contract]
pub struct EcoaToken;

fn total_supply_sym(env: &Env) -> Symbol {
    Symbol::new(env, "tot_sup")
}

#[contractimpl]
impl EcoaToken {
    pub fn initialize(env: Env, admin: Address, name: String, decimal: u32) {
        if env.storage().instance().has(&symbol_short!("admin")) {
            panic!("already initialized");
        }
        env.storage().instance().set(&symbol_short!("admin"), &admin);
        env.storage().instance().set(&symbol_short!("name"), &name);
        env.storage().instance().set(&symbol_short!("symbol"), &String::from_str(&env, ECOA_SYMBOL));
        env.storage().instance().set(&symbol_short!("decimal"), &decimal);
        env.storage().instance().set(&total_supply_sym(&env), &0i128);
    }

    pub fn mint(env: Env, to: Address, amount: i128) {
        let admin: Address = env.storage().instance().get(&symbol_short!("admin")).unwrap();
        admin.require_auth();

        let balance_key = (symbol_short!("balance"), to.clone());
        let current: i128 = env.storage().persistent().get(&balance_key).unwrap_or(0);
        env.storage().persistent().set(&balance_key, &(current + amount));

        let total: i128 = env.storage().instance().get(&total_supply_sym(&env)).unwrap_or(0);
        env.storage().instance().set(&total_supply_sym(&env), &(total + amount));

        env.events().publish(
            (symbol_short!("mint"), symbol_short!("ecoa")),
            (to, amount),
        );
    }

    pub fn burn(env: Env, from: Address, amount: i128) {
        from.require_auth();

        let balance_key = (symbol_short!("balance"), from.clone());
        let current: i128 = env.storage().persistent().get(&balance_key).unwrap_or(0);
        if current < amount {
            panic!("insufficient balance");
        }
        env.storage().persistent().set(&balance_key, &(current - amount));

        let total: i128 = env.storage().instance().get(&total_supply_sym(&env)).unwrap_or(0);
        env.storage().instance().set(&total_supply_sym(&env), &(total - amount));

        env.events().publish(
            (symbol_short!("burn"), symbol_short!("ecoa")),
            (from, amount),
        );
    }

    pub fn transfer(env: Env, from: Address, to: Address, amount: i128) {
        from.require_auth();

        let from_key = (symbol_short!("balance"), from.clone());
        let from_balance: i128 = env.storage().persistent().get(&from_key).unwrap_or(0);
        if from_balance < amount {
            panic!("insufficient balance");
        }

        let to_key = (symbol_short!("balance"), to.clone());
        let to_balance: i128 = env.storage().persistent().get(&to_key).unwrap_or(0);

        env.storage().persistent().set(&from_key, &(from_balance - amount));
        env.storage().persistent().set(&to_key, &(to_balance + amount));

        env.events().publish(
            (symbol_short!("transfer"), symbol_short!("ecoa")),
            (from, to, amount),
        );
    }

    pub fn balance(env: Env, owner: Address) -> i128 {
        let key = (symbol_short!("balance"), owner);
        env.storage().persistent().get(&key).unwrap_or(0)
    }

    pub fn total_supply(env: Env) -> i128 {
        env.storage().instance().get(&total_supply_sym(&env)).unwrap_or(0)
    }

    pub fn name(env: Env) -> String {
        env.storage().instance().get(&symbol_short!("name")).unwrap()
    }

    pub fn symbol(env: Env) -> String {
        env.storage().instance().get(&symbol_short!("symbol")).unwrap()
    }

    pub fn decimals(env: Env) -> u32 {
        env.storage().instance().get(&symbol_short!("decimal")).unwrap()
    }
}
