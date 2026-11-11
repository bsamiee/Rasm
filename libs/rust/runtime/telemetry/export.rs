use core::time::Duration;
use std::collections::HashSet;
use std::sync::mpsc::{self, RecvTimeoutError, SendError};
use std::sync::nonpoison::Mutex;
use std::thread;

use enum_map::EnumMap;
use nonempty_collections::NEVec;
use opentelemetry::logs::{LogRecord as _, Severity};
use opentelemetry::trace::TracerProvider as _;
use opentelemetry::{InstrumentationScope, KeyValue};
use opentelemetry_appender_tracing::layer::OpenTelemetryTracingBridge;
use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, SpanExporter};
use opentelemetry_resource_detectors::{ContainerResourceDetector, HostResourceDetector, OsResourceDetector, ProcessResourceDetector, ServiceInstanceIdResourceDetector};
use opentelemetry_sdk::error::{OTelSdkError, OTelSdkResult};
use opentelemetry_sdk::logs::{LogProcessor, SdkLogRecord, SdkLoggerProvider};
use opentelemetry_sdk::metrics::SdkMeterProvider;
use opentelemetry_sdk::metrics::exporter::PushMetricExporter;
use opentelemetry_sdk::resource::{EnvResourceDetector, ResourceBuilder, ResourceDetector as _, TelemetryResourceDetector};
use opentelemetry_sdk::trace::{Sampler, SdkTracerProvider};
use opentelemetry_sdk::{Resource, logs, trace};
use opentelemetry_semantic_conventions::SCHEMA_URL;
use opentelemetry_semantic_conventions::attribute::PROCESS_COMMAND_ARGS;
use tokio::time::Instant;
use tracing::{Level, Subscriber};
use tracing_subscriber::Layer;
use tracing_subscriber::filter::{LevelFilter, Targets, dynamic_filter_fn, filter_fn};
use tracing_subscriber::layer::Filter;
use tracing_subscriber::registry::LookupSpan;
use validated::Validated;

use crate::{ObservabilityError, PANIC, Signal};

// --- [CONSTANTS] -----------------------------------------------------------------------

const SDK: &str = "opentelemetry";

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone, Debug)]
pub struct Providers {
    pub tracer: SdkTracerProvider,
    pub meter: Option<SdkMeterProvider>,
    pub logger: Option<SdkLoggerProvider>,
    pub exported: EnumMap<Signal, bool>,
}

impl Providers {
    pub fn new<S, M, L>(resource: &Resource, spans: Option<S>, metrics: Option<M>, records: Option<L>) -> Self
    where
        S: trace::SpanExporter + 'static,
        M: PushMetricExporter,
        L: logs::LogExporter + 'static,
    {
        let (tracer, signals) = (SdkTracerProvider::builder().with_resource(resource.clone()), EnumMap::from_array([spans.is_some(), metrics.is_some(), records.is_some()]));
        Self {
            tracer: match spans {
                Some(exporter) => tracer.with_batch_exporter(exporter),
                None => tracer.with_sampler(Sampler::ParentBased(Box::new(Sampler::AlwaysOff))),
            }
            .build(),
            meter: metrics.map(|exporter| SdkMeterProvider::builder().with_resource(resource.clone()).with_periodic_exporter(exporter).build()),
            logger: records.map(|exporter| SdkLoggerProvider::builder().with_resource(resource.clone()).with_log_processor(Fatal).with_batch_exporter(exporter).build()),
            exported: signals,
        }
    }

    pub fn otlp(exported: EnumMap<Signal, bool>, resource: &Resource) -> Result<Self, NEVec<ObservabilityError>> {
        fn built<B>(exported: EnumMap<Signal, bool>, signal: Signal, build: impl FnOnce() -> Result<B, ExporterBuildError>) -> Validated<Option<B>, ObservabilityError> {
            exported[signal].then(build).transpose().map_err(|source| ObservabilityError::Exporter { signal, source }).into()
        }
        built(exported, Signal::Traces, || SpanExporter::builder().build())
            .map3(built(exported, Signal::Metrics, || MetricExporter::builder().build()), built(exported, Signal::Logs, || LogExporter::builder().build()), |spans, metrics, records| {
                Self::new(resource, spans, metrics, records)
            })
            .ok()
    }

