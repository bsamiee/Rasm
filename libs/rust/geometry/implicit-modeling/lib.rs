#![feature(array_try_map, const_convert, const_result_trait_fn, const_trait_impl, control_flow_into_value, default_field_values, float_conversions, iterator_try_collect)]

use core::{
    array,
    borrow::Borrow,
    iter,
    num::{NonZero, TryFromIntError},
    ops::ControlFlow,
    range::RangeInclusive,
};

use derive_where::derive_where;
use fidget::{
    context::Tree,
    eval::{BulkEvaluator, TracingEvaluator},
    jit::{JitFloatSliceEval, JitGradSliceEval, JitIntervalEval, JitShape},
    render::{CancelToken, ThreadPool},
    shape::{EzShape as _, ShapeBulkEval, ShapeBulkEvalError, ShapeTape, ShapeTracingEval, ShapeTracingEvalError, ShapeVars, Transformable},
    types::{Grad, Interval},
    var::Var,
};
use itertools::{Itertools as _, iproduct, izip};
use nalgebra::{Isometry3, Point3, Unit, Vector3, Vector4};
use parry3d_f64::shape::TriMeshBuilderError;
use rayon::{
    iter::{IndexedParallelIterator as _, IntoParallelIterator as _, ParallelIterator as _},
    slice::ParallelSlice as _,
};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Sample: Sized + Send {
    type Eval: BulkEvaluator<Data: Default + Transformable + Sync>;
    fn tape(shape: &Shape) -> &ShapeTape<<Self::Eval as BulkEvaluator>::Tape>;
    fn seed(position: Vector3<f32>) -> [<Self::Eval as BulkEvaluator>::Data; 3];
    fn widen(value: <Self::Eval as BulkEvaluator>::Data) -> Option<Self>;
}

impl Sample for f64 {
    type Eval = JitFloatSliceEval;
    fn tape(shape: &Shape) -> &ShapeTape<<Self::Eval as BulkEvaluator>::Tape> { &shape.float }
    fn seed(position: Vector3<f32>) -> [f32; 3] { position.into() }
    fn widen(value: f32) -> Option<Self> { value.is_finite().then_some(Self::from(value)) }
}

