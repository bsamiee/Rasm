use core::f64::consts::{PI, TAU};
use core::num::NonZero;

use gauss_quad::GaussLegendre;
use itertools::Itertools as _;
use nalgebra::{ComplexField as _, Isometry3, Point2, Point3, Rotation2, Translation3, UnitQuaternion, Vector2};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
#[cfg(feature = "geometry")]
use parry3d_f64::{math::Vector, shape::Polyline};
use serde::{Deserialize, Serialize};
use uom::si::f64::Length;
use uom::si::length::meter;
use validated::Validated;

use crate::schemas::{Attribute, Station, Target, attribute};
use crate::{Composed, Fault, IfcError, Model, Path, PositiveLength};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Deserialize)]
#[serde(rename_all = "UPPERCASE")]
enum Horizontal {
    Line,
    CircularArc,
    Clothoid,
    Cubic,
    HelmertCurve,
    BlossCurve,
    CosineCurve,
    SineCurve,
    VienneseBend,
    #[serde(other)]
    Unknown,
}

#[derive(Deserialize)]
#[serde(rename_all = "UPPERCASE")]
enum Vertical {
    ConstantGradient,
    CircularArc,
    ParabolicArc,
    Clothoid,
    #[serde(other)]
    Unknown,
}

#[derive(Deserialize)]
#[serde(rename_all = "UPPERCASE")]
enum Cant {
    #[serde(rename = "CONSTANTCANT")]
    Constant,
    LinearTransition,
    HelmertCurve,
    BlossCurve,
    CosineCurve,
    SineCurve,
    VienneseBend,
    #[serde(other)]
    Unknown,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Transition {
    Constant,
    Linear,
    Helmert,
    Bloss,
    Cosine,
    Sine,
    Viennese,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Plan {
    Spiral(Transition),
    Cubic,
}

// --- [MODELS] --------------------------------------------------------------------------

// --- [STATIONS]

#[derive(Debug, Clone, PartialEq)]
pub struct Evaluation {
    pub degree: NonZero<usize> = NonZero::new(16).unwrap(),
    pub iterations: NonZero<usize> = NonZero::new(8).unwrap(),
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
    pub fn new(model: &Model, path: &Path, evaluation: &Evaluation) -> Result<(Self, Vec<IfcError>), NEVec<IfcError>> {
        fn records<'model, T>(segments: &[&'model Composed]) -> Validated<Vec<(&'model Path, T)>, IfcError>
        where T: Attribute {
            segments.iter().filter_map(|segment| attribute::<T>(segment).transpose().map(|record| record.map(|record| (&segment.path, record)))).collect()
        }
        fn placed<T, U>(node: &Path, distance: f64, length: f64, geometry: Result<U, Fault>) -> Result<Segment<U>, IfcError>
        where T: Attribute {
            (length >= 0.0).ok_or(Fault::Range { value: length }).and(geometry).map(|geometry| Segment { distance, length, geometry }).map_err(|reason| IfcError::Attribute {
                node: node.clone(),
                attribute: T::ID.into(),
                reason,
            })
        }
        fn ordered<U>(segments: Vec<Segment<U>>) -> Option<NEVec<Segment<U>>> {
            segments.into_iter().sorted_by(|left, right| left.distance.total_cmp(&right.distance)).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>)
        }
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
        let (horizontals, verticals, cants, stations) = records::<HorizontalSegment>(&segments)
            .map4(records::<VerticalSegment>(&segments), records::<CantSegment>(&segments), stations, |horizontals, verticals, cants, stations| (horizontals, verticals, cants, stations))
            .ok()?;
        let tolerance = evaluation.tolerance.map_or_default(|tolerance| tolerance.into_inner().get::<meter>());
        let (cant, refused_cant): (Vec<_>, Vec<_>) = cants
            .into_iter()
            .map(|(node, CantSegment { start_dist_along, horizontal_length, start_cant_left, start_cant_right, end_cant_left, end_cant_right, geometry_type })| {
                let transition = match geometry_type {
                    Cant::Constant => Ok(Transition::Constant),
                    Cant::LinearTransition => Ok(Transition::Linear),
                    Cant::HelmertCurve => Ok(Transition::Helmert),
                    Cant::BlossCurve => Ok(Transition::Bloss),
                    Cant::CosineCurve => Ok(Transition::Cosine),
                    Cant::SineCurve => Ok(Transition::Sine),
                    Cant::VienneseBend => Ok(Transition::Viennese),
                    Cant::Unknown => Err(Fault::Unknown),
                };
                let geometry = transition.and_then(|transition| {
                    let [_, reached, _] = progress(transition, 1.0);
                    let (entry, change) = (start_cant_left - start_cant_right, (end_cant_left - end_cant_right) - (start_cant_left - start_cant_right));
                    let exit = change.mul_add(reached, entry);
                    match evaluation.rail_head_distance.map(|distance| distance.into_inner().get::<meter>()) {
                        Some(distance) if entry.abs().max(exit.abs()) > distance => Err(Fault::Cant { cant: if entry.abs() > distance { entry } else { exit }, distance }),
                        Some(_) | None => Ok(Superelevation {
                            elevation: start_cant_left.midpoint(start_cant_right),
                            cant: entry,
                            rise: end_cant_left.midpoint(end_cant_right) - start_cant_left.midpoint(start_cant_right),
                            twist: change,
                            transition,
                        }),
                    }
                });
                placed::<CantSegment, _>(node, start_dist_along, horizontal_length, geometry)
            })
            .partition_result();
        let cant = ordered(cant);
        let (vertical, refused_vertical): (Vec<_>, Vec<_>) = verticals
            .into_iter()
            .map(|(node, VerticalSegment { start_dist_along, horizontal_length, start_height, start_gradient, end_gradient, geometry_type })| {
                let geometry = match geometry_type {
                    Vertical::ConstantGradient => Ok(Profile::Parabola { height: start_height, gradient: start_gradient, curvature: 0.0 }),
                    Vertical::ParabolicArc => Ok(Profile::Parabola { height: start_height, gradient: start_gradient, curvature: rate(end_gradient - start_gradient, horizontal_length) }),
                    Vertical::CircularArc => {
                        let inclination = start_gradient.atan();
                        Ok(Profile::Arc { height: start_height, inclination, curvature: rate(end_gradient.atan().sin() - inclination.sin(), horizontal_length) })
                    }
                    Vertical::Clothoid => Err(Fault::Underdetermined),
                    Vertical::Unknown => Err(Fault::Unknown),
                };
                placed::<VerticalSegment, _>(node, start_dist_along, horizontal_length, geometry)
            })
            .partition_result();
        let (curves, refused_horizontal): (Vec<_>, Vec<_>) = horizontals
            .into_iter()
            .scan(0.0, |reached: &mut f64, (node, record)| {
                let distance = *reached;
                *reached += record.segment_length;
                Some((node, distance, record))
            })
            .map(
                |(
                    node,
                    distance,
                    HorizontalSegment { start_point, start_direction, start_radius_of_curvature, end_radius_of_curvature, segment_length, gravity_center_line_height, geometry_type },
                )| {
                    let (entry, exit) = (curvature(start_radius_of_curvature), curvature(end_radius_of_curvature));
                    let tilt = |along: f64| {
                        let distance = evaluation.rail_head_distance?.into_inner().get::<meter>();
                        let [_, _, cant] = superelevation(cant.as_ref()?, along, tolerance)?;
                        Some((cant / distance).clamp(-1.0, 1.0).asin())
                    };
                    let shape = match (geometry_type, gravity_center_line_height) {
                        (Horizontal::Line, _) => Ok((Plan::Spiral(Transition::Constant), 0.0, 0.0, 0.0)),
                        (Horizontal::CircularArc, _) => Ok((Plan::Spiral(Transition::Constant), entry, 0.0, 0.0)),
                        (Horizontal::Clothoid, _) => Ok((Plan::Spiral(Transition::Linear), entry, exit - entry, 0.0)),
                        (Horizontal::Cubic, _) => Ok((Plan::Cubic, entry, exit - entry, 0.0)),
                        (Horizontal::HelmertCurve, _) => Ok((Plan::Spiral(Transition::Helmert), entry, exit - entry, 0.0)),
                        (Horizontal::BlossCurve, _) => Ok((Plan::Spiral(Transition::Bloss), entry, exit - entry, 0.0)),
                        (Horizontal::CosineCurve, _) => Ok((Plan::Spiral(Transition::Cosine), entry, exit - entry, 0.0)),
                        (Horizontal::SineCurve, _) => Ok((Plan::Spiral(Transition::Sine), entry, exit - entry, 0.0)),
                        (Horizontal::VienneseBend, None) => Err(Fault::Underdetermined),
                        (Horizontal::VienneseBend, Some(0.0)) => Ok((Plan::Spiral(Transition::Viennese), entry, exit - entry, 0.0)),
                        (Horizontal::VienneseBend, Some(height)) => tilt(distance + segment_length)
                            .zip(tilt(distance))
                            .map(|(last, first)| (Plan::Spiral(Transition::Viennese), entry, exit - entry, rate(rate(-420.0 * height * (last - first), segment_length), segment_length)))
                            .ok_or(Fault::Underdetermined),
                        (Horizontal::Unknown, _) => Err(Fault::Unknown),
                    };
                    let geometry = shape.and_then(|(plan, entry, change, lean)| match &start_point {
                        &[x, y] | &[x, y, _] => Ok(Curve { start: Point2::new(x, y), direction: start_direction, entry, change, lean, plan }),
                        _ => Err(Fault::Partial { count: start_point.len() }),
                    });
                    placed::<HorizontalSegment, _>(node, distance, segment_length, geometry)
                },
            )
            .partition_result();
        let failures = [refused_horizontal, refused_vertical, refused_cant].into_iter().flatten().collect::<Vec<_>>();
        let Some(horizontal) = NEVec::try_from_vec(curves) else {
            return Err(NEVec::try_from_vec(failures).unwrap_or_else(|| NEVec::new(IfcError::Attribute { node: path.clone(), attribute: HorizontalSegment::ID.into(), reason: Fault::Absent })));
        };
        Ok((
            Self {
                start: Length::new::<meter>(stations.into_iter().map(|Station(station)| station.get::<meter>()).min_by(f64::total_cmp).unwrap_or_default()),
                horizontal,
                vertical: ordered(vertical),
                cant,
                rule: GaussLegendre::new(evaluation.degree),
                evaluation: evaluation.clone(),
            },
            failures,
        ))
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

    #[cfg(feature = "geometry")]
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
        let bend = |segment: &Segment<Curve>, run: f64| {
            let Segment { distance: _, length, geometry: Curve { start: _, direction: _, entry, change, lean, plan } } = *segment;
            let ratio = rate(run, length);
            match plan {
                Plan::Spiral(transition) => {
                    let ([_, shape, _], [_, leaned]) = (progress(transition, ratio), gravity(ratio));
                    change.mul_add(shape, lean.mul_add(leaned, entry))
                }
                Plan::Cubic => change.mul_add(ratio, entry),
            }
        };
        let interior = breaks.into_iter().tuple_windows().flat_map(move |(from, to)| {
            let (middle, half) = (from.midpoint(to), (to - from) * 0.5);
            let plan = at(&self.horizontal, middle, 0.0).map_or_default(|(segment, run)| [run - half, run, run + half].into_iter().map(|along| bend(segment, along).abs()).fold(0.0, f64::max));
            let profile = self.vertical.as_ref().and_then(|layout| at(layout, middle, 0.0)).map_or_default(|(segment, _)| match segment.geometry {
                Profile::Arc { height: _, inclination: _, curvature } | Profile::Parabola { height: _, gradient: _, curvature } => curvature.abs(),
            });
            let steps = ((to - from) / (8.0 * sagitta / plan.max(profile)).sqrt()).ceil().max(1.0).to_int_saturating::<usize>();
            (0..steps).map(move |step| nalgebra::convert::<usize, f64>(step).mul_add((to - from) / nalgebra::convert::<usize, f64>(steps), from))
        });
        interior.chain(last).filter_map(move |along| self.frame(along).map(|frame| (along, frame)))
    }

