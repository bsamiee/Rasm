use core::error::Error;
use core::mem::DropGuard;
use core::num::NonZero;
use core::str::FromStr as _;
use core::time::Duration;
use core::{any, iter};
use std::collections::HashMap;
use std::error::Report;
use std::ffi::OsStr;
use std::panic::{self, PanicHookInfo};
use std::sync::nonpoison::Mutex;
use std::sync::{Arc, OnceLock};
use std::{io, process};

use anstream::{AutoStream, ColorChoice};
use bytesize::ByteSize;
use camino::Utf8PathBuf;
use clap::builder::{PossibleValue, TypedValueParser};
use clap::error::ContextKind;
use clap::{Arg, Args, Command, ValueEnum, value_parser};
use enum_map::EnumMap;
use itertools::{Either, Itertools as _};
use jiff::{Span, SpanRelativeTo};
use log::SetLoggerError;
use nonempty_collections::NEVec;
use opentelemetry::propagation::{TextMapCompositePropagator, TextMapPropagator};
use opentelemetry::{Context, KeyValue, global};
use opentelemetry_sdk::propagation::{BaggagePropagator, TraceContextPropagator};
use opentelemetry_semantic_conventions::attribute::{EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, EXCEPTION_TYPE, SERVICE_NAME, SERVICE_VERSION};
use tokio::task;
use tracing::subscriber::{self, SetGlobalDefaultError};
use tracing::{Dispatch, Level, Subscriber, dispatcher, field};
use tracing_appender::non_blocking::{NonBlocking, NonBlockingBuilder, WorkerGuard};
use tracing_log::LogTracer;
use tracing_subscriber::filter::{Directive, LevelFilter, filter_fn};
use tracing_subscriber::layer::{self, Layered, SubscriberExt as _};
use tracing_subscriber::registry::LookupSpan;
use tracing_subscriber::{EnvFilter, Registry, fmt, reload};
use validated::Validated;

use crate::export::{self, Providers};
use crate::logs::Json;
use crate::rotation::{Age, RotatingFile, Rotation};
use crate::{ObservabilityError, PANIC, metrics};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait MakeWriter = for<'writer> fmt::MakeWriter<'writer> + Clone + Send + Sync + 'static;

pub trait Layer = layer::Layer<Layered<reload::Layer<EnvFilter, Registry>, Registry>> + Send + Sync + 'static;

pub type Stated<T> = Result<Option<T>, Refusal>;

#[derive(Clone, Copy, Debug, Default, PartialEq, Eq, ValueEnum)]
enum Target {
    #[default]
    File,
    Stderr,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, ValueEnum)]
#[value(rename_all = "lowercase")]
enum Exporter {
    None,
    Otlp,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ValueEnum)]
