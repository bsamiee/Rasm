#![feature(default_field_values, trait_alias, vec_into_chunks)]

use core::{borrow::Borrow, num::NonZero};
use std::collections::BTreeSet;

use bounded_integer::BoundedU16;
use manifold_rust::{
    cancel::{self, CancelToken},
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
    shape::{TriMesh, TriMeshBuilderError},
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

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, derive_more::Display)]
#[display(rename_all = "kebab-case")]
pub enum Defect {
    SelfIntersecting,
    Inverted,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Tolerance(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Import<'faces> {
    pub tolerance: Option<Tolerance> = None,
    pub repair: Option<WindingRule> = None,
    pub face_id: Option<&'faces [u32]> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Extrusion {
    pub n_divisions: u16 = 0,
    pub twist: f64 = 0.0,
    pub scale_top: Scale2<f64> = Scale2::new(1.0_f64, 1.0_f64),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Source {
    pub original: Option<NonZero<u32>>,
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
            face_id: face_id.map_or_default(<[u32]>::to_vec),
            tolerance: tolerance.map_or_default(Tolerance::into_inner),
            ..MeshGLP::default()
        };
        buffers.merge();
        let clean = |manifold: &Manifold| manifold.status() == Error::NoError && !manifold.as_impl().is_soup && !soup::has_self_intersections_with_token(manifold.as_impl(), token);
        let nested = |manifold: &Manifold| repair::shells_well_nested(&soup::impl_to_tris(manifold.as_impl()));
        match repair.map(|rule| (rule, Manifold::from_mesh_glp_robust(&buffers))) {
            None => Self::checked(Operation::Import, Manifold::from_mesh_glp(&buffers), token, Some(Defect::Inverted)),
            Some((WindingRule::Positive, imported)) if clean(&imported) && nested(&imported) => Self::checked(Operation::Import, imported, token, None),
            Some((WindingRule::Nonzero, imported))
                if clean(&imported)
                    && let repaired = imported.repair_orientation_with_token(token)
                    && nested(&repaired) =>
                Self::checked(Operation::Import, repaired, token, None),
            Some((rule, imported)) if imported.status() == Error::NoError =>
                Self::checked(Operation::Import, Manifold::from_impl(robust::rebuild_with_rule(imported.as_impl(), rule, token, progress)), token, Some(Defect::Inverted)),
            Some((_, imported)) => Self::checked(Operation::Import, imported, token, None),
        }
    }

    pub fn extrude(profile: impl Profile, height: f64, extrusion: Extrusion, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError> {
        let Extrusion { n_divisions, twist, scale_top } = extrusion;
        Self::checked(
            Operation::Extrude,
            Manifold::extrude(&polygons(profile), height, i32::from(n_divisions), twist.to_degrees(), Vec2::from(<[f64; 2]>::from(scale_top.vector))),
            token,
            Some(Defect::SelfIntersecting),
        )
    }

    pub fn revolve(profile: impl Profile, circular_segments: BoundedU16<3, { u16::MAX }>, angle: f64, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Revolve, Manifold::revolve(&polygons(profile), i32::from(circular_segments.get()), angle.to_degrees()), token, Some(Defect::SelfIntersecting))
    }

