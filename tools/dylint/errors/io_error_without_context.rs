use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    paths::PathNS,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{Expr, ExprKind, MatchSource, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, GenericArgsRef, Ty, TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::Symbol;

use crate::{awaited_output, expanded_externally, impl_owner, is_text, item_path, path, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    io_error: Option<DefId>,
    contextual: Symbol,
}

lint!(Pass; IO_ERROR_WITHOUT_CONTEXT: "`io::Error` values from calls stating no operation or subject that enter a crate error through `From`, and `io::Error` values built from text");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { io_error: path(tcx, PathNS::Type, "std::io::Error"), contextual: Symbol::intern("fs_err") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn producer<'tcx>(cx: &LateContext<'tcx>, io_error: DefId, expr: &'tcx Expr<'tcx>) -> Option<(DefId, GenericArgsRef<'tcx>)> {
            let value = peel_blocks(expr);
            let produced = desugar_await(value).map_or(value, peel_blocks);
            let typeck = cx.typeck_results();
            match produced.kind {
                ExprKind::MethodCall(_, receiver, ..)
                    if let Some(adapter) = typeck.type_dependent_def_id(produced.hir_id)
                        && impl_owner(cx.tcx, adapter).is_diag_item(cx, sym::Result)
                        && result_parts(cx, typeck.expr_ty(receiver)).is_some_and(|(_, error)| error.opt_def_id() == Some(io_error)) =>
                    producer(cx, io_error, receiver),
                ExprKind::Call(callee, _) => fn_def_id(cx, produced).map(|function| (function, typeck.node_args(callee.hir_id))),
                ExprKind::MethodCall(..) => typeck.type_dependent_def_id(produced.hir_id).map(|function| (function, typeck.node_args(produced.hir_id))),
                _ => None,
            }
        }

        let Some(io_error) = self.io_error else { return };
        let typeck = cx.typeck_results();
        let bare = |error: Ty<'_>| error.opt_def_id() == Some(io_error);
        let converted = match expr.kind {
            ExprKind::Match(branch, arms, MatchSource::TryDesugar(_))
                if let ExprKind::Call(_, [operand]) = branch.kind
                    && result_parts(cx, typeck.expr_ty(operand)).is_some_and(|(_, error)| bare(error))
                    && let Some(returned) = arms.iter().find_map(|arm| if let ExprKind::Ret(Some(returned)) | ExprKind::Break(_, Some(returned)) = arm.body.kind { Some(returned) } else { None })
                    && fn_def_id(cx, returned).is_lang_item(cx, LangItem::TryTraitFromResidual)
                    && let Some((_, target)) = result_parts(cx, typeck.expr_ty(returned)) =>
                Some((operand, target, expr.span)),
            ExprKind::MethodCall(segment, receiver, [conversion], _)
                if segment.ident.name == sym::map_err
                    && result_parts(cx, typeck.expr_ty(receiver)).is_some_and(|(_, error)| bare(error))
                    && let Some(converter) = match conversion.kind {
                        ExprKind::Closure(closure)
                            if let body = cx.tcx.hir_body(closure.body)
                                && let [parameter] = body.params
                                && let value = peel_blocks(body.value)
                                && let ExprKind::Call(_, [argument]) | ExprKind::MethodCall(_, argument, [], _) = value.kind
                                && argument.res_local_id() == Some(parameter.pat.hir_id) =>
                            fn_def_id(cx, value),
                        _ if let ty::FnDef(function, _) = *typeck.expr_ty(conversion).kind() => Some(function),
                        _ => None,
                    }
                    && matches!(converter.opt_parent(cx).opt_diag_name(cx), Some(sym::From | sym::Into))
                    && let Some((_, target)) = result_parts(cx, typeck.expr_ty(expr)) =>
                Some((receiver, target, segment.ident.span.to(expr.span.shrink_to_hi()))),
            _ => None,
        };
        if let Some((operand, target, span)) = converted
            && !expanded_externally(cx, expr.span)
            && let ty::Adt(adt, _) = target.kind()
            && adt.did().is_local()
            && let produced = producer(cx, io_error, operand)
            && !produced.is_some_and(|(function, args)| {
                cx.tcx
                    .trait_of_assoc(function)
                    .map_or(Some(function.krate), |_| args.type_at(0).peel_refs().ty_adt_def().map(|owner| owner.did().krate))
                    .is_some_and(|owner| cx.tcx.crate_name(owner) == self.contextual)
            })
        {
            let origin = produced.map_or_else(|| String::from("an expression"), |(function, _)| format!("`{}`", item_path(cx, function)));
            span_lint_hir_and_then(
                cx,
                IO_ERROR_WITHOUT_CONTEXT,
                expr.hir_id,
                span,
                with_forced_trimmed_paths!(format!("{origin} returns an `io::Error` stating no operation or subject, and it enters `{target}` through `From`")),
                |diag| {
                    diag.help("call the `fs_err` form stating operation and path, or `map_err` into a variant holding the operation and `#[source] io::Error`");
                },
            );
        }
        if let ExprKind::Call(_, arguments) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(callee) = fn_def_id(cx, expr)
            && impl_owner(cx.tcx, callee).is_some_and(|implemented| implemented.opt_def_id() == Some(io_error))
            && arguments.iter().any(|argument| is_text(cx, typeck.expr_ty(argument)))
        {
            let enclosing = cx.tcx.typeck_root_def_id(cx.tcx.hir_enclosing_body_owner(expr.hir_id).to_def_id());
            let streamed = matches!(cx.tcx.def_kind(enclosing), DefKind::Fn | DefKind::AssocFn) && {
                let output = awaited_output(cx.tcx, enclosing);
                result_parts(cx, output).map_or(output, |(_, error)| error).opt_def_id() == Some(io_error)
            };
            let constructor = cx.tcx.item_name(callee);
            span_lint_hir_and_then(cx, IO_ERROR_WITHOUT_CONTEXT, expr.hir_id, expr.span, format!("`io::Error::{constructor}` wraps text callers cannot match"), |diag| {
                diag.help(if streamed {
                    format!("pass a typed error to `io::Error::{constructor}` in place of the text")
                } else {
                    String::from("return a `thiserror` variant holding the subject and its typed source")
                });
            });
        }
    }
}
