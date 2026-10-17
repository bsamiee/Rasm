use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_enclosing_block,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    visitors::for_each_expr,
};
use rustc_hir::{Expr, ExprKind, Node, PatKind};
use rustc_lint::{LateContext, LateLintPass};

use crate::{expanded_externally, method_call_on};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(BUFFERED_JOIN: "`Vec` bindings a loop fills with `push` that only `join` reads");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, [_], _) = expr.kind
            && segment.ident.name == sym::join
            && !expanded_externally(cx, expr.span)
            && let Some(binding) = receiver.res_local_id()
            && let Node::LetStmt(local) = cx.tcx.parent_hir_node(binding)
            && let PatKind::Binding(_, _, name, None) = local.pat.kind
            && local.init.and_then(|init| fn_def_id(cx, init)).opt_diag_name(cx).is_some_and(|built| matches!(built, sym::vec_new | sym::vec_with_capacity))
            && let Some(block) = get_enclosing_block(cx, local.hir_id)
        {
            let mut pushed = false;
            let read = for_each_expr(cx.tcx, block, |used| match (used.res_local_id() == Some(binding) && used.hir_id != receiver.hir_id).then(|| method_call_on(cx, used)) {
                None => ControlFlow::Continue(()),
                Some(Some((_, method)))
                    if cx.tcx.item_name(method) == sym::push
                        && cx
                            .tcx
                            .hir_parent_iter(used.hir_id)
                            .take_while(|&(ancestor, _)| ancestor != block.hir_id)
                            .any(|(_, node)| matches!(node, Node::Expr(Expr { kind: ExprKind::Loop(..), .. }))) =>
                {
                    pushed = true;
                    ControlFlow::Continue(())
                }
                Some(_) => ControlFlow::Break(()),
            });
            if pushed && read.is_none() {
                span_lint_hir_and_then(cx, BUFFERED_JOIN, expr.hir_id, expr.span, format!("loop pushes into `{name}` only for `join` to read it"), |diag| {
                    diag.help("call `Itertools::join(<iterator>, <separator>)` on an iterator yielding each pushed item (`use itertools::Itertools as _;`)");
                });
            }
        }
    }
}
