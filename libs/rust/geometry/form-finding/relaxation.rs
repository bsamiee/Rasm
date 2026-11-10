use core::num::NonZero;
use core::ops::ControlFlow;
use core::{cmp, iter};

use faer::sparse::SparseColMatRef;
use faer::{ColRef, Mat};
use nalgebra::DVector;
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
    let (symbolic, values) = network.free.parts();
    let values = values.iter().copied().map(f64::abs).collect::<Vec<_>>();
    let unsigned = SparseColMatRef::new(symbolic, &values);
    let settle = |step: u32, free: &Mat<f64>| {
        let spans = spans(network, free.as_ref());
        let densities = DVector::from_vec(
            spans
                .par_row_iter()
                .zip(members.par_iter())
                .map(|(span, &Member { stiffness, rest, response })| {
                    let stretch = if rest > 0.0 { 1.0 - rest / span.norm_l2() } else { 1.0 };
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
        let (residual, reactions) = balance(network, spans.as_ref(), densities.as_view());
        let peak = residual.row_iter().map(|force| force.norm_l2()).try_fold(0.0_f64, |peak, norm| norm.is_finite().then_some(peak.max(norm)));
        match peak {
            None => ControlFlow::Break(Err(FormError::Diverged { step })),
            Some(peak) if peak <= tolerance.into_inner() => ControlFlow::Break(Ok(form(network, free.as_ref(), spans.as_ref(), densities.as_view(), (residual, reactions)))),
            Some(peak) => ControlFlow::Continue((residual, densities, peak)),
        }
    };
    let start = network
        .nodes
        .iter()
        .filter_map(|node| match *node {
            Node::Support(_) => None,
            Node::Free { position, load: _ } => Some(position),
        })
        .collect::<Vec<_>>();
    let start = Mat::from_fn(start.len(), 3, |row, axis| start[row][axis]);
    let outcome = try bikeshed ControlFlow<Result<Form, FormError>, Result<Form, FormError>> {
        let (positions, ..) = (0..steps.get()).try_fold((start, Mat::zeros(network.loads.nrows(), 3), 0.0_f64), |(positions, velocity, previous), step| {
            let (residual, densities, _) = settle(step, &positions)?;
            let stiffness = iter::zip(members, densities.iter()).map(|(member, density)| member.stiffness + density.abs()).collect::<Vec<_>>();
            let masses = unsigned.transpose() * ColRef::from_slice(&stiffness);
            let accelerations = Mat::from_fn(residual.nrows(), 3, |row, axis| residual[(row, axis)] / masses[row]);
            let velocity = velocity + if previous > 0.0 { 1.0 } else { 0.5 } * &accelerations;
            let energy = iter::zip(velocity.row_iter(), masses.iter()).map(|(motion, &mass)| mass * motion.squared_norm_l2()).sum::<f64>();
            ControlFlow::<Result<Form, FormError>, _>::Continue(if energy < previous {
                (positions - 0.5 * (velocity - accelerations), Mat::zeros(masses.nrows(), 3), 0.0)
            } else {
                (positions + &velocity, velocity, energy)
            })
        })?;
        let (.., residual) = settle(steps.get(), &positions)?;
        Err(FormError::Unconverged { steps, residual })
    };
    outcome.into_value().map_err(NEVec::new)
}
