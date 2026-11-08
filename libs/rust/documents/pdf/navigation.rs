use core::{fmt, iter, num::NonZero};
use std::{
    collections::{BTreeMap, BTreeSet},
    sync::{Arc, LazyLock},
};

use hayro_syntax::object::dict::keys::{COUNT, D, DEST, DESTS, FIRST, LAST, NAMES, NEXT, NUMS, OUTLINES, P, PAGE_LABELS, PARENT, PREV, S, ST, TITLE, TYPE};
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

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read(document: &Document) -> Result<Navigation, PdfError> {
    let engine = engine(document)?;
    engine.get_outline().and_then(|outline| PageLabelExtractor::extract(&engine).map(|labels| Navigation { outline: outline.unwrap_or_default(), labels })).map_err(PdfError::from)
}

#[must_use]
pub fn label(labels: &[PageLabelRange], page: usize) -> Option<String> {
    let range = labels.first().map(|_first| labels.iter().filter(|range| range.start_page <= page).max_by_key(|range| range.start_page).unwrap_or(&UNCOVERED))?;
    formatted(range, page)
}

fn formatted(range: &PageLabelRange, page: usize) -> Option<String> {
    const LETTERS: NonZero<u32> = NonZero::new(26).unwrap();
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
    let placed = parts
        .into_iter()
        .scan(0_usize, |offset, (navigation, pages)| {
            let start = *offset;
            *offset = start.strict_add(pages.len());
            Some((navigation, iter::zip(pages.iter().copied(), start..*offset)))
        })
        .collect::<Vec<_>>();
    let numbering = placed.iter().flat_map(|(navigation, pages)| {
        let ranges = indexed_labels(&navigation.labels);
        pages.clone().map(move |(source, target)| {
            let (range, page) = ranges.range(..=source).next_back().map_or((&*UNCOVERED, target), |(_, &range)| (range, source));
            numbered(range, page).map_or_else(
                || PageLabelRange::new(target).with_style(PageLabelStyle::None),
                |start_value| PageLabelRange { start_page: target, style: range.style, prefix: range.prefix.clone(), start_value },
            )
        })
    });
    Navigation {
        outline: placed
            .iter()
            .flat_map(|(navigation, pages)| {
                let sources = pages.clone().into_grouping_map().min();
                map_outline(&navigation.outline, |item, children| {
                    let children = children.into_iter().flatten().collect::<Vec<_>>();
                    let dest = match &item.dest {
                        Some(Destination::PageIndex(source)) => sources.get(source).copied().map(Destination::PageIndex),
                        Some(Destination::Named(_)) | None => None,
                    };
                    (dest.is_some() || !children.is_empty()).then(|| OutlineItem { title: item.title.clone(), dest, children })
                })
                .into_iter()
                .flatten()
            })
            .collect(),
        labels: if placed.iter().any(|(navigation, _)| !navigation.labels.is_empty()) {
            numbering
                .map(|range| {
                    let last = range.start_value;
                    (range, last)
                })
                .coalesce(|(earlier, last), (later, end)| {
                    if earlier.style == later.style && earlier.prefix == later.prefix && (earlier.style == PageLabelStyle::None || last.checked_add(1) == Some(later.start_value)) {
                        Ok((earlier, end))
                    } else {
                        Err(((earlier, last), (later, end)))
                    }
                })
                .map(|(range, _)| range)
                .collect()
        } else {
            Vec::new()
        },
    }
}

