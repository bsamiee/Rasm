#![feature(default_field_values, trait_alias)]

use core::borrow::Borrow;

use bounded_integer::BoundedU16;
use manifold_rust::{
    cancel::CancelToken,
    cross_section::CrossSection,
    linalg::{Mat3x4, Vec2, Vec3},
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
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "kebab-case")]
pub enum Defect {
    SelfIntersecting,
    Inverted,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Import {
    pub tolerance: f64 = 0.0,
    pub repair: Option<WindingRule> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Extrusion {
    pub divisions: u16 = 0,
    pub twist: f64 = 0.0,
    pub scale: Scale2<f64> = Scale2::new(1.0_f64, 1.0_f64),
}

#[derive(Clone, derive_more::Debug)]
pub struct Solid(#[debug(skip)] Manifold);

impl Solid {
    pub fn new(mesh: &TriMesh, import: Import, token: Option<&CancelToken>, progress: Option<&ProgressReporter>) -> Result<Option<Self>, MeshError> {
        let Import { tolerance, repair } = import;
        let mut buffers = MeshGLP::<f64, u32> {
            num_prop: 3,
            vert_properties: mesh.vertices().iter().flat_map(Vector::to_array).collect(),
            tri_verts: mesh.indices().as_flattened().to_vec(),
            tolerance,
            ..MeshGLP::default()
        };
        buffers.merge();
        Self::oriented(
            Operation::Import,
            repair.map_or_else(
                || Manifold::from_mesh_glp(&buffers),
                |rule| {
                    let imported = Manifold::from_mesh_glp_robust(&buffers);
                    let rebuilt = |source: &Manifold| Manifold::from_impl(robust::rebuild_with_rule(source.as_impl(), rule, token, progress));
                    match (imported.status(), rule) {
                        (Error::NoError, WindingRule::Nonzero) if !imported.as_impl().is_soup && !soup::has_self_intersections_with_token(imported.as_impl(), token) =>
                            Some(imported.repair_orientation_with_token(token))
                                .filter(|repaired| repair::shells_well_nested(&soup::impl_to_tris(repaired.as_impl())))
                                .unwrap_or_else(|| rebuilt(&imported)),
                        (Error::NoError, WindingRule::Positive | WindingRule::Nonzero) => rebuilt(&imported),
                        (_, WindingRule::Positive | WindingRule::Nonzero) => imported,
                    }
                },
            ),
        )
    }

    pub fn extrude(profile: impl Profile, height: f64, extrusion: Extrusion) -> Result<Option<Self>, MeshError> {
        let Extrusion { divisions, twist, scale } = extrusion;
        Self::checked(Operation::Extrude, Manifold::extrude(&polygons(profile), height, i32::from(divisions), twist.to_degrees(), Vec2::from(<[f64; 2]>::from(scale.vector))))
    }

    pub fn revolve(profile: impl Profile, segments: BoundedU16<3, { u16::MAX }>, angle: f64) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Revolve, Manifold::revolve(&polygons(profile), i32::from(segments.get()), angle.to_degrees()))
    }

    pub fn transform(&self, placement: &Affine3<f64>) -> Result<Option<Self>, MeshError> { Self::checked(Operation::Transform, self.0.transform(&columns(placement.matrix()))) }

    pub fn warp<F>(&self, warp: F) -> Result<Option<Self>, MeshError>
    where F: Fn(Point3<f64>) -> Point3<f64> + Sync {
        Self::oriented(Operation::Warp, self.0.warp_batch(|vertices| vertices.par_iter_mut().for_each(|vertex| *vertex = Vec3::from(<[f64; 3]>::from(warp(Point3::from(<[f64; 3]>::from(*vertex))))))))
    }

    pub fn to_mesh(&self, flags: TriMeshFlags) -> Result<TriMesh, MeshError> {
        let (vertices, indices) = boundary(&self.0);
        trimesh(vertices, indices, flags)
    }

    fn checked(operation: Operation, manifold: Manifold) -> Result<Option<Self>, MeshError> {
        match manifold.status() {
            Error::NoError if manifold.volume() <= manifold.get_epsilon() * manifold.surface_area() => Ok(None),
            Error::NoError if manifold.has_self_intersections() => Err(MeshError::Invalid { operation, defect: Defect::SelfIntersecting }),
            Error::NoError => Ok(Some(Self(manifold))),
            status => Err(MeshError::Kernel { operation, status }),
        }
    }

    fn oriented(operation: Operation, manifold: Manifold) -> Result<Option<Self>, MeshError> {
        Self::checked(operation, manifold)?
            .map(|solid| repair::shells_well_nested(&soup::impl_to_tris(solid.0.as_impl())).then_some(solid).ok_or(MeshError::Invalid { operation, defect: Defect::Inverted }))
            .transpose()
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum MeshError {
    #[error("{operation} failed: {status}")]
    Kernel { operation: Operation, status: Error },
    #[error("{operation} produced a surface that is {defect}")]
    Invalid { operation: Operation, defect: Defect },
    #[error(transparent)]
    Mesh(#[from] TriMeshBuilderError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn polygons(profile: impl Profile) -> Polygons {
    CrossSection::new(profile.into_iter().map(|ring| ring.into_iter().map(|vertex| Vec2::from(<[f64; 2]>::from(*vertex.borrow()))).collect()).collect()).to_polygons()
}

fn columns(placement: &Matrix4<f64>) -> Mat3x4 {
    let [x, y, z, w] = <[[f64; 3]; 4]>::from(placement.fixed_view::<3, 4>(0, 0)).map(Vec3::from);
    Mat3x4::from_cols(x, y, z, w)
}

fn boundary(manifold: &Manifold) -> (Vec<Vector>, Vec<[u32; 3]>) {
    let buffers = manifold.get_mesh_glp::<f64, u32>(-1);
    (Vec::from_fn(buffers.num_vert(), |vertex| Vector::from_array(buffers.get_vert_pos(vertex))), buffers.tri_verts.as_chunks::<3>().0.to_vec())
}

fn trimesh(vertices: Vec<Vector>, indices: Vec<[u32; 3]>, flags: TriMeshFlags) -> Result<TriMesh, MeshError> {
    let mut mesh = TriMesh::new(vertices, indices)?;
    mesh.set_flags(flags).map_err(TriMeshBuilderError::TopologyError)?;
    Ok(mesh)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod booleans;
pub mod decomposition;
pub mod measures;
pub mod reduction;
pub mod refinement;
pub mod sections;
