use core::{
    f64::consts::{FRAC_PI_3, FRAC_PI_6},
    iter,
    range::Range,
    slice,
};
use std::{borrow::Cow, collections::HashMap, sync::Arc};

use fontdb::{Database, Source};
use fontique::{Attributes, Collection, CollectionOptions, FallbackKey, QueryFamily, SourceCache, Synthesis};
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Circle, Line, PathEl, Point, Rect, Shape as _, Triangle, Vec2};
use linebender_resource_handle::{Blob, FontData};
use linesweeper::FillRule;
use nonempty_collections::NEVec;
use parlance::{FontFamily, FontFamilyName, FontVariation, GenericFamily, Script, Tag};
use parley::{Alignment, AlignmentOptions, Cluster, FontContext, FontFeatures, Layout, LayoutContext, LineHeight, PositionedLayoutItem, StyleProperty};
use rasm_drawing::{Anchor, Baseline, Color, Element, FontSize, FontSizeError, Glyph, GlyphRun, Opacity, Placement, Region, Span, Target, Text, Tolerance, Width};
use skrifa::{DrawMemory, Font, GlyphId, PathSink, ScaleError, ScalerFont};
use uom::{ConstZero, si::f64::Angle};
use validated::Validated;

use crate::{Defect, DraftingError, Graphics, Lettering, Lookup, Style, fraction, indexed, located, oriented, polyline, readable, stroked, validated};

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
pub enum Output {
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
    pub spans: Vec<Span> = Vec::new(),
    pub lettering: Option<Lettering> = None,
    pub max_advance: Option<Width> = None,
    pub fit: Option<Width> = None,
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

struct Face {
    family: Arc<str>,
    font: Font,
}

struct Outline<'path> {
    path: &'path mut BezPath,
    affine: Affine,
}

impl PathSink for Outline<'_> {
    fn move_to(&mut self, x: f32, y: f32) { self.path.push(self.affine * PathEl::MoveTo((x, y).into())); }

    fn line_to(&mut self, x: f32, y: f32) { self.path.push(self.affine * PathEl::LineTo((x, y).into())); }

    fn quad_to(&mut self, cx0: f32, cy0: f32, x: f32, y: f32) { self.path.push(self.affine * PathEl::QuadTo((cx0, cy0).into(), (x, y).into())); }

    fn curve_to(&mut self, cx0: f32, cy0: f32, cx1: f32, cy1: f32, x: f32, y: f32) { self.path.push(self.affine * PathEl::CurveTo((cx0, cy0).into(), (cx1, cy1).into(), (x, y).into())); }

    fn close(&mut self) { self.path.close_path(); }
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
    #[debug(skip)]
    faces: HashMap<(u64, u32), Face>,
}

