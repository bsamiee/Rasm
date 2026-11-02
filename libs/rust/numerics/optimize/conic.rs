use core::{cell::LazyCell, fmt, iter, mem, slice};

use clarabel::{
    algebra::{BlockConcatenate as _, CscMatrix, MatrixConcatenationError, SparseFormatError},
    solver::{DataUpdateError, DefaultInfo, DefaultSettings, DefaultSolution, DefaultSolver, IPSolver as _, SolverStatus, SupportedConeT},
};
use itertools::{EitherOrBoth, Itertools as _};
use nonempty_collections::NEVec;
use ordered_float::OrderedFloat;
use validated::Validated;

use crate::{Input, OptimizeError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Refusal {
    #[display(
        "holds {rows} matrix rows and {constants} constants where its cone takes {}",
        fmt::from_fn(|formatter| expected.map_or_else(|| formatter.write_str("more than a usize counts"), |expected| write!(formatter, "{expected}")))
    )]
    Dimension { rows: usize, constants: usize, expected: Option<usize> },
    #[display("holds a power outside its domain")]
    Power,
    #[display("holds constant {index} as NaN or as an infinity outside a nonnegative row")]
    Constant { index: usize },
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Block {
    #[as_ref]
    cone: SupportedConeT<f64>,
    #[as_ref]
    matrix: CscMatrix<f64>,
    #[as_ref]
    b: Vec<f64>,
}

impl Block {
    pub fn new(cone: SupportedConeT<f64>, matrix: CscMatrix<f64>, b: Vec<f64>) -> Result<Self, OptimizeError> {
        matrix.check_format()?;
        let dimension = |expected: Option<usize>| {
            expected.filter(|&expected| expected == matrix.m && expected == b.len()).map(drop).ok_or(Refusal::Dimension { rows: matrix.m, constants: b.len(), expected })
        };
        let unbounded = matches!(cone, SupportedConeT::NonnegativeConeT(_) | SupportedConeT::SecondOrderConeT(1));
        let refusal = match &cone {
            SupportedConeT::ZeroConeT(order) | SupportedConeT::NonnegativeConeT(order) | SupportedConeT::SecondOrderConeT(order) => dimension(Some(*order)),
            SupportedConeT::ExponentialConeT() => dimension(Some(3)),
            SupportedConeT::PowerConeT(alpha) if 0.0 < *alpha && *alpha < 1.0 => dimension(Some(3)),
            SupportedConeT::GenPowerConeT(alpha, order)
                if alpha.iter().all(|&power| power > 0.0) && (1.0 - alpha.iter().sum::<f64>()).abs() < f64::EPSILON * nalgebra::convert::<usize, f64>(alpha.len()) * 0.5 =>
                dimension(alpha.len().checked_add(*order)),
            SupportedConeT::PSDTriangleConeT(order) => dimension(order.checked_add(1).and_then(|next| order.checked_mul(next)).map(|count| count / 2)),
            SupportedConeT::PowerConeT(_) | SupportedConeT::GenPowerConeT(_, _) => Err(Refusal::Power),
        }
        .and_then(|()| b.iter().position(|&constant| !(constant.is_finite() || (unbounded && constant == f64::INFINITY))).map_or(Ok(()), |index| Err(Refusal::Constant { index })));
        match refusal {
            Ok(()) => Ok(Self { cone, matrix, b }),
            Err(reason) => Err(OptimizeError::Cone { cone, reason }),
        }
    }
}

