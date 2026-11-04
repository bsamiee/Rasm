#![feature(can_vector)]

use core::{hint::black_box, time::Duration};
use std::{
    fs::File,
    io::{self, BufRead as _, BufReader, Read as _},
    path::{Path, PathBuf},
    thread,
};

use fs_err::{PathExt as _, tokio::read_to_string};
use tokio::task;

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! both {
    ($path:expr) => {
        fs_err::read($path).and_then(|_| fs_err::read($path))
    };
}

// --- [MODELS] --------------------------------------------------------------------------

struct Memory<T> {
    bytes: io::Cursor<Vec<u8>>,
    marker: core::marker::PhantomData<T>,
}

impl<T> io::Read for Memory<T> {
    fn read(&mut self, buffer: &mut [u8]) -> io::Result<usize> { self.bytes.read(buffer) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn slept() { thread::sleep(Duration::ZERO); }

fn called<F: FnOnce() + 'static>(call: F) { call(); }

async fn inline_callback() { called(|| thread::sleep(Duration::ZERO)); }

async fn module_function(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }

async fn native_read(path: &Path) -> io::Result<Vec<u8>> { std::fs::read(path) }

async fn described(path: &Path) -> io::Result<u64> { path.fs_err_metadata().map(|metadata| metadata.len()) }

async fn first_line(file: io::Result<File>) -> io::Result<String> {
    let mut line = String::new();
    BufReader::new(file?).read_line(&mut line)?;
    Ok(line)
}

async fn sizes(paths: &[PathBuf]) -> io::Result<Vec<usize>> { paths.iter().map(|path| fs_err::read(path).map(|bytes| bytes.len())).collect() }

async fn doubled(path: &Path) -> io::Result<Vec<u8>> { both!(path) }

async fn offloaded(path: PathBuf) -> io::Result<String> { task::spawn_blocking(move || fs_err::read_to_string(path)).await? }

async fn scoped() {
    thread::scope(|scope| {
        scope.spawn(|| thread::sleep(Duration::ZERO));
    });
}

async fn awaited(path: &Path) -> io::Result<String> { read_to_string(path).await }

async fn in_memory() -> io::Result<Vec<u8>> {
    let mut bytes = Vec::new();
    b"text".as_slice().read_to_end(&mut bytes)?;
    Ok(bytes)
}

fn size(path: &Path) -> io::Result<u64> { Ok(fs_err::metadata(path)?.len()) }

async fn drained(received: std::sync::mpsc::Receiver<u8>) -> usize { received.iter().count() }

async fn served(listener: std::net::TcpListener) -> usize { listener.incoming().take(1).count() }

async fn qualified_read(file: &mut File) -> io::Result<usize> { File::read(file, &mut [0; 1]) }

async fn rayon_offloaded() { rayon::spawn(|| thread::sleep(Duration::ZERO)); }

async fn metadata_file(memory: &mut Memory<File>) -> io::Result<usize> { memory.read(&mut [0; 1]) }

async fn prepared(received: &std::sync::mpsc::Receiver<u8>) -> std::sync::mpsc::Iter<'_, u8> { received.iter() }

async fn buffered_lines(reader: BufReader<File>) -> io::Lines<BufReader<File>> { reader.lines() }

async fn capabilities(file: &File) -> bool { file.is_read_vectored() }

async fn timed_received(received: &std::sync::mpsc::Receiver<u8>) -> Result<u8, std::sync::mpsc::RecvTimeoutError> { received.recv_timeout(Duration::from_secs(1)) }

async fn timed_connect(address: &std::net::SocketAddr) -> io::Result<std::net::TcpStream> { std::net::TcpStream::connect_timeout(address, Duration::from_secs(1)) }

async fn deferred_callback() -> impl FnOnce() { || thread::sleep(Duration::ZERO) }

async fn blocked_frame() {
    futures::executor::block_on(async {
        let state = std::rc::Rc::new(());
        core::future::pending::<()>().await;
        black_box(state);
    });
}

fn spawned_frame() -> task::JoinHandle<()> { task::spawn(blocked_frame()) }

async fn parallel_callback() {
    use rayon::prelude::*;

    [0_u8].par_iter().for_each(|_| thread::sleep(Duration::ZERO));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let path = Path::new("Cargo.toml");
    slept().await;
    inline_callback().await;
    scoped().await;
    black_box(&(
        module_function(path).await,
        described(path).await,
        first_line(File::open(path)).await,
        sizes(&[path.to_path_buf()]).await,
        doubled(path).await,
        offloaded(path.to_path_buf()).await,
        awaited(path).await,
        in_memory().await,
        size(path),
        drained(std::sync::mpsc::channel().1).await,
        std::net::TcpListener::bind("127.0.0.1:0").map(served),
    ));
}
