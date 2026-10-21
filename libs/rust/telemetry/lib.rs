#![feature(error_reporter, try_blocks)]

use core::error::Error;
use std::{
    backtrace::Backtrace,
    error::Report,
    io,
    panic::{self, AssertUnwindSafe},
    process::{ExitCode, Termination},
};

use clap::{Args, Command, ValueEnum};
use json_subscriber::JsonLayer;
use log::SetLoggerError;
use nonempty_collections::NEVec;
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
    fmt::time::SystemTime,
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

#[derive(Clone, Copy, Debug, Default, ValueEnum)]
enum Exporter {
    #[default]
    None,
    #[value(help = "OTLP over HTTP/protobuf to the endpoint, headers, timeout, and gzip compression the OTEL_EXPORTER_OTLP_* variables name")]
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

#[derive(Args, Debug)]
#[group(skip)]
#[command(next_help_heading = "Telemetry")]
pub struct Options {
    #[arg(long, global = true, env = EnvFilter::DEFAULT_ENV, value_name = "DIRECTIVE", value_delimiter = ',', help = "Tracing filter directives over the program's defaults")]
    log: Vec<Directive>,
    #[arg(long, global = true, env = "OTEL_SERVICE_NAME", value_name = "NAME", help = "Service name over the program name, its attributes, and OTEL_RESOURCE_ATTRIBUTES")]
    service_name: Option<String>,
    #[arg(long, global = true, env = "OTEL_PROPAGATORS", value_name = "PROPAGATOR", value_enum, value_delimiter = ',', default_values_t = [Propagator::TraceContext, Propagator::Baggage])]
    #[arg(help = "Context propagators every carrier injects and extracts")]
    propagators: Vec<Propagator>,
    #[arg(long, global = true, env = "OTEL_TRACES_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving spans")]
    traces_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_METRICS_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving metrics")]
    metrics_exporter: Exporter,
    #[arg(long, global = true, env = "OTEL_LOGS_EXPORTER", value_name = "EXPORTER", value_enum, default_value_t, help = "Exporter receiving log records")]
    logs_exporter: Exporter,
}

impl Options {
    pub fn run<L, T, E>(
        self,
        command: &Command,
        attributes: impl IntoIterator<Item = KeyValue>,
        defaults: &str,
        layer: L,
        work: impl FnOnce(&reload::Handle<EnvFilter, Registry>) -> Result<T, NEVec<E>>,
    ) -> ExitCode
    where
        L: Layer<Layered<reload::Layer<EnvFilter, Registry>, Registry>> + Send + Sync + 'static,
        T: Termination,
        E: Error,
    {
        fn built<T>(signal: Signal, exporter: Exporter, build: impl FnOnce() -> Result<T, ExporterBuildError>) -> Validated<Option<T>, Failed> {
            match exporter {
                Exporter::None => Validated::Good(None),
                Exporter::Otlp => build().map(Some).map_err(|source| Failed::Build { signal, source }).into(),
            }
        }
        fn json<S>(resource: &Resource) -> JsonLayer<S, fn() -> io::Stderr>
        where S: Subscriber + for<'span> LookupSpan<'span> {
            let mut layer = JsonLayer::stderr();
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
        fn flushed(results: impl IntoIterator<Item = (Signal, OTelSdkResult)>) -> Result<(), NEVec<Failed>> {
            results.into_iter().map(|(signal, result)| result.map_err(|source| Failed::Flush { signal, source })).collect::<Validated<(), _>>().ok()
        }
        let Self { log, service_name, propagators, traces_exporter, metrics_exporter, logs_exporter } = self;
        let detectors: [Box<dyn ResourceDetector>; _] =
            [Box::new(TelemetryResourceDetector), Box::new(OsResourceDetector), Box::new(ProcessResourceDetector), Box::new(HostResourceDetector::default()), Box::new(ContainerResourceDetector)];
        let resource = Resource::builder_empty()
            .with_schema_url([], SCHEMA_URL)
            .with_service_name(command.get_name().to_owned())
            .with_attributes(command.get_version().map(|version| KeyValue::new(SERVICE_VERSION, version.to_owned())))
            .with_detector(Box::new(ServiceInstanceIdResourceDetector))
            .with_detectors(
                if [traces_exporter, metrics_exporter, logs_exporter].into_iter().any(|exporter| match exporter {
                    Exporter::None => false,
                    Exporter::Otlp => true,
                }) {
                    &detectors
                } else {
                    &[]
                },
            )
            .with_detector(Box::new(EnvResourceDetector::new()))
            .with_attributes(attributes)
            .with_attributes(service_name.map(|name| KeyValue::new(SERVICE_NAME, name)))
            .build();
        let early = Dispatch::new(tracing_subscriber::registry().with(json(&resource)).with(LevelFilter::WARN));
        let started = dispatcher::with_default(&early, || {
            let started: Result<_, NEVec<Failed>> = try {
                let (filter, span_exporter, metric_exporter, log_exporter) = Validated::from(EnvFilter::try_new(defaults).map_err(Failed::Filter))
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
                    .with(json(&resource))
                    .with(tracing_opentelemetry::layer().with_tracer(traces.tracer_with_scope(scope)))
                    .with(OpenTelemetryTracingBridge::builder(&logs).with_tracing_span_attributes(TracingSpanAttributes::all()).build());
                subscriber::set_global_default(dispatched)
                    .map_err(|SetGlobalDefaultError { .. }| Failed::Dispatch)
                    .and_then(|()| LogTracer::init().map_err(|SetLoggerError { .. }| Failed::Forward))
                    .map_err(NEVec::new)?;
                (traces, metrics, logs, handle)
            };
            started.inspect_err(reported)
        });
        let Ok((traces, metrics, logs, handle)) = started else { return ExitCode::FAILURE };
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
        let (previous, hooked) = (panic::take_hook(), (traces.clone(), metrics.clone(), logs.clone()));
        panic::set_hook(Box::new(move |info| {
            tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked");
            flushed([(Signal::Traces, hooked.0.force_flush()), (Signal::Metrics, hooked.1.force_flush()), (Signal::Logs, hooked.2.force_flush())]).unwrap_or_else(|errors| reported(&errors));
        }));
        let worked = panic::catch_unwind(AssertUnwindSafe(|| work(&handle).inspect_err(reported)));
        panic::set_hook(previous);
        let stopped = flushed([(Signal::Traces, traces.shutdown()), (Signal::Metrics, metrics.shutdown()), (Signal::Logs, logs.shutdown())]).inspect_err(reported);
        worked.unwrap_or_else(|payload| panic::resume_unwind(payload)).ok().zip(stopped.ok()).map_or(ExitCode::FAILURE, |(outcome, ())| outcome.report())
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failed {
    #[error("cannot parse the program's filter directives")]
    Filter(#[source] ParseError),
    #[error("cannot build OTLP {signal} exporter")]
    Build { signal: Signal, source: ExporterBuildError },
    #[error("cannot install tracing subscriber, one is installed")]
    Dispatch,
    #[error("cannot forward log records, a logger is installed")]
    Forward,
    #[error("cannot flush {signal}")]
    Flush { signal: Signal, source: OTelSdkError },
}
