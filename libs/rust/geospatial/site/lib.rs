#![feature(float_conversions)]

use core::fmt;
use std::path::PathBuf;

use geo::validation::InvalidMultiPolygon;
use geo::{BoundingRect as _, GeodesicArea as _, LinesIter as _, Validation as _};
use geo_types::{Line, MultiPolygon, Point, Rect};
use geographiclib_rs::{Geodesic, InverseGeodesic as _};
use jiff::tz::{self, TimeZone};
use nalgebra::Point3;
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator, nev};
use proj::{ProjCreateError, ProjError};
use serde::{Deserialize, Serialize};
use uom::si::angle::degree;
use uom::si::area::square_meter;
use uom::si::f64::{Angle, Area, Length};
use uom::si::length::meter;
use validated::Validated;

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

impl AsRef<Self> for Place {
    fn as_ref(&self) -> &Self { self }
}

#[derive(Debug, Clone, PartialEq, derive_more::Deref, Deserialize)]
#[serde(try_from = "MultiPolygon<f64>")]
pub struct Boundary {
    #[deref]
    geometry: MultiPolygon<f64>,
    extent: Rect<f64>,
}

impl TryFrom<MultiPolygon<f64>> for Boundary {
    type Error = SiteError;

    fn try_from(value: MultiPolygon<f64>) -> Result<Self, SiteError> {
        let polygons = value.iter().enumerate().flat_map(|(polygon, member)| {
            [
                member.lines_iter().any(|line| (line.end.x - line.start.x).abs() > half_turn()).then_some(Defect::Antimeridian(polygon)),
                (member.geodesic_area_signed() == 0.0).then_some(Defect::Degenerate(polygon)),
            ]
            .into_iter()
            .flatten()
        });
        let valid = value
            .lines_iter()
            .map(|line| Point(line.start))
            .filter(|point| located(*point).is_none())
            .map(Defect::Geographic)
            .chain(value.validation_errors().into_iter().map(Defect::Geometry))
            .chain(polygons)
            .try_into_nonempty_iter()
            .map(NonEmptyIterator::collect::<NEVec<_>>)
            .map_or(Validated::Good(()), Validated::Fail);
        Validated::from(value.bounding_rect().ok_or(Defect::Empty)).map2(valid, |extent, ()| Self { geometry: value, extent }).ok().map_err(SiteError::from)
    }
}

impl From<Boundary> for MultiPolygon<f64> {
    fn from(Boundary { geometry, extent: _ }: Boundary) -> Self { geometry }
}

impl Serialize for Boundary {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where S: serde::Serializer {
        self.geometry.serialize(serializer)
    }
}

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
    #[display("holds no exterior coordinate")]
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
    Zone { name: String, source: jiff::Error },
    #[error("PROJ refused {operation}")]
    Create { operation: Operation, source: ProjCreateError },
    #[error("PROJ failed on {operation}")]
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

pub fn zone<'name>(position: Position, find: impl FnOnce(f64, f64) -> &'name str) -> Result<TimeZone, SiteError> {
    let name = Some(find(position.0.x(), position.0.y())).filter(|name| !name.is_empty()).ok_or(SiteError::Uncovered { position })?;
    tz::db().get(name).map_err(|source| SiteError::Zone { name: name.to_owned(), source })
}

#[must_use]
pub fn measure(boundary: &Boundary) -> Survey {
    let half = half_turn();
    let geodesic = Geodesic::wgs84();
    let courses = boundary
        .lines_iter()
        .filter(|line| line.start.x.total_cmp(&line.end.x).is_ne() || line.start.x.abs().total_cmp(&half).is_ne())
        .map(|line| {
            let (distance, bearing, _, _): (f64, f64, f64, f64) = geodesic.inverse(line.start.y, line.start.x, line.end.y, line.end.x);
            (line, Angle::new::<degree>(bearing.rem_euclid(Angle::FULL_TURN.get::<degree>())), Length::new::<meter>(distance))
        })
        .collect::<Vec<_>>();
    let perimeter = courses.iter().map(|&(_, _, length)| length).sum();
    Survey { courses, perimeter, area: Area::new::<square_meter>(boundary.iter().map(|polygon| polygon.geodesic_area_signed().abs()).sum()) }
}

fn half_turn() -> f64 { Angle::HALF_TURN.get::<degree>() }

fn located(point: Point<f64>) -> Option<Position> { (point.x().abs() <= half_turn() && point.y().abs() <= half_turn() / 2.0).then_some(Position(point)) }

// --- [COMPOSITION] ---------------------------------------------------------------------

mod crs;
mod frame;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use crs::{Context, Crs, Encoding, Extent, Factors, Transformation};
pub use frame::{Anchor, Frame, North, Scale, Toward};
