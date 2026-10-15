use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    res::MaybeResPath as _,
    sym,
    ty::{option_arg_ty, same_type_modulo_regions},
    visitors::for_each_expr,
};
use rustc_hir::{Expr, FieldDef, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::{Ty, Visibility};

use crate::{constructed, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    CONSTRUCTION_INVARIANT,
    Warn,
    "construction and field writes that skip the checked constructor of a type"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] ConstructionInvariant => [CONSTRUCTION_INVARIANT]);

impl<'tcx> LateLintPass<'tcx> for ConstructionInvariant {
    fn check_field_def(&mut self, cx: &LateContext<'tcx>, field: &'tcx FieldDef<'tcx>) {
        let owner = cx.tcx.parent(field.def_id.to_def_id());
        if cx.tcx.def_kind(owner) == DefKind::Struct
            && !field.span.from_expansion()
            && cx.tcx.visibility(field.def_id) != Visibility::Restricted(cx.tcx.parent_module_from_def_id(field.def_id).to_mod_id())
            && let value = cx.tcx.type_of(field.def_id).instantiate_identity().skip_normalization()
            && let Some(check) = check(cx, owner, field.def_id.to_def_id())
        {
            let name = cx.tcx.item_name(owner);
            let checker = format!("{name}::{}", cx.tcx.item_name(check));
            span_lint_hir_and_then(
                cx,
                CONSTRUCTION_INVARIANT,
                field.hir_id,
                field.span,
                format!("field `{}` lets code outside the module write `{name}` past the `{value}` check of `{checker}`", field.ident),
                |diag| {
                    diag.span_note(cx.tcx.def_span(check), format!("`{checker}` checks the `{value}` here"));
                    diag.help("make the field private and give code outside the module an accessor that reads it");
                },
            );
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn bypass<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
            let typeck = cx.typeck_results();
            if let Some((_, fields)) = constructed(typeck, expr)
                && let built = typeck.expr_ty(expr)
                && let Some(adt) = built.ty_adt_def()
                && adt.is_struct()
                && adt.did().is_local()
                && let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id)
                && matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
                && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none()
                && cx.tcx.visibility(owner) != Visibility::Restricted(cx.tcx.parent_module_from_def_id(owner).to_mod_id())
                && same_type_modulo_regions(cx.tcx.fn_sig(owner).instantiate_identity().skip_normalization().skip_binder().output(), built)
                && let parameters = cx.tcx.hir_body_owned_by(owner).params
                && let Some((value, check)) = fields.iter().find_map(|(field, argument)| {
                    parameters
                        .iter()
                        .any(|parameter| argument.res_local_id() == Some(parameter.pat.hir_id))
                        .then(|| check(cx, adt.did(), field.did))
                        .flatten()
                        .map(|check| (typeck.expr_ty(argument), check))
                })
            {
                let name = cx.tcx.item_name(adt.did());
                let checker = format!("{name}::{}", cx.tcx.item_name(check));
                span_lint_hir_and_then(cx, CONSTRUCTION_INVARIANT, expr.hir_id, expr.span, format!("`{name}` built from a `{value}` parameter that skips `{checker}`"), |diag| {
                    diag.span_note(cx.tcx.def_span(check), format!("`{checker}` checks the `{value}` here"));
                    diag.help(format!("build `{name}` through `{checker}`"));
                });
            }
        }

        if !expr.span.from_expansion() {
            bypass(cx, expr);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn check(cx: &LateContext<'_>, owner: DefId, field: DefId) -> Option<DefId> {
    fn refusing<'tcx>(cx: &LateContext<'tcx>, output: Ty<'tcx>, built: Ty<'tcx>) -> bool {
        result_parts(cx, output).is_some_and(|(ok, _)| same_type_modulo_regions(ok, built)) || option_arg_ty(cx, output).is_some_and(|held| same_type_modulo_regions(held, built))
    }

    let (built, value) = (cx.tcx.type_of(owner).instantiate_identity().skip_normalization(), cx.tcx.type_of(field).instantiate_identity().skip_normalization());
    let assoc_fns = |implementation: DefId| cx.tcx.associated_item_def_ids(implementation).iter().copied().filter(move |item| cx.tcx.def_kind(*item) == DefKind::AssocFn);
    let flows = |function: DefId| {
        function.as_local().and_then(|local| Some((cx.tcx.hir_maybe_body_owned_by(local)?, cx.tcx.typeck(local)))).is_some_and(|(body, typeck)| {
            for_each_expr(cx.tcx, body.value, |expr| {
                let unchanged = constructed(typeck, expr).is_some_and(|(_, fields)| {
                    typeck.expr_ty(expr).ty_adt_def().is_some_and(|adt| adt.did() == owner)
                        && fields.iter().any(|(declared, argument)| declared.did == field && body.params.iter().any(|parameter| argument.res_local_id() == Some(parameter.pat.hir_id)))
                });
                if unchanged { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }
            })
            .is_some()
        })
    };
    let converting = cx.tcx.get_diagnostic_item(sym::TryFrom).into_iter().flat_map(|try_from| {
        cx.tcx
            .non_blanket_impls_for_ty(try_from, built)
            .filter(|implementation| same_type_modulo_regions(cx.tcx.impl_trait_ref(*implementation).instantiate_identity().skip_normalization().args.type_at(1), value))
            .flat_map(assoc_fns)
    });
    let refusing = cx.tcx.inherent_impls(owner).iter().copied().flat_map(assoc_fns).filter(|function| {
        let signature = cx.tcx.fn_sig(*function).instantiate_identity().skip_normalization().skip_binder();
        refusing(cx, signature.output(), built) && signature.inputs().iter().any(|input| same_type_modulo_regions(*input, value))
    });
    converting.chain(refusing).find(|&function| flows(function))
}
