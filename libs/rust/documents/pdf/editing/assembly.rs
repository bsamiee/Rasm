use core::{borrow::Borrow, iter, mem};
use std::collections::{BTreeMap, BTreeSet, HashMap, HashSet};

use itertools::Either;
use lopdf::{Dictionary, Object, ObjectId, decode_text_string, dictionary};
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _};
use rasm_linework::Visibility;
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
const KIDS: &str = "Kids";
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
    groups: Vec<(ObjectId, Group)>,
}

struct Placement {
    trailer: Dictionary,
    pages: Vec<ObjectId>,
    groups: Vec<(ObjectId, Group)>,
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
    let structure = crate::structure(document).and_then(|structure| layers::entries(&structure).map(|groups| (structure, groups)));
    Validated::from(structure).map2(Validated::from(navigation::read(document)), |(structure, groups), navigation| Source { structure, navigation, groups })
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
    const DESTS: &str = "Dests";
    const FIELDS: &str = "Fields";
    const NAMES: &str = "Names";
    const NEED_APPEARANCES: &str = "NeedAppearances";
    const OPEN_ACTION: &str = "OpenAction";
    const OPTIONAL_CONTENT: &str = "OCProperties";
    let opens = |assembly: &Assembly, explicit: &[Object]| matches!(explicit.first(), Some(Object::Reference(page)) if assembly.held.contains(page));
    let linked = |assembly: &mut Assembly, page: ObjectId, dests: &BTreeMap<Vec<u8>, Vec<Object>>| -> Result<(), lopdf::Error> {
        let Some(listed) = entry(&assembly.structure, assembly.structure.get_dictionary(page)?, ANNOTS)?.map(Object::as_array).transpose()?.cloned() else {
            return Ok(());
        };
        let link = |annotation: &Dictionary| -> Result<Link, lopdf::Error> {
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
        };
        let verdicts = listed
            .into_iter()
            .map(|element| {
                let verdict = match resolved(&assembly.structure, &element)? {
                    Object::Dictionary(annotation) => link(annotation)?,
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
    };
    let mut merged = lopdf::Document::with_version(parts.nonempty_iter().map(|(source, _)| source.structure.version.as_str()).max());
    let placed = parts.nonempty_iter().map(|&(source, chosen)| placed(source, &mut merged, chosen)).collect::<Result<NEVec<_>, _>>()?;
    let roots = placed.nonempty_iter().map(|placement| placement.trailer.get(b"Root").and_then(Object::as_reference)).collect::<Result<NEVec<_>, _>>()?;
    let catalogs = roots.nonempty_iter().map(|&root| merged.get_dictionary(root).cloned()).collect::<Result<NEVec<_>, _>>()?;
    let mut seen = HashSet::new();
    let kept = placed
        .iter()
        .map(|placement| {
            placement
                .pages
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
        Object::Dictionary(dictionary! { "Type" => "Pages", KIDS => pages.iter().copied().map(Object::Reference).collect::<Vec<_>>(), "Count" => pages.len().saturating_cast::<i64>() }),
    );
    let directory = |catalog: &Dictionary| -> Result<Directory, lopdf::Error> {
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
                    .and_then(|root| crate::leaves(structure, root, NAMES, &name))
                    .map(|found| (kind.clone(), found.into_iter().collect::<BTreeMap<_, _>>()))
            })
            .try_collect::<BTreeMap<_, _>>()?;
        let dests = trees.entry(DESTS.as_bytes().to_vec()).or_default();
        for (key, value) in entry(structure, catalog, DESTS)?.map(Object::as_dict).transpose()?.into_iter().flat_map(Dictionary::iter) {
            dests.entry(key.clone()).or_insert_with(|| value.clone());
        }
        Ok(trees)
    };
    let trees = catalogs.iter().map(directory).try_collect::<Vec<_>>()?;
    let explicit = |value: &Object| -> Result<Option<Vec<Object>>, lopdf::Error> {
        Ok(match resolved(&assembly.structure, value)? {
            Object::Array(explicit) => Some(explicit.clone()),
            Object::Dictionary(action) => entry(&assembly.structure, action, "D")?.map(Object::as_array).transpose()?.cloned(),
            Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) | Object::Stream(_) | Object::Reference(_) => None,
        })
    };
    let destinations = trees
        .iter()
        .map(|named| {
            named
                .get(DESTS.as_bytes())
                .into_iter()
                .flatten()
                .map(|(name, value)| explicit(value).map(|target| target.map(|target| (name.clone(), target))))
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
        .map(|field| retained(&mut assembly, field, &widgets).map(|keep| keep.then_some(Object::Reference(field))))
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
        tagging(&mut assembly, catalog, sheets, base, structure_root).and_then(|tagging| {
            let next = tagging.as_ref().map_or(Ok(base), |tagging| i64::try_from(i128::from(base).strict_add(i128::from(tagging.span))))?;
            tagged.extend(tagging);
            Ok((next, tagged))
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
    let opening = match entry(&assembly.structure, &catalog, OPEN_ACTION)? {
        None => true,
        Some(Object::Array(explicit)) => opens(&assembly, explicit),
        Some(Object::Dictionary(action)) => match entry(&assembly.structure, action, "D")? {
            Some(Object::Array(explicit)) => opens(&assembly, explicit),
            Some(_) | None => true,
        },
        Some(_) => false,
    };
    if !opening {
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
    let mut trailer = placed.first().trailer.clone();
    for key in ["Prev", "XRefStm", "Encrypt"] {
        trailer.remove(key.as_bytes());
    }
    assembly.structure.trailer = trailer;
    {
        let live = reachable(&assembly.structure, assembly.structure.trailer.iter().flat_map(|(_, value)| referenced(value)).collect(), |_, _| true);
        assembly.structure.objects.retain(|id, _| live.contains(id));
    };
    let bindings = placed.iter().flat_map(|placement| &placement.groups).map(|(id, group)| (*id, group)).collect::<HashMap<_, _>>();
    let registered = assembly
        .structure
        .objects
        .iter()
        .filter_map(|(&id, object)| if let Object::Dictionary(node) = object { node.has_type(b"OCG").then_some((id, node)) } else { None })
        .map(|(id, node)| {
            bindings.get(&id).map_or_else(
                || node.get_deref(b"Name", &assembly.structure).and_then(decode_text_string).map(|name| (id, Group { name: name.into(), print: Visibility::Visible, view: Visibility::Visible })),
                |&group| Ok((id, group.clone())),
            )
        })
        .try_collect::<Vec<_>>()?;
    if !registered.is_empty() {
        assembly
            .structure
            .get_dictionary_mut(root)?
            .set(OPTIONAL_CONTENT, dictionary! { "OCGs" => registered.iter().map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>(), "D" => Dictionary::new() });
    }
    let indices = parts.iter().map(|(_, chosen)| chosen.iter().map(|&(index, _)| index).collect::<Vec<_>>()).collect::<Vec<_>>();
    navigation::write(&mut assembly.structure, &navigation::assembled(iter::zip(parts, &indices).map(|((source, _), sheets)| (&source.navigation, sheets.as_slice()))))?;
    if !registered.is_empty() {
        layers::apply(&mut assembly.structure, &registered)?;
    }
    crate::saved(assembly.structure)
}

fn placed(source: &Source, merged: &mut lopdf::Document, chosen: &[(usize, ObjectId)]) -> Result<Placement, lopdf::Error> {
    let structure = &source.structure;
    let inherited = |page: ObjectId| -> Result<Vec<(Vec<u8>, Object)>, lopdf::Error> {
        const INHERITED: [&str; 4] = [RESOURCES, "MediaBox", "CropBox", "Rotate"];
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
    };
    let sheets = structure.page_iter().collect::<HashSet<_>>();
    let selected = chosen.iter().map(|&(_, page)| page).collect::<BTreeSet<_>>();
    let inheritance = selected.iter().map(|&page| inherited(page).map(|held| (page, held))).try_collect::<Vec<_>>()?;
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
    let shift = |root: &mut Object| {
        let mut pending = vec![root];
        while let Some(object) = pending.pop() {
            match object {
                Object::Reference(id) => *object = if admitted.contains(id) { Object::Reference(moved(*id)) } else { Object::Null },
                Object::Array(items) => pending.extend(items.iter_mut().rev()),
                Object::Dictionary(entries) => pending.extend(entries.iter_mut().rev().map(|(_, value)| value)),
                Object::Stream(stream) => pending.extend(stream.dict.iter_mut().rev().map(|(_, value)| value)),
                Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) => {}
            }
        }
    };
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
    Ok(Placement {
        trailer,
        pages: chosen.iter().map(|&(_, page)| moved(page)).collect(),
        groups: source.groups.iter().filter(|(id, _)| admitted.contains(id)).map(|(id, group)| (moved(*id), group.clone())).collect(),
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

fn retained(assembly: &mut Assembly, root: ObjectId, widgets: &HashSet<ObjectId>) -> Result<bool, lopdf::Error> {
    let children = |document: &lopdf::Document, id| entry(document, document.get_dictionary(id)?, KIDS)?.map(Object::as_array).transpose().map(Option::<&Vec<Object>>::cloned);
    let Some(kids) = children(&assembly.structure, root)? else {
        return Ok(widgets.contains(&root));
    };
    let mut pending = NEVec::new((root, kids.into_iter(), Vec::new()));
    let mut active = HashSet::from([root]);
    loop {
        let current = pending.last_mut();
        let Some(kid) = current.1.next() else {
            let keep = !current.2.is_empty() || widgets.contains(&current.0);
            assembly.structure.get_dictionary_mut(current.0)?.set(KIDS, mem::take(&mut current.2));
            active.remove(&current.0);
            let Some((node, _, _)) = pending.pop() else {
                return Ok(keep);
            };
            pending.last_mut().2.extend(keep.then_some(Object::Reference(node)));
            continue;
        };
        let node = kid.as_reference()?;
        active.insert(node).ok_or(lopdf::Error::ReferenceCycle(node))?;
        if let Some(descendants) = children(&assembly.structure, node)? {
            pending.push((node, descendants.into_iter(), Vec::new()));
        } else {
            active.remove(&node);
            current.2.extend(widgets.contains(&node).then_some(kid));
        }
    }
}

fn tagging(assembly: &mut Assembly, catalog: &Dictionary, sheets: &[(ObjectId, ObjectId)], base: i64, root: ObjectId) -> Result<Option<Tagging>, lopdf::Error> {
    const STRUCT_PARENT: &str = "StructParent";
    fn marked(structure: &mut lopdf::Document, held: &HashSet<ObjectId>, root: ObjectId) -> Result<bool, lopdf::Error> {
        let placed = |holder: &Dictionary, inherited: Option<bool>| match holder.as_hashmap().get(b"Pg".as_slice()) {
            Some(Object::Reference(sheet)) => Some(held.contains(sheet)),
            Some(_) => Some(false),
            None => inherited,
        };
        let children = |document: &lopdf::Document, node: &Dictionary, page| {
            entry(document, node, "K").map(|content| {
                (
                    placed(node, page),
                    content.cloned().map(|content| match content {
                        Object::Array(items) => Either::Left(items.into_iter()),
                        single @ (Object::Null
                        | Object::Boolean(_)
                        | Object::Integer(_)
                        | Object::Real(_)
                        | Object::Name(_)
                        | Object::String(_, _)
                        | Object::Dictionary(_)
                        | Object::Stream(_)
                        | Object::Reference(_)) => Either::Right(iter::once(single)),
                    }),
                )
            })
        };
        let (here, items) = children(structure, structure.get_dictionary(root)?, None)?;
        let Some(items) = items else {
            return Ok(here != Some(false));
        };
        let mut pending = NEVec::new((root, here, items, Vec::new()));
        let mut active = HashSet::from([root]);
        loop {
            let current = pending.last_mut();
            let Some(item) = current.2.next() else {
                let keep = !current.3.is_empty();
                structure.get_dictionary_mut(current.0)?.set("K", mem::take(&mut current.3));
                active.remove(&current.0);
                let Some((element, _, _, _)) = pending.pop() else {
                    return Ok(keep);
                };
                pending.last_mut().3.extend(keep.then_some(Object::Reference(element)));
                continue;
            };
            let keep = match (&item, resolved(structure, &item)?) {
                (Object::Integer(_), _) => current.1 == Some(true),
                (Object::Dictionary(reference), _) => placed(reference, current.1) == Some(true),
                (Object::Reference(_), Object::Dictionary(reference)) if reference.has_type(b"MCR") || reference.has_type(b"OBJR") => placed(reference, current.1) == Some(true),
                (Object::Reference(element), Object::Dictionary(reference)) => {
                    active.insert(*element).ok_or(lopdf::Error::ReferenceCycle(*element))?;
                    match children(structure, reference, current.1)? {
                        (page, Some(descendants)) => pending.push((*element, page, descendants, Vec::new())),
                        (page, None) => {
                            active.remove(element);
                            current.3.extend((page != Some(false)).then_some(item));
                        }
                    }
                    continue;
                }
                _ => false,
            };
            current.3.extend(keep.then_some(item));
        }
    }
    let Some(tree) = entry(&assembly.structure, catalog, STRUCT_TREE_ROOT)?.map(Object::as_dict).transpose()?.cloned() else {
        return Ok(None);
    };
    let numbers = entry(&assembly.structure, &tree, "ParentTree")?
        .map(Object::as_dict)
        .transpose()?
        .map(|numbers| crate::leaves(&assembly.structure, numbers, "Nums", &Object::as_i64))
        .transpose()?
        .unwrap_or_default()
        .into_iter()
        .collect::<BTreeMap<_, _>>();
    let declared = entry(&assembly.structure, &tree, "ParentTreeNextKey")?.map(Object::as_i64).transpose()?;
    let span = iter::chain(numbers.keys().next_back().map(|&key| i64::try_from(i128::from(key).strict_add(1))).transpose()?, declared).max().unwrap_or_default();
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
            let Some(holder) = holder else {
                return Ok(None);
            };
            let Some(own) = holder.as_hashmap().get(key.as_bytes()).map(Object::as_i64).transpose()? else {
                return Ok(None);
            };
            let shifted = i64::try_from(i128::from(own).strict_add(i128::from(base)))?;
            holder.set(key, shifted);
            Ok::<_, lopdf::Error>(numbers.get(&own).map(|value| (shifted, value.clone())))
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
        .map(|element| marked(&mut assembly.structure, &assembly.held, element).map(|keep| keep.then_some(element)))
        .try_collect::<Vec<_>>()?
        .into_iter()
        .flatten()
        .collect::<Vec<_>>();
    tops.iter().try_for_each(|&element| assembly.structure.get_dictionary_mut(element).map(|node| node.set("P", root)))?;
    Ok(Some(Tagging { kids: tops.into_iter().map(Object::Reference).collect(), numbers: numbered, span, tree }))
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

gen fn referenced(root: &Object) -> ObjectId {
    let mut pending = vec![root];
    while let Some(object) = pending.pop() {
        match object {
            Object::Reference(id) => yield *id,
            Object::Array(items) => pending.extend(items.iter().rev()),
            Object::Dictionary(entries) => pending.extend(entries.iter().rev().map(|(_, value)| value)),
            Object::Stream(stream) => pending.extend(stream.dict.iter().rev().map(|(_, value)| value)),
            Object::Null | Object::Boolean(_) | Object::Integer(_) | Object::Real(_) | Object::Name(_) | Object::String(_, _) => {}
        }
    }
}
