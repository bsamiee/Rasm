use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    source::snippet,
    sym,
    usage::local_used_after_expr,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_errors::Applicability;
use rustc_hir::{BorrowKind, Expr, ExprKind, Mutability, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, path, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    lossy: Option<DefId>,
}

lint!(Pass; MANUAL_FROM_UTF8_LOSSY_OWNED: "`String::from_utf8_lossy(&bytes)` converted to an owned `String` when nothing reads the buffer afterwards");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { lossy: path(tcx, PathNS::Value, "alloc::string::String::from_utf8_lossy") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::MethodCall(segment, lossy, [], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && matches!(segment.ident.name, sym::into_owned | sym::to_string | sym::to_owned | sym::into)
            && typeck.expr_ty(expr).is_lang_item(cx, LangItem::String)
            && let ExprKind::Call(_, [borrowed]) = lossy.kind
            && fn_def_id(cx, lossy).is_some_and(|called| Some(called) == self.lossy)
            && let ExprKind::AddrOf(BorrowKind::Ref, Mutability::Not, bytes) = borrowed.kind
            && typeck.expr_ty(bytes).is_diag_item(cx, sym::Vec)
            && let projections = iter::successors(Some(bytes), |place| if let ExprKind::Field(base, _) = place.kind { Some(base) } else { None })
            && projections.clone().skip(1).all(|base| typeck.expr_adjustments(base).is_empty() && typeck.expr_ty(base).ty_adt_def().is_some_and(|owner| owner.destructor(cx.tcx).is_none()))
            && let Some(local) = projections.last().res_local_id()
            && cx.tcx.hir_enclosing_body_owner(local) == cx.tcx.hir_enclosing_body_owner(expr.hir_id)
            && !local_used_after_expr(cx, local, expr)
        {
            let bytes = snippet(cx, bytes.span, "..");
            span_lint_hir_and_then(cx, MANUAL_FROM_UTF8_LOSSY_OWNED, expr.hir_id, expr.span, format!("lossy decoding copies `{bytes}`, which nothing reads afterwards"), |diag| {
                suggest(cx, expr.hir_id, diag, expr.span, "call `String::from_utf8_lossy_owned`", format!("String::from_utf8_lossy_owned({bytes})"), Applicability::MachineApplicable);
            });
        }
    }
}
