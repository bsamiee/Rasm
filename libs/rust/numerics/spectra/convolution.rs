use ndarray_conv::{ConvFFTExt as _, ConvMode, Error, PaddingMode, ReverseKernel as _};

use crate::{Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Product {
    Convolution,
    Correlation,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy)]
pub struct Boundary {
    pub mode: ConvMode<1> = ConvMode::Full,
    pub padding: PaddingMode<1, f64> = PaddingMode::Zeros,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn convolve(data: &Series, kernel: &Series, product: Product, boundary: Boundary) -> Result<Series, SpectraError> {
    let Boundary { mode, padding } = boundary;
    (data.rate == kernel.rate).ok_or(SpectraError::Rates { data: data.rate, kernel: kernel.rate })?;
    let stride = match mode {
        ConvMode::Full | ConvMode::Same | ConvMode::Valid => 1,
        ConvMode::Custom { padding: _, strides: [stride] } | ConvMode::Explicit { padding: _, strides: [stride] } => stride,
    };
    (stride == 1).ok_or(Error::MismatchShape(mode, [kernel.samples.len()]))?;
    let oriented = match product {
        Product::Convolution => kernel.samples.reverse(),
        Product::Correlation => kernel.samples.no_reverse(),
    };
    Samples::try_new(data.samples.conv_fft_par(oriented, mode, padding)?).map(|samples| Series { rate: data.rate, samples })
}