#[derive(Debug, Clone)]
pub struct Update {
    pub quadratic: Option<CscMatrix<f64>> = None,
    pub linear: Vec<f64> = Vec::new(),
    pub blocks: Vec<Block> = Vec::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn triangle(variables: usize, quadratic: &CscMatrix<f64>) -> Result<CscMatrix<f64>, NEVec<OptimizeError>> {
    quadratic.check_format().map_err(|source| NEVec::new(source.into()))?;
    [(Input::QuadraticRows, quadratic.m), (Input::QuadraticColumns, quadratic.n)]
        .into_iter()
        .filter(|&(_, found)| found != variables)
        .map(|(input, found)| Err(OptimizeError::Shape { input, expected: variables, found }))
        .collect::<Validated<(), _>>()
        .ok()?;
    iter::zip(0.., &quadratic.nzval)
        .map(|(index, &value)| (quadratic.index_to_coord(index), value))
        .filter(|&((row, column), value)| row > column && OrderedFloat(value) != OrderedFloat(quadratic.get_entry((column, row)).unwrap_or_default()))
        .map(|((row, column), _)| Err(OptimizeError::Quadratic { row, column }))
        .collect::<Validated<(), _>>()
        .ok()?;
    Ok(quadratic.to_triu())
}

fn stack(variables: usize, blocks: &[Block]) -> Result<(CscMatrix<f64>, Vec<f64>), NEVec<OptimizeError>> {
    blocks
        .iter()
        .map(|block| block.matrix.n)
        .enumerate()
        .filter(|&(_, found)| found != variables)
        .map(|(block, found)| Err(OptimizeError::Shape { input: Input::Block { block }, expected: variables, found }))
        .collect::<Validated<(), _>>()
        .ok()?;
    let head = CscMatrix::zeros((0, variables));
    let stacked = iter::chain(iter::once(&head), blocks.iter().map(|block| &block.matrix)).collect::<Vec<_>>();
    let matrix = CscMatrix::hvcat(&stacked.iter().map(slice::from_ref).collect::<Vec<_>>())
        .map_err(|MatrixConcatenationError::IncompatibleDimension| NEVec::new(SparseFormatError::IncompatibleDimension.into()))?;
    Ok((matrix, blocks.iter().flat_map(|block| block.b.iter().copied()).collect()))
}

pub fn solve<F>(
    settings: DefaultSettings<f64>,
    termination: F,
    quadratic: Option<&CscMatrix<f64>>,
    linear: &[f64],
    blocks: &[Block],
    updates: &[Update],
) -> Result<NEVec<Result<DefaultSolution<f64>, NEVec<OptimizeError>>>, NEVec<OptimizeError>>
where
    F: FnMut(&DefaultInfo<f64>) -> bool + Send + Sync + 'static,
{
    let variables = linear.len();
    let zeros = LazyCell::new(|| CscMatrix::zeros((variables, variables)));
    let (quadratic, (matrix, b)) = triangle(variables, quadratic.unwrap_or_else(|| &zeros))
        .map_or_else(Validated::Fail, Validated::Good)
        .map2(stack(variables, blocks).map_or_else(Validated::Fail, Validated::Good), |quadratic, stacked| (quadratic, stacked))
        .ok()?;
    let cones = blocks.iter().map(|block| block.cone.clone()).collect::<Vec<_>>();
    let settings = match updates {
        [] => DefaultSettings { verbose: false, ..settings },
        [_, ..] => DefaultSettings { verbose: false, presolve_enable: false, input_sparse_dropzeros: false, chordal_decomposition_enable: false, ..settings },
    };
    let mut solver = DefaultSolver::new(&quadratic, linear, &matrix, &b, &cones, settings).map_err(|source| NEVec::new(source.into()))?;
    solver.set_termination_callback(termination);
    let mut outcome = |update: Option<&Update>| -> Result<DefaultSolution<f64>, NEVec<OptimizeError>> {
        if let Some(Update { quadratic, linear, blocks }) = update {
            let quadratic = quadratic.as_ref().map(|quadratic| triangle(variables, quadratic)).transpose()?;
            let (matrix, b) = match blocks {
                [] => None,
                [_, ..] => Some(
                    cones
                        .iter()
                        .zip_longest(blocks.iter().map(|block| &block.cone))
                        .position(|pair| !matches!(pair, EitherOrBoth::Both(base, cone) if base == cone))
                        .map_or_else(|| stack(variables, blocks), |block| Err(NEVec::new(OptimizeError::Cones { block })))?,
                ),
            }
            .unzip();
            (try {
                [(quadratic, DefaultSolver::update_P::<CscMatrix<f64>> as fn(&mut DefaultSolver<f64>, &CscMatrix<f64>) -> Result<(), DataUpdateError>), (matrix, DefaultSolver::update_A)]
                    .into_iter()
                    .try_for_each(|(data, set)| data.map_or(Ok(()), |data| set(&mut solver, &data)))?;
                solver.update_q(linear)?;
                solver.update_b(&b.unwrap_or_default())?;
            })
            .map_err(|source| NEVec::new(OptimizeError::Update(source)))?;
        }
        solver.solve();
        let solution = mem::replace(&mut solver.solution, DefaultSolution::new(variables, matrix.m));
        match solution.status {
            SolverStatus::Solved | SolverStatus::AlmostSolved => Ok(solution),
            SolverStatus::Unsolved
            | SolverStatus::PrimalInfeasible
            | SolverStatus::DualInfeasible
            | SolverStatus::AlmostPrimalInfeasible
            | SolverStatus::AlmostDualInfeasible
            | SolverStatus::MaxIterations
            | SolverStatus::MaxTime
            | SolverStatus::NumericalError
            | SolverStatus::InsufficientProgress
            | SolverStatus::CallbackTerminated => Err(NEVec::new(OptimizeError::Conic { solution })),
        }
    };
    Ok(NEVec::from((outcome(None), updates.iter().map(|update| outcome(Some(update))).collect())))
}
