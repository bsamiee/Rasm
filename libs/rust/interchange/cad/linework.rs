use core::{iter, mem, slice};
use std::{borrow::Cow, sync::Arc};

use acadrust::{
    CadDocument, Color, DxfVersion, Handle, LineWeight, Transparency, Vector2, Vector3,
    entities::{AttributeEntity, EntityCommon, EntityType, Hatch, HatchPattern, HatchPatternLine, HatchPatternType, Insert, Viewport},
    objects::{KnownXRecordKind, ObjectType, XRecordValue},
    plot_style::PlotStyleTable,
    tables::{Layer, LayerFlags, LineType, LineTypeElement, TextStyle},
    types::Transform,
};
use color::{OpaqueColor, Rgba8, Srgb};
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Circle, PathEl, Point, Rect, Shape as _, Vec2};
use linesweeper::FillRule;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use parlance::{FontFamily, FontFamilyName, GenericFamily};
use rasm_linework::{
    Alignment, Dash, DashOffset, DashOffsetError, DashesError, Descriptor, Element, Family, FamilyError, Linetype, LineworkError, Opacity, OpacityError, Path, Pattern, Pen, Placement, Region, Side, Target,
    Tolerance, Visibility, Width, WidthError, cleanup, pages,
};
use uom::si::{f64::Length, length::millimeter};
use validated::Validated;

use crate::{CadError, Operation, Refusal, Subject, Unit};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Projection {
    pub tolerance: Tolerance,
    pub unit: Option<Unit> = None,
    pub plot_styles: Vec<PlotStyleTable> = Vec::new(),
    pub lineweight: Option<Width> = None,
    pub families: Cow<'static, [FontFamilyName<'static>]> = Cow::Borrowed(&[FontFamilyName::Generic(GenericFamily::SansSerif)]),
}

#[derive(Debug, Clone, Copy)]
pub struct Authoring {
    pub tolerance: Tolerance,
    pub unit: Unit = Unit::Millimeters,
    pub version: DxfVersion = DxfVersion::AC1032,
}

#[derive(Debug, Clone, Copy)]
pub(crate) struct Context<'source> {
    pub(crate) frame: Affine,
    pub(crate) table: Option<&'source PlotStyleTable>,
    pub(crate) viewport: Option<&'source Viewport>,
    pub(crate) drafting: Option<f64>,
    pub(crate) view_height: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn layers(document: &mut CadDocument, projection: &Projection) -> Result<Vec<pages::Layer>, NEVec<CadError>> {
    let code = document.header.insertion_units;
    let unit = projection.unit.or_else(|| Unit::from_repr(code)).ok_or(CadError::Unit { code }).map_err(NEVec::new)?;
    let model = document.header.model_space_block_handle;
    let sheet = document.objects.values().find_map(|object| if let ObjectType::Layout(layout) = object && layout.block_record == model { Some(layout.plot_style_sheet.as_str()) } else { None });
    let context = Context {
        frame: Affine::scale(Length::from(unit).get::<millimeter>()),
        table: sheet.and_then(|sheet| projection.plot_styles.iter().find(|table| table.name.eq_ignore_ascii_case(sheet))),
        viewport: None,
        drafting: None,
        view_height: document.vports.get("*Active").map_or(document.header.model_space_extents_max.y - document.header.model_space_extents_min.y, |active| active.view_height),
    };
    projected(document, "*Model_Space", &context, projection)
}

pub fn drawing(layers: Vec<pages::Layer>, authoring: &Authoring) -> Result<CadDocument, NEVec<CadError>> {
    let mut document = authored(authoring);
    written(&mut document, "Model", layers, authoring).map(|_| document)
}

#[must_use]
pub(crate) fn authored(authoring: &Authoring) -> CadDocument {
    let mut document = CadDocument::with_version(authoring.version);
    document.header.insertion_units = authoring.unit as i16;
    document.header.measurement = match authoring.unit {
        Unit::Millimeters
        | Unit::Centimeters
        | Unit::Meters
        | Unit::Kilometers
        | Unit::Angstroms
        | Unit::Nanometers
        | Unit::Microns
        | Unit::Decimeters
        | Unit::Dekameters
        | Unit::Hectometers
        | Unit::Gigameters => 1,
        Unit::Inches
        | Unit::Feet
        | Unit::Miles
        | Unit::Microinches
        | Unit::Mils
        | Unit::Yards
        | Unit::AstronomicalUnits
        | Unit::LightYears
        | Unit::Parsecs
        | Unit::UsSurveyFeet
        | Unit::UsSurveyInches
        | Unit::UsSurveyYards
        | Unit::UsSurveyMiles => 0,
    };
    document.header.paper_space_linetype_scaling = true;
    document.header.linetype_scale = 1.0;
    document
}

