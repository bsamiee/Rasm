use clippy_utils::{
    diagnostics::span_lint_hir_and_then, fn_def_id, is_lang_item_or_ctor, paths::PathNS, peel_blocks, peel_hir_expr_while, peel_hir_pat_refs, res::MaybeResPath as _, ty::get_iterator_item_ty,
};
use rustc_ast::LitKind;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Arm, Expr, ExprKind, HirId, MatchSource, Pat, PatKind, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, TypeckResults};

use crate::{expanded_externally, fn_item_value, path, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    partition: Option<DefId>,
    cases: FxHashMap<DefId, bool>,
    as_ref: Option<DefId>,
}

lint!(Pass; PARTITION_BY_RESULT: "`partition` calls splitting `Result` items by `is_ok`, `is_err`, or an `Ok` or `Err` pattern");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            partition: path(tcx, PathNS::Value, "core::iter::Iterator::partition"),
            cases: resolve(tcx, PathNS::Value, &[("core::result::Result::is_ok", true), ("core::result::Result::is_err", false)]),
            as_ref: path(tcx, PathNS::Value, "core::result::Result::as_ref"),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn selects_ok(pass: &Pass, cx: &LateContext<'_>, typeck: &TypeckResults<'_>, item: HirId, tested: &Expr<'_>) -> Option<bool> {
            let reaches = |operand: &Expr<'_>| {
                peel_hir_expr_while(operand, |inner| match inner.kind {
                    ExprKind::AddrOf(_, _, peeled) | ExprKind::Unary(UnOp::Deref, peeled) => Some(peeled),
                    ExprKind::MethodCall(_, peeled, [], _) if typeck.type_dependent_def_id(inner.hir_id).is_some_and(|method| Some(method) == pass.as_ref) => Some(peeled),
                    _ => None,
                })
                .res_local_id()
                    == Some(item)
            };
            let case = |pat: &Pat<'_>| match pat.kind {
                PatKind::TupleStruct(qpath, fields, _) if fields.iter().all(|field| matches!(field.kind, PatKind::Wild)) => typeck
                    .qpath_res(&qpath, pat.hir_id)
                    .opt_def_id()
                    .filter(|&ctor| [LangItem::ResultOk, LangItem::ResultErr].into_iter().any(|lang| is_lang_item_or_ctor(cx, ctor, lang)))
                    .map(|ctor| is_lang_item_or_ctor(cx, ctor, LangItem::ResultOk)),
                _ => None,
            };
            let answer = |arm: &Arm<'_>| match arm.body.kind {
                ExprKind::Lit(literal) if arm.guard.is_none() =>
                    if let LitKind::Bool(answer) = literal.node {
                        Some(answer)
                    } else {
                        None
                    },
                _ => None,
            };
            let mut negated = false;
            let tested = peel_hir_expr_while(peel_blocks(tested), |inner| {
                if let ExprKind::Unary(UnOp::Not, operand) = inner.kind {
                    negated = !negated;
                    Some(peel_blocks(operand))
                } else {
                    None
                }
            });
            match tested.kind {
                ExprKind::MethodCall(_, receiver, [], _) => typeck.type_dependent_def_id(tested.hir_id).and_then(|method| pass.cases.get(&method).copied()).filter(|_| reaches(receiver)),
                ExprKind::Call(Expr { kind: ExprKind::Path(qpath), hir_id, .. }, [operand]) =>
                    typeck.qpath_res(qpath, *hir_id).opt_def_id().and_then(|function| pass.cases.get(&function).copied()).filter(|_| reaches(operand)),
                ExprKind::Match(scrutinee, [hit, miss], MatchSource::Normal)
                    if (matches!(miss.pat.kind, PatKind::Wild) || case(miss.pat).is_some()) && answer(miss).is_some() && reaches(scrutinee) =>
                    Some(case(hit.pat)? == answer(hit)?),
                _ => None,
            }
            .map(|selected| selected != negated)
        }

        if let ExprKind::MethodCall(segment, receiver, [predicate], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|method| Some(method) == self.partition)
            && let named = fn_item_value(cx, predicate)
            && let Some(ok_first) = named.and_then(|named| self.cases.get(&named).copied()).or_else(|| {
                match predicate.kind {
                    ExprKind::Closure(closure) => Some((closure.def_id, cx.tcx.hir_body(closure.body))),
                    _ => named.and_then(DefId::as_local).and_then(|local| Some((local, cx.tcx.hir_maybe_body_owned_by(local)?))),
                }
                .and_then(|(owner, body)| match body.params {
                    [item] => selects_ok(self, cx, cx.tcx.typeck(owner), peel_hir_pat_refs(item.pat).0.hir_id, &body.value),
                    _ => None,
                })
            })
        {
            let owned = get_iterator_item_ty(cx, cx.typeck_results().expr_ty_adjusted(receiver)).is_none_or(|item| !item.is_ref());
            span_lint_hir_and_then(
                cx,
                PARTITION_BY_RESULT,
                expr.hir_id,
                segment.ident.span.to(expr.span.shrink_to_hi()),
                "`partition` leaves each half of the results wrapped in `Ok` or `Err`",
                |diag| {
                    diag.help(format!(
                        "call `Itertools::partition_result`{}{}",
                        if owned { "" } else { " on owned results" },
                        if ok_first { " and delete the unwraps after it" } else { ", swap the halves, and delete the unwraps after it" },
                    ));
                },
            );
        }
    }
}
