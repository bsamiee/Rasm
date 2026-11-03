use core::{
    iter,
    ops::ControlFlow,
    sync::atomic::{AtomicBool, Ordering},
};

use nalgebra::DMatrix;
use nonempty_collections::NEVec;
use petgraph::{
    data::{Element, FromElements as _},
    stable_graph::StableDiGraph,
};
use rasm_dependency::cycles::{self, Torn};
use uom::si::f64::Time;
use validated::Validated;

use crate::{Causality, Connection, DynamicsError, Endpoint, Grid, Kind, Port, Stop, Trajectory, cosimulation::Fmu, lumped::Instance};

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
    pub fn set(&mut self, values: impl IntoIterator<Item = (usize, f64)>) -> Result<(), NEVec<DynamicsError>> {
        let (references, values): (Vec<_>, Vec<_>) = values.into_iter().unzip();
        references
            .iter()
            .filter(|&&reference| !self.as_ref().iter().any(|port| port.reference == reference && port.causality != Causality::Output))
            .map(|&reference| Err(DynamicsError::Reference { reference }))
            .collect::<Validated<(), _>>()
            .ok()?;
        self.write(&references, &values).map_err(NEVec::new)
    }

    fn write(&mut self, references: &[usize], values: &[f64]) -> Result<(), DynamicsError> {
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

pub fn trajectory(subsystems: Vec<Subsystem>, connections: &[Connection], grid: &Grid, probes: &[Endpoint], cancel: &AtomicBool) -> Result<Trajectory, NEVec<DynamicsError>> {
    fn pass(
        ordered: &mut [(usize, Subsystem)],
        couplings: &mut [(Endpoint, Reading)],
        recorded: &mut [Reading],
        time: Time,
        mut act: impl FnMut(&mut Subsystem, &[usize], &[f64]) -> Result<ControlFlow<Time>, DynamicsError>,
    ) -> Result<Option<(usize, Time)>, NEVec<DynamicsError>> {
        ordered
            .iter_mut()
            .map(|(index, subsystem)| try {
                let failure = failed(*index, time);
                let (references, inputs): (Vec<_>, Vec<_>) = couplings.iter().filter(|(target, _)| target.subsystem == *index).map(|(target, reading)| (target.reference, reading.value)).unzip();
                match act(subsystem, &references, &inputs).map_err(failure)? {
                    ControlFlow::Break(reached) => Some((*index, reached)),
                    ControlFlow::Continue(()) => {
                        let due = couplings.iter_mut().map(|(_, reading)| reading).chain(&mut *recorded).filter(|reading| reading.source.subsystem == *index).collect::<Vec<_>>();
                        let sources = due.iter().map(|reading| reading.source.reference).collect::<Vec<_>>();
                        let read = match subsystem {
                            Subsystem::Fmu(fmu) => fmu.get(&sources),
                            Subsystem::Network(instance) => Ok(instance.get(&sources)),
                        }
                        .map_err(failure)?;
                        iter::zip(due, read).for_each(|(reading, value)| reading.value = reading.scale.mul_add(value, reading.shift));
                        None
                    }
                }
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
        exit.kind
            .conversion(entry.kind)
            .map(|(scale, shift)| (target, Reading { source, scale, shift, value: f64::NAN }))
            .ok_or_else(|| DynamicsError::Connection { connection, kinds: Box::new([exit.kind, entry.kind]) })
    }) {
        Validated::Good(related) => Validated::from(related),
        Validated::Fail(refused) => Validated::Fail(refused),
    };
    let probe = |source: Endpoint| {
        port(source, Causality::Output).map(|exit| {
            let (scale, shift) = match exit.kind {
                Kind::Real(Some(unit)) => (unit.factor, unit.offset),
                Kind::Real(None) | Kind::Integer | Kind::Boolean => (1.0, 0.0),
            };
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
    let start = *grid.first();
    pass(&mut ordered, &mut couplings, &mut recorded, start, |subsystem, _, _| {
        match subsystem {
            Subsystem::Fmu(fmu) => fmu.initialize(grid),
            Subsystem::Network(_) => Ok(()),
        }
        .map(ControlFlow::Continue)
    })?;
    pass(&mut ordered, &mut couplings, &mut recorded, start, |subsystem, references, inputs| {
        subsystem.write(references, inputs)?;
        match subsystem {
            Subsystem::Fmu(fmu) => fmu.exit(),
            Subsystem::Network(_) => Ok(()),
        }
        .map(ControlFlow::Continue)
    })?;
    let mut times = NEVec::new(start);
    let mut columns = recorded.iter().map(|reading| reading.value).collect::<Vec<_>>();
    let stop = grid
        .as_ref()
        .array_windows()
        .map(|&[time, next]| {
            if cancel.load(Ordering::Relaxed) {
                return Ok::<_, NEVec<DynamicsError>>(Some(Stop::Cancelled));
            }
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
            Ok(requested.map(|(subsystem, reached)| Stop::Request { subsystem, time: reached }))
        })
        .find_map(Result::transpose)
        .transpose()?;
    let reached = *times.last();
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
    let values = DMatrix::from_vec(recorded.len(), times.len().get(), columns);
    Ok(Trajectory { times: times.into(), values, stop })
}

const fn failed(subsystem: usize, time: Time) -> impl Fn(DynamicsError) -> DynamicsError + Copy { move |source| DynamicsError::Subsystem { subsystem, time, source: Box::new(source) } }
