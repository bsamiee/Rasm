#![feature(error_reporter)]
#![cfg_attr(unix, feature(exit_status_error))]

use core::error::Error;
#[cfg(target_vendor = "apple")]
use core::ffi::c_char;
#[cfg(unix)]
use core::hash::BuildHasher as _;
use core::num::NonZero;
use core::pin::{Pin, pin};
use core::time::Duration;
use core::{fmt, iter};
#[cfg(windows)]
use core::{ptr, slice};
use std::collections::VecDeque;
#[cfg(target_vendor = "apple")]
use std::collections::{BTreeMap, HashSet};
use std::error::Report;
use std::ffi::{OsStr, OsString};
#[cfg(unix)]
use std::fs::File;
#[cfg(unix)]
use std::hash::RandomState;
use std::io;
#[cfg(unix)]
use std::os::fd::{AsFd, OwnedFd};
#[cfg(target_vendor = "apple")]
use std::os::fd::{AsRawFd, RawFd};
#[cfg(unix)]
use std::os::unix::ffi::OsStrExt as _;
#[cfg(unix)]
use std::os::unix::fs::FileTypeExt as _;
#[cfg(target_vendor = "apple")]
use std::os::unix::fs::MetadataExt as _;
#[cfg(unix)]
use std::os::unix::net::UnixStream as Socket;
#[cfg(windows)]
use std::os::windows::ffi::OsStringExt as _;
use std::path::{Path, PathBuf};
#[cfg(unix)]
use std::process;
use std::process::{ExitStatus, Stdio};

#[cfg(target_vendor = "apple")]
use fs_err::PathExt as _;
use itertools::Itertools as _;
#[cfg(target_vendor = "apple")]
use libproc::bsd_info::BSDInfo;
#[cfg(target_vendor = "apple")]
use libproc::file_info::{ListFDs, ProcFDInfo, ProcFDType, pidfdinfo};
#[cfg(target_vendor = "apple")]
use libproc::net_info::{SocketFDInfo, SocketInfoKind};
#[cfg(target_vendor = "apple")]
use libproc::proc_pid::{listpidinfo, pidcwd, pidinfo};
#[cfg(target_vendor = "apple")]
use libproc::processes::{ProcFilter, pids_by_type};
#[cfg(target_vendor = "apple")]
use nix::sys::event::{EvFlags, EventFilter, FilterFlag, KEvent, Kqueue};
#[cfg(target_vendor = "apple")]
use nix::sys::time::TimeSpec;
#[cfg(unix)]
use nix::unistd::{Uid, User};
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use process_wrap::tokio::{ChildWrapper, CommandWrap, KillOnDrop};
#[cfg(unix)]
use process_wrap::tokio::{CommandWrapper, ProcessGroup, ProcessSession};
#[cfg(windows)]
use process_wrap::tokio::{CreationFlags, JobObject};
#[cfg(unix)]
use rustix::{io::Errno, process::Signal};
#[cfg(unix)]
use socket2::SockRef;
use thiserror::Error;
#[cfg(target_vendor = "apple")]
use tokio::io::Interest;
#[cfg(target_vendor = "apple")]
use tokio::io::unix::AsyncFd;
use tokio::io::{AsyncBufReadExt as _, AsyncRead, AsyncReadExt as _, AsyncWrite, BufReader};
#[cfg(unix)]
use tokio::net::UnixStream;
#[cfg(unix)]
use tokio::net::unix::pipe;
use tokio::process::ChildStderr;
#[cfg(any(windows, target_vendor = "apple"))]
use tokio::task;
use tokio::time::{Instant, sleep_until, timeout};
#[cfg(unix)]
use tokio_util::either::Either;
use tokio_util::sync::CancellationToken;
#[cfg(target_vendor = "apple")]
use tokio_util::task::AbortOnDropHandle;
#[cfg(windows)]
use windows::Win32::Foundation::HANDLE;
#[cfg(windows)]
use windows::Win32::Security::{TOKEN_DUPLICATE, TOKEN_QUERY};
#[cfg(windows)]
use windows::Win32::System::Environment::{CreateEnvironmentBlock, DestroyEnvironmentBlock};
#[cfg(windows)]
use windows::Win32::System::Threading::{CREATE_NEW_PROCESS_GROUP, DETACHED_PROCESS, GetCurrentProcess, OpenProcessToken};
#[cfg(windows)]
use windows::core::{Owned, PCWSTR};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
pub enum ProcessOperation {
    #[display("start the process")]
    Start,
    #[display("observe the process exit")]
    Wait,
    #[display("attach process streams")]
    Attach,
    #[display("release the process group or job")]
    Release,
    #[display("read the login environment")]
    Login,
    #[display("watch the temporary folders for sockets the process group binds")]
    Watch,
    #[display("remove a socket the process group left")]
    Remove,
}

