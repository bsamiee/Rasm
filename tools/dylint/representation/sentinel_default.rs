use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_enclosing_block, get_parent_expr, is_default_equivalent,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    visitors::for_each_expr,
};
use rustc_ast::LitKind;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{BinOpKind, Expr, ExprKind, Node, PatExpr, PatExprKind, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::Span;

use crate::{expanded_externally, is_text, paths, pointee};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    fallbacks: FxHashSet<DefId>,
}

lint!(Pass; SENTINEL_DEFAULT: "`Option::unwrap_or_default`, `unwrap_or`, and `map_or` calls with a default fallback that a later `==`, `!=`, `is_empty`, or `match` tests as absence");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { fallbacks: paths(tcx, PathNS::Value, &["core::option::Option::unwrap_or_default", "core::option::Option::unwrap_or", "core::option::Option::map_or"]) }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let tested = |used: &'tcx Expr<'tcx>| -> Option<Span> {
            let parent = get_parent_expr(cx, used)?;
            let default_pattern = |kind: &PatKind<'_>| {
                matches!(*kind, PatKind::Expr(&PatExpr { kind: PatExprKind::Lit { lit, negated: false }, .. })
                if match lit.node {
                    LitKind::Int(value, _) => value.get() == 0,
                    LitKind::Str(text, _) => text.is_empty(),
                    LitKind::Bool(value) => !value,
                    _ => false,
                })
            };
            match parent.kind {
                ExprKind::Binary(operator, left, right)
                    if matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne) && is_default_equivalent(cx, if left.hir_id == used.hir_id { right } else { left }) =>
                    Some(parent.span),
                ExprKind::MethodCall(segment, receiver, [], _) if receiver.hir_id == used.hir_id && segment.ident.name == sym::is_empty => Some(parent.span),
                ExprKind::Match(scrutinee, arms, _) if scrutinee.hir_id == used.hir_id && arms.iter().any(|arm| default_pattern(&arm.pat.kind)) => Some(parent.span),
                _ => None,
            }
        };
        if let ExprKind::MethodCall(segment, _, arguments, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|method| self.fallbacks.contains(&method))
            && arguments.first().is_none_or(|fallback| is_default_equivalent(cx, fallback))
            && let held = cx.typeck_results().expr_ty(expr)
            && let content = pointee(cx, held)
            && (is_text(cx, held)
                || held.is_numeric()
                || held.is_bool()
                || held.is_char()
                || content.is_slice()
                || matches!(
                    content.opt_diag_name(cx),
                    Some(sym::Vec | sym::VecDeque | sym::BTreeMap | sym::BTreeSet | sym::HashMap | sym::HashSet | sym::Path | sym::PathBuf | sym::OsStr | sym::OsString)
                ))
            && let Some(test) = match cx.tcx.parent_hir_node(expr.hir_id) {
                Node::LetStmt(local) if let PatKind::Binding(_, binding, ..) = local.pat.kind => get_enclosing_block(cx, local.hir_id).and_then(|block| {
                    for_each_expr(cx.tcx, block, |used| {
                        if used.res_local_id() == Some(binding)
                            && let Some(test) = tested(used)
                        {
                            ControlFlow::Break(test)
                        } else {
                            ControlFlow::Continue(())
                        }
                    })
                }),
                _ => tested(expr),
            }
        {
            let (span, held) = (segment.ident.span.to(expr.span.shrink_to_hi()), with_forced_trimmed_paths!(held.to_string()));
            span_lint_hir_and_then(cx, SENTINEL_DEFAULT, expr.hir_id, span, format!("`{}` turns `None` into a `{held}` default that a later test reads as absence", segment.ident), |diag| {
                diag.span_note(test, "tested here");
                diag.help("keep the `Option` and match `None` where the test acts on absence");
            });
        }
    }
}