    pub fn hull(points: impl IntoIterator<Item: Borrow<Point3<f64>>>) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Hull, Manifold::hull(&points.into_iter().map(|point| Vec3::from(<[f64; 3]>::from(*point.borrow()))).collect::<Vec<_>>()), None, None)
    }

    pub fn hull_solids(solids: impl IntoIterator<Item: Borrow<Self>>) -> Result<Option<Self>, MeshError> {
        let solids = solids.into_iter().collect::<Vec<_>>();
        Self::checked(Operation::Hull, Manifold::hull(&solids.iter().flat_map(|solid| solid.borrow().0.as_impl().vert_pos.iter().copied()).collect::<Vec<_>>()), None, None)
    }

    pub fn transform(&self, placement: &Affine3<f64>, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError> {
        Self::checked(Operation::Transform, self.0.transform(&columns(placement.matrix())), token, Some(Defect::SelfIntersecting))
    }

    pub fn warp<F>(&self, warp: F, token: Option<&CancelToken>) -> Result<Option<Self>, MeshError>
    where F: Fn(Point3<f64>) -> Point3<f64> + Sync {
        Self::checked(
            Operation::Warp,
            self.0.warp_batch(|vertices| vertices.par_iter_mut().for_each(|vertex| *vertex = Vec3::from(<[f64; 3]>::from(warp(Point3::from(<[f64; 3]>::from(*vertex))))))),
            token,
            Some(Defect::Inverted),
        )
    }

    #[must_use]
    pub fn as_original(&self) -> Self { Self(self.0.as_original()) }

    #[must_use]
    pub fn original_ids(&self) -> BTreeSet<NonZero<u32>> {
        self.0.as_impl().mesh_relation.mesh_id_transform.values().filter_map(|relation| NonZero::new(relation.original_id.max(0).cast_unsigned())).collect()
    }

    pub fn to_mesh(&self) -> Result<(TriMesh, Vec<Source>), MeshError> {
        let buffers = self.0.get_mesh_glp(-1);
        let sources = buffers
            .run_index
            .array_windows::<2>()
            .zip(&buffers.run_original_id)
            .flat_map(|(&[start, end], &original)| (start / 3..end / 3).map(move |_| NonZero::new(original)))
            .zip(&buffers.face_id)
            .map(|(original, &face)| Source { original, face })
            .collect();
        let (vertices, indices) = boundary(buffers);
        Ok((trimesh(vertices, indices, Operation::Export)?, sources))
    }

    fn checked(operation: Operation, manifold: Manifold, token: Option<&CancelToken>, through: Option<Defect>) -> Result<Option<Self>, MeshError> {
        let surface = manifold.as_impl();
        let intersecting = || through >= Some(Defect::SelfIntersecting) && soup::has_self_intersections_with_token(surface, token);
        match manifold.status() {
            Error::NoError if surface.is_soup => Err(MeshError::Kernel { operation, status: Error::NotManifold }),
            Error::NoError if !surface.is_finite() => Err(MeshError::Kernel { operation, status: Error::NonFiniteVertex }),
            Error::NoError if manifold.volume() <= manifold.get_epsilon() * manifold.surface_area() => Ok(None),
            Error::NoError if intersecting() && cancel::is_cancelled(token) => Err(MeshError::Kernel { operation, status: Error::Cancelled }),
            Error::NoError if intersecting() => Err(MeshError::Invalid { operation, defect: Defect::SelfIntersecting }),
            Error::NoError if through >= Some(Defect::Inverted) && !repair::shells_well_nested(&soup::impl_to_tris(surface)) => Err(MeshError::Invalid { operation, defect: Defect::Inverted }),
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

fn polygons(profile: impl Profile) -> Polygons {
    CrossSection::new(profile.into_iter().map(|ring| ring.into_iter().map(|vertex| Vec2::from(<[f64; 2]>::from(*vertex.borrow()))).collect()).collect()).to_polygons()
}

fn columns(placement: &Matrix4<f64>) -> Mat3x4 {
    let [x, y, z, w] = <[[f64; 3]; 4]>::from(placement.fixed_view::<3, 4>(0, 0)).map(Vec3::from);
    Mat3x4::from_cols(x, y, z, w)
}

fn boundary(buffers: MeshGLP<f64, u32>) -> (Vec<Vector>, Vec<[u32; 3]>) {
    (buffers.vert_properties.into_chunks::<3>().into_iter().map(Vector::from_array).collect(), buffers.tri_verts.into_chunks::<3>())
}

fn trimesh(vertices: Vec<Vector>, indices: Vec<[u32; 3]>, operation: Operation) -> Result<TriMesh, MeshError> {
    TriMesh::new(vertices, indices).map_err(|source| MeshError::Mesh { operation, source })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod booleans;
pub mod decomposition;
pub mod measures;
pub mod reduction;
pub mod refinement;
pub mod sections;
