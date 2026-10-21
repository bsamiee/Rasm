use core::hint::black_box;

use tracing::field::Empty;

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
}

fn preformatted(count: u32) {
    let message = format!("handled {count} rows");
    tracing::info!(%message);
    tracing::info!(summary = %format_args!("handled {count}"), "done");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let owned = black_box("config").to_owned();
    loaded(black_box(3), &owned);
    listening(black_box(8080), &owned);
    preformatted(black_box(2));
}
