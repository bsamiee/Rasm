use core::hint::black_box;

use rmcp::{ErrorData, model::ContentBlock};
use serde::Serialize;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Serialize)]
struct Port {
    number: u16,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tried(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string(port)?)) }

fn bound(port: &Port) -> Result<ContentBlock, serde_json::Error> {
    let text = serde_json::to_string(port)?;
    Ok(ContentBlock::text(text))
}

fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) }

fn pretty(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string_pretty(port)?)) }

fn direct(port: &Port) -> Result<ContentBlock, ErrorData> { ContentBlock::json(port) }

fn rendered(port: &Port) -> ContentBlock { ContentBlock::text(port.number.to_string()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(tried, bound, defaulted, pretty, direct, rendered)); }
