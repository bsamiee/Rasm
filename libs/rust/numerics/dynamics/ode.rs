use core::iter;

use diffsol::{
    AdjointOdeSolverMethod, CheckpointingPath, Context as _, DefaultDenseMatrix, DefaultSolver, DenseMatrix, DiffsolError, OdeEquations, OdeEquationsAdjoint, OdeEquationsImplicit,
    OdeEquationsImplicitAdjoint, OdeEquationsImplicitSens, OdeSolverMethod, OdeSolverProblem, OdeSolverState, OdeSolverStopReason, SensitivitiesOdeSolverMethod, Vector, VectorRef, VectorView as _,
    VectorViewMut as _, matrix::MatrixRef,
};
use itertools::Itertools as _;
use nalgebra::{DMatrix, DMatrixView};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use uom::{
    fmt::DisplayStyle::Abbreviation,
    si::{f64::Time, time::second},
};

use crate::{DynamicsError, Grid, Stop, Trajectory};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum Method {
    #[default]
    Bdf,
    TrBdf2,
    Esdirk34,
    Tsit45,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn t_eval(instants: &[Time]) -> Vec<f64> { instants.iter().map(Time::get::<second>).collect() }

fn reached(instants: &[Time], reason: OdeSolverStopReason<f64>) -> (&[Time], Option<(Time, usize)>) {
    match reason {
        OdeSolverStopReason::RootFound(t_root, function) => {
            let instant = Time::new::<second>(t_root);
            (instants.split_once(|later| *later > instant).map_or(instants, |(written, _)| written), Some((instant, function)))
        }
        OdeSolverStopReason::InternalTimestep | OdeSolverStopReason::TstopReached => (instants, None),
    }
}

fn recorded(instants: &[Time], reason: OdeSolverStopReason<f64>) -> (Vec<usize>, Vec<Time>, Option<Stop>) {
    let (written, root) = reached(instants, reason);
    let appended = instants.get(written.len()).and(root).map(|(instant, _)| (written.len(), instant));
    let (columns, times) = written.iter().copied().enumerate().filter(|&(_, earlier)| appended.is_none_or(|(_, instant)| earlier < instant)).chain(appended).unzip();
    (columns, times, root.map(|(_, function)| Stop::Root { function }))
}

fn selected<M>(matrix: &M, columns: &[usize]) -> DMatrix<f64>
where M: DenseMatrix<T = f64> {
    DMatrix::from_iterator(matrix.nrows().strict_mul(matrix.context().nbatch()), columns.len(), columns.iter().flat_map(|&column| matrix.column(column).into_owned().clone_as_vec()))
}

fn finite<'matrix>(times: &[Time], matrices: impl IntoIterator<Item = &'matrix DMatrix<f64>>) -> Result<(), DiffsolError> {
    let diverged = matrices.into_iter().filter_map(|matrix| matrix.column_iter().position(|column| column.iter().any(|entry| !entry.is_finite()))).min().and_then(|column| times.get(column));
    diverged.map_or(Ok(()), |instant| Err(DiffsolError::Other(format!("solved values leave the finite range by {}", instant.into_format_args(second, Abbreviation)))))
}

pub fn trajectory<Eqn>(problem: &OdeSolverProblem<Eqn>, method: Method, grid: &Grid) -> Result<Trajectory, DynamicsError>
where
    Eqn: OdeEquationsImplicit<T = f64>,
    Eqn::V: DefaultDenseMatrix<T = f64, C = Eqn::C>,
    Eqn::M: DefaultSolver,
    for<'vector> &'vector Eqn::V: VectorRef<Eqn::V>,
    for<'matrix> &'matrix Eqn::M: MatrixRef<Eqn::M>,
{
    fn dense<'problem, Eqn, Solver>(mut solver: Solver, grid: &Grid) -> Result<Trajectory, DynamicsError>
    where
        Eqn: OdeEquationsImplicit<T = f64> + 'problem,
        Eqn::V: DefaultDenseMatrix,
        Solver: OdeSolverMethod<'problem, Eqn>,
    {
        let (values, reason) = solver.solve_dense(&t_eval(grid.as_ref()))?;
        let (columns, times, stop) = recorded(grid.as_ref(), reason);
        let values = selected(&values, &columns);
        finite(&times, [&values])?;
        Ok(Trajectory { times, values, stop })
    }
    match method {
        Method::Bdf => dense::<Eqn, _>(problem.bdf::<<Eqn::M as DefaultSolver>::LS>()?, grid),
        Method::TrBdf2 => dense::<Eqn, _>(problem.tr_bdf2::<<Eqn::M as DefaultSolver>::LS>()?, grid),
        Method::Esdirk34 => dense::<Eqn, _>(problem.esdirk34::<<Eqn::M as DefaultSolver>::LS>()?, grid),
        Method::Tsit45 => dense::<Eqn, _>(problem.tsit45()?, grid),
    }
}

