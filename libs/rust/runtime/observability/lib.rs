#![feature(const_result_trait_fn, const_trait_impl, default_field_values, error_generic_member_access, error_reporter, integer_casts)]
#![cfg_attr(windows, feature(windows_by_handle))]
#![cfg_attr(any(feature = "export", feature = "metrics"), feature(nonpoison_mutex, sync_nonpoison))]
#![cfg_attr(feature = "metrics", feature(array_try_map))]

use core::iter;
use std::borrow::Cow;
#[cfg(feature = "export")]
use std::io;
use std::process::Command;

use enum_map::EnumMap;
#[cfg(any(feature = "export", feature = "metrics"))]
use opentelemetry::InstrumentationScope;
use opentelemetry::propagation::Injector;
#[cfg(feature = "export")]
use opentelemetry_otlp::ExporterBuildError;
#[cfg(feature = "export")]
use opentelemetry_sdk::error::OTelSdkError;
#[cfg(any(feature = "export", feature = "metrics"))]
use opentelemetry_semantic_conventions::SCHEMA_URL;
use tracing_subscriber::filter::LevelFilter;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, derive_more::Display, enum_map::Enum)]
#[display(rename_all = "lowercase")]
pub enum Signal {
    Traces,
    Metrics,
    Logs,
}

#[derive(Clone, Copy, Debug, Default, PartialEq, Eq, Hash, derive_more::IsVariant)]
#[cfg_attr(feature = "clap", derive(clap::ValueEnum), value(rename_all = "lowercase"))]
pub enum Exporter {
    #[default]
    None,
    Otlp,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash)]
#[cfg_attr(feature = "clap", derive(clap::ValueEnum), value(rename_all = "lowercase"))]
pub enum Propagator {
    TraceContext,
    Baggage,
    None,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const OTEL_SDK_DISABLED: &str = "OTEL_SDK_DISABLED";
pub const OTEL_SERVICE_NAME: &str = "OTEL_SERVICE_NAME";
pub const OTEL_PROPAGATORS: &str = "OTEL_PROPAGATORS";
pub const OTEL_SIGNAL_EXPORTER: EnumMap<Signal, &str> = EnumMap::from_array(["OTEL_TRACES_EXPORTER", "OTEL_METRICS_EXPORTER", "OTEL_LOGS_EXPORTER"]);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Debug, Default, PartialEq, Eq)]
pub struct Settings {
    pub sdk_disabled: bool = false,
    pub exporters: EnumMap<Signal, Exporter> = EnumMap::from_array([Exporter::None; 3]),
    pub propagators: Cow<'static, [Propagator]> = Cow::Borrowed(&[Propagator::TraceContext, Propagator::Baggage]),
    pub service_name: Option<String> = None,
    pub span_events: LevelFilter = LevelFilter::WARN,
}

impl Settings {
    #[must_use]
    pub fn exported(&self) -> EnumMap<Signal, bool> { self.exporters.map(|_, exporter| !self.sdk_disabled && exporter.is_otlp()) }
}

#[derive(Debug)]
pub struct Environment<'command>(pub &'command mut Command);

impl Injector for Environment<'_> {
    fn set(&mut self, key: &str, value: String) {
        let letters = key.chars().map(|letter| if letter.is_ascii_alphanumeric() { letter.to_ascii_uppercase() } else { '_' });
        self.0.env(iter::chain(key.chars().next().is_none_or(|first| first.is_ascii_digit()).then_some('_'), letters).collect::<String>(), value);
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[cfg(feature = "export")]
#[derive(Debug, thiserror::Error)]
pub enum ObservabilityError {
    #[error("cannot build OTLP {signal} exporter")]
    Exporter { signal: Signal, source: ExporterBuildError },
    #[error("cannot start a thread to shut down {signal}")]
    Thread { signal: Signal, source: io::Error },
    #[error("cannot shut down {signal}")]
    Provider { signal: Signal, source: OTelSdkError },
}

#[cfg(feature = "export")]
impl From<ObservabilityError> for sysexits::ExitCode {
    fn from(value: ObservabilityError) -> Self {
        match value {
            ObservabilityError::Exporter { signal: _, source: ExporterBuildError::InvalidConfiguration(_) } => Self::Config,
            ObservabilityError::Exporter { signal: _, source: ExporterBuildError::InternalFailure(_) } | ObservabilityError::Provider { signal: _, source: OTelSdkError::AlreadyShutdown } =>
                Self::Software,
            ObservabilityError::Thread { signal: _, source } => source.into(),
            ObservabilityError::Provider { signal: _, source: OTelSdkError::Timeout(_) } => Self::TempFail,
            ObservabilityError::Provider { signal: _, source: OTelSdkError::InternalFailure(_) } => Self::Unavailable,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(any(feature = "export", feature = "metrics"))]
fn scope() -> InstrumentationScope { InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_version(env!("CARGO_PKG_VERSION")).with_schema_url(SCHEMA_URL).build() }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(feature = "export")]
pub mod export;
pub mod logs;
#[cfg(feature = "metrics")]
pub mod metrics;
pub mod panics;
pub mod rotation;
