use clippy_utils::{
    SpanlessEq,
    diagnostics::span_lint_hir_and_then,
    eq_expr_value, fn_def_id, peel_blocks, peel_hir_expr_while,
    res::{MaybeDef as _, MaybeResPath as _},
    source::snippet,
    sym,
    ty::get_iterator_item_ty,
};
use itertools::Itertools as _;
use rustc_hir::{Block, BodyId, Expr, ExprKind, LetStmt, Pat, PatKind, Stmt, StmtKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};

use super::{collects_vec, repeatable};
use crate::{expanded_externally, parameters_match};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
struct Split<'tcx> {
    site: &'tcx Expr<'tcx>,
    source: &'tcx Expr<'tcx>,
    predicate: BodyId,
    value: &'tcx Expr<'tcx>,
    negated: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_PARTITION: "adjacent `filter` collections splitting one unchanged slice on a pure predicate and its negation");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_block(&mut self, cx: &LateContext<'tcx>, block: &'tcx Block<'tcx>) {
        fn split<'tcx>(cx: &LateContext<'tcx>, statement: &'tcx Stmt<'tcx>) -> Option<Split<'tcx>> {
            let StmtKind::Let(&LetStmt { pat: Pat { kind: PatKind::Binding(_, _, _, None), .. }, init: Some(collected), els: None, .. }) = statement.kind else { return None };
            let iterator = match collected.kind {
                ExprKind::MethodCall(segment, filtered, [], _) if segment.ident.name == sym::collect => filtered,
                ExprKind::Call(_, [filtered]) => filtered,
                _ => return None,
            };
            let collected_ty = cx.typeck_results().expr_ty(collected);
            if !fn_def_id(cx, collected).is_diag_item(cx, sym::iterator_collect_fn) || !collected_ty.is_diag_item(cx, sym::Vec) {
                return None;
            }
            let filtered = if fn_def_id(cx, iterator).is_diag_item(cx, sym::iter_copied) {
                let (ExprKind::MethodCall(_, inner, [], _) | ExprKind::Call(_, [inner])) = iterator.kind else { return None };
                inner
            } else {
                iterator
            };
            let (ExprKind::MethodCall(_, iterated, [predicate], _) | ExprKind::Call(_, [iterated, predicate])) = filtered.kind else { return None };
            let (ExprKind::MethodCall(_, source, [], _) | ExprKind::Call(_, [source])) = iterated.kind else { return None };
            let ExprKind::Closure(closure) = predicate.kind else { return None };
            let source = source.peel_borrows();
            if !fn_def_id(cx, filtered).is_diag_item(cx, sym::iter_filter)
                || !fn_def_id(cx, iterated).is_diag_item(cx, sym::slice_iter)
                || !match source.kind {
                    ExprKind::Path(_) => true,
                    ExprKind::Field(base, _) => repeatable(cx, base),
                    _ => false,
                }
                || peel_hir_expr_while(source, |place| if let ExprKind::Field(base, _) = place.kind { Some(base) } else { None }).res_local_id().is_none()
                || get_iterator_item_ty(cx, cx.typeck_results().expr_ty(iterator)).is_none_or(|item| !collects_vec(cx, item, collected_ty))
            {
                return None;
            }
            let held = cx.typeck_results().expr_ty(source).peel_refs();
            if !(held.is_slice() || held.is_array() || held.is_diag_item(cx, sym::Vec)) || !held.is_freeze(cx.tcx, cx.typing_env()) {
                return None;
            }
            let body = cx.tcx.hir_body(closure.body);
            let value = peel_blocks(body.value);
            let (negated, value) = if let ExprKind::Unary(UnOp::Not, value) = value.kind { (true, value) } else { (false, value) };
            let context = LateContext { enclosing_body: Some(body.id()), typeck_results: Some(cx.tcx.typeck_body(body.id())), param_env: cx.tcx.param_env(closure.def_id), ..*cx };
            (context.typeck_results().expr_ty(value).is_bool() && repeatable(&context, value) && !expanded_externally(cx, filtered.span)).then_some(Split {
                site: collected,
                source,
                predicate: body.id(),
                value,
                negated,
            })
        }

        for (left, right) in block.stmts.iter().map(|statement| split(cx, statement)).tuple_windows().filter_map(|(left, right)| Some((left?, right?))).filter(|(left, right)| {
            left.negated != right.negated
                && cx.tcx.erase_and_anonymize_regions(cx.typeck_results().expr_ty(left.site)) == cx.tcx.erase_and_anonymize_regions(cx.typeck_results().expr_ty(right.site))
                && eq_expr_value(cx, right.site.span.ctxt(), left.source, right.source)
                && {
                    let mut spanless = SpanlessEq::new(cx);
                    let mut inter = spanless.inter_expr(right.site.span.ctxt());
                    parameters_match(cx.tcx, &mut inter.locals, left.predicate, right.predicate) && inter.eq_expr(left.value, right.value)
                }
        }) {
            let source = snippet(cx, right.source.span, "..");
            span_lint_hir_and_then(cx, MANUAL_PARTITION, right.site.hir_id, right.site.span, format!("two `filter` passes split `{source}` on one predicate and its negation"), |diag| {
                diag.span_note(left.site.span, "other pass here");
                diag.help(format!("iterate `{source}` once with `Iterator::partition`"));
            });
        }
    }
}
