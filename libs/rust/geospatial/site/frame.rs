use geo::AffineTransform;
use nalgebra::{Point3, Similarity3, UnitQuaternion, Vector3};
use uom::si::{
    angle::radian,
    f64::{Angle, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::{
    Elevation, Place, SiteError,
    crs::{Crs, Factors},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum North {
    Grid,
    True,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Scale {
    Grid,
    Ground,
    Stated(Ratio),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Toward {
    Grid,
    Site,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Frame {
    #[as_ref]
    crs: Crs,
    #[as_ref]
    similarity: Similarity3<f64>,
    #[as_ref]
    factors: Factors,
    #[as_ref]
    place: Place,
}

impl Frame {
    pub fn from_anchor(crs: Crs, anchor: Point3<f64>, north: North, rotation: Angle, scale: Scale) -> Result<Self, SiteError> {
        let factors = crs.factors()?(anchor)?;
        let unit = factors.grid_unit.get::<meter>();
        let stretch = match scale {
            Scale::Grid => unit.recip(),
            Scale::Ground => factors.parallel_scale.get::<ratio>() * factors.elevation_scale.get::<ratio>() / unit,
            Scale::Stated(stated) => stated.get::<ratio>(),
        };
        let turn = match north {
            North::Grid => rotation.get::<radian>(),
            North::True => rotation.get::<radian>() + factors.meridian_convergence.get::<radian>(),
        };
        let elevation = Elevation::try_new(Length::new::<meter>(anchor.z * unit)).ok().filter(|_| turn.is_finite() && stretch.is_normal() && stretch.is_sign_positive()).ok_or(SiteError::Anchor {
            anchor,
            rotation,
            scale: Ratio::new::<ratio>(stretch),
        })?;
        let place = Place::try_from((factors.position, elevation))?;
        Ok(Self { crs, similarity: Similarity3::new(anchor.coords, Vector3::z() * turn, stretch), factors, place })
    }

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
