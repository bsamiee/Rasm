use geo::AffineTransform;
use nalgebra::{Point3, Similarity3, UnitQuaternion, Vector3};
use uom::si::{
    angle::radian,
    f64::{Angle, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::{
    Elevation, SiteError,
    crs::{Crs, Factors},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum North {
    Grid,
    True,
}

#[derive(Debug, Clone, Copy, PartialEq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Scale {
    Grid,
    Ground,
    #[display("{}", _0.get::<ratio>())]
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
    elevation: Elevation,
}

impl Frame {
    pub fn from_anchor(crs: Crs, anchor: Point3<f64>, north: North, rotation: Angle, scale: Scale) -> Result<Self, SiteError> {
        let refused = || SiteError::Anchor { anchor, rotation, scale };
        let positive = match scale {
            Scale::Grid | Scale::Ground => true,
            Scale::Stated(stated) => stated.get::<ratio>().is_normal() && stated.is_sign_positive(),
        };
        (anchor.iter().all(|coordinate| coordinate.is_finite()) && rotation.is_finite() && positive).ok_or_else(refused)?;
        let factors = crs.factors()?(anchor)?;
        let unit = factors.grid_unit.get::<meter>();
        let stretch = match scale {
            Scale::Grid => 1.0,
            Scale::Ground => factors.parallel_scale.get::<ratio>() * factors.elevation_scale.get::<ratio>(),
            Scale::Stated(stated) => stated.get::<ratio>(),
        } / unit;
        let turn = match north {
            North::Grid => rotation.get::<radian>(),
            North::True => rotation.get::<radian>() + factors.meridian_convergence.get::<radian>(),
        };
        let elevation = Elevation::try_new(Length::new::<meter>(anchor.z * unit)).ok().filter(|_| turn.is_finite() && stretch.is_normal() && stretch.is_sign_positive()).ok_or_else(refused)?;
        Ok(Self { crs, similarity: Similarity3::new(anchor.coords, Vector3::z() * turn, stretch), factors, elevation })
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
