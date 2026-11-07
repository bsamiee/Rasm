use core::{
    cell::LazyCell,
    fmt::Debug,
    iter,
    ops::{ControlFlow, Range},
};
use std::collections::HashMap;

use average::{Mean, Variance};
use itertools::Itertools as _;
use nalgebra::{DMatrix, DMatrixView, DVector, DVectorView};
use nonempty_collections::{NEBTreeMap, NEVec, NonEmptyIterator as _};
use num_complex::Complex64;
use num_dual::{Dual64, DualNum as _};
use nuts_rs::{Chain as _, CpuLogpFunc, CpuMath, CpuMathError, EuclideanAdaptOptions, HasDims, NutsSettings, Progress, Settings};
use rand::{Rng, SeedableRng as _, rngs::ChaCha8Rng};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use rustfft::FftPlanner;
use serde::Serialize;
use statrs::{
    distribution::{ContinuousCDF as _, Normal},
    statistics::{Data, OrderStatistics as _, RankTieBreaker},
};
use validated::Validated;

use crate::{Bounds, Moment, Sample, Space, StatsError, descriptive::quantile_data};

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
    Settings::validate(settings).map_err(|source| NEVec::new(StatsError::Settings(source)))?;
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
            let failed = |source| StatsError::Sampler { chain, source };
            let mut rng = ChaCha8Rng::seed_from_u64(settings.seed());
            rng.set_stream(chain);
            let mut sampler = settings
                .new_chain(chain, CpuMath::new(Posterior { space, density: &density, constrained: Vec::with_capacity(dimensions), slopes: Vec::with_capacity(dimensions) }), &mut rng)
                .map_err(failed)?;
            sampler
                .set_position(&iter::zip(starts.column(index).iter(), space.values()).map(|(&x, bounds)| (x - bounds.start).ln() - (bounds.last - x).ln()).collect::<Box<[f64]>>())
                .map_err(failed)?;
            iter::repeat_with(|| sampler.draw().map_err(failed)).take(settings.hint_num_tune().strict_add(settings.hint_num_draws())).process_results(|drawn| {
                drawn.take_while_inclusive(|(_, progress)| stop(progress).is_continue()).filter(|(_, progress)| !progress.tuning).fold(
                    Trace { draws: DMatrix::zeros(dimensions, 0), progress: Vec::new() },
                    |mut trace, (draw, progress)| {
                        trace.draws.extend(iter::zip(draw, space.values()).map(|(z, bounds)| bounded(Dual64::from(z), bounds).0.re));
                        trace.progress.push(progress);
                        trace
                    },
                )
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
    let padded = half.strict_mul(2).strict_sub(1).next_power_of_two();
    let stages = nalgebra::convert::<u32, usize>(padded.ilog2());
    let mut transforms = LazyCell::new(|| {
        let mut planner = FftPlanner::<f64>::new();
        let (forward, inverse) = (planner.plan_fft_forward(padded), planner.plan_fft_inverse(padded));
        let scratch = vec![Complex64::default(); forward.get_inplace_scratch_len().max(inverse.get_inplace_scratch_len())];
        (forward, inverse, Vec::with_capacity(padded), scratch)
    });
    let mut autocovariance = |centered: DMatrixView<'_, f64>| {
        let (forward, inverse, buffer, scratch) = LazyCell::force_mut(&mut transforms);
        let mut lagged = centered.column_iter().fold(DVector::<f64>::zeros(half), |mut lagged, chain| {
            buffer.clear();
            buffer.extend(chain.iter().copied().map(Complex64::from));
            buffer.resize(padded, Complex64::default());
            forward.process_with_scratch(buffer, scratch);
            for value in &mut *buffer {
                *value = Complex64::from(value.norm_sqr());
            }
            inverse.process_with_scratch(buffer, scratch);
            lagged.zip_apply(&DVectorView::from_slice(buffer, half), |sum, value| *sum += value.re);
            lagged
        });
        lagged.unscale_mut(nalgebra::convert::<usize, f64>(padded) * draws * nalgebra::convert::<usize, f64>(centered.ncols()));
        lagged
    };
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
    let rhat = |(_, within, plus): &(Vec<Variance>, f64, f64)| (plus / within).sqrt();
    let mut ess = |mut values: Vec<f64>, (moments, within, plus): &(Vec<Variance>, f64, f64)| {
        let size = nalgebra::convert::<usize, f64>(values.len());
        nalgebra::DMatrixViewMut::from_slice(&mut values, half, moments.len()).column_iter_mut().zip(moments).for_each(|(mut chain, moments)| chain.add_scalar_mut(-moments.mean()));
        let centered = DMatrixView::from_slice(&values, half, moments.len());
        let transformed = LazyCell::new(|| autocovariance(centered));
        let autocorrelation = |lag: usize| {
            let lagged = if lag <= stages {
                centered.column_iter().map(|chain| chain.rows_range(..half.strict_sub(lag)).dot(&chain.rows_range(lag..)) / draws).collect::<Mean>().mean()
            } else {
                transformed.row(lag).x
            };
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
    space
        .keys()
        .enumerate()
        .map(|(dimension, key)| {
            let values = Iterator::try_collect::<Vec<_>>(&mut traces.iter().enumerate().flat_map(|(index, trace)| {
                let indexed = |range: Range<usize>| iter::zip(range.clone(), trace.draws.view_range(dimension, range));
                iter::chain(indexed(0..half), indexed(length.strict_sub(half)..length))
                    .map(move |(draw, &value)| value.is_finite().then_some(value).ok_or(StatsError::Draw { trace: index, dimension, draw }))
            }))?;
            let mut ordered = Data::new(values.clone());
            let bulk = scores(&mut ordered);
            let [low, median, high] = [0.05, 0.5, 0.95].map(|tail| quantile_data(&mut ordered, tail));
            let folded = split(&scores(&mut Data::new(values.iter().map(|value| (value - median).abs()).collect())));
            let [below, above] = [low, high].map(|quantile| {
                let indicators = values.iter().map(|&value| if value <= quantile { 1.0 } else { 0.0 }).collect::<Vec<_>>();
                let moments = split(&indicators);
                ess(indicators, &moments)
            });
            let moments = split(&bulk);
            Ok((key, Convergence { rhat: rhat(&moments).max(rhat(&folded)), ess_bulk: ess(bulk, &moments), ess_tail: below.min(above) }))
        })
        .collect()
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
