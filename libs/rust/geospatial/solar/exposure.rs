use core::{borrow::Borrow, cmp::Ordering, iter, num::NonZero, ops::Bound};

use nalgebra::{Point3, Unit, UnitQuaternion, Vector3};
use ndarray::{Array1, Array2, Axis, ShapeBuilder as _, Zip};
use rasm_proximity::{Assembly, Distance, ProximityError, rays::occluded};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use uom::si::{
    f64::{Ratio, Time},
    ratio::ratio,
    solid_angle::steradian,
    time::second,
};

use crate::{
    SolarError,
    dome::{Patch, patches},
};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn incidence(assembly: &Assembly, offset: Distance, sensors: &[(Point3<f64>, Unit<Vector3<f64>>)], directions: &[Unit<Vector3<f64>>]) -> Result<Array2<f64>, SolarError> {
    let mut matrix = Array2::zeros((sensors.len(), directions.len()).f());
    matrix.axis_iter_mut(Axis(1)).into_par_iter().zip(directions).try_for_each(|(mut column, &direction)| cosines(assembly, offset, sensors, direction).map(|unshaded| column.assign(&unshaded)))?;
    Ok(matrix)
}

pub fn sun_hours<I>(assembly: &Assembly, offset: Distance, sensors: &[(Point3<f64>, Unit<Vector3<f64>>)], suns: I) -> Result<Array1<Time>, SolarError>
where I: IntoIterator<Item: Borrow<(Unit<Vector3<f64>>, Time)>> {
    let above =
        suns.into_iter().map(|sun| *sun.borrow()).filter(|(direction, _)| direction.z.partial_cmp(&0.0).is_none_or(Ordering::is_gt)).map(|(direction, duration)| (direction, duration.get::<second>()));
    Ok(accumulated(assembly, offset, sensors, above, |cosine| f64::from(cosine > 0.0))?.mapv(Time::new::<second>))
}

pub fn sky_view(assembly: &Assembly, offset: Distance, subdivision: NonZero<u8>, sensors: &[(Point3<f64>, Unit<Vector3<f64>>)]) -> Result<Array1<Ratio>, SolarError> {
    let dome = patches(subdivision, UnitQuaternion::identity()).into_iter().map(|Patch { direction, solid_angle }| (direction, solid_angle.get::<steradian>())).collect::<Vec<_>>();
    let hemisphere = dome.iter().fold(0.0, |sum, (direction, steradians)| direction.z.mul_add(*steradians, sum));
    Ok(accumulated(assembly, offset, sensors, dome, |cosine| cosine)?.mapv(|projected| Ratio::new::<ratio>(projected / hemisphere)))
}

fn accumulated(
    assembly: &Assembly,
    offset: Distance,
    sensors: &[(Point3<f64>, Unit<Vector3<f64>>)],
    weights: impl IntoIterator<Item = (Unit<Vector3<f64>>, f64)>,
    response: impl Fn(f64) -> f64,
) -> Result<Array1<f64>, ProximityError> {
    weights.into_iter().try_fold(Array1::zeros(sensors.len()), |mut total, (direction, weight)| {
        cosines(assembly, offset, sensors, direction).map(|unshaded| {
            Zip::from(&mut total).and(&unshaded).for_each(|sum, &cosine| *sum = response(cosine).mul_add(weight, *sum));
            total
        })
    })
}

fn cosines(assembly: &Assembly, offset: Distance, sensors: &[(Point3<f64>, Unit<Vector3<f64>>)], direction: Unit<Vector3<f64>>) -> Result<Array1<f64>, ProximityError> {
    let (facing, empty) = ((Bound::Unbounded, Bound::Unbounded), (Bound::Included(offset), Bound::Excluded(offset)));
    let cosines = sensors.iter().map(|(_, normal)| normal.dot(&direction)).collect::<Vec<_>>();
    occluded(assembly, sensors.par_iter().zip(&cosines).map(|(&(point, normal), &cosine)| (point + normal.into_inner() * offset.into_inner(), direction, if cosine > 0.0 { facing } else { empty })))
        .map(|shaded| iter::zip(cosines, shaded).map(|(cosine, shaded)| if shaded { 0.0 } else { cosine.max(0.0) }).collect::<Array1<_>>())
}
