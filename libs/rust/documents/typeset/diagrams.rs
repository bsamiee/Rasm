use merman::{
    OperationControl, RenderError, RenderOutput, RenderRequest, Renderer, SvgOutput, SvgRequest,
    svg::export::{NativeFontContext, NativeFontLimits},
};
use typst::foundations::Bytes;
use typst_kit::fonts::FontStore;

use crate::TypesetError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn context(fonts: &FontStore, limits: NativeFontLimits, control: &OperationControl) -> Result<NativeFontContext, TypesetError> {
    let mut buffers = (0..).map_while(|index| fonts.source(index).map(|_| fonts.font(index))).flatten().map(|font| font.data().clone()).collect::<Vec<_>>();
    buffers.dedup();
    NativeFontContext::from_font_bytes(buffers.into_iter().map(Bytes::into_vec), limits, control).map_err(RenderError::NativeFont).map_err(TypesetError::Diagram)
}

pub fn svg(context: NativeFontContext, renderer: &Renderer, request: SvgRequest, source: &str, control: OperationControl) -> Result<Option<SvgOutput>, TypesetError> {
    let request = SvgRequest {
        environment: request.environment.with_native_font_context(context).map_err(RenderError::NativeFont)?,
        pipeline: Some(request.pipeline.unwrap_or_default().into_resvg_safe()),
        ..request
    };
    match renderer.render(RenderRequest::svg(source, control, request))? {
        RenderOutput::Svg(output) => Ok(output),
        RenderOutput::Semantic(_) | RenderOutput::LayoutJson(_) | RenderOutput::EdgeGeometryJson(_) | RenderOutput::SvgPlan(_) => Err(TypesetError::Diagram(RenderError::UnsupportedTarget("svg"))),
    }
}
