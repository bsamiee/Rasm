use core::{hint::black_box, time::Duration};
use std::{
    fs::{self, FileTimes},
    io::{self, BufRead as _, BufReader, Read as _, Write as _},
    net::TcpStream,
    path::{Path, PathBuf},
    process::Command,
    sync::mpsc as channel,
    thread,
};

use fs_err::{File, OpenOptions, PathExt as _, tokio::read_to_string};
use tokio::{
    sync::{Mutex, mpsc},
    task::{self, JoinHandle},
};

// --- [MACROS] --------------------------------------------------------------------------

macro_rules! both {
    ($path:expr) => {
        fs_err::read($path).and_then(|_| fs_err::read($path))
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn module_function(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }

async fn opened(path: &Path) -> io::Result<[u8; 4]> {
    let mut file = File::open(path)?;
    let mut header = [0; 4];
    file.read_exact(&mut header)?;
    Ok(header)
}

async fn appended(path: &Path) -> io::Result<()> { OpenOptions::new().append(true).open(path)?.sync_all() }

async fn first_line(path: &Path) -> io::Result<String> {
    let mut line = String::new();
    BufReader::new(File::create_new(path)?).read_line(&mut line)?;
    Ok(line)
}

async fn qualified(path: &Path) -> io::Result<[u8; 4]> {
    let mut header = [0; 4];
    io::Read::read_exact(&mut File::open(path)?, &mut header)?;
    Ok(header)
}

async fn prompt() -> io::Result<String> {
    io::stdout().write_all(b"name: ")?;
    let mut line = String::new();
    io::stdin().read_line(&mut line)?;
    Ok(line)
}

async fn parked() { thread::park_timeout(Duration::ZERO); }

async fn received(receiver: &mut mpsc::Receiver<u64>, lock: &Mutex<u64>) -> u64 { receiver.blocking_recv().unwrap_or_default().strict_add(*lock.blocking_lock()) }

async fn sizes(paths: &[PathBuf]) -> io::Result<Vec<usize>> { paths.iter().map(|path| fs_err::read(path).map(|bytes| bytes.len())).collect() }

fn deferred(path: PathBuf) -> JoinHandle<io::Result<String>> { tokio::spawn(async move { fs_err::read_to_string(path) }) }

async fn offloaded(path: PathBuf) -> io::Result<String> { task::spawn_blocking(move || fs_err::read_to_string(path)).await? }

async fn awaited(path: &Path) -> io::Result<String> { read_to_string(path).await }

async fn in_memory() -> io::Result<Vec<u8>> {
    let mut bytes = Vec::new();
    b"text".as_slice().read_to_end(&mut bytes)?;
    Ok(bytes)
}

async fn slept() { thread::sleep(Duration::ZERO); }

async fn joined() -> thread::Result<u64> { thread::spawn(|| 1_u64).join() }

async fn queued(sender: &channel::SyncSender<u64>) -> bool { sender.send(1).is_ok() }

async fn listed() -> io::Result<Vec<u8>> { Command::new("ls").output().map(|output| output.stdout) }

async fn connected() -> io::Result<TcpStream> { TcpStream::connect("127.0.0.1:80") }

async fn doubled(path: &Path) -> io::Result<Vec<u8>> { both!(path) }

async fn scoped(path: &Path) -> io::Result<Vec<u8>> { thread::scope(|scope| scope.spawn(|| fs_err::read(path)).join().unwrap_or_else(|_| Ok(Vec::new()))) }

async fn described(path: &Path) -> io::Result<u64> { path.fs_err_metadata().map(|metadata| metadata.len()) }

async fn touched(path: &Path) -> io::Result<()> { fs::set_times(path, FileTimes::new()) }

fn size(path: &Path) -> io::Result<u64> { Ok(fs_err::metadata(path)?.len()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    let path = Path::new("Cargo.toml");
    let (sender, mut receiver) = mpsc::channel(1);
    parked().await;
    slept().await;
    received(&mut receiver, &Mutex::new(2)).await;
    queued(&channel::sync_channel(1).0).await;
    black_box(&(
        sender.send(1).await,
        module_function(path).await,
        opened(path).await,
        appended(path).await,
        first_line(path).await,
        qualified(path).await,
        prompt().await,
        sizes(&[path.to_path_buf()]).await,
        deferred(path.to_path_buf()).await,
        offloaded(path.to_path_buf()).await,
        awaited(path).await,
        in_memory().await,
        size(path),
        joined().await.unwrap_or_default(),
        listed().await,
        connected().await,
        doubled(path).await,
        scoped(path).await,
        described(path).await,
        touched(path).await,
    ));
}
