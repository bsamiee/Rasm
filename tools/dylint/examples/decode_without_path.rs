use core::hint::black_box;

use serde::{Deserialize, Deserializer, de::value};

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

#[derive(Deserialize)]
enum Keyword {
    #[serde(rename = "READY")]
    Ready,
    #[serde(rename = "DONE", alias = "FINISHED")]
    Done,
}

#[derive(Deserialize)]
enum Delegated {
    #[serde(deserialize_with = "unit")]
    Unit,
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

fn text(value: &serde_json::Value) -> Result<&str, serde_json::Error> { <&str>::deserialize(value) }

fn tracked(text: &str) -> Result<Wrapped, serde_path_to_error::Error<serde_json::Error>> { serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(text)) }

fn dynamic(bytes: &[u8]) -> Result<serde_json::Value, serde_json::Error> { serde_json::Value::deserialize(&mut serde_json::Deserializer::from_slice(bytes)) }

fn manually_tracked(text: &str) -> Result<Port, serde_json::Error> {
    let mut track = serde_path_to_error::Track::new();
    Port::deserialize(serde_path_to_error::Deserializer::new(&mut serde_json::Deserializer::from_str(text), &mut track))
}

fn keyword(text: &str) -> Result<Keyword, value::Error> { Keyword::deserialize(value::StrDeserializer::new(text)) }

fn keyword_json(text: &str) -> Result<Keyword, serde_json::Error> { Keyword::deserialize(&mut serde_json::Deserializer::from_str(text)) }

fn scalar_record(text: &str) -> Result<Port, value::Error> { Port::deserialize(value::StrDeserializer::new(text)) }

fn unit<'de, D: Deserializer<'de>>(deserializer: D) -> Result<(), D::Error> { <()>::deserialize(deserializer) }

fn custom_unit(text: &str) -> Result<Delegated, value::Error> { Delegated::deserialize(value::StrDeserializer::new(text)) }

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
        text(&serde_json::Value::Null).map(str::len),
        dynamic(b"null"),
        manually_tracked("{}").map(read),
        keyword("READY"),
        keyword("FINISHED"),
        keyword("unknown"),
        keyword_json("null"),
        scalar_record("unknown").map(read),
        custom_unit("Unit"),
    ));
}
