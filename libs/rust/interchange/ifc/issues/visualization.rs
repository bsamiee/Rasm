use core::iter;
use std::collections::BTreeSet;

use color::{AlphaColor, Srgb};
use nalgebra::{Point3, Vector3};
use nonempty_collections::NEVec;
use strum::VariantArray as _;
use uom::si::f64::{Angle, Length};
use validated::Validated;
use xot::Node;

use super::{Codec, ImageFormat, NodeUuid, Reader, Writer, bind};
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
const ORTHOGONAL_CAMERA: &str = "OrthogonalCamera";
const PERSPECTIVE_CAMERA: &str = "PerspectiveCamera";
const CAMERA_VIEW_POINT: &str = "CameraViewPoint";
const CAMERA_DIRECTION: &str = "CameraDirection";
const CAMERA_UP_VECTOR: &str = "CameraUpVector";
const VIEW_TO_WORLD_SCALE: &str = "ViewToWorldScale";
const FIELD_OF_VIEW: &str = "FieldOfView";
const ASPECT_RATIO: &str = "AspectRatio";
const IFC_GUID: &str = "IfcGuid";
const AUTHORING_TOOL_ID: &str = "AuthoringToolId";
const FORMAT: &str = "Format";
const REFERENCE: &str = "Reference";
const LOCATION: &str = "Location";
const NORMAL: &str = "Normal";
const UP: &str = "Up";
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
    pub view_point: Point3<f64>,
    pub direction: Vector3<f64>,
    pub up_vector: Vector3<f64>,
    pub aspect_ratio: f64,
    pub projection: Projection,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Projection {
    Orthogonal { view_to_world_scale: Length },
    Perspective { field_of_view: Angle },
}

#[derive(Debug, Clone, PartialEq, Default)]
pub struct Components {
    pub selection: Vec<Component>,
    pub visibility: Option<Visibility>,
    pub coloring: Vec<Coloring>,
}

