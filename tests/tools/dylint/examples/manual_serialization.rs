#![cfg_attr(dylint_lib = "rasm_lints", allow(discarded_error, forwarding_function, reason = "these lints fire incidentally here and are exercised by their own fixtures"))]

use core::hint::black_box;

use rmcp::ErrorData;
use rmcp::model::ContentBlock;
use serde::Serialize;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Serialize)]
struct Port {
    number: u16,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tried(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string(port)?)) } //~ manual_serialization

fn bound(port: &Port) -> Result<ContentBlock, serde_json::Error> {
    let text = serde_json::to_string(port)?;
    //~v manual_serialization
    Ok(ContentBlock::text(text))
}

fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) } //~ manual_serialization

fn pretty(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string_pretty(port)?)) }

fn direct(port: &Port) -> Result<ContentBlock, ErrorData> { ContentBlock::json(port) }

fn rendered(port: &Port) -> ContentBlock { ContentBlock::text(port.number.to_string()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(tried, bound, defaulted, pretty, direct, rendered, changed)); }

fn changed(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).map(|_| String::from("other")).unwrap_or_default()) }
