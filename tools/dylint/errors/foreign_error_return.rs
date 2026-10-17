use clippy_utils::{diagnostics::span_lint_hir_and_then, is_in_test, res::MaybeQPath as _};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    ExprKind, FnRetTy, ImplItem, Item, TraitItem,
    def::DefKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, GenericArg, TyCtxt};

use crate::{any_expr, awaited_output, exported, generated, is_error, pointee, result_parts, signature};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    mandated: FxHashMap<DefId, FxHashSet<DefId>>,
}

lint! {
    default Pass;
    FOREIGN_ERROR_RETURN:
        "exported functions returning another crate's error type, except wrappers of a same-named foreign function and functions sharing a parameter type with a foreign trait method that returns it",
}

impl Pass {
    fn inspect(&self, cx: &LateContext<'_>, owner: LocalDefId) {
        fn adapts(tcx: TyCtxt<'_>, owner: LocalDefId, error: DefId) -> bool {
            let Some(body) = tcx.hir_maybe_body_owned_by(owner) else { return false };
            let (typeck, name) = (tcx.typeck(owner), tcx.item_name(owner));
            any_expr(tcx, &body.value, |expr| {
                typeck.type_dependent_def_id(expr.hir_id).or_else(|| if let ExprKind::Call(callee, _) = expr.kind { callee.res(typeck).opt_def_id() } else { None }).is_some_and(|callee| {
                    !callee.is_local()
                        && tcx.opt_item_name(callee) == Some(name)
                        && awaited_output(tcx, callee).walk().filter_map(GenericArg::as_type).any(|held| held.ty_adt_def().map(AdtDef::did) == Some(error))
                })
            })
        }

        if let Some(declaration) = cx.tcx.hir_node_by_def_id(owner).fn_decl()
            && let FnRetTy::Return(output) = declaration.output
            && let node = cx.tcx.local_def_id_to_hir_id(owner)
            && !generated(cx, node)
            && !is_in_test(cx.tcx, node)
            && exported(cx, owner)
            && let Some((_, error)) = result_parts(cx, awaited_output(cx.tcx, owner.to_def_id()))
            && let ty::Adt(adt, _) = pointee(cx, error).kind()
            && !adt.did().is_local()
            && !self.mandated.get(&adt.did()).is_some_and(|mandating| parameters(cx.tcx, owner.to_def_id()).any(|parameter| mandating.contains(&parameter)))
            && is_error(cx, error)
            && !adapts(cx.tcx, owner, adt.did())
        {
            let foreign = cx.tcx.def_path_str(adt.did());
            span_lint_hir_and_then(cx, FOREIGN_ERROR_RETURN, node, output.span.source_callsite(), format!("exported `{}` returns `{foreign}` from another crate", cx.tcx.item_name(owner)), |diag| {
                diag.help(format!("return a local error enum with a variant holding `{foreign}` as `#[source]`"));
            });
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        self.mandated = cx
            .tcx
            .all_local_trait_impls(())
            .iter()
            .filter(|(implemented, impls)| !implemented.is_local() && impls.iter().any(|&implementation| !generated(cx, cx.tcx.local_def_id_to_hir_id(implementation))))
            .flat_map(|(&implemented, _)| cx.tcx.associated_item_def_ids(implemented))
            .filter(|&&item| cx.tcx.def_kind(item) == DefKind::AssocFn)
            .filter_map(|&item| Some((pointee(cx, result_parts(cx, awaited_output(cx.tcx, item))?.1).ty_adt_def()?.did(), item)))
            .fold(FxHashMap::default(), |mut mandated, (error, item)| {
                mandated.entry(error).or_default().extend(parameters(cx.tcx, item));
                mandated
            });
    }

    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) { self.inspect(cx, item.owner_id.def_id); }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) { self.inspect(cx, item.owner_id.def_id); }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if cx.tcx.trait_impl_of_assoc(item.owner_id.to_def_id()).is_none() {
            self.inspect(cx, item.owner_id.def_id);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parameters(tcx: TyCtxt<'_>, function: DefId) -> impl Iterator<Item = DefId> { signature(tcx, function).inputs().iter().filter_map(|input| input.peel_refs().ty_adt_def().map(AdtDef::did)) }
