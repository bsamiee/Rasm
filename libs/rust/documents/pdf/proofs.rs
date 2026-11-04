use core::{
    alloc,
    num::{NonZero, TryFromIntError},
};

use color::{AlphaColor, Srgb, palette};
use derive_where::derive_where;
use hayro::{
    RenderCache, RenderSettings,
    hayro_interpret::InterpreterSettings,
    render_into,
    vello_cpu::{Pixmap, RasterizerSettings, RenderContext, Resources, TargetInit},
};
use hayro_svg::SvgRenderSettings;
use image::{FlatSamples, GenericImage as _, ImageError, Pixel, RgbImage, Rgba, RgbaImage, buffer::ConvertBuffer as _, flat::SampleLayout};
use image_compare::{Algorithm, Similarity};
use kurbo::Affine;
use moxcms::{ColorProfile, Layout, RenderingIntent, TransformOptions};
use uom::si::linear_number_density::per_millimeter;
use validated::Validated;
use vello_common::pixmap::unpremultiply_rgba8;

use crate::{Document, OutputIntent, PdfError, Resolution, Subject, page};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub enum Appearance {
    Screen { background: AlphaColor<Srgb> },
    Press { intent: OutputIntent, rendering: RenderingIntent },
}

#[derive(Clone)]
#[derive_where(Debug)]
pub struct Raster {
    pub resolution: Resolution,
    pub appearance: Option<Appearance> = None,
    pub tile: NonZero<u16> = NonZero::<u16>::saturating_add(NonZero::<u16>::MIN, 4095),
    #[derive_where(skip(Debug))]
    pub interpreter_settings: Option<InterpreterSettings> = None,
    #[derive_where(skip(Debug))]
    pub render_settings: Option<RenderSettings> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn render(document: &Document, index: usize, raster: &Raster) -> Result<RgbaImage, PdfError> {
    let page = page(document, index)?;
    let (frame, paper) = crate::frame(page);
    let density = raster.resolution.get::<per_millimeter>();
    let [width, height] = [paper.width, paper.height].map(|side| (side * density).ceil().to_int_saturating::<u64>());
    let (columns, rows) = match (u32::try_from(width), u32::try_from(height)) {
        (Ok(columns), Ok(rows)) => alloc::Layout::array::<Rgba<u8>>(u64::from(columns).strict_mul(u64::from(rows)).saturating_cast())
            .map(|_| (columns, rows))
            .map_err(|alloc::LayoutError { .. }| PdfError::Raster { width, height })?,
        (Ok(_), Err(TryFromIntError { .. })) | (Err(TryFromIntError { .. }), Ok(_) | Err(TryFromIntError { .. })) => return Err(PdfError::Raster { width, height }),
    };
    let (background, proof) = match &raster.appearance {
        None => (palette::css::WHITE, None),
        Some(Appearance::Screen { background }) => (*background, None),
        Some(Appearance::Press { intent, rendering }) => {
            let screen = ColorProfile::new_srgb();
            let press: &ColorProfile = intent.as_ref();
            let options = TransformOptions { rendering_intent: *rendering, ..TransformOptions::default() };
            let display = TransformOptions { rendering_intent: RenderingIntent::RelativeColorimetric, ..options };
            (
                palette::css::WHITE,
                Some([(&screen, press, options), (press, &screen, display)].try_map(|(source, target, settings)| source.create_transform_8bit(Layout::Rgba, target, Layout::Rgba, settings))?),
            )
        }
    };
    let tile = raster.tile.get();
    let side = |origin: u32, extent: u32| extent.strict_sub(origin).saturating_cast::<u16>().min(tile);
    let cache = RenderCache::new();
    let interpreter_settings = raster.interpreter_settings.clone().unwrap_or_default();
    let render_settings = raster.render_settings.unwrap_or_default();
    let placement = Affine::scale(density) * frame;
    let mut context = RenderContext::new(side(0, columns), side(0, rows));
    let mut pixmap = Pixmap::new(context.width(), context.height());
    let mut resources = Resources::new();
    let mut image = RgbaImage::new(columns, rows);
    let mut inks = Vec::new();
    (0..rows).step_by(usize::from(tile)).flat_map(|y| (0..columns).step_by(usize::from(tile)).map(move |x| (x, y))).try_for_each(|(x, y)| {
        let [across, down] = [side(x, columns), side(y, rows)];
        context.reset_and_resize(across, down);
        render_into(page, &cache, &interpreter_settings, &render_settings, &mut context, Affine::translate((-f64::from(x), -f64::from(y))) * placement);
        context.flush();
        pixmap.resize(across, down);
        context.render_with(&mut pixmap, &mut resources, RasterizerSettings { target_init: TargetInit::Clear(background), ..RasterizerSettings::default() });
        if pixmap.may_have_transparency() {
            unpremultiply_rgba8(pixmap.data_as_u8_slice_mut());
        }
        let pixels = pixmap.data_as_u8_slice_mut();
        if let Some([forward, back]) = &proof {
            inks.resize(pixels.len(), 0);
            forward.transform(pixels, &mut inks)?;
            back.transform(&inks, pixels)?;
        }
        let samples = FlatSamples { samples: pixels, layout: SampleLayout::row_major_packed(<Rgba<u8> as Pixel>::CHANNEL_COUNT, across.into(), down.into()), color_hint: None };
        image.copy_from(&samples.as_view::<Rgba<u8>>().map_err(ImageError::from)?, x, y).map_err(PdfError::from)
    })?;
    Ok(image)
}

pub fn svg(document: &Document, index: usize, background: AlphaColor<Srgb>, interpreter_settings: &InterpreterSettings) -> Result<String, PdfError> {
    page(document, index).map(|sheet| hayro_svg::convert(sheet, &hayro_svg::RenderCache::new(), interpreter_settings, &SvgRenderSettings { bg_color: background.to_rgba8().to_u8_array() }))
}

pub fn compare(pages: [(&Document, usize); 2], raster: &Raster, algorithm: &Algorithm) -> Result<Similarity, PdfError> {
    let rendered = |part: usize, (document, index): (&Document, usize)| Validated::from(render(document, index, raster).map(|image| image.convert()).map_err(|error| (Subject::Part(part), error)));
    let [before, after] = pages;
    rendered(0, before)
        .map2(rendered(1, after), |first: RgbImage, second: RgbImage| image_compare::rgb_similarity_structure(algorithm, &first, &second))
        .ok()
        .map_err(PdfError::Refused)?
        .map_err(PdfError::from)
}