pub type Input = Pin<Box<dyn AsyncWrite + Send>>;

pub type Output = Pin<Box<dyn AsyncRead + Send>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(derive_more::Debug)]
pub struct Streams {
    pub id: NonZero<u32>,
    #[debug(skip)]
    pub stdin: Option<Input>,
    #[debug(skip)]
    pub stdout: Option<Output>,
    pub stderr: Option<ChildStderr>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, Error)]
pub enum Failure {
    #[error("process cancelled")]
    Cancelled,
    #[error("process deadline expired")]
    Deadline,
    #[error("cannot {operation}")]
    Io { operation: ProcessOperation, source: io::Error },
}

#[derive(Debug, Error)]
#[error(
    "{} {}failed: {}",
    Path::new(program).display(),
    fmt::from_fn(|formatter| match process {
        Some((pid, Some(status))) => write!(formatter, "({pid}, {status}) "),
        Some((pid, None)) => write!(formatter, "({pid}) "),
        None => Ok(()),
    }),
    failures.iter().format_with("; ", |failure, emit| emit(&iter::successors(Some(failure as &dyn Error), |&cause| cause.source()).format(": ")))
)]
pub struct ProcessError {
    pub program: OsString,
    pub process: Option<(NonZero<u32>, Option<ExitStatus>)>,
    pub failures: NEVec<Failure>,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[cfg(unix)]
#[derive(Debug)]
struct KillOnParentDeath {
    grace: Duration,
}

#[cfg(unix)]
impl CommandWrapper for KillOnParentDeath {
    fn wrap_child(&mut self, child: Box<dyn ChildWrapper>, _core: &CommandWrap) -> io::Result<Box<dyn ChildWrapper>> {
        let grace = self.grace.as_secs_f64();
        let script = format!("exec 3<&0; {{ trap '' HUP INT TERM; read -r line; sleep {grace}; kill -s TERM 0; sleep {grace}; kill -s KILL 0; }} <&3 3<&- &");
        let leader = child.id().ok_or_else(|| io::Error::other("spawned process has no PID"))?;
        let (watch, parent) = io::pipe()?;
        let mut watcher = tokio::process::Command::new("sh");
        watcher.args(["-c", &script]).stdin(watch).stdout(Stdio::null()).stderr(Stdio::null());
        drop(CommandWrap::from(watcher).wrap(ProcessGroup::attach_to(leader)).spawn()?);
        Ok(Box::new(KillOnParentDeathChild { inner: child, _parent: parent }))
    }
}

#[cfg(unix)]
#[derive(Debug)]
struct KillOnParentDeathChild {
    inner: Box<dyn ChildWrapper>,
    _parent: io::PipeWriter,
}

#[cfg(unix)]
impl ChildWrapper for KillOnParentDeathChild {
    fn inner(&self) -> &dyn ChildWrapper { self.inner.as_ref() }

    fn inner_mut(&mut self) -> &mut dyn ChildWrapper { self.inner.as_mut() }

