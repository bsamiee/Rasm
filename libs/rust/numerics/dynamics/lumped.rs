use core::{
    cell::{RefCell, RefMut},
    iter,
};
use std::{borrow::Cow, collections::HashSet};

use diffsol::{
    ConstantOp, ConstantOpSens, ConstantOpSensAdjoint, FaerContext, FaerSparseMat, FaerVec, LinearOp, LinearOpTranspose, Matrix, NonLinearOp, NonLinearOpAdjoint, NonLinearOpJacobian, NonLinearOpSens,
    NonLinearOpSensAdjoint, OdeBuilder, OdeEquations, OdeEquationsAdjoint, OdeEquationsImplicitAdjoint, OdeEquationsImplicitSens, OdeEquationsRef, OdeSolverProblem, Op, UnitCallable, Vector as _,
    VectorCommon as _,
};
use faer::{
    Accum, ColMut, ColRef, Par, Side,
    linalg::solvers::Solve as _,
    sparse::{Pair, SparseColMat, SparseColMatRef, SymbolicSparseColMat, Triplet, linalg::matmul::sparse_dense_matmul},
};
use itertools::{Either, Itertools as _, izip};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator, nev};
use petgraph::{
    algo::kosaraju_scc,
    graph::{EdgeIndex, NodeIndex, UnGraph},
    visit::{EdgeRef as _, IntoNodeReferences as _},
};
use uom::{
    ConstZero as _,
    fmt::DisplayStyle::Abbreviation,
    si::{
        f64::{HeatCapacity, Power, TemperatureInterval, ThermalConductance, ThermodynamicTemperature, Time},
        heat_capacity::joule_per_kelvin,
        power::{self, watt},
        temperature_interval,
        thermal_conductance::watt_per_kelvin,
        thermodynamic_temperature::{self, kelvin},
        time::second,
    },
};
use validated::Validated;

use crate::{
    BaseUnit, Causality, DynamicsError, Grid, Kind, Port,
    ode::{self, Method},
};

// --- [TYPES] ---------------------------------------------------------------------------

type Drive<'network, Value> = Box<dyn Fn(Time, &mut [Value]) + 'network>;

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

#[derive(Debug, Clone, PartialEq, Default)]
pub struct Options<Sensors = Option<NEVec<NodeIndex>>> {
    pub rtol: Option<f64>,
    pub atol: Option<TemperatureInterval>,
    pub sens_rtol: Option<f64>,
    pub sens_atol: Option<TemperatureInterval>,
    pub sensors: Sensors,
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Network {
    #[as_ref]
    graph: UnGraph<Node, ThermalConductance>,
    incidence: SparseColMat<usize, f64>,
    rows: Vec<Option<usize>>,
}

impl Network {
    pub fn new(graph: UnGraph<Node, ThermalConductance>) -> Result<Self, NEVec<DynamicsError>> {
        let assembled = SparseColMat::<usize, f64>::try_new_from_triplets(
            graph.edge_count(),
            graph.node_count(),
            &graph.edge_references().flat_map(|edge| [Triplet::new(edge.id().index(), edge.source().index(), 1.0), Triplet::new(edge.id().index(), edge.target().index(), -1.0)]).collect::<Vec<_>>(),
        )
        .map_err(DynamicsError::from);
        let rows = graph
            .node_weights()
            .scan(0_usize.., |states, node| {
                Some(match node {
                    Node::Capacity(_) => states.next(),
                    Node::Boundary => None,
                })
            })
            .collect::<Vec<_>>();
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
            .map2(Validated::from(assembled), |(), incidence| Self { graph, incidence, rows })
            .ok()
    }

    fn row(&self, node: NodeIndex) -> Option<usize> { self.rows[node.index()] }

    fn free<'values, Value>(&self, values: &'values [Value]) -> impl Iterator<Item = &'values Value> { iter::zip(&self.rows, values).filter(|(row, _)| row.is_some()).map(|(_, value)| value) }

    fn capacities(&self) -> impl Iterator<Item = HeatCapacity> {
        self.graph.node_weights().filter_map(|node| match node {
            Node::Capacity(capacity) => Some(*capacity),
            Node::Boundary => None,
        })
    }

    fn holding<Slot>(&self, slots: impl IntoIterator<Item = Slot>) -> impl Iterator<Item = Option<Slot>> {
        self.capacities().scan(slots.into_iter(), |pending, capacity| Some((capacity > HeatCapacity::ZERO).then(|| pending.next()).flatten()))
    }

