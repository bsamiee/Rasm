use core::{iter, num::NonZero, time::Duration};

use bounded_integer::BoundedU32;
use nalgebra::DVector;
use nlopt::{Algorithm, FailState, MObjFn, Nlopt, ObjFn, Target};
use nonempty_collections::NEVec;
use rasm_stats::{Bounds, Space};

use crate::{OptimizeError, Solution};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(derive_more::Debug, Clone)]
pub struct Mapping<'function> {
    pub tolerances: NEVec<f64>,
    #[debug(skip)]
    pub evaluate: &'function (dyn MObjFn<()> + Sync),
}

#[derive(Debug, Clone)]
pub struct Optimizer {
    pub algorithm: Algorithm,
    pub maxeval: BoundedU32<1, { i32::MAX.cast_unsigned() }>,
    pub maxtime: Option<Duration> = None,
    pub ftol_rel: Option<f64> = None,
    pub ftol_abs: Option<f64> = None,
    pub xtol_rel: Option<f64> = None,
    pub xtol_abs: Option<DVector<f64>> = None,
    pub population: Option<NonZero<u16>> = None,
    pub vector_storage: Option<NonZero<u16>> = None,
}

#[derive(Debug, Clone)]
pub struct Settings<'problem> {
    pub optimizer: Optimizer,
    pub local_optimizer: Option<Optimizer> = None,
    pub target: Target = Target::Minimize,
    pub stopval: Option<f64> = None,
    pub initial_step: Option<DVector<f64>> = None,
    pub inequalities: Vec<Mapping<'problem>> = Vec::new(),
    pub equalities: Vec<Mapping<'problem>> = Vec::new(),
    pub seed: Option<u64> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn optimize<K, F>(space: &Space<K>, start: DVector<f64>, objective: F, settings: &Settings<'_>) -> Result<Solution, OptimizeError>
where F: ObjFn<()> {
    fn configure<O, U>(nlopt: &mut Nlopt<O, U>, optimizer: &Optimizer) -> Result<(), FailState>
    where O: ObjFn<U> {
        nlopt.set_maxeval(optimizer.maxeval.get())?;
        optimizer.maxtime.map(|maxtime| nlopt.set_maxtime(maxtime.as_secs_f64())).transpose()?;
        optimizer.ftol_rel.map(|ftol_rel| nlopt.set_ftol_rel(ftol_rel)).transpose()?;
        optimizer.ftol_abs.map(|ftol_abs| nlopt.set_ftol_abs(ftol_abs)).transpose()?;
        optimizer.xtol_rel.map(|xtol_rel| nlopt.set_xtol_rel(xtol_rel)).transpose()?;
        optimizer.xtol_abs.as_ref().map(|xtol_abs| nlopt.set_xtol_abs(xtol_abs.as_slice())).transpose()?;
        optimizer.population.map(|population| nlopt.set_population(NonZero::<usize>::from(population).get())).transpose()?;
        optimizer.vector_storage.map(|vector_storage| nlopt.set_vector_storage(Some(NonZero::<usize>::from(vector_storage).get()))).transpose().map(drop)
    }
    let dimensions = space.len().get();
    iter::chain(iter::once(&start), &settings.initial_step)
        .chain(&settings.optimizer.xtol_abs)
        .chain(settings.local_optimizer.iter().filter_map(|local| local.xtol_abs.as_ref()))
        .try_for_each(|vector| (vector.len() == dimensions).ok_or(OptimizeError::Shape { expected: dimensions, found: vector.len() }))?;
    let ends = |end: fn(&Bounds) -> f64| space.values().into_iter().map(end).collect::<Vec<_>>();
    let mut nlopt = Nlopt::new(settings.optimizer.algorithm, dimensions, objective, settings.target, ());
    let configured: Result<(), FailState> = try {
        nlopt.set_lower_bounds(&ends(|bounds| bounds.start))?;
        nlopt.set_upper_bounds(&ends(|bounds| bounds.last))?;
        configure(&mut nlopt, &settings.optimizer)?;
        settings.stopval.map(|stopval| nlopt.set_stopval(stopval)).transpose()?;
        settings.initial_step.as_ref().map(|initial_step| nlopt.set_initial_step(initial_step.as_slice())).transpose()?;
        if let Some(local) = &settings.local_optimizer {
            let mut local_optimizer = nlopt.get_local_optimizer(local.algorithm);
            configure(&mut local_optimizer, local)?;
            nlopt.set_local_optimizer(local_optimizer)?;
        }
        settings.inequalities.iter().try_for_each(|mapping| nlopt.add_inequality_mconstraint(mapping.tolerances.len().get(), mapping.evaluate, (), mapping.tolerances.as_ref()).map(drop))?;
        settings.equalities.iter().try_for_each(|mapping| nlopt.add_equality_mconstraint(mapping.tolerances.len().get(), mapping.evaluate, (), mapping.tolerances.as_ref()).map(drop))?;
    };
    configured.map_err(|state| OptimizeError::Setting { state })?;
    Nlopt::<F, ()>::srand_seed(settings.seed);
    let mut point = start;
    match nlopt.optimize(point.as_mut_slice()) {
        Ok((status, value)) => Ok(Solution { point, objective: value, status }),
        Err((state, value)) => Err(OptimizeError::Nonlinear { state, point, objective: value }),
    }
}
