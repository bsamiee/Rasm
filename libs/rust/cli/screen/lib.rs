#![feature(control_flow_into_value, error_reporter, panic_update_hook, try_blocks, try_blocks_heterogeneous)]

use core::{error::Error, fmt, future, iter, ops::ControlFlow, pin::pin, time::Duration};
use std::{
    error::Report,
    io::{self, IsTerminal as _},
    panic, process,
};

use anstream::{AutoStream, ColorChoice};
use clap::{CommandFactory, FromArgMatches, builder::Styles};
#[cfg(unix)]
use crossterm::terminal::EnterAlternateScreen;
use crossterm::{
    cursor::Show,
    event::{DisableBracketedPaste, EnableBracketedPaste, Event, EventStream, KeyEvent, KeyEventKind, KeyEventState},
    execute, terminal,
};
use futures::{Stream, StreamExt as _, executor, stream};
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::{CommandError, Process};
use rasm_telemetry::Defaults;
use ratatui::{DefaultTerminal, Frame, layout::Rect};
#[cfg(unix)]
use rustix::process::{Signal, kill_current_process_group};
use sysexits::ExitCode;
use tokio::{
    task,
    time::{self, MissedTickBehavior},
};
use tokio_stream::wrappers::IntervalStream;
use tracing_subscriber::fmt::writer::MakeWriterExt as _;
use tui_logger::{LevelFilter, TuiTracingSubscriberLayer, TuiWidgetEvent, TuiWidgetState};

