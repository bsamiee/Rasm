use core::{borrow::Borrow, iter};
use std::{
    collections::{BTreeMap, BTreeSet, HashMap, HashSet},
    sync::Arc,
};

use color::{OpaqueColor, Srgb};
use krilla::{
    SerializeSettings,
    blend::BlendMode,
    configure::{ConfigurationBuilder, PdfVersion},
    geom::Size,
    page::PageSettings,
    pdf::PdfDocument,
};
use lopdf::{
    Dictionary, Object, ObjectId,
    content::{Content, Operation},
    decode_text_string, dictionary,
};
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _};
use validated::Validated;

use crate::{
    entry, resolved,
    Document, PdfError, Subject,
    layers::{self, Group},
    navigation::{self, Navigation},
};

// --- [TYPES] ---------------------------------------------------------------------------

type Directory = BTreeMap<Vec<u8>, BTreeMap<Vec<u8>, Object>>;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Mixing {
    Additive,
    Subtractive,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ANNOTS: &str = "Annots";
const DESTS: &str = "Dests";
const KIDS: &str = "Kids";
const NAMES: &str = "Names";
const OPEN_ACTION: &str = "OpenAction";
const PARENT: &str = "Parent";
const RESOURCES: &str = "Resources";
const STRUCT_PARENTS: &str = "StructParents";
const STRUCT_TREE_ROOT: &str = "StructTreeRoot";
const FILL: &str = "rg";
const STROKE: &str = "RG";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Part {
    pub document: Document,
    pub pages: Option<NEVec<usize>>,
}

struct Source {
    structure: lopdf::Document,
    navigation: Navigation,
    groups: Vec<Group>,
}

struct Assembly {
    structure: lopdf::Document,
    held: HashSet<ObjectId>,
}

struct Tagging {
    kids: Vec<Object>,
    numbers: Vec<(i64, Object)>,
    span: i64,
    tree: Dictionary,
}

enum Link {
    Kept,
    Redirected(Vec<Object>),
    Unlinked,
    Dropped,
}

enum Painted {
    Page(ObjectId),
    Form(ObjectId),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PARTS]

pub fn assemble<I>(parts: I) -> Result<Document, PdfError>
where I: IntoNonEmptyIterator<Item: Borrow<Part>> {
    let (opening, rest) = parts
        .into_nonempty_iter()
        .enumerate()
        .map(|(index, part)| {
            let Part { document, pages } = part.borrow();
            let located = |error: PdfError| (Subject::Part(index), error);
            source(document).ok().map_or_else(
                |failures| Validated::Fail(failures.into_nonempty_iter().map(located).collect()),
                |source| Validated::from(selection(&source, pages.as_ref()).map(|chosen| (source, chosen)).map_err(located)),
            )
        })
        .next();
    let read = opening.map2(rest.collect::<Validated<Vec<_>, _>>(), |opening, rest| NEVec::from((opening, rest))).ok().map_err(PdfError::Refused)?;
    composed(&read.nonempty_iter().map(|(source, chosen)| (source, chosen.as_slice())).collect::<NEVec<_>>())
}

pub fn split<I>(document: &Document, selections: I) -> Result<Vec<Document>, PdfError>
where I: IntoIterator<Item = NEVec<usize>> {
    let whole = source(document).ok().map_err(|failures| failures.into_nonempty_iter().next().0)?;
    selections
        .into_iter()
        .enumerate()
        .map(|(index, pages)| selection(&whole, Some(&pages)).and_then(|chosen| composed(&NEVec::new((&whole, chosen.as_slice())))).map_err(|error| (Subject::Part(index), error)))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)
}

fn source(document: &Document) -> Validated<Source, PdfError> {
    Validated::from(crate::structure(document)).map3(Validated::from(navigation::read(document)), Validated::from(layers::groups(document)), |structure, navigation, groups| Source { structure, navigation, groups })
}

fn selection(source: &Source, pages: Option<&NEVec<usize>>) -> Result<Vec<(usize, ObjectId)>, PdfError> {
    let sheets = source.structure.page_iter().collect::<Vec<_>>();
    pages.map_or_else(
        || Ok(sheets.iter().copied().enumerate().collect()),
        |chosen| {
            chosen
                .iter()
                .map(|&index| sheets.get(index).map(|&sheet| (index, sheet)).ok_or((Subject::Page(index), PdfError::Page { index, count: sheets.len() })))
                .collect::<Validated<Vec<_>, _>>()
                .ok()
                .map_err(PdfError::Refused)
        },
    )
}

