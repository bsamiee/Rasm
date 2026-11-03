use std::{
    borrow::Borrow,
    collections::{BTreeMap, HashMap, HashSet},
    sync::Arc,
};

use lopdf::{
    Dictionary, Object, ObjectId,
    content::{Content, Operation},
    decode_text_string, dictionary, text_string,
};
use rasm_linework::Visibility;

use crate::{Document, PdfError, Standard, conformance, entry};

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROPERTIES: &str = "OCProperties";
const GROUPS: &str = "OCGs";
const CONFIGURATION: &str = "D";
const APPLICATIONS: &str = "AS";
const EVENT: &str = "Event";
const CATEGORY: &str = "Category";
const NAME: &str = "Name";
const USAGE: &str = "Usage";
const PRINT: &str = "Print";
const VIEW: &str = "View";
const PRINT_STATE: &str = "PrintState";
const VIEW_STATE: &str = "ViewState";
const ON: &str = "ON";
const OFF: &str = "OFF";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct Group {
    pub name: Arc<str>,
    pub print: Visibility,
    pub view: Visibility,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn groups(document: &Document) -> Result<Vec<Group>, PdfError> {
    crate::structure(document).and_then(|structure| entries(&structure).map_err(PdfError::from)).map(|held| held.into_iter().map(|(_, group)| group).collect())
}

pub fn set<I>(document: &Document, groups: I) -> Result<Document, PdfError>
where I: IntoIterator<Item: Borrow<Group>> {
    let mut structure = crate::structure(document)?;
    let held = entries(&structure)?;
    let requested = groups.into_iter().collect::<Vec<_>>();
    let wanted = requested.iter().map(Borrow::<Group>::borrow).collect::<Vec<_>>();
    let present = held.iter().map(|(_, group)| &*group.name).collect::<HashSet<_>>();
    wanted.iter().find(|group| !present.contains(&*group.name)).map_or(Ok(()), |missing| Err(PdfError::Group(Arc::clone(&missing.name))))?;
    if held.is_empty() {
        return Ok(document.clone());
    }
    let named = wanted.iter().map(|&group| (&*group.name, group)).collect::<HashMap<_, _>>();
    let ocgs = held.into_iter().map(|(id, present)| (id, named.get(&*present.name).map_or(present, |&group| group.clone()))).collect::<Vec<_>>();
    let forbids = conformance::declared(&structure)?.iter().any(|claim| matches!(claim, Ok(Standard::Archival(_) | Standard::Accessibility(_))));
    permitted(ocgs.iter().map(|(_, group)| group), forbids)?;
    ocgs.iter().try_for_each(|(id, group)| stamp(&mut structure, *id, group))?;
    configure(&mut structure, &ocgs, forbids)?;
    crate::saved(structure)
}

pub(crate) fn bind(structure: &mut lopdf::Document, pages: &[Vec<Group>], forbids: bool) -> Result<(), PdfError> {
    permitted(pages.iter().flatten(), forbids)?;
    let mut registered = HashMap::<&Group, ObjectId>::new();
    let bound = pages
        .iter()
        .map(|layers| {
            layers.iter().map(|group| *registered.entry(group).or_insert_with(|| structure.add_object(dictionary! { "Type" => "OCG", NAME => text_string(&group.name) }))).collect::<Vec<_>>()
        })
        .collect::<Vec<_>>();
    let ocgs = registered.into_iter().map(|(group, id)| (id, group.clone())).collect::<BTreeMap<_, _>>().into_iter().collect::<Vec<_>>();
    ocgs.iter().try_for_each(|(id, group)| stamp(structure, *id, group))?;
    let resource = |id: ObjectId| format!("oc{}", id.0);
    structure.page_iter().collect::<Vec<_>>().into_iter().zip(bound).filter(|(_, layers)| !layers.is_empty()).try_for_each(|(page, layers)| {
        let mut pending = layers.iter().copied().peekable();
        let operations = structure
            .get_and_decode_page_content(page)?
            .operations
            .into_iter()
            .flat_map(|operation| match pending.next_if(|_| operation.operator == "Do") {
                Some(id) => vec![Operation::new("BDC", vec![Object::from("OC"), Object::from(resource(id))]), operation, Operation::new("EMC", Vec::new())],
                None => vec![operation],
            })
            .collect::<Vec<_>>();
        structure.change_page_content(page, Content { operations }.encode()?)?;
        let properties =
            structure.get_or_create_resources(page)?.as_dict_mut()?.as_hashmap_mut().entry(b"Properties".to_vec()).or_insert_with(|| Object::Dictionary(Dictionary::new())).as_dict_mut()?;
        properties.as_hashmap_mut().extend(layers.iter().map(|&id| (resource(id).into_bytes(), Object::Reference(id))));
        Ok::<_, lopdf::Error>(())
    })?;
    configure(structure, &ocgs, forbids).map_err(PdfError::from)
}

fn permitted<'group>(groups: impl IntoIterator<Item = &'group Group>, forbids: bool) -> Result<(), PdfError> {
    groups.into_iter().find(|group| forbids && group.print != group.view).map_or(Ok(()), |group| Err(PdfError::Usage(Arc::clone(&group.name))))
}

