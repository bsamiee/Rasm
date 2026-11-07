use core::{iter, slice};
use std::{
    ffi::OsStr,
    io::{self, ErrorKind},
    path::{self, Path, PathBuf},
    sync::Arc,
};

use clippy_utils::{
    diagnostics::{span_lint, span_lint_hir_and_then},
    is_entrypoint_fn, peel_blocks, peel_hir_expr_refs, tokenize_with_text,
};
use itertools::Itertools as _;
use rustc_ast::visit;
use rustc_attr_ir::find_attr;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_errors::{Diag, DiagCtxt, catch_fatal_errors, emitter::SilentEmitter};
use rustc_expand::module::{DirOwnership, ModError, default_submod_path};
use rustc_hir::{
    Block, Expr, ExprKind, HirId, ImplItem, Item, ItemKind, Node, Pat, PatExprKind, PatKind, TraitItem,
    def::DefKind,
    def_id::{CRATE_DEF_ID, DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lexer::TokenKind;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{hir::nested_filter, ty::TyCtxt};
use rustc_parse::{lexer::StripTokens, new_parser_from_source_str};
use rustc_session::parse::ParseSess;
use rustc_span::{BytePos, FileName, Ident, Pos as _, RelativeBytePos, SourceFile, Span, Symbol, kw, sym};

use crate::{expanded_externally, exported, fn_item_value, generated, parameter};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    sites: FxIndexMap<LocalDefId, Vec<(LocalDefId, Span)>>,
    pinned: FxHashSet<LocalDefId>,
}

lint!(default Pass;
    ORPHAN_MODULE: "source files and `mod.rs` folders in a module's folder that no module declares",
    SINGLE_READER: "crate-private free functions, constants, and statics with one reader in their module, other than the entry point"
);

impl Pass {
    fn orphans(cx: &LateContext<'_>, files: &[Arc<SourceFile>], declared: impl IntoIterator<Item = PathBuf>) {
        const EXTENSION: &str = "rs";
        let declared = declared.into_iter().collect::<FxHashSet<_>>();
        let rooted = |stem: Option<&str>| stem.is_some_and(|stem| matches!(stem, "lib" | "main" | "mod" | "build"));
        for (file, path) in files.iter().filter_map(|file| match &file.name {
            FileName::Real(name) => name.local_path().filter(|path| path.extension().is_some_and(|extension| extension == EXTENSION)).map(|path| (file, path)),
            _ => None,
        }) {
            let first = file.src.as_ref().and_then(|source| source.chars().next()).map_or_default(char::len_utf8);
            let span = Span::with_root_ctxt(file.start_pos, file.absolute_position(RelativeBytePos::from_usize(first)));
            let report = |message: String| span_lint(cx, ORPHAN_MODULE, span, message);
            let folder = if rooted(path.file_stem().and_then(OsStr::to_str)) { path.with_file_name("") } else { path.with_extension("") };
            match fs_err::read_dir(folder).and_then(|entries| entries.map(|entry| entry.map(|entry| entry.path())).collect::<Result<Vec<_>, _>>()) {
                Ok(entries) => entries.into_iter().sorted().filter(|entry| !rooted(entry.file_stem().and_then(OsStr::to_str))).for_each(|entry| {
                    let nested = entry.extension().is_none_or(|extension| extension != EXTENSION);
                    let module = if nested { entry.join("mod.rs") } else { entry };
                    match fs_err::canonicalize(&module) {
                        Ok(physical) if declared.contains(dunce::simplified(&physical)) => {}
                        Ok(_) => report(format!("`{}` sits in this module's folder with no module declaring it, declare it here or delete it", module.display())),
                        Err(error) if nested && matches!(error.kind(), ErrorKind::NotFound | ErrorKind::NotADirectory) => {}
                        Err(error) => report(error.to_string()),
                    }
                }),
                Err(error) if error.kind() == ErrorKind::NotFound => {}
                Err(error) => report(error.to_string()),
            }
        }
    }

    fn reference(&mut self, cx: &LateContext<'_>, target: Option<DefId>, node: HirId, span: Span) {
        let tcx = cx.tcx;
        if let Some(target) = target
            .and_then(DefId::as_local)
            .filter(|&target| matches!(tcx.def_kind(target), DefKind::Fn | DefKind::Const | DefKind::Static { nested: false, .. }) && tcx.def_kind(tcx.local_parent(target)) == DefKind::Mod)
            .filter(|target| !self.pinned.contains(target))
        {
            let owner = tcx.hir_enclosing_body_owner(node);
            let reader = match tcx.def_kind(owner) {
                DefKind::AnonConst => tcx
                    .hir_parent_iter(tcx.local_def_id_to_hir_id(owner))
                    .find_map(|(parent, found)| match found {
                        Node::Expr(_) => Some(Some(tcx.hir_enclosing_body_owner(parent))),
                        _ => found.associated_body().map(|_| None),
                    })
                    .flatten()
                    .filter(|&reader| tcx.def_kind(reader) != DefKind::AnonConst),
                _ => Some(owner),
            };
            let listed =
                matches!(tcx.def_kind(owner), DefKind::Const | DefKind::AssocConst | DefKind::Static { .. }) && matches!(tcx.hir_node(node), Node::Expr(read) if fn_item_value(cx, read).is_some());
            let forwarded = tcx.hir_maybe_body_owned_by(owner).filter(|_| matches!(tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)).is_some_and(|body| {
                let passed = |arguments: &[Expr<'_>]| arguments.iter().all(|argument| parameter(body, peel_hir_expr_refs(argument).0).is_some());
                match peel_blocks(body.value) {
                    value if value.span.from_expansion() => false,
                    Expr { kind: ExprKind::Call(callee, arguments), .. } => callee.hir_id == node && passed(arguments),
                    Expr { kind: ExprKind::MethodCall(_, receiver, arguments, _), hir_id, .. } => *hir_id == node && passed(slice::from_ref(receiver)) && passed(arguments),
                    _ => false,
                }
            });
            match reader {
                Some(reader) if !expanded_externally(cx, span) && !generated(cx, node) && !listed && !forwarded =>
                    self.sites.entry(target).or_default().push((tcx.typeck_root_def_id_local(reader), span)),
                _ => {
                    self.pinned.insert(target);
                }
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn module_paths(psess: &ParseSess, name: FileName, file: &Path, relative: Option<Ident>, source: &str) -> Option<Vec<(PathBuf, bool)>> {
    fn explicit_paths(directory: &Path, paths: &mut Vec<PathBuf>, attribute: &rustc_ast::MetaItem) {
        if attribute.has_name(sym::path) {
            paths.extend(attribute.value_str().map(|path| directory.join(path.as_str())));
        } else if attribute.has_name(sym::cfg_attr) {
            attribute.meta_item_list().into_iter().flatten().skip(1).filter_map(rustc_ast::MetaItemInner::meta_item).for_each(|nested| explicit_paths(directory, paths, nested));
        }
    }

    struct Modules<'scan> {
        psess: &'scan ParseSess,
        directory: &'scan Path,
        relative: Option<Ident>,
        paths: &'scan mut Vec<(PathBuf, bool)>,
    }

    impl<'ast> visit::Visitor<'ast> for Modules<'_> {
        fn visit_item(&mut self, item: &'ast rustc_ast::Item) {
            let rustc_ast::ItemKind::Mod(_, name, module) = &item.kind else { return visit::walk_item(self, item) };
            let direct = item.attrs.iter().position(|attribute| attribute.has_name(sym::path));
            let attributes = direct.map_or(&*item.attrs, |index| &item.attrs[..=index]);
            let mut explicit = Vec::new();
            attributes
                .iter()
                .filter(|attribute| attribute.has_name(sym::path) || attribute.has_name(sym::cfg_attr))
                .filter_map(rustc_ast::Attribute::meta)
                .for_each(|attribute| explicit_paths(self.directory, &mut explicit, &attribute));
            let rustc_ast::ModKind::Loaded(items, ..) = module else {
                self.paths.extend(explicit.into_iter().map(|path| (path, true)));
                if direct.is_some() {
                    return;
                }
                return match default_submod_path(self.psess, *name, self.relative, self.directory) {
                    Ok(path) => self.paths.push((path.file_path, matches!(path.dir_ownership, DirOwnership::Owned { relative: None }))),
                    Err(ModError::MultipleCandidates(_, sibling, folder)) => self.paths.extend([(sibling, false), (folder, true)]),
                    Err(_) => (),
                };
            };
            let default = direct.is_none().then(|| self.relative.map_or_else(|| self.directory.to_path_buf(), |relative| self.directory.join(relative.as_str())).join(name.as_str()));
            for directory in iter::chain(explicit, default) {
                let mut inner = Modules { psess: self.psess, directory: &directory, relative: None, paths: self.paths };
                items.iter().for_each(|item| inner.visit_item(item));
            }
        }
    }

    let krate = catch_fatal_errors(|| {
        let mut parser = new_parser_from_source_str(psess, name, source.to_owned(), StripTokens::ShebangAndFrontmatter).map_err(|errors| errors.into_iter().for_each(Diag::cancel)).ok()?;
        parser.parse_crate_mod().map_err(Diag::cancel).ok()
    })
    .ok()
    .flatten();
    let parsed = psess.dcx().has_errors().is_none();
    psess.dcx().reset_err_count();
    let mut paths = Vec::new();
    let mut modules = Modules { psess, directory: file.parent().unwrap_or_else(|| Path::new("")), relative, paths: &mut paths };
    visit::Visitor::visit_crate(&mut modules, &krate.filter(|_| parsed)?);
    Some(paths)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let target = match expr.kind {
            ExprKind::Path(qpath) => cx.qpath_res(&qpath, expr.hir_id).opt_def_id(),
            ExprKind::MethodCall(..) => cx.typeck_results().type_dependent_def_id(expr.hir_id),
            _ => None,
        };
        self.reference(cx, target, expr.hir_id, expr.span);
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::Expr(pattern) = pat.kind
            && let PatExprKind::Path(qpath) = &pattern.kind
        {
            self.reference(cx, cx.qpath_res(qpath, pattern.hir_id).opt_def_id(), pat.hir_id, pat.span);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        const LEVELS: usize = 3;
        const RAW_IDENTIFIER_PREFIX: &str = "r#";
        struct Walk<'tcx> {
            tcx: TyCtxt<'tcx>,
            seen: FxHashSet<BytePos>,
            modules: FxHashMap<BytePos, bool>,
            outermost: Option<LocalDefId>,
            depth: usize,
            enclosing: FxHashMap<LocalDefId, usize>,
            deepest: FxHashMap<LocalDefId, usize>,
        }
        impl Walk<'_> {
            fn owned<F>(&mut self, owner: LocalDefId, walk: F)
            where F: FnOnce(&mut Self) {
                self.enclosing.insert(owner, self.depth);
                let outer = self.outermost;
                self.outermost = if self.depth == 0 { Some(owner) } else { outer };
                walk(self);
                self.outermost = outer;
            }
        }
        impl<'tcx> Visitor<'tcx> for Walk<'tcx> {
            type NestedFilter = nested_filter::All;

            fn maybe_tcx(&mut self) -> TyCtxt<'tcx> { self.tcx }

            fn visit_ident(&mut self, ident: Ident) { self.seen.insert(ident.span.lo()); }

            fn visit_item(&mut self, i: &'tcx Item<'tcx>) {
                if let ItemKind::Mod(_, module) = i.kind
                    && !module.spans.inner_span.is_dummy()
                    && let file = self.tcx.sess.source_map().lookup_source_file(module.spans.inner_span.lo())
                    && !file.contains(i.span.lo())
                {
                    self.modules.insert(file.start_pos, find_attr!(self.tcx, i.hir_id(), Path(..)));
                }
                self.owned(i.owner_id.def_id, |walk| intravisit::walk_item(walk, i));
            }

            fn visit_impl_item(&mut self, ii: &'tcx ImplItem<'tcx>) { self.owned(ii.owner_id.def_id, |walk| intravisit::walk_impl_item(walk, ii)); }

            fn visit_trait_item(&mut self, ti: &'tcx TraitItem<'tcx>) { self.owned(ti.owner_id.def_id, |walk| intravisit::walk_trait_item(walk, ti)); }

            fn visit_block(&mut self, b: &'tcx Block<'tcx>) {
                let braced = usize::from(!b.span.from_expansion());
                self.depth = self.depth.strict_add(braced);
                if let Some(outermost) = self.outermost {
                    self.deepest.entry(outermost).and_modify(|deepest| *deepest = (*deepest).max(self.depth)).or_insert(self.depth);
                }
                intravisit::walk_block(self, b);
                self.depth = self.depth.strict_sub(braced);
            }
        }
        let tcx = cx.tcx;
        let mut walk = Walk { tcx, seen: FxHashSet::default(), modules: FxHashMap::default(), outermost: None, depth: 0, enclosing: FxHashMap::default(), deepest: FxHashMap::default() };
        tcx.hir_walk_toplevel_module(&mut walk);
        let map = tcx.sess.source_map();
        let root = map.lookup_source_file(tcx.def_span(CRATE_DEF_ID).lo());
        walk.modules.insert(root.start_pos, true);
        let compiled = map.files().iter().filter(|file| file.is_real_file() && !file.is_imported()).cloned().collect::<Vec<_>>();
        let local = |name: &FileName| if let FileName::Real(real) = name { real.local_path().map(Path::to_path_buf) } else { None };
        let identity = |file: &Path, siblings: bool| {
            let absolute = path::absolute(file)?;
            let resolved = fs_err::canonicalize(&absolute)?;
            let directory = absolute.parent().map(fs_err::canonicalize).transpose()?;
            Ok::<_, io::Error>((
                dunce::simplified(&resolved).to_owned(),
                directory.as_deref().map(dunce::simplified).map(Path::to_path_buf),
                if siblings { None } else { absolute.file_stem().map(ToOwned::to_owned) },
            ))
        };
        let mod_file = |file: &Path| file.file_stem().is_some_and(|stem| stem == kw::Mod.as_str());
        let mut pending = compiled
            .iter()
            .filter_map(|file| {
                let path = local(&file.name)?;
                let siblings = walk.modules.get(&file.start_pos).map(|&overridden| overridden || mod_file(&path));
                Some((Arc::clone(file), path, siblings))
            })
            .collect::<Vec<_>>();
        let Ok(mut known) = pending.iter().map(|(_, path, siblings)| identity(path, siblings.unwrap_or_default())).collect::<io::Result<FxHashSet<_>>>() else { return };
        let mut parser = ParseSess::with_dcx(DiagCtxt::new(Box::new(SilentEmitter)), tcx.sess.psess.clone_source_map());
        parser.edition = tcx.sess.edition();
        let mut unseen = FxHashSet::default();
        while let Some((source_file, source_path, sibling_modules)) = pending.pop() {
            let Some(source) = &source_file.src else { continue };
            for (_, text, span) in tokenize_with_text(source).filter(|(kind, ..)| matches!(kind, TokenKind::Ident | TokenKind::RawIdent)) {
                let name = text.trim_prefix(RAW_IDENTIFIER_PREFIX);
                if !walk.seen.contains(&source_file.absolute_position(RelativeBytePos::from_usize(span.start.strict_add(text.len().strict_sub(name.len()))))) {
                    unseen.insert(Symbol::intern(name));
                }
            }
            let Some(sibling_modules) = sibling_modules else { continue };
            let relative = source_path.file_stem().filter(|_| !sibling_modules).and_then(OsStr::to_str).map(Ident::from_str);
            let Some(declared) = module_paths(&parser, source_file.name.clone(), &source_path, relative, source) else { return };
            for (candidate, siblings) in declared {
                let id = match identity(&candidate, siblings) {
                    Ok(id) => id,
                    Err(error) if error.kind() == ErrorKind::NotFound => continue,
                    Err(_) => return,
                };
                if !known.insert(id) {
                    continue;
                }
                match map.load_file(&candidate) {
                    Ok(file) => pending.push((file, candidate, Some(siblings))),
                    Err(error) if error.kind() == ErrorKind::NotFound => (),
                    Err(_) => return,
                }
            }
        }
        if !tcx.sess.is_test_crate() {
            Self::orphans(cx, &compiled, known.into_iter().map(|(path, _, _)| path));
        }
        for (&target, sites) in &self.sites {
            let outside = || sites.iter().filter(move |&&(caller, _)| caller != target);
            if let Ok(caller) = outside().map(|&(caller, _)| caller).all_equal_value()
                && !is_entrypoint_fn(cx, caller.to_def_id())
                && !self.pinned.contains(&target)
                && !expanded_externally(cx, tcx.def_span(target))
                && !exported(cx, target)
                && tcx.parent_module_from_def_id(caller) == tcx.parent_module_from_def_id(target)
                && !is_entrypoint_fn(cx, target.to_def_id())
                && !find_attr!(tcx, target, Used { .. })
                && let name = tcx.item_name(target)
                && !unseen.contains(&name)
                && walk.enclosing.get(&caller).is_some_and(|&enclosing| walk.deepest.get(&target).copied().unwrap_or_default().strict_add(enclosing) <= LEVELS)
            {
                let caller_name = tcx.item_name(caller);
                let generic = |item: LocalDefId| tcx.generics_of(item).requires_monomorphization(tcx);
                let form = match tcx.def_kind(target) {
                    DefKind::Fn if sites.iter().any(|&(reader, _)| reader == target) || generic(target) && !generic(caller) || tcx.is_const_fn(target.to_def_id()) => "a nested `fn`",
                    DefKind::Fn => "a closure",
                    _ => "a nested item",
                };
                span_lint_hir_and_then(cx, SINGLE_READER, tcx.local_def_id_to_hir_id(target), tcx.def_span(target), format!("`{name}` has one reader, `{caller_name}`"), |diag| {
                    diag.span_note(outside().map(|&(_, site)| site).collect::<Vec<_>>(), "read here");
                    diag.help(format!("move `{name}` into `{caller_name}` as {form}"));
                });
            }
        }
    }
}
