use core::iter;
use std::{collections::HashMap, sync::Arc};

use color::{AlphaColor, OpaqueColor, Srgb};
use indexmap::IndexMap;
use itertools::Itertools as _;
use nalgebra::{Affine3, Matrix4, Point3};
use nonempty_collections::NEVec;
use parry3d_f64::{
    math::Vector,
    shape::{Polyline, Segment, TriMesh, TriMeshBuilderError, TriMeshFlags},
};
use pcd_rs::{DataKind, DynReader, DynRecord, Field, Rgb};
use serde::{Deserialize, Serialize};
use serde_with::{base64::Base64, serde_as};
use validated::Validated;

use crate::{
    Composed, Constraint, Fault, IfcError, Model, Path, Value, bind, distinct,
    schemas::{Attribute, attribute, encoded},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize)]
#[serde(rename_all = "lowercase")]
enum VisibilityToken {
    Inherited,
    Invisible,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "UPPERCASE")]
pub enum AlphaMode {
    Opaque,
    Mask,
    Blend,
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

#[derive(Debug, Clone, PartialEq)]
pub struct Placement {
    pub world: Affine3<f64>,
    pub color: Option<AlphaColor<Srgb>>,
    pub material: Option<Arc<Material>>,
    pub visible: bool,
}

#[derive(Debug, Clone)]
pub struct Instance {
    pub path: Path,
    pub node: Path,
    pub shape: Shape,
    pub placement: Placement,
}

#[derive(Debug, Clone, PartialEq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Material {
    pub pbr_metallic_roughness: Option<PbrMetallicRoughness>,
    pub normal_texture: Option<TextureInfo>,
    pub occlusion_texture: Option<TextureInfo>,
    pub emissive_texture: Option<Box<str>>,
    pub emissive_factor: Option<[f32; 3]>,
    pub alpha_mode: Option<AlphaMode>,
    pub alpha_cutoff: Option<f32>,
    pub double_sided: Option<bool>,
}

impl Attribute for Material {
    const ID: &'static str = "gltf::material";
}

#[derive(Debug, Clone, PartialEq, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct PbrMetallicRoughness {
    pub base_color_factor: Option<[f32; 4]>,
    pub base_color_texture: Option<Box<str>>,
    pub metallic_factor: Option<f32>,
    pub roughness_factor: Option<f32>,
    pub metallic_roughness_texture: Option<Box<str>>,
}

#[derive(Debug, Clone, PartialEq, Deserialize)]
pub struct TextureInfo {
    pub texture: Box<str>,
    #[serde(alias = "strength")]
    pub scale: Option<f32>,
}

#[nutype::nutype(
    validate(with = |value: &f32| -> Result<(), IfcError> { (0.0..=1.0).contains(value).ok_or(IfcError::Constrained(Constraint::Fraction(*value))) }, error = IfcError),
    derive(Debug, Clone, Copy, PartialEq, Deserialize)
)]
struct Fraction(f32);

// --- [ATTRIBUTES]

#[derive(Serialize, Deserialize)]
struct Transform {
    transform: [[f64; 4]; 4],
}

