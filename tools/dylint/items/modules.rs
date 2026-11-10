use core::iter;
use std::ffi::OsStr;
use std::io::{self, ErrorKind};
use std::path::{self, Path, PathBuf};
use std::sync::Arc;

use clippy_utils::diagnostics::{span_lint, span_lint_hir_and_then};
use clippy_utils::res::MaybeResPath as _;
use clippy_utils::{desugar_await, get_async_fn_body, is_entrypoint_fn, peel_blocks_with_stmt, peel_hir_expr_refs, tokenize_with_text};
use itertools::{Either, Itertools as _};
use rustc_ast::visit;
use rustc_attr_ir::find_attr;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_errors::emitter::SilentEmitter;
use rustc_errors::{DiagCtxt, FatalError, catch_fatal_errors};
use rustc_expand::module::{DirOwnership, ModError, default_submod_path};
use rustc_hir::def::{DefKind, Res};
use rustc_hir::def_id::{CRATE_DEF_ID, DefId, LocalDefId, LocalModId};
use rustc_hir::intravisit::{self, FnKind, Visitor};
use rustc_hir::{Block, Body, Expr, ExprKind, FnDecl, HirId, ImplItem, Item, ItemKind, LocalSource, Node, Pat, PatExprKind, PatKind, StmtKind, TraitItem};
use rustc_lexer::TokenKind;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::hir::nested_filter;
use rustc_middle::ty::{GenericArg, Ty, TyCtxt, TypeckResults, TypingEnv, Unnormalized};
use rustc_parse::lexer::StripTokens;
use rustc_parse::{new_parser_from_source_str, unwrap_or_emit_fatal};
use rustc_session::parse::ParseSess;
use rustc_span::{BytePos, FileName, Ident, Pos as _, RelativeBytePos, SourceFile, Span, Symbol, kw, sym};

use crate::{accessed_field, awaited_output, constructed, expanded_externally, exported, fn_item_value, generated, in_test, item_path, parameter, pattern_fields, signature};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    sites: FxIndexMap<LocalDefId, Vec<(LocalDefId, Span)>>,
    pinned: FxHashSet<LocalDefId>,
    forwards: FxHashSet<HirId>,
}

lint!(default Pass;
    ORPHAN_MODULE: "source files and `mod.rs` folders in a module's folder that no module declares",
    SINGLE_READER: "crate-private free items with one reader, outside macro output, reading no item hidden from that reader, and with no signature type their module declares",
    FORWARDING_FUNCTION: "functions and inherent methods whose body calls another function with each parameter once, in any order, and returns its result unchanged"
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
            match reader {
                Some(reader) if !expanded_externally(cx, span) && !generated(cx, node) && !listed && !self.forwards.contains(&node) =>
                    self.sites.entry(target).or_default().push((tcx.typeck_root_def_id_local(reader), span)),
                _ => {
                    self.pinned.insert(target);
                }
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn movable(cx: &LateContext<'_>, target: LocalDefId, module: LocalModId) -> bool {
    struct Reach<'tcx> {
        tcx: TyCtxt<'tcx>,
        typeck: &'tcx TypeckResults<'tcx>,
        module: DefId,
        hidden: bool,
    }
    impl Reach<'_> {
        fn read(&mut self, item: DefId) {
            self.hidden = self.hidden
                || item.as_local().is_some_and(|local| {
                    !matches!(
                        self.tcx.def_kind(local),
                        DefKind::TyParam | DefKind::ConstParam | DefKind::LifetimeParam | DefKind::OpaqueTy | DefKind::AnonConst | DefKind::Closure | DefKind::Impl { .. }
                    ) && !self.tcx.visibility(local).is_accessible_from(self.module, self.tcx)
                });
        }
    }
    impl<'tcx> Visitor<'tcx> for Reach<'tcx> {
        type NestedFilter = nested_filter::OnlyBodies;

        fn maybe_tcx(&mut self) -> TyCtxt<'tcx> { self.tcx }

        fn visit_path(&mut self, path: &rustc_hir::Path<'tcx>, _: HirId) {
            path.segments.iter().map(|segment| segment.res).chain([path.res]).filter_map(|res| res.opt_def_id()).for_each(|item| self.read(item));
            intravisit::walk_path(self, path);
        }

        fn visit_expr(&mut self, ex: &'tcx Expr<'tcx>) {
            let typeck = self.typeck;
            match ex.kind {
                ExprKind::MethodCall(..) => typeck.type_dependent_def_id(ex.hir_id).into_iter().for_each(|item| self.read(item)),
                ExprKind::Field(..) => accessed_field(typeck, ex).into_iter().for_each(|item| self.read(item)),
                _ => constructed(typeck, ex).into_iter().flat_map(|(_, fields)| fields).for_each(|(field, _)| self.read(field.did)),
            }
            intravisit::walk_expr(self, ex);
        }

        fn visit_pat(&mut self, p: &'tcx Pat<'tcx>) {
            pattern_fields(self.typeck, p).for_each(|(field, _)| self.read(field));
            intravisit::walk_pat(self, p);
        }
    }
    let tcx = cx.tcx;
    let home = tcx.parent_module_from_def_id(target);
    let held = match tcx.def_kind(target) {
        DefKind::Fn => Either::Left(signature(tcx, target.to_def_id()).inputs_and_output.iter()),
        _ => Either::Right(iter::once(tcx.type_of(target).instantiate_identity().skip_normalization())),
    };
    let operation =
        held.flat_map(Ty::walk).filter_map(GenericArg::as_type).any(|ty| ty.ty_adt_def().and_then(|adt| adt.did().as_local()).is_some_and(|adt| tcx.parent_module_from_def_id(adt) == home));
    let mut reach = Reach { tcx, typeck: tcx.typeck(target), module: module.to_def_id(), hidden: false };
    !operation && {
        reach.visit_item(tcx.hir_expect_item(target));
        !reach.hidden
    }
}

