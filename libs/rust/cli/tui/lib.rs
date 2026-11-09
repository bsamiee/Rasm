#![feature(control_flow_into_value, error_reporter, integer_casts, lock_value_accessors, nonpoison_mutex, panic_update_hook, sync_nonpoison, try_blocks_heterogeneous)]

use core::{fmt, future, iter, num::NonZero, ops::ControlFlow, pin::pin, time::Duration};
#[cfg(not(panic = "abort"))]
use std::thread;
use std::{
    error::Report,
    io::{self, IsTerminal as _},
    panic, process,
    sync::{Arc, nonpoison::Mutex},
    time::Instant,
};

use anstream::{AutoStream, ColorChoice};
use clap::{CommandFactory, FromArgMatches, builder::Styles};
use crossterm::event::{Event, EventStream};
use futures::{
    Stream, StreamExt as _,
    channel::mpsc::{self, UnboundedSender},
    executor,
    future::{Either, select},
    stream::{self, LocalBoxStream},
};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::{
    CommandError, Process,
    telemetry::{Defaults, Failure, Handle, Layer},
};
use ratatui::{Frame, Terminal, backend::CrosstermBackend, layout::Rect};
use sysexits::ExitCode;
use tokio::{
    task,
    time::{self, MissedTickBehavior},
};
use tokio_stream::wrappers::IntervalStream;
use tracing_log::AsLog as _;
use tui_logger::{TuiTracingSubscriberLayer, TuiWidgetState};

use crate::session::{Held, Session};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Effects<P> = UnboundedSender<LocalBoxStream<'static, Result<<P as Program>::Item, <P as Program>::Error>>>;

pub trait Program {
    type Action: Clone + fmt::Display;
    type Item: 'static;
    type Error: Failure + From<CommandError> + From<ScreenError> + Send + 'static;

    const REFRESH: Duration = Duration::from_millis(100);
    const HISTORY: NonZero<usize> = NonZero::new(10_000).unwrap();
    const MODES: &'static [Mode] = &[];

    fn keys(&self) -> &[Binding<Self::Action>] { &[] }

    fn update(&mut self, input: Input<Self::Action, Self::Item>, effects: &Effects<Self>) -> ControlFlow<Result<(), Self::Error>>;

    fn view(&mut self, frame: &mut Frame<'_>, area: Rect, styles: &Styles);
}

#[derive(Clone, Copy, Debug, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Operation {
    Enter,
    #[display("read input from")]
    Read,
    Draw,
    #[cfg(unix)]
    Suspend,
    Leave,
}

#[derive(Clone, Copy, Debug)]
pub enum Mode {
    Mouse,
    Focus,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub enum Input<K, I> {
    Action(K),
    Event(Event),
    Item(I),
    Tick(Instant),
    End,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ScreenError {
    #[error("cannot {operation} the screen")]
    Terminal {
        operation: Operation,
        source: io::Error,
    },
    #[error("cannot {} the screen, standard output is not a terminal", Operation::Draw)]
    Detached,
}

impl From<ScreenError> for ExitCode {
    fn from(value: ScreenError) -> Self {
        match value {
            ScreenError::Terminal { operation: _, source } => source.into(),
            ScreenError::Detached => Self::Usage,
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, L, P, S>(defaults: Defaults, layer: L, start: impl AsyncFnOnce(A, Handle) -> Result<(P, S), NEVec<P::Error>> + Send + 'static) -> process::ExitCode
where
    A: CommandFactory + FromArgMatches + Send + 'static,
    L: Layer,
    P: Program,
    S: Stream<Item = Result<P::Item, P::Error>>,
{
    const BATCH: usize = 256;
    let held = Arc::new(Held { lines: Mutex::default(), depth: P::HISTORY });
    rasm_command::run(defaults, Arc::clone(&held), TuiTracingSubscriberLayer.and_then(layer), async move |Process { args, styles, mut signals, filter, level }| {
        let styles = match AutoStream::choice(&io::stdout()) {
            ColorChoice::Never => Styles::plain(),
            ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => styles,
        };
        let level = level.as_log();
        tui_logger::set_default_level(tui_logger::LevelFilter::Trace);
        tui_logger::set_buffer_depth(P::HISTORY.get());
        task::spawn_blocking(move || {
            io::stdout().is_terminal().ok_or_else(|| NEVec::new(P::Error::from(ScreenError::Detached)))?;
            let Either::Left((started, _)) = executor::block_on(select(pin!(start(args, filter.clone())), signals.next())) else {
                return Ok(ExitCode::TempFail);
            };
            let (program, items) = started?;
            let mut ticks = time::interval(
                const {
                    assert!(!P::REFRESH.is_zero(), "refresh period is zero, set `Program::REFRESH` to a nonzero duration");
                    P::REFRESH
                },
            );
            ticks.set_missed_tick_behavior(MissedTickBehavior::Skip);
            #[cfg(not(panic = "abort"))]
            let owner = thread::current().id();
            let hooked = Arc::clone(&held);
            panic::update_hook(move |hook, info| {
                if cfg_select! {
                    panic = "abort" => true,
                    _ => thread::current().id() == owner,
                } {
                    session::left(P::MODES, &hooked).for_each(|error| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "terminal release failed"));
                }
                hook(info);
            });
            let entering = try bikeshed io::Result<Terminal<CrosstermBackend<io::Stdout>>> {
                session::entered(P::MODES, &held)?;
                Terminal::new(CrosstermBackend::new(io::stdout()))?
            };
            let (effects, requested) = mpsc::unbounded();
            let delivered = |item: Result<P::Item, P::Error>| item.map(Input::Item).map_err(Either::Right);
            let mut inputs = pin!(
                stream::select(
                    EventStream::new().map(|event| event.map(Input::Event).map_err(Either::Left)),
                    stream::select(
                        stream::select(items.map(delivered).chain(stream::once(future::ready(Ok(Input::End)))), requested.flatten_unordered(None).map(delivered)),
                        IntervalStream::new(ticks).map(|instant| Ok(Input::Tick(instant.into_std()))),
                    ),
                )
                .take_until(signals.next())
                .ready_chunks(BATCH)
            );
            let failures = match entering {
                Err(source) => vec![P::Error::from(session::failed(Operation::Enter)(source))],
                Ok(terminal) => {
                    let Session { terminal: _, program: _, pane: _, level: stepped, logs: _, filter, effects: _, held: _, failures } = executor::block_on_stream(inputs.as_mut())
                        .try_fold(
                            Session {
                                terminal,
                                program,
                                pane: None,
                                level,
                                logs: TuiWidgetState::new().set_default_display_level(level),
                                filter,
                                effects,
                                held: Arc::clone(&held),
                                failures: Vec::new(),
                            },
                            |session, batch| session::batched(session, batch, &styles),
                        )
                        .into_value();
                    if stepped != level {
                        session::reloaded(&filter, level);
                    }
                    failures
                }
            };
            let status = if inputs.get_ref().is_stopped() { ExitCode::TempFail } else { ExitCode::Ok };
            iter::chain(failures, session::left(P::MODES, &held).map(P::Error::from)).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>).map_or(Ok(status), Err)
        })
        .await
        .unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()))
    })
}

mod keymap;
mod session;
mod view;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use keymap::{Action, Binding, Pane};
pub use clap;
pub use crossterm;
pub use futures;
pub use nonempty_collections;
pub use rasm_command;
pub use ratatui;
pub use sysexits;
