use core::iter;
use std::{collections::BTreeMap, sync::Arc};

use lopdf::{Dictionary, Object, ObjectId, dictionary, text_string};
use pdf_oxide::{
    Destination, OutlineItem,
    extractors::{PageLabelExtractor, PageLabelRange, PageLabelStyle},
};

use crate::{Document, PdfError, engine};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Default)]
pub struct Navigation {
    pub outline: Vec<OutlineItem>,
    pub labels: Vec<PageLabelRange>,
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
pub(crate) fn assembled<'part, I>(parts: I) -> Navigation
where I: IntoIterator<Item = (&'part Navigation, &'part [usize])> {
    fn relocated<F>(items: &[OutlineItem], target: &F) -> Vec<OutlineItem>
    where F: Fn(&Destination) -> Option<usize> {
        items
            .iter()
            .filter_map(|item| {
                let children = relocated(&item.children, target);
                let dest = item.dest.as_ref().and_then(target).map(Destination::PageIndex);
                (dest.is_some() || !children.is_empty()).then(|| OutlineItem { title: item.title.clone(), dest, children })
            })
            .collect()
    }
    let covering = |labels: &'part [PageLabelRange], page: usize| labels.iter().filter(|range| range.start_page <= page).max_by_key(|range| range.start_page);
    let relabeled = |labels: &'part [PageLabelRange], source: usize, target: usize| {
        let range = covering(labels, source)?;
        let start_value = range.start_value.checked_add(u32::try_from(source.strict_sub(range.start_page)).ok()?)?;
        Some(PageLabelRange { start_page: target, style: range.style, prefix: range.prefix.clone(), start_value })
    };
    let continues = |earlier: &PageLabelRange, later: &PageLabelRange| {
        earlier.style == later.style && earlier.prefix == later.prefix && (earlier.style == PageLabelStyle::None || earlier.start_value.checked_add(1) == Some(later.start_value))
    };
    let placed = parts
        .into_iter()
        .scan(0_usize, |offset, (navigation, pages)| {
            let start = *offset;
            *offset = start.strict_add(pages.len());
            Some((start, navigation, pages))
        })
        .collect::<Vec<_>>();
    let numbering = placed
        .iter()
        .flat_map(|&(start, navigation, pages)| {
            iter::zip(start.., pages).map(move |(target, &source)| relabeled(&navigation.labels, source, target).unwrap_or_else(|| PageLabelRange::new(target).with_style(PageLabelStyle::None)))
        })
        .collect::<Vec<_>>();
    Navigation {
        outline: placed
            .iter()
            .flat_map(|&(start, navigation, pages)| {
                relocated(&navigation.outline, &|destination: &Destination| {
                    let position = match destination {
                        Destination::PageIndex(source) => pages.iter().position(|page| page == source),
                        Destination::Named(name) => pages.iter().position(|&page| covering(&navigation.labels, page).is_some_and(|range| range.format_label(page) == *name)),
                    };
                    position.map(|index| start.strict_add(index))
                })
            })
            .collect(),
        labels: placed.iter().any(|&(_, navigation, _)| !navigation.labels.is_empty()).then(|| numbering.chunk_by(continues).filter_map(<[_]>::first).cloned().collect()).unwrap_or_default(),
    }
}

