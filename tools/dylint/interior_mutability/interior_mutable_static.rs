use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS, res::MaybeDef as _, sym, ty::InteriorMut};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Item, ItemKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, GenericArg, TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, generated, in_test, path};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    lazy: Option<DefId>,
    interior: InteriorMut<'tcx>,
}

lint!(Pass<'_>; INTERIOR_MUTABLE_STATIC: "`static` items and `thread_local!` statics holding an interior mutable type");

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(tcx: TyCtxt<'tcx>) -> Self { Self { lazy: path(tcx, PathNS::Type, "std::sync::LazyLock"), interior: InteriorMut::new(tcx, &[]) } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Static(_, ident, ..) | ItemKind::Const(ident, ..) = item.kind
            && !expanded_externally(cx, ident.span)
            && !generated(cx, item.hir_id())
            && !in_test(cx.tcx, item.hir_id())
            && let declared = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization()
            && let Some(held) = match (item.kind, declared.kind()) {
                (ItemKind::Static(..), ty::Adt(lazy, args)) if Some(lazy.did()) == self.lazy => Some(args.type_at(0)),
                (ItemKind::Static(..), _) => Some(declared),
                (_, ty::Adt(key, args)) if key.is_diag_item(cx, sym::LocalKey) => Some(args.type_at(0)),
                _ => None,
            }
            && let Some(chain) = self.interior.interior_mut_ty_chain(cx, held)
            && let written = held.walk().filter_map(GenericArg::as_type).collect::<FxHashSet<_>>()
            && let Some(link) = chain.iter().find(|link| written.contains(link))
        {
            span_lint_hir_and_then(
                cx,
                INTERIOR_MUTABLE_STATIC,
                item.hir_id(),
                ident.span,
                with_forced_trimmed_paths!(if link == held {
                    format!("`static {ident}` holds interior mutable `{held}`")
                } else {
                    format!("`static {ident}` holds interior mutable `{link}` in `{held}`")
                }),
                |diag| {
                    diag.help("build the value in `main` and pass it to each operation that reads or writes it");
                },
            );
        }
    }
}
