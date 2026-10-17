use core::iter;

use clippy_utils::{diagnostics::span_lint_hir_and_then, macros::macro_backtrace};
use rustc_hir::{ImplItem, ImplItemKind};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_span::{Ident, MacroKind, symbol::kw};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    RENAMED_MACRO_PARAMS,
    Warn,
    "trait impl methods a local `macro_rules!` expands with parameters the trait names differently"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] RenamedMacroParams => [RENAMED_MACRO_PARAMS]);

impl<'tcx> LateLintPass<'tcx> for RenamedMacroParams {
    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Fn(_, body) = item.kind
            && let Some(expansion) = macro_backtrace(item.span).next().filter(|call| call.kind == MacroKind::Bang && call.def_id.is_local())
            && let Some(declared) = cx.tcx.trait_item_of(item.owner_id)
        {
            let named = |ident: Option<Ident>| ident.filter(|named| named.name != kw::Underscore && !named.name.as_str().starts_with('_'));
            let renamed = iter::zip(cx.tcx.fn_arg_idents(declared).iter().copied(), cx.tcx.hir_body_param_idents(body))
                .filter_map(|(trait_ident, impl_ident)| Some((named(trait_ident)?, named(impl_ident)?)))
                .filter(|(trait_ident, impl_ident)| trait_ident.name != impl_ident.name);
            for (trait_ident, impl_ident) in renamed {
                span_lint_hir_and_then(
                    cx,
                    RENAMED_MACRO_PARAMS,
                    item.hir_id(),
                    impl_ident.span,
                    format!("`{}!` names the `{}` parameter `{trait_ident}` as `{impl_ident}`", cx.tcx.item_name(expansion.def_id), cx.tcx.def_path_str(declared)),
                    |diag| {
                        diag.span_note(expansion.span, "the method is expanded here");
                        diag.help(format!("name the parameter `{trait_ident}` in the macro or its invocation, the trait owns each parameter name"));
                    },
                );
            }
        }
    }
}
