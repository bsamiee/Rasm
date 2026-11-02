use core::{cell::RefCell, iter};
use std::rc::Rc;

use diffsol::{DiffsolError, FaerContext, FaerSparseMat, FaerVec, LaError, OdeBuilder, OdeEquationsImplicitSens, OdeSolverProblem};
use faer::{
    Accum, ColMut, ColRef, Par, Side,
    linalg::solvers::Solve as _,
    sparse::{CreationError, SparseColMat, SparseColMatRef, Triplet, linalg::matmul::sparse_dense_matmul},
};
use itertools::izip;
use nonempty_collections::NEVec;
use petgraph::{
    algo::kosaraju_scc,
    graph::{EdgeIndex, NodeIndex, UnGraph},
    visit::{EdgeRef as _, IntoNodeReferences as _},
};
use uom::{
    ConstZero as _,
    si::{
        f64::{HeatCapacity, Power, ThermalConductance, ThermodynamicTemperature, Time},
        heat_capacity::joule_per_kelvin,
        power::{self, watt},
        thermal_conductance::watt_per_kelvin,
        thermodynamic_temperature::{self, kelvin},
        time::second,
    },
};
use validated::Validated;

use crate::{
    BaseUnit, Causality, DynamicsError, Grid, Port,
    ode::{self, Method},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Node {
    Capacity(HeatCapacity),
    Boundary,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Parameter {
    Conductance(EdgeIndex),
    Capacity(NodeIndex),
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Network {
    #[as_ref]
    graph: UnGraph<Node, ThermalConductance>,
    incidence: SparseColMat<usize, f64>,
}

impl Network {
    pub fn new(graph: UnGraph<Node, ThermalConductance>) -> Result<Self, NEVec<DynamicsError>> {
        let assembled = SparseColMat::try_new_from_triplets(
            graph.edge_count(),
            graph.node_count(),
            &graph.edge_references().flat_map(|edge| [Triplet::new(edge.id().index(), edge.source().index(), 1.0), Triplet::new(edge.id().index(), edge.target().index(), -1.0)]).collect::<Vec<_>>(),
        )
        .map_err(assembly);
        graph
            .node_references()
            .filter_map(|(node, weight)| match *weight {
                Node::Capacity(capacity) => (!capacity.is_finite() || capacity < HeatCapacity::ZERO).then_some(DynamicsError::Capacity { node, capacity }),
                Node::Boundary => None,
            })
            .chain(graph.edge_references().filter_map(|edge| {
                let conductance = *edge.weight();
                (!(conductance.is_normal() && conductance.is_sign_positive())).then_some(DynamicsError::Conductance { edge: edge.id(), conductance })
            }))
            .map(Err)
            .collect::<Validated<(), _>>()
            .map2(Validated::from(assembled), |(), incidence| Self { graph, incidence })
            .ok()
    }
}

enum Row<Slot> {
    Capacity(HeatCapacity, Slot),
    Massless,
    Boundary,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, derive_more::AsRef)]
pub struct Instance {
    network: Network,
    method: Method,
    state: Vec<ThermodynamicTemperature>,
    temperatures: Vec<ThermodynamicTemperature>,
    heats: Vec<Power>,
    #[as_ref([Port])]
    ports: Vec<Port>,
}

impl Instance {
    pub(crate) fn set(&mut self, references: &[u32], values: &[f64]) {
        iter::zip(references, values).for_each(|(&reference, &value)| {
            let node = NodeIndex::<u32>::from(reference);
            if let Some(weight) = self.network.graph.node_weight(node)
                && let (Some(held), Some(gain)) = (self.temperatures.get_mut(node.index()), self.heats.get_mut(node.index()))
            {
                match weight {
                    Node::Boundary => *held = ThermodynamicTemperature::new::<kelvin>(value),
                    Node::Capacity(_) => *gain = Power::new::<watt>(value),
                }
            }
        });
    }

    pub(crate) fn get(&self, references: &[u32]) -> Vec<f64> {
        references.iter().filter_map(|&reference| self.state.get(NodeIndex::<u32>::from(reference).index())).map(ThermodynamicTemperature::get::<kelvin>).collect()
    }

    pub(crate) fn step(&mut self, time: Time, next: Time) -> Result<(), DynamicsError> {
        let grid = Grid::try_new(vec![time, next])?;
        let trajectory =
            ode::trajectory(&problem(&self.network, &self.state, time, |_, held| held.copy_from_slice(&self.temperatures), |_, gains| gains.copy_from_slice(&self.heats))?, self.method, &grid)?;
        if let Some(reached) = trajectory.values.column_iter().next_back() {
            iter::zip(&mut self.state, reached).for_each(|(state, &value)| *state = ThermodynamicTemperature::new::<kelvin>(value));
        }
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn parameters(network: &Network) -> impl Iterator<Item = Parameter> { nominal(network).map(|(parameter, _)| parameter) }

pub fn steady(network: &Network, time: Time, temperature: impl Fn(Time, &mut [ThermodynamicTemperature]), heat: impl Fn(Time, &mut [Power])) -> Result<Vec<ThermodynamicTemperature>, DynamicsError> {
    let (graph, nodes) = (&network.graph, network.graph.node_count());
    let free = |node: NodeIndex| graph.node_weight(node) != Some(&Node::Boundary);
    NEVec::try_from_vec(kosaraju_scc(graph).into_iter().filter(|component| component.iter().all(|&node| free(node))).collect())
        .map_or(Ok(()), |components| Err(DynamicsError::Floating { components }))?;
    let (mut prescribed, mut gains) = (vec![ThermodynamicTemperature::default(); nodes], vec![Power::ZERO; nodes]);
    temperature(time, &mut prescribed);
    heat(time, &mut gains);
    let anchors = iter::zip(graph.node_weights(), &prescribed)
        .map(|(node, boundary)| match node {
            Node::Boundary => boundary.get::<kelvin>(),
            Node::Capacity(_) => 0.0,
        })
        .collect::<Vec<_>>();
    let (mut flux, mut inflows) = (vec![0.0; graph.edge_count()], vec![0.0; nodes]);
    conduction(network.incidence.as_ref(), graph.edge_weights().map(ThermalConductance::get::<watt_per_kelvin>), &anchors, &mut flux, &mut inflows);
    let mut solution = izip!(graph.node_weights(), &anchors, &inflows, &gains)
        .map(|(node, &anchor, &inflow, gain)| match node {
            Node::Boundary => anchor,
            Node::Capacity(_) => inflow + gain.get::<watt>(),
        })
        .collect::<Vec<_>>();
    let triplets = graph
        .edge_references()
        .flat_map(|edge| {
            let (source, target, conductance) = (edge.source(), edge.target(), edge.weight().get::<watt_per_kelvin>());
            let coupled = (free(source) && free(target)).then_some(-conductance);
            [(source, source, free(source).then_some(conductance)), (target, target, free(target).then_some(conductance)), (source, target, coupled), (target, source, coupled)]
        })
        .chain(graph.node_indices().filter(|&node| !free(node)).map(|node| (node, node, Some(1.0))))
        .filter_map(|(row, column, value)| value.map(|entry| Triplet::new(row.index(), column.index(), entry)))
        .collect::<Vec<_>>();
    SparseColMat::<usize, f64>::try_new_from_triplets(nodes, nodes, &triplets).map_err(assembly)?.sp_cholesky(Side::Lower)?.solve_in_place(ColMut::from_slice_mut(&mut solution));
    Ok(solution.into_iter().map(ThermodynamicTemperature::new::<kelvin>).collect())
}

pub fn problem<'network, Temperature, Heat>(
    network: &'network Network,
    initial: &[ThermodynamicTemperature],
    start: Time,
    temperature: Temperature,
    heat: Heat,
) -> Result<OdeSolverProblem<impl OdeEquationsImplicitSens<M = FaerSparseMat<f64>, V = FaerVec<f64>, T = f64, C = FaerContext> + use<'network, Temperature, Heat>>, DynamicsError>
where
    Temperature: Fn(Time, &mut [ThermodynamicTemperature]),
    Heat: Fn(Time, &mut [Power]),
{
    per_node(network, initial)?;
    let (nodes, edges, incidence) = (network.graph.node_count(), network.graph.edge_count(), network.incidence.as_ref());
    let initial_state = initial.iter().map(ThermodynamicTemperature::get::<kelvin>).collect::<Vec<_>>();
    let heat = Rc::new(heat);
    let sensed = Rc::clone(&heat);
    let drive = RefCell::new((vec![0.0; edges], vec![0.0; nodes], initial.to_vec(), vec![Power::ZERO; nodes]));
    let rhs = move |temperatures: &[f64], parameters: &[f64], time: f64, rates: &mut [f64]| {
        let (flux, inflows, boundary, gains) = &mut *drive.borrow_mut();
        let instant = Time::new::<second>(time);
        temperature(instant, boundary);
        (*heat)(instant, gains);
        conduction(incidence, parameters.iter().take(edges).copied(), temperatures, flux, inflows);
        izip!(rates, rows(network, parameters.iter().skip(edges).copied()), &*inflows, &*boundary, &*gains, temperatures).for_each(|(rate, row, &inflow, prescribed, gain, &current)| {
            *rate = match row {
                Row::Capacity(_, capacity) => (inflow + gain.get::<watt>()) / capacity,
                Row::Massless => inflow + gain.get::<watt>(),
                Row::Boundary => prescribed.get::<kelvin>() - current,
            };
        });
    };
    let jacobian = RefCell::new((vec![0.0; edges], vec![0.0; nodes]));
    let jac = move |_: &[f64], parameters: &[f64], _: f64, direction: &[f64], product: &mut [f64]| {
        let (flux, inflows) = &mut *jacobian.borrow_mut();
        conduction(incidence, parameters.iter().take(edges).copied(), direction, flux, inflows);
        izip!(product, rows(network, parameters.iter().skip(edges).copied()), &*inflows, direction).for_each(|(entry, row, &inflow, &change)| {
            *entry = match row {
                Row::Capacity(_, capacity) => inflow / capacity,
                Row::Massless => inflow,
                Row::Boundary => -change,
            };
        });
    };
    let sensitivity = RefCell::new((vec![0.0; edges], vec![0.0; nodes], vec![0.0; nodes], vec![Power::ZERO; nodes]));
    let sens = move |temperatures: &[f64], parameters: &[f64], time: f64, direction: &[f64], product: &mut [f64]| {
        let (flux, inflows, response, gains) = &mut *sensitivity.borrow_mut();
        (*sensed)(Time::new::<second>(time), gains);
        conduction(incidence, parameters.iter().take(edges).copied(), temperatures, flux, inflows);
        conduction(incidence, direction.iter().take(edges).copied(), temperatures, flux, response);
        izip!(product, rows(network, iter::zip(parameters.iter().skip(edges), direction.iter().skip(edges))), &*inflows, &*response, &*gains).for_each(|(entry, row, &inflow, &change, gain)| {
            *entry = match row {
                Row::Capacity(_, (&capacity, &scale)) => ((inflow + gain.get::<watt>()) / capacity).mul_add(-scale, change) / capacity,
                Row::Massless => change,
                Row::Boundary => 0.0,
            };
        });
    };
    let mass = move |direction: &[f64], _: &[f64], _: f64, beta: f64, product: &mut [f64]| {
        izip!(product, rows(network, iter::repeat(())), direction).for_each(|(entry, row, &change)| {
            *entry = match row {
                Row::Capacity(_, ()) => beta.mul_add(*entry, change),
                Row::Massless | Row::Boundary => beta * *entry,
            };
        });
    };
    OdeBuilder::<FaerSparseMat<f64>>::new()
        .t0(start.get::<second>())
        .p(nominal(network).map(|(_, value)| value))
        .rhs_sens_implicit(rhs, jac, sens)
        .mass(mass)
        .init_sens(move |_: &[f64], _: f64, state: &mut [f64]| state.copy_from_slice(&initial_state), |_: &[f64], _: f64, _: &[f64], state: &mut [f64]| state.fill(0.0), nodes)
        .build()
        .map_err(DynamicsError::from)
}

pub fn instance(network: Network, initial: Vec<ThermodynamicTemperature>, method: Method) -> Result<Instance, DynamicsError> {
    per_node(&network, &initial)?;
    let ports = iter::zip(0_u32.., network.graph.node_weights())
        .flat_map(|(reference, node)| {
            let temperature = Port { name: format!("temperature of node {reference}"), causality: Causality::Output, reference, unit: Some(BaseUnit::of::<thermodynamic_temperature::Dimension>()) };
            let input = match node {
                Node::Boundary => Port { causality: Causality::Input, ..temperature.clone() },
                Node::Capacity(_) => Port { name: format!("heat into node {reference}"), causality: Causality::Input, reference, unit: Some(BaseUnit::of::<power::Dimension>()) },
            };
            [input, temperature]
        })
        .collect();
    let heats = vec![Power::ZERO; initial.len()];
    Ok(Instance { network, method, state: initial.clone(), temperatures: initial, heats, ports })
}

fn nominal(network: &Network) -> impl Iterator<Item = (Parameter, f64)> {
    network.graph.edge_references().map(|edge| (Parameter::Conductance(edge.id()), edge.weight().get::<watt_per_kelvin>())).chain(
        iter::zip(network.graph.node_indices(), rows(network, iter::repeat(()))).filter_map(|(node, row)| match row {
            Row::Capacity(capacity, ()) => Some((Parameter::Capacity(node), capacity.get::<joule_per_kelvin>())),
            Row::Massless | Row::Boundary => None,
        }),
    )
}

fn rows<Slot>(network: &Network, slots: impl IntoIterator<Item = Slot>) -> impl Iterator<Item = Row<Slot>> {
    network.graph.node_weights().scan(slots.into_iter(), |pending, node| match *node {
        Node::Capacity(capacity) if capacity > HeatCapacity::ZERO => pending.next().map(|value| Row::Capacity(capacity, value)),
        Node::Capacity(_) => Some(Row::Massless),
        Node::Boundary => Some(Row::Boundary),
    })
}

fn conduction(incidence: SparseColMatRef<'_, usize, f64>, conductances: impl IntoIterator<Item = f64>, temperatures: &[f64], flux: &mut [f64], inflows: &mut [f64]) {
    sparse_dense_matmul(ColMut::from_slice_mut(flux).as_mat_mut(), Accum::Replace, incidence, ColRef::from_slice(temperatures).as_mat(), 1.0, Par::Seq);
    iter::zip(&mut *flux, conductances).for_each(|(flow, conductance)| *flow *= conductance);
    sparse_dense_matmul(ColMut::from_slice_mut(inflows).as_mat_mut(), Accum::Replace, incidence.transpose(), ColRef::from_slice(flux).as_mat(), -1.0, Par::Seq);
}

fn per_node(network: &Network, initial: &[ThermodynamicTemperature]) -> Result<(), DynamicsError> {
    let (nodes, found) = (network.graph.node_count(), initial.len());
    (found == nodes).ok_or(DynamicsError::Initial { nodes, found })
}

fn assembly(source: CreationError) -> DynamicsError { DynamicsError::Solver(DiffsolError::LaError(LaError::MatrixError(source.into()))) }
