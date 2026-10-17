use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id_with_node_args,
    paths::{PathNS, find_crates},
    res::MaybeDef as _,
};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_span::{Span, Symbol};

use super::AsyncContext;
use crate::{item_path, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    calls: FxHashSet<DefId>,
    owners: FxHashSet<DefId>,
    streams: FxHashSet<DefId>,
    handles: FxHashSet<DefId>,
    async_context: AsyncContext,
    reported: FxHashSet<Span>,
}

lint!(Pass; BLOCKING_CALL_IN_ASYNC: "blocking `std`, `fs_err`, `futures`, and `tokio` calls inside an async body");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let calls = paths(
            tcx,
            PathNS::Value,
            &[
                "std::thread::sleep",
                "std::thread::park",
                "std::thread::park_timeout",
                "std::thread::scope",
                "std::thread::JoinHandle::join",
                "std::thread::ScopedJoinHandle::join",
                "std::sync::Barrier::wait",
                "std::sync::nonpoison::Condvar::wait",
                "std::sync::nonpoison::Condvar::wait_while",
                "std::sync::nonpoison::Condvar::wait_timeout",
                "std::sync::nonpoison::Condvar::wait_timeout_while",
                "std::sync::mpsc::Receiver::recv",
                "std::sync::mpsc::Receiver::recv_timeout",
                "std::sync::mpsc::SyncSender::send",
                "std::fs::set_times",
                "std::fs::set_times_nofollow",
                "std::fs::set_permissions_nofollow",
                "std::fs::File::sync_all",
                "std::fs::File::sync_data",
                "std::fs::File::set_len",
                "std::fs::File::lock",
                "fs_err::read_dir",
                "fs_err::File::open",
                "fs_err::File::create",
                "fs_err::File::create_new",
                "fs_err::File::sync_all",
                "fs_err::File::sync_data",
                "fs_err::File::set_len",
                "fs_err::File::metadata",
                "fs_err::File::try_clone",
                "fs_err::File::set_permissions",
                "fs_err::File::set_times",
                "fs_err::File::set_modified",
                "fs_err::File::lock",
                "fs_err::File::lock_shared",
                "fs_err::File::unlock",
                "fs_err::OpenOptions::open",
                "std::io::read_to_string",
                "std::io::copy",
                "std::io::Stdin::read_line",
                "std::io::Stdin::lines",
                "std::io::Stdin::lock",
                "std::io::Stdout::lock",
                "std::io::Stderr::lock",
                "std::net::ToSocketAddrs::to_socket_addrs",
                "std::net::TcpStream::connect",
                "std::net::TcpStream::connect_timeout",
                "std::net::TcpListener::accept",
                "std::net::UdpSocket::recv",
                "std::net::UdpSocket::recv_from",
                "std::net::UdpSocket::send",
                "std::net::UdpSocket::send_to",
                "std::process::Command::output",
                "std::process::Command::status",
                "std::process::Child::wait",
                "std::process::Child::wait_with_output",
                "futures::executor::block_on",
                "futures::executor::block_on_stream",
                "tokio::runtime::Runtime::block_on",
                "tokio::runtime::Handle::block_on",
                "tokio::sync::Mutex::blocking_lock",
                "tokio::sync::Mutex::blocking_lock_owned",
                "tokio::sync::RwLock::blocking_read",
                "tokio::sync::RwLock::blocking_write",
                "tokio::sync::mpsc::Sender::blocking_send",
                "tokio::sync::mpsc::Receiver::blocking_recv",
                "tokio::sync::mpsc::Receiver::blocking_recv_many",
                "tokio::sync::mpsc::UnboundedReceiver::blocking_recv",
                "tokio::sync::mpsc::UnboundedReceiver::blocking_recv_many",
                "tokio::sync::oneshot::Receiver::blocking_recv",
                "tokio::sync::broadcast::Receiver::blocking_recv",
            ],
        );
        let owners = paths(tcx, PathNS::Type, &["fs_err::PathExt"]).into_iter().chain(find_crates(tcx, Symbol::intern("fs_err")).iter().copied()).collect();
        let streams = paths(tcx, PathNS::Type, &["std::io::Read", "std::io::Write", "std::io::BufRead", "std::io::Seek"]);
        let handles = paths(
            tcx,
            PathNS::Type,
            &[
                "std::fs::File",
                "fs_err::File",
                "std::io::Stdin",
                "std::io::StdinLock",
                "std::io::Stdout",
                "std::io::StdoutLock",
                "std::io::Stderr",
                "std::io::StderrLock",
                "std::net::TcpStream",
                "std::net::TcpListener",
                "std::net::UdpSocket",
                "std::process::ChildStdin",
                "std::process::ChildStdout",
                "std::process::ChildStderr",
            ],
        );
        Self { calls, owners, streams, handles, async_context: AsyncContext::new(tcx), reported: FxHashSet::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some((callee, args)) = fn_def_id_with_node_args(cx, expr)
            && let parent = cx.tcx.parent(callee)
            && (self.calls.contains(&callee)
                || self.owners.contains(&parent)
                || self.streams.contains(&parent) && args.type_at(0).walk().any(|arg| arg.as_type().opt_def_id().is_some_and(|handle| self.handles.contains(&handle))))
            && self.async_context.is_async(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id))
            && let span = expr.span.source_callsite()
            && (!expr.span.from_expansion() || self.reported.insert(span))
        {
            span_lint_hir_and_then(cx, BLOCKING_CALL_IN_ASYNC, expr.hir_id, span, format!("`{}` blocks the worker thread polling its async body", item_path(cx, callee)), |diag| {
                diag.help("await an async counterpart, or run the call in `tokio::task::spawn_blocking`");
            });
        }
    }
}
