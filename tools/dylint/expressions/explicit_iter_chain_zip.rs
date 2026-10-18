use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, res::MaybeDef as _, source::snippet_with_context, sugg::Sugg, sym, ty::make_normalized_projection};
use rustc_data_structures::fx::FxHashSet;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, Mutability, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};

use crate::{expanded_externally, paths, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    adapters: FxHashSet<DefId>,
}

lint!(Pass; EXPLICIT_ITER_CHAIN_ZIP: "`chain` and `zip` calls on a value converted through `into_iter`, `iter`, or `iter_mut` that `iter::chain` and `iter::zip` convert themselves");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { adapters: paths(tcx, PathNS::Value, &["core::iter::Iterator::chain", "core::iter::Iterator::zip"]) } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let (tcx, typeck) = (cx.tcx, cx.typeck_results());
        if let ExprKind::MethodCall(adapter, converted, [other], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|called| self.adapters.contains(&called))
            && let ExprKind::MethodCall(conversion, source, [], _) = converted.kind
            && let Some(into_iterator) = tcx.get_diagnostic_item(sym::IntoIterator)
            && let written = typeck.expr_ty(source)
            && let Some((passed, borrow)) = match (conversion.ident.name, written.kind()) {
                (sym::into_iter, _) if fn_def_id(cx, converted).opt_parent(cx) == Some(into_iterator) && typeck.expr_adjustments(source).is_empty() => Some((written, None)),
                (sym::iter, ty::Ref(_, _, Mutability::Not)) => Some((written, None)),
                (sym::iter, _) if !written.is_ref() => Some((Ty::new_imm_ref(tcx, tcx.lifetimes.re_erased, written), Some(Mutability::Not))),
                (sym::iter_mut, _) if !written.is_ref() => Some((Ty::new_mut_ref(tcx, tcx.lifetimes.re_erased, written), Some(Mutability::Mut))),
                _ => None,
            }
            && make_normalized_projection(tcx, cx.typing_env(), into_iterator, sym::IntoIter, [passed])
                .is_some_and(|iterator| tcx.erase_and_anonymize_regions(iterator) == tcx.erase_and_anonymize_regions(typeck.expr_ty(converted)))
        {
            let (ctxt, mut applicability) = (expr.span.ctxt(), Applicability::MaybeIncorrect);
            let shown = Sugg::hir_with_context(cx, source, ctxt, "..", &mut applicability);
            let message = format!("`{}` converts `{shown}` into an iterator only to call `{}`", conversion.ident, adapter.ident);
            let operand = match borrow {
                None => shown,
                Some(Mutability::Not) => shown.addr(),
                Some(Mutability::Mut) => shown.mut_addr(),
            };
            let (other, _) = snippet_with_context(cx, other.span, ctxt, "..", &mut applicability);
            span_lint_hir_and_then(cx, EXPLICIT_ITER_CHAIN_ZIP, expr.hir_id, expr.span, message, |diag| {
                let name = adapter.ident;
                suggest(cx, expr.hir_id, diag, expr.span, format!("call `iter::{name}` with `use core::iter;`"), format!("iter::{name}({operand}, {other})"), applicability);
            });
        }
    }
}
