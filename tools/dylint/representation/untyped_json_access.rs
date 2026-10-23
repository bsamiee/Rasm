use core::{iter, slice};

use clippy_utils::{
    consts::{ConstEvalCtxt, Constant},
    diagnostics::span_lint_and_help,
    expr_or_init, fn_def_id, get_parent_expr, last_path_segment,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    source::snippet,
    sym,
};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, Node, Pat, PatExpr, PatExprKind, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, fn_item_value, is_text, item_path, paths, resolve, signature};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Document {
    Value,
    Map,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HELP: &str = "decode the document into a struct deriving `serde::Deserialize`";

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    documents: FxHashMap<DefId, Document>,
    methods: FxHashMap<DefId, Document>,
}

lint!(Pass; UNTYPED_JSON_ACCESS: "JSON documents accessed by variant or fixed key as an untyped `serde_json::Value` or `Map`");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let documents = resolve(tcx, PathNS::Type, &[("serde_json::Value", Document::Value), ("serde_json::Map", Document::Map)]);
        let built = paths(tcx, PathNS::Value, &["serde_json::Map::insert", "serde_json::Map::entry"]);
        let methods = documents
            .iter()
            .flat_map(|(&document, &kind)| {
                tcx.inherent_impls(document)
                    .iter()
                    .flat_map(|implementation| tcx.associated_items(*implementation).in_definition_order())
                    .filter(|item| item.is_method() && !built.contains(&item.def_id))
                    .map(move |method| (method.def_id, kind))
            })
            .collect();
        Self { documents, methods }
    }

    fn document(&self, ty: Ty<'_>) -> Option<Document> { ty.peel_refs().opt_def_id().and_then(|document| self.documents.get(&document)).copied() }

    fn keyed(&self, cx: &LateContext<'_>, source: &Expr<'_>) -> bool {
        let keys = match source.kind {
            ExprKind::MethodCall(_, _, keys, _) | ExprKind::Call(_, [_, keys @ ..])
                if fn_def_id(cx, source).is_some_and(|method| self.methods.contains_key(&method) && cx.tcx.item_name(method) == sym::get) =>
                keys,
            ExprKind::Index(document, key, _) if self.document(cx.typeck_results().expr_ty(document)).is_some() => slice::from_ref(key),
            _ => &[],
        };
        keys.iter().any(|key| {
            key.peel_borrows().res_local_id().is_some_and(|binding| {
                matches!(cx.tcx.parent_hir_node(binding), Node::Param(_))
                    && cx
                        .tcx
                        .trait_impl_of_assoc(cx.tcx.hir_enclosing_body_owner(binding).to_def_id())
                        .and_then(|implementation| cx.tcx.impl_opt_trait_id(implementation))
                        .is_some_and(|implemented| !implemented.is_local())
            })
        })
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn fixed(cx: &LateContext<'_>, key: &Expr<'_>) -> Option<String> {
            let key = expr_or_init(cx, key.peel_borrows());
            if let ExprKind::MethodCall(_, operand, [], _) | ExprKind::Call(_, [operand]) = key.kind
                && is_text(cx, cx.typeck_results().expr_ty(key))
            {
                fixed(cx, operand)
            } else {
                Some(match ConstEvalCtxt::new(cx).eval(key)? {
                    Constant::Str(text) => format!("{text:?}"),
                    Constant::Int(index) => index.to_string(),
                    _ => snippet(cx, key.span, "..").into_owned(),
                })
            }
        }

        let accessed = |method: Option<DefId>, arguments: &[Expr<'_>], operand: Option<&Expr<'_>>| {
            let method = method?;
            let kind = *self.methods.get(&method)?;
            let access = match kind {
                _ if let Some(key) = arguments.iter().find_map(|key| fixed(cx, key)) => format!("reads `{key}` from an untyped JSON document"),
                Document::Value if arguments.is_empty() && self.document(signature(cx.tcx, method).output()).is_none() && !operand.is_some_and(|operand| self.keyed(cx, operand)) =>
                    String::from("reads an untyped JSON document by variant"),
                Document::Value | Document::Map => return None,
            };
            Some(with_forced_trimmed_paths!(format!("`{}` {access}", item_path(cx, method))))
        };
        if !expanded_externally(cx, expr.span)
            && let Some((span, message)) = match expr.kind {
                ExprKind::MethodCall(segment, receiver, arguments, _) =>
                    accessed(fn_def_id(cx, expr), arguments, Some(receiver)).map(|message| (segment.ident.span.to(expr.span.shrink_to_hi()), message)),
                ExprKind::Call(_, [operand, arguments @ ..]) => accessed(fn_def_id(cx, expr), arguments, Some(operand)).map(|message| (expr.span, message)),
                ExprKind::Path(_) => {
                    let receiver = get_parent_expr(cx, expr).and_then(|call| if let ExprKind::MethodCall(_, receiver, ..) = call.kind { Some(receiver) } else { None });
                    fn_item_value(cx, expr).and_then(|function| accessed(Some(function), &[], receiver)).map(|message| (expr.span, message))
                }
                ExprKind::Index(..)
                    if !get_parent_expr(cx, expr).is_some_and(|outer| matches!(outer.kind, ExprKind::Index(inner, ..) if inner.hir_id == expr.hir_id))
                        && let keys = iter::successors(Some(expr), |indexed| if let ExprKind::Index(document, ..) = indexed.kind { Some(document) } else { None })
                            .map_while(|indexed| match indexed.kind {
                                ExprKind::Index(document, key, _) if self.document(cx.typeck_results().expr_ty(document)).is_some() => fixed(cx, key),
                                _ => None,
                            })
                            .collect::<Vec<_>>()
                        && !keys.is_empty() =>
                    Some((expr.span, format!("indexing reads `{}` from an untyped JSON document", keys.iter().rev().format_with("", |key, write| write(&format_args!("[{key}]")))))),
                _ => None,
            }
        {
            span_lint_and_help(cx, UNTYPED_JSON_ACCESS, span, message, None, HELP);
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::TupleStruct(qpath, ..) | PatKind::Struct(qpath, ..) | PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), .. }) = pat.kind
            && !expanded_externally(cx, pat.span)
            && self.document(cx.typeck_results().pat_ty(pat)) == Some(Document::Value)
            && !cx
                .tcx
                .hir_parent_iter(pat.hir_id)
                .find(|(_, node)| !matches!(node, Node::Pat(_) | Node::PatField(_) | Node::Arm(_)))
                .and_then(|(_, node)| match node {
                    Node::LetStmt(local) => local.init,
                    Node::Expr(&Expr { kind: ExprKind::Match(scrutinee, ..), .. }) => Some(scrutinee),
                    Node::Expr(&Expr { kind: ExprKind::Let(binding), .. }) => Some(binding.init),
                    _ => None,
                })
                .is_some_and(|scrutinee| self.keyed(cx, scrutinee))
        {
            span_lint_and_help(cx, UNTYPED_JSON_ACCESS, pat.span, format!("`Value::{}` pattern reads an untyped JSON document by variant", last_path_segment(&qpath).ident), None, HELP);
        }
    }
}