    fn stiffness(&self, conductances: impl IntoIterator<Item = f64>) -> impl Iterator<Item = (usize, usize, f64)> {
        iter::zip(self.graph.edge_references(), conductances)
            .flat_map(|(edge, conductance)| {
                let (source, target) = (self.row(edge.source()), self.row(edge.target()));
                [(source, source, conductance), (target, target, conductance), (source, target, -conductance), (target, source, -conductance)]
            })
            .filter_map(|(row, column, value)| Some((row?, column?, value)))
    }
}

struct Scratch {
    prescribed: Vec<ThermodynamicTemperature>,
    heats: Vec<Power>,
    temperatures: Vec<f64>,
    differences: Vec<f64>,
    flux: Vec<f64>,
    inflows: Vec<f64>,
    nets: Vec<f64>,
    weights: Vec<f64>,
}

impl Scratch {
    fn new(network: &Network, prescribed: Vec<ThermodynamicTemperature>) -> Self {
        let (nodes, edges, states) = (network.graph.node_count(), network.graph.edge_count(), network.rows.iter().flatten().count());
        Self {
            prescribed,
            heats: vec![Power::ZERO; nodes],
            temperatures: vec![0.0; nodes],
            differences: vec![0.0; edges],
            flux: vec![0.0; edges],
            inflows: vec![0.0; nodes],
            nets: vec![0.0; states],
            weights: vec![0.0; states],
        }
    }

    fn conduct(&mut self, network: &Network, conductances: impl IntoIterator<Item = f64>) {
        let incidence = network.incidence.as_ref();
        difference(incidence, &self.temperatures, &mut self.differences);
        izip!(&mut self.flux, &self.differences, conductances).for_each(|(flow, change, conductance)| *flow = change * conductance);
        sparse_dense_matmul(ColMut::from_slice_mut(&mut self.inflows).as_mat_mut(), Accum::Replace, incidence.transpose(), ColRef::from_slice(&self.flux).as_mat(), -1.0, Par::Seq);
    }

    fn balance(&mut self, network: &Network, conductances: impl IntoIterator<Item = f64>, state: &[f64]) {
        spread(network, state, self.prescribed.iter().map(ThermodynamicTemperature::get::<kelvin>), &mut self.temperatures);
        self.conduct(network, conductances);
        izip!(&mut self.nets, network.free(&self.inflows), network.free(&self.heats)).for_each(|(net, inflow, heat)| *net = inflow + heat.get::<watt>());
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, thiserror::Error)]
pub enum Setting {
    #[error("initial temperature of node {} is not finite", node.index())]
    InitialTemperature { node: NodeIndex },
    #[error("{}relative tolerance {value} is negative or not finite", if *sensitivity { "sensitivity " } else { "" })]
    RelativeTolerance { sensitivity: bool, value: f64 },
    #[error("{}absolute tolerance {} is not a positive normal number", if *sensitivity { "sensitivity " } else { "" }, value.into_format_args(temperature_interval::kelvin, Abbreviation))]
    AbsoluteTolerance { sensitivity: bool, value: TemperatureInterval },
    #[error("sensor node {} lies outside a network of {nodes} nodes", sensor.index())]
    Sensor { sensor: NodeIndex, nodes: usize },
}

// --- [SERVICES] ------------------------------------------------------------------------

// --- [EQUATIONS]
struct Equations<'network> {
    network: Cow<'network, Network>,
    sensors: Vec<NodeIndex>,
    temperature: Drive<'network, ThermodynamicTemperature>,
    heat: Drive<'network, Power>,
    initial: Vec<f64>,
    parameters: Vec<f64>,
    scales: Vec<f64>,
    scratch: RefCell<Scratch>,
    context: FaerContext,
}

