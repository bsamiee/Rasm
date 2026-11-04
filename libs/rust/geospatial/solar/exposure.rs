use core::{borrow::Borrow, hash::BuildHasher, iter, num::NonZero, ops::Bound};
use std::collections::HashMap;

use itertools::{Either, Itertools as _};
use nalgebra::{Point3, UnitQuaternion, UnitVector3};
use ndarray::{Array1, Array2, Axis, ShapeBuilder as _};
use nonempty_collections::nev;
use rasm_proximity::{
    Assembly, Distance, ProximityError,
    rays::{crossings, occluded},
};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use uom::si::{
    f64::{Ratio, Time},
    ratio::ratio,
    solid_angle::steradian,
    time::second,
};

use crate::{Input, SolarError, dome::patches, finite, refused};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum SkyMeasure {
    ViewFactor,
    Exposure,
    VerticalComponent,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0, less_or_equal = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd, Deref))]
pub struct Transmittance(f64);

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Sensor {
    pub point: Point3<f64>,
    pub normal: UnitVector3<f64>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn incidence<S>(assembly: &Assembly, translucent: &HashMap<usize, Transmittance, S>, offset: Distance, sensors: &[Sensor], directions: &[UnitVector3<f64>]) -> Result<Array2<f64>, SolarError>
where S: BuildHasher + Sync {
    refused(directions.iter().positions(|direction| !finite(direction.as_slice())).map(Input::Direction))?;
    let mut matrix = Array2::zeros((sensors.len(), directions.len()).f());
    matrix.axis_iter_mut(Axis(1)).into_par_iter().zip(directions).try_for_each(|(mut column, &direction)| {
        exposed(assembly, translucent, offset, sensors, direction).map(|values| iter::zip(&mut column, values).for_each(|(cell, (share, cosine))| *cell = share * cosine))
    })?;
    Ok(matrix)
}

pub fn sun_hours<S, I>(assembly: &Assembly, translucent: &HashMap<usize, Transmittance, S>, offset: Distance, sensors: &[Sensor], suns: I) -> Result<Array1<Time>, SolarError>
where
    S: BuildHasher + Sync,
    I: IntoIterator<Item: Borrow<(UnitVector3<f64>, Time)>>,
{
    let checked = suns.into_iter().enumerate().map(|(index, sun)| match *sun.borrow() {
        (direction, _) if !finite(direction.as_slice()) => Err(SolarError::from(nev![Input::Direction(index)])),
        (_, duration) if !(duration.is_finite() && duration.get::<second>() >= 0.0) => Err(SolarError::from(nev![Input::Duration(index)])),
        (direction, duration) => Ok((direction, duration.get::<second>())),
    });
    Ok(accumulated(assembly, translucent, offset, sensors, checked, |cosine| f64::from(cosine > 0.0))?.mapv(Time::new::<second>))
}

pub fn sky_view<S>(
    assembly: &Assembly,
    translucent: &HashMap<usize, Transmittance, S>,
    offset: Distance,
    subdivision: NonZero<u8>,
    sensors: &[Sensor],
    measure: SkyMeasure,
) -> Result<Array1<Ratio>, SolarError>
where
    S: BuildHasher + Sync,
{
    let luminance: fn(f64) -> f64 = match measure {
        SkyMeasure::ViewFactor | SkyMeasure::Exposure => |_| 1.0,
        SkyMeasure::VerticalComponent => |height| 2.0_f64.mul_add(height, 1.0) / 3.0,
    };
    let response: fn(f64) -> f64 = match measure {
        SkyMeasure::ViewFactor | SkyMeasure::VerticalComponent => |cosine| cosine,
        SkyMeasure::Exposure => |cosine| f64::from(cosine > 0.0),
    };
    let mut horizontal = 0.0;
    let sky = patches(subdivision, UnitQuaternion::identity())
        .filter(|patch| patch.direction.z > 0.0)
        .map(|patch| (patch.direction, luminance(patch.direction.z) * patch.solid_angle.get::<steradian>()))
        .inspect(|&(direction, weight)| horizontal = response(direction.z).mul_add(weight, horizontal));
    let received = accumulated(assembly, translucent, offset, sensors, sky.map(Ok), response)?;
    Ok(received.mapv(|received| Ratio::new::<ratio>(received / horizontal)))
}

fn accumulated<S>(
    assembly: &Assembly,
    translucent: &HashMap<usize, Transmittance, S>,
    offset: Distance,
    sensors: &[Sensor],
    weights: impl IntoIterator<Item = Result<(UnitVector3<f64>, f64), SolarError>>,
    response: impl Fn(f64) -> f64,
) -> Result<Array1<f64>, SolarError>
where
    S: BuildHasher + Sync,
{
    weights.into_iter().try_fold(Array1::zeros(sensors.len()), |mut total, weighted| {
        let (direction, weight) = weighted?;
        if direction.z > 0.0 {
            iter::zip(&mut total, exposed(assembly, translucent, offset, sensors, direction)?).for_each(|(sum, (share, cosine))| *sum = (response(cosine) * share).mul_add(weight, *sum));
        }
        Ok(total)
    })
}

fn exposed<S>(
    assembly: &Assembly,
    translucent: &HashMap<usize, Transmittance, S>,
    offset: Distance,
    sensors: &[Sensor],
    direction: UnitVector3<f64>,
) -> Result<impl Iterator<Item = (f64, f64)>, ProximityError>
where
    S: BuildHasher + Sync,
{
    let rays = sensors.par_iter().map(|&Sensor { point, normal }| {
        let reach = if normal.dot(&direction) > 0.0 { (Bound::Unbounded, Bound::Unbounded) } else { (Bound::Included(offset), Bound::Excluded(offset)) };
        (point + normal.into_inner() * offset.into_inner(), direction, reach)
    });
    let shares = if translucent.is_empty() {
        Either::Left(occluded(assembly, rays)?.into_iter().map(|shaded| f64::from(!shaded)))
    } else {
        Either::Right(crossings(assembly, rays)?.into_iter().map(|hits| hits.iter().map(|hit| translucent.get(&hit.element).map_or_default(|transmittance| **transmittance)).product::<f64>()))
    };
    Ok(shares.zip(sensors.iter().map(move |Sensor { point: _, normal }| normal.dot(&direction).max(0.0))))
}
