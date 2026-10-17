use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    get_enclosing_block,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
};
use rustc_hir::{LetStmt, PatKind};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};

use crate::{any_expr, receiving_call};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    UNSHARED_CELL,
    Warn,
    "local `Cell` or `RefCell` that no closure captures and no call shares"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] UnsharedCell => [UNSHARED_CELL]);

impl<'tcx> LateLintPass<'tcx> for UnsharedCell {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        let typeck = cx.typeck_results();
        if let PatKind::Binding(_, binding, name, None) = local.pat.kind
            && !local.span.from_expansion()
            && let Some(cell) = typeck.pat_ty(local.pat).opt_diag_name(cx).filter(|cell| matches!(*cell, sym::Cell | sym::RefCell))
            && !typeck.closure_min_captures.items().any(|(_, captures)| captures.contains_key(&binding))
            && get_enclosing_block(cx, local.hir_id).is_some_and(|block| !any_expr(cx.tcx, block, |used| used.res_local_id() == Some(binding) && receiving_call(cx, used).is_none()))
        {
            span_lint_hir_and_then(cx, UNSHARED_CELL, local.hir_id, local.span, format!("`{name}` holds its value in a `{cell}` that no closure captures and no call shares"), |diag| {
                diag.help("bind the value with `let mut` and read and write it in place");
            });
        }
    }
}
