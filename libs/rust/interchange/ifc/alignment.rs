use core::num::NonZero;

use gauss_quad::GaussLegendre;
use nalgebra::{Isometry3, Point2, Translation3, UnitQuaternion, Vector2};
use nonempty_collections::NEVec;
use serde::Deserialize;
use uom::si::{f64::Length, length::meter};
use validated::Validated;

use crate::{
    Composed, IfcError, Model, Path,
    schemas::{Attribute, attribute},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize)]
#[serde(rename_all = "UPPERCASE")]
enum Horizontal {
    Line,
    CircularArc,
    Clothoid,
}

#[derive(Deserialize)]
#[serde(rename_all = "UPPERCASE")]
enum Vertical {
    ConstantGradient,
    CircularArc,
}

#[derive(Deserialize)]
#[serde(rename_all = "UPPERCASE")]
enum Cant {
    ConstantCant,
    LinearTransition,
}

// --- [MODELS] --------------------------------------------------------------------------

// --- [STATIONS]

#[derive(Debug, Clone, PartialEq)]
pub struct Evaluation {
    pub degree: NonZero<usize> = NonZero::new(16).unwrap(),
    pub gauge: Option<Length> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Alignment {
    horizontal: NEVec<Segment<Curve>>,
    vertical: Vec<Segment<Profile>>,
    cant: Vec<Segment<Superelevation>>,
    rule: GaussLegendre,
    gauge: Option<Length>,
}

impl Alignment {
    pub fn new(model: &Model, path: &Path, evaluation: &Evaluation) -> Result<Self, NEVec<IfcError>> {
        fn decoded<T, U>(segments: &[&Composed], form: impl Fn(&Path, T) -> Result<U, IfcError>) -> Validated<Vec<U>, IfcError>
        where T: Attribute {
            segments.iter().filter_map(|segment| attribute::<T>(segment).transpose().map(|decoded| decoded.and_then(|record| form(&segment.node, record)))).collect()
        }
        let malformed = |node: &Path, id: &str| IfcError::Malformed { node: node.clone(), attribute: id.into() };
        let per = |change: f64, length: f64| if length > 0.0 { change / length } else { 0.0 };
        let curvature = |radius: f64| if radius == 0.0 { 0.0 } else { radius.recip() };
        let alignment = model.get(path).ok_or_else(|| NEVec::new(IfcError::Unknown { path: path.clone() }))?;
        let segments = alignment
            .children
            .values()
            .filter_map(|layout| attribute::<Segments>(layout).transpose())
            .collect::<Validated<Vec<_>, _>>()
            .ok()?
            .into_iter()
            .flat_map(|Segments { segments }| segments)
            .map(|Target { r#ref }| model.get(&r#ref).ok_or(IfcError::Unknown { path: r#ref }))
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        let horizontal = decoded(&segments, |node, HorizontalSegment { start_point, start_direction, start_radius_of_curvature, end_radius_of_curvature, segment_length, geometry_type }| {
            let (entry, exit) = match geometry_type {
                Horizontal::Line => (0.0, 0.0),
                Horizontal::CircularArc => (curvature(start_radius_of_curvature), curvature(start_radius_of_curvature)),
                Horizontal::Clothoid => (curvature(start_radius_of_curvature), curvature(end_radius_of_curvature)),
            };
            match (start_point.as_slice(), segment_length >= 0.0) {
                (&[x, y] | &[x, y, _], true) => Ok((segment_length, Curve { start: Point2::new(x, y), direction: start_direction, curvature: entry, rate: per(exit - entry, segment_length) })),
                _ => Err(malformed(node, HorizontalSegment::ID)),
            }
        })
        .map(|curves| {
            curves
                .into_iter()
                .scan(0.0, |reached, (length, form)| {
                    let distance = *reached;
                    *reached += length;
                    Some(Segment { distance, length, form })
                })
                .collect::<Vec<_>>()
        });
        let vertical = decoded(&segments, |node, VerticalSegment { start_dist_along, horizontal_length, start_height, start_gradient, end_gradient, geometry_type }| {
            let inclination = start_gradient.atan();
            let exit = match geometry_type {
                Vertical::ConstantGradient => inclination,
                Vertical::CircularArc => end_gradient.atan(),
            };
            (horizontal_length >= 0.0).ok_or_else(|| malformed(node, VerticalSegment::ID)).map(|()| Segment {
                distance: start_dist_along,
                length: horizontal_length,
                form: Profile { height: start_height, inclination, curvature: per(exit.sin() - inclination.sin(), horizontal_length) },
            })
        });
        let cant = decoded(&segments, |node, CantSegment { start_dist_along, horizontal_length, start_cant_left, start_cant_right, end_cant_left, end_cant_right, geometry_type }| {
            let entry = start_cant_left - start_cant_right;
            let exit = match geometry_type {
                Cant::ConstantCant => entry,
                Cant::LinearTransition => end_cant_left - end_cant_right,
            };
            (horizontal_length >= 0.0).ok_or_else(|| malformed(node, CantSegment::ID)).map(|()| Segment {
                distance: start_dist_along,
                length: horizontal_length,
                form: Superelevation { cant: entry, rate: per(exit - entry, horizontal_length) },
            })
        });
        horizontal
            .map3(vertical, cant, |curves, profiles, superelevations| {
                Some(Self { horizontal: NEVec::try_from_vec(curves)?, vertical: profiles, cant: superelevations, rule: GaussLegendre::new(evaluation.degree), gauge: evaluation.gauge })
            })
            .ok()?
            .ok_or_else(|| NEVec::new(IfcError::Layout { path: path.clone() }))
    }

    #[must_use]
    pub fn length(&self) -> Length {
        let &Segment { distance, length, form: _ } = self.horizontal.last();
        Length::new::<meter>(distance + length)
    }

    #[must_use]
    pub fn station(&self, distance: Length) -> Option<Isometry3<f64>> {
        fn at<T>(layout: &[Segment<T>], along: f64) -> Option<(&T, f64)> {
            let Segment { distance, length, form } = layout.get(layout.partition_point(|segment| segment.distance <= along).checked_sub(1)?)?;
            let run = along - distance;
            (run <= *length).then_some((form, run))
        }
        let along = distance.get::<meter>();
        let (curve, arc) = at(self.horizontal.iter().as_slice(), along)?;
        let heading = |run: f64| (0.5 * curve.rate).mul_add(run, curve.curvature).mul_add(run, curve.direction);
        let position = curve.start + Vector2::new(self.rule.integrate(0.0, arc, |run| heading(run).cos()), self.rule.integrate(0.0, arc, |run| heading(run).sin()));
        let (height, inclination) = match self.vertical.as_slice() {
            [] => Some((0.0, 0.0)),
            layout => at(layout, along).map(|(&Profile { height, inclination, curvature }, run)| {
                let reached = curvature.mul_add(run, inclination.sin()).asin();
                (run.mul_add(inclination.midpoint(reached).tan(), height), reached)
            }),
        }?;
        let roll = match (self.gauge, self.cant.as_slice()) {
            (Some(gauge), layout @ [_, ..]) => at(layout, along).map(|(&Superelevation { cant, rate }, run)| rate.mul_add(run, cant).atan2(gauge.get::<meter>()))?,
            (None, _) | (Some(_), []) => 0.0,
        };
        Some(Isometry3::from_parts(Translation3::new(position.x, position.y, height), UnitQuaternion::from_euler_angles(roll, -inclination, heading(arc))))
    }
}

#[derive(Debug, Clone, PartialEq)]
struct Segment<T> {
    distance: f64,
    length: f64,
    form: T,
}

#[derive(Debug, Clone, PartialEq)]
struct Curve {
    start: Point2<f64>,
    direction: f64,
    curvature: f64,
    rate: f64,
}

#[derive(Debug, Clone, PartialEq)]
struct Profile {
    height: f64,
    inclination: f64,
    curvature: f64,
}

#[derive(Debug, Clone, PartialEq)]
struct Superelevation {
    cant: f64,
    rate: f64,
}

// --- [ATTRIBUTES]

#[derive(Deserialize)]
struct Target {
    r#ref: Path,
}

#[derive(Deserialize)]
struct Segments {
    segments: Vec<Target>,
}

impl Attribute for Segments {
    const ID: &'static str = "bsi::ifc::alignment";
}

#[derive(Deserialize)]
#[serde(rename_all = "PascalCase")]
struct HorizontalSegment {
    start_point: Vec<f64>,
    start_direction: f64,
    start_radius_of_curvature: f64,
    end_radius_of_curvature: f64,
    segment_length: f64,
    geometry_type: Horizontal,
}

impl Attribute for HorizontalSegment {
    const ID: &'static str = "bsi::ifc::alignmenthorizontalsegment";
}

#[derive(Deserialize)]
#[serde(rename_all = "PascalCase")]
struct VerticalSegment {
    start_dist_along: f64,
    horizontal_length: f64,
    start_height: f64,
    start_gradient: f64,
    end_gradient: f64,
    geometry_type: Vertical,
}

impl Attribute for VerticalSegment {
    const ID: &'static str = "bsi::ifc::alignmentverticalsegment";
}

#[derive(Deserialize)]
#[serde(rename_all = "PascalCase")]
struct CantSegment {
    start_dist_along: f64,
    horizontal_length: f64,
    start_cant_left: f64,
    start_cant_right: f64,
    end_cant_left: f64,
    end_cant_right: f64,
    geometry_type: Cant,
}

impl Attribute for CantSegment {
    const ID: &'static str = "bsi::ifc::alignmentcantsegment";
}
