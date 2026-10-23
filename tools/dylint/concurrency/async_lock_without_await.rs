use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_enclosing_block, get_parent_expr, get_parent_expr_for_hir,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _, MaybeTypeckRes as _},
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    CoroutineKind, Expr, ExprKind, HirId, Node, Pat, PatKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};
use rustc_span::{Span, sym};

use super::PlaceUses;
use crate::{Access, Place, access, any_expr, cloned, field_at, generated, impl_owner, method_call_on, pattern_fields, place, pointee, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Release {
    Awaited,
    Blocking,
    Exclusive,
}

// --- [MODELS] --------------------------------------------------------------------------

struct Site {
    call: Span,
    scope: Span,
    guard: DefId,
    body: LocalDefId,
}

struct Locked {
    lock: DefId,
    sites: Vec<Site>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, Release>,
    locks: FxHashSet<DefId>,
    owners: PlaceUses<Locked>,
    bindings: FxHashMap<HirId, LocalDefId>,
}

lint!(Pass; ASYNC_LOCK_WITHOUT_AWAIT: "`tokio::sync` locks with no guard held across an `.await`");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let methods = resolve(
            tcx,
            PathNS::Value,
            &[
                ("tokio::sync::Mutex::lock", Release::Awaited),
                ("tokio::sync::RwLock::read", Release::Awaited),
                ("tokio::sync::RwLock::write", Release::Awaited),
                ("tokio::sync::Mutex::blocking_lock", Release::Blocking),
                ("tokio::sync::RwLock::blocking_read", Release::Blocking),
                ("tokio::sync::RwLock::blocking_write", Release::Blocking),
                ("tokio::sync::Mutex::get_mut", Release::Exclusive),
                ("tokio::sync::Mutex::into_inner", Release::Exclusive),
                ("tokio::sync::RwLock::get_mut", Release::Exclusive),
                ("tokio::sync::RwLock::into_inner", Release::Exclusive),
            ],
        );
        let locks = methods.keys().filter_map(|&method| impl_owner(tcx, method).opt_def_id()).collect();
        Self { methods, locks, owners: PlaceUses::default(), bindings: FxHashMap::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn recloned<'tcx>(cx: &LateContext<'tcx>, call: &'tcx Expr<'tcx>, owner: Place) -> bool {
            let typeck = cx.typeck_results();
            cloned(cx, call).is_some()
                && matches!(cx.tcx.parent_hir_node(call.hir_id), Node::ExprField(field)
                    if get_parent_expr_for_hir(cx, field.hir_id)
                        .and_then(|literal| field_at(typeck, typeck.expr_ty(literal), field.hir_id)?.as_local())
                        .is_some_and(|stored| Place::Field(stored) == owner))
        }

        fn guard_scope<'tcx>(cx: &LateContext<'tcx>, guard: &'tcx Expr<'tcx>) -> Option<Span> {
            fn borrowed<'tcx>(cx: &LateContext<'tcx>, used: &'tcx Expr<'tcx>) -> bool {
                match access(cx, used) {
                    Access::Moved => get_parent_expr(cx, used).is_some_and(|parent| fn_def_id(cx, parent).is_diag_item(cx, sym::mem_drop)),
                    Access::Copied | Access::Borrowed | Access::Mutated | Access::Assigned(_) => true,
                }
            }

            if let Node::LetStmt(local) = cx.tcx.parent_hir_node(guard.hir_id)
                && let PatKind::Binding(_, binding, _, None) = local.pat.kind
            {
                let body = cx.tcx.hir_enclosing_body_owner(local.hir_id);
                let escaped = any_expr(cx.tcx, get_enclosing_block(cx, local.hir_id)?, |used| {
                    used.res_local_id() == Some(binding) && (cx.tcx.hir_enclosing_body_owner(used.hir_id) != body || !borrowed(cx, used))
                });
                (!escaped).then_some(local.span)
            } else {
                borrowed(cx, guard).then(|| {
                    cx.tcx
                        .hir_parent_iter(guard.hir_id)
                        .take_while(|(_, node)| node.associated_body().is_none())
                        .find(|(_, node)| matches!(node, Node::Stmt(_)))
                        .map_or(guard.span, |(statement, _)| cx.tcx.hir_span(statement))
                })
            }
        }

        fn awaited<'tcx>(cx: &LateContext<'tcx>, call: &Expr<'_>) -> Option<&'tcx Expr<'tcx>> {
            get_parent_expr(cx, call).and_then(|into_future| get_parent_expr(cx, into_future)).filter(|awaiting| desugar_await(awaiting).is_some_and(|operand| operand.hir_id == call.hir_id))
        }

        let owner = |used: &Expr<'_>| {
            let ty = cx.typeck_results().expr_ty(used);
            let lock = lock_of(cx, &self.locks, ty)?;
            match place(cx, used)? {
                Place::Local(binding) | Place::Parameter(binding) if let Some(&field) = self.bindings.get(&binding) => Some((Place::Field(field), lock)),
                held @ Place::Local(_) => (ty.opt_def_id() == Some(lock)).then_some((held, lock)),
                held @ (Place::Field(_) | Place::Static(_)) => Some((held, lock)),
                Place::Parameter(_) => None,
            }
        };
        if self.locks.is_empty() {
            return;
        }
        if matches!(expr.kind, ExprKind::Field(..) | ExprKind::Path(_))
            && let Some((owner, _)) = owner(expr)
            && method_call_on(cx, expr).is_none()
            && !get_parent_expr(cx, expr).is_some_and(|parent| matches!(parent.kind, ExprKind::AddrOf(..)) && get_parent_expr(cx, parent).is_some_and(|call| recloned(cx, call, owner)))
            && !generated(cx, expr.hir_id)
        {
            self.owners.escape(owner);
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && let Some((owner, lock)) = owner(receiver)
            && let Some(method) = cx.ty_based_def(expr).opt_def_id()
        {
            let guard = match self.methods.get(&method) {
                Some(Release::Exclusive) => return,
                None if recloned(cx, expr, owner) => return,
                Some(Release::Awaited) => awaited(cx, expr),
                Some(Release::Blocking) => Some(expr),
                None => None,
            };
            let site = guard.and_then(|guard| {
                Some(Site { call: expr.span, scope: guard_scope(cx, guard)?, guard: cx.typeck_results().expr_ty(guard).opt_def_id()?, body: cx.tcx.hir_enclosing_body_owner(expr.hir_id) })
            });
            match site {
                Some(site) =>
                    if let Some(locked) = self.owners.record(owner, || Locked { lock, sites: Vec::new() }) {
                        locked.sites.push(site);
                    },
                None => self.owners.escape(owner),
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if self.locks.is_empty() {
            return;
        }
        let typeck = cx.typeck_results();
        self.bindings.extend(pattern_fields(typeck, pat).filter_map(|(field, bound)| {
            if let PatKind::Binding(_, binding, _, None) = bound.kind
                && lock_of(cx, &self.locks, typeck.pat_ty(bound)).is_some()
            {
                Some((binding, field.as_local()?))
            } else {
                None
            }
        }));
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let held = |site: &Site| {
            matches!(cx.tcx.coroutine_kind(site.body), Some(CoroutineKind::Desugared(..)))
                && cx
                    .tcx
                    .mir_coroutine_witnesses(site.body)
                    .is_some_and(|layout| layout.field_tys.iter().any(|saved| saved.ty.opt_def_id() == Some(site.guard) && site.scope.contains(saved.source_info.span)))
        };
        for (owner, Locked { lock, sites }) in self.owners.unescaped(cx).filter(|(_, locked)| !locked.sites.iter().any(held)) {
            let (name, path, owner_name) = (cx.tcx.item_name(*lock), cx.tcx.def_path_str(*lock), owner.name(cx));
            span_lint_hir_and_then(cx, ASYNC_LOCK_WITHOUT_AWAIT, owner.hir_id(cx), owner.span(cx), format!("no `{path}` guard of `{owner_name}` is held across an `.await`"), |diag| {
                diag.span_note(sites.iter().map(|site| site.call).collect::<Vec<_>>(), format!("`{owner_name}` is locked here"));
                diag.help(format!("declare `std::sync::nonpoison::{name}` in place of `{path}` and lock it with no `.await`"));
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn lock_of<'tcx>(cx: &LateContext<'tcx>, locks: &FxHashSet<DefId>, ty: Ty<'tcx>) -> Option<DefId> { pointee(cx, ty).opt_def_id().filter(|adt| locks.contains(adt)) }
