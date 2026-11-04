use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    source::snippet,
    sym,
    usage::local_used_after_expr,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_errors::Applicability;
use rustc_hir::{
    Body, BorrowKind, Expr, ExprKind, HirId, Mutability, QPath,
    def_id::{DefId, LocalDefId},
};
use rustc_hir_typeck::expr_use_visitor::{Delegate, ExprUseVisitor, PlaceBase, PlaceWithHirId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{
    mir::FakeReadCause,
    ty::{self, TyCtxt},
};

use super::uses_global_allocator;
use crate::{associated_trait, expanded_externally, path, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

struct Borrows {
    sites: FxHashMap<HirId, Option<HirId>>,
}

impl<'tcx> Delegate<'tcx> for Borrows {
    fn consume(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}

    fn use_cloned(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}

    fn borrow(&mut self, place_with_id: &PlaceWithHirId<'tcx>, _: HirId, _: ty::BorrowKind) {
        if let PlaceBase::Local(local) = place_with_id.place.base {
            self.sites.entry(local).and_modify(|site| *site = site.filter(|&site| site == place_with_id.hir_id)).or_insert(Some(place_with_id.hir_id));
        }
    }

    fn mutate(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}

    fn fake_read(&mut self, _: &PlaceWithHirId<'tcx>, _: FakeReadCause, _: HirId) {}
}

pub(crate) struct Pass {
    lossy: Option<DefId>,
    owned: Option<DefId>,
    borrows: FxHashMap<LocalDefId, Borrows>,
}

lint!(Pass; MANUAL_FROM_UTF8_LOSSY_OWNED: "`String::from_utf8_lossy(&bytes)` converted to an owned `String` when nothing reads the buffer afterwards");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { lossy: path(tcx, PathNS::Value, "alloc::string::String::from_utf8_lossy"), owned: path(tcx, PathNS::Value, "alloc::borrow::Cow::into_owned"), borrows: FxHashMap::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(_, lossy, [], _) | ExprKind::Call(_, [lossy]) = expr.kind
            && let Some(method) = fn_def_id(cx, expr)
            && (Some(method) == self.owned || matches!(associated_trait(cx.tcx, method).opt_diag_name(cx), Some(sym::ToString | sym::ToOwned | sym::Into)))
            && let typeck = cx.typeck_results()
            && typeck.expr_ty(expr).is_lang_item(cx, LangItem::String)
            && let ExprKind::Call(callee, [borrowed]) = lossy.kind
            && let Some(called) = fn_def_id(cx, lossy).filter(|&called| Some(called) == self.lossy)
            && let ExprKind::Path(path) = &callee.kind
            && typeck.qpath_res(path, callee.hir_id).opt_def_id() == Some(called)
            && let Some(member) = match path {
                QPath::Resolved(_, path) => path.segments.last().map(|segment| segment.ident),
                QPath::TypeRelative(_, segment) => Some(segment.ident),
            }
            && !expanded_externally(cx, expr.span)
            && let ExprKind::AddrOf(BorrowKind::Ref, Mutability::Not, bytes) = borrowed.kind
            && let ty::Adt(vector, arguments) = typeck.expr_ty(bytes).kind()
            && vector.is_diag_item(cx, sym::Vec)
            && uses_global_allocator(cx, *vector, arguments)
            && let Some(root) = iter::successors(Some(bytes), |place| if let ExprKind::Field(base, _) = place.kind { Some(base) } else { None })
                .skip(1)
                .try_fold(bytes, |_, base| (typeck.expr_adjustments(base).is_empty() && typeck.expr_ty(base).ty_adt_def().is_some_and(|owner| owner.destructor(cx.tcx).is_none())).then_some(base))
            && let Some(local) = root.res_local_id()
            && let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id)
            && cx.tcx.hir_enclosing_body_owner(local) == owner
            && !local_used_after_expr(cx, local, expr)
            && self
                .borrows
                .entry(owner)
                .or_insert_with(|| {
                    let mut borrows = Borrows { sites: FxHashMap::default() };
                    ExprUseVisitor::for_clippy(cx, owner, &mut borrows).consume_body(cx.tcx.hir_body_owned_by(owner)).into_ok();
                    borrows
                })
                .sites
                .get(&local)
                == Some(&Some(bytes.hir_id))
        {
            let bytes = snippet(cx, bytes.span, "..");
            span_lint_hir_and_then(cx, MANUAL_FROM_UTF8_LOSSY_OWNED, expr.hir_id, expr.span, format!("lossy decoding copies `{bytes}`, which nothing reads afterwards"), |diag| {
                suggest(
                    cx,
                    expr.hir_id,
                    diag,
                    expr.span,
                    "call `String::from_utf8_lossy_owned`",
                    format!("{}from_utf8_lossy_owned({bytes})", snippet(cx, callee.span.with_hi(member.span.lo()), "..")),
                    Applicability::MachineApplicable,
                );
            });
        }
    }

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) { self.borrows.remove(&cx.tcx.hir_body_owner_def_id(body.id())); }
}
