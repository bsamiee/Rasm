#![feature(error_reporter, float_conversions, try_with_capacity)]

use core::{any::Any, error::Error as _, fmt, iter, num::NonZero};
use std::{
    collections::HashMap,
    error::Report,
    io::{self, Read as _},
    path::PathBuf,
    sync::Arc,
};

use jiff::{SignedDuration, Zoned, civil::Date, tz::TimeZone};
use merman::{RenderError, svg::export::NativeFontError};
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::{ERROR_TYPE, EXCEPTION_MESSAGE, HTTP_REQUEST_METHOD, HTTP_RESPONSE_STATUS_CODE, URL_FULL};
use rayon::iter::{IndexedParallelIterator, IntoParallelIterator, ParallelIterator as _};
use reqwest::{
    Method, StatusCode,
    blocking::{Client, Response},
};
use serde::Serialize;
use serde_json::value::Serializer;
use tracing::field::{self, Empty};
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
    fonts::FontStore,
    packages::SystemPackages,
};
use validated::Validated;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum TypesetError {
    #[error("sys.inputs record refused")]
    Inputs { source: serde_path_to_error::Error<serde_json::Error> },
    #[error("font buffer {index} holds no face or claims more faces than it holds")]
    Font { index: usize },
    #[error("{report}")]
    Typst { diagnostics: EcoVec<SourceDiagnostic>, report: String },
    #[error("{} page {page} rasterizes to {width} by {height} pixels past the {budget} pixel budget or the pixmap bounds", path.get_without_slash())]
    Raster { path: VirtualPath, page: NonZero<usize>, width: u32, height: u32, budget: NonZero<u64> },
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
    fn stream(&self, key: &dyn Any, url: &str) -> io::Result<(Option<usize>, Box<dyn io::Read>)> {
        self.download(key, url).map(|body| (Some(body.len()), Box::new(io::Cursor::new(body)) as Box<dyn io::Read>))
    }

    fn download(&self, _key: &dyn Any, url: &str) -> io::Result<Vec<u8>> {
        let request = tracing::info_span!(
            "request",
            otel.name = Method::GET.as_str(),
            otel.kind = "client",
            otel.status_code = Empty,
            { HTTP_REQUEST_METHOD } = Method::GET.as_str(),
            { URL_FULL } = url,
            { HTTP_RESPONSE_STATUS_CODE } = Empty,
            { ERROR_TYPE } = Empty
        )
        .entered();
        let failed = |error: io::Error, kind: &dyn fmt::Debug| {
            request.record(ERROR_TYPE, field::debug(kind)).record("otel.status_code", "error");
            error
        };
        let mut response = self
            .0
            .get(url)
            .send()
            .inspect(|response| {
                request.record(HTTP_RESPONSE_STATUS_CODE, response.status().as_u16());
            })
            .and_then(Response::error_for_status)
            .map_err(|error| match error.status() {
                Some(status @ StatusCode::NOT_FOUND) => failed(io::Error::new(io::ErrorKind::NotFound, error), &status),
                Some(status) => failed(io::Error::other(error), &status),
                None => {
                    let kind = iter::successors(error.source(), |&cause| cause.source()).find_map(|cause| cause.downcast_ref::<io::Error>()).map_or(io::ErrorKind::Other, io::Error::kind);
                    failed(io::Error::new(kind, error), &kind)
                }
            })?;
        response
            .content_length()
            .map(usize::try_from)
            .transpose()
            .map_err(|error| io::Error::new(io::ErrorKind::FileTooLarge, error))
            .and_then(|size| size.map_or_else(|| Ok(Vec::new()), Vec::try_with_capacity).map_err(|error| io::Error::new(io::ErrorKind::OutOfMemory, error)))
            .and_then(|mut body| response.read_to_end(&mut body).map(|_| body))
            .map_err(|error| {
                let kind = error.kind();
                failed(error, &kind)
            })
    }
}

