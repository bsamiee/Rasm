use core::hint::black_box;

use opentelemetry_otlp::OTEL_EXPORTER_OTLP_ENDPOINT;
use opentelemetry_semantic_conventions::attribute::{SERVICE_NAME, SERVICE_VERSION};
use serde::Deserialize;

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENDPOINT: &str = "OTEL_EXPORTER_OTLP_ENDPOINT";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Deserialize)]
struct Event {
    #[serde(rename = "service.name")]
    service: String,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn spelled() -> [(&'static str, &'static str); 2] { [("service.name", "bridge"), ("http.server.request.duration", "1.5")] }

fn located(key: &str) -> bool { matches!(key, "code.file.path") }

macro_rules! namespaced {
    () => {
        "service.namespace"
    };
}

fn expanded() -> &'static str { namespaced!() }

fn moved() -> (&'static str, &'static str) { ("mcp.method.name", "tools/list") }

fn worded() -> (&'static str, &'static str) { ("request", "ping") }

fn derived() -> [&'static str; 3] { [SERVICE_NAME, SERVICE_VERSION, OTEL_EXPORTER_OTLP_ENDPOINT] }

fn decoded(event: Event) -> String { event.service }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(ENDPOINT, spelled, located, expanded, moved, worded, derived, decoded)); }
