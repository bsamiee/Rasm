use core::ops::ControlFlow;

use clippy_utils::{diagnostics::span_lint_hir_and_then, higher::While, paths::PathNS, res::MaybeDef as _, sym, visitors::for_each_expr_without_closures};
use rustc_ast::LitKind;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, HirId, Pat, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::{Span, Spanned};

use super::{AsyncContext, PlaceUses, cloned_into_binding, stored_place};
use crate::{generated, method_call_on, resolve};

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
    async_context: AsyncContext,
    flags: PlaceUses<Usage>,
}

lint!(Pass; MANUAL_CANCELLATION_TOKEN: "`AtomicBool` flags that every write sets to `true` and a `while` loop polls");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            methods: resolve(
                tcx,
                PathNS::Value,
                &[("core::sync::atomic::Atomic::load", Method::Read), ("core::sync::atomic::Atomic::store", Method::Set), ("core::sync::atomic::Atomic::swap", Method::Set)],
            ),
            async_context: AsyncContext::new(tcx),
            flags: PlaceUses::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        const AWAITED: &str = "declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and await `cancelled()` or run the loop through `run_until_cancelled(..)`";
        const POLLED: &str = "declare `tokio_util::sync::CancellationToken`, call `cancel()` in place of each write, and poll `is_cancelled()` in the loop condition";

        let flag = |used: &'tcx Expr<'tcx>| stored_place(cx, used).filter(|&(_, ty)| is_atomic_bool(cx, ty)).map(|(place, _)| place);
        let method = |used: &'tcx Expr<'tcx>| method_call_on(cx, used).and_then(|(call, method)| Some((*self.methods.get(&method)?, call)));
        if let Some(polled) = While::hir(expr) {
            let help = if self.async_context.is_async(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id)) { AWAITED } else { POLLED };
            let _: Option<()> = for_each_expr_without_closures(polled.condition, |inner| {
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
                format!("every write to `{}` stores `true`, and a `while` loop polls it as a stop flag", flag.name(cx)),
                |diag| {
                    diag.span_note(written, "set here");
                    diag.span_note(cx.tcx.hir_span(polled), "polled here");
                    diag.help(help);
                },
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn is_atomic_bool<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool { matches!(held.kind(), ty::Adt(atomic, args) if atomic.is_diag_item(cx, sym::Atomic) && args.type_at(0).is_bool()) }
