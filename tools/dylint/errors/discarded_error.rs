use core::ops::ControlFlow;
use std::borrow::Cow;

use clippy_utils::{
    ExprUseNode, contains_return,
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_enclosing_block, get_expr_use_site, get_parent_expr, is_from_proc_macro, is_inside_always_const_context,
    macros::root_macro_call,
    paths::PathNS,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{get_iterator_item_ty, is_copy},
    usage::local_used_after_expr,
    visitors::{contains_break_or_continue, find_all_ret_expressions, for_each_expr_without_closures, is_const_evaluatable, is_local_used},
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    Block, Expr, ExprKind, HirId, LetStmt, LocalSource, MatchSource, Pat, PatKind, Stmt, StmtKind,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::Span;

use super::{body_output, has_single_failure_cause, singular, unread, untyped_kind};
use crate::{associated_trait, expanded_externally, fn_item_value, impl_owner, is_error, path, paths, pointee, resolve, result_parts};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Discard {
    Fallback,
    Success,
    Absence,
    Unmatched,
    View,
    Iterated,
    Flattened,
    Statement,
    Unnamed,
}

#[derive(Clone, Copy)]
enum Gate {
    Always,
    Unread,
    Succeeds,
    Relayed,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    exit_code: Option<DefId>,
    methods: FxHashMap<DefId, Option<(Discard, Gate)>>,
    views: FxHashSet<DefId>,
    query_results: FxHashSet<DefId>,
    err: Option<DefId>,
    inspect_err: Option<DefId>,
}

lint! {
    Pass;
    DISCARDED_ERROR:
        "`Result` errors replaced by a fallback, success, or `None`, skipped by a pattern or iteration, tested into a `bool` outside a body returning `bool`, or dropped by a statement or `Err(_)`",
}

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let methods = [
            (
                [
                    "core::result::Result::unwrap_or",
                    "core::option::Option::unwrap_or",
                    "core::result::Result::unwrap_or_default",
                    "core::option::Option::unwrap_or_default",
                    "core::result::Result::map_or",
                    "core::option::Option::map_or",
                    "core::result::Result::map_or_default",
                    "core::option::Option::map_or_default",
                ]
                .as_slice(),
                Some((Discard::Fallback, Gate::Always)),
            ),
            (
                &["core::result::Result::unwrap_or_else", "core::option::Option::unwrap_or_else", "core::result::Result::map_or_else", "core::option::Option::map_or_else"],
                Some((Discard::Fallback, Gate::Unread)),
            ),
            (
                &[
                    "core::result::Result::or",
                    "core::option::Option::or",
                    "core::option::Option::xor",
                    "core::option::Option::reduce",
                    "core::result::Result::or_else",
                    "core::option::Option::or_else",
                    "core::option::Option::get_or_try_insert_with",
                ],
                Some((Discard::Success, Gate::Succeeds)),
            ),
            (&["core::result::Result::ok"], Some((Discard::Absence, Gate::Relayed))),
            (
                &[
                    "core::result::Result::is_ok",
                    "core::option::Option::is_some",
                    "core::result::Result::is_err",
                    "core::option::Option::is_none",
                    "core::result::Result::is_ok_and",
                    "core::option::Option::is_some_and",
                    "core::option::Option::is_none_or",
                ],
                Some((Discard::View, Gate::Always)),
            ),
            (&["core::result::Result::is_err_and"], Some((Discard::View, Gate::Unread))),
            (
                &["core::result::Result::iter", "core::option::Option::iter", "core::result::Result::iter_mut", "core::option::Option::iter_mut", "core::iter::IntoIterator::into_iter"],
                Some((Discard::Iterated, Gate::Always)),
            ),
            (&["tokio::sync::broadcast::Sender::send"], None),
        ];
        Self {
            exit_code: path(tcx, PathNS::Type, "std::process::ExitCode"),
            methods: resolve(tcx, PathNS::Value, &methods.iter().flat_map(|&(listed, classification)| listed.iter().map(move |&method| (method, classification))).collect::<Vec<_>>()),
            views: paths(tcx, PathNS::Value, &["core::result::Result::as_ref", "core::result::Result::as_mut", "core::result::Result::as_deref", "core::result::Result::as_deref_mut"]),
            query_results: paths(tcx, PathNS::Type, &["itertools::structs::ExactlyOneError", "itertools::structs::AllEqualValueError"]),
            err: path(tcx, PathNS::Value, "core::result::Result::err"),
            inspect_err: path(tcx, PathNS::Value, "core::result::Result::inspect_err"),
        }
    }

    fn query_result<'tcx>(&self, cx: &LateContext<'tcx>, error: Ty<'tcx>) -> bool { pointee(cx, error).opt_def_id().is_some_and(|result| self.query_results.contains(&result)) }

    fn failing<'tcx>(&self, cx: &LateContext<'tcx>, error: Ty<'tcx>) -> bool { !self.query_result(cx, error) && super::failing(cx, error) }

    fn publication(&self, cx: &LateContext<'_>, expr: &Expr<'_>) -> bool { matches!(fn_def_id(cx, expr).and_then(|method| self.methods.get(&method)), Some(None)) }

    fn failure<'tcx>(&self, cx: &LateContext<'tcx>, held: &'tcx Expr<'tcx>, site: &Expr<'_>) -> Option<Ty<'tcx>> {
        let typeck = cx.typeck_results();
        let viewed = if let ExprKind::MethodCall(_, base, [], _) = held.kind
            && fn_def_id(cx, held).is_some_and(|view| self.views.contains(&view))
        {
            base
        } else {
            held
        };
        let error = result_parts(cx, typeck.expr_ty_adjusted(held).peel_refs())?.1;
        (self.failing(cx, error) && fn_def_id(cx, held).is_none_or(|called| Some(called) != self.inspect_err) && viewed.res_local_id().is_none_or(|local| !local_used_after_expr(cx, local, site)))
            .then_some(error)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn skipped(cx: &LateContext<'_>, pattern: &Pat<'_>, struct_rest: bool) -> bool {
    let mut found = false;
    pattern.walk(|branch| {
        !found
            && match branch.kind {
                PatKind::Or(_) => true,
                PatKind::TupleStruct(qpath, fields, _) => {
                    found = (matches!(fields, [] | [Pat { kind: PatKind::Wild | PatKind::Binding(_, _, _, None), .. }])
                        || struct_rest && matches!(fields, [Pat { kind: PatKind::Struct(whole, [], Some(_)), hir_id, .. }] if !matches!(cx.qpath_res(whole, *hir_id), Res::Def(DefKind::Variant, _))))
                        && cx.qpath_res(&qpath, branch.hir_id).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr);
                    false
                }
                _ => false,
            }
    });
    found
}

