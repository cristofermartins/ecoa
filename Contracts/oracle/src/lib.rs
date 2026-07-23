#![no_std]

use soroban_sdk::{contract, contractimpl, contracttype, symbol_short, Address, Env, String, Symbol};

#[contract]
pub struct Oracle;

#[contracttype]
#[derive(Clone)]
pub struct ActionValidation {
    pub action_id: String,
    pub user: Address,
    pub action_type: String,
    pub ecoa_amount: i128,
    pub qualitative_value: i128,
    pub qualitative_unit: String,
    pub timestamp: u64,
    pub validated: bool,
}

fn eco_token_sym(env: &Env) -> Symbol {
    Symbol::new(env, "eco_token")
}

fn act_count_sym(env: &Env) -> Symbol {
    Symbol::new(env, "act_count")
}

#[contractimpl]
impl Oracle {
    pub fn initialize(env: Env, admin: Address, ecoa_token: Address) {
        if env.storage().instance().has(&symbol_short!("admin")) {
            panic!("already initialized");
        }
        env.storage().instance().set(&symbol_short!("admin"), &admin);
        env.storage().instance().set(&eco_token_sym(&env), &ecoa_token);
        env.storage().instance().set(&act_count_sym(&env), &0u64);
    }

    pub fn validate_action(
        env: Env,
        action_id: String,
        user: Address,
        action_type: String,
        ecoa_amount: i128,
        qualitative_value: i128,
        qualitative_unit: String,
    ) {
        let admin: Address = env.storage().instance().get(&symbol_short!("admin")).unwrap();
        admin.require_auth();

        let count: u64 = env.storage().instance().get(&act_count_sym(&env)).unwrap_or(0);
        let new_count = count + 1;
        env.storage().instance().set(&act_count_sym(&env), &new_count);

        let timestamp = env.ledger().timestamp();

        let key = (symbol_short!("action"), action_id.clone());
        let validation = ActionValidation {
            action_id: action_id.clone(),
            user: user.clone(),
            action_type: action_type.clone(),
            ecoa_amount,
            qualitative_value,
            qualitative_unit: qualitative_unit.clone(),
            timestamp,
            validated: true,
        };
        env.storage().persistent().set(&key, &validation);

        env.events().publish(
            (symbol_short!("validated"), symbol_short!("action")),
            (action_id, user, action_type, ecoa_amount, qualitative_value, qualitative_unit, timestamp),
        );
    }

    pub fn is_validated(env: Env, action_id: String) -> bool {
        let key = (symbol_short!("action"), action_id);
        if let Some(validation) = env.storage().persistent().get::<_, ActionValidation>(&key) {
            validation.validated
        } else {
            false
        }
    }

    pub fn get_action_metadata(env: Env, action_id: String) -> Option<ActionValidation> {
        let key = (symbol_short!("action"), action_id);
        env.storage().persistent().get(&key)
    }

    pub fn get_action_count(env: Env) -> u64 {
        env.storage().instance().get(&act_count_sym(&env)).unwrap_or(0)
    }
}
