use core::{iter, ops::ControlFlow};

use nalgebra::DMatrix;
use nonempty_collections::NEVec;
use petgraph::{
    data::{Element, FromElements as _},
    stable_graph::StableDiGraph,
};
use rasm_dependency::cycles::{self, Torn};
use uom::si::f64::Time;
use validated::Validated;

use crate::{Causality, Connection, DynamicsError, Endpoint, Grid, Port, Stop, Trajectory, cosimulation::Fmu, lumped::Instance};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
struct Reading {
    source: Endpoint,
    scale: f64,
    shift: f64,
    value: f64,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, derive_more::From)]
pub enum Subsystem {
    #[from(Fmu)]
    Fmu(Box<Fmu>),
    #[from(Instance)]
    Network(Box<Instance>),
}

impl AsRef<[Port]> for Subsystem {
    fn as_ref(&self) -> &[Port] {
        match self {
            Self::Fmu(fmu) => (**fmu).as_ref(),
            Self::Network(instance) => (**instance).as_ref(),
        }
    }
}

impl Subsystem {
    pub fn set(&mut self, values: impl IntoIterator<Item = (u32, f64)>) -> Result<(), NEVec<DynamicsError>> {
        let (references, values): (Vec<_>, Vec<_>) = values.into_iter().unzip();
        references
            .iter()
            .filter(|&&reference| !self.as_ref().iter().any(|port| port.reference == reference && port.causality != Causality::Output))
            .map(|&reference| Err(DynamicsError::Reference { reference }))
            .collect::<Validated<(), _>>()
            .ok()?;
        self.write(&references, &values).map_err(NEVec::new)
    }

