#![feature(deref_patterns, error_reporter, iterator_try_collect, type_alias_impl_trait)]

use core::{error::Error, iter, mem, num::NonZero};
use std::{
    collections::{BTreeMap, BTreeSet, HashMap},
    error::Report,
    io,
    path::{Path, PathBuf},
    sync::LazyLock,
};

use ast_grep_config::{SelectorError, Transformation, parse_selector};
use ast_grep_core::{
    Matcher as _, Pattern, PatternError,
    matcher::PatternNode,
    tree_sitter::{LanguageExt as _, TSLanguage},
};
use ast_grep_dynamic::{CustomLang, DynamicLang, DynamicLangError};
use nonempty_collections::{NEVec, nev};
use regex_syntax::{
    ast::{
        self, Ast, ClassAscii, ClassAsciiKind, ClassBracketed, ClassPerl, ClassPerlKind, ClassSet, ClassSetItem, ClassSetRange, ClassSetUnion, Flag, FlagsItem, FlagsItemKind, Group, GroupKind,
        Visitor, parse::Parser, visit,
    },
    hir::{
        self, ClassUnicode, ClassUnicodeRange, Hir, HirKind, Look, LookSet,
        literal::Extractor,
        translate::{Translator, TranslatorBuilder},
    },
};
use serde::{Deserialize, Serialize};
use serde_json::{
    Value,
    de::{IoRead, SliceRead},
};

// --- [TYPES] ---------------------------------------------------------------------------

type Unparsed = impl Error + Send + Sync + 'static;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Deserialize)]
struct Kind {
    #[serde(rename = "type")]
    name: String,
    named: bool,
}

#[derive(Deserialize)]
struct NodeType {
    #[serde(flatten)]
    kind: Kind,
    #[serde(default)]
    fields: HashMap<String, Field>,
}

#[derive(Deserialize)]
struct Field {
    types: Vec<Kind>,
}

#[derive(Deserialize)]
struct Subject {
    kinds: Vec<String>,
    parent: Option<Box<(String, Self)>>,
}

#[derive(Deserialize)]
#[serde(untagged)]
enum Written {
    Regex { regex: String, site: Option<(String, Subject)> },
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
#[serde(rename_all = "camelCase", deny_unknown_fields)]
struct Request {
    tokens: Vec<(String, bool)>,
    regexes: Vec<Written>,
    languages: HashMap<String, CustomLang>,
    node_types: HashMap<String, PathBuf>,
}

#[derive(Serialize)]
#[serde(tag = "_tag")]
enum Finding {
    Characters { source: String, negated: bool, ranges: Vec<(u32, u32)> },
    UnicodePerl { source: String },
    UnicodeBoundary { source: String },
    Literal { kind: String, text: String },
}

#[derive(Serialize)]
#[serde(tag = "type")]
enum Node {
    CharacterClass { ranges: Vec<(u32, u32)> },
    Alternation { alternatives: Vec<Vec<Self>> },
    Quantifier { min: u32, max: Option<u32>, alternatives: Vec<Vec<Self>> },
}

struct Walker<'pattern> {
    pattern: &'pattern str,
    scopes: NEVec<TranslatorBuilder>,
    findings: Vec<Finding>,
}

