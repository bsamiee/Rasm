use core::panic::Location;
#[cfg(feature = "export")]
use core::time::Duration;
use std::backtrace::Backtrace;
use std::panic::PanicHookInfo;

#[cfg(feature = "export")]
use opentelemetry::InstrumentationScope;
#[cfg(feature = "export")]
use opentelemetry::logs::{LogRecord as _, Severity};
#[cfg(feature = "export")]
use opentelemetry_sdk::error::OTelSdkResult;
#[cfg(feature = "export")]
use opentelemetry_sdk::logs::{LogProcessor, SdkLogRecord};
use opentelemetry_semantic_conventions::attribute::{CODE_COLUMN_NUMBER, CODE_FILE_PATH, CODE_LINE_NUMBER, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE};

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const PANIC: &str = "panic";

// --- [SERVICES] ------------------------------------------------------------------------

#[cfg(feature = "export")]
#[derive(Clone, Copy, Debug)]
pub struct Fatal;

#[cfg(feature = "export")]
impl LogProcessor for Fatal {
    fn emit(&self, data: &mut SdkLogRecord, _: &InstrumentationScope) {
        if data.event_name() == Some(PANIC) {
            data.set_severity_number(Severity::Fatal);
            data.set_severity_text(Severity::Fatal.name());
        }
    }

    fn force_flush(&self) -> OTelSdkResult { Ok(()) }

    fn shutdown_with_timeout(&self, _: Duration) -> OTelSdkResult { Ok(()) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn record(info: &PanicHookInfo<'_>) {
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
