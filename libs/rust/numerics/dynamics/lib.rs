#![feature(deref_patterns, float_conversions, try_blocks)]

use core::{fmt, iter};
use std::path::PathBuf;

use diffsol::DiffsolError;
use faer::sparse::{CreationError, linalg::LltError};
use itertools::Itertools as _;
use nalgebra::DMatrix;
use nonempty_collections::NEVec;
use petgraph::graph::{EdgeIndex, NodeIndex};
use uom::{
    fmt::DisplayStyle::Abbreviation,
    si::{
        Dimension,
        f64::{HeatCapacity, ThermalConductance, Time},
        heat_capacity::joule_per_kelvin,
        thermal_conductance::watt_per_kelvin,
        time::second,
    },
    typenum::Integer as _,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Causality {
    Parameter,
    Input,
    Output,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, derive_more::Display)]
#[display("{}", fmt::from_fn(|formatter| {
    let symbols = iter::zip(["kg", "m", "s", "A", "K", "mol", "cd", "rad"], exponents).filter(|&(_, &exponent)| exponent != 0).format_with(" ", |(symbol, exponent), write| match *exponent {
        1 => write(&symbol),
        _ => write(&format_args!("{symbol}^{exponent}")),
    });
    let base = fmt::from_fn(|written| match exponents {
        [0, 0, 0, 0, 0, 0, 0, 0] => written.write_str("1"),
        _ => write!(written, "{symbols}"),
    });
    match (*factor, *offset) {
        (1.0, 0.0) => write!(formatter, "{base}"),
        (1.0, _) => write!(formatter, "{base} + {offset}"),
        (_, 0.0) => write!(formatter, "{factor} {base}"),
        (_, _) => write!(formatter, "{factor} {base} + {offset}"),
    }
}))]
pub struct BaseUnit {
    pub exponents: [i32; 8],
    pub factor: f64,
    pub offset: f64,
}

impl BaseUnit {
    #[must_use]
    pub const fn of<D>() -> Self
    where D: Dimension + ?Sized {
        Self { exponents: [D::M::I32, D::L::I32, D::T::I32, D::I::I32, D::Th::I32, D::N::I32, D::J::I32, 0], factor: 1.0, offset: 0.0 }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Kind {
    #[display("real{}", fmt::from_fn(|formatter| _0.map_or(Ok(()), |unit| write!(formatter, " in {unit}"))))]
    Real(Option<BaseUnit>),
    Integer,
    Boolean,
}

impl Kind {
    #[must_use]
    pub fn conversion(self, target: Self) -> Option<(f64, f64)> {
        match (self, target) {
            (Self::Real(Some(from)), Self::Real(Some(to))) => {
                let (scale, shift) = (from.factor / to.factor, (from.offset - to.offset) / to.factor);
                (from.exponents == to.exponents && scale.is_normal() && shift.is_finite()).then_some((scale, shift))
            }
            (Self::Real(None), Self::Real(_)) | (Self::Real(_), Self::Real(None)) | (Self::Integer, Self::Integer) | (Self::Boolean, Self::Boolean) => Some((1.0, 0.0)),
            (Self::Real(_), Self::Integer | Self::Boolean) | (Self::Integer, Self::Real(_) | Self::Boolean) | (Self::Boolean, Self::Real(_) | Self::Integer) => None,
        }
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Port {
    pub name: String,
    pub causality: Causality,
    pub reference: usize,
    pub kind: Kind,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display("port {reference} of subsystem {subsystem}")]
pub struct Endpoint {
    pub subsystem: usize,
    pub reference: usize,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Connection {
    pub source: Endpoint,
    pub target: Endpoint,
}

#[nutype::nutype(
    validate(
        with = |instants: &NEVec<Time>| -> Result<(), DynamicsError> {
            let first = *instants.first();
            NEVec::try_from_vec(
                iter::chain(
                    (!first.is_finite()).then_some((0, first)),
                    iter::zip(1.., instants.as_ref().array_windows()).filter(|&(_, [earlier, instant])| !instant.is_finite() || instant <= earlier).map(|(index, &[_, instant])| (index, instant)),
                )
                .collect(),
            )
            .map_or(Ok(()), |instants| Err(DynamicsError::Instants { instants }))
        },
        error = DynamicsError
    ),
    derive(Debug, Clone, PartialEq, Deref)
)]
pub struct Grid(NEVec<Time>);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Stop {
    Root { function: usize },
    Request { subsystem: usize, time: Time },
    Cancelled,
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Trajectory {
    #[as_ref([Time])]
    times: Vec<Time>,
    #[as_ref]
    values: DMatrix<f64>,
    #[as_ref]
    stop: Option<Stop>,
}

#[derive(Debug, Clone, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Operation {
    #[display("import of {}", _0.display())]
    Import(PathBuf),
    #[display("instantiation of {}", _0.display())]
    Instantiate(PathBuf),
    Initialize,
    Set,
    Get,
    Step,
    Terminate,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum DynamicsError {
    #[error(
        "instants {} are not finite or do not follow their predecessors",
        instants.iter().format_with(", ", |(index, instant), write| write(&format_args!("{} at index {index}", instant.into_format_args(second, Abbreviation))))
    )]
    Instants { instants: NEVec<(usize, Time)> },
    #[error("a scenario of {found} parameters meets a problem of {expected}")]
    Parameters { expected: usize, found: usize },
    #[error(transparent)]
    Solver(#[from] DiffsolError),
    #[error("node {} holds capacity {} that is negative or not finite", node.index(), capacity.into_format_args(joule_per_kelvin, Abbreviation))]
    Capacity { node: NodeIndex, capacity: HeatCapacity },
    #[error("edge {} holds conductance {} that is not a positive normal number", edge.index(), conductance.into_format_args(watt_per_kelvin, Abbreviation))]
    Conductance { edge: EdgeIndex, conductance: ThermalConductance },
    #[error("{found} initial temperatures meet a network of {nodes} nodes")]
    Initial { nodes: usize, found: usize },
    #[error("components {} reach no boundary and hold no steady state", components.iter().map(|component| component.iter().map(|node| node.index()).format(", ")).format("; "))]
    Floating { components: NEVec<Vec<NodeIndex>> },
    #[error(transparent)]
    Assembly(#[from] CreationError),
    #[error(transparent)]
    Factorization(#[from] LltError),
    #[error("FMU {} declares FMI {major}, neither 2 nor 3", path.display())]
    Version { path: PathBuf, major: u64 },
    #[error("{operation} failed")]
    Fmi { operation: Operation, source: fmi::Error },
    #[error("port {reference} is no parameter or input")]
    Reference { reference: usize },
    #[error("{endpoint} names no {causality} port")]
    Endpoint { endpoint: Endpoint, causality: Causality },
    #[error("connection from {} to {} joins {}, which no conversion relates", connection.source, connection.target, kinds.iter().format(" to "))]
    Connection { connection: Connection, kinds: Box<[Kind; 2]> },
    #[error("subsystem {subsystem} failed at {}", time.into_format_args(second, Abbreviation))]
    Subsystem { subsystem: usize, time: Time, source: Box<Self> },
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod cosimulation;
pub mod lumped;
pub mod master;
pub mod ode;
