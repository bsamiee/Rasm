use std::{borrow::Cow, collections::BTreeMap, sync::Arc};

use color::{OpaqueColor, Srgb};
use fontdb::Family;
use itertools::Itertools as _;
use kurbo::{Affine, Axis, BezPath, Cap, Join, PathEl, Point, Rect, Shape as _, Vec2};
use linesweeper::{BinaryOp, FillRule};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use parlance::{FontFamily, FontFamilyName, GenericFamily};
use rasm_linework::{
    Alignment, Anchor, Baseline, Color, Content, Dash, DashOffset, DashOffsetError, Dashes, DashesError, Element, FontSize, FontSizeError, FontStyle, FontWeight, FontWeightError, FontWidth, Linetype,
    LineworkError, MiterLimit, MiterLimitError, Opacity, Path, Pen, Placement, Region, Side, Target, Text, Tolerance, ToleranceError, Width, WidthError, regions, strokes,
};
use stacksafe::StackSafe;
use uom::{
    ConstZero,
    si::{
        angle::radian,
        f64::{Angle, Length},
        length::inch,
    },
};
use usvg::{
    AlignmentBaseline, BaselineShift, BlendMode, ClipPath, DominantBaseline, FontStretch, Group, ImageHrefResolver, ImageKind, LineCap, LineJoin, Node, Options, Paint, PaintOrder, TextAnchor,
    TextChunk, TextFlow, Transform, Tree, WritingMode,
    tiny_skia_path::{self, PathSegment},
};
use validated::Validated;

