use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, res::MaybeDef as _, sym};
use rustc_hir::{Expr, ExprKind, Item, ItemKind, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt, print::with_forced_trimmed_paths};

use crate::{awaited_output, expanded_externally, impl_owner, is_error, is_text, path, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    io_error: Option<DefId>,
}

lint!(Pass; IO_ERROR_WITHOUT_CONTEXT: "error types that take every `io::Error` through `From`, and `io::Error` values built from text");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { io_error: path(tcx, PathNS::Type, "std::io::Error") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(implementation) = item.kind
            && implementation.of_trait.is_some()
            && !expanded_externally(cx, item.span)
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
                IO_ERROR_WITHOUT_CONTEXT,
                cx.tcx.local_def_id_to_hir_id(holder),
                cx.tcx.def_span(holder),
                with_forced_trimmed_paths!(format!("`{implementor}` takes every `io::Error` through `From` and keeps no operation or subject")),
                |diag| {
                    diag.help("declare a variant holding an operation enum, the subject, and the `io::Error` source, and build it with `map_err` at each call");
                },
            );
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(_, arguments) = expr.kind
            && let Some(io_error) = self.io_error
            && !expanded_externally(cx, expr.span)
            && let Some(callee) = fn_def_id(cx, expr)
            && impl_owner(cx.tcx, callee).is_some_and(|implemented| implemented.opt_def_id() == Some(io_error))
            && arguments.iter().any(|argument| is_text(cx, cx.typeck_results().expr_ty(argument)))
        {
            let enclosing = cx.tcx.typeck_root_def_id(cx.tcx.hir_enclosing_body_owner(expr.hir_id).to_def_id());
            let streamed = matches!(cx.tcx.def_kind(enclosing), DefKind::Fn | DefKind::AssocFn)
                && result_parts(cx, awaited_output(cx.tcx, enclosing)).is_some_and(|(_, error)| error.opt_def_id() == Some(io_error));
            span_lint_hir_and_then(cx, IO_ERROR_WITHOUT_CONTEXT, expr.hir_id, expr.span, "`io::Error` built from text holds a failure callers cannot match", |diag| {
                diag.help(if streamed { "build it from a typed error: `io::Error::new(kind, <thiserror value>)`" } else { "return a `thiserror` variant holding the subject and its typed source" });
            });
        }
    }
}
