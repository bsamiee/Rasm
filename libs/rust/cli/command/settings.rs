use core::{borrow::Borrow, iter, mem, ops::Range};
use std::sync::Arc;

use annotate_snippets::{AnnotationKind, Element, Level, Renderer, Snippet};
use camino::Utf8PathBuf;
use clap::{Args, ValueHint};
use fs_err::tokio::read_to_string;
use futures::future;
use itertools::Itertools as _;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use serde::de::DeserializeOwned;
use serde_path_to_error::{Path, Segment};
use toml::{
    Spanned,
    de::{DeTable, DeValue, Deserializer},
};
use validated::Validated;

use crate::CommandError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args, Debug)]
#[group(skip)]
pub struct Layers {
    #[arg(long, global = true, env, value_name = "PATH", value_hint = ValueHint::FilePath)]
    #[arg(help = "Settings file layered over earlier ones and field defaults")]
    pub settings: Vec<Utf8PathBuf>,
}

#[derive(Debug, derive_more::Display)]
pub enum Phase {
    #[display("cannot parse settings")]
    Parse(Origin),
    #[display("cannot decode settings key `{path}`")]
    Decode { path: Path, origin: Option<Origin> },
}

#[derive(Debug)]
pub struct Origin {
    pub path: Utf8PathBuf,
    pub text: Arc<str>,
    pub span: Option<Range<usize>>,
}

impl Origin {
    #[must_use]
    pub fn excerpt(&self, title: &str) -> String {
        Renderer::plain().render(&[Level::ERROR.no_name().primary_title(title).element(match &self.span {
            Some(span) => Element::from(Snippet::source(&*self.text).path(self.path.as_str()).annotation(AnnotationKind::Primary.span(span.clone()))),
            None => Element::from(annotate_snippets::Origin::path(self.path.as_str())),
        })])
    }
}

#[derive(Debug)]
pub struct Provenance {
    layers: Vec<(Utf8PathBuf, Arc<str>)>,
}

impl Provenance {
    #[must_use]
    pub fn origin<P>(&self, path: P) -> Option<Origin>
    where P: IntoIterator<Item: Borrow<Segment>> + Copy {
        self.layers
            .iter()
            .map(|(file, text)| {
                let (table, _) = DeTable::parse_recoverable(text);
                let root = Spanned::new(table.span(), DeValue::Table(table.into_inner()));
                let held = path
                    .into_iter()
                    .scan(&root, |value, segment| {
                        let next = match segment.borrow() {
                            Segment::Map { key } | Segment::Enum { variant: key } => value.get_ref().get(key),
                            Segment::Seq { index } => value.get_ref().get(*index),
                            Segment::Unknown => None,
                        }?;
                        *value = next;
                        Some(next.span())
                    })
                    .enumerate()
                    .last();
                (held, file, text)
            })
            .max_by_key(|(held, _, _)| held.as_ref().map(|(depth, _)| *depth))
            .map(|(held, file, text)| Origin { path: Utf8PathBuf::clone(file), text: Arc::clone(text), span: held.map(|(_, span)| span) })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn decoded<S>(paths: impl IntoIterator<Item = Utf8PathBuf>) -> Result<(S, Provenance), NEVec<CommandError>>
where S: DeserializeOwned {
    fn merged<'text>(lower: DeTable<'text>, upper: DeTable<'text>) -> DeTable<'text> {
        upper.into_iter().fold(lower, |mut table, (key, entry)| {
            let span = entry.span();
            match (table.get_mut(&key).map(Spanned::get_mut), entry.into_inner()) {
                (Some(DeValue::Table(earlier)), DeValue::Table(later)) => *earlier = merged(mem::take(earlier), later),
                (_, later) => {
                    table.insert(key, Spanned::new(span, later));
                }
            }
            table
        })
    }
    let (layers, missing): (Vec<_>, Vec<_>) =
        future::join_all(paths.into_iter().map(async |path| read_to_string(&path).await.map(|text| (path, Arc::<str>::from(text))))).await.into_iter().partition_result();
    let provenance = Provenance { layers };
    let table = iter::chain(
        missing.into_iter().map(|source| Validated::fail(CommandError::File(source))),
        provenance.layers.iter().map(|(path, text)| {
            let (table, failures) = DeTable::parse_recoverable(text);
            match NEVec::try_from_vec(failures) {
                None => Validated::Good(table.into_inner()),
                Some(failures) => Validated::Fail(
                    failures
                        .into_nonempty_iter()
                        .map(|mut source| {
                            source.set_input(None);
                            CommandError::Settings { phase: Phase::Parse(Origin { path: path.clone(), text: Arc::clone(text), span: source.span() }), source: Box::new(source) }
                        })
                        .collect(),
                ),
            }
        }),
    )
    .collect::<Validated<Vec<_>, _>>()
    .ok()?
    .into_iter()
    .fold(DeTable::new(), merged);
    serde_path_to_error::deserialize(Deserializer::from(Spanned::new(0..0, table)))
        .map_err(|source| NEVec::new(CommandError::Settings { phase: Phase::Decode { origin: provenance.origin(source.path()), path: source.path().clone() }, source: Box::new(source.into_inner()) }))
        .map(|settings| (settings, provenance))
}
