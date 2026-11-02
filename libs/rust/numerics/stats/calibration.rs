use core::{fmt::Debug, iter, ops::ControlFlow};
use std::collections::HashMap;

use average::{Mean, Variance};
use itertools::Itertools as _;
use nalgebra::{DMatrix, DMatrixView};
use nonempty_collections::{NEBTreeMap, NEVec, NonEmptyIterator as _};
use num_dual::{Dual64, DualNum as _};
use nuts_rs::{Chain as _, CpuLogpFunc, CpuMath, CpuMathError, EuclideanAdaptOptions, HasDims, NutsError, NutsSettings, Progress, Settings};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rand::{Rng, SeedableRng as _, rngs::ChaCha8Rng};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use serde::Serialize;
use statrs::{
    distribution::{ContinuousCDF as _, Normal},
    statistics::{Data, OrderStatistics as _, RankTieBreaker},
};
use validated::Validated;

use crate::{Bounds, Moment, Sample, Space, StatsError};

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
pub struct Convergence {
    pub rhat: f64,
    pub ess_bulk: f64,
    pub ess_tail: f64,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Fit {
    pub nmbe: f64,
    pub cv_rmse: f64,
    pub non_finite: u64,
}

struct Posterior<'call, K, F> {
    space: &'call Space<K>,
    density: &'call F,
    constrained: Vec<f64>,
    slopes: Vec<(f64, f64)>,
}

impl<K, F> HasDims for Posterior<'_, K, F>
where F: Fn(&[f64], &mut [f64]) -> f64
{
    fn dim_sizes(&self) -> HashMap<String, u64> { HashMap::from([(String::from("unconstrained_parameter"), nalgebra::convert::<usize, u64>(self.dim()))]) }
}

