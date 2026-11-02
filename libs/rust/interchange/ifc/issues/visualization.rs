use core::iter;
use std::collections::{BTreeSet, HashMap, HashSet};

use color::{AlphaColor, Srgb};
use nalgebra::{Isometry3, Point3, Translation3, Unit, UnitQuaternion, Vector3};
use nonempty_collections::NEVec;
use strum::VariantArray as _;
use uom::si::{
    angle::degree,
    f64::{Angle, Length},
};
use validated::Validated;
use xot::Node;

use super::{Codec, Image, ImageFormat, NodeUuid, REFERENCE, Reader, Simple, Text, Writer};
use crate::IfcError;

// --- [TYPES] ---------------------------------------------------------------------------

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
const ORTHOGONAL_CAMERA: &str = "OrthogonalCamera";
const PERSPECTIVE_CAMERA: &str = "PerspectiveCamera";
const VIEW_TO_WORLD_SCALE: &str = "ViewToWorldScale";
const FIELD_OF_VIEW: &str = "FieldOfView";
const ASPECT_RATIO: &str = "AspectRatio";
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
    Orthogonal(ViewToWorldScale),
    Perspective(FieldOfView),
}

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Display, AsRef, Deref, TryFrom, Into))]
pub struct AspectRatio(f64);

#[nutype::nutype(
    validate(predicate = |scale| scale.value.is_finite() && scale.value > 0.0),
    derive(Debug, Clone, Copy, PartialEq, PartialOrd, AsRef, Deref, TryFrom, Into)
)]
pub struct ViewToWorldScale(Length);

#[nutype::nutype(
    validate(predicate = |angle| {
        angle.value > 0.0 && *angle < Angle::HALF_TURN
    }),
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

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Reference {
    Node { node: NodeUuid, authoring_tool_id: Option<Text> },
    Tool { authoring_tool_id: Text },
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
    pub height: Length,
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
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        f64::decode(text, entry, element)
            .and_then(|ratio| Self::try_new(ratio).map_err(|(AspectRatioError::FiniteViolated | AspectRatioError::GreaterViolated)| IfcError::Element { entry: entry.into(), element }))
    }

    fn encode(&self) -> String { self.into_inner().encode() }
}

impl Simple for ViewToWorldScale {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        Length::decode(text, entry, element).and_then(|scale| Self::try_new(scale).map_err(|ViewToWorldScaleError::PredicateViolated| IfcError::Element { entry: entry.into(), element }))
    }

    fn encode(&self) -> String { self.into_inner().encode() }
}

impl Simple for FieldOfView {
    fn decode(text: &str, entry: &str, element: &'static str) -> Result<Self, IfcError> {
        f64::decode(text, entry, element)
            .map(Angle::new::<degree>)
            .and_then(|angle| Self::try_new(angle).map_err(|FieldOfViewError::PredicateViolated| IfcError::Element { entry: entry.into(), element }))
    }

    fn encode(&self) -> String { self.into_inner().get::<degree>().encode() }
}

impl Codec for Camera {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        let projected = |camera: Node, projection: Validated<Projection, IfcError>| {
            placement(reader, camera, CAMERA).map3(projection, reader.element(camera, &[], ASPECT_RATIO), |placement, projection, aspect_ratio| Self { placement, projection, aspect_ratio })
        };
        match (reader.named(node, ORTHOGONAL_CAMERA).next(), reader.named(node, PERSPECTIVE_CAMERA).next()) {
            (Some(camera), _) => projected(camera, reader.element(camera, &[], VIEW_TO_WORLD_SCALE).map(Projection::Orthogonal)),
            (None, Some(camera)) => projected(camera, reader.element(camera, &[], FIELD_OF_VIEW).map(Projection::Perspective)),
            (None, None) => Validated::fail(reader.lacks(ORTHOGONAL_CAMERA)),
        }
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { placement, projection, aspect_ratio } = self;
        let camera = match projection {
            Projection::Orthogonal(scale) => {
                let camera = writer.append(node, ORTHOGONAL_CAMERA)?;
                place(writer, camera, CAMERA, placement)?;
                writer.element(camera, &[], VIEW_TO_WORLD_SCALE, scale).map(|()| camera)
            }
            Projection::Perspective(angle) => {
                let camera = writer.append(node, PERSPECTIVE_CAMERA)?;
                place(writer, camera, CAMERA, placement)?;
                writer.element(camera, &[], FIELD_OF_VIEW, angle).map(|()| camera)
            }
        }?;
        writer.element(camera, &[], ASPECT_RATIO, aspect_ratio)
    }
}

impl Codec for BTreeSet<ViewSetupHint> {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        ViewSetupHint::VARIANTS
            .iter()
            .map(|&hint| reader.attribute::<Option<bool>>(node, &[], hint.into()).map(|shown| shown.unwrap_or_default().then_some(hint)))
            .collect::<Validated<Vec<_>, _>>()
            .map(|hints| hints.into_iter().flatten().collect())
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.iter().try_for_each(|&hint| writer.attribute(node, &[], hint.into(), &true)) }
}

