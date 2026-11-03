use core::iter;
use std::{io::Cursor, sync::Arc};

use fast_image_resize::{ResizeOptions, Resizer};
use image::{
    DynamicImage, ImageBuffer, ImageError, ImageFormat, RgbaImage,
    error::{ParameterError, ParameterErrorKind},
};
use krilla::{
    Data,
    image::{BitsPerComponent, CustomImage, Image, ImageColorspace},
};
use kurbo::Size;
use nonempty_collections::{NEVec, nev};
use tiff::{
    ColorType, TiffError, TiffFormatError, TiffUnsupportedError,
    decoder::{Decoder, DecodingResult, Limits, ifd::Value},
    tags::{PlanarConfiguration, ResolutionUnit, Tag},
};
use uom::si::{
    f64::{Length, Ratio},
    length::{centimeter, inch, millimeter},
    ratio::ratio,
};
use validated::Validated;

use crate::{PdfError, Resolution};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Hash)]
pub(crate) struct Frame {
    color_channel: Arc<[u8]>,
    alpha_channel: Option<Arc<[u8]>>,
    bits_per_component: BitsPerComponent,
    size: (u32, u32),
    icc_profile: Option<Arc<[u8]>>,
    color_space: ImageColorspace,
}

impl CustomImage for Frame {
    fn color_channel(&self) -> &[u8] { &self.color_channel }

    fn alpha_channel(&self) -> Option<&[u8]> { self.alpha_channel.as_deref() }

    fn bits_per_component(&self) -> BitsPerComponent { self.bits_per_component }

    fn size(&self) -> (u32, u32) { self.size }

    fn icc_profile(&self) -> Option<&[u8]> { self.icc_profile.as_deref() }

    fn color_space(&self) -> ImageColorspace { self.color_space }
}

