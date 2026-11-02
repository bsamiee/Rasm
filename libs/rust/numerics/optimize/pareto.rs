use core::{cmp::Ordering, iter};

use itertools::Itertools as _;
use nalgebra::DMatrixView;
use ordered_float::OrderedFloat;
use rayon::{
    iter::{IndexedParallelIterator as _, IntoParallelIterator as _, ParallelIterator as _},
    slice::ParallelSliceMut as _,
};

use crate::{Input, OptimizeError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Rank {
    pub point: usize,
    pub front: usize,
    pub violation: f64,
    pub crowding: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn rank(objectives: DMatrixView<'_, f64>, constraints: Option<DMatrixView<'_, f64>>) -> Result<Vec<Rank>, OptimizeError> {
    let points = objectives.ncols();
    constraints.map(|constraints| constraints.ncols()).filter(|&found| found != points).map_or(Ok(()), |found| Err(OptimizeError::Shape { input: Input::Constraints, expected: points, found }))?;
    let objective = |point: usize| objectives.column(point).into_iter().copied().map(OrderedFloat);
    let dominates = |&(left, violation): &(usize, f64), &(right, against): &(usize, f64)| match OrderedFloat(violation).cmp(&OrderedFloat(against)) {
        Ordering::Less => true,
        Ordering::Equal =>
            violation == 0.0 && iter::zip(objective(left), objective(right)).try_fold(false, |strict, (left, right)| left.cmp(&right).is_le().then_some(strict || left < right)) == Some(true),
        Ordering::Greater => false,
    };
    let crowding = |members: &[(usize, f64)]| {
        objectives.row_iter().fold(vec![0.0; members.len()], |mut crowding, row| {
            let sorted = members
                .iter()
                .filter_map(|&(point, _)| row.get(point).copied())
                .enumerate()
                .filter(|&(_, value)| !value.is_nan())
                .sorted_unstable_by(|&(_, left), &(_, right)| left.total_cmp(&right))
                .collect::<Vec<_>>();
            let mut finite = sorted.iter().map(|&(_, value)| value).filter(|value| value.is_finite());
            let span = finite.next().zip(finite.next_back()).map(|(low, high)| high - low).filter(|span| span.is_normal());
            let gaps = iter::once(f64::INFINITY)
                .chain(sorted.array_windows().map(|&[(_, before), _, (_, after)]| span.filter(|_| after != before).map_or_default(|span| (after - before) / span)))
                .chain(iter::once(f64::INFINITY))
                .take(sorted.len());
            iter::zip(&sorted, gaps).for_each(|(&(slot, _), gap)| {
                if let Some(entry) = crowding.get_mut(slot) {
                    *entry += gap;
                }
            });
            crowding
        })
    };
    let mut population = Vec::from_fn(points, |point| (point, constraints.map_or_default(|constraints| constraints.column(point).iter().map(|value| value.clamp(0.0, f64::INFINITY)).sum::<f64>())));
    population.par_sort_by(|&(left, violation), &(right, against)| OrderedFloat(violation).cmp(&OrderedFloat(against)).then_with(|| objective(left).cmp(objective(right))));
    let fronts = population.into_iter().fold(Vec::<Vec<(usize, f64)>>::new(), |mut fronts, member| {
        let index = fronts.partition_point(|front| front.iter().rev().any(|other| dominates(other, &member)));
        match fronts.get_mut(index) {
            Some(front) => front.push(member),
            None => fronts.push(vec![member]),
        }
        fronts
    });
    Ok(fronts
        .into_par_iter()
        .enumerate()
        .flat_map_iter(|(front, members)| {
            iter::zip(crowding(&members), members)
                .map(|(crowding, (point, violation))| Rank { point, front, violation, crowding })
                .sorted_by(|left, right| right.crowding.total_cmp(&left.crowding))
        })
        .collect())
}
