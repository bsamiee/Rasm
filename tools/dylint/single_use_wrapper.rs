use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS, res::MaybeDef as _, sym};
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    ExprKind, HirId, Item, ItemKind, Node, Path,
    def::{CtorOf, DefKind, Res},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::TyCtxt;

use crate::resolve;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Role {
    Formatting,
    Iteration,
}

#[derive(Clone, Copy)]
enum Mention {
    Built(HirId),
    Named,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    SINGLE_USE_WRAPPER,
    Warn,
    "structs built at one site whose only impls format or iterate the value"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct SingleUseWrapper {
    roles: FxHashMap<DefId, Role>,
    implemented: Vec<Option<LocalDefId>>,
    mentions: FxIndexMap<LocalDefId, Vec<Mention>>,
}

impl_lint_pass!(SingleUseWrapper => [SINGLE_USE_WRAPPER]);

impl SingleUseWrapper {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            roles: [(sym::Display, Role::Formatting), (sym::Debug, Role::Formatting), (sym::Iterator, Role::Iteration)]
                .into_iter()
                .filter_map(|(name, role)| Some((tcx.get_diagnostic_item(name)?, role)))
                .chain(resolve(tcx, PathNS::Type, &[("core::iter::FusedIterator", Role::Iteration)]))
                .collect(),
            implemented: Vec::new(),
            mentions: FxIndexMap::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for SingleUseWrapper {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        self.implemented.push(if matches!(item.kind, ItemKind::Impl(_)) {
            cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization().ty_adt_def().and_then(|owner| owner.did().as_local())
        } else {
            None
        });
    }

    fn check_item_post(&mut self, _: &LateContext<'tcx>, _: &'tcx Item<'tcx>) { self.implemented.pop(); }

    fn check_path(&mut self, cx: &LateContext<'tcx>, path: &Path<'tcx>, node: HirId) {
        if let Res::Def(kind @ (DefKind::Struct | DefKind::Ctor(CtorOf::Struct, _)), named) = path.res
            && let Some(structure) = named.ctor_parent(cx).unwrap_or(named).as_local()
            && !self.implemented.contains(&Some(structure))
        {
            let built = matches!(cx.tcx.hir_node(node), Node::Expr(built) if kind != DefKind::Struct || matches!(built.kind, ExprKind::Struct(..)));
            self.mentions.entry(structure).or_default().push(if built { Mention::Built(node) } else { Mention::Named });
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let owners = cx.tcx.all_local_trait_impls(()).iter().flat_map(|(implemented, implementations)| implementations.iter().map(move |implementation| (*implemented, *implementation))).fold(
            FxIndexMap::<LocalDefId, Vec<(DefId, LocalDefId)>>::default(),
            |mut owners, (implemented, implementation)| {
                if let Some(owner) = cx.tcx.type_of(implementation).instantiate_identity().skip_normalization().ty_adt_def().and_then(|owner| owner.did().as_local())
                    && self.mentions.contains_key(&owner)
                {
                    owners.entry(owner).or_default().push((implemented, implementation));
                }
                owners
            },
        );
        for (structure, mentions) in &self.mentions {
            let node = cx.tcx.local_def_id_to_hir_id(*structure);
            if let [Mention::Built(site)] = mentions
                && !cx.effective_visibilities.is_exported(*structure)
                && !cx.tcx.def_span(*structure).from_expansion()
                && cx.tcx.inherent_impls(*structure).is_empty()
                && let Some(implementations) = owners.get(structure)
                && implementations.iter().any(|(_, implementation)| !cx.tcx.is_automatically_derived(implementation.to_def_id()))
                && let Some(role) = implementations.first().and_then(|(implemented, _)| self.roles.get(implemented).copied())
                && implementations.iter().all(|(implemented, _)| self.roles.get(implemented) == Some(&role))
            {
                let name = cx.tcx.item_name(*structure);
                let (message, help) = match role {
                    Role::Formatting => (format!("`{name}` exists to format a value at its one construction"), "format at that site with `fmt::from_fn(move |formatter| ..)`"),
                    Role::Iteration =>
                        (format!("`{name}` holds iterator state for its one construction"), "write the iteration at that site as a `gen move { .. }` block under `#![feature(gen_blocks)]`"),
                };
                span_lint_hir_and_then(cx, SINGLE_USE_WRAPPER, node, cx.tcx.def_ident_span(*structure).unwrap_or_else(|| cx.tcx.def_span(*structure)), message, |diag| {
                    diag.span_note(cx.tcx.hir_span(*site), "the one construction");
                    diag.help(help);
                });
            }
        }
    }
}