pub fn sensitivities<Eqn>(problem: &OdeSolverProblem<Eqn>, method: Method, grid: &Grid) -> Result<(Trajectory, Vec<DMatrix<f64>>), DynamicsError>
where
    Eqn: OdeEquationsImplicitSens<T = f64>,
    Eqn::V: DefaultDenseMatrix<T = f64, C = Eqn::C>,
    Eqn::M: DefaultSolver,
    for<'vector> &'vector Eqn::V: VectorRef<Eqn::V>,
    for<'matrix> &'matrix Eqn::M: MatrixRef<Eqn::M>,
{
    fn dense<'problem, Eqn, Solver>(mut solver: Solver, grid: &Grid) -> Result<(Trajectory, Vec<DMatrix<f64>>), DynamicsError>
    where
        Eqn: OdeEquationsImplicitSens<T = f64> + 'problem,
        Eqn::V: DefaultDenseMatrix,
        Eqn::M: DefaultSolver,
        Solver: SensitivitiesOdeSolverMethod<'problem, Eqn>,
    {
        let (values, sensitivities, reason) = solver.solve_dense_sensitivities(&t_eval(grid.as_ref()))?;
        let (columns, times, stop) = recorded(grid.as_ref(), reason);
        let (values, sensitivities) = (selected(&values, &columns), sensitivities.iter().map(|sensitivity| selected(sensitivity, &columns)).collect::<Vec<_>>());
        finite(&times, iter::once(&values).chain(&sensitivities))?;
        Ok((Trajectory { times, values, stop }, sensitivities))
    }
    match method {
        Method::Bdf => dense::<Eqn, _>(problem.bdf_sens::<<Eqn::M as DefaultSolver>::LS>()?, grid),
        Method::TrBdf2 => dense::<Eqn, _>(problem.tr_bdf2_sens::<<Eqn::M as DefaultSolver>::LS>()?, grid),
        Method::Esdirk34 => dense::<Eqn, _>(problem.esdirk34_sens::<<Eqn::M as DefaultSolver>::LS>()?, grid),
        Method::Tsit45 => dense::<Eqn, _>(problem.tsit45_sens()?, grid),
    }
}

