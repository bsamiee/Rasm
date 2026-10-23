use clippy_utils::{
    consts::{Constant, mir_to_const},
    diagnostics::span_lint_hir_and_then,
};
use rustc_ast::LitKind;
use rustc_crate_store::ExternCrate;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_errors::Applicability;
use rustc_hir::{
    Expr, ExprKind, HirId, Pat, PatExpr, PatExprKind, PatKind,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_span::{Span, Symbol};

use crate::{expanded_externally, generated, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    named: FxHashMap<Symbol, DefId>,
}

lint!(Pass; RESTATED_CONSTANT: "string literals equal to a public `&str` constant of a direct dependency named after the uppercased literal");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        fn constants(tcx: TyCtxt<'_>, module: DefId, visited: &mut FxHashSet<DefId>) -> Vec<DefId> {
            if visited.insert(module) {
                tcx.module_children(module)
                    .iter()
                    .filter(|child| child.vis.is_public())
                    .flat_map(|child| match child.res {
                        Res::Def(DefKind::Mod, inner) => constants(tcx, inner, visited),
                        Res::Def(DefKind::Const, constant) => vec![constant],
                        _ => Vec::new(),
                    })
                    .collect()
            } else {
                Vec::new()
            }
        }
        let mut visited = FxHashSet::default();
        Self {
            named: tcx
                .crates(())
                .iter()
                .filter(|&&krate| tcx.extern_crate(krate).is_some_and(ExternCrate::is_direct))
                .flat_map(|krate| constants(tcx, krate.as_def_id(), &mut visited))
                .filter(|&constant| tcx.lookup_deprecation(constant).is_none())
                .filter_map(|constant| {
                    let held = tcx.type_of(constant).instantiate_identity().skip_normalization();
                    if held.peel_refs().is_str()
                        && let Some(Constant::Str(text)) = tcx.const_eval_poly(constant).ok().and_then(|value| mir_to_const(tcx, value, held))
                        && text.contains(['.', '_', '-'])
                        && tcx.item_name(constant).as_str() == text.to_ascii_uppercase().replace(['.', '-'], "_")
                    {
                        Some((Symbol::intern(&text), constant))
                    } else {
                        None
                    }
                })
                .collect(),
        }
    }

    fn restated(&self, cx: &LateContext<'_>, node: HirId, span: Span, literal: LitKind) {
        if let LitKind::Str(text, _) = literal
            && let Some(&constant) = self.named.get(&text)
            && !expanded_externally(cx, span)
            && !generated(cx, node)
        {
            let named = cx.tcx.def_path_str(constant);
            span_lint_hir_and_then(cx, RESTATED_CONSTANT, node, span, format!("`\"{text}\"` spells `{named}`"), |diag| {
                suggest(cx, node, diag, span, "replace the literal with the constant", &named, Applicability::MaybeIncorrect);
            });
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Lit(literal) = expr.kind {
            self.restated(cx, expr.hir_id, expr.span, literal.node);
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::Expr(&PatExpr { kind: PatExprKind::Lit { lit, .. }, .. }) = pat.kind {
            self.restated(cx, pat.hir_id, pat.span, lit.node);
        }
    }
}