#[value(rename_all = "lowercase")]
enum Propagator {
    TraceContext,
    Baggage,
    None,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Program {
    pub scopes: Vec<(&'static str, Option<LevelFilter>)>,
    pub level: LevelFilter = LevelFilter::INFO,
    pub attributes: Vec<KeyValue> = Vec::new(),
    pub refused: Vec<Refusal> = Vec::new(),
}

#[derive(Clone, Debug)]
pub struct Refusal {
    setting: Option<Box<str>>,
    error: Arc<clap::Error>,
}

#[derive(Clone, Debug)]
pub struct Lenient<P>(pub P);

impl<P> TypedValueParser for Lenient<P>
where P: TypedValueParser
{
    type Value = Stated<P::Value>;

    fn parse_ref(&self, cmd: &Command, arg: Option<&Arg>, value: &OsStr) -> Result<Self::Value, clap::Error> {
        let named = || arg.and_then(|arg| arg.get_env().and_then(OsStr::to_str).or_else(|| arg.get_long()));
        Ok(if value.is_empty() { Ok(None) } else { self.0.parse_ref(cmd, arg, value).map(Some).map_err(|error| Refusal { setting: named().map(Box::from), error: Arc::new(error) }) })
    }

    fn possible_values(&self) -> Option<Box<dyn Iterator<Item = PossibleValue> + '_>> { self.0.possible_values() }
}

#[derive(Args, Debug)]
#[group(skip)]
#[command(next_help_heading = "Process")]
pub struct Options {
    #[arg(long, global = true, value_name = "LEVEL", value_parser = Lenient(LevelFilter::from_str), help = "Least severity recorded for the program's own scopes, default info")]
    log_level: Option<Stated<LevelFilter>>,
    #[arg(long, global = true, value_name = "SCOPE=LEVEL", value_delimiter = ',', value_parser = Lenient(Directive::from_str))]
    #[arg(help = "Tracing filter directives over the program level, every scope outside the program at warn when unset")]
    log_filter: Vec<Stated<Directive>>,
    #[arg(long, global = true, value_name = "TARGET", value_parser = Lenient(value_parser!(Target)))]
    #[arg(help = "Records to a rotating file with warnings as text on standard error, or to standard error alone, default file")]
    log_target: Option<Stated<Target>>,
    #[arg(long, global = true, value_name = "SIZE", value_parser = Lenient(TypedValueParser::try_map(ByteSize::from_str, |size| NonZero::try_from(size.as_u64()))))]
    #[arg(help = format!("Bytes at which the log file rotates, default {}", ByteSize(Rotation { .. }.bytes.get())))]
    log_size: Option<Stated<NonZero<u64>>>,
    #[arg(long, global = true, value_name = "SPAN")]
    #[arg(value_parser = Lenient(TypedValueParser::try_map(TypedValueParser::try_map(Span::from_str, |span| span.to_duration(SpanRelativeTo::days_are_24_hours())), Age::try_new)))]
    #[arg(help = format!("Age at which the log file rotates, default {:#}", *Rotation { .. }.age))]
    log_age: Option<Stated<Age>>,
    #[arg(long, global = true, value_name = "COUNT", value_parser = Lenient(NonZero::<usize>::from_str))]
    #[arg(help = format!("Rotated log files kept, oldest removed first, default {}", Rotation { .. }.files))]
    log_files: Option<Stated<NonZero<usize>>>,
    #[arg(long, global = true, env = "OTEL_SERVICE_NAME", value_name = "NAME", help = "Service name over program defaults and OTEL_RESOURCE_ATTRIBUTES")]
    service_name: Option<String>,
    #[arg(long, global = true, env = "OTEL_PROPAGATORS", value_name = "PROPAGATORS", value_delimiter = ',', value_parser = Lenient(value_parser!(Propagator)))]
    #[arg(help = "Propagators injecting and extracting context, W3C trace context and baggage when unset")]
    propagators: Vec<Stated<Propagator>>,
    #[arg(long, global = true, env = "OTEL_TRACES_EXPORTER", value_name = "EXPORTER", value_parser = Lenient(value_parser!(Exporter)), help = "Exporter receiving spans, none when unset")]
    traces_exporter: Option<Stated<Exporter>>,
    #[arg(long, global = true, env = "OTEL_METRICS_EXPORTER", value_name = "EXPORTER", value_parser = Lenient(value_parser!(Exporter)), help = "Exporter receiving metrics, none when unset")]
    metrics_exporter: Option<Stated<Exporter>>,
    #[arg(long, global = true, env = "OTEL_LOGS_EXPORTER", value_name = "EXPORTER", value_parser = Lenient(value_parser!(Exporter)), help = "Exporter receiving log records, none when unset")]
    logs_exporter: Option<Stated<Exporter>>,
    #[arg(long, global = true, env = "OTEL_SDK_DISABLED", value_name = "BOOL", value_parser = Lenient(|text: &str| text.to_ascii_lowercase().parse::<bool>()))]
    #[arg(help = "Disable every signal's exporter")]
    sdk_disabled: Option<Stated<bool>>,
    #[arg(long, global = true, hide = true, env)]
    traceparent: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    tracestate: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    baggage: Option<String>,
}

impl Options {
    #[must_use]
    pub fn exported(variable: &str) -> bool { variable.starts_with("OTEL_") }

    pub async fn instrumented<W, L, T, E>(
        self,
        command: &Command,
        program: Program,
        directory: Option<Utf8PathBuf>,
        stderr: W,
        layer: L,
        work: impl AsyncFnOnce(Filter, Context) -> Result<T, NEVec<E>>,
    ) -> Result<T, Either<NEVec<ObservabilityError>, NEVec<E>>>
    where
        W: MakeWriter,
        L: Layer,
        E: Error + 'static,
    {
        fn stated<T>(value: Option<Stated<T>>, refused: &mut Vec<Refusal>) -> Option<T> {
            match value? {
                Ok(value) => value,
                Err(refusal) => {
                    refused.push(refusal);
                    None
                }
            }
        }
        fn written<S, W>(json: &Json, file: Option<&Arc<Appender>>, stderr: W) -> impl layer::Layer<S> + Send + Sync + 'static
        where
            S: Subscriber + for<'span> LookupSpan<'span>,
            W: MakeWriter,
        {
            let lines = file.map(|file| fmt::layer().event_format(json.clone()).with_writer(Arc::clone(file)));
            let shown = filter_fn(|metadata| *metadata.level() <= Level::WARN && metadata.name() != PANIC);
            let text = fmt::layer().without_time().with_ansi(AutoStream::choice(&io::stderr()) != ColorChoice::Never).with_writer(stderr.clone());
            let records = file.is_none().then(|| fmt::layer().event_format(json.clone()).with_writer(stderr));
            layer::Layer::with_filter(layer::Layer::and_then(layer::Layer::and_then(lines, records), file.map(|_| layer::Layer::with_filter(text, shown))), export::diagnostics())
        }
        fn reported<F>(errors: &NEVec<F>)
        where F: Error + 'static {
            for error in errors {
                let chain = error.source().map(|_| field::display(Report::new(error).pretty(true).show_backtrace(true)));
                tracing::error!({ EXCEPTION_TYPE } = any::type_name::<F>(), { EXCEPTION_MESSAGE } = %Report::new(error), { EXCEPTION_STACKTRACE } = chain, "command failed");
            }
        }
        fn stopped((providers, local): (Providers, Dispatch)) {
            const LIMIT: Duration = Duration::from_millis(900);
            providers.shutdown(LIMIT).unwrap_or_else(|errors| dispatcher::with_default(&local, || reported(&errors)));
        }
        let (Program { scopes, level, attributes, mut refused }, bounds) = (program, Rotation { .. });
        let level = stated(self.log_level, &mut refused).unwrap_or(level);
        let directives = Vec::from_iter(self.log_filter.into_iter().filter_map(|directive| stated(Some(directive), &mut refused)));
        let rotation = Rotation {
            bytes: stated(self.log_size, &mut refused).unwrap_or(bounds.bytes),
            age: stated(self.log_age, &mut refused).unwrap_or(bounds.age),
            files: stated(self.log_files, &mut refused).unwrap_or(bounds.files),
        };
        let named = Vec::from_iter(self.propagators.into_iter().filter_map(|propagator| stated(Some(propagator), &mut refused)));
        let propagators = if named.is_empty() { vec![Propagator::TraceContext, Propagator::Baggage] } else { named };
        let composed = propagators.into_iter().unique().filter_map(|propagator| -> Option<Box<dyn TextMapPropagator + Send + Sync>> {
            match propagator {
                Propagator::TraceContext => Some(Box::new(TraceContextPropagator::new())),
                Propagator::Baggage => Some(Box::new(BaggagePropagator::new())),
                Propagator::None => None,
            }
        });
        let enabled = !stated(self.sdk_disabled, &mut refused).unwrap_or(false);
        let exported = EnumMap::from_array([self.traces_exporter, self.metrics_exporter, self.logs_exporter].map(|exporter| stated(exporter, &mut refused) == Some(Exporter::Otlp) && enabled));
        let (described, detected) = ([KeyValue::new(SERVICE_NAME, command.get_name().to_owned())], exported.values().any(|exported| *exported));
        let versioned = command.get_version().map(|version| KeyValue::new(SERVICE_VERSION, version.to_owned()));
        let resource = export::resource(exported, iter::chain(described, versioned).chain(attributes)).with_attributes(self.service_name.map(|name| KeyValue::new(SERVICE_NAME, name))).build();
        let json = Json::new(|key| resource.get(key));
        let file = match (stated(self.log_target, &mut refused).unwrap_or_default(), directory) {
            (Target::Stderr, Some(_) | None) => Ok(None),
            (Target::File, None) => Err(ObservabilityError::Folder),
            (Target::File, Some(directory)) => Ok(Some(Arc::new(Appender { path: directory.join(format!("{}.log", command.get_name())), rotation, lines: OnceLock::new(), worker: Mutex::new(None) }))),
        };
        let fields = [(stringify!(traceparent), self.traceparent), (stringify!(tracestate), self.tracestate), (stringify!(baggage), self.baggage)];
        let carrier = HashMap::<_, _>::from_iter(fields.into_iter().filter_map(|(name, value)| Some((name.to_owned(), value?))));
        let located = file.as_ref().ok().and_then(Option::as_ref);
        let local = Dispatch::new(tracing_subscriber::registry().with(LevelFilter::WARN).with(written(&json, located, stderr.clone())));
        let started = dispatcher::with_default(&local, || {
            (try bikeshed Result<_, NEVec<ObservabilityError>> {
                let file = file?;
                let (filtered, handle) = reload::Layer::new(Filter::directed(&scopes, &directives, level)?);
                let filter = Filter { handle, scopes: scopes.into(), directives: directives.into(), level: Arc::new(Mutex::new(level)) };
                let providers = Providers::otlp(exported, &resource)?;
                let records = tracing_subscriber::registry().with(filtered).with(layer).with(written(&json, file.as_ref(), stderr)).with(providers.layer());
                Validated::from(subscriber::set_global_default(records).map_err(|SetGlobalDefaultError { .. }| ObservabilityError::Dispatch))
                    .map2(Validated::from(LogTracer::init().map_err(|SetLoggerError { .. }| ObservabilityError::Forward)), |(), ()| ())
                    .ok()?;
                global::set_tracer_provider(providers.tracer.clone());
                global::set_text_map_propagator(TextMapCompositePropagator::new(composed.collect()));
                let parent = global::get_text_map_propagator(|propagator| propagator.extract(&carrier));
                if let Some(meter) = &providers.meter {
                    global::set_meter_provider(meter.clone());
                    opentelemetry_instrumentation_tokio::observe_current_runtime();
                    metrics::process(meter, || [process::id()]).unwrap_or_else(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process metrics unavailable"));
                }
                let exporting = detected.then(|| DropGuard::new((providers, local.clone()), stopped as fn(_)));
                let previous = Arc::<dyn Fn(&PanicHookInfo<'_>) + Send + Sync>::from(panic::take_hook());
                let chained = Arc::clone(&previous);
                #[cfg(panic = "abort")]
                let flushed = (exporting.as_deref().cloned(), file.clone());
                panic::set_hook(Box::new(move |info| {
                    crate::panicked(info);
                    #[cfg(panic = "abort")]
                    flushed.0.iter().cloned().for_each(stopped);
                    #[cfg(panic = "abort")]
                    flushed.1.iter().cloned().for_each(Appender::flushed);
                    chained(info);
                }));
                (file.map(|file| DropGuard::new(file, Appender::flushed as fn(_))), filter, parent, exporting, previous)
            })
            .inspect_err(reported)
        });
        let (appended, filter, parent, exporting, previous) = started.map_err(Either::Left)?;
        for Refusal { setting, error } in &refused {
            let (value, accepted) = (error.get(ContextKind::InvalidValue).map(field::display), error.get(ContextKind::ValidValue).map(field::display));
            tracing::warn!(setting = setting.as_deref(), value, accepted, { EXCEPTION_MESSAGE } = error.source().map(|source| field::display(Report::new(source))), "setting ignored, default stands");
        }
        let worked = work(filter, parent).await.inspect_err(reported);
        panic::set_hook(Box::new(move |info| previous(info)));
        if let Some(held) = exporting.map(DropGuard::dismiss) {
            task::spawn_blocking(move || stopped(held)).await.unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()));
        }
        drop(appended);
        worked.map_err(Either::Right)
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
struct Appender {
    path: Utf8PathBuf,
    rotation: Rotation,
    lines: OnceLock<Result<NonBlocking, Arc<io::Error>>>,
    worker: Mutex<Option<WorkerGuard>>,
}

impl Appender {
    fn flushed(self: Arc<Self>) {
        let worker = self.worker.lock().take();
        drop(worker);
    }
}

impl io::Write for &Appender {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        const QUEUE: usize = 1_024;
        let opened = self.lines.get_or_init(|| {
            let (lines, worker) = NonBlockingBuilder::default().lossy(false).buffered_lines_limit(QUEUE).finish(RotatingFile::open(self.path.clone(), self.rotation).map_err(Arc::new)?);
            *self.worker.lock() = Some(worker);
            Ok(lines)
        });
        match opened {
            Ok(lines) => lines.clone().write(buf),
            Err(error) => Err(io::Error::other(Arc::clone(error))),
        }
    }

    fn flush(&mut self) -> io::Result<()> { Ok(()) }
}

#[derive(Clone, Debug)]
pub struct Filter {
    handle: reload::Handle<EnvFilter, Registry>,
    scopes: Arc<[(&'static str, Option<LevelFilter>)]>,
    directives: Arc<[Directive]>,
    level: Arc<Mutex<LevelFilter>>,
}

impl Filter {
    fn directed(scopes: &[(&'static str, Option<LevelFilter>)], directives: &[Directive], level: LevelFilter) -> Result<EnvFilter, ObservabilityError> {
        let own = scopes.iter().map(|&(scope, fixed)| format!("{scope}={}", fixed.unwrap_or(level)).parse::<Directive>().map_err(|source| ObservabilityError::Scope { scope, source }));
        own.chain(directives.iter().cloned().map(Ok)).try_fold(EnvFilter::default().add_directive(LevelFilter::WARN.into()), |filter, directive| Ok(filter.add_directive(directive?)))
    }

    #[must_use]
    pub fn level(&self) -> LevelFilter { *self.level.lock() }

    pub fn reload(&self, level: LevelFilter) -> Result<(), ObservabilityError> {
        self.handle.reload(Self::directed(&self.scopes, &self.directives, level)?).map(|()| *self.level.lock() = level).map_err(ObservabilityError::Reload)
    }
}