pub(crate) enum Raster {
    Encoded { image: Image, bytes: Arc<[u8]> },
    Frame(Frame),
    Pixels(RgbaImage),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn raster(bytes: &Arc<[u8]>) -> Result<Raster, PdfError> {
    let encoded = |image: Result<Image, String>| image.map(|image| Raster::Encoded { image, bytes: Arc::clone(bytes) }).map_err(|message| PdfError::Decode(message.into()));
    let shared: Arc<dyn AsRef<[u8]> + Send + Sync> = Arc::new(Arc::clone(bytes));
    match image::guess_format(bytes)? {
        ImageFormat::Tiff => decoded(&mut decoder(bytes)?).map(|(_, frame)| Raster::Frame(frame)),
        ImageFormat::Png => encoded(Image::from_png(Data::from(shared), false)),
        ImageFormat::Jpeg => encoded(Image::from_jpeg(Data::from(shared), false)),
        _ => Ok(Raster::Pixels(image::load_from_memory(bytes)?.into_rgba8())),
    }
}

pub(crate) fn frames(tiff: &[u8]) -> Result<impl Iterator<Item = Result<(Size, Raster), PdfError>>, PdfError> {
    let mut reader = decoder(tiff)?;
    let mut pending = true;
    Ok(iter::from_fn(move || {
        pending.then(|| {
            pending = false;
            let (paper, frame) = decoded(&mut reader)?;
            if reader.more_images() {
                reader.next_image()?;
                pending = true;
            }
            Ok::<_, PdfError>((paper, Raster::Frame(frame)))
        })
    }))
}

#[must_use]
pub(crate) fn target(raster: &Raster, placed: Size, cap: Option<Resolution>) -> (u32, u32) {
    let (width, height) = match raster {
        Raster::Encoded { image, bytes: _ } => image.size(),
        Raster::Frame(frame) => frame.size,
        Raster::Pixels(pixels) => pixels.dimensions(),
    };
    cap.map_or((width, height), |density| {
        let limited = |pixels: u32, paper: f64| {
            let measure: Ratio = Length::new::<millimeter>(paper) * *density;
            measure.get::<ratio>().ceil().to_int_saturating::<u32>().clamp(1, pixels.max(1))
        };
        (limited(width, placed.width), limited(height, placed.height))
    })
}

pub(crate) fn image(raster: &Raster, (across, down): (u32, u32)) -> Result<Image, NEVec<PdfError>> {
    let resized = |source: &RgbaImage| -> Result<Image, PdfError> {
        let pixels = if source.dimensions() == (across, down) {
            source.clone()
        } else {
            let mut target = RgbaImage::new(across, down);
            Resizer::new().resize(source, &mut target, None::<&ResizeOptions>)?;
            target
        };
        Ok(Image::from_rgba8(pixels.into_raw(), across, down))
    };
    let custom = |frame: Frame| {
        frame.color_channel.len().checked_ilog2().is_none_or(|bits| bits < u32::BITS).ok_or_else(|| nev![PdfError::Raster { width: across.into(), height: down.into() }])?;
        Image::from_custom(frame, false).map_err(|message| nev![PdfError::Decode(message.into())])
    };
    match raster {
        Raster::Encoded { image, bytes: _ } if image.size() == (across, down) => Ok(image.clone()),
        Raster::Encoded { image: _, bytes } => image::load_from_memory(bytes).map_err(PdfError::from).and_then(|decoded| resized(&decoded.into_rgba8())).map_err(|error| nev![error]),
        Raster::Pixels(pixels) => resized(pixels).map_err(|error| nev![error]),
        Raster::Frame(frame) if frame.size == (across, down) => custom(frame.clone()),
        Raster::Frame(frame) => {
            let Frame { color_channel, alpha_channel, bits_per_component, size: (width, height), icc_profile: _, color_space } = frame;
            let plane = |channel: &[u8], space: ImageColorspace| {
                let wide = || channel.as_chunks::<2>().0.iter().map(|&pair| u16::from_be_bytes(pair)).collect::<Vec<_>>();
                let source = match (bits_per_component, space) {
                    (BitsPerComponent::Eight, ImageColorspace::Luma) => ImageBuffer::from_raw(*width, *height, channel.to_vec()).map(DynamicImage::ImageLuma8),
                    (BitsPerComponent::Eight, ImageColorspace::Rgb) => ImageBuffer::from_raw(*width, *height, channel.to_vec()).map(DynamicImage::ImageRgb8),
                    (BitsPerComponent::Eight, ImageColorspace::Cmyk) => ImageBuffer::from_raw(*width, *height, channel.to_vec()).map(DynamicImage::ImageRgba8),
                    (BitsPerComponent::Sixteen, ImageColorspace::Luma) => ImageBuffer::from_raw(*width, *height, wide()).map(DynamicImage::ImageLuma16),
                    (BitsPerComponent::Sixteen, ImageColorspace::Rgb) => ImageBuffer::from_raw(*width, *height, wide()).map(DynamicImage::ImageRgb16),
                    (BitsPerComponent::Sixteen, ImageColorspace::Cmyk) => ImageBuffer::from_raw(*width, *height, wide()).map(DynamicImage::ImageRgba16),
                };
                Validated::from(source.ok_or_else(|| PdfError::from(ImageError::Parameter(ParameterError::from_kind(ParameterErrorKind::DimensionMismatch)))).and_then(|source| {
                    let mut output = DynamicImage::new(across, down, source.color());
                    Resizer::new().resize(&source, &mut output, &ResizeOptions::new().use_alpha(false))?;
                    Ok(match output.as_flat_samples_u16() {
                        Some(samples) => samples.samples.iter().flat_map(|sample| sample.to_be_bytes()).collect(),
                        None => Arc::from(output.into_bytes()),
                    })
                }))
            };
            plane(color_channel, *color_space)
                .map2(alpha_channel.as_deref().map_or(Validated::Good(None), |alpha| plane(alpha, ImageColorspace::Luma).map(Some)), |color_channel, alpha_channel| Frame {
                    color_channel,
                    alpha_channel,
                    size: (across, down),
                    ..frame.clone()
                })
                .ok()
                .and_then(custom)
        }
    }
}

fn decoder(tiff: &[u8]) -> Result<Decoder<Cursor<&[u8]>>, PdfError> { Decoder::new(Cursor::new(tiff)).map(|opened| opened.with_limits(Limits::unlimited())).map_err(PdfError::from) }

fn decoded(reader: &mut Decoder<Cursor<&[u8]>>) -> Result<(Size, Frame), PdfError> {
    let size @ (width, height) = reader.dimensions()?;
    let color = reader.colortype()?;
    let (color_space, components, samples) = match color {
        ColorType::Gray(_) => (ImageColorspace::Luma, 1_usize, 1_usize),
        ColorType::GrayA(_) | ColorType::Multiband { bit_depth: _, num_samples: 2 } => (ImageColorspace::Luma, 1, 2),
        ColorType::RGB(_) => (ImageColorspace::Rgb, 3, 3),
        ColorType::RGBA(_) => (ImageColorspace::Rgb, 3, 4),
        ColorType::CMYK(_) => (ImageColorspace::Cmyk, 4, 4),
        ColorType::CMYKA(_) => (ImageColorspace::Cmyk, 4, 5),
        other => return Err(TiffError::UnsupportedError(TiffUnsupportedError::UnsupportedColorType(other)).into()),
    };
    (reader.image_buffer_layout()?.planes == 1).ok_or(TiffError::UnsupportedError(TiffUnsupportedError::UnsupportedPlanarConfig(Some(PlanarConfiguration::Planar))))?;
    let unit = match reader.find_tag_unsigned::<u16>(Tag::ResolutionUnit)?.map(ResolutionUnit::from_u16) {
        None | Some(Some(ResolutionUnit::Inch)) => Length::new::<inch>(1.0),
        Some(Some(ResolutionUnit::Centimeter)) => Length::new::<centimeter>(1.0),
        Some(_) => return Err(TiffError::FormatError(TiffFormatError::InvalidTag).into()),
    };
    let mut length = |pixels: u32, tag: Tag| -> Result<f64, TiffError> {
        let density = match reader.get_tag(tag)? {
            Value::Rational(numerator, denominator) => f64::from(numerator) / f64::from(denominator),
            other => other.into_f64()?,
        };
        (density.is_normal() && density.is_sign_positive()).ok_or(TiffError::FormatError(TiffFormatError::InvalidTag)).map(|()| (unit * (f64::from(pixels) / density)).get::<millimeter>())
    };
    let measured = Size::new(length(width, Tag::XResolution)?, length(height, Tag::YResolution)?);
    let icc_profile = reader.find_tag(Tag::IccProfile)?.map(Value::into_u8_vec).transpose()?.map(Arc::from);
    let mut buffer = DecodingResult::U8(Vec::new());
    reader.read_image_to_buffer(&mut buffer)?;
    let (bytes, bits_per_component, width_of_sample) = match (buffer, color.bit_depth()) {
        (DecodingResult::U8(packed), 8) => (packed, BitsPerComponent::Eight, 1_usize),
        (DecodingResult::U8(packed), depth @ (1 | 2 | 4)) => {
            let across = usize::try_from(width).map_err(TiffError::from)?.strict_mul(samples);
            let mask = u8::MAX.strict_shr(u32::from(8_u8.strict_sub(depth)));
            let widened = packed
                .chunks_exact(across.strict_mul(depth.into()).div_ceil(8))
                .flat_map(|row| {
                    row.iter()
                        .flat_map(move |&byte| (1..=8_u8.strict_div(depth)).map(move |position| (byte.rotate_left(u32::from(position.strict_mul(depth))) & mask).strict_mul(u8::MAX.strict_div(mask))))
                        .take(across)
                })
                .collect();
            (widened, BitsPerComponent::Eight, 1)
        }
        (DecodingResult::U16(wide), 16) => (wide.iter().flat_map(|sample| sample.to_be_bytes()).collect(), BitsPerComponent::Sixteen, 2),
        (_, depth) => return Err(TiffError::UnsupportedError(TiffUnsupportedError::UnsupportedBitsPerChannel(depth)).into()),
    };
    let (color_channel, alpha_channel) = if samples == components {
        (Arc::from(bytes), None)
    } else {
        let (colors, alphas): (Vec<_>, Vec<_>) = bytes.chunks_exact(samples.strict_mul(width_of_sample)).filter_map(|pixel| pixel.split_at_checked(components.strict_mul(width_of_sample))).unzip();
        (Arc::from(colors.concat()), Some(Arc::from(alphas.concat())))
    };
    Ok((measured, Frame { color_channel, alpha_channel, bits_per_component, size, icc_profile, color_space }))
}
