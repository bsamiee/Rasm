use clippy_utils::{diagnostics::span_lint_hir_and_then, source::walk_span_to_context};
use rustc_hir::Expr;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{Recorded, is_error};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    recorded: Recorded,
}

lint!(Pass; ERROR_FIELD_WITHOUT_SOURCES: "tracing fields recording an error outside `std::error::Report`");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { recorded: Recorded::new(tcx) } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some((value, call)) = self.recorded.field(cx, expr)
            && let held = cx.typeck_results().expr_ty(value)
            && is_error(cx, held)
            && let span = walk_span_to_context(value.span, call.ctxt()).unwrap_or(call)
        {
            span_lint_hir_and_then(cx, ERROR_FIELD_WITHOUT_SOURCES, expr.hir_id, span, format!("tracing field records error `{}` outside `Report`", held.peel_refs()), |diag| {
                diag.help("record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`");
            });
        }
    }
}
