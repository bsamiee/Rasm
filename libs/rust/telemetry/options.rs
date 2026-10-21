use clap::{Args, ValueEnum};
use tracing_subscriber::{EnvFilter, filter::Directive};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, Default, PartialEq, Eq, ValueEnum)]
pub(crate) enum Exporter {
    #[default]
    None,
    #[value(help = "OTLP over HTTP/protobuf to the endpoint, headers, timeout, and gzip compression the OTEL_EXPORTER_OTLP_* variables name")]
    Otlp,
}

#[derive(Clone, Copy, Debug, ValueEnum)]
pub(crate) enum Propagator {
    #[value(name = "tracecontext")]
    TraceContext,
    Baggage,
    None,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args, Debug)]
#[group(skip)]
#[command(next_help_heading = "Telemetry")]
pub struct Options {
    #[arg(long, global = true, env = EnvFilter::DEFAULT_ENV, value_name = "DIRECTIVE", value_delimiter = ',', help = "Tracing filter directives over the program's defaults")]
    pub(crate) log: Vec<Directive>,
    #[arg(long, global = true, env = "OTEL_SERVICE_NAME", value_name = "NAME", help = "Service name over the program name, its attributes, and OTEL_RESOURCE_ATTRIBUTES")]
    pub(crate) service_name: Option<String>,
    #[arg(long, global = true, env = "OTEL_PROPAGATORS", value_name = "PROPAGATOR", value_enum, value_delimiter = ',', default_values_t = [Propagator::TraceContext, Propagator::Baggage])]
    #[arg(help = "Context propagators every carrier injects and extracts")]
    pub(crate) propagators: Vec<Propagator>,
    #[arg(long, global = true, env = "OTEL_TRACES_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving spans")]
    pub(crate) traces_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_METRICS_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving metrics")]
    pub(crate) metrics_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_LOGS_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving log records")]
    pub(crate) logs_exporter: Exporter,
}
