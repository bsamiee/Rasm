use core::{borrow::Borrow, iter};
use std::sync::Arc;

use fontdb::Database;
use jiff::{Zoned, tz::TimeZone};
use krilla::{
    SerializeSettings,
    action::LinkAction,
    annotation::{Annotation, LinkAnnotation, Target},
    configure::{PdfVersion, ValidationError},
    destination::XyzDestination,
    embed::EmbeddedFile,
    error::KrillaError,
    geom::{self, Quadrilateral, Transform},
    metadata::{DateTime, Metadata},
    outline::Outline,
    page::PageSettings,
    tagging::TagTree,
};
use kurbo::{Point, Size};
use nonempty_collections::{IntoNonEmptyIterator, NEVec, NonEmptyIterator as _};
use pdf_oxide::compliance::{PdfXLevel, validate_pdf_x};
use rasm_drawing::{
    Tolerance, Visibility,
    pages::{self, Destination, Link, Page},
};
use uom::si::length::point_computer;
use validated::Validated;

use self::{content::Drawing, tagging::Leaves};
use crate::{
    Document, PdfError, Refusal, Resolution, Subject,
    layers::{self, Group},
    measurement,
    navigation::{self, Navigation},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Options {
    pub application: Arc<str>,
    pub metadata: Metadata,
    pub created: Zoned,
    pub standards: Standards = Standards { .. },
    pub navigation: Navigation = Navigation { .. },
    pub attachments: Vec<EmbeddedFile> = Vec::new(),
    pub fonts: Arc<Database>,
    pub resolution: Option<Resolution> = None,
    pub tolerance: Tolerance,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn author<I>(pages: I, options: &Options) -> Result<Document, PdfError>
where I: IntoNonEmptyIterator<Item: Borrow<Page>> {
    let held = pages.into_nonempty_iter().collect::<NEVec<_>>();
    let sheets = held.iter().map(Borrow::<Page>::borrow).collect::<Vec<_>>();
    let settings = standards::settings(&options.standards)?;
    let layered = settings.configuration.version() >= PdfVersion::Pdf15;
    let labels = &options.navigation.labels;
    let count = sheets.len();
    let annotation = |link: &Link, scale: f64| {
        let area = link.area.abs().scale_from_origin(scale);
        let quad = geom::Rect::from_ltrb(area.x0.cast::<f32>(), area.y0.cast::<f32>(), area.x1.cast::<f32>(), area.y1.cast::<f32>()).ok_or(PdfError::Area(link.area))?;
        let (target, alt) = match &link.destination {
            Destination::Uri(uri) => (Target::Action(LinkAction::new(uri.to_string()).into()), uri.to_string()),
            Destination::Page { index, area: anchor } => {
                (*index < count).ok_or(PdfError::Page { index: *index, count })?;
                let origin = anchor.map_or(Point::ZERO, |rect| rect.abs().origin()).to_vec2() * scale;
                let label = navigation::label(labels, *index).filter(|text| !text.is_empty()).unwrap_or_else(|| format!("page {}", index.strict_add(1)));
                (Target::Destination(XyzDestination::new(*index, geom::Point::from_xy(origin.x.cast::<f32>(), origin.y.cast::<f32>())).into()), label)
            }
        };
        Ok::<_, PdfError>(Annotation::new_link(LinkAnnotation::new_with_quad_points(vec![Quadrilateral::from(quad)], target), Some(alt)))
    };
    let mut document = opened(settings, options)?;
    let mut drawing = Drawing::new(options);
    let (groups, parts): (Vec<_>, Vec<_>) = sheets
        .iter()
        .enumerate()
        .map(|(index, page)| {
            let located = |error: PdfError| (Subject::Page(index), error);
            let scale = (*page.unit).get::<point_computer>();
            let sized = Validated::from(dimensioned(page.size.to_rect().scale_from_origin(scale).size()).map_err(located));
            let paper = page
                .viewports
                .iter()
                .map(|viewport| pages::paper(viewport.clone()).map_or_else(Validated::Fail, Validated::Good))
                .collect::<Validated<Vec<_>, _>>()
                .map_err(|errors| errors.into_nonempty_iter().map(|error| located(error.into())).collect());
            let linked = page.links.iter().map(|link| annotation(link, scale).map_err(located)).collect::<Validated<Vec<_>, _>>();
            let ((page_settings, _), paper, annotations) = match sized.map3(paper, linked, |sized, paper, annotations| (sized, paper, annotations)) {
                Validated::Good(parts) => parts,
                Validated::Fail(errors) => return Validated::Fail(errors),
            };
            let sheet = page.size.to_rect();
            let mut canvas = document.start_page_with(page_settings);
            let mut surface = canvas.surface();
            surface.push_transform(&Transform::from_scale(scale.cast::<f32>(), scale.cast::<f32>()));
            let layers = iter::chain(paper.iter().flatten(), &page.layers)
                .map(|layer| {
                    let mut leaves = Leaves::default();
                    if layered {
                        surface.push_isolated();
                    }
                    let result = match (layered, layer.view) {
                        (true, Visibility::Visible | Visibility::Hidden) | (false, Visibility::Visible) => content::draw(&mut surface, &layer.elements, sheet, &mut drawing, &mut leaves),
                        (false, Visibility::Hidden) => Validated::Good(()),
                    };
                    if layered {
                        surface.pop();
                    }
                    result
                        .map(|()| (layer, leaves))
                        .map_err(|errors| errors.into_nonempty_iter().map(|(element, error)| (Subject::Element { page: index, layer: Arc::clone(&layer.name), index: element }, error)).collect())
                })
                .collect::<Validated<Vec<_>, _>>();
            surface.pop();
            surface.finish();
            let links = annotations.into_iter().map(|link| canvas.add_tagged_annotation(link)).collect::<Vec<_>>();
            canvas.finish();
            layers.map(|bound| {
                let groups = if layered { bound.iter().map(|(layer, _)| Group { name: Arc::clone(&layer.name), print: layer.print, view: layer.view }).collect() } else { Vec::new() };
                (groups, tagging::part(bound.into_iter().map(|(layer, leaves)| (&layer.name, leaves)), links))
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)?
        .into_iter()
        .unzip();
    document.set_tag_tree(TagTree::from(parts));
    closed(document, &sheets, &groups, options)
}

pub fn scan(tiff: &[u8], options: &Options) -> Result<Document, PdfError> {
    let settings = standards::settings(&options.standards)?;
    (!settings.enable_tagging).ok_or_else(|| KrillaError::Validation(vec![(ValidationError::MissingTagging, settings.configuration.validators())]))?;
    let mut document = opened(settings, options)?;
    let mut resizer = fast_image_resize::Resizer::new();
    rasters::frames(tiff)?.enumerate().try_for_each(|(index, frame)| {
        let (paper, raster) = frame?;
        let (page, size) = dimensioned(paper)?;
        let image = rasters::image(&raster, rasters::target(&raster, paper, options.resolution), &mut resizer)
            .map_err(|errors| PdfError::Refused(errors.into_nonempty_iter().map(|error| (Subject::Page(index), error)).collect()))?;
        document.start_page_with(page).surface().draw_image(image, size);
        Ok::<_, PdfError>(())
    })?;
    closed(document, &[], &[], options)
}

fn opened(settings: SerializeSettings, options: &Options) -> Result<krilla::Document, PdfError> {
    let zoned = options.created.with_time_zone(TimeZone::UTC);
    let created = DateTime::new(u16::try_from(zoned.year()).map_err(|_error| PdfError::Year(zoned.year()))?)
        .month(zoned.month().cast_unsigned())
        .day(zoned.day().cast_unsigned())
        .hour(zoned.hour().cast_unsigned())
        .minute(zoned.minute().cast_unsigned())
        .second(zoned.second().cast_unsigned())
        .utc_offset_hour(0);
    let mut document = krilla::Document::new_with(settings);
    document.set_metadata(options.metadata.clone().creation_date(created));
    document.set_outline(Outline::new());
    options.attachments.iter().try_for_each(|file| {
        document
            .embed_file(EmbeddedFile { modification_date: file.modification_date.or(Some(created)), ..file.clone() })
            .ok_or_else(|| PdfError::Named { name: Arc::from(file.path.as_str()), refusal: Refusal::Attachment })
    })?;
    Ok(document)
}

fn closed(document: krilla::Document, sheets: &[&Page], groups: &[Vec<Group>], options: &Options) -> Result<Document, PdfError> {
    let mut structure = lopdf::Document::load_mem(&document.finish()?)?;
    standards::close(&mut structure, &options.standards)?;
    if groups.iter().any(|layer_groups| !layer_groups.is_empty()) {
        layers::bind(&mut structure, groups, &options.application)?;
    }
    measurement::write(&mut structure, sheets.iter().copied())?;
    navigation::write(&mut structure, &options.navigation)?;
    let identifiers = structure.page_iter().collect::<Vec<_>>();
    iter::zip(identifiers, sheets).try_for_each(|(identifier, sheet)| crate::write_fields(&mut structure, &options.application, identifier, &sheet.fields, &options.created))?;
    let document = crate::saved(structure)?;
    let Some(press) = &options.standards.press else {
        return Ok(document);
    };
    let level = *AsRef::<PdfXLevel>::as_ref(press);
    let verdict = validate_pdf_x(&mut crate::engine(&document)?, level)?;
    if verdict.is_compliant { Ok(document) } else { Err(PdfError::Nonconforming { level, errors: verdict.errors }) }
}

fn dimensioned(paper: Size) -> Result<(PageSettings, geom::Size), PdfError> {
    let size = crate::extent(paper)?;
    Ok((PageSettings::new(size).with_trim_box(geom::Rect::from_xywh(0.0, 0.0, size.width(), size.height())), size))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod content;
mod rasters;
mod standards;
mod tagging;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use standards::{Press, Standards};
