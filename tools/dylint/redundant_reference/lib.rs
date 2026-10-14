#![feature(rustc_private)]

extern crate rustc_data_structures;
extern crate rustc_hir;
extern crate rustc_middle;
extern crate rustc_span;

use std::{mem, ops::ControlFlow};

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, res::MaybeDef as _, sym, ty::is_copy};
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    Expr, ExprKind, HirId, ItemKind, Lifetime, LifetimeKind, Mutability, Pat, PatKind, TyKind, VariantData,
    def_id::{DefId, LocalDefId},
    intravisit::Visitor,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TypeckResults, layout::LayoutOf as _};
use rustc_span::Span;

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::impl_late_lint! {
    pub REDUNDANT_REFERENCE,
    Warn,
    "reference field read only for one `Copy` subfield",
    RedundantReference::default()
}

#[derive(Default)]
struct RedundantReference {
    fields: FxIndexMap<LocalDefId, FieldUse>,
}

enum FieldUse {
    Subfield { subfield: DefId, reads: Vec<Span> },
    Other,
}

struct OtherLifetimeUse {
    param: LocalDefId,
    field: HirId,
}

impl<'tcx> Visitor<'tcx> for OtherLifetimeUse {
    type Result = ControlFlow<()>;

    fn visit_lifetime(&mut self, lifetime: &'tcx Lifetime) -> Self::Result {
        if lifetime.kind == LifetimeKind::Param(self.param) && lifetime.hir_id != self.field { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn field_of(typeck: &TypeckResults<'_>, access: &Expr<'_>) -> Option<DefId> {
    if let ExprKind::Field(operand, _) = access.kind {
        Some(typeck.expr_ty_adjusted(operand).peel_refs().ty_adt_def()?.non_enum_variant().fields.get(typeck.opt_field_index(access.hir_id)?)?.did)
    } else {
        None
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for RedundantReference {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::Field(operand, _) = expr.kind
            && let Some(field) = field_of(typeck, expr).and_then(DefId::as_local)
            && !cx.tcx.trait_impl_of_assoc(cx.tcx.typeck_root_def_id(cx.tcx.hir_enclosing_body_owner(expr.hir_id).to_def_id())).is_some_and(|implementation| {
                cx.tcx.is_automatically_derived(implementation) && [sym::Debug, sym::Clone].into_iter().any(|derived| cx.tcx.impl_trait_id(implementation).is_diag_item(cx, derived))
            })
        {
            let read = get_parent_expr(cx, expr).and_then(|parent| Some((field_of(typeck, parent).filter(|_| is_copy(cx, typeck.expr_ty(parent)))?, parent.span.with_lo(operand.span.hi()))));
            match (self.fields.get_mut(&field), read) {
                (Some(FieldUse::Subfield { subfield, reads }), Some((read, span))) if *subfield == read => reads.push(span),
                (Some(field_use), _) => *field_use = FieldUse::Other,
                (None, read) => {
                    self.fields.insert(field, read.map_or(FieldUse::Other, |(subfield, span)| FieldUse::Subfield { subfield, reads: vec![span] }));
                }
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let typeck = cx.typeck_results();
        if let PatKind::Struct(_, fields, _) = pat.kind
            && let Some(adt) = typeck.pat_ty(pat).ty_adt_def().filter(|adt| adt.is_struct())
        {
            self.fields.extend(fields.iter().filter_map(|field| adt.non_enum_variant().fields.get(typeck.opt_field_index(field.hir_id)?)?.did.as_local()).map(|field| (field, FieldUse::Other)));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (field, field_use) in mem::take(&mut self.fields) {
            let owner = cx.tcx.local_parent(field);
            let item = cx.tcx.hir_expect_item(owner);
            if let FieldUse::Subfield { subfield, reads } = field_use
                && let ItemKind::Struct(ident, _, VariantData::Struct { fields, recovered: _ }) = item.kind
                && let Some(field_def) = fields.iter().find(|field_def| field_def.def_id == field)
                && !(cx.tcx.visibility(owner).is_public() && cx.tcx.visibility(field).is_public())
                && let TyKind::Ref(lifetime, _, Mutability::Not) = field_def.ty.kind
                && let LifetimeKind::Param(param) = lifetime.kind
                && (OtherLifetimeUse { param, field: lifetime.hir_id }).visit_item(item).is_continue()
                && let reference = cx.tcx.type_of(field).instantiate_identity().skip_normalization()
                && let ty::Adt(_, args) = reference.peel_refs().kind()
                && let ty = cx.tcx.type_of(subfield).instantiate(cx.tcx, args).skip_normalization()
                && cx.layout_of(ty).is_ok_and(|stored| cx.layout_of(reference).is_ok_and(|replaced| stored.size <= replaced.size))
            {
                let (name, field_name) = (cx.tcx.item_name(subfield), field_def.ident);
                span_lint_hir_and_then(
                    cx,
                    REDUNDANT_REFERENCE,
                    item.hir_id(),
                    field_def.span,
                    format!("`{field_name}` is the only field of `{ident}` naming `{lifetime}` and is read only for `{field_name}.{name}`"),
                    |diag| {
                        diag.span_note(reads, format!("`{field_name}.{name}` read here")).help(format!("store `{name}: {ty}` in place of `{field_name}` and remove `{lifetime}`"));
                    },
                );
            }
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
