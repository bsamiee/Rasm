use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, get_parent_expr, is_default_equivalent, paths::PathNS, sym};
use rustc_ast::LitKind;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{BinOpKind, Expr, ExprKind, Mutability, PatExpr, PatExprKind, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    fallbacks: FxHashSet<DefId>,
}

lint!(Pass; SENTINEL_DEFAULT: "`Option::unwrap_or_default`, `unwrap_or`, and `map_or` calls producing a native `Copy` default tested immediately");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { fallbacks: paths(tcx, PathNS::Value, &["core::option::Option::unwrap_or_default", "core::option::Option::unwrap_or", "core::option::Option::map_or"]) }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let ExprKind::MethodCall(segment, _, arguments, _) = expr.kind else { return };
        let typeck = cx.typeck_results();
        let held = typeck.expr_ty(expr);
        if expanded_externally(cx, expr.span)
            || !(held.is_numeric() || held.is_bool() || held.is_char() || matches!(held.kind(), ty::Ref(_, content, Mutability::Not) if content.is_str() || content.is_slice()))
            || !fn_def_id(cx, expr).is_some_and(|callee| self.fallbacks.contains(&callee))
            || !arguments.first().is_none_or(|fallback| is_default_equivalent(cx, fallback))
        {
            return;
        }
        let Some(parent) = get_parent_expr(cx, expr) else { return };
        let default_pattern = |kind: &PatKind<'_>| {
            matches!(*kind, PatKind::Expr(&PatExpr { kind: PatExprKind::Lit { lit, negated: false }, .. })
            if match lit.node {
                LitKind::Int(value, _) => value.get() == 0,
                LitKind::Str(text, _) => text.is_empty(),
                LitKind::Bool(value) => !value,
                LitKind::Char(value) => value == '\0',
                _ => false,
            })
        };
        let test = match parent.kind {
            ExprKind::Binary(operator, left, right)
                if let other = if left.hir_id == expr.hir_id { right } else { left }
                    && matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne)
                    && typeck.expr_ty_adjusted(other) == held
                    && is_default_equivalent(cx, other) =>
                Some(parent.span),
            ExprKind::MethodCall(method, receiver, [], _)
                if receiver.hir_id == expr.hir_id && method.ident.name == sym::is_empty && fn_def_id(cx, parent).is_some_and(|callee| cx.tcx.inherent_impl_of_assoc(callee).is_some()) =>
                Some(parent.span),
            ExprKind::Match(scrutinee, arms, _) if scrutinee.hir_id == expr.hir_id && arms.iter().any(|arm| default_pattern(&arm.pat.kind)) => Some(parent.span),
            _ => None,
        };
        if let Some(test) = test {
            let (span, held) = (segment.ident.span.to(expr.span.shrink_to_hi()), with_forced_trimmed_paths!(held.to_string()));
            span_lint_hir_and_then(cx, SENTINEL_DEFAULT, expr.hir_id, span, format!("`{}` replaces `None` with a `{held}` default tested immediately", segment.ident), |diag| {
                diag.span_note(test, "tested here");
                diag.help("test the `Option` directly with `is_none_or`, `is_some_and`, or a match; preserve mapping and `Some` values equal to the default");
            });
        }
    }
}
