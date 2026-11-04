#![feature(array_try_map, default_field_values)]

use core::{mem, num::NonZero};
use std::sync::Arc;

use color::{AlphaColor, Srgb, palette};
use fontdb::Database;
use image::Rgba;
use itertools::Either;
use jiff::{Timestamp, tz::TimeZone};
use krilla::metadata::Metadata;
use kurbo::Size;
use lopdf::{
    Dictionary, Object, Stream,
    content::{Content, Operation},
    dictionary, text_string,
};
use nonempty_collections::nev;
use pdf_oxide::{Destination, OutlineItem};
use rasm_linework::{Tolerance, ToleranceError, Visibility, pages::Page};
use rasm_pdf::{
    Document, PdfError, Refusal, Resolution, ResolutionError, Subject,
    authoring::{Options, author},
    editing::assembly::{Part, assemble},
    layers::{self, Group},
    navigation::Navigation,
    proofs::{self, Appearance, Raster},
};
use uom::si::{f64::LinearNumberDensity, linear_number_density::per_inch};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn document(size: [i32; 2], contents: impl FnOnce(&mut lopdf::Document) -> Result<(Content, Dictionary), lopdf::Error>) -> Result<Document, PdfError> {
    let [width, height] = size;
    let mut structure = lopdf::Document::with_version("1.7");
    let pages = structure.try_new_object_id()?;
    let catalog = structure.try_add_object(dictionary! { "Type" => "Catalog", "Pages" => pages })?;
    structure.trailer.set("Root", catalog);
    let (content, resources) = contents(&mut structure)?;
    let content = structure.try_add_object(Stream::new(Dictionary::new(), content.encode()?))?;
    let page = structure.try_add_object(dictionary! {
        "Type" => "Page", "Parent" => pages, "Contents" => content, "Resources" => resources,
        "MediaBox" => vec![0.into(), 0.into(), width.into(), height.into()],
    })?;
    structure.set_object(pages, dictionary! { "Type" => "Pages", "Kids" => vec![Object::Reference(page)], "Count" => 1 });
    let mut bytes = Vec::new();
    structure.save_to(&mut bytes).map_err(lopdf::Error::from)?;
    Document::new(bytes, None)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn deep_outline_failures_preserve_ancestor_and_sibling_order() -> Result<(), ToleranceError> {
    let make_item = |title: &str, page| OutlineItem { title: title.into(), dest: Some(Destination::PageIndex(page)), children: Vec::new() };
    let chain = (0..20_000).fold(make_item("Leaf", 0), |child, _| OutlineItem { children: vec![child], ..make_item("Branch", 0) });
    let mut options = Options {
        metadata: Metadata::new(),
        created: Timestamp::UNIX_EPOCH.to_zoned(TimeZone::UTC),
        navigation: Navigation { outline: vec![OutlineItem { children: vec![chain, make_item("Child", 2)], ..make_item("Parent", 1) }, make_item("Sibling", 3)], .. },
        fonts: Arc::new(Database::new()),
        tolerance: Tolerance::try_new(0.1)?,
        ..
    };
    let result = author(nev![Page { size: Size::new(10.0, 10.0), .. }], &options);
    let mut pending = mem::take(&mut options.navigation.outline);
    while let Some(item) = pending.pop() {
        pending.extend(item.children);
    }
    let expected = ["Parent", "Child", "Sibling"];
    core::assert_matches!(
        result,
        Err(PdfError::Refused(errors))
            if errors.len().get() == expected.len()
                && errors.iter().zip(expected).all(|(actual, expected)| matches!(actual, (Subject::Outline, PdfError::Named { name, refusal: Refusal::Destination }) if name.as_ref() == expected))
    );
    Ok(())
}

#[test]
fn assembly_preserves_independent_same_name_groups() -> Result<(), PdfError> {
    let source = document([24, 16], |structure| {
        let groups = ["Left", "Right"].try_map(|key| structure.try_add_object(dictionary! { "Type" => "OCG", "Name" => text_string("Shared") }).map(|id| (key, id)))?;
        let references = groups.iter().map(|&(_, id)| Object::Reference(id)).collect::<Vec<_>>();
        structure.catalog_mut()?.set("OCProperties", dictionary! { "OCGs" => references.clone(), "D" => dictionary! { "ON" => references } });
        let operations = groups
            .iter()
            .flat_map(|&(key, _)| {
                [
                    Operation::new("BDC", vec!["OC".into(), key.into()]),
                    Operation::new("re", vec![0.into(), 0.into(), 8.into(), 8.into()]),
                    Operation::new("f", Vec::new()),
                    Operation::new("EMC", Vec::new()),
                ]
            })
            .collect();
        let properties = groups.into_iter().map(|(key, id)| (key, Object::Reference(id))).collect::<Dictionary>();
        Ok((Content { operations }, dictionary! { "Properties" => properties }))
    })?;
    let assembled = assemble(nev![Part { document: source, pages: None }])?;
    let groups = layers::groups(&assembled)?;
    assert_eq!(groups.len(), 2, "Assembly retains both native group identities");
    let (first, second) = (&groups[0], &groups[1]);
    assert_ne!(first.0, second.0, "Equal display data retains separate native object IDs");
    assert_eq!(first.1, second.1, "Equal names and visibility do not merge groups");
    let changed = Group { name: "Renamed".into(), print: Visibility::Hidden, view: Visibility::Hidden };
    let edited = layers::set(&assembled, [(first.0, changed.clone())])?;
    assert_eq!(layers::groups(&edited)?, [(first.0, changed.clone()), second.clone()], "Native ID edits preserve the other same-name group");
    let missing = (0, 0);
    core::assert_matches!(layers::set(&assembled, [(missing, changed)]), Err(PdfError::Group(id)) if id == missing, "Unknown group IDs return typed refusal");
    Ok(())
}

#[test]
fn transparent_proofs_preserve_pixels_across_boundary_tiles() -> Result<(), Either<PdfError, ResolutionError>> {
    let alpha = 0.5;
    let source = document([35, 19], |structure| {
        let state = structure.try_add_object(dictionary! { "Type" => "ExtGState", "ca" => alpha })?;
        let operations = vec![
            Operation::new("gs", vec!["Translucent".into()]),
            Operation::new("rg", vec![1.into(), 0.into(), 0.into()]),
            Operation::new("re", vec![2.into(), 2.into(), 14.into(), 10.into()]),
            Operation::new("f", Vec::new()),
        ];
        Ok((Content { operations }, dictionary! { "ExtGState" => dictionary! { "Translucent" => state } }))
    })
    .map_err(Either::Left)?;
    let raster = Raster {
        resolution: Resolution::try_new(LinearNumberDensity::new::<per_inch>(72.0)).map_err(Either::Right)?,
        appearance: Some(Appearance::Screen { background: AlphaColor::<Srgb>::TRANSPARENT }),
        tile: const { NonZero::new(8).unwrap() },
        ..
    };
    let tiled = proofs::render(&source, 0, &raster).map_err(Either::Left)?;
    let whole = proofs::render(&source, 0, &Raster { tile: const { NonZero::new(64).unwrap() }, ..raster.clone() }).map_err(Either::Left)?;
    assert_eq!(tiled, whole, "Tile reuse preserves straight RGBA over transparent backgrounds");
    let tile = NonZero::<u32>::from(raster.tile);
    let boundary = (tiled.width() / tile).strict_mul(tile.get());
    assert!(boundary > tile.get() && boundary < tiled.width(), "Proof includes a later partial tile");
    assert_eq!(*tiled.get_pixel(tile.get(), tiled.height().strict_sub(7)), Rgba(palette::css::RED.with_alpha(alpha).to_rgba8().to_u8_array()), "Interior color is unpremultiplied");
    assert!(tiled.enumerate_pixels().filter(|(x, _, _)| *x >= boundary).all(|(_, _, pixel)| *pixel == Rgba([0; 4])), "Later boundary tiles clear previous colors");
    Ok(())
}
