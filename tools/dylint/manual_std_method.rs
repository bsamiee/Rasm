use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    eq_expr_value, fn_def_id, get_parent_expr,
    higher::{Range, RangeTy},
    is_integer_literal, path_to_local_with_projections,
    paths::{PathNS, lookup_path_str},
    res::{MaybeDef as _, MaybeResPath as _, MaybeTypeckRes as _},
    source::snippet,
    sugg::Sugg,
    sym,
    usage::local_used_after_expr,
    visitors::for_each_expr,
};
use rustc_ast::{AssignOpKind, BinOpKind, LitKind};
use rustc_attr_ir::lang_items::LangItem;
use rustc_errors::Applicability;
use rustc_hir::{BorrowKind, Expr, ExprKind, ImplItem, ImplItemKind, Mutability, PatKind, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, TyCtxt};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_STD_METHOD,
    Warn,
    "hand-written forms a std method or type states directly"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualStdMethod {
    lossy: Option<DefId>,
}

impl_lint_pass!(ManualStdMethod => [MANUAL_STD_METHOD]);

impl ManualStdMethod {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { lossy: lookup_path_str(tcx, PathNS::Value, "alloc::string::String::from_utf8_lossy").into_iter().next() } }
}

impl<'tcx> LateLintPass<'tcx> for ManualStdMethod {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if expr.span.from_expansion() {
            return;
        }
        let typeck = cx.typeck_results();
        let text = |part: &Expr<'_>| snippet(cx, part.span, "..");
        let negated = if let ExprKind::Assign(place, value, _) = expr.kind
            && let ExprKind::Unary(UnOp::Not, negated) = value.kind
            && eq_expr_value(cx, expr.span.ctxt(), place, negated)
        {
            Some(place)
        } else if let ExprKind::AssignOp(operator, place, value) = expr.kind
            && operator.node == AssignOpKind::BitXorAssign
            && let ExprKind::Lit(literal) = value.kind
            && matches!(literal.node, LitKind::Bool(true))
        {
            Some(place)
        } else {
            None
        };
        if let Some(place) = negated.filter(|place| typeck.expr_ty(place).is_bool()) {
            span_lint_hir_and_then(cx, MANUAL_STD_METHOD, expr.hir_id, expr.span, "assignment negates the `bool` it writes", |diag| {
                let mut applicability = Applicability::MachineApplicable;
                let receiver = if let ExprKind::Unary(UnOp::Deref, referent) = place.kind { referent } else { place };
                let toggled = Sugg::hir_with_context(cx, receiver, expr.span.ctxt(), "..", &mut applicability).maybe_paren();
                diag.span_suggestion(expr.span, "call `bool::toggle`", format!("{toggled}.toggle()"), applicability);
            });
        }
        if let ExprKind::MethodCall(_, mapped, [], _) = expr.kind
            && cx.ty_based_def(expr).opt_def_id().is_some_and(|collect| collect.is_diag_item(cx, sym::iterator_collect_fn))
            && typeck.expr_ty(expr).ty_adt_def().is_some_and(|built| built.is_diag_item(cx, sym::Vec))
            && let ExprKind::MethodCall(segment, range, [producer], _) = mapped.kind
            && segment.ident.name == sym::map
            && cx.ty_based_def(mapped).opt_def_id().and_then(|map| cx.tcx.trait_of_assoc(map)).is_some_and(|iterator| iterator.is_diag_item(cx, sym::Iterator))
            && let Some(Range { ty: RangeTy::OpsRange, start: Some(start), end: Some(end), span: _ }) = Range::hir(cx, range)
            && is_integer_literal(start, 0)
            && typeck.expr_ty(end).is_usize()
            && !matches!(producer.kind, ExprKind::Closure(closure) if matches!(cx.tcx.hir_body(closure.body).params, [parameter] if matches!(parameter.pat.kind, PatKind::Wild)))
        {
            span_lint_hir_and_then(cx, MANUAL_STD_METHOD, expr.hir_id, expr.span, "`(0..n).map(f).collect()` builds a `Vec` from each index", |diag| {
                diag.span_suggestion(expr.span, "call `Vec::from_fn`", format!("Vec::from_fn({}, {})", text(end), text(producer)), Applicability::MachineApplicable);
            });
        }
        let source = if let ExprKind::Call(_, [source]) = expr.kind
            && fn_def_id(cx, expr).is_some_and(|called| called.is_diag_item(cx, sym::try_from_fn))
        {
            Some(source)
        } else if let ExprKind::MethodCall(_, source, [], _) = expr.kind
            && cx.ty_based_def(expr).opt_def_id().and_then(|called| cx.tcx.trait_of_assoc(called)).is_some_and(|converted| converted.is_diag_item(cx, sym::TryInto))
        {
            Some(source)
        } else {
            None
        };
        if let Some(source) = source
            && matches!(typeck.expr_ty_adjusted(source).kind(), ty::Ref(_, slice, _) if slice.is_slice())
            && let ty::Adt(_, arguments) = typeck.expr_ty(expr).kind()
            && let target = arguments.type_at(0)
            && let Some((mutable, copied)) = if let ty::Ref(_, array, mutability) = target.kind()
                && array.is_array()
            {
                Some((mutability.is_mut(), ""))
            } else {
                target.is_array().then_some((false, ".copied()"))
            }
        {
            let viewed = source.peel_borrows();
            let bounded = if let ExprKind::Index(base, index, _) = viewed.kind { Some((base, Range::hir(cx, index))) } else { None };
            let suffix = if mutable { "_mut" } else { "" };
            let (replacement, exact) = match bounded {
                Some((base, Some(Range { ty: RangeTy::OpsTo, start: None, end: Some(_), span: _ }))) => (format!("{}.first_chunk{suffix}(){copied}", text(base)), false),
                Some((base, Some(Range { ty: RangeTy::OpsFrom, start: Some(start), end: None, span: _ })))
                    if let ExprKind::Binary(operator, length, _) = start.kind
                        && operator.node == BinOpKind::Sub
                        && let ExprKind::MethodCall(segment, measured, [], _) = length.kind
                        && segment.ident.name == sym::len
                        && eq_expr_value(cx, expr.span.ctxt(), measured, base) =>
                    (format!("{}.last_chunk{suffix}(){copied}", text(base)), false),
                Some(_) | None => (format!("{}.as{suffix}_array(){copied}", text(viewed)), true),
            };
            let converted = get_parent_expr(cx, expr).filter(|parent| {
                matches!(parent.kind, ExprKind::MethodCall(_, receiver, [], _) if receiver.hir_id == expr.hir_id)
                    && cx.ty_based_def(*parent).opt_def_id().is_some_and(|ok| ok.is_diag_item(cx, sym::result_ok_method))
            });
            span_lint_hir_and_then(cx, MANUAL_STD_METHOD, expr.hir_id, expr.span, "a fallible conversion checks the length of a slice to view it as an array", |diag| {
                diag.span_suggestion(
                    converted.map_or(expr.span, |ok| ok.span),
                    "view the slice through the method returning `Option`",
                    replacement,
                    if exact && converted.is_some() { Applicability::MachineApplicable } else { Applicability::MaybeIncorrect },
                );
            });
        }
        if let ExprKind::MethodCall(segment, lossy, [], _) = expr.kind
            && [sym::into_owned, sym::to_string, sym::to_owned, sym::into].contains(&segment.ident.name)
            && typeck.expr_ty(expr).is_lang_item(cx, LangItem::String)
            && let ExprKind::Call(_, [borrowed]) = lossy.kind
            && fn_def_id(cx, lossy).is_some_and(|called| Some(called) == self.lossy)
            && let ExprKind::AddrOf(BorrowKind::Ref, Mutability::Not, bytes) = borrowed.kind
            && typeck.expr_ty(bytes).ty_adt_def().is_some_and(|owner| owner.is_diag_item(cx, sym::Vec))
            && let Some(local) = path_to_local_with_projections(bytes)
            && cx.tcx.hir_enclosing_body_owner(local) == cx.tcx.hir_enclosing_body_owner(expr.hir_id)
            && !local_used_after_expr(cx, local, expr)
            && (bytes.res_local_id().is_some() || typeck.node_type(local).ty_adt_def().is_some_and(|owner| owner.destructor(cx.tcx).is_none()))
        {
            span_lint_hir_and_then(cx, MANUAL_STD_METHOD, expr.hir_id, expr.span, "lossy decoding copies a byte buffer nothing reads afterwards", |diag| {
                diag.span_suggestion(expr.span, "decode the buffer in place", format!("String::from_utf8_lossy_owned({})", text(bytes)), Applicability::MachineApplicable);
            });
        }
    }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Fn(_, body) = item.kind
            && !item.span.from_expansion()
            && let Some(implementation) = cx.tcx.trait_impl_of_assoc(item.owner_id.to_def_id())
            && cx.tcx.impl_trait_id(implementation).is_lang_item(cx, LangItem::Drop)
            && let ty::Adt(guard, arguments) = cx.tcx.type_of(implementation).instantiate_identity().skip_normalization().kind()
            && guard.is_struct()
            && let [field] = &guard.non_enum_variant().fields.raw
            && let held = field.ty(cx.tcx, arguments).skip_normalization()
            && let closure = if let ty::Adt(wrapper, inner) = held.kind()
                && (wrapper.is_diag_item(cx, sym::Option) || wrapper.is_lang_item(cx, LangItem::ManuallyDrop))
            {
                inner.type_at(0)
            } else {
                held
            }
            && matches!(closure.kind(), ty::Param(_))
            && let typeck = cx.tcx.typeck_body(body)
            && for_each_expr(cx.tcx, cx.tcx.hir_body(body).value, |called| {
                if let ExprKind::Call(callee, _) = called.kind
                    && typeck.expr_ty(callee) == closure
                {
                    ControlFlow::Break(())
                } else {
                    ControlFlow::Continue(())
                }
            })
            .is_some()
        {
            span_lint_hir_and_then(cx, MANUAL_STD_METHOD, item.hir_id(), cx.tcx.def_span(guard.did()), format!("`{}` runs its one closure on drop", cx.tcx.item_name(guard.did())), |diag| {
                diag.help("hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed");
            });
        }
    }
}
