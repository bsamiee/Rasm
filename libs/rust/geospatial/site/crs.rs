use core::{borrow::Borrow, iter};
use std::{borrow::Cow, path::Path};

use geo::BoundingRect;
use geo_types::{Coord, Point, Rect, coord};
use itertools::Itertools as _;
use nalgebra::{Point3, Vector2};
use proj::{Area, Proj, ProjBuilder, ProjCreateError, ProjError, Transform, wkt::WktVersion};
use projjson::{
    common::{Code, CustomUnit, Id, Measure, PredefinedUnit, Unit, UnitType, ValueAndUnit},
    crs::{self, GeodeticCrs, GeodeticCrsType},
    cs::{Axis, AxisDirection, CoordinateSystem, CoordinateSystemSubtype},
    datum::{DatumEnsemble, Ellipsoid, FlattenedEllipsoid, GeodeticDatum, Sphere, TwoAxisEllipsoid},
};
use serde::{Deserialize, Serialize, de::DeserializeOwned};
use serde_json::{Value, value::Serializer};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::{Boundary, Operation, Position, SiteError, half_turn, located};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub enum Encoding {
    Wkt(#[debug(skip)] Option<WktVersion>),
    Projjson,
}

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
            _ => (east.floor() + 180.0).rem_euclid(360.0).div_euclid(6.0).to_int_saturating::<u32>().strict_add(1),
        };
        let code = match north {
            _ if north > 84.0 => 32_661,
            ..-80.0 => 32_761,
            ..0.0 => 32_700_u32.strict_add(zone),
            _ => 32_600_u32.strict_add(zone),
        };
        Self(Cow::Owned(format!("EPSG:{code}")))
    }

    #[expect(clippy::type_complexity, reason = "Projection setup returns its coordinate unit and opaque evaluator")]
    pub fn factors(&self) -> Result<(Length, impl Fn(Point3<f64>) -> Result<Factors, SiteError> + use<>), SiteError> {
        let system = |subtype, [east, north]: [[&str; 2]; 2], unit| CoordinateSystem {
            schema: None,
            r#type: None,
            name: None,
            subtype,
            axis: [(east, AxisDirection::East), (north, AxisDirection::North)]
                .map(|([name, abbreviation], direction)| Axis {
                    schema: None,
                    r#type: None,
                    name: name.into(),
                    abbreviation: abbreviation.into(),
                    direction,
                    meridian: None,
                    unit: Some(Unit::Predefined(unit)),
                    minimum_value: None,
                    maximum_value: None,
                    range_meaning: None,
                    id: None,
                    ids: None,
                })
                .into(),
            id: None,
            ids: None,
        };
        let unprojected = || SiteError::Unprojected { crs: self.clone() };
        let linear = |unit: &Unit| match unit {
            Unit::Predefined(PredefinedUnit::Metre) => Some(1.0),
            Unit::Custom(CustomUnit { r#type: UnitType::LinearUnit, conversion_factor: Some(factor), .. }) => Some(f64::from(*factor)),
            Unit::Predefined(PredefinedUnit::Degree | PredefinedUnit::Unity) | Unit::Custom(CustomUnit { .. }) => None,
        };
        let metres = |measure: &Measure| match measure {
            Measure::Value(value) => Some(f64::from(*value)),
            Measure::ValueAndUnit(ValueAndUnit { value, unit }) => linear(unit).map(|unit| f64::from(*value) * unit),
        };
        let mut definition = self.projjson(&self.proj()?)?;
        let mut projected = loop {
            definition = match definition {
                crs::Crs::Projected(projected) => break *projected,
                crs::Crs::Compound(compound) => compound.components.into_iter().next().ok_or_else(unprojected)?,
                crs::Crs::Bound(bound) => bound.source_crs,
                crs::Crs::Geodetic(_)
                | crs::Crs::Vertical(_)
                | crs::Crs::Engineering(_)
                | crs::Crs::Parametric(_)
                | crs::Crs::Temporal(_)
                | crs::Crs::DerivedGeodetic(_)
                | crs::Crs::DerivedProjected(_)
                | crs::Crs::DerivedVertical(_)
                | crs::Crs::DerivedEngineering(_)
                | crs::Crs::DerivedParametric(_)
                | crs::Crs::DerivedTemporal(_) => return Err(unprojected()),
            };
        };
        let ellipsoid = match (&projected.base_crs.datum, &projected.base_crs.datum_ensemble) {
            (Some(GeodeticDatum::GeodeticReferenceFrame(datum)), _) => &datum.ellipsoid,
            (Some(GeodeticDatum::DynamicGeodeticReferenceFrame(datum)), _) => &datum.ellipsoid,
            (None, Some(DatumEnsemble { ellipsoid: Some(ellipsoid), .. })) => ellipsoid,
            (None, None | Some(DatumEnsemble { ellipsoid: None, .. })) => return Err(unprojected()),
        };
        let (major, flattening) = match ellipsoid {
            Ellipsoid::InverseFlattening(FlattenedEllipsoid { semi_major_axis, inverse_flattening, .. }) => metres(semi_major_axis).map(|major| (major, f64::from(*inverse_flattening).recip())),
            Ellipsoid::SemiMinorAxis(TwoAxisEllipsoid { semi_major_axis, semi_minor_axis, .. }) =>
                metres(semi_major_axis).zip(metres(semi_minor_axis)).map(|(major, minor)| (major, 1.0 - minor / major)),
            Ellipsoid::Sphere(Sphere { radius, .. }) => metres(radius).map(|radius| (radius, 0.0)),
        }
        .ok_or_else(unprojected)?;
        let grid_unit = projected.coordinate_system.axis.first().and_then(|axis| axis.unit.as_ref()).and_then(linear).ok_or_else(unprojected)?;
        projected.base_crs = GeodeticCrs {
            r#type: Some(GeodeticCrsType::GeographicCrs),
            coordinate_system: Some(system(CoordinateSystemSubtype::Ellipsoidal, [["Longitude", "lon"], ["Latitude", "lat"]], PredefinedUnit::Degree)),
            ..projected.base_crs
        };
        let geographic = serde_path_to_error::serialize(&projected.base_crs, Serializer).map_err(self.converted())?.to_string();
        projected.coordinate_system = system(CoordinateSystemSubtype::Cartesian, [["Easting", "E"], ["Northing", "N"]], PredefinedUnit::Metre);
        let metric = serde_path_to_error::serialize(&projected, Serializer).map_err(self.converted())?.to_string();
        let inverse = Proj::new_known_crs(self.as_ref(), &geographic, None).map_err(self.refused())?;
        let forward = Proj::new_known_crs(&geographic, &metric, None).map_err(self.refused())?;
        let (crs, eccentricity_squared, step) = (self.clone(), flattening * (2.0 - flattening), Angle::new::<radian>(1.0e-5));
        let (offset, run) = (step.get::<degree>(), 4.0 * step.get::<radian>());
        let grid_unit = Length::new::<meter>(grid_unit);
        Ok((grid_unit, move |grid: Point3<f64>| {
            let position = located(inverse.convert(Point::new(grid.x, grid.y)).map_err(crs.failed())?).ok_or_else(|| SiteError::Domain { crs: crs.clone(), grid })?;
            let phi = position.0.y().clamp(offset - 90.0, 90.0 - offset);
            let mut corners = [(1.0, 1.0), (1.0, -1.0), (-1.0, -1.0), (-1.0, 1.0)].map(|(east, north)| coord! { x: offset.mul_add(east, position.0.x()), y: offset.mul_add(north, phi) });
            forward.convert_array(&mut corners).map_err(crs.failed())?;
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
                elevation_scale: Ratio::new::<ratio>(radius / grid.z.mul_add(grid_unit.get::<meter>(), radius)),
                grid_unit,
            })
        }))
    }

    pub fn area_of_use(&self) -> Result<Extent, SiteError> {
        match self.proj()?.area_of_use() {
            Ok((area, description)) => Ok(Extent { description, bounds: area.into_iter().flat_map(|Area { north, south, east, west }| rects([west, south, east, north])).collect() }),
            Err(ProjError::UnknownAreaOfUse) => Ok(Extent { description: None, bounds: Vec::new() }),
            Err(source) => Err(self.failed()(source)),
        }
    }

    pub fn encode(&self, encoding: Encoding) -> Result<String, SiteError> {
        let proj = self.proj()?;
        match encoding {
            Encoding::Wkt(version) => proj.as_wkt(version, None),
            Encoding::Projjson => proj.to_projjson(None, None, None),
        }
        .map_err(self.failed())
    }

    fn proj(&self) -> Result<Proj, SiteError> { Proj::new(self.as_ref()).map_err(self.refused()) }

    fn projjson<T>(&self, proj: &Proj) -> Result<T, SiteError>
    where T: DeserializeOwned {
        serde_path_to_error::deserialize(&mut serde_json::Deserializer::from_str(&proj.to_projjson(None, None, None).map_err(self.failed())?)).map_err(self.converted())
    }

    fn refused(&self) -> impl Fn(ProjCreateError) -> SiteError { move |source| SiteError::Create { operation: Operation::Crs(self.clone()), source } }

    fn failed(&self) -> impl Fn(ProjError) -> SiteError { move |source| SiteError::Proj { operation: Operation::Crs(self.clone()), source } }

    fn converted(&self) -> impl Fn(serde_path_to_error::Error<serde_json::Error>) -> SiteError { move |source| SiteError::Projjson { crs: self.clone(), source } }
}

