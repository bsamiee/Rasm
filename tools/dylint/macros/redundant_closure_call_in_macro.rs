use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_hir::{ClosureKind, Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{ExpnKind, MacroKind};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REDUNDANT_CLOSURE_CALL_IN_MACRO: "closures a local `macro_rules!` takes as an `expr` fragment and calls in its expansion");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(callee, _) = expr.kind
            && let ExprKind::Closure(closure) = callee.kind
            && closure.kind == ClosureKind::Closure
            && callee.span.ctxt() != expr.span.ctxt()
            && let expansion = expr.span.ctxt().outer_expn_data()
            && let ExpnKind::Macro(MacroKind::Bang, name) = expansion.kind
            && expansion.macro_def_id.is_some_and(DefId::is_local)
        {
            span_lint_hir_and_then(cx, REDUNDANT_CLOSURE_CALL_IN_MACRO, callee.hir_id, callee.span, format!("`{name}!` calls its closure argument in its expansion"), |diag| {
                diag.span_note(expr.span, "called here");
                diag.help("match the closure as `|$<param>:pat_param| $<body>:expr` in the macro matcher and transcribe the body with each parameter bound to its argument");
            });
        }
    }
}
