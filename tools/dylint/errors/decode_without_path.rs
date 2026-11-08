use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, sym};
use rustc_ast::tokenstream::TokenTree;
use rustc_attr_ir::{AttrArgs, Attribute, HasAttrs as _};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def::CtorKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, TypeVisitableExt as _, print::with_forced_trimmed_paths};
use rustc_span::Symbol;

use crate::{expanded_externally, is_text, paths, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    deserialize: FxHashSet<DefId>,
    value: FxHashSet<DefId>,
    tracked: FxHashSet<DefId>,
    str_deserializers: FxHashSet<DefId>,
    deserialize_derive: FxHashSet<DefId>,
}

lint!(Pass; DECODE_WITHOUT_PATH: "`Deserialize::deserialize` calls on concrete untracked deserializers, except primitives, text, `serde_json::Value`, or derived unit enums decoded by `StrDeserializer`");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            deserialize: paths(tcx, PathNS::Value, &["serde::de::Deserialize::deserialize"]),
            value: paths(tcx, PathNS::Type, &["serde_json::value::Value"]),
            tracked: paths(tcx, PathNS::Type, &["serde_path_to_error::Deserializer"]),
            str_deserializers: paths(tcx, PathNS::Type, &["serde::de::value::StrDeserializer"]),
            deserialize_derive: paths(tcx, PathNS::Macro, &["serde::Deserialize"]),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let delegated = |owner: DefId, keys: &[Symbol]| {
            owner
                .get_attrs(&cx.tcx)
                .iter()
                .filter_map(|attribute| {
                    if let Attribute::DeriveHelper(helper) = attribute
                        && helper.path.segments.iter().eq([&sym::serde])
                        && let AttrArgs::Delimited(arguments) = &helper.args
                    {
                        Some(&arguments.tokens)
                    } else {
                        None
                    }
                })
                .flat_map(|tokens| tokens.iter())
                .any(|tree| matches!(tree, TokenTree::Token(token, _) if keys.iter().any(|&key| token.is_ident_named(key))))
        };
        if let ExprKind::Call(callee, [deserializer]) = expr.kind
            && let Some(called) = fn_def_id(cx, expr).and_then(|called| cx.tcx.opt_associated_item(called)?.trait_item_or_self().ok())
            && self.deserialize.contains(&called)
            && let typeck = cx.typeck_results()
            && let source = typeck.expr_ty(deserializer)
            && !source.has_param()
            && let source_adt = source.peel_refs().ty_adt_def()
            && source_adt.is_none_or(|held| !self.tracked.contains(&held.did()))
            && result_parts(cx, typeck.expr_ty(expr)).is_some_and(|(decoded, _)| {
                let target = decoded.peel_refs();
                !target.is_primitive()
                    && !is_text(cx, target)
                    && target.ty_adt_def().is_none_or(|held| {
                        !(self.value.contains(&held.did())
                            || source_adt.is_some_and(|source| self.str_deserializers.contains(&source.did()))
                                && held.is_enum()
                                && !delegated(held.did(), &[sym::from, sym::try_from])
                                && held
                                    .variants()
                                    .iter()
                                    .all(|variant| variant.ctor_kind() == Some(CtorKind::Const) && !delegated(variant.def_id, &[Symbol::intern("with"), Symbol::intern("deserialize_with")]))
                                && cx.tcx.non_blanket_impls_for_ty(cx.tcx.parent(called), decoded).any(|implementation| {
                                    cx.tcx.is_automatically_derived(implementation)
                                        && cx.tcx.expn_that_defined(implementation).expn_data().macro_def_id.is_some_and(|derived| self.deserialize_derive.contains(&derived))
                                }))
                    })
            })
            && !expanded_externally(cx, expr.span)
        {
            let source = with_forced_trimmed_paths!(source.to_string());
            span_lint_hir_and_then(cx, DECODE_WITHOUT_PATH, expr.hir_id, callee.span, format!("`Deserialize::deserialize` decodes `{source}` without tracking the field path"), |diag| {
                diag.help("decode through `serde_path_to_error::deserialize`");
            });
        }
    }
}
