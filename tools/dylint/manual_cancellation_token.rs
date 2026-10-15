use core::ops::ControlFlow;

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, higher::While, in_automatically_derived, paths::PathNS, res::MaybeDef as _, sym, visitors::for_each_expr_without_closures};
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{Expr, ExprKind, HirId, Pat, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::Span;

use crate::{Place, held, pattern_fields, rebound, resolve};

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
    polls: Vec<HirId>,
    escaped: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualCancellationToken {
    methods: FxHashMap<DefId, Method>,
    flags: FxIndexMap<Place, Usage>,
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
            flags: FxIndexMap::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for ManualCancellationToken {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let flag = |used: &'tcx Expr<'tcx>| held(cx, used).filter(|&(place, ty)| !place.exported(cx) && flagged(cx, ty)).map(|(place, _)| place);
        let method = |used: &'tcx Expr<'tcx>| {
            let call = get_parent_expr(cx, used).filter(|call| matches!(call.kind, ExprKind::MethodCall(_, receiver, ..) if receiver.hir_id == used.hir_id))?;
            Some((*self.methods.get(&cx.typeck_results().type_dependent_def_id(call.hir_id)?)?, call))
        };
        if let Some(polled) = While::hir(expr) {
            let _: Option<()> = for_each_expr_without_closures(polled.condition, |inner| {
                if let ExprKind::MethodCall(_, receiver, ..) = inner.kind
                    && let Some(flag) = flag(receiver)
                    && let Some((Method::Read, _)) = method(receiver)
                {
                    self.flags.entry(flag).or_default().polls.push(inner.hir_id);
                }
                ControlFlow::Continue(())
            });
        }
        if let Some(flag) = flag(expr)
            && !in_automatically_derived(cx.tcx, expr.hir_id)
        {
            let found = method(expr);
            let usage = self.flags.entry(flag).or_default();
            match found {
                Some((Method::Set, call))
                    if let ExprKind::MethodCall(_, _, [set, ..], _) = call.kind
                        && let ExprKind::Lit(literal) = set.kind
                        && matches!(literal.node, LitKind::Bool(true)) =>
                    usage.writes.push(call.span),
                Some((Method::Read, _)) => {}
                None if rebound(cx, expr) => {}
                Some((Method::Set, _)) | None => usage.escaped = true,
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let bound = pattern_fields(cx.typeck_results(), pat)
            .filter_map(|(field, _)| field.as_local())
            .filter(|&field| !cx.effective_visibilities.is_exported(field) && flagged(cx, cx.tcx.type_of(field).instantiate_identity().skip_normalization()))
            .collect::<Vec<_>>();
        for field in bound {
            self.flags.entry(Place::Field(field)).or_default().escaped = true;
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let stops = self.flags.iter().filter(|(flag, usage)| !usage.escaped && !flag.span(cx).from_expansion()).filter_map(|(flag, usage)| Some((flag, *usage.writes.first()?, *usage.polls.first()?)));
        for (flag, written, polled) in stops {
            span_lint_hir_and_then(
                cx,
                MANUAL_CANCELLATION_TOKEN,
                flag.hir_id(cx),
                flag.span(cx),
                format!("`{}` is a one-way stop flag, every write sets `true` and a `while` loop polls it", flag.name(cx)),
                |diag| {
                    diag.span_note(written, "the flag is set here");
                    diag.span_note(cx.tcx.hir_span(polled), "the loop polls the flag here");
                    diag.help(if cx.tcx.coroutine_is_async(cx.tcx.hir_enclosing_body_owner(polled).to_def_id()) {
                        "hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and await `token.cancelled()` or drive the loop through `token.run_until_cancelled(..)`"
                    } else {
                        "hold a `tokio_util::sync::CancellationToken`, call `cancel()` where the flag is set, and poll `token.is_cancelled()` in the loop condition"
                    });
                },
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn flagged<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool { matches!(held.kind(), ty::Adt(atomic, args) if atomic.is_diag_item(cx, sym::Atomic) && args.type_at(0).is_bool()) }
