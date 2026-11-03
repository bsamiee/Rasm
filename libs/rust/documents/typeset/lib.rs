#![feature(error_iter, error_reporter, float_conversions, try_blocks_heterogeneous)]

use core::{any::Any, error::Error, fmt, iter, num::NonZero, ops::Deref};
use std::{
    collections::{HashMap, HashSet},
    error::Report,
    io::{self, Read},
    path::{Path, PathBuf},
    sync::Arc,
};

use cap_std::{ambient_authority, fs::Dir};
use itertools::Itertools as _;
use jiff::{SignedDuration, Zoned, tz::Offset};
use merman::{RenderError, svg::export::NativeFontError};
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, HTTP_REQUEST_METHOD, HTTP_RESPONSE_STATUS_CODE, OTEL_STATUS_CODE, URL_FULL};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, ParallelIterator as _};
use reqwest::{
    Method, StatusCode,
    blocking::{Client, Response},
};
use rmp_serde::{decode, encode};
use serde::{Serialize, de::DeserializeOwned};
use tracing::{
    Span,
    field::{self, Empty},
};
use typst::{
    Features, Library, LibraryExt as _, World, WorldExt as _,
    comemo::{self, Track as _},
    diag::{At as _, FileError, FileResult, PackageError, Severity, SourceDiagnostic, Warned},
    ecow::{EcoString, EcoVec, eco_format},
    engine::Sink,
    foundations::{Bytes, Context, Datetime, Dict, Duration, LocatableSelector, Output, Scope, Selector, Value},
    introspection::{self, EmptyIntrospector, MetadataElem},
    routines::SpanMode,
    syntax::{
        self, DiagSpan, FileId, RootedPath, Source, Spanned, SyntaxMode, VirtualPath, VirtualRoot,
        package::{PackageManifest, PackageSpec},
    },
    text::{Font, FontBook, FontInfo},
    utils::LazyHash,
};
use typst_kit::{
    diagnostics::{DiagnosticFormat, DiagnosticWorld, emit, termcolor::NoColor},
    downloader::Downloader,
    files::{FileLoader, FileStore, FsRoot},
    fonts::FontStore,
    packages::SystemPackages,
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub type Compiled<T> = Result<Warned<T>, TypesetError>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const TYPST_WARNINGS: &str = "typst.warnings";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Serialize)]
pub struct Location {
    pub file: PathBuf,
    pub line: usize,
    pub column: usize,
    pub end_line: usize,
    pub end_column: usize,
}

