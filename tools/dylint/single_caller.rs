use std::path::Path;

use clippy_utils::{diagnostics::span_lint_hir_and_then, is_entrypoint_fn, tokenize_with_text};
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap, FxIndexSet};
use rustc_hir::{
    Expr, ExprKind, HirId, Node, Pat, PatExprKind, PatKind,
    def::DefKind,
    def_id::{CRATE_DEF_ID, DefId, LocalDefId},
    intravisit::Visitor,
};
use rustc_lexer::TokenKind;
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::{hir::nested_filter, ty::TyCtxt};
use rustc_span::{BytePos, FileName, Ident, Pos as _, RelativeBytePos, Span, Symbol, kw, sym};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    SINGLE_CALLER,
    Warn,
    "crate-private functions, constants, and statics one item other than the entry point reads, which belong inside that item"
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct SingleCaller {
    sites: FxIndexMap<LocalDefId, Vec<(LocalDefId, Span)>>,
    expanded: FxHashSet<LocalDefId>,
}

impl_lint_pass!(SingleCaller => [SINGLE_CALLER]);

impl SingleCaller {
    fn reference(&mut self, cx: &LateContext<'_>, target: Option<LocalDefId>, node: HirId, span: Span) {
        if let Some(target) = target.filter(|&target| matches!(cx.tcx.def_kind(target), DefKind::Fn | DefKind::AssocFn | DefKind::Const | DefKind::Static { safety: _, mutability: _, nested: false }))
        {
            if span.from_expansion() {
                self.expanded.insert(target);
            } else {
                self.sites.entry(target).or_default().push((cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(node)), span));
            }
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for SingleCaller {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let target = if let ExprKind::Path(qpath) = &expr.kind {
            cx.qpath_res(qpath, expr.hir_id).opt_def_id()
        } else if let ExprKind::MethodCall(..) = expr.kind {
            cx.typeck_results().type_dependent_def_id(expr.hir_id)
        } else {
            None
        };
        self.reference(cx, target.and_then(DefId::as_local), expr.hir_id, expr.span);
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::Expr(pattern) = pat.kind
            && let PatExprKind::Path(qpath) = &pattern.kind
        {
            self.reference(cx, cx.qpath_res(qpath, pattern.hir_id).opt_def_id().and_then(DefId::as_local), pat.hir_id, pat.span);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        struct Idents<'tcx> {
            tcx: TyCtxt<'tcx>,
            seen: FxHashSet<BytePos>,
        }
        impl<'tcx> Visitor<'tcx> for Idents<'tcx> {
            type NestedFilter = nested_filter::All;

            fn maybe_tcx(&mut self) -> TyCtxt<'tcx> { self.tcx }

            fn visit_ident(&mut self, ident: Ident) { self.seen.insert(ident.span.lo()); }
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
            let tokens = tokenize_with_text(source)
                .filter(|(kind, ..)| !matches!(kind, TokenKind::Whitespace | TokenKind::LineComment { doc_style: _ } | TokenKind::BlockComment { doc_style: _, terminated: _ }))
                .collect::<Vec<_>>();
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
                        (TokenKind::Literal { kind: _, suffix_start: _ }, literal, _),
                        (TokenKind::CloseBracket, ..),
                        ..,
                    ] if *attribute == sym::path.as_str() => renamed = Some(literal.trim_matches('"').to_owned()),
                    [(TokenKind::Ident, keyword, _), (TokenKind::Ident, name, _), (TokenKind::Semi, ..), ..] if *keyword == kw::Mod.as_str() =>
                        declared.extend(renamed.take().map_or_else(|| vec![nested.join(format!("{name}.rs")), nested.join(name).join("mod.rs")], |renamed| vec![directory.join(renamed)])),
                    [(TokenKind::Ident, attribute, _), (TokenKind::OpenParen, ..), arguments @ ..] if *attribute == sym::cfg_attr.as_str() =>
                        opaque |= arguments.iter().take_while(|(kind, ..)| !matches!(kind, TokenKind::CloseBracket)).any(|&(kind, text, _)| matches!(kind, TokenKind::Ident) && text == sym::path.as_str()),
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
        let mut idents = Idents { tcx, seen: FxHashSet::default() };
        tcx.hir_walk_toplevel_module(&mut idents);
        for (&target, sites) in &self.sites {
            let outside = sites.iter().filter(|&&(caller, _)| caller != target).collect::<Vec<_>>();
            if let [(caller, _), rest @ ..] = &outside
                && rest.iter().all(|(other, _)| other == caller)
                && !is_entrypoint_fn(cx, caller.to_def_id())
                && !self.expanded.contains(&target)
                && !tcx.def_span(target).from_expansion()
                && !cx.effective_visibilities.is_exported(target)
                && tcx.parent_module_from_def_id(*caller) == tcx.parent_module_from_def_id(target)
                && matches!(tcx.hir_node_by_def_id(target), Node::Item(_) | Node::ImplItem(_))
                && (tcx.def_kind(target) != DefKind::AssocFn || tcx.inherent_impl_of_assoc(target.to_def_id()).is_some())
                && !is_entrypoint_fn(cx, target.to_def_id())
                && let parent = tcx.local_parent(target)
                && (tcx.def_kind(parent) == DefKind::Mod || matches!(tcx.def_kind(parent), DefKind::Impl { of_trait: false }) && tcx.def_kind(tcx.local_parent(parent)) == DefKind::Mod)
                && let name = tcx.item_name(target)
                && mentions.get(&name).is_some_and(|positions| positions.iter().all(|position| idents.seen.contains(position)))
            {
                let caller_name = tcx.item_name(*caller);
                span_lint_hir_and_then(cx, SINGLE_CALLER, tcx.local_def_id_to_hir_id(target), tcx.def_span(target), format!("`{name}` has one reader, `{caller_name}`"), |diag| {
                    diag.span_note(outside.iter().map(|&&(_, site)| site).collect::<Vec<_>>(), "read here");
                    diag.help(format!("move `{name}` into `{caller_name}` as a nested item"));
                });
            }
        }
    }
}