impl Equations<'_> {
    fn conductances(&self) -> impl Iterator<Item = f64> { self.parameters.iter().take(self.network.graph.edge_count()).copied() }

    fn drive(&self, scratch: &mut Scratch, time: f64) {
        let instant = Time::new::<second>(time);
        (self.temperature)(instant, &mut scratch.prescribed);
        (self.heat)(instant, &mut scratch.heats);
    }

    fn balanced(&self, x: &FaerVec<f64>, time: f64) -> RefMut<'_, Scratch> {
        let mut scratch = self.scratch.borrow_mut();
        self.drive(&mut scratch, time);
        scratch.balance(&self.network, self.conductances(), x.inner().col_as_slice(0));
        scratch
    }

    fn sensitivity<'scratch>(&'scratch self, scratch: &'scratch Scratch) -> impl Iterator<Item = (usize, usize, f64)> {
        let network: &'scratch Network = &self.network;
        iter::zip(network.graph.edge_references(), &scratch.differences)
            .flat_map(move |(edge, &change)| {
                [(edge.source(), -change), (edge.target(), change)].into_iter().filter_map(move |(node, value)| {
                    let row = network.row(node)?;
                    Some((row, edge.id().index(), value * self.scales[row]))
                })
            })
            .chain(izip!(0_usize.., network.holding(network.graph.edge_count()..), &scratch.nets, &self.scales).filter_map(|(row, column, net, scale)| Some((row, column?, -net * scale * scale))))
    }

    fn masses(&self) -> impl Iterator<Item = (usize, usize, f64)> { self.network.capacities().positions(|capacity| capacity > HeatCapacity::ZERO).map(|row| (row, row, 1.0)) }

    fn readings(&self) -> impl Iterator<Item = (usize, usize)> { self.sensors.iter().enumerate().filter_map(|(sensor, &node)| Some((sensor, self.network.row(node)?))) }
}

impl Op for Equations<'_> {
    type T = f64;
    type V = FaerVec<f64>;
    type M = FaerSparseMat<f64>;
    type C = FaerContext;

    fn context(&self) -> &Self::C { &self.context }

    fn nstates(&self) -> usize { self.initial.len() }

    fn nout(&self) -> usize { self.sensors.len() }

    fn nparams(&self) -> usize { self.parameters.len() }
}

impl<'equations> OdeEquationsRef<'equations> for Equations<'_> {
    type Mass = Free<'equations>;
    type Rhs = Free<'equations>;
    type Root = UnitCallable<FaerSparseMat<f64>>;
    type Init = Free<'equations>;
    type Out = &'equations Self;
    type Reset = UnitCallable<FaerSparseMat<f64>>;
}

impl OdeEquations for Equations<'_> {
    fn rhs(&self) -> <Self as OdeEquationsRef<'_>>::Rhs { Free(self) }

    fn mass(&self) -> Option<<Self as OdeEquationsRef<'_>>::Mass> { self.network.capacities().any(|capacity| capacity <= HeatCapacity::ZERO).then_some(Free(self)) }

    fn out(&self) -> Option<<Self as OdeEquationsRef<'_>>::Out> { Some(self) }

    fn init(&self) -> <Self as OdeEquationsRef<'_>>::Init { Free(self) }

    fn set_params(&mut self, p: &Self::V) {
        self.parameters.copy_from_slice(p.inner().col_as_slice(0));
        iter::zip(&mut self.scales, scales(&self.network, &self.parameters)).for_each(|(held, scale)| *held = scale);
    }

    fn get_params(&self, p: &mut Self::V) { p.for_each_batch_host([], |held, [], _| held.copy_from_slice(&self.parameters)); }
}

impl NonLinearOp for Equations<'_> {
    fn call_inplace(&self, x: &Self::V, t: Self::T, y: &mut Self::V) {
        let scratch = &mut *self.scratch.borrow_mut();
        (self.temperature)(Time::new::<second>(t), &mut scratch.prescribed);
        y.for_each_batch_host([x], |outputs, [state], _| {
            iter::zip(outputs, &self.sensors).for_each(|(output, &node)| {
                *output = match self.network.row(node) {
                    Some(row) => state[row],
                    None => scratch.prescribed[node.index()].get::<kelvin>(),
                };
            });
        });
    }
}

impl NonLinearOpJacobian for Equations<'_> {
    fn jac_mul_inplace(&self, _x: &Self::V, _t: Self::T, v: &Self::V, y: &mut Self::V) {
        y.for_each_batch_host([v], |outputs, [direction], _| {
            iter::zip(outputs, &self.sensors).for_each(|(output, &node)| *output = self.network.row(node).map_or_default(|row| direction[row]));
        });
    }

    fn jacobian_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nout(), self.nstates(), self.readings().map(|(sensor, row)| (sensor, row, 1.0)))) }
}

