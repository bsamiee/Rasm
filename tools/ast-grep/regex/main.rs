#![feature(error_reporter, type_alias_impl_trait)]

use std::{error::Report, io};

use ast_grep_config::Transformation;
use regex_syntax::{
    ast::{self, Ast, ClassSetItem, Flag, Visitor, parse::Parser, visit},
    hir::{self, Hir, HirKind, translate::TranslatorBuilder},
};
use serde::{Deserialize, Serialize};

// --- [TYPES] ---------------------------------------------------------------------------

type Unparsed = impl core::error::Error + Send + Sync + 'static;

#[derive(Serialize)]
enum Reading {
    Plain,
    UnicodePerl,
}

#[derive(Serialize)]
enum Polarity {
    Stated,
    Complement,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCOPED: [Flag; 2] = [Flag::CaseInsensitive, Flag::Unicode];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Deserialize)]
struct Token {
    pattern: String,
    caseless: bool,
}

#[derive(Deserialize)]
#[serde(untagged)]
enum Written {
    Regex(String),
    Transform { transform: serde_json::Value },
}

#[derive(Deserialize)]
struct Replaced {
    replace: String,
}

#[derive(Deserialize)]
struct Transformed {
    replace: Option<Replaced>,
}

#[derive(Deserialize)]
struct Request {
    tokens: Vec<Token>,
    regexes: Vec<Written>,
}

#[derive(Serialize)]
struct Class {
    source: String,
    reading: Reading,
    polarity: Polarity,
    ranges: Vec<(u32, u32)>,
}

#[derive(Serialize)]
#[serde(tag = "type")]
enum Node {
    CharacterClass { ranges: Vec<(u32, u32)> },
    Alternation { alternatives: Vec<Vec<Self>> },
    Quantifier { min: u32, max: Option<u32>, alternatives: Vec<Vec<Self>> },
}

#[derive(Serialize)]
struct Reply {
    tokens: Vec<Node>,
    classes: Vec<Vec<Class>>,
}