impl Typesetter {
    pub fn new<F, G, B>(fonts: F, generics: G, fallbacks: B) -> Result<Self, NEVec<DraftingError>>
    where
        F: IntoIterator<Item = Blob<u8>>,
        G: IntoIterator<Item = (GenericFamily, Arc<str>)>,
        B: IntoIterator<Item = (Script, Arc<str>)>,
    {
        let mut collection = Collection::new(CollectionOptions { shared: false, system_fonts: false });
        let mut database = Database::new();
        let registered = fonts
            .into_iter()
            .enumerate()
            .map(|(position, blob)| {
                let key = blob.id();
                let faces = collection
                    .register_fonts(blob.clone(), None)
                    .into_iter()
                    .flat_map(|(family, infos)| infos.into_iter().map(move |info| (family, info.index())))
                    .map(|(family, index)| {
                        collection.family_name(family).map(Arc::<str>::from).zip(Font::new(blob.clone().into_raw_parts().0, index)).map(|(name, font)| ((key, index), Face { family: name, font }))
                    })
                    .collect::<Option<Vec<_>>>()
                    .and_then(NEVec::try_from_vec)
                    .ok_or(DraftingError::Font { position });
                database.load_font_source(Source::Binary(blob.into_raw_parts().0));
                faces
            })
            .collect::<Validated<Vec<_>, _>>();
        let unknown = |name: &Arc<str>| DraftingError::Unknown { lookup: Lookup::Family, name: Arc::clone(name) };
        let mut named = |generic: GenericFamily, name: &str| match generic {
            GenericFamily::Serif => database.set_serif_family(name),
            GenericFamily::SansSerif => database.set_sans_serif_family(name),
            GenericFamily::Cursive => database.set_cursive_family(name),
            GenericFamily::Fantasy => database.set_fantasy_family(name),
            GenericFamily::Monospace => database.set_monospace_family(name),
            GenericFamily::SystemUi
            | GenericFamily::UiSerif
            | GenericFamily::UiSansSerif
            | GenericFamily::UiMonospace
            | GenericFamily::UiRounded
            | GenericFamily::Emoji
            | GenericFamily::Math
            | GenericFamily::FangSong => (),
        };
        let assigned = generics
            .into_iter()
            .map(|(generic, name)| {
                let id = collection.family_id(&name).ok_or_else(|| unknown(&name))?;
                if collection.generic_families(generic).next().is_none() {
                    named(generic, &name);
                }
                collection.append_generic_families(generic, iter::once(id));
                Ok::<_, DraftingError>(())
            })
            .collect::<Validated<(), _>>();
        let fallen = fallbacks
            .into_iter()
            .map(|(script, name)| {
                collection.family_id(&name).ok_or_else(|| unknown(&name)).map(|id| {
                    collection.append_fallbacks(FallbackKey::new(script, None), iter::once(id));
                })
            })
            .collect::<Validated<(), _>>();
        registered
            .map3(assigned, fallen, |faces, (), ()| Self {
                fonts: FontContext { collection, source_cache: SourceCache::default() },
                context: LayoutContext::new(),
                layout: Layout::new(),
                database: Arc::new(database),
                faces: faces.into_iter().flatten().collect(),
            })
            .ok()
    }

