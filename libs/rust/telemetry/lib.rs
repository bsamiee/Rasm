#![feature(error_reporter)]

use core::error::Error;
use std::{
    backtrace::Backtrace,
    error::Report,
    panic,
    process::{ExitCode, Termination},
};

use clap::Command;
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
    Subscriber,
    subscriber::{self, SetGlobalDefaultError},
};
use tracing_log::LogTracer;
use tracing_subscriber::{
    EnvFilter, Layer, Registry,
    filter::LevelFilter,
    fmt::{MakeWriter, time::SystemTime},
    layer::{Layered, SubscriberExt as _},
    registry::LookupSpan,
    reload,
};
use validated::Validated;

use crate::options::{Exporter, Options, Propagator};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Signal {
    Traces,
    Metrics,
    Logs,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failed {
    #[error("cannot build OTLP {signal} exporter")]
    Build { signal: Signal, source: ExporterBuildError },
    #[error("cannot install tracing subscriber, one is installed")]
    Dispatch,
    #[error("cannot forward log records, a logger is installed")]
    Forward,
}

#[derive(Debug, thiserror::Error)]
#[error("cannot flush {signal}")]
pub struct Unflushed {
    pub signal: Signal,
    pub source: OTelSdkError,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Telemetry {
    pub filter: reload::Handle<EnvFilter, Registry>,
    pub traces: SdkTracerProvider,
    pub metrics: SdkMeterProvider,
    pub logs: SdkLoggerProvider,
}

impl Telemetry {
    pub fn run<W, L, T, E>(
        options: Options,
        command: &Command,
        attributes: impl IntoIterator<Item = KeyValue>,
        defaults: EnvFilter,
        writer: W,
        layer: L,
        work: impl FnOnce(&Self) -> Result<T, NEVec<E>>,
    ) -> ExitCode
    where
        W: for<'writer> MakeWriter<'writer> + Clone + Send + Sync + 'static,
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
        fn json<S, W>(writer: W, resource: &Resource) -> JsonLayer<S, W>
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
        let detectors: [Box<dyn ResourceDetector>; 5] =
            [Box::new(TelemetryResourceDetector), Box::new(OsResourceDetector), Box::new(ProcessResourceDetector), Box::new(HostResourceDetector::default()), Box::new(ContainerResourceDetector)];
        let resource = Resource::builder_empty()
            .with_schema_url([], SCHEMA_URL)
            .with_service_name(command.get_name().to_owned())
            .with_attributes(command.get_version().map(|version| KeyValue::new(SERVICE_VERSION, version.to_owned())))
            .with_detector(Box::new(ServiceInstanceIdResourceDetector))
            .with_detectors(if [options.traces_exporter, options.metrics_exporter, options.logs_exporter].contains(&Exporter::Otlp) { &detectors } else { &[] })
            .with_detector(Box::new(EnvResourceDetector::new()))
            .with_attributes(attributes)
            .with_attributes(options.service_name.map(|name| KeyValue::new(SERVICE_NAME, name)))
            .build();
        let exporters = subscriber::with_default(tracing_subscriber::registry().with(json(writer.clone(), &resource)).with(LevelFilter::WARN), || {
            built(Signal::Traces, options.traces_exporter, || SpanExporter::builder().build())
                .map3(
                    built(Signal::Metrics, options.metrics_exporter, || MetricExporter::builder().build()),
                    built(Signal::Logs, options.logs_exporter, || LogExporter::builder().build()),
                    |span_exporter, metric_exporter, log_exporter| (span_exporter, metric_exporter, log_exporter),
                )
                .ok()
                .inspect_err(reported)
        });
        let Ok((span_exporter, metric_exporter, log_exporter)) = exporters else { return ExitCode::FAILURE };
        let traces = span_exporter.into_iter().fold(SdkTracerProvider::builder().with_resource(resource.clone()), TracerProviderBuilder::with_batch_exporter).build();
        let metrics = metric_exporter.into_iter().fold(SdkMeterProvider::builder().with_resource(resource.clone()), MeterProviderBuilder::with_periodic_exporter).build();
        let logs = log_exporter.into_iter().fold(SdkLoggerProvider::builder().with_resource(resource.clone()), LoggerProviderBuilder::with_batch_exporter).build();
        let scope = InstrumentationScope::builder(env!("CARGO_PKG_NAME")).with_version(env!("CARGO_PKG_VERSION")).with_schema_url(SCHEMA_URL).build();
        let (filter, handle) = reload::Layer::new(options.log.into_iter().fold(defaults, EnvFilter::add_directive));
        let dispatched = tracing_subscriber::registry()
            .with(filter)
            .with(layer)
            .with(json(writer, &resource))
            .with(tracing_opentelemetry::layer().with_tracer(traces.tracer_with_scope(scope)))
            .with(OpenTelemetryTracingBridge::builder(&logs).with_tracing_span_attributes(TracingSpanAttributes::all()).build());
        let installed = Validated::from(subscriber::set_global_default(dispatched).map_err(|SetGlobalDefaultError { .. }| Failed::Dispatch))
            .map2(LogTracer::init().map_err(|SetLoggerError { .. }| Failed::Forward).into(), |(), ()| ())
            .ok()
            .inspect_err(reported);
        let telemetry = Self { filter: handle, traces, metrics, logs };
        let ran = installed.ok().map(|()| {
            global::set_tracer_provider(telemetry.traces.clone());
            global::set_meter_provider(telemetry.metrics.clone());
            global::set_text_map_propagator(TextMapCompositePropagator::new(
                options
                    .propagators
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
            let hooked = telemetry.clone();
            panic::set_hook(Box::new(move |info| {
                tracing::error!({ EXCEPTION_MESSAGE } = %info, { EXCEPTION_STACKTRACE } = %Backtrace::force_capture(), "panicked");
                hooked.force_flush().unwrap_or_else(|errors| reported(&errors));
            }));
            work(&telemetry).inspect_err(reported)
        });
        let stopped = flushed([(Signal::Traces, telemetry.traces.shutdown()), (Signal::Metrics, telemetry.metrics.shutdown()), (Signal::Logs, telemetry.logs.shutdown())]).inspect_err(reported);
        match (ran, stopped) {
            (Some(Ok(outcome)), Ok(())) => outcome.report(),
            (None | Some(Err(_)), _) | (_, Err(_)) => ExitCode::FAILURE,
        }
    }

    pub fn force_flush(&self) -> Result<(), NEVec<Unflushed>> {
        flushed([(Signal::Traces, self.traces.force_flush()), (Signal::Metrics, self.metrics.force_flush()), (Signal::Logs, self.logs.force_flush())])
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn flushed(results: [(Signal, OTelSdkResult); 3]) -> Result<(), NEVec<Unflushed>> {
    results.into_iter().map(|(signal, result)| result.map_err(|source| Unflushed { signal, source })).collect::<Validated<(), _>>().ok()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod options;
