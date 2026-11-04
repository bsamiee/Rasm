use core::{array, error::Error};

use approx::assert_abs_diff_eq;
use diffsol::{FaerContext, FaerMat, FaerVec, OdeBuilder, OdeEquations as _, Op as _, Vector as _};
use nalgebra::{DMatrix, dmatrix};
use nonempty_collections::{NEVec, nev};
use petgraph::graph::UnGraph;
use rasm_dynamics::{
    DynamicsError, Grid,
    lumped::{Network, Node, Options, Parameter, parameters, problem},
    ode::{Method, gradient, sensitivities},
};
use uom::si::{
    f64::{HeatCapacity, Power, TemperatureInterval, ThermalConductance, ThermodynamicTemperature, Time},
    heat_capacity::joule_per_kelvin,
    power::watt,
    temperature_interval,
    thermal_conductance::watt_per_kelvin,
    thermodynamic_temperature::kelvin,
    time,
};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn thermal_decay_preserves_parallel_conductances_sensor_order_and_parameter_updates() -> Result<(), NEVec<DynamicsError>> {
    let mut graph = UnGraph::new_undirected();
    let boundary = graph.add_node(Node::Boundary);
    let first = graph.add_node(Node::Capacity(HeatCapacity::new::<joule_per_kelvin>(2.0)));
    let second = graph.add_node(Node::Capacity(HeatCapacity::new::<joule_per_kelvin>(5.0)));
    let parallel = [graph.add_edge(boundary, first, ThermalConductance::new::<watt_per_kelvin>(1.0)), graph.add_edge(boundary, first, ThermalConductance::new::<watt_per_kelvin>(2.0))];
    let separate = graph.add_edge(boundary, second, ThermalConductance::new::<watt_per_kelvin>(4.0));
    let network = Network::new(graph)?;
    let sensors = [second, boundary, first, second];
    let instants = [0.0, 0.2, 0.7, 1.5];
    let grid = Grid::try_new(nev![Time::new::<time::second>(instants[0]), Time::new::<time::second>(instants[1]), Time::new::<time::second>(instants[2]), Time::new::<time::second>(instants[3])])
        .map_err(NEVec::new)?;
    let mut problem = problem(
        &network,
        &[300.0, 304.0, 298.0].map(ThermodynamicTemperature::new::<kelvin>),
        Time::new::<time::second>(0.0),
        |_, temperatures| temperatures.fill(ThermodynamicTemperature::new::<kelvin>(300.0)),
        |_, heat| heat.fill(Power::new::<watt>(0.0)),
        Options {
            rtol: Some(1.0e-10),
            atol: Some(TemperatureInterval::new::<temperature_interval::kelvin>(1.0e-10)),
            sens_rtol: Some(1.0e-9),
            sens_atol: Some(TemperatureInterval::new::<temperature_interval::kelvin>(1.0e-10)),
            sensors: Some(nev![second, boundary, first, second]),
        },
    )
    .map_err(NEVec::new)?;
    [[1.0, 2.0, 4.0, 2.0, 5.0], [3.0, 1.0, 2.0, 4.0, 3.0]].into_iter().try_for_each(|values| {
        problem.eqn.set_params(&FaerVec::from_vec(values.to_vec(), FaerContext::default()));
        let (trajectory, derivatives) = sensitivities(&problem, Method::Bdf, &grid).map_err(NEVec::new)?;
        let temperatures: &DMatrix<f64> = trajectory.as_ref();
        let decay = |node, time: f64| {
            let (conductance, capacity, offset) = if node == first { (values[0] + values[1], values[3], 4.0) } else { (values[2], values[4], -2.0) };
            (offset * (-conductance * time / capacity).exp(), conductance, capacity)
        };
        assert_eq!(temperatures.shape(), (sensors.len(), instants.len()));
        assert_abs_diff_eq!(
            temperatures.as_slice(),
            DMatrix::from_fn(sensors.len(), instants.len(), |row, column| if sensors[row] == boundary { 300.0 } else { 300.0 + decay(sensors[row], instants[column]).0 }).as_slice(),
            epsilon = 2.0e-7
        );
        assert_eq!(derivatives.len(), values.len());
        parameters(&network).zip(&derivatives).for_each(|((parameter, _), derivative)| {
            let expected = DMatrix::from_fn(sensors.len(), instants.len(), |row, column| {
                let (node, time) = (sensors[row], instants[column]);
                let (offset, conductance, capacity) = decay(node, time);
                match parameter {
                    Parameter::Conductance(edge) if (node == first && parallel.contains(&edge)) || (node == second && edge == separate) => -time * offset / capacity,
                    Parameter::Capacity(held) if held == node => time * conductance * offset / capacity.powi(2),
                    Parameter::Conductance(_) | Parameter::Capacity(_) => 0.0,
                }
            });
            assert_eq!(derivative.shape(), expected.shape());
            assert_abs_diff_eq!(derivative.as_slice(), expected.as_slice(), epsilon = 2.0e-7);
        });
        Ok(())
    })
}

