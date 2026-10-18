use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id,
    macros::{macro_backtrace, root_macro_call},
    paths::PathNS,
    peel_hir_expr_refs,
    res::MaybeDef as _,
    source::walk_span_to_context,
    sym,
    visitors::for_each_expr_without_closures,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt};
use rustc_span::{Span, hygiene::MacroKind};

use crate::{is_text, path, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    value: Option<DefId>,
    wrappers: FxHashSet<DefId>,
}

lint!(Pass; FORMATTED_TRACING_VALUE: "tracing events and spans recording values rendered to text in place of typed fields");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { value: path(tcx, PathNS::Type, "tracing_core::field::Value"), wrappers: paths(tcx, PathNS::Value, &["tracing_core::field::display", "tracing_core::field::debug"]) }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn rendered<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>, call: Span) -> Option<(&'static str, &'static str)> {
            let typeck = cx.typeck_results();
            let held = typeck.expr_ty(value);
            let interpolated = held.is_lang_item(cx, LangItem::FormatArguments)
                && for_each_expr_without_closures(value, |part| if typeck.expr_ty(part).is_lang_item(cx, LangItem::FormatArgument) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) })
                    .is_some();
            if interpolated && !macro_backtrace(value.span).next().is_some_and(|format| format.span.ctxt() == call.ctxt()) {
                Some(("tracing message interpolates values into text", "record each value as its own field (`name = %value` or `name = ?value`) beside a constant message"))
            } else if interpolated
                || is_text(cx, held)
                    && (root_macro_call(value.span).is_some_and(|format| cx.tcx.is_diagnostic_item(sym::format_macro, format.def_id))
                        || matches!(value.kind, ExprKind::MethodCall(_, receiver, [], _)
                            if fn_def_id(cx, value).is_some_and(|method| cx.tcx.is_diagnostic_item(sym::to_string_method, method)) && !is_text(cx, typeck.expr_ty(receiver))))
            {
                Some(("tracing field records a value rendered to text", "record the value itself with `%value` or `?value`, the subscriber formats it"))
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
            && let value = match value.kind {
                ExprKind::Call(_, [shown]) if fn_def_id(cx, value).is_some_and(|wrapper| self.wrappers.contains(&wrapper)) => peel_hir_expr_refs(shown).0,
                _ => value,
            }
            && let value = expr_or_init(cx, value)
            && let Some((message, help)) = rendered(cx, value, call)
            && let span = walk_span_to_context(value.span, call.ctxt()).unwrap_or(call)
        {
            span_lint_hir_and_then(cx, FORMATTED_TRACING_VALUE, expr.hir_id, span, message, |diag| {
                diag.help(help);
            });
        }
    }
}
