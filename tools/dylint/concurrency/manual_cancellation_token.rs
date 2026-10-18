use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    higher::While,
    paths::PathNS,
    peel_blocks_with_stmt,
    res::MaybeDef as _,
    sym,
    visitors::{Descend, for_each_expr_without_closures},
};
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Block, Expr, ExprKind, HirId, LoopSource, Pat, StmtKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::{Span, Spanned};

use super::{AsyncContext, PlaceUses, cloned_into_binding, stored_place};
use crate::{generated, method_call_on, paths, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Method {
    Read,
    Set,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Default)]
struct Usage {
    written: Option<Span>,
    polled: Option<(HirId, &'static str)>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, Method>,
    waits: FxHashSet<DefId>,
    async_context: AsyncContext,
    flags: PlaceUses<Usage>,
}

lint!(Pass; MANUAL_CANCELLATION_TOKEN: "`AtomicBool` flags that every write sets to `true` and a loop polls");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            methods: resolve(
                tcx,
                PathNS::Value,
                &[("core::sync::atomic::Atomic::load", Method::Read), ("core::sync::atomic::Atomic::store", Method::Set), ("core::sync::atomic::Atomic::swap", Method::Set)],
            ),
            waits: paths(
                tcx,
                PathNS::Value,
                &[
                    "tokio::time::sleep",
                    "tokio::time::sleep_until",
                    "tokio::time::Interval::tick",
                    "tokio::task::yield_now",
                    "std::thread::sleep",
                    "std::thread::sleep_until",
                    "std::thread::yield_now",
                    "std::thread::park_timeout",
                ],
            ),
            async_context: AsyncContext::new(tcx),
            flags: PlaceUses::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let flag = |used: &'tcx Expr<'tcx>| stored_place(cx, used).filter(|&(_, ty)| is_atomic_bool(cx, ty)).map(|(place, _)| place);
        let method = |used: &'tcx Expr<'tcx>| method_call_on(cx, used).and_then(|(call, method)| Some((*self.methods.get(&method)?, call)));
        let waits = |body: &'tcx Block<'tcx>| {
            for_each_expr_without_closures(body, |inner| match inner.kind {
                ExprKind::Loop(..) => ControlFlow::Continue(Descend::No),
                _ if fn_def_id(cx, inner).is_some_and(|called| self.waits.contains(&called)) => ControlFlow::Break(()),
                _ => ControlFlow::Continue(Descend::Yes),
            })
            .is_some()
        };
        let polled = match (While::hir(expr), expr.kind) {
            (Some(While { condition, body: &Expr { kind: ExprKind::Block(body, _), .. }, .. }), _) => Some((condition, body)),
            (None, ExprKind::Loop(body, _, LoopSource::Loop, _))
                if let Some(&Expr { kind: ExprKind::If(condition, stop, None), .. }) =
                    body.stmts.first().map_or(body.expr, |statement| if let StmtKind::Expr(first) | StmtKind::Semi(first) = statement.kind { Some(first) } else { None })
                    && matches!(peel_blocks_with_stmt(stop).kind, ExprKind::Break(..)) =>
                Some((condition, body)),
            _ => None,
        };
        if let Some((condition, body)) = polled
            && !waits(body)
        {
            let help = if self.async_context.is_async(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id)) {
                "await `cancelled()` or run the loop through `run_until_cancelled(..)`"
            } else {
                "poll `is_cancelled()` in the loop condition"
            };
            let _: Option<()> = for_each_expr_without_closures(condition, |inner| {
                if let ExprKind::MethodCall(_, receiver, ..) = inner.kind
                    && let Some(flag) = flag(receiver)
                    && let Some((Method::Read, _)) = method(receiver)
                    && let Some(usage) = self.flags.record(flag, Usage::default)
                {
                    usage.polled.get_or_insert((inner.hir_id, help));
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
                        && matches!(set.kind, ExprKind::Lit(Spanned { node: LitKind::Bool(true), .. })) =>
                    if let Some(usage) = self.flags.record(flag, Usage::default) {
                        usage.written.get_or_insert(call.span);
                    },
                Some((Method::Set, _)) => self.flags.escape(flag),
                None if !cloned_into_binding(cx, expr) => self.flags.escape(flag),
                Some((Method::Read, _)) | None => {
                    self.flags.record(flag, Usage::default);
                }
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.flags.escape_bound(cx, pat, |held| is_atomic_bool(cx, held)); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (flag, written, (polled, help)) in self.flags.unescaped(cx).filter_map(|(flag, usage)| Some((flag, usage.written?, usage.polled?))) {
            span_lint_hir_and_then(
                cx,
                MANUAL_CANCELLATION_TOKEN,
                flag.hir_id(cx),
                flag.span(cx),
                format!("every write to `{}` stores `true`, and a loop polls it as a stop flag", flag.name(cx)),
                |diag| {
                    diag.span_note(written, "set here");
                    diag.span_note(cx.tcx.hir_span(polled), "polled here");
                    diag.help(format!("declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and {help}"));
                },
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn is_atomic_bool<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool { matches!(held.kind(), ty::Adt(atomic, args) if atomic.is_diag_item(cx, sym::Atomic) && args.type_at(0).is_bool()) }
