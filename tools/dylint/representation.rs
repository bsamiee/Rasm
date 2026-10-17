use clippy_utils::{
    peel_blocks, peel_hir_expr_while,
    res::{MaybeDef as _, MaybeQPath as _, MaybeResPath as _},
    sym,
};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, UnOp};
use rustc_lint::LateContext;
use rustc_middle::ty::{self, AdtDef, Ty, VariantDef};
use rustc_span::Symbol;

use crate::{expanded_externally, impl_owner, parameter_bindings};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum VariantCollection {
    Array,
    Map,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn fieldless_local_enum(held: Ty<'_>) -> Option<AdtDef<'_>> {
    held.peel_refs().ty_adt_def().filter(|adt| adt.is_enum() && adt.did().is_local() && adt.variants().len() > 1 && adt.all_fields().next().is_none())
}

fn variant_collection<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(VariantCollection, AdtDef<'tcx>)> {
    let typeck = cx.typeck_results();
    if expanded_externally(cx, expr.span) {
        return None;
    }
    if let ExprKind::Array(elements @ [first, ..]) = expr.kind {
        let keyed = fieldless_local_enum(typeck.expr_ty(first))?;
        return itertools::equal(elements.iter().map(|element| element.res(typeck).opt_def_id()), keyed.variants().iter().map(VariantDef::ctor_def_id)).then_some((VariantCollection::Array, keyed));
    }
    let ty::Adt(map, args) = typeck.expr_ty(expr).kind() else { return None };
    let keyed = map.is_diag_item(cx, sym::BTreeMap).then(|| fieldless_local_enum(args.type_at(0)))??;
    let pairs = match expr.kind {
        ExprKind::Call(_, [pairs]) => pairs,
        ExprKind::MethodCall(collected, pairs, [], _) if collected.ident.name == sym::collect => pairs,
        _ => return None,
    };
    let enumerated = |source: &'tcx Expr<'tcx>| {
        let source = peel_hir_expr_while(source, |step| match step.kind {
            ExprKind::MethodCall(segment, receiver, [], _)
                if matches!(segment.ident.name, sym::iter | sym::into_iter | sym::copied | sym::cloned | sym::rev | sym::fuse | sym::peekable | sym::by_ref) =>
                Some(receiver),
            ExprKind::MethodCall(segment, receiver, [_], _) if segment.ident.name == sym::inspect => Some(receiver),
            _ => None,
        });
        let owned = |path: &'tcx Expr<'tcx>, name: Symbol| {
            path.res_if_named(typeck, name).opt_def_id().is_some_and(|item| {
                impl_owner(cx.tcx, item).and_then(Ty::ty_adt_def) == Some(keyed)
                    || cx.tcx.trait_of_assoc(item).is_some() && typeck.node_args(path.hir_id).types().next().and_then(Ty::ty_adt_def) == Some(keyed)
            })
        };
        matches!(source.kind, ExprKind::Call(callee, []) if owned(callee, sym::iter)) || owned(source, Symbol::intern("VARIANTS"))
    };
    let listed = |entries: &'tcx [Expr<'tcx>]| {
        entries.len() == keyed.variants().len()
            && entries
                .iter()
                .map(|entry| if let ExprKind::Tup([key, _]) = entry.kind { key.res(typeck).opt_def_id() } else { None })
                .try_collect::<FxHashSet<_>>()
                .is_some_and(|keys| keys.len() == entries.len())
    };
    match pairs.kind {
        ExprKind::MethodCall(mapped, source, [Expr { kind: ExprKind::Closure(closure), .. }], _) if mapped.ident.name == sym::map =>
            matches!(peel_blocks(&cx.tcx.hir_body(closure.body).value).kind, ExprKind::Tup([key, _])
                if peel_hir_expr_while(key, |inner| if let ExprKind::Unary(UnOp::Deref, operand) = inner.kind { Some(operand) } else { None })
                    .res_local_id()
                    .is_some_and(|binding| parameter_bindings(cx.tcx, closure.body).contains(&binding)))
                && enumerated(source),
        ExprKind::MethodCall(zipped, source, [_], _) if zipped.ident.name == sym::zip => enumerated(source),
        ExprKind::MethodCall(iterated, &Expr { kind: ExprKind::Array(entries), .. }, [], _) if iterated.ident.name == sym::into_iter => listed(entries),
        ExprKind::Array(entries) => listed(entries),
        _ => false,
    }
    .then_some((VariantCollection::Map, keyed))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod fn_pointer_iterator_type;
pub(crate) mod formatted_tracing_value;
pub(crate) mod forwarding_newtype;
pub(crate) mod manual_serialization;
pub(crate) mod manual_slot_map;
pub(crate) mod manual_variant_collection;
pub(crate) mod needless_reference_field;
pub(crate) mod needless_struct;
pub(crate) mod option_field_pairs;
pub(crate) mod primitive_domain_binding;
pub(crate) mod sentinel_default;
pub(crate) mod single_impl_trait;
pub(crate) mod stringified_uuid;
pub(crate) mod stringly_typed_value;
pub(crate) mod unchecked_construction;
pub(crate) mod unparsed_field;
pub(crate) mod untyped_json_access;
pub(crate) mod variant_constant_functions;
