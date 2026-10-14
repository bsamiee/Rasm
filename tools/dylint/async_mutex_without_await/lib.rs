#![feature(rustc_private)]

extern crate rustc_attr_ir;
extern crate rustc_data_structures;
extern crate rustc_hir;
extern crate rustc_middle;
extern crate rustc_span;

use std::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_and_then,
    fn_def_id, get_enclosing_block, get_parent_expr, get_parent_expr_for_hir,
    paths::{PathNS, lookup_path_str},
    res::{MaybeDef as _, MaybeResPath as _},
    visitors::for_each_expr,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{ClosureKind, CoroutineKind, Expr, ExprKind, HirId, MatchSource, Node, Pat, PatKind, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty};
use rustc_span::{Span, sym};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Release {
    Awaited,
    Blocking,
    Exclusive,
}

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum Owner {
    Field(DefId),
    Local(HirId),
    Static(DefId),
}

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::impl_late_lint! {
    pub ASYNC_MUTEX_WITHOUT_AWAIT,
    Warn,
    "`tokio::sync` locks with no guard held across an `.await`",
    AsyncMutexWithoutAwait::default()
}

struct Site {
    call: Span,
    scope: Span,
    guard: DefId,
}

struct Usage {
    lock: DefId,
    sites: Vec<Site>,
    escaped: bool,
}

#[derive(Default)]
struct AsyncMutexWithoutAwait {
    locks: Vec<DefId>,
    methods: Vec<(DefId, Release)>,
    owners: FxIndexMap<Owner, Usage>,
    bindings: FxHashMap<HirId, DefId>,
    held: Vec<(DefId, Span)>,
}

