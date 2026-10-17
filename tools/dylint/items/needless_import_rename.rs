use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    paths::{PathNS, lookup_path},
};
use rustc_attr_ir::find_attr;
use rustc_hir::{
    HirId, Item, ItemKind, Node, PrimTy, StmtKind, UseKind, UseTree,
    def::{DefKind, Namespace, PerNS, Res},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{middle::resolve::ModChild, ty::TyCtxt};
use rustc_span::{Ident, Span, Symbol, def_id::LOCAL_CRATE, kw};

use crate::{expanded_externally, exported};

// --- [MODELS] --------------------------------------------------------------------------

struct Rename {
    node: HirId,
    span: Span,
    original: Ident,
    alias: Ident,
    imported: PerNS<Option<Res>>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    prelude: Vec<(Symbol, Res)>,
}

lint!(Pass; NEEDLESS_IMPORT_RENAME: "`use .. as` renames of imports with a name that collides with no other item in scope");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let preludes = tcx
            .hir_root_module()
            .item_ids
            .iter()
            .map(|&id| tcx.hir_item(id))
            .filter(|item| find_attr!(tcx, item.hir_id(), PreludeImport))
            .filter_map(|item| {
                if let ItemKind::Use(tree) = item.kind { Some(tree.prefix.segments.iter().map(|segment| segment.ident.name).filter(|&name| name != kw::PathRoot).collect::<Vec<_>>()) } else { None }
            })
            .flat_map(|path| lookup_path(tcx, PathNS::Type, &path));
        let prelude = preludes
            .flat_map(|prelude| {
                let exported = tcx.module_children(prelude.krate.as_def_id()).iter().filter(|child| child.res.ns() == Some(Namespace::MacroNS));
                tcx.module_children(prelude).iter().chain(exported).map(binding)
            })
            .chain(
                tcx.crates(())
                    .iter()
                    .filter(|&&krate| tcx.extern_crate(krate).is_some_and(|source| source.dependency_of == LOCAL_CRATE))
                    .map(|&krate| (tcx.crate_name(krate), Res::Def(DefKind::Mod, krate.as_def_id()))),
            )
            .chain(PrimTy::ALL.into_iter().map(|primitive| (primitive.name(), Res::PrimTy(primitive))))
            .collect();
        Self { prelude }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        fn item_bindings(cx: &LateContext<'_>, item: &Item<'_>) -> Vec<(Symbol, Res)> {
            fn tree_bindings(cx: &LateContext<'_>, tree: &UseTree<'_>) -> Vec<(Symbol, Res)> {
                match tree.kind {
                    UseKind::Single(ident) => tree.prefix.res.present_items().map(|res| (ident.name, res)).collect(),
                    UseKind::Glob => tree.prefix.res.present_items().filter_map(|res| res.opt_def_id()).flat_map(|target| cx.tcx.module_children(target)).map(binding).collect(),
                    UseKind::Nested { items } => items.iter().flat_map(|(nested, ..)| tree_bindings(cx, nested)).collect(),
                }
            }

            if let ItemKind::Use(tree) = item.kind {
                tree_bindings(cx, &tree)
            } else {
                item.kind.ident().map(|ident| (ident.name, Res::Def(cx.tcx.def_kind(item.owner_id), item.owner_id.to_def_id()))).into_iter().collect()
            }
        }

        fn renames(tree: &UseTree<'_>, node: HirId, outer: Option<Ident>) -> Vec<Rename> {
            let named = tree.prefix.segments.iter().rev().map(|segment| segment.ident).find(|ident| ident.name != kw::SelfLower).or(outer);
            match tree.kind {
                UseKind::Single(alias) => named
                    .filter(|original| ![original.name, kw::SelfLower, kw::Underscore].contains(&alias.name))
                    .map(|original| Rename { node, span: tree.prefix.span.to(alias.span), original, alias, imported: tree.prefix.res })
                    .into_iter()
                    .collect(),
                UseKind::Glob => Vec::new(),
                UseKind::Nested { items } => items.iter().flat_map(|(nested, nested_node, _)| renames(nested, *nested_node, named)).collect(),
            }
        }

        if let ItemKind::Use(tree) = item.kind
            && !expanded_externally(cx, item.span)
            && let renames = renames(&tree, item.hir_id(), None)
            && !renames.is_empty()
            && let module = cx.tcx.parent_module_from_def_id(item.owner_id.def_id)
            && !exported(cx, item.owner_id.def_id)
            && !find_attr!(cx.tcx, item.hir_id(), CfgTrace(..))
        {
            let boundary = cx.tcx.local_def_id_to_hir_id(module.to_local_def_id());
            let scope = cx
                .tcx
                .hir_parent_iter(item.hir_id())
                .take_while(|&(parent, _)| parent != boundary)
                .filter_map(|(_, node)| if let Node::Block(block) = node { Some(block) } else { None })
                .flat_map(|block| block.stmts.iter().filter_map(|stmt| if let StmtKind::Item(id) = stmt.kind { Some(cx.tcx.hir_item(id)) } else { None }))
                .flat_map(|scoped| item_bindings(cx, scoped))
                .chain(cx.tcx.module_children_local(module.to_local_def_id()).iter().map(binding))
                .chain(self.prelude.iter().copied())
                .collect::<Vec<_>>();
            let collides =
                |rename: &Rename| scope.iter().any(|&(name, res)| name == rename.original.name && res.ns().is_none_or(|namespace| rename.imported[namespace].is_some_and(|imported| imported != res)));
            for rename in renames.iter().filter(|rename| !collides(rename)) {
                span_lint_hir_and_then(
                    cx,
                    NEEDLESS_IMPORT_RENAME,
                    rename.node,
                    rename.span,
                    format!("`{}` renames `{}`, which collides with no other item in scope", rename.alias, rename.original),
                    |diag| {
                        diag.help(format!("import `{}` under its own name", rename.original));
                    },
                );
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn binding(child: &ModChild) -> (Symbol, Res) { (child.ident.name, child.res.map_id(|never| never)) }