impl NonLinearOpSens for Equations<'_> {
    fn sens_mul_inplace(&self, _x: &Self::V, _t: Self::T, _v: &Self::V, y: &mut Self::V) { y.fill(0.0); }

    fn sens_inplace(&self, _x: &Self::V, _t: Self::T, _y: &mut Self::M) {}

    fn sens_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nout(), self.nparams(), iter::empty())) }
}

impl NonLinearOpAdjoint for Equations<'_> {
    fn jac_transpose_mul_inplace(&self, _x: &Self::V, _t: Self::T, directions: &Self::V, y: &mut Self::V) {
        y.for_each_batch_host([directions], |products, [direction], _| {
            products.fill(0.0);
            self.readings().for_each(|(sensor, row)| products[row] -= direction[sensor]);
        });
    }

    fn adjoint_inplace(&self, _x: &Self::V, _t: Self::T, y: &mut Self::M) { assign(y, self.readings().map(|(sensor, row)| (row, sensor, -1.0))); }

    fn adjoint_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nstates(), self.nout(), self.readings().map(|(sensor, row)| (row, sensor, -1.0)))) }
}

impl NonLinearOpSensAdjoint for Equations<'_> {
    fn sens_transpose_mul_inplace(&self, _x: &Self::V, _t: Self::T, _v: &Self::V, y: &mut Self::V) { y.fill(0.0); }

    fn sens_adjoint_inplace(&self, _x: &Self::V, _t: Self::T, _y: &mut Self::M) {}

    fn sens_adjoint_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nparams(), self.nout(), iter::empty())) }
}

struct Free<'equations>(&'equations Equations<'equations>);

impl Op for Free<'_> {
    type T = f64;
    type V = FaerVec<f64>;
    type M = FaerSparseMat<f64>;
    type C = FaerContext;

    fn context(&self) -> &Self::C { &self.0.context }

    fn nstates(&self) -> usize { self.0.initial.len() }

    fn nout(&self) -> usize { self.0.initial.len() }

    fn nparams(&self) -> usize { self.0.parameters.len() }
}

impl NonLinearOp for Free<'_> {
    fn call_inplace(&self, x: &Self::V, t: Self::T, y: &mut Self::V) {
        let scratch = &mut *self.0.scratch.borrow_mut();
        self.0.drive(scratch, t);
        y.for_each_batch_host([x], |rates, [state], _| {
            scratch.balance(&self.0.network, self.0.conductances(), state);
            izip!(rates, &scratch.nets, &self.0.scales).for_each(|(rate, net, scale)| *rate = net * scale);
        });
    }
}

impl NonLinearOpJacobian for Free<'_> {
    fn jac_mul_inplace(&self, _x: &Self::V, _t: Self::T, v: &Self::V, y: &mut Self::V) {
        let scratch = &mut *self.0.scratch.borrow_mut();
        y.for_each_batch_host([v], |products, [direction], _| {
            spread(&self.0.network, direction, iter::repeat(0.0), &mut scratch.temperatures);
            scratch.conduct(&self.0.network, self.0.conductances());
            izip!(products, self.0.network.free(&scratch.inflows), &self.0.scales).for_each(|(product, inflow, scale)| *product = inflow * scale);
        });
    }

    fn jacobian_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nstates(), self.nstates(), self.0.network.stiffness(self.0.conductances()))) }

    fn jacobian_inplace(&self, _x: &Self::V, _t: Self::T, y: &mut Self::M) {
        assign(y, self.0.network.stiffness(self.0.conductances()).map(|(row, column, value)| (row, column, -value * self.0.scales[row])));
    }
}

impl NonLinearOpSens for Free<'_> {
    fn sens_mul_inplace(&self, x: &Self::V, time: Self::T, directions: &Self::V, y: &mut Self::V) {
        let scratch = &mut *self.0.balanced(x, time);
        let edges = self.0.network.graph.edge_count();
        y.for_each_batch_host([directions], |products, [direction], _| {
            scratch.conduct(&self.0.network, direction.iter().take(edges).copied());
            izip!(products, self.0.network.free(&scratch.inflows), &scratch.nets, &self.0.scales, self.0.network.holding(direction.iter().skip(edges))).for_each(
                |(product, inflow, net, scale, change)| {
                    *product = scale * (net * scale).mul_add(-change.copied().unwrap_or_default(), *inflow);
                },
            );
        });
    }

    fn sens_inplace(&self, x: &Self::V, t: Self::T, y: &mut Self::M) { assign(y, self.0.sensitivity(&self.0.balanced(x, t))); }

    fn sens_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nstates(), self.nparams(), self.0.sensitivity(&self.0.scratch.borrow()))) }
}

