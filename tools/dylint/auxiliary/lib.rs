// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, PartialEq, Eq, Hash)]
#[non_exhaustive]
pub enum Choice {
    First,
    Second,
    Third,
}

#[derive(Debug)]
pub enum Inner {
    Left,
    Right,
}

#[derive(Debug)]
pub enum Unanswered {
    Declined,
    Cancelled,
    Empty,
    Unsupported,
    Abandoned,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
#[non_exhaustive]
pub enum Outer<Content> {
    Known(Content),
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
#[error("single failure")]
pub struct SingleError;

#[derive(Debug, Default, thiserror::Error)]
#[error("open failure")]
#[non_exhaustive]
pub struct OpenError;

#[derive(Debug, thiserror::Error)]
#[error("dropped failure")]
pub struct DroppedError;

#[expect(clippy::empty_drop, reason = "Native destructor classification is the compiler fixture contract")]
impl Drop for DroppedError {
    fn drop(&mut self) {}
}
