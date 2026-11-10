use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::paths::PathNS;
use clippy_utils::source::snippet_with_context;
use clippy_utils::sugg::Sugg;
use clippy_utils::ty::make_normalized_projection;
use clippy_utils::{fn_def_id, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_errors::Applicability;
use rustc_hir::def_id::DefId;
use rustc_hir::{Expr, ExprKind, Mutability};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Instance, Ty, TyCtxt};

use crate::{associated_trait, expanded_externally, paths, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    adapters: FxHashSet<DefId>,
}

lint!(Pass; EXPLICIT_ITER_CHAIN_ZIP: "`chain` and `zip` calls on a value converted through `into_iter`, `iter`, or `iter_mut` that `iter::chain` and `iter::zip` convert themselves");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { adapters: paths(tcx, PathNS::Value, &["core::iter::Iterator::chain", "core::iter::Iterator::zip"]) } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let (tcx, typeck) = (cx.tcx, cx.typeck_results());
        let standard = |item: DefId| matches!(tcx.crate_name(item.krate), sym::core | sym::alloc | sym::std) && tcx.lookup_stability(item.krate.as_def_id()).is_some();
        if let ExprKind::MethodCall(adapter, converted, [other], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|called| self.adapters.contains(&called))
            && let ExprKind::MethodCall(conversion, source, [], _) = converted.kind
            && let Some(converting) = fn_def_id(cx, converted)
            && let Some(into_iterator) = tcx.get_diagnostic_item(sym::IntoIterator)
            && let consumes = associated_trait(tcx, converting) == Some(into_iterator)
            && let written = typeck.expr_ty(source)
            && (consumes || standard(converting))
            && (!consumes || !written.has_significant_drop(tcx, cx.typing_env()))
            && let Some((passed, borrow)) = match (conversion.ident.name, written.kind()) {
                (sym::into_iter, _) if consumes && typeck.expr_adjustments(source).is_empty() => Some((written, None)),
                (sym::iter, ty::Ref(_, _, Mutability::Not)) => Some((written, None)),
                (sym::iter, _) if !written.is_ref() => Some((Ty::new_imm_ref(tcx, tcx.lifetimes.re_erased, written), Some(Mutability::Not))),
                (sym::iter_mut, _) if !written.is_ref() => Some((Ty::new_mut_ref(tcx, tcx.lifetimes.re_erased, written), Some(Mutability::Mut))),
                _ => None,
            }
            && tcx
                .lang_items()
                .into_iter_fn()
                .is_some_and(|into_iter| matches!(Instance::try_resolve(tcx, cx.typing_env(), into_iter, tcx.mk_args(&[passed.into()])), Ok(Some(instance)) if standard(instance.def_id())))
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
