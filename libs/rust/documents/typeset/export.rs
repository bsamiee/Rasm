use core::{iter, num::NonZero};
use std::{io, path::Path};

use cap_std::fs::Dir;
use jiff::Zoned;
use png::{ColorType, Compression, Encoder, PixelDimensions, SrgbRenderingIntent, Unit};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, IntoParallelRefIterator as _, ParallelIterator as _, once};
use typst::{
    foundations::{Bytes, Datetime},
    layout::{Abs, Axes, Sides, Size},
    model::{Document as _, PagedFormat},
    syntax::VirtualPath,
    utils::hash128,
};
use typst_bundle::{Bundle, BundleDocument, BundleFile, BundleOptions, PagedExtras, VirtualFs};
use typst_html::{HtmlDocument, HtmlOptions};
use typst_kit::diagnostics::DiagnosticWorld;
use typst_layout::{Page, PagedDocument};
use typst_pdf::Timestamp;
use typst_render::RenderOptions;
use validated::Validated;

use crate::{TypesetError, typst_error};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Exported = Result<(VirtualPath, Bytes), TypesetError>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, Default)]
pub struct Raster {
    pub fit: Option<Axes<NonZero<u32>>>,
    pub budget: Option<NonZero<u64>>,
    pub compression: Compression,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [EXPORT]
#[must_use]
pub fn paged(
    world: &dyn DiagnosticWorld,
    document: &PagedDocument,
    format: PagedFormat,
    options: &BundleOptions,
    raster: &Raster,
    prior: &[u128],
) -> Validated<(Vec<u128>, Vec<VirtualPath>, impl IndexedParallelIterator<Item = Exported>), TypesetError> {
    let main = world.main().get().vpath();
    let extension = match format {
        PagedFormat::Pdf => "pdf",
        PagedFormat::Png => "png",
        PagedFormat::Svg => "svg",
    };
    let file = move |number: NonZero<usize>| main.with_extension(&format!("{number}.{extension}"));
    let scaled = move |page: &Page| {
        let extent = extent(page, &options.png);
        let pixel_per_pt = options.png.pixel_per_pt.get();
        let pixel_per_pt = raster.fit.map_or(pixel_per_pt, |fit| pixel_per_pt.min(f64::from(fit.x.get()) / extent.x.to_pt()).min(f64::from(fit.y.get()) / extent.y.to_pt()));
        RenderOptions { pixel_per_pt: pixel_per_pt.into(), ..options.png.clone() }
    };
    let hashes = document.pages().par_iter().map(hash128).chain(once(hash128(document.info()))).collect::<Vec<_>>();
    let document_changed = prior != hashes;
    let selected = iter::zip(document.pages(), &hashes)
        .enumerate()
        .map(|(index, (page, hash))| (NonZero::<usize>::MIN.saturating_add(index), page, if format == PagedFormat::Pdf { document_changed } else { prior.get(index) != Some(hash) }))
        .filter(|(number, _, _)| options.pdf.page_ranges.as_ref().is_none_or(|ranges| ranges.includes_page(*number)))
        .collect::<Vec<_>>();
    if format == PagedFormat::Png
        && let Validated::Fail(errors) = within_budget(main, selected.iter().map(|&(number, page, _)| pixel_count(main, number, page, &scaled(page))), raster.budget)
    {
        return Validated::Fail(errors);
    }
    let limit = if format == PagedFormat::Pdf { 1 } else { selected.len() };
    let changed = selected.into_iter().filter(|&(_, _, changed)| changed).take(limit).collect::<Vec<_>>();
    let removed = (document.pages().len()..prior.len().saturating_sub(1)).filter(|_| format != PagedFormat::Pdf).map(|index| file(NonZero::<usize>::MIN.saturating_add(index))).collect();
    Validated::Good((
        hashes,
        removed,
        changed.into_par_iter().map(move |(number, page, _)| match format {
            PagedFormat::Pdf => typst_pdf::pdf(document, &options.pdf).map(|bytes| (main.with_extension(extension), Bytes::new(bytes))).map_err(|diagnostics| typst_error(world, &diagnostics)),
            PagedFormat::Svg => Ok((file(number), Bytes::from_string(typst_svg::svg(page, &options.svg)))),
            PagedFormat::Png => {
                let rendering = scaled(page);
                let pixmap = typst_render::render(page, &rendering);
                let density = NonZero::new((rendering.pixel_per_pt.get() * Abs::cm(100.0).to_pt()).round().to_int_saturating::<u32>()).map(|ppu| PixelDimensions {
                    xppu: ppu.get(),
                    yppu: ppu.get(),
                    unit: Unit::Meter,
                });
                let mut data = Vec::new();
                let mut encoder = Encoder::new(&mut data, pixmap.width(), pixmap.height());
                encoder.set_color(ColorType::Rgba);
                encoder.set_source_srgb(SrgbRenderingIntent::RelativeColorimetric);
                encoder.set_pixel_dims(density);
                encoder.set_compression(raster.compression);
                encoder
                    .write_header()
                    .and_then(|mut writer| writer.write_image_data(&pixmap.take_demultiplied()).and_then(|()| writer.finish()))
                    .map(|()| (file(number), Bytes::new(data)))
                    .map_err(|source| TypesetError::Encode { path: main.clone(), page: number, source })
            }
        }),
    ))
}

pub fn html(world: &dyn DiagnosticWorld, document: &HtmlDocument, options: &HtmlOptions) -> Result<Bytes, TypesetError> {
    typst_html::html(document, options).map(Bytes::from_string).map_err(|diagnostics| typst_error(world, &diagnostics))
}

#[must_use]
pub fn bundle(world: &dyn DiagnosticWorld, bundle: &Bundle, options: &BundleOptions, budget: Option<NonZero<u64>>) -> Validated<VirtualFs, TypesetError> {
    let pixels = bundle.files.iter().filter_map(|(path, file)| match file {
        BundleFile::Document(BundleDocument::Paged(document, PagedExtras { format: PagedFormat::Png, anchors: _ })) =>
            document.pages().first().map(|page| pixel_count(path, NonZero::<usize>::MIN, page, &options.png)),
        BundleFile::Document(BundleDocument::Paged(_, PagedExtras { format: PagedFormat::Pdf | PagedFormat::Svg, anchors: _ }) | BundleDocument::Html(_)) | BundleFile::Asset(_) => None,
    });
    within_budget(world.main().get().vpath(), pixels, budget)
        .ok()
        .map_or_else(Validated::Fail, |()| typst_bundle::export(bundle, options).map_err(|diagnostics| typst_error(world, &diagnostics)).into())
}

#[must_use]
pub fn write<I>(directory: &Dir, files: I) -> Validated<(), TypesetError>
where I: IntoParallelIterator<Item = Exported> {
    let written = |path: &VirtualPath, bytes: &Bytes| -> io::Result<()> {
        let target = Path::new(path.get_without_slash());
        if let Some(parent) = target.parent() {
            directory.create_dir_all(parent)?;
        }
        directory.write(target, bytes)
    };
    files
        .into_par_iter()
        .map(|file| {
            let (path, bytes) = file?;
            written(&path, &bytes).map_err(|source| TypesetError::Write { path, source })
        })
        .collect()
}

#[must_use]
pub fn timestamp(now: &Zoned) -> Option<Timestamp> {
    Datetime::from_ymd_hms(i32::from(now.year()), now.month().cast_unsigned(), now.day().cast_unsigned(), now.hour().cast_unsigned(), now.minute().cast_unsigned(), now.second().cast_unsigned())
        .and_then(|datetime| Timestamp::new_local(datetime, now.offset().seconds() / 60))
}

// --- [RASTER]
fn extent(page: &Page, options: &RenderOptions) -> Size {
    let bleed = if options.render_bleed { page.bleed } else { Sides::default() };
    page.frame.size() + bleed.sum_by_axis()
}

fn pixel_count(path: &VirtualPath, number: NonZero<usize>, page: &Page, options: &RenderOptions) -> Result<u64, TypesetError> {
    let size = extent(page, options).map(|axis| (options.pixel_per_pt.get().cast::<f32>() * axis.to_pt().cast::<f32>()).round().max(1.0).to_int_saturating::<u32>());
    let row_fits = size.x <= i32::MAX.cast_unsigned() / 4;
    row_fits.then(|| u64::from(size.x).strict_mul(u64::from(size.y))).ok_or_else(|| TypesetError::Raster { path: path.clone(), page: number, width: size.x, height: size.y })
}

fn within_budget<I>(path: &VirtualPath, pixels: I, budget: Option<NonZero<u64>>) -> Validated<(), TypesetError>
where I: IntoIterator<Item = Result<u64, TypesetError>> {
    match (pixels.into_iter().collect::<Validated<Vec<u64>, _>>().map(|counts| counts.into_iter().fold(0, u64::saturating_add)), budget) {
        (Validated::Good(pixels), Some(budget)) if pixels > budget.get() => Validated::fail(TypesetError::Budget { path: path.clone(), pixels, budget }),
        (Validated::Good(_), _) => Validated::Good(()),
        (Validated::Fail(errors), _) => Validated::Fail(errors),
    }
}
