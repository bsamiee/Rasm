use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id,
    paths::PathNS,
    peel_hir_expr_refs,
    res::{MaybeDef as _, MaybeResPath as _},
    source::snippet,
    sym,
    usage::local_used_after_expr,
};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{cloned, expanded_externally, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    consuming: FxHashMap<DefId, Option<&'static str>>,
}

lint!(Pass; CLONED_COMMAND_MATCHES: "`clap::Command` bindings, parameters, and fields cloned for a consuming `[try_]get_matches[_from]` call");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let from_mut = Some("try_get_matches_from_mut");
        Self {
            consuming: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("clap::Command::get_matches", Some("get_matches_mut")),
                    ("clap::Command::try_get_matches", None),
                    ("clap::Command::get_matches_from", from_mut),
                    ("clap::Command::try_get_matches_from", from_mut),
                ],
            ),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, arguments, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(&replacement) = fn_def_id(cx, expr).and_then(|method| self.consuming.get(&method))
            && let copy = expr_or_init(cx, receiver)
            && let Some(source) = cloned(cx, copy).or_else(|| match copy.kind {
                ExprKind::MethodCall(_, source, [], _) if fn_def_id(cx, copy).opt_parent(cx).is_diag_item(cx, sym::ToOwned) => Some(source),
                _ => None,
            })
            && let source = peel_hir_expr_refs(source).0
            && (matches!(source.kind, ExprKind::Field(..)) || source.res_local_id().is_some_and(|local| cx.typeck_results().expr_ty(source).is_ref() || local_used_after_expr(cx, local, expr)))
        {
            let command = snippet(cx, source.span, "..");
            let help = replacement.map_or_else(
                || String::from("parse once through `Parser::try_parse`"),
                |replacement| format!("call `{replacement}({})` on a mutable borrow of `{command}`", arguments.first().map_or_default(|argument| snippet(cx, argument.span, ".."))),
            );
            span_lint_hir_and_then(cx, CLONED_COMMAND_MATCHES, expr.hir_id, expr.span, format!("`{}` consumes a clone of `{command}`", segment.ident), |diag| {
                diag.help(help);
            });
        }
    }
}
