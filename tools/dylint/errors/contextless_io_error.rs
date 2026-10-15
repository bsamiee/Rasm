use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, res::MaybeDef as _, sym};
use rustc_hir::{Expr, ExprKind, Item, ItemKind, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, TyCtxt, print::with_forced_trimmed_paths};

use super::io_error;
use crate::{delivered, is_error, is_text, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    CONTEXTLESS_IO_ERROR,
    Warn,
    "error types taking every `io::Error` through `From`, and `io::Error` values built from text"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ContextlessIoError {
    io_error: Option<DefId>,
}

impl_lint_pass!(ContextlessIoError => [CONTEXTLESS_IO_ERROR]);

impl ContextlessIoError {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { io_error: io_error(tcx) } }
}

impl<'tcx> LateLintPass<'tcx> for ContextlessIoError {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(implementation) = item.kind
            && implementation.of_trait.is_some()
            && !item.span.from_expansion()
            && let conversion = cx.tcx.impl_trait_ref(item.owner_id).instantiate_identity().skip_normalization()
            && conversion.def_id.is_diag_item(cx, sym::From)
            && let Some(io_error) = self.io_error
            && conversion.args.type_at(1).opt_def_id() == Some(io_error)
            && let implementor = conversion.self_ty()
            && let ty::Adt(adt, _) = implementor.kind()
            && let Some(local) = adt.did().as_local()
            && is_error(cx, implementor)
        {
            let holder = adt
                .variants()
                .iter()
                .find(|variant| variant.fields.iter().any(|field| cx.tcx.type_of(field.did).instantiate_identity().skip_normalization().opt_def_id() == Some(io_error)))
                .and_then(|variant| variant.def_id.as_local())
                .unwrap_or(local);
            span_lint_hir_and_then(
                cx,
                CONTEXTLESS_IO_ERROR,
                cx.tcx.local_def_id_to_hir_id(holder),
                cx.tcx.def_span(holder),
                with_forced_trimmed_paths!(format!("`{implementor}` takes every `io::Error` through `From` and keeps no operation or subject")),
                |diag| {
                    diag.help("declare a variant per operation holding its subject beside the `io::Error` source and map at the call");
                },
            );
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(_, arguments) = expr.kind
            && let Some(io_error) = self.io_error
            && !expr.span.from_expansion()
            && let Some(callee) = fn_def_id(cx, expr)
            && callee.assoc_fn_parent(cx).opt_impl_ty(cx).is_some_and(|implemented| implemented.opt_def_id() == Some(io_error))
            && arguments.iter().any(|argument| is_text(cx, cx.typeck_results().expr_ty(argument)))
        {
            let enclosing = cx.tcx.typeck_root_def_id(cx.tcx.hir_enclosing_body_owner(expr.hir_id).to_def_id());
            let streamed =
                matches!(cx.tcx.def_kind(enclosing), DefKind::Fn | DefKind::AssocFn) && result_parts(cx, delivered(cx.tcx, enclosing)).is_some_and(|(_, error)| error.opt_def_id() == Some(io_error));
            span_lint_hir_and_then(cx, CONTEXTLESS_IO_ERROR, expr.hir_id, expr.span, "`io::Error` built from text holds a failure callers cannot match", |diag| {
                diag.help(if streamed {
                    "pass a typed error as the payload: `io::Error::new(kind, <thiserror value>)`"
                } else {
                    "return a `thiserror` variant holding the subject and its typed source"
                });
            });
        }
    }
}
