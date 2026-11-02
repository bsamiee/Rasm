#![feature(deref_patterns, error_reporter, iterator_try_collect, type_changing_struct_update)]

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
    f64::{Angle, Area, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::crs::Crs;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum Defect {
    #[display("spans over 180° of longitude, split it at the antimeridian")]
    Antimeridian,
    #[display("encloses no geodesic area")]
    Degenerate,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(with = geographic, error = SiteError), derive(Debug, Clone, Copy, PartialEq, Deref, Serialize, Deserialize))]
pub struct Position(Point<f64>);

#[nutype::nutype(validate(predicate = |length: &Length| length.is_finite()), derive(Debug, Clone, Copy, PartialEq, Deref, Serialize, Deserialize))]
pub struct Elevation(Length);

#[derive(Debug, Clone, PartialEq, derive_more::AsRef, derive_more::Into, Serialize, Deserialize)]
#[serde(try_from = "(Position, Elevation)", into = "(Position, Elevation)")]
pub struct Place {
    #[as_ref]
    position: Position,
    #[as_ref]
    elevation: Elevation,
    #[as_ref]
    #[into(skip)]
    zone: TimeZone,
}

impl TryFrom<(Position, Elevation)> for Place {
    type Error = SiteError;

    fn try_from((position, elevation): (Position, Elevation)) -> Result<Self, SiteError> {
        static FINDER: LazyLock<DefaultFinder> = LazyLock::new(DefaultFinder::new);
        let name = Some(FINDER.get_tz_name(position.x(), position.y())).filter(|name| !name.is_empty()).ok_or(SiteError::Uncovered { position })?;
        tz::db().get(name).map(|zone| Self { position, elevation, zone }).map_err(|source| SiteError::Zone { name, source })
    }
}

#[nutype::nutype(validate(with = enclosure, error = SiteError), derive(Debug, Clone, PartialEq, Deref, Serialize, Deserialize))]
pub struct Boundary(MultiPolygon<f64>);

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SiteError {
    #[error("longitude {} or latitude {} lies outside ±180° or ±90°", point.x(), point.y())]
    Geographic { point: Point<f64> },
    #[error("no time zone covers longitude {} and latitude {}", position.x(), position.y())]
    Uncovered { position: Position },
    #[error("jiff's time zone database lacks {name}, update it")]
    Zone { name: &'static str, source: jiff::Error },
    #[error("boundary geometry is invalid")]
    Geometry { source: InvalidMultiPolygon },
    #[error("boundary holds no polygon")]
    Empty,
    #[error("boundary polygon {polygon} {defect}")]
    Polygon { polygon: usize, defect: Defect },
    #[error("boundary is invalid{}", fmt::from_fn(|formatter| .failures.iter().try_for_each(|failure| write!(formatter, "\n{}", Report::new(failure)))))]
    Boundary { failures: NEVec<Self> },
    #[error("PROJ refused the definition {definition}")]
    Definition { definition: Cow<'static, str>, source: ProjCreateError },
    #[error("PROJ holds no operation from {from} to {to}")]
    Operation { from: Crs, to: Crs, source: ProjCreateError },
    #[error("PROJ refused grid directory {}", path.display())]
    Search { path: PathBuf, source: ProjError },
    #[error("PROJ failed from {from} to {to}")]
    Projection { from: Crs, to: Crs, source: ProjError },
    #[error("PROJ failed to describe {crs}")]
    Export { crs: Crs, source: ProjError },
    #[error("PROJJSON of {crs} does not convert")]
    Projjson { crs: Crs, source: serde_path_to_error::Error<serde_json::Error> },
    #[error("{crs} is no projected coordinate reference system on linear axes, pass one")]
    Unprojected { crs: Crs },
    #[error("anchor {anchor} turned {}° at scale {} is not finite, or the scale is not positive", rotation.get::<degree>(), scale.get::<ratio>())]
    Anchor { anchor: Point3<f64>, rotation: Angle, scale: Ratio },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn measure(boundary: &Boundary) -> (Length, Area) {
    let half = Angle::HALF_TURN.get::<degree>();
    (
        Length::new::<meter>(boundary.lines_iter().filter(|line| line.start.x.total_cmp(&line.end.x).is_ne() || line.start.x.abs().total_cmp(&half).is_ne()).map(|line| Geodesic.length(&line)).sum()),
        Area::new::<square_meter>(boundary.iter().map(|polygon| polygon.geodesic_area_signed().abs()).sum()),
    )
}

fn geographic(point: &Point<f64>) -> Result<(), SiteError> {
    (point.x().abs() <= Angle::HALF_TURN.get::<degree>() && point.y().abs() <= Angle::HALF_TURN.get::<degree>() / 2.0).ok_or(SiteError::Geographic { point: *point })
}

fn enclosure(boundary: &MultiPolygon<f64>) -> Result<(), SiteError> {
    let half = Angle::HALF_TURN.get::<degree>();
    let defects = boundary.iter().enumerate().flat_map(|(polygon, member)| {
        [member.lines_iter().any(|line| (line.end.x - line.start.x).abs() > half).then_some(Defect::Antimeridian), (member.geodesic_area_signed() == 0.0).then_some(Defect::Degenerate)]
            .into_iter()
            .flatten()
            .map(move |defect| SiteError::Polygon { polygon, defect })
    });
    NEVec::try_from_vec(
        iter::chain(boundary.0.is_empty().then_some(SiteError::Empty), boundary.lines_iter().filter_map(|line| Position::try_new(Point(line.start)).err()))
            .chain(boundary.validation_errors().into_iter().map(|source| SiteError::Geometry { source }))
            .chain(defects)
            .collect(),
    )
    .map_or(Ok(()), |failures| Err(SiteError::Boundary { failures }))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod crs;
pub mod frame;
