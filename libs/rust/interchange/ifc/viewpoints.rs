use itertools::EitherOrBoth;
use rasm_bcf::{Components, IfcGuid, Presentation, Reference, ViewSetupHint, presentation};
use uuid::{Uuid, fmt::Hyphenated};

use crate::{
    Composed, IfcError, Model, Path,
    schemas::{Attribute as _, attribute},
    spatial::{Class, SpaceBoundary, entity},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn node_presentation(components: &Components) -> impl Fn(&Composed) -> Result<Option<Presentation>, IfcError> {
    let present = presentation(components);
    move |node| reference(&node.path).and_then(|reference| hint(node).map(|hint| present(reference.as_ref(), hint)))
}

pub fn reference(path: &Path) -> Result<Option<Reference>, IfcError> {
    let head = path.head();
    matches!(head.len(), IfcGuid::LENGTH | Hyphenated::LENGTH).then(|| head.parse().map(EitherOrBoth::Left).map_err(|source| IfcError::Guid { node: path.clone(), source })).transpose()
}

#[must_use]
pub fn referent<'model>(model: &'model Model, reference: &Reference) -> Option<&'model Composed> {
    let &identity = reference.as_ref().left()?;
    let mut text = [0; Hyphenated::LENGTH];
    model.head(Uuid::from(identity).hyphenated().encode_lower(&mut text)).or_else(|| model.head(&identity.to_string()))
}

pub fn hint(node: &Composed) -> Result<Option<ViewSetupHint>, IfcError> {
    attribute::<Class>(node).map(|class| match class.as_ref().and_then(entity) {
        Some("Space") => Some(ViewSetupHint::Spaces),
        Some("OpeningElement" | "OpeningStandardCase") => Some(ViewSetupHint::Openings),
        Some(_) | None => node.attributes.contains_key(SpaceBoundary::ID).then_some(ViewSetupHint::SpaceBoundaries),
    })
}
