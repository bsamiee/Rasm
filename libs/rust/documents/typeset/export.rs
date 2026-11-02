use core::num::NonZero;

use jiff::{Zoned, tz::TimeZone};
use png::{ColorType, Encoder, Info, PixelDimensions, SrgbRenderingIntent, Unit};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use typst::{
    comemo,
    foundations::{Bytes, Datetime},
    layout::{Abs, Page, Sides},
    model::PagedFormat,
    syntax::VirtualPath,
};
use typst_bundle::{Bundle, BundleDocument, BundleFile, BundleOptions, PagedExtras, VirtualFs};
use typst_html::{HtmlDocument, HtmlOptions};
use typst_kit::{diagnostics::DiagnosticWorld, files::FileLoader};
use typst_layout::PagedDocument;
use typst_pdf::{PdfOptions, Timestamp};
use typst_render::RenderOptions;
use validated::Validated;

use crate::{Project, TypesetError, located};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn paged<L>(project: &Project<L>, document: &PagedDocument, format: PagedFormat, options: BundleOptions, budget: NonZero<u64>) -> Validated<Vec<(NonZero<usize>, Bytes)>, TypesetError>
where L: FileLoader + Send + Sync {
    let options = stamped(&project.now, options);
    let selected = |number: &NonZero<usize>| options.pdf.page_ranges.as_ref().is_none_or(|ranges| ranges.includes_page(*number));
    let pages = document.pages().par_iter().enumerate().map(|(index, page)| (NonZero::<usize>::MIN.saturating_add(index), page)).filter(|(number, _)| selected(number));
    match format {
        PagedFormat::Pdf => match (1..=document.pages().len()).filter_map(NonZero::new).find(selected) {
            None => Validated::Good(Vec::new()),
            Some(first) => typst_pdf::pdf(document, &options.pdf).map(|bytes| vec![(first, Bytes::new(bytes))]).map_err(|diagnostics| located(project, diagnostics)).into(),
        },
        PagedFormat::Svg => Validated::Good(pages.map(|(number, page)| (number, Bytes::from_string(typst_svg::svg(page, &options.svg)))).collect()),
        PagedFormat::Png => {
            let density = NonZero::new((options.png.pixel_per_pt.get() * Abs::cm(100.0).to_pt()).round().to_int_saturating::<u32>()).map(|ppu| PixelDimensions { xppu: ppu.get(), yppu: ppu.get(), unit: Unit::Meter });
            pages
                .map(|(number, page)| {
                    raster(project.main.vpath(), number, page, &options.png, budget)?;
                    let pixmap = typst_render::render(page, &options.png);
                    let mut data = Vec::new();
                    Encoder::with_info(
                        &mut data,
                        Info { width: pixmap.width(), height: pixmap.height(), color_type: ColorType::Rgba, srgb: Some(SrgbRenderingIntent::RelativeColorimetric), pixel_dims: density, ..Info::default() },
                    )
                    .and_then(Encoder::write_header)
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
pub fn bundle<L>(project: &Project<L>, bundle: &Bundle, options: BundleOptions, budget: NonZero<u64>, max_age: usize) -> Validated<VirtualFs, TypesetError>
where L: FileLoader + Send + Sync {
    let options = stamped(&project.now, options);
    let bounded = bundle
        .files
        .iter()
        .filter_map(|(path, file)| match file {
            BundleFile::Document(BundleDocument::Paged(document, PagedExtras { format: PagedFormat::Png, anchors: _ })) => {
                document.pages().first().map(|page| raster(path, NonZero::<usize>::MIN, page, &options.png, budget))
            }
            BundleFile::Document(BundleDocument::Paged(_, PagedExtras { format: PagedFormat::Pdf | PagedFormat::Svg, anchors: _ }) | BundleDocument::Html(_)) | BundleFile::Asset(_) => None,
        })
        .collect::<Validated<(), _>>();
    match bounded {
        Validated::Good(()) => {
            let exported = typst_bundle::export(bundle, &options).map_err(|diagnostics| located(project, diagnostics));
            comemo::evict(max_age);
            exported.into()
        }
        Validated::Fail(errors) => Validated::Fail(errors),
    }
}

fn raster(path: &VirtualPath, page: NonZero<usize>, content: &Page, options: &RenderOptions, budget: NonZero<u64>) -> Result<(), TypesetError> {
    let size = (content.frame.size() + if options.render_bleed { content.bleed } else { Sides::default() }.sum_by_axis())
        .map(|axis| (options.pixel_per_pt.get().cast::<f32>() * axis.to_pt().cast::<f32>()).round().max(1.0).to_int_saturating::<u32>());
    size.x
        .checked_mul(4)
        .filter(|&row| i32::try_from(row).is_ok())
        .and_then(|row| u64::from(row).checked_mul(u64::from(size.y)))
        .is_some_and(|length| usize::try_from(length).is_ok() && u64::from(size.x).strict_mul(u64::from(size.y)) <= budget.get())
        .ok_or(TypesetError::Raster { path: path.clone(), page, width: size.x, height: size.y, budget })
}

fn stamped(now: &Zoned, options: BundleOptions) -> BundleOptions {
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
    let timestamp = options.pdf.timestamp.or_else(|| {
        (offset.rem_euclid(60) == 0 && !(-3599..0).contains(&offset))
            .then(|| datetime(now))
            .flatten()
            .and_then(|local| Timestamp::new_local(local, offset.div_euclid(60)))
            .or_else(|| datetime(&now.with_time_zone(TimeZone::UTC)).map(Timestamp::new_utc))
    });
    BundleOptions { pdf: PdfOptions { timestamp, ..options.pdf }, ..options }
}
