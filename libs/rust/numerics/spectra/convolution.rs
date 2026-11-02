use ndarray_conv::{ConvFFTExt as _, ConvMode, PaddingMode, ReverseKernel as _};

use crate::{Samples, Series, SpectraError};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Product {
    Convolution,
    Correlation,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn convolve(data: &Series, kernel: &Series, product: Product) -> Result<Series, SpectraError> {
    (data.rate == kernel.rate).ok_or(SpectraError::Rates { data: data.rate, kernel: kernel.rate })?;
    let kernel = match product {
        Product::Convolution => kernel.samples.reverse(),
        Product::Correlation => kernel.samples.no_reverse(),
    };
    Samples::try_new(data.samples.conv_fft_par(kernel, ConvMode::Full, PaddingMode::Zeros)?).map(|samples| Series { rate: data.rate, samples })
}