fn report<'tcx>(cx: &LateContext<'tcx>, node: HirId, span: Span, discard: Discard, label: &str, error: Ty<'tcx>) {
    const HANDLED: &str = "match the `Result`, handle each error the caller acts on, and return the rest with `?`";
    let error = with_forced_trimmed_paths!(error.to_string());
    let (message, help) = match discard {
        Discard::Fallback => (format!("`{label}` replaces `{error}` with a fallback value"), Cow::Borrowed(HANDLED)),
        Discard::Success => (format!("`{label}` turns `{error}` into success"), Cow::Borrowed(HANDLED)),
        Discard::Absence => (format!("`{label}` turns `{error}` into `None`"), Cow::Borrowed("return the `Result` with `?`, or convert its error into a caller variant with `map_err`")),
        Discard::Unmatched => (format!("pattern of `{label}` leaves `{error}` unmatched"), Cow::Borrowed(HANDLED)),
        Discard::View => (format!("`{label}` tests the `Result` and drops `{error}`"), Cow::Borrowed("match the `Result` and handle `Err` in place of the test")),
        Discard::Iterated => (format!("`{label}` iterates the `Result` and skips its `{error}`"), Cow::Borrowed(HANDLED)),
        Discard::Flattened =>
            (format!("`{label}` iterates results and skips each `{error}`"), Cow::Owned(format!("collect the results into `validated::Validated<Vec<_>, {error}>` and call `.ok()`"))),
        Discard::Statement => (format!("`{label}` drops a `Result` and its unhandled `{error}`"), Cow::Borrowed("match the `Result`, or return it with `?`")),
        Discard::Unnamed => (format!("`{label}` drops `{error}` without naming its type"), Cow::Borrowed("match the `Err` value by its type, `Err(<Type> { .. })`")),
    };
    span_lint_hir_and_then(cx, DISCARDED_ERROR, node, span, message, |diag| {
        diag.help(help);
    });
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let constructs = |pattern: &Pat<'_>, item: LangItem| matches!(pattern.kind, PatKind::TupleStruct(qpath, ..) if cx.qpath_res(&qpath, pattern.hir_id).ctor_parent(cx).is_lang_item(cx, item));
        let idle = |body: &Expr<'_>| match peel_blocks(body).kind {
            ExprKind::Block(Block { stmts: [], expr: None, .. }, _) | ExprKind::Tup([]) | ExprKind::Continue(_) | ExprKind::Ret(None) => true,
            ExprKind::Block(Block { stmts: [Stmt { kind: StmtKind::Semi(left), .. }], expr: None, .. }, _) => matches!(left.kind, ExprKind::Continue(_) | ExprKind::Ret(None)),
            _ => false,
        };
        let fallback = |body: &'tcx Expr<'tcx>| is_const_evaluatable(cx.tcx, typeck, body) || fn_def_id(cx, peel_blocks(body)).is_diag_item(cx, sym::default_fn);
        let gated = |gate, receiver: &'tcx Expr<'tcx>, arguments: &'tcx [Expr<'tcx>], expr: &'tcx Expr<'tcx>| match gate {
            Gate::Always => true,
            Gate::Unread => arguments.first().is_some_and(|handler| unread(cx, handler).is_some()) || typeck.expr_ty_adjusted(receiver).peel_refs().is_diag_item(cx, sym::Option),
            Gate::Succeeds => arguments.first().is_some_and(|value| {
                let body = unread(cx, value).unwrap_or_else(|| peel_blocks(value));
                if typeck.expr_ty_adjusted(receiver).peel_refs().is_diag_item(cx, sym::Option) {
                    !find_all_ret_expressions(cx, body, |leaf| {
                        matches!(leaf.kind, ExprKind::Path(qpath) if cx.qpath_res(&qpath, leaf.hir_id).ctor_parent(cx).is_lang_item(cx, LangItem::OptionNone))
                            || matches!(leaf.kind, ExprKind::Call(_, [argument])
                                if fn_def_id(cx, leaf).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr)
                                    && result_parts(cx, typeck.expr_ty(leaf)).is_some_and(|(_, translated)| is_error(cx, translated) && untyped_kind(cx, translated).is_none())
                                    && !contains_return(argument)
                                    && !contains_break_or_continue(argument))
                    })
                } else {
                    fn_def_id(cx, body).ctor_parent(cx).is_lang_item(cx, LangItem::ResultOk)
                }
            }),
            Gate::Relayed => {
                let parent = get_parent_expr(cx, expr);
                let returned = matches!(get_expr_use_site(cx.tcx, typeck, expr.span.ctxt(), expr).use_node(cx), ExprUseNode::Return(_));
                let branch = parent.filter(|parent| matches!(parent.kind, ExprKind::Call(..)) && fn_def_id(cx, parent).is_lang_item(cx, LangItem::TryTraitBranch));
                returned
                    || parent.is_some_and(|parent| match parent.kind {
                        ExprKind::MethodCall(_, relayed, ..) =>
                            relayed.hir_id == expr.hir_id
                                && !(matches!(fn_def_id(cx, parent).and_then(|def_id| cx.tcx.get_diagnostic_name(def_id)), Some(sym::option_unwrap | sym::option_expect))
                                    && is_inside_always_const_context(cx.tcx, expr.hir_id)),
                        _ => branch.is_some(),
                    })
            }
        };
        let found = match expr.kind {
            ExprKind::Match(scrutinee, [tested, ..], MatchSource::Normal)
                if let Some(call) = root_macro_call(expr.span).filter(|call| cx.tcx.is_diagnostic_item(sym::matches_macro, call.def_id) && !expanded_externally(cx, call.span))
                    && let PatKind::TupleStruct(..) = tested.pat.kind
                    && let Some(error) = self.failure(cx, scrutinee, expr) =>
                Some((call.span, Discard::View, Cow::Borrowed("matches!"), error)),
            _ if expanded_externally(cx, expr.span) || is_from_proc_macro(cx, expr) => None,
            ExprKind::MethodCall(segment, receiver, arguments, _)
                if let Some(&Some((discard, gate))) = fn_def_id(cx, expr).and_then(|method| self.methods.get(&method))
                    && let Some(error) = self.failure(cx, receiver, expr)
                    && gated(gate, receiver, arguments, expr) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), discard, Cow::Borrowed(segment.ident.as_str()), error)),
            ExprKind::Path(_)
                if let Some(Some((Discard::Absence, _))) = fn_item_value(cx, expr).and_then(|function| self.methods.get(&function))
                    && let Some((_, error)) = typeck.expr_ty(expr).fn_sig(cx.tcx).skip_binder().inputs().first().and_then(|&held| result_parts(cx, held))
                    && self.failing(cx, error) =>
                Some((expr.span, Discard::Absence, Cow::Borrowed("Result::ok"), error)),
            ExprKind::MethodCall(segment, viewed, [], _)
                if matches!(segment.ident.name, sym::is_none | sym::is_some)
                    && let ExprKind::MethodCall(_, source, [], _) = viewed.kind
                    && self.err.is_some_and(|err| fn_def_id(cx, viewed) == Some(err))
                    && let Some(error) = self.failure(cx, source, expr) =>
                Some((viewed.span.with_lo(source.span.hi()).to(expr.span.shrink_to_hi()), Discard::View, Cow::Owned(format!("err().{}", segment.ident)), error)),
            ExprKind::MethodCall(segment, items, [], _)
                if segment.ident.name == sym::flatten
                    && fn_def_id(cx, expr).and_then(|method| associated_trait(cx.tcx, method)).is_diag_item(cx, sym::Iterator)
                    && let Some(item) = get_iterator_item_ty(cx, typeck.expr_ty_adjusted(items))
                    && let Some((_, error)) = result_parts(cx, item).filter(|&(_, error)| self.failing(cx, error)) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), Discard::Flattened, Cow::Borrowed(segment.ident.as_str()), error)),
            ExprKind::Call(_, [dropped])
                if fn_def_id(cx, expr).is_diag_item(cx, sym::mem_drop)
                    && !self.publication(cx, dropped)
                    && !is_copy(cx, typeck.expr_ty(dropped))
                    && let Some(error) = self.failure(cx, dropped, expr) =>
                Some((expr.span, Discard::Statement, Cow::Borrowed("drop"), error)),
            ExprKind::If(condition, then, otherwise) => for_each_expr_without_closures(condition, |part| {
                if let ExprKind::Let(tested) = part.kind
                    && let Some(error) = self.failure(cx, tested.init, expr)
                    && let Some(discard) = match (constructs(tested.pat, LangItem::ResultOk), otherwise) {
                        (true, None) => Some(Discard::Unmatched),
                        (true, Some(otherwise)) => fallback(otherwise).then_some(Discard::Fallback),
                        (false, None) => (skipped(cx, tested.pat, true) && idle(then)).then_some(Discard::Statement),
                        (false, Some(_)) => None,
                    }
                {
                    ControlFlow::Break((tested.span, discard, Cow::Borrowed("if let"), error))
                } else {
                    ControlFlow::Continue(())
                }
            }),
            ExprKind::Match(scrutinee, arms, MatchSource::Normal) if let Some(error) = self.failure(cx, scrutinee, expr) =>
                arms.iter().filter(|arm| arm.guard.is_none() && skipped(cx, arm.pat, true)).find_map(|arm| {
                    idle(arm.body).then_some(Discard::Statement).or_else(|| fallback(arm.body).then_some(Discard::Fallback)).map(|discard| (arm.span, discard, Cow::Borrowed("Err(_) =>"), error))
                }),
            _ => None,
        };
        let mut site = expr;
        let found = found.and_then(|(span, discard, label, error)| {
            if !matches!(discard, Discard::Absence) {
                return Some((span, discard, label, error));
            }
            let singleton = singular(cx, error);
            if !singleton
                && !matches!(expr.kind, ExprKind::MethodCall(_, receiver, [], _)
                        if matches!(receiver.kind, ExprKind::Call(function, ..) if has_single_failure_cause(cx, function))
                            || has_single_failure_cause(cx, receiver))
            {
                return Some((span, discard, label, error));
            }
            let mut relayed = expr;
            while let Some(parent) = get_parent_expr(cx, relayed)
                && let ExprKind::MethodCall(segment, receiver, arguments, _) = parent.kind
                && receiver.hir_id == relayed.hir_id
                && fn_def_id(cx, parent).is_some_and(|method| impl_owner(cx.tcx, method).is_diag_item(cx, sym::Option) || self.methods.contains_key(&method))
            {
                if let Some(&Some((discard, gate))) = fn_def_id(cx, parent).and_then(|method| self.methods.get(&method))
                    && gated(gate, receiver, arguments, parent)
                {
                    site = parent;
                    return Some((segment.ident.span.to(parent.span.shrink_to_hi()), discard, Cow::Borrowed(segment.ident.as_str()), error));
                }
                let output = typeck.expr_ty(parent);
                if !output.is_diag_item(cx, sym::Option) {
                    return (!result_parts(cx, output).is_some_and(|(_, translated)| is_error(cx, translated) && untyped_kind(cx, translated).is_none())).then_some((span, discard, label, error));
                }
                relayed = parent;
            }
            get_parent_expr(cx, relayed)
                .filter(|branch| !singleton && matches!(branch.kind, ExprKind::Call(..)) && fn_def_id(cx, branch).is_lang_item(cx, LangItem::TryTraitBranch))
                .filter(|branch| !matches!(get_parent_expr(cx, branch), Some(parent) if matches!(parent.kind, ExprKind::Match(_, _, MatchSource::TryDesugar(called)) if called == branch.hir_id)))
                .map(|_branch| (span, discard, label, error))
        });
        if let Some((span, discard, label, error)) = found
            && match discard {
                Discard::View => !body_output(cx, cx.tcx.hir_enclosing_body_owner(site.hir_id)).is_some_and(Ty::is_bool),
                Discard::Absence => true,
                Discard::Fallback | Discard::Success | Discard::Unmatched | Discard::Iterated | Discard::Flattened | Discard::Statement | Discard::Unnamed =>
                    typeck.expr_ty(site).opt_def_id().is_none_or(|produced| Some(produced) != self.exit_code),
            }
        {
            report(cx, site.hir_id, span, discard, &label, error);
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        const ALTERNATIVE: &str = "let (.. | Err(_))";
        if let Some(init) = local.init
            && !expanded_externally(cx, local.span.source_callsite())
        {
            let found = match (self.failure(cx, init, init), local.pat.kind, local.els) {
                (Some(error), PatKind::Wild, None) if matches!(local.source, LocalSource::AssignDesugar) => Some((Discard::Statement, Cow::Borrowed("_ ="), error)),
                (Some(error), PatKind::Binding(_, binding, ident, None), None) if get_enclosing_block(cx, local.hir_id).is_some_and(|block| !is_local_used(cx, block, binding)) =>
                    Some((Discard::Statement, Cow::Owned(format!("let {ident}")), error)),
                (Some(error), PatKind::TupleStruct(qpath, ..), Some(_)) if cx.qpath_res(&qpath, local.pat.hir_id).ctor_parent(cx).is_lang_item(cx, LangItem::ResultOk) =>
                    Some((Discard::Unmatched, Cow::Borrowed("let Ok(..) else"), error)),
                (Some(error), PatKind::Or(_), None) if skipped(cx, local.pat, true) => Some((Discard::Statement, Cow::Borrowed(ALTERNATIVE), error)),
                (None, PatKind::Or(_), None)
                    if skipped(cx, local.pat, false)
                        && let Some((_, error)) = result_parts(cx, cx.typeck_results().expr_ty(init))
                        && error.is_adt()
                        && !self.query_result(cx, error) =>
                    Some((Discard::Unnamed, Cow::Borrowed(ALTERNATIVE), error)),
                _ => None,
            };
            if let Some((discard, label, error)) = found
                && !(matches!(discard, Discard::Statement) && self.publication(cx, init))
            {
                report(cx, local.hir_id, local.span.source_callsite(), discard, &label, error);
            }
        }
    }
}
