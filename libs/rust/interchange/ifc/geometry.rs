use core::ptr;
use std::{iter, sync::Arc};

use color::{AlphaColor, OpaqueColor, Srgb};
use indexmap::IndexMap;
use nalgebra::{Affine3, Matrix4, Point3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    math::Vector,
    shape::{Polyline, TriMesh},
};
use serde::Deserialize;
use serde_with::{base64::Base64, serde_as};
use validated::Validated;

use crate::{
    Composed, IfcError, Model, Path,
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
    pub presentation: Option<AlphaColor<Srgb>>,
}

struct Occurrence<'model> {
    path: Path,
    node: &'model Composed,
    world: Affine3<f64>,
    presentation: Option<AlphaColor<Srgb>>,
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

pub fn instances(model: &Model) -> Result<Vec<Instance>, NEVec<IfcError>> {
    fn walk<'model>(node: &'model Composed, path: &Path, parent: &Affine3<f64>, inherited: Option<AlphaColor<Srgb>>) -> Vec<Result<Occurrence<'model>, IfcError>> {
        let placed = attribute::<Transform>(node).and_then(|record| {
            record.map_or_else(
                || Ok(Affine3::identity()),
                |Transform { transform }| {
                    nalgebra::try_convert::<Matrix4<f64>, Affine3<f64>>(Matrix4::from(transform)).ok_or_else(|| IfcError::Malformed { node: node.node.clone(), attribute: Transform::ID.into() })
                },
            )
        });
        let painted = attribute::<DiffuseColor>(node).and_then(|record| {
            record.map(|DiffuseColor(components)| attribute::<Opacity>(node).map(|opacity| OpaqueColor::new(components).with_alpha(opacity.map_or(1.0, |Opacity(alpha)| alpha)))).transpose()
        });
        match Validated::from(placed).map3(Validated::from(attribute::<Visibility>(node)), Validated::from(painted), |local, visibility, own| (local, visibility, own)) {
            Validated::Fail(failures) => failures.into_iter().map(Err).collect(),
            Validated::Good((_, Some(Visibility { visibility: Shown::Invisible }), _)) => Vec::new(),
            Validated::Good((local, Some(Visibility { visibility: Shown::Inherited }) | None, own)) => {
                let world = parent * local;
                let presentation = own.or(inherited);
                iter::once(Ok(Occurrence { path: path.clone(), node, world, presentation }))
                    .chain(node.children.iter().flat_map(|(name, child)| walk(child, &path.child(name.clone()), &world, presentation)))
                    .collect()
            }
        }
    }
    let triples = |bytes: &[u8]| -> Option<Vec<[f32; 3]>> {
        let (words, []) = bytes.as_chunks::<4>() else { return None };
        let (vectors, []) = words.as_chunks::<3>() else { return None };
        Some(vectors.iter().map(|vector| vector.map(f32::from_le_bytes)).collect())
    };
    let decode = |node: &Composed| -> Validated<Option<Shape>, IfcError> {
        let malformed = |id: &str| IfcError::Malformed { node: node.node.clone(), attribute: id.into() };
        let points = |id: &str, positions: Vec<[f64; 3]>, colors: Option<Vec<[f32; 3]>>| match colors {
            None => Ok(Some(Shape::Points(Arc::new(Points::Plain(positions.into_iter().map(Point3::from).collect()))))),
            Some(colors) if colors.len() == positions.len() => {
                Ok(Some(Shape::Points(Arc::new(Points::Colored(positions.into_iter().map(Point3::from).zip(colors.into_iter().map(OpaqueColor::new)).collect())))))
            }
            Some(_) => Err(malformed(id)),
        };
        let decoded = Validated::from(attribute::<Mesh>(node)).map4(
            Validated::from(attribute::<Curves>(node)),
            Validated::from(attribute::<PointArray>(node)),
            Validated::from(attribute::<PointText>(node)),
            |mesh, curves, array, text| (mesh, curves, array, text),
        );
        match decoded {
            Validated::Fail(failures) => Validated::Fail(failures),
            Validated::Good((Some(Mesh { points: vertices, face_vertex_indices }), _, _, _)) => {
                let (triangles, remainder) = face_vertex_indices.as_chunks::<3>();
                Validated::from(
                    (remainder.is_empty() && face_vertex_indices.iter().all(|index| usize::try_from(*index).is_ok_and(|vertex| vertex < vertices.len())))
                        .ok_or_else(|| malformed(Mesh::ID))
                        .and_then(|()| {
                            TriMesh::new(vertices.into_iter().map(Vector::from_array).collect(), triangles.to_vec())
                                .map(|mesh| Some(Shape::Mesh(Arc::new(mesh))))
                                .map_err(|source| IfcError::Mesh { node: node.node.clone(), source })
                        }),
                )
            }
            Validated::Good((None, Some(Curves { points: vertices }), _, _)) => {
                Validated::Good(Some(Shape::Curve(Arc::new(Polyline::new(vertices.into_iter().map(Vector::from_array).collect(), None)))))
            }
            Validated::Good((None, None, Some(PointArray { positions, colors }), _)) => Validated::from(points(PointArray::ID, positions, colors)),
            Validated::Good((None, None, None, Some(PointText { positions, colors }))) => Validated::from(match (triples(&positions), colors.as_deref().map(triples)) {
                (Some(positions), colors @ (None | Some(Some(_)))) => points(PointText::ID, positions.into_iter().map(|position| position.map(f64::from)).collect(), colors.flatten()),
                (None, _) | (Some(_), Some(None)) => Err(malformed(PointText::ID)),
            }),
            Validated::Good((None, None, None, None)) => Validated::Good(None),
        }
    };
    let occurrences = model.roots().flat_map(|root| walk(root, &root.node, &Affine3::identity(), None)).collect::<Validated<Vec<_>, _>>().ok()?;
    let shapes = occurrences
        .iter()
        .map(|occurrence| (ptr::from_ref(occurrence.node), occurrence.node))
        .collect::<IndexMap<_, _>>()
        .into_iter()
        .map(|(definition, node)| decode(node).map(|shape| (definition, shape)))
        .collect::<Validated<IndexMap<_, _>, _>>()
        .ok()?;
    Ok(occurrences
        .into_iter()
        .filter_map(|Occurrence { path, node, world, presentation }| {
            shapes.get(&ptr::from_ref(node)).cloned().flatten().map(|shape| Instance { path, node: node.node.clone(), world, shape, presentation })
        })
        .collect())
}
