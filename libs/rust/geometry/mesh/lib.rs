#![feature(default_field_values, float_conversions, trait_alias, vec_into_chunks)]

use core::{borrow::Borrow, cell::LazyCell, iter, num::NonZero};

use bounded_integer::{BoundedU16, BoundedU32};
use manifold_rust::{
    cancel::{self, CancelToken},
    cross_section::CrossSection,
    linalg::{Mat3x4, Vec3},
    manifold::Manifold,
    progress::ProgressReporter,
    robust::{self, repair, soup},
    types::{Error, MeshGLP, Polygons, WindingRule},
};
use nalgebra::{Affine3, Matrix4, Point2, Point3, Scale2};
use parry3d_f64::{
    math::Vector,
    shape::{TriMesh, TriMeshBuilderError, TriMeshFlags},
};
use rayon::iter::{IntoParallelRefMutIterator as _, ParallelIterator as _};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Profile = IntoIterator<Item: IntoIterator<Item: Borrow<Point2<f64>>>>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Operation {
    Import,
    Extrude,
    Revolve,
    Transform,
    Warp,
    Boolean,
    Hull,
    Minkowski,
    Trim,
    Refine,
    Simplify,
    Export,
    Reduce,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "kebab-case")]
pub enum Defect {
    SelfIntersecting,
    Inverted,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Tolerance(f64);

#[nutype::nutype(validate(predicate = |scale| scale.vector.iter().all(|factor| *factor >= 0.0)), derive(Debug, Clone, Copy, PartialEq))]
pub struct Taper(Scale2<f64>);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Import<'faces> {
    pub tolerance: Option<Tolerance> = None,
    pub repair: Option<WindingRule> = None,
    pub face_id: Option<&'faces [BoundedU32<0, { i32::MAX.cast_unsigned() }>]> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Extrusion {
    pub n_divisions: u16 = 0,
    pub twist: f64 = 0.0,
    pub scale_top: Option<Taper> = None,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct Source {
    pub original: Option<NonZero<u32>>,
    pub run: u32,
    pub face: u32,
}

#[derive(Clone, derive_more::Debug)]
pub struct Solid(#[debug(skip)] Manifold);

impl Solid {
    pub fn new(mesh: &TriMesh, import: Import<'_>, token: Option<&CancelToken>, progress: Option<&ProgressReporter>) -> Result<Option<Self>, MeshError> {
        let Import { tolerance, repair, face_id } = import;
        let mut buffers = MeshGLP::<f64, u32> {
            num_prop: 3,
            vert_properties: mesh.vertices().iter().flat_map(Vector::to_array).collect(),
            tri_verts: mesh.indices().as_flattened().to_vec(),
            face_id: face_id.unwrap_or_default().iter().map(|face| face.get()).collect(),
            tolerance: crate::tolerance(tolerance),
            ..MeshGLP::default()
        };
        buffers.merge();
        let checked = |manifold| Self::checked(Operation::Import, manifold, token);
        let Some(rule) = repair else {
            return checked(Manifold::from_mesh_glp(&buffers));
        };
        let imported = Manifold::from_mesh_glp_robust(&buffers);
        let oriented = match rule {
            WindingRule::Positive => imported.clone(),
            WindingRule::Nonzero => imported.repair_orientation_with_token(token),
        };
        match checked(oriented) {
            Err(MeshError::Invalid { operation: _, defect: _ } | MeshError::Kernel { operation: _, status: Error::NotManifold }) if imported.status() == Error::NoError =>
                checked(Manifold::from_impl(robust::rebuild_with_rule(imported.as_impl(), rule, token, progress))),
            result @ (Ok(_) | Err(MeshError::Kernel { operation: _, status: _ } | MeshError::Invalid { operation: _, defect: _ } | MeshError::Mesh { operation: _, source: _ })) => result,
        }
    }

    pub fn extrude(profile: impl Profile, height: f64, extrusion: Extrusion, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError> {
        let Extrusion { n_divisions, twist, scale_top } = extrusion;
        let top = scale_top.map_or_else(Scale2::identity, Taper::into_inner);
        Self::checked(
            Operation::Extrude,
            mirrored(Manifold::extrude(&polygons(profile), height.abs(), i32::from(n_divisions), twist.to_degrees(), coordinates(top.vector)), height, Vec3::new(0.0, 0.0, 1.0)),
            token,
        )
    }

    pub fn revolve(profile: impl Profile, circular_segments: BoundedU16<3, { u16::MAX }>, angle: f64, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Revolve, mirrored(Manifold::revolve(&polygons(profile), i32::from(circular_segments.get()), angle.abs().to_degrees()), angle, Vec3::new(0.0, 1.0, 0.0)), token)
    }

    pub fn hull(points: impl IntoIterator<Item: Borrow<Point3<f64>>>) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Hull, Manifold::hull(&points.into_iter().map(|point| coordinates(*point.borrow())).collect::<Vec<Vec3>>()), None)
    }