pub(crate) fn write(structure: &mut lopdf::Document, navigation: &Navigation) -> Result<(), PdfError> {
    let [outlines, names] = ["Outlines", "Names"];
    let place = |dictionary: &mut Dictionary, key: &str, value: Option<Object>| match value {
        Some(value) => dictionary.set(key, value),
        None => drop(dictionary.remove(key.as_bytes())),
    };
    let pages = structure.page_iter().collect::<Vec<_>>();
    let labels = navigation.labels.iter().map(|range| (range.start_page, range)).collect::<BTreeMap<_, _>>();
    labels.range(pages.len()..).next().map_or(Ok(()), |(&index, _)| Err(PdfError::Page { index, count: pages.len() }))?;
    let destinations = iter::zip(0_usize.., &pages)
        .filter_map(|(page, &id)| labels.range(..=page).next_back().map(|(_, range)| (Arc::<str>::from(range.format_label(page)), id)))
        .filter(|(label, _)| !label.is_empty())
        .try_fold(BTreeMap::new(), |mut destinations, (label, id)| destinations.insert(Arc::clone(&label), id).map_or(Ok(destinations), |_earlier| Err(PdfError::Label(label))))?;
    let outline = entries(&navigation.outline, &pages, &destinations)?;
    let mut directory = structure.catalog().and_then(|catalog| catalog.has(names.as_bytes()).then(|| structure.get_dict_in_dict(catalog, names.as_bytes()).cloned()).transpose())?.unwrap_or_default();
    let nums = (0..pages.len())
        .zip(0_i64..)
        .filter_map(|(page, number)| {
            labels.get(&page).map(|range| {
                [
                    Object::Integer(number),
                    Object::Dictionary(present([("S", range.style.to_name().map(Object::from)), ("P", range.prefix.as_deref().map(text_string)), ("St", Some(range.start_value.into()))])),
                ]
            })
        })
        .flatten()
        .collect::<Vec<_>>();
    let ids = outline.iter().map(|_entry| structure.new_object_id()).collect::<Vec<_>>();
    let root = ids.first().zip(ids.last()).map(|(&first, &last)| {
        let root = structure.new_object_id();
        let count = linked(structure, outline, &ids, root);
        structure.set_object(root, present([("Type", Some(outlines.into())), ("First", Some(first.into())), ("Last", Some(last.into())), ("Count", Some(count.into()))]));
        Object::Reference(root)
    });
    place(
        &mut directory,
        "Dests",
        (!destinations.is_empty())
            .then(|| dictionary! { names => destinations.iter().flat_map(|(label, &page)| [Object::string_literal(label.as_bytes()), destination(page)]).collect::<Vec<_>>() }.into()),
    );
    let catalog = structure.catalog_mut()?;
    [(outlines, root), ("PageLabels", (!nums.is_empty()).then(|| dictionary! { "Nums" => nums }.into())), (names, Some(directory).filter(|dictionary| !dictionary.is_empty()).map(Object::from))]
        .into_iter()
        .for_each(|(key, value)| place(catalog, key, value));
    Ok(())
}

fn entries(items: &[OutlineItem], pages: &[ObjectId], destinations: &BTreeMap<Arc<str>, ObjectId>) -> Result<Vec<Entry>, PdfError> {
    items
        .iter()
        .map(|item| {
            let refused = || PdfError::Destination(Arc::from(item.title.as_str()));
            let target = match &item.dest {
                None => Ok(None),
                Some(Destination::PageIndex(index)) => pages.get(*index).map(|&page| Some(destination(page))).ok_or_else(refused),
                Some(Destination::Named(name)) => destinations.contains_key(name.as_str()).ok_or_else(refused).map(|()| Some(Object::string_literal(name.as_str()))),
            };
            target.and_then(|dest| entries(&item.children, pages, destinations).map(|children| Entry { title: text_string(&item.title), dest, children }))
        })
        .try_collect::<Vec<_>>()
}

fn linked(structure: &mut lopdf::Document, siblings: Vec<Entry>, ids: &[ObjectId], parent: ObjectId) -> i64 {
    iter::zip(siblings, ids).enumerate().fold(0_i64, |count, (index, (Entry { title, dest, children }, &id))| {
        let below = children.iter().map(|_child| structure.new_object_id()).collect::<Vec<_>>();
        let descendants = linked(structure, children, &below, id);
        let ends = below.first().zip(below.last());
        structure.set_object(
            id,
            present([
                ("Title", Some(title)),
                ("Parent", Some(parent.into())),
                ("Prev", index.checked_sub(1).and_then(|before| ids.get(before)).copied().map(Object::from)),
                ("Next", ids.get(index.strict_add(1)).copied().map(Object::from)),
                ("First", ends.map(|(&first, _)| first.into())),
                ("Last", ends.map(|(_, &last)| last.into())),
                ("Count", (!below.is_empty()).then_some(descendants.into())),
                ("Dest", dest),
            ]),
        );
        count.strict_add(descendants).strict_add(1)
    })
}

fn present<const N: usize>(fields: [(&str, Option<Object>); N]) -> Dictionary { fields.into_iter().filter_map(|(key, value)| value.map(|value| (key, value))).collect() }

fn destination(page: ObjectId) -> Object { Object::Array(vec![page.into(), "XYZ".into(), Object::Null, Object::Null, Object::Null]) }
