use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    paths::PathNS,
    res::{MaybeDef as _, MaybeTypeckRes as _},
    sugg::Sugg,
    sym,
    ty::{implements_trait, option_arg_ty},
};
use rustc_data_structures::fx::FxHashSet;
use rustc_errors::Applicability;
use rustc_hir::{BorrowKind, Expr, ExprKind, LetStmt, MatchSource, Mutability, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};

use crate::{expanded_externally, path, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    views: FxHashSet<DefId>,
}

lint!(Pass; MANUAL_DEREF_PATTERN: "`as_str`, `as_slice`, `as_deref`, or `&*` scrutinees that deref a value for its patterns");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { views: [sym::string_as_str, sym::vec_as_slice].into_iter().filter_map(|view| tcx.get_diagnostic_item(view)).chain(path(tcx, PathNS::Value, "core::option::Option::as_deref")).collect() }
    }

    fn scrutinee<'tcx>(&self, cx: &LateContext<'tcx>, scrutinee: &'tcx Expr<'tcx>) {
        let derefs_purely = |owned: Ty<'tcx>| cx.tcx.lang_items().deref_pure_trait().is_some_and(|pure| implements_trait(cx, owned, pure, &[]));
        let typeck = cx.typeck_results();
        let viewed = match scrutinee.kind {
            ExprKind::MethodCall(view, receiver, [], _)
                if cx.ty_based_def(scrutinee).opt_def_id().is_some_and(|resolved| self.views.contains(&resolved))
                    && let received = typeck.expr_ty(receiver).peel_refs()
                    && derefs_purely(option_arg_ty(cx, received).unwrap_or(received)) =>
                Some((receiver, format!("`{}`", view.ident))),
            ExprKind::AddrOf(BorrowKind::Ref, Mutability::Not, &Expr { kind: ExprKind::Unary(UnOp::Deref, owner), .. })
                if let owned = typeck.expr_ty(owner)
                    && !owned.is_ref()
                    && derefs_purely(owned) =>
                Some((
                    match owner.kind {
                        ExprKind::Unary(UnOp::Deref, referenced) if typeck.expr_ty(referenced).is_ref() => referenced,
                        _ => owner,
                    },
                    "`&*`".to_owned(),
                )),
            _ => None,
        };
        if let Some((receiver, view)) = viewed
            && !expanded_externally(cx, scrutinee.span)
        {
            let mut applicability = Applicability::MaybeIncorrect;
            let borrowed = typeck.expr_ty(receiver).is_ref();
            let receiver = Sugg::hir_with_context(cx, receiver, scrutinee.span.ctxt(), "..", &mut applicability);
            let message = format!("{view} derefs `{receiver}` for its patterns");
            let matched = if borrowed { receiver } else { receiver.addr() };
            span_lint_hir_and_then(cx, MANUAL_DEREF_PATTERN, scrutinee.hir_id, scrutinee.span, message, |diag| {
                suggest(cx, scrutinee.hir_id, diag, scrutinee.span, format!("match `{matched}` under `#![feature(deref_patterns)]`"), matched, applicability);
            });
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let (Some(init), Some(_)) = (local.init, local.els) {
            self.scrutinee(cx, init);
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        match expr.kind {
            ExprKind::Match(scrutinee, _, MatchSource::Normal) => self.scrutinee(cx, scrutinee),
            ExprKind::Let(binding) => self.scrutinee(cx, binding.init),
            _ => {}
        }
    }
}
