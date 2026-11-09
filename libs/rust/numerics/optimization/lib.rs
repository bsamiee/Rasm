#![feature(default_field_values, deref_patterns, trait_alias, try_blocks, try_blocks_heterogeneous)]

#[cfg(feature = "conic")]
use clarabel::{
    algebra::SparseFormatError,
    solver::{DataUpdateError, DefaultSolution, SolverError, SupportedConeT},
};
#[cfg(feature = "linear")]
use good_lp::{ResolutionError, solvers::MipGapError};
#[cfg(feature = "nonlinear")]
use nalgebra::DVector;
#[cfg(feature = "nonlinear")]
use nlopt::{Algorithm, FailState, SuccessState};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Input {
    #[cfg(feature = "nonlinear")]
    #[display("start")]
    Start,
    #[cfg(feature = "nonlinear")]
    #[display("initial step")]
    InitialStep,
    #[cfg(feature = "nonlinear")]
    #[display("xtol_abs")]
    XtolAbs,
    #[cfg(feature = "nonlinear")]
    #[display("local xtol_abs")]
    LocalXtolAbs,
    #[cfg(feature = "linear")]
    #[display("demand rows of resource {resource}")]
    DemandRows { resource: usize },
    #[cfg(feature = "linear")]
    #[display("demand columns of resource {resource}")]
    DemandColumns { resource: usize },
    #[cfg(feature = "linear")]
    #[display("capacities of resource {resource}")]
    Capacities { resource: usize },
    #[display("constraint columns")]
    Constraints,
    #[cfg(feature = "conic")]
    #[display("quadratic rows")]
    QuadraticRows,
    #[cfg(feature = "conic")]
    #[display("quadratic columns")]
    QuadraticColumns,
    #[cfg(feature = "conic")]
    #[display("columns of block {block}")]
    Block { block: usize },
}

#[cfg(feature = "nonlinear")]
#[derive(Debug, Clone)]
pub struct Solution {
    pub point: DVector<f64>,
    pub objective: f64,
    pub status: SuccessState,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum OptimizeError {
    #[error("{input} holds {found} entries where the problem holds {expected}")]
    Shape { input: Input, expected: usize, found: usize },
    #[error("{input} entry {index} lies outside its domain")]
    Entry { input: Input, index: usize },
    #[cfg(feature = "linear")]
    #[error("item {item} holds no share a finite cost and demand permit")]
    Item { item: usize },
    #[cfg(feature = "conic")]
    #[error("{cone} block {reason}")]
    Cone { cone: SupportedConeT<f64>, reason: Refusal },
    #[cfg(feature = "conic")]
    #[error("quadratic entry ({row}, {column}) differs from its mirrored entry")]
    Quadratic { row: usize, column: usize },
    #[cfg(feature = "conic")]
    #[error("update cones differ from the base cones at block {block}")]
    Cones { block: usize },
    #[cfg(feature = "conic")]
    #[error(transparent)]
    Format(#[from] SparseFormatError),
    #[cfg(feature = "conic")]
    #[error(transparent)]
    Solver(#[from] SolverError),
    #[cfg(feature = "conic")]
    #[error(transparent)]
    Update(#[from] DataUpdateError),
    #[cfg(feature = "conic")]
    #[error("conic solve ended {}", solution.status)]
    Conic { solution: DefaultSolution<f64> },
    #[cfg(feature = "linear")]
    #[error(transparent)]
    Gap(#[from] MipGapError),
    #[cfg(feature = "linear")]
    #[error(transparent)]
    Resolution(#[from] ResolutionError),
    #[cfg(feature = "nonlinear")]
    #[error("nlopt refused {algorithm:?} {}", failure(*state))]
    Algorithm { algorithm: Algorithm, state: FailState },
    #[cfg(feature = "nonlinear")]
    #[error("nlopt refused constraint {constraint} {}", failure(*state))]
    Constraint { constraint: usize, state: FailState },
    #[cfg(feature = "nonlinear")]
    #[error("nonlinear solve ended {} at objective {objective}", .termination.map_or("on a non-finite objective", failure))]
    Nonlinear { point: DVector<f64>, objective: f64, termination: Option<FailState> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(feature = "nonlinear")]
const fn failure(state: FailState) -> &'static str {
    match state {
        FailState::Failure => "in failure",
        FailState::InvalidArgs => "on invalid arguments",
        FailState::OutOfMemory => "out of memory",
        FailState::RoundoffLimited => "limited by roundoff",
        FailState::ForcedStop => "on a forced stop",
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[cfg(feature = "conic")]
mod conic;
#[cfg(feature = "linear")]
mod linear;
#[cfg(feature = "nonlinear")]
mod nonlinear;
mod pareto;

// --- [EXPORTS] -------------------------------------------------------------------------

#[cfg(feature = "conic")]
pub use conic::{Block, Refusal, Update, solve};
#[cfg(feature = "linear")]
pub use linear::{Allocation, Coverage, CoverageError, Duals, Integrality, Resource, allocation};
#[cfg(feature = "nonlinear")]
pub use nlopt::Target;
#[cfg(feature = "nonlinear")]
pub use nonlinear::{Constraint, Maxtime, MaxtimeError, Objective, Optimizer, Relation, Settings, Stopval, StopvalError, optimize};
pub use pareto::{Rank, rank};
