#![feature(default_field_values, deref_patterns, trait_alias, try_blocks)]

use clarabel::{
    algebra::SparseFormatError,
    solver::{DataUpdateError, DefaultSolution, SolverError, SupportedConeT},
};
use good_lp::{ResolutionError, solvers::MipGapError};
use nalgebra::DVector;
use nlopt::{Algorithm, FailState, SuccessState};

use crate::{conic::Refusal, nonlinear::Termination};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Input {
    #[display("start")]
    Start,
    #[display("initial step")]
    InitialStep,
    #[display("xtol_abs")]
    XtolAbs,
    #[display("local xtol_abs")]
    LocalXtolAbs,
    #[display("demand rows of resource {resource}")]
    DemandRows { resource: usize },
    #[display("demand columns of resource {resource}")]
    DemandColumns { resource: usize },
    #[display("capacities of resource {resource}")]
    Capacities { resource: usize },
    #[display("constraint columns")]
    Constraints,
    #[display("quadratic rows")]
    QuadraticRows,
    #[display("quadratic columns")]
    QuadraticColumns,
    #[display("columns of block {block}")]
    Block { block: usize },
}

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
    #[error("item {item} holds no share a finite cost and demand permit")]
    Item { item: usize },
    #[error("{cone} block {reason}")]
    Cone { cone: SupportedConeT<f64>, reason: Refusal },
    #[error("quadratic entry ({row}, {column}) differs from its mirrored entry")]
    Quadratic { row: usize, column: usize },
    #[error("update cones differ from the base cones at block {block}")]
    Cones { block: usize },
    #[error(transparent)]
    Format(#[from] SparseFormatError),
    #[error(transparent)]
    Solver(#[from] SolverError),
    #[error(transparent)]
    Update(#[from] DataUpdateError),
    #[error("conic solve ended {}", solution.status)]
    Conic { solution: DefaultSolution<f64> },
    #[error(transparent)]
    Gap(#[from] MipGapError),
    #[error(transparent)]
    Resolution(#[from] ResolutionError),
    #[error("nlopt refused to run its algorithm {}", failure(*state))]
    Algorithm { algorithm: Algorithm, state: FailState },
    #[error("nlopt refused constraint {constraint} {}", failure(*state))]
    Constraint { constraint: usize, state: FailState },
    #[error("nonlinear solve ended {termination} at objective {objective}")]
    Nonlinear { point: DVector<f64>, objective: f64, termination: Termination },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

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

pub mod conic;
pub mod linear;
pub mod nonlinear;
pub mod pareto;
