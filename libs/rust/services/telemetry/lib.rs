#![feature(default_field_values, error_reporter, trait_alias, try_blocks_heterogeneous)]

use core::{error::Error, iter, mem::DropGuard};
use std::{
    backtrace::Backtrace,
    borrow::Cow,
    collections::HashMap,
    error::Report,
    io,
    panic::{self, PanicHookInfo},
    process::{ExitCode, Termination},
    sync::Arc,
    thread,
};

use clap::{ArgAction, Args, Command, ValueEnum};
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
    resource::{EnvResourceDetector, ResourceDetector, TelemetryResourceDetector},
    trace::{SdkTracerProvider, TracerProviderBuilder},
};
use opentelemetry_semantic_conventions::{
    SCHEMA_URL,
    attribute::{CODE_FILE_PATH, CODE_LINE_NUMBER, EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, SERVICE_INSTANCE_ID, SERVICE_NAME, SERVICE_VERSION, THREAD_NAME},
};
use strum::VariantArray as _;
use tracing::{
    Dispatch, Subscriber, dispatcher,
    subscriber::{self, SetGlobalDefaultError},
};
use tracing_log::LogTracer;
use tracing_subscriber::{
    EnvFilter, Registry,
    filter::{Directive, LevelFilter, ParseError},
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

#[derive(Clone, Copy, Debug, derive_more::Display, strum::VariantArray)]
#[display(rename_all = "lowercase")]
enum Signal {
    Traces,
    Metrics,
    Logs,
}

#[derive(Clone, Copy, Debug, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Operation {
    Flush,
    #[display("shut down")]
    Shutdown,
}

#[derive(Clone, Copy, Debug, Default, ValueEnum, derive_more::IsVariant)]
enum Exporter {
    #[default]
    None,
    #[value(help = "OTLP over HTTP/protobuf, settings from OTEL_EXPORTER_OTLP_* variables")]
    Otlp,
}

#[derive(Clone, Copy, Debug, ValueEnum)]
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

#[derive(Args, Debug)]
#[group(skip)]
#[command(next_help_heading = "Telemetry")]
pub struct Options {
    #[arg(long, global = true, env = EnvFilter::DEFAULT_ENV, value_name = "DIRECTIVE", value_delimiter = ',', help = "Tracing filter directives over program defaults")]
    log: Vec<Directive>,
    #[arg(long, global = true, env = "OTEL_SERVICE_NAME", value_name = "NAME", help = "Service name over program defaults and OTEL_RESOURCE_ATTRIBUTES")]
    service_name: Option<String>,
    #[arg(long, global = true, env = "OTEL_PROPAGATORS", value_name = "PROPAGATOR", value_enum, value_delimiter = ',', default_values_t = [Propagator::TraceContext, Propagator::Baggage])]
    #[arg(help = "Propagators injecting and extracting context")]
    propagators: Vec<Propagator>,
    #[arg(long, global = true, env = "OTEL_TRACES_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving spans")]
    traces_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_METRICS_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving metrics")]
    metrics_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_LOGS_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving log records")]
    logs_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_SDK_DISABLED", value_name = "BOOL", action = ArgAction::Set, default_value_t, help = "Disable every signal's exporter")]
    sdk_disabled: bool,
    #[arg(long, global = true, hide = true, env)]
    traceparent: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    tracestate: Option<String>,
    #[arg(long, global = true, hide = true, env)]
    baggage: Option<String>,
}

