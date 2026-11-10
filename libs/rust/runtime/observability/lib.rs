#![cfg_attr(any(feature = "logs", feature = "metrics"), feature(default_field_values, error_reporter, integer_casts))]
#![cfg_attr(feature = "logs", feature(const_result_trait_fn, const_trait_impl, error_generic_member_access))]
#![cfg_attr(all(feature = "logs", windows), feature(windows_by_handle))]
#![cfg_attr(any(feature = "export", feature = "metrics"), feature(nonpoison_mutex, sync_nonpoison))]
#![cfg_attr(feature = "metrics", feature(array_try_map))]
#![cfg_attr(feature = "process", feature(trait_alias, try_blocks_heterogeneous))]

use core::iter;
use core::panic::Location;
use std::backtrace::Backtrace;
use std::borrow::Cow;
#[cfg(feature = "export")]
use std::io;
use std::panic::PanicHookInfo;
use std::process::Command;

#[cfg(feature = "export")]
use nonempty_collections::NEVec;
use opentelemetry::metrics::Histogram;
use opentelemetry::propagation::{Extractor, Injector};
use opentelemetry::trace::Status;
use opentelemetry::{InstrumentationScope, KeyValue, StringValue, global};
#[cfg(feature = "export")]
use opentelemetry_otlp::ExporterBuildError;
#[cfg(feature = "export")]
use opentelemetry_sdk::error::OTelSdkError;
use opentelemetry_semantic_conventions::SCHEMA_URL;
use opentelemetry_semantic_conventions::attribute::{CODE_COLUMN_NUMBER, CODE_FILE_PATH, CODE_LINE_NUMBER, ERROR_TYPE, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, RPC_RESPONSE_STATUS_CODE};
use tokio::time::Instant;
use tracing::Span;
use tracing_opentelemetry::{OpenTelemetrySpanExt as _, SetParentError};
#[cfg(feature = "process")]
use tracing_subscriber::filter::ParseError;
#[cfg(feature = "process")]
use tracing_subscriber::reload;

// --- [TYPES] ---------------------------------------------------------------------------

#[cfg(feature = "export")]
#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, derive_more::Display, enum_map::Enum)]
#[display(rename_all = "lowercase")]
pub enum Signal {
    Traces,
    Metrics,
    Logs,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const PANIC: &str = "panic";

// --- [MODELS] --------------------------------------------------------------------------

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
    #[cfg(feature = "process")]
    #[error("cannot filter scope {scope}")]
    Scope { scope: &'static str, source: ParseError },
    #[cfg(feature = "process")]
    #[error(transparent)]
    Reload(reload::Error),
    #[cfg(feature = "process")]
    #[error("cannot install tracing subscriber, one is installed")]
    Dispatch,
    #[cfg(feature = "process")]
    #[error("cannot forward log records, a logger is installed")]
    Forward,
    #[cfg(feature = "process")]
    #[error("log target `file` has no folder, pass a log folder")]
    Folder,
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
            #[cfg(feature = "process")]
            ObservabilityError::Scope { scope: _, source: _ } | ObservabilityError::Reload(_) | ObservabilityError::Dispatch | ObservabilityError::Forward => Self::Software,
            #[cfg(feature = "process")]
            ObservabilityError::Folder => Self::Config,
        }
    }
}

#[cfg(feature = "export")]
impl From<ObservabilityError> for NEVec<ObservabilityError> {
    fn from(value: ObservabilityError) -> Self { Self::new(value) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Call {
    pub span: Span,
    pub started: Instant,
    pub attributes: Vec<KeyValue>,
}

impl Call {
    pub fn start(span: Span, parent: Option<&impl Extractor>, attributes: impl IntoIterator<Item = KeyValue>) -> Self {
        if let Some(carrier) = parent {
            let context = global::get_text_map_propagator(|propagator| propagator.extract(carrier));
            let (Ok(()) | Err(SetParentError::LayerNotFound | SetParentError::SpanDisabled | SetParentError::AlreadyStarted)) = span.set_parent(context);
        }
        Self { span, started: Instant::now(), attributes: attributes.into_iter().collect() }
    }

    pub fn finish(self, duration: &Histogram<f64>, status: Option<StringValue>, failure: Option<(StringValue, Cow<'static, str>)>) {
        let (Self { span, started, mut attributes }, (kind, description)) = (self, failure.unzip());
        span.record(RPC_RESPONSE_STATUS_CODE, status.as_ref().map(StringValue::as_str)).record(ERROR_TYPE, kind.as_ref().map(StringValue::as_str));
        if let Some(description) = description {
            span.set_status(Status::error(description));
        }
        attributes.extend(iter::chain(status.map(|code| KeyValue::new(RPC_RESPONSE_STATUS_CODE, code)), kind.map(|kind| KeyValue::new(ERROR_TYPE, kind))));
        duration.record(started.elapsed().as_secs_f64(), &attributes);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn scope(package: &'static str) -> InstrumentationScope { InstrumentationScope::builder(package).with_schema_url(SCHEMA_URL).build() }

#[must_use]
pub fn duration(package: &'static str, name: &'static str, boundaries: Vec<f64>) -> Histogram<f64> {
    global::meter_with_scope(scope(package)).f64_histogram(name).with_unit("s").with_boundaries(boundaries).build()
}

pub fn panicked(info: &PanicHookInfo<'_>) {
    let location = info.location();
    tracing::error!(
        name: PANIC,
        {
            { EXCEPTION_MESSAGE } = info.payload_as_str(),
            { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(),
            { CODE_FILE_PATH } = location.map(Location::file),
            { CODE_LINE_NUMBER } = location.map(Location::line),
            { CODE_COLUMN_NUMBER } = location.map(Location::column),
        },
        "panicked"
    );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(feature = "export")]
pub mod export;
#[cfg(feature = "logs")]
pub mod logs;
#[cfg(feature = "metrics")]
pub mod metrics;
#[cfg(feature = "process")]
pub mod process;
#[cfg(feature = "logs")]
pub mod rotation;
