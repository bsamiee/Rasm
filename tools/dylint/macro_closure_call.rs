use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_hir::{ClosureKind, Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_span::{ExpnKind, MacroKind};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MACRO_CLOSURE_CALL,
    Warn,
    "closures a local `macro_rules!` takes as an `expr` fragment and calls in its expansion"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] MacroClosureCall => [MACRO_CLOSURE_CALL]);

impl<'tcx> LateLintPass<'tcx> for MacroClosureCall {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(callee, _) = expr.kind
            && let ExprKind::Closure(closure) = callee.kind
            && closure.kind == ClosureKind::Closure
            && callee.span.ctxt() != expr.span.ctxt()
            && let expansion = expr.span.ctxt().outer_expn_data()
            && let ExpnKind::Macro(MacroKind::Bang, name) = expansion.kind
            && expansion.macro_def_id.is_some_and(DefId::is_local)
        {
            span_lint_hir_and_then(cx, MACRO_CLOSURE_CALL, callee.hir_id, callee.span, format!("`{name}!` calls this closure in its expansion"), |diag| {
                diag.span_note(expr.span, "the expansion calls the closure here");
                diag.help("match the closure in the macro's matcher as `|$<param>:pat_param| $<body>:expr` and transcribe the body with each parameter bound to its argument");
            });
        }
    }
}
