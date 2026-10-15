use clippy_utils::{
    consts::ConstEvalCtxt,
    diagnostics::span_lint_and_help,
    fn_def_id, get_parent_expr, last_path_segment,
    paths::PathNS,
    res::{MaybeDef as _, MaybeQPath as _},
};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, Pat, PatExpr, PatExprKind, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{Ty, TyCtxt, print::with_forced_trimmed_paths};

use crate::{is_text, item_path, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Document {
    Value,
    Map,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    UNTYPED_DOCUMENT,
    Warn,
    "JSON documents read by variant or fixed key as an untyped `serde_json::Value` or `Map`"
}

const HELP: &str = "declare a `#[derive(Deserialize, Serialize)]` record for the document and decode into it";

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct UntypedDocument {
    documents: FxHashMap<DefId, Document>,
    methods: FxHashMap<DefId, Document>,
}

impl_lint_pass!(UntypedDocument => [UNTYPED_DOCUMENT]);

impl UntypedDocument {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let documents = resolve(tcx, PathNS::Type, &[("serde_json::Value", Document::Value), ("serde_json::Map", Document::Map)]);
        let methods = documents
            .iter()
            .flat_map(|(&document, &kind)| {
                tcx.inherent_impls(document)
                    .iter()
                    .flat_map(|implementation| tcx.associated_items(*implementation).in_definition_order())
                    .filter(|item| item.is_method())
                    .map(move |method| (method.def_id, kind))
            })
            .collect();
        Self { documents, methods }
    }

    fn document(&self, ty: Ty<'_>) -> Option<Document> { ty.peel_refs().opt_def_id().and_then(|document| self.documents.get(&document)).copied() }
}

impl<'tcx> LateLintPass<'tcx> for UntypedDocument {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn fixed(cx: &LateContext<'_>, key: &Expr<'_>) -> bool {
            if let ExprKind::MethodCall(_, operand, [], _) | ExprKind::Call(_, [operand]) = key.kind
                && is_text(cx, cx.typeck_results().expr_ty(key))
            {
                fixed(cx, operand)
            } else {
                ConstEvalCtxt::new(cx).eval(key).is_some()
            }
        }

        let accessed = |method: Option<DefId>, arguments: &[Expr<'_>]| {
            let method = method?;
            let kind = *self.methods.get(&method)?;
            let access = if arguments.iter().any(|key| fixed(cx, key)) {
                "accesses an untyped JSON document by a fixed key"
            } else if kind == Document::Value && arguments.is_empty() && self.document(cx.tcx.fn_sig(method).instantiate_identity().skip_normalization().skip_binder().output()).is_none() {
                "reads an untyped JSON document by variant"
            } else {
                return None;
            };
            Some(with_forced_trimmed_paths!(format!("`{}` {access}", item_path(cx, method))))
        };
        if !expr.span.from_expansion()
            && let Some((span, message)) = {
                let invoked = if let ExprKind::MethodCall(segment, _, arguments, _) = expr.kind {
                    accessed(fn_def_id(cx, expr), arguments).map(|message| (segment.ident.span.to(expr.span.shrink_to_hi()), message))
                } else {
                    None
                };
                invoked
                    .or_else(|| if let ExprKind::Call(_, [_, arguments @ ..]) = expr.kind { accessed(fn_def_id(cx, expr), arguments).map(|message| (expr.span, message)) } else { None })
                    .or_else(|| {
                        matches!(expr.kind, ExprKind::Path(_))
                            .then(|| accessed(expr.res(cx).opt_def_id(), &[]))
                            .flatten()
                            .filter(|_| !get_parent_expr(cx, expr).is_some_and(|call| matches!(call.kind, ExprKind::Call(callee, _) if callee.hir_id == expr.hir_id)))
                            .map(|message| (expr.span, message))
                    })
                    .or_else(|| {
                        if let ExprKind::Index(document, key, _) = expr.kind
                            && self.document(cx.typeck_results().expr_ty(document)).is_some()
                            && fixed(cx, key)
                            && !get_parent_expr(cx, expr).is_some_and(|outer| matches!(outer.kind, ExprKind::Index(inner, ..) if inner.hir_id == expr.hir_id))
                        {
                            Some((expr.span, "indexing accesses an untyped JSON document by a fixed key".to_owned()))
                        } else {
                            None
                        }
                    })
            }
        {
            span_lint_and_help(cx, UNTYPED_DOCUMENT, span, message, None, HELP);
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        if let PatKind::TupleStruct(qpath, ..) | PatKind::Struct(qpath, ..) | PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id: _, span: _ }) = pat.kind
            && !pat.span.from_expansion()
            && self.document(cx.typeck_results().pat_ty(pat)) == Some(Document::Value)
        {
            span_lint_and_help(cx, UNTYPED_DOCUMENT, pat.span, format!("`Value::{}` pattern reads an untyped JSON document by variant", last_path_segment(&qpath).ident), None, HELP);
        }
    }
}
