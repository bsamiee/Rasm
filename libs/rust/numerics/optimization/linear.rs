use core::{cmp::Ordering, iter, range::RangeInclusive};

use good_lp::{
    Constraint, DualValues as _, Expression, ProblemVariables, Solution as _, SolutionStatus, SolutionWithDual as _, SolverModel as _, Variable, highs,
    solvers::{MipGapError, highs::HighsProblem},
    variable,
};
use itertools::{Itertools as _, izip};
use nalgebra::{DMatrix, DMatrixView, DVector, DVectorView};
use nonempty_collections::NEVec;
use validated::Validated;

use crate::{Input, OptimizeError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Integrality {
    Binary,
    Continuous,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(
    validate(predicate = |coverage| coverage.start.is_finite() && coverage.start >= 0.0 && coverage.start <= coverage.last),
    derive(Debug, Clone, Copy, PartialEq, Deref)
)]
pub struct Coverage(RangeInclusive<f64>);

#[derive(Debug, Clone, Copy)]
pub struct Resource<'data> {
    pub demands: DMatrixView<'data, f64>,
    pub capacities: DVectorView<'data, f64>,
}

#[derive(Debug, Clone)]
pub struct Duals {
    pub capacities: DMatrix<f64>,
    pub coverage: DVector<f64>,
    pub constraints: Vec<f64>,
}

#[derive(Debug, Clone)]
pub struct Allocation {
    pub shares: DMatrix<f64>,
    pub objective: f64,
    pub status: SolutionStatus,
    pub duals: Option<Duals>,
    pub variables: Vec<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn allocation<M, C>(costs: DMatrixView<'_, f64>, resources: &[Resource<'_>], coverage: Coverage, integrality: Integrality, model: M, configure: C) -> Result<Allocation, NEVec<OptimizeError>>
where
    M: FnOnce(&mut ProblemVariables, &DMatrix<Variable>) -> (Expression, Vec<Constraint>),
    C: FnOnce(HighsProblem) -> Result<HighsProblem, MipGapError>,
{
    fn weighted<'term>(terms: impl IntoIterator<Item = (&'term f64, &'term bool, &'term Variable)>) -> Expression {
        let mut expression = Expression::default();
        terms.into_iter().filter(|&(weight, permitted, _)| *permitted && *weight != 0.0).for_each(|(&weight, _, &share)| expression.add_mul(weight, share));
        expression
    }
    resources
        .iter()
        .enumerate()
        .flat_map(|(resource, Resource { demands, capacities })| {
            iter::chain(
                [
                    (Input::DemandRows { resource }, costs.nrows(), demands.nrows()),
                    (Input::DemandColumns { resource }, costs.ncols(), demands.ncols()),
                    (Input::Capacities { resource }, costs.ncols(), capacities.len()),
                ]
                .map(|(input, expected, found)| (expected == found).ok_or(OptimizeError::Shape { input, expected, found })),
                capacities
                    .iter()
                    .positions(|capacity| !matches!(capacity.partial_cmp(&f64::NEG_INFINITY), Some(Ordering::Greater)))
                    .map(move |index| Err(OptimizeError::Entry { input: Input::Capacities { resource }, index })),
            )
        })
        .collect::<Validated<(), _>>()
        .ok()?;
    let permitted = resources.iter().fold(costs.map(f64::is_finite), |mut permitted, resource| {
        permitted.zip_apply(&resource.demands, |permitted, demand| *permitted = *permitted && demand.is_finite());
        permitted
    });
    permitted.row_iter().positions(|row| coverage.start > 0.0 && !row.iter().any(|&share| share)).map(|item| Err(OptimizeError::Item { item })).collect::<Validated<(), _>>().ok()?;
    let mut variables = ProblemVariables::new();
    let shares = {
        let definition = match integrality {
            Integrality::Binary => variable().binary(),
            Integrality::Continuous => variable().bounds(0.0..=1.0),
        };
        DMatrix::from_vec(costs.nrows(), costs.ncols(), variables.add_all(permitted.iter().map(|&permitted| definition.clone().max(u8::from(permitted)))))
    };
    let (terms, constraints) = model(&mut variables, &shares);
    let integer = variables.iter_variables_with_def().any(|(_, definition)| definition.is_integer());
    let objective = weighted(izip!(costs, &permitted, &shares)) + terms;
    let mut problem = configure(variables.minimise(&objective).using(highs)).map_err(|source| NEVec::new(source.into()))?;
    let bounds = shares
        .row_iter()
        .map(|row| {
            let total = row.iter().sum::<Expression>();
            [problem.add_constraint(total.clone().geq(coverage.start)), problem.add_constraint(total.leq(coverage.last))]
        })
        .collect::<Vec<_>>();
    let limits = resources
        .iter()
        .flat_map(|resource| {
            izip!(resource.demands.column_iter(), permitted.column_iter(), shares.column_iter(), &resource.capacities)
                .map(|(demands, permitted, shares, &capacity)| (weighted(izip!(demands, permitted, shares)), capacity))
        })
        .map(|(load, capacity)| problem.add_constraint(load.leq(capacity)))
        .collect::<Vec<_>>();
    let rows = constraints.into_iter().map(|constraint| problem.add_constraint(constraint)).collect::<Vec<_>>();
    let mut solution = problem.solve().map_err(|source| NEVec::new(source.into()))?;
    Ok(Allocation {
        shares: shares.map(|share| solution.value(share)),
        objective: solution.eval(&objective),
        status: solution.status(),
        duals: (!integer).then(|| {
            let dual = solution.compute_dual();
            Duals {
                capacities: DMatrix::from_row_iterator(resources.len(), costs.ncols(), limits.into_iter().map(|limit| dual.dual(limit))),
                coverage: DVector::from_iterator(costs.nrows(), bounds.into_iter().map(|[lower, upper]| dual.dual(lower) + dual.dual(upper))),
                constraints: rows.into_iter().map(|row| dual.dual(row)).collect(),
            }
        }),
        variables: solution.into_inner().columns()[shares.len()..].to_vec(),
    })
}
