use core::iter;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::res::{MaybeDef as _, MaybeResPath as _};
use clippy_utils::{peel_blocks_with_stmt, peel_hir_expr_refs, sym};
use rustc_attr_ir::find_attr;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Constness, Expr, ExprKind, Item, ItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_middle::ty::print::with_forced_trimmed_paths;

use crate::{accessed_field, associated_trait, expanded_externally, exported, generated};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_REFERENCE_IMPL: "manual `AsRef` or `AsMut` impls on crate-private newtypes that `derive_more` supplies");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let tcx = cx.tcx;
        if let ItemKind::Impl(implementation) = item.kind
            && implementation.constness == Constness::NotConst
            && let Some(implemented) = tcx.impl_opt_trait_id(item.owner_id)
            && matches!(implemented.opt_diag_name(cx), Some(sym::AsRef | sym::AsMut))
            && !tcx.defaultness(item.owner_id).is_default()
            && tcx.generics_of(item.owner_id).count() == 0
            && tcx.clauses_of(item.owner_id).clauses.is_empty()
            && let Some(wrapper) = tcx.type_of(item.owner_id).instantiate_identity().skip_normalization().ty_adt_def()
            && wrapper.is_struct()
            && let Some(local) = wrapper.did().as_local()
            && tcx.generics_of(local).count() == 0
            && let [inner] = &wrapper.non_enum_variant().fields.raw
            && let ancestors = tcx.hir_parent_id_iter(tcx.local_def_id_to_hir_id(local)).collect::<FxHashSet<_>>()
            && !iter::once(item.hir_id())
                .chain(tcx.hir_parent_id_iter(item.hir_id()).take_while(|parent| !ancestors.contains(parent)))
                .any(|owner| find_attr!(tcx, owner, CfgTrace(..) | CfgAttrTrace(..)))
            && !expanded_externally(cx, tcx.def_span(local))
            && !exported(cx, local)
            && !generated(cx, item.hir_id())
            && let [method] = tcx.associated_item_def_ids(item.owner_id)
            && let Some(function) = method.as_local()
            && !tcx.associated_item(*method).defaultness(tcx).is_default()
            && !find_attr!(tcx, *method, CfgTrace(..) | CfgAttrTrace(..) | TrackCaller(..))
            && let attributes = tcx.codegen_fn_attrs(*method)
            && !attributes.contains_extern_indicator()
            && attributes.foreign_item_symbol_aliases.is_empty()
            && let (body, typeck) = (tcx.hir_body_owned_by(function), tcx.typeck(function))
            && let value = peel_blocks_with_stmt(body.value)
            && typeck.expr_ty(value) == typeck.expr_ty_adjusted(value)
            && let held = tcx.type_of(inner.did).instantiate_identity().skip_normalization()
            && let target = tcx.impl_trait_ref(item.owner_id).instantiate_identity().skip_normalization().args.type_at(1)
            && let reads_field = (|read: &Expr<'_>| {
                let receiver = body.params[0].pat.hir_id;
                let read = peel_hir_expr_refs(read).0;
                matches!(read.kind, ExprKind::Field(base, _) if base.res_local_id() == Some(receiver)) && accessed_field(typeck, read) == Some(inner.did)
            })
            && match value.kind {
                ExprKind::MethodCall(_, receiver, [], _) if held != target && reads_field(receiver) =>
                    typeck.type_dependent_def_id(value.hir_id).filter(|&called| associated_trait(tcx, called) == Some(implemented)).is_some_and(|called| {
                        let selected = tcx.fn_sig(called).instantiate(tcx, typeck.node_args(value.hir_id)).skip_normalization().skip_binder();
                        if let ([receiver], ty::Ref(_, output, _)) = (selected.inputs(), selected.output().kind())
                            && let ty::Ref(_, owner, _) = receiver.kind()
                        {
                            tcx.erase_and_anonymize_regions(*owner) == tcx.erase_and_anonymize_regions(held) && tcx.erase_and_anonymize_regions(*output) == tcx.erase_and_anonymize_regions(target)
                        } else {
                            false
                        }
                    }),
                _ => held == target && reads_field(value),
            }
        {
            let (name, viewing, viewed) = (tcx.item_name(local), tcx.item_name(implemented), tcx.item_name(*method));
            span_lint_hir_and_then(
                cx,
                MANUAL_REFERENCE_IMPL,
                item.hir_id(),
                item.span,
                with_forced_trimmed_paths!(format!("`impl {viewing}<{target}> for {name}` returns the wrapped `{held}` by hand")),
                |diag| {
                    diag.help(with_forced_trimmed_paths!(format!("derive `derive_more::{viewing}` on `{name}` with `#[{viewed}({target})]` and delete the impl")));
                },
            );
        }
    }
}
