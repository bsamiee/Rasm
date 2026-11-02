use core::{
    f64::consts::{FRAC_PI_2, FRAC_PI_3, FRAC_PI_6},
    iter,
    range::Range,
};
use std::{collections::HashMap, sync::Arc};

use fontdb::{Database, Source};
use fontique::{Attributes, Collection, CollectionOptions, QueryFamily, SourceCache};
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Circle, Line, PathEl, Point, Rect, Shape as _, Vec2};
use linebender_resource_handle::Blob;
use linesweeper::FillRule;
use nonempty_collections::{NEVec, nev};
use parlance::{FontFamily, FontFamilyName, FontVariation, GenericFamily, Tag};
use parley::{Alignment, AlignmentOptions, Cluster, FontContext, Layout, LayoutContext, PositionedLayoutItem, StyleProperty};
use rasm_linework::{Anchor, Baseline, Color, Element, FontSize, FontSizeError, FontStyle, Glyph, GlyphRun, Opacity, Placement, Region, Slant, Target, Text, Tolerance, Width};
use skrifa::{DrawMemory, Font, GlyphId, PathSink, ScaleError, ScalerFont};
use uom::{
    ConstZero,
    si::{angle::radian, f64::Angle},
};
use validated::Validated;

use crate::{AnnotationError, Defect, Graphics, Lookup, Style, stroked};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Frame {
    Rectangle,
    Capsule,
    Circle,
    Hexagon,
    Triangle,
    Underline,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Form {
    Glyphs,
    Outlines(Tolerance),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Block {
    pub runs: Vec<GlyphRun>,
    pub extent: Rect,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Label {
    pub content: Arc<str>,
    pub max_advance: Option<Width> = None,
    pub frame: Option<Frame> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Note {
    pub at: Point,
    pub angle: Angle = ConstZero::ZERO,
    pub anchor: Anchor = Anchor::Start,
    pub baseline: Baseline = Baseline::Alphabetic,
    pub label: Label,
}

struct Outline(BezPath);

impl PathSink for Outline {
    fn move_to(&mut self, x: f32, y: f32) { self.0.move_to((x, y)); }

    fn line_to(&mut self, x: f32, y: f32) { self.0.line_to((x, y)); }

    fn quad_to(&mut self, cx0: f32, cy0: f32, x: f32, y: f32) { self.0.quad_to((cx0, cy0), (x, y)); }

    fn curve_to(&mut self, cx0: f32, cy0: f32, cx1: f32, cy1: f32, x: f32, y: f32) { self.0.curve_to((cx0, cy0), (cx1, cy1), (x, y)); }

    fn close(&mut self) { self.0.close_path(); }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub struct Typesetter {
    #[debug(skip)]
    fonts: FontContext,
    #[debug(skip)]
    context: LayoutContext<()>,
    #[debug(skip)]
    layout: Layout<()>,
    pub(crate) database: Arc<Database>,
    families: HashMap<(u64, u32), Arc<str>>,
}

impl Typesetter {
    pub fn new(fonts: impl IntoIterator<Item = Blob<u8>>, generics: impl IntoIterator<Item = (GenericFamily, Arc<str>)>) -> Result<Self, NEVec<AnnotationError>> {
        let mut collection = Collection::new(CollectionOptions { shared: false, system_fonts: false });
        let mut database = Database::new();
        let registered = fonts
            .into_iter()
            .enumerate()
            .map(|(position, blob)| {
                let key = blob.id();
                let named = collection
                    .register_fonts(blob.clone(), None)
                    .into_iter()
                    .flat_map(|(family, infos)| infos.into_iter().map(move |info| (family, info.index())))
                    .filter_map(|(family, index)| collection.family_name(family).map(|name| ((key, index), Arc::<str>::from(name))))
                    .collect::<Vec<_>>();
                database.load_font_source(Source::Binary(blob.into_raw_parts().0));
                NEVec::try_from_vec(named).ok_or(AnnotationError::Font { position })
            })
            .collect::<Validated<Vec<_>, _>>();
        let assigned = generics
            .into_iter()
            .map(|(generic, name)| -> Result<(), AnnotationError> {
                let family = collection.family_id(&name).ok_or_else(|| AnnotationError::Unknown { lookup: Lookup::Family, name: Arc::clone(&name) })?;
                if collection.generic_families(generic).next().is_none() {
                    match generic {
                        GenericFamily::Serif => database.set_serif_family(&*name),
                        GenericFamily::SansSerif => database.set_sans_serif_family(&*name),
                        GenericFamily::Cursive => database.set_cursive_family(&*name),
                        GenericFamily::Fantasy => database.set_fantasy_family(&*name),
                        GenericFamily::Monospace => database.set_monospace_family(&*name),
                        GenericFamily::SystemUi
                        | GenericFamily::UiSerif
                        | GenericFamily::UiSansSerif
                        | GenericFamily::UiMonospace
                        | GenericFamily::UiRounded
                        | GenericFamily::Emoji
                        | GenericFamily::Math
                        | GenericFamily::FangSong => (),
                    }
                }
                collection.append_generic_families(generic, iter::once(family));
                Ok(())
            })
            .collect::<Validated<(), _>>();
        registered
            .map2(assigned, |families, ()| Self {
                fonts: FontContext { collection, source_cache: SourceCache::default() },
                context: LayoutContext::new(),
                layout: Layout::new(),
                database: Arc::new(database),
                families: families.into_iter().flatten().collect(),
            })
            .ok()
    }

    pub fn typeset(&mut self, text: &Text) -> Result<Block, AnnotationError> {
        let Self { fonts, context, layout, database: _, families } = self;
        let style = match text.style {
            FontStyle::Normal => parlance::FontStyle::Normal,
            FontStyle::Italic => parlance::FontStyle::Italic,
            FontStyle::Oblique { angle } => parlance::FontStyle::Oblique(angle.map(Slant::into_inner)),
        };
        let weight = parlance::FontWeight::new(text.weight.into_inner());
        let width = parlance::FontWidth::from_ratio(text.width.into_inner());
        let stack = match &text.family {
            FontFamily::Source(source) =>
                FontFamilyName::parse_css_list(source).collect::<Result<Vec<_>, _>>().map_err(|parse| AnnotationError::Stack { list: Arc::from(source.as_ref()), source: parse })?,
            FontFamily::Single(name) => vec![name.clone()],
            FontFamily::List(names) => names.to_vec(),
        };
        let face = {
            let mut query = fonts.collection.query(&mut fonts.source_cache);
            query.set_families(stack.iter().map(|name| match name {
                FontFamilyName::Named(named) => QueryFamily::Named(named),
                FontFamilyName::Generic(generic) => QueryFamily::Generic(*generic),
            }));
            query.set_attributes(Attributes::new(width, style, weight));
            query.first_available_font()
        }
        .ok_or_else(|| AnnotationError::Unknown { lookup: Lookup::Family, name: stack.iter().join(", ").into() })?;
        let instance = Font::new(face.blob.into_raw_parts().0, face.index)
            .ok_or(AnnotationError::Face { index: face.index, defect: Defect::Unreadable })?
            .instance_builder()
            .variations(face.synthesis.variation_settings().iter().map(|&(tag, value)| (skrifa::Tag::from_be_bytes(tag.to_be_bytes()), value)))
            .build();
        let capital = text.size.into_inner();
        let size = instance.metrics().cap_height.ok_or(AnnotationError::Face { index: face.index, defect: Defect::Capless }).and_then(|cap_height| {
            FontSize::try_new(capital * f64::from(instance.units_per_em()) / cap_height.to_f64())
                .map_err(|(FontSizeError::FiniteViolated | FontSizeError::GreaterViolated)| AnnotationError::Face { index: face.index, defect: Defect::Capless })
        })?;
        let mut builder = context.ranged_builder(fonts, &text.content, 1.0, false);
        for property in [
            StyleProperty::FontFamily(text.family.clone()),
            StyleProperty::FontSize(size.into_inner().cast::<f32>()),
            StyleProperty::FontWeight(weight),
            StyleProperty::FontWidth(width),
            StyleProperty::FontStyle(style),
        ] {
            builder.push_default(property);
        }
        builder.build_into(layout, &text.content);
        let max_advance = text.max_advance.map(|advance| advance.into_inner().cast::<f32>());
        layout.break_all_lines(max_advance);
        layout.align(
            match text.anchor {
                Anchor::Start => Alignment::Start,
                Anchor::Middle => Alignment::Center,
                Anchor::End => Alignment::End,
            },
            AlignmentOptions { align_when_overflowing: true, ..AlignmentOptions::default() },
        );
        let rtl = layout.is_rtl();
        let measure = f64::from(max_advance.unwrap_or_else(|| layout.width()));
        let along = match (text.anchor, rtl) {
            (Anchor::Start, false) | (Anchor::End, true) => 0.0,
            (Anchor::Middle, false | true) => 0.5,
            (Anchor::End, false) | (Anchor::Start, true) => 1.0,
        };
        let down = match text.baseline {
            Baseline::Top => 0.0,
            Baseline::Middle => layout.first_baseline().zip(layout.last_baseline()).map_or_default(|(first, last)| (f64::from(first) - capital).midpoint(f64::from(last))),
            Baseline::Alphabetic => layout.first_baseline().map_or_default(f64::from),
            Baseline::Bottom => f64::from(layout.height()),
        };
        let origin = Affine::translate((-measure * along, -down));
        let (left, right) = layout
            .lines()
            .flat_map(|line| {
                let metrics = line.metrics();
                let [offset, advance, hanging] = [metrics.offset, metrics.advance, metrics.hanging_advance].map(f64::from);
                if rtl { [offset + hanging, offset + advance] } else { [offset, offset + advance - hanging] }
            })
            .chain([0.0, measure])
            .minmax_by(f64::total_cmp)
            .into_option()
            .unwrap_or((0.0, measure));
        let extent = Rect::new(left, 0.0, right, f64::from(layout.height()));
        let runs = layout
            .lines()
            .flat_map(|line| line.items())
            .filter_map(|item| match item {
                PositionedLayoutItem::GlyphRun(glyph_run) => Some(glyph_run),
                PositionedLayoutItem::InlineBox(_) => None,
            })
            .map(|glyph_run| -> Result<GlyphRun, AnnotationError> {
                let run = glyph_run.run();
                let font = run.font();
                let synthesis = run.synthesis();
                let family = families.get(&(font.data.id(), font.index)).ok_or(AnnotationError::Face { index: font.index, defect: Defect::Unregistered })?;
                let skew = synthesis.skew().map_or(Affine::IDENTITY, |angle| Affine::skew(-f64::from(angle).to_radians().tan(), 0.0));
                Ok(GlyphRun {
                    font: font.clone(),
                    family: Arc::clone(family),
                    size,
                    variations: synthesis.variation_settings().iter().map(|&(tag, value)| FontVariation { tag: Tag::from_bytes(tag.to_be_bytes()), value }).collect(),
                    glyphs: run
                        .visual_clusters()
                        .flat_map(|cluster| {
                            let range = cluster.text_range();
                            let end = if cluster.is_ligature_start() {
                                iter::successors(cluster.next_logical(), Cluster::next_logical)
                                    .take_while(Cluster::is_ligature_continuation)
                                    .last()
                                    .map_or(range.end, |continuation| continuation.text_range().end)
                            } else {
                                range.end
                            };
                            let span = Range { start: range.start, end };
                            cluster.glyphs().map(move |glyph| Glyph {
                                id: glyph.id,
                                cluster: span,
                                advance: Vec2::new(f64::from(glyph.advance), 0.0),
                                offset: Vec2::new(f64::from(glyph.x), f64::from(glyph.y)),
                            })
                        })
                        .collect(),
                    text: Arc::clone(&text.content),
                    placement: Placement::try_new(*text.placement * origin * Affine::translate((f64::from(glyph_run.offset()), f64::from(glyph_run.baseline()))) * skew)?,
                    target: text.target,
                })
            })
            .collect::<Result<Vec<_>, _>>()?;
        Ok(Block { runs, extent: origin.transform_rect_bbox(extent) })
    }

    pub fn shape(&mut self, elements: Vec<Element>, form: Form) -> Result<Vec<Element>, NEVec<AnnotationError>> {
        let outlined = |run: &GlyphRun, tolerance: Tolerance| -> Result<Region, NEVec<AnnotationError>> {
            let font = Font::new(run.font.data.clone().into_raw_parts().0, run.font.index).ok_or_else(|| nev![AnnotationError::Face { index: run.font.index, defect: Defect::Unreadable }])?;
            let instance = font.instance_builder().variations(run.variations.iter().map(|variation| (skrifa::Tag::from_be_bytes(variation.tag.to_bytes()), variation.value))).build();
            let em = run.size.into_inner() / f64::from(instance.units_per_em());
            let scaler = ScalerFont::unhinted(instance, None);
            let glyphs = run
                .glyphs
                .iter()
                .scan(Affine::IDENTITY, |pen, glyph| {
                    let origin = pen.then_translate(glyph.offset);
                    *pen = pen.then_translate(glyph.advance);
                    Some((glyph, origin))
                })
                .map(|(glyph, origin)| {
                    let id = GlyphId::new(glyph.id);
                    let mut sink = Outline(BezPath::new());
                    scaler
                        .outline(id)
                        .ok_or(ScaleError::GlyphNotFound(id))
                        .and_then(|outline| outline.draw(DrawMemory::default(), &mut sink))
                        .map(|_metrics| *run.placement * origin * Affine::scale_non_uniform(em, -em) * sink.0)
                        .map_err(|source| AnnotationError::Outline { glyph: glyph.id, source })
                })
                .collect::<Validated<Vec<_>, _>>()
                .ok()?;
            Region::new(&glyphs.into_iter().flatten().collect::<BezPath>(), FillRule::NonZero, tolerance).map_err(|error| nev![AnnotationError::from(error)])
        };
        let painted = |run: GlyphRun, color: Color, opacity: Opacity| -> Validated<Element, AnnotationError> {
            match form {
                Form::Glyphs => Validated::Good(Element::Glyphs { run, color, opacity }),
                Form::Outlines(tolerance) => outlined(&run, tolerance).map_or_else(Validated::Fail, |region| Validated::Good(Element::Fill { region, color, opacity })),
            }
        };
        elements
            .into_iter()
            .enumerate()
            .map(|(index, element)| {
                match element {
                    Element::Text { text, color, opacity } => self.typeset(&text).map_err(|error| nev![error]).and_then(|Block { runs, extent: _ }| {
                        let count = runs.len();
                        iter::zip(runs, iter::repeat_n(color, count)).map(|(run, color)| painted(run, color, opacity)).collect::<Validated<Vec<_>, _>>().ok()
                    }),
                    Element::Glyphs { run, color, opacity } => painted(run, color, opacity).ok().map(|drawn| vec![drawn]),
                    Element::Clip { region, keep, elements: grouped } => self.shape(grouped, form).map(|shaped| vec![Element::Clip { region, keep, elements: shaped }]),
                    kept @ (Element::Stroke { path: _, pen: _, linetype: _ }
                    | Element::Fill { region: _, color: _, opacity: _ }
                    | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
                    | Element::Placed { content: _, placement: _, opacity: _ }) => Ok(vec![kept]),
                }
                .map_err(|errors| AnnotationError::Item { index, errors })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .map(|shaped| shaped.into_iter().flatten().collect())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn labelled(
    typesetter: &mut Typesetter,
    style: &Style,
    label: &Label,
    at: Point,
    direction: Vec2,
    anchor: Anchor,
    baseline: Baseline,
) -> Result<(Vec<Element>, Region), NEVec<AnnotationError>> {
    let readable = if (-FRAC_PI_2..FRAC_PI_2).contains(&direction.atan2()) { direction } else { -direction };
    let (placement, block) = Placement::try_new(Affine::rotate(readable.atan2()).then_translate(at.to_vec2()))
        .map_err(AnnotationError::from)
        .and_then(|placement| {
            typesetter
                .typeset(&Text {
                    content: Arc::clone(&label.content),
                    family: style.lettering.family.clone(),
                    size: style.lettering.size,
                    placement,
                    weight: style.lettering.weight,
                    width: style.lettering.width,
                    style: style.lettering.style,
                    anchor,
                    baseline,
                    max_advance: label.max_advance,
                    target: Target::Drafting,
                })
                .map(|block| (placement, block))
        })
        .map_err(|error| nev![error])?;
    let tolerance = style.tolerance.into_inner();
    let gap = style.gap.into_inner();
    let rect = block.extent.inflate(gap, gap);
    let polygon = |start: Point, rest: &[Point]| iter::once(PathEl::MoveTo(start)).chain(rest.iter().copied().map(PathEl::LineTo)).chain(iter::once(PathEl::ClosePath)).collect::<BezPath>();
    let boundary = match label.frame {
        None | Some(Frame::Rectangle | Frame::Underline) => rect.to_path(tolerance),
        Some(Frame::Capsule) => rect.to_rounded_rect(rect.height() * 0.5).to_path(tolerance),
        Some(Frame::Circle) => Circle::new(rect.center(), rect.size().to_vec2().length() * 0.5).to_path(tolerance),
        Some(Frame::Hexagon) => {
            let reach = rect.height() * 0.5 * FRAC_PI_6.tan();
            let middle = rect.center().y;
            polygon(
                Point::new(rect.x0 - reach, middle),
                &[Point::new(rect.x0, rect.y0), Point::new(rect.x1, rect.y0), Point::new(rect.x1 + reach, middle), Point::new(rect.x1, rect.y1), Point::new(rect.x0, rect.y1)],
            )
        }
        Some(Frame::Triangle) => {
            let half = rect.height().mul_add(FRAC_PI_6.tan(), rect.width() * 0.5);
            let middle = rect.center().x;
            polygon(Point::new(middle - half, rect.y1), &[Point::new(middle, half.mul_add(-FRAC_PI_3.tan(), rect.y1)), Point::new(middle + half, rect.y1)])
        }
    };
    let rule = label.frame.map(|frame| match frame {
        Frame::Underline => Line::new((rect.x0, rect.y1), (rect.x1, rect.y1)).to_path(tolerance),
        Frame::Rectangle | Frame::Capsule | Frame::Circle | Frame::Hexagon | Frame::Triangle => boundary.clone(),
    });
    Validated::from(rule.map(|outline| stroked(*placement * outline, &style.pen, None)).transpose())
        .map2(Validated::from(Region::new(&(*placement * boundary), FillRule::NonZero, style.tolerance).map_err(AnnotationError::from)), |stroke, mask| {
            (block.runs.into_iter().map(|run| Element::Glyphs { run, color: style.lettering.color.clone(), opacity: style.lettering.opacity }).chain(stroke).collect(), mask)
        })
        .ok()
}

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, note: &Note) -> Result<Graphics, NEVec<AnnotationError>> {
    let Note { at, angle, anchor, baseline, label } = note;
    let direction = (placement.with_translation(Vec2::ZERO) * Vec2::from_angle(angle.get::<radian>()).to_point()).to_vec2();
    labelled(typesetter, style, label, *placement * *at, direction, *anchor, *baseline).map(|(elements, mask)| Graphics { elements, mask })
}