use crate::{
    AnnotationError, Feature, Graphics, Lettering, Lookup, Subject, colored, directed, filled, located, oriented,
    text::{self, Label, Typesetter, labelled},
    transformed, units, validated, within,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum Base {
    Center,
    Origin,
    Id(Arc<str>),
}

#[derive(Debug, Clone, PartialEq)]
struct Slot {
    lettering: Lettering,
    content: Arc<str>,
    basis: Affine,
    anchor: Anchor,
    baseline: Baseline,
    fit: Option<Width>,
}

struct Lowered {
    elements: Vec<Element>,
    slots: BTreeMap<Arc<str>, Slot>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Definition {
    elements: Vec<Element>,
    slots: BTreeMap<Arc<str>, Slot>,
    pub(crate) extent: Rect,
}

impl Definition {
    pub fn new(typesetter: &mut Typesetter, svg: &[u8], base: &Base, tolerance: Tolerance) -> Result<Self, NEVec<AnnotationError>> {
        fn lowered(group: &Group, basis: Affine, tolerance: Tolerance, typesetter: &mut Typesetter) -> Validated<Lowered, AnnotationError> {
            let basis = basis * affine(group.transform());
            let shaped = |path: &usvg::Path| -> Validated<Vec<Element>, AnnotationError> {
                let local = outline(path);
                let fill = path
                    .fill()
                    .map(|fill| Validated::from(painted(fill.paint(), fill.opacity()).and_then(|(color, opacity)| filled(basis * &local, fill_rule(fill.rule()), color, opacity, tolerance))));
                let stroke = path.stroke().map(|stroke| traced(stroke, &local, basis, tolerance));
                match path.paint_order() {
                    PaintOrder::FillAndStroke => [fill, stroke],
                    PaintOrder::StrokeAndFill => [stroke, fill],
                }
                .into_iter()
                .flatten()
                .collect()
            };
            let refusals = refused(&[
                (group.mask().is_some(), Feature::Mask),
                (!group.filters().is_empty(), Feature::Filter),
                (group.blend_mode() != BlendMode::Normal, Feature::Blend),
                (group.opacity() < usvg::Opacity::ONE, Feature::Opacity),
            ]);
            let parts = group
                .children()
                .iter()
                .filter(|node| match node {
                    Node::Path(path) => path.is_visible(),
                    Node::Image(image) => image.is_visible(),
                    Node::Group(_) | Node::Text(_) => true,
                })
                .map(|node| match node {
                    Node::Group(inner) => lowered(inner, basis, tolerance, typesetter),
                    Node::Path(path) => shaped(path).map(|elements| Lowered { elements, slots: BTreeMap::new() }),
                    Node::Image(image) => match image.kind() {
                        ImageKind::SVG(tree) => lowered(tree.root(), basis, tolerance, typesetter),
                        ImageKind::JPEG(bytes) | ImageKind::PNG(bytes) | ImageKind::GIF(bytes) | ImageKind::WEBP(bytes) => Validated::from(
                            Placement::try_new(basis * Affine::map_unit_square(Rect::new(0.0, 0.0, f64::from(image.size().width()), f64::from(image.size().height()))))
                                .map(|placement| Lowered { elements: vec![Element::Placed { content: Content::Raster(Arc::from(bytes.as_slice())), placement, .. }], slots: BTreeMap::new() })
                                .map_err(AnnotationError::from),
                        ),
                    },
                    Node::Text(text) => match slotted(text, basis, typesetter) {
                        Some(slot) => slot.map(|named| Lowered { elements: Vec::new(), slots: BTreeMap::from([named]) }),
                        None => lowered(text.flattened(), basis, tolerance, typesetter),
                    },
                })
                .collect::<Validated<Vec<_>, _>>();
            refusals.map3(parts, validated(clipped(group.clip_path(), basis, tolerance)), |(), children, clip| {
                let (elements, slots): (Vec<_>, Vec<_>) = children.into_iter().map(|Lowered { elements, slots }| (elements, slots)).unzip();
                let drawn = elements.into_iter().flatten().collect();
                Lowered {
                    elements: match clip {
                        Some(region) => vec![Element::Clip { region, keep: Side::Inside, elements: StackSafe::new(drawn) }],
                        None => drawn,
                    },
                    slots: slots.into_iter().flatten().collect(),
                }
            })
        }

        fn slotted(text: &usvg::Text, basis: Affine, typesetter: &mut Typesetter) -> Option<Validated<(Arc<str>, Slot), AnnotationError>> {
            let (false, [chunk, rest @ ..]) = (text.id().is_empty(), text.chunks()) else { return None };
            let [span, ..] = chunk.spans() else { return None };
            let (Some(fill), true) = (span.fill(), span.is_visible()) else { return None };
            let scale = basis.determinant().abs().sqrt();
            let spans = text.chunks().iter().flat_map(|part| part.spans().iter().map(move |piece| (part, piece)));
            let beyond_first = |shifts: &[f32]| shifts.iter().skip(1).any(|shift| *shift != 0.0);
            let pitches = text.chunks().array_windows().map(|[above, below]| above.y().zip(below.y()).map(|(top, bottom)| bottom - top)).collect::<Option<Vec<_>>>();
            let stroked = spans.clone().any(|(_, piece)| piece.stroke().is_some());
            let styled = spans.clone().any(|(_, piece)| piece.small_caps() || !piece.apply_kerning() || !piece.font().variations().is_empty());
            let vertical = text.writing_mode() == WritingMode::TopToBottom;
            let rotated = text.rotate().iter().any(|rotation| *rotation != 0.0);
            let shifted = beyond_first(text.dx()) || beyond_first(text.dy());
            let pathed = text.chunks().iter().any(|part| matches!(part.text_flow(), TextFlow::Path(_)));
            let spaced = spans.clone().any(|(_, piece)| piece.letter_spacing() != 0.0 || piece.word_spacing() != 0.0);
            let decorated = spans.clone().any(|(_, piece)| {
                let decoration = piece.decoration();
                [decoration.underline(), decoration.overline(), decoration.line_through()].iter().any(Option::is_some) || piece.baseline_shift().iter().any(|shift| *shift != BaselineShift::Baseline)
            });
            let mixed = !spans
                .clone()
                .map(|(part, piece)| {
                    let paint = piece.fill().map(|filling| {
                        let solid = match filling.paint() {
                            Paint::Color(color) => Some(*color),
                            Paint::LinearGradient(_) | Paint::RadialGradient(_) | Paint::Pattern(_) => None,
                        };
                        (solid, filling.opacity())
                    });
                    (part.anchor(), piece.font(), piece.font_size(), piece.dominant_baseline(), piece.alignment_baseline(), paint, piece.is_visible(), piece.text_length())
                })
                .all_equal();
            let unstacked = rest.iter().any(|part| part.x() != chunk.x()) || pitches.as_ref().is_none_or(|steps| !steps.iter().all_equal() || steps.iter().any(|step| *step <= 0.0));
            let refusals = refused(&[(stroked, Feature::Stroke), (styled, Feature::Font), (vertical || rotated || shifted || pathed || spaced || decorated || mixed || unstacked, Feature::Layout)]);
            let font = span.font();
            let origin = Vec2::new(
                chunk.x().map_or_default(f64::from) + text.dx().first().copied().map_or_default(f64::from),
                chunk.y().map_or_default(f64::from) + text.dy().first().copied().map_or_default(f64::from),
            );
            let em = f64::from(span.font_size().get()) * scale;
            let stretch = match font.stretch() {
                FontStretch::UltraCondensed => const { FontWidth::try_new(parlance::FontWidth::ULTRA_CONDENSED.ratio()).ok().unwrap() },
                FontStretch::ExtraCondensed => const { FontWidth::try_new(parlance::FontWidth::EXTRA_CONDENSED.ratio()).ok().unwrap() },
                FontStretch::Condensed => const { FontWidth::try_new(parlance::FontWidth::CONDENSED.ratio()).ok().unwrap() },
                FontStretch::SemiCondensed => const { FontWidth::try_new(parlance::FontWidth::SEMI_CONDENSED.ratio()).ok().unwrap() },
                FontStretch::Normal => FontWidth::NORMAL,
                FontStretch::SemiExpanded => const { FontWidth::try_new(parlance::FontWidth::SEMI_EXPANDED.ratio()).ok().unwrap() },
                FontStretch::Expanded => const { FontWidth::try_new(parlance::FontWidth::EXPANDED.ratio()).ok().unwrap() },
                FontStretch::ExtraExpanded => const { FontWidth::try_new(parlance::FontWidth::EXTRA_EXPANDED.ratio()).ok().unwrap() },
                FontStretch::UltraExpanded => const { FontWidth::try_new(parlance::FontWidth::ULTRA_EXPANDED.ratio()).ok().unwrap() },
            };
            let family = FontFamily::List(Cow::Owned(
                font.families()
                    .iter()
                    .map(|family| match family {
                        usvg::FontFamily::Serif => GenericFamily::Serif.into(),
                        usvg::FontFamily::SansSerif => GenericFamily::SansSerif.into(),
                        usvg::FontFamily::Cursive => GenericFamily::Cursive.into(),
                        usvg::FontFamily::Fantasy => GenericFamily::Fantasy.into(),
                        usvg::FontFamily::Monospace => GenericFamily::Monospace.into(),
                        usvg::FontFamily::Named(name) => FontFamilyName::Named(Cow::Owned(name.clone())),
                    })
                    .collect(),
            ));
            let slant = match font.style() {
                usvg::FontStyle::Normal => FontStyle::Normal,
                usvg::FontStyle::Italic => FontStyle::Italic,
                usvg::FontStyle::Oblique => FontStyle::Oblique { angle: None },
            };
            let anchor = match chunk.anchor() {
                TextAnchor::Start => Anchor::Start,
                TextAnchor::Middle => Anchor::Middle,
                TextAnchor::End => Anchor::End,
            };
            let baseline = match (span.alignment_baseline(), span.dominant_baseline()) {
                (AlignmentBaseline::BeforeEdge | AlignmentBaseline::TextBeforeEdge | AlignmentBaseline::Hanging, _)
                | (AlignmentBaseline::Auto | AlignmentBaseline::Baseline, DominantBaseline::Hanging | DominantBaseline::TextBeforeEdge) => Baseline::Top,
                (AlignmentBaseline::Middle | AlignmentBaseline::Central | AlignmentBaseline::Mathematical, _)
                | (AlignmentBaseline::Auto | AlignmentBaseline::Baseline, DominantBaseline::Middle | DominantBaseline::Central | DominantBaseline::Mathematical) => Baseline::Middle,
                (AlignmentBaseline::AfterEdge | AlignmentBaseline::TextAfterEdge | AlignmentBaseline::Ideographic, _)
                | (AlignmentBaseline::Auto | AlignmentBaseline::Baseline, DominantBaseline::Ideographic | DominantBaseline::TextAfterEdge) => Baseline::Bottom,
                (AlignmentBaseline::Alphabetic, _)
                | (
                    AlignmentBaseline::Auto | AlignmentBaseline::Baseline,
                    DominantBaseline::Auto | DominantBaseline::UseScript | DominantBaseline::NoChange | DominantBaseline::ResetSize | DominantBaseline::Alphabetic,
                ) => Baseline::Alphabetic,
            };
            let content = Arc::<str>::from(text.chunks().iter().map(TextChunk::text).join("\n"));
            let distance = |value: f32| width(f64::from(value) * scale, Feature::Layout);
            let spacing = Validated::from(pitches.as_ref().and_then(|steps| steps.first().copied()).map(distance).transpose())
                .map2(Validated::from(span.text_length().map(distance).transpose()), |line_height, fit| (line_height, fit));
            let sized = Validated::from(
                FontWeight::try_new(f32::from(font.weight()))
                    .map_err(|(FontWeightError::FiniteViolated | FontWeightError::GreaterOrEqualViolated | FontWeightError::LessOrEqualViolated)| AnnotationError::Unsupported(Feature::Font)),
            )
            .map2(Validated::from(font_size(em)), |weight, size| (weight, size))
            .ok()
            .and_then(|(weight, size)| {
                let (_, ratio) =
                    text::em(typesetter, &Text { content: Arc::clone(&content), family: family.clone(), size, placement: Placement::default(), weight, width: stretch, style: slant, .. })?;
                font_size(em / ratio).map(|capital| (weight, capital)).map_err(NEVec::from)
            });
            Some(refusals.map4(validated(sized), Validated::from(painted(fill.paint(), fill.opacity())), spacing, |(), (weight, size), (color, opacity), (line_height, fit)| {
                (
                    Arc::from(text.id()),
                    Slot {
                        lettering: Lettering { family, size, weight, width: stretch, style: slant, line_height, features: Vec::new(), color, opacity },
                        content,
                        basis: basis * Affine::translate(origin) * Affine::scale(scale.recip()),
                        anchor,
                        baseline,
                        fit,
                    },
                )
            }))
        }

        let options = Options {
            font_family: typesetter.database.family_name(&Family::Serif).to_owned(),
            image_href_resolver: ImageHrefResolver { resolve_data: ImageHrefResolver::default_data_resolver(), resolve_string: Box::new(|_, _| None) },
            fontdb: Arc::clone(&typesetter.database),
            ..Options::default()
        };
        let tree = Tree::from_data(svg, &options).map_err(AnnotationError::from)?;
        let size = tree.size();
        let canvas = Rect::new(0.0, 0.0, f64::from(size.width()), f64::from(size.height()));
        let origin = match base {
            Base::Center => canvas.center(),
            Base::Origin => Point::ORIGIN,
            Base::Id(id) => tree
                .node_by_id(id)
                .map(|node| {
                    let bounds = node.abs_bounding_box();
                    Point::new(f64::from(bounds.left()).midpoint(f64::from(bounds.right())), f64::from(bounds.top()).midpoint(f64::from(bounds.bottom())))
                })
                .ok_or_else(|| AnnotationError::Unknown { lookup: Lookup::Element, name: Arc::clone(id) })?,
        };
        let paper = Affine::scale(units(Length::new::<inch>(1.0)) / f64::from(options.dpi)).pre_translate(-origin.to_vec2());
        lowered(tree.root(), paper, tolerance, typesetter).map(|Lowered { elements, slots }| Self { elements, slots, extent: paper.transform_rect_bbox(canvas) }).ok()
    }

    pub fn slots(&self) -> impl Iterator<Item = (&Arc<str>, &Arc<str>)> { self.slots.iter().map(|(name, slot)| (name, &slot.content)) }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Instance {
    pub definition: Arc<Definition>,
    pub at: Point,
    pub angle: Angle = ConstZero::ZERO,
    pub scale: f64 = 1.0,
    pub mirror: Option<Axis> = None,
    pub target: Target = Target::Drafting,
    pub slots: BTreeMap<Arc<str>, Arc<str>> = BTreeMap::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, instance: &Instance) -> Result<Graphics, NEVec<AnnotationError>> {
    let Instance { definition, at, angle, scale, mirror, target, slots } = instance;
    let reflection = match mirror {
        None => Affine::IDENTITY,
        Some(Axis::Horizontal) => Affine::FLIP_Y,
        Some(Axis::Vertical) => Affine::FLIP_X,
    };
    let positioned = match target {
        Target::Drafting => {
            let (point, direction) = located(placement, *at, *angle);
            oriented(point, direction)
        }
        Target::Model => *placement * Affine::translate(at.to_vec2()) * Affine::rotate(angle.get::<radian>()),
    };
    let basis = positioned * Affine::scale(*scale) * reflection;
    let factor = basis.determinant().abs().sqrt();
    let scaled = |distance: Width| width(distance.into_inner() * factor, Feature::Font);
    let mut filled_slot = |Slot { lettering, content: authored, basis: local, anchor, baseline, fit }: &Slot, content: Option<&Arc<str>>| -> Result<Graphics, NEVec<AnnotationError>> {
        let label = Validated::from(font_size(lettering.size.into_inner() * factor))
            .map3(Validated::from(lettering.line_height.map(scaled).transpose()), Validated::from(fit.map(scaled).transpose()), |size, line_height, fit| Label {
                content: Arc::clone(content.unwrap_or(authored)),
                lettering: Some(Lettering { size, line_height, ..lettering.clone() }),
                fit,
                ..
            })
            .ok()?;
        let anchored = basis * *local;
        labelled(typesetter, style, &label, anchored * Point::ORIGIN, directed(anchored, Vec2::new(1.0, 0.0)).normalize(), *anchor, *baseline)
    };
    let unknown = slots
        .keys()
        .filter(|name| !definition.slots.contains_key(*name))
        .map(|name| Err::<(), _>(AnnotationError::Unknown { lookup: Lookup::Slot, name: Arc::clone(name) }))
        .collect::<Validated<(), _>>();
    let placed = Region::new(definition.extent, FillRule::NonZero, style.tolerance)
        .map_err(|error| NEVec::from(AnnotationError::from(error)))
        .and_then(|mask| transformed(Graphics { elements: definition.elements.clone(), masks: vec![mask] }, basis));
    let set = definition.slots.iter().map(|(name, slot)| within(&Subject::Slot(Arc::clone(name)), filled_slot(slot, slots.get(name)))).collect::<Validated<Graphics, _>>();
    unknown.map3(validated(placed), set, |(), drawn, texts| [drawn, texts].into_iter().collect()).ok()
}

fn traced(stroke: &usvg::Stroke, local: &BezPath, basis: Affine, tolerance: Tolerance) -> Validated<Element, AnnotationError> {
    let magnitude = basis.determinant().abs();
    let scale = magnitude.sqrt();
    let major = basis.spectral_norm();
    let minor = magnitude / major;
    let unsupported = || AnnotationError::Unsupported(Feature::Stroke);
    let cap = match stroke.linecap() {
        LineCap::Butt => Cap::Butt,
        LineCap::Round => Cap::Round,
        LineCap::Square => Cap::Square,
    };
    let join = match stroke.linejoin() {
        LineJoin::Miter | LineJoin::MiterClip => Join::Miter,
        LineJoin::Round => Join::Round,
        LineJoin::Bevel => Join::Bevel,
    };
    let line_width = f64::from(stroke.width().get());
    let dashing = stroke.dasharray().map(|dasharray| (dasharray.iter().map(|dash| f64::from(*dash)).collect::<Vec<_>>(), f64::from(stroke.dashoffset())));
    let pen = |factor: f64| {
        Validated::from(width(line_width * factor, Feature::Stroke)).map3(
            Validated::from(painted(stroke.paint(), stroke.opacity())),
            Validated::from(MiterLimit::try_new(f64::from(stroke.miterlimit().get())).map_err(|(MiterLimitError::FiniteViolated | MiterLimitError::GreaterOrEqualViolated)| unsupported())),
            |width, (color, opacity), miter_limit| Pen { width, color, opacity, cap, join, miter_limit },
        )
    };
    let linetype = |factor: f64| {
        dashing
            .as_ref()
            .and_then(|(lengths, offset)| lengths.iter().map(|length| length * factor).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).map(|scaled| (scaled, offset * factor)))
            .map_or(Validated::Good(None), |(lengths, offset)| {
                Validated::from(Dashes::try_new(lengths).map_err(|DashesError::PredicateViolated| AnnotationError::from(LineworkError::Scaled(Dash::Array))))
                    .map2(Validated::from(DashOffset::try_new(offset).map_err(|DashOffsetError::FiniteViolated| AnnotationError::from(LineworkError::Scaled(Dash::Offset)))), |dashes, dash_offset| {
                        Some(Linetype { dashes, alignment: Alignment::Start { dash_offset }, target: Target::Drafting })
                    })
            })
    };
    let reach = dashing.as_ref().map_or(line_width * 0.5, |_| local.perimeter(tolerance.into_inner() / major).max(line_width * 0.5));
    if (major - minor) * reach > tolerance.into_inner() {
        validated(
            pen(1.0)
                .map3(
                    linetype(1.0),
                    Validated::from(Tolerance::try_new(tolerance.into_inner() / major).map_err(|(ToleranceError::FiniteViolated | ToleranceError::GreaterViolated)| unsupported())),
                    |pen, linetype, accuracy| (pen, linetype, accuracy),
                )
                .ok()
                .and_then(|(pen, linetype, accuracy)| {
                    filled(basis * strokes::outline(local, &pen, linetype.as_ref(), accuracy), FillRule::NonZero, pen.color, pen.opacity, tolerance).map_err(NEVec::from)
                }),
        )
    } else {
        pen(scale).map3(linetype(scale), Validated::from(Path::try_new(basis * local).map_err(|error| AnnotationError::from(LineworkError::from(error)))), |pen, linetype, path| Element::Stroke {
            path,
            pen,
            linetype,
        })
    }
}

fn refused(refusals: &[(bool, Feature)]) -> Validated<(), AnnotationError> {
    refusals.iter().filter_map(|&(present, feature)| present.then_some(feature)).unique().map(|feature| Err::<(), _>(AnnotationError::Unsupported(feature))).collect()
}

fn width(value: f64, feature: Feature) -> Result<Width, AnnotationError> {
    Width::try_new(value).map_err(|(WidthError::FiniteViolated | WidthError::GreaterOrEqualViolated)| AnnotationError::Unsupported(feature))
}

fn font_size(value: f64) -> Result<FontSize, AnnotationError> {
    FontSize::try_new(value).map_err(|(FontSizeError::FiniteViolated | FontSizeError::GreaterViolated)| AnnotationError::Unsupported(Feature::Font))
}

fn painted(paint: &Paint, opacity: usvg::Opacity) -> Result<(Color, Opacity), AnnotationError> {
    match paint {
        Paint::Color(usvg::Color { red, green, blue }) => colored(OpaqueColor::<Srgb>::from_rgb8(*red, *green, *blue).with_alpha(opacity.get())),
        Paint::LinearGradient(_) | Paint::RadialGradient(_) => Err(AnnotationError::Unsupported(Feature::Gradient)),
        Paint::Pattern(_) => Err(AnnotationError::Unsupported(Feature::Pattern)),
    }
}

fn clipped(clip: Option<&ClipPath>, basis: Affine, tolerance: Tolerance) -> Result<Option<Region>, NEVec<AnnotationError>> {
    fn area(group: &Group, basis: Affine, tolerance: Tolerance) -> Validated<Region, AnnotationError> {
        let basis = basis * affine(group.transform());
        let shapes = group
            .children()
            .iter()
            .filter_map(|node| match node {
                Node::Group(inner) => Some(area(inner, basis, tolerance)),
                Node::Path(path) =>
                    path.fill().filter(|_| path.is_visible()).map(|fill| Validated::from(Region::new(basis * outline(path), fill_rule(fill.rule()), tolerance).map_err(AnnotationError::from))),
                Node::Text(text) => Some(area(text.flattened(), basis, tolerance)),
                Node::Image(_) => None,
            })
            .collect::<Validated<Vec<_>, _>>();
        let union = validated(shapes.ok().and_then(|parts| regions::boolean(&Region::default(), &parts, BinaryOp::Union, tolerance).map_err(|error| AnnotationError::from(error).into())));
        validated(bounded(union, group.clip_path(), basis, tolerance))
    }
    clip.map(|clip_path| bounded(area(clip_path.root(), basis * affine(clip_path.transform()), tolerance), clip_path.clip_path(), basis, tolerance)).transpose()
}

fn bounded(shape: Validated<Region, AnnotationError>, clip: Option<&ClipPath>, basis: Affine, tolerance: Tolerance) -> Result<Region, NEVec<AnnotationError>> {
    shape.map2(validated(clipped(clip, basis, tolerance)), |shape, bound| (shape, bound)).ok().and_then(|(shape, bound)| match bound {
        None => Ok(shape),
        Some(region) => regions::boolean(&region, [&shape], BinaryOp::Intersection, tolerance).map_err(|error| AnnotationError::from(error).into()),
    })
}

fn outline(path: &usvg::Path) -> BezPath {
    let point = |tiny_skia_path::Point { x, y }| Point::new(f64::from(x), f64::from(y));
    path.data()
        .segments()
        .map(|segment| match segment {
            PathSegment::MoveTo(to) => PathEl::MoveTo(point(to)),
            PathSegment::LineTo(to) => PathEl::LineTo(point(to)),
            PathSegment::QuadTo(control, to) => PathEl::QuadTo(point(control), point(to)),
            PathSegment::CubicTo(first, second, to) => PathEl::CurveTo(point(first), point(second), point(to)),
            PathSegment::Close => PathEl::ClosePath,
        })
        .collect()
}

fn affine(Transform { sx, kx, ky, sy, tx, ty }: Transform) -> Affine { Affine::new([sx, ky, kx, sy, tx, ty].map(f64::from)) }

const fn fill_rule(rule: usvg::FillRule) -> FillRule {
    match rule {
        usvg::FillRule::NonZero => FillRule::NonZero,
        usvg::FillRule::EvenOdd => FillRule::EvenOdd,
    }
}
