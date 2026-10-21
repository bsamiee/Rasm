use core::{convert::identity, iter};

use clippy_utils::{diagnostics::span_lint_hir_and_then, expr_or_init, fn_def_id, paths::PathNS, peel_hir_expr_refs};
use itertools::Itertools as _;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{any_expr, expanded_externally, path, paths, pointee, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    setters: FxHashMap<DefId, &'static [&'static str]>,
    detectors: FxHashSet<DefId>,
    seeded: Option<DefId>,
    environment: Option<DefId>,
}

lint!(Pass; ENVIRONMENT_OVERRIDE: "builder calls overriding a value that a dependency reads from its standard environment variable");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            setters: resolve::<&[&str]>(
                tcx,
                PathNS::Value,
                &[
                    ("opentelemetry_otlp::WithExportConfig::with_endpoint", &["OTEL_EXPORTER_OTLP_ENDPOINT"]),
                    ("opentelemetry_otlp::WithExportConfig::with_protocol", &["OTEL_EXPORTER_OTLP_PROTOCOL"]),
                    ("opentelemetry_otlp::WithExportConfig::with_timeout", &["OTEL_EXPORTER_OTLP_TIMEOUT"]),
                    ("opentelemetry_otlp::WithHttpConfig::with_compression", &["OTEL_EXPORTER_OTLP_COMPRESSION"]),
                    ("opentelemetry_otlp::MetricExporterBuilder::with_temporality", &["OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_sampler", &["OTEL_TRACES_SAMPLER", "OTEL_TRACES_SAMPLER_ARG"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_attributes_per_span", &["OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_events_per_span", &["OTEL_SPAN_EVENT_COUNT_LIMIT"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_links_per_span", &["OTEL_SPAN_LINK_COUNT_LIMIT"]),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_span_limits", &["OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT", "OTEL_SPAN_EVENT_COUNT_LIMIT", "OTEL_SPAN_LINK_COUNT_LIMIT"]),
                    ("opentelemetry_sdk::trace::BatchConfigBuilder::with_max_queue_size", &["OTEL_BSP_MAX_QUEUE_SIZE"]),
                    ("opentelemetry_sdk::trace::BatchConfigBuilder::with_scheduled_delay", &["OTEL_BSP_SCHEDULE_DELAY"]),
                    ("opentelemetry_sdk::trace::BatchConfigBuilder::with_max_export_batch_size", &["OTEL_BSP_MAX_EXPORT_BATCH_SIZE"]),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_queue_size", &["OTEL_BLRP_MAX_QUEUE_SIZE"]),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_scheduled_delay", &["OTEL_BLRP_SCHEDULE_DELAY"]),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_export_batch_size", &["OTEL_BLRP_MAX_EXPORT_BATCH_SIZE"]),
                    ("opentelemetry_sdk::metrics::PeriodicReaderBuilder::with_interval", &["OTEL_METRIC_EXPORT_INTERVAL"]),
                ],
            ),
            detectors: paths(tcx, PathNS::Value, &["opentelemetry_sdk::resource::ResourceBuilder::with_detector", "opentelemetry_sdk::resource::ResourceBuilder::with_detectors"]),
            seeded: path(tcx, PathNS::Value, "opentelemetry_sdk::resource::Resource::builder"),
            environment: path(tcx, PathNS::Type, "opentelemetry_sdk::resource::EnvResourceDetector"),
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
        let typeck = cx.typeck_results();
        let environmental = |detector: &'tcx Expr<'tcx>| any_expr(cx.tcx, detector, |inner| pointee(cx, typeck.expr_ty(inner)).ty_adt_def().is_some_and(|adt| Some(adt.did()) == self.environment));
        let merged = |call: &'tcx Expr<'tcx>| match call.kind {
            ExprKind::MethodCall(_, _, [detected], _) | ExprKind::Call(_, [_, detected]) if fn_def_id(cx, call).is_some_and(|called| self.detectors.contains(&called)) =>
                match expr_or_init(cx, peel_hir_expr_refs(detected).0).kind {
                    ExprKind::Array(detectors) => detectors.iter().map(environmental).collect(),
                    _ => vec![environmental(detected)],
                },
            _ => Vec::new(),
        };
        let overridden = || {
            let seeded = iter::successors(Some(receiver), |&step| match expr_or_init(cx, step).kind {
                ExprKind::MethodCall(_, inner, ..) | ExprKind::Call(_, [inner, ..]) if typeck.expr_ty(inner) == typeck.expr_ty(step) => Some(inner),
                _ => None,
            })
            .map(|step| expr_or_init(cx, step))
            .any(|step| self.seeded.is_some_and(|builder| fn_def_id(cx, step) == Some(builder)) || merged(step).contains(&true));
            merged(expr)
                .into_iter()
                .scan(seeded, |earlier, current| {
                    let overrides = *earlier && !current;
                    *earlier |= current;
                    Some(overrides)
                })
                .any(identity)
        };
        let Some(called) = fn_def_id(cx, expr).filter(|_| !expanded_externally(cx, expr.span)) else { return };
        let (variables, help) = match self.setters.get(&called) {
            Some(&variables) => {
                let variables = variables.iter().map(|variable| format!("`{variable}`")).join(", ");
                let help = format!("delete the call and set {variables} in the process environment");
                (variables, help)
            }
            None if self.detectors.contains(&called) && overridden() =>
                (String::from("`OTEL_RESOURCE_ATTRIBUTES`"), String::from("start from `Resource::builder_empty()` and merge `EnvResourceDetector` after every other detector")),
            None => return,
        };
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
