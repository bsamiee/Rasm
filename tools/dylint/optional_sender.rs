use clippy_utils::{as_some_pattern, diagnostics::span_lint_hir_and_then, get_parent_expr, higher::IfLet, paths::PathNS, res::MaybeDef as _, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, HirId, Mutability, Pat, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, Ty, TyCtxt};

use crate::{Ledger, Place, generated, paths, place, receiving_call};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    OPTIONAL_SENDER,
    Warn,
    "`Option`s of receiverless channel senders that every use sends through only when present"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct OptionalSender {
    senders: FxHashSet<DefId>,
    places: Ledger<Vec<HirId>>,
}

impl_lint_pass!(OptionalSender => [OPTIONAL_SENDER]);

impl OptionalSender {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { senders: paths(tcx, PathNS::Type, &["tokio::sync::watch::Sender", "tokio::sync::broadcast::Sender"]), places: Ledger::default() } }
}

impl<'tcx> LateLintPass<'tcx> for OptionalSender {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn guarded<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<HirId> {
            let borrowed = receiving_call(cx, expr)
                .filter(|&(_, method)| cx.tcx.item_name(method) == sym::as_ref)
                .map(|(call, _)| call)
                .or_else(|| get_parent_expr(cx, expr).filter(|parent| matches!(parent.kind, ExprKind::AddrOf(_, Mutability::Not, _))))?;
            let tested = get_parent_expr(cx, borrowed).filter(|tested| matches!(tested.kind, ExprKind::Let(test) if test.init.hir_id == borrowed.hir_id && as_some_pattern(cx, test.pat).is_some()))?;
            let branch = get_parent_expr(cx, tested)?;
            IfLet::hir(cx, branch).filter(|branched| branched.if_else.is_none() && branched.let_expr.hir_id == borrowed.hir_id).map(|_| branch.hir_id)
        }

        if let Some(held) = place(cx, expr).filter(|held| matches!(held, Place::Field(_) | Place::Local(_)) && !held.exported(cx))
            && sender(cx, &self.senders, cx.typeck_results().expr_ty(expr))
            && !generated(cx, expr.hir_id)
        {
            match guarded(cx, expr) {
                Some(site) =>
                    if let Some(guarded) = self.places.record(held, Vec::new) {
                        guarded.push(site);
                    },
                None => self.places.escape(held),
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.places.escape_bound(cx, pat, |held| sender(cx, &self.senders, held)); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (held, site) in self.places.held(cx).filter_map(|(held, guarded)| Some((held, *guarded.first()?))) {
            span_lint_hir_and_then(
                cx,
                OPTIONAL_SENDER,
                held.hir_id(cx),
                held.span(cx),
                format!("`{}` wraps a channel sender in `Option` though every use sends only when it is present", held.name(cx)),
                |diag| {
                    diag.span_note(cx.tcx.hir_span(site), "the sender is used only when present here");
                    diag.help("hold the sender always, a sender with no receiver accepts every send, and leave absence to the consumer that subscribes");
                },
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn sender<'tcx>(cx: &LateContext<'tcx>, senders: &FxHashSet<DefId>, held: Ty<'tcx>) -> bool {
    matches!(held.kind(), ty::Adt(option, args) if option.is_diag_item(cx, sym::Option) && args.type_at(0).ty_adt_def().is_some_and(|sender| senders.contains(&sender.did())))
}