impl NonLinearOpAdjoint for Free<'_> {
    fn jac_transpose_mul_inplace(&self, _x: &Self::V, _t: Self::T, directions: &Self::V, y: &mut Self::V) {
        let scratch = &mut *self.0.scratch.borrow_mut();
        y.for_each_batch_host([directions], |products, [direction], _| {
            izip!(&mut scratch.weights, direction, &self.0.scales).for_each(|(weight, change, scale)| *weight = change * scale);
            spread(&self.0.network, &scratch.weights, iter::repeat(0.0), &mut scratch.temperatures);
            scratch.conduct(&self.0.network, self.0.conductances());
            iter::zip(products, self.0.network.free(&scratch.inflows)).for_each(|(product, inflow)| *product = -inflow);
        });
    }

    fn adjoint_inplace(&self, _x: &Self::V, _t: Self::T, y: &mut Self::M) {
        assign(y, self.0.network.stiffness(self.0.conductances()).map(|(row, column, value)| (row, column, value * self.0.scales[column])));
    }

    fn adjoint_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { self.jacobian_sparsity() }
}

impl NonLinearOpSensAdjoint for Free<'_> {
    fn sens_transpose_mul_inplace(&self, x: &Self::V, time: Self::T, directions: &Self::V, y: &mut Self::V) {
        let scratch = &mut *self.0.balanced(x, time);
        let edges = self.0.network.graph.edge_count();
        y.for_each_batch_host([directions], |products, [direction], _| {
            izip!(&mut scratch.weights, direction, &self.0.scales).for_each(|(weight, change, scale)| *weight = change * scale);
            spread(&self.0.network, &scratch.weights, iter::repeat(0.0), &mut scratch.temperatures);
            difference(self.0.network.incidence.as_ref(), &scratch.temperatures, &mut scratch.flux);
            let mut slots = products.iter_mut();
            izip!(slots.by_ref().take(edges), &scratch.differences, &scratch.flux).for_each(|(product, change, weighted)| *product = change * weighted);
            izip!(self.0.network.holding(slots), &scratch.weights, &scratch.nets, &self.0.scales).for_each(|(slot, weight, net, scale)| {
                if let Some(product) = slot {
                    *product = weight * net * scale;
                }
            });
        });
    }

    fn sens_adjoint_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> {
        Some(pattern(self.nparams(), self.nstates(), self.0.sensitivity(&self.0.scratch.borrow()).map(|(row, column, value)| (column, row, value))))
    }
}

impl ConstantOp for Free<'_> {
    fn call_inplace(&self, _t: Self::T, y: &mut Self::V) { y.for_each_batch_host([], |state, [], _| state.copy_from_slice(&self.0.initial)); }
}

impl ConstantOpSens for Free<'_> {
    fn sens_mul_inplace(&self, _t: Self::T, _v: &Self::V, y: &mut Self::V) { y.fill(0.0); }

    fn sens_inplace(&self, _t: Self::T, _y: &mut Self::M) {}

    fn sens_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nstates(), self.nparams(), iter::empty())) }
}

impl ConstantOpSensAdjoint for Free<'_> {
    fn sens_transpose_mul_inplace(&self, _t: Self::T, _v: &Self::V, y: &mut Self::V) { y.fill(0.0); }
}

impl LinearOp for Free<'_> {
    fn gemv_inplace(&self, x: &Self::V, _t: Self::T, beta: Self::T, y: &mut Self::V) {
        y.for_each_batch_host([x], |products, [direction], _| {
            izip!(products, direction, self.0.network.capacities()).for_each(|(product, change, capacity)| {
                *product = if capacity > HeatCapacity::ZERO { beta.mul_add(*product, *change) } else { beta * *product };
            });
        });
    }

    fn matrix_inplace(&self, _t: Self::T, y: &mut Self::M) { assign(y, self.0.masses()); }

    fn sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { Some(pattern(self.nstates(), self.nstates(), self.0.masses())) }
}