    fn write(&mut self, references: &[u32], values: &[f64]) -> Result<(), DynamicsError> {
        match self {
            Self::Fmu(fmu) => fmu.set(references, values),
            Self::Network(instance) => {
                instance.set(references, values);
                Ok(())
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn trajectory(subsystems: Vec<Subsystem>, connections: &[Connection], grid: &Grid, probes: &[Endpoint]) -> Result<Trajectory, NEVec<DynamicsError>> {
    fn pass(
        ordered: &mut [(usize, Subsystem)],
        couplings: &mut [(Endpoint, Reading)],
        recorded: &mut [Reading],
        time: Time,
        mut act: impl FnMut(&mut Subsystem, &[u32], &[f64]) -> Result<ControlFlow<()>, DynamicsError>,
    ) -> Result<Option<usize>, NEVec<DynamicsError>> {
        ordered
            .iter_mut()
            .map(|(index, subsystem)| try {
                let failure = failed(*index, time);
                let (references, inputs): (Vec<_>, Vec<_>) = couplings.iter().filter(|(target, _)| target.subsystem == *index).map(|(target, reading)| (target.reference, reading.value)).unzip();
                let flow = act(subsystem, &references, &inputs).map_err(failure)?;
                let due = couplings.iter_mut().map(|(_, reading)| reading).chain(&mut *recorded).filter(|reading| reading.source.subsystem == *index).collect::<Vec<_>>();
                let sources = due.iter().map(|reading| reading.source.reference).collect::<Vec<_>>();
                let read = match subsystem {
                    Subsystem::Fmu(fmu) => fmu.get(&sources),
                    Subsystem::Network(instance) => Ok(instance.get(&sources)),
                }
                .map_err(failure)?;
                iter::zip(due, read).for_each(|(reading, value)| reading.value = reading.scale.mul_add(value, reading.shift));
                flow.break_value().map(|()| *index)
            })
            .find_map(Result::transpose)
            .transpose()
            .map_err(NEVec::new)
    }
    let port = |endpoint: Endpoint, causality: Causality| {
        subsystems
            .get(endpoint.subsystem)
            .and_then(|subsystem| subsystem.as_ref().iter().find(|port| port.reference == endpoint.reference && port.causality == causality))
            .ok_or(DynamicsError::Endpoint { endpoint, causality })
    };
    let couple = |connection @ Connection { source, target }: Connection| match Validated::from(port(source, Causality::Output)).map2(Validated::from(port(target, Causality::Input)), |exit, entry| {
        match (exit.unit, entry.unit) {
            (Some(from), Some(to)) => (from.exponents == to.exponents && [from, to].iter().all(|unit| unit.factor.is_normal() && unit.offset.is_finite()))
                .ok_or(DynamicsError::Units { connection, units: [from, to] })
                .map(|()| (target, Reading { source, scale: from.factor / to.factor, shift: (from.offset - to.offset) / to.factor, value: f64::NAN })),
            (None, _) | (_, None) => Ok((target, Reading { source, scale: 1.0, shift: 0.0, value: f64::NAN })),
        }
    }) {
        Validated::Good(related) => Validated::from(related),
        Validated::Fail(refused) => Validated::Fail(refused),
    };
    let probe = |source: Endpoint| {
        port(source, Causality::Output).map(|&Port { name: _, causality: _, reference: _, unit }| {
            let (scale, shift) = unit.map_or((1.0, 0.0), |stated| (stated.factor, stated.offset));
            Reading { source, scale, shift, value: f64::NAN }
        })
    };
    let (mut couplings, mut recorded) = connections
        .iter()
        .copied()
        .map(couple)
        .collect::<Validated<Vec<_>, _>>()
        .map2(probes.iter().copied().map(probe).collect::<Validated<Vec<_>, _>>(), |couplings, recorded| (couplings, recorded))
        .ok()?;
    let Torn { graph, edges: _ } = cycles::tear(StableDiGraph::<(), ()>::from_elements(iter::chain(
        subsystems.iter().map(|_| Element::Node { weight: () }),
        connections.iter().map(|connection| Element::Edge { source: connection.source.subsystem, target: connection.target.subsystem, weight: () }),
    )));
    let evaluation = rasm_dependency::evaluation(graph).map_err(|source| NEVec::new(DynamicsError::from(source)))?;
    let mut slots = subsystems.into_iter().map(Some).collect::<Vec<_>>();
    let mut ordered = evaluation.order().nodes_iter().filter_map(|node| Some((node.index(), slots.get_mut(node.index())?.take()?))).collect::<Vec<_>>();
    let (&start, &end) = grid.first().zip(grid.last()).ok_or_else(|| NEVec::new(DynamicsError::Empty))?;
    pass(&mut ordered, &mut couplings, &mut recorded, start, |subsystem, _, _| {
        match subsystem {
            Subsystem::Fmu(fmu) => fmu.initialize(start, end),
            Subsystem::Network(_) => Ok(()),
        }
        .map(|()| ControlFlow::Continue(()))
    })?;
    pass(&mut ordered, &mut couplings, &mut recorded, start, |subsystem, references, inputs| {
        subsystem.write(references, inputs)?;
        match subsystem {
            Subsystem::Fmu(fmu) => fmu.exit(),
            Subsystem::Network(_) => Ok(()),
        }
        .map(|()| ControlFlow::Continue(()))
    })?;
    let mut times = vec![start];
    let mut columns = recorded.iter().map(|reading| reading.value).collect::<Vec<_>>();
    let ended = grid
        .array_windows()
        .map(|&[time, next]| try {
            let requested = pass(&mut ordered, &mut couplings, &mut recorded, time, |subsystem, references, inputs| {
                subsystem.write(references, inputs)?;
                match subsystem {
                    Subsystem::Fmu(fmu) => fmu.step(time, next - time),
                    Subsystem::Network(instance) => instance.step(time, next).map(|()| ControlFlow::Continue(())),
                }
            })?;
            if requested.is_none() {
                times.push(next);
                columns.extend(recorded.iter().map(|reading| reading.value));
            }
            requested.map(|subsystem| (Stop::Request { subsystem }, time))
        })
        .find_map(Result::transpose)
        .transpose()?;
    let (stop, reached) = ended.map_or((None, end), |(stop, time)| (Some(stop), time));
    ordered
        .iter_mut()
        .map(|(index, subsystem)| {
            match subsystem {
                Subsystem::Fmu(fmu) => fmu.terminate(),
                Subsystem::Network(_) => Ok(()),
            }
            .map_err(failed(*index, reached))
        })
        .collect::<Validated<(), _>>()
        .ok()?;
    let values = DMatrix::from_vec(recorded.len(), times.len(), columns);
    Ok(Trajectory { times, values, stop })
}

const fn failed(subsystem: usize, time: Time) -> impl Fn(DynamicsError) -> DynamicsError + Copy { move |source| DynamicsError::Subsystem { subsystem, time, source: Box::new(source) } }
