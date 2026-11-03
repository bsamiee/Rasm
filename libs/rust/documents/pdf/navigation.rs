use core::{fmt, iter, num::NonZero};
use std::{
    collections::{BTreeMap, BTreeSet},
    sync::{Arc, LazyLock},
};

use itertools::Itertools as _;
use lopdf::{Dictionary, Object, ObjectId, dictionary, text_string};
use nonempty_collections::{IntoNonEmptyIterator as _, NonEmptyIterator as _};
use pdf_oxide::{
    Destination, OutlineItem,
    extractors::{PageLabelExtractor, PageLabelRange, PageLabelStyle},
};
use validated::Validated;

use crate::{Document, PdfError, Refusal, Subject, engine};

// --- [CONSTANTS] -----------------------------------------------------------------------

static UNCOVERED: LazyLock<PageLabelRange> = LazyLock::new(PageLabelRange::default);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Default)]
pub struct Navigation {
    pub outline: Vec<OutlineItem> = Vec::new(),
    pub labels: Vec<PageLabelRange> = Vec::new(),
}

struct Entry {
    title: Object,
    dest: Option<Object>,
    children: Vec<Self>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read(document: &Document) -> Result<Navigation, PdfError> {
    let engine = engine(document)?;
    engine.get_outline().and_then(|outline| PageLabelExtractor::extract(&engine).map(|labels| Navigation { outline: outline.unwrap_or_default(), labels })).map_err(PdfError::from)
}

#[must_use]
pub fn label(labels: &[PageLabelRange], page: usize) -> Option<String> {
    const LETTERS: NonZero<u32> = NonZero::new(26).unwrap();
    let range = covering(labels, page)?;
    let number = numbered(range, page)?;
    match range.style {
        PageLabelStyle::AlphaUpper | PageLabelStyle::AlphaLower => {
            let position = number.checked_sub(1)?;
            let letter = PageLabelRange::new(page).with_style(range.style).with_start_value((position % LETTERS).strict_add(1)).format_label(page);
            let repeats = position / LETTERS;
            Some(format!("{}{}", range.prefix.as_deref().unwrap_or_default(), fmt::from_fn(|formatter| (0..=repeats).try_for_each(|_| formatter.write_str(&letter)))))
        }
        PageLabelStyle::Decimal | PageLabelStyle::RomanUpper | PageLabelStyle::RomanLower | PageLabelStyle::None => Some(range.format_label(page)),
    }
}

#[must_use]
pub(crate) fn assembled<'part, I>(parts: I) -> Navigation
where I: IntoIterator<Item = (&'part Navigation, &'part [usize])> {
    fn relocated(items: &[OutlineItem], sources: &BTreeMap<usize, usize>) -> Vec<OutlineItem> {
        items
            .iter()
            .filter_map(|item| {
                let children = relocated(&item.children, sources);
                let dest = match &item.dest {
                    Some(Destination::PageIndex(source)) => sources.get(source).copied().map(Destination::PageIndex),
                    Some(Destination::Named(_)) | None => None,
                };
                (dest.is_some() || !children.is_empty()).then(|| OutlineItem { title: item.title.clone(), dest, children })
            })
            .collect()
    }
    let placed = parts
        .into_iter()
        .scan(0_usize, |offset, (navigation, pages)| {
            let start = *offset;
            *offset = start.strict_add(pages.len());
            Some((navigation, iter::zip(pages.iter().copied(), start..*offset)))
        })
        .collect::<Vec<_>>();
    let numbering = placed
        .iter()
        .flat_map(|(navigation, pages)| {
            pages.clone().map(move |(source, target)| {
                let (range, page) = covering(&navigation.labels, source).map_or((&*UNCOVERED, target), |range| (range, source));
                numbered(range, page).map_or_else(
                    || PageLabelRange::new(target).with_style(PageLabelStyle::None),
                    |start_value| PageLabelRange { start_page: target, style: range.style, prefix: range.prefix.clone(), start_value },
                )
            })
        })
        .collect::<Vec<_>>();
    Navigation {
        outline: placed.iter().flat_map(|(navigation, pages)| relocated(&navigation.outline, &pages.clone().rev().collect())).collect(),
        labels: if placed.iter().any(|(navigation, _)| !navigation.labels.is_empty()) {
            numbering
                .chunk_by(|earlier, later| {
                    earlier.style == later.style && earlier.prefix == later.prefix && (earlier.style == PageLabelStyle::None || earlier.start_value.checked_add(1) == Some(later.start_value))
                })
                .filter_map(<[_]>::first)
                .cloned()
                .collect()
        } else {
            Vec::new()
        },
    }
}

pub(crate) fn write(structure: &mut lopdf::Document, navigation: &Navigation) -> Result<(), PdfError> {
    const DESTS: &str = "Dests";
    const NAMES: &str = "Names";
    const OUTLINES: &str = "Outlines";
    fn resolved<F>(items: &[OutlineItem], target: &F) -> Validated<Vec<Entry>, PdfError>
    where F: Fn(&OutlineItem, &Destination) -> Result<Object, PdfError> {
        items
            .iter()
            .map(|item| {
                Validated::from(item.dest.as_ref().map(|destination| target(item, destination)).transpose()).map2(resolved(&item.children, target), |dest, children| Entry {
                    title: text_string(&item.title),
                    dest,
                    children,
                })
            })
            .collect()
    }
    fn linked(structure: &mut lopdf::Document, siblings: Vec<Entry>, parent: ObjectId) -> Result<(i64, Option<[ObjectId; 2]>), lopdf::Error> {
        let ids = siblings.iter().map(|_sibling| structure.try_new_object_id()).collect::<Result<Vec<_>, _>>()?;
        let count = iter::zip(siblings, &ids).enumerate().try_fold(0_i64, |count, (index, (Entry { title, dest, children }, &id))| {
            let (descendants, ends) = linked(structure, children, id)?;
            structure.set_object(
                id,
                present([
                    ("Title", Some(title)),
                    ("Parent", Some(parent.into())),
                    ("Prev", index.checked_sub(1).and_then(|before| ids.get(before)).copied().map(Object::from)),
                    ("Next", ids.get(index.strict_add(1)).copied().map(Object::from)),
                    ("First", ends.map(|[first, _]| first.into())),
                    ("Last", ends.map(|[_, last]| last.into())),
                    ("Count", ends.map(|_ends| descendants.into())),
                    ("Dest", dest),
                ]),
            );
            Ok::<_, lopdf::Error>(count.strict_add(descendants).strict_add(1))
        })?;
        Ok((count, ids.first().zip(ids.last()).map(|(&first, &last)| [first, last])))
    }
    let destination = |page: ObjectId| Object::Array(vec![page.into(), "XYZ".into(), Object::Null, Object::Null, Object::Null]);
    let place = |dictionary: &mut Dictionary, key: &str, value: Option<Object>| match value {
        Some(value) => dictionary.set(key, value),
        None => drop(dictionary.remove(key.as_bytes())),
    };
    let pages = structure.page_iter().collect::<Vec<_>>();
    navigation
        .labels
        .iter()
        .map(|range| match (range.start_page, range.start_value) {
            (index, _) if index >= pages.len() => Err((Subject::Page(index), PdfError::Page { index, count: pages.len() })),
            (index, 0) => Err((Subject::Page(index), PdfError::Numbering(index))),
            _ => Ok(()),
        })
        .collect::<Validated<(), _>>()
        .ok()
        .map_err(PdfError::Refused)?;
    let labeled = iter::zip(0_usize.., &pages)
        .filter_map(|(page, &id)| label(&navigation.labels, page).filter(|name| !name.is_empty()).map(|name| (name.into_bytes(), id)))
        .into_group_map()
        .into_iter()
        .collect::<BTreeMap<Vec<u8>, Vec<ObjectId>>>();
    let catalog = structure.catalog()?;
    let mut directory = crate::nested(structure, catalog, &[NAMES.as_bytes()])?.cloned().unwrap_or_default();
    let held = pages.iter().copied().collect::<BTreeSet<_>>();
    let opens = |value: &Object| {
        crate::resolved(structure, value)
            .and_then(|target| match target {
                Object::Dictionary(action) => crate::entry(structure, action, b"D"),
                Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Array(_) | Object::Stream(_) | Object::Reference(_) =>
                    Ok(Some(target)),
            })
            .is_ok_and(|target| matches!(target, Some(Object::Array(explicit)) if matches!(explicit.first(), Some(Object::Reference(page)) if held.contains(page))))
    };
    let legacy = crate::nested(structure, catalog, &[DESTS.as_bytes()])?
        .into_iter()
        .flat_map(Dictionary::iter)
        .filter(|&(_, value)| opens(value))
        .map(|(name, value)| (name.clone(), value.clone()))
        .collect::<Dictionary>();
    let named = crate::nested(structure, &directory, &[DESTS.as_bytes()])?.map(|tree| crate::leaves(structure, tree, NAMES, &|name: &Object| name.as_str().map(<[u8]>::to_vec), &[])).transpose()?;
    let destinations = iter::chain(
        labeled.iter().filter_map(|(name, owners)| match owners {
            [page] => Some((name.clone(), destination(*page))),
            _ => None,
        }),
        named.into_iter().flatten().filter(|(_, value)| opens(value)),
    )
    .collect::<BTreeMap<_, _>>();
    let target = |item: &OutlineItem, dest: &Destination| match dest {
        Destination::PageIndex(index) => pages.get(*index).map(|&page| destination(page)).ok_or_else(|| PdfError::Named { name: Arc::from(item.title.as_str()), refusal: Refusal::Destination }),
        Destination::Named(name) => match (destinations.contains_key(name.as_bytes()), legacy.has(name.as_bytes()), labeled.contains_key(name.as_bytes())) {
            (true, _, _) => Ok(Object::string_literal(name.as_str())),
            (false, true, _) => Ok(Object::Name(name.as_bytes().to_vec())),
            (false, false, true) => Err(PdfError::Named { name: Arc::from(name.as_str()), refusal: Refusal::Label }),
            (false, false, false) => Err(PdfError::Named { name: Arc::from(item.title.as_str()), refusal: Refusal::Destination }),
        },
    };
    let outline = resolved(&navigation.outline, &target).ok().map_err(|errors| PdfError::Refused(errors.into_nonempty_iter().map(|error| (Subject::Outline, error)).collect()))?;
    let ranges = iter::chain(navigation.labels.first().map(|_first| &*UNCOVERED), &navigation.labels).map(|range| (range.start_page, range)).collect::<BTreeMap<_, _>>();
    let nums = iter::zip(0..pages.len(), 0_i64..)
        .filter_map(|(page, key)| {
            ranges.get(&page).map(|range| {
                [
                    Object::Integer(key),
                    Object::Dictionary(present([("S", range.style.to_name().map(Object::from)), ("P", range.prefix.as_deref().map(text_string)), ("St", Some(range.start_value.into()))])),
                ]
            })
        })
        .flatten()
        .collect::<Vec<_>>();
    let prior = catalog.get(OUTLINES.as_bytes()).and_then(Object::as_reference).ok();
    place(
        &mut directory,
        DESTS,
        (!destinations.is_empty()).then(|| dictionary! { NAMES => destinations.into_iter().flat_map(|(name, dest)| [Object::string_literal(name), dest]).collect::<Vec<_>>() }.into()),
    );
    let root = match (prior, outline.as_slice()) {
        (Some(root), _) => Some(root),
        (None, [_, ..]) => Some(structure.try_new_object_id()?),
        (None, []) => None,
    };
    if let Some(root) = root {
        let (count, ends) = linked(structure, outline, root)?;
        structure.set_object(
            root,
            present([("Type", Some(OUTLINES.into())), ("First", ends.map(|[first, _]| first.into())), ("Last", ends.map(|[_, last]| last.into())), ("Count", ends.map(|_ends| count.into()))]),
        );
    }
    let written = structure.catalog_mut()?;
    for (key, value) in [
        (OUTLINES, root.map(Object::Reference)),
        ("PageLabels", (!nums.is_empty()).then(|| dictionary! { "Nums" => nums }.into())),
        (NAMES, Some(directory).filter(|names| !names.is_empty()).map(Object::from)),
        (DESTS, (!legacy.is_empty()).then(|| legacy.into())),
    ] {
        place(written, key, value);
    }
    Ok(())
}

fn covering(labels: &[PageLabelRange], page: usize) -> Option<&PageLabelRange> {
    labels.first().map(|_first| labels.iter().filter(|range| range.start_page <= page).max_by_key(|range| range.start_page).unwrap_or(&UNCOVERED))
}

fn numbered(range: &PageLabelRange, page: usize) -> Option<u32> { range.start_value.checked_add(u32::try_from(page.checked_sub(range.start_page)?).unwrap_or(u32::MAX)) }

fn present<const N: usize>(fields: [(&str, Option<Object>); N]) -> Dictionary { fields.into_iter().filter_map(|(key, value)| value.map(|value| (key, value))).collect() }
