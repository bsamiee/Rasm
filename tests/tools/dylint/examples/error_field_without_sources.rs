#![feature(error_reporter)]

use core::error::Error;
use core::hint::black_box;
use core::num::ParseIntError;
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

fn displayed(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error, "port unread")) } //~ error_field_without_sources

fn debugged(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = ?error, "port unread")) } //~ error_field_without_sources

//~v error_field_without_sources
fn erased(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = error as &(dyn Error + 'static), "port unread")) }

fn sourced(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error.source, "port unread")) } //~ error_field_without_sources

fn spanned(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| drop(tracing::info_span!("port", { EXCEPTION_MESSAGE } = %error))) } //~ error_field_without_sources

fn recorded(text: &str) -> Result<u16, Unread> {
    parsed(text).inspect_err(|error| {
        tracing::info_span!("port", { EXCEPTION_MESSAGE } = Empty).record(EXCEPTION_MESSAGE, display(error)); //~ error_field_without_sources
    })
}

fn reported(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "port unread")) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(displayed, debugged, erased, sourced, spanned, recorded, reported)); }
