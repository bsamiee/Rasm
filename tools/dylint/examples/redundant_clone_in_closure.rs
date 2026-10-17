use core::{hint::black_box, num::ParseIntError};

use futures::executor::block_on;

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
#[error("cannot read a port from {path}")]
struct ReadError {
    path: String,
    #[source]
    source: ParseIntError,
}

#[derive(Clone)]
struct Lease(String);

impl Drop for Lease {
    fn drop(&mut self) { black_box(&self.0); }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn mapped(path: String, text: &str) -> Result<u16, ReadError> { text.parse().map_err(|source| ReadError { path: path.clone(), source }) }

fn fallback(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.clone())) }

fn deferred(name: String) -> String { block_on(async { name.clone() }) }

fn owned(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.to_owned())) }

fn called(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(Clone::clone(&name))) }

async fn awaited(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.clone())) }

fn copied(port: u16, given: Option<u16>) -> Option<u16> { given.or_else(|| Some(port.clone())) }

fn leased(lease: Lease, given: Option<Lease>) -> Lease { given.unwrap_or_else(|| lease.clone()) }

fn reused(path: String, text: &str) -> (Result<u16, ReadError>, usize) {
    let parsed = text.parse().map_err(|source| ReadError { path: path.clone(), source });
    (parsed, path.len())
}

fn repeated(name: &str, items: &[u8]) -> Vec<String> {
    let owned = name.to_owned();
    items.iter().map(|_| owned.clone()).collect()
}

fn moved(path: String, text: &str) -> Result<u16, ReadError> { text.parse().map_err(move |source| ReadError { path: path.clone(), source }) }

fn borrowed(path: &String, text: &str) -> Result<u16, ReadError> { text.parse().map_err(|source| ReadError { path: path.clone(), source }) }

fn looped(name: String, givens: Vec<Option<String>>) -> Vec<Option<String>> {
    let mut found = Vec::new();
    for given in givens {
        found.push(given.or_else(|| Some(name.clone())));
    }
    found
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let path = String::from("ports.txt");
    black_box(&(
        mapped(path.clone(), "80"),
        fallback(String::from("default"), None),
        deferred(String::from("task")),
        owned(String::from("owned"), None),
        called(String::from("called"), None),
        block_on(awaited(String::from("awaited"), None)),
        copied(80, None),
        leased(Lease(String::from("lease")), None).0.len(),
        reused(path.clone(), "81"),
        repeated("item", &[1, 2]),
        moved(path.clone(), "82"),
        borrowed(&path, "83"),
        looped(String::from("each"), vec![None, None]),
    ));
}
