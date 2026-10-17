use core::mem;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    res::{MaybeDef as _, MaybeTypeckRes as _},
    sym,
    ty::option_arg_ty,
};
use rustc_data_structures::fx::{FxIndexMap, FxIndexSet};
use rustc_hir::{Expr, ExprKind, def_id::LocalDefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_span::Symbol;

use crate::{Place, expanded_externally, place};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    free_lists: FxIndexMap<LocalDefId, FxIndexSet<Symbol>>,
}

lint!(default Pass; MANUAL_SLOT_MAP: "structs holding `Vec<Option<T>>` slots and a `Vec<usize>` free list that their methods push to and pop from");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, ..) = expr.kind
            && matches!(segment.ident.name, sym::push | sym::pop)
            && !expanded_externally(cx, expr.span)
            && let Some(Place::Field(field)) = place(cx, receiver)
            && matches!(cx.tcx.type_of(field).instantiate_identity().skip_normalization().kind(), ty::Adt(vector, args) if vector.is_diag_item(cx, sym::Vec) && args.type_at(0).is_usize())
            && cx.ty_based_def(expr).opt_parent(cx).opt_impl_ty(cx).is_diag_item(cx, sym::Vec)
        {
            self.free_lists.entry(field).or_default().insert(segment.ident.name);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        for (free, recycled) in mem::take(&mut self.free_lists) {
            let owner = tcx.local_parent(free);
            if recycled.len() == 2
                && let Some(slots) = tcx.adt_def(owner).all_fields().filter_map(|field| field.did.as_local()).find(|&field| {
                    let declared = tcx.type_of(field).instantiate_identity().skip_normalization();
                    matches!(declared.kind(), ty::Adt(vector, args) if vector.is_diag_item(cx, sym::Vec) && option_arg_ty(cx, args.type_at(0)).is_some())
                })
            {
                let (name, slots_name, free_name) = (tcx.item_name(owner), tcx.item_name(slots), tcx.item_name(free));
                span_lint_hir_and_then(
                    cx,
                    MANUAL_SLOT_MAP,
                    tcx.local_def_id_to_hir_id(owner),
                    tcx.def_span(owner),
                    format!("`{name}` tracks reusable indexes of `{slots_name}` in free list `{free_name}` by hand"),
                    |diag| {
                        diag.help("hold `slotmap::SlotMap<K, T>`, `insert` returns a generational key and `remove` frees its slot");
                    },
                );
            }
        }
    }
}