fn entries(structure: &lopdf::Document) -> Result<Vec<(ObjectId, Group)>, lopdf::Error> {
    let Some(properties) = entry(structure, structure.catalog()?, PROPERTIES)?.map(Object::as_dict).transpose()? else {
        return Ok(Vec::new());
    };
    let absent = Dictionary::new();
    let configuration = entry(structure, properties, CONFIGURATION)?.map(Object::as_dict).transpose()?.unwrap_or(&absent);
    let references = |list: &Object| list.as_array().and_then(|items| items.iter().map(Object::as_reference).try_collect::<HashSet<_>>());
    let listed = |parent: &Dictionary, key: &str| entry(structure, parent, key)?.map_or_else(|| Ok(HashSet::new()), references);
    let applications = applications(structure, configuration)?;
    let applied = |event: &str| {
        applications
            .iter()
            .map(|application| {
                let governs = application.get_deref(EVENT.as_bytes(), structure)?.as_name()? == event.as_bytes()
                    && application.get_deref(CATEGORY.as_bytes(), structure)?.as_array()?.iter().any(|category| category.as_name().is_ok_and(|name| name == event.as_bytes()));
                if governs { listed(application, GROUPS) } else { Ok(HashSet::new()) }
            })
            .try_collect::<Vec<_>>()
            .map(|lists| lists.into_iter().flatten().collect::<HashSet<_>>())
    };
    let (on, off, viewed, printed) = (listed(configuration, ON)?, listed(configuration, OFF)?, applied(VIEW)?, applied(PRINT)?);
    let based_off = entry(structure, configuration, "BaseState")?.map(Object::as_name).transpose()? == Some(OFF.as_bytes());
    let visibility = |shown: bool| if shown { Visibility::Visible } else { Visibility::Hidden };
    let stated = |ocg: &Dictionary, applying: bool, category: &str, key: &str| -> Result<Option<Visibility>, lopdf::Error> {
        Ok(if applying { crate::nested(structure, ocg, &[USAGE.as_bytes(), category.as_bytes()])?.map(|usage| entry(structure, usage, key)).transpose()?.flatten().map(Object::as_name).transpose()?.map(|state| visibility(state != OFF.as_bytes())) } else { None })
    };
    entry(structure, properties, GROUPS)?
        .map_or(Ok(&[][..]), |list| list.as_array().map(Vec::as_slice))?
        .iter()
        .map(|reference| {
            let id = reference.as_reference()?;
            let ocg = structure.get_dictionary(id)?;
            let view = stated(ocg, viewed.contains(&id), VIEW, VIEW_STATE)?.unwrap_or_else(|| visibility(if based_off { on.contains(&id) } else { !off.contains(&id) }));
            Ok::<_, lopdf::Error>((
                id,
                Group { name: Arc::from(decode_text_string(ocg.get_deref(NAME.as_bytes(), structure)?)?), print: stated(ocg, printed.contains(&id), PRINT, PRINT_STATE)?.unwrap_or(view), view },
            ))
        })
        .try_collect::<Vec<_>>()
}

