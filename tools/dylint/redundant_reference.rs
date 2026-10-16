use core::{mem, ops::ControlFlow};

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, res::MaybeDef as _, sym};
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    Expr, ExprKind, HirId, ItemKind, Lifetime, LifetimeKind, Mutability, Pat, TyKind,
    def_id::{DefId, LocalDefId},
    intravisit::Visitor,
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, layout::LayoutOf as _};
use rustc_span::Span;

use crate::{Access, Place, access, accessed_field, enclosing_trait_impl, pattern_fields, place};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    REDUNDANT_REFERENCE,
    Warn,
    "shared reference fields read only to copy one field of their referent"
}

// --- [MODELS] --------------------------------------------------------------------------

enum FieldUse {
    Subfield { subfield: DefId, reads: Vec<Span> },
    Other,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct RedundantReference {
    fields: FxIndexMap<LocalDefId, FieldUse>,
}

impl_lint_pass!(RedundantReference => [REDUNDANT_REFERENCE]);

impl<'tcx> LateLintPass<'tcx> for RedundantReference {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::Field(_, ident) = expr.kind
            && matches!(typeck.expr_ty(expr).kind(), ty::Ref(_, _, Mutability::Not))
            && let Some(Place::Field(field)) = place(cx, expr)
            && !enclosing_trait_impl(cx, expr.hir_id).is_some_and(|implementation| {
                cx.tcx.is_automatically_derived(implementation) && [sym::Debug, sym::Clone].into_iter().any(|derived| cx.tcx.impl_trait_id(implementation).is_diag_item(cx, derived))
            })
        {
            let read = get_parent_expr(cx, expr)
                .filter(|parent| matches!(access(cx, parent), Access::Copied))
                .and_then(|parent| if let ExprKind::Field(_, subfield) = parent.kind { Some((accessed_field(typeck, parent)?, ident.span.to(subfield.span))) } else { None });
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
        self.fields.extend(pattern_fields(cx.typeck_results(), pat).filter_map(|(field, _)| field.as_local()).map(|field| (field, FieldUse::Other)));
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
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

        let findings = mem::take(&mut self.fields).into_iter().filter_map(|(field, field_use)| {
            if let FieldUse::Subfield { subfield, reads } = field_use
                && let item = cx.tcx.hir_expect_item(cx.tcx.local_parent(field))
                && let ItemKind::Struct(ident, _, data) = item.kind
                && let Some(field_def) = data.fields().iter().find(|field_def| field_def.def_id == field)
                && !field_def.span.from_expansion()
                && !cx.effective_visibilities.is_exported(field)
                && let TyKind::Ref(lifetime, _, Mutability::Not) = field_def.ty.kind
                && let reference = cx.tcx.type_of(field).instantiate_identity().skip_normalization()
                && let ty::Adt(owner, args) = reference.peel_refs().kind()
                && let Some(read) = owner.all_fields().find(|candidate| candidate.did == subfield)
                && let ty = read.ty(cx.tcx, args).skip_normalization()
                && cx.layout_of(ty).is_ok_and(|stored| cx.layout_of(reference).is_ok_and(|replaced| stored.size <= replaced.size))
            {
                let released = matches!(lifetime.kind, LifetimeKind::Param(param) if (OtherLifetimeUse { param, field: lifetime.hir_id }).visit_item(item).is_continue());
                Some((field_def, ident, read.name, ty, reads, released.then_some(lifetime)))
            } else {
                None
            }
        });
        for (field_def, ident, name, ty, reads, released) in findings {
            let field_name = field_def.ident;
            let removal = released.map_or_default(|lifetime| format!(" and remove `{lifetime}`"));
            span_lint_hir_and_then(cx, REDUNDANT_REFERENCE, field_def.hir_id, field_def.span, format!("`{ident}` holds `{field_name}` by reference only to copy `{field_name}.{name}`"), |diag| {
                diag.span_note(reads, format!("`{field_name}.{name}` copied here")).help(format!("store the `{ty}` of `{field_name}.{name}` in place of `{field_name}`{removal}"));
            });
        }
    }
}