    fn into_inner(self: Box<Self>) -> Box<dyn ChildWrapper> { self.inner }
}

#[derive(Debug)]
struct Released(Box<dyn ChildWrapper>);

impl Drop for Released {
    fn drop(&mut self) {
        if let Err(error) = self.0.start_kill() {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process group already released");
        }
    }
}

#[cfg(target_vendor = "apple")]
#[derive(Debug)]
struct Sockets {
    queue: Kqueue,
    folders: Vec<(PathBuf, usize, fs_err::File)>,
    seen: HashSet<(u64, u64)>,
}

#[cfg(target_vendor = "apple")]
impl AsRawFd for Sockets {
    fn as_raw_fd(&self) -> RawFd { self.queue.as_fd().as_raw_fd() }
}

#[cfg(target_vendor = "apple")]
impl Sockets {
    fn watch(named: impl IntoIterator<Item = PathBuf>) -> io::Result<Self> {
        let opened = named.into_iter().map(|path| match fs_err::File::open(&path).and_then(|file| Ok((file.metadata()?, file))) {
            Ok((folder, file)) => Ok(Some(((folder.dev(), folder.ino()), (path, file)))),
            Err(error) if error.kind() == io::ErrorKind::NotFound => Ok(None),
            Err(error) => Err(error),
        });
        let distinct = opened.flatten_ok().collect::<io::Result<BTreeMap<_, _>>>()?.into_values();
        let folders = distinct.map(|(path, file)| Ok::<_, io::Error>((path, usize::try_from(file.as_raw_fd()).map_err(io::Error::other)?, file))).collect::<io::Result<Vec<_>>>()?;
        let changes = Vec::from_iter(folders.iter().map(|(_, ident, _)| KEvent::new(*ident, EventFilter::EVFILT_VNODE, EvFlags::EV_ADD | EvFlags::EV_CLEAR, FilterFlag::NOTE_WRITE, 0, 0)));
        let queue = Kqueue::new()?;
        queue.kevent(&changes, &mut [], None)?;
        let seen = folders.iter().map(|(path, _, _)| sockets(path)).flatten_ok().map_ok(|(_, file)| file).collect::<io::Result<HashSet<_>>>()?;
        Ok(Self { queue, folders, seen })
    }

    fn woke(&mut self, group: NonZero<u32>) -> io::Result<Vec<(PathBuf, (u64, u64))>> {
        fn unread<T>(error: io::Error) -> io::Result<Option<T>> {
            match Errno::from_io_error(&error) {
                Some(Errno::SRCH | Errno::BADF | Errno::PERM) => Ok(None),
                Some(_) | None => Err(error),
            }
        }
        let read = |_: String| io::Error::last_os_error();
        let listed = |pid: u32| {
            let pid = i32::try_from(pid).map_err(io::Error::other)?;
            let opened = usize::try_from(pidinfo::<BSDInfo>(pid, 0).map_err(read)?.pbi_nfiles).map_err(io::Error::other)?;
            listpidinfo::<ListFDs>(pid, opened).map_err(read).map(|descriptors| Some(descriptors.into_iter().map(move |descriptor| (pid, descriptor))))
        };
        let address = |(pid, descriptor): (i32, ProcFDInfo)| {
            let socket = pidfdinfo::<SocketFDInfo>(pid, descriptor.proc_fd).map_err(read)?;
            if !matches!(SocketInfoKind::from(socket.psi.soi_kind), SocketInfoKind::Un) {
                return Ok(None);
            }
            let bytes = unsafe { socket.psi.soi_proto.pri_un.unsi_addr.ua_sun }.sun_path.map(c_char::cast_unsigned);
            let path = Path::new(OsStr::from_bytes(bytes.split(|byte| *byte == 0).next().unwrap_or_default()));
            match (path.is_empty(), path.is_absolute()) {
                (true, _) => Ok(None),
                (false, true) => Ok(Some(path.to_owned())),
                (false, false) => pidcwd(pid).map_err(read).map(|directory| Some(directory.join(path))),
            }
        };
        let held = |file: (u64, u64)| {
            let members = pids_by_type(ProcFilter::ByProgramGroup { pgrpid: group.get() })?;
            let descriptors = members.into_iter().map(|pid| listed(pid).or_else(unread)).flatten_ok().flatten_ok();
            let sockets = descriptors.filter_ok(|(_, descriptor)| matches!(ProcFDType::from(descriptor.proc_fdtype), ProcFDType::Socket));
            let paths = sockets.map(|socket| socket.and_then(|socket| address(socket).or_else(unread))).flatten_ok();
            let matched = paths.map(|path| match path.and_then(|path| path.fs_err_symlink_metadata()) {
                Ok(socket) => Ok((socket.dev(), socket.ino()) == file),
                Err(error) if error.kind() == io::ErrorKind::NotFound => Ok(false),
                Err(error) => Err(error),
            });
            matched.process_results(|mut matched| matched.contains(&true))
        };
        let mut events = vec![KEvent::new(0, EventFilter::EVFILT_VNODE, EvFlags::empty(), FilterFlag::empty(), 0, 0); self.folders.len()];
        let count = self.queue.kevent(&[], &mut events, Some(*TimeSpec::new(0, 0).as_ref()))?;
        if count == 0 {
            return Err(io::ErrorKind::WouldBlock.into());
        }
        let woken = self.folders.iter().filter(|(_, ident, _)| events.iter().take(count).any(|event| event.ident() == *ident));
        let fresh = woken.map(|(path, _, _)| sockets(path)).flatten_ok().filter_ok(|(_, file)| self.seen.insert(*file));
        fresh.filter_map(|found| found.and_then(|(path, file)| Ok(held(file)?.then_some((path, file)))).transpose()).collect()
    }

