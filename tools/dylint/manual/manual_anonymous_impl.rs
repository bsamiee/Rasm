use std::rc::Rc;

use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, get_parent_expr, sym, ty::get_iterator_item_ty};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    Expr, ExprKind, HirId, Item, ItemKind, Node, Path,
    def::{CtorKind, CtorOf, DefKind, Res},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, TyCtxt};

use super::{collects_vec, iterator_method};
use crate::{derived_impl, duplication::Bodies, expanded_externally, exported, trait_impls};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Anonymous {
    Formatter,
    Generator,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    bodies: Rc<Bodies<'tcx>>,
    implemented: Vec<(LocalDefId, LocalDefId)>,
    mentions: FxIndexMap<LocalDefId, Option<HirId>>,
}

lint! {
    Pass<'_>;
    MANUAL_FMT_FROM_FN: "structs built at one site with only `Display` or `Debug` impls",
    MANUAL_GEN_BLOCK: "structs consumed at one construction site implementing only `Iterator` or `FusedIterator`, with `next` their only method",
}

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(_: TyCtxt<'tcx>, bodies: Rc<Bodies<'tcx>>) -> Self { Self { bodies, implemented: Vec::new(), mentions: FxIndexMap::default() } }
}

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(implementation) = item.kind
            && implementation.of_trait.is_some()
            && let Some(owner) = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization().ty_adt_def().and_then(|owner| owner.did().as_local())
        {
            self.implemented.push((item.owner_id.def_id, owner));
        }
    }

    fn check_item_post(&mut self, _: &LateContext<'tcx>, item: &'tcx Item<'tcx>) { self.implemented.pop_if(|(implementation, _)| *implementation == item.owner_id.def_id); }

    fn check_path(&mut self, cx: &LateContext<'tcx>, path: &Path<'tcx>, node: HirId) {
        let structure = match path.res {
            Res::Def(DefKind::Struct, named) => Some(named),
            Res::Def(DefKind::Ctor(CtorOf::Struct, _), named) => Some(cx.tcx.parent(named)),
            Res::SelfTyAlias { alias_to, .. } | Res::SelfCtor(alias_to) => cx.tcx.type_of(alias_to).instantiate_identity().skip_normalization().ty_adt_def().map(AdtDef::did),
            _ => None,
        };
        if let Some(structure) = structure.and_then(DefId::as_local)
            && self.implemented.last().filter(|&&(_, owner)| owner == structure).is_none_or(|&(implementation, _)| {
                matches!(cx.tcx.get_diagnostic_name(cx.tcx.impl_trait_id(implementation)), Some(sym::Display | sym::Debug))
                    && cx.tcx.hir_expect_item(implementation).expect_impl().items.iter().filter_map(|method| cx.tcx.hir_owner_node(method.owner_id).body_id()).any(|body| {
                        let root = cx.tcx.hir_body(body).value.hir_id;
                        node == root || cx.tcx.hir_parent_id_iter(node).any(|parent| parent == root)
                    })
            })
        {
            self.mentions.entry(structure).and_modify(|site| *site = None).or_insert_with(|| match (path.res, cx.tcx.hir_node(node), cx.tcx.parent_hir_node(node)) {
                (Res::Def(DefKind::Struct, _), Node::Expr(Expr { kind: ExprKind::Struct(..), .. }), _) | (Res::Def(DefKind::Ctor(_, CtorKind::Const), _), Node::Expr(_), _) => Some(node),
                (Res::Def(DefKind::Ctor(_, CtorKind::Fn), _), _, Node::Expr(call @ Expr { kind: ExprKind::Call(callee, _), .. })) if callee.hir_id == node => Some(call.hir_id),
                _ => None,
            });
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Path(_) = expr.kind
            && let Some(&(implementation, structure)) = self.implemented.last()
            && matches!(cx.tcx.get_diagnostic_name(cx.tcx.impl_trait_id(implementation)), Some(sym::Display | sym::Debug))
            && cx.typeck_results().expr_ty(expr).peel_refs().ty_adt_def().is_some_and(|owner| owner.did() == structure.to_def_id())
            && !matches!(get_parent_expr(cx, expr), Some(Expr { kind: ExprKind::Field(base, _), .. }) if base.hir_id == expr.hir_id)
        {
            self.mentions.insert(structure, None);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let consumed = |site| {
            let owner = tcx.hir_enclosing_body_owner(site);
            let context =
                LateContext { enclosing_body: Some(tcx.hir_body_owned_by(owner).id()), typeck_results: Some(tcx.typeck(tcx.typeck_root_def_id_local(owner))), param_env: tcx.param_env(owner), ..*cx };
            let built = tcx.hir_expect_expr(site);
            let built_type = context.typeck_results().expr_ty(built);
            let structure = built_type.ty_adt_def();
            !built_type.has_significant_drop(tcx, context.typing_env())
                && get_iterator_item_ty(&context, built_type).is_some_and(|item| {
                    !item.walk().filter_map(ty::GenericArg::as_type).any(|held| held.ty_adt_def() == structure)
                        && get_parent_expr(&context, built)
                            .filter(|call| matches!(call.kind, ExprKind::MethodCall(_, receiver, ..) | ExprKind::Call(_, [receiver, ..]) if receiver.hir_id == site))
                            .and_then(|call| fn_def_id(&context, call).map(|method| (call, method)))
                            .is_some_and(|(call, method)| {
                                iterator_method(&context, call)
                                    && match tcx.item_name(method) {
                                        sym::count | sym::fold | sym::for_each | sym::last | sym::reduce => true,
                                        sym::collect => collects_vec(&context, item, context.typeck_results().expr_ty(call)),
                                        sym::sum | sym::product => context.typeck_results().expr_ty(call).is_numeric() && item.peel_refs().is_numeric(),
                                        _ => false,
                                    }
                            })
                })
        };
        let mut implementations = trait_impls(tcx);
        for (&structure, &site) in &self.mentions {
            if let Some(site) = site
                && let Some(owned) = implementations.remove(&structure)
                && !exported(cx, structure)
                && !expanded_externally(cx, tcx.def_span(structure))
                && tcx.inherent_impls(structure).is_empty()
                && owned.iter().any(|&(_, implementation)| !derived_impl(tcx, implementation.to_def_id()))
                && let Ok(Some(anonymous)) = owned
                    .iter()
                    .map(|&(implemented, _)| match tcx.get_diagnostic_name(implemented) {
                        Some(sym::Display | sym::Debug) => Some(Anonymous::Formatter),
                        _ if [tcx.lang_items().iterator_trait(), tcx.lang_items().fused_iterator_trait()].contains(&Some(implemented)) => Some(Anonymous::Generator),
                        _ => None,
                    })
                    .all_equal_value()
                && match anonymous {
                    Anonymous::Generator =>
                        consumed(site)
                            && owned.iter().all(|&(_, implementation)| tcx.associated_items(implementation.to_def_id()).in_definition_order().all(|item| !item.is_fn() || item.name() == sym::next)),
                    Anonymous::Formatter => owned
                        .iter()
                        .flat_map(|&(_, implementation)| tcx.associated_items(implementation.to_def_id()).in_definition_order())
                        .filter(|item| item.is_fn())
                        .filter_map(|item| item.def_id.as_local())
                        .tuple_windows()
                        .all(|(left, right)| {
                            self.bodies.callable(tcx, left).zip(self.bodies.callable(tcx, right)).is_some_and(|(left_contract, right_contract)| left_contract == right_contract)
                                && self.bodies.equal(cx, left, right, |_, _, _, _| false, |left_type, right_type| left_type == right_type).is_some()
                        }),
                }
            {
                let (name, traits) = (tcx.item_name(structure), owned.iter().map(|&(implemented, _)| implemented).unique().map(|implemented| tcx.item_name(implemented)).format("`, `"));
                let (lint, help) = match anonymous {
                    Anonymous::Formatter => (MANUAL_FMT_FROM_FN, format!("replace `{name}` with `fmt::from_fn(move |formatter| ..)` at its construction site")),
                    Anonymous::Generator => (MANUAL_GEN_BLOCK, format!("replace `{name}` with a `gen move {{ .. }}` block at its construction site under `#![feature(gen_blocks)]`")),
                };
                span_lint_hir_and_then(
                    cx,
                    lint,
                    tcx.local_def_id_to_hir_id(structure),
                    tcx.def_ident_span(structure).unwrap_or_else(|| tcx.def_span(structure)),
                    format!("`{name}` is built at one site and implements only `{traits}`"),
                    |diag| {
                        diag.span_note(tcx.hir_span(site), "built here");
                        diag.help(help);
                    },
                );
            }
        }
    }
}
