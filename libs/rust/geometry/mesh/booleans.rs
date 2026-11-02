use core::{borrow::Borrow, iter};

use manifold_rust::{cancel::CancelToken, csg_tree::CsgNode, linalg::Vec3, manifold::Manifold, progress::ProgressReporter, types::OpType};
use nalgebra::Point3;

use crate::{MeshError, Operation, Solid};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Minkowski {
    Sum,
    Difference,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn boolean(target: Solid, tools: impl IntoIterator<Item = Solid>, op: OpType, token: Option<&CancelToken>, progress: Option<&ProgressReporter>) -> Result<Option<Solid>, MeshError> {
    Solid::checked(
        Operation::Boolean,
        Manifold::from_impl(CsgNode::op_n(op, iter::chain(iter::once(target), tools).map(|solid| CsgNode::leaf(solid.0.into_impl())).collect()).evaluate_with_token_and_progress(token, progress)),
    )
}

pub fn hull(points: impl IntoIterator<Item: Borrow<Point3<f64>>>) -> Result<Option<Solid>, MeshError> {
    Solid::checked(Operation::Hull, Manifold::hull(&points.into_iter().map(|point| Vec3::from(<[f64; 3]>::from(*point.borrow()))).collect::<Vec<_>>()))
}

pub fn minkowski(solid: &Solid, tool: &Solid, minkowski: Minkowski, token: Option<&CancelToken>, progress: Option<&ProgressReporter>) -> Result<Option<Solid>, MeshError> {
    let (solid, tool) = (&solid.0, &tool.0);
    Solid::checked(
        Operation::Minkowski,
        match minkowski {
            Minkowski::Sum => solid
                .try_dilate_by_convex(tool, token, progress)
                .or_else(|| tool.try_dilate_by_convex(solid, token, progress))
                .unwrap_or_else(|| solid.minkowski_sum_with_progress(tool, token, progress)),
            Minkowski::Difference => solid
                .try_convex_erosion(tool, token, progress)
                .or_else(|| solid.try_erode_by_convex(tool, token, progress))
                .unwrap_or_else(|| solid.minkowski_difference_with_progress(tool, token, progress)),
        },
    )
}
