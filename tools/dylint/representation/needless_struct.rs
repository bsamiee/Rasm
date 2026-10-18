use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, sym};
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{Expr, ExprKind, Item, ItemKind, Pat, PatKind, StructTailExpr, VariantData, def_id::LocalDefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::Ty;

use crate::{expanded_externally, exported, generated};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    structs: FxIndexMap<LocalDefId, bool>,
}

lint!(default Pass; NEEDLESS_STRUCT: "crate-private one-field structs with no impl beyond `Debug` that every use builds or destructures whole");

impl Pass {
    fn disqualify(&mut self, held: Ty<'_>) {
        if let Some(structure) = held.peel_refs().ty_adt_def().and_then(|adt| adt.did().as_local()) {
            self.structs.insert(structure, false);
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let candidate = || {
            let adt = cx.tcx.adt_def(item.owner_id);
            let variant = adt.non_enum_variant();
            !adt.repr().c() && !adt.repr().transparent() && adt.repr().pack.is_none() && adt.repr().align.is_none() && !variant.is_field_list_non_exhaustive()
        };
        match item.kind {
            ItemKind::Struct(_, _, VariantData::Struct { fields: [_], .. }) if !expanded_externally(cx, item.span) && !exported(cx, item.owner_id.def_id) && candidate() => {
                self.structs.entry(item.owner_id.def_id).or_insert(true);
            }
            ItemKind::Impl(implementation) if !implementation.of_trait.is_some_and(|declared| declared.trait_ref.trait_def_id().is_diag_item(cx, sym::Debug)) => {
                self.disqualify(cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization());
            }
            _ => {}
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if generated(cx, expr.hir_id) {
            return;
        }
        let typeck = cx.typeck_results();
        match expr.kind {
            ExprKind::Field(base, _) => self.disqualify(typeck.expr_ty_adjusted(base)),
            ExprKind::MethodCall(_, receiver, ..) => self.disqualify(typeck.expr_ty_adjusted(receiver)),
            ExprKind::Struct(_, _, StructTailExpr::Base(_) | StructTailExpr::DefaultFields(_) | StructTailExpr::NoneWithError(_)) => self.disqualify(typeck.expr_ty(expr)),
            _ => {}
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::Struct(_, _, Some(_)) = pat.kind {
            self.disqualify(cx.typeck_results().pat_ty(pat));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (&structure, _) in self.structs.iter().filter(|&(_, &needless)| needless) {
            let name = cx.tcx.item_name(structure);
            let held = cx.tcx.type_of(cx.tcx.adt_def(structure).non_enum_variant().single_field().did).instantiate_identity().skip_normalization();
            span_lint_hir_and_then(
                cx,
                NEEDLESS_STRUCT,
                cx.tcx.local_def_id_to_hir_id(structure),
                cx.tcx.def_span(structure),
                format!("`{name}` has no impl beyond `Debug`, and every use builds or destructures it whole"),
                |diag| {
                    diag.help(format!("replace `{name}` with its field type `{held}`"));
                },
            );
        }
    }
}
