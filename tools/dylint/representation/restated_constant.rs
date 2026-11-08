use core::cell::OnceCell;

use clippy_utils::{
    consts::{Constant, mir_to_const},
    diagnostics::span_lint_hir_and_then,
};
use itertools::Itertools as _;
use rustc_ast::LitKind;
use rustc_crate_store::ExternCrate;
use rustc_data_structures::fx::{FxBuildHasher, FxHashMap, FxHashSet};
use rustc_errors::Applicability;
use rustc_hir::{
    Expr, ExprKind, HirId, Pat, PatExpr, PatExprKind, PatKind,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{Span, Symbol};

use crate::{expanded_externally, generated, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    named: OnceCell<FxHashMap<Symbol, Vec<(DefId, DefId)>>>,
    values: FxHashMap<DefId, Option<Symbol>>,
}

lint!(default Pass; RESTATED_CONSTANT: "string literals equal to a public `&str` constant of a direct dependency named after the uppercased literal");

impl Pass {
    fn restated(&mut self, cx: &LateContext<'_>, node: HirId, span: Span, literal: LitKind) {
        let (tcx, Self { named, values }) = (cx.tcx, self);
        let declared = gen {
            let mut visited = FxHashSet::default();
            let mut modules = tcx.crates(()).iter().filter(|&&krate| tcx.extern_crate(krate).is_some_and(ExternCrate::is_direct)).map(|krate| krate.as_def_id()).collect::<Vec<_>>();
            while let Some(module) = modules.pop() {
                if !visited.insert(module) {
                    continue;
                }
                for child in tcx.module_children(module).iter().filter(|child| child.vis.is_public()) {
                    match child.res {
                        Res::Def(DefKind::Mod, inner) => modules.push(inner),
                        Res::Def(DefKind::Const, constant) if tcx.lookup_deprecation(constant).is_none() => yield (child.ident.name, (constant, module)),
                        _ => (),
                    }
                }
            }
        };
        if let LitKind::Str(text, _) = literal
            && text.as_str().contains(['.', '_', '-'])
            && !expanded_externally(cx, span)
            && !generated(cx, node)
            && let name = Symbol::intern(&text.as_str().to_ascii_uppercase().replace(['.', '-'], "_"))
            && let Some(candidates) = named.get_or_init(|| declared.into_grouping_map_with_hasher(FxBuildHasher).collect()).get(&name)
            && let Some((_, module)) = candidates
                .iter()
                .copied()
                .filter(|&(constant, _)| {
                    *values.entry(constant).or_insert_with(|| {
                        let held = tcx.type_of(constant).instantiate_identity().skip_normalization();
                        held.peel_refs()
                            .is_str()
                            .then(|| tcx.const_eval_poly(constant).ok().and_then(|value| mir_to_const(tcx, value, held)))
                            .flatten()
                            .and_then(|value| if let Constant::Str(contents) = value { Some(Symbol::intern(&contents)) } else { None })
                    }) == Some(text)
                })
                .min_by_key(|&(_, module)| tcx.def_path_str(module))
        {
            let named = format!("{}::{name}", tcx.def_path_str(module));
            span_lint_hir_and_then(cx, RESTATED_CONSTANT, node, span, format!("`\"{text}\"` spells `{named}`"), |diag| {
                suggest(cx, node, diag, span, "replace the literal with the constant", &named, Applicability::MaybeIncorrect);
            });
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

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
