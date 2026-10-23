use core::ops::ControlFlow;

use clippy_utils::{
    ExprUseNode,
    consts::{ConstEvalCtxt, Constant},
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_enclosing_block, get_expr_use_site, get_parent_expr,
    macros::root_macro_call,
    paths::PathNS,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{approx_ty_size, get_iterator_item_ty, same_type_modulo_regions},
    usage::local_used_after_expr,
    visitors::{for_each_expr_without_closures, is_const_evaluatable, is_local_used},
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Block, Expr, ExprKind, HirId, LetStmt, LocalSource, MatchSource, Pat, PatKind, Stmt, StmtKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::Span;

use super::{UntypedKind, body_output, unread, untyped_kind};
use crate::{expanded_externally, fn_item_value, is_error, path, paths, resolve, result_parts, workspace_def};

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
    methods: FxHashMap<DefId, (Discard, Gate)>,
    views: FxHashSet<DefId>,
    err: Option<DefId>,
    inspect_err: Option<DefId>,
    try_from_int: Option<DefId>,
}

lint! {
    Pass;
    DISCARDED_ERROR:
        "`Result` errors replaced by a fallback, success, or `None`, skipped by a pattern, tested into a `bool` outside a body returning `bool`, skipped by iteration, or dropped by a statement",
}

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            exit_code: path(tcx, PathNS::Type, "std::process::ExitCode"),
            methods: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("core::result::Result::unwrap_or", (Discard::Fallback, Gate::Always)),
                    ("core::result::Result::unwrap_or_default", (Discard::Fallback, Gate::Always)),
                    ("core::result::Result::map_or", (Discard::Fallback, Gate::Always)),
                    ("core::result::Result::unwrap_or_else", (Discard::Fallback, Gate::Unread)),
                    ("core::result::Result::map_or_else", (Discard::Fallback, Gate::Unread)),
                    ("core::result::Result::or", (Discard::Success, Gate::Succeeds)),
                    ("core::result::Result::or_else", (Discard::Success, Gate::Succeeds)),
                    ("core::result::Result::ok", (Discard::Absence, Gate::Relayed)),
                    ("core::result::Result::is_ok", (Discard::View, Gate::Always)),
                    ("core::result::Result::is_err", (Discard::View, Gate::Always)),
                    ("core::result::Result::is_ok_and", (Discard::View, Gate::Always)),
                    ("core::result::Result::is_err_and", (Discard::View, Gate::Unread)),
                    ("core::result::Result::iter", (Discard::Iterated, Gate::Always)),
                    ("core::result::Result::iter_mut", (Discard::Iterated, Gate::Always)),
                    ("core::iter::IntoIterator::into_iter", (Discard::Iterated, Gate::Always)),
                ],
            ),
            views: paths(tcx, PathNS::Value, &["core::result::Result::as_ref", "core::result::Result::as_mut", "core::result::Result::as_deref", "core::result::Result::as_deref_mut"]),
            err: path(tcx, PathNS::Value, "core::result::Result::err"),
            inspect_err: path(tcx, PathNS::Value, "core::result::Result::inspect_err"),
            try_from_int: path(tcx, PathNS::Type, "core::num::TryFromIntError"),
        }
    }

    fn failure<'tcx>(&self, cx: &LateContext<'tcx>, held: &'tcx Expr<'tcx>, site: &Expr<'_>) -> Option<Ty<'tcx>> {
        fn returned<'tcx>(cx: &LateContext<'tcx>, argument: Ty<'tcx>, owner: Ty<'tcx>, item: Option<Ty<'tcx>>) -> bool {
            same_type_modulo_regions(argument, owner)
                || item.is_some_and(|item| same_type_modulo_regions(argument, item))
                || match argument.kind() {
                    ty::Array(inner, _) => returned(cx, *inner, owner, item),
                    ty::Tuple(parts) => !parts.is_empty() && parts.iter().all(|part| returned(cx, part, owner, item)),
                    ty::Adt(option, arguments) if option.is_diag_item(cx, sym::Option) => returned(cx, arguments.type_at(0), owner, item),
                    _ => false,
                }
        }

        let typeck = cx.typeck_results();
        let viewed = if let ExprKind::MethodCall(_, base, [], _) = held.kind
            && fn_def_id(cx, held).is_some_and(|view| self.views.contains(&view))
        {
            base
        } else {
            held
        };
        let error = result_parts(cx, typeck.expr_ty_adjusted(held).peel_refs())?.1;
        let witness = if let ExprKind::MethodCall(_, receiver, ..) = held.kind
            && let ty::Adt(_, arguments) = error.kind()
            && arguments.types().next().is_some()
        {
            let owner = typeck.expr_ty_adjusted(receiver).peel_refs();
            let item = get_iterator_item_ty(cx, owner);
            arguments.types().all(|argument| returned(cx, argument, owner, item))
        } else {
            false
        };
        (failing(cx, error)
            && !witness
            && fn_def_id(cx, held).is_none_or(|called| Some(called) != self.inspect_err)
            && viewed.res_local_id().is_none_or(|local| !local_used_after_expr(cx, local, site)))
        .then_some(error)
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let constructs = |pattern: &Pat<'_>, item: LangItem| matches!(pattern.kind, PatKind::TupleStruct(qpath, ..) if cx.qpath_res(&qpath, pattern.hir_id).ctor_parent(cx).is_lang_item(cx, item));
        let skipped = |pattern: &Pat<'_>| {
            constructs(pattern, LangItem::ResultErr) && matches!(pattern.kind, PatKind::TupleStruct(_, [] | [Pat { kind: PatKind::Wild | PatKind::Binding(_, _, _, None), .. }], _))
        };
        let idle = |body: &Expr<'_>| match peel_blocks(body).kind {
            ExprKind::Block(Block { stmts: [], expr: None, .. }, _) | ExprKind::Tup([]) | ExprKind::Continue(_) | ExprKind::Ret(None) => true,
            ExprKind::Block(Block { stmts: [Stmt { kind: StmtKind::Semi(left), .. }], expr: None, .. }, _) => matches!(left.kind, ExprKind::Continue(_) | ExprKind::Ret(None)),
            _ => false,
        };
        let saturates = |receiver: &Expr<'_>, error: Ty<'tcx>, value: &'tcx Expr<'tcx>| {
            if error.opt_def_id().is_some_and(|narrowing| Some(narrowing) == self.try_from_int)
                && let Some((ok, _)) = result_parts(cx, typeck.expr_ty_adjusted(receiver))
                && ok.is_integral()
                && let Some((min, max)) = ok.numeric_min_and_max_as_bits(cx.tcx)
            {
                let bounded = |limit: &Expr<'_>| matches!(ConstEvalCtxt::new(cx).eval(limit), Some(Constant::Int(bits)) if bits == min || bits == max);
                bounded(value)
                    || unread(cx, value).is_some_and(|(_, body)| match body.kind {
                        ExprKind::If(_, then, Some(otherwise)) => bounded(then) && bounded(otherwise),
                        _ => bounded(body),
                    })
            } else {
                false
            }
        };
        let fallback = |body: &'tcx Expr<'tcx>| is_const_evaluatable(cx.tcx, typeck, body) || fn_def_id(cx, peel_blocks(body)).is_diag_item(cx, sym::default_fn);
        let found = match expr.kind {
            ExprKind::Match(scrutinee, [tested, ..], MatchSource::Normal)
                if let Some(call) = root_macro_call(expr.span).filter(|call| cx.tcx.is_diagnostic_item(sym::matches_macro, call.def_id) && !expanded_externally(cx, call.span))
                    && let PatKind::TupleStruct(..) = tested.pat.kind
                    && let Some(error) = self.failure(cx, scrutinee, expr) =>
                Some((call.span, Discard::View, String::from("matches!"), error)),
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::MethodCall(segment, receiver, arguments, _)
                if let Some(&(discard, gate)) = fn_def_id(cx, expr).and_then(|method| self.methods.get(&method))
                    && let Some(error) = self.failure(cx, receiver, expr)
                    && match gate {
                        Gate::Always => true,
                        Gate::Unread => arguments.first().is_some_and(|handler| unread(cx, handler).is_some()),
                        Gate::Succeeds => arguments
                            .first()
                            .is_some_and(|value| fn_def_id(cx, unread(cx, value).map_or_else(|| peel_blocks(value), |(_, body)| body)).ctor_parent(cx).is_lang_item(cx, LangItem::ResultOk)),
                        Gate::Relayed =>
                            get_parent_expr(cx, expr).is_some_and(|parent| match parent.kind {
                                ExprKind::MethodCall(_, relayed, ..) => relayed.hir_id == expr.hir_id,
                                ExprKind::Call(..) => fn_def_id(cx, parent).is_lang_item(cx, LangItem::TryTraitBranch),
                                _ => false,
                            }) || matches!(get_expr_use_site(cx.tcx, typeck, expr.span.ctxt(), expr).use_node(cx), ExprUseNode::Return(_)),
                    }
                    && !(matches!(discard, Discard::Fallback) && arguments.first().is_some_and(|value| saturates(receiver, error, value))) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), discard, segment.ident.to_string(), error)),
            ExprKind::Path(_)
                if let Some((Discard::Absence, _)) = fn_item_value(cx, expr).and_then(|function| self.methods.get(&function))
                    && let Some((_, error)) = typeck.expr_ty(expr).fn_sig(cx.tcx).skip_binder().inputs().first().and_then(|&held| result_parts(cx, held))
                    && failing(cx, error) =>
                Some((expr.span, Discard::Absence, String::from("Result::ok"), error)),
            ExprKind::MethodCall(segment, viewed, [], _)
                if matches!(segment.ident.name, sym::is_none | sym::is_some)
                    && let ExprKind::MethodCall(_, source, [], _) = viewed.kind
                    && self.err.is_some_and(|err| fn_def_id(cx, viewed) == Some(err))
                    && let Some(error) = self.failure(cx, source, expr) =>
                Some((viewed.span.with_lo(source.span.hi()).to(expr.span.shrink_to_hi()), Discard::View, format!("err().{}", segment.ident), error)),
            ExprKind::MethodCall(segment, items, [], _)
                if segment.ident.name == sym::flatten
                    && let Some(item) = get_iterator_item_ty(cx, typeck.expr_ty_adjusted(items))
                    && let Some((_, error)) = result_parts(cx, item).filter(|&(_, error)| failing(cx, error)) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), Discard::Flattened, segment.ident.to_string(), error)),
            ExprKind::Call(_, [dropped])
                if fn_def_id(cx, expr).is_diag_item(cx, sym::mem_drop)
                    && let Some(error) = self.failure(cx, dropped, expr) =>
                Some((expr.span, Discard::Statement, String::from("drop"), error)),
            ExprKind::If(condition, then, otherwise) => for_each_expr_without_closures(condition, |part| {
                if let ExprKind::Let(tested) = part.kind
                    && let Some(error) = self.failure(cx, tested.init, expr)
                    && let Some(discard) = match (constructs(tested.pat, LangItem::ResultOk), otherwise) {
                        (true, None) => Some(Discard::Unmatched),
                        (true, Some(otherwise)) => fallback(otherwise).then_some(Discard::Fallback),
                        (false, None) => (skipped(tested.pat) && idle(then)).then_some(Discard::Statement),
                        (false, Some(_)) => None,
                    }
                {
                    ControlFlow::Break((tested.span, discard, String::from("if let"), error))
                } else {
                    ControlFlow::Continue(())
                }
            }),
            ExprKind::Match(scrutinee, arms, MatchSource::Normal) if let Some(error) = self.failure(cx, scrutinee, expr) =>
                arms.iter().filter(|arm| arm.guard.is_none() && skipped(arm.pat)).find_map(|arm| {
                    idle(arm.body).then_some(Discard::Statement).or_else(|| fallback(arm.body).then_some(Discard::Fallback)).map(|discard| (arm.span, discard, String::from("Err(_) =>"), error))
                }),
            _ => None,
        };
        if let Some((span, discard, label, error)) = found
            && match discard {
                Discard::View => approx_ty_size(cx, error) > 0 && !body_output(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id)).is_some_and(Ty::is_bool),
                Discard::Absence => approx_ty_size(cx, error) > 0,
                Discard::Fallback | Discard::Success | Discard::Unmatched | Discard::Iterated | Discard::Flattened | Discard::Statement =>
                    typeck.expr_ty(expr).opt_def_id().is_none_or(|produced| Some(produced) != self.exit_code),
            }
        {
            report(cx, expr.hir_id, span, discard, &label, error);
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let Some(init) = local.init
            && !expanded_externally(cx, local.span.source_callsite())
            && let Some(error) = self.failure(cx, init, init)
            && let Some((discard, label)) = match (local.pat.kind, local.els) {
                (PatKind::Wild, None) if matches!(local.source, LocalSource::AssignDesugar) => Some((Discard::Statement, String::from("_ ="))),
                (PatKind::Binding(_, binding, ident, None), None) if get_enclosing_block(cx, local.hir_id).is_some_and(|block| !is_local_used(cx, block, binding)) =>
                    Some((Discard::Statement, format!("let {ident}"))),
                (PatKind::TupleStruct(qpath, ..), Some(_)) if cx.qpath_res(&qpath, local.pat.hir_id).ctor_parent(cx).is_lang_item(cx, LangItem::ResultOk) =>
                    Some((Discard::Unmatched, String::from("let Ok(..) else"))),
                _ => None,
            }
        {
            report(cx, local.hir_id, local.span.source_callsite(), discard, &label, error);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn failing<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> bool {
    !error.is_privately_uninhabited(cx.tcx, cx.typing_env())
        && match untyped_kind(cx, error) {
            Some(UntypedKind::Text | UntypedKind::Erased | UntypedKind::Report | UntypedKind::Many(_) | UntypedKind::Messages | UntypedKind::NoError(_)) => true,
            Some(UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Unit) | None => is_error(cx, error) || error.ty_adt_def().is_some_and(|adt| workspace_def(cx, adt.did())),
        }
}

fn report<'tcx>(cx: &LateContext<'tcx>, node: HirId, span: Span, discard: Discard, label: &str, error: Ty<'tcx>) {
    const HANDLED: &str = "match the `Result`, handle each error the caller acts on, and return the rest with `?`";
    let error = with_forced_trimmed_paths!(error.to_string());
    let (message, help) = match discard {
        Discard::Fallback => (format!("`{label}` replaces `{error}` with a fallback value"), String::from(HANDLED)),
        Discard::Success => (format!("`{label}` turns `{error}` into success"), String::from(HANDLED)),
        Discard::Absence => (format!("`{label}` turns `{error}` into `None`"), String::from("return the `Result` with `?`, or convert its error into a caller variant with `map_err`")),
        Discard::Unmatched => (format!("pattern of `{label}` leaves `{error}` unmatched"), String::from(HANDLED)),
        Discard::View => (format!("`{label}` tests the `Result` and drops `{error}`"), String::from("match the `Result` and handle `Err` in place of the test")),
        Discard::Iterated => (format!("`{label}` iterates the `Result` and skips its `{error}`"), String::from(HANDLED)),
        Discard::Flattened => (format!("`{label}` iterates results and skips each `{error}`"), format!("collect the results into `validated::Validated<Vec<_>, {error}>` and call `.ok()`")),
        Discard::Statement => (format!("`{label}` drops a `Result` and its unhandled `{error}`"), String::from("match the `Result`, or return it with `?`")),
    };
    span_lint_hir_and_then(cx, DISCARDED_ERROR, node, span, message, |diag| {
        diag.help(help);
    });
}
