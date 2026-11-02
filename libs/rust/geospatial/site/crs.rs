use core::borrow::Borrow;
use std::{borrow::Cow, path::Path};

use geo::BoundingRect;
use geo_types::{Coord, Point, Rect, coord};
use nalgebra::{Point3, Vector2};
use proj::{Area, Proj, ProjBuilder, ProjError, Transform};
use serde::{Deserialize, Serialize, de::IgnoredAny};
use serde_json::{Map, Value, value::Serializer};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::{Boundary, Position, SiteError, located};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Encoding {
    Wkt,
    Projjson,
}

#[derive(Clone, Copy, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
enum Unit {
    Degree,
    Metre,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub struct Factors {
    pub position: Position,
    pub meridian_convergence: Angle,
    pub meridional_scale: Ratio,
    pub parallel_scale: Ratio,
    pub areal_scale: Ratio,
    pub elevation_scale: Ratio,
    pub grid_unit: Length,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Extent {
    pub description: Option<String>,
    pub bounds: Vec<Rect<f64>>,
}

#[derive(Debug, Clone, Copy, Default)]
pub struct Context<'path> {
    pub area: Option<Area>,
    pub search_path: Option<&'path Path>,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, derive_more::Display, derive_more::AsRef, Serialize, Deserialize)]
#[as_ref(str)]
#[serde(try_from = "Cow<'static, str>")]
pub struct Crs(Cow<'static, str>);

impl Crs {
    pub const WGS84: Self = Self(Cow::Borrowed("EPSG:4326"));

    #[must_use]
    pub fn universal(position: Position) -> Self {
        let (east, north) = position.0.x_y();
        let zone = match (north, east) {
            (56.0..64.0, 3.0..12.0) => 32,
            (72.0.., 0.0..9.0) => 31,
            (72.0.., 9.0..21.0) => 33,
            (72.0.., 21.0..33.0) => 35,
            (72.0.., 33.0..42.0) => 37,
            _ => (east + 180.0).rem_euclid(360.0).div_euclid(6.0).to_int_saturating::<u32>().strict_add(1),
        };
        let code = match north {
            _ if north > 84.0 => 32_661,
            ..-80.0 => 32_761,
            ..0.0 => 32_700_u32.strict_add(zone),
            _ => 32_600_u32.strict_add(zone),
        };
        Self(Cow::Owned(format!("EPSG:{code}")))
    }

    pub fn factors(&self) -> Result<impl Fn(Point3<f64>) -> Result<Factors, SiteError> + use<>, SiteError> {
        const GEOGRAPHIC: System = System {
            subtype: "ellipsoidal",
            axis: [Axis { name: "Longitude", abbreviation: "lon", direction: "east", unit: Unit::Degree }, Axis { name: "Latitude", abbreviation: "lat", direction: "north", unit: Unit::Degree }],
        };
        const METRIC: System = System {
            subtype: "Cartesian",
            axis: [Axis { name: "Easting", abbreviation: "E", direction: "east", unit: Unit::Metre }, Axis { name: "Northing", abbreviation: "N", direction: "north", unit: Unit::Metre }],
        };
        fn projected(definition: Definition<Projjson<IgnoredAny>, Axes>) -> Option<Projected<Projjson<IgnoredAny>, Axes>> {
            match definition {
                Definition::Projected(projected) => Some(*projected),
                Definition::Compound { components } => components.into_iter().next().and_then(projected),
                Definition::Bound { source_crs } => projected(*source_crs),
                Definition::Other => None,
            }
        }
        fn encoded<T>(value: &T) -> Result<Value, serde_path_to_error::Error<serde_json::Error>>
        where T: Serialize {
            serde_path_to_error::serialize(value, Serializer)
        }
        let converted = |source| SiteError::Projjson { crs: self.clone(), source };
        let unprojected = || SiteError::Unprojected { crs: self.clone() };
        let refused = |source| SiteError::Definition { definition: self.0.clone(), source };
        let metres = |measure| match measure {
            Measure::Metres(value) => value,
            Measure::Scaled { value, unit: Linear { conversion_factor } } => value * conversion_factor,
        };
        let Projected { base_crs, own } =
            projected(serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(&self.encode(Encoding::Projjson)?)).map_err(converted)?).ok_or_else(unprojected)?;
        let (major, flattening) = match serde_path_to_error::deserialize::<_, Geodetic>(&base_crs.rest).map_err(converted)?.datum.ellipsoid {
            Ellipsoid::Flattened { semi_major_axis, inverse_flattening } => (metres(semi_major_axis), inverse_flattening.recip()),
            Ellipsoid::Axes { semi_major_axis, semi_minor_axis } => (metres(semi_major_axis), 1.0 - metres(semi_minor_axis) / metres(semi_major_axis)),
            Ellipsoid::Sphere { radius } => (metres(radius), 0.0),
        };
        let grid_unit = match &own.coordinate_system.axis {
            [Dimension { unit: Units::Scaled(Linear { conversion_factor }) }, ..] => Some(*conversion_factor),
            [Dimension { unit: Units::Named(Unit::Metre) }, ..] => Some(1.0),
            [Dimension { unit: Units::Named(Unit::Degree) }, ..] | [] => None,
        }
        .ok_or_else(unprojected)?;
        let geographic = encoded(&Projjson { coordinate_system: GEOGRAPHIC, ..base_crs }).map_err(converted)?;
        let metric = encoded(&Definition::Projected(Box::new(Projected { base_crs: &geographic, own: Projjson { coordinate_system: METRIC, ..own } }))).map_err(converted)?;
        let (geographic, metric) = (geographic.to_string(), metric.to_string());
        let inverse = Proj::new_known_crs(self.as_ref(), &geographic, None).map_err(refused)?;
        let forward = Proj::new_known_crs(&geographic, &metric, None).map_err(refused)?;
        let (crs, eccentricity_squared, step) = (self.clone(), flattening * (2.0 - flattening), Angle::new::<radian>(1.0e-5));
        let (offset, run) = (step.get::<degree>(), 4.0 * step.get::<radian>());
        Ok(move |grid: Point3<f64>| {
            let failed = |source| SiteError::Proj { crs: crs.clone(), source };
            let position = located(inverse.convert(Point::new(grid.x, grid.y)).map_err(failed)?)?;
            let phi = position.0.y().clamp(offset - 90.0, 90.0 - offset);
            let mut corners = [(1.0, 1.0), (1.0, -1.0), (-1.0, -1.0), (-1.0, 1.0)].map(|(east, north)| coord! { x: offset.mul_add(east, position.0.x()), y: offset.mul_add(north, phi) });
            forward.convert_array(&mut corners).map_err(failed)?;
            let [north_east, south_east, south_west, north_west] = corners.map(|corner: Coord<f64>| Vector2::new(corner.x, corner.y));
            let (parallel, meridian) = ((north_east + south_east - south_west - north_west) / run, (north_east - south_east - south_west + north_west) / run);
            let (sin, cos) = Angle::new::<degree>(phi).get::<radian>().sin_cos();
            let w = (eccentricity_squared * sin).mul_add(-sin, 1.0).sqrt();
            let normal = major / w;
            let meridional = major * (1.0 - eccentricity_squared) / w.powi(3);
            let radius = (meridional * normal).sqrt();
            Ok(Factors {
                position,
                meridian_convergence: Angle::new::<radian>((-meridian.x).atan2(meridian.y)),
                meridional_scale: Ratio::new::<ratio>(meridian.norm() / meridional),
                parallel_scale: Ratio::new::<ratio>(parallel.norm() / (normal * cos)),
                areal_scale: Ratio::new::<ratio>(parallel.perp(&meridian) / (meridional * normal * cos)),
                elevation_scale: Ratio::new::<ratio>(radius / grid.z.mul_add(grid_unit, radius)),
                grid_unit: Length::new::<meter>(grid_unit),
            })
        })
    }

    pub fn area_of_use(&self) -> Result<Extent, SiteError> {
        match self.proj()?.area_of_use() {
            Ok((area, description)) => Ok(Extent { description, bounds: area.map_or_default(|Area { north, south, east, west }| extent([west, south, east, north])) }),
            Err(ProjError::UnknownAreaOfUse) => Ok(Extent { description: None, bounds: Vec::new() }),
            Err(source) => Err(SiteError::Proj { crs: self.clone(), source }),
        }
    }

    pub fn encode(&self, encoding: Encoding) -> Result<String, SiteError> {
        let proj = self.proj()?;
        match encoding {
            Encoding::Wkt => proj.as_wkt(None, None),
            Encoding::Projjson => proj.to_projjson(None, None, None),
        }
        .map_err(|source| SiteError::Proj { crs: self.clone(), source })
    }

    fn proj(&self) -> Result<Proj, SiteError> { Proj::new(self.as_ref()).map_err(|source| SiteError::Definition { definition: self.0.clone(), source }) }
}

impl TryFrom<Cow<'static, str>> for Crs {
    type Error = SiteError;

    fn try_from(value: Cow<'static, str>) -> Result<Self, SiteError> {
        let refused = |source| SiteError::Definition { definition: value.clone(), source };
        let proj = Proj::new(&value).map_err(refused)?;
        proj.create_crs_to_crs_from_pj(&proj, None, None).map_err(refused)?;
        let crs = Self(value);
        let described: Value =
            serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(&proj.to_projjson(None, None, None).map_err(|source| SiteError::Proj { crs: crs.clone(), source })?))
                .map_err(|source| SiteError::Projjson { crs: crs.clone(), source })?;
        let Identified { id } = serde_path_to_error::deserialize(&described).map_err(|source| SiteError::Projjson { crs: crs.clone(), source })?;
        let identical = |code: &String| {
            *code != *crs.0
                && Proj::new(code).is_ok_and(|registered| {
                    registered
                        .to_projjson(None, None, None)
                        .is_ok_and(|text| serde_path_to_error::deserialize::<_, Value>(&mut serde_json::Deserializer::from_str(&text)).is_ok_and(|other| other == described))
                })
        };
        Ok(id.map(|Id { authority, code }| format!("{authority}:{code}")).filter(identical).map_or(crs, |code| Self(Cow::Owned(code))))
    }
}

