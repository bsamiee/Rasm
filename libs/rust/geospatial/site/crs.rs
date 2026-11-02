use std::{borrow::Cow, path::Path};

use geo::BoundingRect;
use geo_types::{Coord, MultiPolygon, Point, Polygon, Rect, coord};
use nalgebra::Vector2;
use proj::{Area, Proj, ProjBuilder, ProjError, Transform};
use serde::{Deserialize, Deserializer, Serialize, de::Error as _};
use serde_json::{Map, Value, value::Serializer};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Ratio},
    ratio::ratio,
};

use crate::{Latitude, Longitude, SiteError, coordinates};

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

#[derive(Clone, Copy, Serialize)]
#[serde(rename_all = "lowercase")]
enum Unit {
    Degree,
    Metre,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Factors {
    pub meridian_convergence: Angle,
    pub meridional_scale: Ratio,
    pub parallel_scale: Ratio,
    pub areal_scale: Ratio,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Extent {
    pub description: Option<String>,
    pub bounds: Option<MultiPolygon<f64>>,
}

#[derive(Debug, Clone, Copy, Default)]
pub struct Context<'path> {
    pub area: Option<Rect<f64>>,
    pub search_path: Option<&'path Path>,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, derive_more::Display, derive_more::AsRef)]
#[as_ref(str)]
pub struct Crs(Cow<'static, str>);

impl Crs {
    pub const WGS84: Self = Self(Cow::Borrowed("EPSG:4326"));

    pub fn new(definition: Cow<'static, str>) -> Result<Self, SiteError> {
        match Proj::new_known_crs(&definition, Self::WGS84.as_ref(), None) {
            Ok(_) => Ok(Self(definition)),
            Err(source) => Err(SiteError::Creation { definition: definition.into(), source }),
        }
    }

    #[must_use]
    pub fn universal(latitude: Latitude, longitude: Longitude) -> Self {
        let (north, east) = (latitude.get::<degree>(), longitude.get::<degree>());
        let zone = nalgebra::convert_unchecked::<f64, u32>((east + 180.0).rem_euclid(360.0).div_euclid(6.0)).strict_add(1);
        let code = match (north, east) {
            (84.0.., _) => 32_661,
            (..-80.0, _) => 32_761,
            (56.0..64.0, 3.0..12.0) => 32_632,
            (72.0.., 0.0..9.0) => 32_631,
            (72.0.., 9.0..21.0) => 32_633,
            (72.0.., 21.0..33.0) => 32_635,
            (72.0.., 33.0..42.0) => 32_637,
            (..0.0, _) => 32_700_u32.strict_add(zone),
            _ => 32_600_u32.strict_add(zone),
        };
        Self(Cow::Owned(format!("EPSG:{code}")))
    }

    pub fn factors(&self, grid: Point<f64>) -> Result<Factors, SiteError> {
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
        fn encoded<T>(value: &T) -> Result<Value, serde_path_to_error::Error<serde_json::Error>>
        where T: Serialize {
            serde_path_to_error::serialize(value, Serializer)
        }
        fn projected(definition: Definition) -> Projected {
            match definition {
                Definition::Projected(projected) => *projected,
                Definition::Compound { components: horizontal } | Definition::Bound { source_crs: horizontal } => projected(*horizontal),
            }
        }
        let decoded = |source| SiteError::Decode { definition: self.as_ref().into(), source };
        let created = |source| SiteError::Creation { definition: self.as_ref().into(), source };
        let metres = |measure| match measure {
            Measure::Metres(value) => value,
            Measure::Scaled { value, unit: Linear { conversion_factor } } => value * conversion_factor,
        };
        let Projected { base_crs, own } =
            serde_path_to_error::deserialize::<_, Definition>(&mut serde_json::Deserializer::from_str(&self.encode(Encoding::Projjson)?)).map_err(decoded).map(projected)?;
        let (major, flattening) = match serde_path_to_error::deserialize::<_, Geodetic>(&base_crs.rest).map_err(decoded)?.datum.ellipsoid {
            Ellipsoid::Flattened { semi_major_axis, inverse_flattening } => (metres(semi_major_axis), inverse_flattening.recip()),
            Ellipsoid::Axes { semi_major_axis, semi_minor_axis } => (metres(semi_major_axis), 1.0 - metres(semi_minor_axis) / metres(semi_major_axis)),
            Ellipsoid::Sphere { radius } => (metres(radius), 0.0),
        };
        let base = Projjson { coordinate_system: encoded(&GEOGRAPHIC).map_err(decoded)?, ..base_crs };
        let geographic = encoded(&base).map_err(decoded)?.to_string();
        let own = Projjson { coordinate_system: encoded(&METRIC).map_err(decoded)?, ..own };
        let projected = encoded(&Definition::Projected(Box::new(Projected { base_crs: base, own }))).map_err(decoded)?.to_string();
        let (latitude, longitude) = coordinates(Proj::new_known_crs(self.as_ref(), &geographic, None).map_err(created)?.convert(grid).map_err(|source| self.failed(source))?)?;
        let step = Angle::new::<radian>(1.0e-5);
        let phi = latitude.get::<degree>().clamp(step.get::<degree>() - 90.0, 90.0 - step.get::<degree>());
        let mut corners = [(1.0, 1.0), (1.0, -1.0), (-1.0, -1.0), (-1.0, 1.0)]
            .map(|(east, north)| coord! { x: step.get::<degree>().mul_add(east, longitude.get::<degree>()), y: step.get::<degree>().mul_add(north, phi) });
        Proj::new_known_crs(&geographic, &projected, None).map_err(created)?.convert_array(&mut corners).map_err(|source| self.failed(source))?;
        let [ahead, right, behind, left] = corners.map(|corner: Coord<f64>| Vector2::new(corner.x, corner.y));
        let (parallel, meridian) = ((ahead + right - behind - left) / (4.0 * step.get::<radian>()), (ahead - right - behind + left) / (4.0 * step.get::<radian>()));
        let eccentricity_squared = flattening * (2.0 - flattening);
        let (sin, cos) = Angle::new::<degree>(phi).get::<radian>().sin_cos();
        let curvature = (eccentricity_squared * sin).mul_add(-sin, 1.0).recip();
        let normal = major * curvature.sqrt();
        let meridional = normal * (1.0 - eccentricity_squared) * curvature;
        Ok(Factors {
            meridian_convergence: Angle::new::<radian>((-meridian.x).atan2(meridian.y)),
            meridional_scale: Ratio::new::<ratio>(meridian.norm() / meridional),
            parallel_scale: Ratio::new::<ratio>(parallel.norm() / (normal * cos)),
            areal_scale: Ratio::new::<ratio>(parallel.perp(&meridian) / (meridional * normal * cos)),
        })
    }

    pub fn area_of_use(&self) -> Result<Extent, SiteError> {
        match self.proj()?.area_of_use() {
            Ok((area, description)) => Ok(Extent { description, bounds: area.map(|Area { north, south, east, west }| MultiPolygon::new(extent([west, south, east, north]))) }),
            Err(ProjError::UnknownAreaOfUse) => Ok(Extent { description: None, bounds: None }),
            Err(source) => Err(self.failed(source)),
        }
    }

    pub fn encode(&self, encoding: Encoding) -> Result<String, SiteError> {
        let proj = self.proj()?;
        match encoding {
            Encoding::Wkt => proj.as_wkt(None, None),
            Encoding::Projjson => proj.to_projjson(None, None, None),
        }
        .map_err(|source| self.failed(source))
    }

    fn proj(&self) -> Result<Proj, SiteError> { Proj::new(self.as_ref()).map_err(|source| SiteError::Creation { definition: self.as_ref().into(), source }) }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Projection { definition: self.as_ref().into(), source } }
}

#[derive(Deserialize, Serialize)]
#[serde(tag = "type")]
enum Definition {
    #[serde(rename = "ProjectedCRS")]
    Projected(Box<Projected>),
    #[serde(rename = "CompoundCRS", skip_serializing)]
    Compound {
        #[serde(deserialize_with = "horizontal")]
        components: Box<Self>,
    },
    #[serde(rename = "BoundCRS", skip_serializing)]
    Bound { source_crs: Box<Self> },
}

#[derive(Deserialize, Serialize)]
struct Projected {
    base_crs: Projjson,
    #[serde(flatten)]
    own: Projjson,
}

#[derive(Deserialize, Serialize)]
struct Projjson {
    coordinate_system: Value,
    #[serde(flatten)]
    rest: Map<String, Value>,
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

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Transformation {
    proj: Proj,
    definition: Box<str>,
}

impl Transformation {
    pub fn new(from: &Crs, to: &Crs, context: Context<'_>) -> Result<Self, SiteError> {
        let definition: Box<str> = format!("{from} to {to}").into();
        let builder = context
            .search_path
            .into_iter()
            .try_fold(ProjBuilder::new(), |mut builder, path| builder.set_search_paths(path).map(|()| builder))
            .map_err(|source| SiteError::Projection { definition: definition.clone(), source })?;
        let area = context.area.map(|rect| Area::new(rect.min().x, rect.min().y, rect.max().x, rect.max().y));
        builder.proj_known_crs(from.as_ref(), to.as_ref(), area).map_err(|source| SiteError::Creation { definition: definition.clone(), source }).map(|proj| Self { proj, definition })
    }

    pub fn apply<G>(&self, mut geometry: G) -> Result<G, SiteError>
    where G: Transform<f64> {
        geometry.transform(&self.proj).map_err(|source| self.failed(source))?;
        Ok(geometry)
    }

    pub fn bounds<G>(&self, region: G) -> Result<MultiPolygon<f64>, SiteError>
    where G: Into<MultiPolygon<f64>> {
        region
            .into()
            .iter()
            .filter_map(BoundingRect::bounding_rect)
            .try_fold(Vec::new(), |mut polygons, rect| {
                polygons.extend(extent(self.proj.transform_bounds(rect.min().x, rect.min().y, rect.max().x, rect.max().y, 21)?));
                Ok::<_, ProjError>(polygons)
            })
            .map(MultiPolygon::new)
            .map_err(|source| self.failed(source))
    }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Projection { definition: self.definition.clone(), source } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn horizontal<'de, D>(deserializer: D) -> Result<Box<Definition>, D::Error>
where D: Deserializer<'de> {
    match &Vec::<Value>::deserialize(deserializer)? {
        [first, ..] => serde_path_to_error::deserialize(first).map_err(D::Error::custom),
        [] => Err(D::Error::invalid_length(0, &"a horizontal component")),
    }
}

fn extent([west, south, east, north]: [f64; 4]) -> Vec<Polygon<f64>> {
    let span = |left: f64, right: f64| Rect::new(coord! { x: left, y: south }, coord! { x: right, y: north }).to_polygon();
    let half = Angle::HALF_TURN.get::<degree>();
    if west > east { vec![span(west, half), span(-half, east)] } else { vec![span(west, east)] }
}
