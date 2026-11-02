use core::num::NonZero;

use jiff::{Zoned, tz::TimeZone};
use png::{BitDepth, ColorType, Encoder, PixelDimensions, SrgbRenderingIntent, Unit};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use typst::{
    foundations::{Bytes, Datetime},
    layout::{Abs, Sides},
    model::PagedFormat,
    syntax::VirtualPath,
};
use typst_bundle::{Bundle, BundleDocument, BundleFile, BundleOptions, PagedExtras, VirtualFs};
use typst_html::{HtmlDocument, HtmlOptions};
use typst_kit::diagnostics::DiagnosticWorld;
use typst_layout::{Page, PagedDocument};
use typst_pdf::Timestamp;
use typst_render::RenderOptions;
use validated::Validated;

use crate::{TypesetError, located};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn paged(
    world: &dyn DiagnosticWorld,
    document: &PagedDocument,
    format: PagedFormat,
    options: &BundleOptions,
    budget: Option<NonZero<u64>>,
) -> Validated<Vec<(NonZero<usize>, Bytes)>, TypesetError> {
    let selected = |number: &NonZero<usize>| options.pdf.page_ranges.as_ref().is_none_or(|ranges| ranges.includes_page(*number));
    let pages = document.pages().par_iter().enumerate().map(|(index, page)| (NonZero::<usize>::MIN.saturating_add(index), page)).filter(|(number, _)| selected(number));
    match format {
        PagedFormat::Pdf if let Some(first) = (1..=document.pages().len()).filter_map(NonZero::new).find(selected) =>
            typst_pdf::pdf(document, &options.pdf).map(|bytes| vec![(first, Bytes::new(bytes))]).map_err(|diagnostics| located(world, diagnostics)).into(),
        PagedFormat::Pdf => Validated::Good(Vec::new()),
        PagedFormat::Svg => Validated::Good(pages.map(|(number, page)| (number, Bytes::from_string(typst_svg::svg(page, &options.svg)))).collect()),
        PagedFormat::Png => {
            let density = NonZero::new((options.png.pixel_per_pt.get() * Abs::cm(100.0).to_pt()).round().to_int_saturating::<u32>()).map(|ppu| PixelDimensions {
                xppu: ppu.get(),
                yppu: ppu.get(),
                unit: Unit::Meter,
            });
            pages
                .map(|(number, page)| {
                    raster(world.main().vpath(), number, page, &options.png, budget)?;
                    let pixmap = typst_render::render(page, &options.png);
                    let mut data = Vec::new();
                    let mut encoder = Encoder::new(&mut data, pixmap.width(), pixmap.height());
                    encoder.set_color(ColorType::Rgba);
                    encoder.set_depth(BitDepth::Eight);
                    encoder.set_source_srgb(SrgbRenderingIntent::RelativeColorimetric);
                    encoder.set_pixel_dims(density);
                    encoder
                        .write_header()
                        .and_then(|mut writer| writer.write_image_data(&pixmap.take_demultiplied()).and_then(|()| writer.finish()))
                        .map(|()| (number, Bytes::new(data)))
                        .map_err(|source| TypesetError::Encode { page: number, source })
                })
                .collect()
        }
    }
}

pub fn html(world: &dyn DiagnosticWorld, document: &HtmlDocument, options: &HtmlOptions) -> Result<Bytes, TypesetError> {
    typst_html::html(document, options).map(Bytes::from_string).map_err(|diagnostics| located(world, diagnostics))
}

#[must_use]
pub fn bundle(world: &dyn DiagnosticWorld, bundle: &Bundle, options: &BundleOptions, budget: Option<NonZero<u64>>) -> Validated<VirtualFs, TypesetError> {
    let bounded = bundle
        .files
        .iter()
        .filter_map(|(path, file)| match file {
            BundleFile::Document(BundleDocument::Paged(document, PagedExtras { format: PagedFormat::Png, anchors: _ })) =>
                document.pages().first().map(|page| raster(path, NonZero::<usize>::MIN, page, &options.png, budget)),
            BundleFile::Document(BundleDocument::Paged(_, PagedExtras { format: PagedFormat::Pdf | PagedFormat::Svg, anchors: _ }) | BundleDocument::Html(_)) | BundleFile::Asset(_) => None,
        })
        .collect::<Validated<(), _>>();
    match bounded {
        Validated::Good(()) => typst_bundle::export(bundle, options).map_err(|diagnostics| located(world, diagnostics)).into(),
        Validated::Fail(errors) => Validated::Fail(errors),
    }
}

#[must_use]
pub fn timestamp(now: &Zoned) -> Option<Timestamp> {
    let datetime = |zoned: &Zoned| {
        Datetime::from_ymd_hms(
            i32::from(zoned.year()),
            zoned.month().cast_unsigned(),
            zoned.day().cast_unsigned(),
            zoned.hour().cast_unsigned(),
            zoned.minute().cast_unsigned(),
            zoned.second().cast_unsigned(),
        )
    };
    let offset = now.offset().seconds();
    (offset.rem_euclid(60) == 0 && !(-3599..0).contains(&offset))
        .then_some(now)
        .and_then(datetime)
        .and_then(|local| Timestamp::new_local(local, offset.div_euclid(60)))
        .or_else(|| datetime(&now.with_time_zone(TimeZone::UTC)).map(Timestamp::new_utc))
}

fn raster(path: &VirtualPath, page: NonZero<usize>, content: &Page, options: &RenderOptions, budget: Option<NonZero<u64>>) -> Result<(), TypesetError> {
    let size = (content.frame.size() + if options.render_bleed { content.bleed } else { Sides::default() }.sum_by_axis())
        .map(|axis| (options.pixel_per_pt.get().cast::<f32>() * axis.to_pt().cast::<f32>()).round().max(1.0).to_int_saturating::<u32>());
    let refused = |bound| TypesetError::Raster { path: path.clone(), page, width: size.x, height: size.y, budget: bound };
    size.x
        .checked_mul(4)
        .is_some_and(|row| row <= i32::MAX.cast_unsigned() && usize::try_from(u64::from(row).strict_mul(u64::from(size.y))).is_ok())
        .ok_or_else(|| refused(None))?;
    budget.is_none_or(|budget| u64::from(size.x).strict_mul(u64::from(size.y)) <= budget.get()).ok_or_else(|| refused(budget))
}
