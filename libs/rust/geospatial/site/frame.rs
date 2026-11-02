use geo::AffineTransform;
use geo_types::Point;
use nalgebra::{Point3, Similarity3, UnitQuaternion, Vector3};
use uom::si::{
    angle::{degree, radian},
    f64::{Angle, Ratio},
    ratio::ratio,
};

use crate::{
    Latitude, LatitudeError, Longitude, LongitudeError, SiteError,
    crs::{Crs, Factors, Transformation},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum North {
    Grid,
    True,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Toward {
    Grid,
    Site,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Frame {
    crs: Crs,
    similarity: Similarity3<f64>,
    factors: Factors,
}

impl Frame {
    pub fn from_anchor(crs: Crs, anchor: Point3<f64>, north: North, rotation: Angle, scale: Ratio) -> Result<Self, SiteError> {
        let refused = || SiteError::Anchor { anchor, rotation, scale };
        (anchor.iter().all(|coordinate| coordinate.is_finite()) && rotation.is_finite() && scale.get::<ratio>().is_normal() && scale.get::<ratio>().is_sign_positive()).ok_or_else(refused)?;
        let geographic = Transformation::new(&crs, &Crs::WGS84)?.apply(Point::new(anchor.x, anchor.y))?;
        let factors = match (Latitude::try_new(Angle::new::<degree>(geographic.y())), Longitude::try_new(Angle::new::<degree>(geographic.x()))) {
            (Ok(latitude), Ok(longitude)) => crs.factors(latitude, longitude),
            (Err(LatitudeError::PredicateViolated), Ok(_) | Err(LongitudeError::PredicateViolated)) | (Ok(_), Err(LongitudeError::PredicateViolated)) => Err(refused()),
        }?;
        let turn = match north {
            North::Grid => rotation.get::<radian>(),
            North::True => rotation.get::<radian>() + factors.meridian_convergence.get::<radian>(),
        };
        Ok(Self { crs, similarity: Similarity3::from_parts(anchor.coords.into(), UnitQuaternion::from_axis_angle(&Vector3::z_axis(), turn), scale.get::<ratio>()), factors })
    }

    #[must_use]
    pub const fn crs(&self) -> &Crs { &self.crs }

    #[must_use]
    pub const fn similarity(&self) -> &Similarity3<f64> { &self.similarity }

    #[must_use]
    pub const fn factors(&self) -> Factors { self.factors }

    #[must_use]
    pub fn true_north(&self) -> UnitQuaternion<f64> {
        UnitQuaternion::from_axis_angle(&Vector3::z_axis(), self.factors.meridian_convergence.get::<radian>()) * self.similarity.isometry.rotation.inverse()
    }

    #[must_use]
    pub fn affine(&self, toward: Toward) -> AffineTransform<f64> {
        let matrix = match toward {
            Toward::Grid => self.similarity,
            Toward::Site => self.similarity.inverse(),
        }
        .to_homogeneous();
        AffineTransform::new(matrix.m11, matrix.m12, matrix.m14, matrix.m21, matrix.m22, matrix.m24)
    }
}
