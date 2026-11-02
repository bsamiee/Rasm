use core::{iter, ptr};
use std::sync::Arc;

use color::{OpaqueColor, Srgb};
use indexmap::IndexMap;
use nalgebra::{Affine3, Matrix4, Point3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    math::Vector,
    shape::{Polyline, TriMesh, TriMeshBuilderError, TriMeshFlags},
};
use serde::Deserialize;
use serde_with::{base64::Base64, serde_as};
use validated::Validated;

use crate::{
    Composed, Fault, IfcError, Model, Path,
    schemas::{Attribute, attribute},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum Shown {
    Inherited,
    Invisible,
}

// --- [MODELS] --------------------------------------------------------------------------

// --- [INSTANCES]

#[derive(Debug, Clone, PartialEq)]
pub enum Points {
    Plain(Vec<Point3<f64>>),
    Colored(Vec<(Point3<f64>, OpaqueColor<Srgb>)>),
}

#[derive(Debug, Clone)]
pub enum Shape {
    Mesh(Arc<TriMesh>),
    Curve(Arc<Polyline>),
    Points(Arc<Points>),
}

#[derive(Debug, Clone)]
pub struct Instance {
    pub path: Path,
    pub node: Path,
    pub world: Affine3<f64>,
    pub shape: Shape,
    pub diffuse_color: Option<OpaqueColor<Srgb>>,
    pub opacity: f32,
    pub visible: bool,
}

struct Occurrence<'model> {
    path: Path,
    node: &'model Composed,
    world: Affine3<f64>,
    diffuse_color: Option<OpaqueColor<Srgb>>,
    opacity: f32,
    visible: bool,
}

// --- [ATTRIBUTES]

#[derive(Deserialize)]
struct Transform {
    transform: [[f64; 4]; 4],
}