fn forwarded_call<'tcx>(cx: &LateContext<'tcx>, owner: LocalDefId, body: &'tcx Body<'tcx>) -> Option<(&'tcx Expr<'tcx>, DefId)> {
    let tcx = cx.tcx;
    let typeck = cx.typeck_results();
    let asynchronous = tcx.asyncness(owner).is_async();
    let result = peel_blocks_with_stmt(if asynchronous { get_async_fn_body(tcx, body)? } else { body.value });
    let call = if asynchronous { desugar_await(result)? } else { result };
    let moved = match body.value.kind {
        ExprKind::Closure(closure) if asynchronous && let ExprKind::Block(block, _) = tcx.hir_body(closure.body).value.kind => block.stmts,
        _ => &[],
    };
    let position = |argument: &Expr<'_>| {
        let argument = peel_hir_expr_refs(argument).0;
        parameter(body, argument).or_else(|| {
            let local = argument.res_local_id()?;
            moved.iter().find_map(|stmt| match stmt.kind {
                StmtKind::Let(binding) if matches!(binding.source, LocalSource::AsyncFn) && binding.pat.hir_id == local => parameter(body, binding.init?),
                _ => None,
            })
        })
    };
    let (callee, arguments) = match call.kind {
        _ if call.span.from_expansion() => None,
        ExprKind::Call(&Expr { kind: ExprKind::Path(qpath), hir_id, .. }, arguments) if let Res::Def(DefKind::Fn | DefKind::AssocFn, callee) = typeck.qpath_res(&qpath, hir_id) =>
            Some((callee, Either::Left(arguments.iter()))),
        ExprKind::MethodCall(_, receiver, arguments, _) => Some((typeck.type_dependent_def_id(call.hir_id)?, Either::Right(iter::once(receiver).chain(arguments)))),
        _ => None,
    }?;
    let unchanged = |held| tcx.normalize_erasing_regions(TypingEnv::post_analysis(tcx, owner), Unnormalized::new(tcx.erase_and_anonymize_regions(held)));
    (callee != owner.to_def_id() && arguments.map(position).sorted().eq((0..body.params.len()).map(Some)) && unchanged(typeck.expr_ty(result)) == unchanged(awaited_output(tcx, owner.to_def_id())))
        .then_some((call, callee))
}

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
        unwrap_or_emit_fatal(new_parser_from_source_str(psess, name, source.to_owned(), StripTokens::ShebangAndFrontmatter)).parse_crate_mod().unwrap_or_else(|error| {
            error.emit();
            FatalError.raise()
        })
    });
    let failed = psess.dcx().has_errors();
    psess.dcx().reset_err_count();
    match (krate, failed) {
        (Ok(krate), None) => {
            let mut paths = Vec::new();
            visit::Visitor::visit_crate(&mut Modules { psess, directory: file.parent().unwrap_or_else(|| Path::new("")), relative, paths: &mut paths }, &krate);
            Some(paths)
        }
        (Ok(_), Some(_)) | (Err(FatalError), _) => None,
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_fn(&mut self, cx: &LateContext<'tcx>, kind: FnKind<'tcx>, _: &'tcx FnDecl<'tcx>, body: &'tcx Body<'tcx>, span: Span, function: LocalDefId) {
        let Some((call, callee)) = (!matches!(kind, FnKind::Closure)).then(|| forwarded_call(cx, function, body)).flatten() else { return };
        self.forwards.insert(if let ExprKind::Call(path, _) = call.kind { path.hir_id } else { call.hir_id });
        let tcx = cx.tcx;
        let node = tcx.local_def_id_to_hir_id(function);
        if !span.from_expansion()
            && tcx.trait_impl_of_assoc(function.to_def_id()).is_none()
            && tcx.trait_of_assoc(function.to_def_id()).is_none()
            && !is_entrypoint_fn(cx, function.to_def_id())
            && !tcx.codegen_fn_attrs(function).contains_extern_indicator()
            && !generated(cx, node)
            && !in_test(tcx, node)
        {
            let (name, target) = (item_path(cx, function.to_def_id()), item_path(cx, callee));
            span_lint_hir_and_then(cx, FORWARDING_FUNCTION, node, tcx.def_span(function), format!("`{name}` passes its parameters unchanged to `{target}`"), |diag| {
                diag.span_note(call.span, "forwarded here");
                diag.help(format!("call `{target}` at each call site of `{name}` and delete `{name}`"));
            });
        }
    }

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
            owners: Vec<(LocalDefId, usize)>,
            depth: usize,
            enclosing: FxHashMap<LocalDefId, usize>,
            deepest: FxHashMap<LocalDefId, usize>,
        }
        impl Walk<'_> {
            fn owned<F>(&mut self, owner: LocalDefId, walk: F)
            where F: FnOnce(&mut Self) {
                self.enclosing.insert(owner, self.depth);
                self.owners.push((owner, self.depth));
                walk(self);
                self.owners.pop();
            }
        }
        impl<'tcx> Visitor<'tcx> for Walk<'tcx> {
            type NestedFilter = nested_filter::All;

            fn maybe_tcx(&mut self) -> TyCtxt<'tcx> { self.tcx }

            fn visit_ident(&mut self, ident: Ident) { self.seen.insert(ident.span.lo()); }

            fn visit_item(&mut self, i: &'tcx Item<'tcx>) {
                let outer = self.depth;
                let filed = if let ItemKind::Mod(_, module) = i.kind
                    && !module.spans.inner_span.is_dummy()
                    && let file = self.tcx.sess.source_map().lookup_source_file(module.spans.inner_span.lo())
                    && !file.contains(i.span.lo())
                {
                    self.modules.insert(file.start_pos, find_attr!(self.tcx, i.hir_id(), Path(..)));
                    true
                } else {
                    false
                };
                let braced = matches!(i.kind, ItemKind::Impl(_) | ItemKind::Trait { .. } | ItemKind::Mod(..)) && !i.span.from_expansion();
                self.depth = if filed { 0 } else { outer.strict_add(usize::from(braced)) };
                self.owned(i.owner_id.def_id, |walk| intravisit::walk_item(walk, i));
                self.depth = outer;
            }

            fn visit_impl_item(&mut self, ii: &'tcx ImplItem<'tcx>) { self.owned(ii.owner_id.def_id, |walk| intravisit::walk_impl_item(walk, ii)); }

            fn visit_trait_item(&mut self, ti: &'tcx TraitItem<'tcx>) { self.owned(ti.owner_id.def_id, |walk| intravisit::walk_trait_item(walk, ti)); }

            fn visit_block(&mut self, b: &'tcx Block<'tcx>) {
                let braced = usize::from(!b.span.from_expansion());
                self.depth = self.depth.strict_add(braced);
                for &(owner, start) in &self.owners {
                    let relative = self.depth.strict_sub(start);
                    self.deepest.entry(owner).and_modify(|deepest| *deepest = (*deepest).max(relative)).or_insert(relative);
                }
                intravisit::walk_block(self, b);
                self.depth = self.depth.strict_sub(braced);
            }
        }
        let tcx = cx.tcx;
        let mut walk = Walk { tcx, seen: FxHashSet::default(), modules: FxHashMap::default(), owners: Vec::new(), depth: 0, enclosing: FxHashMap::default(), deepest: FxHashMap::default() };
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
        let mut parser = ParseSess::with_dcx(DiagCtxt::new(Box::new(SilentEmitter)), tcx.sess.psess.clone_source_map());
        parser.edition = tcx.sess.edition();
        let failed = |error: io::Error| span_lint(cx, ORPHAN_MODULE, tcx.def_span(CRATE_DEF_ID), error.to_string());
        let mut known = match pending.iter().map(|(_, path, siblings)| identity(path, siblings.unwrap_or_default())).collect::<io::Result<FxHashSet<_>>>() {
            Ok(known) => known,
            Err(error) => return failed(error),
        };
        let mut unseen = FxHashSet::default();
        while let Some((source_file, source_path, sibling_modules)) = pending.pop() {
            let Some(source) = &source_file.src else { continue };
            unseen.extend(tokenize_with_text(source).filter(|(kind, ..)| matches!(kind, TokenKind::Ident | TokenKind::RawIdent)).filter_map(|(_, text, span)| {
                let name = text.trim_prefix(RAW_IDENTIFIER_PREFIX);
                (!walk.seen.contains(&source_file.absolute_position(RelativeBytePos::from_usize(span.start.strict_add(text.len().strict_sub(name.len())))))).then(|| Symbol::intern(name))
            }));
            let Some(sibling_modules) = sibling_modules else { continue };
            let relative = source_path.file_stem().filter(|_| !sibling_modules).and_then(OsStr::to_str).map(Ident::from_str);
            let Some(declared) = module_paths(&parser, source_file.name.clone(), &source_path, relative, source) else { return };
            for (candidate, siblings) in declared {
                let id = match identity(&candidate, siblings) {
                    Ok(id) => id,
                    Err(error) if error.kind() == ErrorKind::NotFound => continue,
                    Err(error) => return failed(error),
                };
                if !known.insert(id) {
                    continue;
                }
                match map.load_file(&candidate) {
                    Ok(file) => pending.push((file, candidate, Some(siblings))),
                    Err(error) if error.kind() == ErrorKind::NotFound => (),
                    Err(error) => return failed(error),
                }
            }
        }
        Self::orphans(cx, &compiled, known.into_iter().map(|(path, _, _)| path));
        for (&target, sites) in &self.sites {
            let outside = || sites.iter().filter(move |&&(caller, _)| caller != target);
            if let Ok(caller) = outside().map(|&(caller, _)| caller).all_equal_value()
                && !is_entrypoint_fn(cx, caller.to_def_id())
                && !self.pinned.contains(&target)
                && !tcx.def_span(target).from_expansion()
                && !exported(cx, target)
                && !is_entrypoint_fn(cx, target.to_def_id())
                && !find_attr!(tcx, target, Used { .. })
                && let name = tcx.item_name(target)
                && !unseen.contains(&name)
                && walk.enclosing.get(&caller).is_some_and(|&enclosing| walk.deepest.get(&target).copied().unwrap_or_default().strict_add(enclosing) <= LEVELS)
                && let module = tcx.parent_module_from_def_id(caller)
                && (module == tcx.parent_module_from_def_id(target) || movable(cx, target, module))
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
