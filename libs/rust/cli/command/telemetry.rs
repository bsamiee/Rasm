use core::{error::Error, iter, mem::DropGuard};
use std::{
    backtrace::Backtrace,
    borrow::Cow,
    collections::HashMap,
    error::Report,
    ffi::OsStr,
    io,
    panic::{self, PanicHookInfo},
    process::{self, ExitCode, Termination},
    sync::Arc,
    thread,
};

use clap::{
    Arg, Args, Command, Id, ValueEnum,
    builder::{EnumValueParser, PossibleValue, PossibleValuesParser, TypedValueParser},
};
use enum_map::EnumMap;
use itertools::Itertools as _;
use json_subscriber::JsonLayer;
use log::SetLoggerError;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use opentelemetry::{
    Context, InstrumentationScope, Key, KeyValue, global,
    propagation::{TextMapCompositePropagator, TextMapPropagator},
    trace::TracerProvider as _,
};
use opentelemetry_appender_tracing::layer::{OpenTelemetryTracingBridge, TracingSpanAttributes};
use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, SpanExporter};
use opentelemetry_resource_detectors::{ContainerResourceDetector, HostResourceDetector, OsResourceDetector, ProcessResourceDetector, ServiceInstanceIdResourceDetector};
use opentelemetry_sdk::{
    Resource,
    error::OTelSdkError,
    logs::{LoggerProviderBuilder, SdkLoggerProvider},
    metrics::{MeterProviderBuilder, SdkMeterProvider},
    propagation::{BaggagePropagator, TraceContextPropagator},
    resource::{EnvResourceDetector, ResourceDetector as _, TelemetryResourceDetector},
    trace::{SdkTracerProvider, TracerProviderBuilder},
};
use opentelemetry_semantic_conventions::{
    SCHEMA_URL,
    attribute::{CODE_FILE_PATH, CODE_LINE_NUMBER, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, PROCESS_COMMAND_ARGS, SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_VERSION, THREAD_NAME},
};
use tracing::{
    Subscriber,
    subscriber::{self, SetGlobalDefaultError},
};
use tracing_log::LogTracer;
use tracing_subscriber::{
    EnvFilter, Registry,
    filter::{Directive, LevelFilter, ParseError, Targets},
    fmt::{self, time::SystemTime},
    layer::{self, Layered, SubscriberExt as _},
    registry::LookupSpan,
    reload,
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait MakeWriter = for<'writer> fmt::MakeWriter<'writer> + Clone + Send + Sync + 'static;

pub trait Layer = layer::Layer<Layered<reload::Layer<EnvFilter, Registry>, Registry>> + Send + Sync + 'static;

pub trait Failure = Error + Into<sysexits::ExitCode>;

pub type Handle = reload::Handle<EnvFilter, Registry>;

type Setting<V> = Result<Option<V>, Unrecognized>;

#[derive(Clone, Copy, Debug, derive_more::Display, enum_map::Enum)]
#[display(rename_all = "lowercase")]
enum Signal {
    Traces,
    Metrics,
    Logs,
}

#[derive(Clone, Copy, Debug, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Operation {
    #[cfg(panic = "abort")]
    Flush,
    #[display("shut down")]
    Shutdown,
}

#[derive(Clone, Copy, Debug, ValueEnum, derive_more::IsVariant)]
enum Exporter {
    None,
    #[value(help = "OTLP over HTTP/protobuf, settings from OTEL_EXPORTER_OTLP_* variables")]
    Otlp,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, Hash, ValueEnum)]
