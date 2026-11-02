use core::num::NonZero;

use diffsol::{
    AdjointOdeSolverMethod, CheckpointingPath, Context as _, DefaultDenseMatrix, DefaultSolver, DenseMatrix, DiffsolError, OdeEquations, OdeEquationsAdjoint, OdeEquationsImplicit,
    OdeEquationsImplicitAdjoint, OdeEquationsImplicitSens, OdeSolverMethod, OdeSolverProblem, OdeSolverState, OdeSolverStopReason, SensitivitiesOdeSolverMethod, Vector, VectorRef, VectorView as _,
    VectorViewMut as _, matrix::MatrixRef,
};
use nalgebra::{DMatrix, DMatrixView, DMatrixViewMut};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use uom::si::{f64::Time, time::second};

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

fn recorded<M>(instants: &[Time], values: &M, reason: OdeSolverStopReason<f64>) -> (Trajectory, impl Fn(&M) -> DMatrix<f64>)
where M: DenseMatrix<T = f64> {
    let (written, root) = reached(instants, reason);
    let appended = instants.get(written.len()).and(root).map(|(instant, _)| (written.len(), instant));
    let (columns, times): (Vec<usize>, Vec<Time>) = written.iter().copied().enumerate().filter(|&(_, earlier)| appended.is_none_or(|(_, instant)| earlier < instant)).chain(appended).unzip();
    let select = move |matrix: &M| {
        DMatrix::from_iterator(matrix.nrows().strict_mul(matrix.context().nbatch()), columns.len(), columns.iter().flat_map(|&column| matrix.column(column).into_owned().clone_as_vec()))
    };
    (Trajectory { times, values: select(values), stop: root.map(|(_, function)| Stop::Root { function }) }, select)
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
        let (values, reason) = solver.solve_dense(&t_eval(grid))?;
        Ok(recorded(grid, &values, reason).0)
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
        let (values, sensitivities, reason) = solver.solve_dense_sensitivities(&t_eval(grid))?;
        let (trajectory, select) = recorded(grid, &values, reason);
        Ok((trajectory, sensitivities.iter().map(select).collect()))
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
    objectives: usize,
    cotangents: impl FnOnce(&Trajectory, &mut [DMatrixViewMut<'_, f64>]),
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
        objectives: usize,
        cotangents: impl FnOnce(&Trajectory, &mut [DMatrixViewMut<'_, f64>]),
    ) -> Result<(Trajectory, DMatrix<f64>), DynamicsError>
    where
        Eqn: OdeEquationsImplicitAdjoint<T = f64> + OdeEquationsAdjoint + 'problem,
        Eqn::V: DefaultDenseMatrix,
        Eqn::M: DefaultSolver,
        Solver: OdeSolverMethod<'problem, Eqn>,
        Adjoint: AdjointOdeSolverMethod<'problem, Eqn, Solver>,
    {
        let (path, values, reason) = forward.solve_dense_with_checkpointing(&t_eval(grid), None)?;
        let (written, _) = reached(grid, reason);
        let (trajectory, _) = recorded(written, &values, reason);
        let mut held = vec![DMatrix::zeros(trajectory.values.nrows(), trajectory.values.ncols()); objectives];
        cotangents(&trajectory, &mut held.iter_mut().map(DMatrixViewMut::from).collect::<Vec<_>>());
        let (context, outputs) = (forward.problem().context().clone(), forward.problem().eqn.nout());
        let lanes = held
            .iter()
            .map(|cotangent| {
                let mut lane = context.dense_mat_zeros::<Eqn::V>(outputs, cotangent.ncols());
                cotangent.column_iter().enumerate().for_each(|(column, values)| lane.column_mut(column).copy_from(&Eqn::V::from_slice(values.as_slice(), context.clone())));
                lane
            })
            .collect::<Vec<_>>();
        let (state, _) = backward(path, forward, NonZero::new(objectives).map(NonZero::get))?.solve_adjoint_backwards_pass(&t_eval(written), &lanes.iter().collect::<Vec<_>>())?;
        let sg = OdeSolverState::as_ref(&state).sg;
        Ok((trajectory, DMatrix::from_vec(sg.len(), sg.context().nbatch(), sg.clone_as_vec())))
    }
    match method {
        Method::Bdf => adjoint::<Eqn, _, _>(
            problem.bdf::<<Eqn::M as DefaultSolver>::LS>()?,
            |path, forward, lanes| problem.bdf_solver_adjoint::<<Eqn::M as DefaultSolver>::LS, _>(path, Some(forward), lanes),
            grid,
            objectives,
            cotangents,
        ),
        Method::TrBdf2 => adjoint::<Eqn, _, _>(
            problem.tr_bdf2::<<Eqn::M as DefaultSolver>::LS>()?,
            |path, forward, lanes| problem.tr_bdf2_solver_adjoint::<<Eqn::M as DefaultSolver>::LS, _>(path, Some(forward), lanes),
            grid,
            objectives,
            cotangents,
        ),
        Method::Esdirk34 => adjoint::<Eqn, _, _>(
            problem.esdirk34::<<Eqn::M as DefaultSolver>::LS>()?,
            |path, forward, lanes| problem.esdirk34_solver_adjoint::<<Eqn::M as DefaultSolver>::LS, _>(path, Some(forward), lanes),
            grid,
            objectives,
            cotangents,
        ),
        Method::Tsit45 => adjoint::<Eqn, _, _>(problem.tsit45()?, |path, forward, lanes| problem.tsit45_solver_adjoint(path, Some(forward), lanes), grid, objectives, cotangents),
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
                problem.eqn.set_params(&<Eqn::V as Vector>::from_slice(scenario.as_slice(), problem.context().clone()));
                solve(problem)
            },
        )
        .collect()
}
