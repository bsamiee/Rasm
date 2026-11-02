use core::num::NonZero;

use gauss_quad::GaussLegendre;
use nalgebra::{Isometry3, Point2, Translation3, UnitQuaternion, Vector2};
use nonempty_collections::NEVec;
use serde::Deserialize;
use uom::si::{f64::Length, length::meter};
use validated::Validated;

use crate::{
    Composed, Fault, IfcError, Model, Part, Path,
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
    #[serde(rename = "CONSTANTCANT")]
    Constant,
    LinearTransition,
}

// --- [MODELS] --------------------------------------------------------------------------

// --- [STATIONS]

#[nutype::nutype(validate(predicate = |span: &Length| span.is_normal() && span.is_sign_positive()), derive(Debug, Clone, Copy, PartialEq))]
pub struct PositiveLength(Length);

#[derive(Debug, Clone, PartialEq)]
pub struct Evaluation {
    pub degree: NonZero<usize> = NonZero::<usize>::saturating_add(NonZero::<usize>::MIN, 15),
    pub rail_head_distance: Option<PositiveLength> = None,
    pub tolerance: Option<PositiveLength> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Alignment {
    horizontal: NEVec<Segment<Curve>>,
    vertical: Option<NEVec<Segment<Profile>>>,
    cant: Option<NEVec<Segment<Superelevation>>>,
    rule: GaussLegendre,
    tolerance: Option<PositiveLength>,
}

impl Alignment {
    pub fn new(model: &Model, path: &Path, evaluation: &Evaluation) -> Result<Self, NEVec<IfcError>> {
        fn decoded<T, S, U>(segments: &[&Composed], state: S, mut form: impl FnMut(&mut S, T) -> Option<U>) -> Validated<Vec<U>, IfcError>
        where T: Attribute {
            segments
                .iter()
                .filter_map(|segment| attribute::<T>(segment).transpose().map(|record| (&segment.node, record)))
                .scan(state, |state, (node, record)| {
                    Some(record.and_then(|record| form(state, record).ok_or_else(|| IfcError::Attribute { node: node.clone(), attribute: T::ID.into(), reason: Fault::Malformed })))
                })
                .collect()
        }
        let per = |change: f64, length: f64| if length > 0.0 { change / length } else { 0.0 };
        let curvature = |radius: f64| if radius == 0.0 { 0.0 } else { radius.recip() };
        let ordered = |floor: &mut Option<f64>, start: f64, length: f64| length >= 0.0 && floor.replace(start).is_none_or(|floor| floor <= start);
        let roll =
            |left: f64, right: f64| evaluation.rail_head_distance.map_or(Some(0.0), |span| Some((left - right) / span.into_inner().get::<meter>()).filter(|ratio| ratio.abs() <= 1.0).map(f64::asin));
        let alignment = model.get(path).ok_or_else(|| NEVec::new(IfcError::Missing { path: path.clone(), part: Part::Node }))?;
        let segments = alignment
            .children
            .values()
            .filter_map(|layout| attribute::<Segments>(layout).transpose())
            .collect::<Validated<Vec<_>, _>>()
            .ok()?
            .into_iter()
            .flat_map(|Segments { segments }| segments)
            .map(|Target { r#ref }| model.get(&r#ref).ok_or(IfcError::Missing { path: r#ref, part: Part::Node }))
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        let horizontal = decoded(&segments, 0.0, |reached, HorizontalSegment { start_point, start_direction, start_radius_of_curvature, end_radius_of_curvature, segment_length, geometry_type }| {
            let (entry, exit) = match geometry_type {
                Horizontal::Line => (0.0, 0.0),
                Horizontal::CircularArc => (curvature(start_radius_of_curvature), curvature(start_radius_of_curvature)),
                Horizontal::Clothoid => (curvature(start_radius_of_curvature), curvature(end_radius_of_curvature)),
            };
            let distance = *reached;
            match (start_point.as_slice(), segment_length >= 0.0) {
                (&[x, y] | &[x, y, _], true) => {
                    *reached = distance + segment_length;
                    Some(Segment { distance, length: segment_length, form: Curve { start: Point2::new(x, y), direction: start_direction, curvature: entry, rate: per(exit - entry, segment_length) } })
                }
                _ => None,
            }
        });
        let vertical = decoded(&segments, None, |floor, VerticalSegment { start_dist_along, horizontal_length, start_height, start_gradient, end_gradient, geometry_type }| {
            let inclination = start_gradient.atan();
            let exit = match geometry_type {
                Vertical::ConstantGradient => inclination,
                Vertical::CircularArc => end_gradient.atan(),
            };
            ordered(floor, start_dist_along, horizontal_length).then(|| Segment {
                distance: start_dist_along,
                length: horizontal_length,
                form: Profile { height: start_height, inclination, curvature: per(exit.sin() - inclination.sin(), horizontal_length) },
            })
        });
        let cant = decoded(&segments, None, |floor, CantSegment { start_dist_along, horizontal_length, start_cant_left, start_cant_right, end_cant_left, end_cant_right, geometry_type }| {
            let (left, right) = match geometry_type {
                Cant::Constant => (start_cant_left, start_cant_right),
                Cant::LinearTransition => (end_cant_left, end_cant_right),
            };
            let (entry, exit) = (roll(start_cant_left, start_cant_right)?, roll(left, right)?);
            let elevation = start_cant_left.midpoint(start_cant_right);
            ordered(floor, start_dist_along, horizontal_length).then(|| Segment {
                distance: start_dist_along,
                length: horizontal_length,
                form: Superelevation { elevation, slope: per(left.midpoint(right) - elevation, horizontal_length), roll: entry, twist: per(exit - entry, horizontal_length) },
            })
        });
        horizontal
            .map3(vertical, cant, |curves, profiles, superelevations| {
                NEVec::try_from_vec(curves).map(|curves| Self {
                    horizontal: curves,
                    vertical: NEVec::try_from_vec(profiles),
                    cant: NEVec::try_from_vec(superelevations),
                    rule: GaussLegendre::new(evaluation.degree),
                    tolerance: evaluation.tolerance,
                })
            })
            .ok()?
            .ok_or_else(|| NEVec::new(IfcError::Missing { path: path.clone(), part: Part::HorizontalLayout }))
    }

    #[must_use]
    pub fn length(&self) -> Length {
        let &Segment { distance, length, form: _ } = self.horizontal.last();
        Length::new::<meter>(distance + length)
    }

    #[must_use]
    pub fn station(&self, distance: Length) -> Option<Isometry3<f64>> {
        fn at<T>(layout: &NEVec<Segment<T>>, along: f64, reach: f64) -> Option<(&T, f64)> {
            let layout = layout.iter().as_slice();
            match layout.get(layout.partition_point(|segment| segment.distance <= along).saturating_sub(1)..)? {
                [Segment { distance, length: _, form }, _, ..] => Some(along - distance).filter(|run| -reach <= *run).map(|run| (form, run)),
                [Segment { distance, length, form }] => Some(along - distance).filter(|run| (-reach..=length + reach).contains(run)).map(|run| (form, run)),
                [] => None,
            }
        }
        let (along, reach) = (distance.get::<meter>(), self.tolerance.map_or_default(|tolerance| tolerance.into_inner().get::<meter>()));
        let (curve, arc) = at(&self.horizontal, along, reach)?;
        let heading = |run: f64| (0.5 * curve.rate).mul_add(run, curve.curvature).mul_add(run, curve.direction);
        let position = curve.start + Vector2::new(self.rule.integrate(0.0, arc, |run| heading(run).cos()), self.rule.integrate(0.0, arc, |run| heading(run).sin()));
        let (height, grade) = self.vertical.as_ref().map_or(Some((0.0, 0.0)), |layout| {
            at(layout, along, reach).map(|(&Profile { height, inclination, curvature }, run)| {
                let reached = curvature.mul_add(run, inclination.sin()).asin();
                (run.mul_add(inclination.midpoint(reached).tan(), height), reached.tan())
            })
        })?;
        let (elevation, slope, roll) = self.cant.as_ref().map_or(Some((0.0, 0.0, 0.0)), |layout| {
            at(layout, along, reach).map(|(&Superelevation { elevation, slope, roll, twist }, run)| (slope.mul_add(run, elevation), slope, twist.mul_add(run, roll)))
        })?;
        Some(Isometry3::from_parts(Translation3::new(position.x, position.y, height + elevation), UnitQuaternion::from_euler_angles(roll, -(grade + slope).atan(), heading(arc))))
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
    elevation: f64,
    slope: f64,
    roll: f64,
    twist: f64,
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