    fn frame(&self, along: f64) -> Option<Isometry3<f64>> {
        let tolerance = self.evaluation.tolerance.map_or_default(|tolerance| tolerance.into_inner().get::<meter>());
        let (segment, run) = at(&self.horizontal, along, tolerance)?;
        let Segment { distance: _, length, geometry: Curve { start, direction, entry, change, lean, plan } } = *segment;
        let heading = |run: f64| {
            let ratio = rate(run, length);
            direction
                + match plan {
                    Plan::Spiral(transition) => {
                        let ([swept, _, _], [leaned, _]) = (progress(transition, ratio), gravity(ratio));
                        entry.mul_add(run, change.mul_add(swept, lean * leaned) * length)
                    }
                    Plan::Cubic => ((0.5 * rate(change, length)).mul_add(run, entry) * run).atan(),
                }
        };
        let displacement = match plan {
            Plan::Spiral(_) if change == 0.0 && lean == 0.0 => {
                let swept = entry * run * 0.5;
                let (sine, cosine) = (direction + swept).sin_cos();
                Vector2::new(cosine, sine) * (run * swept.sinc())
            }
            Plan::Spiral(_) => Vector2::new(self.rule.integrate(0.0, run, |at| heading(at).cos()), self.rule.integrate(0.0, run, |at| heading(at).sin())),
            Plan::Cubic => Rotation2::new(direction) * Vector2::new(run, (rate(change, length) / 6.0).mul_add(run, entry * 0.5) * run * run),
        };
        let position = start + displacement;
        let (height, grade) = self.vertical.as_ref().map_or(Some((0.0, 0.0)), |layout| {
            let (segment, run) = at(layout, along, tolerance)?;
            Some(match segment.geometry {
                Profile::Arc { height, inclination, curvature } => {
                    let reached = curvature.mul_add(run, inclination.sin()).clamp(-1.0, 1.0).asin();
                    (run.mul_add(inclination.midpoint(reached).tan(), height), reached.tan())
                }
                Profile::Parabola { height, gradient, curvature } => ((0.5 * curvature).mul_add(run, gradient).mul_add(run, height), curvature.mul_add(run, gradient)),
            })
        })?;
        let [elevation, slope, cant] = self.cant.as_ref().map_or(Some([0.0; 3]), |layout| superelevation(layout, along, tolerance))?;
        let roll = self.evaluation.rail_head_distance.map_or_default(|distance| (cant / distance.into_inner().get::<meter>()).clamp(-1.0, 1.0).asin());
        Some(Isometry3::from_parts(Translation3::new(position.x, position.y, height + elevation), UnitQuaternion::from_euler_angles(roll, -(grade + slope).atan(), heading(run))))
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
    entry: f64,
    change: f64,
    lean: f64,
    plan: Plan,
}

#[derive(Debug, Clone, Copy, PartialEq)]
enum Profile {
    Arc { height: f64, inclination: f64, curvature: f64 },
    Parabola { height: f64, gradient: f64, curvature: f64 },
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct Superelevation {
    elevation: f64,
    cant: f64,
    rise: f64,
    twist: f64,
    transition: Transition,
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
    #[serde(default)]
    gravity_center_line_height: Option<f64>,
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

// --- [LAYOUTS]

fn at<T>(layout: &NEVec<Segment<T>>, along: f64, tolerance: f64) -> Option<(&Segment<T>, f64)> {
    let segment = layout.get(layout.partition_point(|segment| segment.distance <= along).saturating_sub(1))?;
    Some(along - segment.distance).filter(|run| (-tolerance..=segment.length + tolerance).contains(run)).map(|run| (segment, run))
}

fn rate(change: f64, length: f64) -> f64 { if length > 0.0 { change / length } else { 0.0 } }

fn superelevation(layout: &NEVec<Segment<Superelevation>>, along: f64, tolerance: f64) -> Option<[f64; 3]> {
    let (&Segment { distance: _, length, geometry: Superelevation { elevation, cant, rise, twist, transition } }, run) = at(layout, along, tolerance)?;
    let [_, shape, slope] = progress(transition, rate(run, length));
    Some([rise.mul_add(shape, elevation), rate(rise * slope, length), twist.mul_add(shape, cant)])
}

// --- [CURVATURE]

fn progress(transition: Transition, ratio: f64) -> [f64; 3] {
    let rest = 1.0 - ratio;
    match transition {
        Transition::Constant => [0.0; 3],
        Transition::Linear => [0.5 * ratio * ratio, ratio, 1.0],
        Transition::Helmert if ratio <= 0.5 => [2.0 / 3.0 * ratio.powi(3), 2.0 * ratio * ratio, 4.0 * ratio],
        Transition::Helmert => [(2.0 / 3.0 * rest).mul_add(rest * rest, ratio - 0.5), (-2.0 * rest).mul_add(rest, 1.0), 4.0 * rest],
        Transition::Bloss => [(-0.5 * ratio).mul_add(ratio.powi(3), ratio.powi(3)), (-2.0 * ratio).mul_add(ratio * ratio, 3.0 * ratio * ratio), 6.0 * ratio * rest],
        Transition::Cosine => [0.5_f64.mul_add(ratio, -((PI * ratio).sin() / TAU)), 0.5 * (1.0 - (PI * ratio).cos()), 0.5 * PI * (PI * ratio).sin()],
        Transition::Sine => [(0.5 * ratio).mul_add(ratio, ((TAU * ratio).cos() - 1.0) / (TAU * TAU)), ratio - (TAU * ratio).sin() / TAU, 1.0 - (TAU * ratio).cos()],
        Transition::Viennese => [
            ratio.powi(5) * ratio.mul_add(ratio.mul_add((-2.5_f64).mul_add(ratio, 10.0), -14.0), 7.0),
            ratio.powi(4) * ratio.mul_add(ratio.mul_add((-20.0_f64).mul_add(ratio, 70.0), -84.0), 35.0),
            140.0 * (ratio * rest).powi(3),
        ],
    }
}

fn gravity(ratio: f64) -> [f64; 2] {
    [ratio.powi(3) * ratio.mul_add(ratio.mul_add((-1.0_f64 / 3.0).mul_add(ratio, 1.0), -1.0), 1.0 / 3.0), ratio * ratio * ratio.mul_add(ratio.mul_add((-2.0_f64).mul_add(ratio, 5.0), -4.0), 1.0)]
}
