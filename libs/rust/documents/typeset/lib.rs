#![feature(error_reporter, float_conversions, trait_alias, try_with_capacity)]

use core::{any::Any, error::Error as _, fmt, iter, num::NonZero};
use std::{
    collections::HashMap,
    error::Report,
    io::{self, Read},
    path::{Path, PathBuf},
    sync::Arc,
};

use jiff::{SignedDuration, Zoned, civil::Date, tz::TimeZone};
use merman::{RenderError, svg::export::NativeFontError};
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, HTTP_REQUEST_METHOD, HTTP_RESPONSE_STATUS_CODE, OTEL_STATUS_CODE, URL_FULL};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, ParallelIterator as _};
use reqwest::{
    Method, StatusCode,
    blocking::{Client, Response},
};
use serde::Serialize;
use serde_json::value::Serializer;
use tracing::{
    Span,
    field::{self, Empty},
};
use typst::{
    Features, Library, LibraryExt as _, World, comemo,
    diag::{FileError, FileResult, PackageError, SourceDiagnostic, Warned},
    ecow::EcoVec,
    foundations::{Bytes, Datetime, Dict, Duration, Output},
    syntax::{FileId, RootedPath, Source, VirtualPath, VirtualRoot},
    text::{Font, FontBook, FontInfo},
    utils::LazyHash,
};
use typst_kit::{
    diagnostics::{DiagnosticFormat, DiagnosticWorld, emit, termcolor::NoColor},
    downloader::Downloader,
    files::{FileLoader, FileStore, FsRoot},
    fonts::{FontSource, FontStore},
    packages::SystemPackages,
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Loader = FileLoader + Send + Sync;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum TypesetError {
    #[error("sys.inputs record refused")]
    Inputs { source: serde_path_to_error::Error<serde_json::Error> },
    #[error("project root refused")]
    Root { source: io::Error },
    #[error("font buffer {index} holds no face or claims more faces than it holds")]
    Font { index: usize },
    #[error("{report}")]
    Typst { diagnostics: EcoVec<SourceDiagnostic>, report: String },
    #[error("{}", fmt::from_fn(|formatter| {
        write!(formatter, "{} page {page} rasterizes to {width} by {height} pixels past the ", path.get_without_slash())?;
        match budget {
            Some(budget) => write!(formatter, "{budget} pixel budget"),
            None => formatter.write_str("pixmap size limit"),
        }
    }))]
    Raster { path: VirtualPath, page: NonZero<usize>, width: u32, height: u32, budget: Option<NonZero<u64>> },
    #[error("page {page} PNG encoding failed")]
    Encode { page: NonZero<usize>, source: png::EncodingError },
    #[error(transparent)]
    NativeFont(NativeFontError),
    #[error("diagram {} refused", path.get_without_slash())]
    Diagram { path: VirtualPath, source: RenderError },
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct HttpDownloader(pub Client);

impl Downloader for HttpDownloader {
    fn stream(&self, _key: &dyn Any, url: &str) -> io::Result<(Option<usize>, Box<dyn Read>)> {
        request(&self.0, url).map(|(size, body)| (size, Box::new(body) as Box<dyn Read>))
    }

    fn download(&self, _key: &dyn Any, url: &str) -> io::Result<Vec<u8>> {
        let (size, mut body) = request(&self.0, url)?;
        size.map_or_else(|| Ok(Vec::new()), Vec::try_with_capacity)
            .map_err(|error| failed(&body.request, io::Error::new(io::ErrorKind::OutOfMemory, error)))
            .and_then(|mut buffer| body.read_to_end(&mut buffer).map(|_| buffer))
    }
}

struct Body {
    request: Span,
    response: Response,
}

impl Read for Body {
    fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
        self.request.in_scope(|| self.response.read(buf)).map_err(|error| failed(&self.request, error))
    }
}

#[derive(Debug, Default, derive_more::AsRef, derive_more::AsMut)]
pub struct Files {
    root: Option<FsRoot>,
    #[as_ref]
    #[as_mut]
    overlay: HashMap<FileId, Bytes>,
    packages: Option<SystemPackages>,
}

