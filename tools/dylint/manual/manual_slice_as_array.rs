use clippy_utils::{
    consts::{ConstEvalCtxt, Constant},
    diagnostics::span_lint_hir_and_then,
    eq_expr_value, fn_def_id, get_parent_expr,
    higher::{Range, RangeTy},
    res::MaybeDef as _,
    source::{snippet, snippet_opt},
    sugg::Sugg,
    sym,
};
use rustc_ast::BinOpKind;
use rustc_errors::Applicability;
use rustc_hir::{
    Expr, ExprKind, PathSegment,
    def::{DefKind, Res},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_span::{Ident, Spanned};

use super::repeatable;
use crate::{associated_trait, expanded_externally, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_SLICE_AS_ARRAY: "`try_into` and `try_from` conversions checking a slice's length to view it as an array");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let text = |part: &Expr<'_>| snippet(cx, part.span, "..");
        let source = match expr.kind {
            ExprKind::Call(_, [source]) | ExprKind::MethodCall(_, source, [], _)
                if matches!(fn_def_id(cx, expr).and_then(|method| associated_trait(cx.tcx, method)).opt_diag_name(cx), Some(sym::TryFrom | sym::TryInto)) =>
                Some(source),
            _ => None,
        };
        if let Some(source) = source
            && !expanded_externally(cx, expr.span)
            && let typeck = cx.typeck_results()
            && let ty::Ref(_, slice, source_mutability) = typeck.expr_ty_adjusted(source).kind()
            && slice.is_slice()
            && let ty::Adt(_, arguments) = typeck.expr_ty(expr).kind()
            && let target = arguments.type_at(0)
            && let Some((array, mutable, copied)) = match target.kind() {
                ty::Ref(_, array, mutability) if let ty::Array(_, length) = array.kind() => Some((*length, mutability.is_mut(), "")),
                ty::Array(_, length) => Some((*length, source_mutability.is_mut(), ".copied()")),
                _ => None,
            }
            && let parameter = match array.kind() {
                ty::ConstKind::Param(parameter) => Some(cx.tcx.generics_of(typeck.hir_owner).const_param(parameter, cx.tcx).def_id),
                _ => None,
            }
            && let size = array.try_to_target_usize(cx.tcx)
            && let Some(length_text) = size.map(|length| length.to_string()).or_else(|| parameter.and_then(|parameter| cx.tcx.def_ident_span(parameter)).and_then(|span| snippet_opt(cx, span)))
        {
            let viewed = source.peel_borrows();
            let bounded = if let ExprKind::Index(base, index, _) = viewed.kind
                && let ty::Array(_, capacity) = typeck.expr_ty(base).peel_refs().kind()
                && (*capacity == array || matches!((capacity.try_to_target_usize(cx.tcx), size), (Some(capacity), Some(length)) if capacity >= length))
            {
                Some((base, Range::hir(cx, index)))
            } else {
                None
            };
            let suffix = if mutable { "_mut" } else { "" };
            let sized = |bound: &Expr<'_>| {
                if let ExprKind::Path(path) = &bound.kind
                    && let Res::Def(DefKind::ConstParam, found) = typeck.qpath_res(path, bound.hir_id)
                {
                    parameter == Some(found)
                } else {
                    matches!((size, ConstEvalCtxt::new(cx).eval(bound)), (Some(length), Some(Constant::Int(bound))) if u128::from(length) == bound)
                }
            };
            let (receiver, method) = match bounded {
                Some((base, Some(Range { ty: RangeTy::OpsTo, start: None, end: Some(end), .. }))) if sized(end) => (base, format!("first_chunk{suffix}")),
                Some((base, Some(Range { ty: RangeTy::OpsFrom, start: Some(start), end: None, .. })))
                    if let ExprKind::Binary(
                        Spanned { node: BinOpKind::Sub, .. },
                        measured_call @ &Expr { kind: ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::len, .. }, .. }, measured, [], _), .. },
                        length,
                    ) = start.kind
                        && eq_expr_value(cx, expr.span.ctxt(), measured, base)
                        && repeatable(cx, base)
                        && fn_def_id(cx, measured_call).is_some_and(|method| associated_trait(cx.tcx, method).is_none())
                        && sized(length) =>
                    (base, format!("last_chunk{suffix}")),
                Some(_) | None => (viewed, format!("as{suffix}_array")),
            };
            let converted = get_parent_expr(cx, expr).filter(|call| fn_def_id(cx, call).is_diag_item(cx, sym::result_ok_method));
            let mut applicability = Applicability::MachineApplicable;
            let argument = Sugg::hir_with_context(cx, receiver, expr.span.ctxt(), "..", &mut applicability);
            let argument = if typeck.expr_ty(receiver).is_ref() {
                argument
            } else if mutable {
                argument.mut_addr()
            } else {
                argument.addr()
            };
            span_lint_hir_and_then(cx, MANUAL_SLICE_AS_ARRAY, expr.hir_id, expr.span, format!("fallible conversion checks the length of `{}` to view it as `{target}`", text(viewed)), |diag| {
                if let Some(ok) = converted {
                    suggest(cx, expr.hir_id, diag, ok.span, format!("call `<[T]>::{method}`"), format!("<[_]>::{method}::<{length_text}>({argument}){copied}"), applicability);
                } else {
                    diag.help(format!("call `<[T]>::{method}::<{length_text}>` and handle `None`"));
                }
            });
        }
    }
}
