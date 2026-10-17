use core::ops::ControlFlow;

use clippy_utils::{diagnostics::span_lint_hir_and_then, higher::While, paths::PathNS, res::MaybeDef as _, sym, visitors::for_each_expr_without_closures};
use rustc_ast::LitKind;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, HirId, Pat, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::Span;

use crate::{Ledger, Polling, generated, held, rebound, receiving_call, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Method {
    Read,
    Set,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_CANCELLATION_TOKEN,
    Warn,
    "one-way `AtomicBool` stop flags a `while` loop polls"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Default)]
struct Usage {
    writes: Vec<Span>,
    polls: Vec<(HirId, &'static str)>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualCancellationToken {
    methods: FxHashMap<DefId, Method>,
    polling: Polling,
    flags: Ledger<Usage>,
}

impl_lint_pass!(ManualCancellationToken => [MANUAL_CANCELLATION_TOKEN]);

impl ManualCancellationToken {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            methods: resolve(
                tcx,
                PathNS::Value,
                &[("core::sync::atomic::Atomic::load", Method::Read), ("core::sync::atomic::Atomic::store", Method::Set), ("core::sync::atomic::Atomic::swap", Method::Set)],
            ),
            polling: Polling::new(tcx),
            flags: Ledger::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for ManualCancellationToken {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        const AWAITED: &str =
            "hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and await `token.cancelled()` or drive the loop through `token.run_until_cancelled(..)`";
        const POLLED: &str = "hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and poll `token.is_cancelled()` in the loop condition";

        let flag = |used: &'tcx Expr<'tcx>| held(cx, used).filter(|&(place, ty)| !place.exported(cx) && flagged(cx, ty)).map(|(place, _)| place);
        let method = |used: &'tcx Expr<'tcx>| receiving_call(cx, used).and_then(|(call, method)| Some((*self.methods.get(&method)?, call)));
        if let Some(polled) = While::hir(expr) {
            let help = if self.polling.polled(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id)) { AWAITED } else { POLLED };
            let _: Option<()> = for_each_expr_without_closures(polled.condition, |inner| {
                if let ExprKind::MethodCall(_, receiver, ..) = inner.kind
                    && let Some(flag) = flag(receiver)
                    && let Some((Method::Read, _)) = method(receiver)
                    && let Some(usage) = self.flags.record(flag, Usage::default)
                {
                    usage.polls.push((inner.hir_id, help));
                }
                ControlFlow::Continue(())
            });
        }
        if let Some(flag) = flag(expr)
            && !generated(cx, expr.hir_id)
        {
            match method(expr) {
                Some((Method::Set, call))
                    if let ExprKind::MethodCall(_, _, [set, ..], _) = call.kind
                        && let ExprKind::Lit(literal) = set.kind
                        && matches!(literal.node, LitKind::Bool(true)) =>
                    if let Some(usage) = self.flags.record(flag, Usage::default) {
                        usage.writes.push(call.span);
                    },
                Some((Method::Set, _)) => self.flags.escape(flag),
                None if !rebound(cx, expr) => self.flags.escape(flag),
                Some((Method::Read, _)) | None => {
                    self.flags.record(flag, Usage::default);
                }
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.flags.escape_bound(cx, pat, |held| flagged(cx, held)); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (flag, written, (polled, help)) in self.flags.held(cx).filter_map(|(flag, usage)| Some((flag, *usage.writes.first()?, *usage.polls.first()?))) {
            span_lint_hir_and_then(
                cx,
                MANUAL_CANCELLATION_TOKEN,
                flag.hir_id(cx),
                flag.span(cx),
                format!("`{}` is a one-way stop flag, every write sets `true` and a `while` loop polls it", flag.name(cx)),
                |diag| {
                    diag.span_note(written, "the flag is set here");
                    diag.span_note(cx.tcx.hir_span(polled), "the loop polls the flag here");
                    diag.help(help);
                },
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn flagged<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool { matches!(held.kind(), ty::Adt(atomic, args) if atomic.is_diag_item(cx, sym::Atomic) && args.type_at(0).is_bool()) }