impl Sample for (f64, Vector3<f64>) {
    type Eval = JitGradSliceEval;
    fn tape(shape: &Shape) -> &ShapeTape<<Self::Eval as BulkEvaluator>::Tape> { &shape.grad }
    fn seed(position: Vector3<f32>) -> [Grad; 3] {
        let [x, y, z]: [f32; 3] = position.into();
        [Grad::new(x, 1.0, 0.0, 0.0), Grad::new(y, 0.0, 1.0, 0.0), Grad::new(z, 0.0, 0.0, 1.0)]
    }
    fn widen(value: Grad) -> Option<Self> { Some(Vector4::from(value).cast::<f64>()).filter(|widened| widened.iter().copied().all(f64::is_finite)).map(|widened| (widened.w, widened.xyz())) }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Classification {
    Inside,
    Straddling,
    Outside,
}

impl From<Interval> for Classification {
    fn from(value: Interval) -> Self {
        match (value.upper() < 0.0, value.lower() > 0.0) {
            (true, _) => Self::Inside,
            (false, true) => Self::Outside,
            (false, false) => Self::Straddling,
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Field {
    pub origin: Point3<f64>,
    pub tree: Tree,
}

#[derive_where(Debug)]
pub struct Shape {
    origin: Point3<f64>,
    offset: [Var; 3],
    half: [Var; 3],
    #[derive_where(skip)]
    float: ShapeTape<<JitFloatSliceEval as BulkEvaluator>::Tape>,
    #[derive_where(skip)]
    grad: ShapeTape<<JitGradSliceEval as BulkEvaluator>::Tape>,
    #[derive_where(skip)]
    interval: ShapeTape<<JitIntervalEval as TracingEvaluator>::Tape>,
    #[derive_where(skip)]
    jit: JitShape,
}

impl From<Field> for Shape {
    fn from(Field { origin, tree }: Field) -> Self {
        let (x, y, z) = Tree::axes();
        let offset = array::from_fn(|_| Var::new());
        let half = array::from_fn(|_| Var::new());
        let [ox, oy, oz] = offset.map(Tree::from);
        let [hx, hy, hz] = half.map(Tree::from);
        let jit = JitShape::from(tree.remap_xyz(x.clone() + ox, y.clone() + oy, z.clone() + oz).max((x.abs() - hx).max(y.abs() - hy).max(z.abs() - hz)));
        Self { origin, offset, half, float: jit.ez_float_slice_tape(), grad: jit.ez_grad_slice_tape(), interval: jit.ez_interval_tape(), jit }
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Domain {
    center: Point3<f64>,
    half: Vector3<f32>,
}

impl Domain {
    #[must_use]
    pub fn new(RangeInclusive { start, last }: RangeInclusive<Point3<f64>>) -> Option<Self> {
        Some(((last - start) / 2.0).cast::<f32>()).filter(|half| half.iter().all(|extent| extent.is_normal() && *extent > 0.0)).map(|half| Self { center: nalgebra::center(&start, &last), half })
    }
}

impl From<Domain> for RangeInclusive<Point3<f64>> {
    fn from(Domain { center, half }: Domain) -> Self { span(center, centered(half)) }
}

#[derive_where(Debug)]
pub struct Evaluation<'caller> {
    pub batch: NonZero<usize>,
    #[derive_where(skip)]
    pub threads: Option<&'caller ThreadPool> = Some(&ThreadPool::Global),
    pub vars: Option<&'caller ShapeVars<f32>> = None,
    #[derive_where(skip)]
    pub cancel: Option<CancelToken> = None,
}

#[derive(Debug, Clone, Copy)]
struct Cell {
    lower: Vector3<f32>,
    upper: Vector3<f32>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ImplicitError {
    #[error(transparent)]
    Evaluation(#[from] ShapeBulkEvalError),
    #[error("field not finite at point {index}")]
    NotFinite {
        index: usize,
    },
    #[error("field singular in cell from {} through {}", .cell.start, .cell.last)]
    Singular {
        cell: RangeInclusive<Point3<f64>>,
    },
    #[error("cell size {size} resolves no octree over half extent {extent}")]
    Resolution {
        size: f64,
        extent: f32,
    },
    #[error("evaluation cancelled")]
    Cancelled,
    #[error("{vertices} vertices exceed u32 triangle mesh indices")]
    Capacity {
        vertices: usize,
        source: TryFromIntError,
    },
    #[error(transparent)]
    Mesh(#[from] TriMeshBuilderError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn place(field: &Field, pose: Isometry3<f64>) -> Field { Field { origin: pose * field.origin, tree: field.tree.remap_affine(nalgebra::convert(pose.rotation.inverse())) } }

pub fn sample<S>(shape: &Shape, evaluation: &Evaluation<'_>, points: impl IntoIterator<Item: Borrow<Point3<f64>>>) -> Result<Vec<S>, ImplicitError>
where S: Sample {
    evaluate(shape, evaluation, &bind(shape, evaluation, shape.origin, None), points.into_iter().map(|point| local(shape.origin, *point.borrow())))
}

pub fn project(
    shape: &Shape,
    evaluation: &Evaluation<'_>,
    tolerance: f64,
    iterations: NonZero<u32>,
    points: impl IntoIterator<Item: Borrow<Point3<f64>>>,
) -> Result<Vec<Option<Point3<f64>>>, ImplicitError> {
    (0..iterations.get())
        .try_fold(points.into_iter().map(|point| ControlFlow::Continue(*point.borrow())).collect::<Vec<_>>(), |mut steps, _| {
            let active = steps.iter().positions(ControlFlow::is_continue).collect::<Vec<_>>();
            let samples = sample::<(f64, Vector3<f64>)>(shape, evaluation, active.iter().map(|&index| steps[index].into_value())).map_err(|error| match error {
                ImplicitError::NotFinite { index } => ImplicitError::NotFinite { index: active[index] },
                unmapped @ (ImplicitError::Evaluation(_)
                | ImplicitError::Singular { cell: _ }
                | ImplicitError::Resolution { size: _, extent: _ }
                | ImplicitError::Cancelled
                | ImplicitError::Capacity { vertices: _, source: _ }
                | ImplicitError::Mesh(_)) => unmapped,
            })?;
            iter::zip(active, samples).for_each(|(index, (value, gradient))| {
                let point = steps[index].into_value();
                steps[index] = if value.abs() > tolerance {
                    ControlFlow::Continue(Unit::try_new_and_get(gradient, 0.0).map_or(point, |(normal, norm)| point - normal.into_inner() * (value / norm)))
                } else {
                    ControlFlow::Break(point)
                };
            });
            Ok(steps)
        })
        .map(|steps| steps.into_iter().map(ControlFlow::break_value).collect())
}

pub fn classify(shape: &Shape, evaluation: &Evaluation<'_>, domain: Domain) -> Result<Classification, ImplicitError> {
    interval(&mut ShapeTracingEval::default(), shape, &bind(shape, evaluation, domain.center, None), centered(domain.half)).map(Classification::from)
}

pub fn bounds(shape: &Shape, evaluation: &Evaluation<'_>, size: f64, search: Domain) -> Result<Option<Domain>, ImplicitError> {
    let vars = bind(shape, evaluation, search.center, None);
    let mut evaluator = ShapeTracingEval::default();
    subdivide(evaluation.cancel.as_ref(), depth(search.half.max(), size)?, centered(search.half), None, &mut |enclosed: Option<Cell>, cell @ Cell { lower, upper }, leaf| {
        let classification = enclosed
            .is_none_or(|Cell { lower: least, upper: most }| (lower - least).min() < 0.0 || (most - upper).min() < 0.0)
            .then(|| interval(&mut evaluator, shape, &vars, cell).map(Classification::from))
            .transpose()?;
        Ok(match (classification, leaf) {
            (None | Some(Classification::Inside | Classification::Outside), _) => ControlFlow::Break(enclosed),
            (Some(Classification::Straddling), true) =>
                ControlFlow::Break(Some(enclosed.map_or(cell, |Cell { lower: least, upper: most }| Cell { lower: least.inf(&lower), upper: most.sup(&upper) }))),
            (Some(Classification::Straddling), false) => ControlFlow::Continue(enclosed),
        })
    })
    .map(|enclosed| enclosed.and_then(|cell| Domain::new(span(search.center, cell))))
}

fn bind(shape: &Shape, evaluation: &Evaluation<'_>, center: Point3<f64>, half: Option<Vector3<f32>>) -> ShapeVars<f32> {
    evaluation
        .vars
        .into_iter()
        .flatten()
        .map(|(&index, &value)| (index, value))
        .chain(
            iter::chain(shape.offset, shape.half)
                .zip(iter::chain(&local(shape.origin, center), &half.unwrap_or_else(|| Vector3::repeat(f32::INFINITY))).copied())
                .filter_map(|(var, value)| var.index().map(|index| (index, value))),
        )
        .fold(ShapeVars::new(), |mut bound, (index, value)| {
            bound.insert(index, value);
            bound
        })
}

fn evaluate<S>(shape: &Shape, evaluation: &Evaluation<'_>, vars: &ShapeVars<f32>, positions: impl IntoIterator<Item = Vector3<f32>>) -> Result<Vec<S>, ImplicitError>
where S: Sample {
    let (xs, ys, zs) = positions
        .into_iter()
        .enumerate()
        .map(|(index, position)| position.iter().copied().all(f32::is_finite).then(|| <(_, _, _)>::from(S::seed(position))).ok_or(ImplicitError::NotFinite { index }))
        .collect::<Result<(Vec<_>, Vec<_>, Vec<_>), _>>()?;
    let size = evaluation.batch.get();
    let batch = |evaluator: &mut ShapeBulkEval<S::Eval>,
                 chunk: usize,
                 x: &[<S::Eval as BulkEvaluator>::Data],
                 y: &[<S::Eval as BulkEvaluator>::Data],
                 z: &[<S::Eval as BulkEvaluator>::Data]|
     -> Result<Vec<S>, ImplicitError> {
        cancellation_point(evaluation.cancel.as_ref())?;
        evaluator.eval_with_vars(S::tape(shape), x, y, z, vars)?.iter().copied().zip(chunk.strict_mul(size)..).map(|(value, index)| S::widen(value).ok_or(ImplicitError::NotFinite { index })).collect()
    };
    match evaluation.threads {
        None => {
            let mut evaluator = ShapeBulkEval::default();
            izip!(xs.chunks(size), ys.chunks(size), zs.chunks(size)).enumerate().map(|(chunk, (x, y, z))| batch(&mut evaluator, chunk, x, y, z)).flatten_ok().collect()
        }
        Some(pool) => pool.run(|| {
            (xs.par_chunks(size), ys.par_chunks(size), zs.par_chunks(size))
                .into_par_iter()
                .enumerate()
                .map_init(ShapeBulkEval::default, |evaluator, (chunk, (x, y, z))| batch(evaluator, chunk, x, y, z))
                .collect::<Result<Vec<_>, _>>()
                .map(|batches| batches.into_iter().flatten().collect())
        }),
    }
}

fn cancellation_point(cancel: Option<&CancelToken>) -> Result<(), ImplicitError> { cancel.is_none_or(|token| !token.is_cancelled()).ok_or(ImplicitError::Cancelled) }

fn interval(evaluator: &mut ShapeTracingEval<JitIntervalEval>, shape: &Shape, vars: &ShapeVars<f32>, Cell { lower, upper }: Cell) -> Result<Interval, ImplicitError> {
    evaluator
        .eval_with_vars(&shape.interval, Interval::new(lower.x, upper.x), Interval::new(lower.y, upper.y), Interval::new(lower.z, upper.z), vars)
        .map(|(enclosure, _)| enclosure)
        .map_err(|ShapeTracingEvalError::MissingVar(source)| ImplicitError::from(ShapeBulkEvalError::from(source)))
}

fn subdivide<A>(cancel: Option<&CancelToken>, depth: u8, cell: Cell, state: A, visit: &mut impl FnMut(A, Cell, bool) -> Result<ControlFlow<A, A>, ImplicitError>) -> Result<A, ImplicitError> {
    let children = |Cell { lower, upper }: Cell| {
        let middle = lower.lerp(&upper, 0.5);
        let halves = [(lower, middle), (middle, upper)];
        iproduct!(halves, halves, halves).map(|((xl, xu), (yl, yu), (zl, zu))| Cell { lower: Vector3::new(xl.x, yl.y, zl.z), upper: Vector3::new(xu.x, yu.y, zu.z) })
    };
    cancellation_point(cancel)?;
    match (visit(state, cell, depth == 0)?, depth.checked_sub(1)) {
        (flow, None) | (flow @ ControlFlow::Break(_), Some(_)) => Ok(flow.into_value()),
        (ControlFlow::Continue(state), Some(next)) => children(cell).try_fold(state, |visited, child| subdivide(cancel, next, child, visited, visit)),
    }
}

fn singular(shape: &Shape, vars: &ShapeVars<f32>, cancel: Option<&CancelToken>, center: Point3<f64>, depth: u8, cube: Cell) -> Result<(), ImplicitError> {
    let mut evaluator = ShapeTracingEval::default();
    subdivide(cancel, depth, cube, (), &mut |(), cell, leaf| {
        let enclosure = interval(&mut evaluator, shape, vars, cell)?;
        match (Classification::from(enclosure), enclosure.lower().is_finite() && enclosure.upper().is_finite(), leaf) {
            (Classification::Inside | Classification::Outside, _, _) | (Classification::Straddling, true, _) => Ok(ControlFlow::Break(())),
            (Classification::Straddling, false, false) => Ok(ControlFlow::Continue(())),
            (Classification::Straddling, false, true) => Err(ImplicitError::Singular { cell: span(center, cell) }),
        }
    })
}

fn depth(extent: f32, size: f64) -> Result<u8, ImplicitError> {
    const DEEPEST: u8 = u8::try_from(usize::MAX.ilog(8)).ok().unwrap();
    let ratio = 2.0 * f64::from(extent) / size;
    (ratio > 0.0).then(|| ratio.log2().ceil().max(1.0)).and_then(f64::to_int_checked::<u8>).filter(|&levels| levels <= DEEPEST).ok_or(ImplicitError::Resolution { size, extent })
}

fn local(origin: Point3<f64>, point: Point3<f64>) -> Vector3<f32> { (point - origin).cast::<f32>() }

fn world(origin: Point3<f64>, position: Vector3<f32>) -> Point3<f64> { origin + position.cast::<f64>() }

fn centered(half: Vector3<f32>) -> Cell { Cell { lower: -half, upper: half } }

fn span(origin: Point3<f64>, Cell { lower, upper }: Cell) -> RangeInclusive<Point3<f64>> { RangeInclusive { start: world(origin, lower), last: world(origin, upper) } }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod expressions;
mod meshing;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use expressions::{Form, Lattice, Truss, UnitCell, distance, lattice, shell, solid, truss};
pub use fidget;
pub use meshing::{Boundary, Surface, mesh};
pub use nalgebra;
pub use parry3d_f64;
