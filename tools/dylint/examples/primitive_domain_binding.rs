use core::{hint::black_box, time::Duration};
use std::{io, path::PathBuf};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn waited(base: u64) -> u64 {
    let wait_millis = base.saturating_mul(2);
    let retry_delay = 500_u64;
    wait_millis.saturating_add(retry_delay)
}

fn located(root: &str) -> (PathBuf, io::Result<String>, io::Result<fs_err::File>) {
    let cache_dir = format!("{root}/cache");
    let data_file = format!("{root}/data.json");
    let log_file = format!("{root}/run.log");
    (PathBuf::from(cache_dir), fs_err::read_to_string(data_file), fs_err::File::open(log_file))
}

fn described(module: &str) -> String {
    let item_path = format!("{module}::Item");
    format!("`{item_path}` repeats")
}

fn stamped(base: i64) -> [i64; 4] {
    let created_at = base.saturating_add(1);
    let since_epoch = base.saturating_add(2);
    let epoch_ms = base.saturating_add(3);
    let timestamp = base.saturating_add(4);
    [created_at, since_epoch, epoch_ms, timestamp]
}

fn dated(day: u8) -> String {
    let start_date = format!("2026-10-{day:02}");
    start_date
}

fn measured(base: f64) -> f64 {
    let length_mm = base * 2.0;
    let angle_degrees = length_mm / 3.0;
    angle_degrees
}

fn typed(base: u64) -> Duration {
    let wait = Duration::from_millis(base);
    let declared_ms: u64 = base;
    wait.saturating_add(Duration::from_millis(declared_ms))
}

fn kept(root: &str) -> PathBuf {
    let config_path = PathBuf::from(root).join("config.toml");
    let file_count = root.len();
    config_path.with_extension(file_count.to_string())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() { black_box(&(waited(1), located("a"), described("b"), stamped(2), dated(3), measured(4.0), typed(5), kept("c"))); }
