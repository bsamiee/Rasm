use core::num::NonZero;

use nalgebra::{DMatrixView, DVector, DVectorView};
use nlopt::{SuccessState, Target};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use num_dual::{DualDVec64, gradient, jacobian};
use rasm_optimize::{
    Solution,
    nonlinear::{Settings, optimize},
};
use rasm_stats::Space;
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{AnalysisError, Estimate, Observations, Prediction, decompose, estimate};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn fit<K, F>(model: F, observations: &Observations, space: &Space<K>, start: DVector<f64>, settings: &Settings<'_>) -> Result<(Estimate, SuccessState), NEVec<AnalysisError>>
where F: Fn(DVectorView<'_, f64>, &DVector<DualDVec64>) -> DualDVec64 + Sync {
    let (points, parameters) = (observations.sites.ncols(), space.len().get());
    let freedom = points.checked_sub(parameters).and_then(NonZero::new).ok_or_else(|| NEVec::new(AnalysisError::Observations { found: points, needed: parameters.strict_add(1) }))?;
    let root = observations.weights.map(f64::sqrt);
    let residuals = |point: DVector<DualDVec64>| {
        DVector::from_vec(
            observations.values.as_slice().par_iter().zip(root.as_slice()).enumerate().map(|(index, (&value, &scale))| (model(observations.sites.column(index), &point) - value) * scale).collect(),
        )
    };
    let squares = |point: DVector<DualDVec64>| residuals(point).norm_squared();
    let Solution { point, objective: _, status } = optimize(
        space,
        start,
        Target::Minimize,
        |point: &[f64], slot: Option<&mut [f64]>, (): &mut ()| {
            let point = DVector::from_column_slice(point);
            match slot {
                Some(slot) => {
                    let (value, derivative) = gradient(squares, &point);
                    slot.copy_from_slice(derivative.as_slice());
                    value
                }
                None => squares(point.map(DualDVec64::from_re)).re,
            }
        },
        settings,
    )
    .map_err(|errors| errors.into_nonempty_iter().map(AnalysisError::from).collect::<NEVec<_>>())?;
    let (weighted, design) = jacobian(residuals, &point);
    weighted.iter().position(|residual| !residual.is_finite()).map_or(Ok(()), |index| Err(NEVec::new(AnalysisError::Value { index })))?;
    decompose(&design).map(|svd| (estimate(&svd, point, weighted.norm_squared(), freedom), status)).map_err(NEVec::new)
}

pub fn predict<F>(model: F, estimate: &Estimate, sites: DMatrixView<'_, f64>) -> Result<Prediction<f64>, NEVec<AnalysisError>>
where F: Fn(DVectorView<'_, f64>, &DVector<DualDVec64>) -> DualDVec64 + Sync {
    (0..sites.ncols())
        .into_par_iter()
        .map(|point| -> Result<(f64, f64), AnalysisError> {
            let site = sites.column(point);
            site.iter().position(|coordinate| !coordinate.is_finite()).map_or(Ok(()), |dimension| Err(AnalysisError::Site { dimension, point }))?;
            let (value, slope) = gradient(|parameters| model(site, &parameters), &estimate.parameters);
            value.is_finite().ok_or(AnalysisError::Value { index: point })?;
            slope.iter().position(|entry| !entry.is_finite()).map_or(Ok(()), |parameter| Err(AnalysisError::Design { point, parameter }))?;
            Ok((value, slope.dot(&(&estimate.covariance * &slope))))
        })
        .collect::<Vec<_>>()
        .into_iter()
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(|moments| {
            let (mean, variance): (Vec<_>, Vec<_>) = moments.into_iter().unzip();
            Prediction { mean: DVector::from_vec(mean), variance: DVector::from_vec(variance) }
        })
}