impl Codec for Reference {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        match reader.attribute::<Option<NodeUuid>>(node, &[], IFC_GUID).map2(reader.element::<Option<Text>>(node, &[], AUTHORING_TOOL_ID), |identity, tool| (identity, tool)) {
            Validated::Good((Some(identity), authoring_tool_id)) => Validated::Good(Self::Node { node: identity, authoring_tool_id }),
            Validated::Good((None, Some(authoring_tool_id))) => Validated::Good(Self::Tool { authoring_tool_id }),
            Validated::Good((None, None)) => Validated::fail(reader.lacks(IFC_GUID)),
            Validated::Fail(failures) => Validated::Fail(failures),
        }
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        match self {
            Self::Node { node: identity, authoring_tool_id } => {
                writer.element(node, &[], AUTHORING_TOOL_ID, authoring_tool_id)?;
                writer.attribute(node, &[], IFC_GUID, identity)
            }
            Self::Tool { authoring_tool_id } => writer.element(node, &[], AUTHORING_TOOL_ID, authoring_tool_id),
        }
    }
}

impl Codec for Bitmap {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        reader.image(node, REFERENCE).map3(placement(reader, node, BITMAP), reader.element(node, &[], HEIGHT), |image, placement, height| Self { image, placement, height })
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { image, placement, height } = self;
        writer.element(node, &[], "Format", AsRef::<ImageFormat>::as_ref(image))?;
        writer.image(node, REFERENCE, image)?;
        place(writer, node, BITMAP, placement)?;
        writer.element(node, &[], HEIGHT, height)
    }
}

impl Codec for Vector3<f64> {
    fn read(reader: &Reader<'_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        let [x, y, z] = AXES.map(|axis| reader.element(node, &[], axis));
        x.map3(y, z, Self::new)
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { iter::zip(AXES, self.iter()).try_for_each(|(axis, value)| writer.element(node, &[], axis, value)) }
}

impl Codec for Unit<Vector3<f64>> {
    fn read(reader: &Reader<'_>, node: Node, element: &'static str) -> Validated<Self, IfcError> {
        Vector3::read(reader, node, element).ok().map_or_else(Validated::Fail, |vector| Self::try_new(vector, 0.0).map_or_else(|| Validated::fail(reader.lacks(element)), Validated::Good))
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.as_ref().write(writer, node) }
}

impl Codec for Point3<f64> {
    fn read(reader: &Reader<'_>, node: Node, element: &'static str) -> Validated<Self, IfcError> { Vector3::read(reader, node, element).map(Self::from) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.coords.write(writer, node) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VISIBILITY]

pub fn presentation(components: &Components) -> impl Fn(NodeUuid, Option<ViewSetupHint>) -> Option<Presentation> {
    let node = |component: &Component| match component.reference {
        Reference::Node { node, authoring_tool_id: _ } => Some(node),
        Reference::Tool { authoring_tool_id: _ } => None,
    };
    let Components { selection, visibility, coloring } = components;
    let selected = selection.iter().filter_map(node).collect::<HashSet<_>>();
    let exceptions = visibility.as_ref().map(|shown| (shown, shown.exceptions.iter().filter_map(node).collect::<HashSet<_>>()));
    let colors = coloring.iter().rev().flat_map(|Coloring { color, components: colored }| colored.iter().filter_map(node).map(|identity| (identity, *color))).collect::<HashMap<_, _>>();
    move |identity, hint| {
        exceptions
            .as_ref()
            .is_none_or(|(shown, listed)| hint.map_or(shown.default, |category| shown.view_setup_hints.contains(&category)) != listed.contains(&identity))
            .then(|| Presentation { selected: selected.contains(&identity), color: colors.get(&identity).copied() })
    }
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
    let shown = |hint: Option<ViewSetupHint>| hint.map_or(default, |category| view_setup_hints.contains(&category));
    let exceptions = visible.into_iter().filter(|&(_, hint)| !shown(hint)).chain(hidden.into_iter().filter(|&(_, hint)| shown(hint))).map(|(component, _)| component).collect();
    Visibility { default, view_setup_hints, exceptions }
}

// --- [PLACEMENT]

fn placement(reader: &Reader<'_>, node: Node, [location, direction, up]: [&'static str; 3]) -> Validated<Isometry3<f64>, IfcError> {
    reader
        .element::<Point3<f64>>(node, &[], location)
        .map3(reader.element::<Vector3<f64>>(node, &[], direction), reader.element::<Vector3<f64>>(node, &[], up), |origin, toward, upward| (origin, toward, upward))
        .ok()
        .map_or_else(Validated::Fail, |(origin, toward, upward)| {
            (upward.cross(&toward) != Vector3::zeros())
                .then(|| Isometry3::from_parts(Translation3::from(origin.coords), UnitQuaternion::face_towards(&toward, &upward)))
                .map_or_else(|| Validated::fail(reader.lacks(up)), Validated::Good)
        })
}

fn place(writer: &mut Writer<'_>, node: Node, [location, direction, up]: [&'static str; 3], placement: &Isometry3<f64>) -> Result<(), IfcError> {
    writer.element(node, &[], location, &Point3::from(placement.translation.vector))?;
    writer.element(node, &[], direction, &placement.rotation.transform_vector(&Vector3::z()))?;
    writer.element(node, &[], up, &placement.rotation.transform_vector(&Vector3::y()))
}
