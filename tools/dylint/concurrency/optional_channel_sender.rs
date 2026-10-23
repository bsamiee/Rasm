use clippy_utils::{as_some_pattern, diagnostics::span_lint_hir_and_then, get_parent_expr, higher::IfLet, paths::PathNS, res::MaybeDef as _, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, HirId, Mutability, Pat, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};

use super::PlaceUses;
use crate::{Place, generated, method_call_on, paths, place};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    senders: FxHashSet<DefId>,
    places: PlaceUses<(HirId, Ty<'tcx>)>,
}

lint!(Pass<'_>; OPTIONAL_CHANNEL_SENDER: "`Option`-wrapped `watch` and `broadcast` senders that every use tests with `if let Some` and no `else`");

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(tcx: TyCtxt<'tcx>) -> Self { Self { senders: paths(tcx, PathNS::Type, &["tokio::sync::watch::Sender", "tokio::sync::broadcast::Sender"]), places: PlaceUses::default() } }
}

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn guarded<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<HirId> {
            let borrowed = method_call_on(cx, expr)
                .filter(|&(_, method)| cx.tcx.item_name(method) == sym::as_ref)
                .map(|(call, _)| call)
                .or_else(|| get_parent_expr(cx, expr).filter(|parent| matches!(parent.kind, ExprKind::AddrOf(_, Mutability::Not, _))))?;
            let tested = get_parent_expr(cx, borrowed).filter(|tested| matches!(tested.kind, ExprKind::Let(test) if test.init.hir_id == borrowed.hir_id && as_some_pattern(cx, test.pat).is_some()))?;
            let branch = get_parent_expr(cx, tested)?;
            IfLet::hir(cx, branch).filter(|branched| branched.if_else.is_none() && branched.let_expr.hir_id == borrowed.hir_id).map(|_| branch.hir_id)
        }

        match place(cx, expr) {
            Some(held @ (Place::Field(_) | Place::Local(_)))
                if let Some(held_sender) = sender(cx, &self.senders, cx.typeck_results().expr_ty(expr))
                    && !generated(cx, expr.hir_id) =>
                match guarded(cx, expr) {
                    Some(site) => {
                        self.places.record(held, || (site, held_sender));
                    }
                    None => self.places.escape(held),
                },
            Some(Place::Field(_) | Place::Local(_) | Place::Static(_) | Place::Parameter(_)) | None => {}
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.places.escape_bound(cx, pat, |held| sender(cx, &self.senders, held).is_some()); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (held, &(site, held_sender)) in self.places.unescaped(cx) {
            span_lint_hir_and_then(cx, OPTIONAL_CHANNEL_SENDER, held.hir_id(cx), held.span(cx), format!("every use of `{}` tests it with `if let Some` and no `else`", held.name(cx)), |diag| {
                diag.span_note(cx.tcx.hir_span(site), "tested here");
                diag.help(format!("hold `{held_sender}` without `Option`"));
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn sender<'tcx>(cx: &LateContext<'tcx>, senders: &FxHashSet<DefId>, held: Ty<'tcx>) -> Option<Ty<'tcx>> {
    let ty::Adt(option, args) = held.kind() else { return None };
    option.is_diag_item(cx, sym::Option).then(|| args.type_at(0)).filter(|inner| inner.opt_def_id().is_some_and(|sender| senders.contains(&sender)))
}
