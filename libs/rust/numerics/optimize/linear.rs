use core::{iter, range::RangeInclusive};

use good_lp::{Constraint, Expression, ProblemVariables, Variable, variable};
use nalgebra::{DMatrix, DMatrixView, DVectorView};
use nonempty_collections::NEVec;
use validated::Validated;

use crate::OptimizeError;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Split {
    Whole,
    Divisible,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Resource<'data> {
    pub demands: DMatrixView<'data, f64>,
    pub capacities: DVectorView<'data, f64>,
}

#[derive(derive_more::Debug)]
pub struct Allocation {
    #[debug(skip)]
    pub variables: ProblemVariables,
    pub objective: Expression,
    pub coverage: Vec<[Constraint; 2]>,
    pub capacities: Vec<Vec<Constraint>>,
    pub shares: DMatrix<Variable>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn allocation(costs: DMatrixView<'_, f64>, resources: &[Resource<'_>], coverage: RangeInclusive<f64>, split: Split) -> Result<Allocation, NEVec<OptimizeError>> {
    resources
        .iter()
        .flat_map(|resource| [(costs.nrows(), resource.demands.nrows()), (costs.ncols(), resource.demands.ncols()), (costs.ncols(), resource.capacities.len())])
        .map(|(expected, found)| Validated::from((expected == found).ok_or(OptimizeError::Shape { expected, found })))
        .collect::<Validated<Vec<()>, _>>()
        .ok()?;
    let permitted = resources.iter().fold(costs.map(f64::is_finite), |permitted, resource| permitted.zip_map(&resource.demands, |permitted, demand| permitted && demand.is_finite()));
    let definition = match split {
        Split::Whole => variable().binary(),
        Split::Divisible => variable().bounds(0.0..=1.0),
    };
    let mut variables = ProblemVariables::new();
    let shares = DMatrix::from_vec(costs.nrows(), costs.ncols(), variables.add_all(permitted.iter().map(|&permitted| definition.clone().max(u8::from(permitted)))));
    let weighted = |weights: DMatrixView<'_, f64>| weights.zip_zip_map(&permitted, &shares, |weight, permitted, share| permitted.then_some((weight, share)));
    Ok(Allocation {
        variables,
        objective: weighted(costs).iter().flatten().map(|&(cost, share)| cost * share).sum(),
        coverage: shares
            .row_iter()
            .map(|row| {
                let total = row.iter().sum::<Expression>();
                [total.clone().geq(coverage.start), total.leq(coverage.last)]
            })
            .collect(),
        capacities: resources
            .iter()
            .map(|resource| {
                iter::zip(weighted(resource.demands).column_iter(), &resource.capacities)
                    .map(|(column, &capacity)| column.iter().flatten().map(|&(demand, share)| demand * share).sum::<Expression>().leq(capacity))
                    .collect()
            })
            .collect(),
        shares,
    })
}
