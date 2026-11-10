use core::error::Error;
use core::mem::DropGuard;
use core::num::NonZero;
use core::str::FromStr as _;
use core::time::Duration;
use core::{any, iter};
use std::error::Report;
use std::ffi::{OsStr, OsString};
use std::io;
use std::panic::{self, PanicHookInfo};
use std::process::{self, ExitCode, Termination};
use std::sync::nonpoison::Mutex;
use std::sync::{Arc, OnceLock};

use anstream::{AutoStream, ColorChoice};
use bytesize::ByteSize;
use camino::Utf8PathBuf;
use clap::builder::{PossibleValue, TypedValueParser};
use clap::error::ContextKind;
use clap::parser::ValueSource;
use clap::{Arg, ArgMatches, Args, Command, ValueEnum, ValueHint, value_parser};
use enum_map::EnumMap;
use itertools::{Either, Itertools as _};
use jiff::{Span, SpanRelativeTo};
use log::SetLoggerError;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use opentelemetry::propagation::Extractor;
use opentelemetry::{Context, KeyValue, global};
use opentelemetry_semantic_conventions::attribute::{EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, EXCEPTION_TYPE, SERVICE_NAME, SERVICE_VERSION};
use rasm_observability::export::{self, Providers};
use rasm_observability::logs::Json;
use rasm_observability::panics::{self, PANIC};
use rasm_observability::rotation::{Age, RotatingFile, Rotation};
use rasm_observability::{Exporter, OTEL_PROPAGATORS, OTEL_SDK_DISABLED, OTEL_SERVICE_NAME, OTEL_SIGNAL_EXPORTER, Propagator, Settings, Signal, metrics};
use tokio::runtime::Runtime;
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

use crate::{CommandError, Folder};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait MakeWriter = for<'writer> fmt::MakeWriter<'writer> + Clone + Send + Sync + 'static;

pub trait Layer = layer::Layer<Layered<reload::Layer<EnvFilter, Registry>, Registry>> + Send + Sync + 'static;

pub trait Failure = Error + Into<sysexits::ExitCode> + 'static;

type Stated<T> = Result<Option<T>, Refusal>;

