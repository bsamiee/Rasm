use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id_with_node_args,
    paths::{PathNS, find_crates, lookup_path},
    res::MaybeDef as _,
    source::snippet,
    sym,
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_span::{Span, Symbol};

use super::AsyncContext;
use crate::{impl_owner, item_path, path, resolve};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Counterpart<T> {
    Async(T),
    Buffered(T),
    Awaited,
    Offloaded,
}

impl<T> Counterpart<T> {
    fn and_then<U, F>(self, found: F) -> Counterpart<U>
    where F: FnOnce(T) -> Option<U> {
        match self {
            Self::Async(named) => found(named).map_or(Counterpart::Offloaded, Counterpart::Async),
            Self::Buffered(named) => found(named).map_or(Counterpart::Offloaded, Counterpart::Buffered),
            Self::Awaited => Counterpart::Awaited,
            Self::Offloaded => Counterpart::Offloaded,
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    calls: FxHashMap<DefId, Counterpart<DefId>>,
    owners: FxHashSet<DefId>,
    streams: FxHashMap<DefId, Counterpart<DefId>>,
    handles: FxHashMap<DefId, Option<DefId>>,
    async_context: AsyncContext,
    reported: FxHashSet<Span>,
}

lint!(Pass; BLOCKING_CALL_IN_ASYNC: "blocking `std`, `fs_err`, `futures`, and `tokio` calls inside an async body");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        use Counterpart::{Async, Awaited, Buffered, Offloaded};
        let (notified, received, sent) = (Async("tokio::sync::Notify::notified"), Async("tokio::sync::mpsc::Receiver::recv"), Async("tokio::sync::mpsc::Sender::send"));
        let (read_dir, connected, accepted) = (Async("fs_err::tokio::read_dir"), Async("tokio::net::TcpStream::connect"), Async("tokio::net::TcpListener::accept"));
        let calls = resolve(
            tcx,
            PathNS::Value,
            &[
                ("std::thread::sleep", Async("tokio::time::sleep")),
                ("std::thread::park", notified),
                ("std::thread::park_timeout", notified),
                ("std::thread::scope", Offloaded),
                ("std::thread::JoinHandle::join", Offloaded),
                ("std::thread::ScopedJoinHandle::join", Offloaded),
                ("std::sync::Barrier::wait", Async("tokio::sync::Barrier::wait")),
                ("std::sync::nonpoison::Condvar::wait", notified),
                ("std::sync::nonpoison::Condvar::wait_while", notified),
                ("std::sync::nonpoison::Condvar::wait_timeout", notified),
                ("std::sync::nonpoison::Condvar::wait_timeout_while", notified),
                ("std::sync::mpsc::Receiver::recv", received),
                ("std::sync::mpsc::Receiver::recv_timeout", received),
                ("std::sync::mpsc::Receiver::iter", received),
                ("std::sync::mpsc::SyncSender::send", sent),
                ("std::fs::set_times", Offloaded),
                ("std::fs::set_times_nofollow", Offloaded),
                ("std::fs::set_permissions_nofollow", Offloaded),
                ("std::fs::File::sync_all", Async("tokio::fs::File::sync_all")),
                ("std::fs::File::sync_data", Async("tokio::fs::File::sync_data")),
                ("std::fs::File::set_len", Async("tokio::fs::File::set_len")),
                ("std::fs::File::lock", Offloaded),
                ("fs_err::read_dir", read_dir),
                ("fs_err::File::open", Async("fs_err::tokio::File::open")),
                ("fs_err::File::create", Async("fs_err::tokio::File::create")),
                ("fs_err::File::create_new", Async("fs_err::tokio::File::create_new")),
                ("fs_err::File::sync_all", Async("fs_err::tokio::File::sync_all")),
                ("fs_err::File::sync_data", Async("fs_err::tokio::File::sync_data")),
                ("fs_err::File::set_len", Async("fs_err::tokio::File::set_len")),
                ("fs_err::File::metadata", Async("fs_err::tokio::File::metadata")),
                ("fs_err::File::try_clone", Async("fs_err::tokio::File::try_clone")),
                ("fs_err::File::set_permissions", Async("fs_err::tokio::File::set_permissions")),
                ("fs_err::File::set_times", Offloaded),
                ("fs_err::File::set_modified", Offloaded),
                ("fs_err::File::lock", Offloaded),
                ("fs_err::File::lock_shared", Offloaded),
                ("fs_err::File::unlock", Offloaded),
                ("fs_err::OpenOptions::open", Async("fs_err::tokio::OpenOptions::open")),
                ("fs_err::PathExt::fs_err_try_exists", Async("fs_err::tokio::try_exists")),
                ("fs_err::PathExt::fs_err_metadata", Async("fs_err::tokio::metadata")),
                ("fs_err::PathExt::fs_err_symlink_metadata", Async("fs_err::tokio::symlink_metadata")),
                ("fs_err::PathExt::fs_err_canonicalize", Async("fs_err::tokio::canonicalize")),
                ("fs_err::PathExt::fs_err_read_link", Async("fs_err::tokio::read_link")),
                ("fs_err::PathExt::fs_err_read_dir", read_dir),
                ("std::io::read_to_string", Async("tokio::io::AsyncReadExt::read_to_string")),
                ("std::io::copy", Async("tokio::io::copy")),
                ("std::io::Stdin::read_line", Buffered("tokio::io::AsyncBufReadExt::read_line")),
                ("std::io::Stdin::lines", Buffered("tokio::io::AsyncBufReadExt::lines")),
                ("std::io::Stdin::lock", Offloaded),
                ("std::io::Stdout::lock", Offloaded),
                ("std::io::Stderr::lock", Offloaded),
                ("std::net::ToSocketAddrs::to_socket_addrs", Async("tokio::net::lookup_host")),
                ("std::net::TcpStream::connect", connected),
                ("std::net::TcpStream::connect_timeout", connected),
                ("std::net::TcpListener::accept", accepted),
                ("std::net::TcpListener::incoming", accepted),
                ("std::net::UdpSocket::recv", Async("tokio::net::UdpSocket::recv")),
                ("std::net::UdpSocket::recv_from", Async("tokio::net::UdpSocket::recv_from")),
                ("std::net::UdpSocket::send", Async("tokio::net::UdpSocket::send")),
                ("std::net::UdpSocket::send_to", Async("tokio::net::UdpSocket::send_to")),
                ("std::process::Command::output", Async("tokio::process::Command::output")),
                ("std::process::Command::status", Async("tokio::process::Command::status")),
                ("std::process::Child::wait", Async("tokio::process::Child::wait")),
                ("std::process::Child::wait_with_output", Async("tokio::process::Child::wait_with_output")),
                ("futures::executor::block_on", Awaited),
                ("futures::executor::block_on_stream", Async("futures::stream::StreamExt::next")),
                ("tokio::runtime::Runtime::block_on", Awaited),
                ("tokio::runtime::Handle::block_on", Awaited),
                ("tokio::sync::Mutex::blocking_lock", Async("tokio::sync::Mutex::lock")),
                ("tokio::sync::Mutex::blocking_lock_owned", Async("tokio::sync::Mutex::lock_owned")),
                ("tokio::sync::RwLock::blocking_read", Async("tokio::sync::RwLock::read")),
                ("tokio::sync::RwLock::blocking_write", Async("tokio::sync::RwLock::write")),
                ("tokio::sync::mpsc::Sender::blocking_send", sent),
                ("tokio::sync::mpsc::Receiver::blocking_recv", received),
                ("tokio::sync::mpsc::Receiver::blocking_recv_many", Async("tokio::sync::mpsc::Receiver::recv_many")),
                ("tokio::sync::mpsc::UnboundedReceiver::blocking_recv", Async("tokio::sync::mpsc::UnboundedReceiver::recv")),
                ("tokio::sync::mpsc::UnboundedReceiver::blocking_recv_many", Async("tokio::sync::mpsc::UnboundedReceiver::recv_many")),
                ("tokio::sync::oneshot::Receiver::blocking_recv", Awaited),
                ("tokio::sync::broadcast::Receiver::blocking_recv", Async("tokio::sync::broadcast::Receiver::recv")),
            ],
        )
        .into_iter()
        .map(|(call, counterpart)| (call, counterpart.and_then(|named| path(tcx, PathNS::Value, named))))
        .collect();
        let streams = resolve(
            tcx,
            PathNS::Type,
            &[
                ("std::io::Read", Async("tokio::io::AsyncReadExt")),
                ("std::io::Write", Async("tokio::io::AsyncWriteExt")),
                ("std::io::BufRead", Buffered("tokio::io::AsyncBufReadExt")),
                ("std::io::Seek", Async("tokio::io::AsyncSeekExt")),
            ],
        )
        .into_iter()
        .map(|(blocking, counterpart)| (blocking, counterpart.and_then(|named| path(tcx, PathNS::Type, named))))
        .collect();
        let (stdin, stdout, stderr) = ("tokio::io::Stdin", "tokio::io::Stdout", "tokio::io::Stderr");
        let handles = resolve(
            tcx,
            PathNS::Type,
            &[
                ("std::fs::File", "tokio::fs::File"),
                ("fs_err::File", "fs_err::tokio::File"),
                ("std::io::Stdin", stdin),
                ("std::io::StdinLock", stdin),
                ("std::io::Stdout", stdout),
                ("std::io::StdoutLock", stdout),
                ("std::io::Stderr", stderr),
                ("std::io::StderrLock", stderr),
                ("std::net::TcpStream", "tokio::net::TcpStream"),
                ("std::net::TcpListener", "tokio::net::TcpListener"),
                ("std::net::UdpSocket", "tokio::net::UdpSocket"),
                ("std::process::ChildStdin", "tokio::process::ChildStdin"),
                ("std::process::ChildStdout", "tokio::process::ChildStdout"),
                ("std::process::ChildStderr", "tokio::process::ChildStderr"),
            ],
        )
        .into_iter()
        .map(|(blocking, counterpart)| (blocking, path(tcx, PathNS::Type, counterpart)))
        .collect();
        let owners = find_crates(tcx, Symbol::intern("fs_err")).iter().copied().collect();
        Self { calls, owners, streams, handles, async_context: AsyncContext::new(tcx), reported: FxHashSet::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let Some((callee, args)) = fn_def_id_with_node_args(cx, expr) else { return };
        let (ExprKind::MethodCall(_, receiver, arguments, _) | ExprKind::Call(receiver, arguments)) = expr.kind else { return };
        let (parent, name) = (cx.tcx.parent(callee), cx.tcx.item_name(callee));
        let counterpart = match self.calls.get(&callee) {
            Some(&counterpart @ Counterpart::Buffered(_)) => Some((counterpart, impl_owner(cx.tcx, callee).opt_def_id().and_then(|owner| self.handles.get(&owner).copied().flatten()))),
            Some(&counterpart) => Some((counterpart, None)),
            None if self.owners.contains(&parent) =>
                Some((lookup_path(cx.tcx, PathNS::Value, &[Symbol::intern("fs_err"), sym::tokio, name]).first().map_or(Counterpart::Offloaded, |&method| Counterpart::Async(method)), None)),
            None =>
                try {
                    let &ext = self.streams.get(&parent)?;
                    let &handle = args.type_at(0).walk().find_map(|arg| self.handles.get(&arg.as_type()?.opt_def_id()?))?;
                    handle.map_or((Counterpart::Offloaded, None), |handle| (ext.and_then(|ext| Some(cx.tcx.associated_items(ext).filter_by_name_unhygienic(name).next()?.def_id)), Some(handle)))
                },
        };
        if let Some((counterpart, handle)) = counterpart
            && self.async_context.is_async(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id))
            && let span = expr.span.source_callsite()
            && (!expr.span.from_expansion() || self.reported.insert(span))
        {
            span_lint_hir_and_then(cx, BLOCKING_CALL_IN_ASYNC, expr.hir_id, span, format!("blocking call to `{}` in async body", item_path(cx, callee)), |diag| {
                diag.help(match (counterpart, handle) {
                    (Counterpart::Async(method), None) => format!("await `{}`", item_path(cx, method)),
                    (Counterpart::Async(method), Some(handle)) => format!("await `{}` on `{}`", item_path(cx, method), cx.tcx.def_path_str(handle)),
                    (Counterpart::Buffered(method), Some(handle)) => format!("await `{}` on `tokio::io::BufReader<{}>`", item_path(cx, method), cx.tcx.def_path_str(handle)),
                    (Counterpart::Awaited, _) => format!("await `{}`", snippet(cx, arguments.last().unwrap_or(receiver).span, "..")),
                    (Counterpart::Buffered(_), None) | (Counterpart::Offloaded, _) => String::from("await the call through `tokio::task::spawn_blocking`"),
                });
            });
        }
    }
}
