use geo::AffineTransform;
use geo_types::{Coord, Point};
use nalgebra::{Isometry3, Matrix4, Point3, Translation3, UnitQuaternion, Vector3};
use uom::si::{
    angle::radian,
    f64::{Angle, Length, Ratio},
    length::meter,
    ratio::ratio,
};

use crate::{
    Elevation, Position, SiteError,
    crs::{Context, Crs, Factors, Transformation},
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

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Anchor {
    Grid(Point3<f64>),
    Site { position: Position, elevation: Elevation },
}

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Frame {
    #[as_ref]
    crs: Crs,
    #[as_ref]
    isometry: Isometry3<f64>,
    #[as_ref]
    scale: Vector3<f64>,
    #[as_ref]
    factors: Factors,
    #[as_ref]
    elevation: Elevation,
}

impl Frame {
    pub fn from_anchor(crs: Crs, anchor: Anchor, north: North, rotation: Angle, scale: Scale) -> Result<Self, SiteError> {
        let (unit, factors) = crs.factors()?;
        let unit = unit.get::<meter>();
        let anchor = match anchor {
            Anchor::Grid(grid) => grid,
            Anchor::Site { position, elevation } => {
                let Point(Coord { x, y }) = Transformation::new(Crs::WGS84, crs.clone(), Context::default())?.apply(position.0)?;
                Point3::new(x, y, elevation.get::<meter>() / unit)
            }
        };
        let refused = || SiteError::Anchor { anchor, rotation, scale };
        anchor.iter().all(|coordinate| coordinate.is_finite()).ok_or_else(refused)?;
        let factors = factors(anchor)?;
        let horizontal = match scale {
            Scale::Grid => 1.0,
            Scale::Ground => factors.parallel_scale.get::<ratio>() * factors.elevation_scale.get::<ratio>(),
            Scale::Stated(stated) => stated.get::<ratio>(),
        } / unit;
        let turn = match north {
            North::Grid => rotation.get::<radian>(),
            North::True => rotation.get::<radian>() + factors.meridian_convergence.get::<radian>(),
        };
        let stretch = Vector3::new(horizontal, horizontal, unit.recip());
        let elevation = Elevation::try_new(Length::new::<meter>(anchor.z * unit))
            .ok()
            .filter(|_| turn.is_finite() && stretch.iter().all(|factor| factor.is_normal() && factor.is_sign_positive()))
            .ok_or_else(refused)?;
        Ok(Self { crs, isometry: Isometry3::from_parts(Translation3::from(anchor.coords), UnitQuaternion::from_axis_angle(&Vector3::z_axis(), turn)), scale: stretch, factors, elevation })
    }

    #[must_use]
    pub fn true_north(&self) -> UnitQuaternion<f64> { UnitQuaternion::from_axis_angle(&Vector3::z_axis(), self.factors.meridian_convergence.get::<radian>()) * self.isometry.rotation.inverse() }

    #[must_use]
    pub fn matrix(&self, toward: Toward) -> Matrix4<f64> {
        match toward {
            Toward::Grid => self.isometry.to_homogeneous().prepend_nonuniform_scaling(&self.scale),
            Toward::Site => self.isometry.inverse().to_homogeneous().append_nonuniform_scaling(&self.scale.map(f64::recip)),
        }
    }

    #[must_use]
    pub fn affine(&self, toward: Toward) -> AffineTransform<f64> {
        let matrix = self.matrix(toward);
        AffineTransform::new(matrix.m11, matrix.m12, matrix.m14, matrix.m21, matrix.m22, matrix.m24)
    }
}
