use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::TyCtxt;

use crate::resolve;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    ENVIRONMENT_OVERRIDE,
    Warn,
    "builder calls setting a value a dependency resolves from its standard environment variable, which the call overrides"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct EnvironmentOverride {
    setters: FxHashMap<DefId, &'static str>,
}

impl_lint_pass!(EnvironmentOverride => [ENVIRONMENT_OVERRIDE]);

impl EnvironmentOverride {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            setters: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("opentelemetry_otlp::WithExportConfig::with_endpoint", "OTEL_EXPORTER_OTLP_ENDPOINT"),
                    ("opentelemetry_otlp::WithExportConfig::with_protocol", "OTEL_EXPORTER_OTLP_PROTOCOL"),
                    ("opentelemetry_otlp::WithExportConfig::with_timeout", "OTEL_EXPORTER_OTLP_TIMEOUT"),
                    ("opentelemetry_otlp::WithHttpConfig::with_compression", "OTEL_EXPORTER_OTLP_COMPRESSION"),
                ],
            ),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for EnvironmentOverride {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let span = match expr.kind {
            ExprKind::MethodCall(segment, ..) => segment.ident.span.to(expr.span.shrink_to_hi()),
            ExprKind::Call(..) => expr.span,
            _ => return,
        };
        if !expr.span.from_expansion()
            && let Some(setter) = fn_def_id(cx, expr)
            && let Some(&variable) = self.setters.get(&setter)
        {
            span_lint_hir_and_then(
                cx,
                ENVIRONMENT_OVERRIDE,
                expr.hir_id,
                span,
                format!("`{}` overrides the `{variable}` variables `{}` reads", cx.tcx.item_name(setter), cx.tcx.crate_name(setter.krate)),
                |diag| {
                    diag.help(format!("remove the call and set `{variable}` in the process environment"));
                },
            );
        }
    }
}
