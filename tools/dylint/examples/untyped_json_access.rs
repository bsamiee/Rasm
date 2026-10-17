use core::{hint::black_box, ops::Index};

use serde::Deserialize;
use serde_json::{Deserializer, Map, Value, json};
use serde_path_to_error::{Error, deserialize};

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAME: &str = "name";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Deserialize)]
struct Tool {
    name: String,
    size: u64,
}

struct Carrier<'meta>(&'meta Map<String, Value>);

impl Index<&str> for Carrier<'_> {
    type Output = str;

    fn index(&self, key: &str) -> &str { self.0.get(key).and_then(Value::as_str).unwrap_or_default() }
}

struct Flags<'meta>(&'meta Map<String, Value>);

impl Index<&str> for Flags<'_> {
    type Output = bool;

    fn index(&self, key: &str) -> &bool { if let Some(Value::Bool(flag)) = self.0.get(key) { flag } else { &false } }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum FixtureError {
    #[error("cannot decode the document")]
    Decode(#[from] Error<serde_json::Error>),
    #[error("cannot encode the counts")]
    Encode(#[from] serde_json::Error),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tool_name(text: &str) -> Result<Option<String>, Error<serde_json::Error>> {
    let document: Value = deserialize(&mut Deserializer::from_str(text))?;
    Ok(document.get("name").and_then(Value::as_str).map(str::to_owned))
}

fn tool_size(bytes: &[u8]) -> Result<u64, Error<serde_json::Error>> {
    let document = deserialize::<_, Value>(&mut Deserializer::from_slice(bytes))?;
    Ok(document["size"]["bytes"].as_u64().unwrap_or_default())
}

fn object_keys(document: Value) -> Result<Vec<String>, Error<serde_json::Error>> {
    let object = deserialize::<_, Map<String, Value>>(document)?;
    Ok(object.keys().cloned().collect())
}

fn object_name(object: &Map<String, Value>) -> Option<&Value> { object.get("name") }

fn constant_name(object: &Map<String, Value>) -> Option<&Value> { Map::get(object, NAME) }

fn renamed(object: &mut Map<String, Value>) -> Option<Value> { object.insert("name".to_owned(), Value::Null) }

fn first(document: &Value) -> Option<&Value> { document.pointer("/items/0") }

fn textual(document: &Value) -> Option<&str> { if let Value::String(text) = document { Some(text) } else { None } }

fn streamed(text: &str) -> Option<Result<Value, serde_json::Error>> { Deserializer::from_str(text).into_iter::<Value>().next() }

fn member<'document>(object: &'document Map<String, Value>, key: &str) -> Option<&'document Value> { object.get(key) }

fn keyed<'document>(object: &'document Map<String, Value>, key: &str) -> Option<&'document str> { object.get(key).and_then(Value::as_str) }

fn picked<'document>(object: &'document Map<String, Value>, keys: &[&str]) -> Vec<&'document str> { keys.iter().copied().filter_map(|key| object.get(key).and_then(Value::as_str)).collect() }

fn nested<'document>(document: &'document Value, key: &str) -> Option<&'document Value> { document.get(key) }

fn taken(document: &mut Value) -> Value { document.take() }

fn forwarded(meta: &mut Map<String, Value>) { meta.retain(|key, _| !key.starts_with("io.")); }

fn listed(text: &str) -> Result<Vec<Value>, Error<serde_json::Error>> { deserialize(&mut Deserializer::from_str(text)) }

fn head(documents: &[Value]) -> Option<&Value> { documents.first() }

fn empty_object() -> Map<String, Value> { Map::with_capacity(1) }

fn typed(text: &str) -> Result<Tool, Error<serde_json::Error>> { deserialize(&mut Deserializer::from_str(text)) }

fn counts(text: &str) -> Result<Vec<u32>, Error<serde_json::Error>> { deserialize(&mut Deserializer::from_str(text)) }

fn built(name: &str) -> Value { json!({ "name": name }) }

fn rendered(counts: &[u32]) -> Result<String, serde_json::Error> { serde_json::to_string(counts) }

fn bound_key(document: &Value) -> Option<&Value> {
    let key = "name";
    document.get(key)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let run = || -> Result<(), FixtureError> {
        let text = r#"{"name": "tool", "size": 3}"#;
        let tool = typed(text)?;
        tool_name(text)?;
        tool_size(text.as_bytes())?;
        object_keys(built(&tool.name))?;
        let mut object = empty_object();
        let mut document = built(&tool.name);
        object_name(&object);
        constant_name(&object);
        renamed(&mut object);
        member(&object, &tool.name);
        forwarded(&mut object);
        first(&document);
        textual(&document);
        nested(&document, &tool.name);
        keyed(&object, &tool.name);
        picked(&object, &[NAME]);
        black_box(&(&Carrier(&object)[NAME], Flags(&object)[NAME]));
        streamed(text).transpose()?;
        head(&listed(&tool.size.to_string())?);
        bound_key(&document);
        taken(&mut document);
        rendered(&counts("[1, 2]")?)?;
        Ok(())
    };
    black_box(&(run(), NAME));
}