impl LinearOpTranspose for Free<'_> {
    fn gemv_transpose_inplace(&self, x: &Self::V, time: Self::T, beta: Self::T, y: &mut Self::V) { LinearOp::gemv_inplace(self, x, time, beta, y); }

    fn transpose_inplace(&self, _t: Self::T, y: &mut Self::M) { assign(y, self.0.masses()); }

    fn transpose_sparsity(&self) -> Option<<Self::M as Matrix>::Sparsity> { self.sparsity() }
}

// --- [INSTANCE]
#[derive(derive_more::Debug, derive_more::AsRef)]
pub struct Instance {
    #[debug(skip)]
    problem: OdeSolverProblem<Equations<'static>>,
    method: Method,
    state: Vec<ThermodynamicTemperature>,
    #[as_ref([Port])]
    ports: Vec<Port>,
}

impl Instance {
    pub(crate) fn set(&mut self, references: &[usize], values: &[f64]) {
        let equations = &mut self.problem.eqn;
        let scratch = equations.scratch.get_mut();
        iter::zip(references, values).for_each(|(&reference, &value)| match equations.network.graph[NodeIndex::new(reference)] {
            Node::Boundary => scratch.prescribed[reference] = ThermodynamicTemperature::new::<kelvin>(value),
            Node::Capacity(_) => scratch.heats[reference] = Power::new::<watt>(value),
        });
    }

    pub(crate) fn get(&self, references: &[usize]) -> impl Iterator<Item = f64> { references.iter().map(|&reference| self.state[reference].get::<kelvin>()) }

