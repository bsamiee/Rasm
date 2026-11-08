use core::ops::ControlFlow;

use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id, get_parent_expr,
    macros::macro_backtrace,
    paths::PathNS,
    peel_hir_expr_while,
    res::{MaybeDef as _, MaybeQPath as _, MaybeResPath as _},
    sym,
    visitors::for_each_expr_without_closures,
};
use itertools::Either;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    Body, BodyId, Expr, ExprKind, HirId, LetStmt, Mutability, Node, Pat, PatKind,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt, UpvarCapture};

use crate::{Access, access, expanded_externally, impl_owner, item_path, paths, signature};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Default)]
struct BodyUses {
    invalidated: FxHashSet<HirId>,
    joined: FxHashMap<HirId, FxHashSet<HirId>>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    joins: FxHashSet<DefId>,
    join_macros: FxHashSet<DefId>,
    maybe_done: FxHashSet<DefId>,
    drains: FxHashSet<DefId>,
    streams: FxHashSet<DefId>,
    wrappers: FxHashSet<DefId>,
    senders: FxHashSet<DefId>,
    subscriptions: FxHashSet<DefId>,
    channels: FxHashSet<DefId>,
    bodies: FxHashMap<BodyId, BodyUses>,
}

lint!(Pass; UNENDING_JOIN: "joined futures draining a `watch` or `broadcast` stream with a sender that outlives the join");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let outputs = |methods: &FxHashSet<DefId>| methods.iter().filter_map(|&method| Some(signature(tcx, method).output().ty_adt_def()?.did())).collect::<FxHashSet<_>>();
        let wrappers = paths(tcx, PathNS::Value, &["tokio_stream::wrappers::WatchStream::new", "tokio_stream::wrappers::WatchStream::from_changes", "tokio_stream::wrappers::BroadcastStream::new"]);
        let subscriptions = paths(tcx, PathNS::Value, &["tokio::sync::watch::Sender::subscribe", "tokio::sync::broadcast::Sender::subscribe"]);
        Self {
            joins: paths(tcx, PathNS::Value, &["futures::future::join", "futures::future::join3", "futures::future::join4", "futures::future::join5"]),
            join_macros: paths(tcx, PathNS::Macro, &["tokio::join", "futures::join"]),
            maybe_done: paths(tcx, PathNS::Value, &["futures::future::maybe_done", "tokio::macros::support::maybe_done"]),
            drains: paths(
                tcx,
                PathNS::Value,
                &[
                    "futures::stream::StreamExt::collect",
                    "futures::stream::StreamExt::concat",
                    "futures::stream::StreamExt::count",
                    "futures::stream::StreamExt::fold",
                    "futures::stream::StreamExt::for_each",
                    "futures::stream::StreamExt::for_each_concurrent",
                    "futures::stream::StreamExt::unzip",
                    "tokio_stream::StreamExt::fold",
                ],
            ),
            streams: outputs(&wrappers),
            wrappers,
            senders: subscriptions.iter().filter_map(|&method| impl_owner(tcx, method).opt_def_id()).collect(),
            subscriptions,
            channels: paths(tcx, PathNS::Value, &["tokio::sync::watch::channel", "tokio::sync::broadcast::channel"]),
            bodies: FxHashMap::default(),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_body(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) {
        if for_each_expr_without_closures(body.value, |part| {
            if matches!(part.kind, ExprKind::Call(..)) && fn_def_id(cx, part).is_some_and(|called| self.joins.contains(&called) || self.maybe_done.contains(&called)) {
                ControlFlow::Break(())
            } else {
                ControlFlow::Continue(())
            }
        })
        .is_some()
        {
            self.bodies.insert(body.id(), BodyUses::default());
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn moved_capture<'tcx>(cx: &LateContext<'tcx>, capture: &ty::CapturedPlace<'tcx>) -> bool {
            let captured = capture.place.ty();
            match capture.info.capture_kind {
                UpvarCapture::ByValue => !cx.type_is_copy_modulo_regions(captured),
                UpvarCapture::ByUse => !cx.type_is_copy_modulo_regions(captured) && !cx.type_is_use_cloned_modulo_regions(captured),
                UpvarCapture::ByRef(ty::BorrowKind::Immutable) => false,
                UpvarCapture::ByRef(ty::BorrowKind::UniqueImmutable | ty::BorrowKind::Mutable) => true,
            }
        }

        let Some(body) = cx.enclosing_body else { return };
        let Some(uses) = self.bodies.get_mut(&body) else { return };
        let body_owner = cx.tcx.hir_body_owner_def_id(body);
        let native_sender = |binding: HirId| {
            let declared = cx.typeck_results().node_type(binding);
            let held = match declared.kind() {
                ty::Adt(wrapper, args) if wrapper.is_box() || matches!(wrapper.opt_diag_name(cx), Some(sym::Arc | sym::Rc)) => args.type_at(0),
                _ => declared,
            };
            held.opt_def_id().is_some_and(|sender| self.senders.contains(&sender))
        };
        let invalidated = |expression| match access(cx, expression) {
            Access::Moved | Access::Assigned(_) | Access::Mutated => true,
            Access::Copied | Access::Borrowed => false,
        };
        if let Some(binding) = expr.res_local_id()
            && native_sender(binding)
            && invalidated(expr)
        {
            uses.invalidated.insert(binding);
        }
        if let ExprKind::Closure(closure) = expr.kind {
            uses.invalidated.extend(
                cx.tcx.closure_captures(closure.def_id).iter().filter(|capture| native_sender(capture.get_root_variable()) && moved_capture(cx, capture)).map(|capture| capture.get_root_variable()),
            );
        }
        let inherited = |binding| cx.tcx.is_closure_like(body_owner.to_def_id()) && cx.tcx.closure_captures(body_owner).iter().any(|capture| capture.get_root_variable() == binding);
        let live_owner = |binding| match cx.typeck_results().node_type(binding).kind() {
            ty::Adt(..) if native_sender(binding) => cx.tcx.hir_enclosing_body_owner(binding) == body_owner || inherited(binding),
            ty::Ref(_, _, Mutability::Not) => inherited(binding),
            _ => false,
        };
        let channeled =
            |tuple: HirId| matches!(cx.tcx.parent_hir_node(tuple), Node::LetStmt(LetStmt { init: Some(init), .. }) if fn_def_id(cx, init).is_some_and(|built| self.channels.contains(&built)));
        let sender = |subscribed: &'tcx Expr<'tcx>| {
            let subscribed = expr_or_init(cx, subscribed);
            match (subscribed.kind, subscribed.res_local_id().map(|received| cx.tcx.parent_hir_node(received))) {
                (ExprKind::MethodCall(_, sender, ..), _) if fn_def_id(cx, subscribed).is_some_and(|method| self.subscriptions.contains(&method)) => {
                    let base = peel_hir_expr_while(sender, |step| match step.kind {
                        ExprKind::Field(base, _) => Some(base),
                        _ => None,
                    });
                    let base = if cx.typeck_results().expr_ty(base).is_ref() { expr_or_init(cx, base).peel_borrows() } else { base };
                    match base.res(cx.typeck_results()) {
                        Res::Local(binding) if live_owner(binding) => Some((Some(binding), sender.span)),
                        Res::Def(DefKind::Static { mutability: Mutability::Not, .. }, _) => Some((None, sender.span)),
                        _ => None,
                    }
                }
                (_, Some(Node::Pat(&Pat { kind: PatKind::Tuple([Pat { kind: PatKind::Binding(_, binding, ..), span, .. }, ..], _), hir_id, .. }))) if channeled(hir_id) && live_owner(*binding) =>
                    Some((Some(*binding), *span)),
                _ => None,
            }
        };
        if let ExprKind::Call(_, operands) = expr.kind
            && let Some((join, expansion)) = fn_def_id(cx, expr).and_then(|called| {
                if self.joins.contains(&called) {
                    (!expanded_externally(cx, expr.span)).then_some((called, None))
                } else if self.maybe_done.contains(&called) {
                    macro_backtrace(expr.span)
                        .filter(|expansion| self.join_macros.contains(&expansion.def_id))
                        .last()
                        .filter(|expansion| !expanded_externally(cx, expansion.span))
                        .map(|expansion| (expansion.def_id, Some(expansion.expn)))
                } else {
                    None
                }
            })
            && (expansion.is_some()
                || get_parent_expr(cx, expr).and_then(|parent| get_parent_expr(cx, parent)).is_some_and(|awaited| desugar_await(awaited).is_some_and(|future| future.hir_id == expr.hir_id)))
        {
            let evaluated = expansion
                .and_then(|expansion| {
                    cx.tcx
                        .hir_parent_iter(expr.hir_id)
                        .filter_map(|(_, node)| if let Node::Expr(parent) = node { Some(parent) } else { None })
                        .take_while(|parent| parent.span.ctxt().outer_expn().is_descendant_of(expansion))
                        .last()
                })
                .unwrap_or(expr);
            let moved_during = uses.joined.entry(evaluated.hir_id).or_insert_with(|| {
                let mut moved = FxHashSet::default();
                let _: Option<()> = for_each_expr_without_closures(evaluated, |part| {
                    moved.extend(match part.kind {
                        ExprKind::Closure(closure) => Either::Left(
                            cx.tcx
                                .closure_captures(closure.def_id)
                                .iter()
                                .filter(|capture| native_sender(capture.get_root_variable()) && moved_capture(cx, capture))
                                .map(|capture| capture.get_root_variable()),
                        ),
                        _ => Either::Right(part.res_local_id().filter(|&binding| native_sender(binding) && invalidated(part)).into_iter()),
                    });
                    ControlFlow::Continue(())
                });
                moved
            });
            let outlives = |binding| !uses.invalidated.contains(&binding) && !moved_during.contains(&binding);
            let join = if expansion.is_some() { format!("{}!", cx.tcx.item_name(join)) } else { item_path(cx, join) };
            let drains = operands.iter().filter_map(|operand| {
                let drain = expr_or_init(cx, operand);
                let ExprKind::MethodCall(method, receiver, ..) = drain.kind else { return None };
                self.drains.contains(&fn_def_id(cx, drain)?).then_some(())?;
                let stream = cx.typeck_results().expr_ty_adjusted(receiver).peel_refs().opt_def_id().filter(|stream| self.streams.contains(stream))?;
                let built = expr_or_init(cx, receiver);
                let ExprKind::Call(_, [subscribed]) = built.kind else { return None };
                self.wrappers.contains(&fn_def_id(cx, built)?).then_some(())?;
                Some((drain, method.ident, stream, sender(subscribed)?))
            });
            for (drain, method, stream, (_, held)) in drains.filter(|&(.., (holder, _))| holder.is_none_or(outlives)) {
                span_lint_hir_and_then(
                    cx,
                    UNENDING_JOIN,
                    drain.hir_id,
                    drain.span,
                    format!("`{method}` over `{}` can keep `{join}` pending while a sender remains alive", cx.tcx.item_name(stream)),
                    |diag| {
                        diag.span_note(held, "sender held here");
                        diag.help(format!("pin the futures, combine any remaining futures, and race `{method}` against them through `futures::future::select` in place of `{join}`"));
                    },
                );
            }
        }
    }

    fn check_body_post(&mut self, _: &LateContext<'tcx>, body: &Body<'tcx>) { self.bodies.remove(&body.id()); }
}