    pub fn typeset(&mut self, text: &Text) -> Result<Block, NEVec<DraftingError>> {
        let (index, ratio) = em(self, text)?;
        let size =
            FontSize::try_new(text.size.into_inner() * ratio).map_err(|(FontSizeError::FiniteViolated | FontSizeError::GreaterViolated)| DraftingError::Face { index, defect: Defect::CapHeight })?;
        let clustered = |cluster: &Cluster<'_, ()>| {
            let range = cluster.text_range();
            let end = if cluster.is_ligature_start() {
                iter::successors(cluster.next_logical(), Cluster::next_logical).take_while(Cluster::is_ligature_continuation).last().map_or(range.end, |continuation| continuation.text_range().end)
            } else {
                range.end
            };
            Range { start: range.start, end }
        };
        let Self { fonts, context, layout, database: _, faces } = self;
        let mut builder = context.ranged_builder(fonts, &text.content, 1.0, false);
        builder.push_default(StyleProperty::FontFamily(text.family.clone()));
        let defaults = [
            Some(StyleProperty::FontSize(size.into_inner().cast::<f32>())),
            Some(StyleProperty::FontWeight(text.weight.into())),
            Some(StyleProperty::FontWidth(text.width.into())),
            Some(StyleProperty::FontStyle(text.style.into())),
            Some(StyleProperty::FontFeatures(FontFeatures::List(Cow::Borrowed(&text.features)))),
            text.line_height.map(|pitch| StyleProperty::LineHeight(LineHeight::Absolute(pitch.into_inner().cast::<f32>()))),
        ];
        for property in defaults.into_iter().flatten() {
            builder.push_default(property);
        }
        let ranged = text.spans.iter().flat_map(|Span { range, weight, style, features }| {
            let bounds = range.start..range.end;
            [
                weight.map(|bold| StyleProperty::FontWeight(bold.into())),
                style.map(|slant| StyleProperty::FontStyle(slant.into())),
                (!features.is_empty()).then(|| StyleProperty::FontFeatures(FontFeatures::List(Cow::Borrowed(features)))),
            ]
            .into_iter()
            .flatten()
            .map(move |property| (property, bounds.clone()))
        });
        for (property, bounds) in ranged {
            builder.push(property, bounds);
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
        let line_length = f64::from(max_advance.unwrap_or_else(|| layout.width()));
        let along = if rtl { 1.0 - fraction(text.anchor) } else { fraction(text.anchor) };
        let down = match text.baseline {
            Baseline::Top => 0.0,
            Baseline::Middle => layout.first_baseline().zip(layout.last_baseline()).map_or_default(|(first, last)| (f64::from(first) - text.size.into_inner()).midpoint(f64::from(last))),
            Baseline::Alphabetic => layout.first_baseline().map_or_default(f64::from),
            Baseline::Bottom => f64::from(layout.height()),
        };
        let origin = Affine::translate((-line_length * along, -down));
        let extent = layout.lines().fold(Rect::new(0.0, 0.0, line_length, f64::from(layout.height())), |bounds, line| {
            let metrics = line.metrics();
            let [offset, advance, hanging] = [metrics.offset, metrics.advance, metrics.hanging_advance].map(f64::from);
            let [left, right] = if rtl { [offset + hanging, offset + advance] } else { [offset, offset + advance - hanging] };
            bounds.union_pt((left, 0.0)).union_pt((right, 0.0))
        });
        let runs = layout
            .lines()
            .flat_map(|line| line.items())
            .filter_map(|item| match item {
                PositionedLayoutItem::GlyphRun(glyph_run) => Some(glyph_run),
                PositionedLayoutItem::InlineBox(_) => None,
            })
            .map(|glyph_run| {
                let run = glyph_run.run();
                let font = run.font();
                let synthesis = run.synthesis();
                let skew = synthesis.skew().map_or(Affine::IDENTITY, |angle| Affine::skew(-f64::from(angle).to_radians().tan(), 0.0));
                let placement = Placement::try_new(*text.placement * origin * Affine::translate((f64::from(glyph_run.offset()), f64::from(glyph_run.baseline()))) * skew).map_err(DraftingError::from);
                Validated::from(registered(faces, font)).map2(Validated::from(placement), |face, placement| GlyphRun {
                    font: font.clone(),
                    family: Arc::clone(&face.family),
                    size,
                    variations: varied(&synthesis),
                    glyphs: run
                        .visual_clusters()
                        .flat_map(|cluster| {
                            let span = clustered(&cluster);
                            cluster.glyphs().map(move |glyph| Glyph {
                                id: glyph.id,
                                cluster: span,
                                advance: Vec2::new(f64::from(glyph.advance), 0.0),
                                offset: Vec2::new(f64::from(glyph.x), f64::from(glyph.y)),
                            })
                        })
                        .collect(),
                    text: Arc::clone(&text.content),
                    placement,
                    target: text.target,
                })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        Ok(Block { runs, extent: origin.transform_rect_bbox(extent) })
    }

    #[stacksafe::stacksafe]
    pub fn shape(&mut self, elements: Vec<Element>, output: Output) -> Result<Vec<Element>, NEVec<DraftingError>> {
        fn outlined(faces: &HashMap<(u64, u32), Face>, run: &GlyphRun, tolerance: Tolerance) -> Result<Region, NEVec<DraftingError>> {
            let font = instance(&registered(faces, &run.font)?.font, &run.variations);
            let em = run.size.into_inner() / f64::from(font.units_per_em());
            let scaler = ScalerFont::unhinted(font, None);
            let mut path = BezPath::new();
            run.glyphs
                .iter()
                .scan(Affine::IDENTITY, |pen, glyph| {
                    let origin = pen.then_translate(glyph.offset);
                    *pen = pen.then_translate(glyph.advance);
                    Some((glyph, origin))
                })
                .map(|(glyph, origin)| {
                    let id = GlyphId::new(glyph.id);
                    let start = path.elements().len();
                    scaler
                        .outline(id)
                        .ok_or(ScaleError::GlyphNotFound(id))
                        .and_then(|outline| outline.draw(DrawMemory::default(), &mut Outline { path: &mut path, affine: *run.placement * origin * Affine::scale_non_uniform(em, -em) }))
                        .map(|_metrics| ())
                        .map_err(|source| {
                            path.truncate(start);
                            DraftingError::Outline { glyph: glyph.id, source }
                        })
                })
                .collect::<Validated<(), _>>()
                .ok()?;
            Region::new(path, FillRule::NonZero, tolerance).map_err(|error| DraftingError::from(error).into())
        }

        let painted = |faces: &HashMap<(u64, u32), Face>, run: GlyphRun, color: Color, opacity: Opacity| match output {
            Output::Glyphs => Validated::Good(Element::Glyphs { run, color, opacity }),
            Output::Outlines(tolerance) => validated(outlined(faces, &run, tolerance)).map(|region| Element::Fill { region, color, opacity }),
        };
        indexed::<_, Vec<_>, _>(elements.into_iter().map(|element| match element {
            Element::Text { text, color, opacity } => {
                let Block { runs, extent: _ } = self.typeset(&text)?;
                runs.into_iter().map(|run| painted(&self.faces, run, color.clone(), opacity)).collect::<Validated<Vec<_>, _>>().ok()
            }
            Element::Glyphs { run, color, opacity } => painted(&self.faces, run, color, opacity).ok().map(|drawn| vec![drawn]),
            Element::Clip { region, keep, elements: grouped } => self.shape(grouped.into_inner(), output).map(|shaped| vec![Element::Clip { region, keep, elements: shaped.into() }]),
            kept @ (Element::Stroke { path: _, pen: _, linetype: _ }
            | Element::Fill { region: _, color: _, opacity: _ }
            | Element::Hatch { region: _, pattern: _, placement: _, pen: _ }
            | Element::Placed { content: _, placement: _, opacity: _ }) => Ok(vec![kept]),
        }))
        .map(|shaped| shaped.into_iter().flatten().collect())
    }
}

pub(crate) fn em(typesetter: &mut Typesetter, text: &Text) -> Result<(u32, f64), DraftingError> {
    let Typesetter { fonts, context: _, layout: _, database: _, faces } = typesetter;
    let names = match &text.family {
        FontFamily::Source(source) =>
            Cow::Owned(FontFamilyName::parse_css_list(source).collect::<Result<Vec<_>, _>>().map_err(|parse| DraftingError::Stack { list: Arc::from(source.as_ref()), source: parse })?),
        FontFamily::Single(name) => Cow::Borrowed(slice::from_ref(name)),
        FontFamily::List(names) => Cow::Borrowed(names.as_ref()),
    };
    let primary = {
        let mut query = fonts.collection.query(&mut fonts.source_cache);
        query.set_families(names.iter().map(|name| match name {
            FontFamilyName::Named(family) => QueryFamily::Named(family),
            FontFamilyName::Generic(generic) => QueryFamily::Generic(*generic),
        }));
        query.set_attributes(Attributes::new(text.width.into(), text.style.into(), text.weight.into()));
        query.first_available_font()
    }
    .ok_or_else(|| DraftingError::Unknown { lookup: Lookup::Family, name: names.iter().join(", ").into() })?;
    let face = faces.get(&(primary.blob.id(), primary.index)).ok_or(DraftingError::Face { index: primary.index, defect: Defect::Unregistered })?;
    let font = instance(&face.font, &varied(&primary.synthesis));
    font.metrics()
        .cap_height
        .map(|cap_height| f64::from(font.units_per_em()) / cap_height.to_f64())
        .filter(|ratio| ratio.is_finite() && *ratio > 0.0)
        .map(|ratio| (primary.index, ratio))
        .ok_or(DraftingError::Face { index: primary.index, defect: Defect::CapHeight })
}

fn registered<'faces>(faces: &'faces HashMap<(u64, u32), Face>, font: &FontData) -> Result<&'faces Face, DraftingError> {
    faces.get(&(font.data.id(), font.index)).ok_or(DraftingError::Face { index: font.index, defect: Defect::Unregistered })
}