impl Attribute for Transform {
    const ID: &'static str = "usd::xformop";
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
struct Mesh {
    points: Vec<[f64; 3]>,
    face_vertex_indices: Vec<u32>,
}

impl Attribute for Mesh {
    const ID: &'static str = "usd::usdgeom::mesh";
}

#[derive(Deserialize)]
struct Curves {
    points: Vec<[f64; 3]>,
}

impl Attribute for Curves {
    const ID: &'static str = "usd::usdgeom::basiscurves";
}

#[derive(Deserialize)]
struct Visibility {
    visibility: Shown,
}

impl Attribute for Visibility {
    const ID: &'static str = "usd::usdgeom::visibility";
}

#[derive(Deserialize)]
struct PointArray {
    positions: Vec<[f64; 3]>,
    colors: Option<Vec<[f32; 3]>>,
}

impl Attribute for PointArray {
    const ID: &'static str = "points::array";
}

#[serde_as]
#[derive(Deserialize)]
struct PointText {
    #[serde_as(as = "Base64")]
    positions: Vec<u8>,
    #[serde_as(as = "Option<Base64>")]
    colors: Option<Vec<u8>>,
}

impl Attribute for PointText {
    const ID: &'static str = "points::base64";
}

#[derive(Deserialize)]
struct DiffuseColor([f32; 3]);

impl Attribute for DiffuseColor {
    const ID: &'static str = "bsi::ifc::presentation::diffuseColor";
}

#[derive(Deserialize)]
struct Opacity(f32);

impl Attribute for Opacity {
    const ID: &'static str = "bsi::ifc::presentation::opacity";
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn instances(model: &Model, flags: TriMeshFlags) -> Result<Vec<Instance>, NEVec<IfcError>> {
    let fraction = |value: f32| (0.0..=1.0).contains(&value);
    let malformed = |node: &Composed, id: &str| IfcError::Attribute { node: node.node.clone(), attribute: id.into(), reason: Fault::Malformed };
    let shapes = |node: &Composed| -> Validated<Vec<Shape>, IfcError> {
        let cloud = |id: &str, positions: Vec<Point3<f64>>, colors: Option<Vec<[f32; 3]>>| match colors {
            None => Ok(Some(Shape::Points(Arc::new(Points::Plain(positions))))),
            Some(colors) if colors.len() == positions.len() && colors.as_flattened().iter().copied().all(fraction) =>
                Ok(Some(Shape::Points(Arc::new(Points::Colored(iter::zip(positions, colors.into_iter().map(OpaqueColor::new)).collect()))))),
            Some(_) => Err(malformed(node, id)),
        };
        let triples = |bytes: &[u8]| {
            let (words, []) = bytes.as_chunks::<4>() else { return None };
            let (vectors, []) = words.as_chunks::<3>() else { return None };
            vectors.iter().map(|vector| Some(vector.map(f32::from_le_bytes)).filter(|triple| triple.iter().all(|component| component.is_finite()))).try_collect::<Vec<_>>()
        };
        let text = |PointText { positions, colors }: PointText| match (triples(&positions), colors.as_deref().map(triples)) {
            (Some(positions), colors @ (None | Some(Some(_)))) => cloud(PointText::ID, positions.into_iter().map(|position| Point3::from(position.map(f64::from))).collect(), colors.flatten()),
            (None, _) | (Some(_), Some(None)) => Err(malformed(node, PointText::ID)),
        };
        let surface = |Mesh { points, face_vertex_indices }: Mesh| {
            if face_vertex_indices.is_empty() {
                Ok(None)
            } else if let (triangles, []) = face_vertex_indices.as_chunks::<3>()
                && face_vertex_indices.iter().all(|index| usize::try_from(*index).is_ok_and(|vertex| vertex < points.len()))
            {
                TriMesh::new(points.into_iter().map(Vector::from_array).collect(), triangles.to_vec())
                    .and_then(|mut mesh| mesh.set_flags(flags).map_err(TriMeshBuilderError::TopologyError).map(|()| Some(Shape::Mesh(Arc::new(mesh)))))
                    .map_err(|source| IfcError::Mesh { node: node.node.clone(), source })
            } else {
                Err(malformed(node, Mesh::ID))
            }
        };
        [
            attribute::<Mesh>(node).and_then(|record| record.map_or(Ok(None), surface)),
            attribute::<Curves>(node).map(|record| record.map(|Curves { points }| Shape::Curve(Arc::new(Polyline::new(points.into_iter().map(Vector::from_array).collect(), None))))),
            attribute::<PointArray>(node)
                .and_then(|record| record.map_or(Ok(None), |PointArray { positions, colors }| cloud(PointArray::ID, positions.into_iter().map(Point3::from).collect(), colors))),
            attribute::<PointText>(node).and_then(|record| record.map_or(Ok(None), text)),
        ]
        .into_iter()
        .collect::<Validated<Vec<_>, _>>()
        .map(|shapes| shapes.into_iter().flatten().collect())
    };
    let own = |node: &Composed| {
        let local = attribute::<Transform>(node).and_then(|record| {
            record
                .map_or(Ok(Affine3::identity()), |Transform { transform }| nalgebra::try_convert::<Matrix4<f64>, Affine3<f64>>(Matrix4::from(transform)).ok_or_else(|| malformed(node, Transform::ID)))
        });
        let diffuse_color = attribute::<DiffuseColor>(node).and_then(|record| {
            record.map(|DiffuseColor(components)| components.into_iter().all(fraction).ok_or_else(|| malformed(node, DiffuseColor::ID)).map(|()| OpaqueColor::new(components))).transpose()
        });
        let opacity = attribute::<Opacity>(node).and_then(|record| record.map(|Opacity(alpha)| fraction(alpha).ok_or_else(|| malformed(node, Opacity::ID)).map(|()| alpha)).transpose());
        let shown = attribute::<Visibility>(node).map(|record| record.is_none_or(|Visibility { visibility }| matches!(visibility, Shown::Inherited)));
        Validated::from(local).map4(Validated::from(diffuse_color), Validated::from(opacity), Validated::from(shown), |local, diffuse_color, opacity, shown| (local, diffuse_color, opacity, shown))
    };
    let walked = model
        .roots()
        .flat_map(|root| {
            let mut pending = Vec::from([Occurrence { path: root.node.clone(), node: root, world: Affine3::identity(), diffuse_color: None, opacity: 1.0, visible: true }]);
            iter::from_fn(move || {
                let inherited = pending.pop()?;
                Some(match own(inherited.node) {
                    Validated::Good((local, diffuse_color, opacity, shown)) => {
                        let occurrence = Occurrence {
                            world: Affine3::from_matrix_unchecked(inherited.world.matrix() * local.matrix()),
                            diffuse_color: diffuse_color.or(inherited.diffuse_color),
                            opacity: opacity.unwrap_or(inherited.opacity),
                            visible: inherited.visible && shown,
                            ..inherited
                        };
                        pending.extend(occurrence.node.children.iter().rev().map(|(name, child)| Occurrence { path: occurrence.path.child(name.clone()), node: child, ..occurrence }));
                        Validated::Good(occurrence)
                    }
                    Validated::Fail(failures) => Validated::Fail(failures),
                })
            })
        })
        .collect::<Vec<_>>();
    let decoded = walked
        .iter()
        .flatten()
        .map(|occurrence| (ptr::from_ref(occurrence.node), occurrence.node))
        .collect::<IndexMap<_, _>>()
        .into_iter()
        .map(|(definition, node)| shapes(node).map(|shapes| (definition, shapes)))
        .collect::<Validated<IndexMap<_, _>, _>>();
    walked
        .into_iter()
        .collect::<Validated<Vec<_>, _>>()
        .map2(decoded, |occurrences, decoded| {
            occurrences
                .into_iter()
                .flat_map(|Occurrence { path, node, world, diffuse_color, opacity, visible }| {
                    decoded.get(&ptr::from_ref(node)).into_iter().flatten().map(move |shape| Instance {
                        path: path.clone(),
                        node: node.node.clone(),
                        world,
                        shape: shape.clone(),
                        diffuse_color,
                        opacity,
                        visible,
                    })
                })
                .collect()
        })
        .ok()
}
