use core::error::{self, Error};
use core::fmt;
use std::backtrace::Backtrace;
use std::error::Report;
use std::process;

use opentelemetry::logs::Severity;
use opentelemetry::trace::TraceContextExt as _;
use opentelemetry::{Context, Key};
use opentelemetry_semantic_conventions::attribute::{CODE_FILE_PATH, CODE_LINE_NUMBER, EXCEPTION_STACKTRACE, PROCESS_PID, SERVICE_NAME, SERVICE_VERSION};
use serde_json::{Map, Value};
use tracing::field::{self, Field, Visit};
use tracing::{Event, Level, Subscriber};
use tracing_log::NormalizeEvent as _;
use tracing_subscriber::fmt::format::Writer;
use tracing_subscriber::fmt::time::{FormatTime as _, SystemTime};
use tracing_subscriber::fmt::{FmtContext, FormatEvent, FormatFields};
use tracing_subscriber::registry::LookupSpan;

use crate::panics::PANIC;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Default)]
struct Fields {
    body: Option<Value>,
    attributes: Map<String, Value>,
}

impl Fields {
    fn held(&mut self, field: &Field, value: Value) {
        match field.name() {
            "message" => self.body = Some(value),
            forwarded if forwarded.starts_with("log.") => {}
            name => drop(self.attributes.insert(name.to_owned(), value)),
        }
    }
}

impl Visit for Fields {
    fn record_f64(&mut self, field: &Field, value: f64) { self.held(field, value.into()); }

    fn record_i64(&mut self, field: &Field, value: i64) { self.held(field, value.into()); }

    fn record_u64(&mut self, field: &Field, value: u64) { self.held(field, value.into()); }

    fn record_bool(&mut self, field: &Field, value: bool) { self.held(field, value.into()); }

    fn record_str(&mut self, field: &Field, value: &str) { self.held(field, value.into()); }

    fn record_error(&mut self, field: &Field, value: &(dyn Error + 'static)) {
        if value.source().is_some() || error::request_ref::<Backtrace>(value).is_some() {
            drop(self.attributes.insert(EXCEPTION_STACKTRACE.to_owned(), Report::new(value).pretty(true).show_backtrace(true).to_string().into()));
        }
        self.record_debug(field, &field::display(value));
    }

    fn record_debug(&mut self, field: &Field, value: &dyn fmt::Debug) { self.held(field, fmt::from_fn(|formatter| value.fmt(formatter)).to_string().into()); }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Json {
    resource: Value,
}

impl Json {
    pub fn new(resource: impl Fn(&Key) -> Option<opentelemetry::Value>) -> Self {
        let service = [SERVICE_NAME, SERVICE_VERSION].into_iter().filter_map(|name| Some((name.to_owned(), Value::from(resource(&Key::from_static_str(name))?.as_str().into_owned()))));
        Self { resource: Value::Object(service.chain([(PROCESS_PID.to_owned(), Value::from(process::id()))]).collect()) }
    }
}

impl<S, N> FormatEvent<S, N> for Json
where
    S: Subscriber + for<'span> LookupSpan<'span>,
    N: for<'writer> FormatFields<'writer> + 'static,
{
    fn format_event(&self, _: &FmtContext<'_, S, N>, mut writer: Writer<'_>, event: &Event<'_>) -> fmt::Result {
        let (normalized, mut fields, mut timestamp) = (event.normalized_metadata(), Fields::default(), String::new());
        let metadata = normalized.as_ref().unwrap_or_else(|| event.metadata());
        event.record(&mut fields);
        SystemTime.format_time(&mut Writer::new(&mut timestamp))?;
        let severity = match *metadata.level() {
            _ if metadata.name() == PANIC => Severity::Fatal,
            Level::TRACE => Severity::Trace,
            Level::DEBUG => Severity::Debug,
            Level::INFO => Severity::Info,
            Level::WARN => Severity::Warn,
            Level::ERROR => Severity::Error,
        };
        if severity != Severity::Fatal && *metadata.level() <= Level::WARN {
            let located = [(CODE_FILE_PATH, metadata.file().map(Value::from)), (CODE_LINE_NUMBER, metadata.line().map(Value::from))];
            fields.attributes.extend(located.into_iter().filter_map(|(key, value)| Some((key.to_owned(), value?))));
        }
        let ids = Context::map_current(|context| {
            let span = context.span();
            let held = span.span_context();
            held.is_valid().then(|| [held.trace_id().to_string(), held.span_id().to_string(), format!("{:02x}", held.trace_flags())].map(Value::from))
        });
        let [trace_id, span_id, trace_flags] = ids.map_or([const { None }; 3], |ids| ids.map(Some));
        let record = [
            ("timestamp", Some(Value::from(timestamp))),
            ("severity_text", Some(Value::from(severity.name()))),
            ("severity_number", Some(Value::from(severity as u8))),
            ("body", fields.body),
            ("scope", Some(Value::from(metadata.target()))),
            ("trace_id", trace_id),
            ("span_id", span_id),
            ("trace_flags", trace_flags),
            ("attributes", Some(fields.attributes).filter(|attributes| !attributes.is_empty()).map(Value::Object)),
            ("resource", Some(self.resource.clone())),
        ];
        writeln!(writer, "{}", Value::Object(record.into_iter().filter_map(|(key, value)| Some((key.to_owned(), value?))).collect()))
    }
}
