use clippy_utils::{diagnostics::span_lint_hir_and_then, sym};
use rustc_ast::{
    token::{self, TokenKind},
    tokenstream::TokenTree,
};
use rustc_attr_ir::{AttrArgs, Attribute};
use rustc_hir::{HirId, Item, ItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{FieldDef, VariantDef};
use rustc_parse_format::{ParseMode, Parser, Piece, Position};
use rustc_span::{Span, Symbol};

use crate::{derives, expanded_externally};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(SOURCE_IN_ERROR_MESSAGE: "`#[error(\"..\")]` messages that print the variant's `#[source]`, `#[from]`, or `source` field");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        fn printed_source(cx: &LateContext<'_>, variant: &VariantDef) -> Option<(HirId, Span, String, String)> {
            let tcx = cx.tcx;
            let [error, source] = ["error", "source"].map(Symbol::intern);
            let node = tcx.local_def_id_to_hir_id(variant.def_id.as_local()?);
            let attribute = tcx.hir_attrs(node).iter().find(|attribute| attribute.has_name(error))?;
            let Attribute::Unparsed(parsed) = attribute else { return None };
            let AttrArgs::Delimited(delimited) = &parsed.args else { return None };
            let tokens = delimited.tokens.iter().map(|tree| if let TokenTree::Token(token, _) = tree { Some(&token.kind) } else { None }).collect::<Vec<_>>();
            let mut groups = tokens.split(|kind| matches!(kind, Some(TokenKind::Comma)));
            let [Some(TokenKind::Literal(token::Lit { kind: token::LitKind::Str, symbol: text, .. }))] = groups.next()? else { return None };
            let arguments = groups
                .filter(|group| !group.is_empty())
                .map(|group| {
                    let (name, value) = match group {
                        [Some(TokenKind::Ident(name, _)), Some(TokenKind::Eq), value @ ..] => (Some(*name), value),
                        value => (None, value),
                    };
                    let field = match value {
                        [Some(TokenKind::Dot), Some(TokenKind::Ident(field, _) | TokenKind::Literal(token::Lit { kind: token::LitKind::Integer, symbol: field, .. }))] => Some(*field),
                        _ => None,
                    };
                    (name, field)
                })
                .collect::<Vec<_>>();
            let marked = |field: &FieldDef| field.did.as_local().is_some_and(|local| tcx.hir_attrs(tcx.local_def_id_to_hir_id(local)).iter().any(|marker| marker.has_any_name(&[source, sym::from])));
            let named = |name: Symbol| variant.fields.iter().find(|field| field.name == name).filter(|field| field.name == source || marked(field)).map(|_| name.to_string());
            let pieces = Parser::new(text.as_str(), None, None, false, ParseMode::Format).collect::<Vec<_>>();
            let shown = pieces.iter().filter_map(|piece| if let Piece::NextArgument(argument) = piece { Some(&argument.position) } else { None }).find_map(|position| match *position {
                Position::ArgumentNamed(name) => {
                    let name = Symbol::intern(name);
                    arguments.iter().find(|&&(written, _)| written == Some(name)).map_or_else(|| named(name), |&(_, field)| field.and_then(named))
                }
                Position::ArgumentIs(index) if arguments.is_empty() => named(sym::integer(index)),
                Position::ArgumentIs(index) | Position::ArgumentImplicitlyIs(index) =>
                    arguments.iter().filter(|(written, _)| written.is_none()).nth(index).and_then(|&(_, field)| field).and_then(named),
            })?;
            let help = match &pieces {
                [Piece::NextArgument(_)] => format!("state the failure in the message in place of field `{shown}`, reporters print the source through `source()`"),
                _ => format!("delete field `{shown}` from the message, reporters print it through `source()`"),
            };
            Some((node, attribute.span(), format!("`{}` message prints its source field `{shown}`", variant.name), help))
        }

        if let ItemKind::Struct(..) | ItemKind::Enum(..) = item.kind
            && !expanded_externally(cx, item.span)
            && derives(cx, cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization(), sym::Error)
        {
            for (node, span, message, help) in cx.tcx.adt_def(item.owner_id).variants().iter().filter_map(|variant| printed_source(cx, variant)) {
                span_lint_hir_and_then(cx, SOURCE_IN_ERROR_MESSAGE, node, span, message, |diag| {
                    diag.help(help);
                });
            }
        }
    }
}