#[derive(Debug, Clone, PartialEq, Eq, Default)]
pub struct Visibility {
    pub default_visible: bool,
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
    pub reference: Reference,
    pub originating_system: Option<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Reference {
    Node { node: NodeUuid, authoring_tool_id: Option<Box<str>> },
    Tool { authoring_tool_id: Box<str> },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Line {
    pub start: Point3<f64>,
    pub end: Point3<f64>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct ClippingPlane {
    pub location: Point3<f64>,
    pub direction: Vector3<f64>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Bitmap {
    pub format: ImageFormat,
    pub reference: Box<str>,
    pub location: Point3<f64>,
    pub normal: Vector3<f64>,
    pub up: Vector3<f64>,
    pub height: Length,
    pub content: Box<[u8]>,
}

codec! {
    Visualization {
        element components(&[], "Components") = default,
        inline camera,
        element lines(&["Lines"], "Line"),
        element clipping_planes(&["ClippingPlanes"], "ClippingPlane"),
        element bitmaps(&["Bitmaps"], "Bitmap"),
    }
    Components {
        element selection(&["Selection"], "Component"),
        element visibility(&[], "Visibility"),
        element coloring(&["Coloring"], "Color"),
    }
    Visibility {
        attribute default_visible(&[], "DefaultVisibility") = default,
        element view_setup_hints(&[], "ViewSetupHints") = default,
        element exceptions(&["Exceptions"], "Component"),
    }
    Coloring {
        attribute color(&[], "Color"),
        element components(&["Components"], "Component"),
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

impl Codec for Camera {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        let chosen = match (reader.named(node, ORTHOGONAL_CAMERA).next(), reader.named(node, PERSPECTIVE_CAMERA).next()) {
            (Some(camera), _) => Some((camera, reader.element(camera, &[], VIEW_TO_WORLD_SCALE).map(|view_to_world_scale| Projection::Orthogonal { view_to_world_scale }))),
            (None, Some(camera)) => Some((camera, reader.element(camera, &[], FIELD_OF_VIEW).map(|field_of_view| Projection::Perspective { field_of_view }))),
            (None, None) => None,
        };
        chosen.map_or_else(
            || Validated::fail(reader.lacks(ORTHOGONAL_CAMERA)),
            |(camera, projected)| {
                reader
                    .element(camera, &[], CAMERA_VIEW_POINT)
                    .map4(
                        reader.element(camera, &[], CAMERA_DIRECTION),
                        reader.element(camera, &[], CAMERA_UP_VECTOR),
                        reader.element(camera, &[], ASPECT_RATIO),
                        |view_point, direction, up_vector, aspect_ratio| (view_point, direction, up_vector, aspect_ratio),
                    )
                    .map2(projected, |(view_point, direction, up_vector, aspect_ratio), projection| Self { view_point, direction, up_vector, aspect_ratio, projection })
            },
        )
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { view_point, direction, up_vector, aspect_ratio, projection } = self;
        let camera = writer.append(
            node,
            match projection {
                Projection::Orthogonal { view_to_world_scale: _ } => ORTHOGONAL_CAMERA,
                Projection::Perspective { field_of_view: _ } => PERSPECTIVE_CAMERA,
            },
        )?;
        writer.element(camera, &[], CAMERA_VIEW_POINT, view_point)?;
        writer.element(camera, &[], CAMERA_DIRECTION, direction)?;
        writer.element(camera, &[], CAMERA_UP_VECTOR, up_vector)?;
        match projection {
            Projection::Orthogonal { view_to_world_scale } => writer.element(camera, &[], VIEW_TO_WORLD_SCALE, view_to_world_scale)?,
            Projection::Perspective { field_of_view } => writer.element(camera, &[], FIELD_OF_VIEW, field_of_view)?,
        }
        writer.element(camera, &[], ASPECT_RATIO, aspect_ratio)
    }
}

impl Codec for BTreeSet<ViewSetupHint> {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        ViewSetupHint::VARIANTS
            .iter()
            .map(|&hint| reader.attribute::<Option<bool>>(node, &[], hint.into()).map(|shown| shown.unwrap_or_default().then_some(hint)))
            .collect::<Validated<Vec<_>, _>>()
            .map(|hints| hints.into_iter().flatten().collect())
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { ViewSetupHint::VARIANTS.iter().try_for_each(|hint| writer.attribute(node, &[], hint.into(), &self.contains(hint))) }
}

impl Codec for Reference {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        bind(reader.attribute::<Option<NodeUuid>>(node, &[], IFC_GUID).map2(reader.element::<Option<Box<str>>>(node, &[], AUTHORING_TOOL_ID), |identity, tool| (identity, tool)), |reference| {
            match reference {
                (Some(identity), authoring_tool_id) => Validated::Good(Self::Node { node: identity, authoring_tool_id }),
                (None, Some(authoring_tool_id)) => Validated::Good(Self::Tool { authoring_tool_id }),
                (None, None) => Validated::fail(reader.lacks(IFC_GUID)),
            }
        })
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
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        reader
            .element(node, &[], FORMAT)
            .map4(
                bind(reader.element::<Box<str>>(node, &[], REFERENCE), |reference| Validated::from(reader.file(&reference).map(|content| (reference, content)))),
                reader.element(node, &[], LOCATION),
                reader.element(node, &[], NORMAL),
                |format, stored, location, normal| (format, stored, location, normal),
            )
            .map3(reader.element(node, &[], UP), reader.element(node, &[], HEIGHT), |(format, (reference, content), location, normal), up, height| Self {
                format,
                reference,
                location,
                normal,
                up,
                height,
                content,
            })
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> {
        let Self { format, reference, location, normal, up, height, content } = self;
        writer.element(node, &[], FORMAT, format)?;
        writer.element(node, &[], REFERENCE, reference)?;
        writer.element(node, &[], LOCATION, location)?;
        writer.element(node, &[], NORMAL, normal)?;
        writer.element(node, &[], UP, up)?;
        writer.element(node, &[], HEIGHT, height)?;
        writer.file(reference, content)
    }
}

impl Codec for Vector3<f64> {
    fn read(reader: &Reader<'_, '_>, node: Node, _: &'static str) -> Validated<Self, IfcError> {
        let [x, y, z] = AXES.map(|axis| reader.element(node, &[], axis));
        x.map3(y, z, Self::new)
    }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { iter::zip(AXES, self.iter()).try_for_each(|(axis, value)| writer.element(node, &[], axis, value)) }
}

impl Codec for Point3<f64> {
    fn read(reader: &Reader<'_, '_>, node: Node, element: &'static str) -> Validated<Self, IfcError> { Vector3::<f64>::read(reader, node, element).map(Self::from) }

    fn write(&self, writer: &mut Writer<'_>, node: Node) -> Result<(), IfcError> { self.coords.write(writer, node) }
}
