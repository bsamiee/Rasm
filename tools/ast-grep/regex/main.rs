#![feature(error_reporter, iterator_try_collect, type_alias_impl_trait)]

use core::iter::successors;
use std::{
    collections::{BTreeSet, HashMap},
    error::Report,
    fs, io,
    path::{Path, PathBuf},
};

use ast_grep_config::{SelectorError, Transformation, parse_selector};
use ast_grep_core::{AstGrep, Language as _, Matcher as _, PatternError, matcher::PatternNode, tree_sitter::LanguageExt as _};
use ast_grep_dynamic::{CustomLang, DynamicLang, DynamicLangError};
use regex_syntax::{
    ast::{self, Ast, ClassSetItem, Flag, Visitor, parse::Parser, visit},
    hir::{
        self, Hir, HirKind, Look, LookSet,
        literal::Extractor,
        translate::{Translator, TranslatorBuilder},
    },
};
use serde::{Deserialize, Serialize};
use serde_json::Value;

// --- [TYPES] ---------------------------------------------------------------------------

type Unparsed = impl core::error::Error + Send + Sync + 'static;

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
struct Kind {
    #[serde(rename = "type")]
    name: String,
    named: bool,
}

#[derive(Deserialize)]
struct Field {
    types: Vec<Kind>,
}

