use core::hint::black_box;

use serde::{Deserialize, Deserializer};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Deserialize)]
struct Port {
    number: u16,
}

#[derive(Deserialize)]
#[serde(untagged)]
enum Reply {
    Port(Port),
    Number(u16),
}

struct Wrapped(Port);

impl<'de> Deserialize<'de> for Wrapped {
    fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
    where D: Deserializer<'de> {
        Port::deserialize(deserializer).map(Self)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decoded(text: &str) -> Result<Port, serde_json::Error> { Port::deserialize(&mut serde_json::Deserializer::from_str(text)) }

fn qualified(text: &str) -> Result<Reply, serde_json::Error> { <Reply as Deserialize>::deserialize(&mut serde_json::Deserializer::from_str(text)) }

fn converted(value: serde_json::Value) -> Result<Port, serde_json::Error> { Port::deserialize(value) }

fn tracked(text: &str) -> Result<Wrapped, serde_path_to_error::Error<serde_json::Error>> { serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(text)) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let read = |port: Port| port.number;
    black_box(&(
        decoded("{}").map(read),
        qualified("1").map(|reply| match reply {
            Reply::Port(port) => read(port),
            Reply::Number(number) => number,
        }),
        converted(serde_json::Value::Null).map(read),
        tracked("{}").map(|Wrapped(port)| read(port)),
    ));
}
