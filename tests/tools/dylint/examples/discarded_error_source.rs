use core::alloc::{Layout, LayoutError};
use core::hint::black_box;
use core::num::{NonZero, ParseIntError, TryFromIntError};

use clap::ColorChoice;
use rasm_lints_auxiliary::{DroppedError, OpenError, SingleError};
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
    #[error("unknown color {color}")]
    Color { color: String },
}

#[derive(Debug, thiserror::Error)]
#[error("the port is reserved")]
struct Reserved;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn parse(text: &str) -> Result<u16, ParseIntError> { text.parse() }

fn named(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_error| Failure::Missing) }

fn ignored(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_| Failure::Missing) }

fn retried(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Err(Failure::Missing)) }

fn retried_named(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_ignored| Err(Failure::Missing)) }

fn destructured(text: &str) -> Result<u16, Failure> { text.parse::<u16>().map_err(anyhow::Error::from).map_err(|anyhow::Error { .. }| Failure::Missing) }

fn kept(text: &str) -> Result<u16, Failure> { parse(text).map_err(Failure::Parse) }

fn read(text: &str) -> Result<u16, Failure> { parse(text).map_err(|error| Failure::Parse(error)) }

fn text_restated(name: &str) -> Result<ColorChoice, Failure> { name.parse().map_err(|_: String| Failure::Color { color: name.to_owned() }) }

fn text_dropped(name: &str) -> Result<ColorChoice, Failure> { name.parse().map_err(|_: String| Failure::Missing) }

fn text_retried(name: &str) -> Result<ColorChoice, Failure> { name.parse().or_else(|_: String| Err(Failure::Color { color: name.to_owned() })) }

async fn received(answer: oneshot::Receiver<u16>) -> Result<u16, Failure> { answer.await.map_err(|_closed| Failure::Closed) }

fn recovered(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Ok(0)) }

fn port(bytes: Vec<u8>) -> Result<u16, Failure> { <[u8; 2]>::try_from(bytes).map(u16::from_be_bytes).map_err(|Vec { .. }| Failure::Missing) }

fn unsigned(value: u64) -> Result<u16, Failure> { u16::try_from(value).map_err(|_| Failure::Missing) }

fn negative_only(value: i16) -> Result<u16, Failure> { u16::try_from(value).map_err(|_| Failure::Missing) }

fn implicit(value: u64) -> Result<u16, Failure> { value.try_into().map_err(|_| Failure::Missing) }

fn translated(value: usize) -> Result<u32, Failure> { u32::try_from(value).or_else(|_| Err(Failure::Missing)) }

fn composed(value: u64) -> Result<u16, Failure> { u16::try_from(value).map(u32::from).and_then(u16::try_from).map_err(|_| Failure::Missing) }

fn composed_closure(value: u64) -> Result<u16, Failure> { u16::try_from(value).and_then(|converted| u16::try_from(u32::from(converted))).map_err(|_| Failure::Missing) }

fn early_return(value: u64) -> Result<u16, Failure> {
    u16::try_from(value)
        .and_then(|converted| {
            u16::try_from({
                if converted == 0 {
                    return u16::try_from(-1_i16);
                }
                u32::from(converted)
            })
        })
        .map_err(|_| Failure::Missing)
}

fn early_error(value: u64) -> Result<u16, Failure> {
    u16::try_from(value)
        .and_then(|converted| {
            u16::try_from({
                let valid = u16::try_from(if converted == 0 { -1_i16 } else { 1 })?;
                u32::from(valid)
            })
        })
        .map_err(|_| Failure::Missing)
}

fn outer_break(value: u64) -> Result<u16, Failure> {
    u16::try_from(value)
        .and_then(|converted| 'conversion: {
            u16::try_from({
                if converted == 0 {
                    break 'conversion u16::try_from(-1_i16);
                }
                u32::from(converted)
            })
        })
        .map_err(|_| Failure::Missing)
}

fn two_sided(value: i32) -> Result<u16, Failure> { u16::try_from(value).map_err(|_| Failure::Missing) }

fn nonzero(value: NonZero<i32>) -> Result<NonZero<u16>, Failure> { NonZero::try_from(value).map_err(|_| Failure::Missing) }

fn opaque(read: impl FnOnce() -> Result<u16, TryFromIntError>) -> Result<u16, Failure> { read().map_err(|_| Failure::Missing) }

fn reserved(value: Result<u16, Reserved>) -> Result<u16, Failure> { value.map_err(|_| Failure::Missing) }

fn array_layout(count: usize) -> Result<usize, Failure> { Layout::array::<u8>(count).map(|layout| layout.size()).map_err(|LayoutError { .. }| Failure::Missing) }

fn aligned_layout(size: usize, align: usize) -> Result<Layout, Failure> { Layout::from_size_align(size, align).map_err(|LayoutError { .. }| Failure::Missing) }

fn opaque_layout(read: impl FnOnce() -> Result<Layout, LayoutError>) -> Result<Layout, Failure> { read().map_err(|LayoutError { .. }| Failure::Missing) }

fn external() -> [Result<u16, Failure>; 4] {
    [
        Err(SingleError).map_err(|_| Failure::Missing),
        Err(SingleError).or_else(|_| Err(Failure::Missing)),
        Err(OpenError::default()).map_err(|_| Failure::Missing),
        Err(DroppedError).map_err(|_| Failure::Missing),
    ]
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let (sender, answer) = oneshot::channel();
    black_box(&(sender.send(1), received(answer).await));
    black_box(&external());
    black_box(&[named("1"), ignored("1"), retried("2"), retried_named("3"), destructured("1"), kept("4"), read("5"), recovered("6"), port(vec![0, 80])]);
    black_box(&[text_restated("auto"), text_dropped("never"), text_retried("always")]);
    black_box(&(
        [unsigned(1), negative_only(-2), implicit(3), composed(4), two_sided(-5), opaque(|| u16::try_from(6_i32)), reserved(Err(Reserved))],
        translated(7),
        nonzero(const { NonZero::new(-8).unwrap() }),
    ));
    black_box(&[composed_closure(9), early_return(0), early_error(0), outer_break(0)]);
    black_box(&(array_layout(10), aligned_layout(11, 3), opaque_layout(|| Layout::from_size_align(12, 4))));
}