fn composed(parts: &NEVec<(&Source, &[(usize, ObjectId)])>) -> Result<Document, PdfError> {
    const ACROFORM: &str = "AcroForm";
    const DEFAULT_RESOURCES: &str = "DR";
    const FIELDS: &str = "Fields";
    const NEED_APPEARANCES: &str = "NeedAppearances";
    const OPTIONAL_CONTENT: &str = "OCProperties";
    let mut merged = lopdf::Document::with_version(parts.nonempty_iter().map(|(source, _)| source.structure.version.as_str()).max());
    let placed = parts.nonempty_iter().map(|&(source, chosen)| placed(source, &mut merged, chosen)).collect::<Result<NEVec<_>, _>>()?;
    let roots = placed.nonempty_iter().map(|(trailer, _)| trailer.get(b"Root").and_then(Object::as_reference)).collect::<Result<NEVec<_>, _>>()?;
    let catalogs = roots.nonempty_iter().map(|&root| merged.get_dictionary(root).cloned()).collect::<Result<NEVec<_>, _>>()?;
    let mut seen = HashSet::new();
    let kept = placed
        .iter()
        .map(|(_, selection)| {
            selection
                .iter()
                .map(|&page| {
                    if seen.insert(page) {
                        Ok((page, page))
                    } else {
                        let mut copy = merged.get_dictionary(page)?.clone();
                        copy.remove(ANNOTS.as_bytes());
                        copy.remove(STRUCT_PARENTS.as_bytes());
                        merged.try_add_object(copy).map(|duplicate| (page, duplicate))
                    }
                })
                .try_collect::<Vec<_>>()
        })
        .try_collect::<Vec<_>>()?;
    let pages = kept.iter().flatten().map(|&(_, page)| page).collect::<Vec<_>>();
    let mut assembly = Assembly { structure: merged, held: pages.iter().copied().collect() };
    let page_tree = assembly.structure.try_new_object_id()?;
    pages.iter().try_for_each(|&page| assembly.structure.get_dictionary_mut(page).map(|sheet| sheet.set(PARENT, page_tree)))?;
    assembly.structure.objects.insert(
        page_tree,
        Object::Dictionary(dictionary! { "Type" => "Pages", KIDS => pages.iter().copied().map(Object::Reference).collect::<Vec<_>>(), "Count" => i64::try_from(pages.len()).unwrap_or(i64::MAX) }),
    );
    let trees = catalogs.iter().map(|catalog| directory(&assembly, catalog)).try_collect::<Vec<_>>()?;
    let destinations = trees
        .iter()
        .map(|named| {
            named
                .get(DESTS.as_bytes())
                .into_iter()
                .flatten()
                .map(|(name, value)| explicit(&assembly, value).map(|target| target.map(|target| (name.clone(), target))))
                .filter_map(Result::transpose)
                .try_collect::<BTreeMap<_, _>>()
        })
        .try_collect::<Vec<_>>()?;
    iter::zip(&kept, &destinations).try_for_each(|(sheets, dests)| sheets.iter().filter(|(original, page)| original == page).try_for_each(|&(_, page)| linked(&mut assembly, page, dests)))?;
    let widgets = pages.iter().map(|&page| annotations(&assembly, page)).try_collect::<Vec<_>>()?.concat().into_iter().collect::<HashSet<_>>();
    let forms = catalogs.iter().map(|catalog| entry(&assembly.structure, catalog, ACROFORM)?.map(Object::as_dict).transpose().map(Option::<&Dictionary>::cloned)).try_collect::<Vec<_>>()?;
    let fields = forms
        .iter()
        .flatten()
        .map(|form| entry(&assembly.structure, form, FIELDS)?.map(Object::as_array).transpose()?.into_iter().flatten().map(Object::as_reference).try_collect::<Vec<_>>())
        .try_collect::<Vec<_>>()?
        .concat()
        .into_iter()
        .map(|field| retained(&mut assembly, field, &widgets, &[]).map(|keep| keep.then_some(Object::Reference(field))))
        .try_collect::<Vec<_>>()?
        .into_iter()
        .flatten()
        .collect::<Vec<_>>();
    let resources = forms
        .iter()
        .flatten()
        .map(|form| {
            entry(&assembly.structure, form, DEFAULT_RESOURCES)?
                .map(Object::as_dict)
                .transpose()?
                .into_iter()
                .flat_map(Dictionary::iter)
                .map(|(category, value)| resolved(&assembly.structure, value).and_then(Object::as_dict).map(|entries| (category.clone(), entries.clone())))
                .try_collect::<Vec<_>>()
        })
        .try_collect::<Vec<_>>()?
        .concat()
        .into_iter()
        .fold(BTreeMap::<Vec<u8>, Dictionary>::new(), |mut first, (category, entries)| {
            let slot = first.entry(category).or_default();
            for (name, value) in entries {
                slot.as_hashmap_mut().entry(name).or_insert(value);
            }
            first
        });
    let appearances = forms.iter().flatten().map(|form| entry(&assembly.structure, form, NEED_APPEARANCES).map(|needed| matches!(needed, Some(Object::Boolean(true))))).try_fold(false, |any, needed| needed.map(|needed| any || needed))?;
    let interactive = forms.iter().flatten().next().cloned().filter(|_| !fields.is_empty()).map(|mut form| {
        form.set(FIELDS, fields);
        form.remove(b"XFA");
        if !resources.is_empty() {
            form.set(DEFAULT_RESOURCES, resources.into_iter().map(|(category, entries)| (category, Object::Dictionary(entries))).collect::<Dictionary>());
        }
        if appearances {
            form.set(NEED_APPEARANCES, true);
        }
        form
    });
    let structure_root = assembly.structure.try_new_object_id()?;
    let (next_key, tagged) = iter::zip(&catalogs, &kept).try_fold((0_i64, Vec::new()), |(base, mut tagged), (catalog, sheets)| {
        tagging(&mut assembly, catalog, sheets, base, structure_root).map(|tagging| {
            let next = tagging.as_ref().map_or(base, |tagging| base.saturating_add(tagging.span));
            tagged.extend(tagging);
            (next, tagged)
        })
    })?;
    let dests = destinations.iter().flatten().filter(|(_, target)| opens(&assembly, target)).rev().map(|(name, target)| (name.clone(), Object::Array(target.clone()))).collect::<BTreeMap<_, _>>();
    let others = trees.iter().flat_map(|named| named.iter().filter(|(kind, _)| kind.as_slice() != DESTS.as_bytes())).fold(Directory::new(), |mut first, (kind, entries)| {
        let slot = first.entry(kind.clone()).or_default();
        for (name, value) in entries {
            slot.entry(name.clone()).or_insert_with(|| value.clone());
        }
        first
    });
    let directory = iter::once((DESTS.as_bytes().to_vec(), dests))
        .chain(others)
        .filter(|(_, entries)| !entries.is_empty())
        .map(|(kind, entries)| (kind, Object::Dictionary(dictionary! { NAMES => entries.into_iter().flat_map(|(name, value)| [Object::string_literal(name), value]).collect::<Vec<_>>() })))
        .collect::<Dictionary>();
    let root = *roots.first();
    let mut catalog = catalogs.first().clone();
    for key in ["Outlines", "PageLabels", DESTS, NAMES, ACROFORM, STRUCT_TREE_ROOT, OPTIONAL_CONTENT] {
        catalog.remove(key.as_bytes());
    }
    catalog.set("Pages", page_tree);
    if !opening(&assembly, &catalog)? {
        catalog.remove(OPEN_ACTION.as_bytes());
    }
    if !directory.is_empty() {
        catalog.set(NAMES, directory);
    }
    if let Some(interactive) = interactive {
        catalog.set(ACROFORM, interactive);
    }
    if !tagged.is_empty() {
        let unioned = |key: &str| {
            tagged.iter().map(|tagging| entry(&assembly.structure, &tagging.tree, key)?.map(Object::as_dict).transpose().map(Option::<&Dictionary>::cloned)).try_collect::<Vec<_>>().map(|found| {
                found.into_iter().flatten().flatten().fold(Dictionary::new(), |mut first, (name, value)| {
                    first.as_hashmap_mut().entry(name).or_insert(value);
                    first
                })
            })
        };
        let structure = dictionary! {
            "Type" => STRUCT_TREE_ROOT,
            "K" => tagged.iter().flat_map(|tagging| tagging.kids.iter().cloned()).collect::<Vec<_>>(),
            "ParentTree" => dictionary! { "Nums" => tagged.iter().flat_map(|tagging| tagging.numbers.iter().cloned()).collect::<BTreeMap<_, _>>().into_iter().flat_map(|(key, value)| [Object::Integer(key), value]).collect::<Vec<_>>() },
            "ParentTreeNextKey" => next_key,
            "RoleMap" => unioned("RoleMap")?,
            "ClassMap" => unioned("ClassMap")?,
        };
        assembly.structure.objects.insert(structure_root, Object::Dictionary(structure));
        catalog.set(STRUCT_TREE_ROOT, structure_root);
        catalog.set("MarkInfo", dictionary! { "Marked" => true });
    }
    assembly.structure.objects.insert(root, Object::Dictionary(catalog));
    let mut trailer = placed.first().0.clone();
    for key in ["Prev", "XRefStm", "Encrypt"] {
        trailer.remove(key.as_bytes());
    }
    assembly.structure.trailer = trailer;
    pruned(&mut assembly);
    let layered = assembly
        .structure
        .objects
        .iter()
        .filter_map(|(&id, object)| if let Object::Dictionary(node) = object { node.has_type(b"OCG").then_some((id, node)) } else { None })
        .map(|(id, node)| node.get_deref(b"Name", &assembly.structure).and_then(decode_text_string).map(|name| (id, name)))
        .try_collect::<Vec<_>>()?;
    let canonical = layered.iter().rev().map(|(id, name)| (name.as_str(), *id)).collect::<HashMap<_, _>>();
    let duplicates = layered.iter().filter_map(|(id, name)| canonical.get(name.as_str()).filter(|&&survivor| survivor != *id).map(|&survivor| (*id, survivor))).collect::<HashMap<_, _>>();
    for object in assembly.structure.objects.values_mut() {
        rewritten(object, &|id| duplicates.get(&id).map(|&survivor| Object::Reference(survivor)));
    }
    let registered = canonical.values().copied().collect::<BTreeSet<_>>();
    if !registered.is_empty() {
        assembly.structure.get_dictionary_mut(root)?.set(OPTIONAL_CONTENT, dictionary! { "OCGs" => registered.iter().copied().map(Object::Reference).collect::<Vec<_>>(), "D" => Dictionary::new() });
    }
    if !duplicates.is_empty() {
        pruned(&mut assembly);
    }
    let indices = parts.iter().map(|(_, chosen)| chosen.iter().map(|&(index, _)| index).collect::<Vec<_>>()).collect::<Vec<_>>();
    navigation::write(&mut assembly.structure, &navigation::assembled(iter::zip(parts, &indices).map(|((source, _), sheets)| (&source.navigation, sheets.as_slice()))))?;
    let written = crate::saved(assembly.structure)?;
    if registered.is_empty() {
        return Ok(written);
    }
    let present = layers::groups(&written)?.into_iter().map(|group| group.name).collect::<HashSet<_>>();
    layers::set(
        &written,
        parts
            .iter()
            .flat_map(|(source, _)| &source.groups)
            .filter(|group| present.contains(&group.name))
            .rev()
            .map(|group| (&*group.name, group))
            .collect::<BTreeMap<_, _>>()
            .into_values(),
    )
}

