use core::iter;
use std::{
    collections::{BTreeSet, HashMap, HashSet},
    sync::Arc,
};

use color::{AlphaColor, Rgba8, Srgb};
use data_encoding::{HEXUPPER, HEXUPPER_PERMISSIVE};
use itertools::{Either, EitherOrBoth};
use nalgebra::{Isometry3, Point3, Translation3, Unit, UnitQuaternion, Vector3};
use nonempty_collections::NEVec;
use strum::VariantArray as _;
use uom::si::{
    angle::degree,
    f64::{Angle, Length},
    length::meter,
};
use uuid::{Uuid, fmt::Hyphenated};
use validated::Validated;
use xot::Node;

use super::{
    ASPECT_RATIO, IFC_GUID_DIGITS, Image, ImageFormat, ORTHOGONAL_CAMERA, PERSPECTIVE_CAMERA, REFERENCE, Text,
    xml::{Codec, Reader, Simple, Writer, codec, local_name, named, sides},
};
use crate::{
    Alphabet, Composed, Constraint, IfcError, Model, Path, PositiveLength, bind,
    schemas::{Attribute as _, attribute},
    spatial::{Class, SpaceBoundary},
};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Reference = EitherOrBoth<Uuid, Text>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, strum::IntoStaticStr, strum::VariantArray)]
pub enum ViewSetupHint {
    #[strum(serialize = "SpacesVisible")]
    Spaces,
    #[strum(serialize = "SpaceBoundariesVisible")]
    SpaceBoundaries,
    #[strum(serialize = "OpeningsVisible")]
    Openings,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const AXES: [&str; 3] = ["X", "Y", "Z"];
const CAMERA: [&str; 3] = ["CameraViewPoint", "CameraDirection", "CameraUpVector"];
const BITMAP: [&str; 3] = ["Location", "Normal", "Up"];
const VIEW_TO_WORLD_SCALE: &str = "ViewToWorldScale";
const FIELD_OF_VIEW: &str = "FieldOfView";
const COMPONENT: &str = "Component";
const COMPONENTS: &str = "Components";
const IFC_GUID: &str = "IfcGuid";
const AUTHORING_TOOL_ID: &str = "AuthoringToolId";
const HEIGHT: &str = "Height";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Visualization {
    pub components: Components,
    pub camera: Camera,
    pub lines: Vec<Line>,
    pub clipping_planes: Vec<ClippingPlane>,
    pub bitmaps: Vec<Bitmap>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Camera {
    pub placement: Isometry3<f64>,
    pub projection: Projection,
    pub aspect_ratio: AspectRatio,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Projection {
    Orthogonal(PositiveLength),
    Perspective(FieldOfView),
}

#[nutype::nutype(
    validate(
        with = |ratio: &f64| -> Result<(), IfcError> {
            (ratio.is_finite() && *ratio > 0.0).ok_or(IfcError::Constrained(Constraint::AspectRatio(*ratio)))
        },
        error = IfcError
    ),
    derive(Debug, Clone, Copy, PartialEq, PartialOrd, Display, AsRef, Deref, TryFrom, Into)
)]
pub struct AspectRatio(f64);

#[nutype::nutype(
    validate(
        with = |angle: &Angle| -> Result<(), IfcError> {
            (angle.value > 0.0 && *angle < Angle::HALF_TURN).ok_or(IfcError::Constrained(Constraint::FieldOfView(angle.get::<degree>())))
        },
        error = IfcError
    ),
    derive(Debug, Clone, Copy, PartialEq, PartialOrd, AsRef, Deref, TryFrom, Into)
)]
pub struct FieldOfView(Angle);

#[derive(Debug, Clone, PartialEq, Default)]
pub struct Components {
    pub selection: Vec<Component>,
    pub visibility: Option<Visibility>,
    pub coloring: Vec<Coloring>,
}

