#![feature(default_field_values, error_reporter, try_blocks_heterogeneous)]

use core::error::Error;
use std::{
    backtrace::Backtrace,
    borrow::Cow,
    error::Report,
    mem::DropGuard,
    panic,
    process::{ExitCode, Termination},
};

use clap::{Args, Command, ValueEnum};
use json_subscriber::JsonLayer;
use log::SetLoggerError;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use opentelemetry::{
    InstrumentationScope, Key, KeyValue, global,
    propagation::{TextMapCompositePropagator, TextMapPropagator},
    trace::TracerProvider as _,
};
use opentelemetry_appender_tracing::layer::{OpenTelemetryTracingBridge, TracingSpanAttributes};
use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, SpanExporter};
use opentelemetry_resource_detectors::{ContainerResourceDetector, HostResourceDetector, OsResourceDetector, ProcessResourceDetector, ServiceInstanceIdResourceDetector};
use opentelemetry_sdk::{
    Resource,
    error::{OTelSdkError, OTelSdkResult},
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
use tracing::{
    Dispatch, Subscriber, dispatcher,
    subscriber::{self, SetGlobalDefaultError},
};
use tracing_log::LogTracer;
use tracing_subscriber::{
    EnvFilter, Layer, Registry,
    filter::{Directive, LevelFilter, ParseError},
    fmt::{MakeWriter, time::SystemTime},
    layer::{Layered, SubscriberExt as _},
    registry::LookupSpan,
    reload,
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Signal {
    Traces,
    Metrics,
    Logs,
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
}

impl Options {
    pub fn run<W, L, T, E>(self, command: &Command, defaults: Defaults, writer: W, layer: L, work: impl FnOnce(&reload::Handle<EnvFilter, Registry>) -> Result<T, NEVec<E>>) -> ExitCode
    where
        W: for<'writer> MakeWriter<'writer> + Clone + Send + Sync + 'static,
        L: Layer<Layered<reload::Layer<EnvFilter, Registry>, Registry>> + Send + Sync + 'static,
        T: Termination,
        E: Error + Into<sysexits::ExitCode>,
    {
        fn built<B>(signal: Signal, exporter: Exporter, build: impl FnOnce() -> Result<B, ExporterBuildError>) -> Validated<Option<B>, TelemetryError> {
            match exporter {
                Exporter::None => Validated::Good(None),
                Exporter::Otlp => build().map(Some).map_err(|source| TelemetryError::Build { signal, source }).into(),
            }
        }
        fn json<S, W>(resource: &Resource, writer: W) -> JsonLayer<S, W>
        where
            S: Subscriber + for<'span> LookupSpan<'span>,
            W: for<'writer> MakeWriter<'writer> + 'static,
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
        fn flushed(results: impl IntoIterator<Item = (Signal, OTelSdkResult)>) -> Result<(), NEVec<TelemetryError>> {
            results.into_iter().map(|(signal, result)| result.map_err(|source| TelemetryError::Flush { signal, source })).collect::<Validated<(), _>>().ok()
        }
        fn shutdown((traces, metrics, logs): &(SdkTracerProvider, SdkMeterProvider, SdkLoggerProvider)) -> Result<(), NEVec<TelemetryError>> {
            flushed([(Signal::Traces, traces.shutdown()), (Signal::Metrics, metrics.shutdown()), (Signal::Logs, logs.shutdown())])
        }
        let Self { log, service_name, propagators, traces_exporter, metrics_exporter, logs_exporter } = self;
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
                let (filter, span_exporter, metric_exporter, log_exporter) = Validated::from(EnvFilter::try_new(&directives).map_err(|source| TelemetryError::Filter { directives, source }))
                    .map4(
                        built(Signal::Traces, traces_exporter, || SpanExporter::builder().build()),
                        built(Signal::Metrics, metrics_exporter, || MetricExporter::builder().build()),
                        built(Signal::Logs, logs_exporter, || LogExporter::builder().build()),
                        |filter, span_exporter, metric_exporter, log_exporter| (filter, span_exporter, metric_exporter, log_exporter),
                    )
                    .ok()?;
                let traces = span_exporter.into_iter().fold(SdkTracerProvider::builder().with_resource(resource.clone()), TracerProviderBuilder::with_batch_exporter).build();
                let metrics = metric_exporter.into_iter().fold(SdkMeterProvider::builder().with_resource(resource.clone()), MeterProviderBuilder::with_periodic_exporter).build();
                let logs = log_exporter.into_iter().fold(SdkLoggerProvider::builder().with_resource(resource.clone()), LoggerProviderBuilder::with_batch_exporter).build();
                let scope = InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_version(env!("CARGO_PKG_VERSION")).build();
                let (filter, handle) = reload::Layer::new(log.into_iter().fold(filter, EnvFilter::add_directive));
                let dispatched = tracing_subscriber::registry()
                    .with(filter)
                    .with(layer)
                    .with(json(&resource, writer))
                    .with(tracing_opentelemetry::layer().with_tracer(traces.tracer_with_scope(scope)))
                    .with(OpenTelemetryTracingBridge::builder(&logs).with_tracing_span_attributes(TracingSpanAttributes::all()).build());
                subscriber::set_global_default(dispatched).map_err(|SetGlobalDefaultError { .. }| NEVec::new(TelemetryError::Dispatch))?;
                LogTracer::init().map_err(|SetLoggerError { .. }| NEVec::new(TelemetryError::Forward))?;
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
        let hooked = (traces.clone(), metrics.clone(), logs.clone());
        let stopping = DropGuard::new((traces, metrics, logs), |providers| shutdown(&providers).unwrap_or_else(|errors| reported(&errors)));
        let worked = {
            let _restore = DropGuard::new(panic::take_hook(), panic::set_hook);
            panic::set_hook(Box::new(move |info| {
                tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked");
                flushed([(Signal::Traces, hooked.0.force_flush()), (Signal::Metrics, hooked.1.force_flush()), (Signal::Logs, hooked.2.force_flush())]).unwrap_or_else(|errors| reported(&errors));
            }));
            work(&handle).inspect_err(reported)
        };
        let stopped = shutdown(&DropGuard::dismiss(stopping)).inspect_err(reported);
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
    #[error("cannot flush {signal}")]
    Flush { signal: Signal, source: OTelSdkError },
}

impl From<TelemetryError> for sysexits::ExitCode {
    fn from(value: TelemetryError) -> Self {
        match value {
            TelemetryError::Filter { directives: _, source: _ } | TelemetryError::Dispatch | TelemetryError::Forward => Self::Software,
            TelemetryError::Build { signal: _, source: _ } => Self::Config,
            TelemetryError::Flush { signal: _, source: _ } => Self::Unavailable,
        }
    }
}