impl Options {
    pub fn run<W, L, T, E>(self, command: &Command, defaults: Defaults, writer: W, layer: L, work: impl FnOnce(Handle) -> Result<T, NEVec<E>>) -> ExitCode
    where
        W: MakeWriter,
        L: Layer,
        T: Termination,
        E: Failure,
    {
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
        let settled = |operation: Operation, (traces, metrics, logs): &(SdkTracerProvider, SdkMeterProvider, SdkLoggerProvider)| {
            thread::scope(|scope| {
                let spawned = match operation {
                    Operation::Flush => [
                        thread::Builder::new().spawn_scoped(scope, || traces.force_flush()),
                        thread::Builder::new().spawn_scoped(scope, || metrics.force_flush()),
                        thread::Builder::new().spawn_scoped(scope, || logs.force_flush()),
                    ],
                    Operation::Shutdown => [
                        thread::Builder::new().spawn_scoped(scope, || traces.shutdown()),
                        thread::Builder::new().spawn_scoped(scope, || metrics.shutdown()),
                        thread::Builder::new().spawn_scoped(scope, || logs.shutdown()),
                    ],
                };
                iter::zip(Signal::VARIANTS, spawned)
                    .map(|(&signal, spawned)| {
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
        let [traces_exporter, metrics_exporter, logs_exporter] = [traces_exporter, metrics_exporter, logs_exporter].map(|exporter| if sdk_disabled { Exporter::None } else { exporter });
        let Defaults { directives, attributes } = defaults;
        let detectors: [Box<dyn ResourceDetector>; _] =
            [Box::new(TelemetryResourceDetector), Box::new(OsResourceDetector), Box::new(ProcessResourceDetector), Box::new(HostResourceDetector::default()), Box::new(ContainerResourceDetector)];
        let resource = Resource::builder_empty()
            .with_schema_url([], SCHEMA_URL)
            .with_service_name(command.get_name().to_owned())
            .with_attributes(command.get_version().map(|version| KeyValue::new(SERVICE_VERSION, version.to_owned())))
            .with_detector(Box::new(ServiceInstanceIdResourceDetector))
            .with_detectors(if [traces_exporter, metrics_exporter, logs_exporter].iter().any(Exporter::is_otlp) { &detectors } else { &[] })
            .with_detector(Box::new(EnvResourceDetector::new()))
            .with_attributes(attributes)
            .with_attributes(service_name.map(|name| KeyValue::new(SERVICE_NAME, name)))
            .build();
        let early = Dispatch::new(tracing_subscriber::registry().with(json(&resource, writer.clone())).with(LevelFilter::WARN));
        let started = dispatcher::with_default(&early, || {
            (try bikeshed Result<_, NEVec<TelemetryError>> {
                let (filter, traces, metrics, logs) = Validated::from(EnvFilter::try_new(&directives).map_err(|source| TelemetryError::Filter { directives, source }))
                    .map4(
                        built(Signal::Traces, traces_exporter, || SpanExporter::builder().build()),
                        built(Signal::Metrics, metrics_exporter, || MetricExporter::builder().build()),
                        built(Signal::Logs, logs_exporter, || LogExporter::builder().build()),
                        |filter, span_exporter, metric_exporter, log_exporter| {
                            (
                                filter,
                                span_exporter.into_iter().fold(SdkTracerProvider::builder().with_resource(resource.clone()), TracerProviderBuilder::with_batch_exporter).build(),
                                metric_exporter.into_iter().fold(SdkMeterProvider::builder().with_resource(resource.clone()), MeterProviderBuilder::with_periodic_exporter).build(),
                                log_exporter.into_iter().fold(SdkLoggerProvider::builder().with_resource(resource.clone()), LoggerProviderBuilder::with_batch_exporter).build(),
                            )
                        },
                    )
                    .ok()?;
                let scope = InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_version(env!("CARGO_PKG_VERSION")).build();
                let (filter, handle) = reload::Layer::new(log.into_iter().fold(filter, EnvFilter::add_directive));
                let dispatched = tracing_subscriber::registry()
                    .with(filter)
                    .with(layer)
                    .with(json(&resource, writer))
                    .with(tracing_opentelemetry::layer().with_tracer(traces.tracer_with_scope(scope)))
                    .with(OpenTelemetryTracingBridge::builder(&logs).with_tracing_span_attributes(TracingSpanAttributes::all()).build());
                Validated::from(subscriber::set_global_default(dispatched).map_err(|SetGlobalDefaultError { .. }| TelemetryError::Dispatch))
                    .map2(Validated::from(LogTracer::init().map_err(|SetLoggerError { .. }| TelemetryError::Forward)), |(), ()| ())
                    .ok()?;
                (traces, metrics, logs, handle)
            })
            .inspect_err(reported)
        });
        let (traces, metrics, logs, handle) = match started {
            Ok(started) => started,
            Err(failures) => return status(failures),
        };
        global::set_tracer_provider(traces.clone());
        global::set_meter_provider(metrics.clone());
        global::set_text_map_propagator(TextMapCompositePropagator::new(
            propagators
                .into_iter()
                .filter_map(|propagator| -> Option<Box<dyn TextMapPropagator + Send + Sync>> {
                    match propagator {
                        Propagator::TraceContext => Some(Box::new(TraceContextPropagator::new())),
                        Propagator::Baggage => Some(Box::new(BaggagePropagator::new())),
                        Propagator::None => None,
                    }
                })
                .collect(),
        ));
        let carried = [(stringify!(traceparent), traceparent), (stringify!(tracestate), tracestate), (stringify!(baggage), baggage)]
            .into_iter()
            .filter_map(|(field, value)| Some((field.to_owned(), value?)))
            .collect::<HashMap<_, _>>();
        let parent = global::get_text_map_propagator(|propagator| propagator.extract(&carried));
        let restored = |hook: Arc<dyn Fn(&PanicHookInfo<'_>) + Send + Sync>| panic::set_hook(Box::new(move |info| hook(info)));
        let previous: Arc<dyn Fn(&PanicHookInfo<'_>) + Send + Sync> = panic::take_hook().into();
        let (chained, hooked) = (Arc::clone(&previous), (traces.clone(), metrics.clone(), logs.clone()));
        let stopping = DropGuard::new(((traces, metrics, logs), previous), move |(providers, hook)| {
            restored(hook);
            settled(Operation::Shutdown, &providers).unwrap_or_else(|errors| reported(&errors));
        });
        panic::set_hook(Box::new(move |info| {
            tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked");
            settled(Operation::Flush, &hooked).unwrap_or_else(|errors| reported(&errors));
            chained(info);
        }));
        let worked = {
            let _parent = parent.attach();
            work(handle).inspect_err(reported)
        };
        let (providers, hook) = DropGuard::dismiss(stopping);
        restored(hook);
        let stopped = settled(Operation::Shutdown, &providers).inspect_err(reported);
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
    #[error("cannot parse filter directives {directives}")]
    Filter { directives: Cow<'static, str>, source: ParseError },
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
            TelemetryError::Filter { directives: _, source: _ }
            | TelemetryError::Dispatch
            | TelemetryError::Forward
            | TelemetryError::Sdk { operation: _, signal: _, source: OTelSdkError::AlreadyShutdown } => Self::Software,
            TelemetryError::Build { signal: _, source: _ } => Self::Config,
            TelemetryError::Spawn { operation: _, signal: _, source } => source.into(),
            TelemetryError::Sdk { operation: _, signal: _, source: OTelSdkError::Timeout(_) } => Self::TempFail,
            TelemetryError::Sdk { operation: _, signal: _, source: OTelSdkError::InternalFailure(_) } => Self::Unavailable,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn environment(context: &Context) -> impl Iterator<Item = (String, String)> {
    global::get_text_map_propagator(|propagator| {
        let mut carried = HashMap::new();
        propagator.inject_context(context, &mut carried);
        carried
    })
    .into_iter()
    .map(|(field, value)| {
        let prefix = field.chars().next().is_none_or(|first| first.is_ascii_digit()).then_some('_');
        (iter::chain(prefix, field.chars().map(|character| if character.is_ascii_alphanumeric() { character.to_ascii_uppercase() } else { '_' })).collect(), value)
    })
}