impl Attribute for Transform {
    const ID: &'static str = "usd::xformop";
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
struct Mesh {
    points: Vec<[f64; 3]>,
    face_vertex_indices: Vec<u32>,
}

impl Attribute for Mesh {
    const ID: &'static str = "usd::usdgeom::mesh";
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
struct Curves {
    points: Vec<[f64; 3]>,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    curve_vertex_counts: Option<Vec<u32>>,
}

impl Attribute for Curves {
    const ID: &'static str = "usd::usdgeom::basiscurves";
}

#[derive(Deserialize)]
struct Visibility {
    visibility: VisibilityToken,
}

impl Attribute for Visibility {
    const ID: &'static str = "usd::usdgeom::visibility";
}

#[derive(Serialize, Deserialize)]
struct PointArray {
    positions: Vec<[f64; 3]>,
    #[serde(skip_serializing_if = "Option::is_none")]
    colors: Option<Vec<[f32; 3]>>,
}

impl Attribute for PointArray {
    const ID: &'static str = "points::array";
}

#[serde_as]
#[derive(Deserialize)]
struct PointBase64 {
    #[serde_as(as = "Base64")]
    positions: Vec<u8>,
    #[serde_as(as = "Option<Base64>")]
    colors: Option<Vec<u8>>,
}

impl Attribute for PointBase64 {
    const ID: &'static str = "points::base64";
}

#[serde_as]
#[derive(Deserialize)]
struct PointCloud(#[serde_as(as = "Base64")] Vec<u8>);

impl Attribute for PointCloud {
    const ID: &'static str = "pcd::base64";
}

#[derive(Deserialize)]
struct DiffuseColor([Fraction; 3]);

impl Attribute for DiffuseColor {
    const ID: &'static str = "bsi::ifc::presentation::diffuseColor";
}

#[derive(Deserialize)]
struct Opacity(Fraction);

impl Attribute for Opacity {
    const ID: &'static str = "bsi::ifc::presentation::opacity";
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODE]

pub fn instances(model: &Model, scope: Option<&[Path]>, flags: TriMeshFlags) -> Result<Vec<Instance>, NEVec<IfcError>> {
    type Decode<'form> = &'form dyn Fn(&Composed) -> Validated<Option<Shape>, IfcError>;
    fn form<T>(node: &Composed, shape: impl FnOnce(T) -> Validated<Option<Shape>, IfcError>) -> Validated<Option<Shape>, IfcError>
    where T: Attribute {
        attribute::<T>(node).map_or_else(Validated::fail, |record| record.map_or(Validated::Good(None), shape))
    }
    let cloud = |node: &Composed, id: &str, positions: Vec<Point3<f64>>, colors: Option<Vec<[f32; 3]>>| -> Result<Option<Shape>, IfcError> {
        let refused = |reason| IfcError::Attribute { node: node.path.clone(), attribute: id.into(), reason };
        match colors {
            _ if positions.is_empty() => Ok(None),
            None => Ok(Some(Shape::Points(Arc::new(Points::Plain(positions))))),
            Some(colors) if colors.len() != positions.len() => Err(refused(Fault::Colors { colors: colors.len(), points: positions.len() })),
            Some(colors) => colors.as_flattened().iter().find(|component| !(0.0..=1.0).contains(*component)).map_or_else(
                || Ok(Some(Shape::Points(Arc::new(Points::Colored(iter::zip(positions, colors.iter().copied().map(OpaqueColor::new)).collect()))))),
                |&outside| Err(refused(Fault::Range { value: f64::from(outside) })),
            ),
        }
    };
    let triples = |node: &Composed, bytes: &[u8]| -> Result<Vec<[f32; 3]>, IfcError> {
        let refused = |reason| IfcError::Attribute { node: node.path.clone(), attribute: PointBase64::ID.into(), reason };
        let (words, []) = bytes.as_chunks::<4>() else { return Err(refused(Fault::Partial { count: bytes.len() })) };
        let (vectors, []) = words.as_chunks::<3>() else { return Err(refused(Fault::Partial { count: words.len() })) };
        vectors.iter().map(|vector| vector.map(f32::from_le_bytes)).map(|triple| triple.iter().find(|component| !component.is_finite()).map_or(Ok(triple), |&outside| Err(refused(Fault::Range { value: f64::from(outside) })))).collect::<Result<Vec<_>, _>>()
    };
    let point_cloud = |node: &Composed, PointCloud(bytes): PointCloud| {
        let failed = |source| IfcError::Cloud { node: node.path.clone(), source };
        let absent = |field| IfcError::Attribute { node: node.path.clone(), attribute: PointCloud::ID.into(), reason: Fault::Field(field) };
        bind(Validated::from(DynReader::from_bytes(&bytes).map_err(failed)), |reader| {
            let located = |field: &'static str| reader.meta().field_defs.iter().position(|definition| definition.name == field).map(|index| (field, index));
            let column = |field: &'static str| Validated::from(located(field).ok_or_else(|| absent(field)));
            let rgb = located("rgb");
            let ascii = matches!(reader.meta().data, DataKind::Ascii);
            let coordinate = |fields: &[Field], (name, index): (&'static str, usize)| {
                Validated::from(fields.get(index).and_then(|field| field.to_value::<f32>().map(f64::from).or_else(|| field.to_value::<f64>())).ok_or_else(|| absent(name)))
            };
            let color = |field: &Field| {
                let Rgb { r, g, b } = Rgb::from_packed(match (ascii, field.to_value::<f32>(), field.to_value::<u32>()) {
                    (false, Some(value), _) => Some(value),
                    (true, Some(value), _) => Some(f32::from_bits(value.to_int_saturating::<u32>())),
                    (_, None, Some(packed)) => Some(f32::from_bits(packed)),
                    (_, None, None) => None,
                }?);
                Some(OpaqueColor::<Srgb>::from_rgb8(r, g, b).components)
            };
            let point = |DynRecord(fields): DynRecord, [x, y, z]: [(&'static str, usize); 3]| {
                let tint = Validated::from(rgb.map(|(name, index)| fields.get(index).and_then(color).ok_or_else(|| absent(name))).transpose());
                coordinate(&fields, x).map4(coordinate(&fields, y), coordinate(&fields, z), tint, |east, north, up, shade| (Point3::new(east, north, up), shade))
            };
            bind(column("x").map3(column("y"), column("z"), |x, y, z| [x, y, z]), |axes| {
                let records = reader.map(|record| bind(Validated::from(record.map_err(failed)), |fields| point(fields, axes))).collect::<Validated<Vec<_>, _>>();
                bind(records, |records| {
                    let (positions, colors): (Vec<_>, Vec<_>) = records.into_iter().unzip();
                    Validated::from(cloud(node, PointCloud::ID, positions, colors.into_iter().collect()))
                })
            })
        })
    };
    let forms: [(&str, Decode<'_>); 5] = [
        (Mesh::ID, &|node: &Composed| {
            form::<Mesh>(node, |Mesh { points, face_vertex_indices }| {
                let refused = |reason| IfcError::Attribute { node: node.path.clone(), attribute: Mesh::ID.into(), reason };
                let (triangles, []) = face_vertex_indices.as_chunks::<3>() else { return Validated::fail(refused(Fault::Partial { count: face_vertex_indices.len() })) };
                if let Some(&index) = face_vertex_indices.iter().find(|index| !usize::try_from(**index).is_ok_and(|vertex| vertex < points.len())) {
                    return Validated::fail(refused(Fault::Face { index, points: points.len() }));
                }
                match triangles {
                    [] => Validated::Good(None),
                    [_, ..] => Validated::from(
                        TriMesh::new(points.into_iter().map(Vector::from_array).collect(), triangles.to_vec())
                            .and_then(|mut mesh| mesh.set_flags(flags).map_err(TriMeshBuilderError::TopologyError).map(|()| Some(Shape::Mesh(Arc::new(mesh)))))
                            .map_err(|source| IfcError::Mesh { node: node.path.clone(), source }),
                    ),
                }
            })
        }),
        (Curves::ID, &|node: &Composed| {
            form::<Curves>(node, |Curves { points, curve_vertex_counts }| {
                let vertices = points.into_iter().map(Vector::from_array).collect::<Vec<_>>();
                let indices = match curve_vertex_counts {
                    None => None,
                    Some(counts) if counts.iter().try_fold(0_u32, |stated, &count| stated.checked_add(count)).is_some_and(|stated| usize::try_from(stated).is_ok_and(|stated| stated == vertices.len())) => Some(
                        counts
                            .iter()
                            .scan(0_u32, |start, &count| {
                                let first = *start;
                                *start = first.strict_add(count);
                                Some(first..start.saturating_sub(1))
                            })
                            .flatten()
                            .map(|from| [from, from.strict_add(1)])
                            .collect::<Vec<_>>(),
                    ),
                    Some(_) => return Validated::fail(IfcError::Attribute { node: node.path.clone(), attribute: Curves::ID.into(), reason: Fault::Counts { points: vertices.len() } }),
                };
                Validated::Good(indices.as_ref().map_or(vertices.len() > 1, |segments| !segments.is_empty()).then(|| Shape::Curve(Arc::new(Polyline::new(vertices, indices)))))
            })
        }),
        (PointArray::ID, &|node: &Composed| form::<PointArray>(node, |PointArray { positions, colors }| Validated::from(cloud(node, PointArray::ID, positions.into_iter().map(Point3::from).collect(), colors)))),
        (PointBase64::ID, &|node: &Composed| {
            form::<PointBase64>(node, |PointBase64 { positions: position_bytes, colors: color_bytes }| {
                let decoded = Validated::from(triples(node, &position_bytes)).map2(Validated::from(color_bytes.as_deref().map(|colors| triples(node, colors)).transpose()), |positions, colors| (positions, colors));
                bind(decoded, |(positions, colors)| Validated::from(cloud(node, PointBase64::ID, positions.into_iter().map(|position| Point3::from(position.map(f64::from))).collect(), colors)))
            })
        }),
        (PointCloud::ID, &|node: &Composed| form::<PointCloud>(node, |record| point_cloud(node, record))),
    ];
    let own = |node: &Composed| {
        let local = attribute::<Transform>(node).and_then(|record| {
            record.map_or(Ok(Affine3::identity()), |Transform { transform }| {
                nalgebra::try_convert::<Matrix4<f64>, Affine3<f64>>(Matrix4::from(transform)).ok_or_else(|| IfcError::Attribute { node: node.path.clone(), attribute: Transform::ID.into(), reason: Fault::Singular })
            })
        });
        let color = attribute::<DiffuseColor>(node).and_then(|diffuse| {
            attribute::<Opacity>(node).map(|opacity| {
                diffuse.map(|DiffuseColor(components)| OpaqueColor::<Srgb>::new(components.map(Fraction::into_inner)).with_alpha(opacity.map_or(1.0, |Opacity(alpha)| alpha.into_inner())))
            })
        });
        let shown = attribute::<Visibility>(node).map(|record| record.is_none_or(|Visibility { visibility }| matches!(visibility, VisibilityToken::Inherited)));
        Validated::from(local).map4(Validated::from(color), Validated::from(attribute::<Material>(node)), Validated::from(shown), |local, color, material, shown| (local, color, material.map(Arc::new), shown))
    };
    let chains = match scope {
        None => model.roots.iter().map(|root| Ok((root.path.clone(), Vec::from([root])))).collect::<Validated<Vec<_>, IfcError>>(),
        Some(paths) => paths
            .iter()
            .map(|path| {
                let (head, names) = path.0.split_first();
                model
                    .heads
                    .get(head)
                    .and_then(|first| {
                        names.iter().try_fold(Vec::from([first]), |mut chain, name| {
                            let node = chain.last().copied()?.children.get(name)?;
                            chain.push(node);
                            Some(chain)
                        })
                    })
                    .map(|chain| (path.clone(), chain))
                    .ok_or_else(|| IfcError::Missing { path: path.clone() })
            })
            .collect(),
    };
    let chains = chains.ok()?;
    let subtrees = distinct(chains.iter().filter_map(|(_, chain)| chain.last().copied())).collect::<Vec<_>>();
    let placements = chains
        .iter()
        .flat_map(|(_, chain)| chain.iter().copied())
        .chain(subtrees.iter().copied())
        .map(|node| (Arc::as_ptr(node), node))
        .collect::<IndexMap<_, _>>()
        .into_iter()
        .map(|(definition, node)| own(node).map(|own| (definition, own)))
        .collect::<Validated<HashMap<_, _>, _>>();
    let shapes = subtrees
        .iter()
        .copied()
        .flat_map(|node| forms.iter().filter_map(move |&(id, decode)| node.attributes.get(id).map(|value| (Arc::as_ptr(value), (node, decode)))))
        .collect::<IndexMap<_, _>>()
        .into_iter()
        .map(|(value, (node, decode))| decode(node).map(|shape| (value, shape)))
        .collect::<Validated<HashMap<_, _>, _>>();
    let (placements, shapes) = placements.map2(shapes, |placements, shapes| (placements, shapes)).ok()?;
    let placed = |inherited: &Placement, node: &Arc<Composed>| {
        placements.get(&Arc::as_ptr(node)).map_or_else(
            || inherited.clone(),
            |(local, color, material, shown)| Placement { world: inherited.world * local, color: color.or(inherited.color), material: material.clone().or_else(|| inherited.material.clone()), visible: inherited.visible && *shown },
        )
    };
    let origin = Placement { world: Affine3::identity(), color: None, material: None, visible: true };
    let (placed, shapes, forms) = (&placed, &shapes, &forms);
    Ok(chains
        .into_iter()
        .flat_map(|(scoped, chain)| {
            let start = chain.last().copied().map(|node| (scoped, node, chain.iter().copied().fold(origin.clone(), |inherited, ancestor| placed(&inherited, ancestor))));
            let mut pending = start.into_iter().collect::<Vec<_>>();
            iter::from_fn(move || {
                let (path, node, placement) = pending.pop()?;
                pending.extend(node.children.iter().rev().map(|(name, child)| (path.child(name.clone()), child, placed(&placement, child))));
                Some((path, node, placement))
            })
        })
        .flat_map(|(path, node, placement)| {
            forms.iter().filter_map(move |&(id, _)| node.attributes.get(id).and_then(|value| shapes.get(&Arc::as_ptr(value)).cloned().flatten())).map(move |shape| Instance { path: path.clone(), node: node.path.clone(), shape, placement: placement.clone() })
        })
        .collect())
}

// --- [ENCODE]

pub fn shape_attribute(node: &Path, shape: &Shape) -> Result<(Box<str>, Option<Value>), IfcError> {
    match shape {
        Shape::Mesh(mesh) => encoded(node, &Mesh { points: mesh.vertices().iter().map(Vector::to_array).collect(), face_vertex_indices: mesh.indices().as_flattened().to_vec() }),
        Shape::Curve(curve) => {
            let runs = iter::zip(curve.indices(), curve.segments())
                .map(|(&[from, to], Segment { a: head, b: tail })| (from, to, Vec::from([head.to_array(), tail.to_array()])))
                .coalesce(|(start, end, mut run), (from, to, next)| {
                    if end == from {
                        run.extend(next.into_iter().skip(1));
                        Ok((start, to, run))
                    } else {
                        Err(((start, end, run), (from, to, next)))
                    }
                })
                .map(|(_, _, run)| run)
                .collect::<Vec<_>>();
            let curve_vertex_counts = runs.iter().map(|run| u32::try_from(run.len()).map_err(|source| IfcError::Vertices { node: node.clone(), count: run.len(), source })).collect::<Result<Vec<_>, _>>()?;
            let points = runs.into_iter().flatten().collect();
            encoded(node, &Curves { points, curve_vertex_counts: matches!(curve_vertex_counts.as_slice(), [_, _, ..]).then_some(curve_vertex_counts) })
        }
        Shape::Points(points) => encoded(node, &match points {
            Points::Plain(positions) => PointArray { positions: positions.iter().map(|position| position.coords.into()).collect(), colors: None },
            Points::Colored(colored) => PointArray { positions: colored.iter().map(|(position, _)| position.coords.into()).collect(), colors: Some(colored.iter().map(|(_, color)| color.components).collect()) },
        }),
    }
}

pub fn placement_attribute(node: &Path, world: &Affine3<f64>) -> Result<(Box<str>, Option<Value>), IfcError> { encoded(node, &Transform { transform: (*world.matrix()).into() }) }
