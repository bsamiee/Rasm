use core::num::NonZero;

use gauss_quad::GaussLegendre;
use itertools::Itertools as _;
use nalgebra::{ComplexField as _, Isometry3, Point2, Point3, Translation3, UnitQuaternion, Vector2};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use parry3d_f64::{math::Vector, shape::Polyline};
use serde::{Deserialize, Serialize};
use uom::si::{f64::Length, length::meter};
use validated::Validated;

use crate::{
    Composed, Fault, IfcError, Model, Path, PositiveLength,
    schemas::{Attribute, Station, Target, attribute},
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

#[derive(Debug, Clone, PartialEq)]
pub struct Evaluation {
    pub degree: NonZero<usize> = NonZero::<usize>::saturating_add(NonZero::<usize>::MIN, 15),
    pub iterations: NonZero<usize> = NonZero::<usize>::saturating_add(NonZero::<usize>::MIN, 7),
    pub rail_head_distance: Option<PositiveLength> = None,
    pub tolerance: Option<PositiveLength> = None,
}

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct LinearReference {
    pub station: Length,
    pub offset_lateral: Length,
    pub offset_vertical: Length,
    pub cartesian_point: [f64; 3],
}

impl Attribute for LinearReference {
    const ID: &'static str = "bsi::ifc::linearreference";
}

#[derive(Debug, Clone, PartialEq)]
pub struct Alignment {
    start: Length,
    horizontal: NEVec<Segment<Curve>>,
    vertical: Option<NEVec<Segment<Profile>>>,
    cant: Option<NEVec<Segment<Superelevation>>>,
    rule: GaussLegendre,
    evaluation: Evaluation,
}

impl Alignment {
    pub fn new(model: &Model, path: &Path, evaluation: &Evaluation) -> Result<Self, NEVec<IfcError>> {
        fn decoded<T, U>(segments: &[&Composed], mut form: impl FnMut(Option<Segment<U>>, T) -> Result<Segment<U>, Fault>) -> Validated<Vec<Segment<U>>, IfcError>
        where
            T: Attribute,
            U: Copy,
        {
            segments
                .iter()
                .filter_map(|segment| attribute::<T>(segment).transpose().map(|record| (&segment.path, record)))
                .scan(None, |previous: &mut Option<Segment<U>>, (node, record)| {
                    let refused = |reason| IfcError::Attribute { node: node.clone(), attribute: T::ID.into(), reason };
                    Some(record.and_then(|record| {
                        let segment = form(*previous, record).map_err(refused)?;
                        (segment.length >= 0.0).ok_or_else(|| refused(Fault::Range { value: segment.length }))?;
                        *previous = Some(segment);
                        Ok(segment)
                    }))
                })
                .collect()
        }
        fn ordered<U>(segments: Vec<Segment<U>>) -> Option<NEVec<Segment<U>>> {
            segments.into_iter().sorted_by(|left, right| left.distance.total_cmp(&right.distance)).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>)
        }
        let rate = |change: f64, length: f64| if length > 0.0 { change / length } else { 0.0 };
        let curvature = |radius: f64| if radius == 0.0 { 0.0 } else { radius.recip() };
        let alignment = model.get(path).map_err(NEVec::new)?;
        let segments = alignment
            .children
            .values()
            .filter_map(|layout| attribute::<Segments>(layout).transpose())
            .collect::<Validated<Vec<_>, _>>()
            .ok()?
            .into_iter()
            .flat_map(|Segments { segments }| segments)
            .map(|Target { reference }| model.get(&reference))
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        let stations = alignment.children.values().filter_map(|child| attribute::<Station>(child).transpose()).collect::<Validated<Vec<_>, _>>();
        let horizontal = decoded(&segments, |previous, HorizontalSegment { start_point, start_direction, start_radius_of_curvature, end_radius_of_curvature, segment_length, geometry_type }| {
            let (entry, exit) = match geometry_type {
                Horizontal::Line => (0.0, 0.0),
                Horizontal::CircularArc => (curvature(start_radius_of_curvature), curvature(start_radius_of_curvature)),
                Horizontal::Clothoid => (curvature(start_radius_of_curvature), curvature(end_radius_of_curvature)),
            };
            let (&[x, y] | &[x, y, _]) = &start_point else { return Err(Fault::Partial { count: start_point.len() }) };
            Ok(Segment {
                distance: previous.map_or_default(|previous: Segment<Curve>| previous.distance + previous.length),
                length: segment_length,
                geometry: Curve { start: Point2::new(x, y), direction: start_direction, curvature: entry, rate: rate(exit - entry, segment_length) },
            })
        });
        let vertical = decoded(&segments, |_, VerticalSegment { start_dist_along, horizontal_length, start_height, start_gradient, end_gradient, geometry_type }| {
            let inclination = start_gradient.atan();
            let exit = match geometry_type {
                Vertical::ConstantGradient => inclination,
                Vertical::CircularArc => end_gradient.atan(),
            };
            Ok(Segment {
                distance: start_dist_along,
                length: horizontal_length,
                geometry: Profile { height: start_height, inclination, curvature: rate(exit.sin() - inclination.sin(), horizontal_length) },
            })
        });
        let cant = decoded(&segments, |_, CantSegment { start_dist_along, horizontal_length, start_cant_left, start_cant_right, end_cant_left, end_cant_right, geometry_type }| {
            let (left, right) = match geometry_type {
                Cant::Constant => (start_cant_left, start_cant_right),
                Cant::LinearTransition => (end_cant_left, end_cant_right),
            };
            let (entry, exit) = (start_cant_left - start_cant_right, left - right);
            let elevation = start_cant_left.midpoint(start_cant_right);
            match evaluation.rail_head_distance.map(|distance| distance.into_inner().get::<meter>()) {
                Some(distance) if entry.abs().max(exit.abs()) > distance => Err(Fault::Cant { cant: if entry.abs() > distance { entry } else { exit }, distance }),
                Some(_) | None => Ok(Segment {
                    distance: start_dist_along,
                    length: horizontal_length,
                    geometry: Superelevation { elevation, slope: rate(left.midpoint(right) - elevation, horizontal_length), cant: entry, twist: rate(exit - entry, horizontal_length) },
                }),
            }
        });
        horizontal
            .map4(vertical, cant, stations, |curves, profiles, superelevations, stations| {
                NEVec::try_from_vec(curves).map(|curves| Self {
                    start: Length::new::<meter>(stations.into_iter().map(|Station(station)| station.get::<meter>()).min_by(f64::total_cmp).unwrap_or_default()),
                    horizontal: curves,
                    vertical: ordered(profiles),
                    cant: ordered(superelevations),
                    rule: GaussLegendre::new(evaluation.degree),
                    evaluation: evaluation.clone(),
                })
            })
            .ok()?
            .ok_or_else(|| NEVec::new(IfcError::Attribute { node: path.clone(), attribute: HorizontalSegment::ID.into(), reason: Fault::Absent }))
    }

    #[must_use]
    pub fn length(&self) -> Length {
        let &Segment { distance, length, geometry: _ } = self.horizontal.last();
        Length::new::<meter>(distance + length)
    }

    #[must_use]
    pub const fn start(&self) -> Length { self.start }

    #[must_use]
    pub fn station(&self, station: Length) -> Option<Isometry3<f64>> { self.frame((station - self.start).get::<meter>()) }

    #[must_use]
    pub fn placement(&self, reference: &LinearReference) -> Option<Isometry3<f64>> {
        let &LinearReference { station, offset_lateral, offset_vertical, cartesian_point: _ } = reference;
        self.station(station).map(|frame| frame * Translation3::new(0.0, offset_lateral.get::<meter>(), offset_vertical.get::<meter>()))
    }

    #[must_use]
    pub fn projection(&self, point: Point3<f64>, chord: PositiveLength) -> Option<LinearReference> {
        let (seed, _) = self.samples(chord).min_by(|(_, left), (_, right)| {
            let distance = |frame: &Isometry3<f64>| nalgebra::distance_squared(&Point3::from(frame.translation.vector), &point);
            distance(left).total_cmp(&distance(right))
        })?;
        let along = (0..self.evaluation.iterations.get()).try_fold(seed, |along, _| self.frame(along).map(|frame| along + frame.inverse_transform_point(&point).x))?;
        let local = self.frame(along)?.inverse_transform_point(&point);
        Some(LinearReference {
            station: self.start + Length::new::<meter>(along),
            offset_lateral: Length::new::<meter>(local.y),
            offset_vertical: Length::new::<meter>(local.z),
            cartesian_point: point.coords.into(),
        })
    }

    #[must_use]
    pub fn polyline(&self, chord: PositiveLength) -> Option<Polyline> {
        let vertices = self.samples(chord).map(|(_, frame)| Vector::from(frame.translation)).collect::<Vec<_>>();
        matches!(&vertices, [_, _, ..]).then(|| Polyline::new(vertices, None))
    }

    fn samples(&self, chord: PositiveLength) -> impl Iterator<Item = (f64, Isometry3<f64>)> {
        fn ends<T>(layout: &NEVec<Segment<T>>) -> impl Iterator<Item = f64> { layout.iter().flat_map(|segment| [segment.distance, segment.distance + segment.length]) }
        let (sagitta, span) = (chord.into_inner().get::<meter>(), self.length().get::<meter>());
        let breaks = ends(&self.horizontal)
            .chain(self.vertical.iter().flat_map(ends))
            .chain(self.cant.iter().flat_map(ends))
            .filter(|along| (0.0..=span).contains(along))
            .sorted_by(f64::total_cmp)
            .dedup()
            .collect::<Vec<_>>();
        let last = breaks.last().copied();
        let interior = breaks.into_iter().tuple_windows().flat_map(move |(from, to)| {
            let (middle, half) = (from.midpoint(to), (to - from) * 0.5);
            let plan =
                at(&self.horizontal, middle, 0.0).map_or_default(|(curve, run)| curve.rate.mul_add(run - half, curve.curvature).abs().max(curve.rate.mul_add(run + half, curve.curvature).abs()));
            let profile = self.vertical.as_ref().and_then(|layout| at(layout, middle, 0.0)).map_or_default(|(profile, _)| profile.curvature.abs());
            let steps = ((to - from) / (8.0 * sagitta / plan.max(profile)).sqrt()).ceil().max(1.0).to_int_saturating::<usize>();
            (0..steps).map(move |step| nalgebra::convert::<usize, f64>(step).mul_add((to - from) / nalgebra::convert::<usize, f64>(steps), from))
        });
        interior.chain(last).filter_map(move |along| self.frame(along).map(|frame| (along, frame)))
    }

    fn frame(&self, along: f64) -> Option<Isometry3<f64>> {
        let tolerance = self.evaluation.tolerance.map_or_default(|tolerance| tolerance.into_inner().get::<meter>());
        let (curve, arc) = at(&self.horizontal, along, tolerance)?;
        let heading = |run: f64| (0.5 * curve.rate).mul_add(run, curve.curvature).mul_add(run, curve.direction);
        let displacement = if curve.rate == 0.0 {
            let turn = curve.curvature * arc * 0.5;
            let (sine, cosine) = (curve.direction + turn).sin_cos();
            Vector2::new(cosine, sine) * (arc * turn.sinc())
        } else {
            Vector2::new(self.rule.integrate(0.0, arc, |run| heading(run).cos()), self.rule.integrate(0.0, arc, |run| heading(run).sin()))
        };
        let position = curve.start + displacement;
        let (height, grade) = self.vertical.as_ref().map_or(Some((0.0, 0.0)), |layout| {
            at(layout, along, tolerance).map(|(&Profile { height, inclination, curvature }, run)| {
                let reached = curvature.mul_add(run, inclination.sin()).clamp(-1.0, 1.0).asin();
                (run.mul_add(inclination.midpoint(reached).tan(), height), reached.tan())
            })
        })?;
        let (elevation, slope, roll) = self.cant.as_ref().map_or(Some((0.0, 0.0, 0.0)), |layout| {
            at(layout, along, tolerance).map(|(&Superelevation { elevation, slope, cant, twist }, run)| {
                let roll = self.evaluation.rail_head_distance.map_or_default(|distance| (twist.mul_add(run, cant) / distance.into_inner().get::<meter>()).clamp(-1.0, 1.0).asin());
                (slope.mul_add(run, elevation), slope, roll)
            })
        })?;
        Some(Isometry3::from_parts(Translation3::new(position.x, position.y, height + elevation), UnitQuaternion::from_euler_angles(roll, -(grade + slope).atan(), heading(arc))))
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct Segment<T> {
    distance: f64,
    length: f64,
    geometry: T,
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct Curve {
    start: Point2<f64>,
    direction: f64,
    curvature: f64,
    rate: f64,
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct Profile {
    height: f64,
    inclination: f64,
    curvature: f64,
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct Superelevation {
    elevation: f64,
    slope: f64,
    cant: f64,
    twist: f64,
}

// --- [ATTRIBUTES]

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

// --- [OPERATIONS] ----------------------------------------------------------------------

fn at<T>(layout: &NEVec<Segment<T>>, along: f64, tolerance: f64) -> Option<(&T, f64)> {
    let Segment { distance, length, geometry } = layout.get(layout.partition_point(|segment| segment.distance <= along).saturating_sub(1))?;
    Some(along - distance).filter(|run| (-tolerance..=length + tolerance).contains(run)).map(|run| (geometry, run))
}
