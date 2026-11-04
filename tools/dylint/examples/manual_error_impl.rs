#![feature(error_generic_member_access)]

use core::{
    error::{Error, Request, request_value},
    fmt,
    hint::black_box,
    num::ParseIntError,
    sync::atomic::{AtomicUsize, Ordering},
};

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! source_type {
    ($source:ty) => {
        $source
    };
}

// --- [TYPES] ---------------------------------------------------------------------------

type Backtrace = u16;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Manual;

impl fmt::Display for Manual {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("manual failure") }
}

impl Error for Manual {}

#[derive(Debug, derive_more::Display)]
#[display("source failure")]
struct Sourced<E>(E);

impl<E: Error + 'static> Error for Sourced<E> {
    fn source(&self) -> Option<&(dyn Error + 'static)> { Some(&self.0) }
}

#[derive(Debug, derive_more::Display)]
#[display("constrained failure")]
struct Constrained<E>(E);

impl<E: Error + Send + 'static> Error for Constrained<E> {
    fn source(&self) -> Option<&(dyn Error + 'static)> { Some(&self.0) }
}

struct MacroSource<E> {
    source: source_type!(E),
}

impl<E> fmt::Debug for MacroSource<E>
where E: fmt::Debug
{
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.debug_struct("MacroSource").field("source", &self.source).finish() }
}

impl<E> fmt::Display for MacroSource<E> {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("macro failure") }
}

impl<E: Error + 'static> Error for MacroSource<E> {
    fn source(&self) -> Option<&(dyn Error + 'static)> { Some(&self.source) }
}

#[derive(Debug, derive_more::Display)]
#[display("marker failure")]
struct Marker<'value>(&'value ());

impl<'value> Error for Marker<'value> where 'value: 'static {}

#[derive(Debug, derive_more::Display)]
#[display("hidden {source}, trace {backtrace}")]
struct Hidden {
    source: ParseIntError,
    backtrace: Backtrace,
}

impl Error for Hidden {}

#[derive(Debug, derive_more::Display, derive_more::Error)]
#[display("supported {source}, trace {backtrace}")]
#[error(ignore)]
struct Supported<E> {
    #[error(source, not(backtrace))]
    source: E,
    backtrace: Backtrace,
}

#[derive(Debug)]
struct Inner {
    source: ParseIntError,
}

#[derive(Debug, derive_more::Display, derive_more::Deref)]
#[display("projected failure")]
struct Projected(Inner);

impl Error for Projected {
    fn source(&self) -> Option<&(dyn Error + 'static)> { Some(&self.source) }
}

#[derive(Debug, derive_more::Display)]
#[display("provided failure {_0}")]
struct Provided(u16);

impl Error for Provided {
    fn provide<'request>(&'request self, request: &mut Request<'request>) { request.provide_value(self.0); }
}

#[derive(Debug, derive_more::Display)]
#[display("conditional failure")]
struct Conditional {
    revealed: bool,
    source: ParseIntError,
}

impl Error for Conditional {
    fn source(&self) -> Option<&(dyn Error + 'static)> { if self.revealed { Some(&self.source) } else { None } }
}

#[derive(Debug, derive_more::Display)]
#[display("observed failure")]
struct Observed {
    reads: AtomicUsize,
    source: ParseIntError,
}

impl Error for Observed {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        self.reads.fetch_add(1, Ordering::Relaxed);
        Some(&self.source)
    }
}

#[derive(Debug, derive_more::Display)]
#[display("legacy failure")]
struct Legacy;

impl Error for Legacy {
    fn description(&self) -> &str { "legacy description" }
    fn cause(&self) -> Option<&dyn Error> { Some(&Manual) }
}

#[derive(Debug, derive_more::Display)]
#[display("specialized failure")]
struct Specialized<const ENABLED: bool>;

impl Error for Specialized<true> {}

pub union RawError {
    pub code: u32,
}

impl fmt::Debug for RawError {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("RawError") }
}

impl fmt::Display for RawError {
    fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("raw failure") }
}

impl Error for RawError {}

#[derive(Debug, thiserror::Error)]
#[error("derived failure")]
struct Derived;

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let source = || "invalid".parse::<u16>().unwrap_err();
    black_box(&(
        Manual.to_string(),
        Manual.source().is_none(),
        Sourced(source()).source().is_some(),
        Constrained(source()).source().is_some(),
        MacroSource { source: source() }.source().is_some(),
        Marker(&()).0,
        Hidden { source: source(), backtrace: 0 }.source().is_none(),
        Supported { source: source(), backtrace: 0 }.source().is_some(),
        Projected(Inner { source: source() }).source().is_some(),
        request_value::<u16>(&Provided(42)),
        Conditional { revealed: true, source: source() }.source().is_some(),
        Observed { reads: AtomicUsize::new(0), source: source() }.source().is_some(),
        Legacy.to_string(),
        Specialized::<true>.to_string(),
        RawError { code: 0 }.to_string(),
        Derived.source().is_none(),
    ));
}