pub fn gradient<Eqn>(
    problem: &OdeSolverProblem<Eqn>,
    method: Method,
    grid: &Grid,
    max_steps_between_checkpoints: Option<usize>,
    cotangents: impl FnOnce(&Trajectory) -> Vec<DMatrix<f64>>,
) -> Result<(Trajectory, DMatrix<f64>), DynamicsError>
where
    Eqn: OdeEquationsImplicitAdjoint<T = f64> + OdeEquationsAdjoint,
    Eqn::V: DefaultDenseMatrix<T = f64, C = Eqn::C>,
    Eqn::M: DefaultSolver,
    for<'vector> &'vector Eqn::V: VectorRef<Eqn::V>,
    for<'matrix> &'matrix Eqn::M: MatrixRef<Eqn::M>,
{
    fn adjoint<'problem, Eqn, Solver, Adjoint>(
        mut forward: Solver,
        backward: impl FnOnce(CheckpointingPath<Eqn, Solver::State>, Solver, Option<usize>) -> Result<Adjoint, DiffsolError>,
        grid: &Grid,
        max_steps_between_checkpoints: Option<usize>,
        cotangents: impl FnOnce(&Trajectory) -> Vec<DMatrix<f64>>,
    ) -> Result<(Trajectory, DMatrix<f64>), DynamicsError>
    where
        Eqn: OdeEquationsImplicitAdjoint<T = f64> + OdeEquationsAdjoint + 'problem,
        Eqn::V: DefaultDenseMatrix<T = f64, C = Eqn::C>,
        Eqn::M: DefaultSolver,
        Solver: OdeSolverMethod<'problem, Eqn>,
        Adjoint: AdjointOdeSolverMethod<'problem, Eqn, Solver>,
    {
        let (path, values, reason) = forward.solve_dense_with_checkpointing(&t_eval(grid.as_ref()), max_steps_between_checkpoints)?;
        let (written, _) = reached(grid.as_ref(), reason);
        let (columns, times, stop) = recorded(written, reason);
        let values = selected(&values, &columns);
        finite(&times, [&values])?;
        let trajectory = Trajectory { times, values, stop };
        let held = cotangents(&trajectory);
        let (context, outputs) = (forward.problem().context(), forward.problem().eqn.nout());
        let integrated = forward.problem().integrate_out.then_some(outputs);
        integrated
            .map_or(!held.is_empty(), |integrated| held.is_empty() || held.len() == integrated)
            .ok_or_else(|| DiffsolError::Other(format!("{} cotangent lanes state no objective over {} integrated outputs", held.len(), integrated.unwrap_or_default())))?;
        let refused = held.iter().positions(|cotangent| cotangent.shape() != trajectory.values.shape() || cotangent.iter().any(|entry| !entry.is_finite())).collect::<Vec<_>>();
        refused.is_empty().ok_or_else(|| {
            DiffsolError::Other(format!(
                "cotangent lanes {} hold other than {} rows by {} instants of finite values",
                refused.iter().format(", "),
                trajectory.values.nrows(),
                trajectory.values.ncols()
            ))
        })?;
        let lanes = held
            .iter()
            .map(|cotangent| {
                let mut lane = context.dense_mat_zeros::<Eqn::V>(outputs, cotangent.ncols());
                cotangent.column_iter().enumerate().for_each(|(column, instant)| lane.column_mut(column).copy_from(&Eqn::V::from_slice(instant.as_slice(), context.clone())));
                lane
            })
            .collect::<Vec<_>>();
        let (state, _) =
            backward(path, forward, (!held.is_empty()).then_some(held.len()))?.solve_adjoint_backwards_pass(&t_eval(if held.is_empty() { &[] } else { written }), &lanes.iter().collect::<Vec<_>>())?;
        let sg = OdeSolverState::as_ref(&state).sg;
        let gradient = DMatrix::from_vec(sg.len(), sg.context().nbatch(), sg.clone_as_vec());
        gradient.iter().all(|entry| entry.is_finite()).ok_or_else(|| DiffsolError::Other("the adjoint pass leaves the finite range".to_owned()))?;
        Ok((trajectory, gradient))
    }
    match method {
        Method::Bdf => adjoint::<Eqn, _, _>(
            problem.bdf::<<Eqn::M as DefaultSolver>::LS>()?,
            |path, forward, lanes| problem.bdf_solver_adjoint::<<Eqn::M as DefaultSolver>::LS, _>(path, Some(forward), lanes),
            grid,
            max_steps_between_checkpoints,
            cotangents,
        ),
        Method::TrBdf2 => adjoint::<Eqn, _, _>(
            problem.tr_bdf2::<<Eqn::M as DefaultSolver>::LS>()?,
            |path, forward, lanes| problem.tr_bdf2_solver_adjoint::<<Eqn::M as DefaultSolver>::LS, _>(path, Some(forward), lanes),
            grid,
            max_steps_between_checkpoints,
            cotangents,
        ),
        Method::Esdirk34 => adjoint::<Eqn, _, _>(
            problem.esdirk34::<<Eqn::M as DefaultSolver>::LS>()?,
            |path, forward, lanes| problem.esdirk34_solver_adjoint::<<Eqn::M as DefaultSolver>::LS, _>(path, Some(forward), lanes),
            grid,
            max_steps_between_checkpoints,
            cotangents,
        ),
        Method::Tsit45 => adjoint::<Eqn, _, _>(problem.tsit45()?, |path, forward, lanes| problem.tsit45_solver_adjoint(path, Some(forward), lanes), grid, max_steps_between_checkpoints, cotangents),
    }
}

#[must_use]
pub fn sweep<Eqn, Output>(
    build: impl Fn() -> Result<OdeSolverProblem<Eqn>, DynamicsError> + Sync,
    solve: impl Fn(&OdeSolverProblem<Eqn>) -> Result<Output, DynamicsError> + Sync,
    design: DMatrixView<'_, f64>,
) -> Vec<Result<Output, DynamicsError>>
where
    Eqn: OdeEquations<T = f64>,
    Output: Send,
{
    (0..design.ncols())
        .into_par_iter()
        .map_init(
            || None::<OdeSolverProblem<Eqn>>,
            |slot, column| {
                let problem = match slot {
                    Some(problem) => problem,
                    None => slot.insert(build()?),
                };
                let scenario = design.column(column);
                let (expected, found) = (problem.eqn.nparams().strict_mul(problem.context().nbatch()), scenario.nrows());
                (found == expected).ok_or(DynamicsError::Parameters { expected, found })?;
                let refused = scenario.iter().positions(|parameter| !parameter.is_finite()).collect::<Vec<_>>();
                refused.is_empty().ok_or_else(|| DiffsolError::Other(format!("design column {column} holds parameters that are not finite at rows {}", refused.iter().format(", "))))?;
                problem.eqn.set_params(&<Eqn::V as Vector>::from_slice(scenario.as_slice(), problem.context().clone()));
                solve(problem)
            },
        )
        .collect()
}