fn placed(source: &Source, merged: &mut lopdf::Document, chosen: &[(usize, ObjectId)]) -> Result<(Dictionary, Vec<ObjectId>), lopdf::Error> {
    let structure = &source.structure;
    let sheets = structure.page_iter().collect::<HashSet<_>>();
    let selected = chosen.iter().map(|&(_, page)| page).collect::<BTreeSet<_>>();
    let inheritance = selected.iter().map(|&page| inherited(source, page).map(|held| (page, held))).try_collect::<Vec<_>>()?;
    let roots = structure
        .trailer
        .iter()
        .flat_map(|(_, value)| referenced(value))
        .chain(selected.iter().copied())
        .chain(inheritance.iter().flat_map(|(_, held)| held.iter().flat_map(|(_, value)| referenced(value))))
        .collect::<Vec<_>>();
    let admitted = reachable(structure, roots, |id, object| if sheets.contains(&id) { selected.contains(&id) } else { !matches!(object, Object::Dictionary(node) if node.has_type(b"Pages")) });
    let offset = merged.max_id;
    let ceiling = admitted.iter().map(|&(number, _)| number).max().unwrap_or_default();
    merged.max_id = offset.checked_add(ceiling).ok_or(lopdf::Error::ObjectIdOverflow)?;
    let moved = |(number, generation): ObjectId| (number.strict_add(offset), generation);
    let shift = |object: &mut Object| rewritten(object, &|id| Some(if admitted.contains(&id) { Object::Reference(moved(id)) } else { Object::Null }));
    merged.objects.extend(admitted.iter().filter_map(|&id| {
        structure.objects.get(&id).map(|object| {
            let mut copied = object.clone();
            shift(&mut copied);
            (moved(id), copied)
        })
    }));
    inheritance.into_iter().try_for_each(|(page, held)| {
        merged.get_dictionary_mut(moved(page)).map(|sheet| {
            for (key, mut value) in held {
                shift(&mut value);
                sheet.set(key, value);
            }
        })
    })?;
    let mut trailer = structure.trailer.clone();
    for (_, value) in &mut trailer {
        shift(value);
    }
    Ok((trailer, chosen.iter().map(|&(_, page)| moved(page)).collect()))
}

