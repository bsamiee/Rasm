#![feature(default_field_values)]
use std::process::ExitCode;

use clap::Parser;
use futures::stream;
use rasm_telemetry::Defaults;
use serde::Serialize;

#[derive(Parser)]
struct Args {
    #[arg(long, default_value_t = 3)]
    rows: usize,
    #[arg(long)]
    wait: bool,
    #[arg(long, default_value_t = 0)]
    wide: usize,
}

#[derive(Debug, thiserror::Error)]
enum ProbeError {
    #[error(transparent)]
    Command(#[from] rasm_command::CommandError),
    #[error(transparent)]
    Output(#[from] rasm_output::OutputError),
}

impl From<ProbeError> for sysexits::ExitCode {
    fn from(value: ProbeError) -> Self {
        match value {
            ProbeError::Command(error) => error.into(),
            ProbeError::Output(error) => error.into(),
        }
    }
}

#[derive(Serialize)]
struct Item {
    id: usize,
    ratio: f64,
    big: f64,
    whole: f64,
    name: String,
    #[serde(with = "serde_bytes_like")]
    raw: Vec<u8>,
    tags: Vec<u8>,
}

mod serde_bytes_like {
    pub fn serialize<S: serde::Serializer>(value: &[u8], serializer: S) -> Result<S::Ok, S::Error> { serializer.serialize_bytes(value) }
}

fn main() -> ExitCode {
    rasm_output::run(Defaults { .. }, async move |Args { rows, wait, wide }: Args| {
        if wait {
            futures::future::pending::<()>().await;
        }
        Ok::<_, nonempty_collections::NEVec<ProbeError>>(stream::iter((0..rows).map(move |id| Ok(Item { id, ratio: 0.1, big: 1e300, whole: 1.0, name: format!("row{id}{}", "x".repeat(wide)), raw: b"hi".to_vec(), tags: vec![1, 2] }))))
    })
}
