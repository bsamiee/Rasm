use core::iter;
use core::ops::DerefMut;
use std::collections::HashMap;
use std::error::Report;

use itertools::Itertools as _;
use merman::svg::export::{NativeFontContext, NativeFontLimits};
use merman::{OperationControl, RenderError, RenderEvidence, RenderOutput, RenderRequest, Renderer, SvgOutput, SvgRequest};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rayon::iter::{IntoParallelIterator, ParallelIterator};
use typst::diag::Warned;
use typst::foundations::{Bytes, Output, Repr as _};
use typst::syntax::ast::{Raw, Text};
use typst::syntax::{FileId, RootedPath, Source, SyntaxKind, SyntaxNode, VirtualRoot};
use typst::utils::hash128;
use typst_kit::files::FileStore;
use typst_kit::fonts::FontStore;

use crate::{Files, Project, TypesetError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub struct Compilation<T> {
    pub output: Result<Warned<T>, TypesetError>,
    pub evidence: Vec<(FileId, RenderEvidence)>,
    pub errors: Vec<TypesetError>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn svg_request<'family, I>(fonts: &FontStore, families: I, request: SvgRequest, limits: NativeFontLimits, control: &OperationControl) -> Result<SvgRequest, TypesetError>
where I: IntoIterator<Item = &'family str> {
    let buffers = families
        .into_iter()
        .flat_map(|family| {
            let indices = fonts.book().select_family(&family.to_lowercase()).collect::<Vec<_>>();
            if indices.is_empty() {
                tracing::warn!(family, "font family holds no face");
            }
            indices
        })
        .filter_map(|index| {
            fonts.font(index).or_else(|| {
                tracing::warn!(index, "font index holds no readable face");
                None
            })
        })
        .map(|font| font.data().clone())
        .unique()
        .map(Bytes::into_vec);
    let environment = NativeFontContext::from_font_bytes(buffers, limits, control).and_then(|context| request.environment.with_native_font_context(context)).map_err(TypesetError::NativeFont)?;
    Ok(SvgRequest { environment, pipeline: Some(request.pipeline.unwrap_or_default().into_resvg_safe()), ..request })
}

#[must_use]
pub fn blocks(source: &Source) -> Vec<(FileId, String)> {
    gen move {
        let mut pending = vec![source.root()];
        while let Some(node) = pending.pop() {
            if node.kind() != SyntaxKind::Raw {
                pending.extend(node.children().rev());
            }
            yield node;
        }
    }
    .filter_map(SyntaxNode::cast::<Raw<'_>>)
    .filter(|raw| raw.lang().is_some_and(|lang| lang.get() == "mermaid"))
    .map(|raw| block(source, raw.lines().map(Text::get).join("\n")))
    .collect()
}

fn block(source: &Source, text: String) -> (FileId, String) { (RootedPath::new(VirtualRoot::Project, source.id().vpath().with_extension(&format!("{:032x}.svg", hash128(&text)))).intern(), text) }

#[must_use]
pub fn svg<D, S>(renderer: &Renderer, request: &SvgRequest, control: &OperationControl, sources: D) -> impl ParallelIterator<Item = (FileId, Result<SvgOutput, RenderError>)>
where
    D: IntoParallelIterator<Item = (FileId, S)>,
    S: AsRef<str>,
{
    sources.into_par_iter().map(move |(id, source)| {
        (
            id,
            renderer.render(RenderRequest::svg(source.as_ref(), control.child(), request.clone())).and_then(|output| match output {
                RenderOutput::Svg(Some(output)) => Ok(output),
                RenderOutput::Svg(None) => Err(RenderError::NoDiagram),
                _ => Err(RenderError::UnsupportedTarget("svg")),
            }),
        )
    })
}

pub fn compile<T, F>(project: &mut Project<F>, renderer: &Renderer, request: &SvgRequest, control: &OperationControl) -> Compilation<T>
where
    T: Output,
    F: DerefMut<Target = FileStore<Files>> + Send + Sync,
{
    let main = project.main;
    let accessed = |files: &mut F| -> Vec<FileId> { files.dependencies().1.filter(|id| *id != main && matches!(id.vpath().extension(), Some("typ" | "mmd"))).chain(iter::once(main)).collect() };
    let present = |sources: Vec<FileId>, files: &F| -> HashMap<FileId, String> {
        sources
            .into_iter()
            .filter_map(|id| files.source(id).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), file = id.vpath().get_without_slash(), "diagram source refused")).ok())
            .flat_map(|source| match source.id().vpath().extension() {
                Some("mmd") => vec![block(&source, source.text().to_owned())],
                _ => blocks(&source),
            })
            .collect()
    };
    let settled = |files: &Files, failures: &HashMap<FileId, RenderError>, id: FileId| files.diagrams.contains_key(&id) || failures.contains_key(&id);
    let module = RootedPath::new(VirtualRoot::Project, main.vpath().with_extension("mermaid.typ")).intern();
    let sources = accessed(&mut project.files);
    project.files.reset();
    let mut current = present(sources, &project.files);
    project.files.loader_mut().diagrams.clear();
    let mut evidence = Vec::new();
    let mut failures = HashMap::new();
    loop {
        let pending = current.iter().filter(|&(&id, _)| !settled(project.files.loader(), &failures, id)).map(|(&id, text)| (id, text.as_str())).collect::<Vec<_>>();
        for (id, result) in svg(renderer, request, control, pending).collect::<Vec<_>>() {
            match result {
                Ok(output) => {
                    let (svg, report) = output.into_parts();
                    project.files.loader_mut().diagrams.insert(id, Bytes::from_string(svg));
                    evidence.push((id, report));
                }
                Err(source) => {
                    failures.insert(id, source);
                }
            }
        }
        let files = project.files.loader_mut();
        files.diagrams.retain(|id, _| current.contains_key(id));
        let table = current
            .iter()
            .filter(|&(id, _)| files.diagrams.contains_key(id))
            .sorted_unstable_by_key(|(id, text)| (*text, id.vpath().get_with_slash()))
            .format_with("", |(id, text), row| row(&format_args!("({}, {}),", text.repr(), id.vpath().get_with_slash().repr())));
        let text = format!(
            "#let diagrams = ({table}).to-dict()\n\
             #let mermaid(body, ..options) = {{\n  \
             show raw.where(lang: \"mermaid\"): it => if it.text in diagrams {{ image(diagrams.at(it.text), alt: it.text, ..options) }} else {{ it }}\n  \
             body\n\
             }}\n"
        );
        files.diagrams.insert(module, Bytes::from_string(text));
        project.files.reset();
        let compiled = crate::compile::<T>(&*project);
        current = present(accessed(&mut project.files), &project.files);
        if current.keys().all(|&id| settled(project.files.loader(), &failures, id)) {
            return Compilation { output: compiled, evidence, errors: failures.into_iter().map(|(id, source)| TypesetError::Diagram { path: id.vpath().clone(), source }).collect() };
        }
    }
}
