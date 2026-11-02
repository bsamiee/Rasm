use itertools::Itertools as _;
use merman::{
    OperationControl, RenderError, RenderOutput, RenderRequest, Renderer, SvgRequest,
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
pub fn diagrams<'a, D>(context: NativeFontContext, renderer: &Renderer, request: SvgRequest, control: &OperationControl, diagrams: D) -> Validated<Vec<(FileId, Bytes)>, TypesetError>
where D: IntoParallelIterator<Item = (VirtualPath, &'a str)> {
    match request.environment.with_native_font_context(context) {
        Err(error) => Validated::fail(TypesetError::NativeFont(error)),
        Ok(environment) => {
            let request = SvgRequest { environment, pipeline: Some(request.pipeline.unwrap_or_default().into_resvg_safe()), ..request };
            diagrams
                .into_par_iter()
                .map(|(path, source)| match renderer.render(RenderRequest::svg(source, control.child(), request.clone())) {
                    Ok(RenderOutput::Svg(Some(output))) => Ok((RootedPath::new(VirtualRoot::Project, path).intern(), Bytes::from_string(output.into_parts().0))),
                    Ok(RenderOutput::Svg(None)) => Err(TypesetError::Diagram { path, source: RenderError::NoDiagram }),
                    Ok(RenderOutput::Semantic(_) | RenderOutput::LayoutJson(_) | RenderOutput::EdgeGeometryJson(_) | RenderOutput::SvgPlan(_)) => {
                        Err(TypesetError::Diagram { path, source: RenderError::UnsupportedTarget("svg") })
                    }
                    Err(error) => Err(TypesetError::Diagram { path, source: error }),
                })
                .collect()
        }
    }
}