fn inherited(source: &Source, page: ObjectId) -> Result<Vec<(Vec<u8>, Object)>, lopdf::Error> {
    const INHERITED: [&str; 4] = [RESOURCES, "MediaBox", "CropBox", "Rotate"];
    let structure = &source.structure;
    let node = |id: ObjectId| if let Some(Object::Dictionary(node)) = structure.objects.get(&id) { Some(node) } else { None };
    let parent = |held: &Dictionary| if let Some(Object::Reference(parent)) = held.as_hashmap().get(PARENT.as_bytes()) { Some(*parent) } else { None };
    let sheet = structure.get_dictionary(page)?;
    let mut visited = HashSet::from([page]);
    let ancestors = iter::successors(parent(sheet), |&id| node(id).and_then(parent)).take_while(|&id| visited.insert(id)).filter_map(node).collect::<Vec<_>>();
    Ok(INHERITED
        .into_iter()
        .filter(|key| !sheet.has(key.as_bytes()))
        .filter_map(|key| ancestors.iter().find_map(|held| held.as_hashmap().get(key.as_bytes())).map(|value| (key.as_bytes().to_vec(), value.clone())))
        .collect())
}

fn opens(assembly: &Assembly, explicit: &[Object]) -> bool { matches!(explicit.first(), Some(Object::Reference(page)) if assembly.held.contains(page)) }

fn directory(assembly: &Assembly, catalog: &Dictionary) -> Result<Directory, lopdf::Error> {
    let structure = &assembly.structure;
    let name = |key: &Object| key.as_str().map(<[u8]>::to_vec);
    let mut trees = entry(structure, catalog, NAMES)?
        .map(Object::as_dict)
        .transpose()?
        .into_iter()
        .flat_map(Dictionary::iter)
        .map(|(kind, root)| resolved(structure, root).and_then(Object::as_dict).and_then(|root| crate::leaves(structure, root, NAMES, &name, &[])).map(|found| (kind.clone(), found.into_iter().collect::<BTreeMap<_, _>>())))
        .try_collect::<BTreeMap<_, _>>()?;
    let dests = trees.entry(DESTS.as_bytes().to_vec()).or_default();
    for (key, value) in entry(structure, catalog, DESTS)?.map(Object::as_dict).transpose()?.into_iter().flat_map(Dictionary::iter) {
        dests.entry(key.clone()).or_insert_with(|| value.clone());
    }
    Ok(trees)
}

fn explicit(assembly: &Assembly, value: &Object) -> Result<Option<Vec<Object>>, lopdf::Error> {
    Ok(match resolved(&assembly.structure, value)? {
        Object::Array(explicit) => Some(explicit.clone()),
        Object::Dictionary(action) => entry(&assembly.structure, action, "D")?.map(Object::as_array).transpose()?.cloned(),
        Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Stream(_) | Object::Reference(_) => None,
    })
}

fn opening(assembly: &Assembly, catalog: &Dictionary) -> Result<bool, lopdf::Error> {
    Ok(match entry(&assembly.structure, catalog, OPEN_ACTION)? {
        None => true,
        Some(Object::Array(explicit)) => opens(assembly, explicit),
        Some(Object::Dictionary(action)) => match entry(&assembly.structure, action, "D")? {
            Some(Object::Array(explicit)) => opens(assembly, explicit),
            Some(_) | None => true,
        },
        Some(_) => false,
    })
}

fn annotations(assembly: &Assembly, page: ObjectId) -> Result<Vec<ObjectId>, lopdf::Error> {
    Ok(entry(&assembly.structure, assembly.structure.get_dictionary(page)?, ANNOTS)?
        .map(Object::as_array)
        .transpose()?
        .into_iter()
        .flatten()
        .filter_map(|element| if let Object::Reference(id) = element { Some(*id) } else { None })
        .collect())
}

