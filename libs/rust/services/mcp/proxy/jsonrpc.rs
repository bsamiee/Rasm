use std::error::Report;
use std::io;

use bytes::{Buf as _, Bytes, BytesMut};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rmcp::ErrorData;
use rmcp::model::JsonRpcVersion2_0;
use serde::{Deserialize, Serialize};
use serde_json::value::RawValue;
use serde_path_to_error::Track;
use tokio_util::codec::Decoder;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize, derive_more::IsVariant)]
pub enum Method {
    #[serde(rename = "initialize")]
    Initialize,
    #[serde(rename = "notifications/initialized")]
    Initialized,
    #[serde(rename = "server/discover")]
    Discover,
    #[serde(rename = "ping")]
    Ping,
    #[serde(rename = "notifications/cancelled")]
    Cancelled,
    #[serde(rename = "notifications/progress")]
    Progress,
    #[serde(rename = "tools/call")]
    CallTool,
    #[serde(rename = "prompts/get")]
    GetPrompt,
    #[serde(rename = "resources/subscribe")]
    Subscribe,
    #[serde(rename = "resources/unsubscribe")]
    Unsubscribe,
    #[serde(rename = "notifications/resources/updated")]
    ResourceUpdated,
    #[serde(rename = "subscriptions/listen")]
    Listen,
    #[serde(rename = "logging/setLevel")]
    SetLevel,
    #[serde(rename = "notifications/message")]
    Logged,
    #[serde(other)]
    Other,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
pub enum Severity {
    Debug,
    Info,
    Notice,
    Warning,
    Error,
    Critical,
    Alert,
    Emergency,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Line {
    Whole(Bytes),
    Oversized { head: Bytes, tail: Bytes },
}

#[derive(Debug, Deserialize)]
pub struct Meta<'line> {
    #[serde(borrow, rename = "progressToken")]
    pub progress_token: Option<&'line RawValue>,
    #[serde(borrow, rename = "io.modelcontextprotocol/subscriptionId")]
    pub subscription_id: Option<&'line RawValue>,
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
    pub protocol_version: Option<&'line RawValue>,
    #[serde(borrow)]
    pub uri: Option<&'line RawValue>,
    #[serde(borrow)]
    pub name: Option<&'line RawValue>,
    #[serde(borrow)]
    pub level: Option<&'line RawValue>,
}

#[derive(Debug, Deserialize)]
pub struct Envelope<'line> {
    #[serde(borrow)]
    pub id: Option<&'line RawValue>,
    pub method: Option<Method>,
    #[serde(borrow)]
    pub params: Option<Params<'line>>,
    #[serde(borrow)]
    pub result: Option<&'line RawValue>,
    #[serde(borrow)]
    pub error: Option<&'line RawValue>,
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

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Lines {
    limit: usize,
    base: Option<usize>,
    scanned: usize,
    oversized: Option<Bytes>,
    tail: BytesMut,
}

impl Lines {
    #[must_use]
    pub fn new(limit: usize) -> Self { Self { limit, base: None, scanned: 0, oversized: None, tail: BytesMut::new() } }

    fn keep(&mut self, discarded: &[u8]) {
        const TAIL: usize = 256;
        let kept = discarded.get(discarded.len().saturating_sub(TAIL)..).unwrap_or(discarded);
        self.tail.advance(self.tail.len().saturating_add(kept.len()).saturating_sub(TAIL).min(self.tail.len()));
        self.tail.extend_from_slice(kept);
    }
}

impl Decoder for Lines {
    type Error = io::Error;
    type Item = Line;

