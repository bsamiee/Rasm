use core::ops::ControlFlow;

use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, peel_hir_expr_while, res::MaybeResPath as _, visitors::for_each_expr};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{
    Expr, ExprKind, HirId, LetStmt, Node, Pat, PatKind,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};

use crate::{Access, access, any_expr, expanded_externally, item_path, paths, signature};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    joins: FxHashSet<DefId>,
    join_macros: FxHashSet<DefId>,
    maybe_done: FxHashSet<DefId>,
    drains: FxHashSet<DefId>,
    streams: FxHashSet<DefId>,
    wrappers: FxHashSet<DefId>,
    bounded: FxHashSet<DefId>,
    subscriptions: FxHashSet<DefId>,
    channels: FxHashSet<DefId>,
}

lint!(Pass; UNENDING_JOIN: "joined futures draining a `watch` or `broadcast` stream with a sender that outlives the join");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let outputs = |methods: &FxHashSet<DefId>| methods.iter().filter_map(|&method| Some(signature(tcx, method).output().ty_adt_def()?.did())).collect::<FxHashSet<_>>();
        let wrappers = paths(tcx, PathNS::Value, &["tokio_stream::wrappers::WatchStream::new", "tokio_stream::wrappers::WatchStream::from_changes", "tokio_stream::wrappers::BroadcastStream::new"]);
        Self {
            joins: paths(
                tcx,
                PathNS::Value,
                &[
                    "futures::future::join",
                    "futures::future::join3",
                    "futures::future::join4",
                    "futures::future::join5",
                    "futures::future::try_join",
                    "futures::future::try_join3",
                    "futures::future::try_join4",
                    "futures::future::try_join5",
                ],
            ),
            join_macros: paths(tcx, PathNS::Macro, &["tokio::join", "tokio::try_join", "futures::join", "futures::try_join"]),
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
                    "futures::stream::StreamExt::forward",
                    "futures::stream::StreamExt::unzip",
                    "futures::stream::TryStreamExt::try_collect",
                    "futures::stream::TryStreamExt::try_concat",
                    "futures::stream::TryStreamExt::try_fold",
                    "futures::stream::TryStreamExt::try_for_each",
                    "futures::stream::TryStreamExt::try_for_each_concurrent",
                    "tokio_stream::StreamExt::collect",
                    "tokio_stream::StreamExt::fold",
                ],
            ),
            streams: outputs(&wrappers),
            wrappers,
            bounded: outputs(&paths(
                tcx,
                PathNS::Value,
                &[
                    "futures::stream::StreamExt::scan",
                    "futures::stream::StreamExt::take",
                    "futures::stream::StreamExt::take_until",
                    "futures::stream::StreamExt::take_while",
                    "futures::stream::StreamExt::zip",
                    "tokio_stream::StreamExt::map_while",
                    "tokio_stream::StreamExt::take",
                    "tokio_stream::StreamExt::take_while",
                ],
            )),
            subscriptions: paths(tcx, PathNS::Value, &["tokio::sync::watch::Sender::subscribe", "tokio::sync::broadcast::Sender::subscribe"]),
            channels: paths(tcx, PathNS::Value, &["tokio::sync::watch::channel", "tokio::sync::broadcast::channel"]),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn outlives(cx: &LateContext<'_>, binding: HirId) -> bool {
            let tcx = cx.tcx;
            let scope = tcx.hir_enclosing_body_owner(binding);
            cx.typeck_results().node_type(binding).is_ref()
                || !any_expr(tcx, tcx.hir_body_owned_by(scope), |mention| {
                    mention.res_local_id() == Some(binding)
                        && (tcx.hir_enclosing_body_owner(mention.hir_id) != scope
                            || match access(cx, mention) {
                                Access::Borrowed | Access::Mutated | Access::Copied => false,
                                Access::Moved | Access::Assigned(_) => true,
                            })
                })
        }
        let streamed = |drained: Ty<'tcx>| {
            let (mut walker, mut streamed) = (drained.walk(), None);
            while let Some(argument) = walker.next() {
                match argument.as_type().map(Ty::kind) {
                    Some(ty::Closure(..) | ty::CoroutineClosure(..) | ty::Coroutine(..)) => walker.skip_current_subtree(),
                    Some(ty::Adt(adapter, _)) if self.bounded.contains(&adapter.did()) => return None,
                    Some(ty::Adt(stream, _)) if self.streams.contains(&stream.did()) => streamed = Some(stream.did()),
                    _ => {}
                }
            }
            streamed
        };
        let channeled =
            |tuple: HirId| matches!(cx.tcx.parent_hir_node(tuple), Node::LetStmt(LetStmt { init: Some(init), .. }) if fn_def_id(cx, init).is_some_and(|built| self.channels.contains(&built)));
        let sender = |subscribed: &'tcx Expr<'tcx>| {
            reach(cx, subscribed, |part| match (part.kind, part.res_local_id().map(|received| cx.tcx.parent_hir_node(received))) {
                (ExprKind::MethodCall(_, sender, ..), _) if fn_def_id(cx, part).is_some_and(|method| self.subscriptions.contains(&method)) => {
                    let base = peel_hir_expr_while(sender, |step| match step.kind {
                        ExprKind::Field(base, _) => Some(base),
                        _ => None,
                    });
                    match base.kind {
                        ExprKind::Path(qpath) => match cx.qpath_res(&qpath, base.hir_id) {
                            Res::Local(binding) => Some((Some(binding), sender.span)),
                            Res::Def(DefKind::Static { .. }, _) => Some((None, sender.span)),
                            _ => None,
                        },
                        _ => None,
                    }
                }
                (_, Some(Node::Pat(&Pat { kind: PatKind::Tuple([Pat { kind: PatKind::Binding(_, binding, ..), span, .. }, ..], _), hir_id, .. }))) if channeled(hir_id) =>
                    Some((Some(*binding), *span)),
                _ => None,
            })
        };
        if let ExprKind::Call(_, operands) = expr.kind
            && let Some(join) = fn_def_id(cx, expr).and_then(|called| {
                if self.joins.contains(&called) {
                    (!expanded_externally(cx, expr.span)).then(|| item_path(cx, called))
                } else if self.maybe_done.contains(&called) {
                    expr.span
                        .macro_backtrace()
                        .filter_map(|expansion| Some((expansion.macro_def_id.filter(|written| self.join_macros.contains(written))?, expansion.call_site)))
                        .last()
                        .filter(|&(_, call_site)| !expanded_externally(cx, call_site))
                        .map(|(written, _)| format!("{}!", cx.tcx.item_name(written)))
                } else {
                    None
                }
            })
        {
            let drains = operands.iter().filter_map(|operand| {
                reach(cx, operand, |part| match part.kind {
                    ExprKind::MethodCall(method, receiver, ..) if fn_def_id(cx, part).is_some_and(|drained| self.drains.contains(&drained)) => streamed(cx.typeck_results().expr_ty_adjusted(receiver))
                        .zip(reach(cx, receiver, |built| match built.kind {
                            ExprKind::Call(_, [subscribed]) if fn_def_id(cx, built).is_some_and(|wrapper| self.wrappers.contains(&wrapper)) => sender(subscribed),
                            _ => None,
                        }))
                        .map(|(stream, held)| (part, method.ident, stream, held)),
                    _ => None,
                })
            });
            for (drain, method, stream, (_, held)) in drains.filter(|&(.., (holder, _))| holder.is_none_or(|binding| outlives(cx, binding))) {
                span_lint_hir_and_then(
                    cx,
                    UNENDING_JOIN,
                    drain.hir_id,
                    drain.span,
                    format!("`{method}` over `{}` never completes while a sender outlives `{join}`", cx.tcx.item_name(stream)),
                    |diag| {
                        diag.span_note(held, "sender held here");
                        diag.help(format!("race `{method}` against the other futures through `futures::future::select` in place of `{join}`"));
                    },
                );
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn reach<'tcx, B, F>(cx: &LateContext<'tcx>, root: &'tcx Expr<'tcx>, mut found: F) -> Option<B>
where F: FnMut(&'tcx Expr<'tcx>) -> Option<B> {
    let (mut pending, mut seen) = (vec![root], FxHashSet::default());
    while let Some(next) = pending.pop() {
        let hit = for_each_expr(cx.tcx, next, |part| {
            if let Some(binding) = part.res_local_id()
                && seen.insert(binding)
                && let Node::LetStmt(LetStmt { init: Some(init), .. }) = cx.tcx.parent_hir_node(binding)
            {
                pending.push(init);
            }
            found(part).map_or(ControlFlow::Continue(()), ControlFlow::Break)
        });
        if hit.is_some() {
            return hit;
        }
    }
    None
}
