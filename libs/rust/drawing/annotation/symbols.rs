use core::iter;
use std::{borrow::Cow, collections::BTreeMap, sync::Arc};

use color::{OpaqueColor, Srgb};
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Cap, Join, PathEl, Point, Rect, Shape as _, Stroke, StrokeOpts, Vec2};
use linesweeper::{BinaryOp, FillRule};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use parlance::{FontFamily, FontFamilyName, GenericFamily};
use rasm_linework::{
    Alignment, Anchor, Baseline, Color, Content, Dash, DashOffset, DashOffsetError, Dashes, DashesError, Element, FontSize, FontSizeError, FontStyle, FontWeight, FontWeightError, FontWidth,
    FontWidthError, Linetype, LineworkError, MiterLimit, MiterLimitError, Opacity, OpacityError, Path, Pen, Placement, Region, Side, Target, Text, Tolerance, Width, WidthError, regions,
};
use uom::{
    ConstZero,
    si::{
        angle::radian,
        f64::{Angle, Length},
        length::{inch, millimeter},
    },
};
use usvg::{
    AlignmentBaseline, BaselineShift, BlendMode, ClipPath, DominantBaseline, FontStretch, Group, ImageHrefResolver, ImageKind, LineCap, LineJoin, Node, Options, Paint, PaintOrder, TextAnchor,
    TextChunk, TextFlow, Transform, Tree, WritingMode,
    fontdb::Family,
    tiny_skia_path::{self, PathSegment},
};
use validated::Validated;

