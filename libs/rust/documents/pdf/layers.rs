use core::borrow::Borrow;
use core::{convert, iter};
use std::collections::{HashMap, HashSet};
use std::sync::Arc;

use hayro_syntax::object::dict::keys::{AS, BASE_STATE, D, NAME, OC, OCG, OCGS, OCPROPERTIES, OFF, ON, ORDER, PRINT, PRINT_STATE, PROPERTIES, TYPE, USAGE, VIEW, VIEW_STATE};
use indexmap::IndexMap;
use indexmap::map::Entry;
use itertools::Either;
use lopdf::content::{Content, Operation};
use lopdf::{Dictionary, Object, ObjectId, decode_text_string, dictionary, text_string};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use pdf_oxide::compliance::{PdfAPart, PdfUaLevel};
use rasm_drawing::Visibility;
use validated::Validated;

use crate::{Document, PdfError, Refusal, Standard, Subject, conformance, entry};

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENT: &[u8] = b"Event";
const CATEGORY: &[u8] = b"Category";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct Group {
    pub name: Arc<str>,
    pub print: Visibility,
    pub view: Visibility,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn groups(document: &Document) -> Result<Vec<(ObjectId, Group)>, PdfError> { crate::structure(document).and_then(|structure| entries(&structure)) }

pub fn set<I>(document: &Document, application: &str, groups: I) -> Result<Document, PdfError>
where I: IntoIterator<Item: Borrow<(ObjectId, Group)>> {
    let held = self::groups(document)?;
    let requested = groups.into_iter().collect::<Vec<_>>();
    let present = held.iter().map(|&(id, _)| id).collect::<HashSet<_>>();
    requested.iter().map(Borrow::<(ObjectId, Group)>::borrow).find(|(id, _)| !present.contains(id)).map_or(Ok(()), |&(id, _)| Err(PdfError::Group(id)))?;
    if held.is_empty() {
        return Ok(document.clone());
    }
    let requested = requested.iter().map(Borrow::<(ObjectId, Group)>::borrow).map(|(id, group)| (*id, group)).collect::<HashMap<_, _>>();
    let ocgs = held.into_iter().map(|(id, stated)| (id, requested.get(&id).map_or(stated, |&group| group.clone()))).collect::<Vec<_>>();
    crate::revised(document, |structure| apply(structure, &ocgs, application))
}

pub(crate) fn apply(structure: &mut lopdf::Document, ocgs: &[(ObjectId, Group)], application: &str) -> Result<(), PdfError> {
    let forbids = forbids(structure)?;
    permitted(ocgs.iter().map(|(_, group)| group), forbids)?;
    ocgs.iter().try_for_each(|(id, group)| stamp(structure, *id, group))?;
    configure(structure, ocgs, forbids, application).map_err(PdfError::from)
}

pub(crate) fn bind(structure: &mut lopdf::Document, pages: &[Vec<Group>], application: &str) -> Result<(), PdfError> {
    let forbids = forbids(structure)?;
    permitted(pages.iter().flatten(), forbids)?;
    let mut registered = IndexMap::<&Group, ObjectId>::new();
    let bound = pages
        .iter()
        .map(|layers| {
            layers
                .iter()
                .map(|group| match registered.entry(group) {
                    Entry::Occupied(held) => Ok(*held.get()),
                    Entry::Vacant(slot) => structure.try_add_object(dictionary! { TYPE => Object::Name(OCG.to_vec()), NAME => text_string(&group.name) }).map(|id| *slot.insert(id)),
                })
                .try_collect::<Vec<_>>()
        })
        .try_collect::<Vec<_>>()?;
    let ocgs = registered.into_iter().map(|(group, id)| (id, group.clone())).collect::<Vec<_>>();
    ocgs.iter().try_for_each(|(id, group)| stamp(structure, *id, group))?;
    let resource = |id: ObjectId| format!("oc{}", id.0);
    let sheets = structure.page_iter().collect::<Vec<_>>();
    iter::zip(sheets, bound).filter(|(_, layers)| !layers.is_empty()).try_for_each(|(page, layers)| {
        let mut pending = layers.iter().copied().peekable();
        let operations = structure
            .get_and_decode_page_content(page)?
            .operations
            .into_iter()
            .flat_map(|operation| match pending.next_if(|_| operation.operator == "Do") {
                Some(id) => Either::Left([Operation::new("BDC", vec![Object::Name(OC.to_vec()), Object::from(resource(id))]), operation, Operation::new("EMC", Vec::new())].into_iter()),
                None => Either::Right(iter::once(operation)),
            })
            .collect::<Vec<_>>();
        structure.change_page_content(page, Content { operations }.encode()?)?;
        let properties = structure.get_or_create_resources(page)?.as_dict_mut()?.as_hashmap_mut().entry(PROPERTIES.to_vec()).or_insert_with(|| Object::Dictionary(Dictionary::new())).as_dict_mut()?;
        properties.as_hashmap_mut().extend(layers.iter().map(|&id| (resource(id).into_bytes(), Object::Reference(id))));
        Ok::<_, lopdf::Error>(())
    })?;
    configure(structure, &ocgs, forbids, application).map_err(PdfError::from)
}

fn forbids(structure: &lopdf::Document) -> Result<bool, PdfError> {
    Ok(conformance::declared(structure)?.0.iter().any(|standard| match standard {
        Standard::Archival(level) => {
            let (PdfAPart::Part1 | PdfAPart::Part2 | PdfAPart::Part3) = level.part();
            true
        }
        Standard::Accessibility(PdfUaLevel::Ua1 | PdfUaLevel::Ua2) => true,
        Standard::Press(_) => false,
    }))
}

fn permitted<'group>(groups: impl IntoIterator<Item = &'group Group>, forbids: bool) -> Result<(), PdfError> {
    groups.into_iter().find(|group| forbids && group.print != group.view).map_or(Ok(()), |group| Err(PdfError::Named { name: Arc::clone(&group.name), refusal: Refusal::Usage }))
}

