use core::num::NonZero;

use jiff::{Zoned, tz::TimeZone};
use png::{BitDepth, ColorType, Encoder, PixelDimensions, SrgbRenderingIntent, Unit};
use rayon::iter::{IndexedParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use typst::{
    foundations::{Bytes, Datetime},
    layout::{Abs, Sides},
    model::PagedFormat,
};
use typst_bundle::BundleOptions;
use typst_layout::PagedDocument;
use typst_pdf::Timestamp;
use validated::Validated;

use crate::TypesetError;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn paged(document: &PagedDocument, format: PagedFormat, options: &BundleOptions) -> Validated<Vec<(NonZero<usize>, Bytes)>, TypesetError> {
    let pages = document
        .pages()
        .par_iter()
        .enumerate()
        .map(|(index, page)| (NonZero::<usize>::MIN.saturating_add(index), page))
        .filter(|&(number, _)| options.pdf.page_ranges.as_ref().is_none_or(|ranges| ranges.includes_page(number)));
    match format {
        PagedFormat::Pdf => pages
            .map(|(number, _)| number)
            .min()
            .map_or_else(|| Validated::Good(Vec::new()), |first| Validated::from(typst_pdf::pdf(document, &options.pdf).map(|bytes| vec![(first, Bytes::new(bytes))]).map_err(TypesetError::Typst))),
        PagedFormat::Svg => Validated::Good(pages.map(|(number, page)| (number, Bytes::from_string(typst_svg::svg(page, &options.svg)))).collect()),
        PagedFormat::Png => {
            let pixel_per_pt = options.png.pixel_per_pt.get();
            let density = NonZero::new((pixel_per_pt * Abs::cm(100.0).to_pt()).round().to_int_saturating::<u32>()).map(|ppu| PixelDimensions { xppu: ppu.get(), yppu: ppu.get(), unit: Unit::Meter });
            pages
                .map(|(number, page)| {
                    let size = (page.frame.size() + if options.png.render_bleed { page.bleed } else { Sides::default() }.sum_by_axis())
                        .map(|axis| (pixel_per_pt.cast::<f32>() * axis.to_pt().cast::<f32>()).round().max(1.0).to_int_saturating::<u32>());
                    size.x
                        .checked_mul(4)
                        .filter(|&row| i32::try_from(row).is_ok())
                        .and_then(|row| u64::from(row).checked_mul(u64::from(size.y)))
                        .filter(|&length| usize::try_from(length).is_ok())
                        .ok_or(TypesetError::Raster { page: number.get(), width: size.x, height: size.y })?;
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
                        .map_err(|source| TypesetError::Encode { page: number.get(), source })
                })
                .collect::<Vec<_>>()
                .into_iter()
                .collect()
        }
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
    let offset = now.offset();
    (offset.seconds().rem_euclid(60) == 0 && !(-3599..0).contains(&offset.seconds()))
        .then(|| datetime(now))
        .flatten()
        .and_then(|local| Timestamp::new_local(local, offset.seconds().div_euclid(60)))
        .or_else(|| datetime(&now.with_time_zone(TimeZone::UTC)).map(Timestamp::new_utc))
}
