use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, res::MaybeDef as _, sym};
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    Expr, ExprKind, HirId, ItemKind, Lifetime, LifetimeKind, Mutability, Node, Pat, TyKind,
    def_id::{DefId, LocalDefId},
    intravisit::Visitor,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_span::Span;

use crate::{Access, access, accessed_field, derived_impl, enclosing_trait_impl, expanded_externally, exported, pattern_fields};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    fields: FxIndexMap<LocalDefId, Option<(DefId, Vec<Span>)>>,
}

lint!(default Pass; NEEDLESS_REFERENCE_FIELD: "shared reference fields read only to copy one field of their referent");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let ExprKind::Field(_, ident) = expr.kind else { return };
        let typeck = cx.typeck_results();
        if matches!(typeck.expr_ty(expr).kind(), ty::Ref(_, _, Mutability::Not))
            && let Some(field) = accessed_field(typeck, expr).and_then(DefId::as_local)
            && !enclosing_trait_impl(cx, expr.hir_id)
                .is_some_and(|implementation| derived_impl(cx.tcx, implementation) && matches!(cx.tcx.impl_trait_id(implementation).opt_diag_name(cx), Some(sym::Debug | sym::Clone)))
        {
            let read = get_parent_expr(cx, expr).and_then(|parent| match (parent.kind, access(cx, parent)) {
                (ExprKind::Field(_, subfield), Access::Copied) => Some((accessed_field(typeck, parent)?, ident.span.to(subfield.span))),
                (_, Access::Copied | Access::Borrowed | Access::Moved | Access::Mutated | Access::Assigned(_)) => None,
            });
            let used = self.fields.entry(field).or_insert_with(|| read.map(|(subfield, _)| (subfield, Vec::new())));
            match (used.as_mut(), read) {
                (Some((subfield, reads)), Some((read, span))) if *subfield == read => reads.push(span),
                _ => *used = None,
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.fields.extend(pattern_fields(cx.typeck_results(), pat).filter_map(|(field, _)| Some((field.as_local()?, None)))); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        struct LifetimeUses {
            single: FxHashMap<LocalDefId, Option<HirId>>,
        }

        impl<'tcx> Visitor<'tcx> for LifetimeUses {
            fn visit_lifetime(&mut self, lifetime: &'tcx Lifetime) {
                if let LifetimeKind::Param(param) = lifetime.kind {
                    self.single.entry(param).and_modify(|single| *single = None).or_insert(Some(lifetime.hir_id));
                }
            }
        }

        let mut lifetimes = FxHashMap::<LocalDefId, FxHashMap<LocalDefId, Option<HirId>>>::default();
        for (field, (subfield, reads)) in mem::take(&mut self.fields).into_iter().filter_map(|(field, used)| Some((field, used?))) {
            let item = cx.tcx.hir_expect_item(cx.tcx.local_parent(field));
            if let ItemKind::Struct(ident, ..) = item.kind
                && let Node::Field(field_def) = cx.tcx.hir_node_by_def_id(field)
                && !expanded_externally(cx, field_def.span)
                && !exported(cx, field)
                && let TyKind::Ref(lifetime, _, Mutability::Not) = field_def.ty.kind
                && let reference = cx.tcx.type_of(field).instantiate_identity().skip_normalization()
                && let ty::Adt(owner, args) = reference.peel_refs().kind()
                && cx.tcx.parent(subfield) == owner.did()
                && let ty = cx.tcx.type_of(subfield).instantiate(cx.tcx, args).skip_normalization()
                && let typing_env = ty::TypingEnv::non_body_analysis(cx.tcx, item.owner_id)
                && match [ty, reference].map(|measured| cx.tcx.layout_of(typing_env.as_query_input(measured))) {
                    [Ok(value), Ok(reference)] => value.size <= reference.size && value.align.abi <= reference.align.abi,
                    [Err(_), _] | [_, Err(_)] => false,
                }
            {
                let (field_name, name) = (field_def.ident, cx.tcx.item_name(subfield));
                let removal = if let LifetimeKind::Param(param) = lifetime.kind
                    && lifetimes
                        .entry(item.owner_id.def_id)
                        .or_insert_with(|| {
                            let mut uses = LifetimeUses { single: FxHashMap::default() };
                            uses.visit_item(item);
                            uses.single
                        })
                        .get(&param)
                        == Some(&Some(lifetime.hir_id))
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
