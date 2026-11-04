use core::num::NonZero;
use std::sync::Arc;

use indexmap::IndexMap;
use itertools::Either;
use jiff::{Timestamp, tz::TimeZone};
use nonempty_collections::NEVec;
use rasm_typeset::{
    Files, Project, TypesetError,
    export::{Export, Raster, paged},
};
use rayon::iter::ParallelIterator as _;
use typst::{
    Features,
    foundations::{Content, Dict, Smart},
    layout::{Abs, Frame, PageRanges, Sides, Size},
    model::{DocumentInfo, PagedFormat},
    syntax::{PathError, VirtualPath},
};
use typst_bundle::BundleOptions;
use typst_kit::fonts::FontStore;
use typst_layout::{Page, PagedDocument};
use validated::Validated;

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn cached_exports_follow_options_format_and_selection() -> Result<(), Either<PathError, NEVec<TypesetError>>> {
    let main = VirtualPath::new("main.typ").map_err(Either::Left)?;
    let project = Project::new(&Dict::new(), Features::default(), Arc::new(FontStore::new()), Files::default(), main, Timestamp::UNIX_EPOCH.to_zoned(TimeZone::UTC))
        .map_err(|source| Either::Right(NEVec::new(source)))?;
    let pages = [7, 3]
        .into_iter()
        .map(|number| Page { frame: Frame::soft(Size::new(Abs::pt(10.0), Abs::pt(20.0))), bleed: Sides::default(), fill: Smart::Auto, numbering: None, supplement: Content::empty(), number })
        .collect();
    let document = PagedDocument::new(pages, DocumentInfo::default());
    let exported = |format, options: &BundleOptions, prior: &IndexMap<VirtualPath, u128>| {
        paged(&project, &document, format, options, &Raster::default(), prior)
            .ok()
            .and_then(|Export { cache, removed, files }| files.collect::<Validated<Vec<_>, _>>().ok().map(|files| (cache, removed, files)))
            .map_err(Either::Right)
    };
    let mut options = BundleOptions::default();
    let (mut cache, mut removed, mut files) = exported(PagedFormat::Svg, &options, &IndexMap::new())?;
    assert_eq!(files.iter().map(|file| file.path.get_without_slash()).collect::<Vec<_>>(), ["main.1.svg", "main.2.svg"], "Cold export emits pages in physical order");
    assert!(removed.is_empty(), "Cold export removes no files");

    let initial_svg = files;
    options.pdf.page_ranges = Some(PageRanges::new(Vec::new()));
    let (empty_cache, empty_removed, empty_files) = exported(PagedFormat::Svg, &options, &cache)?;
    assert!(empty_cache.is_empty() && empty_files.is_empty(), "Empty selection retains no cache entries or outputs");
    assert_eq!(empty_removed.iter().map(VirtualPath::get_without_slash).collect::<Vec<_>>(), ["main.1.svg", "main.2.svg"], "Deselected pages are removed in physical order");
    options.pdf.page_ranges = None;
    let (next_cache, next_removed, next_files) = exported(PagedFormat::Svg, &options, &cache)?;
    assert_eq!(next_cache, cache, "Unchanged export preserves cached entries");
    (cache, removed, files) = (next_cache, next_removed, next_files);
    assert!(files.is_empty() && removed.is_empty(), "Unchanged export retains cached files");

    options.svg.pretty.toggle();
    (cache, removed, files) = exported(PagedFormat::Svg, &options, &cache)?;
    assert_eq!(files.iter().map(|file| file.path.get_without_slash()).collect::<Vec<_>>(), ["main.1.svg", "main.2.svg"], "SVG options invalidate unchanged page content");
    assert!(removed.is_empty(), "Encoding options retain output paths");
    assert_ne!(files, initial_svg, "SVG formatting changes encoded bytes");

    let first = Some(NonZero::<usize>::MIN);
    options.pdf.page_ranges = Some(PageRanges::new(vec![first..=first]));
    (cache, removed, files) = exported(PagedFormat::Svg, &options, &cache)?;
    assert!(files.is_empty(), "Unchanged selected page retains cached encoding");
    assert_eq!(removed.iter().map(VirtualPath::get_without_slash).collect::<Vec<_>>(), ["main.2.svg"], "Deselected page removes previous output");

    (cache, removed, files) = exported(PagedFormat::Png, &options, &cache)?;
    assert_eq!(files.iter().map(|file| file.path.get_without_slash()).collect::<Vec<_>>(), ["main.1.png"], "Format changes emit requested encoding");
    assert_eq!(removed.iter().map(VirtualPath::get_without_slash).collect::<Vec<_>>(), ["main.1.svg"], "Format changes remove obsolete encodings");

    options.pdf.page_ranges = Some(PageRanges::new(Vec::new()));
    (cache, removed, files) = exported(PagedFormat::Png, &options, &cache)?;
    assert!(cache.is_empty() && files.is_empty(), "Empty selection retains no cache entries or pending outputs");
    assert_eq!(removed.iter().map(VirtualPath::get_without_slash).collect::<Vec<_>>(), ["main.1.png"], "Empty selection removes previous outputs");
    Ok(())
}
