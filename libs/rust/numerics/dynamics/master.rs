use core::{
    cell::Cell,
    iter,
    ops::ControlFlow,
    sync::atomic::{AtomicBool, Ordering},
};

use nalgebra::DMatrix;
use nonempty_collections::NEVec;
use petgraph::{
    data::{Element, FromElements as _},
    stable_graph::{NodeIndex, StableDiGraph},
};
use rasm_dependency::cycles::{self, Torn};
use uom::si::f64::Time;
use validated::Validated;

use crate::{Causality, Connection, DynamicsError, Endpoint, Grid, Kind, Port, Stop, Trajectory, cosimulation::Fmu, lumped::Instance};

// --- [TYPES] ---------------------------------------------------------------------------

type Exchange<'reading> = (Vec<usize>, Vec<&'reading Reading>);

// --- [MODELS] --------------------------------------------------------------------------

struct Reading {
    source: Endpoint,
    scale: f64,
    shift: f64,
    value: Cell<f64>,
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
            .filter(|&&reference| !self.as_ref().get(reference).is_some_and(|port| port.causality != Causality::Output))
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
        ordered: &[(NodeIndex, Exchange<'_>, Exchange<'_>)],
        subsystems: &mut StableDiGraph<Subsystem, ()>,
        scratch: &mut [Vec<f64>; 2],
        time: Time,
        mut act: impl FnMut(&mut Subsystem, &[usize], &[f64]) -> Result<ControlFlow<Option<Time>>, DynamicsError>,
    ) -> Result<Option<Stop>, NEVec<DynamicsError>> {
        let [inputs, outputs] = scratch;
        ordered
            .iter()
            .map(|(node, (input_references, incoming), (output_references, outgoing))| try {
                let index = node.index();
                let (failure, subsystem) = (failed(index, time), &mut subsystems[*node]);
                inputs.clear();
                inputs.extend(incoming.iter().map(|reading| reading.value.get()));
                match act(subsystem, input_references, inputs).map_err(failure)? {
                    ControlFlow::Break(Some(reached)) => return Ok(Some(Stop::Request { subsystem: index, time: reached })),
                    ControlFlow::Break(None) => return Ok(Some(Stop::Cancelled)),
                    ControlFlow::Continue(()) => {}
                }
                match subsystem {
                    Subsystem::Fmu(fmu) => {
                        outputs.resize(output_references.len(), 0.0);
                        fmu.get(output_references, outputs)
                    }
                    Subsystem::Network(instance) => {
                        outputs.clear();
                        outputs.extend(instance.get(output_references));
                        Ok(())
                    }
                }
                .map_err(failure)?;
                iter::zip(outgoing, &*outputs).for_each(|(reading, &value)| reading.value.set(reading.scale.mul_add(value, reading.shift)));
                None
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
            .map(|(scale, shift)| (target, Reading { source, scale, shift, value: Cell::new(f64::NAN) }))
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
            Reading { source, scale, shift, value: Cell::new(f64::NAN) }
        })
    };
    let (couplings, recorded) = connections
        .iter()
        .copied()
        .map(couple)
        .collect::<Validated<Vec<_>, _>>()
        .map2(probes.iter().copied().map(probe).collect::<Validated<Vec<_>, _>>(), |couplings, recorded| (couplings, recorded))
        .ok()?;
    let Torn { evaluation, blocks: _ } = cycles::tear(
        StableDiGraph::<Subsystem, ()>::from_elements(iter::chain(
            subsystems.into_iter().map(|weight| Element::Node { weight }),
            connections.iter().map(|connection| Element::Edge { source: connection.source.subsystem, target: connection.target.subsystem, weight: () }),
        )),
        |&()| (),
    )
    .map_err(|cycle| NEVec::new(DynamicsError::Cycle { cycle }))?;
    let ordered = evaluation
        .nodes_iter()
        .map(|node| {
            let index = node.index();
            (
                node,
                couplings.iter().filter(|(target, _)| target.subsystem == index).map(|(target, reading)| (target.reference, reading)).unzip(),
                couplings.iter().map(|(_, reading)| reading).chain(&recorded).filter(|reading| reading.source.subsystem == index).map(|reading| (reading.source.reference, reading)).unzip(),
            )
        })
        .collect::<Vec<_>>();
    let mut graph = evaluation.into_inner();
    let mut scratch = [Vec::new(), Vec::new()];
    let start = *grid.first();
    pass(&ordered, &mut graph, &mut scratch, start, |subsystem, _, _| {
        match subsystem {
            Subsystem::Fmu(fmu) => fmu.initialize(grid),
            Subsystem::Network(_) => Ok(()),
        }
        .map(ControlFlow::Continue)
    })?;
    pass(&ordered, &mut graph, &mut scratch, start, |subsystem, references, inputs| {
        subsystem.write(references, inputs)?;
        match subsystem {
            Subsystem::Fmu(fmu) => fmu.exit(),
            Subsystem::Network(_) => Ok(()),
        }
        .map(ControlFlow::Continue)
    })?;
    let mut times = NEVec::new(start);
    let mut columns = recorded.iter().map(|reading| reading.value.get()).collect::<Vec<_>>();
    let stop = grid
        .as_ref()
        .array_windows()
        .map(|&[time, next]| {
            if cancel.load(Ordering::Relaxed) {
                return Ok::<_, NEVec<DynamicsError>>(Some(Stop::Cancelled));
            }
            let requested = pass(&ordered, &mut graph, &mut scratch, time, |subsystem, references, inputs| {
                subsystem.write(references, inputs)?;
                match subsystem {
                    Subsystem::Fmu(fmu) => fmu.step(time, next - time, cancel),
                    Subsystem::Network(instance) => instance.step(time, next).map(|()| ControlFlow::Continue(())),
                }
            })?;
            if requested.is_none() {
                times.push(next);
                columns.extend(recorded.iter().map(|reading| reading.value.get()));
            }
            Ok(requested)
        })
        .find_map(Result::transpose)
        .transpose()?;
    let reached = *times.last();
    ordered
        .iter()
        .map(|(node, _, _)| {
            match &mut graph[*node] {
                Subsystem::Fmu(fmu) => fmu.terminate(),
                Subsystem::Network(_) => Ok(()),
            }
            .map_err(failed(node.index(), reached))
        })
        .collect::<Validated<(), _>>()
        .ok()?;
    let values = DMatrix::from_vec(recorded.len(), times.len().get(), columns);
    Ok(Trajectory { times: times.into(), values, stop })
}

const fn failed(subsystem: usize, time: Time) -> impl Fn(DynamicsError) -> DynamicsError + Copy { move |source| DynamicsError::Subsystem { subsystem, time, source: Box::new(source) } }