#[derive(Deserialize)]
struct Identified {
    id: Option<Id>,
}

#[derive(Deserialize)]
struct Id {
    authority: String,
    code: Code,
}

#[derive(Deserialize, derive_more::Display)]
#[serde(untagged)]
enum Code {
    Number(u64),
    Text(String),
}

#[derive(Deserialize, Serialize)]
#[serde(tag = "type")]
enum Definition<B, O> {
    #[serde(rename = "ProjectedCRS")]
    Projected(Box<Projected<B, O>>),
    #[serde(rename = "CompoundCRS", skip_serializing)]
    Compound { components: Vec<Self> },
    #[serde(rename = "BoundCRS", skip_serializing)]
    Bound { source_crs: Box<Self> },
    #[serde(other, skip_serializing)]
    Other,
}

#[derive(Deserialize, Serialize)]
struct Projected<B, O> {
    base_crs: B,
    #[serde(flatten)]
    own: Projjson<O>,
}

#[derive(Deserialize, Serialize)]
struct Projjson<S> {
    coordinate_system: S,
    #[serde(flatten)]
    rest: Map<String, Value>,
}

#[derive(Serialize)]
struct System {
    subtype: &'static str,
    axis: [Axis; 2],
}

#[derive(Serialize)]
struct Axis {
    name: &'static str,
    abbreviation: &'static str,
    direction: &'static str,
    unit: Unit,
}

