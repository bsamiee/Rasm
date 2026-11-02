use core::num::NonZero;

use kiddo::{Eytzinger, QueryResultItem, SquaredEuclidean, kd_tree::KdTree, leaf_strategies::VecOfArenas};
use nalgebra::Point3;
use nonempty_collections::NESlice;

use crate::{Distance, ProximityError};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Neighbour = QueryResultItem<Point3<f64>, usize, f64>;
type Item = (usize, Point3<f64>);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Neighbourhood {
    Nearest { max_qty: NonZero<usize> },
    Within { radius: Distance },
    NearestWithin { max_qty: NonZero<usize>, radius: Distance },
}

#[derive(derive_more::Debug)]
pub struct Cloud(#[debug(skip)] KdTree<f64, Item, Eytzinger, VecOfArenas<f64, Item, 3, 32>, 3, 32>);

impl Cloud {
    pub fn new(points: &NESlice<'_, Point3<f64>>) -> Result<Self, ProximityError> {
        Ok(Self(KdTree::new_from_entries(&points.as_ref().iter().copied().enumerate().zip(coordinates(points.as_ref())?).collect::<Vec<_>>())?))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn nearest(cloud: &Cloud, queries: &[Point3<f64>]) -> Result<Vec<Neighbour>, ProximityError> {
    Ok(cloud.0.query_batch(&coordinates(queries)?).nearest_one::<SquaredEuclidean<f64>>().execute().into_iter().map(neighbour).collect())
}

pub fn neighbours(cloud: &Cloud, queries: &[Point3<f64>], neighbourhood: Neighbourhood) -> Result<Vec<Vec<Neighbour>>, ProximityError> {
    let coordinates = coordinates(queries)?;
    let batch = cloud.0.query_batch(&coordinates);
    Ok(match neighbourhood {
        Neighbourhood::Nearest { max_qty } => batch.nearest_n::<SquaredEuclidean<f64>>(max_qty).execute(),
        Neighbourhood::Within { radius } => batch.within::<SquaredEuclidean<f64>>(radius.into_inner().powi(2)).execute(),
        Neighbourhood::NearestWithin { max_qty, radius } => batch.nearest_n::<SquaredEuclidean<f64>>(max_qty).within::<SquaredEuclidean<f64>>(radius.into_inner().powi(2)).execute(),
    }
    .into_nested_vec()
    .into_iter()
    .map(|group| group.into_iter().map(neighbour).collect())
    .collect())
}

fn coordinates(points: &[Point3<f64>]) -> Result<Vec<[f64; 3]>, ProximityError> {
    crate::finite(points.iter().map(|point| nalgebra::distance_squared(point, &-point).is_finite()))?;
    Ok(points.iter().map(|&point| <[f64; 3]>::from(point)).collect())
}

fn neighbour(QueryResultItem { point: (), item: (item, point), distance }: QueryResultItem<(), Item, f64>) -> Neighbour { QueryResultItem { point, item, distance: distance.sqrt() } }
