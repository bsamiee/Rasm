#![feature(control_flow_into_value, error_reporter, panic_update_hook, try_blocks_heterogeneous)]

use core::{fmt, future, iter, ops::ControlFlow, pin::pin, time::Duration};
#[cfg(not(panic = "abort"))]
use std::thread;
use std::{
    error::Report,
    io::{self, IsTerminal as _, Write as _},
    panic, process,
    time::Instant,
};

use anstream::{AutoStream, ColorChoice};
use clap::{CommandFactory, FromArgMatches, builder::Styles};
#[cfg(unix)]
use crossterm::event::{KeyboardEnhancementFlags, PopKeyboardEnhancementFlags, PushKeyboardEnhancementFlags};
use crossterm::{
    cursor::Show,
    event::{DisableBracketedPaste, DisableFocusChange, DisableMouseCapture, EnableBracketedPaste, EnableFocusChange, EnableMouseCapture, Event, EventStream},
    execute, queue,
    terminal::{self, EnterAlternateScreen, LeaveAlternateScreen},
};
use futures::{
    Stream, StreamExt as _, executor,
    future::{Either, select},
    stream,
};
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::{CommandError, Process};
use rasm_telemetry::{Defaults, Failure};
use ratatui::{DefaultTerminal, Frame, Terminal, backend::CrosstermBackend, layout::Rect};
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
use validated::Validated;