impl AsyncMutexWithoutAwait {
    fn owner(&self, cx: &LateContext<'_>, expr: &Expr<'_>) -> Option<(Owner, DefId)> {
        let typeck = cx.typeck_results();
        let ty = typeck.expr_ty(expr);
        let lock = lock_of(cx, &self.locks, ty)?;
        if let ExprKind::Field(base, _) = expr.kind {
            let field = typeck.expr_ty_adjusted(base).peel_refs().ty_adt_def()?.non_enum_variant().fields.get(typeck.opt_field_index(expr.hir_id)?)?.did;
            Some((Owner::Field(field), lock))
        } else if let Some(local) = expr.res_local_id() {
            match self.bindings.get(&local) {
                Some(field) => Some((Owner::Field(*field), lock)),
                None => (ty.opt_def_id() == Some(lock)).then_some((Owner::Local(local), lock)),
            }
        } else {
            expr.basic_res().opt_def_id().filter(|item| cx.tcx.is_static(*item)).map(|item| (Owner::Static(item), lock))
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn lock_of<'tcx>(cx: &LateContext<'tcx>, locks: &[DefId], ty: Ty<'tcx>) -> Option<DefId> {
    let shared = ty.peel_refs();
    let inner = if let ty::Adt(adt, args) = shared.kind()
        && (adt.is_box() || adt.is_diag_item(cx, sym::Arc))
    {
        args.types().next()?
    } else {
        shared
    };
    inner.opt_def_id().filter(|adt| locks.contains(adt))
}

fn recloned(cx: &LateContext<'_>, call: &Expr<'_>, owner: Owner) -> bool {
    let typeck = cx.typeck_results();
    fn_def_id(cx, call).is_some_and(|clone| clone.is_lang_item(cx, LangItem::CloneFn))
        && matches!(cx.tcx.parent_hir_node(call.hir_id), Node::ExprField(field)
            if get_parent_expr_for_hir(cx, field.hir_id)
                .and_then(|literal| typeck.expr_ty(literal).ty_adt_def()?.non_enum_variant().fields.get(typeck.opt_field_index(field.hir_id)?))
                .is_some_and(|stored| Owner::Field(stored.did) == owner))
}

fn awaited<'tcx>(cx: &LateContext<'tcx>, call: &Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
    cx.tcx.hir_parent_iter(call.hir_id).take(2).find_map(|(_, node)| {
        if let Node::Expr(expr) = node
            && let ExprKind::Match(_, _, MatchSource::AwaitDesugar) = expr.kind
        {
            Some(expr)
        } else {
            None
        }
    })
}

fn borrowed(cx: &LateContext<'_>, used: &Expr<'_>) -> bool {
    get_parent_expr(cx, used).is_some_and(|parent| {
        matches!(parent.kind, ExprKind::Unary(UnOp::Deref, _) | ExprKind::Field(..) | ExprKind::AddrOf(..) | ExprKind::Index(..))
            || matches!(parent.kind, ExprKind::MethodCall(_, target, ..) | ExprKind::Assign(target, ..) | ExprKind::AssignOp(_, target, _) if target.hir_id == used.hir_id)
            || fn_def_id(cx, parent).is_some_and(|callee| callee.is_diag_item(cx, sym::mem_drop))
    })
}

fn guard_scope<'tcx>(cx: &LateContext<'tcx>, guard: &'tcx Expr<'tcx>) -> Option<Span> {
    if let Node::LetStmt(local) = cx.tcx.parent_hir_node(guard.hir_id)
        && let PatKind::Binding(_, binding, _, None) = local.pat.kind
        && let Some(block) = get_enclosing_block(cx, local.hir_id)
    {
        for_each_expr(cx.tcx, block, |used: &'tcx Expr<'tcx>| if used.res_local_id() == Some(binding) && !borrowed(cx, used) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) })
            .is_none()
            .then_some(local.span)
    } else {
        borrowed(cx, guard).then(|| {
            cx.tcx
                .hir_parent_iter(guard.hir_id)
                .take_while(|(_, node)| node.associated_body().is_none())
                .find_map(|(_, node)| if let Node::Stmt(statement) = node { Some(statement.span) } else { None })
                .unwrap_or(guard.span)
        })
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for AsyncMutexWithoutAwait {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        self.locks = ["tokio::sync::Mutex", "tokio::sync::RwLock"].into_iter().flat_map(|path| lookup_path_str(cx.tcx, PathNS::Type, path)).collect();
        self.methods = [
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
        ]
        .into_iter()
        .flat_map(|(path, release)| lookup_path_str(cx.tcx, PathNS::Value, path).into_iter().map(move |method| (method, release)))
        .collect();
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if self.locks.is_empty() {
            return;
        }
        if let ExprKind::Closure(closure) = expr.kind
            && let ClosureKind::Coroutine(CoroutineKind::Desugared(..)) = closure.kind
            && let Some(layout) = cx.tcx.mir_coroutine_witnesses(closure.def_id)
        {
            self.held.extend(layout.field_tys.iter().filter_map(|saved| Some((saved.ty.opt_def_id()?, saved.source_info.span))));
        }
        if matches!(expr.kind, ExprKind::Field(..) | ExprKind::Path(_))
            && !expr.span.from_expansion()
            && let Some((owner, lock)) = self.owner(cx, expr)
            && !get_parent_expr(cx, expr).is_some_and(|parent| {
                matches!(parent.kind, ExprKind::MethodCall(_, receiver, ..) if receiver.hir_id == expr.hir_id)
                    || matches!(parent.kind, ExprKind::AddrOf(..)) && get_parent_expr(cx, parent).is_some_and(|call| recloned(cx, call, owner))
            })
        {
            self.owners.entry(owner).or_insert(Usage { lock, sites: Vec::new(), escaped: false }).escaped = true;
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && let Some((owner, lock)) = self.owner(cx, receiver)
            && let Some(method) = cx.typeck_results().type_dependent_def_id(expr.hir_id)
        {
            let guard = match self.methods.iter().find_map(|(known, release)| (*known == method).then_some(*release)) {
                Some(Release::Exclusive) => return,
                None if recloned(cx, expr, owner) => return,
                Some(Release::Awaited) => awaited(cx, expr),
                Some(Release::Blocking) => Some(expr),
                None => None,
            };
            let site = guard.and_then(|guard| Some(Site { call: expr.span, scope: guard_scope(cx, guard)?, guard: cx.typeck_results().expr_ty(guard).opt_def_id()? }));
            let usage = self.owners.entry(owner).or_insert(Usage { lock, sites: Vec::new(), escaped: false });
            match site {
                Some(site) => usage.sites.push(site),
                None => usage.escaped = true,
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let typeck = cx.typeck_results();
        if let PatKind::Struct(_, fields, _) = pat.kind
            && let Some(adt) = typeck.pat_ty(pat).ty_adt_def().filter(|adt| adt.is_struct())
        {
            self.bindings.extend(fields.iter().filter_map(|field| {
                if let PatKind::Binding(_, binding, _, None) = field.pat.kind
                    && lock_of(cx, &self.locks, typeck.pat_ty(field.pat)).is_some()
                {
                    Some((binding, adt.non_enum_variant().fields.get(typeck.opt_field_index(field.hir_id)?)?.did))
                } else {
                    None
                }
            }));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let unheld = self.owners.iter().filter(|(owner, usage)| {
            !usage.escaped
                && !usage.sites.is_empty()
                && match **owner {
                    Owner::Field(item) | Owner::Static(item) => item.as_local().is_some_and(|local| !cx.effective_visibilities.is_exported(local)),
                    Owner::Local(_) => true,
                }
                && !usage.sites.iter().any(|site| self.held.iter().any(|(guard, span)| *guard == site.guard && site.scope.contains(*span)))
        });
        for (owner, usage) in unheld {
            let declared = match *owner {
                Owner::Field(item) | Owner::Static(item) => cx.tcx.def_span(item),
                Owner::Local(binding) => cx.tcx.hir_span(binding),
            };
            span_lint_and_then(cx, ASYNC_MUTEX_WITHOUT_AWAIT, declared, format!("`{}` holds no guard across an `.await`", cx.tcx.def_path_str(usage.lock)), |diag| {
                diag.span_note(usage.sites.iter().map(|site| site.call).collect::<Vec<_>>(), "every lock site releases its guard before the next `.await`");
                diag.help(format!("declare `parking_lot::{}` in place of `{}`", cx.tcx.item_name(usage.lock), cx.tcx.def_path_str(usage.lock)));
            });
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
