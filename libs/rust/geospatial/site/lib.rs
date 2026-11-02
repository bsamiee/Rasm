#![feature(deref_patterns)]

use geo::{CoordsIter as _, Geodesic, GeodesicArea as _, Length as _, LinesIter as _, Validation as _, validation::InvalidMultiPolygon};
use geo_types::{MultiPolygon, Point};
use jiff::tz::{self, TimeZone};
use nalgebra::Point3;
use proj::{ProjCreateError, ProjError};
use tzf_rs::DefaultFinder;
use uom::si::{
    angle::degree,
    area::square_meter,
    f64::{Angle, Area, Length, Ratio},
    length::meter,
};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |angle: &Angle| angle.get::<degree>().abs() <= 90.0), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Latitude(Angle);

#[nutype::nutype(validate(predicate = |angle: &Angle| angle.get::<degree>().abs() <= 180.0), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Longitude(Angle);

#[nutype::nutype(validate(predicate = |length: &Length| length.is_finite()), derive(Debug, Clone, Copy, PartialEq, Deref))]
pub struct Elevation(Length);

#[derive(Debug, Clone, PartialEq)]
pub struct Place {
    pub latitude: Latitude,
    pub longitude: Longitude,
    pub elevation: Elevation,
    pub zone: TimeZone,
}

#[nutype::nutype(validate(with = enclosure, error = SiteError), derive(Debug, Clone, PartialEq, Deref))]
pub struct Boundary(MultiPolygon<f64>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SiteError {
    #[error("longitude {} and latitude {} lie outside ±180° and ±90°", point.x(), point.y())]
    Geographic { point: Point<f64> },
    #[error("no time zone database entry covers longitude {} and latitude {}", point.x(), point.y())]
    Zone { point: Point<f64>, source: jiff::Error },
    #[error("boundary geometry is invalid")]
    Geometry { source: InvalidMultiPolygon },
    #[error("boundary holds no polygon")]
    Empty,
    #[error("boundary polygon {polygon} encloses no geodesic area")]
    Area { polygon: usize },
    #[error("PROJ refused {definition}")]
    Creation { definition: Box<str>, source: ProjCreateError },
    #[error("PROJ failed on {definition}")]
    Projection { definition: Box<str>, source: ProjError },
    #[error("PROJJSON of {definition} holds no projected coordinate reference system")]
    Decode { definition: Box<str>, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("anchor {anchor}, its rotation or its scale is not finite, or the scale is not positive")]
    Anchor { anchor: Point3<f64>, rotation: Angle, scale: Ratio },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn point(latitude: Latitude, longitude: Longitude) -> Point<f64> { Point::new(longitude.get::<degree>(), latitude.get::<degree>()) }

pub fn coordinates(point: Point<f64>) -> Result<(Latitude, Longitude), SiteError> {
    match (Latitude::try_new(Angle::new::<degree>(point.y())), Longitude::try_new(Angle::new::<degree>(point.x()))) {
        (Ok(latitude), Ok(longitude)) => Ok((latitude, longitude)),
        (Err(LatitudeError::PredicateViolated), Ok(_) | Err(LongitudeError::PredicateViolated)) | (Ok(_), Err(LongitudeError::PredicateViolated)) => Err(SiteError::Geographic { point }),
    }
}

pub fn zone(finder: &DefaultFinder, latitude: Latitude, longitude: Longitude) -> Result<TimeZone, SiteError> {
    let point = point(latitude, longitude);
    tz::db().get(finder.get_tz_name(point.x(), point.y())).map_err(|source| SiteError::Zone { point, source })
}

#[must_use]
pub fn measure(boundary: &Boundary) -> (Length, Area) {
    (
        Length::new::<meter>(boundary.lines_iter().filter(|line| !matches!((line.start.x, line.end.x), (180.0, 180.0) | (-180.0, -180.0))).map(|line| Geodesic.length(&line)).sum()),
        Area::new::<square_meter>(boundary.iter().map(|polygon| polygon.geodesic_area_signed().abs()).sum()),
    )
}

fn enclosure(polygons: &MultiPolygon<f64>) -> Result<(), SiteError> {
    polygons.coords_iter().try_for_each(|coord| coordinates(Point(coord)).map(|(_, _)| ()))?;
    polygons.check_validation().map_err(|source| SiteError::Geometry { source })?;
    match &polygons.0 {
        [] => Err(SiteError::Empty),
        members => members.iter().position(|polygon| polygon.geodesic_area_signed() == 0.0).map_or(Ok(()), |polygon| Err(SiteError::Area { polygon })),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod crs;
pub mod frame;