#[derive(Clone, Copy)]
struct Scope([bool; SCOPED.len()]);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("cannot decode the request")]
    Request(#[source] serde_path_to_error::Error<serde_json::Error>),
    #[error("cannot read pattern {pattern}")]
    Pattern { pattern: String, source: Box<regex_syntax::Error> },
    #[error("cannot read transform {transform}")]
    Document { transform: serde_json::Value, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("cannot parse transform {transform}")]
    Text { transform: serde_json::Value, source: Unparsed },
    #[error("cannot write the reply")]
    Reply(#[source] serde_json::Error),
}

impl Failure {
    fn pattern<E>(pattern: &str) -> impl Fn(E) -> Self
    where E: Into<regex_syntax::Error> {
        move |source| Self::Pattern { pattern: pattern.to_owned(), source: Box::new(source.into()) }
    }

    fn document(transform: &serde_json::Value) -> impl Fn(serde_path_to_error::Error<serde_json::Error>) -> Self { move |source| Self::Document { transform: transform.clone(), source } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [STRUCTURE]

fn class_ranges(class: &hir::Class) -> Vec<(u32, u32)> {
    match class {
        hir::Class::Unicode(unicode) => unicode.ranges().iter().map(|range| (u32::from(range.start()), u32::from(range.end()))).collect(),
        hir::Class::Bytes(bytes) => bytes.ranges().iter().map(|range| (u32::from(range.start()), u32::from(range.end()))).collect(),
    }
}

fn literal_ranges(bytes: &[u8]) -> Vec<(u32, u32)> { String::from_utf8_lossy(bytes).chars().map(|letter| (u32::from(letter), u32::from(letter))).collect() }

fn token_structure(Token { pattern, caseless }: &Token) -> Result<Node, Failure> {
    const GENERATOR_CLASSES: [(&str, &str); 6] = [(r"\w", "[0-9A-Za-z_]"), (r"\s", r"[\t-\r ]"), (r"\d", "[0-9]"), (r"\W", "[^0-9A-Za-z_]"), (r"\S", r"[^\t-\r ]"), (r"\D", "[^0-9]")];
    fn structured(hir: &Hir) -> Node {
        match hir.kind() {
            HirKind::Empty | HirKind::Look(_) => Node::Alternation { alternatives: vec![Vec::new()] },
            HirKind::Literal(hir::Literal(bytes)) => Node::Alternation { alternatives: vec![literal_ranges(bytes).into_iter().map(|range| Node::CharacterClass { ranges: vec![range] }).collect()] },
            HirKind::Class(class) => Node::CharacterClass { ranges: class_ranges(class) },
            HirKind::Repetition(repetition) => Node::Quantifier { min: repetition.min, max: repetition.max, alternatives: vec![vec![structured(&repetition.sub)]] },
            HirKind::Capture(capture) => structured(&capture.sub),
            HirKind::Concat(parts) => Node::Alternation { alternatives: vec![parts.iter().map(structured).collect()] },
            HirKind::Alternation(parts) => Node::Alternation { alternatives: parts.iter().map(|part| vec![structured(part)]).collect() },
        }
    }
    let source = GENERATOR_CLASSES.iter().fold(pattern.clone(), |text, (written, spelled)| text.replace(written, spelled));
    let tree = Parser::new().parse(&source).map_err(Failure::pattern(pattern))?;
    Ok(structured(&TranslatorBuilder::new().utf8(false).case_insensitive(*caseless).build().translate(&source, &tree).map_err(Failure::pattern(pattern))?))
}

// --- [CLASSES]

impl Scope {
    fn applied(self, flags: &ast::Flags) -> Self { Self(core::array::from_fn(|at| flags.flag_state(SCOPED[at]).unwrap_or(self.0[at]))) }
}

struct Collector<'pattern> {
    pattern: &'pattern str,
    scope: Scope,
    enclosing: Vec<Scope>,
    classes: Vec<Class>,
}

impl Collector<'_> {
    fn collected(&mut self, class: &Ast, negated: bool) -> Result<(), hir::Error> {
        let Scope([caseless, unicode]) = self.scope;
        let hir = TranslatorBuilder::new().utf8(false).case_insensitive(caseless).unicode(unicode).build().translate(self.pattern, class)?;
        let ranges = match hir.kind() {
            HirKind::Class(held) => class_ranges(held),
            HirKind::Literal(hir::Literal(bytes)) => literal_ranges(bytes),
            _ => Vec::new(),
        };
        let span = class.span();
        self.classes.push(Class {
            source: self.pattern[span.start.offset..span.end.offset].to_owned(),
            reading: if matches!(class, Ast::ClassPerl(_)) && unicode { Reading::UnicodePerl } else { Reading::Plain },
            polarity: if negated { Polarity::Complement } else { Polarity::Stated },
            ranges,
        });
        Ok(())
    }
}

impl Visitor for Collector<'_> {
    type Err = hir::Error;
    type Output = Vec<Class>;

    fn finish(self) -> Result<Self::Output, Self::Err> { Ok(self.classes) }

    fn visit_pre(&mut self, node: &Ast) -> Result<(), Self::Err> {
        match node {
            Ast::Flags(flags) => {
                self.scope = self.scope.applied(&flags.flags);
                Ok(())
            }
            Ast::Group(group) => {
                self.enclosing.push(self.scope);
                self.scope = group.flags().map_or(self.scope, |flags| self.scope.applied(flags));
                Ok(())
            }
            Ast::ClassPerl(class) => self.collected(node, class.negated),
            Ast::ClassUnicode(class) => self.collected(node, class.is_negated()),
            Ast::ClassBracketed(class) => self.collected(node, class.negated),
            _ => Ok(()),
        }
    }

    fn visit_post(&mut self, node: &Ast) -> Result<(), Self::Err> {
        if matches!(node, Ast::Group(_)) {
            self.scope = self.enclosing.pop().unwrap_or(self.scope);
        }
        Ok(())
    }

    fn visit_class_set_item_pre(&mut self, item: &ClassSetItem) -> Result<(), Self::Err> {
        if let ClassSetItem::Perl(_) = item
            && let Scope([_, true]) = self.scope
            && let Some(class) = self.classes.last_mut()
        {
            class.reading = Reading::UnicodePerl;
        }
        Ok(())
    }
}

#[define_opaque(Unparsed)]
fn written_classes(written: &Written) -> Result<Vec<Class>, Failure> {
    let Some(pattern) = (match written {
        Written::Regex(pattern) => Some(pattern.clone()),
        Written::Transform { transform } => {
            let parsed = match serde_path_to_error::deserialize(transform).map_err(Failure::document(transform))? {
                Transformation::Simplied(text) => Transformation::Object(text.parse().map_err(|source| Failure::Text { transform: transform.clone(), source })?),
                object @ Transformation::Object(_) => object,
            };
            let serialized = serde_path_to_error::serialize(&parsed, serde_json::value::Serializer).map_err(Failure::document(transform))?;
            let Transformed { replace } = serde_path_to_error::deserialize(&serialized).map_err(Failure::document(transform))?;
            replace.map(|Replaced { replace }| replace)
        }
    }) else {
        return Ok(Vec::new());
    };
    let tree = Parser::new().parse(&pattern).map_err(Failure::pattern(&pattern))?;
    visit(&tree, Collector { pattern: &pattern, scope: Scope([false, true]), enclosing: Vec::new(), classes: Vec::new() }).map_err(Failure::pattern(&pattern))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), Report<Failure>> {
    let Request { tokens, regexes }: Request = serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_reader(io::stdin().lock())).map_err(Failure::Request)?;
    let reply = Reply { tokens: tokens.iter().map(token_structure).collect::<Result<_, _>>()?, classes: regexes.iter().map(written_classes).collect::<Result<_, _>>()? };
    serde_json::to_writer(io::stdout().lock(), &reply).map_err(Failure::Reply)?;
    Ok(())
}
