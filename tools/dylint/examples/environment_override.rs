use core::{hint::black_box, time::Duration};

use opentelemetry_otlp::{ExporterBuildError, LogExporter, Protocol, SpanExporter, WithExportConfig as _};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn configured() -> Result<SpanExporter, ExporterBuildError> {
    SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_timeout(Duration::from_secs(5)).build()
}

fn qualified() -> Result<LogExporter, ExporterBuildError> { opentelemetry_otlp::WithExportConfig::with_endpoint(LogExporter::builder().with_http(), "http://127.0.0.1:4318/v1/logs").build() }

fn environmental() -> Result<SpanExporter, ExporterBuildError> { SpanExporter::builder().with_http().build() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(configured, qualified, environmental)); }
