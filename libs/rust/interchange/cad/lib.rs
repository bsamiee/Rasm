#![feature(const_result_trait_fn, const_trait_impl, default_field_values, deref_patterns, float_conversions, iterator_try_collect)]

use std::{
    fmt,
    io::{Read, Seek, Write},
    sync::Arc,
};

use acadrust::{AuditIssue, AuditLocation, AuditSeverity, CadDocument, DwgReadOptions, DwgReader, DwgWriter, DxfError, DxfOmission, DxfReader, DxfReaderConfiguration, DxfWriter, Handle, ReadOutcome};
use derive_where::derive_where;
use nonempty_collections::NEVec;
use rasm_linework::LineworkError;
use uom::si::{
    f64::Length,
    length::{angstrom, astronomical_unit, centimeter, decameter, decimeter, foot, gigameter, hectometer, inch, kilometer, light_year, meter, microinch, micron, mil, mile, millimeter, nanometer, parsec, yard},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::FromRepr)]
#[repr(i16)]
pub enum Unit {
    Inches = 1,
    Feet,
    Miles,
    Millimeters,
    Centimeters,
    Meters,
    Kilometers,
    Microinches,
    Mils,
    Yards,
    Angstroms,
    Nanometers,
    Microns,
    Decimeters,
    Dekameters,
    Hectometers,
    Gigameters,
    AstronomicalUnits,
    LightYears,
    Parsecs,
    UsSurveyFeet,
    UsSurveyInches,
    UsSurveyYards,
    UsSurveyMiles,
}

