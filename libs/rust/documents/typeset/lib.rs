#![feature(error_reporter, float_conversions)]

use core::{any::Any, fmt, iter};
use std::{collections::HashMap, error::Report, io, path::PathBuf, sync::Arc};

use jiff::{SignedDuration, Zoned, civil::Date, tz::TimeZone};
use merman::RenderError;
use opentelemetry_semantic_conventions::attribute::{EXCEPTION_MESSAGE, HTTP_REQUEST_METHOD, HTTP_RESPONSE_STATUS_CODE, URL_FULL};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use reqwest::{
    Method, StatusCode,
    blocking::{Client, Response},
};
use serde::Serialize;
use serde_json::value::Serializer;
use tracing::{Span, field::Empty};
use typst::{
    Features, Library, LibraryExt as _, World, comemo,
    diag::{FileError, FileResult, PackageError, SourceDiagnostic, Warned},
    ecow::EcoVec,
    foundations::{Bytes, Datetime, Dict, Duration, Output},
    syntax::{FileId, Source, VirtualPath, VirtualRoot},
    text::{Font, FontBook},
    utils::LazyHash,
};
use typst_kit::{
    diagnostics::DiagnosticWorld,
    downloader::Downloader,
    files::{FileLoader, FileStore, FsRoot},
    fonts::{FontStore, embedded},
    packages::SystemPackages,
};
use validated::Validated;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum TypesetError {
    #[error("sys.inputs record refused")]
    Inputs { source: serde_path_to_error::Error<serde_json::Error> },
    #[error("font buffer {index} holds no face")]
    Font { index: usize },
    #[error("typst diagnostics{}", fmt::from_fn(|formatter| .0.iter().try_for_each(|diagnostic| write!(formatter, "\n{}", diagnostic.message))))]
    Typst(EcoVec<SourceDiagnostic>),
    #[error("page {page} rasterizes to {width} by {height} pixels past the pixmap bounds")]
    Raster { page: usize, width: u32, height: u32 },
    #[error("page {page} PNG encoding failed")]
    Encode { page: usize, source: png::EncodingError },
    #[error(transparent)]
    Diagram(#[from] RenderError),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct HttpDownloader(pub Client);

impl Downloader for HttpDownloader {
    fn stream(&self, _key: &dyn Any, url: &str) -> io::Result<(Option<usize>, Box<dyn io::Read>)> {
        let response = self
            .0
            .request(Method::GET, url)
            .send()
            .inspect(|response| {
                Span::current().record(HTTP_RESPONSE_STATUS_CODE, response.status().as_u16());
            })
            .and_then(Response::error_for_status)
            .map_err(|error| match error.status() {
                Some(StatusCode::NOT_FOUND) => io::Error::new(io::ErrorKind::NotFound, error),
                Some(_) | None => io::Error::other(error),
            })?;
        let size = response.content_length().map(usize::try_from).transpose().map_err(|error| io::Error::new(io::ErrorKind::FileTooLarge, error))?;
        Ok((size, Box::new(response)))
    }

    fn download(&self, key: &dyn Any, url: &str) -> io::Result<Vec<u8>> {
        let _request = tracing::info_span!(
            "request",
            otel.name = Method::GET.as_str(),
            otel.kind = "client",
            { HTTP_REQUEST_METHOD } = Method::GET.as_str(),
            { URL_FULL } = url,
            { HTTP_RESPONSE_STATUS_CODE } = Empty
        )
        .entered();
        let (size, mut reader) = self.stream(key, url)?;
        let mut body = Vec::new();
        size.map_or(Ok(()), |size| body.try_reserve_exact(size)).map_err(|error| io::Error::new(io::ErrorKind::OutOfMemory, error))?;
        reader.read_to_end(&mut body).map(|_| body)
    }
}

#[derive(Debug)]
pub struct Sources {
    pub root: Option<FsRoot>,
    pub overlay: HashMap<VirtualPath, Bytes>,
    pub packages: Option<SystemPackages>,
}

impl FileLoader for Sources {
    fn load(&self, id: FileId) -> FileResult<Bytes> {
        match (id.root(), self.overlay.get(id.vpath()), &self.root, &self.packages) {
            (VirtualRoot::Project, Some(bytes), _, _) => Ok(bytes.clone()),
            (VirtualRoot::Project, None, Some(root), _) => root.load(id.vpath()),
            (VirtualRoot::Project, None, None, _) => Err(FileError::NotFound(PathBuf::from(id.vpath().get_without_slash()))),
            (VirtualRoot::Package(spec), _, _, Some(packages)) => packages.obtain(spec)?.load(id.vpath()),
            (VirtualRoot::Package(spec), _, _, None) => Err(FileError::Package(PackageError::NotFound(spec.clone()))),
        }
    }
}

#[derive(derive_more::Debug)]
pub struct Project<L = Sources> {
    pub library: LazyHash<Library>,
    #[debug(skip)]
    pub fonts: Arc<FontStore>,
    #[debug(skip)]
    pub files: FileStore<L>,
    pub main: FileId,
    pub now: Zoned,
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

pub fn library<I>(inputs: &I, features: Features) -> Result<LazyHash<Library>, TypesetError>
where I: Serialize + ?Sized {
    serde_path_to_error::serialize(inputs, Serializer)
        .and_then(serde_path_to_error::deserialize::<_, Dict>)
        .map(|dict| LazyHash::new(Library::builder().with_inputs(dict).with_features(features).build()))
        .map_err(|source| TypesetError::Inputs { source })
}

#[must_use]
pub fn fonts<B>(buffers: B) -> Validated<FontStore, TypesetError>
where B: IntoIterator<Item = Bytes> {
    buffers
        .into_iter()
        .enumerate()
        .collect::<Vec<_>>()
        .into_par_iter()
        .map(|(index, data)| Some(Font::iter(data).collect::<Vec<_>>()).filter(|faces| !faces.is_empty()).ok_or(TypesetError::Font { index }))
        .collect::<Validated<Vec<_>, _>>()
        .map(|buffers| {
            let mut store = FontStore::new();
            store.extend(buffers.into_iter().flatten().map(|font| (font.clone(), font.info().clone())).chain(embedded()));
            store
        })
}

pub fn compile<T>(world: &dyn World, max_age: usize) -> Result<Warned<T>, TypesetError>
where T: Output {
    let Warned { output, warnings } = typst::compile::<T>(world);
    comemo::evict(max_age);
    Ok(Warned { output: output.map_err(|errors| TypesetError::Typst(iter::chain(errors, warnings.iter().cloned()).collect()))?, warnings })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod diagrams;
pub mod export;
