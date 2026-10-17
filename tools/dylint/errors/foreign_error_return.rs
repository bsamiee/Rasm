use clippy_utils::{diagnostics::span_lint_hir_and_then, is_in_test, res::MaybeQPath as _};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    ExprKind, FnRetTy, ImplItem, Item, TraitItem,
    def::DefKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, AdtDef, GenericArg, TyCtxt};

use crate::{any_expr, delivered, generated, is_error, pointee, result_parts, signature};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    FOREIGN_ERROR_RETURN,
    Warn,
    "exported functions returning another crate's error type, outside adapters of a same-named foreign function and takers of a parameter type of a foreign trait method naming it"
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct ForeignErrorReturn {
    mandated: FxHashMap<DefId, FxHashSet<DefId>>,
}

impl_lint_pass!(ForeignErrorReturn => [FOREIGN_ERROR_RETURN]);

impl ForeignErrorReturn {
    fn inspect(&self, cx: &LateContext<'_>, owner: LocalDefId) {
        fn adapts(tcx: TyCtxt<'_>, owner: LocalDefId, error: DefId) -> bool {
            let Some(body) = tcx.hir_maybe_body_owned_by(owner) else { return false };
            let (typeck, name) = (tcx.typeck(owner), tcx.item_name(owner));
            any_expr(tcx, body.value, |expr| {
                typeck.type_dependent_def_id(expr.hir_id).or_else(|| if let ExprKind::Call(callee, _) = expr.kind { callee.res(typeck).opt_def_id() } else { None }).is_some_and(|callee| {
                    !callee.is_local()
                        && tcx.opt_item_name(callee) == Some(name)
                        && delivered(tcx, callee).walk().filter_map(GenericArg::as_type).any(|held| held.ty_adt_def().map(AdtDef::did) == Some(error))
                })
            })
        }

        if let Some(declaration) = cx.tcx.hir_node_by_def_id(owner).fn_decl()
            && let FnRetTy::Return(output) = declaration.output
            && !generated(cx, cx.tcx.local_def_id_to_hir_id(owner))
            && cx.effective_visibilities.is_exported(owner)
            && let Some((_, error)) = result_parts(cx, delivered(cx.tcx, owner.to_def_id()))
            && let ty::Adt(adt, _) = pointee(cx, error).kind()
            && !adt.did().is_local()
            && !self.mandated.get(&adt.did()).is_some_and(|mandating| parameters(cx.tcx, owner.to_def_id()).any(|parameter| mandating.contains(&parameter)))
            && !adapts(cx.tcx, owner, adt.did())
            && let node = cx.tcx.local_def_id_to_hir_id(owner)
            && !is_in_test(cx.tcx, node)
            && is_error(cx, error)
        {
            let foreign = cx.tcx.def_path_str(adt.did());
            span_lint_hir_and_then(
                cx,
                FOREIGN_ERROR_RETURN,
                node,
                output.span.source_callsite(),
                format!("exported `{}` returns `{foreign}`, an error type another crate defines", cx.tcx.item_name(owner)),
                |diag| {
                    diag.help(format!("return an error enum this crate defines, with a variant holding the `{foreign}` as its `#[source]`"));
                },
            );
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for ForeignErrorReturn {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        self.mandated = cx
            .tcx
            .all_local_trait_impls(())
            .iter()
            .filter(|(implemented, impls)| !implemented.is_local() && impls.iter().any(|&implementation| !generated(cx, cx.tcx.local_def_id_to_hir_id(implementation))))
            .flat_map(|(&implemented, _)| cx.tcx.associated_item_def_ids(implemented))
            .filter(|&&item| cx.tcx.def_kind(item) == DefKind::AssocFn)
            .filter_map(|&item| Some((pointee(cx, result_parts(cx, delivered(cx.tcx, item))?.1).ty_adt_def()?.did(), item)))
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
