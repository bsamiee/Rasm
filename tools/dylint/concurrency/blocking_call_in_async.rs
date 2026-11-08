use clippy_utils::{
    diagnostics::span_lint_hir,
    fn_def_id,
    paths::{PathNS, find_crates},
    res::MaybeDef as _,
    sym,
};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{CoroutineDesugaring, CoroutineKind, Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};
use rustc_span::{Span, Symbol};

use crate::{associated_trait, item_path, paths, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    calls: FxHashSet<DefId>,
    owners: FxHashSet<DefId>,
    streams: FxHashSet<DefId>,
    handles: FxHashSet<DefId>,
    forwarders: FxHashSet<DefId>,
    reported: FxHashSet<Span>,
}

lint!(Pass; BLOCKING_CALL_IN_ASYNC: "blocking `std`, `fs_err`, `futures`, and `tokio` calls inside an async body");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            calls: paths(
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
                    "fs_err::PathExt::fs_err_try_exists",
                    "fs_err::PathExt::fs_err_metadata",
                    "fs_err::PathExt::fs_err_symlink_metadata",
                    "fs_err::PathExt::fs_err_canonicalize",
                    "fs_err::PathExt::fs_err_read_link",
                    "fs_err::PathExt::fs_err_read_dir",
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
            ),
            owners: find_crates(tcx, Symbol::intern("fs_err")).iter().copied().chain(tcx.get_diagnostic_item(sym::File).map(|file| tcx.parent(file))).collect(),
            streams: [sym::IoRead, sym::IoWrite, sym::IoBufRead, sym::IoSeek].into_iter().filter_map(|blocking| tcx.get_diagnostic_item(blocking)).collect(),
            handles: paths(
                tcx,
                PathNS::Type,
                &[
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
            )
            .into_iter()
            .chain(tcx.get_diagnostic_item(sym::File))
            .collect(),
            forwarders: paths(tcx, PathNS::Type, &["std::io::BufReader", "std::io::BufWriter", "std::io::LineWriter", "std::io::Take", "std::io::Chain"]),
            reported: FxHashSet::default(),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if !matches!(expr.kind, ExprKind::MethodCall(..) | ExprKind::Call(..))
            || !matches!(
                cx.enclosing_body.and_then(|body| cx.tcx.coroutine_kind(cx.tcx.hir_body_owner_def_id(body))),
                Some(CoroutineKind::Desugared(CoroutineDesugaring::Async | CoroutineDesugaring::AsyncGen, _))
            )
        {
            return;
        }
        let Some(callee) = fn_def_id(cx, expr) else { return };
        let parent = associated_trait(cx.tcx, callee).unwrap_or_else(|| cx.tcx.parent(callee));
        if !self.calls.contains(&callee) && !self.owners.contains(&parent) {
            if !self.streams.contains(&parent) || result_parts(cx, cx.typeck_results().expr_ty(expr)).is_none() {
                return;
            }
            let (ExprKind::MethodCall(_, operand, ..) | ExprKind::Call(_, [operand, ..])) = expr.kind else { return };
            let mut walked = cx.typeck_results().expr_ty_adjusted(operand).walk();
            loop {
                let Some(argument) = walked.next() else { return };
                let held = argument.as_type();
                if held.and_then(Ty::opt_def_id).is_some_and(|handle| self.handles.contains(&handle)) {
                    break;
                }
                if !held.is_some_and(Ty::is_ref) && !held.and_then(Ty::ty_adt_def).is_some_and(|adt| adt.is_box() || adt.is_diag_item(cx, sym::Arc) || self.forwarders.contains(&adt.did())) {
                    walked.skip_current_subtree();
                }
            }
        }
        let span = expr.span.source_callsite();
        if !expr.span.from_expansion() || self.reported.insert(span) {
            span_lint_hir(cx, BLOCKING_CALL_IN_ASYNC, expr.hir_id, span, format!("blocking call to `{}` in async body", item_path(cx, callee)));
        }
    }
}
