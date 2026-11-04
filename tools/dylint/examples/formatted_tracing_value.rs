use core::hint::black_box;

use tracing::field::Empty;

// --- [MODELS] --------------------------------------------------------------------------

struct Token(u16);

impl ToString for Token {
    fn to_string(&self) -> String { self.0.to_string() }
}

#[derive(Debug)]
enum Status {
    Ready,
    Waiting,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn loaded(rows: usize, path: &str) {
    tracing::info!("loaded {} rows from {path}", rows);
    tracing::info!(rows, %path, "loaded");
    tracing::info!("loaded every row");
}

fn listening(port: u16, owned: &String) {
    tracing::warn!(port = format!("{port}"), "listening");
    tracing::debug!(port = port.to_string(), "listening");
    tracing::debug!(%owned, "named");
    tracing::debug!(name = owned.to_string(), "named");
    let _span = tracing::info_span!("request", port = format!("{port}"));
    tracing::info_span!("request", port = Empty).record("port", port.to_string());
    tracing::debug!(port = ToString::to_string(&port), "listening");
    tracing::info_span!("request", port = Empty).record("port", ToString::to_string(&port));
    let span = tracing::info_span!("qualified", port = Empty);
    tracing::Span::record(&span, "port", &port.to_string());
    tracing::Span::record(&span, "port", format!("{port}"));
}

fn preformatted(count: u32) {
    let message = format!("handled {count} rows");
    tracing::info!(%message);
    tracing::info!(summary = %format_args!("handled {count}"), "done");
}

fn opaque(token: &Token) {
    tracing::debug!(token = token.to_string(), "token");
    tracing::debug!(token = ToString::to_string(token), "token");
    tracing::info_span!("request", token = Empty).record("token", token.to_string());
}

fn debugged(status: Status) {
    tracing::info!("state {status:?}");
    tracing::debug!(state = format!("{status:?}"), "state");
    tracing::info_span!("request", state = Empty).record("state", format!("{status:?}"));
}

fn retained() {
    let error = std::io::Error::from(std::io::ErrorKind::NotFound);
    tracing::info_span!("request", error = Empty).record("error", error.to_string());
    black_box(&error);
    let boxed: Box<dyn std::error::Error> = Box::new(error);
    tracing::info_span!("request", error = Empty).record("error", boxed.to_string());
    black_box(boxed);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let owned = black_box("config").to_owned();
    loaded(black_box(3), &owned);
    listening(black_box(8080), &owned);
    preformatted(black_box(2));
    opaque(&Token(black_box(3)));
    debugged(black_box(Status::Ready));
    debugged(black_box(Status::Waiting));
    retained();
}
