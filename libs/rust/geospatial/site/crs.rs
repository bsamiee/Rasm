use core::{borrow::Borrow, fmt, marker::PhantomData};
use std::{borrow::Cow, path::Path};

use geo::BoundingRect;
use geo_types::{Coord, MultiPolygon, Point, Polygon, Rect, coord};
use nalgebra::{Point3, Vector2};
use proj::{Area, Proj, ProjBuilder, ProjError, Transform};
use serde::{
    Deserialize, Deserializer, Serialize,
    de::{self, IgnoredAny, SeqAccess, Visitor},
};
use serde_json::{Map, Value, value::Serializer};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::{Boundary, Position, SiteError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Encoding {
    Wkt,
    Projjson,
}

#[derive(Clone, Copy, Serialize)]
#[serde(rename_all = "lowercase")]
enum Direction {
    East,
    North,
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
pub struct Context<'context> {
    pub area: Option<&'context Boundary>,
    pub search_paths: &'context [&'context Path],
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, derive_more::Display, derive_more::AsRef, Serialize, Deserialize)]
#[as_ref(str)]
#[serde(try_from = "Cow<'static, str>")]
pub struct Crs(Cow<'static, str>);

impl Crs {
    pub const WGS84: Self = Self(Cow::Borrowed("EPSG:4326"));

    #[must_use]
    pub fn universal(position: Position) -> Self {
        let (east, north) = position.x_y();
        let zone = match (north, east) {
            (56.0..64.0, 3.0..12.0) => 32,
            (72.0.., 0.0..9.0) => 31,
            (72.0.., 9.0..21.0) => 33,
            (72.0.., 21.0..33.0) => 35,
            (72.0.., 33.0..42.0) => 37,
            _ => nalgebra::convert_unchecked::<f64, u32>((east + 180.0).rem_euclid(360.0).div_euclid(6.0)).strict_add(1),
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
            axis: [
                Axis { name: "Longitude", abbreviation: "lon", direction: Direction::East, unit: Unit::Degree },
                Axis { name: "Latitude", abbreviation: "lat", direction: Direction::North, unit: Unit::Degree },
            ],
        };
        const METRIC: System = System {
            subtype: "Cartesian",
            axis: [
                Axis { name: "Easting", abbreviation: "E", direction: Direction::East, unit: Unit::Metre },
                Axis { name: "Northing", abbreviation: "N", direction: Direction::North, unit: Unit::Metre },
            ],
        };
        fn projected(definition: Definition<IgnoredAny, Axes>) -> Option<Projected<IgnoredAny, Axes>> {
            match definition {
                Definition::Projected(projected) => Some(*projected),
                Definition::Compound { components: horizontal } | Definition::Bound { source_crs: horizontal } => projected(*horizontal),
                Definition::Other => None,
            }
        }
        fn encoded<T>(value: &T) -> Result<String, serde_path_to_error::Error<serde_json::Error>>
        where T: Serialize {
            serde_path_to_error::serialize(value, Serializer).map(|tree| tree.to_string())
        }
        let converted = |source| SiteError::Projjson { crs: self.clone(), source };
        let unprojected = || SiteError::Unprojected { crs: self.clone() };
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
        let base = Projjson { coordinate_system: GEOGRAPHIC, ..base_crs };
        let geographic = Self(Cow::Owned(encoded(&base).map_err(converted)?));
        let metric = Self(Cow::Owned(encoded(&Definition::Projected(Box::new(Projected { base_crs: base, own: Projjson { coordinate_system: METRIC, ..own } }))).map_err(converted)?));
        let inverse = Transformation::new(self, &geographic, Context::default())?;
        let forward = Transformation::new(&geographic, &metric, Context::default())?;
        let eccentricity_squared = flattening * (2.0 - flattening);
        Ok(move |grid: Point3<f64>| {
            let position = Position::try_new(inverse.apply(Point::new(grid.x, grid.y))?)?;
            let step = Angle::new::<radian>(1.0e-5);
            let phi = Angle::new::<degree>(position.y().clamp(step.get::<degree>() - 90.0, 90.0 - step.get::<degree>()));
            let mut corners = [(1.0, 1.0), (1.0, -1.0), (-1.0, -1.0), (-1.0, 1.0)]
                .map(|(east, north)| coord! { x: step.get::<degree>().mul_add(east, position.x()), y: step.get::<degree>().mul_add(north, phi.get::<degree>()) });
            forward.proj.convert_array(&mut corners).map_err(|source| forward.failed(source))?;
            let [north_east, south_east, south_west, north_west] = corners.map(|corner: Coord<f64>| Vector2::new(corner.x, corner.y));
            let (parallel, meridian) =
                ((north_east + south_east - south_west - north_west) / (4.0 * step.get::<radian>()), (north_east - south_east - south_west + north_west) / (4.0 * step.get::<radian>()));
            let (sin, cos) = phi.get::<radian>().sin_cos();
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
            Err(source) => Err(SiteError::Export { crs: self.clone(), source }),
        }
    }

    pub fn encode(&self, encoding: Encoding) -> Result<String, SiteError> {
        let proj = self.proj()?;
        match encoding {
            Encoding::Wkt => proj.as_wkt(None, None),
            Encoding::Projjson => proj.to_projjson(Some(false), None, None),
        }
        .map_err(|source| SiteError::Export { crs: self.clone(), source })
    }

    fn proj(&self) -> Result<Proj, SiteError> { Proj::new(self.as_ref()).map_err(|source| SiteError::Definition { definition: self.0.clone(), source }) }
}

impl TryFrom<Cow<'static, str>> for Crs {
    type Error = SiteError;

    fn try_from(value: Cow<'static, str>) -> Result<Self, SiteError> {
        Proj::new_known_crs(&value, Self::WGS84.as_ref(), None).map_err(|source| SiteError::Definition { definition: value.clone(), source })?;
        let crs = Self(value);
        match serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(&crs.encode(Encoding::Projjson)?)) {
            Ok(Identified { id: Some(Id { authority, code }) }) => Ok(Self(Cow::Owned(format!("{authority}:{code}")))),
            Ok(Identified { id: None }) => Ok(crs),
            Err(source) => Err(SiteError::Projjson { crs, source }),
        }
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
    Compound {
        #[serde(deserialize_with = "horizontal")]
        components: Box<Self>,
    },
    #[serde(rename = "BoundCRS", skip_serializing)]
    Bound { source_crs: Box<Self> },
    #[serde(other, skip_serializing)]
    Other,
}

#[derive(Deserialize, Serialize)]
struct Projected<B, O> {
    base_crs: Projjson<B>,
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
    direction: Direction,
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
        let builder = context
            .search_paths
            .iter()
            .try_fold(ProjBuilder::new(), |mut builder, path| builder.set_search_paths(path).map(|()| builder).map_err(|source| SiteError::Search { path: path.to_path_buf(), source }))?;
        builder
            .proj_known_crs(from.as_ref(), to.as_ref(), context.area.and_then(|boundary| interest(boundary)))
            .map(|proj| Self { proj, from: from.clone(), to: to.clone() })
            .map_err(|source| SiteError::Operation { from: from.clone(), to: to.clone(), source })
    }

    pub fn apply<G>(&self, mut geometry: G) -> Result<G, SiteError>
    where G: Transform<f64> {
        geometry.transform(&self.proj).map_err(|source| self.failed(source))?;
        Ok(geometry)
    }

    pub fn bounds<I>(&self, region: I) -> Result<Vec<Rect<f64>>, SiteError>
    where I: IntoIterator<Item: Borrow<Polygon<f64>>> {
        region
            .into_iter()
            .filter_map(|polygon| polygon.borrow().bounding_rect())
            .map(|rect| self.proj.transform_bounds(rect.min().x, rect.min().y, rect.max().x, rect.max().y, 21).map(extent))
            .try_collect::<Vec<_>>()
            .map(|parts| parts.concat())
            .map_err(|source| self.failed(source))
    }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Projection { from: self.from.clone(), to: self.to.clone(), source } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn horizontal<'de, D, B, O>(deserializer: D) -> Result<Box<Definition<B, O>>, D::Error>
where
    D: Deserializer<'de>,
    B: Deserialize<'de>,
    O: Deserialize<'de>,
{
    struct First<B, O>(PhantomData<(B, O)>);
    impl<'de, B, O> Visitor<'de> for First<B, O>
    where
        B: Deserialize<'de>,
        O: Deserialize<'de>,
    {
        type Value = Box<Definition<B, O>>;

        fn expecting(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("components led by a horizontal coordinate reference system") }

        fn visit_seq<A>(self, mut seq: A) -> Result<Self::Value, A::Error>
        where A: SeqAccess<'de> {
            let first = seq.next_element()?.ok_or_else(|| de::Error::invalid_length(0, &self))?;
            while seq.next_element::<IgnoredAny>()?.is_some() {}
            Ok(first)
        }
    }
    deserializer.deserialize_seq(First(PhantomData))
}

fn interest(boundary: &MultiPolygon<f64>) -> Option<Area> {
    let half = Angle::HALF_TURN.get::<degree>();
    boundary.bounding_rect().map(|rect| {
        let (west, east) = if rect.min().x.total_cmp(&-half).is_eq() && rect.max().x.total_cmp(&half).is_eq() {
            boundary.iter().filter_map(BoundingRect::bounding_rect).fold(
                (half, -half),
                |(low, high), part| {
                    if part.center().x >= 0.0 { (low.min(part.min().x), high) } else { (low, high.max(part.max().x)) }
                },
            )
        } else {
            (rect.min().x, rect.max().x)
        };
        Area::new(west, rect.min().y, east, rect.max().y)
    })
}

fn extent([west, south, east, north]: [f64; 4]) -> Vec<Rect<f64>> {
    let span = |left: f64, right: f64| Rect::new(coord! { x: left, y: south }, coord! { x: right, y: north });
    let half = Angle::HALF_TURN.get::<degree>();
    if west > east { vec![span(west, half), span(-half, east)] } else { vec![span(west, east)] }
}