impl From<Unit> for Length {
    fn from(value: Unit) -> Self {
        match value {
            Unit::Inches => Self::new::<inch>(1.0),
            Unit::Feet => Self::new::<foot>(1.0),
            Unit::Miles => Self::new::<mile>(1.0),
            Unit::Millimeters => Self::new::<millimeter>(1.0),
            Unit::Centimeters => Self::new::<centimeter>(1.0),
            Unit::Meters => Self::new::<meter>(1.0),
            Unit::Kilometers => Self::new::<kilometer>(1.0),
            Unit::Microinches => Self::new::<microinch>(1.0),
            Unit::Mils => Self::new::<mil>(1.0),
            Unit::Yards => Self::new::<yard>(1.0),
            Unit::Angstroms => Self::new::<angstrom>(1.0),
            Unit::Nanometers => Self::new::<nanometer>(1.0),
            Unit::Microns => Self::new::<micron>(1.0),
            Unit::Decimeters => Self::new::<decimeter>(1.0),
            Unit::Dekameters => Self::new::<decameter>(1.0),
            Unit::Hectometers => Self::new::<hectometer>(1.0),
            Unit::Gigameters => Self::new::<gigameter>(1.0),
            Unit::AstronomicalUnits => Self::new::<astronomical_unit>(1.0),
            Unit::LightYears => Self::new::<light_year>(1.0),
            Unit::Parsecs => Self::new::<parsec>(1.0),
            Unit::UsSurveyFeet => Self::new::<meter>(1200.0 / 3937.0),
            Unit::UsSurveyInches => Self::new::<meter>(100.0 / 3937.0),
            Unit::UsSurveyYards => Self::new::<meter>(3600.0 / 3937.0),
            Unit::UsSurveyMiles => Self::new::<meter>(6_336_000.0 / 3937.0),
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Format {
    #[display("DXF")]
    Dxf,
    #[display("binary DXF")]
    BinaryDxf,
    #[display("DWG")]
    Dwg,
}

#[derive(Debug, Clone, PartialEq, Eq, derive_more::Display)]
pub enum Operation {
    #[display("read the drawing")]
    Read,
    #[display("write {_0}")]
    Write(Format),
    #[display("author layout {_0}")]
    Layout(Arc<str>),
    #[display("add table entry {_0}")]
    Table(String),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Refusal {
    #[display("has no linework form")]
    Unsupported,
    #[display("holds fit points alone")]
    Fitted,
    #[display("inserts itself")]
    Cycle,
    #[display("has no text height")]
    Height,
    #[display("has a shape or text dash")]
    Linetype,
    #[display("fills a gradient")]
    Gradient,
    #[display("looks along no plan view")]
    View,
    #[display("measures paper in pixels")]
    Units,
    #[display("prints a device color")]
    Ink,
    #[display("places off a similarity")]
    Skew,
    #[display("places external content")]
    Content,
    #[display("sits under a clip DXF cannot hold")]
    Clipped,
    #[display("crops by several contours")]
    Boundary,
}

#[derive(Debug, Clone, PartialEq, Eq, derive_more::Display)]
pub enum Subject {
    #[display("{kind} {handle}")]
    Record { handle: Handle, kind: &'static str },
    #[display("element {index} on layer {layer} in {space}")]
    Element { space: Arc<str>, layer: Arc<str>, index: usize },
    #[display("viewport {index} on {layout}")]
    Viewport { layout: Arc<str>, index: usize },
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone)]
#[derive_where(Debug)]
pub struct Reading {
    pub options: DwgReadOptions,
    pub default_encoding: Option<String> = None,
    #[derive_where(skip(Debug))]
    pub progress: Option<Arc<dyn Fn(u16) + Send + Sync>> = None,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum CadError {
    #[error("{operation}")]
    Drawing { operation: Operation, source: DxfError },
    #[error("drawing unit code {code} names no length")]
    Unit { code: i16 },
    #[error("audit finds structural errors{}", fmt::from_fn(|formatter| .issues.iter().try_for_each(|AuditIssue { code: _, severity: _, location, handle: _, message, repairable: _ }| match location {
        AuditLocation::Document => write!(formatter, "\ndocument: {message}"),
        AuditLocation::TableEntry { table, name, handle } => write!(formatter, "\n{table} entry {name} {handle}: {message}"),
        AuditLocation::Entity { handle } => write!(formatter, "\nentity {handle}: {message}"),
        AuditLocation::Object { handle } => write!(formatter, "\nobject {handle}: {message}"),
        AuditLocation::Reference { owner, target } => write!(formatter, "\nreference {owner} to {target}: {message}"),
    })))]
    Audit { issues: NEVec<AuditIssue> },
    #[error("a DXF save drops architecture records{}", fmt::from_fn(|formatter| .records.iter().try_for_each(|record| write!(formatter, "\n{} {}: {}", record.name, record.handle, record.reason))))]
    Omitted { records: NEVec<DxfOmission> },
    #[error("{subject} {reason}")]
    Refused { subject: Subject, reason: Refusal },
    #[error("{subject}")]
    Geometry { subject: Subject, source: LineworkError },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn read<R>(mut stream: R, reading: Reading) -> Result<ReadOutcome, CadError>
where R: Read + Seek + 'static {
    let Reading { options, default_encoding, progress } = reading;
    let drawing = |source| CadError::Drawing { operation: Operation::Read, source };
    let mut signature = [0_u8; 2];
    stream.read_exact(&mut signature).and_then(|()| stream.rewind()).map_err(DxfError::from).map_err(drawing)?;
    match signature {
        [b'A', b'C'] => default_encoding.into_iter().try_fold(DwgReader::from_stream_with_options(stream, options), DwgReader::with_r12_code_page).and_then(|mut reader| {
            if let Some(progress) = progress {
                reader.set_progress_callback(progress);
            }
            reader.read_with_stats()
        }),
        _ => DxfReader::from_reader(stream).and_then(|reader| reader.with_configuration(DxfReaderConfiguration { failsafe: options.failsafe, default_encoding }).read_with_stats()),
    }
    .map_err(drawing)
}

pub fn write<W>(document: &CadDocument, format: Format, sink: W) -> Result<(), CadError>
where W: Write + Seek {
    NEVec::try_from_vec(document.audit().issues.into_iter().filter(|issue| matches!(issue.severity, AuditSeverity::Error | AuditSeverity::Critical)).collect()).map_or(Ok(()), |issues| Err(CadError::Audit { issues }))?;
    let drawing = |source| CadError::Drawing { operation: Operation::Write(format), source };
    let dxf = |writer: DxfWriter<'_>, output: W| {
        let writer = writer.with_version(document.version);
        NEVec::try_from_vec(writer.omitted_architecture_records()).map_or_else(|| writer.write_to_writer(output).map_err(drawing), |records| Err(CadError::Omitted { records }))
    };
    match format {
        Format::Dxf => dxf(DxfWriter::new(document), sink),
        Format::BinaryDxf => dxf(DxfWriter::new_binary(document), sink),
        Format::Dwg => DwgWriter::write_to_writer(sink, document).map_err(drawing),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod layouts;
pub mod linework;