#[derive(Debug, Default)]
pub struct Files {
    pub root: Option<FsRoot>,
    pub overlay: HashMap<FileId, Bytes>,
    pub packages: Option<SystemPackages>,
}

impl FileLoader for Files {
    fn load(&self, id: FileId) -> FileResult<Bytes> {
        let confined = |root: &FsRoot| {
            let path = root.resolve(id.vpath())?;
            let real = fs_err::canonicalize(&path).map_err(|error| FileError::from_io(error, &path))?;
            real.starts_with(fs_err::canonicalize(root.path()).map_err(|error| FileError::from_io(error, root.path()))?).ok_or(FileError::AccessDenied)?;
            (!fs_err::metadata(&real).map_err(|error| FileError::from_io(error, &path))?.is_dir()).ok_or(FileError::IsDirectory)?;
            fs_err::read(&real).map(Bytes::new).map_err(|error| FileError::from_io(error, &path))
        };
        match (self.overlay.get(&id), id.root(), &self.root, &self.packages) {
            (Some(bytes), _, _, _) => Ok(bytes.clone()),
            (None, VirtualRoot::Project, Some(root), _) => confined(root),
            (None, VirtualRoot::Project, None, _) => Err(FileError::NotFound(PathBuf::from(id.vpath().get_without_slash()))),
            (None, VirtualRoot::Package(spec), _, Some(packages)) => confined(&packages.obtain(spec)?),
            (None, VirtualRoot::Package(spec), _, None) => Err(FileError::Package(PackageError::NotFound(spec.clone()))),
        }
    }
}

#[derive(derive_more::Debug)]
pub struct Project<L = Files> {
    pub library: LazyHash<Library>,
    #[debug(skip)]
    pub fonts: Arc<FontStore>,
    #[debug(skip)]
    pub files: FileStore<L>,
    pub main: FileId,
    pub now: Zoned,
}

impl<L> Project<L>
where L: FileLoader
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
where L: FileLoader + Send + Sync
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
where L: FileLoader + Send + Sync
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
pub fn fonts<B, E>(buffers: B, embedded: E) -> Validated<FontStore, TypesetError>
where
    B: IntoParallelIterator<Item = Bytes>,
    B::Iter: IndexedParallelIterator,
    E: IntoIterator<Item = (Font, FontInfo)>,
{
    buffers
        .into_par_iter()
        .enumerate()
        .map(|(index, data)| {
            let room = data.len().saturating_sub(size_of::<[u32; 3]>()) / const { NonZero::new(size_of::<u32>()).unwrap() };
            ttf_parser::fonts_in_collection(&data)
                .map_or(Some(1), |count| usize::try_from(count).is_ok_and(|faces| faces <= room).then_some(count))
                .and_then(|count| NEVec::try_from_vec((0..count).filter_map(|face| Font::new(data.clone(), face)).collect()))
                .ok_or(TypesetError::Font { index })
        })
        .collect::<Validated<Vec<_>, _>>()
        .map(|buffers| {
            let mut store = FontStore::new();
            store.extend(buffers.into_iter().flatten().map(|font| (font.clone(), font.info().clone())).chain(embedded));
            store
        })
}

pub fn compile<T>(world: &dyn DiagnosticWorld, max_age: usize) -> Result<Warned<T>, TypesetError>
where T: Output {
    let Warned { output, warnings } = typst::compile::<T>(world);
    comemo::evict(max_age);
    match output {
        Ok(output) => Ok(Warned { output, warnings }),
        Err(errors) => Err(located(world, iter::chain(errors, warnings).collect())),
    }
}

fn located(world: &dyn DiagnosticWorld, diagnostics: EcoVec<SourceDiagnostic>) -> TypesetError {
    let mut report = NoColor::new(Vec::new());
    if let Err(error) = emit(&mut report, world, &diagnostics, DiagnosticFormat::Human) {
        tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "diagnostic location refused");
    }
    TypesetError::Typst { report: String::from_utf8_lossy(&report.into_inner()).into_owned(), diagnostics }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod diagrams;
pub mod export;