#[derive(Debug, Clone, Serialize)]
pub struct Diagnostic {
    pub severity: &'static str,
    #[serde(flatten)]
    pub location: Option<Location>,
    pub message: EcoString,
    pub hints: Vec<(EcoString, Option<Location>)>,
    pub trace: Vec<(String, Location)>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum TypesetError {
    #[error("typed value refused on encode")]
    Serialize { source: serde_path_to_error::Error<encode::Error> },
    #[error("typed value refused on decode")]
    Deserialize { source: serde_path_to_error::Error<decode::Error> },
    #[error("queried element {index} refused")]
    Element { index: usize, source: Box<Self> },
    #[error("font buffer {index} face {face} is unreadable")]
    Font { index: usize, face: u32 },
    #[error("{spec} template refused")]
    Template { spec: PackageSpec, source: FileError },
    #[error("{report}")]
    Typst { diagnostics: Vec<Diagnostic>, report: String },
    #[error("{} page {page} rasterizes to {width} by {height} pixels past the pixmap size limit", path.get_without_slash())]
    Raster { path: VirtualPath, page: NonZero<usize>, width: u32, height: u32 },
    #[error("{} rasterizes to {pixels} pixels past the {budget} pixel budget", path.get_without_slash())]
    Budget { path: VirtualPath, pixels: u64, budget: NonZero<u64> },
    #[error("{} page {page} PNG encoding failed", path.get_without_slash())]
    Encode { path: VirtualPath, page: NonZero<usize>, source: png::EncodingError },
    #[error("{} write refused", path.get_without_slash())]
    Write { path: VirtualPath, source: io::Error },
    #[error(transparent)]
    NativeFont(NativeFontError),
    #[error("diagram {} refused", path.get_without_slash())]
    Diagram { path: VirtualPath, source: RenderError },
}

// --- [SERVICES] ------------------------------------------------------------------------

// --- [TRANSPORT]
#[derive(Debug, Clone)]
pub struct HttpDownloader(pub Client);

impl Downloader for HttpDownloader {
    fn stream(&self, _key: &dyn Any, url: &str) -> io::Result<(Option<usize>, Box<dyn Read>)> {
        let span = tracing::info_span!(
            "request",
            otel.name = Method::GET.as_str(),
            otel.kind = "client",
            { OTEL_STATUS_CODE } = Empty,
            { HTTP_REQUEST_METHOD } = Method::GET.as_str(),
            { URL_FULL } = url,
            { HTTP_RESPONSE_STATUS_CODE } = Empty,
            { ERROR_TYPE } = Empty,
            { EXCEPTION_MESSAGE } = Empty
        );
        let response = span
            .in_scope(|| self.0.get(url).send())
            .inspect(|response| {
                span.record(HTTP_RESPONSE_STATUS_CODE, response.status().as_u16());
            })
            .and_then(Response::error_for_status)
            .map_err(|error| failure(&span, io::Error::other(error)))?;
        let length = response.content_length().map(usize::try_from).transpose().map_err(|error| failure(&span, io::Error::other(error)))?;
        Ok((length, Box::new(Body { span, response })))
    }
}

struct Body {
    span: Span,
    response: Response,
}

impl Read for Body {
    fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> { self.span.in_scope(|| self.response.read(buf)).map_err(|error| failure(&self.span, error)) }
}

fn kind(error: &reqwest::Error) -> io::ErrorKind {
    match error.status() {
        Some(StatusCode::NOT_FOUND) => io::ErrorKind::NotFound,
        Some(_) => io::ErrorKind::Other,
        None if error.is_timeout() => io::ErrorKind::TimedOut,
        None => <dyn Error>::sources(error).find_map(|cause| cause.downcast_ref::<io::Error>()).map_or(io::ErrorKind::Other, io::Error::kind),
    }
}

fn failure(span: &Span, error: io::Error) -> io::Error {
    let (status, error) = error.downcast::<reqwest::Error>().map_or_else(|error| (None, error), |error| (error.status(), io::Error::new(kind(&error), error)));
    match status {
        Some(status) => span.record(ERROR_TYPE, status.as_str()),
        None => span.record(ERROR_TYPE, field::debug(error.kind())),
    }
    .record(OTEL_STATUS_CODE, "error")
    .record(EXCEPTION_MESSAGE, field::display(Report::new(&error)));
    error
}

// --- [FILES]
#[derive(Debug, Default)]
pub struct Files {
    pub root: Option<FsRoot>,
    pub overlay: HashMap<FileId, Bytes>,
    pub diagrams: HashMap<FileId, Bytes>,
    pub packages: Option<SystemPackages>,
    pub locked: Option<HashSet<PackageSpec>>,
}

impl Files {
    fn memory(&self, id: FileId) -> Option<&Bytes> { self.overlay.get(&id).or_else(|| self.diagrams.get(&id)) }

    fn root(&self, id: FileId) -> FileResult<FsRoot> {
        match (id.root(), &self.root, &self.packages) {
            (VirtualRoot::Project, Some(root), _) => Ok(root.clone()),
            (VirtualRoot::Project, None, _) => Err(FileError::NotFound(PathBuf::from(id.vpath().get_without_slash()))),
            (VirtualRoot::Package(spec), _, Some(packages)) if self.locked.as_ref().is_none_or(|locked| locked.contains(spec)) => Ok(packages.obtain(spec)?),
            (VirtualRoot::Package(spec), _, _) => Err(FileError::Package(PackageError::NotFound(spec.clone()))),
        }
    }

