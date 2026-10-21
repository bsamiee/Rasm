use core::{hint::black_box, time::Duration};

use opentelemetry_otlp::{ExporterBuildError, LogExporter, MetricExporter, Protocol, SpanExporter, WithExportConfig as _};
use opentelemetry_sdk::{
    Resource, logs,
    metrics::{PeriodicReader, Temporality},
    resource::{EnvResourceDetector, TelemetryResourceDetector},
    trace::{self, Sampler, SdkTracerProvider, SpanLimits},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn configured() -> Result<SpanExporter, ExporterBuildError> {
    SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_timeout(Duration::from_secs(5)).build()
}

fn qualified() -> Result<LogExporter, ExporterBuildError> { opentelemetry_otlp::WithExportConfig::with_endpoint(LogExporter::builder().with_http(), "http://127.0.0.1:4318/v1/logs").build() }

fn temporal() -> Result<MetricExporter, ExporterBuildError> { MetricExporter::builder().with_temporality(Temporality::Delta).build() }

fn sampled() -> SdkTracerProvider { SdkTracerProvider::builder().with_sampler(Sampler::AlwaysOn).with_max_events_per_span(16).with_span_limits(SpanLimits::default()).build() }

fn batched() -> (trace::BatchConfig, logs::BatchConfig) {
    (trace::BatchConfigBuilder::default().with_max_queue_size(512).with_scheduled_delay(Duration::from_secs(1)).build(), logs::BatchConfigBuilder::default().with_max_export_batch_size(64).build())
}

fn periodic(exporter: MetricExporter) -> PeriodicReader<MetricExporter> { PeriodicReader::builder(exporter).with_interval(Duration::from_secs(5)).build() }

fn detected() -> Resource { Resource::builder().with_detector(Box::new(TelemetryResourceDetector)).build() }

fn chained() -> Resource {
    let seeded = Resource::builder();
    seeded.with_detectors(&[Box::new(TelemetryResourceDetector)]).build()
}

fn reordered() -> Resource { Resource::builder_empty().with_detector(Box::new(EnvResourceDetector::new())).with_detector(Box::new(TelemetryResourceDetector)).build() }

fn inverted() -> Resource { Resource::builder_empty().with_detectors(&[Box::new(EnvResourceDetector::new()), Box::new(TelemetryResourceDetector)]).build() }

fn layered() -> Resource { Resource::builder_empty().with_detector(Box::new(TelemetryResourceDetector)).with_detector(Box::new(EnvResourceDetector::new())).with_service_name("fixture").build() }

fn listed() -> Resource { Resource::builder_empty().with_detectors(&[Box::new(TelemetryResourceDetector), Box::new(EnvResourceDetector::new())]).build() }

fn environmental() -> Result<SpanExporter, ExporterBuildError> { SpanExporter::builder().build() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(configured, qualified, temporal, sampled, batched, periodic, detected, chained, reordered, inverted, layered, listed, environmental)); }
