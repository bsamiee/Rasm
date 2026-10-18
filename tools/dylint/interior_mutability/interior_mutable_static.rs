use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS, res::MaybeDef as _, source::snippet, sym, ty::InteriorMut};
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    Expr, ExprKind, Item, ItemKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::{Ident, Span};

use crate::{Place, expanded_externally, exported, fn_item_value, generated, method_call_on, path, place, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Cell {
    Lazy,
    Once,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    cells: FxHashMap<DefId, Cell>,
    initialize: Option<DefId>,
    interior: InteriorMut<'tcx>,
    statics: FxIndexMap<LocalDefId, (Ident, Ty<'tcx>, Ty<'tcx>, Ty<'tcx>)>,
    uses: FxHashMap<LocalDefId, Vec<Option<Span>>>,
}

lint!(Pass<'_>; INTERIOR_MUTABLE_STATIC: "`static` items, `thread_local!` statics included, with a type that mutates through a shared reference");

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(tcx: TyCtxt<'tcx>) -> Self {
        Self {
            cells: resolve(tcx, PathNS::Type, &[("std::sync::LazyLock", Cell::Lazy), ("std::sync::OnceLock", Cell::Once)]),
            initialize: path(tcx, PathNS::Value, "std::sync::OnceLock::get_or_init"),
            interior: InteriorMut::new(tcx, &[]),
            statics: FxIndexMap::default(),
            uses: FxHashMap::default(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Static(_, ident, ..) | ItemKind::Const(ident, ..) = item.kind
            && !expanded_externally(cx, ident.span)
            && !generated(cx, item.hir_id())
            && let declared = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization()
            && let Some(held) = match (item.kind, declared.kind()) {
                (ItemKind::Static(..), ty::Adt(lazy, args)) if self.cells.get(&lazy.did()) == Some(&Cell::Lazy) => Some(args.type_at(0)),
                (ItemKind::Static(..), _) => Some(declared),
                (_, ty::Adt(key, args)) if key.is_diag_item(cx, sym::LocalKey) => Some(args.type_at(0)),
                _ => None,
            }
            && let Some(link) = self.interior.interior_mut_ty_chain(cx, held).and_then(|chain| chain.iter().find(|&link| held.walk().any(|written| written.as_type() == Some(link))))
            && !cx.tcx.sess.is_test_crate()
        {
            self.statics.insert(item.owner_id.def_id, (ident, declared, held, link));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(Place::Static(item)) = place(cx, expr) {
            let initialized = method_call_on(cx, expr)
                .filter(|&(_, method)| Some(method) == self.initialize)
                .and_then(|(call, _)| if let ExprKind::MethodCall(_, _, [initializer], _) = call.kind { Some(initializer) } else { None })
                .filter(|initializer| matches!(initializer.kind, ExprKind::Closure(closure) if cx.tcx.closure_captures(closure.def_id).is_empty()) || fn_item_value(cx, initializer).is_some())
                .map(|initializer| initializer.span);
            self.uses.entry(item).or_default().push(initialized);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (item, (ident, declared, held, link)) in mem::take(&mut self.statics) {
            let once = if let ty::Adt(cell, args) = declared.kind()
                && self.cells.get(&cell.did()) == Some(&Cell::Once)
                && !exported(cx, item)
            {
                Some(args.type_at(0))
            } else {
                None
            };
            let initializer = self.uses.get(&item).and_then(|uses| if let [Some(initializer)] = uses { Some(*initializer) } else { None });
            span_lint_hir_and_then(
                cx,
                INTERIOR_MUTABLE_STATIC,
                cx.tcx.local_def_id_to_hir_id(item),
                ident.span,
                with_forced_trimmed_paths!(if link == held {
                    format!("`static {ident}` holds `{held}`, which mutates through a shared reference")
                } else {
                    format!("`static {ident}` holds `{held}`, and its `{link}` mutates through a shared reference")
                }),
                |diag| {
                    match once.zip(initializer) {
                        Some((value, initializer)) => diag.help(with_forced_trimmed_paths!(format!(
                            "declare `static {ident}: LazyLock<{value}> = LazyLock::new({})`, its only use initializes it from no input",
                            snippet(cx, initializer, "..")
                        ))),
                        None => diag.help("build the value in `main` and pass it to each operation that reads or writes it"),
                    };
                },
            );
        }
    }
}