fn linked(assembly: &mut Assembly, page: ObjectId, dests: &BTreeMap<Vec<u8>, Vec<Object>>) -> Result<(), lopdf::Error> {
    let Some(listed) = entry(&assembly.structure, assembly.structure.get_dictionary(page)?, ANNOTS)?.map(Object::as_array).transpose()?.cloned() else {
        return Ok(());
    };
    let verdicts = listed
        .into_iter()
        .map(|element| {
            let verdict = match resolved(&assembly.structure, &element)? {
                Object::Dictionary(annotation) => link(assembly, annotation, dests)?,
                Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Array(_) | Object::Stream(_) | Object::Reference(_) => Link::Dropped,
            };
            Ok::<_, lopdf::Error>((element, verdict))
        })
        .try_collect::<Vec<_>>()?;
    let edited = |target: &mut lopdf::Document, element: Object, edit: &dyn Fn(&mut Dictionary)| match element {
        Object::Reference(id) => target.get_dictionary_mut(id).map(edit).map(|()| Object::Reference(id)),
        Object::Dictionary(mut annotation) => {
            edit(&mut annotation);
            Ok(Object::Dictionary(annotation))
        }
        other @ (Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Array(_) | Object::Stream(_)) => Ok(other),
    };
    let retained = verdicts
        .into_iter()
        .filter_map(|(element, verdict)| match verdict {
            Link::Kept => Some(Ok(element)),
            Link::Redirected(explicit) => Some(edited(&mut assembly.structure, element, &|annotation| {
                annotation.remove(b"A");
                annotation.set("Dest", explicit.clone());
            })),
            Link::Unlinked => Some(edited(&mut assembly.structure, element, &|annotation| {
                annotation.remove(b"A");
                annotation.remove(b"Dest");
            })),
            Link::Dropped => None,
        })
        .try_collect::<Vec<_>>()?;
    assembly.structure.get_dictionary_mut(page).map(|sheet| sheet.set(ANNOTS, retained))
}

fn link(assembly: &Assembly, annotation: &Dictionary, dests: &BTreeMap<Vec<u8>, Vec<Object>>) -> Result<Link, lopdf::Error> {
    let structure = &assembly.structure;
    let action = entry(structure, annotation, "A")?.map(Object::as_dict).transpose()?;
    let destination = match (entry(structure, annotation, "Dest")?, action) {
        (Some(destination), _) => Some(destination),
        (None, Some(action)) if matches!(entry(structure, action, "S")?, Some(Object::Name(kind)) if kind == b"GoTo") => entry(structure, action, "D")?,
        (None, Some(_) | None) => None,
    };
    let dead = if matches!(entry(structure, annotation, "Subtype")?, Some(Object::Name(subtype)) if subtype == b"Link") { Link::Dropped } else { Link::Unlinked };
    Ok(match destination {
        None => Link::Kept,
        Some(Object::Array(explicit)) if opens(assembly, explicit) => Link::Kept,
        Some(Object::Name(name) | Object::String(name, _)) => dests.get(name).filter(|explicit| opens(assembly, explicit)).map_or(dead, |explicit| Link::Redirected(explicit.clone())),
        Some(_) => dead,
    })
}

fn retained(assembly: &mut Assembly, node: ObjectId, widgets: &HashSet<ObjectId>, ancestors: &[ObjectId]) -> Result<bool, lopdf::Error> {
    (!ancestors.contains(&node)).ok_or(lopdf::Error::ReferenceCycle(node))?;
    let Some(kids) = entry(&assembly.structure, assembly.structure.get_dictionary(node)?, KIDS)?.map(Object::as_array).transpose()?.cloned() else {
        return Ok(widgets.contains(&node));
    };
    let lineage = [ancestors, &[node]].concat();
    let kept = kids.into_iter().map(|kid| kid.as_reference().and_then(|id| retained(assembly, id, widgets, &lineage)).map(|keep| keep.then_some(kid))).try_collect::<Vec<_>>()?.into_iter().flatten().collect::<Vec<_>>();
    let any = !kept.is_empty();
    assembly.structure.get_dictionary_mut(node)?.set(KIDS, kept);
    Ok(any || widgets.contains(&node))
}

