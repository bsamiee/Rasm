use std::{collections::HashMap, sync::Arc};

use enum_map::EnumMap;
use rasm_drawing::pages::Destination;

use crate::{Annotation, symbols::Instance};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum)]
pub enum Mention {
    Number,
    Sheet,
}

pub(crate) type Targets = HashMap<Arc<str>, (EnumMap<Mention, Arc<str>>, Destination)>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Reference {
    pub view: Arc<str>,
    pub marker: Instance,
    pub mentions: EnumMap<Mention, Option<Arc<str>>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn resolved(reference: Reference, (texts, destination): &(EnumMap<Mention, Arc<str>>, Destination)) -> (Annotation, Destination) {
    let Reference { view: _, mut marker, mentions } = reference;
    marker.slots.extend(mentions.into_iter().filter_map(|(mention, slot)| slot.map(|slot| (slot, Arc::clone(&texts[mention])))));
    (Annotation::Symbol(marker), destination.clone())
}
