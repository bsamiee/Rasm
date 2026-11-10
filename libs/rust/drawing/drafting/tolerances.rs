use core::iter;
use std::sync::Arc;

use enum_map::EnumMap;
use itertools::Itertools as _;
use kurbo::Point;
use nonempty_collections::{NEVec, nev};
use rasm_drawing::{Anchor, Placement};
use uom::si::f64::Length;

use crate::notation::{self, Sign};
use crate::text::{Label, Typesetter};
use crate::{Content, DraftingError, Graphics, tables};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Characteristic {
    #[display("⏤")]
    Straightness,
    #[display("⏥")]
    Flatness,
    #[display("○")]
    Circularity,
    #[display("⌭")]
    Cylindricity,
    #[display("⌒")]
    LineProfile,
    #[display("⌓")]
    SurfaceProfile,
    #[display("∠")]
    Angularity,
    #[display("⟂")]
    Perpendicularity,
    #[display("∥")]
    Parallelism,
    #[display("⌖")]
    Position,
    #[display("◎")]
    Concentricity,
    #[display("⌯")]
    Symmetry,
    #[display("↗")]
    CircularRunout,
    #[display("⌰")]
    TotalRunout,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Modifier {
    #[display("Ⓜ")]
    MaximumMaterial,
    #[display("Ⓛ")]
    LeastMaterial,
    #[display("Ⓟ")]
    ProjectedZone,
    #[display("Ⓕ")]
    FreeState,
    #[display("Ⓣ")]
    TangentPlane,
    #[display("Ⓤ")]
    UnequalProfile,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Zone {
    #[display("")]
    Width,
    #[display("⌀")]
    Diameter,
    #[display("S⌀")]
    Spherical,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct Datum {
    pub letter: Arc<str>,
    pub modifiers: Vec<Modifier> = Vec::new(),
}

#[derive(Debug, Clone, PartialEq)]
pub struct FeatureControl {
    pub at: Point,
    pub characteristic: Characteristic,
    pub zone: Zone = Zone::Width,
    pub tolerance: Length,
    pub modifiers: Vec<Modifier> = Vec::new(),
    pub datums: Vec<Datum> = Vec::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, control: &FeatureControl) -> Result<Graphics, NEVec<DraftingError>> {
    let FeatureControl { at, characteristic, zone, tolerance, modifiers, datums } = control;
    let cell = |content: String| tables::Cell { content: Content::Label(Label { content: content.into(), .. }), .. };
    let row = NEVec::from((
        cell(characteristic.to_string()),
        iter::once(format!("{zone}{}{}", notation::linear(&style.linear, *tolerance, Sign::Negative), modifiers.iter().format("")))
            .chain(datums.iter().map(|Datum { letter, modifiers: marks }| format!("{letter}{}", marks.iter().format(""))))
            .map(cell)
            .collect::<Vec<_>>(),
    ));
    let ruled = |rule: tables::Rule| match rule {
        tables::Rule::Border | tables::Rule::Columns => Some(style.pen.clone()),
        tables::Rule::Header | tables::Rule::Rows => None,
    };
    tables::draw(
        typesetter,
        style,
        placement,
        &tables::Table {
            at: *at,
            columns: NEVec::from_elem(tables::Column { anchor: Anchor::Middle, vertical: Anchor::Middle, .. }, row.len()),
            rows: nev![row.into()],
            rules: EnumMap::from_fn(ruled),
            ..
        },
    )
}
