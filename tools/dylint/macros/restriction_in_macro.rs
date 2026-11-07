use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    macros::{first_node_macro_backtrace, is_panic, macro_backtrace},
    paths::PathNS,
    res::MaybeDef as _,
    sym,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    Body, Expr, ExprKind, LetStmt,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, Lint};
use rustc_middle::ty::TyCtxt;
use rustc_span::{ExpnId, Span};

use crate::{expanded_externally, impl_owner, item_path, paths, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, (&'static Lint, (&'static str, &'static str))>,
    pins: FxHashSet<DefId>,
    pin_sites: FxHashMap<LocalDefId, FxHashSet<ExpnId>>,
}

lint! {
    Pass;
    UNWRAP_USED_IN_MACRO: "`unwrap` and `unwrap_err` on `Option` and `Result` inside a local `macro_rules!` expansion",
    EXPECT_USED_IN_MACRO: "`expect` and `expect_err` on `Option` and `Result` inside a local `macro_rules!` expansion",
    PANIC_IN_MACRO: "`panic!`, `todo!`, `unimplemented!`, and `unreachable!` inside a local `macro_rules!` expansion",
    PRINT_IN_MACRO: "`print!`, `println!`, `eprint!`, and `eprintln!` inside a local `macro_rules!` expansion",
    USE_DEBUG_IN_MACRO: "`{:?}` in `print!`, `println!`, `eprint!`, `eprintln!`, `write!`, and `writeln!` inside a local `macro_rules!` expansion",
    TOKIO_PIN: "Tokio pinning macros with the standard `core::pin::pin!` replacement outside external macro output",
}

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        const ON_NONE: (&str, &str) = ("`None`", "convert `None` into an error with `ok_or_else` and return it with `?`");
        const ON_ERR: (&str, &str) = ("`Err`", "return the error with `?`");
        const ON_OK: (&str, &str) = ("`Ok`", "match the `Result` and handle `Ok`");
        Self {
            methods: [(sym::option_unwrap, (UNWRAP_USED_IN_MACRO, ON_NONE)), (sym::option_expect, (EXPECT_USED_IN_MACRO, ON_NONE))]
                .into_iter()
                .filter_map(|(method, reported)| Some((tcx.get_diagnostic_item(method)?, reported)))
                .chain(resolve(
                    tcx,
                    PathNS::Value,
                    &[
                        ("core::result::Result::unwrap", (UNWRAP_USED_IN_MACRO, ON_ERR)),
                        ("core::result::Result::unwrap_err", (UNWRAP_USED_IN_MACRO, ON_OK)),
                        ("core::result::Result::expect", (EXPECT_USED_IN_MACRO, ON_ERR)),
                        ("core::result::Result::expect_err", (EXPECT_USED_IN_MACRO, ON_OK)),
                    ],
                ))
                .collect(),
            pins: paths(tcx, PathNS::Macro, &["tokio::pin"]),
            pin_sites: FxHashMap::default(),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if !self.pins.is_empty()
            && let Some(call) = macro_backtrace(local.span).find(|call| self.pins.contains(&call.def_id) && !expanded_externally(cx, call.span))
            && self.pin_sites.entry(cx.tcx.hir_enclosing_body_owner(local.hir_id)).or_default().insert(call.expn)
        {
            span_lint_hir_and_then(cx, TOKIO_PIN, local.hir_id, call.span, format!("`{}!` duplicates standard pinning", cx.tcx.def_path_str(call.def_id)), |diag| {
                diag.help("bind pinned values with `core::pin::pin!`");
            });
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if !expr.span.from_expansion() {
            return;
        }
        let local = |span: Span| span.from_expansion() && !expanded_externally(cx, span);
        let called = match expr.kind {
            ExprKind::Call(..) | ExprKind::MethodCall(..) => fn_def_id(cx, expr),
            _ => None,
        };
        let found = match expr.kind {
            ExprKind::Call(..) | ExprKind::MethodCall(..)
                if local(expr.span)
                    && let Some(method) = called
                    && let Some(&(lint, (failure, help))) = self.methods.get(&method) =>
            {
                let span = match expr.kind {
                    ExprKind::MethodCall(segment, ..) => segment.ident.span.to(expr.span.shrink_to_hi()),
                    _ => expr.span,
                };
                Some((lint, span, format!("`{}` in a local macro expansion panics on {failure}", item_path(cx, method)), String::from(help)))
            }
            ExprKind::Call(..) if called.is_some_and(|function| cx.tcx.item_name(function) == sym::new_debug && impl_owner(cx.tcx, function).is_lang_item(cx, LangItem::FormatArgument)) =>
                macro_backtrace(expr.span)
                    .find(|call| local(call.span))
                    .filter(|call| {
                        matches!(cx.tcx.get_diagnostic_name(call.def_id), Some(sym::print_macro | sym::println_macro | sym::eprint_macro | sym::eprintln_macro | sym::write_macro | sym::writeln_macro))
                    })
                    .map(|call| {
                        (
                            USE_DEBUG_IN_MACRO,
                            call.span,
                            format!("`{}!` in a local macro expansion formats through `Debug`", cx.tcx.item_name(call.def_id)),
                            String::from("format through `Display` with `{}`"),
                        )
                    }),
            _ => first_node_macro_backtrace(cx, expr).filter(|call| local(call.span)).find_map(|call| {
                let name = cx.tcx.item_name(call.def_id);
                let printed =
                    |stream: &str| Some((PRINT_IN_MACRO, call.span, format!("`{name}!` in a local macro expansion writes to {stream}"), format!("emit a `tracing` event in place of `{name}!`")));
                match cx.tcx.get_diagnostic_name(call.def_id)? {
                    sym::print_macro | sym::println_macro => printed("stdout"),
                    sym::eprint_macro | sym::eprintln_macro => printed("stderr"),
                    sym::todo_macro | sym::unimplemented_macro =>
                        Some((PANIC_IN_MACRO, call.span, format!("`{name}!` in a local macro expansion panics"), format!("replace `{name}!` with the implementation"))),
                    diagnostic if is_panic(cx, call.def_id) || diagnostic == sym::unreachable_macro => Some((
                        PANIC_IN_MACRO,
                        call.span,
                        format!("`{name}!` in a local macro expansion panics"),
                        String::from("return a typed error for a failure the input causes, or state the invariant in the types"),
                    )),
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

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) { self.pin_sites.remove(&cx.tcx.hir_body_owner_def_id(body.id())); }
}
