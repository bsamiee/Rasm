use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    higher::{Range, RangeTy},
    is_integer_literal,
    res::{MaybeDef as _, MaybeTypeckRes as _},
    source::snippet,
    std_or_core, sym,
    ty::get_iterator_item_ty,
};
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, PatKind, PathSegment};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::Ident;

use super::{collects_vec, iterator_method};
use crate::{expanded_externally, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_VEC_FROM_FN: "`(0..n).map(f).collect::<Vec<_>>()` building a `Vec` from each index");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(_, mapped, [], _) = expr.kind
            && cx.ty_based_def(expr).is_diag_item(cx, sym::iterator_collect_fn)
            && let typeck = cx.typeck_results()
            && typeck.expr_ty(expr).is_diag_item(cx, sym::Vec)
            && let ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::map, .. }, .. }, range, [producer], _) = mapped.kind
            && iterator_method(cx, mapped)
            && let Some(Range { ty: RangeTy::OpsRange, start: Some(start), end: Some(end), .. }) = Range::hir(cx, range)
            && is_integer_literal(start, 0)
            && typeck.expr_ty(end).is_usize()
            && !matches!(producer.kind, ExprKind::Closure(closure) if matches!(cx.tcx.hir_body(closure.body).params, [parameter] if matches!(parameter.pat.kind, PatKind::Wild)))
            && !expanded_externally(cx, expr.span)
            && get_iterator_item_ty(cx, typeck.expr_ty(mapped)).is_some_and(|item| collects_vec(cx, item, typeck.expr_ty(expr)))
        {
            let (length, producer) = (snippet(cx, end.span, ".."), snippet(cx, producer.span, ".."));
            span_lint_hir_and_then(cx, MANUAL_VEC_FROM_FN, expr.hir_id, expr.span, format!("`(0..{length}).map(..).collect()` builds a `Vec` from each index"), |diag| {
                let help = "call `Vec::from_fn`";
                if std_or_core(cx) == Some("std") {
                    suggest(cx, expr.hir_id, diag, expr.span, help, format!("::std::vec::Vec::from_fn({length}, {producer})"), Applicability::MachineApplicable);
                } else {
                    diag.help(help);
                }
            });
        }
    }
}
