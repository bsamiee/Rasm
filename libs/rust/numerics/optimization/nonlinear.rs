use core::{iter, num::NonZero, time::Duration};

use bounded_integer::BoundedU32;
use itertools::{Either, Itertools as _};
use nalgebra::DVector;
use nlopt::{Algorithm, FailState, MObjFn, Nlopt, ObjFn, OptResult, Target};
use nonempty_collections::NEVec;
use rasm_stats::{Bounds, Space, Tolerance};
use validated::Validated;

use crate::{Input, OptimizeError, Solution};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Relation {
    Inequality,
    Equality,
}

pub trait Objective<E> = FnMut(&[f64], Option<&mut [f64]>) -> Result<f64, E>;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |maxtime| !maxtime.is_zero()), derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord))]
pub struct Maxtime(Duration);

#[nutype::nutype(validate(finite), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Stopval(f64);

#[derive(derive_more::Debug, Clone)]
pub struct Constraint<'function> {
    pub relation: Relation,
    pub tolerances: NEVec<Option<Tolerance>>,
    #[debug(skip)]
    pub evaluate: &'function (dyn MObjFn<()> + Sync),
}

#[derive(Debug, Clone)]
pub struct Optimizer {
    pub algorithm: Algorithm,
    pub maxeval: BoundedU32<1, { i32::MAX.cast_unsigned() }>,
    pub maxtime: Option<Maxtime> = None,
    pub ftol_rel: Option<Tolerance> = None,
    pub ftol_abs: Option<Tolerance> = None,
    pub xtol_rel: Option<Tolerance> = None,
    pub xtol_abs: Option<DVector<Tolerance>> = None,
    pub population: Option<NonZero<u16>> = None,
    pub vector_storage: Option<NonZero<u16>> = None,
}

#[derive(Debug, Clone)]
pub struct Settings<'problem> {
    pub optimizer: Optimizer,
    pub seed: u32,
    pub local_optimizer: Option<Optimizer> = None,
    pub stopval: Option<Stopval> = None,
    pub initial_step: Option<DVector<f64>> = None,
    pub constraints: Vec<Constraint<'problem>> = Vec::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn optimize<K, F, E>(space: &Space<K>, start: DVector<f64>, target: Target, objective: F, settings: &Settings<'_>) -> Result<Solution, Either<E, NEVec<OptimizeError>>>