impl<K, F> CpuLogpFunc for Posterior<'_, K, F>
where F: Fn(&[f64], &mut [f64]) -> f64
{
    type LogpError = StatsError;
    type FlowParameters = ();
    type ExpandedVector = ();

    fn dim(&self) -> usize { self.space.len().get() }

    fn logp(&mut self, position: &[f64], gradient: &mut [f64]) -> Result<f64, Self::LogpError> {
        self.constrained.clear();
        self.slopes.clear();
        let jacobian = iter::zip(position, self.space.values()).fold(0.0_f64, |total, (&z, bounds)| {
            let ((x, rising), (jacobian, bending)) = num_dual::first_derivative(|dual| bounded(dual, bounds), z);
            self.constrained.push(x);
            self.slopes.push((rising, bending));
            total + jacobian
        });
        let logp = (self.density)(&self.constrained, gradient) + jacobian;
        iter::zip(gradient.iter_mut(), &self.slopes).for_each(|(slope, &(rising, bending))| *slope = slope.mul_add(rising, bending));
        (logp.is_finite() && gradient.iter().all(|slope| slope.is_finite())).then_some(logp).ok_or(StatsError::NotFinite)
    }

    fn expand_vector<R>(&mut self, _rng: &mut R, _array: &[f64]) -> Result<Self::ExpandedVector, CpuMathError>
    where R: Rng + ?Sized {
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [POSTERIOR]
pub fn calibrate<K, F, S, M>(space: &Space<K>, settings: &NutsSettings<EuclideanAdaptOptions<M>>, starts: DMatrixView<'_, f64>, density: F, stop: S) -> Result<Vec<Trace>, NEVec<StatsError>>
where
    K: Sync,
    F: Fn(&[f64], &mut [f64]) -> f64 + Sync,
    S: Fn(&Progress) -> ControlFlow<()> + Sync,
    M: Debug + Default + Copy + Serialize,
    NutsSettings<EuclideanAdaptOptions<M>>: Settings,
{
    Settings::validate(settings).map_err(|report| {
        tracing::error!({ EXCEPTION_MESSAGE } = %report, "sampler settings refused");
        NEVec::new(StatsError::Settings)
    })?;
    let dimensions = space.len().get();
    let chains = settings.num_chains();
    Validated::from((starts.nrows() == dimensions).ok_or(StatsError::Shape { expected: dimensions, found: starts.nrows() }))
        .map2(Validated::from((starts.ncols() == chains).ok_or(StatsError::Shape { expected: chains, found: starts.ncols() })), |(), ()| ())
        .ok()?;
    starts
        .column_iter()
        .enumerate()
        .flat_map(|(index, start)| {
            iter::zip(start, space.values())
                .enumerate()
                .map(move |(dimension, (&x, bounds))| (bounds.start < x && x < bounds.last).ok_or(StatsError::Start { chain: nalgebra::convert::<usize, u64>(index), dimension }))
        })
        .collect::<Validated<(), StatsError>>()
        .ok()?;
    (0..chains)
        .into_par_iter()
        .map(|index| -> Result<Trace, StatsError> {
            let chain = nalgebra::convert::<usize, u64>(index);
            let failed = |error: anyhow::Error| StatsError::Sampler {
                chain,
                source: error.downcast::<NutsError>().map_or_else(
                    |report| {
                        tracing::error!({ EXCEPTION_MESSAGE } = %report, chain, "sampler raised a report with no typed source");
                        None
                    },
                    Some,
                ),
            };
            let mut rng = ChaCha8Rng::seed_from_u64(settings.seed());
            rng.set_stream(chain);
            let mut sampler = settings
                .new_chain(chain, CpuMath::new(Posterior { space, density: &density, constrained: Vec::with_capacity(dimensions), slopes: Vec::with_capacity(dimensions) }), &mut rng)
                .map_err(failed)?;
            sampler
                .set_position(&iter::zip(starts.column(index).iter(), space.values()).map(|(&x, bounds)| (x - bounds.start).ln() - (bounds.last - x).ln()).collect::<Box<[f64]>>())
                .map_err(failed)?;
            let (draws, progress): (Vec<Box<[f64]>>, Vec<Progress>) = iter::repeat_with(|| sampler.draw().map_err(failed))
                .take(settings.hint_num_tune().strict_add(settings.hint_num_draws()))
                .process_results(|drawn| drawn.take_while_inclusive(|(_, progress)| stop(progress).is_continue()).filter(|(_, progress)| !progress.tuning).unzip())?;
            Ok(Trace {
                draws: DMatrix::from_iterator(dimensions, draws.len(), draws.iter().flat_map(|draw| iter::zip(draw, space.values()).map(|(&z, bounds)| bounded(Dual64::from(z), bounds).0.re))),
                progress,
            })
        })
        .collect::<Validated<Vec<Trace>, StatsError>>()
        .ok()
}

fn bounded(z: Dual64, bounds: &Bounds) -> (Dual64, Dual64) {
    let negative = z.re.is_sign_negative();
    let magnitude = if negative { -z } else { z };
    let softplus = (-magnitude).exp().ln_1p();
    let [near, far] = [softplus, magnitude + softplus].map(|surprisal| (-surprisal).exp());
    let (rising, falling) = if negative { (far, near) } else { (near, far) };
    (rising * bounds.last + falling * bounds.start, -(magnitude + softplus * 2.0) + (bounds.last - bounds.start).ln())
}

// --- [DIAGNOSTICS]
pub fn convergence<'space, K>(space: &'space Space<K>, traces: &[Trace]) -> Result<NEBTreeMap<&'space K, Convergence>, StatsError>
where K: Ord {
    let dimensions = space.len().get();
    traces.iter().try_for_each(|trace| (trace.draws.nrows() == dimensions).ok_or(StatsError::Shape { expected: dimensions, found: trace.draws.nrows() }))?;
    let length = traces.iter().map(|trace| trace.draws.ncols()).min().unwrap_or_default();
    let half = length.strict_div(2);
    (half >= 2).ok_or(StatsError::Degenerate { found: length, sample: Sample::Draws })?;
    let normal = Normal::standard();
    let draws = nalgebra::convert::<usize, f64>(half);
    let scores = |data: &mut Data<Vec<f64>>| {
        let count = nalgebra::convert::<usize, f64>(data.len());
        data.ranks(RankTieBreaker::Average).into_iter().map(|rank| normal.inverse_cdf((rank - 0.375) / (count + 0.25))).collect::<Vec<_>>()
    };
    let split = |values: &[f64]| {
        let moments = values.chunks_exact(half).map(|chain| chain.iter().copied().collect::<Variance>()).collect::<Vec<_>>();
        let within = moments.iter().map(Variance::sample_variance).collect::<Mean>().mean();
        let plus = within.mul_add((draws - 1.0) / draws, moments.iter().map(Variance::mean).collect::<Variance>().sample_variance());
        (moments, within, plus)
    };
    let rhat = |values: &[f64]| {
        let (_, within, plus) = split(values);
        (plus / within).sqrt()
    };
    let ess = |values: &[f64]| {
        let (moments, within, plus) = split(values);
        let size = nalgebra::convert::<usize, f64>(values.len());
        let autocorrelation = |lag: usize| {
            let lagged = iter::zip(values.chunks_exact(half), &moments)
                .map(|(chain, moments)| iter::zip(chain, chain.iter().skip(lag)).map(|(early, late)| (early - moments.mean()) * (late - moments.mean())).sum::<f64>() / draws)
                .collect::<Mean>()
                .mean();
            1.0 - (within - lagged) / plus
        };
        let pairs = iter::once(1.0 + autocorrelation(1))
            .chain((1..half.strict_div(2)).map(|pair| autocorrelation(pair.strict_mul(2)) + autocorrelation(pair.strict_mul(2).strict_add(1))))
            .take_while(|&sum| sum > 0.0)
            .scan(f64::INFINITY, |floor, sum| {
                *floor = floor.min(sum);
                Some(*floor)
            })
            .sum::<f64>();
        size / 2.0_f64.mul_add(pairs, -1.0).max(size.log10().recip())
    };
    Ok(space
        .keys()
        .enumerate()
        .map(|(dimension, key)| {
            let values = traces
                .iter()
                .flat_map(|trace| {
                    let row = trace.draws.row(dimension);
                    iter::chain(row.columns_range(..half), row.columns_range(length.strict_sub(half)..length)).copied().collect::<Vec<_>>()
                })
                .collect::<Vec<_>>();
            let mut ordered = Data::new(values.clone());
            let bulk = scores(&mut ordered);
            let [low, median, high] = [0.05, 0.5, 0.95].map(|tail| ordered.quantile(tail));
            let folded = scores(&mut Data::new(values.iter().map(|value| (value - median).abs()).collect()));
            let [below, above] = [low, high].map(|quantile| values.iter().map(|&value| if value <= quantile { 1.0 } else { 0.0 }).collect::<Vec<_>>());
            (key, Convergence { rhat: rhat(&bulk).max(rhat(&folded)), ess_bulk: ess(&bulk), ess_tail: ess(&below).min(ess(&above)) })
        })
        .collect())
}

// --- [GRADING]
pub fn fit(pairs: impl IntoIterator<Item = (f64, f64)>, parameters: u32) -> Result<Fit, StatsError> {
    let ((measurements, residuals, squares), non_finite) = pairs.into_iter().fold((<(Mean, Mean, Mean)>::default(), 0_u64), |(mut moments, non_finite), (measured, simulated)| {
        let residual = measured - simulated;
        if measured.is_finite() && simulated.is_finite() {
            moments.extend([(measured, residual, residual.powi(2))]);
            (moments, non_finite)
        } else {
            (moments, non_finite.strict_add(1))
        }
    });
    (measurements.len() > u64::from(parameters)).ok_or(StatsError::Pairs { found: measurements.len(), parameters })?;
    let scale = measurements.mean().abs();
    (scale > 0.0).ok_or(StatsError::Zero(Moment::Mean))?;
    let count = nalgebra::convert::<u64, f64>(measurements.len());
    let dof = count - f64::from(parameters);
    Ok(Fit { nmbe: count * residuals.mean() / (dof * scale), cv_rmse: (count * squares.mean() / dof).sqrt() / scale, non_finite })
}

#[must_use]
pub const fn calibrated(fit: Fit, resolution: Resolution) -> bool {
    let (nmbe, cv_rmse): (f64, f64) = match resolution {
        Resolution::Monthly => (0.05, 0.15),
        Resolution::Hourly => (0.10, 0.30),
    };
    fit.nmbe.abs() <= nmbe && fit.cv_rmse <= cv_rmse
}
