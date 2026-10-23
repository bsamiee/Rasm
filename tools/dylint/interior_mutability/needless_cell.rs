use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    get_enclosing_block,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
};
use rustc_hir::{LetStmt, PatKind};
use rustc_lint::{LateContext, LateLintPass};

use crate::{any_expr, expanded_externally, method_call_on};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(NEEDLESS_CELL: "local `Cell` and `RefCell` bindings used only as method receivers and captured by no closure");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        let typeck = cx.typeck_results();
        if let PatKind::Binding(_, binding, name, None) = local.pat.kind
            && !expanded_externally(cx, local.span)
            && let Some(cell) = typeck.pat_ty(local.pat).opt_diag_name(cx).filter(|cell| matches!(*cell, sym::Cell | sym::RefCell))
            && !typeck.closure_min_captures.items().any(|(_, captures)| captures.contains_key(&binding))
            && get_enclosing_block(cx, local.hir_id).is_some_and(|block| !any_expr(cx.tcx, block, |used| used.res_local_id() == Some(binding) && method_call_on(cx, used).is_none()))
        {
            span_lint_hir_and_then(cx, NEEDLESS_CELL, local.hir_id, local.span, format!("`{name}` does not need `{cell}`"), |diag| {
                diag.note(format!("no closure captures `{name}` and every use is a method receiver"));
                diag.help(format!("bind the value with `let mut {name}` and replace each `{cell}` method call with a direct read or write"));
            });
        }
    }
}
