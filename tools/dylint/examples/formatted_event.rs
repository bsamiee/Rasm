use core::hint::black_box;

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
    let _span = tracing::info_span!("request", port = format!("{port}"));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let owned = black_box("config").to_owned();
    loaded(black_box(3), &owned);
    listening(black_box(8080), &owned);
}