impl Files {
    pub fn new(root: Option<&Path>, overlay: HashMap<FileId, Bytes>, packages: Option<SystemPackages>) -> Result<Self, TypesetError> {
        root.map(fs_err::canonicalize)
            .transpose()
            .map(|root| Self { root: root.map(FsRoot::new), overlay, packages })
            .map_err(|source| TypesetError::Root { source })
    }
}

impl FileLoader for Files {
    fn load(&self, id: FileId) -> FileResult<Bytes> {
        let confined = |root: &FsRoot| {
            let path = root.resolve(id.vpath())?;
            let failed = |error: io::Error| FileError::from_io(error, &path);
            let real = fs_err::canonicalize(&path).map_err(failed)?;
            real.starts_with(root.path()).ok_or(FileError::AccessDenied)?;
            (!fs_err::metadata(&real).map_err(failed)?.is_dir()).ok_or(FileError::IsDirectory)?;
            fs_err::read(&real).map(Bytes::new).map_err(failed)
        };
        match (self.overlay.get(&id), id.root(), &self.root, &self.packages) {
            (Some(bytes), _, _, _) => Ok(bytes.clone()),
            (None, VirtualRoot::Project, Some(root), _) => confined(root),
            (None, VirtualRoot::Project, None, _) => Err(FileError::NotFound(PathBuf::from(id.vpath().get_without_slash()))),
            (None, VirtualRoot::Package(spec), _, Some(packages)) => {
                let root = packages.obtain(spec)?;
                confined(&FsRoot::new(fs_err::canonicalize(root.path()).map_err(|error| FileError::from_io(error, root.path()))?))
            }
            (None, VirtualRoot::Package(spec), _, None) => Err(FileError::Package(PackageError::NotFound(spec.clone()))),
        }
    }
}

#[derive(derive_more::Debug, derive_more::AsRef, derive_more::AsMut)]
pub struct Project<L = Files> {
    #[as_mut]
    library: LazyHash<Library>,
    #[debug(skip)]
    #[as_ref(FontStore)]
    fonts: Arc<FontStore>,
    #[debug(skip)]
    #[as_ref]
    #[as_mut]
    files: FileStore<L>,
    #[as_mut]
    main: FileId,
    #[as_ref]
    #[as_mut]
    now: Zoned,
}

impl<L> Project<L>
where L: Loader
{
    pub fn new<I>(inputs: &I, features: Features, fonts: Arc<FontStore>, files: L, main: VirtualPath, now: Zoned) -> Result<Self, TypesetError>
    where I: Serialize + ?Sized {
        serde_path_to_error::serialize(inputs, Serializer)
            .and_then(serde_path_to_error::deserialize::<_, Dict>)
            .map(|dict| Self {
                library: LazyHash::new(Library::builder().with_inputs(dict).with_features(features).build()),
                fonts,
                files: FileStore::new(files),
                main: RootedPath::new(VirtualRoot::Project, main).intern(),
                now,
            })
            .map_err(|source| TypesetError::Inputs { source })
    }
}

impl<L> World for Project<L>
where L: Loader
{
    fn library(&self) -> &LazyHash<Library> { &self.library }

    fn book(&self) -> &LazyHash<FontBook> { self.fonts.book() }

    fn main(&self) -> FileId { self.main }

    fn source(&self, id: FileId) -> FileResult<Source> { self.files.source(id) }

    fn file(&self, id: FileId) -> FileResult<Bytes> { self.files.file(id) }

    fn font(&self, index: usize) -> Option<Font> { self.fonts.font(index) }

    fn today(&self, offset: Option<Duration>) -> Option<Datetime> {
        let calendar = |date: Date| Datetime::from_ymd(i32::from(date.year()), date.month().cast_unsigned(), date.day().cast_unsigned());
        match offset.map(|shift| SignedDuration::try_from_secs_f64(shift.seconds()).and_then(|span| self.now.timestamp().checked_add(span))) {
            None => calendar(self.now.date()),
            Some(Ok(instant)) => calendar(instant.to_zoned(TimeZone::UTC).date()),
            Some(Err(error)) => {
                tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "today offset refused");
                None
            }
        }
    }
}