type Hook = Arc<dyn Fn(&PanicHookInfo<'_>) + Send + Sync>;

#[derive(Clone, Copy, Debug, Default, PartialEq, Eq, ValueEnum)]
enum Target {
    #[default]
    File,
    Stderr,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Program {
    pub scopes: Vec<(&'static str, Option<LevelFilter>)>,
    pub level: LevelFilter = LevelFilter::INFO,
    pub attributes: Vec<KeyValue> = Vec::new(),
}

#[derive(Clone, Debug)]
struct Refusal {
    setting: Option<Box<str>>,
    error: Arc<clap::Error>,
}

#[derive(Clone, Debug)]
struct Lenient<P>(P);

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
pub(crate) struct Options {
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
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of the log file, default the per-user log folder")]
    log_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of cached files, default the per-user cache folder")]
    cache_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of kept state, default the per-user state folder")]
    state_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of sockets and locks, default the per-user runtime folder")]
    runtime_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, env = OTEL_SERVICE_NAME, value_name = "NAME", help = "Service name over program defaults and OTEL_RESOURCE_ATTRIBUTES")]
    service_name: Option<String>,
    #[arg(long, global = true, env = OTEL_PROPAGATORS, value_name = "PROPAGATORS", value_delimiter = ',', value_parser = Lenient(value_parser!(Propagator)))]
    #[arg(help = "Propagators injecting and extracting context, W3C trace context and baggage when unset")]
    propagators: Vec<Stated<Propagator>>,
    #[arg(long, global = true, env = OTEL_SIGNAL_EXPORTER[Signal::Traces], value_name = "EXPORTER", value_parser = Lenient(value_parser!(Exporter)))]
    #[arg(help = "Exporter receiving spans, none when unset")]
    traces_exporter: Option<Stated<Exporter>>,
    #[arg(long, global = true, env = OTEL_SIGNAL_EXPORTER[Signal::Metrics], value_name = "EXPORTER", value_parser = Lenient(value_parser!(Exporter)))]
    #[arg(help = "Exporter receiving metrics, none when unset")]
    metrics_exporter: Option<Stated<Exporter>>,
    #[arg(long, global = true, env = OTEL_SIGNAL_EXPORTER[Signal::Logs], value_name = "EXPORTER", value_parser = Lenient(value_parser!(Exporter)))]
    #[arg(help = "Exporter receiving log records, none when unset")]
    logs_exporter: Option<Stated<Exporter>>,
    #[arg(long, global = true, env = OTEL_SDK_DISABLED, value_name = "BOOL", value_parser = Lenient(|text: &str| text.to_ascii_lowercase().parse::<bool>()), help = "Disable every signal's exporter")]
    sdk_disabled: Option<Stated<bool>>,
    #[command(flatten)]
    parent: Parent,
}

impl Options {
    pub(crate) fn stated(command: &Command, matches: &ArgMatches) -> [Vec<(OsString, OsString)>; 2] {
        let declared = |augment: fn(Command) -> Command| {
            Vec::from_iter(augment(Command::new(command.get_name().to_owned())).get_arguments().filter(|argument| argument.get_env().is_some()).map(|argument| argument.get_id().clone()))
        };
        let (exported, carried) = (declared(Self::augment_args), declared(Parent::augment_args));
        let rows = command.get_arguments().filter(|argument| !carried.contains(argument.get_id())).filter_map(|argument| {
            let (id, variable) = (argument.get_id(), argument.get_env()?);
            let mut values = matches.get_raw(id.as_str()).filter(|_| matches!(matches.value_source(id.as_str()), Some(ValueSource::CommandLine | ValueSource::EnvVariable)))?;
            let value = match argument.get_value_delimiter() {
                Some(delimiter) => values.collect_vec().join(OsStr::new(delimiter.encode_utf8(&mut [0; char::MAX_LEN_UTF8]))),
                None => values.next_back()?.to_owned(),
            };
            Some((exported.contains(id), (variable.to_owned(), value)))
        });
        let (export, program) = rows.partition_map(|(exported, row)| if exported { Either::Left(row) } else { Either::Right(row) });
        [program, export]
    }

    pub(crate) fn start<W, L>(self, command: &Command, program: Program, stderr: W, layer: L, runtime: &Runtime) -> Result<Started, NEVec<CommandError>>
    where
        W: MakeWriter,
        L: Layer,
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
        let Self {
            log_level,
            log_filter,
            log_target,
            log_size,
            log_age,
            log_files,
            log_dir,
            cache_dir,
            state_dir,
            runtime_dir,
            service_name,
            propagators,
            traces_exporter,
            metrics_exporter,
            logs_exporter,
            sdk_disabled,
            parent,
        } = self;
        let Program { scopes, level, attributes } = program;
        let (bounds, defaults, mut refused) = (Rotation { .. }, Settings { .. }, Vec::new());
        let level = stated(log_level, &mut refused).unwrap_or(level);
        let directives = Vec::from_iter(log_filter.into_iter().filter_map(|directive| stated(Some(directive), &mut refused)));
        let rotation = Rotation {
            bytes: stated(log_size, &mut refused).unwrap_or(bounds.bytes),
            age: stated(log_age, &mut refused).unwrap_or(bounds.age),
            files: stated(log_files, &mut refused).unwrap_or(bounds.files),
        };
        let chosen = EnumMap::<Folder, _>::from_array([log_dir, cache_dir, state_dir, runtime_dir].map(|folder| stated(folder, &mut refused)));
        let folders = chosen.map(|folder, chosen| {
            let shared = || match folder {
                Folder::Log => cfg_select! {
                    target_vendor = "apple" => dirs::home_dir().map(|home| home.join("Library").join("Logs")),
                    windows => dirs::data_local_dir().map(|data| data.join("logs")),
                    _ => dirs::state_dir(),
                },
                Folder::Cache => dirs::cache_dir(),
                Folder::State => dirs::state_dir().or_else(dirs::data_local_dir),
                Folder::Runtime => dirs::runtime_dir().or_else(dirs::cache_dir),
            };
            chosen.or_else(|| Utf8PathBuf::from_path_buf(shared()?.join(command.get_name())).ok())
        });
        let named = Vec::from_iter(propagators.into_iter().filter_map(|propagator| stated(Some(propagator), &mut refused)));
        let settings = Settings {
            sdk_disabled: stated(sdk_disabled, &mut refused).unwrap_or(defaults.sdk_disabled),
            exporters: EnumMap::from_array([traces_exporter, metrics_exporter, logs_exporter].map(|exporter| stated(exporter, &mut refused).unwrap_or_default())),
            propagators: if named.is_empty() { defaults.propagators } else { named.into() },
            service_name,
            span_events: defaults.span_events,
        };
        let described = iter::chain(iter::once(KeyValue::new(SERVICE_NAME, command.get_name().to_owned())), command.get_version().map(|version| KeyValue::new(SERVICE_VERSION, version.to_owned())));
        let resource = export::resource(&settings, described.chain(attributes));
        let json = Json::new(|key| resource.get(key));
        let file = match stated(log_target, &mut refused).unwrap_or_default() {
            Target::Stderr => None,
            Target::File => {
                let path = folders[Folder::Log].as_ref().ok_or(CommandError::Folder { folder: Folder::Log })?.join(command.get_name()).with_extension("log");
                Some(Arc::new(Appender { path, rotation, lines: OnceLock::new(), worker: Mutex::new(None) }))
            }
        };
        let local = Dispatch::new(tracing_subscriber::registry().with(LevelFilter::WARN).with(written(&json, file.as_ref(), stderr.clone())));
        let started = dispatcher::with_default(&local, || {
            (try bikeshed Result<Started, NEVec<CommandError>> {
                let (filtered, filter) = Filter::new(scopes.into(), directives.into(), level)?;
                let providers = Providers::otlp(&settings, &resource).map_err(|failures| failures.into_nonempty_iter().map(CommandError::from).collect::<NEVec<_>>())?;
                let records = tracing_subscriber::registry().with(filtered).with(layer).with(written(&json, file.as_ref(), stderr)).with(providers.layer(&settings));
                Validated::from(subscriber::set_global_default(records).map_err(|SetGlobalDefaultError { .. }| CommandError::Dispatch))
                    .map2(Validated::from(LogTracer::init().map_err(|SetLoggerError { .. }| CommandError::Forward)), |(), ()| ())
                    .ok()?;
                global::set_tracer_provider(providers.tracer.clone());
                global::set_text_map_propagator(export::propagator(&settings));
                if let Some(meter) = &providers.meter {
                    global::set_meter_provider(meter.clone());
                    metrics::runtime(meter, runtime.handle());
                    metrics::process(meter, || [process::id()]).unwrap_or_else(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process metrics unavailable"));
                }
                let exporting = providers.exported.values().any(|exported| *exported).then(|| DropGuard::new((providers, local.clone()), stopped as fn(_)));
                let previous = Hook::from(panic::take_hook());
                let chained = Arc::clone(&previous);
                #[cfg(panic = "abort")]
                let flushed = (exporting.as_deref().cloned(), file.clone());
                panic::set_hook(Box::new(move |info| {
                    panics::record(info);
                    #[cfg(panic = "abort")]
                    flushed.0.iter().cloned().for_each(stopped);
                    #[cfg(panic = "abort")]
                    flushed.1.iter().cloned().for_each(Appender::flushed);
                    chained(info);
                }));
                Started {
                    folders,
                    filter,
                    parent: global::get_text_map_propagator(|propagator| propagator.extract(&parent)),
                    providers: exporting,
                    previous,
                    worker: file.clone().map(|file| DropGuard::new(file, Appender::flushed as fn(_))),
                }
            })
            .inspect_err(reported)
        })?;
        for Refusal { setting, error } in &refused {
            let (value, accepted) = (error.get(ContextKind::InvalidValue).map(field::display), error.get(ContextKind::ValidValue).map(field::display));
            tracing::warn!(setting = setting.as_deref(), value, accepted, { EXCEPTION_MESSAGE } = error.source().map(|source| field::display(Report::new(source))), "setting ignored, default stands");
        }
        Ok(started)
    }
}

#[derive(Args, Debug)]
#[group(skip)]
struct Parent {
    #[arg(long, global = true, hide = true, env)]
    traceparent: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    tracestate: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    baggage: Option<String>,
}

impl Parent {
    fn held(&self) -> impl Iterator<Item = (&'static str, &str)> {
        [(stringify!(traceparent), &self.traceparent), (stringify!(tracestate), &self.tracestate), (stringify!(baggage), &self.baggage)]
            .into_iter()
            .filter_map(|(name, value)| Some((name, value.as_deref()?)))
    }
}

impl Extractor for Parent {
    fn get(&self, key: &str) -> Option<&str> { self.held().find_map(|(name, value)| key.eq_ignore_ascii_case(name).then_some(value)) }

    fn keys(&self) -> Vec<&str> { self.held().map(|(name, _)| name).collect() }
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
    fn new(scopes: Arc<[(&'static str, Option<LevelFilter>)]>, directives: Arc<[Directive]>, level: LevelFilter) -> Result<(reload::Layer<EnvFilter, Registry>, Self), CommandError> {
        let (layer, handle) = reload::Layer::new(Self::directed(&scopes, &directives, level)?);
        Ok((layer, Self { handle, scopes, directives, level: Arc::new(Mutex::new(level)) }))
    }

    fn directed(scopes: &[(&'static str, Option<LevelFilter>)], directives: &[Directive], level: LevelFilter) -> Result<EnvFilter, CommandError> {
        let own = scopes.iter().map(|&(scope, fixed)| format!("{scope}={}", fixed.unwrap_or(level)).parse::<Directive>().map_err(|source| CommandError::Scope { scope, source }));
        own.chain(directives.iter().cloned().map(Ok)).try_fold(EnvFilter::default().add_directive(LevelFilter::WARN.into()), |filter, directive| Ok(filter.add_directive(directive?)))
    }

    #[must_use]
    pub fn level(&self) -> LevelFilter { *self.level.lock() }

    pub fn reload(&self, level: LevelFilter) -> Result<(), CommandError> {
        self.handle.reload(Self::directed(&self.scopes, &self.directives, level)?).map(|()| *self.level.lock() = level).map_err(CommandError::Reload)
    }
}

pub(crate) struct Started {
    pub(crate) folders: EnumMap<Folder, Option<Utf8PathBuf>>,
    pub(crate) filter: Filter,
    pub(crate) parent: Context,
    providers: Option<DropGuard<(Providers, Dispatch), fn((Providers, Dispatch))>>,
    previous: Hook,
    worker: Option<DropGuard<Arc<Appender>, fn(Arc<Appender>)>>,
}

impl Started {
    pub(crate) fn finish<T, E>(self, runtime: &Runtime, worked: Result<T, NEVec<E>>) -> ExitCode
    where
        T: Termination,
        E: Failure,
    {
        let Self { folders: _, filter: _, parent: _, providers, previous, worker } = self;
        let worked = worked.inspect_err(reported);
        panic::set_hook(Box::new(move |info| previous(info)));
        if let Some(held) = providers.map(DropGuard::dismiss) {
            runtime.block_on(task::spawn_blocking(move || stopped(held))).unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()));
        }
        drop(worker);
        worked.map_or_else(status, Termination::report)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn stopped((providers, local): (Providers, Dispatch)) {
    const LIMIT: Duration = Duration::from_millis(900);
    providers.shutdown(LIMIT).unwrap_or_else(|errors| dispatcher::with_default(&local, || reported(&errors)));
}

fn reported<F>(errors: &NEVec<F>)
where F: Error + 'static {
    for error in errors {
        let chain = error.source().map(|_| field::display(Report::new(error).pretty(true).show_backtrace(true)));
        tracing::error!({ EXCEPTION_TYPE } = any::type_name::<F>(), { EXCEPTION_MESSAGE } = %Report::new(error), { EXCEPTION_STACKTRACE } = chain, "command failed");
    }
}

pub(crate) fn status<F>(failures: NEVec<F>) -> ExitCode
where F: Into<sysexits::ExitCode> {
    sysexits::ExitCode::report(failures.into_nonempty_iter().next().0.into())
}
