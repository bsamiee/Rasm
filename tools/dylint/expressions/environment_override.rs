use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    setters: FxHashMap<DefId, &'static str>,
    exported: FxHashMap<DefId, &'static str>,
    signals: FxHashMap<DefId, &'static str>,
}

lint!(Pass; ENVIRONMENT_OVERRIDE: "builder calls overriding a value that a dependency reads from its standard environment variable");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            setters: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("opentelemetry_otlp::MetricExporterBuilder::with_temporality", "OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE"),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_attributes_per_span", "OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT"),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_events_per_span", "OTEL_SPAN_EVENT_COUNT_LIMIT"),
                    ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_links_per_span", "OTEL_SPAN_LINK_COUNT_LIMIT"),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_queue_size", "OTEL_BLRP_MAX_QUEUE_SIZE"),
                    ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_export_batch_size", "OTEL_BLRP_MAX_EXPORT_BATCH_SIZE"),
                ],
            ),
            exported: resolve(tcx, PathNS::Value, &[("opentelemetry_otlp::WithExportConfig::with_protocol", "PROTOCOL"), ("opentelemetry_otlp::WithHttpConfig::with_compression", "COMPRESSION")]),
            signals: resolve(
                tcx,
                PathNS::Type,
                &[("opentelemetry_otlp::SpanExporterBuilder", "TRACES"), ("opentelemetry_otlp::MetricExporterBuilder", "METRICS"), ("opentelemetry_otlp::LogExporterBuilder", "LOGS")],
            ),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let (span, receiver) = match expr.kind {
            ExprKind::MethodCall(segment, receiver, ..) => (segment.ident.span.to(expr.span.shrink_to_hi()), receiver),
            ExprKind::Call(_, [receiver, ..]) => (expr.span, receiver),
            _ => return,
        };
        if expanded_externally(cx, expr.span) {
            return;
        }
        let Some(called) = fn_def_id(cx, expr) else { return };
        let quoted = |variable: &str| format!("`{variable}`");
        let (variables, help) = if let Some(&variable) = self.setters.get(&called) {
            let variables = quoted(variable);
            let help = format!("delete the call and set {variables} in the process environment");
            (variables, help)
        } else if let Some(&setting) = self.exported.get(&called) {
            let every = quoted(&format!("OTEL_EXPORTER_OTLP_{setting}"));
            match cx.typeck_results().expr_ty(receiver).peel_refs().ty_adt_def().and_then(|builder| self.signals.get(&builder.did())) {
                Some(signal) => {
                    let scoped = quoted(&format!("OTEL_EXPORTER_OTLP_{signal}_{setting}"));
                    (format!("{scoped}, {every}"), format!("delete the call and set {scoped} for {} or {every} for every signal", signal.to_lowercase()))
                }
                None => (every.clone(), format!("delete the call and set {every} in the process environment")),
            }
        } else {
            return;
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
