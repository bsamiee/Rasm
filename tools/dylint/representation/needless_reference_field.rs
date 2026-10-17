use core::{mem, ops::ControlFlow};

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, res::MaybeDef as _, sym};
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    Expr, ExprKind, HirId, ItemKind, Lifetime, LifetimeKind, Mutability, Pat, TyKind,
    def_id::{DefId, LocalDefId},
    intravisit::Visitor,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, layout::LayoutOf as _};
use rustc_span::Span;

use crate::{Access, Place, access, accessed_field, derived_impl, enclosing_trait_impl, expanded_externally, exported, pattern_fields, place};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    fields: FxIndexMap<LocalDefId, Option<(DefId, Vec<Span>)>>,
}

lint!(default Pass; NEEDLESS_REFERENCE_FIELD: "shared reference fields read only to copy one field of their referent");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::Field(_, ident) = expr.kind
            && matches!(typeck.expr_ty(expr).kind(), ty::Ref(_, _, Mutability::Not))
            && let Some(Place::Field(field)) = place(cx, expr)
            && !enclosing_trait_impl(cx, expr.hir_id)
                .is_some_and(|implementation| derived_impl(cx.tcx, implementation) && matches!(cx.tcx.impl_trait_id(implementation).opt_diag_name(cx), Some(sym::Debug | sym::Clone)))
        {
            let read = get_parent_expr(cx, expr).and_then(|parent| match (parent.kind, access(cx, parent)) {
                (ExprKind::Field(_, subfield), Access::Copied) => Some((accessed_field(typeck, parent)?, ident.span.to(subfield.span))),
                (_, Access::Copied | Access::Borrowed | Access::Moved | Access::Mutated | Access::Assigned(_)) => None,
            });
            let used = self.fields.entry(field).or_insert_with(|| read.map(|(subfield, _)| (subfield, Vec::new())));
            *used = used.take().zip(read).and_then(|((subfield, mut reads), (read, span))| {
                (subfield == read).then(|| {
                    reads.push(span);
                    (subfield, reads)
                })
            });
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.fields.extend(pattern_fields(cx.typeck_results(), pat).filter_map(|(field, _)| Some((field.as_local()?, None)))); }

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

        for (field, (subfield, reads)) in mem::take(&mut self.fields).into_iter().filter_map(|(field, used)| Some((field, used?))) {
            let item = cx.tcx.hir_expect_item(cx.tcx.local_parent(field));
            if let ItemKind::Struct(ident, _, data) = item.kind
                && let Some(field_def) = data.fields().iter().find(|field_def| field_def.def_id == field)
                && !expanded_externally(cx, field_def.span)
                && !exported(cx, field)
                && let TyKind::Ref(lifetime, _, Mutability::Not) = field_def.ty.kind
                && let reference = cx.tcx.type_of(field).instantiate_identity().skip_normalization()
                && let ty::Adt(owner, args) = reference.peel_refs().kind()
                && let Some(read) = owner.all_fields().find(|candidate| candidate.did == subfield)
                && let ty = read.ty(cx.tcx, args).skip_normalization()
                && match (cx.layout_of(ty), cx.layout_of(reference)) {
                    (Ok(stored), Ok(replaced)) => stored.size <= replaced.size,
                    (Err(_), _) | (_, Err(_)) => false,
                }
            {
                let (field_name, name) = (field_def.ident, read.name);
                let removal = if let LifetimeKind::Param(param) = lifetime.kind
                    && (OtherLifetimeUse { param, field: lifetime.hir_id }).visit_item(item).is_continue()
                {
                    format!(" and remove `{lifetime}`")
                } else {
                    String::new()
                };
                span_lint_hir_and_then(
                    cx,
                    NEEDLESS_REFERENCE_FIELD,
                    field_def.hir_id,
                    field_def.span,
                    format!("`{ident}` holds `{field_name}` by reference only to copy `{field_name}.{name}`"),
                    |diag| {
                        diag.span_note(reads, format!("`{field_name}.{name}` copied here")).help(format!("store the `{ty}` of `{field_name}.{name}` in place of `{field_name}`{removal}"));
                    },
                );
            }
        }
    }
}
