use core::hint::black_box;
use std::collections::BTreeMap;

use uuid::Uuid;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Account {
    user_id: String,
    name: String,
}

#[derive(Debug)]
struct Session {
    key: String,
}

#[derive(Debug)]
struct Typed {
    user_id: Uuid,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn account(name: &str) -> Account { Account { user_id: Uuid::new_v4().to_string(), name: name.to_owned() } }

fn session(id: Uuid) -> Session { Session { key: format!("session-{id}") } }

fn rotated(session: &mut Session) {
    let fresh = Uuid::new_v4();
    session.key = fresh.hyphenated().to_string();
}

fn typed() -> Typed { Typed { user_id: Uuid::new_v4() } }

fn keyed() -> BTreeMap<String, u32> {
    let key = Uuid::new_v4().to_string();
    BTreeMap::from([(key, 1)])
}

fn failed(error: uuid::Error) -> Session { Session { key: error.to_string() } }

fn formatted(id: Uuid) -> Session { Session { key: format!("{id}") } }

fn counted(id: Uuid) -> Session { Session { key: format!("{}", id.as_bytes().len()) } }

fn qualified(id: Uuid) -> Session { Session { key: ToString::to_string(&id.hyphenated()) } }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let mut opened = session(Uuid::nil());
    rotated(&mut opened);
    let created = account("ada");
    black_box(&(created.user_id.len(), created.name.len(), opened.key.len(), typed().user_id.is_nil(), keyed().len(), failed, formatted, counted, qualified));
}
