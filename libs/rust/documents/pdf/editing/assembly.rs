use core::{borrow::Borrow, iter};
use std::collections::{BTreeMap, BTreeSet, HashMap, HashSet};

use lopdf::{Dictionary, Object, ObjectId, decode_text_string, dictionary};
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _};
use validated::Validated;

use crate::{
    Document, PdfError, Subject, entry,
    layers::{self, Group},
    navigation::{self, Navigation},
    resolved,
};

// --- [TYPES] ---------------------------------------------------------------------------

type Directory = BTreeMap<Vec<u8>, BTreeMap<Vec<u8>, Object>>;

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

// --- [OPERATIONS] ----------------------------------------------------------------------

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
    Validated::from(crate::structure(document)).map3(Validated::from(navigation::read(document)), Validated::from(layers::groups(document)), |structure, navigation, groups| Source {
        structure,
        navigation,
        groups,
    })
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
    let appearances = forms
        .iter()
        .flatten()
        .map(|form| entry(&assembly.structure, form, NEED_APPEARANCES).map(|needed| matches!(needed, Some(Object::Boolean(true)))))
        .try_fold(false, |any, needed| needed.map(|needed| any || needed))?;
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
        let numbers =
            tagged.iter().flat_map(|tagging| tagging.numbers.iter().cloned()).collect::<BTreeMap<_, _>>().into_iter().flat_map(|(key, value)| [Object::Integer(key), value]).collect::<Vec<_>>();
        let structure = dictionary! {
            "Type" => STRUCT_TREE_ROOT,
            "K" => tagged.iter().flat_map(|tagging| tagging.kids.iter().cloned()).collect::<Vec<_>>(),
            "ParentTree" => dictionary! { "Nums" => numbers },
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
        parts.iter().flat_map(|(source, _)| &source.groups).filter(|group| present.contains(&group.name)).rev().map(|group| (&*group.name, group)).collect::<BTreeMap<_, _>>().into_values(),
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
        .map(|(kind, root)| {
            resolved(structure, root)
                .and_then(Object::as_dict)
                .and_then(|root| crate::leaves(structure, root, NAMES, &name, &[]))
                .map(|found| (kind.clone(), found.into_iter().collect::<BTreeMap<_, _>>()))
        })
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
                Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Array(_) | Object::Stream(_) | Object::Reference(_) =>
                    Link::Dropped,
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
    let kept = kids
        .into_iter()
        .map(|kid| kid.as_reference().and_then(|id| retained(assembly, id, widgets, &lineage)).map(|keep| keep.then_some(kid)))
        .try_collect::<Vec<_>>()?
        .into_iter()
        .flatten()
        .collect::<Vec<_>>();
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
            single @ (Object::Null
            | Object::Boolean(_)
            | Object::Integer(_)
            | Object::Real(_)
            | Object::Name(_)
            | Object::String(_, _)
            | Object::Dictionary(_)
            | Object::Stream(_)
            | Object::Reference(_)) => vec![single],
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
        .map(|&page| {
            entry(&assembly.structure, assembly.structure.get_dictionary(page)?, RESOURCES)?
                .map(Object::as_dict)
                .transpose()?
                .map(|resources| super::forms(&assembly.structure, resources, BTreeSet::new()))
                .transpose()
        })
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
        Object::Reference(id) =>
            if let Some(replacement) = replace(*id) {
                *object = replacement;
            },
        Object::Array(items) => items.iter_mut().for_each(|item| rewritten(item, replace)),
        Object::Dictionary(entries) => entries.iter_mut().for_each(|(_, value)| rewritten(value, replace)),
        Object::Stream(stream) => stream.dict.iter_mut().for_each(|(_, value)| rewritten(value, replace)),
        Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) => {}
    }
}