fn varied(synthesis: &Synthesis) -> Arc<[FontVariation]> { synthesis.variation_settings().iter().map(|&(tag, value)| FontVariation { tag: Tag::from_bytes(tag.to_be_bytes()), value }).collect() }

fn instance(font: &Font, variations: &[FontVariation]) -> Font {
    font.instance_builder().variations(variations.iter().map(|variation| (skrifa::Tag::from_be_bytes(variation.tag.to_bytes()), variation.value))).build()
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn labelled(typesetter: &mut Typesetter, style: &Style, label: &Label, at: Point, direction: Vec2, anchor: Anchor, baseline: Baseline) -> Result<Graphics, NEVec<DraftingError>> {
    let upright = readable(direction);
    let flipped = upright.dot(direction).is_sign_negative();
    let anchor = match (flipped, anchor) {
        (true, Anchor::Start) => Anchor::End,
        (true, Anchor::End) => Anchor::Start,
        (true, Anchor::Middle) | (false, Anchor::Start | Anchor::Middle | Anchor::End) => anchor,
    };
    let baseline = match (flipped, baseline) {
        (true, Baseline::Top) => Baseline::Bottom,
        (true, Baseline::Bottom) => Baseline::Top,
        (true, Baseline::Middle | Baseline::Alphabetic) | (false, Baseline::Top | Baseline::Middle | Baseline::Alphabetic | Baseline::Bottom) => baseline,
    };
    let placement = Placement::try_new(oriented(at, upright)).map_err(DraftingError::from)?;
    let Lettering { family, size, weight, width, style: slant, line_height, features, color, opacity } = label.lettering.as_ref().unwrap_or(&style.lettering);
    let block = typesetter.typeset(&Text {
        content: Arc::clone(&label.content),
        family: family.clone(),
        size: *size,
        placement,
        weight: *weight,
        width: *width,
        style: *slant,
        anchor,
        baseline,
        max_advance: label.max_advance,
        line_height: *line_height,
        features: features.clone(),
        spans: label.spans.clone(),
        target: Target::Drafting,
    })?;
    let (runs, extent) = match label.fit.map(|fit| fit.into_inner() / block.extent.width()).filter(|ratio| *ratio < 1.0) {
        None => (Ok(block.runs), block.extent),
        Some(ratio) => {
            let squeeze = Affine::scale_non_uniform(ratio, 1.0);
            let compression = *placement * squeeze * placement.inverse();
            (
                block.runs.into_iter().map(|run| Placement::try_new(compression * *run.placement).map(|moved| GlyphRun { placement: moved, ..run })).collect::<Result<Vec<_>, _>>(),
                squeeze.transform_rect_bbox(block.extent),
            )
        }
    };
    let accuracy = style.tolerance.into_inner();
    let gap = style.gap.into_inner();
    let rect = extent.inflate(gap, gap);
    let framed = |outline: BezPath| (outline.clone(), Some(outline));
    let (boundary, rule) = match label.frame {
        None => (rect.to_path(accuracy), None),
        Some(Frame::Underline) => (rect.to_path(accuracy), Some(Line::new((rect.x0, rect.y1), (rect.x1, rect.y1)).to_path(accuracy))),
        Some(Frame::Rectangle) => framed(rect.to_path(accuracy)),
        Some(Frame::Capsule) => framed(rect.to_rounded_rect(rect.height() * 0.5).to_path(accuracy)),
        Some(Frame::Circle) => framed(Circle::new(rect.center(), rect.size().to_vec2().length() * 0.5).to_path(accuracy)),
        Some(Frame::Hexagon) => {
            let reach = rect.height() * 0.5 * FRAC_PI_6.tan();
            let middle = rect.center().y;
            let mut hexagon = polyline([
                Point::new(rect.x0 - reach, middle),
                Point::new(rect.x0, rect.y0),
                Point::new(rect.x1, rect.y0),
                Point::new(rect.x1 + reach, middle),
                Point::new(rect.x1, rect.y1),
                Point::new(rect.x0, rect.y1),
            ]);
            hexagon.close_path();
            framed(hexagon)
        }
        Some(Frame::Triangle) => {
            let half = rect.height().mul_add(FRAC_PI_6.tan(), rect.width() * 0.5);
            let middle = rect.center().x;
            framed(Triangle::new((middle - half, rect.y1), (middle, half.mul_add(-FRAC_PI_3.tan(), rect.y1)), (middle + half, rect.y1)).to_path(accuracy))
        }
    };
    Validated::from(rule.map(|outline| stroked(*placement * outline, &style.pen, None)).transpose())
        .map3(
            Validated::from(Region::new(*placement * boundary, FillRule::NonZero, style.tolerance).map_err(DraftingError::from)),
            Validated::from(runs.map_err(DraftingError::from)),
            |stroke, mask, runs| Graphics { elements: runs.into_iter().map(|run| Element::Glyphs { run, color: color.clone(), opacity: *opacity }).chain(stroke).collect(), masks: vec![mask] },
        )
        .ok()
}

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, note: &Note) -> Result<Graphics, NEVec<DraftingError>> {
    let Note { at, angle, anchor, baseline, label } = note;
    let (point, direction) = located(placement, *at, *angle);
    labelled(typesetter, style, label, point, direction, *anchor, *baseline)
}
