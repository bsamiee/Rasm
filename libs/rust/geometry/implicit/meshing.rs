use core::range::RangeInclusive;

use bounded_integer::BoundedU8;
use derive_where::derive_where;
use fidget::{
    jit::JitShape,
    mesh::{Mesh, Octree, Settings},
    render::{CancelToken, ThreadPool},
    shape::{EzShape as _, ShapeBulkEvalError, ShapeTracingEvalError, ShapeVars},
    shapes::{self, Intersection, types::Vec3},
    types::Interval,
};
use nalgebra::{Matrix4, Point3};
use parry3d_f64::{math::Vector, shape::TriMesh};

use crate::{Domain, Field, ImplicitError, bind, recentre};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Boundary {
    Open,
    Capped,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MAX_DEPTH: u8 = u8::try_from(usize::MAX.ilog(8)).ok().unwrap();

// --- [MODELS] --------------------------------------------------------------------------

#[derive_where(Debug)]
pub struct Meshing<'pool, 'vars> {
    pub depth: BoundedU8<1, MAX_DEPTH>,
    pub boundary: Boundary = Boundary::Capped,
    #[derive_where(skip)]
    pub threads: Option<&'pool ThreadPool> = Some(&ThreadPool::Global),
    pub vars: Option<&'vars ShapeVars<f32>> = None,
    #[derive_where(skip)]
    pub cancel: Option<CancelToken> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn mesh(field: Field, domain: Domain, Meshing { depth, boundary, threads, vars, cancel }: Meshing<'_, '_>) -> Result<Option<TriMesh>, ImplicitError> {
    let RangeInclusive { start, last } = domain.into_inner();
    let centre = Point3::from(start.coords.zip_map(&last.coords, f64::midpoint));
    let half = ((last - start) / 2.0).cast::<f32>();
    let world_to_model = Matrix4::new_nonuniform_scaling(&half);
    let surface = recentre(field, centre).tree;
    let bound = bind(
        match boundary {
            Boundary::Open => surface,
            Boundary::Capped => Intersection { input: vec![surface, shapes::Box { lower: Vec3::from(-half), upper: Vec3::from(half) }.into()] }.into(),
        },
        vars,
    )?;
    let unit = Interval::new(-1.0, 1.0);
    let (root, _) = JitShape::new_interval_eval()
        .eval_with_transform_and_vars(&bound.shape().ez_interval_tape(), unit, unit, unit, &world_to_model, bound.vars())
        .map_err(|ShapeTracingEvalError::MissingVar(source)| ShapeBulkEvalError::from(source))?;
    (root.lower() > 0.0 || root.upper() < 0.0 || (root.lower().is_finite() && root.upper().is_finite())).ok_or(ImplicitError::NotFinite { domain })?;
    let Mesh { triangles, vertices } =
        Octree::build(&bound, &Settings { depth: depth.get(), world_to_model, threads, cancel: cancel.unwrap_or_default() }).ok_or(ImplicitError::Cancelled)?.walk_dual();
    vertices.iter().all(|vertex| vertex.iter().all(|coordinate| coordinate.is_finite())).ok_or(ImplicitError::NotFinite { domain })?;
    let indices = triangles
        .into_iter()
        .map(|triangle| <[usize; 3]>::from(triangle).try_map(u32::try_from))
        .try_collect::<Vec<_>>()
        .map_err(|source| ImplicitError::Capacity { vertices: vertices.len(), source })?;
    Ok(TriMesh::new(vertices.into_iter().map(|vertex| Vector::from(vertex.cast::<f64>() + centre.coords)).collect(), indices).ok())
}
