use core::ops::ControlFlow;
use std::rc::Rc;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::macros::{macro_backtrace, root_macro_call};
use clippy_utils::res::MaybeDef as _;
use clippy_utils::source::walk_span_to_context;
use clippy_utils::sugg::Sugg;
use clippy_utils::ty::implements_trait;
use clippy_utils::visitors::for_each_expr_without_closures;
use clippy_utils::{expr_or_init, fn_def_id, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_span::Span;

use crate::{Recorded, ReportedRenderings, associated_trait, is_text, record_rendering, suggest};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Rendering<'tcx> {
    Interpolated,
    Formatted,
    Converted { receiver: &'tcx Expr<'tcx>, typed: bool },
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    recorded: Rc<Recorded>,
    renderings: ReportedRenderings,
}

lint!(Pass; FORMATTED_TRACING_VALUE: "tracing events and spans recording values rendered to text in place of typed fields");

impl Pass {
    pub(crate) const fn new(_: TyCtxt<'_>, recorded: Rc<Recorded>, renderings: ReportedRenderings) -> Self { Self { recorded, renderings } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn rendered<'tcx>(cx: &LateContext<'tcx>, recorded: &Recorded, value: &'tcx Expr<'tcx>, call: Span) -> Option<Rendering<'tcx>> {
            let typeck = cx.typeck_results();
            let held = typeck.expr_ty(value);
            let interpolated = held.is_lang_item(cx, LangItem::FormatArguments)
                && for_each_expr_without_closures(value, |part| if typeck.expr_ty(part).is_lang_item(cx, LangItem::FormatArgument) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) })
                    .is_some();
            match value.kind {
                _ if interpolated && !macro_backtrace(value.span).next().is_some_and(|format| format.span.ctxt() == call.ctxt()) => Some(Rendering::Interpolated),
                _ if interpolated || is_text(cx, held) && root_macro_call(value.span).is_some_and(|format| cx.tcx.is_diagnostic_item(sym::format_macro, format.def_id)) => Some(Rendering::Formatted),
                ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver])
                    if is_text(cx, held)
                        && fn_def_id(cx, value).and_then(|method| associated_trait(cx.tcx, method)).is_diag_item(cx, sym::ToString)
                        && let converted = typeck.expr_ty(receiver)
                        && !is_text(cx, converted)
                        && let typed = recorded.value.iter().any(|&traced| implements_trait(cx, converted, traced, &[]))
                        && (typed || cx.tcx.get_diagnostic_item(sym::Display).is_some_and(|display| implements_trait(cx, converted, display, &[]))) =>
                    Some(Rendering::Converted { receiver, typed }),
                _ => None,
            }
        }

        let Some((field, call)) = self.recorded.field(cx, expr) else { return };
        let value = expr_or_init(cx, field);
        let Some(rendering) = rendered(cx, &self.recorded, value, call) else { return };
        let span = walk_span_to_context(value.span, call.ctxt()).unwrap_or(call);
        let recorded = matches!(expr.kind, ExprKind::MethodCall(..) | ExprKind::Call(..));
        let message = match rendering {
            Rendering::Interpolated => "tracing message interpolates values into text",
            Rendering::Formatted | Rendering::Converted { .. } => "tracing field records a value rendered to text",
        };
        span_lint_hir_and_then(cx, FORMATTED_TRACING_VALUE, expr.hir_id, span, message, |diag| {
            if matches!(rendering, Rendering::Formatted) {
                record_rendering(cx, &self.renderings, value.hir_id);
            }
            match rendering {
                Rendering::Interpolated => {
                    diag.help("move each value out of the message into a field: bare for `tracing::Value`, `%value` for `Display`, `?value` for `Debug`");
                }
                Rendering::Converted { receiver, typed } if value.hir_id == field.hir_id => {
                    let mut applicability = Applicability::MaybeIncorrect;
                    let source = Sugg::hir_with_context(cx, receiver, call.ctxt(), "..", &mut applicability);
                    let shown = if recorded { source.addr() } else { source };
                    let converted = cx.typeck_results().expr_ty(receiver);
                    let displayed = "record the value through its `Display` impl";
                    let (help, replacement) = match (typed, recorded) {
                        (true, _) => (format!("record the `{converted}` as a typed field"), shown.to_string()),
                        (false, false) => (String::from(displayed), format!("%{shown}")),
                        (false, true) => (String::from(displayed), format!("tracing::field::display({shown})")),
                    };
                    suggest(cx, value.hir_id, diag, span, help, replacement, applicability);
                }
                Rendering::Formatted | Rendering::Converted { .. } if recorded => {
                    diag.help("pass `Span::record` a `tracing::Value`, `tracing::field::display(value)`, or `tracing::field::debug(value)`");
                }
                Rendering::Formatted | Rendering::Converted { .. } => {
                    diag.help("record the value as a field: bare for `tracing::Value`, `%value` for `Display`, `?value` for `Debug`");
                }
            }
        });
    }
}
