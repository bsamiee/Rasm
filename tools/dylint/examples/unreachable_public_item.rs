use core::hint::black_box;

// --- [OPERATIONS] ----------------------------------------------------------------------

mod ledger {
    macro_rules! counted {
        ($name:ident) => {
            pub fn $name() -> u32 { 3 }
        };
    }

    pub(crate) struct Account;

    impl Account {
        pub const fn opened() -> Self { Self }
    }

    tokio::task_local! {
        pub static REQUEST: u32;
    }

    pub const fn balance() -> u32 { 1 }

    pub(crate) const fn total() -> u32 { 2 }

    counted!(count);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box((ledger::balance(), ledger::total(), ledger::Account::opened(), ledger::count(), &ledger::REQUEST)); }
