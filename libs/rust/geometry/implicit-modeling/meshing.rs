use core::ops::ControlFlow;

use fidget::mesh::{Mesh, Octree, Settings};
use fidget::shape::{BoundShape, ShapeBulkEvalError, ShapeTracingEval};
use nalgebra::{Matrix4, Vector3};
use parry3d_f64::math::Vector;
use parry3d_f64::shape::{TriMesh, TriMeshBuilderError};

use crate::{Classification, Domain, Evaluation, ImplicitError, Shape, bind, centered, depth, evaluate, interval, span, subdivide, world};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Boundary {
    Open,
    Capped,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Surface {
    pub mesh: TriMesh,
    pub gradients: Vec<Vector3<f64>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn mesh(shape: &Shape, evaluation: &Evaluation<'_>, size: f64, boundary: Boundary, domain: Domain) -> Result<Option<Surface>, ImplicitError> {
    let extent = domain.half.max();
    let depth = depth(extent, size)?;
    let (half, octree) = match boundary {
        Boundary::Open => (None, domain.half),
        Boundary::Capped => (Some(domain.half), Vector3::repeat(extent)),
    };
    let vars = bind(shape, evaluation, domain.center, half);
    let mut evaluator = ShapeTracingEval::default();
    subdivide(evaluation.cancel.as_ref(), depth, centered(octree), (), &mut |(), cell, leaf| {
        let enclosure = interval(&mut evaluator, shape, &vars, cell)?;
        match (Classification::from(enclosure), enclosure.lower().is_finite() && enclosure.upper().is_finite(), leaf) {
            (Classification::Inside | Classification::Outside, _, _) | (Classification::Straddling, true, _) => Ok(ControlFlow::Break(())),
            (Classification::Straddling, false, false) => Ok(ControlFlow::Continue(())),
            (Classification::Straddling, false, true) => Err(ImplicitError::Singular { cell: span(domain.center, cell) }),
        }
    })?;
    let settings = Settings { depth, world_to_model: Matrix4::new_nonuniform_scaling(&octree), threads: evaluation.threads, cancel: evaluation.cancel.clone().unwrap_or_default() };
    let Mesh { triangles, vertices } = Octree::build(&BoundShape::new(shape.jit.clone(), &vars).map_err(ShapeBulkEvalError::from)?, &settings).ok_or(ImplicitError::Cancelled)?.walk_dual();
    let indices = triangles
        .into_iter()
        .map(|triangle| <[usize; 3]>::from(triangle).try_map(u32::try_from))
        .try_collect::<Vec<_>>()
        .map_err(|source| ImplicitError::Capacity { vertices: vertices.len(), source })?;
    let gradients = evaluate::<(f64, Vector3<f64>)>(shape, evaluation, &vars, vertices.iter().copied())?;
    match TriMesh::new(vertices.into_iter().map(|vertex| Vector::from(world(domain.center, vertex).coords)).collect(), indices) {
        Ok(mesh) => Ok(Some(Surface { mesh, gradients: gradients.into_iter().map(|(_, gradient)| gradient).collect() })),
        Err(TriMeshBuilderError::EmptyIndices) => Ok(None),
        Err(source) => Err(ImplicitError::from(source)),
    }
}
