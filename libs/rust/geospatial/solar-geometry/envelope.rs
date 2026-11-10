use core::iter;

use itertools::Itertools as _;
use nalgebra::{Point3, Unit, UnitVector3, Vector2};
use ndarray::{Array, ArrayRef, Dimension, Zip};
use rstar::primitives::{GeomWithData, Line};
use rstar::{AABB, RTree};
use uom::si::f64::Length;
use uom::si::length::meter;

use crate::{Input, SolarError, finite, refused};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Envelope {
    Rights { cap: Option<Length> },
    Collection,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn envelope<D>(kind: Envelope, cells: &ArrayRef<Point3<f64>, D>, edges: &[[Point3<f64>; 2]], suns: &[UnitVector3<f64>]) -> Result<Array<Option<f64>, D>, SolarError>
where D: Dimension {
    let (cap, sign, lift) = match kind {
        Envelope::Rights { cap } => (cap.filter(|cap| !cap.is_finite()).map(|_| Input::Cap), -1.0, cap.map(|cap| cap.get::<meter>())),
        Envelope::Collection => (None, 1.0, Some(0.0)),
    };
    let rotated = |heading: &Unit<Vector2<f64>>, point: &Point3<f64>| [heading.dot(&point.coords.xy()), heading.perp(&point.coords.xy())];
    refused(
        iter::chain(
            cells.iter().positions(|cell| !finite(cell.coords.as_slice())).map(Input::Cell),
            edges.iter().positions(|edge| !edge.iter().all(|point| finite(point.coords.as_slice()))).map(Input::Edge),
        )
        .chain(suns.iter().positions(|sun| !finite(sun.as_slice())).map(Input::Direction))
        .chain(cap),
    )?;
    let limits = Zip::from(cells).par_map_collect(|cell| lift.map(|lift| cell.z + lift));
    Ok(suns.iter().filter(|sun| sun.z > 0.0).filter_map(|sun| Unit::try_new_and_get(sun.xy() * sign, 0.0).map(|(heading, run)| (heading, sun.z / run))).fold(limits, |mut limits, (heading, slope)| {
        let tree = RTree::bulk_load(edges.iter().map(|edge @ [start, end]| GeomWithData::new(Line::new(rotated(&heading, start), rotated(&heading, end)), edge)).collect());
        Zip::from(&mut limits).and(cells).par_for_each(|limit, cell| {
            let [x, y] = rotated(&heading, cell);
            *limit = tree
                .locate_in_envelope_intersecting(AABB::from_corners([x, y], [f64::INFINITY, y]))
                .filter_map(|&GeomWithData { data: &[start, end], .. }| {
                    let (edge, offset) = (end.xy() - start.xy(), start.xy() - cell.xy());
                    let across = heading.perp(&edge);
                    let (distance, share) = (offset.perp(&edge) / across, offset.perp(&heading) / across);
                    (distance >= 0.0 && (0.0..=1.0).contains(&share)).then(|| (-sign * distance).mul_add(slope, (end.z - start.z).mul_add(share, start.z)))
                })
                .chain(*limit)
                .reduce(|left, right| sign * (sign * left).max(sign * right));
        });
        limits
    }))
}
