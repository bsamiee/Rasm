use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, is_in_test, paths::PathNS, res::MaybeDef as _, source::snippet, sym, ty::InteriorMut};
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    Expr, ExprKind, Item, ItemKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, LintContext as _, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, Ty, TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::{Ident, Span};

use crate::{Place, fn_value, generated, path, place, receiving_call, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Cell {
    Lazy,
    Once,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    INTERIOR_MUTABLE_STATIC,
    Warn,
    "`static` items, `thread_local!` ones included, whose type mutates through a shared reference"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct InteriorMutableStatic<'tcx> {
    cells: FxHashMap<DefId, Cell>,
    initialize: Option<DefId>,
    interior: InteriorMut<'tcx>,
    statics: FxIndexMap<LocalDefId, (Ident, Ty<'tcx>, Ty<'tcx>, Ty<'tcx>)>,
    uses: FxHashMap<LocalDefId, Vec<Option<Span>>>,
}

impl_lint_pass!(InteriorMutableStatic<'_> => [INTERIOR_MUTABLE_STATIC]);

impl<'tcx> InteriorMutableStatic<'tcx> {
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

impl<'tcx> LateLintPass<'tcx> for InteriorMutableStatic<'tcx> {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Static(_, ident, ..) | ItemKind::Const(ident, ..) = item.kind
            && !ident.span.in_external_macro(cx.sess().source_map())
            && !generated(cx, item.hir_id())
            && let declared = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization()
            && let Some(held) = match (item.kind, declared.kind()) {
                (ItemKind::Static(..), ty::Adt(lazy, args)) if self.cells.get(&lazy.did()) == Some(&Cell::Lazy) => Some(args.type_at(0)),
                (ItemKind::Static(..), _) => Some(declared),
                (_, ty::Adt(key, args)) if key.is_diag_item(cx, sym::LocalKey) => Some(args.type_at(0)),
                _ => None,
            }
            && let Some(link) = self.interior.interior_mut_ty_chain(cx, held).and_then(|chain| chain.iter().find(|&link| held.walk().any(|written| written.as_type() == Some(link))))
            && !is_in_test(cx.tcx, item.hir_id())
        {
            self.statics.insert(item.owner_id.def_id, (ident, declared, held, link));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(Place::Static(item)) = place(cx, expr) {
            let initialized = receiving_call(cx, expr)
                .filter(|&(_, method)| Some(method) == self.initialize)
                .and_then(|(call, _)| if let ExprKind::MethodCall(_, _, [initializer], _) = call.kind { Some(initializer) } else { None })
                .filter(|initializer| matches!(initializer.kind, ExprKind::Closure(closure) if cx.tcx.closure_captures(closure.def_id).is_empty()) || fn_value(cx, initializer).is_some())
                .map(|initializer| initializer.span);
            self.uses.entry(item).or_default().push(initialized);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (item, (ident, declared, held, link)) in mem::take(&mut self.statics) {
            let once = if let ty::Adt(cell, args) = declared.kind()
                && self.cells.get(&cell.did()) == Some(&Cell::Once)
                && !cx.effective_visibilities.is_exported(item)
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
                    format!("`static {ident}` holds `{held}`, whose `{link}` mutates through a shared reference")
                }),
                |diag| {
                    match once.zip(initializer) {
                        Some((value, initializer)) => diag.help(with_forced_trimmed_paths!(format!(
                            "declare `static {ident}: LazyLock<{value}> = LazyLock::new({})`, its one use initializes it from no input",
                            snippet(cx, initializer, "..")
                        ))),
                        None => diag.help("build the value in `main` and pass it to each operation that reads or writes it"),
                    };
                },
            );
        }
    }
}
