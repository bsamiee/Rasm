use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, sym};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{Expr, ExprKind, Item, ItemKind, Pat, PatKind, StructTailExpr, VariantData, def_id::LocalDefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{FieldDef, Ty};

use crate::generated;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    PLAIN_RECORD,
    Warn,
    "crate-private structs with no impl beyond `Debug` that every use builds or destructures whole"
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct PlainRecord {
    records: FxIndexMap<LocalDefId, bool>,
}

impl_lint_pass!(PlainRecord => [PLAIN_RECORD]);

impl PlainRecord {
    fn spoil(&mut self, held: Ty<'_>) {
        if let Some(record) = held.peel_refs().ty_adt_def().and_then(|adt| adt.did().as_local()) {
            self.records.insert(record, false);
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for PlainRecord {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let laid_out = || {
            let adt = cx.tcx.adt_def(item.owner_id);
            adt.repr().c() || adt.repr().transparent() || adt.repr().pack.is_some() || adt.repr().align.is_some() || adt.non_enum_variant().is_field_list_non_exhaustive()
        };
        match item.kind {
            ItemKind::Struct(_, _, VariantData::Struct { fields: [_, ..], .. }) if !item.span.from_expansion() && !cx.effective_visibilities.is_exported(item.owner_id.def_id) && !laid_out() => {
                self.records.entry(item.owner_id.def_id).or_insert(true);
            }
            ItemKind::Impl(implementation) if !implementation.of_trait.is_some_and(|declared| declared.trait_ref.trait_def_id().is_some_and(|declared| declared.is_diag_item(cx, sym::Debug))) => {
                self.spoil(cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization());
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
            ExprKind::Field(base, _) => self.spoil(typeck.expr_ty_adjusted(base)),
            ExprKind::MethodCall(_, receiver, ..) => self.spoil(typeck.expr_ty_adjusted(receiver)),
            ExprKind::Struct(_, _, StructTailExpr::Base(_) | StructTailExpr::DefaultFields(_) | StructTailExpr::NoneWithError(_)) => self.spoil(typeck.expr_ty(expr)),
            _ => {}
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::Struct(_, _, Some(_)) = pat.kind {
            self.spoil(cx.typeck_results().pat_ty(pat));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (&record, _) in self.records.iter().filter(|&(_, &plain)| plain) {
            let declared = |field: &FieldDef| cx.tcx.type_of(field.did).instantiate_identity().skip_normalization();
            let help = match &cx.tcx.adt_def(record).non_enum_variant().fields.raw {
                [only] => format!("replace it with its field's type `{}`, every use builds and destructures that value", declared(only)),
                fields => format!("replace it with the tuple `({})`, every use builds and destructures the tuple", fields.iter().map(declared).join(", ")),
            };
            span_lint_hir_and_then(
                cx,
                PLAIN_RECORD,
                cx.tcx.local_def_id_to_hir_id(record),
                cx.tcx.def_span(record),
                format!("`{}` has no impl beyond `Debug` and every use builds or destructures it whole", cx.tcx.item_name(record)),
                |diag| {
                    diag.help(help);
                },
            );
        }
    }
}
