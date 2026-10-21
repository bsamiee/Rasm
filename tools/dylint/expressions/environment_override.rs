use core::{iter, ops::ControlFlow};

use clippy_utils::{diagnostics::span_lint_hir_and_then, expr_or_init, fn_def_id, paths::PathNS, peel_hir_expr_refs, visitors::for_each_expr};
use itertools::Itertools as _;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{AdtDef, TyCtxt};

use crate::{expanded_externally, method_call_on, path, paths, pointee, resolve};

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPAN_ATTRIBUTE_LIMIT: &str = "OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT";
const SPAN_EVENT_LIMIT: &str = "OTEL_SPAN_EVENT_COUNT_LIMIT";
const SPAN_LINK_LIMIT: &str = "OTEL_SPAN_LINK_COUNT_LIMIT";

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    setters: FxHashMap<DefId, &'static [&'static str]>,
    exported: FxHashMap<DefId, &'static str>,
    signals: FxHashMap<DefId, &'static str>,
    detectors: FxHashSet<DefId>,
    seeded: Option<DefId>,
    readers: Vec<(DefId, &'static str)>,
}

lint!(Pass; ENVIRONMENT_OVERRIDE: "builder calls overriding a value that a dependency reads from its standard environment variable");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            setters: resolve::<&[&str]>(
                tcx,
                PathNS::Value,
                &[
                    ("opentelemetry_otlp::MetricExporterBuilder::with_temporality", &["OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_sampler", &["OTEL_TRACES_SAMPLER", "OTEL_TRACES_SAMPLER_ARG"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_attributes_per_span", &[SPAN_ATTRIBUTE_LIMIT]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_events_per_span", &[SPAN_EVENT_LIMIT]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_links_per_span", &[SPAN_LINK_LIMIT]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_span_limits", &[SPAN_ATTRIBUTE_LIMIT, SPAN_EVENT_LIMIT, SPAN_LINK_LIMIT]),
                    ("opentelemetry_sdk::trace::BatchConfigBuilder::with_max_queue_size", &["OTEL_BSP_MAX_QUEUE_SIZE"]),
                    ("opentelemetry_sdk::trace::BatchConfigBuilder::with_scheduled_delay", &["OTEL_BSP_SCHEDULE_DELAY"]),
                    ("opentelemetry_sdk::trace::BatchConfigBuilder::with_max_export_batch_size", &["OTEL_BSP_MAX_EXPORT_BATCH_SIZE"]),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_queue_size", &["OTEL_BLRP_MAX_QUEUE_SIZE"]),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_scheduled_delay", &["OTEL_BLRP_SCHEDULE_DELAY"]),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_export_batch_size", &["OTEL_BLRP_MAX_EXPORT_BATCH_SIZE"]),
                    ("opentelemetry_sdk::metrics::PeriodicReaderBuilder::with_interval", &["OTEL_METRIC_EXPORT_INTERVAL"]),
                ],
            ),
            exported: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("opentelemetry_otlp::WithExportConfig::with_endpoint", "ENDPOINT"),
                    ("opentelemetry_otlp::WithExportConfig::with_protocol", "PROTOCOL"),
                    ("opentelemetry_otlp::WithExportConfig::with_timeout", "TIMEOUT"),
                    ("opentelemetry_otlp::WithHttpConfig::with_compression", "COMPRESSION"),
                ],
            ),
            signals: resolve(
                tcx,
                PathNS::Type,
                &[("opentelemetry_otlp::SpanExporterBuilder", "TRACES"), ("opentelemetry_otlp::MetricExporterBuilder", "METRICS"), ("opentelemetry_otlp::LogExporterBuilder", "LOGS")],
            ),
            detectors: paths(tcx, PathNS::Value, &["opentelemetry_sdk::resource::ResourceBuilder::with_detector", "opentelemetry_sdk::resource::ResourceBuilder::with_detectors"]),
            seeded: path(tcx, PathNS::Value, "opentelemetry_sdk::resource::Resource::builder"),
            readers: [("opentelemetry_sdk::resource::SdkProvidedResourceDetector", "OTEL_SERVICE_NAME"), ("opentelemetry_sdk::resource::EnvResourceDetector", "OTEL_RESOURCE_ATTRIBUTES")]
                .into_iter()
                .filter_map(|(reader, variable)| Some((path(tcx, PathNS::Type, reader)?, variable)))
                .collect(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let (span, receiver) = match expr.kind {
            ExprKind::MethodCall(segment, receiver, ..) => (segment.ident.span.to(expr.span.shrink_to_hi()), receiver),
            ExprKind::Call(_, [receiver, ..]) => (expr.span, receiver),
            _ => return,
        };
        let Some(called) = fn_def_id(cx, expr) else { return };
        let typeck = cx.typeck_results();
        let reading = |detector: &'tcx Expr<'tcx>| {
            for_each_expr(cx.tcx, detector, |inner| {
                pointee(cx, typeck.expr_ty(inner))
                    .ty_adt_def()
                    .map(AdtDef::did)
                    .filter(|&detected| self.readers.iter().any(|&(reader, _)| reader == detected))
                    .map_or(ControlFlow::Continue(()), ControlFlow::Break)
            })
        };
        let merged = |call: &'tcx Expr<'tcx>| {
            let detected = match call.kind {
                ExprKind::MethodCall(_, _, [detected], _) | ExprKind::Call(_, [_, detected]) if fn_def_id(cx, call).is_some_and(|merging| self.detectors.contains(&merging)) => Some(detected),
                _ => None,
            };
            detected.map_or_default(|detected| match expr_or_init(cx, peel_hir_expr_refs(detected).0).kind {
                ExprKind::Array(detectors) => detectors.iter().map(reading).collect(),
                _ => vec![reading(detected)],
            })
        };
        let overridden = || {
            let own = merged(expr);
            let earlier = iter::successors(Some(receiver), |&step| match expr_or_init(cx, step).kind {
                ExprKind::MethodCall(_, inner, ..) | ExprKind::Call(_, [inner, ..]) if typeck.expr_ty(inner) == typeck.expr_ty(step) => Some(inner),
                _ => None,
            })
            .map(|step| expr_or_init(cx, step))
            .flat_map(|step| if self.seeded.is_some_and(|builder| fn_def_id(cx, step) == Some(builder)) { self.readers.iter().map(|&(reader, _)| Some(reader)).collect() } else { merged(step) })
            .flatten()
            .collect::<FxHashSet<_>>();
            let later = iter::successors(method_call_on(cx, expr), |&(step, _)| method_call_on(cx, step)).flat_map(|(step, _)| merged(step)).flatten().collect::<FxHashSet<_>>();
            own.iter().rposition(Option::is_none).map_or_default(|last| {
                self.readers
                    .iter()
                    .copied()
                    .filter(|(reader, _)| !later.contains(reader) && own.iter().rposition(|&detected| detected == Some(*reader)).map_or_else(|| earlier.contains(reader), |at| at < last))
                    .collect::<Vec<_>>()
            })
        };
        let quoted = |variable: &str| format!("`{variable}`");
        let (variables, help) = if let Some(&variables) = self.setters.get(&called) {
            let variables = variables.iter().copied().map(quoted).join(", ");
            let help = format!("delete the call and set {variables} in the process environment");
            (variables, help)
        } else if let Some(&setting) = self.exported.get(&called) {
            let every = quoted(&format!("OTEL_EXPORTER_OTLP_{setting}"));
            match typeck.expr_ty(receiver).peel_refs().ty_adt_def().and_then(|builder| self.signals.get(&builder.did())) {
                Some(signal) => {
                    let scoped = quoted(&format!("OTEL_EXPORTER_OTLP_{signal}_{setting}"));
                    (format!("{scoped}, {every}"), format!("delete the call and set {scoped} for this signal or {every} for every signal"))
                }
                None => (every.clone(), format!("delete the call and set {every} in the process environment")),
            }
        } else if self.detectors.contains(&called)
            && let overridden = overridden()
            && !overridden.is_empty()
        {
            (
                overridden.iter().map(|&(_, variable)| quoted(variable)).join(", "),
                format!("merge {} after every other detector, starting from `Resource::builder_empty()`", overridden.iter().map(|&(reader, _)| quoted(cx.tcx.item_name(reader).as_str())).join(", ")),
            )
        } else {
            return;
        };
        if !expanded_externally(cx, expr.span) {
            span_lint_hir_and_then(
                cx,
                ENVIRONMENT_OVERRIDE,
                expr.hir_id,
                span,
                format!("`{}` overrides {variables}, which `{}` reads", cx.tcx.item_name(called), cx.tcx.crate_name(called.krate)),
                |diag| {
                    diag.help(help);
                },
            );
        }
    }
}