    async fn swept(self, group: NonZero<u32>, exited: CancellationToken) -> Result<(), NEVec<Failure>> {
        let failed = |operation| move |source| Failure::Io { operation, source };
        let left = |(path, file): &(PathBuf, (u64, u64))| {
            let socket = match path.fs_err_symlink_metadata() {
                Err(error) if error.kind() == io::ErrorKind::NotFound => return Ok(()),
                stated => stated?,
            };
            if !socket.file_type().is_socket() || socket.uid() != Uid::effective().as_raw() || (socket.dev(), socket.ino()) != *file {
                return Ok(());
            }
            match Socket::connect(path) {
                Err(error) if error.kind() == io::ErrorKind::ConnectionRefused => fs_err::remove_file(path),
                Ok(_listening) => Ok(()),
                Err(error) => {
                    tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), path = %path.display(), "socket kept, its listener is unproven");
                    Ok(())
                }
            }
        };
        let registered = unsafe { AsyncFd::register_with_interest(self, Interest::READABLE) };
        let mut watch = registered.map_err(|refused| NEVec::new(failed(ProcessOperation::Watch)(refused.into_parts().1)))?;
        let mut found = Vec::new();
        let stopped = loop {
            let woke = tokio::select! {
                biased;
                () = exited.cancelled() => break None,
                ready = watch.readable_mut() => ready.map(|mut ready| ready.try_io(|watched| watched.get_mut().woke(group))),
            };
            match woke {
                Ok(Ok(Ok(bound))) => found.extend(bound),
                Ok(Ok(Err(error))) | Err(error) => break Some(error),
                Ok(Err(_blocked)) => {}
            }
        };
        exited.cancelled().await;
        let removed = found.iter().map(left).filter_map(Result::err).map(failed(ProcessOperation::Remove));
        NEVec::try_from_vec(Vec::from_iter(iter::chain(stopped.map(failed(ProcessOperation::Watch)), removed))).map_or(Ok(()), Err)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn grouped(command: impl Into<CommandWrap>, grace: Duration) -> CommandWrap {
    let mut command = command.into();
    cfg_select! {
        unix => command.wrap(ProcessGroup::leader()).wrap(KillOnParentDeath { grace }),
        windows => {{
            let _: Duration = grace;
            command.wrap(KillOnDrop).wrap(JobObject)
        }}
    };
    command
}

#[must_use]
pub fn detached(command: impl Into<CommandWrap>) -> CommandWrap {
    let mut command = command.into();
    cfg_select! {
        unix => command.wrap(ProcessSession),
        windows => command.wrap(CreationFlags((DETACHED_PROCESS | CREATE_NEW_PROCESS_GROUP).0)),
    };
    command
}

#[cfg(unix)]
fn sized(socket: &Socket, buffer: usize) -> io::Result<()> {
    let options = SockRef::from(socket);
    options.set_send_buffer_size(buffer)?;
    options.set_recv_buffer_size(buffer)
}

#[cfg(unix)]
pub fn standard(buffer: Option<usize>) -> io::Result<(impl AsyncRead + Send + Unpin + 'static, impl AsyncWrite + Send + Unpin + 'static)> {
    fn attached<P, B>(descriptor: impl AsFd, buffer: Option<usize>, piped: impl FnOnce(OwnedFd) -> io::Result<P>, blocking: B) -> io::Result<Either<UnixStream, Either<P, B>>> {
        let file = File::from(descriptor.as_fd().try_clone_to_owned()?);
        let kind = file.metadata()?.file_type();
        match (kind.is_socket(), kind.is_fifo()) {
            (true, _) => {
                let socket = Socket::from(OwnedFd::from(file));
                buffer.map(|buffer| sized(&socket, buffer)).transpose()?;
                socket.set_nonblocking(true)?;
                UnixStream::from_std(socket).map(Either::Left)
            }
            (false, true) => piped(OwnedFd::from(file)).map(|pipe| Either::Right(Either::Left(pipe))),
            (false, false) => Ok(Either::Right(Either::Right(blocking))),
        }
    }
    Ok((attached(io::stdin(), buffer, pipe::Receiver::from_owned_fd, tokio::io::stdin())?, attached(io::stdout(), buffer, pipe::Sender::from_owned_fd, tokio::io::stdout())?))
}

pub async fn tail<R>(reader: R, kept: usize, limit: u64, mut each: impl FnMut(&str)) -> VecDeque<String>
where R: AsyncRead + Unpin {
    let (mut reader, mut line, mut tail) = (BufReader::new(reader), Vec::new(), VecDeque::with_capacity(kept));
    loop {
        line.clear();
        match (&mut reader).take(limit).read_until(b'\n', &mut line).await {
            Ok(0) => break tail,
            Ok(_) => {
                let text = String::from_utf8_lossy(&line);
                each(text.trim_end());
                tail.push_back(text.trim_end().to_owned());
                tail.retain_back(kept);
            }
            Err(error) => {
                tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process output unreadable");
                break tail;
            }
        }
    }
}

#[cfg(target_vendor = "apple")]
fn sockets(folder: &Path) -> io::Result<Vec<(PathBuf, (u64, u64))>> {
    let user = Uid::effective().as_raw();
    let typed = fs_err::read_dir(folder)?.map(|entry| entry.and_then(|entry| Ok((entry.file_type()?, entry.path()))));
    let stated = typed.filter_ok(|(kind, _)| kind.is_socket()).map_ok(|(_, path)| (path.fs_err_symlink_metadata(), path));
    stated
        .filter_map(|stated| match stated {
            Ok((Ok(socket), path)) => (socket.uid() == user).then(|| Ok((path, (socket.dev(), socket.ino())))),
            Ok((Err(error), _)) if error.kind() == io::ErrorKind::NotFound => None,
            Ok((Err(error), _)) | Err(error) => Some(Err(error)),
        })
        .collect()
}

pub async fn run(
    command: impl Into<CommandWrap>,
    streams: Option<usize>,
    temporary: &[PathBuf],
    deadline: Option<Instant>,
    grace: Duration,
    stopping: &CancellationToken,
    attach: impl AsyncFnOnce(Streams) -> io::Result<()>,
) -> Result<ExitStatus, ProcessError> {
    let failed = |operation| move |source| Failure::Io { operation, source };
    let mut command = grouped(command, grace);
    command.wrap(KillOnDrop);
    let program = command.command().as_std().get_program().to_owned();
    let refused = |failure| ProcessError { program: program.clone(), process: None, failures: NEVec::new(failure) };
    if stopping.is_cancelled() {
        return Err(refused(Failure::Cancelled));
    }
    if deadline.is_some_and(|deadline| Instant::now() >= deadline) {
        return Err(refused(Failure::Deadline));
    }

    #[cfg(unix)]
    let linked = streams.map(|buffer| {
        let (near, far) = Socket::pair()?;
        sized(&near, buffer)?;
        sized(&far, buffer)?;
        near.set_nonblocking(true)?;
        command.command_mut().stdin(OwnedFd::from(far.try_clone()?)).stdout(OwnedFd::from(far));
        UnixStream::from_std(near).map(UnixStream::into_split)
    });
    #[cfg(unix)]
    let linked = linked.transpose().map_err(|source| refused(failed(ProcessOperation::Start)(source)))?;
    #[cfg(windows)]
    if streams.is_some() {
        command.command_mut().stdin(Stdio::piped()).stdout(Stdio::piped());
    }
    #[cfg(target_vendor = "apple")]
    let watch = {
        let given = command.command().as_std().get_envs().find_map(|(name, value)| (name == "TMPDIR").then_some(value).flatten()).map(PathBuf::from);
        Sockets::watch(iter::chain(temporary.iter().cloned(), given))
    };
    let mut child = Released(command.spawn().map_err(|source| refused(failed(ProcessOperation::Start)(source)))?);
    drop(command);
    let pid = child.0.id().and_then(NonZero::new).ok_or_else(|| refused(failed(ProcessOperation::Start)(io::Error::other("spawned process has no PID"))))?;
    #[cfg(target_vendor = "apple")]
    let sweep = CancellationToken::new();
    #[cfg(target_vendor = "apple")]
    let sweeping = watch.map(|watch| AbortOnDropHandle::new(task::spawn(watch.swept(pid, sweep.clone()))));
    let (stdout, stdin) = (child.0.stdout().take().map(|pipe| Box::pin(pipe) as Output), child.0.stdin().take().map(|pipe| Box::pin(pipe) as Input));
    #[cfg(unix)]
    let (stdout, stdin) = linked.map_or((stdout, stdin), |(reader, writer)| (Some(Box::pin(reader) as Output), Some(Box::pin(writer) as Input)));
    let mut attach = pin!(attach(Streams { id: pid, stdin, stdout, stderr: child.0.stderr().take() }));
    let (ended, attached) = {
        let exited = cfg_select! {
            unix => child.0.wait(),
            windows => child.0.inner_mut().wait(),
        };
        let mut ending = pin!(async {
            tokio::select! {
                biased;
                status = exited => status.map(drop).map_err(failed(ProcessOperation::Wait)),
                Some(()) = async {
                    sleep_until(deadline?).await;
                    Some(())
                } => Err(Failure::Deadline),
                () = stopping.cancelled() => Err(Failure::Cancelled),
            }
        });
        tokio::select! {
            biased;
            ended = &mut ending => (ended, None),
            attached = &mut attach => match attached {
                Ok(()) => (ending.await, Some(Ok(()))),
                Err(error) => (Ok(()), Some(Err(error))),
            },
        }
    };

    let exited = timeout(grace, child.0.wait()).await;
    #[cfg(unix)]
    let exited = if let Err(tokio::time::error::Elapsed { .. }) = &exited {
        if let Err(error) = child.0.signal(Signal::TERM.as_raw()) {
            tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(&error), "process group ended before its terminate signal");
        }
        timeout(grace, child.0.wait()).await
    } else {
        exited
    };
    let released = match child.0.start_kill() {
        Ok(()) => Ok(()),
        #[cfg(unix)]
        Err(error) if Errno::from_io_error(&error) == Some(Errno::SRCH) => Ok(()),
        Err(error) => Err(error),
    };
    let (reaped, attached) = tokio::join!(
        async {
            match exited {
                Ok(status) => status,
                Err(tokio::time::error::Elapsed { .. }) => timeout(grace, child.0.wait()).await?,
            }
        },
        async {
            match attached {
                Some(attached) => attached,
                None => timeout(grace, attach).await?,
            }
        }
    );
    let swept = cfg_select! {
        target_vendor = "apple" => {{
            sweep.cancel();
            match sweeping {
                Ok(sweeping) => sweeping.await.map_err(|error| NEVec::new(failed(ProcessOperation::Watch)(error.into()))).flatten(),
                Err(error) => Err(NEVec::new(failed(ProcessOperation::Watch)(error))),
            }
        }}
        _ => {{
            let _: &[PathBuf] = temporary;
            Ok::<(), NEVec<Failure>>(())
        }}
    };
    let unswept = swept.err().into_iter().flatten();
    let later = [ended.err(), released.err().map(failed(ProcessOperation::Release)), attached.err().map(failed(ProcessOperation::Attach))].into_iter().flatten().chain(unswept).collect::<Vec<_>>();
    let error = |status, failures| ProcessError { program, process: Some((pid, status)), failures };
    match reaped {
        Ok(status) => NEVec::try_from_vec(later).map_or(Ok(status), |failures| Err(error(Some(status), failures))),
        Err(source) => Err(error(None, NEVec::from((failed(ProcessOperation::Wait)(source), later)))),
    }
}

