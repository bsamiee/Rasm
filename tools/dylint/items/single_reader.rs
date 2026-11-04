use core::slice;
use std::{
    io::{self, ErrorKind},
    path::{self, Path},
    sync::Arc,
};

use clippy_utils::{diagnostics::span_lint_hir_and_then, is_entrypoint_fn, peel_blocks, peel_hir_expr_refs, tokenize_with_text};
use itertools::{Either, Itertools as _};
use litrs::StringLit;
use rustc_attr_ir::find_attr;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_hir::{
    Block, Expr, ExprKind, HirId, ImplItem, Item, ItemKind, Node, Pat, PatExprKind, PatKind, TraitItem,
    def::DefKind,
    def_id::{CRATE_DEF_ID, DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lexer::TokenKind;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{hir::nested_filter, ty::TyCtxt};
use rustc_span::{BytePos, FileName, Ident, Pos as _, RelativeBytePos, Span, Symbol, kw, sym};

use crate::{expanded_externally, exported, fn_item_value, generated, parameter};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    sites: FxIndexMap<LocalDefId, Vec<(LocalDefId, Span)>>,
    pinned: FxHashSet<LocalDefId>,
}

lint!(default Pass; SINGLE_READER: "crate-private free functions, constants, and statics with one reader in their module, other than the entry point");

impl Pass {
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
            paths: FxHashSet<BytePos>,
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
                    && find_attr!(self.tcx, i.hir_id(), Path(..))
                    && let file = self.tcx.sess.source_map().lookup_source_file(module.spans.inner_span.lo())
                    && !file.contains(i.span.lo())
                {
                    self.paths.insert(file.start_pos);
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
        let mut walk = Walk { tcx, seen: FxHashSet::default(), paths: FxHashSet::default(), outermost: None, depth: 0, enclosing: FxHashMap::default(), deepest: FxHashMap::default() };
        tcx.hir_walk_toplevel_module(&mut walk);
        let map = tcx.sess.source_map();
        let root = map.lookup_source_file(tcx.def_span(CRATE_DEF_ID).lo());
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
        let mut pending = map
            .files()
            .iter()
            .filter(|file| file.is_real_file() && !file.is_imported())
            .filter_map(|file| {
                let path = local(&file.name)?;
                let siblings = walk.paths.contains(&file.start_pos) || file.start_pos == root.start_pos || mod_file(&path);
                Some((Arc::clone(file), path, siblings))
            })
            .collect::<Vec<_>>();
        let mut known = FxHashSet::default();
        let mut opaque = false;
        for (_, path, siblings) in &pending {
            if let Ok(id) = identity(path, *siblings) {
                known.insert(id);
            } else {
                opaque = true;
                break;
            }
        }
        if opaque {
            return;
        }
        let mut unseen = FxHashSet::default();
        while let Some((source_file, source_path, sibling_modules)) = pending.pop() {
            let Some(source) = &source_file.src else { continue };
            let tokens = tokenize_with_text(source).filter(|(kind, ..)| !matches!(kind, TokenKind::Whitespace | TokenKind::LineComment { .. } | TokenKind::BlockComment { .. })).collect::<Vec<_>>();
            let directory = source_path.parent().map_or_default(Path::to_path_buf);
            let nested = if sibling_modules { directory.clone() } else { source_path.with_extension("") };
            let mut renamed = None;
            let mut declared = FxIndexMap::default();
            let mut scopes = Vec::new();
            let mut depth = 0_usize;
            let mut entering = None;
            for rest in (0..tokens.len()).filter_map(|start| tokens.get(start..)) {
                let (directory, nested) = scopes.last().map_or((&directory, &nested), |(_, path)| (path, path));
                match rest {
                    [
                        (TokenKind::Pound, ..),
                        (TokenKind::OpenBracket, ..),
                        (TokenKind::Ident, attribute, _),
                        (TokenKind::Eq, ..),
                        (TokenKind::Literal { .. }, literal, _),
                        (TokenKind::CloseBracket, ..),
                        ..,
                    ] if *attribute == sym::path.as_str() => {
                        renamed = StringLit::parse(*literal).inspect_err(|_| opaque = true).ok().map(StringLit::into_value);
                    }
                    [(TokenKind::Ident, keyword, _), (TokenKind::Ident | TokenKind::RawIdent, name, _), (TokenKind::Semi, ..), ..] if *keyword == kw::Mod.as_str() => declared.extend(
                        renamed
                            .take()
                            .map_or_else(
                                || {
                                    Either::Left([
                                        (nested.join(format!("{}.rs", name.trim_prefix(RAW_IDENTIFIER_PREFIX))), false),
                                        (nested.join(name.trim_prefix(RAW_IDENTIFIER_PREFIX)).join("mod.rs"), false),
                                    ])
                                },
                                |renamed| Either::Right([(directory.join(&*renamed), true)]),
                            )
                            .into_iter(),
                    ),
                    [(TokenKind::Ident, keyword, _), (TokenKind::Ident | TokenKind::RawIdent, name, _), (TokenKind::OpenBrace, ..), ..] if *keyword == kw::Mod.as_str() =>
                        entering = Some(renamed.take().map_or_else(|| nested.join(name.trim_prefix(RAW_IDENTIFIER_PREFIX)), |renamed| directory.join(&*renamed))),
                    [(TokenKind::Ident, attribute, _), (TokenKind::OpenParen, ..), arguments @ ..] if *attribute == sym::cfg_attr.as_str() =>
                        opaque |=
                            arguments.iter().take_while(|(kind, ..)| !matches!(kind, TokenKind::CloseBracket)).any(|&(kind, text, _)| matches!(kind, TokenKind::Ident) && text == sym::path.as_str()),
                    [(TokenKind::OpenBrace, ..), ..] => {
                        depth = depth.strict_add(1);
                        scopes.extend(entering.take().map(|path| (depth, path)));
                        renamed = None;
                    }
                    [(TokenKind::CloseBrace, ..), ..] if let Some(outer) = depth.checked_sub(1) => {
                        scopes.pop_if(|(entered, _)| *entered == depth);
                        depth = outer;
                        renamed = None;
                    }
                    [(TokenKind::CloseBrace, ..), ..] => {
                        opaque = true;
                        break;
                    }
                    [(TokenKind::Semi, ..), ..] => renamed = None,
                    _ => {}
                }
            }
            pending.extend(declared.into_iter().filter_map(|(candidate, overridden)| {
                let siblings = overridden || mod_file(&candidate);
                let loaded = identity(&candidate, siblings).and_then(|id| if known.insert(id) { map.load_file(&candidate).map(Some) } else { Ok(None) });
                match loaded {
                    Ok(Some(file)) => Some((file, candidate, siblings)),
                    Ok(None) => None,
                    Err(error) if error.kind() == ErrorKind::NotFound => None,
                    Err(_) => {
                        opaque = true;
                        None
                    }
                }
            }));
            for (_, text, span) in tokens.iter().filter(|(kind, ..)| matches!(kind, TokenKind::Ident | TokenKind::RawIdent)) {
                let name = text.trim_prefix(RAW_IDENTIFIER_PREFIX);
                if !walk.seen.contains(&source_file.absolute_position(RelativeBytePos::from_usize(span.start.strict_add(text.len().strict_sub(name.len()))))) {
                    unseen.insert(Symbol::intern(name));
                }
            }
        }
        if opaque {
            return;
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
