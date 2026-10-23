use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_hir::{ClosureKind, Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{ExpnKind, MacroKind};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REDUNDANT_CLOSURE_CALL_IN_MACRO: "closure arguments and parameterized closure declarations that a local `macro_rules!` expansion calls");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(callee, arguments) = expr.kind
            && let ExprKind::Closure(closure) = callee.kind
            && closure.kind == ClosureKind::Closure
            && let expansion = expr.span.ctxt().outer_expn_data()
            && let ExpnKind::Macro(MacroKind::Bang, name) = expansion.kind
            && expansion.macro_def_id.is_some_and(DefId::is_local)
            && let passed = callee.span.ctxt() != expr.span.ctxt()
            && (passed || !arguments.is_empty())
        {
            let (message, help) = if passed {
                ("calls its closure argument", "match the closure as `|$parameter:ident $(: $type:ty)?| $body:expr` and expand to `{ let $parameter $(: $type)? = <argument>; $body }`")
            } else {
                ("calls the closure it declares", "replace the call with `{ let <parameter> = <argument>; <body> }`")
            };
            span_lint_hir_and_then(cx, REDUNDANT_CLOSURE_CALL_IN_MACRO, callee.hir_id, callee.span, format!("`{name}!` {message} in its expansion"), |diag| {
                if passed {
                    diag.span_note(expr.span, "called here");
                }
                diag.help(help);
            });
        }
    }
}
