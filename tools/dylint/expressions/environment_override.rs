use clippy_utils::consts::{Constant, mir_to_const};
use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::fn_def_id;
use clippy_utils::paths::PathNS;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::def_id::DefId;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, path, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    variables: FxHashMap<(DefId, Option<DefId>), String>,
    signals: FxHashMap<DefId, &'static str>,
}

lint!(Pass; ENVIRONMENT_OVERRIDE: "builder calls overriding a value that a dependency reads from its standard environment variable");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let variable = |constant: &str| {
            let constant = path(tcx, PathNS::Value, constant)?;
            match mir_to_const(tcx, tcx.const_eval_poly(constant).ok()?, tcx.type_of(constant).instantiate_identity().skip_normalization())? {
                Constant::Str(variable) => Some(variable),
                _ => None,
            }
        };
        let (traces, metrics, logs) = ("opentelemetry_otlp::SpanExporterBuilder", "opentelemetry_otlp::MetricExporterBuilder", "opentelemetry_otlp::LogExporterBuilder");
        let (protocol, compression) = ("opentelemetry_otlp::WithExportConfig::with_protocol", "opentelemetry_otlp::WithHttpConfig::with_compression");
        Self {
            variables: [
                ("opentelemetry_otlp::MetricExporterBuilder::with_temporality", None, variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE")),
                ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_attributes_per_span", None, Some(String::from("OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT"))),
                ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_events_per_span", None, Some(String::from("OTEL_SPAN_EVENT_COUNT_LIMIT"))),
                ("opentelemetry_sdk::trace::TracerProviderBuilder::with_max_links_per_span", None, Some(String::from("OTEL_SPAN_LINK_COUNT_LIMIT"))),
                ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_queue_size", None, variable("opentelemetry_sdk::logs::OTEL_BLRP_MAX_QUEUE_SIZE")),
                ("opentelemetry_sdk::logs::BatchConfigBuilder::with_max_export_batch_size", None, variable("opentelemetry_sdk::logs::OTEL_BLRP_MAX_EXPORT_BATCH_SIZE")),
                (protocol, None, variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_PROTOCOL")),
                (protocol, Some(traces), variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_TRACES_PROTOCOL")),
                (protocol, Some(metrics), variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_METRICS_PROTOCOL")),
                (protocol, Some(logs), variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_LOGS_PROTOCOL")),
                (compression, None, variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_COMPRESSION")),
                (compression, Some(traces), variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_TRACES_COMPRESSION")),
                (compression, Some(metrics), variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_METRICS_COMPRESSION")),
                (compression, Some(logs), variable("opentelemetry_otlp::OTEL_EXPORTER_OTLP_LOGS_COMPRESSION")),
            ]
            .into_iter()
            .filter_map(|(method, builder, variable)| Some(((path(tcx, PathNS::Value, method)?, builder.map_or(Some(None), |builder| path(tcx, PathNS::Type, builder).map(Some))?), variable?)))
            .collect(),
            signals: resolve(tcx, PathNS::Type, &[(traces, "traces"), (metrics, "metrics"), (logs, "logs")]),
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
        let Some(every) = self.variables.get(&(called, None)) else { return };
        let quoted = |variable: &str| format!("`{variable}`");
        let every = quoted(every);
        let scoped = cx.typeck_results().expr_ty(receiver).peel_refs().ty_adt_def().and_then(|builder| Some((self.signals.get(&builder.did())?, self.variables.get(&(called, Some(builder.did())))?)));
        let (variables, help) = match scoped {
            Some((signal, scoped)) => {
                let scoped = quoted(scoped);
                (format!("{scoped}, {every}"), format!("delete the call and set {scoped} for {signal} or {every} for every signal"))
            }
            None => (every.clone(), format!("delete the call and set {every} in the process environment")),
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