use crate::keymap::{Action, Binding, Pane};

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Program {
    type Action: Clone + fmt::Display;
    type Item;
    type Error;

    const REFRESH: Duration = Duration::from_millis(100);
    const MODES: &'static [Mode] = &[];

    fn keys(&self) -> &[Binding<Self::Action>] { &[] }

    fn update(&mut self, input: Input<Self::Action, Self::Item>) -> ControlFlow<Result<(), Self::Error>>;

    fn view(&mut self, frame: &mut Frame<'_>, area: Rect);
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
    Terminal { operation: Operation, source: io::Error },
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

pub fn run<A, P, E, S>(defaults: Defaults, start: impl AsyncFnOnce(A) -> Result<(P, S), NEVec<E>> + Send + 'static) -> process::ExitCode
where
    A: CommandFactory + FromArgMatches + Send + 'static,
    P: Program,
    E: Failure + From<CommandError> + From<ScreenError> + From<P::Error> + Send + 'static,
    S: Stream<Item = Result<P::Item, E>>,
{
    const BATCH: usize = 256;
    #[derive(derive_more::IsVariant)]
    enum Arrival<I, E> {
        Event(io::Result<Event>),
        Item(Result<I, E>),
        End,
        Tick(Instant),
    }
    struct Session<P, E> {
        terminal: DefaultTerminal,
        program: P,
        pane: Option<Pane>,
        level: LevelFilter,
        logs: TuiWidgetState,
        failures: Vec<E>,
    }
    let raw = || terminal::is_raw_mode_enabled().is_ok_and(|enabled| enabled);
    let entered = || {
        terminal::enable_raw_mode()?;
        let mut out = io::stdout();
        queue!(out, EnterAlternateScreen, EnableBracketedPaste)?;
        #[cfg(unix)]
        queue!(out, PushKeyboardEnhancementFlags(KeyboardEnhancementFlags::DISAMBIGUATE_ESCAPE_CODES | KeyboardEnhancementFlags::REPORT_ALTERNATE_KEYS))?;
        P::MODES.iter().try_for_each(|mode| match mode {
            Mode::Mouse => queue!(out, EnableMouseCapture),
            Mode::Focus => queue!(out, EnableFocusChange),
        })?;
        out.flush()
    };
    let released = || {
        let cooked = terminal::disable_raw_mode();
        let mut out = io::stdout();
        let escaped = try bikeshed io::Result<()> {
            #[cfg(unix)]
            queue!(out, PopKeyboardEnhancementFlags)?;
            P::MODES.iter().try_for_each(|mode| match mode {
                Mode::Mouse => queue!(out, DisableMouseCapture),
                Mode::Focus => queue!(out, DisableFocusChange),
            })?;
            execute!(out, DisableBracketedPaste, Show, LeaveAlternateScreen)?
        };
        Validated::from(cooked).map2(Validated::from(escaped), |(), ()| ()).ok()
    };
    let failed = |operation: Operation| move |source: io::Error| ScreenError::Terminal { operation, source };
    let checked = move |mut session: Session<P, E>, operation: Operation, outcome: Result<(), NEVec<io::Error>>| match outcome {
        Ok(()) => ControlFlow::Continue(session),
        Err(sources) => {
            session.failures.extend(sources.into_iter().map(failed(operation)).map(E::from));
            ControlFlow::Break(session)
        }
    };
    let updated = |mut session: Session<P, E>, input: Input<P::Action, P::Item>| match session.program.update(input) {
        ControlFlow::Continue(()) => ControlFlow::Continue(session),
        ControlFlow::Break(ended) => {
            session.failures.extend(ended.err().map(E::from));
            ControlFlow::Break(session)
        }
    };
    #[cfg(unix)]
    let suspended = move |mut session: Session<P, E>| {
        let resumed = released().and_then(|()| {
            (try bikeshed io::Result<()> {
                kill_current_process_group(Signal::TSTP)?;
                entered()?;
                session.terminal.clear()?
            })
            .map_err(NEVec::new)
        });
        checked(session, Operation::Suspend, resumed)
    };
    let keyed = move |session: Session<P, E>, event: Event| {
        let toggled = |current: Session<P, E>, pane: Pane| ControlFlow::Continue(Session { pane: (current.pane != Some(pane)).then_some(pane), ..current });
        let paged = |current: Session<P, E>, page: TuiWidgetEvent| {
            current.logs.transition(page);
            ControlFlow::Continue(current)
        };
        let leveled = |current: Session<P, E>, step: fn(&LevelFilter) -> LevelFilter| {
            let level = step(&current.level);
            ControlFlow::Continue(Session { level, logs: current.logs.set_default_display_level(level), ..current })
        };
        let bound = event.as_key_event().and_then(|key| {
            let pressed = keymap::normalized(key.code, key.modifiers);
            keymap::resolved(session.program.keys())
                .find(|row| keymap::active(&row.action, session.pane) && keymap::normalized(row.code, row.modifiers) == pressed)
                .map(|row| (!key.is_release()).then(|| row.into_owned().action))
        });
        match bound {
            Some(None) => ControlFlow::Continue(session),
            Some(Some(Action::Quit)) => ControlFlow::Break(session),
            #[cfg(unix)]
            Some(Some(Action::Suspend)) => suspended(session),
            Some(Some(Action::Toggle(pane))) => toggled(session, pane),
            Some(Some(Action::Older)) => paged(session, TuiWidgetEvent::PrevPageKey),
            Some(Some(Action::Newer)) => paged(session, TuiWidgetEvent::NextPageKey),
            Some(Some(Action::Newest)) => paged(session, TuiWidgetEvent::EscapeKey),
            Some(Some(Action::Verbose)) => leveled(session, LevelFilter::increment_severity),
            Some(Some(Action::Quiet)) => leveled(session, LevelFilter::decrement_severity),
            Some(Some(Action::Program(action))) => updated(session, Input::Action(action)),
            None => updated(session, Input::Event(event)),
        }
    };
    let arrived = move |mut session: Session<P, E>, arrival: Arrival<P::Item, E>| match arrival {
        Arrival::Event(Ok(event)) => keyed(session, event),
        Arrival::Event(Err(source)) => checked(session, Operation::Read, Err(NEVec::new(source))),
        Arrival::Item(Ok(item)) => updated(session, Input::Item(item)),
        Arrival::Item(Err(failure)) => {
            session.failures.push(failure);
            ControlFlow::Continue(session)
        }
        Arrival::End => updated(session, Input::End),
        Arrival::Tick(instant) => updated(session, Input::Tick(instant)),
    };
    let framed = move |mut session: Session<P, E>, styles: &Styles| {
        let shown = session.terminal.draw(|frame| view::drawn(frame, &mut session.program, session.pane, &session.logs, styles)).map(|_frame| ()).map_err(NEVec::new);
        checked(session, Operation::Draw, shown)
    };
    let batched = move |session: Session<P, E>, chunk: Vec<Arrival<P::Item, E>>, styles: &Styles| {
        tui_logger::move_events();
        let drawn = !chunk.iter().all(Arrival::is_item);
        let folded = chunk.into_iter().try_fold(session, arrived)?;
        if drawn { framed(folded, styles) } else { ControlFlow::Continue(folded) }
    };
    rasm_command::run(defaults, io::stderr.with_filter(move |_| !raw()), TuiTracingSubscriberLayer, async move |Process { args, styles, mut signals, filter: _ }| {
        let styles = match AutoStream::choice(&io::stdout()) {
            ColorChoice::Never => Styles::plain(),
            ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => styles,
        };
        tui_logger::set_default_level(LevelFilter::Trace);
        task::spawn_blocking(move || {
            io::stdout().is_terminal().ok_or_else(|| NEVec::new(E::from(ScreenError::Detached)))?;
            let Either::Left((started, _)) = executor::block_on(select(pin!(start(args)), signals.next())) else {
                return Ok(ExitCode::TempFail);
            };
            let (program, items) = started?;
            let mut ticks = time::interval(P::REFRESH);
            ticks.set_missed_tick_behavior(MissedTickBehavior::Skip);
            #[cfg(not(panic = "abort"))]
            let owner = thread::current().id();
            panic::update_hook(move |hook, info| {
                if cfg_select! {
                    panic = "abort" => raw(),
                    _ => raw() && thread::current().id() == owner,
                } && let Err(sources) = released()
                {
                    sources.into_iter().map(failed(Operation::Leave)).for_each(|error| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(error), "terminal release failed"));
                }
                hook(info);
            });
            let entering = entered().and_then(|()| Terminal::new(CrosstermBackend::new(io::stdout())));
            let mut arrivals = pin!(
                stream::select(
                    EventStream::new().map(Arrival::Event),
                    stream::select(items.map(Arrival::Item).chain(stream::once(future::ready(Arrival::End))), IntervalStream::new(ticks).map(|instant| Arrival::Tick(instant.into_std())))
                )
                .take_until(signals.next())
                .ready_chunks(BATCH)
            );
            let failures = entering.map_or_else(
                |source| vec![E::from(failed(Operation::Enter)(source))],
                |terminal| {
                    executor::block_on_stream(arrivals.as_mut())
                        .try_fold(Session { terminal, program, pane: None, level: LevelFilter::Trace, logs: TuiWidgetState::new(), failures: Vec::new() }, |session, chunk| {
                            batched(session, chunk, &styles)
                        })
                        .into_value()
                        .failures
                },
            );
            let status = if arrivals.get_ref().is_stopped() { ExitCode::TempFail } else { ExitCode::Ok };
            let left = released().err().into_iter().flatten().map(failed(Operation::Leave)).map(E::from);
            NEVec::try_from_vec(iter::chain(failures, left).collect()).map_or(Ok(status), Err)
        })
        .await
        .unwrap_or_else(|failure| panic::resume_unwind(failure.into_panic()))
    })
}

pub mod keymap;
mod view;