    pub(crate) fn step(&mut self, time: Time, next: Time) -> Result<(), DynamicsError> {
        let grid = Grid::try_new(nev![next])?;
        self.problem.t0 = time.get::<second>();
        iter::zip(&mut self.problem.eqn.initial, self.problem.eqn.network.free(&self.state)).for_each(|(held, temperature)| *held = temperature.get::<kelvin>());
        let reached = ode::trajectory(&self.problem, self.method, &grid)?;
        iter::zip(&mut self.state, reached.values.iter()).for_each(|(held, &temperature)| *held = ThermodynamicTemperature::new::<kelvin>(temperature));
        Ok(())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn parameters(network: &Network) -> impl Iterator<Item = (Parameter, f64)> {
    network.graph.edge_references().map(|edge| (Parameter::Conductance(edge.id()), edge.weight().get::<watt_per_kelvin>())).chain(network.graph.node_references().filter_map(|(node, weight)| {
        match *weight {
            Node::Capacity(capacity) => (capacity > HeatCapacity::ZERO).then(|| (Parameter::Capacity(node), capacity.get::<joule_per_kelvin>())),
            Node::Boundary => None,
        }
    }))
}

pub fn steady(network: &Network, time: Time, temperature: impl Fn(Time, &mut [ThermodynamicTemperature]), heat: impl Fn(Time, &mut [Power])) -> Result<Vec<ThermodynamicTemperature>, DynamicsError> {
    floating(network, |node| matches!(node, Node::Boundary))?;
    let conductances = || network.graph.edge_weights().map(ThermalConductance::get::<watt_per_kelvin>);
    let mut scratch = Scratch::new(network, vec![ThermodynamicTemperature::default(); network.graph.node_count()]);
    temperature(time, &mut scratch.prescribed);
    heat(time, &mut scratch.heats);
    izip!(network.graph.node_indices(), network.graph.node_weights(), &scratch.prescribed, &scratch.heats)
        .filter_map(|(node, weight, prescribed, gain)| match weight {
            Node::Boundary => (!prescribed.is_finite()).then_some((node, Either::Left(*prescribed))),
            Node::Capacity(_) => (!gain.is_finite()).then_some((node, Either::Right(*gain))),
        })
        .try_into_nonempty_iter()
        .map(NonEmptyIterator::collect::<NEVec<_>>)
        .map_or(Ok(()), |values| Err(DynamicsError::Forcing { time, values }))?;
    let states = scratch.nets.len();
    scratch.balance(network, conductances(), &vec![0.0; states]);
    SparseColMat::<usize, f64>::try_new_from_triplets(states, states, &network.stiffness(conductances()).map(|(row, column, value)| Triplet::new(row, column, value)).collect::<Vec<_>>())?
        .sp_cholesky(Side::Lower)?
        .solve_in_place(ColMut::from_slice_mut(&mut scratch.nets));
    spread(network, &scratch.nets, scratch.prescribed.iter().map(ThermodynamicTemperature::get::<kelvin>), &mut scratch.temperatures);
    Ok(scratch.temperatures.into_iter().map(ThermodynamicTemperature::new::<kelvin>).collect())
}

pub fn problem<'network, Temperature, Heat>(
    network: &'network Network,
    initial: &[ThermodynamicTemperature],
    start: Time,
    temperature: Temperature,
    heat: Heat,
    options: Options,
) -> Result<
    OdeSolverProblem<
        impl OdeEquationsImplicitSens<M = FaerSparseMat<f64>, V = FaerVec<f64>, T = f64, C = FaerContext> + OdeEquationsImplicitAdjoint + OdeEquationsAdjoint + use<'network, Temperature, Heat>,
    >,
    DynamicsError,
>
where
    Temperature: Fn(Time, &mut [ThermodynamicTemperature]) + 'network,
    Heat: Fn(Time, &mut [Power]) + 'network,
{
    let options = settings(network, initial, options)?;
    transient(Cow::Borrowed(network), initial, start, Box::new(temperature), Box::new(heat), options)
}

pub fn instance(network: Network, initial: Vec<ThermodynamicTemperature>, method: Method, options: Options) -> Result<Instance, DynamicsError> {
    let options = settings(&network, &initial, options)?;
    let outputs = options.sensors.iter().copied().collect::<HashSet<_>>();
    let temperature = Kind::Real(Some(BaseUnit::of::<thermodynamic_temperature::Dimension>()));
    let ports = network
        .graph
        .node_weights()
        .enumerate()
        .map(|(reference, node)| match node {
            Node::Boundary => Port { name: format!("temperature of node {reference}"), causality: Causality::Input, reference, kind: temperature },
            Node::Capacity(_) => Port { name: format!("heat into node {reference}"), causality: Causality::Input, reference, kind: Kind::Real(Some(BaseUnit::of::<power::Dimension>())) },
        })
        .chain(network.graph.node_indices().filter(|node| outputs.contains(node)).map(|node| Port {
            name: format!("temperature of node {}", node.index()),
            causality: Causality::Output,
            reference: node.index(),
            kind: temperature,
        }))
        .collect();
    let sensors = network.graph.node_indices().collect();
    let problem = transient(Cow::Owned(network), &initial, Time::ZERO, Box::new(|_, _| ()), Box::new(|_, _| ()), Options { sensors, ..options })?;
    Ok(Instance { problem, method, state: initial, ports })
}

fn settings(network: &Network, initial: &[ThermodynamicTemperature], options: Options) -> Result<Options<Vec<NodeIndex>>, DynamicsError> {
    let Options { rtol, atol, sens_rtol, sens_atol, sensors: _ } = options;
    let (nodes, found) = (network.graph.node_count(), initial.len());
    (found == nodes).ok_or(DynamicsError::Initial { nodes, found })?;
    let selected = match options.sensors {
        None => Validated::Good(network.graph.node_indices().collect()),
        Some(sensors) => sensors.iter().copied().filter(|sensor| sensor.index() >= nodes).map(|sensor| Err(Setting::Sensor { sensor, nodes })).collect::<Validated<(), _>>().map(|()| sensors.into()),
    };
    iter::chain(
        initial.iter().positions(|start_temperature| !start_temperature.is_finite()).map(|node| Setting::InitialTemperature { node: NodeIndex::new(node) }),
        [(false, rtol), (true, sens_rtol)]
            .into_iter()
            .filter_map(|(sensitivity, tolerance)| tolerance.filter(|value| !(value.is_finite() && *value >= 0.0)).map(|value| Setting::RelativeTolerance { sensitivity, value })),
    )
    .chain(
        [(false, atol), (true, sens_atol)]
            .into_iter()
            .filter_map(|(sensitivity, tolerance)| tolerance.filter(|value| !(value.is_normal() && value.is_sign_positive())).map(|value| Setting::AbsoluteTolerance { sensitivity, value })),
    )
    .map(Err)
    .collect::<Validated<(), _>>()
    .map2(selected, |(), sensors| Options { rtol, atol, sens_rtol, sens_atol, sensors })
    .ok()
    .map_err(|settings| DynamicsError::Settings { settings })
}

fn transient<'network>(
    network: Cow<'network, Network>,
    initial: &[ThermodynamicTemperature],
    start: Time,
    temperature: Drive<'network, ThermodynamicTemperature>,
    heat: Drive<'network, Power>,
    options: Options<Vec<NodeIndex>>,
) -> Result<OdeSolverProblem<Equations<'network>>, DynamicsError> {
    let Options { rtol, atol, sens_rtol, sens_atol, sensors } = options;
    floating(&network, |node| match node {
        Node::Capacity(capacity) => *capacity > HeatCapacity::ZERO,
        Node::Boundary => true,
    })?;
    let state = network.free(initial).map(ThermodynamicTemperature::get::<kelvin>).collect::<Vec<_>>();
    (!state.is_empty()).ok_or(DynamicsError::State { nodes: network.graph.node_count() })?;
    let parameters = parameters(&network).map(|(_, value)| value).collect::<Vec<_>>();
    let (scales, scratch) = (scales(&network, &parameters).collect(), RefCell::new(Scratch::new(&network, initial.to_vec())));
    let interval = |tolerance: TemperatureInterval| [tolerance.get::<temperature_interval::kelvin>()];
    let massful = network.capacities().map(|capacity| capacity > HeatCapacity::ZERO).collect::<Vec<_>>();
    let builder = atol.map(interval).into_iter().fold(rtol.into_iter().fold(OdeBuilder::<FaerSparseMat<f64>>::new(), OdeBuilder::rtol), OdeBuilder::atol);
    sens_atol
        .map(interval)
        .into_iter()
        .fold(sens_rtol.into_iter().fold(builder, OdeBuilder::sens_rtol), OdeBuilder::sens_atol)
        .t0(start.get::<second>())
        .p(parameters.iter().copied())
        .param_scales(parameters.iter().copied())
        .build_from_eqn(Equations { network, sensors, temperature, heat, initial: state, parameters, scales, scratch, context: FaerContext::default() })
        .map(|mut problem| {
            problem.atol = FaerVec::from_vec(iter::zip(problem.atol.clone_as_vec(), massful).map(|(tolerance, held)| if held { tolerance } else { f64::INFINITY }).collect(), *problem.context());
            problem
        })
        .map_err(DynamicsError::from)
}

