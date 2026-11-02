use core::num::NonZero;

use faer::{MatRef, Side};
use kiddo::{Eytzinger, QueryResultItem, SquaredEuclidean, kd_tree::KdTree, leaf_strategies::VecOfArenas};
use nalgebra::{Matrix3, Point3, Unit, Vector3};
use nonempty_collections::NESlice;
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};

use crate::{Distance, ProximityError};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Neighbor = QueryResultItem<Point3<f64>, usize, f64>;
type Item = (usize, Point3<f64>);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Neighborhood {
    Nearest { max_qty: NonZero<usize> },
    Within { radius: Distance },
    NearestWithin { max_qty: NonZero<usize>, radius: Distance },
}

#[derive(derive_more::Debug, derive_more::AsRef)]
pub struct Cloud(#[debug(skip)] KdTree<f64, Item, Eytzinger, VecOfArenas<f64, Item, 3, 32>, 3, 32>);

impl Cloud {
    pub fn new(points: &NESlice<'_, Point3<f64>>) -> Result<Self, ProximityError> {
        crate::bounded(points.as_ref())?;
        Ok(Self(KdTree::new_from_source_parallel(points.as_ref(), |point, axis| point[axis], |index, &point| (index, point))?))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn nearest(cloud: &Cloud, queries: &[Point3<f64>]) -> Result<Vec<Neighbor>, ProximityError> {
    crate::bounded(queries)?;
    Ok(cloud.0.query_batch(bytemuck::cast_slice(queries)).nearest_one::<SquaredEuclidean<f64>>().execute().into_iter().map(neighbor).collect())
}

pub fn neighbors(cloud: &Cloud, queries: &[Point3<f64>], neighborhood: Neighborhood) -> Result<Vec<Vec<Neighbor>>, ProximityError> {
    crate::bounded(queries)?;
    Ok(search(cloud, queries, neighborhood))
}

pub fn normals(cloud: &Cloud, queries: &[Point3<f64>], neighborhood: Neighborhood, toward: Option<Point3<f64>>) -> Result<Vec<Option<Unit<Vector3<f64>>>>, ProximityError> {
    crate::bounded(queries.iter().chain(&toward))?;
    Ok(search(cloud, queries, neighborhood).into_par_iter().map(|group| normal(&group, toward)).collect())
}

pub(crate) fn search(cloud: &Cloud, queries: &[Point3<f64>], neighborhood: Neighborhood) -> Vec<Vec<Neighbor>> {
    let batch = cloud.0.query_batch(bytemuck::cast_slice(queries));
    match neighborhood {
        Neighborhood::Nearest { max_qty } => batch.nearest_n::<SquaredEuclidean<f64>>(max_qty).execute(),
        Neighborhood::Within { radius } => batch.within::<SquaredEuclidean<f64>>(radius.into_inner().powi(2)).execute(),
        Neighborhood::NearestWithin { max_qty, radius } => batch.nearest_n::<SquaredEuclidean<f64>>(max_qty).within::<SquaredEuclidean<f64>>(radius.into_inner().powi(2)).execute(),
    }
    .into_nested_vec()
    .into_iter()
    .map(|group| group.into_iter().map(neighbor).collect())
    .collect()
}

pub(crate) fn normal(group: &[Neighbor], toward: Option<Point3<f64>>) -> Option<Unit<Vector3<f64>>> {
    let centroid = group.iter().map(|neighbor| neighbor.point.coords).sum::<Vector3<f64>>() / nalgebra::convert::<usize, f64>(group.len());
    let covariance = group.iter().map(|neighbor| (neighbor.point.coords - centroid) * (neighbor.point.coords - centroid).transpose()).sum::<Matrix3<f64>>();
    let eigen = MatRef::from_column_major_slice(covariance.as_slice(), 3, 3).self_adjoint_eigen(Side::Lower).ok()?;
    let values = eigen.S().column_vector();
    (*values.get(1) > *values.get(2) * f64::EPSILON * nalgebra::convert::<usize, f64>(covariance.nrows())).then(|| {
        let normal = Unit::new_normalize(Vector3::from_fn(|row, _| *eigen.U().get(row, 0)));
        if toward.is_some_and(|toward| (toward.coords - centroid).dot(&normal) < 0.0) { -normal } else { normal }
    })
}

fn neighbor(QueryResultItem { point: (), item: (item, point), distance }: QueryResultItem<(), Item, f64>) -> Neighbor { QueryResultItem { point, item, distance: distance.sqrt() } }
