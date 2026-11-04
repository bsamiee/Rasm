use core::{iter, num::NonZero, ops::ControlFlow};

use average::Covariance;
use nalgebra::{DMatrix, DMatrixView, convert};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use statrs::statistics::{Data, OrderStatistics as _, RankTieBreaker, Statistics};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Dependence {
    Covariance,
    Pearson,
    Spearman,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [QUANTILES]
#[must_use]
pub fn quantile(frequencies: impl IntoIterator<Item = (f64, usize)>, probability: f64) -> Option<f64> {
    (0.0..=1.0).contains(&probability).then_some(())?;
    let mut frequencies = frequencies.into_iter().filter(|&(_, count)| count > 0).collect::<Vec<_>>();
    let minimum = || Statistics::min(frequencies.iter().map(|&(value, _)| value));
    match quantile_position(NonZero::new(frequencies.iter().try_fold(0_usize, |total, &(_, count)| total.checked_add(count))?)?.get(), probability) {
        ControlFlow::Break(order) => Some(if order == 1 { minimum() } else { Statistics::max(frequencies.iter().map(|&(value, _)| value)) }),
        ControlFlow::Continue((rank, interpolate)) => {
            let minimum = (rank == 1).then(minimum);
            frequencies.sort_unstable_by(|(left, _), (right, _)| left.total_cmp(right));
            let mut samples = frequencies.into_iter().flat_map(|(value, count)| iter::repeat_n(value, count));
            let (lower, upper) = match minimum {
                Some(lower) => (lower, samples.nth(rank)?),
                None => (samples.nth(rank.strict_sub(1))?, samples.next()?),
            };
            Some(interpolate(lower, upper))
        }
    }
}

#[must_use]
pub(crate) fn quantile_data<D>(data: &mut Data<D>, probability: f64) -> f64
where D: AsRef<[f64]> + AsMut<[f64]> {
    match (0.0..=1.0).contains(&probability).then(|| quantile_position(data.len(), probability)) {
        Some(ControlFlow::Break(order)) => data.order_statistic(order),
        Some(ControlFlow::Continue((order, interpolate))) => interpolate(data.order_statistic(order), data.order_statistic(order.strict_add(1))),
        None => f64::NAN,
    }
}

#[expect(clippy::float_cmp, reason = "R8 endpoint probabilities are exact domain boundaries")]
fn quantile_position(count: usize, probability: f64) -> ControlFlow<usize, (usize, impl FnOnce(f64, f64) -> f64)> {
    let offset = 1.0 / 3.0;
    let position = (convert::<usize, f64>(count) + offset).mul_add(probability, offset);
    match position.to_int_saturating::<usize>() {
        0 => ControlFlow::Break(1),
        rank if rank >= count || probability == 1.0 => ControlFlow::Break(count),
        rank => {
            let fraction = position - convert::<usize, f64>(rank);
            ControlFlow::Continue((rank, move |lower, upper| fraction.mul_add(upper, (1.0 - fraction) * lower)))
        }
    }
}

// --- [DEPENDENCE]
#[must_use]
pub fn dependence(points: DMatrixView<'_, f64>, measure: Dependence) -> DMatrix<f64> {
    let ranked = |values: Vec<f64>| Data::new(values).ranks(RankTieBreaker::Average);
    let variables = (0..points.nrows())
        .into_par_iter()
        .map(|variable| {
            let row = points.row(variable);
            let ranks = (measure == Dependence::Spearman && row.iter().all(|value| value.is_finite())).then(|| ranked(row.iter().copied().collect()));
            (row, ranks)
        })
        .collect::<Vec<_>>();
    symmetric(&variables, |(left, left_ranks), (right, right_ranks)| {
        let complete = || iter::zip(left.iter().copied(), right.iter().copied()).filter(|(x, y)| x.is_finite() && y.is_finite());
        let moments = match (measure, left_ranks, right_ranks) {
            (Dependence::Covariance | Dependence::Pearson, _, _) => complete().collect::<Covariance>(),
            (Dependence::Spearman, Some(x), Some(y)) => iter::zip(x.iter().copied(), y.iter().copied()).collect(),
            (Dependence::Spearman, _, _) => {
                let (x, y): (Vec<f64>, Vec<f64>) = complete().unzip();
                iter::zip(ranked(x), ranked(y)).collect()
            }
        };
        match measure {
            Dependence::Covariance => moments.sample_covariance(),
            Dependence::Pearson | Dependence::Spearman => moments.pearson().clamp(-1.0, 1.0),
        }
    })
}

// --- [MATRICES]
#[must_use]
pub(crate) fn symmetric<T, F>(items: &[T], entry: F) -> DMatrix<f64>
where
    T: Sync,
    F: Fn(&T, &T) -> f64 + Sync,
{
    let entry = &entry;
    let mut matrix = DMatrix::from_vec(
        items.len(),
        items.len(),
        items.par_iter().enumerate().flat_map_iter(|(column, right)| items.iter().enumerate().map(move |(row, left)| if row <= column { entry(left, right) } else { 0.0_f64 })).collect(),
    );
    matrix.fill_lower_triangle_with_upper_triangle();
    matrix
}
