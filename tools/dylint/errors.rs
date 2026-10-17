use clippy_utils::{peel_blocks, res::MaybeDef as _, sym, ty::implements_trait, visitors::is_local_used};
use rustc_hir::{
    Expr, ExprKind, Pat, PatKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::LateContext;
use rustc_middle::ty::{self, GenericArg, Ty, TypeVisitableExt as _, fast_reject::DeepRejectCtxt};

use crate::{bounds_of, is_error, is_text, pointee};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum UntypedKind {
    Text,
    Code,
    Tuple,
    Unit,
    Erased,
    Report,
    Many,
    Messages,
    NoError(DefId),
}

impl UntypedKind {
    const fn reason(self) -> &'static str {
        match self {
            Self::Text => "text callers cannot match",
            Self::Code => "a primitive that names no failure",
            Self::Tuple => "a tuple that names no failure",
            Self::Unit => "a unit type that names no failure",
            Self::Erased => "an erased `dyn Error` callers cannot match",
            Self::Report => "a report that erases each failure's type",
            Self::Many => "a collection of errors that can be empty",
            Self::Messages => "a collection of text callers cannot match",
            Self::NoError(_) => "a `Display` type that implements no `core::error::Error`",
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn untyped_kind<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> Option<UntypedKind> {
    let (error_trait, display) = (cx.tcx.get_diagnostic_item(sym::Error)?, cx.tcx.get_diagnostic_item(sym::Display)?);
    match error.kind() {
        _ if error.is_unit() => Some(UntypedKind::Unit),
        _ if is_text(cx, error) => Some(UntypedKind::Text),
        ty::Int(_) | ty::Uint(_) | ty::Char | ty::Bool => Some(UntypedKind::Code),
        ty::Tuple(_) => Some(UntypedKind::Tuple),
        _ if matches!(pointee(cx, error).kind(), ty::Dynamic(predicates, ..) if predicates.principal_def_id() == Some(error_trait)) => Some(UntypedKind::Erased),
        ty::Adt(report, _)
            if !report.did().is_local()
                && !implements_trait(cx, error, error_trait, &[])
                && cx.tcx.get_diagnostic_item(sym::From).is_some_and(|from| {
                    cx.tcx.non_blanket_impls_for_ty(from, error).any(|implementation| {
                        let converted = cx.tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization();
                        let source = converted.args.type_at(1);
                        matches!(source.kind(), ty::Param(_))
                            && DeepRejectCtxt::relate_rigid_infer(cx.tcx).types_may_unify(error, converted.self_ty())
                            && !converted.self_ty().walk().any(|held| held == source.into())
                            && bounds_of(cx.tcx, implementation, source).any(|bound| bound.def_id() == error_trait)
                    })
                }) =>
            Some(UntypedKind::Report),
        ty::Adt(collection, args) if matches!(collection.opt_diag_name(cx), Some(sym::Vec | sym::BTreeSet)) =>
            is_error(cx, args.type_at(0)).then_some(UntypedKind::Many).or_else(|| is_text(cx, args.type_at(0)).then_some(UntypedKind::Messages)),
        _ => error
            .opt_def_id()
            .filter(|_| {
                !error.has_param()
                    && implements_trait(cx, error, display, &[])
                    && !implements_trait(cx, error, error_trait, &[])
                    && !error.walk().skip(1).filter_map(GenericArg::as_type).any(|held| is_error(cx, held))
            })
            .map(UntypedKind::NoError),
    }
}

fn body_output<'tcx>(cx: &LateContext<'tcx>, body: LocalDefId) -> Option<Ty<'tcx>> {
    let typeck = cx.typeck_results();
    let node = cx.tcx.local_def_id_to_hir_id(body);
    typeck
        .liberated_fn_sigs()
        .get(node)
        .map(|signature| signature.output())
        .or_else(|| if let ty::Coroutine(_, args) = typeck.node_type_opt(node)?.kind() { Some(args.as_coroutine().return_ty()) } else { None })
}

fn unread<'tcx>(cx: &LateContext<'tcx>, handler: &Expr<'tcx>) -> Option<(&'tcx Pat<'tcx>, &'tcx Expr<'tcx>)> {
    let ExprKind::Closure(closure) = handler.kind else { return None };
    let body = cx.tcx.hir_body(closure.body);
    let [parameter] = body.params else { return None };
    match parameter.pat.kind {
        PatKind::Wild => true,
        PatKind::Binding(_, binding, _, None) => !is_local_used(cx, &body.value, binding),
        _ => false,
    }
    .then(|| (parameter.pat, peel_blocks(&body.value)))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod debug_main_error;
pub(crate) mod discarded_error;
pub(crate) mod discarded_error_source;
pub(crate) mod foreign_error_return;
pub(crate) mod independent_question_marks;
pub(crate) mod io_error_without_context;
pub(crate) mod manual_error_impl;
pub(crate) mod partition_by_result;
pub(crate) mod redundant_error_conversion;
pub(crate) mod source_in_error_message;
pub(crate) mod stringified_error;
pub(crate) mod untyped_error;
