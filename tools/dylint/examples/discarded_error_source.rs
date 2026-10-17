use core::{hint::black_box, num::ParseIntError};

use tokio::sync::oneshot;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error("the port is not a number")]
    Parse(#[source] ParseIntError),
    #[error("the port is missing")]
    Missing,
    #[error("the reply channel closed")]
    Closed,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parse(text: &str) -> Result<u16, ParseIntError> { text.parse() }

fn named(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_error| Failure::Missing) }

fn ignored(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_| Failure::Missing) }

fn retried(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Err(Failure::Missing)) }

fn retried_named(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_ignored| Err(Failure::Missing)) }

fn kept(text: &str) -> Result<u16, Failure> { parse(text).map_err(Failure::Parse) }

fn read(text: &str) -> Result<u16, Failure> { parse(text).map_err(|error| Failure::Parse(error)) }

async fn received(answer: oneshot::Receiver<u16>) -> Result<u16, Failure> { answer.await.map_err(|_closed| Failure::Closed) }

fn recovered(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Ok(0)) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let (sender, answer) = oneshot::channel();
    black_box(&(sender.send(1), received(answer).await));
    black_box(&[named("1"), ignored("1"), retried("2"), retried_named("3"), kept("4"), read("5"), recovered("6")]);
}
