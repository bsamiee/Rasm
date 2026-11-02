#![feature(array_try_map, const_convert, const_result_trait_fn, const_trait_impl, default_field_values, iterator_try_collect)]

use core::{
    borrow::Borrow,
    num::{NonZero, TryFromIntError},
    range::RangeInclusive,
};

use fidget::{
    context::Tree,
    eval::BulkEvaluator,
    jit::{JitFloatSliceEval, JitFunction, JitGradSliceEval, JitShape},
    render::ThreadPool,
    shape::{BoundShape, EzShape as _, ShapeBulkEval, ShapeBulkEvalError, ShapeTape, ShapeVars, Transformable},
    shapes::{Move, types::Vec3},
    types::Grad,
};
use nalgebra::{Point3, Vector3};
use nutype::nutype;
use rayon::{iter::ParallelIterator as _, slice::ParallelSlice as _};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Sample: Sized + Send {
    type Eval: BulkEvaluator<Data: Default + Transformable>;
    fn tape(shape: &JitShape) -> ShapeTape<<Self::Eval as BulkEvaluator>::Tape>;
    fn seed(position: Vector3<f32>) -> [<Self::Eval as BulkEvaluator>::Data; 3];
    fn widen(value: <Self::Eval as BulkEvaluator>::Data) -> Self;
}

impl Sample for f64 {
    type Eval = JitFloatSliceEval;
    fn tape(shape: &JitShape) -> ShapeTape<<Self::Eval as BulkEvaluator>::Tape> { shape.ez_float_slice_tape() }
    fn seed(position: Vector3<f32>) -> [f32; 3] { position.into() }
    fn widen(value: f32) -> Self { Self::from(value) }
}

impl Sample for (f64, Vector3<f64>) {
    type Eval = JitGradSliceEval;
    fn tape(shape: &JitShape) -> ShapeTape<<Self::Eval as BulkEvaluator>::Tape> { shape.ez_grad_slice_tape() }
    fn seed(position: Vector3<f32>) -> [Grad; 3] {
        let [x, y, z]: [f32; 3] = position.into();
        [Grad::new(x, 1.0, 0.0, 0.0), Grad::new(y, 0.0, 1.0, 0.0), Grad::new(z, 0.0, 0.0, 1.0)]
    }
    fn widen(Grad { v: value, dx, dy, dz }: Grad) -> Self { (f64::from(value), Vector3::new(dx, dy, dz).cast::<f64>()) }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Field {
    pub origin: Point3<f64>,
    pub tree: Tree,
}

#[nutype(
    validate(predicate = |domain| ((domain.last - domain.start) / 2.0).cast::<f32>().iter().all(|half| half.is_finite() && *half > 0.0)),
    derive(Debug, Clone, Copy)
)]
pub struct Domain(RangeInclusive<Point3<f64>>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ImplicitError {
    #[error(transparent)]
    Evaluation(#[from] ShapeBulkEvalError),
    #[error("the field or its gradient is not finite between {} and {}", .domain.into_inner().start, .domain.into_inner().last)]
    NotFinite { domain: Domain },
    #[error("meshing was cancelled")]
    Cancelled,
    #[error("meshing produced {vertices} vertices, beyond the u32 indices a triangle mesh holds")]
    Capacity { vertices: usize, source: TryFromIntError },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn recentre(Field { origin: from, tree }: Field, origin: Point3<f64>) -> Field { Field { origin, tree: Move { shape: tree, offset: Vec3::from((from - origin).cast::<f32>()) }.into() } }

pub fn sample<S>(Field { origin, tree }: Field, points: impl IntoIterator<Item: Borrow<Point3<f64>>>, vars: Option<&ShapeVars<f32>>, threads: Option<&ThreadPool>) -> Result<Vec<S>, ImplicitError>
where S: Sample {
    let positions = points.into_iter().map(|point| (point.borrow() - origin).cast::<f32>()).collect::<Vec<_>>();
    let bound = bind(tree, vars)?;
    let tape = S::tape(bound.shape());
    let evaluate = |evaluator: &mut ShapeBulkEval<S::Eval>, chunk: &[Vector3<f32>]| {
        let (x, y, z) = chunk.iter().map(|&position| <(_, _, _)>::from(S::seed(position))).collect::<(Vec<_>, Vec<_>, Vec<_>)>();
        evaluator.eval_with_vars(&tape, &x, &y, &z, bound.vars()).map(|values| values.iter().copied().map(S::widen).collect::<Vec<_>>())
    };
    match (NonZero::new(positions.len()), threads) {
        (None, _) => Ok(Vec::new()),
        (Some(_), None) => evaluate(&mut ShapeBulkEval::default(), &positions),
        (Some(count), Some(pool)) => pool
            .run(|| positions.par_chunks(count.get().div_ceil(pool.thread_count())).map_init(ShapeBulkEval::default, evaluate).collect::<Result<Vec<_>, _>>())
            .map(|chunks| chunks.into_iter().flatten().collect()),
    }
    .map_err(ImplicitError::from)
}

fn bind(tree: Tree, vars: Option<&ShapeVars<f32>>) -> Result<BoundShape<'_, JitFunction, f32>, ImplicitError> {
    let shape = JitShape::from(tree);
    match vars {
        Some(vars) => BoundShape::new(shape, vars),
        None => BoundShape::try_from(shape),
    }
    .map_err(|source| ImplicitError::Evaluation(source.into()))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod expressions;
pub mod meshing;
