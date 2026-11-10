use core::cell::LazyCell;
use core::iter;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::paths::{PathNS, lookup_path};
use itertools::Itertools as _;
use rustc_attr_ir::find_attr;
use rustc_data_structures::fx::{FxBuildHasher, FxHashMap, FxHashSet};
use rustc_hir::def::{DefKind, Namespace, PerNS, Res};
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_hir::intravisit::{self, Visitor};
use rustc_hir::{FieldDef, HirId, Item, ItemKind, Node, OwnerNode, Pat, PatKind, Path, PrimTy, StmtKind, UseKind, UseTree};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::middle::region::{Scope, ScopeData};
use rustc_middle::middle::resolve::ModChild;
use rustc_middle::ty::{GenericParamDefKind, TyCtxt};
use rustc_span::def_id::LOCAL_CRATE;
use rustc_span::{Ident, Span, Symbol, kw};

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

#[derive(Default)]
pub(crate) struct Pass {
    renames: Vec<(Rename, HirId)>,
    bindings: FxHashMap<Symbol, Vec<HirId>>,
    references: FxHashMap<(Symbol, Res), Vec<HirId>>,
}

lint!(default Pass;
    NEEDLESS_IMPORT_RENAME: "`use .. as` renames of imports with a name that collides with no other item in scope",
    REEXPORTED_DEPENDENCY: "exported `use` items naming a dependency's module, glob, value, or macro, or a dependency type or trait no exported signature names"
);

// --- [OPERATIONS] ----------------------------------------------------------------------

fn binding(child: &ModChild) -> (Symbol, Res) { (child.ident.name, child.res.map_id(|never| never)) }