impl Walker<'_> {
    fn translated(&self, class: &Ast) -> Result<Hir, hir::Error> { self.scopes.last().build().translate(self.pattern, class) }

    // --- [CLASSES]

    fn collected(&mut self, class: &Ast, negated: bool) -> Result<(), hir::Error> {
        let hir = self.translated(class)?;
        let source = self.pattern[class.span().start.offset..class.span().end.offset].to_owned();
        self.findings.extend(match (class, hir.kind()) {
            (Ast::ClassPerl(_), HirKind::Class(hir::Class::Unicode(_))) => Some(Finding::UnicodePerl { source }),
            (_, HirKind::Look(look)) => LookSet::singleton(*look).contains_word_unicode().then_some(Finding::UnicodeBoundary { source }),
            (_, HirKind::Class(held)) => Some(Finding::Characters { source, negated, ranges: ranges(held) }),
            (_, HirKind::Literal(hir::Literal(bytes))) => Some(Finding::Characters { source, negated, ranges: letters(bytes).map(|letter| (u32::from(letter), u32::from(letter))).collect() }),
            (_, HirKind::Empty | HirKind::Repetition(_) | HirKind::Capture(_) | HirKind::Concat(_) | HirKind::Alternation(_)) => None,
        });
        Ok(())
    }

    // --- [FOLDING]

    fn folded(&mut self, node: &mut Ast) -> Result<(), hir::Error> {
        let wrapped = |node: &mut Ast| {
            let span = *node.span();
            let flags = ast::Flags { span, items: [FlagsItemKind::Negation, FlagsItemKind::Flag(Flag::CaseInsensitive)].map(|kind| FlagsItem { span, kind }).to_vec() };
            *node = Ast::group(Group { span, kind: GroupKind::NonCapturing(flags), ast: Box::new(mem::replace(node, Ast::empty(span))) });
        };
        match node {
            Ast::Flags(flags) => *self.scopes.last_mut() = scoped(self.scopes.last(), Some(&flags.flags)),
            Ast::Group(group) => {
                self.scopes.push(scoped(self.scopes.last(), group.flags()));
                self.folded(&mut group.ast)?;
                self.scopes.pop();
            }
            Ast::Repetition(repetition) => self.folded(&mut repetition.ast)?,
            Ast::Alternation(ast::Alternation { span: _, asts }) | Ast::Concat(ast::Concat { span: _, asts }) => asts.iter_mut().try_for_each(|part| self.folded(part))?,
            Ast::Empty(_) | Ast::Dot(_) | Ast::Assertion(_) => {}
            Ast::ClassPerl(perl) => {
                *node = Ast::class_bracketed(ascii(perl));
                self.folded(node)?;
            }
            Ast::Literal(_) | Ast::ClassUnicode(_) => {
                let negated = matches!(node, Ast::ClassUnicode(class) if class.is_negated());
                if let Some(class) = self.refolded(node, negated)? {
                    *node = Ast::class_bracketed(class);
                    wrapped(node);
                }
            }
            Ast::ClassBracketed(class) =>
                if self.folded_set(&mut class.kind)? {
                    wrapped(node);
                },
        }
        Ok(())
    }

    fn folded_set(&self, set: &mut ClassSet) -> Result<bool, hir::Error> {
        match set {
            ClassSet::Item(item) => self.folded_item(item),
            ClassSet::BinaryOp(operation) => Ok(self.folded_set(&mut operation.lhs)? | self.folded_set(&mut operation.rhs)?),
        }
    }

    fn folded_item(&self, item: &mut ClassSetItem) -> Result<bool, hir::Error> {
        let negated = match item {
            ClassSetItem::Bracketed(class) => return self.folded_set(&mut class.kind),
            ClassSetItem::Union(union) => return union.items.iter_mut().try_fold(false, |folds, item| Ok(folds | self.folded_item(item)?)),
            ClassSetItem::Empty(_) => return Ok(false),
            ClassSetItem::Literal(_) | ClassSetItem::Range(_) => false,
            ClassSetItem::Ascii(class) => class.negated,
            ClassSetItem::Perl(perl) => {
                *item = ClassSetItem::Bracketed(Box::new(ascii(perl)));
                return self.folded_item(item);
            }
            ClassSetItem::Unicode(class) => class.is_negated(),
        };
        let written = Ast::class_bracketed(ClassBracketed { span: *item.span(), negated: false, kind: ClassSet::Item(item.clone()) });
        let Some(class) = self.refolded(&written, negated)? else { return Ok(false) };
        *item = ClassSetItem::Bracketed(Box::new(class));
        Ok(true)
    }

    fn refolded(&self, written: &Ast, negated: bool) -> Result<Option<ClassBracketed>, hir::Error> {
        static EXOTIC: LazyLock<ClassUnicode> = LazyLock::new(|| {
            let mut folded = ClassUnicode::new([ClassUnicodeRange::new('A', 'z')]);
            folded.case_fold_simple();
            folded.difference(&ClassUnicode::new([ClassUnicodeRange::new('\0', '\x7F')]));
            folded
        });
        let scoped = self.translated(written)?;
        let HirKind::Class(hir::Class::Unicode(_)) = scoped.kind() else { return Ok(None) };
        let plain = TranslatorBuilder::new().utf8(false).build().translate(self.pattern, written)?;
        if scoped == plain {
            return Ok(None);
        }
        let mut operand = match plain.into_kind() {
            HirKind::Class(hir::Class::Unicode(class)) => class,
            HirKind::Literal(hir::Literal(bytes)) => ClassUnicode::new(letters(&bytes).map(|letter| ClassUnicodeRange::new(letter, letter))),
            HirKind::Empty | HirKind::Class(hir::Class::Bytes(_)) | HirKind::Look(_) | HirKind::Repetition(_) | HirKind::Capture(_) | HirKind::Concat(_) | HirKind::Alternation(_) =>
                ClassUnicode::empty(),
        };
        if negated {
            operand.negate();
        }
        let mut asked = EXOTIC.clone();
        asked.intersect(&operand);
        operand.difference(&EXOTIC);
        operand.case_fold_simple();
        operand.difference(&EXOTIC);
        operand.union(&asked);
        let span = *written.span();
        let literal = |letter| ast::Literal { span, kind: ast::LiteralKind::Verbatim, c: letter };
        let items = operand.ranges().iter().map(|range| ClassSetItem::Range(ClassSetRange { span, start: literal(range.start()), end: literal(range.end()) })).collect();
        Ok(Some(ClassBracketed { span, negated, kind: ClassSet::Item(ClassSetUnion { span, items }.into_item()) }))
    }
}