#[derive(Deserialize)]
struct Axes {
    axis: Vec<Dimension>,
}

#[derive(Deserialize)]
struct Dimension {
    unit: Units,
}

#[derive(Deserialize)]
#[serde(untagged)]
enum Units {
    Scaled(Linear),
    Named(Unit),
}

#[derive(Deserialize)]
struct Geodetic {
    #[serde(alias = "datum_ensemble")]
    datum: Datum,
}

#[derive(Deserialize)]
struct Datum {
    ellipsoid: Ellipsoid,
}

#[derive(Deserialize)]
#[serde(untagged)]
enum Ellipsoid {
    Flattened { semi_major_axis: Measure, inverse_flattening: f64 },
    Axes { semi_major_axis: Measure, semi_minor_axis: Measure },
    Sphere { radius: Measure },
}

#[derive(Clone, Copy, Deserialize)]
#[serde(untagged)]
enum Measure {
    Metres(f64),
    Scaled { value: f64, unit: Linear },
}

#[derive(Clone, Copy, Deserialize)]
struct Linear {
    conversion_factor: f64,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Transformation {
    proj: Proj,
    from: Crs,
    to: Crs,
}

impl Transformation {
    pub fn new(from: &Crs, to: &Crs, context: Context<'_>) -> Result<Self, SiteError> {
        context
            .search_path
            .into_iter()
            .try_fold(ProjBuilder::new(), |mut builder, path| builder.set_search_paths(path).map(|()| builder).map_err(|source| SiteError::Search { path: path.to_path_buf(), source }))?
            .proj_known_crs(from.as_ref(), to.as_ref(), context.area)
            .map(|proj| Self { proj, from: from.clone(), to: to.clone() })
            .map_err(|source| SiteError::Operation { from: from.clone(), to: to.clone(), source })
    }