use crate::{
    keymap::{Action, Binding},
    view::Pane,
};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Program {
    type Action: Clone + fmt::Display;
    type Item;

    fn keys(&self) -> &[Binding<Self::Action>] { &[] }

    fn update(&mut self, input: Input<Self::Action, Self::Item>) -> ControlFlow<()>;

    fn view(&mut self, frame: &mut Frame<'_>, area: Rect);
}

#[derive(Clone, Copy, Debug, derive_more::Display)]
pub enum Operation {
    #[display("enter the screen")]
    Enter,
    #[display("read terminal input")]
    Read,
    #[display("draw the screen")]
    Draw,
    #[cfg(unix)]
    #[display("suspend the screen")]
    Suspend,
    #[display("leave the screen")]
    Leave,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
pub enum Input<K, I> {
    Action(K),
    Event(Event),
    Item(I),
    End,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum ScreenError {
    #[error("cannot {operation}")]
    Terminal { operation: Operation, source: io::Error },
    #[error("cannot draw the screen on a standard output that is no terminal")]
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

pub fn run<A, P, E, S>(defaults: Defaults, start: impl AsyncFnOnce(A) -> Result<(P, S), NEVec<E>> + Send + 'static) -> process::ExitCode
where
    A: CommandFactory + FromArgMatches + Send + 'static,
    P: Program,
    E: Error + From<CommandError> + From<ScreenError> + Into<ExitCode> + Send + 'static,
    S: Stream<Item = Result<P::Item, E>>,
{
    const REFRESH: Duration = Duration::from_millis(100);
    const BATCH: usize = 256;
    enum Arrival<I, E> {
        Event(io::Result<Event>),
        Item(Result<I, E>),
        End,
        Refresh,
    }
    struct Session<P, E> {
        terminal: DefaultTerminal,
        program: P,
        pane: Option<Pane>,
        logs: TuiWidgetState,
        failures: Vec<E>,
    }
    fn released() -> io::Result<()> { execute!(io::stdout(), DisableBracketedPaste, Show) }
    let checked = |mut session: Session<P, E>, operation: Operation, outcome: io::Result<()>| match outcome {
        Ok(()) => ControlFlow::Continue(session),
        Err(source) => {
            session.failures.push(E::from(ScreenError::Terminal { operation, source }));
            ControlFlow::Break(session)
        }
    };
    let updated = |mut session: Session<P, E>, input: Input<P::Action, P::Item>| match session.program.update(input) {
        ControlFlow::Continue(()) => ControlFlow::Continue(session),
        ControlFlow::Break(()) => ControlFlow::Break(session),
    };
    #[cfg(unix)]
    let suspended = move |mut session: Session<P, E>| {
        let resumed = try bikeshed io::Result<()> {
            ratatui::try_restore()?;
            released()?;
            kill_current_process_group(Signal::TSTP)?;
            terminal::enable_raw_mode()?;
            execute!(io::stdout(), EnterAlternateScreen, EnableBracketedPaste)?;
            session.terminal.clear()?
        };
        checked(session, Operation::Suspend, resumed)
    };
    let keyed = move |session: Session<P, E>, event: Event| {
        let toggled = |current: Session<P, E>, pane: Pane| ControlFlow::Continue(Session { pane: (current.pane != Some(pane)).then_some(pane), ..current });
        let paged = |current: Session<P, E>, page: TuiWidgetEvent| {
            current.logs.transition(page);
            ControlFlow::Continue(current)
        };
        let pressed = event.as_key_event().filter(|key| key.is_press() || key.is_repeat()).map(|key| KeyEvent { kind: KeyEventKind::Press, state: KeyEventState::NONE, ..key });
        let action =
            pressed.and_then(|key| keymap::resolved(session.program.keys()).into_iter().find(|row| keymap::active(&row.action, session.pane) && KeyEvent::from(row) == key)).map(|row| row.action);
        match action {
            Some(Action::Quit) => ControlFlow::Break(session),
            #[cfg(unix)]
            Some(Action::Suspend) => suspended(session),
            Some(Action::Logs) => toggled(session, Pane::Logs),
            Some(Action::Keys) => toggled(session, Pane::Keys),
            Some(Action::Older) => paged(session, TuiWidgetEvent::PrevPageKey),
            Some(Action::Newer) => paged(session, TuiWidgetEvent::NextPageKey),
            Some(Action::Newest) => paged(session, TuiWidgetEvent::EscapeKey),
            Some(Action::Program(action)) => updated(session, Input::Action(action)),
            None => updated(session, Input::Event(event)),
        }
    };
    let arrived = move |mut session: Session<P, E>, arrival: Arrival<P::Item, E>| match arrival {
        Arrival::Event(Ok(event)) => keyed(session, event),
        Arrival::Event(Err(source)) => checked(session, Operation::Read, Err(source)),
        Arrival::Item(Ok(item)) => updated(session, Input::Item(item)),
        Arrival::Item(Err(failure)) => {
            session.failures.push(failure);
            ControlFlow::Continue(session)
        }
        Arrival::End => updated(session, Input::End),
        Arrival::Refresh => ControlFlow::Continue(session),
    };
    let framed = move |mut session: Session<P, E>, styles: &Styles| {
        tui_logger::move_events();
        let shown = session.terminal.draw(|frame| view::drawn(frame, &mut session.program, session.pane, &session.logs, styles)).map(|_frame| ());
        checked(session, Operation::Draw, shown)
    };
    rasm_command::run(defaults, io::stderr.with_filter(|_| !terminal::is_raw_mode_enabled().is_ok_and(|raw| raw)), TuiTracingSubscriberLayer, async move |Process { args, mut signals, filter: _ }| {
        let styles = match AutoStream::choice(&io::stdout()) {
            ColorChoice::Never => Styles::plain(),
            ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => A::command().get_styles().clone(),
        };
        tui_logger::set_default_level(LevelFilter::Trace);
        task::spawn_blocking(move || {
            io::stdout().is_terminal().ok_or_else(|| NEVec::new(E::from(ScreenError::Detached)))?;
            let (program, items) = executor::block_on(start(args))?;
            let mut refresh = time::interval(REFRESH);
            refresh.set_missed_tick_behavior(MissedTickBehavior::Skip);
            panic::update_hook(|hook, info| {
                if let Err(error) = released() {
                    tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "cannot restore terminal");
                }
                hook(info);
            });
            let entered: io::Result<DefaultTerminal> = try {
                let terminal = ratatui::try_init()?;
                execute!(io::stdout(), EnableBracketedPaste)?;
                terminal
            };
            let mut arrivals = pin!(
                stream::select(
                    EventStream::new().map(Arrival::Event),
                    stream::select(items.map(Arrival::Item).chain(stream::once(future::ready(Arrival::End))), IntervalStream::new(refresh).map(|_| Arrival::Refresh))
                )
                .take_until(signals.next())
                .ready_chunks(BATCH)
            );
            let folded = entered.map_err(|source| ScreenError::Terminal { operation: Operation::Enter, source }).map(|terminal| {
                executor::block_on_stream(arrivals.as_mut()).try_fold(Session { terminal, program, pane: None, logs: TuiWidgetState::new(), failures: Vec::new() }, |session, chunk| {
                    framed(chunk.into_iter().try_fold(session, arrived)?, &styles)
                })
            });
            let failures = match folded.map(ControlFlow::into_value) {
                Ok(session) => session.failures,
                Err(entering) => vec![E::from(entering)],
            };
            let left = (try bikeshed io::Result<()> {
                ratatui::try_restore()?;
                released()?
            })
            .map_err(|source| ScreenError::Terminal { operation: Operation::Leave, source });
            NEVec::try_from_vec(iter::chain(failures, left.err().map(E::from)).collect()).map_or_else(|| Ok(if arrivals.get_ref().is_stopped() { ExitCode::TempFail } else { ExitCode::Ok }), Err)
        })
        .await
        .unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()))
    })
}

pub mod keymap;
mod view;
