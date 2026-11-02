use core::iter;

use average::Covariance;
use nalgebra::{DMatrix, DMatrixView};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
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
