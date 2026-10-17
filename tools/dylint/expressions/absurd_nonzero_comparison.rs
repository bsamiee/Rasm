use clippy_utils::{
    comparisons::{Rel, normalize_comparison},
    consts::{ConstEvalCtxt, Constant},
    diagnostics::span_lint_hir_and_then,
    fn_def_id, path_to_local_with_projections,
    paths::PathNS,
    res::MaybeDef as _,
    source::snippet,
    sym,
};
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt};

use crate::{expanded_externally, path, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    get: Option<DefId>,
}

lint!(Pass; ABSURD_NONZERO_COMPARISON: "comparisons of a `NonZero` value with `0` or `1` that the type decides");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { get: path(tcx, PathNS::Value, "core::num::NonZero::get") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let read = |operand: &'tcx Expr<'tcx>| match operand.kind {
            ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver])
                if fn_def_id(cx, operand).is_some_and(|called| Some(called) == self.get || matches!(called.opt_parent(cx).opt_diag_name(cx), Some(sym::From | sym::Into)))
                    && typeck.expr_ty(receiver).peel_refs().is_diag_item(cx, sym::NonZero)
                    && typeck.expr_ty(operand).is_integral() =>
                Some(receiver),
            _ => None,
        };
        if let ExprKind::Binary(operator, left, right) = expr.kind
            && let Some((relation, lower, upper)) = normalize_comparison(operator.node, left, right)
            && let Some((receiver, bound, below)) = read(lower).map(|receiver| (receiver, upper, true)).or_else(|| read(upper).map(|receiver| (receiver, lower, false)))
            && let unsigned = matches!(typeck.expr_ty(bound).kind(), ty::Uint(_))
            && let Some(fixed) = match (relation, ConstEvalCtxt::new(cx).eval(bound), below) {
                (Rel::Eq, Some(Constant::Int(0)), _) => Some(false),
                (Rel::Ne, Some(Constant::Int(0)), _) => Some(true),
                (Rel::Lt, Some(Constant::Int(0)), false) | (Rel::Le, Some(Constant::Int(1)), false) if unsigned => Some(true),
                (Rel::Lt, Some(Constant::Int(1)), true) | (Rel::Le, Some(Constant::Int(0)), true) if unsigned => Some(false),
                _ => None,
            }
            && !expanded_externally(cx, expr.span)
        {
            let nonzero = typeck.expr_ty_adjusted(receiver).peel_refs();
            let applicability = if path_to_local_with_projections(receiver.peel_borrows()).is_some() { Applicability::MachineApplicable } else { Applicability::MaybeIncorrect };
            span_lint_hir_and_then(cx, ABSURD_NONZERO_COMPARISON, expr.hir_id, expr.span, format!("`{}` is always `{fixed}`, `{nonzero}` holds no zero", snippet(cx, expr.span, "..")), |diag| {
                suggest(cx, expr.hir_id, diag, expr.span, format!("replace the comparison with `{fixed}`"), fixed.to_string(), applicability);
            });
        }
    }
}
