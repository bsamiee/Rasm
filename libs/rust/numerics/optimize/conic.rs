use core::{iter, slice};

use clarabel::{
    algebra::{BlockConcatenate as _, CscMatrix, MatrixConcatenationError},
    solver::{DefaultInfo, DefaultSettings, DefaultSolution, DefaultSolver, IPSolver as _, SolverStatus, SupportedConeT},
};

use crate::OptimizeError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Block {
    cone: SupportedConeT<f64>,
    matrix: CscMatrix<f64>,
    b: Vec<f64>,
}

impl Block {
    pub fn new(cone: SupportedConeT<f64>, matrix: CscMatrix<f64>, b: Vec<f64>) -> Result<Self, OptimizeError> {
        let dimension = match &cone {
            SupportedConeT::ZeroConeT(dimension) | SupportedConeT::NonnegativeConeT(dimension) | SupportedConeT::SecondOrderConeT(dimension) => Some(*dimension),
            SupportedConeT::ExponentialConeT() => Some(3),
            SupportedConeT::PowerConeT(alpha) if 0.0 < *alpha && *alpha < 1.0 => Some(3),
            SupportedConeT::GenPowerConeT(alpha, dimension)
                if alpha.iter().all(|&power| power > 0.0) && (1.0 - alpha.iter().sum::<f64>()).abs() < f64::EPSILON * nalgebra::convert::<usize, f64>(alpha.len()) * 0.5 =>
                alpha.len().checked_add(*dimension),
            SupportedConeT::PowerConeT(_) | SupportedConeT::GenPowerConeT(_, _) => None,
        };
        matrix.check_format()?;
        if dimension.is_some_and(|dimension| dimension == matrix.m && dimension == b.len())
            && b.iter().all(|&constant| constant.is_finite() || (constant == f64::INFINITY && matches!(cone, SupportedConeT::NonnegativeConeT(_) | SupportedConeT::SecondOrderConeT(1))))
        {
            Ok(Self { cone, matrix, b })
        } else {
            Err(OptimizeError::Cone { rows: matrix.m, constants: b.len(), cone })
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn solve<F>(settings: DefaultSettings<f64>, termination: F, quadratic: &CscMatrix<f64>, linear: &[f64], blocks: &[Block]) -> Result<DefaultSolution<f64>, OptimizeError>
where F: FnMut(&DefaultInfo<f64>) -> bool + Send + Sync + 'static {
    quadratic.check_format()?;
    let head = CscMatrix::zeros((0, linear.len()));
    let stacked = iter::chain(iter::once(&head), blocks.iter().map(|block| &block.matrix)).collect::<Vec<_>>();
    let matrix = CscMatrix::hvcat(&stacked.iter().map(slice::from_ref).collect::<Vec<_>>()).map_err(|MatrixConcatenationError::IncompatibleDimension| OptimizeError::Columns)?;
    let mut solver = DefaultSolver::new(
        quadratic,
        linear,
        &matrix,
        &blocks.iter().flat_map(|block| block.b.iter().copied()).collect::<Vec<_>>(),
        &blocks.iter().map(|block| block.cone.clone()).collect::<Vec<_>>(),
        DefaultSettings { verbose: false, ..settings },
    )?;
    solver.set_termination_callback(termination);
    solver.solve();
    match solver.solution.status {
        SolverStatus::Solved | SolverStatus::AlmostSolved => Ok(solver.solution),
        SolverStatus::Unsolved
        | SolverStatus::PrimalInfeasible
        | SolverStatus::DualInfeasible
        | SolverStatus::AlmostPrimalInfeasible
        | SolverStatus::AlmostDualInfeasible
        | SolverStatus::MaxIterations
        | SolverStatus::MaxTime
        | SolverStatus::NumericalError
        | SolverStatus::InsufficientProgress
        | SolverStatus::CallbackTerminated => Err(OptimizeError::Conic { solution: solver.solution }),
    }
}