fn tagging(assembly: &mut Assembly, catalog: &Dictionary, sheets: &[(ObjectId, ObjectId)], base: i64, root: ObjectId) -> Result<Option<Tagging>, lopdf::Error> {
    const STRUCT_PARENT: &str = "StructParent";
    fn marked(structure: &mut lopdf::Document, held: &HashSet<ObjectId>, element: ObjectId, page: Option<bool>, ancestors: &[ObjectId]) -> Result<bool, lopdf::Error> {
        (!ancestors.contains(&element)).ok_or(lopdf::Error::ReferenceCycle(element))?;
        let placed = |holder: &Dictionary, inherited: Option<bool>| match holder.as_hashmap().get(b"Pg".as_slice()) {
            Some(Object::Reference(sheet)) => Some(held.contains(sheet)),
            Some(_) => Some(false),
            None => inherited,
        };
        let node = structure.get_dictionary(element)?;
        let here = placed(node, page);
        let Some(content) = entry(structure, node, "K")?.cloned() else {
            return Ok(here != Some(false));
        };
        let items = match content {
            Object::Array(items) => items,
            single @ (Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Dictionary(_) | Object::Stream(_) | Object::Reference(_)) => vec![single],
        };
        let lineage = [ancestors, &[element]].concat();
        let kept = items
            .into_iter()
            .map(|item| {
                let keep = match (&item, resolved(structure, &item)?) {
                    (Object::Integer(_), _) => here == Some(true),
                    (Object::Dictionary(reference), _) => placed(reference, here) == Some(true),
                    (Object::Reference(_), Object::Dictionary(reference)) if reference.has_type(b"MCR") || reference.has_type(b"OBJR") => placed(reference, here) == Some(true),
                    (Object::Reference(kid), Object::Dictionary(_)) => marked(structure, held, *kid, here, &lineage)?,
                    _ => false,
                };
                Ok::<_, lopdf::Error>(keep.then_some(item))
            })
            .try_collect::<Vec<_>>()?
            .into_iter()
            .flatten()
            .collect::<Vec<_>>();
        let any = !kept.is_empty();
        structure.get_dictionary_mut(element)?.set("K", kept);
        Ok(any)
    }
    let Some(tree) = entry(&assembly.structure, catalog, STRUCT_TREE_ROOT)?.map(Object::as_dict).transpose()?.cloned() else {
        return Ok(None);
    };
    let numbers = entry(&assembly.structure, &tree, "ParentTree")?
        .map(Object::as_dict)
        .transpose()?
        .map(|numbers| crate::leaves(&assembly.structure, numbers, "Nums", &Object::as_i64, &[]))
        .transpose()?
        .unwrap_or_default()
        .into_iter()
        .collect::<BTreeMap<_, _>>();
    let declared = entry(&assembly.structure, &tree, "ParentTreeNextKey")?.map(Object::as_i64).transpose()?;
    let span = iter::chain(numbers.keys().next_back().map(|key| key.saturating_add(1)), declared).max().unwrap_or_default();
    let originals = sheets.iter().filter(|(original, page)| original == page).map(|&(page, _)| page).collect::<Vec<_>>();
    let annotated = originals.iter().map(|&page| annotations(assembly, page)).try_collect::<Vec<_>>()?.concat();
    let forms = originals
        .iter()
        .map(|&page| entry(&assembly.structure, assembly.structure.get_dictionary(page)?, RESOURCES)?.map(Object::as_dict).transpose()?.map(|resources| nested(&assembly.structure, resources, BTreeSet::new())).transpose())
        .try_collect::<Vec<_>>()?
        .into_iter()
        .flatten()
        .flatten()
        .collect::<BTreeSet<_>>();
    let numbered = originals
        .iter()
        .map(|&id| (id, STRUCT_PARENTS))
        .chain(annotated.iter().map(|&id| (id, STRUCT_PARENT)))
        .chain(forms.iter().map(|&id| (id, STRUCT_PARENTS)))
        .map(|(id, key)| {
            let holder = match assembly.structure.get_object_mut(id)? {
                Object::Dictionary(holder) => Some(holder),
                Object::Stream(stream) => Some(&mut stream.dict),
                Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Array(_) | Object::Reference(_) => None,
            };
            Ok::<_, lopdf::Error>(holder.and_then(|holder| {
                let Some(&Object::Integer(own)) = holder.as_hashmap().get(key.as_bytes()) else {
                    return None;
                };
                let shifted = own.checked_add(base)?;
                holder.set(key, shifted);
                numbers.get(&own).map(|value| (shifted, value.clone()))
            }))
        })
        .try_collect::<Vec<_>>()?
        .into_iter()
        .flatten()
        .collect::<Vec<_>>();
    let kids = match entry(&assembly.structure, &tree, "K")?.cloned() {
        Some(Object::Array(kids)) => kids,
        Some(single) => vec![single],
        None => Vec::new(),
    };
    let tops = kids
        .into_iter()
        .filter_map(|kid| if let Object::Reference(element) = kid { Some(element) } else { None })
        .map(|element| marked(&mut assembly.structure, &assembly.held, element, None, &[]).map(|keep| keep.then_some(element)))
        .try_collect::<Vec<_>>()?
        .into_iter()
        .flatten()
        .collect::<Vec<_>>();
    tops.iter().try_for_each(|&element| assembly.structure.get_dictionary_mut(element).map(|node| node.set("P", root)))?;
    Ok(Some(Tagging { kids: tops.into_iter().map(Object::Reference).collect(), numbers: numbered, span, tree }))
}

fn pruned(assembly: &mut Assembly) {
    let live = reachable(&assembly.structure, assembly.structure.trailer.iter().flat_map(|(_, value)| referenced(value)).collect(), |_, _| true);
    assembly.structure.objects.retain(|id, _| live.contains(id));
}

fn reachable<F>(structure: &lopdf::Document, roots: Vec<ObjectId>, admits: F) -> HashSet<ObjectId>
where F: Fn(ObjectId, &Object) -> bool {
    let mut found = HashSet::new();
    let mut pending = roots;
    while let Some(id) = pending.pop() {
        if !found.contains(&id)
            && let Some(object) = structure.objects.get(&id)
            && admits(id, object)
        {
            found.insert(id);
            pending.extend(referenced(object));
        }
    }
    found
}

fn referenced(object: &Object) -> Vec<ObjectId> {
    match object {
        Object::Reference(id) => vec![*id],
        Object::Array(items) => items.iter().flat_map(referenced).collect(),
        Object::Dictionary(entries) => entries.iter().flat_map(|(_, value)| referenced(value)).collect(),
        Object::Stream(stream) => stream.dict.iter().flat_map(|(_, value)| referenced(value)).collect(),
        Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) => Vec::new(),
    }
}

