use core::{cmp::Ordering, iter};

use nalgebra::{DMatrixView, DVector};
use rayon::{
    iter::{IndexedParallelIterator as _, IntoParallelIterator as _, ParallelIterator as _},
    slice::ParallelSliceMut as _,
};

use crate::OptimizeError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Rank {
    pub point: usize,
    pub front: usize,
    pub crowding: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn rank(objectives: DMatrixView<'_, f64>, constraints: DMatrixView<'_, f64>) -> Result<Vec<Rank>, OptimizeError> {
    (constraints.ncols() == objectives.ncols()).ok_or(OptimizeError::Shape { expected: objectives.ncols(), found: constraints.ncols() })?;
    let order = |left: f64, right: f64| left.partial_cmp(&right).unwrap_or_else(|| left.is_nan().cmp(&right.is_nan()));
    let orders = |left: usize, right: usize| iter::zip(objectives.column(left), objectives.column(right)).map(|(&left, &right)| order(left, right));
    let dominates = |&(left, violation): &(usize, f64), &(right, against): &(usize, f64)| match order(violation, against) {
        Ordering::Less => true,
        Ordering::Equal => order(violation, 0.0).is_eq() && orders(left, right).try_fold(false, |strict, ordering| ordering.is_le().then_some(strict || ordering.is_lt())) == Some(true),
        Ordering::Greater => false,
    };
    let crowding = |members: &[(usize, f64)]| {
        objectives.select_columns(members.iter().map(|(point, _)| point)).row_iter().fold(DVector::zeros(members.len()), |crowding, row| {
            let mut sorted = row.iter().map(|value| value.min(f64::INFINITY)).enumerate().collect::<Vec<_>>();
            sorted.sort_by(|&(_, left), &(_, right)| left.total_cmp(&right));
            let mut finite = sorted.iter().map(|&(_, value)| value).filter(|value| value.is_finite());
            let span = finite.next().zip(finite.next_back()).map(|(low, high)| high - low).filter(|span| span.is_normal());
            let mut gaps = iter::chain(
                iter::chain(sorted.first(), sorted.last()).map(|&(slot, _)| (slot, f64::INFINITY)),
                sorted.array_windows().map(|&[(_, before), (slot, _), (_, after)]| (slot, span.map_or_default(|span| ((after - before) / span).max(0.0)))),
            )
            .collect::<Vec<_>>();
            gaps.sort_unstable_by_key(|&(slot, _)| slot);
            gaps.dedup_by_key(|&mut (slot, _)| slot);
            crowding + DVector::from_iterator(members.len(), gaps.into_iter().map(|(_, gap)| gap))
        })
    };
    let mut points = constraints.column_iter().map(|column| column.iter().map(|value| value.clamp(0.0, f64::INFINITY)).sum()).enumerate().collect::<Vec<_>>();
    points.par_sort_by(|&(left, violation), &(right, against)| order(violation, against).then_with(|| orders(left, right).find(|ordering| ordering.is_ne()).unwrap_or(Ordering::Equal)));
    let fronts = points.into_iter().fold(Vec::<Vec<(usize, f64)>>::new(), |mut fronts, point| {
        let index = fronts.partition_point(|front| front.iter().rev().any(|member| dominates(member, &point)));
        match fronts.get_mut(index) {
            Some(front) => front.push(point),
            None => fronts.push(vec![point]),
        }
        fronts
    });
    Ok(fronts
        .into_par_iter()
        .enumerate()
        .flat_map_iter(|(front, members)| {
            let mut ranks = iter::zip(Vec::from(crowding(&members).data), members).map(|(crowding, (point, _))| Rank { point, front, crowding }).collect::<Vec<_>>();
            ranks.sort_by(|left, right| right.crowding.total_cmp(&left.crowding));
            ranks
        })
        .collect())
}
