use core::iter;

use average::Covariance;
use nalgebra::{DMatrix, DMatrixView, DVectorView};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
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
    let variables = points.transpose();
    let columns = variables.column_iter().collect::<Vec<_>>();
    let statistic = |left: &DVectorView<'_, f64>, right: &DVectorView<'_, f64>| {
        let complete = || left.iter().copied().zip(right.iter().copied()).filter(|(x, y)| x.is_finite() && y.is_finite());
        let moments = match measure {
            Dependence::Covariance | Dependence::Pearson => complete().collect::<Covariance>(),
            Dependence::Spearman => {
                let (x, y): (Vec<f64>, Vec<f64>) = complete().unzip();
                iter::zip(Data::new(x).ranks(RankTieBreaker::Average), Data::new(y).ranks(RankTieBreaker::Average)).collect::<Covariance>()
            }
        };
        match measure {
            Dependence::Covariance => moments.sample_covariance(),
            Dependence::Pearson | Dependence::Spearman => moments.pearson().clamp(-1.0, 1.0),
        }
    };
    let mut symmetric = DMatrix::from_vec(
        columns.len(),
        columns.len(),
        columns.par_iter().enumerate().flat_map_iter(|(column, right)| columns.iter().enumerate().map(move |(row, left)| if row <= column { statistic(left, right) } else { 0.0_f64 })).collect(),
    );
    symmetric.fill_lower_triangle_with_upper_triangle();
    symmetric
}