fn rewritten<F>(object: &mut Object, replace: &F)
where F: Fn(ObjectId) -> Option<Object> {
    match object {
        Object::Reference(id) => {
            if let Some(replacement) = replace(*id) {
                *object = replacement;
            }
        }
        Object::Array(items) => items.iter_mut().for_each(|item| rewritten(item, replace)),
        Object::Dictionary(entries) => entries.iter_mut().for_each(|(_, value)| rewritten(value, replace)),
        Object::Stream(stream) => stream.dict.iter_mut().for_each(|(_, value)| rewritten(value, replace)),
        Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) => {}
    }
}

// --- [OVERLAY]

pub fn overlay(earlier: &Document, later: &Document, pairs: impl IntoIterator<Item = (usize, usize)>, colors: [OpaqueColor<Srgb>; 2]) -> Result<Document, PdfError> {
    let sized = |document: &Document, index: usize| {
        Validated::from(
            crate::page(document, index)
                .and_then(|sheet| {
                    let (width, height) = sheet.render_dimensions();
                    crate::extent(kurbo::Size::new(f64::from(width), f64::from(height)))
                })
                .map_err(|error| (Subject::Page(index), error)),
        )
    };
    let sheets = pairs
        .into_iter()
        .map(|(before, after)| sized(earlier, before).map2(sized(later, after), |first, second| ((before, first), (after, second))))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)?;
    let located = |part: usize| move |error: PdfError| (Subject::Part(part), error);
    let [first, second] = colors;
    let (before, after) = Validated::from(recolored(earlier, sheets.iter().map(|&((page, _), _)| page), first).map_err(located(0)))
        .map2(Validated::from(recolored(later, sheets.iter().map(|&(_, (page, _))| page), second).map_err(located(1))), |before, after| (before, after))
        .ok()
        .map_err(PdfError::Refused)?;
    let version = if earlier.version().max(later.version()) == hayro_syntax::PdfVersion::Pdf20 { PdfVersion::Pdf20 } else { PdfVersion::default() };
    let configuration = ConfigurationBuilder::new().with_version(version).finish().map_err(PdfError::Configuration)?;
    let mut canvas = krilla::Document::new_with(SerializeSettings { configuration, enable_tagging: false, ..SerializeSettings::default() });
    for ((earlier_page, earlier_size), (later_page, later_size)) in sheets {
        let paper = Size::from_wh(earlier_size.width().max(later_size.width()), earlier_size.height().max(later_size.height())).unwrap_or(later_size);
        let mut sheet = canvas.start_page_with(PageSettings::new(paper));
        let mut surface = sheet.surface();
        surface.draw_pdf_page(&before, earlier_size, earlier_page);
        surface.push_blend_mode(BlendMode::Multiply);
        surface.draw_pdf_page(&after, later_size, later_page);
        surface.pop();
        surface.finish();
        sheet.finish();
    }
    Document::new(canvas.finish()?, None)
}

fn recolored<I>(document: &Document, pages: I, color: OpaqueColor<Srgb>) -> Result<PdfDocument, PdfError>
where I: IntoIterator<Item = usize> {
    fn spaces<'held, I>(structure: &lopdf::Document, resources: I) -> Result<BTreeMap<Vec<u8>, Option<Mixing>>, lopdf::Error>
    where I: IntoIterator<Item = &'held Dictionary> {
        const COLOR_SPACE: &[u8] = b"ColorSpace";
        resources
            .into_iter()
            .filter(|held| held.has(COLOR_SPACE))
            .map(|held| structure.get_dict_in_dict(held, COLOR_SPACE))
            .try_collect::<Vec<_>>()?
            .into_iter()
            .flat_map(Dictionary::iter)
            .map(|(name, space)| mixing(structure, space).map(|mixing| (name.clone(), mixing)))
            .collect()
    }
    fn mixing(structure: &lopdf::Document, space: &Object) -> Result<Option<Mixing>, lopdf::Error> {
        match structure.dereference(space)?.1 {
            Object::Name(name) => Ok(device(name)),
            Object::Array(family) => match family.as_slice() {
                [Object::Name(name), profile] if name == b"ICCBased" => structure.dereference(profile)?.1.as_stream()?.dict.get(b"N")?.as_i64().map(|components| match components {
                    1 | 3 => Some(Mixing::Additive),
                    4 => Some(Mixing::Subtractive),
                    _ => None,
                }),
                [Object::Name(name), ..] => Ok(match name.as_slice() {
                    b"CalGray" | b"CalRGB" => Some(Mixing::Additive),
                    b"Separation" | b"DeviceN" => Some(Mixing::Subtractive),
                    _ => None,
                }),
                _ => Ok(None),
            },
            _ => Ok(None),
        }
    }
    let mut structure = crate::structure(document)?;
    let sheets = structure.page_iter().collect::<Vec<_>>();
    let selected = pages.into_iter().collect::<BTreeSet<_>>().into_iter().map(|index| sheets.get(index).copied().ok_or(PdfError::Page { index, count: sheets.len() })).try_collect::<Vec<_>>()?;
    let revision = color.components;
    let held = selected
        .iter()
        .map(|&sheet| {
            let (own, inherited) = structure.get_page_resources(sheet)?;
            inherited.into_iter().rev().map(|id| structure.get_dictionary(id)).chain(own.map(Ok)).try_collect::<Vec<_>>().map(|resources| (sheet, resources))
        })
        .try_collect::<Vec<_>>()?;
    let found = held.iter().flat_map(|(_, resources)| resources).try_fold(BTreeSet::new(), |found, resources| nested(&structure, resources, found))?;
    let painted = held
        .iter()
        .map(|(sheet, resources)| {
            structure
                .get_dictionary(*sheet)?
                .has(b"Contents")
                .then(|| {
                    spaces(&structure, resources.iter().copied()).and_then(|colorspaces| {
                        structure.get_and_decode_page_content(*sheet).and_then(|content| Content { operations: iter::chain([painted(FILL, revision), painted(STROKE, revision)], repainted(content.operations, &colorspaces, revision)).collect::<Vec<_>>() }.encode())
                    })
                })
                .transpose()
                .map(|content| content.map(|content| (Painted::Page(*sheet), content)))
        })
        .filter_map(Result::transpose)
        .chain(found.into_iter().map(|form| {
            let stream = structure.get_object(form)?.as_stream()?;
            let resources = stream.dict.has(RESOURCES.as_bytes()).then(|| structure.get_dict_in_dict(&stream.dict, RESOURCES.as_bytes())).transpose()?;
            spaces(&structure, resources).and_then(|colorspaces| {
                stream.decompressed_content().and_then(|bytes| Content::decode(&bytes)).and_then(|content| Content { operations: repainted(content.operations, &colorspaces, revision).collect::<Vec<_>>() }.encode())
            })
            .map(|content| (Painted::Form(form), content))
        }))
        .try_collect::<Vec<_>>()?;
    for (target, content) in painted {
        match target {
            Painted::Page(sheet) => structure.change_page_content(sheet, content)?,
            Painted::Form(form) => structure.get_object_mut(form)?.as_stream_mut()?.set_plain_content(content),
        }
    }
    crate::saved(structure).and_then(|document| crate::embedded(&document))
}

