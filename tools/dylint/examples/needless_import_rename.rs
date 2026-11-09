use core::{
    cell::UnsafeCell as Cell,
    cmp::{Reverse as Flipped, *},
    fmt::{self as format, Error, Write as _},
    hint::black_box,
    mem::{drop as discard, take as taken},
    sync::atomic::{AtomicU32, Ordering as Atomic},
};
#[cfg(unix)]
use std::os::unix::ffi::OsStrExt as Native;
#[cfg(windows)]
use std::os::windows::ffi::OsStrExt as Native;
use std::{
    collections::BTreeMap as Map,
    ffi::{self, OsStr},
    io::{Error as IoError, ErrorKind, Result as IoResult},
    process::ExitCode,
    sync::Arc as Shared,
    time::Instant as Clock,
    vec::Vec as Items,
};

use ::tokio::fs::DirEntry;

use self::tokio as runtime;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Instant;

// --- [OPERATIONS] ----------------------------------------------------------------------

mod tokio {
    pub(crate) const fn local() -> bool { true }
}

const fn fmt() -> u32 { 1 }

fn rendered(map: &Map<u32, u32>) -> Result<String, Error> {
    let mut text = String::new();
    write!(text, "{}", map.len())?;
    Ok(text)
}

fn shown(value: &dyn format::Display) -> String { value.to_string() }

const fn failed(error: IoError) -> IoResult<()> { Err(error) }

fn loaded(counter: &AtomicU32) -> bool { max(counter.load(Atomic::Relaxed), 1).cmp(&1) == Ordering::Equal }

fn elapsed(start: Option<Clock>, local: &Instant) -> bool { start.is_some_and(|start| start.duration_since(start).is_zero()) && format!("{local:?}").is_empty() }

const fn waited() -> bool {
    use core::time::Duration as Span;
    Span::from_secs(1).is_zero()
}

fn shadowed() -> bool {
    use core::time::Duration as Span;
    #[derive(Debug)]
    struct Duration;
    Span::from_secs(1).is_zero() && format!("{Duration:?}").is_empty()
}

fn dropped() -> bool {
    use std::{rc::*, sync::Weak as Detached};
    Detached::<u8>::new().upgrade().is_none() && Weak::<u8>::new().upgrade().is_none()
}

fn ranked() -> bool { Flipped(1) < Reverse(0) && runtime::local() && size_of::<DirEntry>() > 0 }

fn native() -> [usize; 2] { [Native::as_bytes(OsStr::new("x")).len(), ffi::OsString::new().len()] }

fn parameter_shadow(drop: String) { discard(drop); }

fn local_shadow() {
    use core::mem::take as moved;
    let mut values = vec![1_u8];
    let take = values.len();
    black_box((moved(&mut values), take));
}

fn closure_shadow(values: Vec<String>) -> Vec<usize> {
    values
        .into_iter()
        .map(|drop| {
            let length = drop.len();
            discard(drop);
            length
        })
        .collect()
}

fn pattern_shadow(value: Option<String>) {
    if let Some(drop) = value {
        discard(drop);
    }
}

fn generic_shadow<Vec>(value: Vec) -> (Vec, Items<u8>) { (value, Items::new()) }

fn implicit_shadow<Write>(value: Write) -> Write {
    use core::fmt::Write as TextWrite;
    let mut text = String::new();
    let _ = text.write_str("x");
    let result: Write = value;
    black_box(text);
    result
}

fn later_binding() {
    use core::mem::take as removed;
    let mut values = vec![1_u8];
    let previous = removed(&mut values);
    let take = values.len();
    black_box((previous, take));
}

fn nested_shadow(drop: String) {
    use core::mem::drop as dismiss;
    fn consume(value: String) { dismiss(value); }
    black_box((drop, consume));
}

fn nested_local_shadow() {
    use core::mem::drop as release;
    let drop = String::new();
    fn consume(value: String) { release(value); }
    black_box((drop, consume));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    let shared = Shared::new(AtomicU32::new(0));
    let rendered = rendered(&Map::new()).map_or(0, |text| text.len());
    let flags = [loaded(&shared), elapsed(None, &Instant), waited(), shadowed(), dropped(), taken(&mut vec![1_u8]).is_empty()];
    black_box(&(rendered, native(), shown(&fmt()), Cell::new(0_u8).into_inner(), failed(IoError::from(ErrorKind::NotFound)), paired::joined(Error, &IoError::from(ErrorKind::NotFound))));
    black_box(&(parameter_shadow, local_shadow, closure_shadow, pattern_shadow, generic_shadow::<u8>, implicit_shadow::<u8>, later_binding, nested_shadow, nested_local_shadow));
    ExitCode::from(u8::from(flags.contains(&true) || ranked() || tokio::local()))
}

mod paired {
    use std::{fmt::Error as FormatError, io::Error as SystemError};

    pub(crate) fn joined(format: FormatError, io: &SystemError) -> bool { format == FormatError && io.raw_os_error().is_none() }
}
