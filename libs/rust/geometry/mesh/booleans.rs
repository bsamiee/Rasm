use core::iter;

use manifold_rust::{cancel::CancelToken, csg_tree::CsgNode, manifold::Manifold, progress::ProgressReporter, types::OpType};

use crate::{Defect, MeshError, Operation, Solid};

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
        token,
        Some(Defect::SelfIntersecting),
    )
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
        token,
        Some(Defect::SelfIntersecting),
    )
}
