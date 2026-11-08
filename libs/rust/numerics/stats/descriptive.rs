use core::iter;

use average::Covariance;
use nalgebra::{DMatrix, DMatrixView, convert};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, ParallelIterator as _};
use statrs::statistics::{Data, OrderStatistics as _, RankTieBreaker};

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
    let total = frequencies.iter().try_fold(0_usize, |total, &(_, count)| total.checked_add(count))?;
    let third = 1.0 / 3.0;
    let position = (convert::<usize, f64>(total) + third).mul_add(probability, third);
    frequencies.sort_unstable_by(|(left, _), (right, _)| left.total_cmp(right));
    match position.to_int_saturating::<usize>() {
        0 => frequencies.first().map(|&(value, _)| value),
        rank if rank >= total => frequencies.last().map(|&(value, _)| value),
        rank => {
            let mut samples = frequencies.into_iter().flat_map(|(value, count)| iter::repeat_n(value, count)).skip(rank.strict_sub(1));
            let (lower, upper) = (samples.next()?, samples.next()?);
            Some((position - convert::<usize, f64>(rank)).mul_add(upper - lower, lower))
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
    let mut matrix = DMatrix::<f64>::zeros(items.len(), items.len());
    matrix.par_column_iter_mut().zip(items).enumerate().for_each(|(column, (mut entries, right))| iter::zip(entries.iter_mut(), &items[..=column]).for_each(|(slot, left)| *slot = entry(left, right)));
    matrix.fill_lower_triangle_with_upper_triangle();
    matrix
}
