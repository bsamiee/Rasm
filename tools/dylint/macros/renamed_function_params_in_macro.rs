use core::iter;

use clippy_utils::{diagnostics::span_lint_hir_and_then, macros::macro_backtrace};
use rustc_hir::{ImplItem, ImplItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{Ident, MacroKind};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(RENAMED_FUNCTION_PARAMS_IN_MACRO: "trait impl methods a local `macro_rules!` expands with parameters the trait names differently");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Fn(_, body) = item.kind
            && let Some(expansion) = macro_backtrace(item.span).next().filter(|call| call.kind == MacroKind::Bang && call.def_id.is_local())
            && let Some(declared) = cx.tcx.trait_item_of(item.owner_id)
        {
            let named = |ident: Option<Ident>| ident.filter(|named| !named.name.as_str().starts_with('_'));
            let renamed = iter::zip(cx.tcx.fn_arg_idents(declared).iter().copied(), cx.tcx.hir_body_param_idents(body))
                .filter_map(|(trait_ident, impl_ident)| Some((named(trait_ident)?, named(impl_ident)?)).filter(|(trait_ident, impl_ident)| trait_ident.name != impl_ident.name));
            for (trait_ident, impl_ident) in renamed {
                span_lint_hir_and_then(
                    cx,
                    RENAMED_FUNCTION_PARAMS_IN_MACRO,
                    item.hir_id(),
                    impl_ident.span,
                    format!("`{}!` names the `{}` parameter `{trait_ident}` as `{impl_ident}`", cx.tcx.item_name(expansion.def_id), cx.tcx.def_path_str(declared)),
                    |diag| {
                        diag.span_note(expansion.span, "expanded here");
                        diag.help(format!("rename `{impl_ident}` to `{trait_ident}`"));
                    },
                );
            }
        }
    }
}
