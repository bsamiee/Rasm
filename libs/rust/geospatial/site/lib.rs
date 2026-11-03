#![feature(deref_patterns, float_conversions, iterator_try_collect, type_changing_struct_update)]

use core::{fmt, iter};
use std::{path::PathBuf, sync::LazyLock};

use geo::{Bearing as _, Distance as _, Geodesic, GeodesicArea as _, LinesIter as _, Validation as _, validation::InvalidMultiPolygon};
use geo_types::{Line, MultiPolygon, Point};
use jiff::tz::{self, TimeZone};
use nalgebra::Point3;
use nonempty_collections::{NEVec, nev};
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

#[derive(Debug, Clone, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("{_0}")]
    Crs(Crs),
    #[display("transformation from {from} to {to}")]
    Transformation { from: Crs, to: Crs },
    #[display("grid directory {}", _0.display())]
    Search(PathBuf),
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
#[serde(try_from = "Degrees", into = "Degrees")]
pub struct Position(Point<f64>);

impl Position {
    pub fn try_new(latitude: Angle, longitude: Angle) -> Result<Self, SiteError> { Self::try_from(Degrees { latitude: latitude.get::<degree>(), longitude: longitude.get::<degree>() }) }

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

    fn try_from(Degrees { latitude, longitude }: Degrees) -> Result<Self, SiteError> {
        let point = Point::new(longitude, latitude);
        located(point).ok_or_else(|| nev![Defect::Geographic(point)].into())
    }
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

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Survey {
    #[as_ref]
    courses: Vec<(Line<f64>, Angle, Length)>,
    #[as_ref]
    perimeter: Length,
    #[as_ref]
    area: Area,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::Display)]
pub enum Defect {
    #[display("longitude {} or latitude {} lies outside ±180° or ±90°", _0.x(), _0.y())]
    Geographic(Point<f64>),
    #[display("{_0}")]
    Geometry(InvalidMultiPolygon),
    #[display("holds no polygon")]
    Empty,
    #[display("polygon {_0} spans over 180° of longitude, split it at the antimeridian")]
    Antimeridian(usize),
    #[display("polygon {_0} encloses no geodesic area")]
    Degenerate(usize),
}

#[derive(Debug, thiserror::Error)]
pub enum SiteError {
    #[error("invalid geometry{}", fmt::from_fn(|formatter| .defects.iter().try_for_each(|defect| write!(formatter, "\n{defect}"))))]
    Geometry { defects: NEVec<Defect> },
    #[error("no time zone covers longitude {} and latitude {}", position.0.x(), position.0.y())]
    Uncovered { position: Position },
    #[error("jiff's time zone database lacks {name}, update it")]
    Zone { name: &'static str, source: jiff::Error },
    #[error("PROJ refused the {operation}")]
    Create { operation: Operation, source: ProjCreateError },
    #[error("PROJ failed on the {operation}")]
    Proj { operation: Operation, source: ProjError },
    #[error("PROJJSON of {crs} does not convert")]
    Projjson { crs: Crs, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("{crs} is no projected coordinate reference system on linear axes, pass one")]
    Unprojected { crs: Crs },
    #[error("grid point {grid} lies outside the domain of {crs}")]
    Domain { crs: Crs, grid: Point3<f64> },
    #[error("anchor {anchor} turned {}° at {scale} scale is not finite, or the scale is not positive", rotation.get::<degree>())]
    Anchor { anchor: Point3<f64>, rotation: Angle, scale: Scale },
}

impl From<NEVec<Defect>> for SiteError {
    fn from(value: NEVec<Defect>) -> Self { Self::Geometry { defects: value } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn zone(position: Position) -> Result<TimeZone, SiteError> {
    static FINDER: LazyLock<DefaultFinder> = LazyLock::new(DefaultFinder::new);
    let name = Some(FINDER.get_tz_name(position.0.x(), position.0.y())).filter(|name| !name.is_empty()).ok_or(SiteError::Uncovered { position })?;
    tz::db().get(name).map_err(|source| SiteError::Zone { name, source })
}

#[must_use]
pub fn measure(boundary: &Boundary) -> Survey {
    let half = half_turn();
    let courses = boundary
        .lines_iter()
        .filter(|line| line.start.x.total_cmp(&line.end.x).is_ne() || line.start.x.abs().total_cmp(&half).is_ne())
        .map(|line| (line, Angle::new::<degree>(Geodesic.bearing(line.start_point(), line.end_point())), Length::new::<meter>(Geodesic.distance(line.start_point(), line.end_point()))))
        .collect::<Vec<_>>();
    let perimeter = courses.iter().map(|&(_, _, length)| length).sum();
    Survey { courses, perimeter, area: Area::new::<square_meter>(boundary.iter().map(|polygon| polygon.geodesic_area_signed().abs()).sum()) }
}

fn half_turn() -> f64 { Angle::HALF_TURN.get::<degree>() }

fn located(point: Point<f64>) -> Option<Position> { (point.x().abs() <= half_turn() && point.y().abs() <= half_turn() / 2.0).then_some(Position(point)) }

fn enclosure(boundary: &MultiPolygon<f64>) -> Result<(), SiteError> {
    let polygons = boundary.iter().enumerate().flat_map(|(polygon, member)| {
        [
            member.lines_iter().any(|line| (line.end.x - line.start.x).abs() > half_turn()).then_some(Defect::Antimeridian(polygon)),
            (member.geodesic_area_signed() == 0.0).then_some(Defect::Degenerate(polygon)),
        ]
        .into_iter()
        .flatten()
    });
    NEVec::try_from_vec(
        iter::chain(boundary.0.is_empty().then_some(Defect::Empty), boundary.lines_iter().map(|line| Point(line.start)).filter(|point| located(*point).is_none()).map(Defect::Geographic))
            .chain(boundary.validation_errors().into_iter().map(Defect::Geometry))
            .chain(polygons)
            .collect(),
    )
    .map_or(Ok(()), |defects| Err(defects.into()))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod crs;
pub mod frame;
