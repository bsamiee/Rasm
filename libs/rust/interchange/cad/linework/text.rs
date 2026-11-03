use core::{iter, range::Range};
use std::{borrow::Cow, collections::BTreeSet, sync::Arc};

use acadrust::{
    entities::{
        AttachmentPoint, AttributeEntity, EntityType, HorizontalAlignment, MText, Text, TextHorizontalAlignment, TextVerticalAlignment, VerticalAlignment,
        mtext_format::{MTextDocument, MTextFont, MTextParagraph, MTextSpan, SpanProperties, parse_mtext, parse_plain_text},
    },
    tables::{Table, TextStyle},
    types::{Color, Matrix3, Vector3},
};
use kurbo::{Affine, Point, Rect, Shape as _, Vec2};
use linesweeper::FillRule;
use parlance::{FontFamily, FontFamilyName};
use rasm_linework::{Anchor, Baseline, FontSize, FontSizeError, FontStyle, FontWeight, Placement, Region, Span, Tolerance, Width};

use super::curves::ocs;
use crate::{CadError, Refusal, Subject};

// --- [TYPES] ---------------------------------------------------------------------------

type Backdrop = (Region, Color);

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(super) fn lifted(
    entity: &EntityType,
    styles: &Table<TextStyle>,
    families: &[FontFamilyName<'static>],
    frame: Affine,
    tolerance: Tolerance,
    subject: &Subject,
) -> Result<Option<(rasm_linework::Text, Option<Backdrop>)>, CadError> {
    let attributed = |attribute: &AttributeEntity| Text {
        value: attribute.value.clone(),
        insertion_point: attribute.insertion_point,
        alignment_point: Some(attribute.alignment_point),
        height: attribute.height,
        rotation: attribute.rotation,
        width_factor: attribute.width_factor,
        oblique_angle: attribute.oblique_angle,
        style: attribute.text_style.clone(),
        horizontal_alignment: match attribute.horizontal_alignment {
            HorizontalAlignment::Left => TextHorizontalAlignment::Left,
            HorizontalAlignment::Center => TextHorizontalAlignment::Center,
            HorizontalAlignment::Right => TextHorizontalAlignment::Right,
            HorizontalAlignment::Aligned => TextHorizontalAlignment::Aligned,
            HorizontalAlignment::Middle => TextHorizontalAlignment::Middle,
            HorizontalAlignment::Fit => TextHorizontalAlignment::Fit,
        },
        vertical_alignment: match attribute.vertical_alignment {
            VerticalAlignment::Baseline => TextVerticalAlignment::Baseline,
            VerticalAlignment::Bottom => TextVerticalAlignment::Bottom,
            VerticalAlignment::Middle => TextVerticalAlignment::Middle,
            VerticalAlignment::Top => TextVerticalAlignment::Top,
        },
        normal: attribute.normal,
        generation_flags: attribute.text_generation_flags,
        ..Text::new()
    };
    let lettered = |text: &Text| single(text, styles, families, frame, subject).map(|lettering| Some((lettering, None)));
    let paragraphed = |mtext: &MText| multi(mtext, styles, families, frame, tolerance, subject).map(Some);
    match entity {
        EntityType::Text(text) => lettered(text),
        EntityType::MText(mtext) => paragraphed(mtext),
        EntityType::AttributeEntity(attribute) if !attribute.is_visible() => Ok(None),
        EntityType::AttributeEntity(attribute) => attribute.embedded_mtext.as_deref().map_or_else(|| lettered(&attributed(attribute)), paragraphed),
        EntityType::AttributeDefinition(definition) => lettered(&attributed(&AttributeEntity::from_definition(definition, Some(definition.tag.clone())))),
        EntityType::Point(_)
        | EntityType::Line(_)
        | EntityType::Circle(_)
        | EntityType::Arc(_)
        | EntityType::Ellipse(_)
        | EntityType::Polyline(_)
        | EntityType::Polyline2D(_)
        | EntityType::Polyline3D(_)
        | EntityType::LwPolyline(_)
        | EntityType::Spline(_)
        | EntityType::Helix(_)
        | EntityType::Dimension(_)
        | EntityType::Hatch(_)
        | EntityType::Solid(_)
        | EntityType::Face3D(_)
        | EntityType::Insert(_)
        | EntityType::Block(_)
        | EntityType::BlockEnd(_)
        | EntityType::Ray(_)
        | EntityType::XLine(_)
        | EntityType::Viewport(_)
        | EntityType::Leader(_)
        | EntityType::MultiLeader(_)
        | EntityType::MLine(_)
        | EntityType::Mesh(_)
        | EntityType::RasterImage(_)
        | EntityType::Solid3D(_)
        | EntityType::Region(_)
        | EntityType::Body(_)
        | EntityType::Surface(_)
        | EntityType::Table(_)
        | EntityType::Tolerance(_)
        | EntityType::PolyfaceMesh(_)
        | EntityType::Wipeout(_)
        | EntityType::Shape(_)
        | EntityType::Underlay(_)
        | EntityType::Seqend(_)
        | EntityType::Ole2Frame(_)
        | EntityType::PolygonMesh(_)
        | EntityType::Light(_)
        | EntityType::SectionSymbol(_)
        | EntityType::ViewBorder(_)
        | EntityType::Extended(_)
        | EntityType::Unknown(_) => Ok(None),
    }
}

#[must_use]
pub(super) fn written(text: &rasm_linework::Text, frame: Affine) -> (EntityType, TextStyle) {
    let local = frame * *text.placement * Affine::FLIP_Y;
    let [x_x, x_y, y_x, y_y, origin_x, origin_y] = local.as_coeffs();
    let (axis, rise, origin) = (Vec2::new(x_x, x_y), Vec2::new(y_x, y_y), Point::new(origin_x, origin_y));
    let determinant = local.determinant();
    let (area, mirrored) = (determinant.abs(), determinant.is_sign_negative());
    let (scale, width, slant) = (area / axis.hypot(), axis.hypot2() / area, axis.dot(rise) / area);
    let size = text.size.into_inner();
    let face = match &text.family {
        FontFamily::Source(source) => FontFamilyName::parse(source).map(FontFamilyName::into_owned),
        FontFamily::Single(name) => Some(name.clone()),
        FontFamily::List(names) => names.first().cloned(),
    };
    let style = match face {
        Some(FontFamilyName::Named(face)) => TextStyle { font_file: String::new(), ..TextStyle::with_truetype(face.clone(), face) },
        Some(FontFamilyName::Generic(_)) | None => TextStyle::standard(),
    };
    let properties = |range: Range<usize>| {
        let covering = || text.spans.iter().rev().filter(move |span| span.range.start <= range.start && range.end <= span.range.end);
        let bold = parlance::FontWeight::from(covering().find_map(|span| span.weight).unwrap_or(text.weight)) > parlance::FontWeight::MEDIUM;
        let italic = !matches!(covering().find_map(|span| span.style).unwrap_or(text.style), FontStyle::Normal);
        SpanProperties {
            font: (bold || italic).then(|| MTextFont::with_flags(style.true_type_font.clone(), bold, italic)),
            width_factor: Some(width).filter(|factor| (factor - 1.0).abs() > f64::EPSILON * 64.0),
            oblique_angle: Some(slant.atan().to_degrees()).filter(|angle| angle.abs() > f64::EPSILON * 64.0),
            ..SpanProperties::default()
        }
    };
    let content: &str = &text.content;
    let paragraphs = content
        .split('\n')
        .scan(0_usize, |start, line| {
            let begin = *start;
            *start = begin.strict_add(line.len()).strict_add(1);
            Some((begin, begin.strict_add(line.len())))
        })
        .map(|(begin, end)| {
            let cuts = iter::chain([begin, end], text.spans.iter().flat_map(|span| [span.range.start, span.range.end]))
                .filter(|cut| (begin..=end).contains(cut) && content.is_char_boundary(*cut))
                .collect::<BTreeSet<_>>();
            MTextParagraph {
                spans: cuts.iter().zip(cuts.iter().skip(1)).filter_map(|(&low, &high)| content.get(low..high).map(|piece| MTextSpan::new(piece, properties(Range { start: low, end: high })))).collect(),
                ..MTextParagraph::new()
            }
        })
        .collect::<Vec<_>>();
    let entity = match (paragraphs.as_slice(), text.max_advance) {
        ([paragraph], None) if paragraph.spans.iter().all(|span| span.properties.font.is_none()) => {
            let horizontal = match text.anchor {
                Anchor::Start => TextHorizontalAlignment::Left,
                Anchor::Middle => TextHorizontalAlignment::Center,
                Anchor::End => TextHorizontalAlignment::Right,
            };
            let vertical = match text.baseline {
                Baseline::Top => TextVerticalAlignment::Top,
                Baseline::Middle => TextVerticalAlignment::Middle,
                Baseline::Alphabetic => TextVerticalAlignment::Baseline,
                Baseline::Bottom => TextVerticalAlignment::Bottom,
            };
            let insertion = Vector3::new(origin.x, origin.y, 0.0);
            EntityType::Text(Text {
                value: content.to_owned(),
                insertion_point: insertion,
                alignment_point: (!matches!((horizontal, vertical), (TextHorizontalAlignment::Left, TextVerticalAlignment::Baseline))).then_some(insertion),
                height: size * scale,
                rotation: if mirrored { (-axis).atan2() } else { axis.atan2() },
                width_factor: width,
                oblique_angle: slant.atan(),
                style: style.name.clone(),
                horizontal_alignment: horizontal,
                vertical_alignment: vertical,
                generation_flags: if mirrored { 2 } else { 0 },
                ..Text::new()
            })
        }
        _ => {
            let insertion = if matches!(text.baseline, Baseline::Alphabetic) { local * Point::new(0.0, size) } else { origin };
            let direction = axis.normalize();
            EntityType::MText(Box::new(MText {
                value: MTextDocument { paragraphs }.to_mtext_string(),
                insertion_point: Vector3::new(insertion.x, insertion.y, 0.0),
                height: size * scale,
                rectangle_width: text.max_advance.map_or(0.0, |advance| advance.into_inner() * axis.hypot()),
                rotation: axis.atan2(),
                dwg_x_direction: Some(Vector3::new(direction.x, direction.y, 0.0)),
                style: style.name.clone(),
                attachment_point: match (text.baseline, text.anchor) {
                    (Baseline::Top | Baseline::Alphabetic, Anchor::Start) => AttachmentPoint::TopLeft,
                    (Baseline::Top | Baseline::Alphabetic, Anchor::Middle) => AttachmentPoint::TopCenter,
                    (Baseline::Top | Baseline::Alphabetic, Anchor::End) => AttachmentPoint::TopRight,
                    (Baseline::Middle, Anchor::Start) => AttachmentPoint::MiddleLeft,
                    (Baseline::Middle, Anchor::Middle) => AttachmentPoint::MiddleCenter,
                    (Baseline::Middle, Anchor::End) => AttachmentPoint::MiddleRight,
                    (Baseline::Bottom, Anchor::Start) => AttachmentPoint::BottomLeft,
                    (Baseline::Bottom, Anchor::Middle) => AttachmentPoint::BottomCenter,
                    (Baseline::Bottom, Anchor::End) => AttachmentPoint::BottomRight,
                },
                line_spacing_factor: text.line_height.map_or(1.0, |pitch| pitch.into_inner() * 3.0 / (size * 5.0)),
                normal: if mirrored { Vector3::new(0.0, 0.0, -1.0) } else { Vector3::UNIT_Z },
                ..MText::new()
            }))
        }
    };
    (entity, style)
}

fn single(text: &Text, styles: &Table<TextStyle>, families: &[FontFamilyName<'static>], frame: Affine, subject: &Subject) -> Result<rasm_linework::Text, CadError> {
    let (size, family, weight, style) = styled(styles.get(&text.style), families, text.height, subject)?;
    let planar = |vector: Vector3| Point::new(vector.x, vector.y);
    let first = planar(text.insertion_point);
    let (anchor, vertical) = match text.horizontal_alignment {
        TextHorizontalAlignment::Left => (Anchor::Start, text.vertical_alignment),
        TextHorizontalAlignment::Center | TextHorizontalAlignment::Aligned | TextHorizontalAlignment::Fit => (Anchor::Middle, text.vertical_alignment),
        TextHorizontalAlignment::Right => (Anchor::End, text.vertical_alignment),
        TextHorizontalAlignment::Middle => (Anchor::Middle, TextVerticalAlignment::Middle),
    };
    let (position, rotation) = match (text.horizontal_alignment, text.vertical_alignment, text.alignment_point.map(planar)) {
        (TextHorizontalAlignment::Aligned | TextHorizontalAlignment::Fit, _, Some(second)) if second != first => (first.midpoint(second), (second - first).atan2()),
        (TextHorizontalAlignment::Left, TextVerticalAlignment::Baseline, _) | (_, _, None) => (first, text.rotation),
        (_, _, Some(second)) => (second, text.rotation),
    };
    let flipped = |bit: i16| if text.generation_flags & bit == 0 { 1.0 } else { -1.0 };
    let (baseline, shift) = seated(vertical, size);
    Placement::try_new(
        frame
            * ocs(text.normal, text.insertion_point.z)
            * Affine::translate(position.to_vec2())
            * Affine::rotate(rotation)
            * Affine::scale_non_uniform(flipped(2), flipped(4))
            * Affine::skew(text.oblique_angle.tan(), 0.0)
            * Affine::scale_non_uniform(text.width_factor, 1.0)
            * shift
            * Affine::FLIP_Y,
    )
    .map(|placement| rasm_linework::Text { content: Arc::from(parse_plain_text(&text.value).to_plain_text()), family, size, placement, weight, style, anchor, baseline, .. })
    .map_err(|source| CadError::Geometry { subject: subject.clone(), source })
}

fn multi(mtext: &MText, styles: &Table<TextStyle>, families: &[FontFamilyName<'static>], frame: Affine, tolerance: Tolerance, subject: &Subject) -> Result<(rasm_linework::Text, Option<Backdrop>), CadError> {
    let entry = styles.get(&mtext.style);
    let (size, family, weight, style) = styled(entry, families, mtext.height, subject)?;
    let widening = entry.map(|found| found.width_factor).filter(|factor| *factor > 0.0).unwrap_or(1.0);
    let (anchor, vertical) = match mtext.attachment_point {
        AttachmentPoint::TopLeft => (Anchor::Start, TextVerticalAlignment::Top),
        AttachmentPoint::TopCenter => (Anchor::Middle, TextVerticalAlignment::Top),
        AttachmentPoint::TopRight => (Anchor::End, TextVerticalAlignment::Top),
        AttachmentPoint::MiddleLeft => (Anchor::Start, TextVerticalAlignment::Middle),
        AttachmentPoint::MiddleCenter => (Anchor::Middle, TextVerticalAlignment::Middle),
        AttachmentPoint::MiddleRight => (Anchor::End, TextVerticalAlignment::Middle),
        AttachmentPoint::BottomLeft => (Anchor::Start, TextVerticalAlignment::Bottom),
        AttachmentPoint::BottomCenter => (Anchor::Middle, TextVerticalAlignment::Bottom),
        AttachmentPoint::BottomRight => (Anchor::End, TextVerticalAlignment::Bottom),
    };
    let (baseline, shift) = seated(vertical, size);
    let angle = mtext.dwg_x_direction.map_or(mtext.rotation, |direction| {
        let Matrix3 { m: [[x_x, y_x, _], [x_y, y_y, _], [x_z, y_z, _]] } = Matrix3::arbitrary_axis(mtext.normal);
        direction.dot(&Vector3::new(y_x, y_y, y_z)).atan2(direction.dot(&Vector3::new(x_x, x_y, x_z)))
    });
    let plane = frame * Affine::translate((mtext.insertion_point.x, mtext.insertion_point.y)) * ocs(mtext.normal, 0.0) * Affine::rotate(angle);
    let pieces = parse_mtext(&mtext.value, true)
        .paragraphs
        .into_iter()
        .enumerate()
        .flat_map(|(index, paragraph)| {
            iter::chain(
                (index > 0).then(|| (String::from("\n"), None)),
                paragraph.spans.into_iter().map(|span| (span.stacking.map_or(span.text, |stacking| stacking.to_plain_text()), span.properties.font)),
            )
        })
        .collect::<Vec<_>>();
    let spans = pieces
        .iter()
        .scan(0_usize, |start, (piece, font)| {
            let range = Range { start: *start, end: start.strict_add(piece.len()) };
            *start = range.end;
            Some((range, font.as_ref()))
        })
        .filter_map(|(range, font)| {
            font.filter(|font| font.bold_explicit || font.italic_explicit).map(|font| {
                Span { range, weight: font.bold_explicit.then(|| weighted(font.bold)), style: font.italic_explicit.then(|| slanted(font.italic)), .. }
            })
        })
        .collect();
    let fill = match (mtext.background_fill_flags & 2, mtext.background_fill_flags & 1) {
        (0, 0) => None,
        (0, _) => Some(mtext.background_color),
        _ => Some(Color::from_rgb(u8::MAX, u8::MAX, u8::MAX)),
    };
    let wide = [mtext.rectangle_width, mtext.extents_width].into_iter().find(|extent| *extent > 0.0);
    let tall = iter::chain([mtext.extents_height], mtext.rectangle_height).find(|extent| *extent > 0.0);
    let geometry = |source| CadError::Geometry { subject: subject.clone(), source };
    let background = fill
        .zip(wide.zip(tall))
        .map(|(color, (wide, tall))| {
            let margin = (mtext.background_scale - 1.0) * size.into_inner();
            let share = match anchor {
                Anchor::Start => 0.0,
                Anchor::Middle => 0.5,
                Anchor::End => 1.0,
            };
            let left = -wide * share;
            let top = match vertical {
                TextVerticalAlignment::Baseline | TextVerticalAlignment::Top => 0.0,
                TextVerticalAlignment::Middle => tall / 2.0,
                TextVerticalAlignment::Bottom => tall,
            };
            Region::new(&(plane * Rect::new(left - margin, top - tall - margin, left + wide + margin, top + margin).to_path(tolerance.into_inner())), FillRule::NonZero, tolerance).map(|region| (region, color))
        })
        .transpose()
        .map_err(geometry)?;
    Ok((
        rasm_linework::Text {
            content: pieces.into_iter().map(|(piece, _)| piece).collect::<String>().into(),
            family,
            size,
            placement: Placement::try_new(plane * Affine::scale_non_uniform(widening, 1.0) * shift * Affine::FLIP_Y).map_err(geometry)?,
            weight,
            style,
            anchor,
            baseline,
            max_advance: Width::try_new(mtext.rectangle_width / widening).ok().filter(|advance| advance.into_inner() > 0.0),
            line_height: Some(mtext.line_spacing_factor).filter(|factor| *factor > 0.0).and_then(|factor| Width::try_new(size.into_inner() * factor * 5.0 / 3.0).ok()),
            spans,
            ..
        },
        background,
    ))
}

fn styled(style: Option<&TextStyle>, families: &[FontFamilyName<'static>], height: f64, subject: &Subject) -> Result<(FontSize, FontFamily<'static>, FontWeight, FontStyle), CadError> {
    const ITALIC: i32 = 0x0100_0000;
    const BOLD: i32 = 0x0200_0000;
    let flagged = |bit: i32| style.is_some_and(|style| style.true_type_font_flags & bit != 0);
    FontSize::try_new(Some(height).filter(|stated| *stated > 0.0).or_else(|| style.map(|style| style.height)).unwrap_or_default())
        .map(|size| {
            let face = style.map(|style| style.true_type_font.trim()).filter(|face| !face.is_empty()).map(|face| FontFamilyName::Named(Cow::Owned(face.to_owned())));
            (size, FontFamily::List(face.into_iter().chain(families.iter().cloned()).collect()), weighted(flagged(BOLD)), slanted(flagged(ITALIC)))
        })
        .map_err(|(FontSizeError::FiniteViolated | FontSizeError::GreaterViolated)| CadError::Refused { subject: subject.clone(), reason: Refusal::Height })
}

const fn weighted(bold: bool) -> FontWeight {
    if bold { const { FontWeight::try_new(parlance::FontWeight::BOLD.value()).ok().unwrap() } } else { const { FontWeight::try_new(parlance::FontWeight::NORMAL.value()).ok().unwrap() } }
}

const fn slanted(italic: bool) -> FontStyle {
    if italic { FontStyle::Italic } else { FontStyle::Normal }
}

fn seated(vertical: TextVerticalAlignment, size: FontSize) -> (Baseline, Affine) {
    match vertical {
        TextVerticalAlignment::Baseline => (Baseline::Alphabetic, Affine::IDENTITY),
        TextVerticalAlignment::Bottom => (Baseline::Bottom, Affine::IDENTITY),
        TextVerticalAlignment::Middle => (Baseline::Middle, Affine::IDENTITY),
        TextVerticalAlignment::Top => (Baseline::Alphabetic, Affine::translate((0.0, -size.into_inner()))),
    }
}
