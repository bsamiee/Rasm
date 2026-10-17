use core::{iter, mem};

use clippy_utils::{
    consts::{ConstEvalCtxt, Constant},
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_parent_expr, is_in_test,
    macros::root_macro_call,
    paths::PathNS,
    peel_blocks, peel_hir_expr_while,
    res::MaybeDef as _,
    sym,
};
use itertools::Itertools as _;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashSet, FxIndexMap, FxIndexSet};
use rustc_hir::{
    BinOpKind, Expr, ExprKind, LetStmt, MatchSource, Pat, PatKind, UnOp,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, AdtDef, TyCtxt};
use rustc_span::{Span, Symbol};

use crate::{Access, Place, access, constructed, fn_value, is_text, paths, place};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    STRING_TAG,
    Warn,
    "text fields, statics, parameters, and locals branched on by string literal, and text-keyed maps and sets addressed by string literal keys, where an enum or a struct states the closed set"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Default)]
struct Domain {
    tags: FxIndexSet<Symbol>,
    open: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct StringTag {
    collections: FxHashSet<DefId>,
    branches: FxIndexMap<Place, Vec<(Span, FxIndexSet<Symbol>)>>,
    written: FxIndexMap<Place, Domain>,
    keyed: FxIndexMap<Place, Vec<(Span, Symbol)>>,
}

impl_lint_pass!(StringTag => [STRING_TAG]);

impl StringTag {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            collections: paths(
                tcx,
                PathNS::Type,
                &["std::collections::HashMap", "std::collections::BTreeMap", "std::collections::HashSet", "std::collections::BTreeSet", "indexmap::IndexMap", "indexmap::IndexSet"],
            ),
            branches: FxIndexMap::default(),
            written: FxIndexMap::default(),
            keyed: FxIndexMap::default(),
        }
    }

    fn write<'tcx>(&mut self, cx: &LateContext<'tcx>, written: Place, value: &'tcx Expr<'tcx>) {
        let domain = self.written.entry(written).or_default();
        match tag(cx, value) {
            Some(tag) => {
                domain.tags.insert(tag);
            }
            None => domain.open = true,
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for StringTag {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn decoded<'tcx>(cx: &LateContext<'tcx>, body: &'tcx Expr<'tcx>) -> bool {
            let typeck = cx.typeck_results();
            let wrappers = [LangItem::OptionSome, LangItem::ResultOk].map(|wrapper| cx.tcx.lang_items().get(wrapper));
            let value = peel_hir_expr_while(peel_blocks(body), |inner| match inner.kind {
                ExprKind::Ret(Some(returned)) | ExprKind::Break(_, Some(returned)) => Some(peel_blocks(returned)),
                ExprKind::Call(_, [argument]) if fn_def_id(cx, inner).is_some_and(|ctor| wrappers.contains(&Some(cx.tcx.parent(ctor)))) => Some(argument),
                _ => None,
            });
            matches!(value.kind, ExprKind::Lit(_))
                || constructed(typeck, value).is_some() && typeck.expr_ty(value).ty_adt_def().is_some_and(AdtDef::is_enum)
                || matches!(&value.kind, ExprKind::Path(path) if matches!(typeck.qpath_res(path, value.hir_id), Res::Def(DefKind::Const | DefKind::AssocConst, _)))
        }

        fn tags(cx: &LateContext<'_>, pat: &Pat<'_>) -> Option<FxIndexSet<Symbol>> {
            match pat.kind {
                PatKind::Expr(expr) if let Some(Constant::Str(text)) = ConstEvalCtxt::new(cx).eval_pat_expr(expr) => Some(FxIndexSet::from_iter([Symbol::intern(&text)])),
                PatKind::Or(alternatives) => alternatives.iter().map(|alternative| tags(cx, alternative)).try_fold(FxIndexSet::default(), |mut all, listed| {
                    all.extend(listed?);
                    Some(all)
                }),
                PatKind::Ref(inner, ..) | PatKind::Deref(inner) => tags(cx, inner),
                _ => None,
            }
        }

        let typeck = cx.typeck_results();
        let site = expr.span.source_callsite();
        let mut branch = |subject: &'tcx Expr<'tcx>, span: Span, tags: FxIndexSet<Symbol>| {
            if !tags.is_empty()
                && let Some(held) = place(cx, peel_hir_expr_while(subject, |inner| text_view(cx, inner)))
            {
                self.branches.entry(held).or_default().push((span, tags));
            }
        };
        if (!expr.span.from_expansion() || root_macro_call(expr.span).is_some_and(|call| cx.tcx.is_diagnostic_item(sym::matches_macro, call.def_id))) && !is_in_test(cx.tcx, expr.hir_id) {
            if let ExprKind::Match(scrutinee, arms, MatchSource::Normal) = expr.kind
                && is_text(cx, typeck.expr_ty(scrutinee))
                && let tagged = arms.iter().filter_map(|arm| Some((tags(cx, arm.pat)?, arm.body))).collect::<Vec<_>>()
                && !tagged.iter().all(|(_, body)| decoded(cx, body))
            {
                branch(scrutinee, site, tagged.into_iter().flat_map(|(tags, _)| tags).collect());
            }
            if let ExprKind::Binary(operator, left, right) = expr.kind
                && matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne)
                && let Some((subject, compared)) = [(left, right), (right, left)].into_iter().find_map(|(subject, other)| Some((subject, tag(cx, other)?)))
                && is_text(cx, typeck.expr_ty(subject))
                && !get_parent_expr(cx, expr)
                    .and_then(|condition| if let ExprKind::DropTemps(_) = condition.kind { get_parent_expr(cx, condition) } else { Some(condition) })
                    .is_some_and(|conditional| matches!(conditional.kind, ExprKind::If(_, then, _) if decoded(cx, then)))
            {
                branch(subject, site, FxIndexSet::from_iter([compared]));
            }
            if let ExprKind::MethodCall(segment, receiver, [subject], _) = expr.kind
                && segment.ident.name == sym::contains
                && let ExprKind::Array(items) = receiver.peel_borrows().kind
                && let Some(listed) = items.iter().map(|item| tag(cx, item)).collect::<Option<FxIndexSet<_>>>()
                && is_text(cx, typeck.expr_ty(subject))
            {
                branch(subject, site, listed);
            }
            if let ExprKind::MethodCall(_, collection, [key, ..], _) | ExprKind::Index(collection, key, _) = expr.kind
                && let ty::Adt(keyed, args) = typeck.expr_ty_adjusted(collection).peel_refs().kind()
                && self.collections.contains(&keyed.did())
                && is_text(cx, args.type_at(0))
                && let Some(literal) = tag(cx, key)
                && let Some(held) =
                    place(cx, peel_hir_expr_while(collection, |inner| if let ExprKind::AddrOf(_, _, operand) | ExprKind::Unary(UnOp::Deref, operand) = inner.kind { Some(operand) } else { None }))
            {
                self.keyed.entry(held).or_default().push((key.span, literal));
            }
        }
        if let Some((_, fields)) = constructed(typeck, expr) {
            let written = fields
                .into_iter()
                .filter(|(field, _)| is_text(cx, cx.tcx.type_of(field.did).instantiate_identity().skip_normalization()))
                .filter_map(|(field, value)| Some((field.did.as_local()?, value)));
            for (field, value) in written {
                self.write(cx, Place::Field(field), value);
            }
        }
        if let Some(held) = place(cx, expr).filter(|_| is_text(cx, typeck.expr_ty(expr))) {
            match access(cx, expr) {
                Access::Assigned(value) => self.write(cx, held, value),
                Access::Mutated => self.written.entry(held).or_default().open = true,
                Access::Copied | Access::Borrowed | Access::Moved => {}
            }
        }
        if let ExprKind::MethodCall(_, receiver, arguments, _) | ExprKind::Call(receiver, arguments) = expr.kind
            && let Some(body) = fn_def_id(cx, expr).and_then(DefId::as_local).and_then(|callee| cx.tcx.hir_maybe_body_owned_by(callee))
        {
            for (parameter, argument) in iter::zip(body.params.iter().rev(), iter::chain([receiver], arguments).rev()) {
                self.write(cx, Place::Parameter(parameter.pat.hir_id), argument);
            }
        }
        if let Some(body) = fn_value(cx, expr).and_then(DefId::as_local).and_then(|function| cx.tcx.hir_maybe_body_owned_by(function)) {
            for parameter in body.params {
                self.written.entry(Place::Parameter(parameter.pat.hir_id)).or_default().open = true;
            }
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && let Some(init) = local.init
        {
            self.write(cx, Place::Local(binding), init);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let written = mem::take(&mut self.written);
        for (held, branches) in mem::take(&mut self.branches) {
            let compared = branches.iter().flat_map(|(_, tags)| tags.iter().copied()).collect::<FxIndexSet<_>>();
            let closable = match held {
                Place::Field(item) | Place::Static(item) => !cx.effective_visibilities.is_exported(item),
                Place::Parameter(binding) => {
                    let owner = cx.tcx.hir_enclosing_body_owner(binding);
                    matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
                        && cx.tcx.trait_of_assoc(owner.to_def_id()).is_none()
                        && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none()
                        && !cx.effective_visibilities.is_exported(owner)
                }
                Place::Local(_) => true,
            };
            let closed = written.get(&held).filter(|domain| closable && !domain.open && !domain.tags.is_empty());
            let tags = closed.map_or_else(|| compared.clone(), |domain| compared.union(&domain.tags).copied().collect());
            let stored = matches!(held, Place::Field(_) | Place::Static(_));
            if tags.len() > 1 && (closed.is_some() || stored && compared.len() > 1) {
                let listed = tags.iter().format_with(", ", |tag, write| write(&format_args!("`\"{tag}\"`")));
                let name = held.name(cx);
                let message = match held {
                    Place::Field(_) => format!("field `{name}` holds one of the tags {listed} as text"),
                    Place::Static(_) => format!("static `{name}` holds one of the tags {listed} as text"),
                    Place::Parameter(_) | Place::Local(_) => format!("`{name}` holds one of the tags {listed} as text"),
                };
                span_lint_hir_and_then(cx, STRING_TAG, held.hir_id(cx), held.span(cx), message, |diag| {
                    diag.span_note(branches.iter().map(|(site, _)| *site).collect::<Vec<_>>(), "branched on by its text here");
                    diag.help("declare an enum with a variant per tag deriving `strum::EnumString`, parse the text once where it enters, and match the variants");
                });
            }
        }
        for (held, keys) in mem::take(&mut self.keyed) {
            let literals = keys.iter().map(|&(_, literal)| literal).collect::<FxIndexSet<_>>();
            if literals.len() > 1 {
                let listed = literals.iter().format_with(", ", |literal, write| write(&format_args!("`\"{literal}\"`")));
                span_lint_hir_and_then(cx, STRING_TAG, held.hir_id(cx), held.span(cx), format!("`{}` is keyed by text and addressed by the fixed keys {listed}", held.name(cx)), |diag| {
                    diag.span_note(keys.iter().map(|&(site, _)| site).collect::<Vec<_>>(), "addressed by a string literal here");
                    diag.help("key the collection by an enum with a variant per key, or hold a struct with a field per key");
                });
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn text_view<'tcx>(cx: &LateContext<'tcx>, inner: &'tcx Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
    let typeck = cx.typeck_results();
    match inner.kind {
        ExprKind::AddrOf(_, _, operand) | ExprKind::Unary(_, operand) => Some(operand),
        ExprKind::MethodCall(_, receiver, [], _) if is_text(cx, typeck.expr_ty(receiver)) && is_text(cx, typeck.expr_ty(inner)) => Some(receiver),
        ExprKind::Call(_, [argument])
            if is_text(cx, typeck.expr_ty(argument))
                && fn_def_id(cx, inner)
                    .and_then(|conversion| cx.tcx.trait_of_assoc(conversion))
                    .is_some_and(|conversion| [sym::From, sym::Into].into_iter().any(|owner| conversion.is_diag_item(cx, owner))) =>
            Some(argument),
        _ => None,
    }
}

fn tag<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> Option<Symbol> {
    if let Some(Constant::Str(text)) = ConstEvalCtxt::new(cx).eval(peel_hir_expr_while(peel_blocks(value), |inner| text_view(cx, inner))) { Some(Symbol::intern(&text)) } else { None }
}
