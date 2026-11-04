use core::{iter, num::NonZero};

use faer::{
    MatMut, MatRef, Par, Spec,
    diag::DiagMut,
    dyn_stack::{MemBuffer, MemStack},
    linalg::evd::{self, ComputeEigenvectors},
};
use kiddo::{Eytzinger, QueryResultItem, SquaredEuclidean, kd_tree::KdTree, leaf_strategies::VecOfArenas};
use nalgebra::{Matrix3, Point3, Unit, Vector3};
use nonempty_collections::NESlice;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};

use crate::{Distance, ProximityError};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Neighbor = QueryResultItem<Point3<f64>, usize, f64>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Neighborhood {
    Nearest { max_qty: NonZero<usize> },
    Within { radius: Distance },
    NearestWithin { max_qty: NonZero<usize>, radius: Distance },
}

#[derive(Debug, Clone, Copy, Default, PartialEq, PartialOrd)]
pub struct Item {
    pub index: usize,
    pub point: Point3<f64>,
}

#[derive(derive_more::Debug, derive_more::AsRef)]
pub struct Cloud(#[debug(skip)] KdTree<f64, Item, Eytzinger, VecOfArenas<f64, Item, 3, 32>, 3, 32>);

impl Cloud {
    pub fn new(points: &NESlice<'_, Point3<f64>>) -> Result<Self, ProximityError> {
        crate::bounded(points.as_ref())?;
        Ok(Self(KdTree::new_from_source_parallel(points.as_ref(), |point, axis| point[axis], |index, &point| Item { index, point })?))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn nearest(cloud: &Cloud, queries: &[Point3<f64>]) -> Result<Vec<Neighbor>, ProximityError> {
    crate::bounded(queries)?;
    Ok(cloud.0.query_batch(bytemuck::cast_slice(queries)).nearest_one::<SquaredEuclidean<f64>>().execute().into_iter().map(neighbor).collect())
}

pub fn neighbors(cloud: &Cloud, queries: &[Point3<f64>], neighborhood: Neighborhood) -> Result<Vec<Vec<Neighbor>>, ProximityError> {
    crate::bounded(queries)?;
    Ok(search(cloud, queries, neighborhood).into_iter().map(|group| group.into_iter().map(neighbor).collect()).collect())
}

pub fn normals(cloud: &Cloud, queries: &[Point3<f64>], neighborhood: Neighborhood, toward: Option<Point3<f64>>) -> Result<Vec<Option<Unit<Vector3<f64>>>>, ProximityError> {
    crate::bounded(iter::chain(queries, &toward))?;
    Ok(search(cloud, queries, neighborhood).into_par_iter().map_init(|| None, |scratch, group| normal(&group, toward, scratch)).collect())
}

pub(crate) fn search(cloud: &Cloud, queries: &[Point3<f64>], neighborhood: Neighborhood) -> Vec<Vec<QueryResultItem<(), Item, f64>>> {
    let batch = cloud.0.query_batch(bytemuck::cast_slice(queries));
    match neighborhood {
        Neighborhood::Nearest { max_qty } => batch.nearest_n::<SquaredEuclidean<f64>>(max_qty).execute(),
        Neighborhood::Within { radius } => batch.within::<SquaredEuclidean<f64>>(radius.into_inner().powi(2)).execute(),
        Neighborhood::NearestWithin { max_qty, radius } => batch.nearest_n::<SquaredEuclidean<f64>>(max_qty).within::<SquaredEuclidean<f64>>(radius.into_inner().powi(2)).execute(),
    }
    .into_nested_vec()
}

pub(crate) fn normal(group: &[QueryResultItem<(), Item, f64>], toward: Option<Point3<f64>>, scratch: &mut Option<(Par, MemBuffer)>) -> Option<Unit<Vector3<f64>>> {
    let centroid = group.iter().map(|neighbor| neighbor.item.point.coords).sum::<Vector3<f64>>() / nalgebra::convert::<usize, f64>(group.len());
    let covariance = group
        .iter()
        .map(|neighbor| {
            let offset = neighbor.item.point.coords - centroid;
            offset * offset.transpose()
        })
        .sum::<Matrix3<f64>>();
    let params = Spec::default();
    let (parallel, scratch) = scratch.get_or_insert_with(|| {
        let parallel = faer::get_global_parallelism();
        (parallel, MemBuffer::new(evd::self_adjoint_evd_scratch::<f64>(covariance.nrows(), ComputeEigenvectors::Yes, parallel, params)))
    });
    let (mut vectors, mut values) = (Matrix3::<f64>::zeros(), Vector3::<f64>::zeros());
    evd::self_adjoint_evd(
        MatRef::from_column_major_slice(covariance.as_slice(), covariance.nrows(), covariance.ncols()),
        DiagMut::from_slice_mut(values.as_mut_slice()),
        Some(MatMut::from_column_major_slice_mut(vectors.as_mut_slice(), covariance.nrows(), covariance.ncols())),
        *parallel,
        MemStack::new(scratch),
        params,
    )
    .ok()?;
    (values.y > values.z * f64::EPSILON * nalgebra::convert::<usize, f64>(covariance.nrows())).then(|| {
        let normal = Unit::new_normalize(vectors.column(0).into_owned());
        if toward.is_some_and(|toward| (toward.coords - centroid).dot(&normal) < 0.0) { -normal } else { normal }
    })
}

fn neighbor(QueryResultItem { point: (), item: Item { index: item, point }, distance }: QueryResultItem<(), Item, f64>) -> Neighbor { QueryResultItem { point, item, distance: distance.sqrt() } }
