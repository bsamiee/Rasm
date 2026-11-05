#![feature(error_reporter, iterator_try_collect, type_alias_impl_trait)]

use core::time::Duration;
use std::{
    collections::{BTreeSet, HashMap},
    error::Report,
    io,
    path::Path,
};

use ast_grep_config::{DeserializeEnv, Rule, RuleSerializeError, SerializableRule, Transformation};
use ast_grep_core::{
    Language as _, Matcher as _,
    matcher::{Pattern, PatternNode},
    tree_sitter::LanguageExt as _,
};
use ast_grep_dynamic::{CustomLang, DynamicLang, DynamicLangError};
use regex_automata::meta;
use regex_syntax::{
    ast::{self, Ast, ClassSetItem, Flag, Visitor, parse::Parser, visit},
    hir::{self, Hir, HirKind, Look, LookSet, literal::Extractor, translate::TranslatorBuilder},
};
use serde::{Deserialize, Serialize};
use serde_json::Value;
use tokio::time::Instant;

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
    node_types: Vec<NodeType>,
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

#[derive(Serialize)]
#[serde(rename_all = "lowercase")]
enum Atom<'text> {
    Kind(&'text str),
    Regex(&'text str),
    Pattern(&'text str),
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
struct Literal {
    kind: String,
    size: usize,
    regex: u128,
    pattern: u128,
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
    #[error("cannot compile pattern {pattern}")]
    Program { pattern: String, source: Box<meta::BuildError> },
    #[error("cannot read {document}")]
    Document { document: Value, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("cannot parse transform {transform}")]
    Text { transform: Value, source: Unparsed },
    #[error("cannot load the custom languages")]
    Library(#[source] DynamicLangError),
    #[error("cannot encode a rule atom")]
    Atom(#[source] serde_json::Error),
    #[error("cannot compile rule {rule}")]
    Rule { rule: Value, source: Box<RuleSerializeError> },
    #[error("cannot write the reply")]
    Reply(#[source] serde_json::Error),
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
        Ok(DynamicLang::all_langs().into_iter().filter_map(|language| Some((language.name().to_owned(), Self::new(language, &types.remove(language.name())?)))).collect())
    }

    fn new(language: DynamicLang, types: &[NodeType]) -> Self {
        fn expanded<'held>(supertypes: &HashMap<&str, &'held [Kind]>, kind: &'held Kind) -> Vec<&'held Kind> {
            supertypes.get(kind.name.as_str()).map_or_else(|| vec![kind], |subtypes| subtypes.iter().flat_map(|subtype| expanded(supertypes, subtype)).collect())
        }
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
        Self { language, fields }
    }

    fn compiled(&self, rule: &Value, serialized: SerializableRule) -> Result<Rule, Failure> {
        DeserializeEnv::new(self.language).deserialize_rule(serialized).map_err(|source| Failure::Rule { rule: rule.clone(), source: Box::new(source) })
    }

    fn stated(&self, kind: &str) -> Result<Option<BTreeSet<u16>>, Failure> {
        let (rule, held) = serialized(&Atom::Kind(kind))?;
        Ok(self.compiled(&rule, held)?.potential_kinds().map(|kinds| kinds.iter().filter_map(|id| u16::try_from(id).ok()).collect()))
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

    fn leaf(&self, text: &str) -> Option<(u16, String)> {
        let recovered = self.language.ast_grep(self.language.pre_process_pattern(text)).root().get_inner_node().has_error();
        match Pattern::try_new(text, self.language).map(|pattern| pattern.node) {
            Ok(PatternNode::Terminal { text: parsed, is_named: true, kind_id }) if !recovered && parsed == text =>
                Some((kind_id, self.language.get_ts_language().node_kind_for_id(kind_id)?.to_owned())),
            Ok(_) | Err(_) => None,
        }
    }

    fn cost(&self, atom: &Atom<'_>) -> Result<Duration, Failure> {
        const SAMPLES: usize = 16;
        let (rule, held) = serialized(atom)?;
        (0..SAMPLES).try_fold(Duration::MAX, |least, _| {
            let copy = held.clone();
            let start = Instant::now();
            self.compiled(&rule, copy)?;
            Ok(least.min(start.elapsed()))
        })
    }

    fn literal(&self, regex: &str, subject: &Subject) -> Result<Option<Literal>, Failure> {
        let hir = regex_syntax::parse(regex).map_err(Failure::pattern(regex))?;
        let properties = hir.properties();
        let whole =
            properties.look_set() == LookSet::singleton(Look::Start).insert(Look::End) && properties.look_set_prefix().contains(Look::Start) && properties.look_set_suffix().contains(Look::End);
        let exact = match Extractor::new().extract(&hir).literals() {
            Some([literal]) if whole && literal.is_exact() => Some(String::from_utf8_lossy(literal.as_bytes()).into_owned()),
            Some(_) | None => None,
        };
        let Some(((id, kind), text)) = exact.and_then(|text| Some((self.leaf(&text)?, text))) else {
            return Ok(None);
        };
        (self.tested(subject)? == Some(BTreeSet::from([id])))
            .then(|| {
                Ok(Literal {
                    kind,
                    size: meta::Regex::new(regex).map_err(|source| Failure::Program { pattern: regex.to_owned(), source: Box::new(source) })?.memory_usage(),
                    regex: self.cost(&Atom::Regex(regex))?.as_nanos(),
                    pattern: self.cost(&Atom::Pattern(&text))?.as_nanos(),
                    text,
                })
            })
            .transpose()
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
        Written::Regex { regex, site: _ } => Some(regex.clone()),
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

// --- [RULES]

fn serialized(atom: &Atom<'_>) -> Result<(Value, SerializableRule), Failure> {
    let rule = serde_json::to_value(atom).map_err(Failure::Atom)?;
    let held = serde_path_to_error::deserialize(&rule).map_err(Failure::document(&rule))?;
    Ok((rule, held))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), Report<Failure>> {
    let Request { tokens, regexes, languages }: Request = serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_reader(io::stdin().lock())).map_err(Failure::Request)?;
    let tables = Table::registered(languages)?;
    let read = |written: &Written| {
        Ok::<_, Failure>(Read {
            classes: written_classes(written)?,
            literal: match written {
                Written::Regex { regex, site: Some(Site { language, subject }) } => tables.get(language).map(|table| table.literal(regex, subject)).transpose()?.flatten(),
                Written::Regex { regex: _, site: None } | Written::Transform { transform: _ } => None,
            },
        })
    };
    let reply = Reply { tokens: tokens.iter().map(token_structure).try_collect()?, regexes: regexes.iter().map(read).try_collect()? };
    serde_json::to_writer(io::stdout().lock(), &reply).map_err(Failure::Reply)?;
    Ok(())
}