#[value(rename_all = "lower")]
enum Propagator {
    TraceContext,
    Baggage,
    None,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Defaults {
    pub directives: Cow<'static, str> = Cow::Borrowed("info"),
    pub attributes: Vec<KeyValue> = Vec::new(),
}

#[derive(Clone, Debug)]
struct Unrecognized {
    setting: Id,
    value: Box<str>,
    source: Arc<clap::Error>,
}

#[derive(Clone)]
struct SettingParser<P>(P)
where P: TypedValueParser;

impl<P> TypedValueParser for SettingParser<P>
where P: TypedValueParser
{
    type Value = Setting<P::Value>;

    fn parse_ref(&self, cmd: &Command, arg: Option<&Arg>, value: &OsStr) -> Result<Self::Value, clap::Error> {
        match (value.is_empty(), arg, value.to_str()) {
            (true, _, _) => Ok(Ok(None)),
            (false, Some(arg), Some(text)) =>
                Ok(self.0.parse_ref(cmd, Some(arg), value).map(Some).map_err(|source| Unrecognized { setting: arg.get_id().clone(), value: text.into(), source: Arc::new(source) })),
            (false, None, _) | (false, Some(_), None) => self.0.parse_ref(cmd, arg, value).map(|parsed| Ok(Some(parsed))),
        }
    }

    fn possible_values(&self) -> Option<Box<dyn Iterator<Item = PossibleValue> + '_>> { self.0.possible_values() }
}

#[derive(Args, Debug)]
#[group(skip)]
#[command(next_help_heading = "Telemetry")]
pub struct Options {
    #[arg(long, global = true, env = EnvFilter::DEFAULT_ENV, value_name = "DIRECTIVE", value_delimiter = ',', help = "Tracing filter directives over program defaults, an invalid one ignored")]
    log: Vec<String>,
    #[arg(long, global = true, env = "OTEL_SERVICE_NAME", value_name = "NAME", help = "Service name over program defaults and OTEL_RESOURCE_ATTRIBUTES")]
    service_name: Option<String>,
    #[arg(long, global = true, env = "OTEL_PROPAGATORS", value_name = "PROPAGATOR", value_delimiter = ',', ignore_case = true, value_parser = SettingParser(EnumValueParser::<Propagator>::new()))]
    #[arg(help = "Propagators injecting and extracting context, W3C trace context and baggage when unset")]
    propagators: Vec<Setting<Propagator>>,
    #[arg(long, global = true, env = "OTEL_TRACES_EXPORTER", value_name = "EXPORTER", ignore_case = true, default_value = "none", value_parser = SettingParser(EnumValueParser::<Exporter>::new()))]
    #[arg(help = "Exporter receiving spans")]
    traces_exporter: Setting<Exporter>,
    #[arg(long, global = true, env = "OTEL_METRICS_EXPORTER", value_name = "EXPORTER", ignore_case = true, default_value = "none", value_parser = SettingParser(EnumValueParser::<Exporter>::new()))]
    #[arg(help = "Exporter receiving metrics")]
    metrics_exporter: Setting<Exporter>,
    #[arg(long, global = true, env = "OTEL_LOGS_EXPORTER", value_name = "EXPORTER", ignore_case = true, default_value = "none", value_parser = SettingParser(EnumValueParser::<Exporter>::new()))]
    #[arg(help = "Exporter receiving log records")]
    logs_exporter: Setting<Exporter>,
    #[arg(long, global = true, env = "OTEL_SDK_DISABLED", value_name = "BOOL", ignore_case = true, default_value = "false", help = "Disable every signal's exporter")]
    #[arg(value_parser = SettingParser(PossibleValuesParser::new(["true", "false"]).map(|value| value.eq_ignore_ascii_case("true"))))]
    sdk_disabled: Setting<bool>,
    #[arg(long, global = true, hide = true, env)]
    traceparent: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    tracestate: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    baggage: Option<String>,
}

impl Options {
    pub fn run<W, L, T, E>(self, command: &Command, defaults: Defaults, writer: W, layer: L, work: impl FnOnce(Handle, LevelFilter) -> Result<T, NEVec<E>>) -> ExitCode
    where
        W: MakeWriter,
        L: Layer,
        T: Termination,
        E: Failure,
    {
        const INTERNAL_LOGS: &str = "opentelemetry=warn";
        const TRANSPORT: [&str; 6] = ["h2", "hyper", "mio", "reqwest", "rustls", "want"];
        fn built<B>(signal: Signal, exporter: Exporter, build: impl FnOnce() -> Result<B, ExporterBuildError>) -> Validated<Option<B>, TelemetryError> {
            match exporter {
                Exporter::None => Validated::Good(None),
                Exporter::Otlp => build().map(Some).map_err(|source| TelemetryError::Build { signal, source }).into(),
            }
        }
        fn json<S, M>(resource: &Resource, writer: M) -> JsonLayer<S, M>
        where
            S: Subscriber + for<'span> LookupSpan<'span>,
            M: MakeWriter,
        {
            let mut layer = JsonLayer::new(writer);
            layer
                .with_timer("timestamp", SystemTime)
                .with_level("level")
                .with_target("target")
                .with_file(CODE_FILE_PATH)
                .with_line_number(CODE_LINE_NUMBER)
                .with_thread_names(THREAD_NAME)
                .with_top_level_flattened_span_list()
                .with_flattened_event()
                .with_opentelemetry_ids(true);
            [SERVICE_NAME, SERVICE_VERSION, SERVICE_INSTANCE_ID]
                .into_iter()
                .filter_map(|name| Some((name, resource.get_ref(&Key::from_static_str(name))?)))
                .for_each(|(name, value)| layer.add_static_field(name, value.as_str().into()));
            layer
        }
        fn reported<F>(errors: &NEVec<F>)
        where F: Error {
            errors.iter().for_each(|error| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "failed"));
        }
        fn status<F>(failures: NEVec<F>) -> ExitCode
        where F: Into<sysexits::ExitCode> {
            sysexits::ExitCode::report(failures.into_nonempty_iter().next().0.into())
        }
        let finished = |operation: Operation, (tracer_provider, meter_provider, logger_provider): &(SdkTracerProvider, SdkMeterProvider, SdkLoggerProvider)| {
            thread::scope(|scope| {
                EnumMap::<Signal, _>::from_fn(|signal| {
                    thread::Builder::new().spawn_scoped(scope, move || match (signal, operation) {
                        #[cfg(panic = "abort")]
                        (Signal::Traces, Operation::Flush) => tracer_provider.force_flush(),
                        (Signal::Traces, Operation::Shutdown) => tracer_provider.shutdown(),
                        #[cfg(panic = "abort")]
                        (Signal::Metrics, Operation::Flush) => meter_provider.force_flush(),
                        (Signal::Metrics, Operation::Shutdown) => meter_provider.shutdown(),
                        #[cfg(panic = "abort")]
                        (Signal::Logs, Operation::Flush) => logger_provider.force_flush(),
                        (Signal::Logs, Operation::Shutdown) => logger_provider.shutdown(),
                    })
                })
                .into_iter()
                .map(|(signal, spawned)| {
                    spawned
                        .map_err(|source| TelemetryError::Spawn { operation, signal, source })?
                        .join()
                        .unwrap_or_else(|payload| panic::resume_unwind(payload))
                        .map_err(|source| TelemetryError::Sdk { operation, signal, source })
                })
                .collect::<Validated<(), _>>()
                .ok()
            })
        };
        let Self { log, service_name, propagators, traces_exporter, metrics_exporter, logs_exporter, sdk_disabled, traceparent, tracestate, baggage } = self;
        let Defaults { directives, attributes } = defaults;
        let exporters = [&traces_exporter, &metrics_exporter, &logs_exporter].map(|exporter| match exporter {
            Ok(Some(exporter)) if !matches!(sdk_disabled, Ok(Some(true))) => *exporter,
            Ok(Some(_) | None) | Err(Unrecognized { setting: _, value: _, source: _ }) => Exporter::None,
        });
        let exported = exporters.iter().any(Exporter::is_otlp);
        let [traces, metrics, logs] = exporters;
        let nonempty = |directive: &&str| !directive.is_empty();
        let default_directives = directives.split(',').filter(nonempty);
        let log = log.iter().map(String::as_str).filter(nonempty);
        let level = iter::chain(default_directives.clone(), log.clone()).filter_map(|directive| directive.parse::<LevelFilter>().ok()).next_back().unwrap_or(LevelFilter::OFF);
        let (accepted, refused): (Vec<Directive>, Vec<_>) = log.map(|directive| directive.parse().map_err(|source: ParseError| (directive, source))).partition_result();
        let resource = Resource::builder_empty()
            .with_schema_url(
                iter::chain([KeyValue::new(SERVICE_NAME, command.get_name().to_owned())], command.get_version().map(|version| KeyValue::new(SERVICE_VERSION, version.to_owned()))),
                SCHEMA_URL,
            )
            .with_detector(Box::new(ServiceInstanceIdResourceDetector));
        let resource = if exported {
            resource
                .with_detectors(&[Box::new(TelemetryResourceDetector), Box::new(OsResourceDetector), Box::new(HostResourceDetector::default()), Box::new(ContainerResourceDetector)])
                .with_attributes(ProcessResourceDetector.detect().iter().filter(|&(key, _)| key.as_str() != PROCESS_COMMAND_ARGS).map(|(key, value)| KeyValue::new(key.clone(), value.clone())))
        } else {
            resource
        }
        .with_detector(Box::new(EnvResourceDetector::new()))
        .with_attributes(iter::chain(attributes, service_name.filter(|name| !name.is_empty()).map(|name| KeyValue::new(SERVICE_NAME, name))))
        .build();
        let started = subscriber::with_default(tracing_subscriber::registry().with(json(&resource, writer.clone())).with(LevelFilter::WARN), || {
            (try bikeshed Result<_, NEVec<TelemetryError>> {
                let (filter, tracer_provider, meter_provider, logger_provider) = iter::chain([INTERNAL_LOGS], default_directives)
                    .map(|directive| directive.parse().map_err(|source| TelemetryError::Filter { directive: directive.into(), source }))
                    .collect::<Validated<Vec<Directive>, _>>()
                    .map4(
                        built(Signal::Traces, traces, || SpanExporter::builder().build()),
                        built(Signal::Metrics, metrics, || MetricExporter::builder().build()),
                        built(Signal::Logs, logs, || LogExporter::builder().build()),
                        |directives, span_exporter, metric_exporter, log_exporter| {
                            (
                                iter::chain(directives, accepted).fold(EnvFilter::default(), EnvFilter::add_directive),
                                span_exporter.into_iter().fold(SdkTracerProvider::builder().with_resource(resource.clone()), TracerProviderBuilder::with_batch_exporter).build(),
                                metric_exporter.into_iter().fold(SdkMeterProvider::builder().with_resource(resource.clone()), MeterProviderBuilder::with_periodic_exporter).build(),
                                log_exporter.into_iter().fold(SdkLoggerProvider::builder().with_resource(resource.clone()), LoggerProviderBuilder::with_batch_exporter).build(),
                            )
                        },
                    )
                    .ok()?;
                let scope = InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_schema_url(SCHEMA_URL).build();
                let (filter, handle) = reload::Layer::new(filter);
                let dispatched = tracing_subscriber::registry().with(filter).with(layer).with(json(&resource, writer)).with(
                    tracing_opentelemetry::layer()
                        .with_tracer(tracer_provider.tracer_with_scope(scope))
                        .and_then(logs.is_otlp().then(|| OpenTelemetryTracingBridge::builder(&logger_provider).with_tracing_span_attributes(TracingSpanAttributes::all()).build()))
                        .with_filter(Targets::new().with_default(LevelFilter::TRACE).with_targets(TRANSPORT.map(|target| (target, LevelFilter::WARN)))),
                );
                Validated::from(subscriber::set_global_default(dispatched).map_err(|SetGlobalDefaultError { .. }| TelemetryError::Dispatch))
                    .map2(Validated::from(LogTracer::init().map_err(|SetLoggerError { .. }| TelemetryError::Forward)), |(), ()| ())
                    .ok()?;
                (tracer_provider, meter_provider, logger_provider, handle)
            })
            .inspect_err(reported)
        });
        let (tracer_provider, meter_provider, logger_provider, handle) = match started {
            Ok(started) => started,
            Err(failures) => return status(failures),
        };
        for (directive, source) in &refused {
            tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(source), directive = *directive, "ignored filter directive");
        }
        for Unrecognized { setting, value, source } in iter::chain(
            propagators.iter().filter_map(|propagator| propagator.as_ref().err()),
            [traces_exporter.as_ref().err(), metrics_exporter.as_ref().err(), logs_exporter.as_ref().err(), sdk_disabled.as_ref().err()].into_iter().flatten(),
        ) {
            let row = command.get_arguments().find(|arg| arg.get_id() == setting);
            let (flag, variable) = (row.and_then(Arg::get_long), row.and_then(Arg::get_env).and_then(OsStr::to_str));
            tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(&**source), flag, variable, value = &**value, "ignored unrecognized value");
        }
        global::set_tracer_provider(tracer_provider.clone());
        global::set_meter_provider(meter_provider.clone());
        global::set_text_map_propagator(TextMapCompositePropagator::new(
            iter::chain(
                propagators.iter().all(|propagator| matches!(propagator, Ok(None))).then_some([Propagator::TraceContext, Propagator::Baggage]).into_iter().flatten(),
                propagators.iter().filter_map(|propagator| propagator.as_ref().ok().copied().flatten()),
            )
            .unique()
            .filter_map(|propagator| -> Option<Box<dyn TextMapPropagator + Send + Sync>> {
                match propagator {
                    Propagator::TraceContext => Some(Box::new(TraceContextPropagator::new())),
                    Propagator::Baggage => Some(Box::new(BaggagePropagator::new())),
                    Propagator::None => None,
                }
            })
            .collect(),
        ));
        let carrier = [(stringify!(traceparent), traceparent), (stringify!(tracestate), tracestate), (stringify!(baggage), baggage)]
            .into_iter()
            .filter_map(|(field, value)| Some((field.to_owned(), value?)))
            .collect::<HashMap<_, _>>();
        let parent = global::get_text_map_propagator(|propagator| propagator.extract(&carrier));
        let previous: Arc<dyn Fn(&PanicHookInfo<'_>) + Send + Sync> = panic::take_hook().into();
        let chained = Arc::clone(&previous);
        #[cfg(panic = "abort")]
        let hooked = (tracer_provider.clone(), meter_provider.clone(), logger_provider.clone());
        panic::set_hook(Box::new(move |info| {
            tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked");
            #[cfg(panic = "abort")]
            finished(Operation::Flush, &hooked).unwrap_or_else(|errors| reported(&errors));
            chained(info);
        }));
        let stopping = DropGuard::new((tracer_provider, meter_provider, logger_provider), |providers| finished(Operation::Shutdown, &providers).unwrap_or_else(|errors| reported(&errors)));
        let worked = {
            let _parent = parent.attach();
            work(handle, level).inspect_err(reported)
        };
        panic::set_hook(Box::new(move |info| previous(info)));
        let stopped = finished(Operation::Shutdown, &DropGuard::dismiss(stopping)).inspect_err(reported);
        match (worked, stopped) {
            (Ok(outcome), Ok(())) => outcome.report(),
            (Err(failures), _) => status(failures),
            (Ok(_), Err(failures)) => status(failures),
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum TelemetryError {
    #[error("cannot parse filter directive {directive}")]
    Filter { directive: Box<str>, source: ParseError },
    #[error("cannot build OTLP {signal} exporter")]
    Build { signal: Signal, source: ExporterBuildError },
    #[error("cannot install tracing subscriber, one is installed")]
    Dispatch,
    #[error("cannot forward log records, a logger is installed")]
    Forward,
    #[error("cannot start a thread to {operation} {signal}")]
    Spawn { operation: Operation, signal: Signal, source: io::Error },
    #[error("cannot {operation} {signal}")]
    Sdk { operation: Operation, signal: Signal, source: OTelSdkError },
}

impl From<TelemetryError> for sysexits::ExitCode {
    fn from(value: TelemetryError) -> Self {
        match value {
            TelemetryError::Filter { directive: _, source: _ }
            | TelemetryError::Build { signal: _, source: ExporterBuildError::InternalFailure(_) }
            | TelemetryError::Dispatch
            | TelemetryError::Forward
            | TelemetryError::Sdk { operation: _, signal: _, source: OTelSdkError::AlreadyShutdown } => Self::Software,
            TelemetryError::Build { signal: _, source: ExporterBuildError::InvalidConfiguration(_) } => Self::Config,
            TelemetryError::Spawn { operation: _, signal: _, source } => source.into(),
            TelemetryError::Sdk { operation: _, signal: _, source: OTelSdkError::Timeout(_) } => Self::TempFail,
            TelemetryError::Sdk { operation: _, signal: _, source: OTelSdkError::InternalFailure(_) } => Self::Unavailable,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn inject(context: &Context, command: &mut process::Command) {
    global::get_text_map_propagator(|propagator| {
        let mut carrier = HashMap::new();
        propagator.inject_context(context, &mut carrier);
        propagator.fields().for_each(|field| {
            let prefix = field.starts_with(|first: char| first.is_ascii_digit()).then_some('_');
            let variable = iter::chain(prefix, field.chars().map(|character| if character.is_ascii_alphanumeric() { character.to_ascii_uppercase() } else { '_' })).collect::<String>();
            match carrier.remove(field) {
                Some(value) => command.env(variable, value),
                None => command.env_remove(variable),
            };
        });
    });
}