fn nested(structure: &lopdf::Document, resources: &Dictionary, found: BTreeSet<ObjectId>) -> Result<BTreeSet<ObjectId>, lopdf::Error> {
    const XOBJECT: &[u8] = b"XObject";
    if !resources.has(XOBJECT) {
        return Ok(found);
    }
    structure.get_dict_in_dict(resources, XOBJECT)?.iter().try_fold(found, |mut found, (_, entry)| match structure.dereference(entry)? {
        (Some(id), Object::Stream(form)) if !found.contains(&id) && matches!(form.dict.as_hashmap().get(b"Subtype".as_slice()), Some(Object::Name(subtype)) if subtype == b"Form") => {
            found.extend([id]);
            if form.dict.has(RESOURCES.as_bytes()) { nested(structure, structure.get_dict_in_dict(&form.dict, RESOURCES.as_bytes())?, found) } else { Ok(found) }
        }
        _ => Ok(found),
    })
}

const fn device(name: &[u8]) -> Option<Mixing> {
    match name {
        b"DeviceGray" | b"DeviceRGB" | b"G" | b"RGB" => Some(Mixing::Additive),
        b"DeviceCMYK" | b"CMYK" => Some(Mixing::Subtractive),
        _ => None,
    }
}

fn repainted(operations: Vec<Operation>, spaces: &BTreeMap<Vec<u8>, Option<Mixing>>, revision: [f32; 3]) -> impl Iterator<Item = Operation> {
    const PAPER: [f32; 3] = [1.0; 3];
    fn bare(operands: &[Object], mixing: Option<Mixing>) -> bool {
        !operands.is_empty()
            && operands.iter().all(|operand| match (mixing, operand) {
                (Some(Mixing::Additive), Object::Integer(tint)) => *tint >= 1,
                (Some(Mixing::Additive), Object::Real(tint)) => *tint >= 1.0,
                (Some(Mixing::Subtractive), Object::Integer(tint)) => *tint <= 0,
                (Some(Mixing::Subtractive), Object::Real(tint)) => *tint <= 0.0,
                (Some(Mixing::Additive | Mixing::Subtractive) | None, _) => false,
            })
    }
    let named = move |operands: &[Object]| match operands {
        [Object::Name(name)] => device(name).or_else(|| spaces.get(name).copied().flatten()),
        _ => None,
    };
    operations.into_iter().scan(NEVec::new([Some(Mixing::Additive); 2]), move |states, operation| {
        let [fill, stroke] = *states.last();
        let selected = match operation.operator.as_str() {
            "q" => {
                states.push([fill, stroke]);
                None
            }
            "Q" => {
                states.pop();
                None
            }
            "g" | FILL => Some((false, Some(Mixing::Additive))),
            "G" | STROKE => Some((true, Some(Mixing::Additive))),
            "k" => Some((false, Some(Mixing::Subtractive))),
            "K" => Some((true, Some(Mixing::Subtractive))),
            "cs" => Some((false, named(&operation.operands))),
            "CS" => Some((true, named(&operation.operands))),
            "sc" | "scn" => Some((false, fill)),
            "SC" | "SCN" => Some((true, stroke)),
            _ => None,
        };
        Some(match selected {
            Some((stroking, mixing)) => {
                let [fill_state, stroke_state] = states.last_mut();
                *(if stroking { stroke_state } else { fill_state }) = mixing;
                painted(if stroking { STROKE } else { FILL }, if bare(&operation.operands, mixing) { PAPER } else { revision })
            }
            None => operation,
        })
    })
}

fn painted(operator: &str, color: [f32; 3]) -> Operation { Operation::new(operator, color.map(Object::from).to_vec()) }