    pub fn hull_solids<'solids>(solids: impl IntoIterator<Item = &'solids Self>) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Hull, Manifold::hull(&solids.into_iter().flat_map(|solid| solid.0.as_impl().vert_pos.iter().copied()).collect::<Vec<_>>()), None)
    }

    fn checked(operation: Operation, manifold: Manifold, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError> {
        let (intersections, nesting) = match operation {
            Operation::Hull | Operation::Export | Operation::Reduce => (false, false),
            Operation::Import | Operation::Warp => (true, true),
            Operation::Extrude | Operation::Revolve | Operation::Transform | Operation::Boolean | Operation::Minkowski | Operation::Trim | Operation::Refine | Operation::Simplify => (true, false),
        };
        let surface = manifold.as_impl();
        let intersecting = LazyCell::new(|| intersections && soup::has_self_intersections_with_token(surface, token));
        match manifold.status() {
            Error::NoError if surface.is_soup => Err(MeshError::Kernel { operation, status: Error::NotManifold }),
            Error::NoError if !surface.is_finite() => Err(MeshError::Kernel { operation, status: Error::NonFiniteVertex }),
            Error::NoError if manifold.volume() <= manifold.get_epsilon() * manifold.surface_area() => Ok(None),
            Error::NoError if *intersecting && cancel::is_cancelled(token) => Err(MeshError::Kernel { operation, status: Error::Cancelled }),
            Error::NoError if *intersecting => Err(MeshError::Invalid { operation, defect: Defect::SelfIntersecting }),
            Error::NoError if nesting && !repair::shells_well_nested(&soup::impl_to_tris(surface)) => Err(MeshError::Invalid { operation, defect: Defect::Inverted }),
            Error::NoError => Ok(Some(Self(manifold))),
            status => Err(MeshError::Kernel { operation, status }),
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum MeshError {
    #[error("{operation} failed: {status}")]
    Kernel { operation: Operation, status: Error },
    #[error("{operation} produced a surface that is {defect}")]
    Invalid { operation: Operation, defect: Defect },
    #[error("{operation} built no triangle mesh")]
    Mesh { operation: Operation, source: TriMeshBuilderError },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn transform(solid: &Solid, placement: &Affine3<f64>, token: Option<&CancelToken>) -> Result<Option<Solid>, MeshError> {
    Solid::checked(Operation::Transform, solid.0.transform(&columns(placement.matrix())), token)
}

pub fn warp<F>(solid: &Solid, warp: F, token: Option<&CancelToken>) -> Result<Option<Solid>, MeshError>
where F: Fn(Point3<f64>) -> Point3<f64> + Sync {
    Solid::checked(Operation::Warp, solid.0.warp_batch(|vertices| vertices.par_iter_mut().for_each(|vertex| *vertex = coordinates(warp(coordinates(*vertex))))), token)
}

#[must_use]
pub fn as_original(solid: &Solid) -> Solid { Solid(solid.0.as_original()) }

#[must_use]
pub fn original(solid: &Solid) -> Option<NonZero<u32>> { original_id(solid.0.original_id()) }

pub fn export(solid: &Solid) -> Result<(TriMesh, Vec<Source>), MeshError> {
    let buffers = solid.0.get_mesh_glp::<f64, u32>(-1);
    let sources = iter::zip(0_u32.., buffers.run_index.array_windows::<2>().zip(&buffers.run_original_id))
        .flat_map(|(run, (&[start, end], &original))| iter::repeat_n((NonZero::new(original), run), (end.strict_sub(start) / 3) as usize))
        .zip(&buffers.face_id)
        .map(|((original, run), &face)| Source { original, run, face })
        .collect();
    trimesh(buffers.vert_properties.into_chunks::<3>().into_iter().map(Vector::from_array).collect(), buffers.tri_verts, TriMeshFlags::empty(), Operation::Export).map(|mesh| (mesh, sources))
}

fn mirrored(manifold: Manifold, signed: f64, normal: Vec3) -> Manifold { if signed.is_sign_negative() { manifold.mirror(normal).as_original() } else { manifold } }

fn polygons(profile: impl Profile) -> Polygons { CrossSection::new(profile.into_iter().map(|ring| ring.into_iter().map(|vertex| coordinates(*vertex.borrow())).collect()).collect()).to_polygons() }

fn columns(placement: &Matrix4<f64>) -> Mat3x4 {
    let [x, y, z, w] = <[[f64; 3]; 4]>::from(placement.fixed_view::<3, 4>(0, 0)).map(Vec3::from);
    Mat3x4::from_cols(x, y, z, w)
}

fn coordinates<const N: usize, B>(value: impl Into<[f64; N]>) -> B
where B: From<[f64; N]> {
    B::from(value.into())
}

fn original_id(original_id: i32) -> Option<NonZero<u32>> { NonZero::new(original_id).filter(|id| id.is_positive()).map(NonZero::<i32>::cast_unsigned) }

fn tolerance(tolerance: Option<Tolerance>) -> f64 { tolerance.map_or_default(Tolerance::into_inner) }

fn trimesh(vertices: Vec<Vector>, indices: Vec<u32>, flags: TriMeshFlags, operation: Operation) -> Result<TriMesh, MeshError> {
    TriMesh::new(vertices, indices.into_chunks::<3>())
        .and_then(|mut mesh| mesh.set_flags(flags).map_err(TriMeshBuilderError::TopologyError).map(|()| mesh))
        .map_err(|source| MeshError::Mesh { operation, source })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod booleans;
pub mod decomposition;
pub mod measures;
pub mod reduction;
pub mod refinement;
pub mod sections;
