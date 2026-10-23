use clippy_utils::{diagnostics::span_lint_hir_and_then, peel_blocks_with_stmt, peel_hir_expr_refs, res::MaybeDef as _, sym, ty::implements_trait};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    Expr, ExprKind,
    def::DefKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, print::with_forced_trimmed_paths};

use crate::{accessed_field, constructed, derived_impl, expanded_externally, exported, generated, parameter, pointee};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Method {
    Forward,
    Construct,
    Other,
}

// --- [SERVICES] ------------------------------------------------------------------------

lint!(FORWARDING_NEWTYPE: "crate-private newtypes with every inherent method forwarding to the wrapped value, a `Deref` impl exposing it, or a hand-written `AsRef` or `AsMut` impl returning it");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        fn method(cx: &LateContext<'_>, function: LocalDefId, inner: DefId) -> Method {
            let (body, typeck) = (cx.tcx.hir_body_owned_by(function), cx.tcx.typeck(function));
            let value = peel_blocks_with_stmt(&body.value);
            let name = cx.tcx.item_name(function);
            let passed = |argument: &Expr<'_>| parameter(body, peel_hir_expr_refs(argument).0).is_some();
            let relayed = |receiver: &Expr<'_>, arguments: &[Expr<'_>]| reads_field(cx, function, receiver, inner) && arguments.iter().all(passed);
            let forwards = matches!(value.kind, ExprKind::MethodCall(segment, receiver, arguments, _) if segment.ident.name == name && relayed(receiver, arguments))
                || matches!(value.kind, ExprKind::Call(callee, [receiver, arguments @ ..])
                    if matches!(&callee.kind, ExprKind::Path(qpath) if typeck.qpath_res(qpath, callee.hir_id).opt_def_id().is_some_and(|called| cx.tcx.item_name(called) == name))
                        && relayed(receiver, arguments));
            let constructs = constructed(typeck, value).is_some_and(|(_, fields)| matches!(*fields.as_slice(), [(field, argument)] if field.did == inner && passed(argument)))
                || matches!(value.kind, ExprKind::MethodCall(_, _, [argument], _)
                    if matches!(typeck.expr_ty(argument).kind(), ty::FnDef(ctor, _) if matches!(cx.tcx.def_kind(*ctor), DefKind::Ctor(..)) && cx.tcx.parent(*ctor) == cx.tcx.parent(inner)));
            match (forwards, constructs) {
                (true, _) => Method::Forward,
                (false, true) => Method::Construct,
                (false, false) => Method::Other,
            }
        }

        fn returns_field(cx: &LateContext<'_>, function: DefId, inner: DefId) -> bool {
            function.as_local().is_some_and(|function| reads_field(cx, function, peel_blocks_with_stmt(&cx.tcx.hir_body_owned_by(function).value), inner))
        }

        let tcx = cx.tcx;
        let wrappers = tcx.hir_crate_items(()).definitions().fold(FxIndexMap::<LocalDefId, (DefId, Vec<DefId>)>::default(), |mut wrappers, item| {
            let implementation = item.to_def_id();
            if matches!(tcx.def_kind(implementation), DefKind::Impl { .. })
                && let Some(wrapper) = tcx.type_of(implementation).instantiate_identity().skip_normalization().ty_adt_def()
                && wrapper.is_struct()
                && let Some(local) = wrapper.did().as_local()
                && let [inner] = &wrapper.non_enum_variant().fields.raw
                && !expanded_externally(cx, tcx.def_span(local))
                && !exported(cx, local)
            {
                wrappers.entry(local).or_insert_with(|| (inner.did, Vec::new())).1.push(implementation);
            }
            wrappers
        });
        for (wrapper, (inner, implementations)) in wrappers {
            let (name, held) = (tcx.item_name(wrapper), tcx.type_of(inner).instantiate_identity().skip_normalization());
            let plain = !held.is_any_ptr() && pointee(cx, held) == held && tcx.adt_def(wrapper).destructor(tcx).is_none();
            let findings = implementations.iter().filter_map(|&implementation| {
                let implemented = tcx.impl_opt_trait_id(implementation)?;
                let derived = implementation.as_local().is_some_and(|local| generated(cx, tcx.local_def_id_to_hir_id(local)));
                if plain
                    && matches!(tcx.as_lang_item(implemented), Some(LangItem::Deref | LangItem::DerefMut))
                    && (derived || tcx.associated_items(implementation).in_definition_order().filter(|associated| associated.is_fn()).all(|associated| returns_field(cx, associated.def_id, inner)))
                {
                    let exposing = tcx.item_name(implemented);
                    Some((
                        implementation,
                        with_forced_trimmed_paths!(format!("`impl {exposing} for {name}` exposes every method of the wrapped `{held}` on `{name}`")),
                        with_forced_trimmed_paths!(format!("use `{held}` in place of `{name}`, or remove the `{exposing}` impl and give `{name}` methods that keep its invariant")),
                    ))
                } else if !derived
                    && matches!(implemented.opt_diag_name(cx), Some(sym::AsRef | sym::AsMut))
                    && let [method] = tcx.associated_item_def_ids(implementation)
                    && let Some(local) = method.as_local()
                    && let viewed = tcx.item_name(*method)
                    && let value = peel_blocks_with_stmt(&tcx.hir_body_owned_by(local).value)
                    && (reads_field(cx, local, value, inner)
                        || matches!(value.kind, ExprKind::MethodCall(segment, receiver, [], _) if segment.ident.name == viewed && reads_field(cx, local, receiver, inner)))
                    && let target = tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization().args.type_at(1)
                    && let Some(attribute) =
                        if held == target { Some(String::new()) } else { implements_trait(cx, held, implemented, &[target.into()]).then(|| format!(" with `#[{viewed}(forward)]`")) }
                {
                    let viewing = tcx.item_name(implemented);
                    Some((
                        implementation,
                        with_forced_trimmed_paths!(format!("`impl {viewing}<{target}> for {name}` returns the wrapped `{held}` by hand")),
                        format!("derive `derive_more::{viewing}` on `{name}`{attribute} and delete the impl"),
                    ))
                } else {
                    None
                }
            });
            for (implementation, message, help) in findings {
                span_lint_hir_and_then(cx, FORWARDING_NEWTYPE, tcx.local_def_id_to_hir_id(wrapper), tcx.def_span(implementation).source_callsite(), message, |diag| {
                    diag.help(help);
                });
            }
            let forwards = implementations
                .iter()
                .filter(|&&implementation| !derived_impl(tcx, implementation))
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
                .filter(|forwards| !forwards.is_empty());
            if let Some(forwards) = forwards {
                span_lint_hir_and_then(
                    cx,
                    FORWARDING_NEWTYPE,
                    tcx.local_def_id_to_hir_id(wrapper),
                    tcx.def_span(wrapper),
                    with_forced_trimmed_paths!(format!("every non-constructor method of newtype `{name}` forwards to the wrapped `{held}`")),
                    |diag| {
                        diag.span_note(forwards, "forwards here");
                        diag.help(with_forced_trimmed_paths!(format!("use `{held}` in place of `{name}`")));
                    },
                );
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn reads_field(cx: &LateContext<'_>, function: LocalDefId, read: &Expr<'_>, inner: DefId) -> bool {
    let read = peel_hir_expr_refs(read).0;
    matches!(read.kind, ExprKind::Field(base, _) if parameter(cx.tcx.hir_body_owned_by(function), base) == Some(0)) && accessed_field(cx.tcx.typeck(function), read) == Some(inner)
}
