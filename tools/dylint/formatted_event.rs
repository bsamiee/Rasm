use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    macros::{macro_backtrace, root_macro_call},
    paths::{PathNS, lookup_path_str},
    peel_hir_expr_refs,
    res::MaybeDef as _,
    source::walk_span_to_context,
    sym,
    visitors::for_each_expr_without_closures,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, TyCtxt};
use rustc_span::{Span, hygiene::MacroKind};

use crate::is_text;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum Rendered {
    Interpolated,
    Text,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    FORMATTED_EVENT,
    Warn,
    "tracing events and spans recording values rendered to text in place of typed fields"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct FormattedEvent {
    value: Option<DefId>,
    reported: FxHashSet<(Span, Rendered)>,
}

impl_lint_pass!(FormattedEvent => [FORMATTED_EVENT]);

impl FormattedEvent {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { value: lookup_path_str(tcx, PathNS::Type, "tracing_core::field::Value").into_iter().next(), reported: FxHashSet::default() } }
}

impl<'tcx> LateLintPass<'tcx> for FormattedEvent {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn rendered<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> Option<Rendered> {
            let typeck = cx.typeck_results();
            let held = typeck.expr_ty(value);
            if held.is_lang_item(cx, LangItem::FormatArguments)
                && for_each_expr_without_closures(value, |part| if typeck.expr_ty(part).is_lang_item(cx, LangItem::FormatArgument) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) })
                    .is_some()
            {
                Some(Rendered::Interpolated)
            } else if is_text(cx, held)
                && (root_macro_call(value.span).is_some_and(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id))
                    || matches!(value.kind, ExprKind::MethodCall(_, receiver, [], _)
                        if fn_def_id(cx, value).is_some_and(|method| cx.tcx.is_diagnostic_item(sym::to_string_method, method)) && !is_text(cx, typeck.expr_ty(receiver))))
            {
                Some(Rendered::Text)
            } else {
                None
            }
        }

        if let ExprKind::Cast(recorded, _) = expr.kind
            && let ty::Ref(_, object, _) = cx.typeck_results().expr_ty(expr).kind()
            && let ty::Dynamic(predicates, ..) = object.kind()
            && predicates.principal_def_id().is_some_and(|principal| Some(principal) == self.value)
            && !macro_backtrace(expr.span).any(|call| matches!(call.kind, MacroKind::Attr | MacroKind::Derive))
            && let call = expr.span.source_callsite()
            && !call.from_expansion()
            && let value = peel_hir_expr_refs(recorded).0
            && let Some(rendered) = rendered(cx, value)
            && let span = walk_span_to_context(value.span, call.ctxt()).unwrap_or(call)
            && self.reported.insert((span, rendered))
        {
            let (message, help) = match rendered {
                Rendered::Interpolated => ("tracing records values interpolated into text", "record each value as its own field (`name = %value` or `name = ?value`) beside a constant message"),
                Rendered::Text => ("tracing records a value rendered to text", "record the value itself with `%value` or `?value`, the subscriber formats it when it records the field"),
            };
            span_lint_hir_and_then(cx, FORMATTED_EVENT, expr.hir_id, span, message, |diag| {
                diag.help(help);
            });
        }
    }
}