impl TryFrom<Cow<'static, str>> for Crs {
    type Error = SiteError;

    fn try_from(value: Cow<'static, str>) -> Result<Self, SiteError> {
        #[derive(Deserialize)]
        struct Identified {
            id: Option<Id>,
        }

        let crs = Self(value);
        let proj = crs.proj()?;
        proj.create_crs_to_crs_from_pj(&proj, None, None).map_err(crs.refused())?;
        let described: Value = crs.projjson(&proj)?;
        let Identified { id } = serde_path_to_error::deserialize(&described).map_err(crs.converted())?;
        let identical = |code: &Self| *code != crs && code.proj().and_then(|registered| code.projjson::<Value>(&registered)).is_ok_and(|other| other == described);
        Ok(id
            .map(|Id { authority, code, .. }| {
                let code = match code {
                    Code::Integer(code) => code.to_string(),
                    Code::String(code) => code,
                };
                Self(Cow::Owned(format!("{authority}:{code}")))
            })
            .filter(identical)
            .unwrap_or(crs))
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Transformation {
    proj: Proj,
    from: Crs,
    to: Crs,
}

impl Transformation {
    pub fn new(from: Crs, to: Crs, context: Context<'_>) -> Result<Self, SiteError> {
        let builder = context.search_path.into_iter().try_fold(ProjBuilder::new(), |mut builder, path| {
            builder.set_search_paths(path).map(|()| builder).map_err(|source| SiteError::Proj { operation: Operation::Search(path.to_path_buf()), source })
        })?;
        let proj = builder
            .proj_known_crs(from.as_ref(), to.as_ref(), context.area)
            .map_err(|source| SiteError::Create { operation: Operation::Transformation { from: from.clone(), to: to.clone() }, source })?;
        Ok(Self { proj, from, to })
    }

    pub fn apply<G>(&self, mut geometry: G) -> Result<G, SiteError>
    where G: Transform<f64> {
        geometry.transform(&self.proj).map(|()| geometry).map_err(|source| self.failed(source))
    }

    pub fn bounds<I>(&self, region: I, densify_points: i32) -> Result<Vec<Rect<f64>>, SiteError>
    where I: IntoIterator<Item: Borrow<Rect<f64>>> {
        region
            .into_iter()
            .map(|rect| {
                let (min, max) = (rect.borrow().min(), rect.borrow().max());
                self.proj.transform_bounds(min.x, min.y, max.x, max.y, densify_points).map(rects)
            })
            .process_results(|parts| parts.flatten().collect())
            .map_err(|source| self.failed(source))
    }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Proj { operation: Operation::Transformation { from: self.from.clone(), to: self.to.clone() }, source } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl From<&Boundary> for Area {
    fn from(value: &Boundary) -> Self {
        let half = half_turn();
        let whole = value.extent;
        let split = whole.min().x.total_cmp(&-half).is_eq() && whole.max().x.total_cmp(&half).is_eq();
        let (west, east) = if split {
            value
                .iter()
                .filter_map(BoundingRect::bounding_rect)
                .fold((half, -half), |(west, east), rect| if rect.center().x >= 0.0 { (west.min(rect.min().x), east) } else { (west, east.max(rect.max().x)) })
        } else {
            (whole.min().x, whole.max().x)
        };
        Self { west, south: whole.min().y, east, north: whole.max().y }
    }
}

fn rects([west, south, east, north]: [f64; 4]) -> impl Iterator<Item = Rect<f64>> {
    let span = |left: f64, right: f64| Rect::new(coord! { x: left, y: south }, coord! { x: right, y: north });
    let split = west > east;
    iter::once(span(west, if split { half_turn() } else { east })).chain(split.then(|| span(-half_turn(), east)))
}