    fn decode(&mut self, src: &mut BytesMut) -> io::Result<Option<Line>> {
        let base = *self.base.get_or_insert_with(|| src.capacity());
        let found = src.get(self.scanned..).and_then(|rest| memchr::memchr(b'\n', rest)).map(|offset| self.scanned.saturating_add(offset).saturating_add(1));
        let line = match (found, self.oversized.take()) {
            (Some(end), Some(head)) => {
                let rest = src.split_to(end);
                self.keep(&rest);
                self.scanned = 0;
                Some(Line::Oversized { head, tail: self.tail.split().freeze() })
            }
            (None, Some(head)) => {
                let rest = src.split();
                self.keep(&rest);
                (self.scanned, self.oversized) = (0, Some(head));
                None
            }
            (Some(end), None) if end > self.limit => {
                let mut whole = src.split_to(end);
                let head = whole.split_to(self.limit).freeze();
                self.keep(&whole);
                self.scanned = 0;
                Some(Line::Oversized { head, tail: self.tail.split().freeze() })
            }
            (Some(end), None) => {
                self.scanned = 0;
                Some(Line::Whole(src.split_to(end).freeze()))
            }
            (None, None) if src.len() > self.limit => {
                let mut whole = src.split();
                let head = whole.split_to(self.limit).freeze();
                self.keep(&whole);
                (self.scanned, self.oversized) = (0, Some(head));
                None
            }
            (None, None) => {
                self.scanned = src.len();
                None
            }
        };
        if src.capacity() > base && src.len() <= base {
            let mut rest = BytesMut::with_capacity(base);
            rest.extend_from_slice(src);
            *src = rest;
        }
        Ok(line)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn decode<'de, T>(bytes: &'de [u8]) -> Result<T, serde_path_to_error::Error<serde_json::Error>>
where T: Deserialize<'de> {
    let mut decoder = serde_json::Deserializer::from_slice(bytes);
    let value = serde_path_to_error::deserialize(&mut decoder)?;
    decoder.end().map(|()| value).map_err(|source| serde_path_to_error::Error::new(Track::new().path(), source))
}

pub fn encoded(message: &impl Serialize) -> Result<Bytes, serde_json::Error> {
    let mut line = serde_json::to_vec(message)?;
    line.push(b'\n');
    Ok(line.into())
}

pub fn refusal(id: &[u8], error: &ErrorData) -> Result<Bytes, serde_json::Error> {
    let id = decode::<&RawValue>(id).map_err(serde_path_to_error::Error::into_inner)?;
    encoded(&Message::<&RawValue, (), ()> { jsonrpc: JsonRpcVersion2_0, id: Some(id), method: None, params: None, result: None, error: Some(error) })
}

#[must_use]
pub fn identity(head: &[u8], tail: &[u8]) -> Option<Bytes> {
    use serde::de::{DeserializeSeed, IgnoredAny, MapAccess, Visitor};
    struct Leading<'slot>(&'slot mut Option<Bytes>);
    impl<'de> Visitor<'de> for Leading<'_> {
        type Value = ();

        fn expecting(&self, formatter: &mut core::fmt::Formatter<'_>) -> core::fmt::Result { formatter.write_str("a JSON-RPC message") }

        fn visit_map<M>(self, mut map: M) -> Result<(), M::Error>
        where M: MapAccess<'de> {
            while let Some(key) = map.next_key::<std::borrow::Cow<'de, str>>()? {
                if key == ID {
                    *self.0 = Some(Bytes::copy_from_slice(map.next_value::<&RawValue>()?.get().as_bytes()));
                } else {
                    map.next_value::<IgnoredAny>()?;
                }
            }
            Ok(())
        }
    }
    impl<'de> DeserializeSeed<'de> for Leading<'_> {
        type Value = ();

        fn deserialize<D>(self, deserializer: D) -> Result<(), D::Error>
        where D: serde::Deserializer<'de> {
            deserializer.deserialize_map(self)
        }
    }
    #[derive(Deserialize)]
    struct Trailing<'tail> {
        #[serde(borrow)]
        id: &'tail RawValue,
    }
    const ID: &str = "id";
    let mut found = None;
    let mut decoder = serde_json::Deserializer::from_slice(head);
    if let Err(error) = Leading(&mut found).deserialize(&mut decoder).and_then(|()| decoder.end()) {
        tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "message head ends before its close");
    }
    found.or_else(|| {
        let start = memchr::memmem::rfind(tail, format!("\"{ID}\"").as_bytes()).filter(|start| start.checked_sub(1).and_then(|before| tail.get(before)) != Some(&b'\\'))?;
        let members = [b"{", tail.get(start..)?].concat();
        match decode::<Trailing<'_>>(&members) {
            Ok(Trailing { id }) => Some(Bytes::copy_from_slice(id.get().as_bytes())),
            Err(error) => {
                tracing::trace!({ EXCEPTION_MESSAGE } = %Report::new(&error), "message tail holds no top-level id");
                None
            }
        }
    })
}
