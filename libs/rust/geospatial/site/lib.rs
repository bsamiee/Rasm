#![feature(deref_patterns, error_reporter, float_conversions, iterator_try_collect, type_changing_struct_update)]

use core::{fmt, iter};
use std::{borrow::Cow, error::Report, path::PathBuf, sync::LazyLock};

use geo::{Geodesic, GeodesicArea as _, Length as _, LinesIter as _, Validation as _, validation::InvalidMultiPolygon};
use geo_types::{MultiPolygon, Point};
use jiff::tz::{self, TimeZone};
use nalgebra::Point3;
use nonempty_collections::NEVec;
use proj::{ProjCreateError, ProjError};
use serde::{Deserialize, Serialize};
use tzf_rs::DefaultFinder;
use uom::si::{
    angle::degree,
    area::square_meter,
    f64::{Angle, Area, Length},
    length::meter,
};

use crate::{crs::Crs, frame::Scale};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Fault {
    #[display("spans over 180° of longitude, split it at the antimeridian")]
    Antimeridian,
    #[display("encloses no geodesic area")]
    Degenerate,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
#[serde(try_from = "Degrees", into = "Degrees")]
pub struct Position(Point<f64>);

impl Position {
    pub fn try_new(latitude: Angle, longitude: Angle) -> Result<Self, SiteError> { located(Point::new(longitude.get::<degree>(), latitude.get::<degree>())) }

    #[must_use]
    pub fn latitude(&self) -> Angle { Angle::new::<degree>(self.0.y()) }

    #[must_use]
    pub fn longitude(&self) -> Angle { Angle::new::<degree>(self.0.x()) }
}

#[derive(Serialize, Deserialize)]
struct Degrees {
    latitude: f64,
    longitude: f64,
}

impl From<Position> for Degrees {
    fn from(Position(point): Position) -> Self { Self { latitude: point.y(), longitude: point.x() } }
}

impl TryFrom<Degrees> for Position {
    type Error = SiteError;

    fn try_from(Degrees { latitude, longitude }: Degrees) -> Result<Self, SiteError> { located(Point::new(longitude, latitude)) }
}

#[nutype::nutype(validate(predicate = |length: &Length| length.is_finite()), derive(Debug, Clone, Copy, PartialEq, Deref, Serialize, Deserialize))]
pub struct Elevation(Length);

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Place {
    pub position: Position,
    pub elevation: Elevation,
    #[serde(with = "jiff::fmt::serde::tz::required")]
    pub zone: TimeZone,
}

#[nutype::nutype(validate(with = enclosure, error = SiteError), derive(Debug, Clone, PartialEq, Deref, Serialize, Deserialize))]
pub struct Boundary(MultiPolygon<f64>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum Defect {
    #[error("vertex at longitude {} and latitude {} lies outside ±180° and ±90°", point.x(), point.y())]
    Geographic { point: Point<f64> },
    #[error(transparent)]
    Geometry(InvalidMultiPolygon),
    #[error("holds no polygon")]
    Empty,
    #[error("polygon {polygon} {fault}")]
    Polygon { polygon: usize, fault: Fault },
}

#[derive(Debug, thiserror::Error)]
pub enum SiteError {
    #[error("longitude {} or latitude {} lies outside ±180° or ±90°", point.x(), point.y())]
    Geographic { point: Point<f64> },
    #[error("no time zone covers longitude {} and latitude {}", position.0.x(), position.0.y())]
    Uncovered { position: Position },
    #[error("jiff's time zone database lacks {name}, update it")]
    Zone { name: &'static str, source: jiff::Error },
    #[error("boundary is invalid{}", fmt::from_fn(|formatter| .defects.iter().try_for_each(|defect| write!(formatter, "\n{}", Report::new(defect)))))]
    Boundary { defects: NEVec<Defect> },
    #[error("PROJ refused the definition {definition}")]
    Definition { definition: Cow<'static, str>, source: ProjCreateError },
    #[error("PROJ holds no operation from {from} to {to}")]
    Operation { from: Crs, to: Crs, source: ProjCreateError },
    #[error("PROJ refused grid directory {}", path.display())]
    Search { path: PathBuf, source: ProjError },
    #[error("PROJ failed from {from} to {to}")]
    Projection { from: Crs, to: Crs, source: ProjError },
    #[error("PROJ failed on {crs}")]
    Proj { crs: Crs, source: ProjError },
    #[error("PROJJSON of {crs} does not convert")]
    Projjson { crs: Crs, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("{crs} is no projected coordinate reference system on linear axes, pass one")]
    Unprojected { crs: Crs },
    #[error("anchor {anchor} turned {}° at {scale} scale is not finite, or the scale is not positive", rotation.get::<degree>())]
    Anchor { anchor: Point3<f64>, rotation: Angle, scale: Scale },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn zone(position: Position) -> Result<TimeZone, SiteError> {
    static FINDER: LazyLock<DefaultFinder> = LazyLock::new(DefaultFinder::new);
    let name = Some(FINDER.get_tz_name(position.0.x(), position.0.y())).filter(|name| !name.is_empty()).ok_or(SiteError::Uncovered { position })?;
    tz::db().get(name).map_err(|source| SiteError::Zone { name, source })
}

#[must_use]
pub fn measure(boundary: &Boundary) -> (Length, Area) {
    let half = Angle::HALF_TURN.get::<degree>();
    (
        Length::new::<meter>(boundary.lines_iter().filter(|line| line.start.x.total_cmp(&line.end.x).is_ne() || line.start.x.abs().total_cmp(&half).is_ne()).map(|line| Geodesic.length(&line)).sum()),
        Area::new::<square_meter>(boundary.iter().map(|polygon| polygon.geodesic_area_signed().abs()).sum()),
    )
}

fn geographic(point: Point<f64>) -> bool { point.x().abs() <= Angle::HALF_TURN.get::<degree>() && point.y().abs() <= Angle::HALF_TURN.get::<degree>() / 2.0 }

fn located(point: Point<f64>) -> Result<Position, SiteError> { geographic(point).then_some(Position(point)).ok_or(SiteError::Geographic { point }) }

fn enclosure(boundary: &MultiPolygon<f64>) -> Result<(), SiteError> {
    let half = Angle::HALF_TURN.get::<degree>();
    let polygons = boundary.iter().enumerate().flat_map(|(polygon, member)| {
        [member.lines_iter().any(|line| (line.end.x - line.start.x).abs() > half).then_some(Fault::Antimeridian), (member.geodesic_area_signed() == 0.0).then_some(Fault::Degenerate)]
            .into_iter()
            .flatten()
            .map(move |fault| Defect::Polygon { polygon, fault })
    });
    NEVec::try_from_vec(
        iter::chain(boundary.0.is_empty().then_some(Defect::Empty), boundary.lines_iter().map(|line| Point(line.start)).filter(|point| !geographic(*point)).map(|point| Defect::Geographic { point }))
            .chain(boundary.validation_errors().into_iter().map(Defect::Geometry))
            .chain(polygons)
            .collect(),
    )
    .map_or(Ok(()), |defects| Err(SiteError::Boundary { defects }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod crs;
pub mod frame;
