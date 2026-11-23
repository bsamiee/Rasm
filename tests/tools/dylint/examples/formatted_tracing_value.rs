use core::hint::black_box;

use tracing::field::Empty;

// --- [MODELS] --------------------------------------------------------------------------

struct Token(u16);

impl ToString for Token {
    fn to_string(&self) -> String { self.0.to_string() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn loaded(rows: usize, path: &str) {
    tracing::info!("loaded {} rows from {path}", rows); //~ formatted_tracing_value interpolates values into text
    tracing::info!(rows, %path, "loaded");
    tracing::info!("loaded every row");
}

fn listening(port: u16, owned: &String) {
    tracing::warn!(port = format!("{port}"), "listening"); //~ formatted_tracing_value record the value as a field
    tracing::debug!(port = port.to_string(), "listening"); //~ formatted_tracing_value record the `u16` as a typed field: `port`
    tracing::debug!(port = ToString::to_string(&port), "listening"); //~ formatted_tracing_value record the `&u16` as a typed field: `&port`
    tracing::debug!(%owned, "named");
    tracing::debug!(name = owned.to_string(), "named");
    tracing::info_span!("request", port = Empty).record("port", port.to_string()); //~ formatted_tracing_value record the `u16` as a typed field: `&port`
    let span = tracing::info_span!("qualified", port = Empty);
    tracing::Span::record(&span, "port", &port.to_string()); //~ formatted_tracing_value record the `u16` as a typed field: `&port`
}

fn preformatted(count: u32) {
    let message = format!("handled {count} rows"); //~ formatted_tracing_value record the value as a field
    tracing::info!(%message);
    tracing::info!(summary = %format_args!("handled {count}"), "done"); //~ formatted_tracing_value record the value as a field
}

fn opaque(token: &Token) {
    tracing::debug!(token = token.to_string(), "token");
    tracing::debug!(token = ToString::to_string(token), "token");
}

fn retained() {
    let error = std::io::Error::from(std::io::ErrorKind::NotFound);
    tracing::info_span!("request", error = Empty).record("error", error.to_string()); //~ formatted_tracing_value record the value through its `Display` impl: `tracing::field::display(&error)`
    black_box(&error);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let owned = black_box("config").to_owned();
    loaded(black_box(3), &owned);
    listening(black_box(8080), &owned);
    preformatted(black_box(2));
    opaque(&Token(black_box(3)));
    retained();
}
