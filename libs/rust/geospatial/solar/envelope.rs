use core::iter;

use itertools::Itertools as _;
use nalgebra::{Point3, Unit, UnitVector3, Vector2};
use ndarray::{Array, ArrayRef, Dimension, Zip};
use rstar::{
    AABB, RTree,
    primitives::{GeomWithData, Line},
};
use uom::si::{f64::Length, length::meter};

use crate::{Input, SolarError, finite, refused};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Envelope {
    Rights { cap: Option<Length> },
    Collection,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn envelope<D>(kind: Envelope, cells: &ArrayRef<Point3<f64>, D>, edges: &[[Point3<f64>; 2]], suns: &[UnitVector3<f64>]) -> Result<Array<Option<Point3<f64>>, D>, SolarError>
where D: Dimension {
    refused(
        iter::chain(
            cells.iter().positions(|cell| !finite(cell.coords.as_slice())).map(Input::Cell),
            edges.iter().positions(|edge| !edge.iter().all(|point| finite(point.coords.as_slice()))).map(Input::Edge),
        )
        .chain(suns.iter().positions(|sun| !finite(sun.as_slice())).map(Input::Sun))
        .chain(match kind {
            Envelope::Rights { cap: Some(cap) } if !cap.is_finite() => Some(Input::Cap),
            Envelope::Rights { cap: _ } | Envelope::Collection => None,
        }),
    )?;
    let (sign, bound): (f64, fn(f64, f64) -> f64) = match kind {
        Envelope::Rights { cap: _ } => (-1.0, f64::min),
        Envelope::Collection => (1.0, f64::max),
    };
    let rotated = |heading: &Unit<Vector2<f64>>, point: &Point3<f64>| [heading.dot(&point.coords.xy()), heading.perp(&point.coords.xy())];
    let limits = Zip::from(cells).par_map_collect(|cell| {
        match kind {
            Envelope::Rights { cap } => cap.map(|cap| cell.z + cap.get::<meter>()),
            Envelope::Collection => Some(cell.z),
        }
        .map(|height| Point3::new(cell.x, cell.y, height))
    });
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
                .fold(*limit, |limit, height| Some(Point3::new(cell.x, cell.y, limit.map_or(height, |limit| bound(limit.z, height)))));
        });
        limits
    }))
}
