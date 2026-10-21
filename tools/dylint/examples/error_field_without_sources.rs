#![feature(error_reporter)]

use core::{error::Error, hint::black_box, num::ParseIntError};
use std::error::Report;

use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use tracing::field::{Empty, display};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
#[error("cannot read port {text}")]
struct Unread {
    text: String,
    source: ParseIntError,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parsed(text: &str) -> Result<u16, Unread> { text.parse().map_err(|source| Unread { text: text.to_owned(), source }) }

fn displayed(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error, "port unread")) }

fn debugged(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = ?error, "port unread")) }

fn erased(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = error as &(dyn Error + 'static), "port unread")) }

fn sourced(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error.source, "port unread")) }

fn spanned(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| drop(tracing::info_span!("port", { EXCEPTION_MESSAGE } = %error))) }

fn recorded(text: &str) -> Result<u16, Unread> {
    parsed(text).inspect_err(|error| {
        tracing::info_span!("port", { EXCEPTION_MESSAGE } = Empty).record(EXCEPTION_MESSAGE, display(error));
    })
}

fn reported(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "port unread")) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(displayed, debugged, erased, sourced, spanned, recorded, reported)); }
