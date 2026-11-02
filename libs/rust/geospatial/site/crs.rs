use std::borrow::Cow;

use geo::{BoundingRect, Destination as _, Geodesic};
use geo_types::{Coord, MultiPolygon, Polygon, Rect, coord};
use nalgebra::Vector2;
use proj::{Area, Proj, ProjError, Transform, wkt::WktVersion};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Ratio},
    ratio::ratio,
};

use crate::{Latitude, Longitude, SiteError, point};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub enum Encoding {
    Wkt(#[debug(skip)] WktVersion),
    Projjson,
}

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

    pub fn factors(&self, latitude: Latitude, longitude: Longitude) -> Result<Factors, SiteError> {
        let wkt = self.proj()?.as_wkt(None, None).map_err(|source| self.failed(source))?;
        let Some((_, projected)) = wkt.split_once("CS[").and_then(|(crs, _)| crs.split_once("PROJCRS[")) else {
            return Err(self.failed(ProjError::Projection(String::from("Invalid value for an argument"))));
        };
        let metric = Self(Cow::Owned(format!(r#"PROJCRS[{projected}CS[Cartesian,2],AXIS["easting",east,LENGTHUNIT["metre",1]],AXIS["northing",north,LENGTHUNIT["metre",1]]]"#)));
        let (origin, step) = (point(latitude, longitude), 1.0);
        let mut steps = [0.0, 90.0].map(|bearing| [step, -step].map(|distance| Geodesic.destination(origin, bearing, distance).0));
        let transformation = Transformation::new(&Self::WGS84, &metric)?;
        transformation.proj.convert_array(steps.as_flattened_mut()).map_err(|source| transformation.failed(source))?;
        let [meridian, parallel] = steps.map(|[ahead, behind]: [Coord<f64>; 2]| Vector2::new(ahead.x - behind.x, ahead.y - behind.y) / (2.0 * step));
        Ok(Factors {
            meridian_convergence: Angle::new::<radian>((-meridian.x).atan2(meridian.y)),
            meridional_scale: Ratio::new::<ratio>(meridian.norm()),
            parallel_scale: Ratio::new::<ratio>(parallel.norm()),
            areal_scale: Ratio::new::<ratio>(meridian.perp(&parallel).abs()),
        })
    }

    pub fn area_of_use(&self) -> Result<Extent, SiteError> {
        match self.proj()?.area_of_use() {
            Ok((area, description)) => Ok(Extent { description, bounds: area.map(|Area { north, south, east, west }| MultiPolygon::new(bbox([west, south, east, north]))) }),
            Err(ProjError::UnknownAreaOfUse) => Ok(Extent { description: None, bounds: None }),
            Err(source) => Err(self.failed(source)),
        }
    }

    pub fn encode(&self, encoding: Encoding) -> Result<String, SiteError> {
        let proj = self.proj()?;
        match encoding {
            Encoding::Wkt(version) => proj.as_wkt(Some(version), None),
            Encoding::Projjson => proj.to_projjson(None, None, None),
        }
        .map_err(|source| self.failed(source))
    }

    fn proj(&self) -> Result<Proj, SiteError> { Proj::new(self.as_ref()).map_err(|source| SiteError::Creation { definition: self.as_ref().into(), source }) }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Projection { definition: self.as_ref().into(), source } }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct Transformation {
    proj: Proj,
    definition: Box<str>,
}

impl Transformation {
    pub fn new(from: &Crs, to: &Crs) -> Result<Self, SiteError> {
        let definition: Box<str> = format!("{from} to {to}").into();
        Proj::new_known_crs(from.as_ref(), to.as_ref(), None).map_err(|source| SiteError::Creation { definition: definition.clone(), source }).map(|proj| Self { proj, definition })
    }

    pub fn apply<G>(&self, mut geometry: G) -> Result<G, SiteError>
    where G: Transform<f64> {
        geometry.transform(&self.proj).map_err(|source| self.failed(source))?;
        Ok(geometry)
    }

    pub fn bounds<G>(&self, region: G, densify_pts: u16) -> Result<MultiPolygon<f64>, SiteError>
    where G: Into<MultiPolygon<f64>> {
        region
            .into()
            .iter()
            .filter_map(BoundingRect::bounding_rect)
            .try_fold(Vec::new(), |mut polygons, rect| {
                polygons.extend(bbox(self.proj.transform_bounds(rect.min().x, rect.min().y, rect.max().x, rect.max().y, i32::from(densify_pts))?));
                Ok::<_, ProjError>(polygons)
            })
            .map(MultiPolygon::new)
            .map_err(|source| self.failed(source))
    }

    fn failed(&self, source: ProjError) -> SiteError { SiteError::Projection { definition: self.definition.clone(), source } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn bbox([west, south, east, north]: [f64; 4]) -> Vec<Polygon<f64>> {
    let span = |left: f64, right: f64| Rect::new(coord! { x: left, y: south }, coord! { x: right, y: north }).to_polygon();
    let half = Angle::HALF_TURN.get::<degree>();
    if west > east { vec![span(west, half), span(-half, east)] } else { vec![span(west, east)] }
}
