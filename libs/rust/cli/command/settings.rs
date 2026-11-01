use core::{iter, ops::Range};
use std::sync::Arc;

use annotate_snippets::{AnnotationKind, Level, Renderer, Snippet};
use camino::{Utf8Path, Utf8PathBuf};
use clap::{Args, ValueHint};
use fs_err::tokio::read_to_string;
use futures::future;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use serde::de::DeserializeOwned;
use serde_path_to_error::Segment;
use toml::{
    Spanned,
    de::{DeTable, DeValue, Deserializer},
};
use validated::Validated;

use crate::{CommandError, Operation};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Args, Debug)]
#[group(skip)]
pub struct Layers {
    #[arg(long, global = true, value_name = "PATH", value_hint = ValueHint::FilePath, help = "Settings file layered over earlier ones and field defaults")]
    pub settings: Vec<Utf8PathBuf>,
}

#[derive(Debug)]
pub struct Origin {
    pub path: Utf8PathBuf,
    pub text: Arc<str>,
    pub span: Option<Range<usize>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn decoded<S, E>(paths: impl IntoIterator<Item: AsRef<Utf8Path>>) -> Result<S, NEVec<E>>
where
    S: DeserializeOwned,
    E: From<CommandError>,
{
    fn merged<'text>(lower: DeTable<'text>, upper: DeTable<'text>) -> DeTable<'text> {
        upper.into_iter().fold(lower, |mut table, (key, entry)| {
            let value = Spanned::new(
                entry.span(),
                match (table.remove(&key).map(Spanned::into_inner), entry.into_inner()) {
                    (Some(DeValue::Table(earlier)), DeValue::Table(later)) => DeValue::Table(merged(earlier, later)),
                    (_, later) => later,
                },
            );
            table.insert(key, value);
            table
        })
    }
    fn located(path: &serde_path_to_error::Path, texts: &[(Utf8PathBuf, Arc<str>)], tables: &[Spanned<DeTable<'_>>]) -> Option<Origin> {
        iter::zip(texts, tables).rev().find_map(|((file, text), table)| {
            let mut segments = path.iter();
            let root = match segments.next()? {
                Segment::Map { key } | Segment::Enum { variant: key } => table.get_ref().get(key.as_str()),
                Segment::Seq { index: _ } | Segment::Unknown => None,
            }?;
            segments
                .try_fold(root, |value, segment| match segment {
                    Segment::Map { key } | Segment::Enum { variant: key } => value.get_ref().get(key),
                    Segment::Seq { index } => value.get_ref().get(*index),
                    Segment::Unknown => None,
                })
                .map(|value| Origin { path: Utf8PathBuf::clone(file), text: Arc::clone(text), span: Some(value.span()) })
        })
    }
    let texts = future::join_all(paths.into_iter().map(|path| path.as_ref().to_owned()).map(async |path| (read_to_string(&path).await, path)))
        .await
        .into_iter()
        .map(|(read, path)| read.map(|text| (path, Arc::<str>::from(text))).map_err(|source| E::from(CommandError::Io { operation: Operation::Read, source })))
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let tables = texts
        .iter()
        .map(|(path, text)| {
            let (table, errors) = DeTable::parse_recoverable(text);
            NEVec::try_from_vec(errors).map_or(Validated::Good(table), |failures| {
                Validated::Fail(
                    failures
                        .into_nonempty_iter()
                        .map(|mut source| {
                            source.set_input(None);
                            E::from(CommandError::Syntax { origin: Origin { path: Utf8PathBuf::clone(path), text: Arc::clone(text), span: source.span() }, source: Box::new(source) })
                        })
                        .collect(),
                )
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    serde_path_to_error::deserialize(Deserializer::from(Spanned::new(0..0, tables.iter().map(|table| table.get_ref().clone()).fold(DeTable::new(), merged))))
        .map_err(|source| NEVec::new(E::from(CommandError::Settings { origin: located(source.path(), &texts, &tables), source: Box::new(source) })))
}

pub(crate) fn excerpt(title: &str, origin: &Origin) -> String {
    Renderer::plain()
        .render(&[Level::ERROR.primary_title(title).element(Snippet::source(&*origin.text).path(origin.path.as_str()).annotations(origin.span.clone().map(|span| AnnotationKind::Primary.span(span))))])
}