pub(crate) fn projected(document: &mut CadDocument, block: &str, context: &Context<'_>, projection: &Projection) -> Result<Vec<pages::Layer>, NEVec<CadError>> {
    let entities = ordered(document, block);
    drawn(document, entities, &[block.to_owned()], None, context, projection).ok()
}

pub(crate) fn written(document: &mut CadDocument, space: &str, layers: Vec<pages::Layer>, authoring: &Authoring) -> Result<Vec<Handle>, NEVec<CadError>> {
    let unit = Affine::scale(Length::from(authoring.unit).get::<millimeter>().recip());
    let space = Arc::<str>::from(space);
    layers
        .into_iter()
        .map(|pages::Layer { name, print, view, elements }| -> Result<Handle, NEVec<CadError>> {
            let (off, is_plottable) = (matches!(view, Visibility::Hidden), matches!(print, Visibility::Visible));
            let handle = if let Some(layer) = document.layers.get_mut(&name) {
                layer.flags.off = off;
                layer.is_plottable = is_plottable;
                layer.handle
            } else {
                let handle = document.allocate_handle();
                document
                    .layers
                    .add(Layer { handle, flags: LayerFlags { off, ..LayerFlags::new() }, is_plottable, ..Layer::new(&*name) })
                    .map_err(|source| NEVec::new(CadError::Drawing { operation: Operation::Table(name.to_string()), source }))?;
                handle
            };
            elements
                .into_iter()
                .enumerate()
                .map(|(index, element)| -> Validated<(), CadError> {
                    let subject = Subject::Element { space: Arc::clone(&space), layer: Arc::clone(&name), index };
                    entities(document, element, &name, unit, authoring.tolerance, &subject)
                        .and_then(|entities| {
                            entities.into_iter().try_for_each(|entity| {
                                document.add_entity_to_layout(entity, &space).map(|_| ()).map_err(|source| NEVec::new(CadError::Drawing { operation: Operation::Layout(Arc::clone(&space)), source }))
                            })
                        })
                        .map_or_else(Validated::Fail, Validated::Good)
                })
                .collect::<Validated<(), _>>()
                .ok()
                .map(|()| handle)
        })
        .map(|outcome| outcome.map_or_else(Validated::Fail, Validated::Good))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

fn ordered(document: &CadDocument, block: &str) -> Vec<EntityType> {
    let owner = document.block_records.get(block).map(|record| record.handle);
    let table = document.objects.values().find_map(|object| if let ObjectType::SortEntitiesTable(table) = object && Some(table.block_owner_handle) == owner { Some(table) } else { None });
    let mut entities = document.entities_in_block(block).cloned().collect::<Vec<_>>();
    if let Some(table) = table {
        entities.sort_by_key(|entity| table.effective_sort_handle(entity.common().handle));
    }
    entities
}

fn drawn(document: &mut CadDocument, entities: Vec<EntityType>, chain: &[String], subject: Option<&Subject>, context: &Context<'_>, projection: &Projection) -> Validated<Vec<pages::Layer>, CadError> {
    entities.into_iter().fold(Validated::Good(Vec::new()), |state, entity| {
        let own = Subject::Record { handle: entity.common().handle, kind: entity.as_entity().entity_type() };
        let subject = subject.unwrap_or(&own);
        match state {
            Validated::Good(layers) => drew(document, &entity, layers, chain, subject, context, projection).map_or_else(Validated::Fail, Validated::Good),
            Validated::Fail(mut errors) => {
                if let Err(more) = drew(document, &entity, Vec::new(), chain, subject, context, projection) {
                    errors.extend(more);
                }
                Validated::Fail(errors)
            }
        }
    })
}

fn drew(
    document: &mut CadDocument,
    entity: &EntityType,
    mut earlier: Vec<pages::Layer>,
    chain: &[String],
    subject: &Subject,
    context: &Context<'_>,
    projection: &Projection,
) -> Result<Vec<pages::Layer>, NEVec<CadError>> {
    let common = entity.common();
    let layer = document.layers.get(&common.layer).cloned().unwrap_or_else(|| Layer::new(common.layer.clone()));
    let (frame, tolerance) = (context.frame, projection.tolerance);
    let name = Arc::<str>::from(layer.name.as_str());
    let print = if layer.is_plottable && !layer.name.eq_ignore_ascii_case("DEFPOINTS") { Visibility::Visible } else { Visibility::Hidden };
    let view = if layer.flags.off { Visibility::Hidden } else { Visibility::Visible };
    let piece = |elements: Vec<Element>| vec![pages::Layer { name: Arc::clone(&name), print, view, elements }];
    let refusing = |reason| CadError::Refused { subject: subject.clone(), reason };
    let misshapen = |source| CadError::Geometry { subject: subject.clone(), source };
    let penned = || Validated::from(pen(document, common, &layer, context, projection).map_err(refusing));
    let stroked = |outline: Option<BezPath>| -> Result<Vec<pages::Layer>, NEVec<CadError>> {
        outline.map_or(Ok(Vec::new()), |outline| {
            Validated::from(Path::try_new(outline).map_err(|source| misshapen(LineworkError::from(source))))
                .map3(penned(), Validated::from(linetype(document, common, &layer, context).map_err(refusing)), |path, pen, linetype| piece(vec![Element::Stroke { path, pen, linetype }]))
                .ok()
        })
    };
    let filled = |region: Region| penned().map(|pen| piece(vec![Element::Fill { region, color: pen.color, opacity: pen.opacity }])).ok();
    let outlined = || curves::path(entity, frame, tolerance).map_err(|reason| NEVec::new(refusing(reason))).and_then(stroked);
    let polygon = |corners: &[Vector3], plane: Affine| {
        let mut points = corners.iter().map(|corner| plane * Point::new(corner.x, corner.y));
        Region::new(&iter::chain(points.next().map(PathEl::MoveTo), points.map(PathEl::LineTo)).collect::<BezPath>(), FillRule::NonZero, tolerance).map_err(|source| NEVec::new(misshapen(source)))
    };
    let hidden = layer.flags.frozen || common.invisible || context.viewport.is_some_and(|viewport| viewport.frozen_layers.contains(&layer.handle));
    match entity {
        _ if hidden => Ok(Vec::new()),
        EntityType::Line(_) | EntityType::Arc(_) | EntityType::Circle(_) | EntityType::Ellipse(_) | EntityType::Spline(_) | EntityType::Helix(_) | EntityType::Face3D(_) => outlined(),
        EntityType::LwPolyline(_) | EntityType::Polyline2D(_) | EntityType::Polyline(_) | EntityType::Polyline3D(_) => match curves::band(entity, frame, tolerance) {
            Ok(Some(region)) => filled(region),
            Ok(None) => outlined(),
            Err(source) => Err(NEVec::new(misshapen(source))),
        },
        EntityType::Text(_) | EntityType::MText(_) | EntityType::AttributeEntity(_) | EntityType::AttributeDefinition(_) => {
            text::lifted(entity, &document.text_styles, &projection.families, frame, tolerance, subject)
                .map_or_else(Validated::Fail, Validated::Good)
                .map2(penned(), |lettering, pen| {
                    lettering.map_or_default(|text::Lettering { text, background }| {
                        piece(
                            iter::chain(
                                background.map(|(region, color)| Element::Fill { region, color: rasm_linework::Color::Srgb(plotted(document, color, &layer, context).0), opacity: pen.opacity }),
                                iter::once(Element::Text { text, color: pen.color, opacity: pen.opacity }),
                            )
                            .collect(),
                        )
                    })
                })
                .ok()
        }
        EntityType::Hatch(hatch) if hatch.gradient_color.is_enabled() => Err(NEVec::new(refusing(Refusal::Gradient))),
        EntityType::Hatch(hatch) if hatch.is_solid => {
            Validated::from(curves::boundary(hatch, frame, tolerance, subject)).map2(penned(), |region, pen| piece(vec![Element::Fill { region, color: pen.color, opacity: pen.opacity }])).ok()
        }
        EntityType::Hatch(hatch) => {
            let family = |(index, line): (usize, &HatchPatternLine)| -> Result<Family, CadError> {
                let dashed = curves::dashes(&line.dash_lengths, 1.0).map_err(|DashesError::PredicateViolated| misshapen(LineworkError::Scaled(Dash::Array)))?;
                let delta = (Affine::rotate(-line.angle) * Point::new(line.offset.x, line.offset.y)).to_vec2();
                let shift = dashed.as_ref().map_or_default(|&(_, offset)| offset);
                Family::try_new(Descriptor { angle: line.angle, origin: Point::new(line.base_point.x, line.base_point.y) - Vec2::from_angle(line.angle) * shift, delta, dashes: dashed.map(|(dashes, _)| dashes) })
                    .map_err(|FamilyError::PredicateViolated| misshapen(LineworkError::Spacing { family: index, spacing: delta.y }))
            };
            Validated::from(curves::boundary(hatch, frame, tolerance, subject))
                .map4(
                    penned(),
                    Validated::from(Placement::try_new(frame * curves::ocs(hatch.normal, hatch.elevation)).map_err(misshapen)),
                    hatch.pattern.lines.iter().enumerate().map(family).collect::<Validated<Vec<_>, _>>(),
                    |region, pen, placement, families| {
                        NEVec::try_from_vec(families).map_or_default(|families| piece(vec![Element::Hatch { region, pattern: Pattern { families, target: Target::Model }, placement, pen }]))
                    },
                )
                .ok()
        }
        EntityType::Solid(solid) => polygon(&[solid.first_corner, solid.second_corner, solid.fourth_corner, solid.third_corner], frame * curves::ocs(solid.normal, solid.first_corner.z)).and_then(filled),
        EntityType::Point(marker) => {
            let header = &document.header;
            let percent = |share: f64| context.view_height * share / 100.0;
            let size = match header.point_display_size {
                stated if stated > 0.0 => stated,
                stated if stated < 0.0 => percent(-stated),
                _ => percent(5.0),
            };
            let (center, half, mode, local) = (Point::new(marker.location.x, marker.location.y), size / 2.0, header.point_display_mode, tolerance.into_inner() / frame.spectral_norm());
            let line = |from: Vec2, to: Vec2| [PathEl::MoveTo(center + from), PathEl::LineTo(center + to)];
            let figure = match mode.rem_euclid(32) {
                0 => line(Vec2::ZERO, Vec2::ZERO).to_vec(),
                2 => [line(Vec2::new(-half, 0.0), Vec2::new(half, 0.0)), line(Vec2::new(0.0, -half), Vec2::new(0.0, half))].concat(),
                3 => [line(Vec2::new(-half, -half), Vec2::new(half, half)), line(Vec2::new(-half, half), Vec2::new(half, -half))].concat(),
                4 => line(Vec2::ZERO, Vec2::new(0.0, half)).to_vec(),
                i16::MIN..=-1 | 1 | 5..=i16::MAX => Vec::new(),
            };
            let circle = ((mode & 32) != 0).then(|| Circle::new(center, half).path_elements(local).collect::<Vec<_>>());
            let square = ((mode & 64) != 0).then(|| Rect::from_center_size(center, (size, size)).path_elements(local).collect::<Vec<_>>());
            let elements = iter::chain(figure, circle.into_iter().flatten()).chain(square.into_iter().flatten()).collect::<BezPath>();
            stroked((!elements.is_empty()).then(|| frame * elements))
        }
        EntityType::Wipeout(wipeout) => polygon(&if wipeout.is_rectangular() { wipeout.corners().to_vec() } else { wipeout.world_boundary_vertices() }, frame).map(|region| {
            mem::take(&mut earlier)
                .into_iter()
                .map(|shown| pages::Layer { elements: vec![Element::Clip { region: region.clone(), keep: Side::Outside, elements: shown.elements }], ..shown })
                .collect()
        }),
        EntityType::Insert(insert) => inserted(document, insert, chain, subject, context, projection),
        EntityType::Dimension(dimension) if document.block_records.contains(&dimension.base().block_name) => {
            let mut insert = Insert::new(dimension.base().block_name.clone(), Vector3::ZERO);
            insert.common = dimension.base().common.clone();
            inserted(document, &insert, chain, subject, context, projection)
        }
        EntityType::Dimension(_)
        | EntityType::MLine(_)
        | EntityType::MultiLeader(_)
        | EntityType::Leader(_)
        | EntityType::Table(_)
        | EntityType::Mesh(_)
        | EntityType::PolyfaceMesh(_)
        | EntityType::PolygonMesh(_)
        | EntityType::Region(_)
        | EntityType::Solid3D(_)
        | EntityType::Body(_)
        | EntityType::Surface(_) => match document.explode_entity(entity) {
            [] => Err(NEVec::new(refusing(Refusal::Unsupported))),
            parts => drawn(document, parts, chain, Some(subject), context, projection).ok(),
        },
        EntityType::Light(_) | EntityType::Viewport(_) | EntityType::Block(_) | EntityType::BlockEnd(_) | EntityType::Seqend(_) => Ok(Vec::new()),
        EntityType::RasterImage(_)
        | EntityType::Underlay(_)
        | EntityType::Ole2Frame(_)
        | EntityType::Shape(_)
        | EntityType::Tolerance(_)
        | EntityType::SectionSymbol(_)
        | EntityType::ViewBorder(_)
        | EntityType::Extended(_)
        | EntityType::Unknown(_)
        | EntityType::Ray(_)
        | EntityType::XLine(_) => Err(NEVec::new(refusing(Refusal::Unsupported))),
    }
    .map(|more| {
        more.into_iter().fold(earlier, |mut layers, added| {
            if let Some(last) = layers.last_mut()
                && last.name == added.name
                && last.print == added.print
                && last.view == added.view
            {
                last.elements.extend(added.elements);
            } else {
                layers.push(added);
            }
            layers
        })
    })
}

fn inserted(document: &mut CadDocument, insert: &Insert, chain: &[String], subject: &Subject, context: &Context<'_>, projection: &Projection) -> Result<Vec<pages::Layer>, NEVec<CadError>> {
    let refused = |reason| Err(NEVec::new(CadError::Refused { subject: subject.clone(), reason }));
    match document.block_records.get(&insert.block_name) {
        _ if chain.iter().any(|name| name.eq_ignore_ascii_case(&insert.block_name)) => refused(Refusal::Cycle),
        None => refused(Refusal::Undefined),
        Some(record) => {
            let base = if let Some(EntityType::Block(block)) = document.get_entity(record.block_entity_handle) { block.base_point } else { record.base_point };
            let block = ordered(document, &insert.block_name)
                .into_iter()
                .map(|mut entity| {
                    entity.as_entity_mut().apply_transform(&Transform::from_translation(-base));
                    entity
                })
                .collect::<Vec<_>>();
            match insert.explode(&block) {
                parts if parts.is_empty() && !block.is_empty() => refused(Refusal::Unsupported),
                parts => {
                    let shown = parts
                        .into_iter()
                        .filter_map(|part| match part {
                            EntityType::AttributeDefinition(definition) => {
                                definition.is_constant().then(|| EntityType::AttributeEntity(AttributeEntity::from_definition(&definition, Some(definition.default_value.clone()))))
                            }
                            shown => Some(shown),
                        })
                        .chain(insert.attributes.iter().cloned().map(EntityType::AttributeEntity))
                        .collect();
                    drawn(document, shown, &[chain, slice::from_ref(&insert.block_name)].concat(), Some(subject), context, projection).ok()
                }
            }
        }
    }
}

fn overridden(document: &CadDocument, layer: &Layer, context: &Context<'_>, kind: KnownXRecordKind) -> Option<XRecordValue> {
    context
        .viewport
        .and_then(|viewport| document.layer_viewport_overrides(layer.handle, kind).into_iter().find_map(|(handle, value)| (handle == viewport.common.handle).then_some(value)))
}

fn plotted(document: &CadDocument, color: Color, layer: &Layer, context: &Context<'_>) -> (OpaqueColor<Srgb>, Option<f64>) {
    let resolved = match color {
        Color::ByLayer => overridden(document, layer, context, KnownXRecordKind::LayerViewportColorOverride)
            .and_then(|value| if let XRecordValue::Int32(packed) = value { Some(Color::from_true_color_value(packed)) } else { None })
            .unwrap_or(layer.color),
        Color::None | Color::ByBlock => Color::WHITE,
        Color::Index(_) | Color::Rgb { r: _, g: _, b: _ } => color,
    };
    let (index, object) = match resolved {
        Color::Rgb { r, g, b } => (None, OpaqueColor::from_rgb8(r, g, b)),
        Color::Index(index) if resolved == Color::WHITE => (Some(index), OpaqueColor::BLACK),
        Color::Index(index) => (Some(index), resolved.rgb().map_or(OpaqueColor::BLACK, |(r, g, b)| OpaqueColor::from_rgb8(r, g, b))),
        Color::ByLayer | Color::ByBlock | Color::None => (None, OpaqueColor::BLACK),
    };
    let (plotted, width, screening) = match context.table {
        None => (None, None, 1.0),
        Some(table) if table.is_stb => table.named_entries.get(&layer.plot_style).map_or((None, None, 1.0), |entry| {
            (
                entry.color.map(|[r, g, b]| OpaqueColor::from_rgb8(r, g, b)),
                Some(entry.lineweight)
                    .filter(|&weight| !matches!(weight, 0 | u8::MAX))
                    .and_then(|weight| table.lineweights.get(usize::from(weight)))
                    .map(|weight| if table.apply_factor { weight * table.scale_factor.max(0.0) } else { *weight }),
                f32::from(entry.screening.min(100)) / 100.0,
            )
        }),
        Some(table) => index.map_or((None, None, 1.0), |aci| (table.resolve_color(aci).map(OpaqueColor::new), table.resolve_lineweight(aci), table.resolve_screening(aci))),
    };
    (OpaqueColor::WHITE.lerp_rect(plotted.unwrap_or(object), screening), width.map(f64::from))
}

fn pen(document: &CadDocument, common: &EntityCommon, layer: &Layer, context: &Context<'_>, projection: &Projection) -> Result<Pen, Refusal> {
    let (color, stated) = plotted(document, common.color, layer, context);
    let weight = match common.line_weight {
        LineWeight::ByLayer => overridden(document, layer, context, KnownXRecordKind::LayerViewportLineweightOverride)
            .and_then(|value| if let XRecordValue::Int32(weight) = value { Some(i16::try_from(weight).map(LineWeight::from_value)) } else { None })
            .transpose()?
            .unwrap_or(layer.line_weight),
        LineWeight::ByBlock | LineWeight::Default | LineWeight::Value(_) => common.line_weight,
    };
    let transparency = match common.transparency {
        Transparency::ByLayer => overridden(document, layer, context, KnownXRecordKind::LayerViewportAlphaOverride)
            .and_then(|value| if let XRecordValue::Int32(alpha) = value { Transparency::from_layer_alpha_value(alpha.cast_unsigned()) } else { None })
            .unwrap_or(layer.transparency),
        Transparency::ByBlock | Transparency::Explicit(_) => common.transparency,
    };
    let width = Width::try_new(stated.or_else(|| weight.millimeters()).unwrap_or_else(|| projection.lineweight.map_or(0.25, Width::into_inner)))
        .map_err(|(WidthError::FiniteViolated | WidthError::GreaterOrEqualViolated)| Refusal::Unsupported)?;
    Opacity::try_new(1.0 - transparency.as_percent())
        .map(|opacity| Pen { width, color: rasm_linework::Color::Srgb(color), opacity, .. })
        .map_err(|(OpacityError::FiniteViolated | OpacityError::GreaterOrEqualViolated | OpacityError::LessOrEqualViolated)| Refusal::Unsupported)
}

fn linetype(document: &CadDocument, common: &EntityCommon, layer: &Layer, context: &Context<'_>) -> Result<Option<Linetype>, Refusal> {
    let stated = match &common.linetype.to_ascii_uppercase() {
        "" | "BYLAYER" => overridden(document, layer, context, KnownXRecordKind::LayerViewportLinetypeOverride)
            .and_then(|value| if let XRecordValue::Handle(handle) = value { document.line_types.iter().find(|entry| entry.handle == handle) } else { None })
            .or_else(|| document.line_types.get(&layer.line_type)),
        "BYBLOCK" => None,
        _ => document.line_types.get(&common.linetype),
    };
    let (factor, target) = context.drafting.map_or_else(|| (context.frame.determinant().abs().sqrt(), Target::Model), |paper| (paper, Target::Drafting));
    match stated {
        Some(entry) if entry.is_complex() => Err(Refusal::Linetype),
        Some(entry) => curves::dashes(&entry.elements.iter().map(|element| element.length).collect::<Vec<_>>(), document.header.linetype_scale * common.linetype_scale * factor)
            .map_err(|DashesError::PredicateViolated| Refusal::Linetype)?
            .map(|(dashes, offset)| {
                DashOffset::try_new(offset).map(|dash_offset| Linetype { dashes, alignment: Alignment::Start { dash_offset }, target }).map_err(|DashOffsetError::FiniteViolated| Refusal::Linetype)
            })
            .transpose(),
        None => Ok(None),
    }
}

fn entities(document: &mut CadDocument, element: Element, layer: &str, unit: Affine, tolerance: Tolerance, subject: &Subject) -> Result<Vec<EntityType>, NEVec<CadError>> {
    fn masked(element: &Element) -> bool {
        match element {
            Element::Text { text: _, color: _, opacity: _ } | Element::Glyphs { run: _, color: _, opacity: _ } | Element::Placed { content: _, placement: _, opacity: _ } => true,
            Element::Stroke { path: _, pen: _, linetype: _ } | Element::Fill { region: _, color: _, opacity: _ } | Element::Hatch { region: _, pattern: _, placement: _, pen: _ } => false,
            Element::Clip { region: _, keep: _, elements } => elements.iter().any(masked),
        }
    }
    let refusing = |reason| NEVec::new(CadError::Refused { subject: subject.clone(), reason });
    let misshapen = |source| NEVec::new(CadError::Geometry { subject: subject.clone(), source });
    let inked = |color: &rasm_linework::Color, opacity: Opacity| match color {
        rasm_linework::Color::Srgb(srgb) => {
            let Rgba8 { r, g, b, a: _ } = srgb.to_rgba8();
            Ok((Color::from_rgb(r, g, b), Transparency::from_percent(1.0 - opacity.into_inner())))
        }
        rasm_linework::Color::Cmyk(_) | rasm_linework::Color::Spot { name: _, tint: _, alternate: _ } => Err(refusing(Refusal::Ink)),
    };
    let weighted = |width: Width| {
        (u8::MIN..=u8::MAX)
            .map(LineWeight::from_dwg_index)
            .filter_map(|weight| weight.millimeters().map(|millimeters| (weight, (millimeters - width.into_inner()).abs())))
            .min_by(|(_, left), (_, right)| left.total_cmp(right))
            .map_or(LineWeight::Default, |(weight, _)| weight)
    };
    let placed = |region: &Region| Placement::try_new(unit).and_then(|placement| region.transform(placement)).map_err(misshapen);
    let styled = |mut entity: EntityType, (color, transparency): (Color, Transparency), line_weight: LineWeight, linetype: String| {
        let common = entity.common_mut();
        layer.clone_into(&mut common.layer);
        common.color = color;
        common.transparency = transparency;
        common.line_weight = line_weight;
        common.linetype = linetype;
        entity
    };
    let mut lettered = |text: &rasm_linework::Text, color: &rasm_linework::Color, opacity: Opacity| -> Result<Vec<EntityType>, NEVec<CadError>> {
        let (entity, style) = text::written(text, unit);
        let ink = inked(color, opacity)?;
        if !document.text_styles.contains(&style.name) {
            let name = style.name.clone();
            let handle = document.allocate_handle();
            document.text_styles.add(TextStyle { handle, ..style }).map_err(|source| NEVec::new(CadError::Drawing { operation: Operation::Table(name), source }))?;
        }
        Ok(vec![styled(entity, ink, LineWeight::ByLayer, String::new())])
    };
    match element {
        Element::Stroke { path, pen, linetype } => {
            let ink = inked(&pen.color, pen.opacity)?;
            let named = linetype
                .map(|linetype| -> Result<String, NEVec<CadError>> {
                    let offset = match linetype.alignment {
                        Alignment::Start { dash_offset } => dash_offset.into_inner(),
                        Alignment::Center | Alignment::Corners { angle: _ } => 0.0,
                    };
                    let scale = unit.determinant().abs().sqrt();
                    let lengths = curves::signed(&linetype.dashes, offset).into_iter().map(|length| length * scale).collect::<Vec<_>>();
                    if let Some(entry) = document.line_types.iter().find(|entry| entry.elements.iter().map(|dash| dash.length).eq(lengths.iter().copied())) {
                        Ok(entry.name.clone())
                    } else {
                        let name = lengths.iter().join(" ");
                        let handle = document.allocate_handle();
                        let entry = LineType {
                            handle,
                            elements: lengths.iter().map(|&length| LineTypeElement { length, complex: None }).collect(),
                            pattern_length: lengths.iter().map(|length| length.abs()).sum(),
                            ..LineType::new(name.clone())
                        };
                        document.line_types.add(entry).map(|()| name.clone()).map_err(|source| NEVec::new(CadError::Drawing { operation: Operation::Table(name), source }))
                    }
                })
                .transpose()?;
            Ok(curves::curves(&(unit * path.into_inner())).into_iter().map(|entity| styled(entity, ink, weighted(pen.width), named.clone().unwrap_or_default())).collect())
        }
        Element::Fill { region, color, opacity } => {
            let ink = inked(&color, opacity)?;
            placed(&region).map(|region| vec![styled(EntityType::Hatch(Box::new(Hatch { paths: curves::loops(&region), ..Hatch::solid() })), ink, LineWeight::ByLayer, String::new())])
        }
        Element::Hatch { region, pattern, placement, pen } => {
            let ink = inked(&pen.color, pen.opacity)?;
            let drawing = unit * *placement;
            let [across_x, across_y, up_x, up_y, _, _] = drawing.as_coeffs();
            let (across, up) = (Vec2::new(across_x, across_y), Vec2::new(up_x, up_y));
            let reach = f64::EPSILON * 64.0 * across.hypot2();
            let similar = across.dot(up).abs() <= reach && (across.hypot2() - up.hypot2()).abs() <= reach;
            let (linear, scale) = (drawing.with_translation(Vec2::ZERO), drawing.determinant().abs().sqrt());
            let lines = pattern
                .families
                .iter()
                .map(|family| {
                    let along = (linear * Point::new(family.angle.cos(), family.angle.sin())).to_vec2();
                    let (origin, offset) = (drawing * family.origin, linear * (Affine::rotate(family.angle) * family.delta.to_point()));
                    HatchPatternLine {
                        angle: along.atan2(),
                        base_point: Vector2::new(origin.x, origin.y),
                        offset: Vector2::new(offset.x, offset.y),
                        dash_lengths: family.dashes.as_ref().map_or_default(|dashes| curves::signed(dashes, 0.0).into_iter().map(|length| length * scale).collect()),
                    }
                })
                .collect::<Vec<_>>();
            let rows = iter::zip(&pattern.families, &lines)
                .map(|(family, line)| {
                    iter::chain([line.angle.to_degrees(), line.base_point.x, line.base_point.y, family.delta.x * scale, family.delta.y * scale], line.dash_lengths.iter().copied()).join(",")
                })
                .join(" ");
            if similar {
                placed(&region).map(|region| {
                    let hatch = Hatch {
                        paths: curves::loops(&region),
                        pattern: HatchPattern { lines, ..HatchPattern::new(rows) },
                        pattern_type: HatchPatternType::Custom,
                        is_solid: false,
                        ..Hatch::new()
                    };
                    vec![styled(EntityType::Hatch(Box::new(hatch)), ink, weighted(pen.width), String::new())]
                })
            } else {
                Err(refusing(Refusal::Skew))
            }
        }
        Element::Text { text, color, opacity } => lettered(&text, &color, opacity),
        Element::Glyphs { run, color, opacity } => lettered(
            &rasm_linework::Text {
                content: Arc::clone(&run.text),
                family: FontFamily::Single(FontFamilyName::Named(Cow::Owned(run.family.to_string()))),
                size: run.size,
                placement: run.placement,
                target: run.target,
                ..
            },
            &color,
            opacity,
        ),
        Element::Placed { content: _, placement: _, opacity: _ } => Err(refusing(Refusal::Content)),
        Element::Clip { region: _, keep: _, elements } if elements.iter().any(masked) => Err(refusing(Refusal::Clipped)),
        Element::Clip { region, keep, elements } => cleanup::clip(elements, &region, keep, tolerance)
            .map_err(|errors| errors.into_nonempty_iter().map(|source| CadError::Geometry { subject: subject.clone(), source }).collect::<NEVec<_>>())?
            .into_iter()
            .map(|clipped| entities(document, clipped, layer, unit, tolerance, subject).map_or_else(Validated::Fail, Validated::Good))
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .map(|groups| groups.into_iter().flatten().collect()),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod curves;
mod text;
