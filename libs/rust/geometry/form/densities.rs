use core::iter;

use faer::{Side, linalg::solvers::Solve as _, sparse::linalg::solvers::Llt};
use nalgebra::{DVector, DVectorView, Matrix3xX};
use nonempty_collections::NEVec;
use validated::Validated;

use crate::{Form, FormError, Input, Network, balance, form, laplacian, spans, view_mut};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub(crate) struct Solved {
    pub(crate) factor: Llt<usize, f64>,
    pub(crate) sign: f64,
    pub(crate) spans: Matrix3xX<f64>,
    pub(crate) free: Matrix3xX<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn sign(densities: &[f64]) -> f64 {
    match densities {
        [first, ..] => 1.0_f64.copysign(*first),
        [] => 1.0,
    }
}

pub(crate) fn solved(network: &Network, densities: &DVector<f64>) -> Result<Solved, FormError> {
    let sign = sign(densities.as_slice());
    let factor = Llt::try_new_with_symbolic(network.llt.clone(), laplacian(network, &(densities * sign))?.as_ref(), Side::Lower)?;
    let [residual, _] = balance(network, &network.anchors, densities);
    let mut free = residual * sign;
    factor.solve_in_place(view_mut(&mut free));
    Ok(Solved { factor, sign, spans: spans(network, &free), free })
}

pub fn equilibrium(network: &Network, densities: DVectorView<'_, f64>) -> Result<Form, NEVec<FormError>> {
    let sign = sign(densities.as_slice());
    iter::chain(
        (densities.len() != network.edges.len()).then_some(FormError::Shape { input: Input::Densities, expected: network.edges.len(), found: densities.len() }),
        densities.iter().enumerate().filter_map(|(index, &density)| (!(density.is_finite() && density * sign > 0.0)).then_some(FormError::Entry { input: Input::Densities, index })),
    )
    .map(Err)
    .collect::<Validated<(), _>>()
    .ok()?;
    let densities = densities.into_owned();
    let Solved { factor: _, sign: _, spans, free } = solved(network, &densities).map_err(NEVec::new)?;
    Ok(form(network, &free, &spans, &densities, balance(network, &spans, &densities)))
}