pub(crate) fn write(structure: &mut lopdf::Document, navigation: &Navigation) -> Result<(), PdfError> {
    fn linked(structure: &mut lopdf::Document, siblings: &[(ObjectId, i64)], parent: ObjectId) -> Result<(i64, Option<[ObjectId; 2]>), lopdf::Error> {
        let count = siblings.iter().enumerate().try_fold(0_i64, |count, (index, &(id, descendants))| {
            structure.get_dictionary_mut(id)?.as_hashmap_mut().extend(present([
                (PARENT, Some(parent.into())),
                (PREV, index.checked_sub(1).and_then(|before| siblings.get(before)).map(|&(previous_id, _)| previous_id.into())),
                (NEXT, siblings.get(index.strict_add(1)).map(|&(next_id, _)| next_id.into())),
            ]));
            Ok::<_, lopdf::Error>(count.strict_add(descendants).strict_add(1))
        })?;
        Ok((count, siblings.first().zip(siblings.last()).map(|(first, last)| [first.0, last.0])))
    }
    let destination = |page: ObjectId| Object::Array(vec![page.into(), "XYZ".into(), Object::Null, Object::Null, Object::Null]);
    let place = |dictionary: &mut Dictionary, key: &[u8], value: Option<Object>| match value {
        Some(value) => dictionary.set(key, value),
        None => drop(dictionary.remove(key)),
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
    let ranges = indexed_labels(&navigation.labels);
    let labeled = iter::zip(0_usize.., &pages)
        .filter_map(|(page, &id)| ranges.range(..=page).next_back().and_then(|(_, range)| formatted(range, page)).filter(|name| !name.is_empty()).map(|name| (name.into_bytes(), id)))
        .into_group_map()
        .into_iter()
        .collect::<BTreeMap<Vec<u8>, Vec<ObjectId>>>();
    let catalog = structure.catalog()?;
    let prior = catalog.get(OUTLINES).and_then(Object::as_reference).ok();
    let mut directory = crate::nested(structure, catalog, &[NAMES])?.cloned().unwrap_or_default();
    let held = pages.iter().copied().collect::<BTreeSet<_>>();
    let opens = |value: &Object| {
        crate::resolved(structure, value)
            .and_then(|target| match target {
                Object::Dictionary(action) => crate::entry(structure, action, D),
                Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Array(_) | Object::Stream(_) | Object::Reference(_) =>
                    Ok(Some(target)),
            })
            .is_ok_and(|target| matches!(target, Some(Object::Array(explicit)) if matches!(explicit.first(), Some(Object::Reference(page)) if held.contains(page))))
    };
    let legacy = crate::nested(structure, catalog, &[DESTS])?
        .into_iter()
        .flat_map(Dictionary::iter)
        .filter(|&(_, value)| opens(value))
        .map(|(name, value)| (name.clone(), value.clone()))
        .collect::<Dictionary>();
    let named = crate::nested(structure, &directory, &[DESTS])?.map(|tree| crate::leaves(structure, tree, NAMES, &|name: &Object| name.as_str().map(<[u8]>::to_vec))).transpose()?;
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
    let outline = map_outline(&navigation.outline, |item, children| {
        match Validated::from(item.dest.as_ref().map(|dest| target(item, dest)).transpose()).map2(children.into_iter().collect::<Validated<Vec<_>, _>>(), |dest, children| (dest, children)) {
            Validated::Good((dest, children)) => Validated::from(
                structure
                    .try_new_object_id()
                    .and_then(|id| {
                        let (count, ends) = linked(structure, &children, id)?;
                        structure.set_object(
                            id,
                            present([
                                (TITLE, Some(text_string(&item.title))),
                                (FIRST, ends.map(|[first, _]| first.into())),
                                (LAST, ends.map(|[_, last]| last.into())),
                                (COUNT, ends.map(|_ends| count.into())),
                                (DEST, dest),
                            ])
                            .collect::<Dictionary>(),
                        );
                        Ok((id, count))
                    })
                    .map_err(PdfError::from),
            ),
            Validated::Fail(errors) => Validated::Fail(errors),
        }
    })
    .into_iter()
    .collect::<Validated<Vec<_>, _>>()
    .ok()
    .map_err(|errors| PdfError::Refused(errors.into_nonempty_iter().map(|error| (Subject::Outline, error)).collect()))?;
    let nums = ranges
        .into_iter()
        .map(|(page, range)| {
            let key = i64::try_from(page).map_err(lopdf::Error::from)?;
            Ok::<_, lopdf::Error>([
                Object::Integer(key),
                Object::Dictionary(present([(S, range.style.to_name().map(Object::from)), (P, range.prefix.as_deref().map(text_string)), (ST, Some(range.start_value.into()))]).collect()),
            ])
        })
        .process_results(|pairs| pairs.flatten().collect::<Vec<_>>())?;
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
        let (count, ends) = linked(structure, &outline, root)?;
        structure.set_object(
            root,
            present([(TYPE, Some(Object::Name(OUTLINES.to_vec()))), (FIRST, ends.map(|[first, _]| first.into())), (LAST, ends.map(|[_, last]| last.into())), (COUNT, ends.map(|_ends| count.into()))])
                .collect::<Dictionary>(),
        );
    }
    let written = structure.catalog_mut()?;
    for (key, value) in [
        (OUTLINES, root.map(Object::Reference)),
        (PAGE_LABELS, (!nums.is_empty()).then(|| dictionary! { NUMS => nums }.into())),
        (NAMES, Some(directory).filter(|names| !names.is_empty()).map(Object::from)),
        (DESTS, (!legacy.is_empty()).then(|| legacy.into())),
    ] {
        place(written, key, value);
    }
    Ok(())
}

fn indexed_labels(labels: &[PageLabelRange]) -> BTreeMap<usize, &PageLabelRange> { iter::chain(labels.first().map(|_first| &*UNCOVERED), labels).map(|range| (range.start_page, range)).collect() }

fn map_outline<T>(items: &[OutlineItem], mut map: impl FnMut(&OutlineItem, Vec<T>) -> T) -> Vec<T> {
    let mut remaining = items.iter();
    let mut mapped = Vec::new();
    let mut ancestors = Vec::new();
    loop {
        match remaining.next() {
            Some(item) => {
                ancestors.push((item, remaining, mapped));
                (remaining, mapped) = (item.children.iter(), Vec::new());
            }
            None => match ancestors.pop() {
                Some((item, siblings, mut preceding)) => {
                    preceding.push(map(item, mapped));
                    (remaining, mapped) = (siblings, preceding);
                }
                None => return mapped,
            },
        }
    }
}

fn numbered(range: &PageLabelRange, page: usize) -> Option<u32> { range.start_value.checked_add(u32::try_from(page.checked_sub(range.start_page)?).ok()?) }

fn present<const N: usize>(fields: [(&[u8], Option<Object>); N]) -> impl Iterator<Item = (Vec<u8>, Object)> { fields.into_iter().filter_map(|(key, value)| value.map(|value| (key.to_vec(), value))) }