fn scales(network: &Network, parameters: &[f64]) -> impl Iterator<Item = f64> {
    network.holding(parameters.iter().skip(network.graph.edge_count())).map(|capacity| capacity.map_or(1.0, |held| held.recip()))
}

fn floating(network: &Network, anchored: impl Fn(&Node) -> bool) -> Result<(), DynamicsError> {
    kosaraju_scc(&network.graph)
        .into_iter()
        .filter(|component| !component.iter().any(|&node| anchored(&network.graph[node])))
        .try_into_nonempty_iter()
        .map(NonEmptyIterator::collect::<NEVec<_>>)
        .map_or(Ok(()), |components| Err(DynamicsError::Floating { components }))
}

fn spread(network: &Network, state: &[f64], boundaries: impl IntoIterator<Item = f64>, temperatures: &mut [f64]) {
    izip!(temperatures, &network.rows, boundaries).for_each(|(temperature, row, boundary)| *temperature = row.map_or(boundary, |index| state[index]));
}

fn difference(incidence: SparseColMatRef<'_, usize, f64>, temperatures: &[f64], differences: &mut [f64]) {
    sparse_dense_matmul(ColMut::from_slice_mut(differences).as_mat_mut(), Accum::Replace, incidence, ColRef::from_slice(temperatures).as_mat(), 1.0, Par::Seq);
}

fn pattern(nrows: usize, ncols: usize, entries: impl IntoIterator<Item = (usize, usize, f64)>) -> SymbolicSparseColMat<usize> {
    #[expect(clippy::expect_used, reason = "Diffsol sparsity getters cannot propagate Faer's native construction failure")]
    SymbolicSparseColMat::try_new_from_indices(nrows, ncols, &entries.into_iter().map(|(row, column, _)| Pair::new(row, column)).collect::<Vec<_>>())
        .map(|(symbolic, _)| symbolic)
        .expect("Faer symbolic sparsity construction failed")
}

fn assign(matrix: &mut FaerSparseMat<f64>, entries: impl IntoIterator<Item = (usize, usize, f64)>) {
    matrix.inner_mut().iter_mut().for_each(|batch| batch.val_mut().fill(0.0));
    entries.into_iter().for_each(|(row, column, value)| matrix.inner_mut().iter_mut().for_each(|batch| batch[(row, column)] += value));
}
