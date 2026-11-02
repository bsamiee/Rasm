use std::{
    borrow::Borrow,
    collections::{BTreeMap, HashMap},
    sync::Arc,
};

use lopdf::{
    Dictionary, Object, ObjectId,
    content::{Content, Operation},
    decode_text_string, dictionary, text_string,
};
use rasm_linework::Visibility;

use crate::{Document, PdfError};

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROPERTIES: &str = "OCProperties";
const GROUPS: &str = "OCGs";
const CONFIGURATION: &str = "D";
const NAME: &str = "Name";
const USAGE: &str = "Usage";
const PRINT: &str = "Print";
const VIEW: &str = "View";
const PRINT_STATE: &str = "PrintState";
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
    let requested = groups.into_iter().collect::<Vec<_>>();
    let wanted = requested.iter().map(Borrow::<Group>::borrow).collect::<Vec<_>>();
    let held = entries(&structure)?;
    wanted.iter().find(|group| held.iter().all(|(_, present)| present.name != group.name)).map_or(Ok(()), |missing| Err(PdfError::Group(Arc::clone(&missing.name))))?;
    let ocgs = held.into_iter().map(|(id, present)| (id, wanted.iter().rfind(|group| group.name == present.name).map_or(present, |&group| group.clone()))).collect::<Vec<_>>();
    ocgs.iter().filter(|(_, group)| wanted.iter().any(|named| named.name == group.name)).try_for_each(|(id, group)| stamp(&mut structure, *id, group))?;
    configure(&mut structure, &ocgs, false)?;
    let mut bytes = Vec::new();
    structure.save_to(&mut bytes)?;
    Document::new(bytes, None)
}

pub(crate) fn bind(structure: &mut lopdf::Document, pages: &[Vec<Group>], forbids: bool) -> Result<(), PdfError> {
    pages.iter().flatten().find(|group| forbids && group.print != group.view).map_or(Ok(()), |group| Err(PdfError::Usage(Arc::clone(&group.name))))?;
    let mut registered = HashMap::<&Group, ObjectId>::new();
    let bound = pages
        .iter()
        .map(|layers| layers.iter().map(|group| *registered.entry(group).or_insert_with(|| structure.add_object(dictionary! { "Type" => "OCG", NAME => text_string(&group.name) }))).collect::<Vec<_>>())
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
        structure.get_or_create_resources(page)?.as_dict_mut().map(|resources| resources.set("Properties", layers.iter().map(|&id| (resource(id), Object::Reference(id))).collect::<Dictionary>()))
    })?;
    configure(structure, &ocgs, forbids).map_err(PdfError::from)
}

fn entries(structure: &lopdf::Document) -> Result<Vec<(ObjectId, Group)>, lopdf::Error> {
    let Some(properties) = lookup(structure, structure.catalog()?, &[], PROPERTIES)?.map(Object::as_dict).transpose()? else {
        return Ok(Vec::new());
    };
    let configuration = properties.get_deref(CONFIGURATION.as_bytes(), structure)?.as_dict()?;
    let listed = |key: &str| -> Result<Vec<ObjectId>, lopdf::Error> { lookup(structure, configuration, &[], key)?.map_or(Ok(Vec::new()), |array| array.as_array().and_then(|references| references.iter().map(Object::as_reference).try_collect::<Vec<_>>())) };
    let (on, off, base) = (listed(ON)?, listed(OFF)?, lookup(structure, configuration, &[], "BaseState")?.map(Object::as_name).transpose()?);
    let visibility = |shown: bool| if shown { Visibility::Visible } else { Visibility::Hidden };
    properties
        .get_deref(GROUPS.as_bytes(), structure)?
        .as_array()?
        .iter()
        .map(|reference| {
            let id = reference.as_reference()?;
            let ocg = structure.get_dictionary(id)?;
            let view = visibility(on.contains(&id) || !(base == Some(OFF.as_bytes()) || off.contains(&id)));
            Ok::<_, lopdf::Error>((id, Group {
                name: Arc::from(decode_text_string(ocg.get_deref(NAME.as_bytes(), structure)?)?),
                print: lookup(structure, ocg, &[USAGE, PRINT], PRINT_STATE)?.map(Object::as_name).transpose()?.map_or(view, |state| visibility(state != OFF.as_bytes())),
                view,
            }))
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
    let usage = [(PRINT, PRINT_STATE, group.print), (VIEW, "ViewState", group.view)].into_iter().try_fold(subdictionary(structure, structure.get_dictionary(id)?, USAGE)?, |mut usage, (category, key, visibility)| {
        let mut entry = subdictionary(structure, &usage, category)?;
        entry.set(key, state(visibility));
        usage.set(category, entry);
        Ok::<_, lopdf::Error>(usage)
    })?;
    structure.get_dictionary_mut(id).map(|ocg| ocg.set(USAGE, usage))
}

fn configure(structure: &mut lopdf::Document, ocgs: &[(ObjectId, Group)], forbids: bool) -> Result<(), lopdf::Error> {
    let all = ocgs.iter().map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>();
    let shown = |visibility: Visibility| ocgs.iter().filter(|(_, group)| group.view == visibility).map(|&(id, _)| Object::Reference(id)).collect::<Vec<_>>();
    let mut properties = subdictionary(structure, structure.catalog()?, PROPERTIES)?;
    let mut configuration = subdictionary(structure, &properties, CONFIGURATION)?;
    configuration.set(ON, shown(Visibility::Visible));
    configuration.set(OFF, shown(Visibility::Hidden));
    configuration.as_hashmap_mut().entry(NAME.as_bytes().to_vec()).or_insert_with(|| text_string(env!("CARGO_PKG_NAME")));
    configuration.as_hashmap_mut().entry(b"Order".to_vec()).or_insert_with(|| Object::Array(all.clone()));
    if !forbids && ocgs.iter().any(|(_, group)| group.print != group.view) {
        configuration.set("AS", [VIEW, PRINT].map(|event| Object::from(dictionary! { "Event" => event, "Category" => vec![Object::from(event)], GROUPS => all.clone() })).to_vec());
    }
    properties.set(GROUPS, all);
    properties.set(CONFIGURATION, configuration);
    structure.catalog_mut().map(|catalog| catalog.set(PROPERTIES, properties))
}

fn lookup<'a>(structure: &'a lopdf::Document, parent: &'a Dictionary, path: &[&str], key: &str) -> Result<Option<&'a Object>, lopdf::Error> {
    path.iter()
        .try_fold(Some(parent), |found, step| found.filter(|dictionary| dictionary.has(step.as_bytes())).map(|dictionary| structure.get_dict_in_dict(dictionary, step.as_bytes())).transpose())?
        .filter(|dictionary| dictionary.has(key.as_bytes()))
        .map(|dictionary| dictionary.get_deref(key.as_bytes(), structure))
        .transpose()
}

fn subdictionary(structure: &lopdf::Document, parent: &Dictionary, key: &str) -> Result<Dictionary, lopdf::Error> {
    lookup(structure, parent, &[], key)?.map_or(Ok(Dictionary::new()), |found| found.as_dict().cloned())
}