#[derive(Deserialize)]
struct NodeType {
    #[serde(flatten)]
    kind: Kind,
    #[serde(default)]
    fields: HashMap<String, Field>,
    #[serde(default)]
    subtypes: Vec<Kind>,
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct Grammar {
    #[serde(flatten)]
    library: CustomLang,
    node_types: PathBuf,
}

#[derive(Deserialize)]
struct Placement {
    field: String,
    subject: Subject,
}

#[derive(Deserialize)]
struct Subject {
    kinds: Vec<String>,
    parent: Option<Box<Placement>>,
}

#[derive(Deserialize)]
struct Site {
    language: String,
    subject: Subject,
}

#[derive(Deserialize)]
#[serde(untagged)]
enum Written {
    Regex { regex: String, site: Option<Site> },
    Transform { transform: Value },
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
    languages: HashMap<String, Grammar>,
}

#[derive(Serialize)]
#[serde(tag = "_tag")]
enum Class {
    Characters { source: String, polarity: Polarity, ranges: Vec<(u32, u32)> },
    UnicodePerl { source: String },
    UnicodeBoundary { source: String },
}

#[derive(Serialize)]
#[serde(tag = "type")]
enum Node {
    CharacterClass { ranges: Vec<(u32, u32)> },
    Alternation { alternatives: Vec<Vec<Self>> },
    Quantifier { min: u32, max: Option<u32>, alternatives: Vec<Vec<Self>> },
}

#[derive(Serialize)]
struct Literal {
    kind: String,
    text: String,
}

#[derive(Serialize)]
struct Read {
    classes: Vec<Class>,
    literal: Option<Literal>,
}

#[derive(Serialize)]
struct Reply {
    tokens: Vec<Node>,
    regexes: Vec<Read>,
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
    #[error("cannot read {document}")]
    Document { document: Value, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("cannot parse transform {transform}")]
    Text { transform: Value, source: Unparsed },
    #[error("cannot load the custom languages")]
    Library(#[source] DynamicLangError),
    #[error("cannot read node types at {path}")]
    ReadNodeTypes { path: PathBuf, source: io::Error },
    #[error("cannot decode node types at {path}")]
    DecodeNodeTypes { path: PathBuf, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("cannot encode JSON")]
    Json(#[from] serde_json::Error),
    #[error("cannot compile kind selector {selector}")]
    Selector { selector: String, source: Box<SelectorError> },
    #[error("cannot inspect the literal as a pattern")]
    LiteralPattern(#[from] PatternError),
}

impl Failure {
    fn pattern<E>(pattern: &str) -> impl Fn(E) -> Self
    where E: Into<regex_syntax::Error> {
        move |source| Self::Pattern { pattern: pattern.to_owned(), source: Box::new(source.into()) }
    }

    fn document(document: &Value) -> impl Fn(serde_path_to_error::Error<serde_json::Error>) -> Self { move |source| Self::Document { document: document.clone(), source } }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Table {
    language: DynamicLang,
    fields: HashMap<String, HashMap<String, BTreeSet<u16>>>,
}

impl Table {
    fn registered(languages: HashMap<String, Grammar>) -> Result<HashMap<String, Self>, Failure> {
        let (libraries, mut types): (HashMap<_, _>, HashMap<_, _>) = languages.into_iter().map(|(name, Grammar { library, node_types })| ((name.clone(), library), (name, node_types))).unzip();
        CustomLang::register(Path::new(""), libraries).map_err(Failure::Library)?;
        DynamicLang::all_langs()
            .into_iter()
            .filter_map(|language| types.remove(language.name()).map(|path| (language, path)))
            .map(|(language, path)| Self::load(language, path).map(|table| (language.name().to_owned(), table)))
            .try_collect()
    }

    fn load(language: DynamicLang, path: PathBuf) -> Result<Self, Failure> {
        fn expanded<'held>(supertypes: &HashMap<&str, &'held [Kind]>, kind: &'held Kind) -> Vec<&'held Kind> {
            supertypes.get(kind.name.as_str()).map_or_else(|| vec![kind], |subtypes| subtypes.iter().flat_map(|subtype| expanded(supertypes, subtype)).collect())
        }
        #[expect(clippy::disallowed_methods, reason = "The error retains this path and workspace fs-err would add Tokio to the checker.")]
        let source = fs::read(&path).map_err(|source| Failure::ReadNodeTypes { path: path.clone(), source })?;
        let mut document = serde_json::Deserializer::from_slice(&source);
        let mut track = serde_path_to_error::Track::new();
        let types = Vec::<NodeType>::deserialize(serde_path_to_error::Deserializer::new(&mut document, &mut track))
            .and_then(|types| document.end().map(|()| types))
            .map_err(|source| Failure::DecodeNodeTypes { path, source: serde_path_to_error::Error::new(track.path(), source) })?;
        let grammar = language.get_ts_language();
        let supertypes: HashMap<&str, &[Kind]> = types.iter().map(|held| (held.kind.name.as_str(), held.subtypes.as_slice())).filter(|(_, subtypes)| !subtypes.is_empty()).collect();
        let fields = types
            .iter()
            .map(|held| {
                (
                    held.kind.name.clone(),
                    held.fields
                        .iter()
                        .map(|(field, Field { types })| {
                            (field.clone(), types.iter().flat_map(|kind| expanded(&supertypes, kind)).map(|Kind { name, named }| grammar.id_for_node_kind(name, *named)).collect())
                        })
                        .collect(),
                )
            })
            .collect();
        Ok(Self { language, fields })
    }

    fn stated(&self, kind: &str) -> Result<Option<BTreeSet<u16>>, Failure> {
        let selector = parse_selector(kind, self.language).map_err(|source| Failure::Selector { selector: kind.to_owned(), source: Box::new(source) })?;
        Ok(selector.potential_kinds().map(|kinds| kinds.iter().filter_map(|id| u16::try_from(id).ok()).collect()))
    }

    fn tested(&self, Subject { kinds, parent }: &Subject) -> Result<Option<BTreeSet<u16>>, Failure> {
        let grammar = self.language.get_ts_language();
        let placed = parent
            .as_deref()
            .map(|Placement { field, subject }| {
                Ok::<Option<BTreeSet<u16>>, Failure>(
                    self.tested(subject)?.map(|parents| parents.iter().filter_map(|&parent| self.fields.get(grammar.node_kind_for_id(parent)?)?.get(field)).flatten().copied().collect()),
                )
            })
            .transpose()?
            .flatten();
        kinds.iter().try_fold(placed, |held, kind| {
            Ok(match (held, self.stated(kind)?) {
                (Some(held), Some(stated)) => Some(held.intersection(&stated).copied().collect()),
                (held, stated) => held.or(stated),
            })
        })
    }

    fn leaf(&self, text: &str) -> Result<Option<(u16, String)>, Failure> {
        let parsed = AstGrep::try_new(&self.language.pre_process_pattern(text), self.language).map_err(PatternError::Parse)?;
        let root = parsed.root();
        if root.get_inner_node().has_error() {
            return Ok(None);
        }
        let goal = successors(Some(root), |node| match node.children().len() {
            1 => node.child(0),
            2 if node.child(1).is_some_and(|last| last.kind().is_empty()) => node.child(0),
            _ => None,
        })
        .skip(1)
        .last()
        .filter(ast_grep_core::Node::is_leaf);
        Ok(goal.and_then(|node| match PatternNode::from(node) {
            PatternNode::Terminal { text: parsed, is_named: true, kind_id } if parsed == text => self.language.get_ts_language().node_kind_for_id(kind_id).map(|kind| (kind_id, kind.to_owned())),
            _ => None,
        }))
    }

    fn literal(&self, regex: &str, tree: &Ast, subject: &Subject) -> Result<Option<Literal>, Failure> {
        let hir = Translator::new().translate(regex, tree).map_err(Failure::pattern(regex))?;
        let properties = hir.properties();
        let whole =
            properties.look_set() == LookSet::singleton(Look::Start).insert(Look::End) && properties.look_set_prefix().contains(Look::Start) && properties.look_set_suffix().contains(Look::End);
        let exact = Extractor::new().extract(&hir);
        let Some([literal]) = exact.literals().filter(|_| whole) else {
            return Ok(None);
        };
        let Some(text) = literal.is_exact().then(|| String::from_utf8_lossy(literal.as_bytes()).into_owned()) else { return Ok(None) };
        let Some((id, kind)) = self.leaf(&text)? else {
            return Ok(None);
        };
        Ok((self.tested(subject)? == Some(BTreeSet::from([id]))).then_some(Literal { kind, text }))
    }
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
        let span = class.span();
        let source = || self.pattern[span.start.offset..span.end.offset].to_owned();
        if matches!(class, Ast::ClassPerl(_)) && unicode {
            self.classes.push(Class::UnicodePerl { source: source() });
            return Ok(());
        }
        let hir = TranslatorBuilder::new().utf8(false).case_insensitive(caseless).unicode(unicode).build().translate(self.pattern, class)?;
        let ranges = match hir.kind() {
            HirKind::Class(held) => class_ranges(held),
            HirKind::Literal(hir::Literal(bytes)) => literal_ranges(bytes),
            HirKind::Look(look) => {
                if LookSet::singleton(*look).contains_word_unicode() {
                    self.classes.push(Class::UnicodeBoundary { source: source() });
                }
                return Ok(());
            }
            _ => Vec::new(),
        };
        self.classes.push(Class::Characters { source: source(), polarity: if negated { Polarity::Complement } else { Polarity::Stated }, ranges });
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
            Ast::Assertion(_) if self.scope.0[1] => self.collected(node, false),
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
            && let Class::Characters { source, polarity: _, ranges: _ } = class
        {
            *class = Class::UnicodePerl { source: core::mem::take(source) };
        }
        Ok(())
    }
}

// --- [RULES]

#[define_opaque(Unparsed)]
fn read(written: Written, tables: &HashMap<String, Table>) -> Result<Read, Failure> {
    let (pattern, site) = match written {
        Written::Regex { regex, site } => (regex, site),
        Written::Transform { transform } => {
            let parsed = match serde_path_to_error::deserialize(&transform).map_err(Failure::document(&transform))? {
                Transformation::Simplied(text) => Transformation::Object(text.parse().map_err(|source| Failure::Text { transform: transform.clone(), source })?),
                object @ Transformation::Object(_) => object,
            };
            let serialized = serde_path_to_error::serialize(&parsed, serde_json::value::Serializer).map_err(Failure::document(&transform))?;
            let Transformed { replace: Some(Replaced { replace }) } = serde_path_to_error::deserialize(&serialized).map_err(Failure::document(&transform))? else {
                return Ok(Read { classes: Vec::new(), literal: None });
            };
            (replace, None)
        }
    };
    let tree = Parser::new().parse(&pattern).map_err(Failure::pattern(&pattern))?;
    let classes = visit(&tree, Collector { pattern: &pattern, scope: Scope([false, true]), enclosing: Vec::new(), classes: Vec::new() }).map_err(Failure::pattern(&pattern))?;
    let literal = site.and_then(|Site { language, subject }| tables.get(&language).map(|table| table.literal(&pattern, &tree, &subject))).transpose()?.flatten();
    Ok(Read { classes, literal })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), Report<Failure>> {
    let Request { tokens, regexes, languages }: Request = serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_reader(io::stdin().lock())).map_err(Failure::Request)?;
    let tables = Table::registered(languages)?;
    let reply = Reply { tokens: tokens.iter().map(token_structure).try_collect()?, regexes: regexes.into_iter().map(|written| read(written, &tables)).try_collect()? };
    serde_json::to_writer(io::stdout().lock(), &reply).map_err(Failure::from)?;
    Ok(())
}