fn stamp(structure: &mut lopdf::Document, id: ObjectId, group: &Group) -> Result<(), lopdf::Error> {
    let state = |visibility: Visibility| {
        Object::from(match visibility {
            Visibility::Visible => ON,
            Visibility::Hidden => OFF,
        })
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
    structure.get_dictionary_mut(id).map(|ocg| ocg.set(USAGE, usage))
}

fn configure(structure: &mut lopdf::Document, ocgs: &[(ObjectId, Group)], forbids: bool) -> Result<(), lopdf::Error> {
    let all = ocgs.iter().map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>();
    let shown = |visibility: Visibility| ocgs.iter().filter(|(_, group)| group.view == visibility).map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>();
    let mut properties = subdictionary(structure, structure.catalog()?, PROPERTIES)?;
    let mut configuration = subdictionary(structure, &properties, CONFIGURATION)?;
    let foreign = if forbids {
        Vec::new()
    } else {
        applications(structure, &configuration)?
            .into_iter()
            .map(|application| {
                let event = application.get_deref(EVENT.as_bytes(), structure)?.as_name()?;
                let managed = |category: &Object| [VIEW, PRINT].iter().any(|ours| ours.as_bytes() == event) && category.as_name().is_ok_and(|name| name == event);
                let categories = application.get_deref(CATEGORY.as_bytes(), structure)?.as_array()?.iter().filter(|category| !managed(category)).cloned().collect::<Vec<_>>();
                Ok::<_, lopdf::Error>((!categories.is_empty()).then(|| {
                    let mut kept = application.clone();
                    kept.set(CATEGORY, categories);
                    Object::Dictionary(kept)
                }))
            })
            .try_collect::<Vec<_>>()?
    };
    let ours = ocgs
        .iter()
        .any(|(_, group)| group.print != group.view)
        .then(|| [VIEW, PRINT].map(|event| Object::from(dictionary! { EVENT => event, CATEGORY => vec![Object::from(event)], GROUPS => all.clone() })));
    let usage = foreign.into_iter().flatten().chain(ours.into_iter().flatten()).collect::<Vec<_>>();
    configuration.set(ON, shown(Visibility::Visible));
    configuration.set(OFF, shown(Visibility::Hidden));
    configuration.as_hashmap_mut().entry(NAME.as_bytes().to_vec()).or_insert_with(|| text_string(env!("CARGO_PKG_NAME")));
    configuration.as_hashmap_mut().entry(b"Order".to_vec()).or_insert_with(|| Object::Array(all.clone()));
    if usage.is_empty() {
        configuration.remove(APPLICATIONS.as_bytes());
    } else {
        configuration.set(APPLICATIONS, usage);
    }
    properties.set(GROUPS, all);
    properties.set(CONFIGURATION, configuration);
    structure.catalog_mut().map(|catalog| catalog.set(PROPERTIES, properties))
}

fn applications<'structure>(structure: &'structure lopdf::Document, configuration: &'structure Dictionary) -> Result<Vec<&'structure Dictionary>, lopdf::Error> {
    entry(structure, configuration, APPLICATIONS)?
        .map_or(Ok(Vec::new()), |list| list.as_array()?.iter().map(|listed| crate::resolved(structure, listed).and_then(Object::as_dict)).try_collect::<Vec<_>>())
}

fn subdictionary(structure: &lopdf::Document, parent: &Dictionary, key: &str) -> Result<Dictionary, lopdf::Error> { crate::nested(structure, parent, &[key.as_bytes()]).map(|found| found.cloned().unwrap_or_default()) }
