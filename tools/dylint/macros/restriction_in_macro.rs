use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    macros::{first_node_macro_backtrace, is_panic},
    paths::PathNS,
    sym,
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, Lint};
use rustc_middle::ty::TyCtxt;
use rustc_span::Span;

use crate::{expanded_externally, item_path, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, &'static Lint>,
    reported: FxHashSet<Span>,
}

lint! {
    Pass;
    UNWRAP_USED_IN_MACRO: "`unwrap` and `unwrap_err` on `Option` and `Result` inside a local `macro_rules!` expansion",
    EXPECT_USED_IN_MACRO: "`expect` and `expect_err` on `Option` and `Result` inside a local `macro_rules!` expansion",
    PANIC_IN_MACRO: "`panic!`, `todo!`, `unimplemented!`, and `unreachable!` inside a local `macro_rules!` expansion",
    PRINT_IN_MACRO: "`print!`, `println!`, `eprint!`, `eprintln!`, and `dbg!` inside a local `macro_rules!` expansion",
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
            reported: FxHashSet::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let local = |span: Span| span.from_expansion() && !expanded_externally(cx, span);
        let found = if let ExprKind::MethodCall(segment, ..) = expr.kind
            && local(expr.span)
            && let Some(method) = fn_def_id(cx, expr)
            && let Some(&lint) = self.methods.get(&method)
        {
            Some((
                lint,
                segment.ident.span.to(expr.span.shrink_to_hi()),
                format!("`{}` in a local macro expansion can panic", item_path(cx, method)),
                "match the value and return the failure through the result type",
            ))
        } else {
            first_node_macro_backtrace(cx, expr).filter(|call| local(call.span)).find_map(|call| {
                let name = cx.tcx.item_name(call.def_id);
                match cx.tcx.get_diagnostic_name(call.def_id)? {
                    sym::print_macro | sym::println_macro | sym::eprint_macro | sym::eprintln_macro | sym::dbg_macro =>
                        Some((PRINT_IN_MACRO, call.span, format!("`{name}!` in a local macro expansion writes to the console"), "record the value through `tracing` at the boundary")),
                    diagnostic if is_panic(cx, call.def_id) || matches!(diagnostic, sym::todo_macro | sym::unimplemented_macro | sym::unreachable_macro) =>
                        Some((PANIC_IN_MACRO, call.span, format!("`{name}!` in a local macro expansion panics"), "return the failure through the result type, or state the invariant in the types")),
                    _ => None,
                }
            })
        };
        if let Some((lint, span, message, help)) = found
            && self.reported.insert(span)
        {
            span_lint_hir_and_then(cx, lint, expr.hir_id, span, message, |diag| {
                diag.help(help);
            });
        }
    }
}