fn signature_items(cx: &LateContext<'_>) -> FxHashSet<DefId> {
    struct Signatures<'tcx> {
        tcx: TyCtxt<'tcx>,
        named: FxHashSet<DefId>,
    }
    impl<'tcx> Visitor<'tcx> for Signatures<'tcx> {
        fn visit_field_def(&mut self, s: &'tcx FieldDef<'tcx>) {
            if self.tcx.visibility(s.def_id).is_public() {
                intravisit::walk_field_def(self, s);
            }
        }

        fn visit_path(&mut self, path: &Path<'tcx>, _: HirId) {
            let aliases = path
                .segments
                .iter()
                .map(|segment| segment.res)
                .chain([path.res])
                .filter(|res| res.opt_def_id().is_some_and(|named| self.named.insert(named)))
                .filter_map(|res| if let Res::Def(DefKind::TyAlias, alias) = res { alias.as_local() } else { None })
                .collect::<Vec<_>>();
            for alias in aliases {
                self.visit_item(self.tcx.hir_expect_item(alias));
            }
            intravisit::walk_path(self, path);
        }
    }
    let tcx = cx.tcx;
    let mut signatures = Signatures { tcx, named: FxHashSet::default() };
    let reached = |item: LocalDefId| cx.effective_visibilities.is_exported(item);
    for owner in tcx.hir_crate_items(()).owners() {
        let header = matches!(tcx.def_kind(owner), DefKind::Impl { of_trait: true })
            && tcx.type_of(owner).instantiate_identity().skip_normalization().peel_refs().ty_adt_def().and_then(|adt| adt.did().as_local()).is_some_and(reached);
        match tcx.hir_owner_node(owner) {
            OwnerNode::Item(node) if header || !matches!(node.kind, ItemKind::Impl(_) | ItemKind::Use(..)) && reached(owner.def_id) => signatures.visit_item(node),
            OwnerNode::ImplItem(node) if reached(owner.def_id) => signatures.visit_impl_item(node),
            OwnerNode::TraitItem(node) if reached(owner.def_id) => signatures.visit_trait_item(node),
            _ => (),
        }
    }
    signatures.named
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    // --- [IMPORTS]

    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        fn scope(tcx: TyCtxt<'_>, bindings: impl IntoIterator<Item = (Symbol, Res)>) -> FxHashMap<Symbol, Vec<Res>> {
            bindings
                .into_iter()
                .flat_map(|(name, res)| iter::once(name).chain(res.opt_def_id().and_then(|declared| tcx.opt_item_name(declared)).filter(|&original| original != name)).map(move |name| (name, res)))
                .into_grouping_map_with_hasher(FxBuildHasher)
                .collect()
        }

        gen fn leaves<'hir>(root: UseTree<'hir>, root_node: HirId) -> (UseTree<'hir>, Option<Ident>, HirId, Option<Ident>) {
            let mut pending = vec![(root, root_node, None)];
            while let Some((tree, node, outer)) = pending.pop() {
                let named = tree.prefix.segments.iter().rev().map(|segment| segment.ident).find(|ident| ident.name != kw::SelfLower).or(outer);
                match tree.kind {
                    UseKind::Nested { items } => pending.extend(items.iter().rev().map(|&(nested, nested_node, _)| (nested, nested_node, named))),
                    UseKind::Single(alias) => yield (tree, Some(alias), node, named),
                    UseKind::Glob => yield (tree, None, node, named),
                }
            }
        }

        gen fn item_bindings(cx: &LateContext<'_>, item: &Item<'_>) -> (Symbol, Res) {
            let ItemKind::Use(import) = item.kind else {
                if let Some(ident) = item.kind.ident() {
                    yield (ident.name, Res::Def(cx.tcx.def_kind(item.owner_id), item.owner_id.to_def_id()));
                }
                return;
            };
            for (tree, alias, ..) in leaves(import, item.hir_id()) {
                match alias {
                    Some(ident) =>
                        for res in tree.prefix.res.present_items() {
                            yield (ident.name, res);
                        },
                    None =>
                        for child in tree.prefix.res.present_items().filter_map(|res| res.opt_def_id()).flat_map(|target| cx.tcx.module_children(target)) {
                            yield binding(child);
                        },
                }
            }
        }

        let tcx = cx.tcx;
        let named = LazyCell::new(|| signature_items(cx));
        let justified = |res: Res| match res {
            Res::Def(DefKind::Struct | DefKind::Enum | DefKind::Union | DefKind::ForeignTy | DefKind::Trait | DefKind::TraitAlias, imported) => named.contains(&imported),
            Res::Def(DefKind::TyAlias, imported) =>
                named.contains(&imported) || tcx.type_of(imported).instantiate_identity().skip_normalization().ty_adt_def().is_some_and(|adt| named.contains(&adt.did())),
            _ => false,
        };
        let exported_leaves = tcx
            .hir_free_items()
            .map(|id| tcx.hir_item(id))
            .filter_map(|item| if let ItemKind::Use(tree) = item.kind { Some((item, tree)) } else { None })
            .filter(|(item, _)| tcx.visibility(item.owner_id).is_public() && exported(cx, tcx.parent_module_from_def_id(item.owner_id.def_id).to_local_def_id()) && !expanded_externally(cx, item.span))
            .flat_map(|(item, root)| leaves(root, item.hir_id()).map(move |(tree, alias, ..)| (item, tree, alias)));
        for (item, tree, alias) in exported_leaves {
            let whole = alias.is_none() || tree.prefix.res.present_items().any(|res| matches!(res, Res::Def(DefKind::Mod, _)));
            let Some(imported) = tree.prefix.res.present_items().find_map(|res| res.opt_def_id().filter(|imported| !imported.is_local())) else { continue };
            if !whole && tree.prefix.res.present_items().any(justified) {
                continue;
            }
            let path = tcx.def_path_str(imported);
            let reach = if whole { " whole" } else { ", which no exported signature takes or returns" };
            span_lint_hir_and_then(cx, REEXPORTED_DEPENDENCY, item.hir_id(), tree.prefix.span, format!("`pub use` exports `{path}`{reach}"), |diag| {
                diag.help(format!("delete the re-export and import `{path}` in each consumer"));
            });
        }
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
                iter::chain(tcx.module_children(prelude), exported).map(binding)
            })
            .chain(
                tcx.crates(())
                    .iter()
                    .filter(|&&krate| tcx.extern_crate(krate).is_some_and(|source| source.dependency_of == LOCAL_CRATE))
                    .map(|&krate| (tcx.crate_name(krate), Res::Def(DefKind::Mod, krate.as_def_id()))),
            )
            .chain(PrimTy::ALL.into_iter().map(|primitive| (primitive.name(), Res::PrimTy(primitive))));
        let prelude = scope(tcx, prelude);
        let mut scopes = FxHashMap::default();
        let imports = tcx
            .hir_free_items()
            .map(|id| tcx.hir_item(id))
            .filter_map(|item| if let ItemKind::Use(tree) = item.kind { Some((item, tree)) } else { None })
            .filter(|(item, _)| !(expanded_externally(cx, item.span) || exported(cx, item.owner_id.def_id) || find_attr!(tcx, item.hir_id(), CfgTrace(..))))
            .map(|(item, root)| {
                let renamed = leaves(root, item.hir_id()).filter_map(|(tree, alias, node, named)| {
                    let (alias, original) = alias.zip(named).filter(|(alias, original)| ![original.name, kw::SelfLower, kw::Underscore].contains(&alias.name))?;
                    Some(Rename { node, span: tree.prefix.span.to(alias.span), original, alias, imported: tree.prefix.res })
                });
                (item, renamed.collect::<Vec<_>>())
            })
            .filter(|(_, renamed)| !renamed.is_empty());
        for (item, mut renames) in imports {
            let module = tcx.parent_module_from_def_id(item.owner_id.def_id);
            let boundary = tcx.local_def_id_to_hir_id(module.to_local_def_id());
            let mut remove_collisions = |bindings: &FxHashMap<Symbol, Vec<Res>>| {
                renames.retain(|rename| {
                    bindings.get(&rename.original.name).is_none_or(|bound| bound.iter().all(|&res| res.ns().is_some_and(|namespace| rename.imported[namespace].is_none_or(|imported| imported == res))))
                });
            };
            remove_collisions(&prelude);
            remove_collisions(scopes.entry(boundary).or_insert_with(|| scope(tcx, tcx.module_children_local(module.to_local_def_id()).iter().map(binding))));
            for (node, block) in
                tcx.hir_parent_iter(item.hir_id()).take_while(|&(parent, _)| parent != boundary).filter_map(|(node, found)| if let Node::Block(block) = found { Some((node, block)) } else { None })
            {
                remove_collisions(scopes.entry(node).or_insert_with(|| {
                    let bindings = block
                        .stmts
                        .iter()
                        .filter_map(|stmt| match stmt.kind {
                            StmtKind::Item(id) => Some(tcx.hir_item(id)),
                            _ => None,
                        })
                        .flat_map(|scoped| item_bindings(cx, scoped));
                    scope(tcx, bindings)
                }));
            }
            let declared =
                tcx.hir_parent_iter(item.hir_id()).take_while(|&(parent, _)| parent != boundary).find_map(|(node, found)| matches!(found, Node::Block(_)).then_some(node)).unwrap_or(boundary);
            for rename in &renames {
                self.bindings.entry(rename.original.name).or_default();
                for res in rename.imported.present_items() {
                    self.references.entry((rename.alias.name, res)).or_default();
                }
            }
            self.renames.extend(renames.into_iter().map(|rename| (rename, declared)));
        }
    }

    // --- [REFERENCES]

    fn check_pat(&mut self, _: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::Binding(_, binding, ident, _) = pat.kind
            && let Some(bindings) = self.bindings.get_mut(&ident.name)
        {
            bindings.push(binding);
        }
    }

    fn check_path(&mut self, _: &LateContext<'tcx>, path: &Path<'tcx>, node: HirId) {
        if let Some(first) = path.segments.first()
            && let Some(references) = self.references.get_mut(&(first.ident.name, first.res))
        {
            references.push(node);
        }
    }

    // --- [DIAGNOSTICS]

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let shadowed = |name, namespace, node: HirId| {
            namespace != Namespace::MacroNS
                && iter::once(node)
                    .chain(tcx.hir_parent_id_iter(node))
                    .dedup_by(|earlier, later| earlier.owner == later.owner)
                    .take_while(|parent| !matches!(tcx.def_kind(parent.owner.def_id), DefKind::Mod))
                    .any(|parent| {
                        let generics = tcx.generics_of(parent.owner.def_id);
                        (0..generics.count()).any(|index| {
                            let param = generics.param_at(index, tcx);
                            param.name == name && matches!((&param.kind, namespace), (GenericParamDefKind::Type { .. }, Namespace::TypeNS) | (GenericParamDefKind::Const { .. }, Namespace::ValueNS))
                        }) || namespace == Namespace::ValueNS
                            && self.bindings.get(&name).into_iter().flatten().filter(|binding| binding.owner == parent.owner).any(|binding| {
                                let tree = tcx.region_scope_tree(parent.owner.def_id);
                                let position = Scope { local_id: parent.local_id, data: ScopeData::Node };
                                parent.owner != node.owner || tree.opt_encl_scope(position).is_none() || tree.var_scope(binding.local_id).is_some_and(|scope| tree.is_subscope_of(position, scope))
                            })
                    })
        };
        for (rename, _) in self.renames.drain(..).filter(|(rename, declared)| {
            let module = tcx.parent_module_from_def_id(rename.node.owner.def_id);
            let within = |node: HirId| tcx.parent_module_from_def_id(node.owner.def_id) == module && iter::once(node).chain(tcx.hir_parent_id_iter(node)).any(|parent| parent == *declared);
            !rename.imported.present_items().any(|res| {
                res.ns().is_some_and(|namespace| {
                    shadowed(rename.original.name, namespace, *declared)
                        || namespace == Namespace::ValueNS
                            && !matches!(res, Res::Def(DefKind::Fn | DefKind::AssocFn, _))
                            && self.bindings.get(&rename.original.name).is_some_and(|bindings| bindings.iter().copied().any(within))
                        || self
                            .references
                            .get(&(rename.alias.name, res))
                            .is_some_and(|references| references.iter().copied().any(|reference| within(reference) && shadowed(rename.original.name, namespace, reference)))
                })
            })
        }) {
            span_lint_hir_and_then(
                cx,
                NEEDLESS_IMPORT_RENAME,
                rename.node,
                rename.span,
                format!("`{}` renamed to `{}` with no other `{}` in scope", rename.original, rename.alias, rename.original),
                |diag| {
                    diag.help(format!("remove `as {}` and name `{}` at each use", rename.alias, rename.original));
                },
            );
        }
    }
}
