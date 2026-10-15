use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id_with_node_args, get_parent_expr,
    paths::{PathNS, find_crates},
    res::MaybeDef,
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{
    CoroutineDesugaring, CoroutineKind, Expr, ExprKind, Node,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{Binder, TyCtxt};
use rustc_span::{Span, Symbol};

use crate::{item_path, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Role {
    Call,
    Owner,
    Stream,
    Handle,
    Offload,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    BLOCKING_CALL_IN_ASYNC,
    Warn,
    "blocking std, futures, and tokio calls on the worker thread polling an async body"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct BlockingCallInAsync {
    roles: FxHashMap<DefId, Role>,
    reported: FxHashSet<Span>,
}

impl_lint_pass!(BlockingCallInAsync => [BLOCKING_CALL_IN_ASYNC]);

impl BlockingCallInAsync {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let roles = resolve(
            tcx,
            PathNS::Value,
            &[
                ("std::thread::sleep", Role::Call),
                ("std::thread::park", Role::Call),
                ("std::thread::park_timeout", Role::Call),
                ("std::thread::scope", Role::Call),
                ("std::thread::JoinHandle::join", Role::Call),
                ("std::thread::ScopedJoinHandle::join", Role::Call),
                ("std::thread::Scope::spawn", Role::Offload),
                ("std::sync::Barrier::wait", Role::Call),
                ("std::sync::Condvar::wait", Role::Call),
                ("std::sync::Condvar::wait_while", Role::Call),
                ("std::sync::Condvar::wait_timeout", Role::Call),
                ("std::sync::Condvar::wait_timeout_while", Role::Call),
                ("std::sync::mpsc::Receiver::recv", Role::Call),
                ("std::sync::mpsc::Receiver::recv_timeout", Role::Call),
                ("std::sync::mpsc::SyncSender::send", Role::Call),
                ("std::fs::set_times", Role::Call),
                ("std::fs::set_times_nofollow", Role::Call),
                ("std::fs::set_permissions_nofollow", Role::Call),
                ("std::fs::File::sync_all", Role::Call),
                ("std::fs::File::sync_data", Role::Call),
                ("std::fs::File::set_len", Role::Call),
                ("std::fs::File::lock", Role::Call),
                ("fs_err::read_dir", Role::Call),
                ("fs_err::File::open", Role::Call),
                ("fs_err::File::create", Role::Call),
                ("fs_err::File::create_new", Role::Call),
                ("fs_err::File::sync_all", Role::Call),
                ("fs_err::File::sync_data", Role::Call),
                ("fs_err::File::set_len", Role::Call),
                ("fs_err::File::metadata", Role::Call),
                ("fs_err::File::try_clone", Role::Call),
                ("fs_err::File::set_permissions", Role::Call),
                ("fs_err::File::set_times", Role::Call),
                ("fs_err::File::set_modified", Role::Call),
                ("fs_err::File::lock", Role::Call),
                ("fs_err::File::lock_shared", Role::Call),
                ("fs_err::File::unlock", Role::Call),
                ("fs_err::OpenOptions::open", Role::Call),
                ("std::io::read_to_string", Role::Call),
                ("std::io::copy", Role::Call),
                ("std::io::Stdin::read_line", Role::Call),
                ("std::io::Stdin::lines", Role::Call),
                ("std::io::Stdin::lock", Role::Call),
                ("std::io::Stdout::lock", Role::Call),
                ("std::io::Stderr::lock", Role::Call),
                ("std::net::ToSocketAddrs::to_socket_addrs", Role::Call),
                ("std::net::TcpStream::connect", Role::Call),
                ("std::net::TcpStream::connect_timeout", Role::Call),
                ("std::net::TcpListener::accept", Role::Call),
                ("std::net::UdpSocket::recv", Role::Call),
                ("std::net::UdpSocket::recv_from", Role::Call),
                ("std::net::UdpSocket::send", Role::Call),
                ("std::net::UdpSocket::send_to", Role::Call),
                ("std::process::Command::output", Role::Call),
                ("std::process::Command::status", Role::Call),
                ("std::process::Child::wait", Role::Call),
                ("std::process::Child::wait_with_output", Role::Call),
                ("futures::executor::block_on", Role::Call),
                ("futures::executor::block_on_stream", Role::Call),
                ("tokio::runtime::Runtime::block_on", Role::Call),
                ("tokio::runtime::Handle::block_on", Role::Call),
                ("tokio::sync::Mutex::blocking_lock", Role::Call),
                ("tokio::sync::Mutex::blocking_lock_owned", Role::Call),
                ("tokio::sync::RwLock::blocking_read", Role::Call),
                ("tokio::sync::RwLock::blocking_write", Role::Call),
                ("tokio::sync::mpsc::Sender::blocking_send", Role::Call),
                ("tokio::sync::mpsc::Receiver::blocking_recv", Role::Call),
                ("tokio::sync::mpsc::Receiver::blocking_recv_many", Role::Call),
                ("tokio::sync::mpsc::UnboundedReceiver::blocking_recv", Role::Call),
                ("tokio::sync::mpsc::UnboundedReceiver::blocking_recv_many", Role::Call),
                ("tokio::sync::oneshot::Receiver::blocking_recv", Role::Call),
                ("tokio::sync::broadcast::Receiver::blocking_recv", Role::Call),
            ],
        )
        .into_iter()
        .chain(resolve(
            tcx,
            PathNS::Type,
            &[
                ("fs_err::PathExt", Role::Owner),
                ("std::io::Read", Role::Stream),
                ("std::io::Write", Role::Stream),
                ("std::io::BufRead", Role::Stream),
                ("std::io::Seek", Role::Stream),
                ("std::fs::File", Role::Handle),
                ("fs_err::File", Role::Handle),
                ("std::io::Stdin", Role::Handle),
                ("std::io::StdinLock", Role::Handle),
                ("std::io::Stdout", Role::Handle),
                ("std::io::StdoutLock", Role::Handle),
                ("std::io::Stderr", Role::Handle),
                ("std::io::StderrLock", Role::Handle),
                ("std::net::TcpStream", Role::Handle),
                ("std::net::TcpListener", Role::Handle),
                ("std::net::UdpSocket", Role::Handle),
                ("std::process::ChildStdin", Role::Handle),
                ("std::process::ChildStdout", Role::Handle),
                ("std::process::ChildStderr", Role::Handle),
            ],
        ))
        .chain(find_crates(tcx, Symbol::intern("fs_err")).iter().map(|root| (*root, Role::Owner)))
        .collect();
        Self { roles, reported: FxHashSet::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for BlockingCallInAsync {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn polled(cx: &LateContext<'_>, roles: &FxHashMap<DefId, Role>, body: LocalDefId) -> bool {
            let offloaded = |closure: &Expr<'_>| {
                if let Some(call) = get_parent_expr(cx, closure)
                    && let ExprKind::Call(_, arguments) | ExprKind::MethodCall(_, _, arguments, _) = call.kind
                    && let Some(position) = arguments.iter().position(|argument| argument.hir_id == closure.hir_id)
                    && let Some((callee, _)) = fn_def_id_with_node_args(cx, call)
                {
                    let inputs = cx.tcx.fn_sig(callee).instantiate_identity().skip_normalization().skip_binder().inputs();
                    let input = inputs.len().checked_sub(arguments.len()).and_then(|receiver| inputs.get(receiver..)?.get(position));
                    matches!(roles.get(&callee), Some(Role::Offload))
                        || cx
                            .tcx
                            .clauses_of(callee)
                            .clauses
                            .iter()
                            .filter_map(|(clause, _)| clause.as_type_outlives_clause())
                            .map(Binder::skip_binder)
                            .any(|outlives| Some(&outlives.0) == input && outlives.1.is_static())
                } else {
                    false
                }
            };
            match cx.tcx.coroutine_kind(body) {
                Some(CoroutineKind::Desugared(CoroutineDesugaring::Async | CoroutineDesugaring::AsyncGen, _)) => true,
                Some(_) => false,
                None => matches!(cx.tcx.hir_node_by_def_id(body), Node::Expr(closure) if !offloaded(closure) && polled(cx, roles, cx.tcx.hir_enclosing_body_owner(closure.hir_id))),
            }
        }
        let role = |item: DefId| self.roles.get(&item).copied();
        if let Some((callee, args)) = fn_def_id_with_node_args(cx, expr)
            && (matches!(role(callee), Some(Role::Call))
                || matches!(role(cx.tcx.parent(callee)), Some(Role::Owner))
                || cx.tcx.trait_of_assoc(callee).is_some_and(|stream| matches!(role(stream), Some(Role::Stream)))
                    && args.type_at(0).walk().any(|arg| arg.as_type().and_then(MaybeDef::opt_def_id).is_some_and(|handle| matches!(role(handle), Some(Role::Handle)))))
            && polled(cx, &self.roles, cx.tcx.hir_enclosing_body_owner(expr.hir_id))
            && let span = expr.span.source_callsite()
            && (!expr.span.from_expansion() || self.reported.insert(span))
        {
            span_lint_hir_and_then(cx, BLOCKING_CALL_IN_ASYNC, expr.hir_id, span, format!("`{}` blocks the worker thread polling this async body", item_path(cx, callee)), |diag| {
                diag.help("await the async counterpart, or move the call into `tokio::task::spawn_blocking`");
            });
        }
    }
}
