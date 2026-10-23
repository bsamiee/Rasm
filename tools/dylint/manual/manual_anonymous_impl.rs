use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS};
use itertools::Itertools as _;
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    ExprKind, HirId, Item, ItemKind, Node, Path,
    def::{CtorOf, DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{hir::nested_filter, ty::TyCtxt};

use crate::{derived_impl, expanded_externally, exported, resolve, trait_impls};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Anonymous {
    Formatter,
    Generator,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    replaced: FxHashMap<DefId, Anonymous>,
}

lint! {
    Pass;
    MANUAL_FMT_FROM_FN: "structs built at one site with only `Display` or `Debug` impls",
    MANUAL_GEN_BLOCK: "structs built at one site with only `Iterator` or `FusedIterator` impls",
}

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            replaced: resolve(
                tcx,
                PathNS::Type,
                &[
                    ("core::fmt::Display", Anonymous::Formatter),
                    ("core::fmt::Debug", Anonymous::Formatter),
                    ("core::iter::Iterator", Anonymous::Generator),
                    ("core::iter::FusedIterator", Anonymous::Generator),
                ],
            ),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        struct Mentions<'tcx> {
            tcx: TyCtxt<'tcx>,
            implemented: Option<LocalDefId>,
            found: FxIndexMap<LocalDefId, Vec<Option<HirId>>>,
        }
        impl<'tcx> Visitor<'tcx> for Mentions<'tcx> {
            type NestedFilter = nested_filter::All;

            fn maybe_tcx(&mut self) -> TyCtxt<'tcx> { self.tcx }

            fn visit_item(&mut self, i: &'tcx Item<'tcx>) {
                let owner = matches!(i.kind, ItemKind::Impl(_)).then(|| self.tcx.type_of(i.owner_id).instantiate_identity().skip_normalization().ty_adt_def()?.did().as_local()).flatten();
                let outer = self.implemented;
                self.implemented = owner.or(outer);
                intravisit::walk_item(self, i);
                self.implemented = outer;
            }

            fn visit_path(&mut self, path: &Path<'tcx>, node: HirId) {
                if let Res::Def(kind @ (DefKind::Struct | DefKind::Ctor(CtorOf::Struct, _)), named) = path.res
                    && let Some(structure) = (if let DefKind::Ctor(..) = kind { self.tcx.parent(named) } else { named }).as_local()
                    && self.implemented != Some(structure)
                {
                    let built = matches!(self.tcx.hir_node(node), Node::Expr(built) if kind != DefKind::Struct || matches!(built.kind, ExprKind::Struct(..)));
                    self.found.entry(structure).or_default().push(built.then_some(node));
                }
                intravisit::walk_path(self, path);
            }
        }
        let tcx = cx.tcx;
        let mut mentions = Mentions { tcx, implemented: None, found: FxIndexMap::default() };
        tcx.hir_walk_toplevel_module(&mut mentions);
        let mut implementations = trait_impls(tcx);
        for (structure, sites) in mentions.found {
            if let [Some(site)] = *sites.as_slice()
                && let Some(owned) = implementations.swap_remove(&structure)
                && !exported(cx, structure)
                && !expanded_externally(cx, tcx.def_span(structure))
                && tcx.inherent_impls(structure).is_empty()
                && owned.iter().any(|&(_, implementation)| !derived_impl(tcx, implementation.to_def_id()))
                && let Ok(Some(&anonymous)) = owned.iter().map(|(implemented, _)| self.replaced.get(implemented)).all_equal_value()
            {
                let (name, traits) = (tcx.item_name(structure), owned.iter().map(|&(implemented, _)| format!("`{}`", tcx.item_name(implemented))).unique().join(", "));
                let (lint, help) = match anonymous {
                    Anonymous::Formatter => (MANUAL_FMT_FROM_FN, format!("replace `{name}` with `fmt::from_fn(move |formatter| ..)` at its construction site")),
                    Anonymous::Generator => (MANUAL_GEN_BLOCK, format!("replace `{name}` with a `gen move {{ .. }}` block at its construction site under `#![feature(gen_blocks)]`")),
                };
                span_lint_hir_and_then(
                    cx,
                    lint,
                    tcx.local_def_id_to_hir_id(structure),
                    tcx.def_ident_span(structure).unwrap_or_else(|| tcx.def_span(structure)),
                    format!("`{name}` is built at one site and implements only {traits}"),
                    |diag| {
                        diag.span_note(tcx.hir_span(site), "built here");
                        diag.help(help);
                    },
                );
            }
        }
    }
}
