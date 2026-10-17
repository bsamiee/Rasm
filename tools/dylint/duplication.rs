use core::{cell::RefCell, iter};

use clippy_utils::SpanlessEq;
use rustc_hir::{BodyId, Expr, ExprKind, HirId, def_id::LocalDefId};
use rustc_lint::LateContext;
use rustc_middle::ty::{BottomUpFolder, Ty, TyCtxt, TypeFoldable, TypeckResults};
use rustc_span::SyntaxContext;

use crate::parameter_bindings;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn equal_bodies<'tcx, F>(cx: &LateContext<'tcx>, left: LocalDefId, right: LocalDefId, mut differ: F) -> Option<Vec<(HirId, HirId)>>
where F: FnMut(&TypeckResults<'tcx>, &Expr<'_>, &TypeckResults<'tcx>, &Expr<'_>) -> bool {
    let closures = RefCell::new(Vec::<(BodyId, BodyId)>::new());
    let differences = RefCell::new(Vec::new());
    let equal = {
        let mut spanless = SpanlessEq::new(cx).expr_fallback(|left_typeck, left_expr, right_typeck, right_expr| {
            if let (ExprKind::Closure(left_closure), ExprKind::Closure(right_closure)) = (left_expr.kind, right_expr.kind) {
                closures.borrow_mut().push((left_closure.body, right_closure.body));
                left_closure.capture_clause == right_closure.capture_clause && left_closure.fn_decl.inputs.len() == right_closure.fn_decl.inputs.len()
            } else if differ(left_typeck, left_expr, right_typeck, right_expr) {
                differences.borrow_mut().push((left_expr.hir_id, right_expr.hir_id));
                true
            } else {
                false
            }
        });
        let mut inter = spanless.inter_expr(SyntaxContext::root());
        let mut compare = |left_body: BodyId, right_body: BodyId| {
            let (left_bindings, right_bindings) = (parameter_bindings(cx.tcx, left_body), parameter_bindings(cx.tcx, right_body));
            left_bindings.len() == right_bindings.len() && {
                inter.locals.extend(iter::zip(left_bindings, right_bindings));
                inter.eq_body(left_body, right_body)
            }
        };
        compare(cx.tcx.hir_body_owned_by(left).id(), cx.tcx.hir_body_owned_by(right).id())
            && iter::from_fn(|| closures.borrow_mut().pop()).all(|(left_body, right_body)| compare(left_body, right_body))
    };
    equal.then(|| differences.into_inner())
}

fn substitute_types<'tcx, T, F>(tcx: TyCtxt<'tcx>, value: T, substitute: F) -> T
where
    T: TypeFoldable<TyCtxt<'tcx>>,
    F: Fn(Ty<'tcx>) -> Option<Ty<'tcx>>,
{
    value.fold_with(&mut BottomUpFolder { tcx, ty_op: |ty| substitute(ty).unwrap_or(ty), lt_op: |region| region, ct_op: |constant| constant })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod duplicate_body;
pub(crate) mod duplicate_trait_constant;
pub(crate) mod duplicate_type;
