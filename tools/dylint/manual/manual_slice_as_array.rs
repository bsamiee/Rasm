use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    eq_expr_value, fn_def_id,
    higher::{Range, RangeTy},
    res::{MaybeDef as _, MaybeTypeckRes as _},
    source::snippet,
    sym,
};
use rustc_ast::BinOpKind;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, PathSegment};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_span::{Ident, Spanned};

use crate::{expanded_externally, method_call_on, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_SLICE_AS_ARRAY: "`try_into` and `try_from` conversions checking a slice's length to view it as an array");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let text = |part: &Expr<'_>| snippet(cx, part.span, "..");
        let source = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::Call(_, [source]) if fn_def_id(cx, expr).is_diag_item(cx, sym::try_from_fn) => Some(source),
            ExprKind::MethodCall(_, source, [], _) if cx.ty_based_def(expr).opt_parent(cx).is_diag_item(cx, sym::TryInto) => Some(source),
            _ => None,
        };
        if let Some(source) = source
            && matches!(typeck.expr_ty_adjusted(source).kind(), ty::Ref(_, slice, _) if slice.is_slice())
            && let ty::Adt(_, arguments) = typeck.expr_ty(expr).kind()
            && let target = arguments.type_at(0)
            && let Some((mutable, copied)) = if let ty::Ref(_, array, mutability) = target.kind()
                && array.is_array()
            {
                Some((mutability.is_mut(), ""))
            } else {
                target.is_array().then_some((false, ".copied()"))
            }
        {
            let viewed = source.peel_borrows();
            let bounded = if let ExprKind::Index(base, index, _) = viewed.kind { Some((base, Range::hir(cx, index))) } else { None };
            let suffix = if mutable { "_mut" } else { "" };
            let (receiver, method, exact) = match bounded {
                Some((base, Some(Range { ty: RangeTy::OpsTo, start: None, end: Some(_), .. }))) => (base, format!("first_chunk{suffix}"), false),
                Some((base, Some(Range { ty: RangeTy::OpsFrom, start: Some(start), end: None, .. })))
                    if let ExprKind::Binary(
                        Spanned { node: BinOpKind::Sub, .. },
                        &Expr { kind: ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::len, .. }, .. }, measured, [], _), .. },
                        _,
                    ) = start.kind
                        && eq_expr_value(cx, expr.span.ctxt(), measured, base) =>
                    (base, format!("last_chunk{suffix}"), false),
                Some(_) | None => (viewed, format!("as{suffix}_array"), true),
            };
            let converted = method_call_on(cx, expr).filter(|&(_, ok)| ok.is_diag_item(cx, sym::result_ok_method)).map(|(call, _)| call);
            let help = if converted.is_some() { format!("call `<[T]>::{method}`") } else { format!("call `<[T]>::{method}` and handle `None`") };
            span_lint_hir_and_then(cx, MANUAL_SLICE_AS_ARRAY, expr.hir_id, expr.span, format!("fallible conversion checks the length of `{}` to view it as `{target}`", text(viewed)), |diag| {
                suggest(
                    cx,
                    expr.hir_id,
                    diag,
                    converted.map_or(expr.span, |ok| ok.span),
                    help,
                    format!("{}.{method}(){copied}", text(receiver)),
                    if exact && converted.is_some() { Applicability::MachineApplicable } else { Applicability::MaybeIncorrect },
                );
            });
        }
    }
}