    pub fn resolve(&self, id: FileId) -> FileResult<Option<PathBuf>> {
        if self.memory(id).is_some() {
            return Ok(None);
        }
        let root = self.root(id)?;
        let path = root.resolve(id.vpath())?;
        directory(&root)?.canonicalize(id.vpath().get_without_slash()).map(|real| Some(root.path().join(real))).map_err(|error| FileError::from_io(error, &path))
    }
}

impl FileLoader for Files {
    fn load(&self, id: FileId) -> FileResult<Bytes> {
        match self.memory(id) {
            Some(bytes) => Ok(bytes.clone()),
            None => self.root(id).and_then(|root| read(&root, id.vpath())),
        }
    }
}

fn directory(root: &FsRoot) -> FileResult<Dir> { Dir::open_ambient_dir(root.path(), ambient_authority()).map_err(|error| FileError::from_io(error, root.path())) }

fn read(root: &FsRoot, path: &VirtualPath) -> FileResult<Bytes> {
    let real = root.resolve(path)?;
    directory(root)?.read(path.get_without_slash()).map(Bytes::new).map_err(|error| match error.kind() {
        io::ErrorKind::IsADirectory => FileError::IsDirectory,
        _ => FileError::from_io(error, &real),
    })
}

// --- [PROJECT]
#[derive(derive_more::Debug)]
pub struct Project<F = Box<FileStore<Files>>> {
    pub library: LazyHash<Library>,
    #[debug(skip)]
    pub fonts: Arc<FontStore>,
    #[debug(skip)]
    pub files: F,
    pub main: FileId,
    pub now: Zoned,
}

impl<L> Project<Box<FileStore<L>>>
where L: FileLoader
{
    pub fn new<I>(inputs: &I, features: Features, fonts: Arc<FontStore>, files: L, main: VirtualPath, now: Zoned) -> Result<Self, TypesetError>
    where I: Serialize + ?Sized {
        library(inputs, features).map(|library| Self { library, fonts, files: Box::new(FileStore::new(files)), main: RootedPath::new(VirtualRoot::Project, main).intern(), now })
    }
}

impl<F, L> World for Project<F>
where
    F: Deref<Target = FileStore<L>> + Send + Sync,
    L: FileLoader,
{
    fn library(&self) -> &LazyHash<Library> { &self.library }

    fn book(&self) -> &LazyHash<FontBook> { self.fonts.book() }

    fn main(&self) -> FileId { self.main }

    fn source(&self, id: FileId) -> FileResult<Source> { self.files.source(id) }

    fn file(&self, id: FileId) -> FileResult<Bytes> { self.files.file(id) }

    fn font(&self, index: usize) -> Option<Font> { self.fonts.font(index) }

    fn today(&self, offset: Option<Duration>) -> Option<Datetime> {
        offset
            .map_or(Ok(self.now.date()), |shift| {
                SignedDuration::try_from_secs_f64(shift.seconds()).and_then(|span| self.now.timestamp().checked_add(span)).map(|instant| Offset::UTC.to_datetime(instant).date())
            })
            .inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "today offset refused"))
            .ok()
            .and_then(|date| Datetime::from_ymd(i32::from(date.year()), date.month().cast_unsigned(), date.day().cast_unsigned()))
    }
}

