use core::{alloc, fmt, num::NonZero};

use color::{AlphaColor, Srgb, palette};
use hayro::{
    RenderCache, RenderSettings,
    hayro_interpret::{InterpreterSettings, TransformExt as _},
    render_into,
    vello_cpu::{Pixmap, RasterizerSettings, RenderContext, Resources, TargetInit, peniko::ImageAlphaType},
};
use hayro_svg::SvgRenderSettings;
use hayro_syntax::object::dict::keys::USER_UNIT;
use image::{FlatSamples, GenericImage as _, ImageError, Pixel, RgbImage, Rgba, RgbaImage, buffer::ConvertBuffer as _, flat::SampleLayout};
use image_compare::{Algorithm, Similarity};
use kurbo::Affine;
use moxcms::{ColorProfile, Layout, RenderingIntent, TransformOptions};
use uom::si::linear_number_density::per_inch;
use validated::Validated;

use crate::{Document, OutputIntent, POINTS_PER_INCH, PdfError, Resolution, Subject, page};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub enum Appearance {
    Screen { background: AlphaColor<Srgb> },
    Press { intent: OutputIntent, rendering: RenderingIntent },
}

#[derive(Clone)]
pub struct Raster {
    pub resolution: Resolution,
    pub appearance: Option<Appearance> = None,
    pub tile: NonZero<u16> = NonZero::<u16>::saturating_add(NonZero::<u16>::MIN, 4095),
    pub interpreter_settings: Option<InterpreterSettings> = None,
    pub render_settings: Option<RenderSettings> = None,
}

impl fmt::Debug for Raster {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result {
        formatter.debug_struct("Raster").field("resolution", &self.resolution).field("appearance", &self.appearance).field("tile", &self.tile).finish_non_exhaustive()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn render(document: &Document, index: usize, raster: &Raster) -> Result<RgbaImage, PdfError> {
    let page = page(document, index)?;
    let unit = page.raw().get::<f64>(USER_UNIT).filter(|unit| unit.is_normal() && unit.is_sign_positive()).unwrap_or(1.0);
    let scale = raster.resolution.get::<per_inch>() * unit / POINTS_PER_INCH;
    let [width, height] = <[f32; 2]>::from(page.render_dimensions()).map(|side| (f64::from(side) * scale).ceil().to_int_saturating::<u64>());
    let (columns, rows) = u32::try_from(width)
        .ok()
        .zip(u32::try_from(height).ok())
        .filter(|&(columns, rows)| usize::try_from(u64::from(columns).strict_mul(u64::from(rows))).is_ok_and(|area| alloc::Layout::array::<Rgba<u8>>(area).is_ok()))
        .ok_or(PdfError::Raster { width, height })?;
    let (background, proof) = match &raster.appearance {
        None => (palette::css::WHITE, None),
        Some(Appearance::Screen { background }) => (*background, None),
        Some(Appearance::Press { intent, rendering }) => {
            let screen = ColorProfile::new_srgb();
            let press: &ColorProfile = intent.as_ref();
            let options = TransformOptions { rendering_intent: *rendering, ..TransformOptions::default() };
            let display = TransformOptions { rendering_intent: RenderingIntent::RelativeColorimetric, ..options };
            (palette::css::WHITE, Some([screen.create_transform_8bit(Layout::Rgba, press, Layout::Rgba, options)?, press.create_transform_8bit(Layout::Rgba, &screen, Layout::Rgba, display)?]))
        }
    };
    let tile = raster.tile.get();
    let side = |origin: u32, extent: u32| u16::try_from(extent.strict_sub(origin)).map_or(tile, |rest| rest.min(tile));
    let cache = RenderCache::new();
    let interpreter_settings = raster.interpreter_settings.clone().unwrap_or_default();
    let render_settings = raster.render_settings.unwrap_or_default();
    let placement = Affine::scale(scale) * page.initial_transform(true).to_kurbo();
    let mut context = RenderContext::new(tile, tile);
    let mut resources = Resources::new();
    let mut image = RgbaImage::new(columns, rows);
    let mut inks = Vec::new();
    (0..rows).step_by(usize::from(tile)).flat_map(|y| (0..columns).step_by(usize::from(tile)).map(move |x| (x, y))).try_for_each(|(x, y)| {
        let [across, down] = [side(x, columns), side(y, rows)];
        context.reset_and_resize(across, down);
        render_into(page, &cache, &interpreter_settings, &render_settings, &mut context, Affine::translate((-f64::from(x), -f64::from(y))) * placement);
        context.flush();
        let mut pixmap = Pixmap::new(across, down);
        context.render_with(&mut pixmap, &mut resources, RasterizerSettings { target_init: TargetInit::Clear(background), ..RasterizerSettings::default() });
        let mut pixels = pixmap.take_rgba8(ImageAlphaType::Alpha);
        if let Some([forward, back]) = &proof {
            inks.resize(pixels.len(), 0);
            forward.transform(&pixels, &mut inks)?;
            back.transform(&inks, &mut pixels)?;
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
