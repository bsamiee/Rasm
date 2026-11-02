use core::iter;

use csaps::{GridCubicSmoothingSpline, NdGridSpline};
use itertools::{Itertools as _, izip};
use nalgebra::convert;
use ndarray::{Array, ArrayView, ArrayView1, Dimension, IntoDimension, Ix1, Slice};
use nonempty_collections::NEVec;
use validated::Validated;

use crate::{AnalysisError, Observations};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0, less_or_equal = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Smooth(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Axis<'data> {
    pub sites: ArrayView1<'data, f64>,
    pub weights: Option<ArrayView1<'data, f64>> = None,
    pub smooth: Option<Smooth> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn curve(observations: &Observations, smooth: Option<Smooth>) -> Result<GridCubicSmoothingSpline<'_, f64, Ix1>, NEVec<AnalysisError>> {
    let sites = observations.sites();
    [
        match sites.nrows() {
            1 => increasing(0, ArrayView1::from(sites.as_slice())),
            found => Err(AnalysisError::Shape { expected: 1, found }),
        },
        (sites.ncols() >= 2).ok_or(AnalysisError::Observations { found: sites.ncols(), needed: 2 }),
    ]
    .into_iter()
    .collect::<Validated<(), _>>()
    .ok()?;
    GridCubicSmoothingSpline::new(&[ArrayView1::from(sites.as_slice())], observations.values().as_slice())
        .with_weights(&[Some(ArrayView1::from(observations.weights().as_slice()))])
        .with_smooth(&[smooth.map(Smooth::into_inner)])
        .make()
        .map_err(|source| NEVec::new(AnalysisError::Spline(source)))
}

pub fn grid<'data, D>(axes: &[Axis<'data>], values: ArrayView<'data, f64, D>) -> Result<GridCubicSmoothingSpline<'data, f64, D>, NEVec<AnalysisError>>
where D: Dimension {
    iter::once(match (axes.len(), values.ndim()) {
        (_, 0) => Err(AnalysisError::Observations { found: values.len(), needed: 2 }),
        (found, expected) => (found == expected).ok_or(AnalysisError::Shape { expected, found }),
    })
    .chain(iter::zip(axes, values.shape()).enumerate().flat_map(|(dimension, (axis, &expected))| {
        let found = axis.sites.len();
        iter::chain(
            [
                (found == expected).ok_or(AnalysisError::Shape { expected, found }),
                (found >= 2).ok_or(AnalysisError::Observations { found, needed: 2 }),
                axis.sites.iter().position(|site| !site.is_finite()).map_or_else(|| increasing(dimension, axis.sites), |point| Err(AnalysisError::Site { dimension, point })),
                axis.weights.map_or(Ok(()), |weights| (weights.len() == found).ok_or(AnalysisError::Shape { expected: found, found: weights.len() })),
            ],
            axis.weights
                .iter()
                .flatten()
                .enumerate()
                .filter(|&(_, &weight)| !weight.is_finite() || weight <= 0.0)
                .map(move |(index, &weight)| Err(AnalysisError::Weight { axis: dimension, index, weight })),
        )
    }))
    .chain(values.iter().positions(|value| !value.is_finite()).map(|index| Err(AnalysisError::Value { index })))
    .collect::<Validated<(), _>>()
    .ok()?;
    let (sites, weights, smooth): (Vec<_>, Vec<_>, Vec<_>) = axes.iter().map(|axis| (axis.sites, axis.weights, axis.smooth.map(Smooth::into_inner))).multiunzip();
    GridCubicSmoothingSpline::new(&sites, values).with_weights(&weights).with_smooth(&smooth).make().map_err(|source| NEVec::new(AnalysisError::Spline(source)))
}

pub fn evaluate<D>(spline: &NdGridSpline<'_, f64, D>, sites: &[ArrayView1<'_, f64>], derivative: impl IntoDimension<Dim = D>) -> Result<Array<f64, D>, NEVec<AnalysisError>>
where D: Dimension {
    let derivative = derivative.into_dimension();
    [(sites.len(), spline.ndim()), (derivative.ndim(), spline.ndim())]
        .into_iter()
        .map(|(found, expected)| (found == expected).ok_or(AnalysisError::Shape { expected, found }))
        .chain(sites.iter().enumerate().map(|(dimension, axis)| axis.iter().position(|site| !site.is_finite()).map_or(Ok(()), |point| Err(AnalysisError::Site { dimension, point }))))
        .collect::<Validated<(), _>>()
        .ok()?;
    let coefficients = izip!(spline.order(), spline.pieces(), derivative.slice()).enumerate().filter(|&(_, (_, _, &times))| times > 0).fold(
        spline.coeffs().to_owned(),
        |coefficients, (axis, (&order, &pieces, &times))| {
            let along = ndarray::Axis(axis);
            let mut derived = coefficients.slice_axis(along, Slice::from(..order.saturating_sub(times).max(1).strict_mul(pieces))).to_owned();
            derived.axis_chunks_iter_mut(along, pieces).enumerate().for_each(|(block, mut chunk)| {
                let power = order.strict_sub(1).strict_sub(block);
                let factor = power.checked_sub(times).map_or_default(|rest| (rest.strict_add(1)..=power).map(convert::<usize, f64>).product::<f64>());
                chunk.mapv_inplace(|coefficient| coefficient * factor);
            });
            derived
        },
    );
    Ok(NdGridSpline::new(spline.breaks().clone(), coefficients).evaluate(sites))
}

fn increasing(axis: usize, sites: ArrayView1<'_, f64>) -> Result<(), AnalysisError> {
    sites
        .iter()
        .tuple_windows()
        .position(|(earlier, later)| later <= earlier || almost::equal(*later, *earlier))
        .map_or(Ok(()), |window| Err(AnalysisError::Order { axis, index: window.strict_add(1) }))
}
