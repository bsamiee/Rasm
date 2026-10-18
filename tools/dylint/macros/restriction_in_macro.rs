use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    macros::{first_node_macro_backtrace, is_panic, macro_backtrace},
    paths::PathNS,
    res::MaybeDef as _,
    sym,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, Lint};
use rustc_middle::ty::TyCtxt;
use rustc_span::Span;

use crate::{expanded_externally, impl_owner, item_path, path, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, &'static Lint>,
    map_err: Option<DefId>,
}

lint! {
    Pass;
    UNWRAP_USED_IN_MACRO: "`unwrap` and `unwrap_err` on `Option` and `Result` inside a local `macro_rules!` expansion",
    EXPECT_USED_IN_MACRO: "`expect` and `expect_err` on `Option` and `Result` inside a local `macro_rules!` expansion",
    MAP_ERR_IGNORE_IN_MACRO: "`map_err(|_| ..)` on `Result` inside a local `macro_rules!` expansion",
    PANIC_IN_MACRO: "`panic!`, `todo!`, `unimplemented!`, and `unreachable!` inside a local `macro_rules!` expansion",
    PRINT_IN_MACRO: "`print!`, `println!`, `eprint!`, and `eprintln!` inside a local `macro_rules!` expansion",
    USE_DEBUG_IN_MACRO: "`{:?}` in `print!`, `println!`, `eprint!`, `eprintln!`, `write!`, and `writeln!` inside a local `macro_rules!` expansion",
}

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            methods: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("core::option::Option::unwrap", UNWRAP_USED_IN_MACRO),
                    ("core::result::Result::unwrap", UNWRAP_USED_IN_MACRO),
                    ("core::result::Result::unwrap_err", UNWRAP_USED_IN_MACRO),
                    ("core::option::Option::expect", EXPECT_USED_IN_MACRO),
                    ("core::result::Result::expect", EXPECT_USED_IN_MACRO),
                    ("core::result::Result::expect_err", EXPECT_USED_IN_MACRO),
                ],
            ),
            map_err: path(tcx, PathNS::Value, "core::result::Result::map_err"),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let local = |span: Span| span.from_expansion() && !expanded_externally(cx, span);
        let called = fn_def_id(cx, expr);
        let found = match expr.kind {
            ExprKind::MethodCall(segment, _, [argument], _)
                if local(expr.span)
                    && called.is_some_and(|method| self.map_err == Some(method))
                    && let ExprKind::Closure(closure) = argument.kind
                    && let [parameter] = cx.tcx.hir_body(closure.body).params
                    && matches!(parameter.pat.kind, PatKind::Wild) =>
                Some((
                    MAP_ERR_IGNORE_IN_MACRO,
                    segment.ident.span.to(expr.span.shrink_to_hi()),
                    String::from("`map_err(|_| ..)` in a local macro expansion discards the error"),
                    "bind the error and keep it as the source of the new error",
                )),
            ExprKind::MethodCall(segment, ..)
                if local(expr.span)
                    && let Some(method) = called
                    && let Some(&lint) = self.methods.get(&method) =>
                Some((
                    lint,
                    segment.ident.span.to(expr.span.shrink_to_hi()),
                    format!("`{}` in a local macro expansion can panic", item_path(cx, method)),
                    "match the value and return the failure through the result type",
                )),
            ExprKind::Call(..) if called.is_some_and(|function| cx.tcx.item_name(function) == sym::new_debug && impl_owner(cx.tcx, function).is_lang_item(cx, LangItem::FormatArgument)) =>
                macro_backtrace(expr.span)
                    .find(|call| local(call.span))
                    .filter(|call| {
                        matches!(cx.tcx.get_diagnostic_name(call.def_id), Some(sym::print_macro | sym::println_macro | sym::eprint_macro | sym::eprintln_macro | sym::write_macro | sym::writeln_macro))
                    })
                    .map(|call| (USE_DEBUG_IN_MACRO, call.span, format!("`{}!` in a local macro expansion formats through `Debug`", cx.tcx.item_name(call.def_id)), "format through `Display`")),
            _ => first_node_macro_backtrace(cx, expr).filter(|call| local(call.span)).find_map(|call| {
                let name = cx.tcx.item_name(call.def_id);
                match cx.tcx.get_diagnostic_name(call.def_id)? {
                    sym::print_macro | sym::println_macro | sym::eprint_macro | sym::eprintln_macro =>
                        Some((PRINT_IN_MACRO, call.span, format!("`{name}!` in a local macro expansion writes to the console"), "record the value through `tracing` at the boundary")),
                    diagnostic if is_panic(cx, call.def_id) || matches!(diagnostic, sym::todo_macro | sym::unimplemented_macro | sym::unreachable_macro) =>
                        Some((PANIC_IN_MACRO, call.span, format!("`{name}!` in a local macro expansion panics"), "return the failure through the result type, or state the invariant in the types")),
                    _ => None,
                }
            }),
        };
        if let Some((lint, span, message, help)) = found {
            span_lint_hir_and_then(cx, lint, expr.hir_id, span, message, |diag| {
                diag.help(help);
            });
        }
    }
}