    #[must_use]
    pub fn layer<S>(&self) -> impl Layer<S> + Send + Sync + 'static
    where S: Subscriber + for<'span> LookupSpan<'span> + Send + Sync {
        const TRANSPORT: [&str; 8] = ["h2", "hyper", "mio", "reqwest", "rustls", "tonic", "tower", "want"];
        let exported = Targets::new()
            .with_default(LevelFilter::TRACE)
            .with_targets(TRANSPORT.map(|target| (target, LevelFilter::WARN)))
            .with_target("tower_http", LevelFilter::TRACE)
            .with_target(SDK, LevelFilter::OFF);
        let tracer = self.tracer.tracer_with_scope(crate::scope(env!("CARGO_PKG_NAME")));
        let spans = tracing_opentelemetry::layer().with_tracer(tracer).with_filter(filter_fn(|metadata| metadata.is_span() || *metadata.level() <= Level::WARN));
        spans.and_then(self.logger.as_ref().map(OpenTelemetryTracingBridge::new)).with_filter(exported)
    }

    pub fn shutdown(&self, limit: Duration) -> Result<(), NEVec<ObservabilityError>> {
        let started = Instant::now();
        let stopping = self.exported.into_iter().filter(|(_, exported)| *exported).map(|(signal, _)| {
            let (providers, (sent, stopped)) = (self.clone(), mpsc::sync_channel(1));
            let spawned = thread::Builder::new().spawn(move || {
                let outcome = match signal {
                    Signal::Traces => providers.tracer.shutdown_with_timeout(limit),
                    Signal::Metrics => providers.meter.as_ref().map_or(Ok(()), |meter| meter.shutdown_with_timeout(limit)),
                    Signal::Logs => providers.logger.as_ref().map_or(Ok(()), |logger| logger.shutdown_with_timeout(limit)),
                };
                let (Ok(()) | Err(SendError(_))) = sent.send(outcome);
            });
            (signal, spawned.map(|_detached| stopped))
        });
        Vec::from_iter(stopping)
            .into_iter()
            .map(|(signal, spawned)| {
                let stopped = spawned.map_err(|source| ObservabilityError::Thread { signal, source })?;
                let outcome = stopped.recv_timeout(limit.saturating_sub(started.elapsed())).map_err(|(RecvTimeoutError::Timeout | RecvTimeoutError::Disconnected)| OTelSdkError::Timeout(limit));
                outcome.flatten().map_err(|source| ObservabilityError::Provider { signal, source })
            })
            .collect::<Validated<(), _>>()
            .ok()
    }
}

#[derive(Clone, Copy, Debug)]
pub struct Fatal;

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

#[must_use]
pub fn diagnostics<S>() -> impl Filter<S> + Send + Sync + 'static
where S: 'static {
    let seen = Mutex::new(HashSet::new());
    dynamic_filter_fn(move |metadata, _| !metadata.target().starts_with(SDK) || seen.lock().insert(metadata.callsite()))
}

#[must_use]
pub fn resource(exported: EnumMap<Signal, bool>, attributes: impl IntoIterator<Item = KeyValue>) -> ResourceBuilder {
    let program = Resource::builder_empty().with_schema_url(attributes, SCHEMA_URL);
    let described = if exported.values().any(|exported| *exported) {
        program
            .with_detectors(&[
                Box::new(ServiceInstanceIdResourceDetector),
                Box::new(TelemetryResourceDetector),
                Box::new(OsResourceDetector),
                Box::new(HostResourceDetector::default()),
                Box::new(ContainerResourceDetector),
            ])
            .with_attributes(ProcessResourceDetector.detect().iter().filter(|&(key, _)| key.as_str() != PROCESS_COMMAND_ARGS).map(|(key, value)| KeyValue::new(key.clone(), value.clone())))
    } else {
        program
    };
    described.with_detector(Box::new(EnvResourceDetector::new()))
}
