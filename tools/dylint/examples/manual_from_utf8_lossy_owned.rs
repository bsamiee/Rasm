use core::hint::black_box;
use std::process::{ExitStatus, Output};

// --- [MODELS] --------------------------------------------------------------------------

struct Buffer {
    bytes: Vec<u8>,
}

impl Drop for Buffer {
    fn drop(&mut self) { self.bytes.clear(); }
}

struct Borrowed<'output> {
    output: &'output Output,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn decoded(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).into_owned() }

fn decoded_output(output: Output) -> String { String::from_utf8_lossy(&output.stdout).into_owned() }

fn reused(bytes: Vec<u8>) -> (String, usize) { (String::from_utf8_lossy(&bytes).into_owned(), bytes.len()) }

fn lent(bytes: &[u8]) -> String { String::from_utf8_lossy(bytes).into_owned() }

fn captured(bytes: Vec<u8>) -> String {
    let decode = || String::from_utf8_lossy(&bytes).into_owned();
    decode()
}

fn drained(buffer: Buffer) -> String { String::from_utf8_lossy(&buffer.bytes).into_owned() }

fn referenced(borrowed: Borrowed<'_>) -> String { String::from_utf8_lossy(&borrowed.output.stdout).into_owned() }

fn uppercased(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).to_uppercase() }

fn boxed(bytes: Vec<u8>) -> Box<str> { String::from_utf8_lossy(&bytes).into() }

fn fixed(bytes: [u8; 2]) -> String { String::from_utf8_lossy(&bytes).into_owned() }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let output = Output { status: ExitStatus::default(), stdout: vec![110], stderr: Vec::new() };
    black_box(&(
        decoded(vec![104]),
        decoded_output(Output { status: ExitStatus::default(), stdout: vec![105], stderr: Vec::new() }),
        reused(vec![106]),
        lent(&[107]),
        captured(vec![108]),
        drained(Buffer { bytes: vec![109] }),
        referenced(Borrowed { output: &output }),
        uppercased(vec![111]),
        boxed(vec![112]),
        fixed([113, 114]),
    ));
}
