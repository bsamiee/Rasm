use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, is_lang_item_or_ctor, paths::PathNS, peel_blocks, peel_hir_expr_while, peel_hir_pat_refs, res::MaybeResPath as _};
use rustc_ast::LitKind;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Arm, Expr, ExprKind, HirId, MatchSource, Pat, PatKind, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, TypeckResults};

use crate::{expanded_externally, fn_item_value, path, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    partition: Option<DefId>,
    cases: FxHashSet<DefId>,
    as_ref: Option<DefId>,
}

lint!(Pass; PARTITION_BY_RESULT: "`partition` calls splitting `Result` items by `is_ok`, `is_err`, or an `Ok` or `Err` pattern");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            partition: path(tcx, PathNS::Value, "core::iter::Iterator::partition"),
            cases: paths(tcx, PathNS::Value, &["core::result::Result::is_ok", "core::result::Result::is_err"]),
            as_ref: path(tcx, PathNS::Value, "core::result::Result::as_ref"),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn tests(pass: &Pass, cx: &LateContext<'_>, typeck: &TypeckResults<'_>, item: HirId, tested: &Expr<'_>) -> bool {
            let reaches = |operand: &Expr<'_>| {
                peel_hir_expr_while(operand, |inner| match inner.kind {
                    ExprKind::AddrOf(_, _, peeled) | ExprKind::Unary(UnOp::Deref, peeled) => Some(peeled),
                    ExprKind::MethodCall(_, peeled, [], _) if typeck.type_dependent_def_id(inner.hir_id).is_some_and(|method| Some(method) == pass.as_ref) => Some(peeled),
                    _ => None,
                })
                .res_local_id()
                    == Some(item)
            };
            let variant = |ctor: DefId| [LangItem::ResultOk, LangItem::ResultErr].into_iter().any(|lang| is_lang_item_or_ctor(cx, ctor, lang));
            let case = |pat: &Pat<'_>| {
                matches!(pat.kind, PatKind::TupleStruct(qpath, fields, _)
                    if fields.iter().all(|field| matches!(field.kind, PatKind::Wild)) && typeck.qpath_res(&qpath, pat.hir_id).opt_def_id().is_some_and(variant))
            };
            let answer = |arm: &Arm<'_>| arm.guard.is_none() && matches!(arm.body.kind, ExprKind::Lit(literal) if matches!(literal.node, LitKind::Bool(_)));
            let tested = peel_hir_expr_while(peel_blocks(tested), |inner| if let ExprKind::Unary(UnOp::Not, negated) = inner.kind { Some(peel_blocks(negated)) } else { None });
            match tested.kind {
                ExprKind::MethodCall(_, receiver, [], _) => typeck.type_dependent_def_id(tested.hir_id).is_some_and(|method| pass.cases.contains(&method)) && reaches(receiver),
                ExprKind::Call(Expr { kind: ExprKind::Path(qpath), hir_id, .. }, [operand]) =>
                    typeck.qpath_res(qpath, *hir_id).opt_def_id().is_some_and(|function| pass.cases.contains(&function)) && reaches(operand),
                ExprKind::Match(scrutinee, [hit, miss], MatchSource::Normal) =>
                    case(hit.pat) && (matches!(miss.pat.kind, PatKind::Wild) || case(miss.pat)) && answer(hit) && answer(miss) && reaches(scrutinee),
                _ => false,
            }
        }

        if let ExprKind::MethodCall(segment, _, [predicate], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|method| Some(method) == self.partition)
            && let named = fn_item_value(cx, predicate)
            && (named.is_some_and(|named| self.cases.contains(&named))
                || match predicate.kind {
                    ExprKind::Closure(closure) => Some((closure.def_id, cx.tcx.hir_body(closure.body))),
                    _ => named.and_then(DefId::as_local).and_then(|local| Some((local, cx.tcx.hir_maybe_body_owned_by(local)?))),
                }
                .is_some_and(|(owner, body)| matches!(body.params, [item] if tests(self, cx, cx.tcx.typeck(owner), peel_hir_pat_refs(item.pat).0.hir_id, &body.value))))
        {
            span_lint_hir_and_then(
                cx,
                PARTITION_BY_RESULT,
                expr.hir_id,
                segment.ident.span.to(expr.span.shrink_to_hi()),
                "`partition` splits results by case and leaves each half wrapped in `Ok` or `Err`",
                |diag| {
                    diag.help("call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it");
                },
            );
        }
    }
}
