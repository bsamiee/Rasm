#![feature(default_field_values, try_blocks)]

use clarabel::{
    algebra::SparseFormatError,
    solver::{DefaultSolution, SolverError, SupportedConeT},
};
use nalgebra::DVector;
use nlopt::{FailState, SuccessState};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Solution {
    pub point: DVector<f64>,
    pub objective: f64,
    pub status: SuccessState,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum OptimizeError {
    #[error("{found} entries where the problem holds {expected}")]
    Shape { expected: usize, found: usize },
    #[error("{cone} refuses a block of {rows} matrix rows and {constants} constants")]
    Cone { cone: SupportedConeT<f64>, rows: usize, constants: usize },
    #[error("a cone block holds a column count other than the variable count")]
    Columns,
    #[error(transparent)]
    Format(#[from] SparseFormatError),
    #[error(transparent)]
    Solver(#[from] SolverError),
    #[error("conic solve ended {}", solution.status)]
    Conic { solution: DefaultSolution<f64> },
    #[error("nlopt refused a setting {}", failure(*state))]
    Setting { state: FailState },
    #[error("nonlinear solve ended {} at objective {objective}", failure(*state))]
    Nonlinear { state: FailState, point: DVector<f64>, objective: f64 },
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
