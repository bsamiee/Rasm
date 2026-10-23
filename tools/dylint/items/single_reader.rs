use core::slice;
use std::path::Path;

use clippy_utils::{diagnostics::span_lint_hir_and_then, is_entrypoint_fn, peel_blocks, peel_hir_expr_refs, tokenize_with_text};
use itertools::Itertools as _;
use rustc_attr_ir::find_attr;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap, FxIndexSet};
use rustc_hir::{
    Block, Expr, ExprKind, HirId, ImplItem, Item, Node, Pat, PatExprKind, PatKind, TraitItem,
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

lint!(default Pass; SINGLE_READER: "crate-private functions, constants, and statics with one reader in their module, other than the entry point");

impl Pass {
    fn reference(&mut self, cx: &LateContext<'_>, target: Option<DefId>, node: HirId, span: Span) {
        let tcx = cx.tcx;
        if let Some(target) = target
            .and_then(DefId::as_local)
            .filter(|&target| matches!(tcx.def_kind(target), DefKind::Fn | DefKind::AssocFn | DefKind::Const | DefKind::AssocConst | DefKind::Static { nested: false, .. }))
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
                match peel_blocks(&body.value) {
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
        struct Walk<'tcx> {
            tcx: TyCtxt<'tcx>,
            seen: FxHashSet<BytePos>,
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

            fn visit_item(&mut self, i: &'tcx Item<'tcx>) { self.owned(i.owner_id.def_id, |walk| intravisit::walk_item(walk, i)); }

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
        let map = tcx.sess.source_map();
        let root = map.lookup_source_file(tcx.def_span(CRATE_DEF_ID).lo());
        let local = |name: &FileName| if let FileName::Real(real) = name { real.local_path().map(Path::to_path_buf) } else { None };
        let mut pending = map.files().iter().filter(|file| file.is_real_file() && !file.is_imported()).cloned().collect::<Vec<_>>();
        let mut known = pending.iter().filter_map(|file| local(&file.name)).collect::<FxHashSet<_>>();
        let mut opaque = false;
        let mut mentions = FxHashMap::<Symbol, Vec<BytePos>>::default();
        while let Some(file) = pending.pop() {
            let (Some(source), Some(path)) = (file.src.as_deref(), local(&file.name)) else { continue };
            let tokens = tokenize_with_text(source).filter(|(kind, ..)| !matches!(kind, TokenKind::Whitespace | TokenKind::LineComment { .. } | TokenKind::BlockComment { .. })).collect::<Vec<_>>();
            let directory = path.parent().map_or_default(Path::to_path_buf);
            let nested = if file.start_pos == root.start_pos || path.file_stem().is_some_and(|stem| stem == kw::Mod.as_str()) { directory.clone() } else { path.with_extension("") };
            let mut renamed = None;
            let mut declared = FxIndexSet::default();
            for rest in (0..tokens.len()).filter_map(|start| tokens.get(start..)) {
                match rest {
                    [
                        (TokenKind::Pound, ..),
                        (TokenKind::OpenBracket, ..),
                        (TokenKind::Ident, attribute, _),
                        (TokenKind::Eq, ..),
                        (TokenKind::Literal { .. }, literal, _),
                        (TokenKind::CloseBracket, ..),
                        ..,
                    ] if *attribute == sym::path.as_str() => renamed = Some(literal.trim_matches('"').to_owned()),
                    [(TokenKind::Ident, keyword, _), (TokenKind::Ident, name, _), (TokenKind::Semi, ..), ..] if *keyword == kw::Mod.as_str() =>
                        declared.extend(renamed.take().map_or_else(|| vec![nested.join(format!("{name}.rs")), nested.join(name).join("mod.rs")], |renamed| vec![directory.join(renamed)])),
                    [(TokenKind::Ident, attribute, _), (TokenKind::OpenParen, ..), arguments @ ..] if *attribute == sym::cfg_attr.as_str() =>
                        opaque |=
                            arguments.iter().take_while(|(kind, ..)| !matches!(kind, TokenKind::CloseBracket)).any(|&(kind, text, _)| matches!(kind, TokenKind::Ident) && text == sym::path.as_str()),
                    [(TokenKind::Semi | TokenKind::OpenBrace | TokenKind::CloseBrace, ..), ..] => renamed = None,
                    _ => {}
                }
            }
            for candidate in declared.into_iter().filter(|candidate| map.file_exists(candidate) && !known.contains(candidate)).collect::<Vec<_>>() {
                match map.load_file(&candidate) {
                    Ok(loaded) => {
                        known.insert(candidate);
                        pending.push(loaded);
                    }
                    Err(_) => opaque = true,
                }
            }
            for (_, text, span) in tokens.iter().filter(|(kind, ..)| matches!(kind, TokenKind::Ident | TokenKind::RawIdent)) {
                let name = text.trim_prefix("r#");
                mentions.entry(Symbol::intern(name)).or_default().push(file.absolute_position(RelativeBytePos::from_usize(span.start.strict_add(text.len().strict_sub(name.len())))));
            }
        }
        if opaque {
            return;
        }
        let mut walk = Walk { tcx, seen: FxHashSet::default(), outermost: None, depth: 0, enclosing: FxHashMap::default(), deepest: FxHashMap::default() };
        tcx.hir_walk_toplevel_module(&mut walk);
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
                && !(tcx.def_kind(target) == DefKind::AssocConst && tcx.generics_of(target).requires_monomorphization(tcx))
                && let parent = tcx.local_parent(target)
                && (tcx.def_kind(parent) == DefKind::Mod || matches!(tcx.def_kind(parent), DefKind::Impl { of_trait: false }) && tcx.def_kind(tcx.local_parent(parent)) == DefKind::Mod)
                && let name = tcx.item_name(target)
                && mentions.get(&name).is_some_and(|positions| positions.iter().all(|position| walk.seen.contains(position)))
                && walk.enclosing.get(&caller).is_some_and(|&enclosing| walk.deepest.get(&target).copied().unwrap_or_default().strict_add(enclosing) <= LEVELS)
            {
                let caller_name = tcx.item_name(caller);
                span_lint_hir_and_then(cx, SINGLE_READER, tcx.local_def_id_to_hir_id(target), tcx.def_span(target), format!("`{name}` has one reader, `{caller_name}`"), |diag| {
                    diag.span_note(outside().map(|&(_, site)| site).collect::<Vec<_>>(), "read here");
                    diag.help(format!("move `{name}` into `{caller_name}` as a nested item"));
                });
            }
        }
    }
}
