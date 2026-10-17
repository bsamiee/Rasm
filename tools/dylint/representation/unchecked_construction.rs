use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    paths::PathNS,
    sym,
    ty::{option_arg_ty, same_type_modulo_regions},
};
use rustc_attr_ir::Attribute;
use rustc_hir::{Expr, FieldDef, Item, ItemKind, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, Visibility};

use crate::{any_expr, constructed, derived_impl, expanded_externally, parameter, path, result_parts, signature};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    deserialize: Option<DefId>,
}

lint!(Pass; UNCHECKED_CONSTRUCTION: "struct literals, public fields, and derived `Deserialize` impls that skip a type's checked constructor");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { deserialize: path(tcx, PathNS::Type, "serde::Deserialize") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let owner = item.owner_id.to_def_id();
        let converted = |attribute: &Attribute| {
            attribute.has_name(sym::serde) && attribute.meta_item_list().is_some_and(|options| options.iter().any(|option| option.has_name(sym::try_from) || option.has_name(sym::from)))
        };
        if let ItemKind::Struct(ident, ..) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(deserialize) = self.deserialize
            && cx.tcx.non_blanket_impls_for_ty(deserialize, cx.tcx.type_of(owner).instantiate_identity().skip_normalization()).any(|implementation| derived_impl(cx.tcx, implementation))
            && !cx.tcx.hir_attrs(item.hir_id()).iter().any(converted)
            && let Some((field, check)) = cx.tcx.adt_def(owner).non_enum_variant().fields.iter().find_map(|field| Some((field, check(cx, owner, field.did)?)))
        {
            let value = cx.tcx.type_of(field.did).instantiate_identity().skip_normalization();
            let checker = format!("{ident}::{}", cx.tcx.item_name(check));
            span_lint_hir_and_then(cx, UNCHECKED_CONSTRUCTION, item.hir_id(), ident.span, format!("derived `Deserialize` builds `{ident}` without the `{value}` check of `{checker}`"), |diag| {
                diag.span_note(cx.tcx.def_span(check), format!("`{checker}` declared here"));
                diag.help(format!("decode through `#[serde(try_from = \"{value}\")]` and a `TryFrom<{value}>` impl that runs `{checker}`"));
            });
        }
    }

    fn check_field_def(&mut self, cx: &LateContext<'tcx>, field: &'tcx FieldDef<'tcx>) {
        let owner = cx.tcx.parent(field.def_id.to_def_id());
        if cx.tcx.def_kind(owner) == DefKind::Struct
            && !expanded_externally(cx, field.span)
            && cx.tcx.visibility(field.def_id) != Visibility::Restricted(cx.tcx.parent_module_from_def_id(field.def_id).to_mod_id())
            && let value = cx.tcx.type_of(field.def_id).instantiate_identity().skip_normalization()
            && let Some(check) = check(cx, owner, field.def_id.to_def_id())
        {
            let name = cx.tcx.item_name(owner);
            let checker = format!("{name}::{}", cx.tcx.item_name(check));
            span_lint_hir_and_then(
                cx,
                UNCHECKED_CONSTRUCTION,
                field.hir_id,
                field.span,
                format!("field `{}` lets code outside the module set `{name}` without the `{value}` check of `{checker}`", field.ident),
                |diag| {
                    diag.span_note(cx.tcx.def_span(check), format!("`{checker}` declared here"));
                    diag.help(format!("make `{}` private and add an accessor for code outside the module", field.ident));
                },
            );
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if !expanded_externally(cx, expr.span)
            && let Some((_, fields)) = constructed(typeck, expr)
            && let built = typeck.expr_ty(expr)
            && let Some(adt) = built.ty_adt_def()
            && adt.is_struct()
            && adt.did().is_local()
            && let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id)
            && matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
            && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none()
            && cx.tcx.visibility(owner) != Visibility::Restricted(cx.tcx.parent_module_from_def_id(owner).to_mod_id())
            && same_type_modulo_regions(signature(cx.tcx, owner.to_def_id()).output(), built)
            && let body = cx.tcx.hir_body_owned_by(owner)
            && let Some((value, check)) =
                fields.iter().find_map(|(field, argument)| parameter(body, argument).and_then(|_| check(cx, adt.did(), field.did)).map(|check| (typeck.expr_ty(argument), check)))
        {
            let name = cx.tcx.item_name(adt.did());
            let checker = format!("{name}::{}", cx.tcx.item_name(check));
            span_lint_hir_and_then(cx, UNCHECKED_CONSTRUCTION, expr.hir_id, expr.span, format!("`{name}` built from a `{value}` parameter skips `{checker}`"), |diag| {
                diag.span_note(cx.tcx.def_span(check), format!("`{checker}` declared here"));
                diag.help(format!("build `{name}` through `{checker}`"));
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn check(cx: &LateContext<'_>, owner: DefId, field: DefId) -> Option<DefId> {
    let (built, value) = (cx.tcx.type_of(owner).instantiate_identity().skip_normalization(), cx.tcx.type_of(field).instantiate_identity().skip_normalization());
    let assoc_fns = |implementation: DefId| cx.tcx.associated_item_def_ids(implementation).iter().copied().filter(move |item| cx.tcx.def_kind(*item) == DefKind::AssocFn);
    let flows = |function: DefId| {
        function.as_local().and_then(|local| Some((cx.tcx.hir_maybe_body_owned_by(local)?, cx.tcx.typeck(local)))).is_some_and(|(body, typeck)| {
            any_expr(cx.tcx, &body.value, |expr| {
                constructed(typeck, expr).is_some_and(|(_, fields)| {
                    typeck.expr_ty(expr).ty_adt_def().is_some_and(|adt| adt.did() == owner) && fields.iter().any(|(declared, argument)| declared.did == field && parameter(body, argument).is_some())
                })
            })
        })
    };
    let converting = cx.tcx.get_diagnostic_item(sym::TryFrom).into_iter().flat_map(|try_from| {
        cx.tcx
            .non_blanket_impls_for_ty(try_from, built)
            .filter(|implementation| same_type_modulo_regions(cx.tcx.impl_trait_ref(*implementation).instantiate_identity().skip_normalization().args.type_at(1), value))
            .flat_map(assoc_fns)
    });
    let refusing = cx.tcx.inherent_impls(owner).iter().copied().flat_map(assoc_fns).filter(|function| {
        let declared = signature(cx.tcx, *function);
        result_parts(cx, declared.output()).map(|(ok, _)| ok).or_else(|| option_arg_ty(cx, declared.output())).is_some_and(|held| same_type_modulo_regions(held, built))
            && declared.inputs().iter().any(|input| same_type_modulo_regions(*input, value))
    });
    converting.chain(refusing).find(|&function| flows(function))
}