pub(crate) fn entries(structure: &lopdf::Document) -> Result<Vec<(ObjectId, Group)>, PdfError> {
    let Some(properties) = entry(structure, structure.catalog()?, OCPROPERTIES)?.map(Object::as_dict).transpose()? else {
        return Ok(Vec::new());
    };
    let absent = Dictionary::new();
    let configuration = entry(structure, properties, D)?.map(Object::as_dict).transpose()?.unwrap_or(&absent);
    let references = |list: &Object| list.as_array().and_then(|items| items.iter().map(Object::as_reference).try_collect::<HashSet<_>>());
    let listed = |parent: &Dictionary, key: &[u8]| entry(structure, parent, key)?.map_or_else(|| Ok(HashSet::new()), references);
    let applications = applications(structure, configuration)?;
    let applied = |event: &[u8]| {
        applications
            .iter()
            .map(|application| {
                let governs = application.get_deref(EVENT, structure)?.as_name()? == event
                    && application.get_deref(CATEGORY, structure)?.as_array()?.iter().any(|category| category.as_name().is_ok_and(|name| name == event));
                if governs { listed(application, OCGS) } else { Ok(HashSet::new()) }
            })
            .try_collect::<Vec<_>>()
            .map(|lists| lists.into_iter().flatten().collect::<HashSet<_>>())
    };
    let (on, off, viewed, printed) = Validated::from(listed(configuration, ON))
        .map4(Validated::from(listed(configuration, OFF)), Validated::from(applied(VIEW)), Validated::from(applied(PRINT)), |on, off, viewed, printed| (on, off, viewed, printed))
        .ok()
        .map_err(|errors| PdfError::Refused(errors.into_nonempty_iter().map(|error| (Subject::Configuration, PdfError::from(error))).collect()))?;
    let based_off = entry(structure, configuration, BASE_STATE)?.map(Object::as_name).transpose()? == Some(OFF);
    let visibility = |shown: bool| if shown { Visibility::Visible } else { Visibility::Hidden };
    let stated = |ocg: &Dictionary, applying: bool, category: &[u8], key: &[u8]| -> Result<Option<Visibility>, lopdf::Error> {
        Ok(if applying {
            crate::nested(structure, ocg, &[USAGE, category])?.map(|usage| entry(structure, usage, key)).transpose()?.flatten().map(Object::as_name).transpose()?.map(|state| visibility(state != OFF))
        } else {
            None
        })
    };
    entry(structure, properties, OCGS)?
        .map_or(Ok(&[][..]), |list| list.as_array().map(Vec::as_slice))?
        .iter()
        .enumerate()
        .map(|(index, reference)| {
            let located = |errors: NEVec<lopdf::Error>| errors.into_nonempty_iter().map(|error| (Subject::Group(index), PdfError::from(error))).collect::<NEVec<_>>();
            let id = reference.as_reference().map_err(|error| located(NEVec::new(error)))?;
            let ocg = structure.get_dictionary(id).map_err(|error| located(NEVec::new(error)))?;
            Validated::from(ocg.get_deref(NAME, structure).and_then(decode_text_string))
                .map3(Validated::from(stated(ocg, viewed.contains(&id), VIEW, VIEW_STATE)), Validated::from(stated(ocg, printed.contains(&id), PRINT, PRINT_STATE)), |name, view, print| {
                    let view = view.unwrap_or_else(|| visibility(if based_off { on.contains(&id) } else { !off.contains(&id) }));
                    (id, Group { name: Arc::from(name), print: print.unwrap_or(view), view })
                })
                .ok()
                .map_err(located)
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|errors| PdfError::Refused(errors.into_nonempty_iter().flat_map(convert::identity).collect()))
}

fn stamp(structure: &mut lopdf::Document, id: ObjectId, group: &Group) -> Result<(), lopdf::Error> {
    let state = |visibility: Visibility| match visibility {
        Visibility::Visible => Object::Name(ON.to_vec()),
        Visibility::Hidden => Object::Name(OFF.to_vec()),
    };
    let usage = [(PRINT, PRINT_STATE, group.print), (VIEW, VIEW_STATE, group.view)].into_iter().try_fold(
        subdictionary(structure, structure.get_dictionary(id)?, USAGE)?,
        |mut usage, (category, key, visibility)| {
            let mut entry = subdictionary(structure, &usage, category)?;
            entry.set(key, state(visibility));
            usage.set(category, entry);
            Ok::<_, lopdf::Error>(usage)
        },
    )?;
    structure.get_dictionary_mut(id).map(|ocg| {
        ocg.set(NAME, text_string(&group.name));
        ocg.set(USAGE, usage);
    })
}

fn configure(structure: &mut lopdf::Document, ocgs: &[(ObjectId, Group)], forbids: bool, application: &str) -> Result<(), lopdf::Error> {
    let all = ocgs.iter().map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>();
    let shown = |visibility: Visibility| ocgs.iter().filter(|(_, group)| group.view == visibility).map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>();
    let mut properties = subdictionary(structure, structure.catalog()?, OCPROPERTIES)?;
    let mut configuration = subdictionary(structure, &properties, D)?;
    let foreign = if forbids {
        Vec::new()
    } else {
        applications(structure, &configuration)?
            .into_iter()
            .map(|usage| {
                let event = usage.get_deref(EVENT, structure)?.as_name()?;
                let managed = |category: &Object| [VIEW, PRINT].contains(&event) && category.as_name().is_ok_and(|name| name == event);
                let categories = usage.get_deref(CATEGORY, structure)?.as_array()?.iter().filter(|category| !managed(category)).cloned().collect::<Vec<_>>();
                Ok::<_, lopdf::Error>((!categories.is_empty()).then(|| {
                    let mut kept = usage.clone();
                    kept.set(CATEGORY, categories);
                    Object::Dictionary(kept)
                }))
            })
            .try_collect::<Vec<_>>()?
    };
    let ours = ocgs
        .iter()
        .any(|(_, group)| group.print != group.view)
        .then(|| [VIEW, PRINT].map(|event| Object::from(dictionary! { EVENT => Object::Name(event.to_vec()), CATEGORY => vec![Object::Name(event.to_vec())], OCGS => all.clone() })));
    let usage = foreign.into_iter().flatten().chain(ours.into_iter().flatten()).collect::<Vec<_>>();
    configuration.set(ON, shown(Visibility::Visible));
    configuration.set(OFF, shown(Visibility::Hidden));
    configuration.as_hashmap_mut().entry(NAME.to_vec()).or_insert_with(|| text_string(application));
    configuration.as_hashmap_mut().entry(ORDER.to_vec()).or_insert_with(|| Object::Array(all.clone()));
    if usage.is_empty() {
        configuration.remove(AS);
    } else {
        configuration.set(AS, usage);
    }
    properties.set(OCGS, all);
    properties.set(D, configuration);
    structure.catalog_mut().map(|catalog| catalog.set(OCPROPERTIES, properties))
}

fn applications<'structure>(structure: &'structure lopdf::Document, configuration: &'structure Dictionary) -> Result<Vec<&'structure Dictionary>, lopdf::Error> {
    entry(structure, configuration, AS)?.map_or(Ok(Vec::new()), |list| list.as_array()?.iter().map(|listed| crate::resolved(structure, listed).and_then(Object::as_dict)).try_collect::<Vec<_>>())
}

fn subdictionary(structure: &lopdf::Document, parent: &Dictionary, key: &[u8]) -> Result<Dictionary, lopdf::Error> {
    crate::nested(structure, parent, &[key]).map(|found| found.cloned().unwrap_or_default())
}