#[test]
fn coupled_thermal_modes_preserve_parallel_edge_adjoint_gradients() -> Result<(), NEVec<DynamicsError>> {
    let mut graph = UnGraph::new_undirected();
    let boundary = graph.add_node(Node::Boundary);
    let [first, second] = array::from_fn(|_| graph.add_node(Node::Capacity(HeatCapacity::new::<joule_per_kelvin>(2.0))));
    for node in [first, second] {
        graph.add_edge(boundary, node, ThermalConductance::new::<watt_per_kelvin>(3.0));
    }
    let parallel = [1.0, 2.0].map(|conductance| graph.add_edge(first, second, ThermalConductance::new::<watt_per_kelvin>(conductance)));
    let network = Network::new(graph)?;
    let instants = [0.2, 0.7, 1.5];
    let grid = Grid::try_new(nev![Time::new::<time::second>(instants[0]), Time::new::<time::second>(instants[1]), Time::new::<time::second>(instants[2])]).map_err(NEVec::new)?;
    let mut problem = problem(
        &network,
        &[300.0, 304.0, 298.0].map(ThermodynamicTemperature::new::<kelvin>),
        Time::new::<time::second>(0.0),
        |_, temperatures| temperatures.fill(ThermodynamicTemperature::new::<kelvin>(300.0)),
        |_, heat| heat.fill(Power::new::<watt>(0.0)),
        Options {
            rtol: Some(1.0e-10),
            atol: Some(TemperatureInterval::new::<temperature_interval::kelvin>(1.0e-10)),
            sens_rtol: Some(1.0e-9),
            sens_atol: Some(TemperatureInterval::new::<temperature_interval::kelvin>(1.0e-10)),
            sensors: Some(nev![second, first]),
        },
    )
    .map_err(NEVec::new)?;
    let gradient_tolerance = 1.0e-10;
    problem.param_rtol = Some(gradient_tolerance);
    problem.param_atol = Some(FaerVec::from_element(problem.eqn.nparams(), gradient_tolerance, *problem.context()));
    let cotangent = dmatrix![2.0, -1.0, 3.0; -4.0, 0.5, 1.0];
    let (trajectory, derivative) = gradient(&problem, Method::Bdf, &grid, None, |_| vec![cotangent.clone()]).map_err(NEVec::new)?;
    let temperatures: &DMatrix<f64> = trajectory.as_ref();
    assert_eq!(temperatures.shape(), (2, instants.len()));
    assert_abs_diff_eq!(
        temperatures.as_slice(),
        DMatrix::from_fn(2, instants.len(), |row, column| {
            let time = instants[column];
            f64::mul_add(if row == 0 { -3.0 } else { 3.0 }, (-4.5_f64 * time).exp(), 300.0 + (-1.5_f64 * time).exp())
        })
        .as_slice(),
        epsilon = 2.0e-7
    );
    assert_eq!(derivative.shape(), (parameters(&network).count(), 1));
    let expected = instants.iter().enumerate().map(|(column, time)| 3.0 * time * (-4.5 * time).exp() * (cotangent[(0, column)] - cotangent[(1, column)])).sum::<f64>();
    parameters(&network).enumerate().for_each(|(row, (parameter, _))| {
        if let Parameter::Conductance(edge) = parameter
            && parallel.contains(&edge)
        {
            assert_abs_diff_eq!(derivative[(row, 0)], expected, epsilon = 2.0e-6);
        }
    });
    Ok(())
}

