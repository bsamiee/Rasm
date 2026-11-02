use itertools::Itertools as _;
use merman::{
    OperationControl, RenderError, RenderEvidence, RenderOutput, RenderRequest, Renderer, SvgRequest,
    svg::export::{NativeFontContext, NativeFontLimits},
};
use rayon::iter::{IntoParallelIterator, ParallelIterator as _};
use typst::{
    foundations::Bytes,
    syntax::{FileId, RootedPath, VirtualPath, VirtualRoot},
};
use typst_kit::fonts::FontStore;
use validated::Validated;

use crate::TypesetError;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn context(fonts: &FontStore, limits: NativeFontLimits, control: &OperationControl) -> Result<NativeFontContext, TypesetError> {
    NativeFontContext::from_font_bytes((0..).map_while(|index| fonts.source(index).map(|_| fonts.font(index))).flatten().map(|font| font.data().clone()).dedup().map(Bytes::into_vec), limits, control)
        .map_err(TypesetError::NativeFont)
}

#[must_use]
pub fn diagrams<'source, D>(context: NativeFontContext, renderer: &Renderer, request: SvgRequest, control: &OperationControl, diagrams: D) -> Validated<Vec<(FileId, Bytes, RenderEvidence)>, TypesetError>
where D: IntoParallelIterator<Item = (VirtualPath, &'source str)> {
    match request.environment.with_native_font_context(context) {
        Err(error) => Validated::fail(TypesetError::NativeFont(error)),
        Ok(environment) => {
            let request = SvgRequest { environment, pipeline: Some(request.pipeline.unwrap_or_default().into_resvg_safe()), ..request };
            diagrams
                .into_par_iter()
                .map(|(path, source)| {
                    match renderer.render(RenderRequest::svg(source, control.child(), request.clone())).and_then(|output| match output {
                        RenderOutput::Svg(Some(output)) => Ok(output.into_parts()),
                        RenderOutput::Svg(None) => Err(RenderError::NoDiagram),
                        RenderOutput::Semantic(_) | RenderOutput::LayoutJson(_) | RenderOutput::EdgeGeometryJson(_) | RenderOutput::SvgPlan(_) => Err(RenderError::UnsupportedTarget("svg")),
                    }) {
                        Ok((svg, evidence)) => Ok((RootedPath::new(VirtualRoot::Project, path).intern(), Bytes::from_string(svg), evidence)),
                        Err(source) => Err(TypesetError::Diagram { path, source }),
                    }
                })
                .collect()
        }
    }
}
