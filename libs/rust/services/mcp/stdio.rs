use std::borrow::Cow;
use std::io;

use bytes::{Buf as _, Bytes, BytesMut};
use rmcp::ErrorData;
use rmcp::model::JsonRpcVersion2_0;
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use tokio_util::codec::Decoder;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Line {
    Whole(Bytes),
    Oversized(Bytes),
}

#[derive(Debug, Deserialize)]
pub struct Meta<'line> {
    #[serde(borrow, rename = "progressToken")]
    pub progress_token: Option<&'line RawValue>,
}

#[derive(Debug, Deserialize)]
pub struct Params<'line> {
    #[serde(borrow, rename = "_meta")]
    pub meta: Option<Meta<'line>>,
    #[serde(borrow, rename = "progressToken")]
    pub progress_token: Option<&'line RawValue>,
    #[serde(borrow, rename = "requestId")]
    pub request_id: Option<&'line RawValue>,
    #[serde(borrow, rename = "protocolVersion")]
    pub protocol_version: Option<Cow<'line, str>>,
    #[serde(borrow)]
    pub uri: Option<Cow<'line, str>>,
}

#[derive(Debug, Deserialize)]
pub struct Envelope<'line> {
    #[serde(borrow)]
    pub id: Option<&'line RawValue>,
    #[serde(borrow)]
    pub method: Option<Cow<'line, str>>,
    #[serde(borrow)]
    pub params: Option<Params<'line>>,
    #[serde(borrow)]
    pub result: Option<&'line RawValue>,
    #[serde(borrow)]
    pub error: Option<&'line RawValue>,
}

impl<'line> Envelope<'line> {
    pub fn read(line: &'line [u8]) -> Result<Self, serde_path_to_error::Error<serde_json::Error>> { serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_slice(line)) }
}

#[derive(Debug, Serialize)]
pub struct Message<'message, I, P, R> {
    pub jsonrpc: JsonRpcVersion2_0,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub id: Option<I>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub method: Option<&'message str>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub params: Option<P>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub result: Option<R>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub error: Option<&'message ErrorData>,
}

impl<I, P, R> Message<'_, I, P, R>
where
    I: Serialize,
    P: Serialize,
    R: Serialize,
{
    pub fn line(&self) -> Result<Bytes, serde_json::Error> {
        let mut line = serde_json::to_vec(self)?;
        line.push(b'\n');
        Ok(line.into())
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Lines {
    limit: usize,
    capacity: usize,
    scanned: usize,
    discarding: bool,
}

impl Lines {
    #[must_use]
    pub const fn new(limit: usize, capacity: usize) -> Self { Self { limit, capacity, scanned: 0, discarding: false } }
}

impl Decoder for Lines {
    type Error = io::Error;
    type Item = Line;

    fn decode(&mut self, source: &mut BytesMut) -> io::Result<Option<Line>> {
        loop {
            let found = source.get(self.scanned..).and_then(|rest| memchr::memchr(b'\n', rest)).map(|offset| self.scanned.saturating_add(offset).saturating_add(1));
            let line = match (found, self.discarding) {
                (Some(end), true) => {
                    source.advance(end);
                    (self.scanned, self.discarding) = (0, false);
                    continue;
                }
                (None, true) => {
                    source.clear();
                    self.scanned = 0;
                    None
                }
                (Some(end), false) if end > self.limit => {
                    let head = source.split_to(end).split_to(self.limit).freeze();
                    self.scanned = 0;
                    Some(Line::Oversized(head))
                }
                (Some(end), false) => {
                    let line = source.split_to(end).freeze();
                    self.scanned = 0;
                    Some(Line::Whole(line))
                }
                (None, false) if source.len() > self.limit => {
                    let head = source.split().split_to(self.limit).freeze();
                    (self.scanned, self.discarding) = (0, true);
                    Some(Line::Oversized(head))
                }
                (None, false) => {
                    self.scanned = source.len();
                    None
                }
            };
            if source.is_empty() && source.capacity() > self.capacity {
                *source = BytesMut::with_capacity(self.capacity);
            }
            return Ok(line);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn identity(head: &[u8]) -> Option<Bytes> {
    use serde::de::{DeserializeSeed, IgnoredAny, MapAccess, Visitor};
    struct Identity<'slot>(&'slot mut Option<Bytes>);
    impl<'de> Visitor<'de> for Identity<'_> {
        type Value = ();

        fn expecting(&self, formatter: &mut core::fmt::Formatter<'_>) -> core::fmt::Result { formatter.write_str("a JSON-RPC message") }

        fn visit_map<M>(self, mut map: M) -> Result<(), M::Error>
        where M: MapAccess<'de> {
            while let Some(key) = map.next_key::<Cow<'de, str>>()? {
                if key == "id" {
                    *self.0 = Some(Bytes::copy_from_slice(map.next_value::<&RawValue>()?.get().as_bytes()));
                } else {
                    map.next_value::<IgnoredAny>()?;
                }
            }
            Ok(())
        }
    }
    impl<'de> DeserializeSeed<'de> for Identity<'_> {
        type Value = ();

        fn deserialize<D>(self, deserializer: D) -> Result<(), D::Error>
        where D: serde::Deserializer<'de> {
            deserializer.deserialize_map(self)
        }
    }
    let mut found = None;
    match Identity(&mut found).deserialize(&mut serde_json::Deserializer::from_slice(head)) {
        Ok(()) | Err(_) => found,
    }
}