#[test]
fn batched_affine_adjoint_preserves_asymmetric_cotangent_orientation() -> Result<(), Box<dyn Error>> {
    let parameters = [[0.5, -0.25], [1.5, 2.0]];
    let problem = OdeBuilder::<FaerMat<f64>>::new()
        .context(FaerContext::with_nbatch(parameters.len()))
        .p(parameters.into_iter().flatten())
        .rtol(1.0e-10)
        .atol([1.0e-10])
        .sens_rtol(1.0e-10)
        .sens_atol([1.0e-10])
        .param_rtol(1.0e-10)
        .param_atol([1.0e-10])
        .rhs_adjoint_implicit(
            |_, coefficients, _, y| y.copy_from_slice(&[coefficients[0], 2.0_f64.mul_add(coefficients[0], coefficients[1])]),
            |_, _, _, _, y| y.fill(0.0),
            |_, _, _, _, y| y.fill(0.0),
            |_, _, _, direction, y| y.copy_from_slice(&[(-2.0_f64).mul_add(direction[1], -direction[0]), -direction[1]]),
        )
        .init_adjoint(|_, _, y| y.copy_from_slice(&[1.0, 3.0]), |_, _, _, y| y.fill(0.0), 2)
        .out_adjoint_implicit(
            |x, _, _, y| y.copy_from_slice(&[2.0_f64.mul_add(x[1], x[0]), 3.0_f64.mul_add(x[0], -x[1])]),
            |_, _, _, direction, y| y.copy_from_slice(&[2.0_f64.mul_add(direction[1], direction[0]), 3.0_f64.mul_add(direction[0], -direction[1])]),
            |_, _, _, direction, y| y.copy_from_slice(&[(-3.0_f64).mul_add(direction[1], -direction[0]), (-2.0_f64).mul_add(direction[0], direction[1])]),
            |_, _, _, _, y| y.fill(0.0),
            2,
        )
        .build()?;
    let instants = [0.25, 0.75, 1.25];
    let grid = Grid::try_new(nev![Time::new::<time::second>(instants[0]), Time::new::<time::second>(instants[1]), Time::new::<time::second>(instants[2])])?;
    let cotangents = [dmatrix![1.0, -2.0, 3.0; 4.0, 0.5, -1.0; -3.0, 2.0, 5.0; 0.25, -4.0, 2.0], dmatrix![-2.0, 1.0, 0.5; 3.0, -1.0, 4.0; 2.0, 7.0, -3.0; -5.0, 2.0, 1.0]];
    let (trajectory, derivative) = gradient(&problem, Method::Bdf, &grid, None, |_| cotangents.to_vec())?;
    let outputs: &DMatrix<f64> = trajectory.as_ref();
    assert_eq!(outputs.shape(), (4, instants.len()));
    assert_abs_diff_eq!(
        outputs.as_slice(),
        DMatrix::from_fn(4, instants.len(), |row, column| {
            let [first, second] = parameters[row / 2];
            let time = instants[column];
            if row.is_multiple_of(2) { 2.0_f64.mul_add(second, 5.0 * first).mul_add(time, 7.0) } else { (first - second) * time }
        })
        .as_slice(),
        epsilon = 1.0e-8
    );
    assert_eq!(derivative.shape(), (2, parameters.len() * cotangents.len()));
    assert_abs_diff_eq!(
        derivative.as_slice(),
        DMatrix::from_fn(2, parameters.len() * cotangents.len(), |parameter, column| {
            let (batch, lane) = (column / cotangents.len(), column % cotangents.len());
            let [first, second] = if parameter == 0 { [5.0, 1.0] } else { [2.0, -1.0] };
            instants.iter().enumerate().map(|(instant, time)| time * f64::mul_add(second, cotangents[lane][(2 * batch + 1, instant)], first * cotangents[lane][(2 * batch, instant)])).sum()
        })
        .as_slice(),
        epsilon = 1.0e-7
    );
    Ok(())
}