use crate::{AnnotationError, Graphics, Lookup, Style, text::Typesetter};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Feature {
    Gradient,
    Pattern,
    Mask,
    Filter,
    Blend,
    Opacity,
    Stroke,
    Font,
    #[display("text layout")]
    Layout,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
struct Slot {
    name: Arc<str>,
    text: Text,
    color: Color,
    opacity: Opacity,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Definition {
    elements: Vec<Element>,
    slots: Vec<Slot>,
}

impl Definition {
    pub fn new(typesetter: &Typesetter, svg: &[u8], tolerance: Tolerance) -> Result<Self, NEVec<AnnotationError>> {
        let options = Options {
            font_family: typesetter.database.family_name(&Family::Serif).to_owned(),
            image_href_resolver: ImageHrefResolver { resolve_data: ImageHrefResolver::default_data_resolver(), resolve_string: Box::new(|_, _| None) },
            fontdb: Arc::clone(&typesetter.database),
            ..Options::default()
        };
        let tree = Tree::from_data(svg, &options).map_err(|error| nev![AnnotationError::from(error)])?;
        let size = tree.size();
        let paper = Affine::scale(Length::new::<inch>(1.0).get::<millimeter>() / f64::from(options.dpi)).pre_translate(Vec2::new(f64::from(size.width()) * -0.5, f64::from(size.height()) * -0.5));
        lowered(tree.root(), paper, tolerance).map(|(elements, slots)| Self { elements, slots }).ok()
    }

    pub fn slots(&self) -> impl Iterator<Item = (&Arc<str>, &Arc<str>)> { self.slots.iter().map(|Slot { name, text, color: _, opacity: _ }| (name, &text.content)) }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Instance {
    pub definition: Arc<Definition>,
    pub at: Point,
    pub angle: Angle = ConstZero::ZERO,
    pub slots: BTreeMap<Arc<str>, Arc<str>> = BTreeMap::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, instance: &Instance) -> Result<Graphics, NEVec<AnnotationError>> {
    let Instance { definition, at, angle, slots } = instance;
    let direction = (placement.with_translation(Vec2::ZERO) * Vec2::from_angle(angle.get::<radian>()).to_point()).to_vec2();
    let frame = Placement::try_new(Affine::rotate(direction.atan2()).then_translate((*placement * *at).to_vec2())).map_err(|error| nev![AnnotationError::from(error)])?;
    let gap = style.gap.into_inner();
    let unknown = slots
        .keys()
        .filter(|name| !definition.slots.iter().any(|slot| slot.name == **name))
        .map(|name| Err::<(), _>(AnnotationError::Unknown { lookup: Lookup::Slot, name: Arc::clone(name) }))
        .collect::<Validated<(), _>>();
    let placed = rasm_linework::transform(definition.elements.clone(), frame).map_or_else(|errors| Validated::Fail(errors.into_nonempty_iter().map(AnnotationError::from).collect()), Validated::Good);
    let set = definition
        .slots
        .iter()
        .map(|Slot { name, text, color, opacity }| -> Result<(Vec<Element>, Option<Region>), AnnotationError> {
            let located = Placement::try_new(*frame * *text.placement)?;
            let block = typesetter.typeset(&Text { content: slots.get(name).map_or_else(|| Arc::clone(&text.content), Arc::clone), placement: located, ..text.clone() })?;
            let clearance =
                (!block.runs.is_empty()).then(|| Region::new(&(*located * block.extent.inflate(gap, gap).to_path(style.tolerance.into_inner())), FillRule::NonZero, style.tolerance)).transpose()?;
            Ok((block.runs.into_iter().map(|run| Element::Glyphs { run, color: color.clone(), opacity: *opacity }).collect(), clearance))
        })
        .collect::<Validated<Vec<_>, _>>();
    let (elements, (glyphs, clearances)) = unknown.map3(placed, set, |(), elements, blocks| (elements, blocks.into_iter().unzip::<_, _, Vec<_>, Vec<_>>())).ok()?;
    let mask = regions::boolean(&Region::default(), clearances.iter().flatten(), BinaryOp::Union, style.tolerance).map_err(|error| nev![AnnotationError::from(error)])?;
    Ok(Graphics { elements: iter::chain(elements, glyphs.into_iter().flatten()).collect(), mask })
}

fn lowered(group: &Group, frame: Affine, tolerance: Tolerance) -> Validated<(Vec<Element>, Vec<Slot>), AnnotationError> {
    let frame = frame * affine(group.transform());
    let magnitude = frame.determinant().abs();
    let scale = magnitude.sqrt();
    let major = frame.spectral_norm();
    let minor = magnitude / major;
    let refused = |refusals: &[(bool, Feature)]| {
        refusals.iter().filter_map(|&(present, feature)| present.then_some(feature)).unique().map(|feature| Err::<(), _>(AnnotationError::Unsupported(feature))).collect::<Validated<(), _>>()
    };
    let colored = |paint: &Paint, alpha: usvg::Opacity| -> Result<(Color, Opacity), AnnotationError> {
        match paint {
            Paint::Color(usvg::Color { red, green, blue }) => Opacity::try_new(f64::from(alpha.get()))
                .map(|opacity| (Color::Srgb(OpaqueColor::<Srgb>::from_rgb8(*red, *green, *blue)), opacity))
                .map_err(|(OpacityError::FiniteViolated | OpacityError::GreaterOrEqualViolated | OpacityError::LessOrEqualViolated)| AnnotationError::Unsupported(Feature::Opacity)),
            Paint::LinearGradient(_) | Paint::RadialGradient(_) => Err(AnnotationError::Unsupported(Feature::Gradient)),
            Paint::Pattern(_) => Err(AnnotationError::Unsupported(Feature::Pattern)),
        }
    };
    let stroked = |stroke: &usvg::Stroke, local: &BezPath| -> Validated<Element, AnnotationError> {
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
        let width = f64::from(stroke.width().get());
        let reach = stroke.dasharray().map_or(width * 0.5, |_| local.perimeter(tolerance.into_inner() / major).max(width * 0.5));
        let paint = Validated::from(colored(stroke.paint(), stroke.opacity()));
        if (major - minor) * reach > tolerance.into_inner() {
            let outline = kurbo::stroke(
                local.iter(),
                &Stroke::new(width)
                    .with_caps(cap)
                    .with_join(join)
                    .with_miter_limit(f64::from(stroke.miterlimit().get()))
                    .with_dashes(f64::from(stroke.dashoffset()), stroke.dasharray().into_iter().flatten().copied().map(f64::from)),
                &StrokeOpts::default(),
                tolerance.into_inner() / major,
            );
            paint.map2(Validated::from(Region::new(&(frame * outline), FillRule::NonZero, tolerance).map_err(AnnotationError::from)), |(color, opacity), region| Element::Fill {
                region,
                color,
                opacity,
            })
        } else {
            let pen = paint.map3(
                Validated::from(Width::try_new(width * scale).map_err(|(WidthError::FiniteViolated | WidthError::GreaterOrEqualViolated)| AnnotationError::Unsupported(Feature::Stroke))),
                Validated::from(
                    MiterLimit::try_new(f64::from(stroke.miterlimit().get()))
                        .map_err(|(MiterLimitError::FiniteViolated | MiterLimitError::GreaterOrEqualViolated)| AnnotationError::Unsupported(Feature::Stroke)),
                ),
                |(color, opacity), width, miter_limit| Pen { width, color, opacity, cap, join, miter_limit },
            );
            let linetype = stroke.dasharray().and_then(|dasharray| NEVec::try_from_vec(dasharray.iter().map(|dash| f64::from(*dash) * scale).collect())).map_or(Validated::Good(None), |lengths| {
                Validated::from(Dashes::try_new(lengths).map_err(|DashesError::PredicateViolated| AnnotationError::from(LineworkError::Scaled(Dash::Array)))).map2(
                    Validated::from(DashOffset::try_new(f64::from(stroke.dashoffset()) * scale).map_err(|DashOffsetError::FiniteViolated| AnnotationError::from(LineworkError::Scaled(Dash::Offset)))),
                    |dashes, dash_offset| Some(Linetype { dashes, alignment: Alignment::Start { dash_offset }, target: Target::Drafting }),
                )
            });
            pen.map3(linetype, Validated::from(Path::try_new(frame * local).map_err(|error| AnnotationError::from(LineworkError::from(error)))), |pen, linetype, centerline| Element::Stroke {
                path: centerline,
                pen,
                linetype,
            })
        }
    };
    let painted = |path: &usvg::Path| -> Validated<Vec<Element>, AnnotationError> {
        let local = outline(path);
        let fill =
            path.fill().map(|fill| {
                Validated::from(colored(fill.paint(), fill.opacity()).and_then(|(color, opacity)| {
                    Region::new(&(frame * &local), fill_rule(fill.rule()), tolerance).map(|region| Element::Fill { region, color, opacity }).map_err(AnnotationError::from)
                }))
            });
        let stroke = path.stroke().map(|stroke| stroked(stroke, &local));
        match path.paint_order() {
            PaintOrder::FillAndStroke => [fill, stroke],
            PaintOrder::StrokeAndFill => [stroke, fill],
        }
        .into_iter()
        .flatten()
        .collect()
    };
    let slotted = |text: &usvg::Text| -> Option<Validated<Slot, AnnotationError>> {
        let (false, [chunk, ..]) = (text.id().is_empty(), text.chunks()) else { return None };
        let [span, ..] = chunk.spans() else { return None };
        let (Some(fill), true) = (span.fill(), span.is_visible()) else { return None };
        let spans = text.chunks().iter().flat_map(|part| part.spans().iter().map(move |piece| (part, piece)));
        let shifted = |shifts: &[f32]| shifts.iter().skip(1).any(|shift| *shift != 0.0);
        let refusals = refused(&[
            (spans.clone().any(|(_, piece)| piece.stroke().is_some()), Feature::Stroke),
            (spans.clone().any(|(_, piece)| piece.small_caps() || !piece.apply_kerning() || !piece.font().variations().is_empty()), Feature::Font),
            (
                text.writing_mode() == WritingMode::TopToBottom
                    || text.rotate().iter().any(|rotation| *rotation != 0.0)
                    || shifted(text.dx())
                    || shifted(text.dy())
                    || text.chunks().iter().any(|part| matches!(part.text_flow(), TextFlow::Path(_)))
                    || spans.clone().any(|(_, piece)| {
                        let decoration = piece.decoration();
                        piece.letter_spacing() != 0.0
                            || piece.word_spacing() != 0.0
                            || piece.text_length().is_some()
                            || [decoration.underline(), decoration.overline(), decoration.line_through()].iter().any(Option::is_some)
                            || piece.baseline_shift().iter().any(|shift| *shift != BaselineShift::Baseline)
                    })
                    || !spans
                        .clone()
                        .map(|(part, piece)| {
                            let paint = piece.fill().map(|filled| {
                                let solid = match filled.paint() {
                                    Paint::Color(color) => Some(*color),
                                    Paint::LinearGradient(_) | Paint::RadialGradient(_) | Paint::Pattern(_) => None,
                                };
                                (solid, filled.opacity())
                            });
                            (part.anchor(), piece.font(), piece.font_size(), piece.dominant_baseline(), piece.alignment_baseline(), paint, piece.is_visible())
                        })
                        .all_equal(),
                Feature::Layout,
            ),
        ]);
        let font = span.font();
        let origin = Vec2::new(
            chunk.x().map_or_default(f64::from) + text.dx().first().copied().map_or_default(f64::from),
            chunk.y().map_or_default(f64::from) + text.dy().first().copied().map_or_default(f64::from),
        );
        let located = Validated::from(Placement::try_new(frame * Affine::translate(origin) * Affine::scale(scale.recip())).map_err(AnnotationError::from));
        let sized = Validated::from(
            FontSize::try_new(f64::from(span.font_size().get()) * scale).map_err(|(FontSizeError::FiniteViolated | FontSizeError::GreaterViolated)| AnnotationError::Unsupported(Feature::Font)),
        );
        let weighted = Validated::from(
            FontWeight::try_new(f32::from(font.weight()))
                .map_err(|(FontWeightError::FiniteViolated | FontWeightError::GreaterOrEqualViolated | FontWeightError::LessOrEqualViolated)| AnnotationError::Unsupported(Feature::Font)),
        );
        let stretch = match font.stretch() {
            FontStretch::UltraCondensed => parlance::FontWidth::ULTRA_CONDENSED,
            FontStretch::ExtraCondensed => parlance::FontWidth::EXTRA_CONDENSED,
            FontStretch::Condensed => parlance::FontWidth::CONDENSED,
            FontStretch::SemiCondensed => parlance::FontWidth::SEMI_CONDENSED,
            FontStretch::Normal => parlance::FontWidth::NORMAL,
            FontStretch::SemiExpanded => parlance::FontWidth::SEMI_EXPANDED,
            FontStretch::Expanded => parlance::FontWidth::EXPANDED,
            FontStretch::ExtraExpanded => parlance::FontWidth::EXTRA_EXPANDED,
            FontStretch::UltraExpanded => parlance::FontWidth::ULTRA_EXPANDED,
        };
        let widened = Validated::from(FontWidth::try_new(stretch.ratio()).map_err(|(FontWidthError::FiniteViolated | FontWidthError::GreaterViolated)| AnnotationError::Unsupported(Feature::Font)));
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
        let style = match font.style() {
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
        Some(refusals.map4(
            located.map2(sized, |placement, size| (placement, size)),
            weighted.map2(widened, |weight, width| (weight, width)),
            Validated::from(colored(fill.paint(), fill.opacity())),
            |(), (placement, size), (weight, width), (color, opacity)| Slot {
                name: Arc::from(text.id()),
                text: Text { content, family, size, placement, weight, width, style, anchor, baseline, target: Target::Drafting, .. },
                color,
                opacity,
            },
        ))
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
            Node::Group(inner) => lowered(inner, frame, tolerance),
            Node::Path(path) => painted(path).map(|elements| (elements, Vec::new())),
            Node::Image(image) => match image.kind() {
                ImageKind::SVG(tree) => lowered(tree.root(), frame, tolerance),
                ImageKind::JPEG(bytes) | ImageKind::PNG(bytes) | ImageKind::GIF(bytes) | ImageKind::WEBP(bytes) => Validated::from(
                    Placement::try_new(frame * Affine::map_unit_square(Rect::new(0.0, 0.0, f64::from(image.size().width()), f64::from(image.size().height()))))
                        .map(|placement| (vec![Element::Placed { content: Content::Raster(Arc::from(bytes.as_slice())), placement, .. }], Vec::new()))
                        .map_err(AnnotationError::from),
                ),
            },
            Node::Text(text) => slotted(text).map_or_else(|| lowered(text.flattened(), frame, tolerance), |slot| slot.map(|template| (Vec::new(), vec![template]))),
        })
        .collect::<Validated<Vec<_>, _>>();
    refusals.map3(parts, clipped(group.clip_path(), frame, tolerance).map_or_else(Validated::Fail, Validated::Good), |(), children, clip| {
        let (elements, slots): (Vec<_>, Vec<_>) = children.into_iter().unzip();
        let drawn = elements.into_iter().flatten().collect();
        (
            match clip {
                Some(region) => vec![Element::Clip { region, keep: Side::Inside, elements: drawn }],
                None => drawn,
            },
            slots.into_iter().flatten().collect(),
        )
    })
}

fn clipped(clip: Option<&ClipPath>, frame: Affine, tolerance: Tolerance) -> Result<Option<Region>, NEVec<AnnotationError>> {
    fn area(group: &Group, frame: Affine, tolerance: Tolerance) -> Validated<Region, AnnotationError> {
        let frame = frame * affine(group.transform());
        group
            .children()
            .iter()
            .filter_map(|node| match node {
                Node::Group(inner) => Some(area(inner, frame, tolerance)),
                Node::Path(path) =>
                    path.fill().filter(|_| path.is_visible()).map(|fill| Validated::from(Region::new(&(frame * outline(path)), fill_rule(fill.rule()), tolerance).map_err(AnnotationError::from))),
                Node::Text(text) => Some(area(text.flattened(), frame, tolerance)),
                Node::Image(_) => None,
            })
            .collect::<Validated<Vec<_>, _>>()
            .map2(clipped(group.clip_path(), frame, tolerance).map_or_else(Validated::Fail, Validated::Good), |shapes, bound| (shapes, bound))
            .ok()
            .and_then(|(shapes, bound)| {
                let (target, op) = bound.map_or_else(|| (Region::default(), BinaryOp::Union), |region| (region, BinaryOp::Intersection));
                regions::boolean(&target, &shapes, op, tolerance).map_err(|error| nev![AnnotationError::from(error)])
            })
            .map_or_else(Validated::Fail, Validated::Good)
    }
    clip.map(|clip_path| {
        area(clip_path.root(), frame * affine(clip_path.transform()), tolerance)
            .map2(clipped(clip_path.clip_path(), frame, tolerance).map_or_else(Validated::Fail, Validated::Good), |shape, bound| (shape, bound))
            .ok()
            .and_then(|(shape, bound)| match bound {
                None => Ok(shape),
                Some(region) => regions::boolean(&region, [&shape], BinaryOp::Intersection, tolerance).map_err(|error| nev![AnnotationError::from(error)]),
            })
    })
    .transpose()
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
