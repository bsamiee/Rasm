use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id,
    macros::{macro_backtrace, root_macro_call},
    res::MaybeDef as _,
    source::{snippet_with_context, walk_span_to_context},
    sym,
    ty::implements_trait,
    visitors::for_each_expr_without_closures,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_span::Span;

use crate::{Recorded, is_text, suggest};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Rendering<'tcx> {
    Interpolated,
    Formatted,
    Converted(&'tcx Expr<'tcx>),
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    recorded: Recorded,
}

lint!(Pass; FORMATTED_TRACING_VALUE: "tracing events and spans recording values rendered to text in place of typed fields");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { recorded: Recorded::new(tcx) } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn rendered<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>, call: Span) -> Option<Rendering<'tcx>> {
            let typeck = cx.typeck_results();
            let held = typeck.expr_ty(value);
            let interpolated = held.is_lang_item(cx, LangItem::FormatArguments)
                && for_each_expr_without_closures(value, |part| if typeck.expr_ty(part).is_lang_item(cx, LangItem::FormatArgument) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) })
                    .is_some();
            match value.kind {
                _ if interpolated && !macro_backtrace(value.span).next().is_some_and(|format| format.span.ctxt() == call.ctxt()) => Some(Rendering::Interpolated),
                _ if interpolated || is_text(cx, held) && root_macro_call(value.span).is_some_and(|format| cx.tcx.is_diagnostic_item(sym::format_macro, format.def_id)) => Some(Rendering::Formatted),
                ExprKind::MethodCall(_, receiver, [], _)
                    if is_text(cx, held) && fn_def_id(cx, value).is_some_and(|method| cx.tcx.is_diagnostic_item(sym::to_string_method, method)) && !is_text(cx, typeck.expr_ty(receiver)) =>
                    Some(Rendering::Converted(receiver)),
                _ => None,
            }
        }

        if let Some((field, call)) = self.recorded.field(cx, expr)
            && let value = expr_or_init(cx, field)
            && let Some(rendering) = rendered(cx, value, call)
            && let span = walk_span_to_context(value.span, call.ctxt()).unwrap_or(call)
        {
            let recorded = matches!(expr.kind, ExprKind::MethodCall(..));
            let message = match rendering {
                Rendering::Interpolated => "tracing message interpolates values into text",
                Rendering::Formatted | Rendering::Converted(_) => "tracing field records a value rendered to text",
            };
            span_lint_hir_and_then(cx, FORMATTED_TRACING_VALUE, expr.hir_id, span, message, |diag| match rendering {
                Rendering::Interpolated => {
                    diag.help("record each interpolated value as its own field (`name = value`, or `name = %value` for a type without `tracing::Value`) beside a constant message");
                }
                Rendering::Converted(receiver) if value.hir_id == field.hir_id => {
                    let mut applicability = Applicability::MaybeIncorrect;
                    let (shown, _) = snippet_with_context(cx, receiver.span, call.ctxt(), "..", &mut applicability);
                    let converted = cx.typeck_results().expr_ty(receiver);
                    let displayed = "record the value through its `Display` impl";
                    let (help, replacement) = match (self.recorded.value.is_some_and(|traced| implements_trait(cx, converted, traced, &[])), recorded) {
                        (true, _) => (format!("record the `{converted}` as a typed field"), shown.into_owned()),
                        (false, false) => (String::from(displayed), format!("%{shown}")),
                        (false, true) => (String::from(displayed), format!("tracing::field::display({shown})")),
                    };
                    suggest(cx, value.hir_id, diag, span, help, replacement, applicability);
                }
                Rendering::Formatted | Rendering::Converted(_) if recorded => {
                    diag.help("pass the formatted value to `Span::record`, or `tracing::field::display(value)` for a type without `tracing::Value`");
                }
                Rendering::Formatted | Rendering::Converted(_) => {
                    diag.help("record each formatted value as its own field (`name = value`, or `name = %value` for a type without `tracing::Value`)");
                }
            });
        }
    }
}
