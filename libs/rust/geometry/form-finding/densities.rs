use core::iter;

use faer::{Mat, linalg::solvers::Solve as _, sparse::linalg::solvers::Lu};
use itertools::Itertools as _;
use nalgebra::DVectorView;
use nonempty_collections::NEVec;
use validated::Validated;

use crate::{Form, FormError, Input, Network, balance, form, laplacian, spans};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub(crate) struct Solved {
    pub(crate) factor: Lu<usize, f64>,
    pub(crate) spans: Mat<f64>,
    pub(crate) free: Mat<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn solved(network: &Network, densities: DVectorView<'_, f64>) -> Result<Solved, FormError> {
    let factor = Lu::try_new_with_symbolic(network.lu.clone(), laplacian(network, densities.iter().copied())?.as_ref())?;
    let (residual, _) = balance(network, network.anchors.as_ref(), densities);
    let free = factor.solve(&residual);
    free.as_ref().is_all_finite().ok_or(FormError::Singular)?;
    Ok(Solved { factor, spans: spans(network, free.as_ref()), free })
}

pub fn equilibrium(network: &Network, densities: DVectorView<'_, f64>) -> Result<Form, NEVec<FormError>> {
    iter::chain(
        (densities.len() != network.edges.len()).then_some(FormError::Shape { input: Input::Densities, expected: network.edges.len(), found: densities.len() }),
        densities.iter().positions(|density| !density.is_finite()).map(|index| FormError::Entry { input: Input::Densities, index }),
    )
    .map(Err)
    .collect::<Validated<(), _>>()
    .ok()?;
    let Solved { factor: _, spans, free } = solved(network, densities).map_err(NEVec::new)?;
    Ok(form(network, free.as_ref(), spans.as_ref(), densities, balance(network, spans.as_ref(), densities)))
}