where F: Objective<E> {
    fn configure<O, U>(nlopt: &mut Nlopt<O, U>, optimizer: &Optimizer) -> Result<(), FailState>
    where O: ObjFn<U> {
        let scalars = [
            (optimizer.maxtime.map(|maxtime| maxtime.into_inner().as_secs_f64()), Nlopt::set_maxtime as fn(&mut Nlopt<O, U>, f64) -> OptResult),
            (optimizer.ftol_rel.map(Tolerance::into_inner), Nlopt::set_ftol_rel),
            (optimizer.ftol_abs.map(Tolerance::into_inner), Nlopt::set_ftol_abs),
            (optimizer.xtol_rel.map(Tolerance::into_inner), Nlopt::set_xtol_rel),
        ];
        nlopt.set_maxeval(optimizer.maxeval.get())?;
        scalars.into_iter().try_for_each(|(value, set)| value.map(|value| set(nlopt, value)).transpose().map(drop))?;
        optimizer.xtol_abs.as_ref().map(|xtol_abs| nlopt.set_xtol_abs(xtol_abs.map(Tolerance::into_inner).as_slice())).transpose()?;
        optimizer.population.map(|population| nlopt.set_population(NonZero::<usize>::from(population).get())).transpose()?;
        nlopt.set_vector_storage(optimizer.vector_storage.map(|storage| NonZero::<usize>::from(storage).get())).map(drop)
    }
    let dimensions = space.len().get();
    let shape = |input: Input, found: usize| (found == dimensions).ok_or(OptimizeError::Shape { input, expected: dimensions, found });
    iter::chain(
        [
            shape(Input::Start, start.len()),
            settings.initial_step.as_ref().map_or(Ok(()), |initial_step| shape(Input::InitialStep, initial_step.len())),
            settings.optimizer.xtol_abs.as_ref().map_or(Ok(()), |xtol_abs| shape(Input::XtolAbs, xtol_abs.len())),
        ],
        settings.local_optimizer.iter().filter_map(|local| local.xtol_abs.as_ref()).map(|xtol_abs| shape(Input::LocalXtolAbs, xtol_abs.len())),
    )
    .chain(iter::zip(space.values(), &start).positions(|(bounds, value)| !bounds.contains(value)).map(|index| Err(OptimizeError::Entry { input: Input::Start, index })))
    .chain(settings.initial_step.iter().flatten().positions(|step| !step.is_normal()).map(|index| Err(OptimizeError::Entry { input: Input::InitialStep, index })))
    .collect::<Validated<(), _>>()
    .ok()
    .map_err(Either::Right)?;
    let ends = |end: fn(&Bounds) -> f64| space.values().into_iter().map(end).collect::<Vec<_>>();
    let (lower, upper) = (ends(|bounds| bounds.start), ends(|bounds| bounds.last));
    let worst = match target {
        Target::Minimize => f64::INFINITY,
        Target::Maximize => f64::NEG_INFINITY,
    };
    let algorithm = settings.optimizer.algorithm;
    let solved: Result<Solution, Either<E, OptimizeError>> = try bikeshed Result<Solution, Either<E, OptimizeError>> {
        let mut nlopt = Nlopt::new(
            algorithm,
            dimensions,
            |point: &[f64], slot: Option<&mut [f64]>, (objective, failure): &mut (F, Option<E>)| match failure {
                Some(_) => worst,
                None => objective(point, slot).unwrap_or_else(|error| {
                    *failure = Some(error);
                    worst
                }),
            },
            target,
            (objective, None),
        );
        (try {
            nlopt.set_lower_bounds(&lower)?;
            nlopt.set_upper_bounds(&upper)?;
            configure(&mut nlopt, &settings.optimizer)?;
            settings.stopval.map(|stopval| nlopt.set_stopval(stopval.into_inner())).transpose()?;
            settings.initial_step.as_ref().map(|initial_step| nlopt.set_initial_step(initial_step.as_slice())).transpose()?;
            if let Some(local) = &settings.local_optimizer {
                let mut local_optimizer = nlopt.get_local_optimizer(local.algorithm);
                configure(&mut local_optimizer, local)?;
                nlopt.set_local_optimizer(local_optimizer)?;
            }
        })
        .map_err(|state| Either::Right(OptimizeError::Algorithm { algorithm, state }))?;
        settings.constraints.iter().enumerate().try_for_each(|(constraint, Constraint { relation, tolerances, evaluate })| {
            let tolerances = tolerances.iter().map(|tolerance| tolerance.map_or_default(Tolerance::into_inner)).collect::<Vec<_>>();
            match relation {
                Relation::Inequality => nlopt.add_inequality_mconstraint(tolerances.len(), *evaluate, (), &tolerances),
                Relation::Equality => nlopt.add_equality_mconstraint(tolerances.len(), *evaluate, (), &tolerances),
            }
            .map(drop)
            .map_err(|state| Either::Right(OptimizeError::Constraint { constraint, state }))
        })?;
        Nlopt::<fn(&[f64], Option<&mut [f64]>, &mut ()) -> f64, ()>::srand_seed(Some(u64::from(settings.seed)));
        let mut point = start;
        let outcome = nlopt.optimize(point.as_mut_slice());
        match (outcome, nlopt.recover_user_data().1) {
            (Err((state @ (FailState::InvalidArgs | FailState::OutOfMemory), _)), _) => Err(Either::Right(OptimizeError::Algorithm { algorithm, state })),
            (_, Some(failure)) => Err(Either::Left(failure)),
            (Err((state, value)), None) => Err(Either::Right(OptimizeError::Nonlinear { point, objective: value, termination: Some(state) })),
            (Ok((_, value)), None) if !value.is_finite() => Err(Either::Right(OptimizeError::Nonlinear { point, objective: value, termination: None })),
            (Ok((status, value)), None) => Ok(Solution { point, objective: value, status }),
        }?
    };
    solved.map_err(|failure| failure.map_right(NEVec::new))
}
