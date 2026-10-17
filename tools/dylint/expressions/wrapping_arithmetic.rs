use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, res::MaybeDef as _, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def::DefKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};
use rustc_span::Symbol;

use crate::{expanded_externally, impl_owner};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashSet<Symbol>,
}

lint!(Pass; WRAPPING_ARITHMETIC: "`wrapping_*` and `overflowing_*` integer arithmetic and `Wrapping` values, which wrap on overflow");

impl Pass {
    pub(crate) fn new(_: TyCtxt<'_>) -> Self {
        let operations = ["add", "sub", "mul", "div", "rem", "neg", "shl", "shr", "pow", "abs"];
        Self { methods: ["wrapping", "overflowing"].into_iter().flat_map(|prefix| operations.map(|operation| Symbol::intern(&format!("{prefix}_{operation}")))).collect() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let message = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::MethodCall(segment, ..)
                if let Some(method) = fn_def_id(cx, expr)
                    && self.methods.contains(&cx.tcx.item_name(method))
                    && impl_owner(cx.tcx, method).is_some_and(Ty::is_integral) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), format!("`{}` wraps on overflow", segment.ident))),
            ExprKind::Call(_, [_])
                if cx.typeck_results().expr_ty(expr).is_diag_item(cx, sym::Wrapping) && fn_def_id(cx, expr).is_some_and(|ctor| matches!(cx.tcx.def_kind(ctor), DefKind::Ctor(..))) =>
                Some((expr.span, String::from("`Wrapping` arithmetic wraps on overflow"))),
            _ => None,
        };
        if let Some((span, message)) = message {
            span_lint_hir_and_then(cx, WRAPPING_ARITHMETIC, expr.hir_id, span, message, |diag| {
                diag.help("call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain");
            });
        }
    }
}
