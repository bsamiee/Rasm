use itertools::Itertools as _;
use nalgebra::{Point3, Unit, Vector3};
use ndarray::{Array, ArrayView, Dimension, Zip};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Envelope {
    Rights,
    Collection,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn envelope<D>(kind: Envelope, cells: ArrayView<'_, Point3<f64>, D>, edges: &[[Point3<f64>; 2]], suns: &[Unit<Vector3<f64>>]) -> Array<Point3<f64>, D>
where D: Dimension {
    let (sign, bound): (f64, fn(f64, f64) -> f64) = match kind {
        Envelope::Rights => (-1.0, f64::min),
        Envelope::Collection => (1.0, f64::max),
    };
    let paths = suns.iter().filter(|sun| sun.z > 0.0).filter_map(|sun| Unit::try_new_and_get(sun.xy() * sign, 0.0).map(|(heading, run)| (heading.into_inner(), sun.z / run))).collect::<Vec<_>>();
    Zip::from(cells).par_map_collect(|cell| {
        let limit = edges
            .iter()
            .cartesian_product(&paths)
            .filter_map(|([start, end], (heading, slope))| {
                let (edge, offset) = (end.xy() - start.xy(), start.xy() - cell.xy());
                let across = heading.perp(&edge);
                let (distance, share) = (offset.perp(&edge) / across, offset.perp(heading) / across);
                (distance >= 0.0 && (0.0..=1.0).contains(&share)).then(|| (-sign * distance).mul_add(*slope, (end.z - start.z).mul_add(share, start.z)))
            })
            .fold(cell.z, bound);
        Point3::new(cell.x, cell.y, limit)
    })
}
