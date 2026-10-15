use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, in_automatically_derived, paths::PathNS, res::MaybeDef as _, sym};
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_hir::{Expr, ExprKind, Pat, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::Span;

use crate::{Access, Place, access, held, pattern_fields, rebound, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Method {
    Read,
    Guard,
    Replace,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_ARC_SWAP,
    Warn,
    "locks over an `Arc` that every write replaces whole"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Default)]
struct Usage {
    writes: Vec<Span>,
    escaped: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualArcSwap {
    methods: FxHashMap<DefId, Method>,
    locks: FxHashSet<DefId>,
    snapshots: FxIndexMap<Place, Usage>,
}

impl_lint_pass!(ManualArcSwap => [MANUAL_ARC_SWAP]);

impl ManualArcSwap {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let methods = resolve(
            tcx,
            PathNS::Value,
            &[
                ("std::sync::nonpoison::RwLock::read", Method::Read),
                ("std::sync::nonpoison::RwLock::with", Method::Read),
                ("std::sync::nonpoison::RwLock::write", Method::Guard),
                ("std::sync::nonpoison::RwLock::set", Method::Replace),
                ("std::sync::nonpoison::RwLock::replace", Method::Replace),
                ("std::sync::nonpoison::Mutex::lock", Method::Guard),
                ("std::sync::nonpoison::Mutex::set", Method::Replace),
                ("std::sync::nonpoison::Mutex::replace", Method::Replace),
            ],
        );
        let locks = methods.keys().filter_map(|method| tcx.type_of(tcx.parent(*method)).instantiate_identity().skip_normalization().opt_def_id()).collect();
        Self { methods, locks, snapshots: FxIndexMap::default() }
    }

    fn snapshot<'tcx>(&self, cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool {
        matches!(held.kind(), ty::Adt(lock, args) if self.locks.contains(&lock.did()) && args.type_at(0).is_diag_item(cx, sym::Arc))
    }
}

impl<'tcx> LateLintPass<'tcx> for ManualArcSwap {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some((snapshot, _)) = held(cx, expr).filter(|&(place, ty)| !place.exported(cx) && self.snapshot(cx, ty))
            && !in_automatically_derived(cx.tcx, expr.hir_id)
        {
            let call = get_parent_expr(cx, expr).filter(|call| matches!(call.kind, ExprKind::MethodCall(_, receiver, ..) if receiver.hir_id == expr.hir_id));
            let method = call.and_then(|call| self.methods.get(&cx.typeck_results().type_dependent_def_id(call.hir_id)?).copied().map(|method| (method, call)));
            let written = match method {
                Some((Method::Read, _)) => Some(None),
                Some((Method::Replace, replaced)) => Some(Some(replaced.span)),
                Some((Method::Guard, guard)) => match get_parent_expr(cx, guard)
                    .filter(|deref| matches!(deref.kind, ExprKind::Unary(UnOp::Deref, _)))
                    .map_or_else(|| (guard, access(cx, guard)), |deref| (deref, access(cx, deref)))
                {
                    (deref, Access::Assigned(value)) => Some(Some(deref.span.to(value.span))),
                    (_, Access::Copied | Access::Borrowed) => Some(None),
                    (_, Access::Mutated | Access::Moved) => None,
                },
                None => rebound(cx, expr).then_some(None),
            };
            let usage = self.snapshots.entry(snapshot).or_default();
            match written {
                Some(Some(span)) => usage.writes.push(span),
                Some(None) => {}
                None => usage.escaped = true,
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let bound = pattern_fields(cx.typeck_results(), pat)
            .filter_map(|(field, _)| field.as_local())
            .filter(|&field| !cx.effective_visibilities.is_exported(field) && self.snapshot(cx, cx.tcx.type_of(field).instantiate_identity().skip_normalization()))
            .collect::<Vec<_>>();
        for field in bound {
            self.snapshots.entry(Place::Field(field)).or_default().escaped = true;
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let replaced = self.snapshots.iter().filter(|(snapshot, usage)| !usage.escaped && !snapshot.span(cx).from_expansion()).filter_map(|(snapshot, usage)| Some((snapshot, *usage.writes.first()?)));
        for (snapshot, written) in replaced {
            span_lint_hir_and_then(cx, MANUAL_ARC_SWAP, snapshot.hir_id(cx), snapshot.span(cx), format!("`{}` locks an `Arc` that every write replaces whole", snapshot.name(cx)), |diag| {
                diag.span_note(written, "the `Arc` is replaced here");
                diag.help("hold an `arc_swap::ArcSwap`, read it with `load_full()`, and replace it with `store(Arc::new(..))` or `rcu(..)`");
            });
        }
    }
}