pub async fn login(directory: &Path, temporary: &[PathBuf], deadline: Option<Instant>, grace: Duration, stopping: &CancellationToken) -> Result<Vec<(OsString, OsString)>, ProcessError> {
    let refused = |program: &OsStr, source| ProcessError { program: program.to_owned(), process: None, failures: NEVec::new(Failure::Io { operation: ProcessOperation::Login, source }) };
    cfg_select! {
        unix => {{
            let user = Uid::effective();
            let read = User::from_uid(user).map_err(io::Error::from).and_then(|account| account.ok_or_else(|| io::ErrorKind::NotFound.into()));
            let account = read.map_err(|source| refused(OsStr::new(&format!("user {user}")), source))?;
            let marker = format!("{}-{:016x}", env!("CARGO_PKG_NAME"), RandomState::new().hash_one(process::id()));
            let mut shell = tokio::process::Command::new(&account.shell);
            shell.args(["-i", "-l", "-c", &format!("printf '%s\\0' {marker}; exec /usr/bin/env -0")]).current_dir(directory).env_clear();
            shell.envs([("HOME", account.dir.as_os_str()), ("USER", OsStr::new(&account.name)), ("LOGNAME", OsStr::new(&account.name)), ("SHELL", account.shell.as_os_str())]);
            shell.stdin(Stdio::null()).stdout(Stdio::piped()).stderr(Stdio::null());
            let mut printed = Vec::new();
            let attach = async |streams: Streams| match streams.stdout {
                Some(mut stdout) => stdout.read_to_end(&mut printed).await.map(drop),
                None => Err(io::ErrorKind::BrokenPipe.into()),
            };
            let status = run(shell, None, temporary, deadline, grace, stopping, attach).await?;
            let mut entries = printed.split(|byte| *byte == 0).skip_while(|entry| !entry.ends_with(marker.as_bytes()));
            let found = status.exit_ok().map_err(io::Error::other).and_then(|()| entries.next().map(drop).ok_or_else(|| io::ErrorKind::InvalidData.into()));
            found.map_err(|source| refused(account.shell.as_os_str(), source))?;
            Ok(entries.filter_map(|entry| entry.split_once(|byte| *byte == b'=')).map(|(name, value)| (OsStr::from_bytes(name).to_owned(), OsStr::from_bytes(value).to_owned())).collect())
        }}
        windows => {{
            let _: (&Path, &[PathBuf], Option<Instant>, Duration, &CancellationToken) = (directory, temporary, deadline, grace, stopping);
            let read = task::spawn_blocking(|| {
                let (mut token, mut block) = (HANDLE::default(), ptr::null_mut());
                unsafe { OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY | TOKEN_DUPLICATE, &raw mut token) }?;
                let token = unsafe { Owned::new(token) };
                unsafe { CreateEnvironmentBlock(&raw mut block, Some(*token), false) }?;
                let mut next = block.cast_const().cast::<u16>();
                let entries = iter::from_fn(|| {
                    let entry = unsafe { slice::from_raw_parts(next, PCWSTR(next).len()) };
                    next = unsafe { entry.as_ptr_range().end.add(1) };
                    (!entry.is_empty()).then_some(entry)
                });
                let variables = entries.filter_map(|entry| entry.split_once(|unit| *unit == u16::from(b'='))).map(|(name, value)| (OsString::from_wide(name), OsString::from_wide(value))).collect();
                unsafe { DestroyEnvironmentBlock(block) }?;
                Ok::<_, io::Error>(variables)
            });
            read.await.map_err(io::Error::from).flatten().map_err(|source| refused(OsStr::new("user"), source))
        }}
    }
}