    pub fn apply<G>(&self, mut geometry: G) -> Result<G, SiteError>
    where G: Transform<f64> {
        geometry.transform(&self.proj).map(|()| geometry).map_err(|source| self.failed(source))
    }

    pub fn bounds<I>(&self, region: I) -> Result<Vec<Rect<f64>>, SiteError>
    where I: IntoIterator<Item: Borrow<Rect<f64>>> {
        region
            .into_iter()
            .map(|rect| {
                let (min, max) = (rect.borrow().min(), rect.borrow().max());
                self.proj.transform_bounds(min.x, min.y, max.x, max.y, 21).map(extent)
            })
            .try_collect::<Vec<_>>()
            .map(|parts| parts.concat())
            .map_err(|source| self.failed(source))
    }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Projection { from: self.from.clone(), to: self.to.clone(), source } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl From<&Boundary> for Area {
    fn from(value: &Boundary) -> Self {
        let half = Angle::HALF_TURN.get::<degree>();
        let [south, north, low, high, eastern, western] =
            value.iter().filter_map(BoundingRect::bounding_rect).fold([half / 2.0, -half / 2.0, half, -half, half, -half], |[south, north, low, high, eastern, western], rect| {
                let east_of_greenwich = rect.center().x >= 0.0;
                [
                    south.min(rect.min().y),
                    north.max(rect.max().y),
                    low.min(rect.min().x),
                    high.max(rect.max().x),
                    if east_of_greenwich { eastern.min(rect.min().x) } else { eastern },
                    if east_of_greenwich { western } else { western.max(rect.max().x) },
                ]
            });
        let split = low.total_cmp(&-half).is_eq() && high.total_cmp(&half).is_eq();
        Self { west: if split { eastern } else { low }, south, east: if split { western } else { high }, north }
    }
}

fn extent([west, south, east, north]: [f64; 4]) -> Vec<Rect<f64>> {
    let span = |left: f64, right: f64| Rect::new(coord! { x: left, y: south }, coord! { x: right, y: north });
    let half = Angle::HALF_TURN.get::<degree>();
    if west > east { vec![span(west, half), span(-half, east)] } else { vec![span(west, east)] }
}
