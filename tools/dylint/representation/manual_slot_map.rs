use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, res::MaybeDef as _, sym, ty::option_arg_ty};
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap, FxIndexSet};
use rustc_hir::{
    Expr, ExprKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt, print::with_forced_trimmed_paths};

use crate::{Place, access, expanded_externally, impl_owner, path, place, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum Recycling {
    Freed,
    Reused,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, Recycling>,
    reverse: Option<DefId>,
    free_lists: FxIndexMap<LocalDefId, FxIndexSet<(Recycling, LocalDefId)>>,
    mutated: FxHashSet<(LocalDefId, LocalDefId)>,
}

lint!(Pass; MANUAL_SLOT_MAP: "structs holding `Vec<Option<T>>` slots and a free list of indexes that the methods writing a slot push to and pop from");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            methods: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("std::vec::Vec::push", Recycling::Freed),
                    ("std::vec::Vec::pop", Recycling::Reused),
                    ("std::collections::VecDeque::push_back", Recycling::Freed),
                    ("std::collections::VecDeque::push_front", Recycling::Freed),
                    ("std::collections::VecDeque::pop_back", Recycling::Reused),
                    ("std::collections::VecDeque::pop_front", Recycling::Reused),
                    ("std::collections::BTreeSet::insert", Recycling::Freed),
                    ("std::collections::BTreeSet::pop_first", Recycling::Reused),
                    ("std::collections::BTreeSet::pop_last", Recycling::Reused),
                    ("std::collections::BinaryHeap::push", Recycling::Freed),
                    ("std::collections::BinaryHeap::pop", Recycling::Reused),
                ],
            ),
            reverse: path(tcx, PathNS::Type, "core::cmp::Reverse"),
            free_lists: FxIndexMap::default(),
            mutated: FxHashSet::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let body = cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(expr.hir_id));
        if let Some(Place::Field(field)) = place(cx, expr)
            && access(cx, expr).writes()
        {
            self.mutated.insert((field, body));
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(method) = fn_def_id(cx, expr)
            && let Some(&recycling) = self.methods.get(&method)
            && let Some(Place::Field(field)) = place(cx, receiver)
            && let ty::Adt(collection, args) = cx.tcx.type_of(field).instantiate_identity().skip_normalization().kind()
            && impl_owner(cx.tcx, method).and_then(Ty::ty_adt_def) == Some(*collection)
            && let index = match args.type_at(0).kind() {
                ty::Adt(reverse, inner) if Some(reverse.did()) == self.reverse => inner.type_at(0),
                _ => args.type_at(0),
            }
            && matches!(index.kind(), ty::Uint(ty::UintTy::Usize | ty::UintTy::U32 | ty::UintTy::U64))
        {
            self.free_lists.entry(field).or_default().insert((recycling, body));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        for (free, recycled) in mem::take(&mut self.free_lists) {
            let owner = tcx.local_parent(free);
            if let Some((slots, held)) =
                tcx.adt_def(owner).all_fields().filter_map(|field| field.did.as_local()).find_map(|field| match tcx.type_of(field).instantiate_identity().skip_normalization().kind() {
                    ty::Adt(vector, args) if vector.is_diag_item(cx, sym::Vec) => Some((field, option_arg_ty(cx, args.type_at(0))?)),
                    _ => None,
                })
                && recycled.iter().filter(|&&(_, body)| self.mutated.contains(&(slots, body))).map(|&(recycling, _)| recycling).collect::<FxHashSet<_>>().len() == 2
            {
                let (name, slots_name, free_name, held) = (tcx.item_name(owner), tcx.item_name(slots), tcx.item_name(free), with_forced_trimmed_paths!(held.to_string()));
                span_lint_hir_and_then(
                    cx,
                    MANUAL_SLOT_MAP,
                    tcx.local_def_id_to_hir_id(owner),
                    tcx.def_span(owner),
                    format!("`{name}` tracks reusable indexes of `{slots_name}` in free list `{free_name}` by hand"),
                    |diag| {
                        diag.help(format!("replace `{slots_name}` and `{free_name}` with one `slotmap::SlotMap<K, {held}>` keyed by a `slotmap::new_key_type!` key"));
                    },
                );
            }
        }
    }
}