impl<F, L> DiagnosticWorld for Project<F>
where
    F: Deref<Target = FileStore<L>> + Send + Sync,
    L: FileLoader,
{
    fn name(&self, id: FileId) -> String {
        match id.root() {
            VirtualRoot::Project => id.vpath().get_without_slash().to_owned(),
            VirtualRoot::Package(spec) => format!("{spec}{}", id.vpath().get_with_slash()),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [FONTS]
#[must_use]
pub fn fonts<B>(buffers: B) -> Validated<Vec<(Font, FontInfo)>, TypesetError>
where
    B: IntoParallelIterator<Item = Bytes>,
    B::Iter: IndexedParallelIterator,
{
    buffers
        .into_par_iter()
        .enumerate()
        .map(|(index, data)| {
            (0..ttf_parser::fonts_in_collection(&data).unwrap_or(1).max(1)).map(|face| Font::new(data.clone(), face).ok_or(TypesetError::Font { index, face })).collect::<Result<Vec<_>, _>>()
        })
        .collect::<Validated<Vec<_>, _>>()
        .map(|buffers| buffers.into_iter().flatten().map(|font| (font.clone(), font.info().clone())).collect())
}

// --- [INPUTS]
fn decode<O>(encoded: &[u8]) -> Result<O, TypesetError>
where O: DeserializeOwned {
    serde_path_to_error::deserialize(&mut rmp_serde::Deserializer::from_read_ref(encoded).with_human_readable()).map_err(|source| TypesetError::Deserialize { source })
}

fn library<I>(inputs: &I, features: Features) -> Result<LazyHash<Library>, TypesetError>
where I: Serialize + ?Sized {
    let mut encoded = Vec::new();
    serde_path_to_error::serialize(inputs, &mut rmp_serde::Serializer::new(&mut encoded).with_struct_map().with_human_readable()).map_err(|source| TypesetError::Serialize { source })?;
    decode(&encoded).map(|dict: Dict| LazyHash::new(Library::builder().with_inputs(dict).with_features(features).build()))
}

pub fn set_inputs<F, I>(project: &mut Project<F>, inputs: &I) -> Result<(), TypesetError>
where I: Serialize + ?Sized {
    library(inputs, project.library.features.clone()).map(|library| project.library = library)
}

// --- [COMPILE]
pub fn compile<T>(world: &dyn DiagnosticWorld, max_age: usize) -> Compiled<T>
where T: Output {
    let compiled = compiled::<T>(world);
    comemo::evict(max_age);
    compiled
}

pub fn compile_records<T, I, F, L>(project: &Project<F>, records: I, max_age: usize) -> Vec<Compiled<T>>
where
    T: Output + Send,
    I: IntoParallelIterator<Item: Serialize>,
    F: Deref<Target = FileStore<L>> + Send + Sync,
    L: FileLoader + Sync,
{
    let compiled = records
        .into_par_iter()
        .map(|record| {
            library(&record, project.library.features.clone())
                .and_then(|library| compiled::<T>(&Project { library, fonts: Arc::clone(&project.fonts), files: &*project.files, main: project.main, now: project.now.clone() }))
        })
        .collect();
    comemo::evict(max_age);
    compiled
}

fn compiled<T>(world: &dyn DiagnosticWorld) -> Compiled<T>
where T: Output {
    let span = tracing::info_span!(
        "compile",
        typst.entry = %world.name(world.main()),
        typst.target = ?T::target(),
        { TYPST_WARNINGS } = Empty,
        { OTEL_STATUS_CODE } = Empty,
        { EXCEPTION_MESSAGE } = Empty
    );
    let Warned { output, warnings } = span.in_scope(|| typst::compile::<T>(world));
    span.record(TYPST_WARNINGS, warnings.len());
    output
        .map_err(|errors| typst_error(world, &iter::chain(errors, warnings.iter().cloned()).collect::<Vec<_>>()))
        .inspect_err(|error| {
            span.record(OTEL_STATUS_CODE, "error").record(EXCEPTION_MESSAGE, field::display(Report::new(error)));
        })
        .map(|output| Warned { output, warnings })
}

pub fn selector(world: &dyn DiagnosticWorld, text: &str) -> Result<Selector, TypesetError> {
    let mut sink = Sink::new();
    (world.library().routines.eval_string)(
        (world as &dyn World).track(),
        world.library(),
        sink.track_mut(),
        EmptyIntrospector.track(),
        Context::none().track(),
        text,
        SpanMode::Uniform(syntax::Span::detached()),
        SyntaxMode::Code,
        Scope::new(),
    )
    .and_then(|value| value.cast::<LocatableSelector>().at(syntax::Span::detached()))
    .map(|LocatableSelector(selector)| selector)
    .map_err(|diagnostics| typst_error(world, &diagnostics))
}

#[must_use]
pub fn query<T, R>(output: &T, selector: &Selector) -> Validated<Vec<(Option<introspection::Location>, R)>, TypesetError>
where
    T: Output,
    R: DeserializeOwned,
{
    output
        .introspector()
        .query(selector)
        .iter()
        .enumerate()
        .map(|(index, content)| {
            let value = content.to_packed::<MetadataElem>().map_or_else(|| Value::Content(content.clone()), |metadata| metadata.value.clone());
            let decoded = try bikeshed Result<R, TypesetError> {
                let mut encoded = Vec::new();
                serde_path_to_error::serialize(&value, &mut rmp_serde::Serializer::new(&mut encoded)).map_err(|source| TypesetError::Serialize { source })?;
                decode(&encoded)?
            };
            decoded.map(|element| (content.location(), element)).map_err(|source| TypesetError::Element { index, source: Box::new(source) })
        })
        .collect()
}

// --- [DIAGNOSTICS]
#[must_use]
pub fn report(world: &dyn DiagnosticWorld, diagnostics: &[SourceDiagnostic], format: DiagnosticFormat) -> String {
    let detached = |diagnostic: &SourceDiagnostic| SourceDiagnostic {
        span: DiagSpan::detached(),
        trace: EcoVec::new(),
        hints: diagnostic.hints.iter().map(|hint| Spanned::new(hint.v.clone(), DiagSpan::detached())).collect(),
        ..diagnostic.clone()
    };
    let emitted = |diagnostic: &SourceDiagnostic| {
        let mut report = NoColor::new(Vec::new());
        emit(&mut report, world, iter::once(diagnostic), format)
            .inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), diagnostic = %diagnostic.message, "diagnostic location refused"))
            .ok()
            .map(|()| report.into_inner())
    };
    String::from_utf8_lossy_owned(diagnostics.iter().filter_map(|diagnostic| emitted(diagnostic).or_else(|| emitted(&detached(diagnostic)))).flatten().collect())
}

