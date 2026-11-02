use core::fmt::Debug;
use std::{collections::HashMap, iter};

use average::Mean;
use itertools::Itertools as _;
use nalgebra::{DMatrix, DVector};
use nonempty_collections::NEVec;
use num_dual::{DualDVec64, DualNum as _};
use nuts_rs::{Chain as _, CpuLogpFunc, CpuMath, CpuMathError, EuclideanAdaptOptions, HasDims, NutsError, NutsSettings, Progress, Settings};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rand::{Rng, RngExt as _, SeedableRng as _, rngs::ChaCha8Rng};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use serde::Serialize;
use validated::Validated;

use crate::{Bounds, Space, StatsError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Resolution {
    Monthly,
    Hourly,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Trace {
    pub draws: DMatrix<f64>,
    pub progress: Vec<Progress>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Fit {
    pub nmbe: f64,
    pub cv_rmse: f64,
}

struct Posterior<'call, K, F> {
    space: &'call Space<K>,
    density: &'call F,
}

impl<K, F> HasDims for Posterior<'_, K, F>
where F: Fn(DVector<DualDVec64>) -> DualDVec64
{
    fn dim_sizes(&self) -> HashMap<String, u64> { HashMap::from([(String::from("unconstrained_parameter"), nalgebra::convert::<usize, u64>(self.dim()))]) }
}

impl<K, F> CpuLogpFunc for Posterior<'_, K, F>
where F: Fn(DVector<DualDVec64>) -> DualDVec64
{
    type LogpError = StatsError;
    type FlowParameters = ();
    type ExpandedVector = ();

    fn dim(&self) -> usize { self.space.len().get() }

    fn logp(&mut self, position: &[f64], gradient: &mut [f64]) -> Result<f64, Self::LogpError> {
        let (logp, derivative) = num_dual::gradient(
            |unconstrained: DVector<DualDVec64>| {
                let (constrained, jacobian): (Vec<_>, Vec<_>) = iter::zip(&unconstrained, self.space.values()).map(|(z, bounds)| bounded(z, bounds)).unzip();
                iter::once((self.density)(DVector::from_vec(constrained))).chain(jacobian).sum::<DualDVec64>()
            },
            &DVector::from_column_slice(position),
        );
        gradient.copy_from_slice(derivative.as_slice());
        (logp.is_finite() && derivative.iter().all(|slope| slope.is_finite())).then_some(logp).ok_or(StatsError::NotFinite)
    }

    fn expand_vector<R>(&mut self, _rng: &mut R, _array: &[f64]) -> Result<Self::ExpandedVector, CpuMathError>
    where R: Rng + ?Sized {
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [POSTERIOR]
pub fn calibrate<K, F, M>(space: &Space<K>, density: F, settings: &NutsSettings<EuclideanAdaptOptions<M>>) -> Result<Vec<Trace>, NEVec<StatsError>>
where
    K: Sync,
    F: Fn(DVector<DualDVec64>) -> DualDVec64 + Sync,
    M: Debug + Default + Copy + Serialize,
    NutsSettings<EuclideanAdaptOptions<M>>: Settings,
{
    let refused = |report: anyhow::Error, failure: StatsError| {
        tracing::error!({ EXCEPTION_MESSAGE } = %report, "sampler raised a report with no typed source");
        failure
    };
    Settings::validate(settings).map_err(|report| NEVec::new(refused(report, StatsError::Settings)))?;
    let dimensions = space.len().get();
    (0..nalgebra::convert::<usize, u64>(settings.num_chains()))
        .into_par_iter()
        .map(|chain| -> Result<Trace, StatsError> {
            let failed = |error: anyhow::Error| error.downcast::<NutsError>().map_or_else(|report| refused(report, StatsError::Chain { chain }), |source| StatsError::Sampler { chain, source });
            let mut rng = ChaCha8Rng::seed_from_u64(settings.seed());
            rng.set_stream(chain);
            let start = DVector::from_fn(dimensions, |_, _| rng.random_range(-2.0_f64..2.0_f64));
            let mut sampler = settings.new_chain(chain, CpuMath::new(Posterior { space, density: &density }), &mut rng).map_err(failed)?;
            sampler.set_position(start.as_slice()).map_err(failed)?;
            let (draws, progress): (Vec<Box<[f64]>>, Vec<Progress>) = iter::repeat_with(|| sampler.draw().map_err(failed))
                .take(settings.hint_num_tune().strict_add(settings.hint_num_draws()))
                .process_results(|drawn| drawn.filter(|(_, progress)| !progress.tuning).unzip())?;
            Ok(Trace {
                draws: DMatrix::from_iterator(dimensions, draws.len(), draws.iter().flat_map(|draw| iter::zip(draw, space.values()).map(|(&z, bounds)| bounded(&DualDVec64::from(z), bounds).0.re))),
                progress,
            })
        })
        .collect::<Validated<Vec<Trace>, StatsError>>()
        .ok()
}

fn bounded(z: &DualDVec64, bounds: &Bounds) -> (DualDVec64, DualDVec64) {
    let negative = z.re.is_sign_negative();
    let magnitude = if negative { -z.clone() } else { z.clone() };
    let softplus = (-magnitude.clone()).exp().ln_1p();
    let [near, far] = [softplus.clone(), magnitude.clone() + softplus.clone()].map(|surprisal| (-surprisal).exp());
    let (rising, falling) = if negative { (far, near) } else { (near, far) };
    (rising * bounds.last + falling * bounds.start, -(magnitude + softplus * 2.0) + (bounds.last - bounds.start).ln())
}

// --- [GRADING]
#[must_use]
pub fn fit(pairs: impl IntoIterator<Item = (f64, f64)>, parameters: u32) -> Option<Fit> {
    let (measurements, residuals, squares) =
        pairs.into_iter().map(|(measured, simulated)| (measured, measured - simulated)).map(|(measured, residual)| (measured, residual, residual.powi(2))).multiunzip::<(Mean, Mean, Mean)>();
    let count = nalgebra::convert::<u64, f64>(measurements.len());
    let dof = count - f64::from(parameters);
    let scale = measurements.mean().abs();
    (dof > 0.0).then(|| Fit { nmbe: count * residuals.mean() / (dof * scale), cv_rmse: (count * squares.mean() / dof).sqrt() / scale }).filter(|fit| fit.nmbe.is_finite() && fit.cv_rmse.is_finite())
}

#[must_use]
pub const fn calibrated(fit: Fit, resolution: Resolution) -> bool {
    let (nmbe, cv_rmse): (f64, f64) = match resolution {
        Resolution::Monthly => (0.05, 0.15),
        Resolution::Hourly => (0.10, 0.30),
    };
    fit.nmbe.abs() <= nmbe && fit.cv_rmse <= cv_rmse
}
