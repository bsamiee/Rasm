use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    setters: FxHashMap<DefId, &'static str>,
}

lint!(Pass; ENVIRONMENT_OVERRIDE: "builder calls overriding a value that a dependency reads from its standard environment variable");

impl Pass {
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

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let span = match expr.kind {
            ExprKind::MethodCall(segment, ..) => segment.ident.span.to(expr.span.shrink_to_hi()),
            ExprKind::Call(..) => expr.span,
            _ => return,
        };
        if !expanded_externally(cx, expr.span)
            && let Some(setter) = fn_def_id(cx, expr)
            && let Some(&variable) = self.setters.get(&setter)
        {
            span_lint_hir_and_then(
                cx,
                ENVIRONMENT_OVERRIDE,
                expr.hir_id,
                span,
                format!("`{}` overrides `{variable}`, which `{}` reads", cx.tcx.item_name(setter), cx.tcx.crate_name(setter.krate)),
                |diag| {
                    diag.help(format!("delete the call and set `{variable}` in the process environment"));
                },
            );
        }
    }
}