impl<L> DiagnosticWorld for Project<L>
where L: Loader
{
    fn name(&self, id: FileId) -> String {
        match id.root() {
            VirtualRoot::Project => id.vpath().get_without_slash().to_owned(),
            VirtualRoot::Package(spec) => format!("{spec}{}", id.vpath().get_with_slash()),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn fonts<B, E, T>(buffers: B, entries: E) -> Validated<FontStore, TypesetError>
where
    B: IntoParallelIterator<Item = Bytes>,
    B::Iter: IndexedParallelIterator,
    E: IntoIterator<Item = (T, FontInfo)>,
    T: FontSource,
{
    buffers
        .into_par_iter()
        .enumerate()
        .map(|(index, data)| {
            let room = data.len().saturating_sub(size_of::<[u32; 3]>()) / const { NonZero::new(size_of::<u32>()).unwrap() };
            let refused = || TypesetError::Font { index };
            ttf_parser::fonts_in_collection(&data).is_none_or(|count| usize::try_from(count).is_ok_and(|count| count <= room)).ok_or_else(refused)?;
            NEVec::try_from_vec(Font::iter(data).collect()).ok_or_else(refused)
        })
        .collect::<Validated<Vec<_>, _>>()
        .map(|buffers| {
            let mut store = FontStore::new();
            store.extend(buffers.into_iter().flatten().map(|font| (font.clone(), font.info().clone())));
            store.extend(entries);
            store
        })
}

pub fn compile<T>(world: &dyn DiagnosticWorld, max_age: usize) -> Result<Warned<T>, TypesetError>
where T: Output {
    let Warned { output, warnings } = typst::compile::<T>(world);
    comemo::evict(max_age);
    output.map_err(|errors| located(world, iter::chain(errors, warnings.iter().cloned()).collect())).map(|output| Warned { output, warnings })
}

fn request(client: &Client, url: &str) -> io::Result<(Option<usize>, Body)> {
    let request = tracing::info_span!(
        "request",
        otel.name = Method::GET.as_str(),
        otel.kind = "client",
        { OTEL_STATUS_CODE } = Empty,
        { HTTP_REQUEST_METHOD } = Method::GET.as_str(),
        { URL_FULL } = url,
        { HTTP_RESPONSE_STATUS_CODE } = Empty,
        { ERROR_TYPE } = Empty
    );
    let response = request
        .in_scope(|| client.get(url).send())
        .inspect(|response| {
            request.record(HTTP_RESPONSE_STATUS_CODE, response.status().as_u16());
        })
        .and_then(Response::error_for_status)
        .map_err(|error| failed(&request, io::Error::other(error)))?;
    response
        .content_length()
        .map(usize::try_from)
        .transpose()
        .map_err(|error| failed(&request, io::Error::new(io::ErrorKind::FileTooLarge, error)))
        .map(|size| (size, Body { request, response }))
}

fn failed(request: &Span, error: io::Error) -> io::Error {
    let (error, status) = match error.downcast::<reqwest::Error>() {
        Ok(error) => {
            let status = error.status();
            let kind = match status {
                Some(StatusCode::NOT_FOUND) => io::ErrorKind::NotFound,
                Some(_) => io::ErrorKind::Other,
                None if error.is_timeout() => io::ErrorKind::TimedOut,
                None => iter::successors(error.source(), |&cause| cause.source()).find_map(|cause| cause.downcast_ref::<io::Error>()).map_or(io::ErrorKind::Other, io::Error::kind),
            };
            (io::Error::new(kind, error), status)
        }
        Err(error) => (error, None),
    };
    match status {
        Some(status) => request.record(ERROR_TYPE, status.as_str()),
        None => request.record(ERROR_TYPE, field::debug(error.kind())),
    }
    .record(OTEL_STATUS_CODE, "error");
    error
}

fn located(world: &dyn DiagnosticWorld, diagnostics: EcoVec<SourceDiagnostic>) -> TypesetError {
    let mut report = NoColor::new(Vec::new());
    diagnostics.iter().for_each(|diagnostic| {
        if let Err(error) = emit(&mut report, world, iter::once(diagnostic), DiagnosticFormat::Human) {
            tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), diagnostic = %diagnostic.message, "diagnostic location refused");
        }
    });
    TypesetError::Typst { report: String::from_utf8_lossy_owned(report.into_inner()), diagnostics }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod diagrams;
pub mod export;
