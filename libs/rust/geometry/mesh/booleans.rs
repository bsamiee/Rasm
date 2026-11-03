use manifold_rust::{cancel::CancelToken, csg_tree::CsgNode, linalg::Mat3x4, manifold::Manifold, progress::ProgressReporter, types::OpType};
use nalgebra::Affine3;

use crate::{MeshError, Operation, Solid};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Minkowski {
    Sum,
    Difference,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, derive_more::Debug)]
pub struct Csg(#[debug(skip)] CsgNode);

impl Csg {
    #[must_use]
    pub fn leaf(solid: Solid) -> Self { Self(CsgNode::leaf(solid.0.into_impl())) }

    #[must_use]
    pub fn op(op: OpType, children: impl IntoIterator<Item = Self>) -> Self { Self(CsgNode::op_n(op, children.into_iter().map(|child| child.0).collect())) }

    #[must_use]
    pub fn transform(self, placement: &Affine3<f64>) -> Self {
        fn placed(node: CsgNode, placement: Mat3x4) -> CsgNode {
            match node {
                CsgNode::Leaf(leaf) => CsgNode::Leaf(leaf.apply_transform(placement)),
                CsgNode::Op { op, children, transform } =>
                    CsgNode::Op { op, children: children.into_iter().map(|child| placed(placed(child, transform), placement)).collect(), transform: Mat3x4::identity() },
            }
        }
        Self(placed(self.0, crate::columns(placement.matrix())))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn boolean(csg: &Csg, token: Option<&CancelToken>, progress: Option<&ProgressReporter>) -> Result<Option<Solid>, MeshError> {
    Solid::checked(Operation::Boolean, Manifold::from_impl(csg.0.evaluate_with_token_and_progress(token, progress)), token)
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
    )
}