#[derive(Debug, Clone, PartialEq, Eq, Default)]
pub struct Visibility {
    pub default: bool,
    pub view_setup_hints: BTreeSet<ViewSetupHint>,
    pub exceptions: Vec<Component>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Coloring {
    pub color: AlphaColor<Srgb>,
    pub components: NEVec<Component>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Component {
    pub originating_system: Option<Text>,
    pub reference: Reference,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Line {
    pub start: Point3<f64>,
    pub end: Point3<f64>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct ClippingPlane {
    pub location: Point3<f64>,
    pub direction: Unit<Vector3<f64>>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Bitmap {
    pub image: Image,
    pub placement: Isometry3<f64>,
    pub height: PositiveLength,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Presentation {
    pub selected: bool,
    pub color: Option<AlphaColor<Srgb>>,
}

codec! {
    Visualization {
        element components(&[], COMPONENTS) = default,
        inline camera,
        element lines(&["Lines"], "Line"),
        element clipping_planes(&["ClippingPlanes"], "ClippingPlane"),
        element bitmaps(&["Bitmaps"], "Bitmap"),
    }
    Components {
        element selection(&["Selection"], COMPONENT),
        element visibility(&[], "Visibility"),
        element coloring(&["Coloring"], "Color"),
    }
    Visibility {
        attribute default(&[], "DefaultVisibility") = default,
        element view_setup_hints(&[], "ViewSetupHints") = default,
        element exceptions(&["Exceptions"], COMPONENT),
    }
    Coloring {
        attribute color(&[], "Color"),
        element components(&[COMPONENTS], COMPONENT),
    }
    Component {
        element originating_system(&[], "OriginatingSystem"),
        inline reference,
    }
    Line {
        element start(&[], "StartPoint"),
        element end(&[], "EndPoint"),
    }
    ClippingPlane {
        element location(&[], "Location"),
        element direction(&[], "Direction"),
    }
}

impl Simple for AspectRatio {
    fn decode(text: &str) -> Result<Self, IfcError> { f64::decode(text).and_then(Self::try_new) }

    fn encode(&self) -> String { self.into_inner().encode() }
}

impl Simple for FieldOfView {
    fn decode(text: &str) -> Result<Self, IfcError> { f64::decode(text).and_then(|angle| Self::try_new(Angle::new::<degree>(angle))) }

    fn encode(&self) -> String { self.into_inner().get::<degree>().encode() }
}

impl Simple for PositiveLength {
    fn decode(text: &str) -> Result<Self, IfcError> { f64::decode(text).and_then(|length| Self::try_new(Length::new::<meter>(length))) }

    fn encode(&self) -> String { self.get::<meter>().encode() }
}

impl Simple for AlphaColor<Srgb> {
    fn decode(text: &str) -> Result<Self, IfcError> {
        match HEXUPPER_PERMISSIVE.decode(text.as_bytes()).map_err(|source| IfcError::Encoded { text: text.into(), alphabet: Alphabet::Hexadecimal, source })? {
            [r, g, b] => Ok(Self::from_rgb8(r, g, b)),
            [alpha, r, g, b] => Ok(Self::from_rgba8(r, g, b, alpha)),
            bytes => Err(IfcError::Constrained(Constraint::Color(bytes.len()))),
        }
    }

    fn encode(&self) -> String {
        let Rgba8 { r, g, b, a: alpha } = self.to_rgba8();
        match alpha {
            u8::MAX => HEXUPPER.encode(&[r, g, b]),
            _ => HEXUPPER.encode(&[alpha, r, g, b]),
        }
    }
}

impl Codec for Camera {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        let projected = |camera: Node, projection: Validated<Projection, IfcError>| {
            placement(reader, camera, CAMERA).map3(projection, reader.element(camera, &[], ASPECT_RATIO), |placement, projection, aspect_ratio| Self { placement, projection, aspect_ratio })
        };
        match (named(reader.xot, node, ORTHOGONAL_CAMERA).next(), named(reader.xot, node, PERSPECTIVE_CAMERA).next()) {
            (Some(camera), _) => projected(camera, reader.element(camera, &[], VIEW_TO_WORLD_SCALE).map(Projection::Orthogonal)),
            (None, Some(camera)) => projected(camera, reader.element(camera, &[], FIELD_OF_VIEW).map(Projection::Perspective)),
            (None, None) => Validated::fail(reader.refused(ORTHOGONAL_CAMERA, None)),
        }
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { placement, projection, aspect_ratio } = self;
        let camera = writer.append(
            node,
            match projection {
                Projection::Orthogonal(_) => ORTHOGONAL_CAMERA,
                Projection::Perspective(_) => PERSPECTIVE_CAMERA,
            },
        )?;
        place(writer, camera, CAMERA, placement)?;
        match projection {
            Projection::Orthogonal(scale) => writer.element(camera, &[], VIEW_TO_WORLD_SCALE, scale),
            Projection::Perspective(angle) => writer.element(camera, &[], FIELD_OF_VIEW, angle),
        }?;
        writer.element(camera, &[], ASPECT_RATIO, aspect_ratio)
    }
}

impl Codec for BTreeSet<ViewSetupHint> {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        ViewSetupHint::VARIANTS
            .iter()
            .map(|&hint| reader.attribute::<Option<bool>>(node, &[], hint.into()).map(|shown| shown.unwrap_or_default().then_some(hint)))
            .collect::<Validated<Vec<_>, _>>()
            .map(|hints| hints.into_iter().flatten().collect())
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.iter().try_for_each(|&hint| writer.attribute(node, &[], hint.into(), &true)) }
}

impl Codec for Reference {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> { sides(reader, reader.attribute(node, &[], IFC_GUID), reader.element(node, &[], AUTHORING_TOOL_ID), IFC_GUID) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        if let Some(tool) = self.as_ref().right() {
            writer.element(node, &[], AUTHORING_TOOL_ID, tool)?;
        }
        if let Some(identity) = self.as_ref().left() {
            writer.attribute(node, &[], IFC_GUID, identity)?;
        }
        Ok(())
    }
}

impl Codec for Bitmap {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        reader.element(node, &[], REFERENCE).map3(placement(reader, node, BITMAP), reader.element(node, &[], HEIGHT), |image, placement, height| Self { image, placement, height })
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { image, placement, height } = self;
        writer.text(node, "Format", AsRef::<ImageFormat>::as_ref(image).into())?;
        writer.element(node, &[], REFERENCE, image)?;
        place(writer, node, BITMAP, placement)?;
        writer.element(node, &[], HEIGHT, height)
    }
}

impl Codec for Vector3<f64> {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        let [x, y, z] = AXES.map(|axis| reader.element(node, &[], axis));
        x.map3(y, z, Self::new)
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { iter::zip(AXES, self.iter()).try_for_each(|(axis, value)| writer.element(node, &[], axis, value)) }
}

impl Codec for Unit<Vector3<f64>> {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> {
        bind(Vector3::read(reader, node), |vector| Self::try_new(vector, 0.0).map_or_else(|| Validated::fail(reader.refused(local_name(reader.xot, node), None)), Validated::Good))
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.as_ref().write(writer, node) }
}

impl Codec for Point3<f64> {
    fn read(reader: &Reader<'_>, node: Node) -> Validated<Self, IfcError> { Vector3::read(reader, node).map(Self::from) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.coords.write(writer, node) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VISIBILITY]

fn shown(visibility: &Visibility, hint: Option<ViewSetupHint>) -> bool { hint.map_or(visibility.default, |category| visibility.view_setup_hints.contains(&category)) }

fn identities(reference: &Reference) -> impl Iterator<Item = Either<Uuid, &Text>> {
    let (identity, tool) = reference.as_ref().left_and_right();
    iter::chain(identity.map(|identity| Either::Left(*identity)), tool.map(Either::Right))
}

pub fn presentation(components: &Components) -> impl Fn(Option<&Reference>, Option<ViewSetupHint>) -> Option<Presentation> {
    fn keyed(listed: &[Component]) -> HashSet<Either<Uuid, &Text>> { listed.iter().flat_map(|component| identities(&component.reference)).collect() }
    let Components { selection, visibility, coloring } = components;
    let selected = keyed(selection);
    let exceptions = visibility.as_ref().map(|stated| (stated, keyed(&stated.exceptions)));
    let colors = coloring
        .iter()
        .rev()
        .flat_map(|Coloring { color, components: colored }| colored.iter().flat_map(|component| identities(&component.reference)).map(move |identity| (identity, *color)))
        .collect::<HashMap<_, _>>();
    move |reference, hint| {
        let matched = |set: &HashSet<_>| reference.is_some_and(|reference| identities(reference).any(|identity| set.contains(&identity)));
        exceptions
            .as_ref()
            .is_none_or(|(stated, listed)| shown(stated, hint) != matched(listed))
            .then(|| Presentation { selected: matched(&selected), color: reference.and_then(|reference| identities(reference).find_map(|identity| colors.get(&identity).copied())) })
    }
}

pub fn node_presentation(components: &Components) -> impl Fn(&Composed) -> Result<Option<Presentation>, IfcError> {
    let present = presentation(components);
    move |node| reference(&node.path).and_then(|reference| hint(node).map(|hint| present(reference.as_ref(), hint)))
}

pub fn reference(path: &Path) -> Result<Option<Reference>, IfcError> {
    let head = path.head();
    matches!(head.len(), IFC_GUID_DIGITS | Hyphenated::LENGTH).then(|| Uuid::decode(head).map(EitherOrBoth::Left)).transpose()
}

#[must_use]
pub fn referent<'model>(model: &'model Model, reference: &Reference) -> Option<&'model Composed> {
    let &identity = reference.as_ref().left()?;
    let mut text = [0; Hyphenated::LENGTH];
    model.heads.get(identity.hyphenated().encode_lower(&mut text)).or_else(|| model.heads.get(identity.encode().as_str())).map(Arc::as_ref)
}

pub fn hint(node: &Composed) -> Result<Option<ViewSetupHint>, IfcError> {
    attribute::<Class>(node).map(|class| match class.as_ref().map(|class| &*class.code) {
        Some("IfcSpace") => Some(ViewSetupHint::Spaces),
        Some("IfcOpeningElement" | "IfcOpeningStandardCase") => Some(ViewSetupHint::Openings),
        _ => node.attributes.contains_key(SpaceBoundary::ID).then_some(ViewSetupHint::SpaceBoundaries),
    })
}

#[must_use]
pub fn visibility<V, H>(visible: V, hidden: H) -> Visibility
where
    V: IntoIterator<Item = (Component, Option<ViewSetupHint>)>,
    H: IntoIterator<Item = (Component, Option<ViewSetupHint>)>,
{
    let visible = visible.into_iter().collect::<Vec<_>>();
    let hidden = hidden.into_iter().collect::<Vec<_>>();
    let default = hidden.len() < visible.len();
    let view_setup_hints = if default { visible.iter().filter_map(|&(_, hint)| hint).collect() } else { BTreeSet::new() };
    let stated = Visibility { default, view_setup_hints, exceptions: Vec::new() };
    let exceptions = visible.into_iter().filter(|&(_, hint)| !shown(&stated, hint)).chain(hidden.into_iter().filter(|&(_, hint)| shown(&stated, hint))).map(|(component, _)| component).collect();
    Visibility { exceptions, ..stated }
}

// --- [PLACEMENT]

fn placement(reader: &Reader<'_>, node: Node, [location, direction, up]: [&'static str; 3]) -> Validated<Isometry3<f64>, IfcError> {
    bind(
        reader
            .element::<Point3<f64>>(node, &[], location)
            .map3(reader.element::<Vector3<f64>>(node, &[], direction), reader.element::<Vector3<f64>>(node, &[], up), |origin, toward, upward| (origin, toward, upward)),
        |(origin, toward, upward)| {
            (upward.cross(&toward) != Vector3::zeros())
                .then(|| Isometry3::from_parts(Translation3::from(origin.coords), UnitQuaternion::face_towards(&toward, &upward)))
                .map_or_else(|| Validated::fail(reader.refused(up, None)), Validated::Good)
        },
    )
}

fn place(writer: &mut Writer<'_>, node: Node, [location, direction, up]: [&'static str; 3], placement: &Isometry3<f64>) -> Result<(), IfcError> {
    writer.element(node, &[], location, &Point3::from(placement.translation.vector))?;
    writer.element(node, &[], direction, &placement.rotation.transform_vector(&Vector3::z()))?;
    writer.element(node, &[], up, &placement.rotation.transform_vector(&Vector3::y()))
}
