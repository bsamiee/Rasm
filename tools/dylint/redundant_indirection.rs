use clippy_utils::{diagnostics::span_lint_hir_and_then, peel_blocks_with_stmt, peel_hir_expr_refs};
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    AmbigArg, Expr, ExprKind, Ty, TyKind,
    def::DefKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, AssocKind, AssocTypeData, print::with_forced_trimmed_paths};
use rustc_span::Span;

use crate::{accessed_field, constructed, parameter};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Method {
    Forward,
    Construct,
    Other,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    REDUNDANT_INDIRECTION,
    Warn,
    "crate-private traits with one implementor and crate-private newtypes whose every inherent method forwards to the wrapped value"
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct RedundantIndirection {
    objects: FxIndexMap<DefId, Vec<Span>>,
}

impl_lint_pass!(RedundantIndirection => [REDUNDANT_INDIRECTION]);

impl<'tcx> LateLintPass<'tcx> for RedundantIndirection {
    fn check_ty(&mut self, _: &LateContext<'tcx>, hir_ty: &'tcx Ty<'tcx, AmbigArg>) {
        if let TyKind::TraitObject(bounds, ..) = hir_ty.kind
            && let Some(principal) = bounds.iter().find_map(|bound| bound.trait_ref.trait_def_id().filter(|principal| principal.is_local()))
        {
            self.objects.entry(principal).or_default().push(hir_ty.span);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        fn method(cx: &LateContext<'_>, function: LocalDefId, inner: DefId) -> Method {
            let (body, typeck) = (cx.tcx.hir_body_owned_by(function), cx.tcx.typeck(function));
            let value = peel_blocks_with_stmt(body.value);
            let name = cx.tcx.item_name(function);
            let passed = |argument: &Expr<'_>| parameter(body, peel_hir_expr_refs(argument).0).is_some();
            let wrapped = |read: &Expr<'_>| {
                let read = peel_hir_expr_refs(read).0;
                matches!(read.kind, ExprKind::Field(base, _) if parameter(body, base) == Some(0)) && accessed_field(typeck, read) == Some(inner)
            };
            let forwards = matches!(value.kind, ExprKind::MethodCall(segment, receiver, arguments, _) if segment.ident.name == name && wrapped(receiver) && arguments.iter().all(passed))
                || matches!(value.kind, ExprKind::Call(callee, [receiver, arguments @ ..])
                    if matches!(&callee.kind, ExprKind::Path(qpath) if typeck.qpath_res(qpath, callee.hir_id).opt_def_id().is_some_and(|called| cx.tcx.item_name(called) == name))
                        && wrapped(receiver) && arguments.iter().all(passed));
            let constructs = constructed(typeck, value).is_some_and(|(_, fields)| matches!(*fields.as_slice(), [(field, argument)] if field.did == inner && passed(argument)))
                || matches!(value.kind, ExprKind::MethodCall(_, _, [argument], _)
                    if matches!(typeck.expr_ty(argument).kind(), ty::FnDef(ctor, _) if matches!(cx.tcx.def_kind(*ctor), DefKind::Ctor(..)) && cx.tcx.parent(*ctor) == cx.tcx.parent(inner)));
            match (forwards, constructs) {
                (true, _) => Method::Forward,
                (false, true) => Method::Construct,
                (false, false) => Method::Other,
            }
        }

        let singles = cx.tcx.hir_crate_items(()).free_items().filter_map(|item| {
            let declared = item.owner_id.to_def_id();
            if cx.tcx.def_kind(declared) == DefKind::Trait
                && !cx.tcx.def_span(declared).from_expansion()
                && !cx.effective_visibilities.is_exported(item.owner_id.def_id)
                && !cx.tcx.associated_item_def_ids(declared).is_empty()
                && !cx.tcx.associated_items(declared).in_definition_order().any(|associated| matches!(associated.kind, AssocKind::Type { data: AssocTypeData::Normal(_) }))
                && let mut impls = cx.tcx.all_impls(declared)
                && let (Some(single), None) = (impls.next(), impls.next())
                && !(cx.tcx.generics_of(single).requires_monomorphization(cx.tcx) && self.objects.contains_key(&declared))
            {
                Some((item.hir_id(), declared, cx.tcx.type_of(single).instantiate_identity().skip_normalization()))
            } else {
                None
            }
        });
        for (node, declared, implementor) in singles {
            let name = cx.tcx.def_path_str(declared);
            let local = implementor.ty_adt_def().is_some_and(|adt| adt.did().is_local());
            span_lint_hir_and_then(cx, REDUNDANT_INDIRECTION, node, cx.tcx.def_span(declared), with_forced_trimmed_paths!(format!("trait `{name}` has one implementor, `{implementor}`")), |diag| {
                if let Some(objects) = self.objects.get(&declared).map(|objects| objects.iter().copied().filter(|object| !object.from_expansion()).collect::<Vec<_>>())
                    && !objects.is_empty()
                {
                    diag.span_note(objects, with_forced_trimmed_paths!(format!("`dyn {name}` holds a `{implementor}` at each use")));
                }
                diag.help(if local {
                    with_forced_trimmed_paths!(format!("replace the trait with an inherent `impl {implementor}` block"))
                } else {
                    with_forced_trimmed_paths!(format!("delete the trait and state each method's body at its call sites on `{implementor}`"))
                });
            });
        }
        let tcx = cx.tcx;
        let wrappers = tcx.hir_crate_items(()).free_items().fold(FxIndexMap::<LocalDefId, (DefId, Vec<DefId>)>::default(), |mut wrappers, item| {
            let implementation = item.owner_id.to_def_id();
            if matches!(tcx.def_kind(implementation), DefKind::Impl { .. })
                && !tcx.is_automatically_derived(implementation)
                && let Some(wrapper) = tcx.type_of(implementation).instantiate_identity().skip_normalization().ty_adt_def()
                && wrapper.is_struct()
                && let Some(local) = wrapper.did().as_local()
                && let [inner] = &wrapper.non_enum_variant().fields.raw
            {
                wrappers.entry(local).or_insert_with(|| (inner.did, Vec::new())).1.push(implementation);
            }
            wrappers
        });
        let forwarding = wrappers.into_iter().filter(|&(wrapper, _)| !tcx.def_span(wrapper).from_expansion() && !cx.effective_visibilities.is_exported(wrapper)).filter_map(
            |(wrapper, (inner, implementations))| {
                implementations
                    .iter()
                    .flat_map(|&implementation| {
                        let inherent = tcx.impl_opt_trait_id(implementation).is_none();
                        tcx.associated_items(implementation).in_definition_order().filter(|associated| associated.is_fn()).map(move |associated| associated.def_id.as_local().filter(|_| inherent))
                    })
                    .try_fold(Vec::new(), |mut forwards, function| match function.map_or(Method::Other, |function| method(cx, function, inner)) {
                        Method::Forward => {
                            forwards.extend(function.map(|function| tcx.def_span(function)));
                            Some(forwards)
                        }
                        Method::Construct => Some(forwards),
                        Method::Other => None,
                    })
                    .filter(|forwards| !forwards.is_empty())
                    .map(|forwards| (wrapper, inner, forwards))
            },
        );
        for (wrapper, inner, forwards) in forwarding {
            let name = tcx.item_name(wrapper);
            let inner = tcx.type_of(inner).instantiate_identity().skip_normalization();
            span_lint_hir_and_then(
                cx,
                REDUNDANT_INDIRECTION,
                tcx.local_def_id_to_hir_id(wrapper),
                tcx.def_span(wrapper),
                with_forced_trimmed_paths!(format!("newtype `{name}` wraps `{inner}` and every method forwards to it")),
                |diag| {
                    diag.span_note(forwards, with_forced_trimmed_paths!(format!("forwards to `{inner}`")));
                    diag.help(with_forced_trimmed_paths!(format!("use `{inner}` in place of `{name}`")));
                },
            );
        }
    }
}