impl Visitor for Walker<'_> {
    type Err = hir::Error;
    type Output = Vec<Finding>;

    fn finish(self) -> Result<Self::Output, Self::Err> { Ok(self.findings) }

    fn visit_pre(&mut self, node: &Ast) -> Result<(), Self::Err> {
        match node {
            Ast::Flags(flags) => *self.scopes.last_mut() = scoped(self.scopes.last(), Some(&flags.flags)),
            Ast::Group(group) => self.scopes.push(scoped(self.scopes.last(), group.flags())),
            Ast::ClassPerl(class) => self.collected(node, class.negated)?,
            Ast::ClassUnicode(class) => self.collected(node, class.is_negated())?,
            Ast::ClassBracketed(class) => self.collected(node, class.negated)?,
            Ast::Assertion(_) => self.collected(node, false)?,
            Ast::Empty(_) | Ast::Literal(_) | Ast::Dot(_) | Ast::Repetition(_) | Ast::Alternation(_) | Ast::Concat(_) => {}
        }
        Ok(())
    }

    fn visit_post(&mut self, node: &Ast) -> Result<(), Self::Err> {
        if let Ast::Group(_) = node {
            self.scopes.pop();
        }
        Ok(())
    }

    fn visit_class_set_item_pre(&mut self, item: &ClassSetItem) -> Result<(), Self::Err> {
        if let ClassSetItem::Perl(perl) = item
            && let HirKind::Class(hir::Class::Unicode(_)) = self.translated(&Ast::ClassPerl(Box::new(perl.clone())))?.kind()
            && let Some(finding) = self.findings.last_mut()
            && let Finding::Characters { source, negated: _, ranges: _ } = finding
        {
            *finding = Finding::UnicodePerl { source: mem::take(source) };
        }
        Ok(())
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("cannot decode the request")]
    Request(#[source] serde_path_to_error::Error<serde_json::Error>),
    #[error("cannot read a pattern")]
    Pattern(#[source] Box<regex_syntax::Error>),
    #[error("cannot decode transform {transform}")]
    Transform { transform: Value, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("cannot parse transform {transform}")]
    Text { transform: Value, source: Unparsed },
    #[error("cannot load the custom languages")]
    Library(#[source] DynamicLangError),
    #[error("cannot find language {language}")]
    Language { language: String },
    #[error("cannot find field {field}")]
    Field { field: String },
    #[error("cannot read node types")]
    ReadNodeTypes(#[from] io::Error),
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
    fn pattern<E>(source: E) -> Self
    where E: Into<regex_syntax::Error> {
        Self::Pattern(Box::new(source.into()))
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Table {
    language: DynamicLang,
    fields: BTreeMap<(NonZero<u16>, u16), Vec<u16>>,
}

impl Table {
    fn registered(languages: HashMap<String, CustomLang>, node_types: HashMap<String, PathBuf>) -> Result<HashMap<String, Self>, Failure> {
        CustomLang::register(Path::new(""), languages).map_err(Failure::Library)?;
        node_types
            .into_iter()
            .map(|(name, path)| {
                let language = name.parse().map_err(|_: String| Failure::Language { language: name.clone() })?;
                Ok((name, Self::load(language, path)?))
            })
            .try_collect()
    }

    fn load(language: DynamicLang, path: PathBuf) -> Result<Self, Failure> {
        fn concrete(grammar: &TSLanguage, kind: u16) -> Vec<u16> {
            if grammar.node_kind_is_supertype(kind) { grammar.subtypes_for_supertype(kind).iter().flat_map(|&subtype| concrete(grammar, subtype)).collect() } else { vec![kind] }
        }
        let source = fs_err::read(&path)?;
        let types: Vec<NodeType> = decoded(SliceRead::new(&source)).map_err(|source| Failure::DecodeNodeTypes { path, source })?;
        let grammar = &language.get_ts_language();
        let fields = types
            .iter()
            .flat_map(|NodeType { kind: Kind { name, named }, fields }| fields.iter().map(move |(field, Field { types })| (field, grammar.id_for_node_kind(name, *named), types)))
            .map(|(field, owner, types)| {
                let field = grammar.field_id_for_name(field).ok_or_else(|| Failure::Field { field: field.clone() })?;
                Ok::<_, Failure>(((field, owner), types.iter().flat_map(|Kind { name, named }| concrete(grammar, grammar.id_for_node_kind(name, *named))).collect()))
            })
            .try_collect()?;
        Ok(Self { language, fields })
    }

    fn tested(&self, Subject { kinds, parent }: &Subject) -> Result<Option<BTreeSet<u16>>, Failure> {
        let placed = match parent {
            Some((name, subject)) => {
                let field = self.language.get_ts_language().field_id_for_name(name).ok_or_else(|| Failure::Field { field: name.clone() })?;
                Some(match self.tested(subject)? {
                    Some(parents) => parents.iter().filter_map(|&parent| self.fields.get(&(field, parent))).flatten().copied().collect(),
                    None => self.fields.range((field, u16::MIN)..=(field, u16::MAX)).flat_map(|(_, kinds)| kinds).copied().collect(),
                })
            }
            None => None,
        };
        kinds.iter().try_fold(placed, |held, kind| {
            let selector = parse_selector(kind, self.language).map_err(|source| Failure::Selector { selector: kind.clone(), source: Box::new(source) })?;
            let stated = selector.potential_kinds().map(|kinds| kinds.iter().filter_map(|id| u16::try_from(id).ok()).collect());
            Ok(match (held, stated) {
                (Some(held), Some(stated)) => Some(&held & &stated),
                (held, stated) => held.or(stated),
            })
        })
    }

    fn literal(&self, hir: &Hir, subject: &Subject) -> Result<Option<Finding>, Failure> {
        let properties = hir.properties();
        let whole =
            properties.look_set() == LookSet::singleton(Look::Start).insert(Look::End) && properties.look_set_prefix().contains(Look::Start) && properties.look_set_suffix().contains(Look::End);
        let exact = Extractor::new().extract(hir);
        let Some([literal]) = exact.literals().filter(|_| whole) else { return Ok(None) };
        let Some(text) = literal.is_exact().then(|| String::from_utf8_lossy(literal.as_bytes()).into_owned()) else { return Ok(None) };
        let kind = match Pattern::try_new(&text, self.language) {
            Ok(Pattern { node: PatternNode::Terminal { text: parsed, is_named: true, kind_id }, .. }) if parsed == text => kind_id,
            Ok(Pattern { .. }) | Err(PatternError::NoContent(_) | PatternError::MultipleNode(_)) => return Ok(None),
            Err(source) => return Err(Failure::LiteralPattern(source)),
        };
        let grammar = self.language.get_ts_language();
        Ok(self.tested(subject)?.filter(|kinds| *kinds == BTreeSet::from([kind])).and_then(|_| grammar.node_kind_for_id(kind)).map(|name| Finding::Literal { kind: name.to_owned(), text }))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODE]

fn decoded<'de, T, R>(read: R) -> Result<T, serde_path_to_error::Error<serde_json::Error>>
where
    T: Deserialize<'de>,
    R: serde_json::de::Read<'de>,
{
    let mut document = serde_json::Deserializer::new(read);
    let mut track = serde_path_to_error::Track::new();
    T::deserialize(serde_path_to_error::Deserializer::new(&mut document, &mut track))
        .and_then(|value| document.end().map(|()| value))
        .map_err(|source| serde_path_to_error::Error::new(track.path(), source))
}

// --- [SCOPES]

fn scoped(scope: &TranslatorBuilder, flags: Option<&ast::Flags>) -> TranslatorBuilder {
    let mut scope = scope.clone();
    [(Flag::CaseInsensitive, TranslatorBuilder::case_insensitive as fn(&mut TranslatorBuilder, bool) -> &mut TranslatorBuilder), (Flag::Unicode, TranslatorBuilder::unicode)]
        .into_iter()
        .filter_map(|(flag, set)| flags?.flag_state(flag).map(|state| (set, state)))
        .fold(&mut scope, |scope, (set, state)| set(scope, state));
    scope
}

// --- [RANGES]

fn ranges(class: &hir::Class) -> Vec<(u32, u32)> {
    match class {
        hir::Class::Unicode(unicode) => unicode.ranges().iter().map(|range| (u32::from(range.start()), u32::from(range.end()))).collect(),
        hir::Class::Bytes(bytes) => bytes.ranges().iter().map(|range| (u32::from(range.start()), u32::from(range.end()))).collect(),
    }
}

fn letters(bytes: &[u8]) -> impl Iterator<Item = char> { bytes.utf8_chunks().flat_map(|chunk| chunk.valid().chars()) }

// --- [TOKENS]

const fn ascii(perl: &ClassPerl) -> ClassBracketed {
    let kind = match perl.kind {
        ClassPerlKind::Digit => ClassAsciiKind::Digit,
        ClassPerlKind::Space => ClassAsciiKind::Space,
        ClassPerlKind::Word => ClassAsciiKind::Word,
    };
    ClassBracketed { span: perl.span, negated: perl.negated, kind: ClassSet::Item(ClassSetItem::Ascii(ClassAscii { span: perl.span, kind, negated: false })) }
}

fn structure((pattern, caseless): &(String, bool)) -> Result<Node, Failure> {
    fn structured(hir: &Hir) -> Node {
        match hir.kind() {
            HirKind::Empty | HirKind::Look(_) => Node::Alternation { alternatives: vec![Vec::new()] },
            HirKind::Literal(hir::Literal(bytes)) =>
                Node::Alternation { alternatives: vec![letters(bytes).map(|letter| Node::CharacterClass { ranges: vec![(u32::from(letter), u32::from(letter))] }).collect()] },
            HirKind::Class(class) => Node::CharacterClass { ranges: ranges(class) },
            HirKind::Repetition(repetition) => Node::Quantifier { min: repetition.min, max: repetition.max, alternatives: vec![vec![structured(&repetition.sub)]] },
            HirKind::Capture(capture) => structured(&capture.sub),
            HirKind::Concat(parts) => Node::Alternation { alternatives: vec![parts.iter().map(structured).collect()] },
            HirKind::Alternation(parts) => Node::Alternation { alternatives: parts.iter().map(|part| vec![structured(part)]).collect() },
        }
    }
    let mut tree = Parser::new().parse(pattern).map_err(Failure::pattern)?;
    let base = TranslatorBuilder::new().utf8(false).case_insensitive(*caseless).clone();
    Walker { pattern, scopes: nev![base.clone()], findings: Vec::new() }.folded(&mut tree).map_err(Failure::pattern)?;
    base.build().translate(pattern, &tree).map(|hir| structured(&hir)).map_err(Failure::pattern)
}

// --- [RULES]

#[define_opaque(Unparsed)]
fn read(written: Written, tables: &HashMap<String, Table>) -> Result<Vec<Finding>, Failure> {
    let (pattern, site) = match written {
        Written::Regex { regex, site } => (regex, site),
        Written::Transform { transform } => {
            let decode = |source| Failure::Transform { transform: transform.clone(), source };
            let parsed = match serde_path_to_error::deserialize(&transform).map_err(decode)? {
                Transformation::Simplied(text) => Transformation::Object(text.parse().map_err(|source| Failure::Text { transform: transform.clone(), source })?),
                object @ Transformation::Object(_) => object,
            };
            let Transformed { replace: Some(Replaced { replace }) } = serde_path_to_error::deserialize(serde_json::to_value(parsed)?).map_err(decode)? else { return Ok(Vec::new()) };
            (replace, None)
        }
    };
    let tree = Parser::new().parse(&pattern).map_err(Failure::pattern)?;
    let hir = Translator::new().translate(&pattern, &tree).map_err(Failure::pattern)?;
    let classes = visit(&tree, Walker { pattern: &pattern, scopes: nev![TranslatorBuilder::new()], findings: Vec::new() }).map_err(Failure::pattern)?;
    let literal = site.and_then(|(language, subject)| tables.get(&language).map(|table| table.literal(&hir, &subject))).transpose()?.flatten();
    Ok(iter::chain(classes, literal).collect())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), Report<Failure>> {
    let Request { tokens, regexes, languages, node_types } = decoded(IoRead::new(io::stdin().lock())).map_err(Failure::Request)?;
    let tables = Table::registered(languages, node_types)?;
    let reply = (tokens.iter().map(structure).try_collect::<Vec<_>>()?, regexes.into_iter().map(|written| read(written, &tables)).try_collect::<Vec<_>>()?);
    serde_json::to_writer(io::stdout().lock(), &reply).map_err(Failure::from)?;
    Ok(())
}
