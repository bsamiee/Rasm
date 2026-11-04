use core::num::NonZero;
use std::{
    collections::{HashMap, HashSet},
    sync::Arc,
};

use acadrust::{
    CadDocument, Handle, Vector3,
    entities::{Entity as _, EntityType, Viewport, ViewportStatusFlags},
    objects::{Layout, ObjectType, PlotPaperUnits},
    sheet_set::{CUSTOM_SHEET_PROP, LayoutReference, SheetSetDatabase, custom_properties, set_custom_property},
    tables::normalize_name,
};
use kurbo::{Affine, Point, Rect, Shape as _, Size, Vec2};
use linesweeper::FillRule;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_linework::{
    Placement, Region,
    pages::{self, Page},
};
use uom::si::{f64::Length, length::millimeter};
use validated::Validated;

use crate::{
    CadError, Operation, Refusal, Subject, Unit,
    linework::{self, Authoring, Context, Projection, curves},
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const STATED: [&str; 3] = ["Number", "Title", "Desc"];
const REFERENCE: &str = "Layout";

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn pages(document: &mut CadDocument, projection: &Projection, sheets: Option<(&SheetSetDatabase, &str)>) -> Result<Vec<(Arc<str>, Page)>, NEVec<CadError>> {
    let code = document.header.insertion_units;
    let model = Length::from(projection.unit.or_else(|| Unit::from_repr(code)).ok_or(CadError::Unit { code }).map_err(NEVec::new)?).get::<millimeter>();
    let space = document.header.model_space_block_handle;
    let mut layouts = document
        .objects
        .values()
        .filter_map(|object| {
            if let ObjectType::Layout(layout) = object
                && layout.block_record != space
            {
                Some(layout.clone())
            } else {
                None
            }
        })
        .collect::<Vec<_>>();
    layouts.sort_by_key(|layout| layout.tab_order);
    let mut paged = |layout: &Layout| -> Result<(Arc<str>, Page), NEVec<CadError>> {
        let unit = match PlotPaperUnits::from_code(layout.plot_paper_units) {
            PlotPaperUnits::Inches => Ok(Unit::Inches),
            PlotPaperUnits::Millimeters => Ok(Unit::Millimeters),
            PlotPaperUnits::Pixels => Err(NEVec::new(CadError::Refused { subject: Subject::Record { handle: layout.handle, kind: "LAYOUT" }, reason: Refusal::Units })),
        }?;
        let size = if layout.plot_rotation.rem_euclid(2) == 1 { Size::new(layout.paper_height, layout.paper_width) } else { Size::new(layout.paper_width, layout.paper_height) };
        let paper = Context {
            frame: Affine::translate((0.0, size.height))
                * Affine::FLIP_Y
                * Affine::translate((layout.plot_margin_left + layout.plot_origin_x, layout.plot_margin_bottom + layout.plot_origin_y))
                * Affine::scale(Length::from(unit).get::<millimeter>() * layout.plot_scale_numerator / layout.plot_scale_denominator),
            table: projection.plot_styles.iter().find(|table| table.name.eq_ignore_ascii_case(&layout.plot_style_sheet)),
            viewport: None,
            drafting: None,
            view_height: layout.max_extents.1 - layout.min_extents.1,
        };
        let block = document.block_records.iter().find(|record| record.handle == layout.block_record).map(|record| record.name.clone());
        let ports = block.as_deref().map_or_default(|block| {
            document
                .entities_in_block(block)
                .filter_map(|entity| {
                    if let EntityType::Viewport(viewport) = entity
                        && viewport.id > 1
                        && viewport.is_on()
                    {
                        Some(viewport.clone())
                    } else {
                        None
                    }
                })
                .collect::<Vec<_>>()
        });
        let sheet = sheets
            .and_then(|(database, drawing)| database.sheet_for(drawing, Some(&layout.name)))
            .filter(|sheet| sheet.named(REFERENCE).and_then(|reference| reference.prop("Name")).is_some_and(|name| name.eq_ignore_ascii_case(&layout.name)));
        let fields = sheet.map_or_default(|sheet| {
            STATED
                .iter()
                .filter_map(|field| sheet.prop(field).map(|value| (Arc::from(*field), Arc::from(value))))
                .chain(custom_properties(sheet).into_iter().map(|(field, value, _)| (Arc::from(field), Arc::from(value))))
                .collect()
        });
        let drawn = block.as_deref().map_or_else(|| Ok(Vec::new()), |block| linework::projected(document, block, &paper, projection)).map_or_else(Validated::Fail, Validated::Good);
        let mut viewed = |viewport: &Viewport| -> Result<pages::Viewport, NEVec<CadError>> {
            let subject = Subject::Record { handle: viewport.common.handle, kind: viewport.entity_type() };
            let refused = |reason| CadError::Refused { subject: subject.clone(), reason };
            let misshapen = |source| CadError::Geometry { subject: subject.clone(), source };
            let nominal = |geometric: f64| -> pages::Scale {
                let whole = |value: f64| NonZero::new(value.to_int_saturating::<u32>()).filter(|whole| f64::from(whole.get()).total_cmp(&value).is_eq());
                let nearest = |ratio: f64| NonZero::<u32>::MIN.saturating_add(ratio.round().to_int_saturating::<u32>().saturating_sub(1));
                document
                    .viewport_annotation_scale(viewport.common.handle)
                    .and_then(|handle| if let Some(ObjectType::Scale(scale)) = document.objects.get(&handle) { Some(scale) } else { None })
                    .filter(|scale| (scale.paper_units / scale.drawing_units - geometric).abs() <= f64::EPSILON * 64.0 * geometric)
                    .and_then(|scale| whole(scale.paper_units).zip(whole(scale.drawing_units)))
                    .map_or_else(
                        || pages::Scale { paper: nearest(geometric), model: nearest(geometric.recip()) },
                        |(paper_units, drawing_units)| pages::Scale { paper: paper_units, model: drawing_units },
                    )
            };
            let Vector3 { x: across, y: up, z: toward } = viewport.view_direction;
            let mapped = (across == 0.0 && up == 0.0 && toward > 0.0)
                .then(|| {
                    paper.frame
                        * Affine::translate((viewport.center.x, viewport.center.y))
                        * Affine::scale(viewport.scale())
                        * Affine::translate((-viewport.view_center.x, -viewport.view_center.y))
                        * Affine::rotate(viewport.twist_angle)
                        * Affine::translate((-viewport.view_target.x, -viewport.view_target.y))
                        * Affine::scale(model.recip())
                })
                .ok_or_else(|| refused(Refusal::View))
                .and_then(|affine| Placement::try_new(affine).map_err(misshapen))
                .and_then(|mapped| {
                    let scale = nominal(mapped.determinant().abs().sqrt());
                    Placement::try_new(*mapped * Affine::scale(f64::from(scale.model.get()) / f64::from(scale.paper.get()))).map(|placement| (placement, scale)).map_err(misshapen)
                });
            let boundary = if let Some(clip) = document.get_entity(viewport.clip_boundary_handle) {
                curves::path(clip, paper.frame, projection.tolerance)
                    .and_then(|outline| outline.ok_or(Refusal::Unsupported))
                    .map_err(|reason| CadError::Refused { subject: Subject::Record { handle: clip.common().handle, kind: clip.as_entity().entity_type() }, reason })
            } else {
                let mut path = Rect::from_center_size((viewport.center.x, viewport.center.y), (viewport.width, viewport.height)).to_path(0.0);
                path.apply_affine(paper.frame);
                Ok(path)
            }
            .and_then(|outline| Region::new(outline, FillRule::NonZero, projection.tolerance).map_err(misshapen));
            let inner = Context {
                frame: Affine::scale(model),
                viewport: Some(viewport),
                drafting: document.header.paper_space_linetype_scaling.then(|| paper.frame.determinant().abs().sqrt()),
                view_height: viewport.view_height,
                ..paper
            };
            let layers = linework::projected(document, "*Model_Space", &inner, projection).map_or_else(Validated::Fail, Validated::Good);
            Validated::from(mapped).map3(Validated::from(boundary), layers, |(placement, scale), boundary, layers| pages::Viewport { boundary: Some(boundary), placement, scale, layers }).ok()
        };
        let viewports = ports.iter().map(|viewport| viewed(viewport).map_or_else(Validated::Fail, Validated::Good)).collect::<Validated<Vec<_>, _>>();
        drawn.map2(viewports, |layers, viewports| (Arc::from(layout.name.as_str()), Page { size, viewports, layers, fields, .. })).ok()
    };
    layouts.iter().map(|layout| paged(layout).map_or_else(Validated::Fail, Validated::Good)).collect::<Validated<Vec<_>, _>>().ok()
}

pub fn drawing(pages: Vec<(Arc<str>, Page)>, authoring: &Authoring) -> Result<CadDocument, NEVec<CadError>> {
    fn ported(
        document: &mut CadDocument,
        key: &Arc<str>,
        index: usize,
        size: Size,
        unit: f64,
        (boundary, affine): (Option<Region>, Placement),
        frozen: impl IntoIterator<Item = Handle>,
    ) -> Result<(), NEVec<CadError>> {
        let subject = Subject::Viewport { layout: Arc::clone(key), index };
        let refused = |reason| NEVec::new(CadError::Refused { subject: subject.clone(), reason });
        let authored = |source| NEVec::new(CadError::Drawing { operation: Operation::Layout(Arc::clone(key)), source });
        let paper = Affine::scale(unit.recip()) * Affine::translate((0.0, size.height)) * Affine::FLIP_Y;
        let mapped = paper * *affine * Affine::scale(unit);
        let [across, up, sideways, upright, _, _] = mapped.as_coeffs();
        let (scale, twist) = (Vec2::new(across, up).hypot(), up.atan2(across));
        let reach = f64::EPSILON * 64.0 * scale;
        let crop = match boundary.as_ref() {
            None => Ok(Some((Rect::from_origin_size(Point::ORIGIN, size), None))),
            Some(region) => match &**region {
                [] => Ok(None),
                [contour] => Ok(Some(region.as_rect().map_or_else(|| (contour.path.bounding_box(), Some(&contour.path)), |bounds| (bounds, None)))),
                [_, _, ..] => Err(refused(Refusal::Boundary)),
            },
        }?;
        match (scale.is_normal() && (across - upright).abs() <= reach && (up + sideways).abs() <= reach, crop) {
            (false, _) => Err(refused(Refusal::View)),
            (true, None) => Ok(()),
            (true, Some((bounds, outline))) => {
                let frame = paper.transform_rect_bbox(bounds);
                let center = frame.center();
                let clip = outline
                    .map(|outline| {
                        <[EntityType; 1]>::try_from(curves::curves(&(paper * outline)))
                            .map_err(|Vec { .. }| refused(Refusal::Boundary))
                            .and_then(|[entity]| document.add_entity_to_layout(entity, key).map_err(authored))
                    })
                    .transpose()?;
                let view = Affine::rotate(twist) * mapped.inverse() * center;
                let mut viewport = Viewport {
                    id: i16::try_from(index.strict_add(2)).map_err(|source| refused(Refusal::from(source)))?,
                    status: ViewportStatusFlags { non_rectangular_clipping: clip.is_some(), ..ViewportStatusFlags::default_on() },
                    view_center: Vector3::new(view.x, view.y, 0.0),
                    twist_angle: twist,
                    clip_boundary_handle: clip.unwrap_or(Handle::NULL),
                    ..Viewport::with_size(Vector3::new(center.x, center.y, 0.0), frame.width(), frame.height())
                };
                viewport.set_scale(scale);
                frozen.into_iter().for_each(|handle| viewport.freeze_layer(handle));
                document.add_entity_to_layout(EntityType::Viewport(Box::new(viewport)), key).map(|_| ()).map_err(authored)
            }
        }
    }
    let mut document = linework::authored(authoring);
    let unit = Length::from(authoring.unit).get::<millimeter>();
    let modeled = |viewport: pages::Viewport, subject: &Subject| -> Result<(Option<Region>, Placement, Vec<pages::Layer>), NEVec<CadError>> {
        let misshapen = |source| CadError::Geometry { subject: subject.clone(), source };
        let affine = pages::affine(&viewport).map_err(|source| NEVec::new(misshapen(source)))?;
        let pages::Viewport { boundary, placement: _, scale: _, layers } = viewport;
        layers
            .into_iter()
            .map(|pages::Layer { name, print, view, elements }| {
                rasm_linework::rebase(elements, affine)
                    .map_err(|errors| errors.into_nonempty_iter().map(misshapen).collect())
                    .map_or_else(Validated::Fail, Validated::Good)
                    .map(|elements| pages::Layer { name, print, view, elements })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()
            .map(|layers| (boundary, affine, layers))
    };
    let sheets = pages
        .into_iter()
        .map(|(key, Page { size, viewports, layers, links: _, fields: _ })| {
            viewports
                .into_iter()
                .enumerate()
                .map(|(index, viewport)| modeled(viewport, &Subject::Viewport { layout: Arc::clone(&key), index }).map_or_else(Validated::Fail, Validated::Good))
                .collect::<Validated<Vec<_>, _>>()
                .map(|viewports| (key, size, layers, viewports))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let mut laid = |index: usize, key: &Arc<str>, size: Size| -> Result<(), CadError> {
        let (paper, dictionary) = (document.header.paper_space_block_handle, document.header.acad_layout_dict_handle);
        let first = (index == 0)
            .then(|| {
                document.objects.values().find_map(|object| {
                    if let ObjectType::Layout(layout) = object
                        && layout.block_record == paper
                    {
                        Some(layout.handle)
                    } else {
                        None
                    }
                })
            })
            .flatten();
        let handle = first.map_or_else(|| document.add_layout(key).map_err(|source| CadError::Drawing { operation: Operation::Layout(Arc::clone(key)), source }), Ok)?;
        if let Some(ObjectType::Dictionary(layouts)) = document.objects.get_mut(&dictionary)
            && let Some((name, _)) = layouts.entries.iter_mut().find(|(_, entry)| *entry == handle)
        {
            key.as_ref().clone_into(name);
        }
        if let Some(ObjectType::Layout(layout)) = document.objects.get_mut(&handle) {
            *layout = Layout {
                name: key.to_string(),
                paper_width: size.width,
                paper_height: size.height,
                max_limits: (size.width / unit, size.height / unit),
                plot_paper_units: PlotPaperUnits::Millimeters.to_code(),
                plot_scale_numerator: unit,
                plot_scale_denominator: 1.0,
                ..layout.clone()
            };
        }
        Ok(())
    };
    sheets.iter().enumerate().map(|(index, (key, size, _, _))| laid(index, key, *size)).collect::<Validated<(), _>>().ok()?;
    let (sheets, distinct) = {
        let mut distinct = Vec::<pages::Layer>::new();
        let mut candidates = HashMap::<_, Vec<usize>>::new();
        let mut indexed = |layer: pages::Layer| {
            let positions = candidates.entry((Arc::clone(&layer.name), layer.print, layer.view)).or_default();
            positions.iter().copied().find(|&position| distinct[position] == layer).unwrap_or_else(|| {
                let position = distinct.len();
                distinct.push(layer);
                positions.push(position);
                position
            })
        };
        let sheets = sheets
            .into_iter()
            .map(|(key, size, paper, viewports)| {
                (key, size, paper, viewports.into_iter().map(|(boundary, affine, model)| (boundary, affine, model.into_iter().map(&mut indexed).collect::<HashSet<_>>())).collect::<Vec<_>>())
            })
            .collect::<Vec<_>>();
        (sheets, distinct)
    };
    let mut taken = distinct.iter().map(|layer| normalize_name(&layer.name)).collect::<HashSet<_>>();
    let mut seen = HashSet::new();
    let named = distinct
        .into_iter()
        .map(|mut layer| {
            if !seen.insert(normalize_name(&layer.name)) {
                let mut repeat = 1_usize;
                layer.name = loop {
                    repeat = repeat.strict_add(1);
                    let name = Arc::<str>::from(format!("{} {repeat}", layer.name));
                    if taken.insert(normalize_name(&name)) {
                        break name;
                    }
                };
            }
            layer
        })
        .collect::<Vec<_>>();
    let handles = linework::written(&mut document, "Model", named, authoring)?;
    let flipped = |layers: Vec<pages::Layer>, size: Size, key: &Arc<str>| -> Result<Vec<pages::Layer>, NEVec<CadError>> {
        let flip = Affine::translate((0.0, size.height)) * Affine::FLIP_Y;
        layers
            .into_iter()
            .map(|pages::Layer { name, print, view, elements }| {
                elements
                    .into_iter()
                    .enumerate()
                    .map(|(index, element)| {
                        let misshapen = |source| CadError::Geometry { subject: Subject::Element { space: Arc::clone(key), layer: Arc::clone(&name), index }, source };
                        Placement::try_new(flip)
                            .map_err(|source| NEVec::new(misshapen(source)))
                            .and_then(|flip| rasm_linework::transform(vec![element], flip).map_err(|errors| errors.into_nonempty_iter().map(misshapen).collect()))
                            .map_or_else(Validated::Fail, Validated::Good)
                    })
                    .collect::<Validated<Vec<_>, _>>()
                    .map(|placed| pages::Layer { name, print, view, elements: placed.into_iter().flatten().collect() })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()
    };
    let outcome = sheets
        .into_iter()
        .map(|(key, size, layers, viewports)| {
            let paper = flipped(layers, size, &key).and_then(|layers| linework::written(&mut document, &key, layers, authoring)).map_or_else(Validated::Fail, Validated::Good);
            let ports = viewports
                .into_iter()
                .enumerate()
                .map(|(index, (boundary, affine, shown))| {
                    let frozen = handles.iter().enumerate().filter(|(position, _)| !shown.contains(position)).map(|(_, handle)| *handle);
                    ported(&mut document, &key, index, size, unit, (boundary, affine), frozen).map_or_else(Validated::Fail, Validated::Good)
                })
                .collect::<Validated<(), _>>();
            paper.map2(ports, |_, ()| ())
        })
        .collect::<Validated<(), _>>();
    outcome.ok().map(|()| document)
}

#[must_use]
pub fn set(document: &CadDocument, name: &str, drawing: &str, pages: &[(Arc<str>, Page)]) -> SheetSetDatabase {
    let database = SheetSetDatabase::new(name, "");
    let parent = database.sheet_set().id().to_owned();
    pages.iter().fold(database, |mut database, (key, page)| {
        let [number, title, description] = STATED.map(|field| page.fields.get(field).map_or("", |value| &**value));
        let handle = document.objects.values().find_map(|object| {
            if let ObjectType::Layout(layout) = object
                && *layout.name == **key
            {
                Some(layout.handle)
            } else {
                None
            }
        });
        let reference = LayoutReference { file_name: drawing.to_owned(), name: key.to_string(), handle: handle.map_or_default(|handle| format!("{handle:X}")) };
        if let Some(sheet) = database.add_sheet(&parent, number, title, description)
            && database.set_layout_reference(&sheet, REFERENCE, &reference)
            && let Some(element) = database.find_mut(&sheet)
        {
            page.fields.iter().filter(|(field, _)| !STATED.contains(&&***field)).for_each(|(field, value)| set_custom_property(element, field, value, CUSTOM_SHEET_PROP));
        }
        database
    })
}
