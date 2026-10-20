use core::{hint::black_box, time::Duration};
use std::{io, path::PathBuf};

// --- [TYPES] ---------------------------------------------------------------------------

trait Schedule {
    fn wait(&self, delay_ms: u64);
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const POLL_INTERVAL: u64 = 250;
static CONFIG_PATH: &str = "config.toml";
const ITEM_PATH: &str = "crate::Item";

// --- [MODELS] --------------------------------------------------------------------------

struct Job {
    timeout_ms: u64,
    output_dir: String,
    label_path: String,
    started_at: i64,
    width_mm: f64,
}

impl Schedule for Job {
    fn wait(&self, delay_ms: u64) { black_box(delay_ms); }
}

#[derive(serde::Serialize)]
struct Reading {
    length_mm: f64,
    sample_bytes: u64,
    created_at: i64,
    window_secs: u64,
}

impl Schedule for Reading {
    fn wait(&self, delay_ms: u64) { black_box((self.sample_bytes, delay_ms)); }
}

#[repr(C)]
struct Native {
    delay_ms: u32,
}

enum Event {
    Sized { payload_bytes: usize },
}

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

fn sized(base: u64) -> u64 {
    let total_bytes = base.saturating_mul(2);
    let block_mb: f64 = 1.5;
    total_bytes.saturating_add(block_mb.to_bits())
}

fn ratio(base: f64) -> f64 {
    let load_percent = base / 2.0;
    load_percent
}

const fn annotated() -> u64 {
    let retry_delay: u64 = 500;
    retry_delay
}

fn scheduled(job: &Job, retry_secs: u64, cache_file: &str, note_file: &str) -> io::Result<String> {
    black_box((job.timeout_ms, job.started_at, job.width_mm, &job.label_path, retry_secs, note_file, POLL_INTERVAL, ITEM_PATH));
    fs_err::create_dir_all(&job.output_dir)?;
    fs_err::write(cache_file, CONFIG_PATH)?;
    fs_err::read_to_string(CONFIG_PATH)
}

extern "C" fn exported(timeout_ms: u64) -> u64 { timeout_ms }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let job = Job { timeout_ms: 1, output_dir: String::new(), label_path: String::new(), started_at: 2, width_mm: 3.0 };
    let reading = Reading { length_mm: 6.0, sample_bytes: 7, created_at: 8, window_secs: 9 };
    let Event::Sized { payload_bytes } = Event::Sized { payload_bytes: 9 };
    let waited_for = |timeout_ms: u64| timeout_ms.saturating_add(1);
    let inferred = |delay_ms| u64::saturating_add(delay_ms, 1);
    let labeled = |label_path: &str| label_path.len();
    black_box((waited_for(1), inferred(2), labeled("c")));
    job.wait(4);
    reading.wait(5);
    black_box(&(waited(1), located("a"), described("b"), stamped(2), dated(3), measured(4.0), typed(5), kept("c"), sized(6), (ratio(7.0), annotated())));
    black_box(&(scheduled(&job, 5, "a", "b"), Native { delay_ms: 8 }, payload_bytes, exported(10), POLL_INTERVAL, ITEM_PATH, CONFIG_PATH));
}