#[must_use]
pub fn diagnostics(world: &dyn DiagnosticWorld, diagnostics: &[SourceDiagnostic]) -> Vec<Diagnostic> {
    let location = |span: DiagSpan| {
        let id = span.id()?;
        let (source, range) =
            world.source(id).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), file = %world.name(id), "diagnostic source refused")).ok().zip(world.range(span))?;
        let (line, column) = source.lines().byte_to_line_column(range.start)?;
        let (end_line, end_column) = source.lines().byte_to_line_column(range.end)?;
        Some(Location { file: world.name(id).into(), line: line.strict_add(1), column: column.strict_add(1), end_line: end_line.strict_add(1), end_column: end_column.strict_add(1) })
    };
    diagnostics
        .iter()
        .map(|diagnostic| Diagnostic {
            severity: match diagnostic.severity {
                Severity::Error => "error",
                Severity::Warning => "warning",
            },
            location: location(diagnostic.span),
            message: diagnostic.message.clone(),
            hints: diagnostic.hints.iter().map(|hint| (hint.v.clone(), location(hint.span))).collect(),
            trace: diagnostic.trace.iter().filter_map(|point| location(point.span.into()).map(|location| (point.v.to_string(), location))).collect(),
        })
        .collect()
}

fn typst_error(world: &dyn DiagnosticWorld, diagnostics: &[SourceDiagnostic]) -> TypesetError {
    TypesetError::Typst { diagnostics: crate::diagnostics(world, diagnostics), report: report(world, diagnostics, DiagnosticFormat::Human) }
}

// --- [PACKAGES]
#[must_use]
pub fn package_closure<L>(files: &mut FileStore<L>) -> HashSet<PackageSpec>
where L: FileLoader {
    files
        .dependencies()
        .1
        .filter_map(|id| match id.root() {
            VirtualRoot::Package(spec) => Some(spec.clone()),
            VirtualRoot::Project => None,
        })
        .collect()
}

pub fn template(packages: &SystemPackages, spec: &PackageSpec) -> Result<(HashMap<FileId, Bytes>, FileId), TypesetError> {
    fn other(error: impl fmt::Display) -> FileError { FileError::Other(Some(eco_format!("{error}"))) }
    fn files(directory: &Dir, path: &Path) -> io::Result<Vec<(PathBuf, Bytes)>> {
        directory
            .entries()?
            .map(|entry| {
                let entry = entry?;
                let path = path.join(entry.file_name());
                if entry.file_type()?.is_dir() { files(&entry.open_dir()?, &path) } else { directory.read(entry.file_name()).map(|bytes| vec![(path, Bytes::new(bytes))]) }
            })
            .flatten_ok()
            .collect()
    }
    let project = |path| RootedPath::new(VirtualRoot::Project, path).intern();
    let instantiated = try bikeshed FileResult<_> {
        let root = packages.obtain(spec)?;
        let manifest = toml::from_slice::<PackageManifest>(&read(&root, &VirtualPath::new("typst.toml").map_err(other)?)?).map_err(other)?;
        manifest.validate(spec).map_err(|message| PackageError::Other(Some(message)))?;
        let template = manifest.template.ok_or_else(|| PackageError::Other(Some(eco_format!("{spec} holds no template"))))?;
        let path = VirtualPath::new(&template.path).map_err(other)?;
        let real = root.resolve(&path)?;
        let entries = directory(&root)?
            .open_dir(path.get_without_slash())
            .and_then(|walked| files(&walked, &real))
            .map_err(|error| FileError::from_io(error, &real))?
            .into_iter()
            .map(|(file, bytes)| Ok((project(VirtualPath::virtualize(&real, &file).map_err(other)?), bytes)))
            .collect::<FileResult<_>>()?;
        (entries, project(VirtualPath::new(&template.entrypoint).map_err(other)?))
    };
    instantiated.map_err(|source| TypesetError::Template { spec: spec.clone(), source })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod diagrams;
pub mod export;
