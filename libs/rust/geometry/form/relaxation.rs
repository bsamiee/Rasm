use core::{cmp, iter, num::NonZero, ops::ControlFlow};

use faer::{
    Accum, MatMut, MatRef, get_global_parallelism,
    sparse::{SparseColMat, linalg::matmul::sparse_dense_matmul},
};
use nalgebra::{DVector, Matrix3xX};
use nonempty_collections::NEVec;
use rasm_stats::Tolerance;
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Form, FormError, Input, Network, Node, balance, form, spans};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Response {
    Bar,
    Cable,
    Strut,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Member {
    pub stiffness: f64,
    pub rest: f64,
    pub response: Response,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn equilibrium(network: &Network, members: &[Member], tolerance: Tolerance, steps: NonZero<u32>) -> Result<Form, NEVec<FormError>> {
    iter::chain(
        (members.len() != network.edges.len()).then_some(FormError::Shape { input: Input::Members, expected: network.edges.len(), found: members.len() }),
        members.iter().enumerate().filter_map(|(index, &Member { stiffness, rest, response: _ })| {
            (!(stiffness.is_finite() && stiffness > 0.0 && rest.is_finite() && rest >= 0.0)).then_some(FormError::Entry { input: Input::Members, index })
        }),
    )
    .map(Err)
    .collect::<Validated<(), _>>()
    .ok()?;
    let (symbolic, values) = network.free.clone().into_parts();
    let unsigned = SparseColMat::<usize, f64>::new(symbolic, values.into_iter().map(f64::abs).collect());
    let settle = |step: u32, free: &Matrix3xX<f64>| {
        let spans = spans(network, free);
        let densities = DVector::from_vec(
            spans
                .par_column_iter()
                .zip(members.par_iter())
                .map(|(span, &Member { stiffness, rest, response })| {
                    let stretch = if rest > 0.0 { 1.0 - rest / span.norm() } else { 1.0 };
                    let [compression, tension] = cmp::minmax_by(stretch, 0.0, f64::total_cmp);
                    stiffness
                        * match response {
                            Response::Bar => stretch,
                            Response::Cable => tension,
                            Response::Strut => compression,
                        }
                })
                .collect(),
        );
        let [residual, reactions] = balance(network, &spans, &densities);
        let peak = residual.column_iter().map(|force| force.norm()).try_fold(0.0_f64, |peak, norm| norm.is_finite().then_some(peak.max(norm)));
        match peak {
            None => ControlFlow::Break(Err(FormError::Diverged { step })),
            Some(peak) if peak <= tolerance.into_inner() => ControlFlow::Break(Ok(form(network, free, &spans, &densities, [residual, reactions]))),
            Some(peak) => ControlFlow::Continue((residual, densities, peak)),
        }
    };
    let start = Matrix3xX::from_iterator(
        network.loads.ncols(),
        network
            .nodes
            .iter()
            .filter_map(|node| match *node {
                Node::Support(_) => None,
                Node::Free { position, load: _ } => Some(position),
            })
            .flat_map(|position| <[f64; 3]>::from(position.coords)),
    );
    let outcome = try bikeshed ControlFlow<Result<Form, FormError>, Result<Form, FormError>> {
        let (positions, ..) = (0..steps.get()).try_fold((start, Matrix3xX::zeros(network.loads.ncols()), 0.0_f64), |(positions, velocity, previous), step| {
            let (residual, densities, _) = settle(step, &positions)?;
            let stiffness = DVector::from_iterator(members.len(), iter::zip(members, densities.iter()).map(|(member, density)| member.stiffness + density.abs()));
            let mut masses = DVector::zeros(positions.ncols());
            sparse_dense_matmul(
                MatMut::from_column_major_slice_mut(masses.as_mut_slice(), positions.ncols(), 1),
                Accum::Replace,
                unsigned.transpose(),
                MatRef::from_column_major_slice(stiffness.as_slice(), members.len(), 1),
                1.0,
                get_global_parallelism(),
            );
            let accelerations = Matrix3xX::from_iterator(positions.ncols(), iter::zip(residual.column_iter(), masses.iter()).flat_map(|(force, &mass)| <[f64; 3]>::from(force / mass)));
            let velocity = velocity + &accelerations * if previous > 0.0 { 1.0 } else { 0.5 };
            let energy = iter::zip(velocity.column_iter(), masses.iter()).map(|(motion, &mass)| mass * motion.norm_squared()).sum::<f64>();
            ControlFlow::<Result<Form, FormError>, _>::Continue(if energy < previous {
                (positions - (velocity - accelerations) * 0.5, Matrix3xX::zeros(masses.len()), 0.0)
            } else {
                (positions + &velocity, velocity, energy)
            })
        })?;
        let (.., residual) = settle(steps.get(), &positions)?;
        Err(FormError::Unconverged { steps, residual })
    };
    outcome.into_value().map_err(NEVec::new)
}
