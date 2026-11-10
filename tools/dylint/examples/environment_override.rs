use core::hint::black_box;
use core::time::Duration;

use opentelemetry_otlp::{Compression, ExporterBuildError, LogExporter, MetricExporter, Protocol, SpanExporter, WithExportConfig as _};
use opentelemetry_sdk::metrics::{PeriodicReader, Temporality};
use opentelemetry_sdk::resource::TelemetryResourceDetector;
use opentelemetry_sdk::trace::{self, SdkTracerProvider, ShouldSample, SpanLimits};
use opentelemetry_sdk::{Resource, logs};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn configured() -> Result<SpanExporter, ExporterBuildError> { SpanExporter::builder().with_http().with_protocol(Protocol::HttpBinary).build() }

fn compressed() -> Result<LogExporter, ExporterBuildError> { opentelemetry_otlp::WithHttpConfig::with_compression(LogExporter::builder().with_http(), Compression::Gzip).build() }

fn qualified() -> Result<LogExporter, ExporterBuildError> { opentelemetry_otlp::WithExportConfig::with_endpoint(LogExporter::builder().with_http(), "").build() }

fn timed() -> Result<SpanExporter, ExporterBuildError> { SpanExporter::builder().with_http().with_timeout(Duration::from_micros(1)).build() }

fn temporal() -> Result<MetricExporter, ExporterBuildError> { MetricExporter::builder().with_temporality(Temporality::Delta).build() }

fn sampled(sampler: Box<dyn ShouldSample>) -> SdkTracerProvider { SdkTracerProvider::builder().with_sampler(sampler).build() }

fn limits() -> SdkTracerProvider { SdkTracerProvider::builder().with_span_limits(SpanLimits { max_attributes_per_event: 2, max_attributes_per_link: 3, ..SpanLimits::default() }).build() }

fn limited() -> SdkTracerProvider { SdkTracerProvider::builder().with_max_attributes_per_span(9).with_max_events_per_span(16).with_max_links_per_span(8).build() }

fn batched() -> (trace::BatchConfig, logs::BatchConfig) {
    (
        trace::BatchConfigBuilder::default().with_max_queue_size(128).with_max_export_batch_size(64).with_scheduled_delay(Duration::from_micros(1)).build(),
        logs::BatchConfigBuilder::default().with_max_queue_size(512).with_max_export_batch_size(64).with_scheduled_delay(Duration::from_micros(1)).build(),
    )
}

fn periodic(exporter: MetricExporter) -> PeriodicReader<MetricExporter> { PeriodicReader::builder(exporter).with_interval(Duration::from_micros(1)).build() }

fn unchanged(exporter: MetricExporter) -> PeriodicReader<MetricExporter> { PeriodicReader::builder(exporter).with_interval(Duration::ZERO).build() }

fn detected() -> Resource { Resource::builder().with_detector(Box::new(TelemetryResourceDetector)).build() }

fn environmental() -> Result<SpanExporter, ExporterBuildError> { SpanExporter::builder().build() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(configured, compressed, qualified, timed, temporal, sampled, limits, limited, batched, periodic, unchanged, detected, environmental)); }
