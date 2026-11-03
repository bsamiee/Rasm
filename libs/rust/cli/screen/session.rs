use core::{mem, num::NonZero, ops::ControlFlow};
use std::{
    collections::VecDeque,
    error::Report,
    io::{self, Write},
    sync::{Arc, nonpoison::Mutex},
};

use clap::builder::Styles;
#[cfg(unix)]
use crossterm::event::{KeyboardEnhancementFlags, PopKeyboardEnhancementFlags, PushKeyboardEnhancementFlags};
use crossterm::{
    cursor::Show,
    event::{DisableBracketedPaste, DisableFocusChange, DisableMouseCapture, EnableBracketedPaste, EnableFocusChange, EnableMouseCapture, Event, KeyEvent},
    execute, queue,
    terminal::{self, EnterAlternateScreen, LeaveAlternateScreen},
};
use futures::future::Either;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_telemetry::Handle;
use ratatui::DefaultTerminal;
#[cfg(unix)]
use rustix::process::{Signal, kill_current_process_group};
use tracing_log::AsTrace as _;
use tracing_subscriber::filter::Directive;
use tui_logger::{LevelFilter, TuiWidgetEvent, TuiWidgetState};
use validated::Validated;

use crate::{
    Effects, Input, Mode, Operation, Program, ScreenError,
    keymap::{self, Action, Pane},
    view,
};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Clone)]
pub(crate) struct Held {
    pub(crate) lines: Arc<Mutex<VecDeque<Box<[u8]>>>>,
    pub(crate) depth: NonZero<usize>,
}

impl Write for Held {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        self.lines.with_mut(|lines| {
            if lines.len() == self.depth.get() {
                lines.pop_front();
            }
            lines.push_back(Box::from(buf));
        });
        Ok(buf.len())
    }

    fn flush(&mut self) -> io::Result<()> { Ok(()) }
}

pub(crate) struct Session<P>
where P: Program
{
    pub(crate) terminal: DefaultTerminal,
    pub(crate) program: P,
    pub(crate) pane: Option<Pane>,
    pub(crate) level: LevelFilter,
    pub(crate) logs: TuiWidgetState,
    pub(crate) filter: Handle,
    pub(crate) effects: Effects<P>,
    pub(crate) held: Held,
    pub(crate) failures: Vec<P::Error>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn raw() -> bool { terminal::is_raw_mode_enabled().is_ok_and(|enabled| enabled) }

pub(crate) fn entered<P>() -> io::Result<()>
where P: Program {
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
}

pub(crate) fn released<P>(held: &Held) -> Result<(), NEVec<io::Error>>
where P: Program {
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
    let delivered = held.lines.replace(VecDeque::new()).iter().try_for_each(|line| io::stderr().write_all(line));
    Validated::from(cooked).map3(Validated::from(escaped), Validated::from(delivered), |(), (), ()| ()).ok()
}

pub(crate) fn failed(operation: Operation) -> impl Fn(io::Error) -> ScreenError { move |source| ScreenError::Terminal { operation, source } }

pub(crate) fn left<P>(held: &Held) -> impl Iterator<Item = ScreenError>
where P: Program {
    raw().then(|| released::<P>(held)).and_then(Result::err).into_iter().flatten().map(failed(Operation::Leave))
}

pub(crate) fn reloaded(filter: &Handle, level: LevelFilter) {
    if let Err(error) = filter.modify(|filter| *filter = mem::take(filter).add_directive(Directive::from(level.as_trace()))) {
        tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "tracing filter reload failed");
    }
}

fn ended<P>(mut session: Session<P>, operation: Operation, outcome: Result<(), NEVec<io::Error>>) -> ControlFlow<Session<P>, Session<P>>
where P: Program {
    match outcome {
        Ok(()) => ControlFlow::Continue(session),
        Err(sources) => {
            session.failures.extend(sources.into_iter().map(failed(operation)).map(P::Error::from));
            ControlFlow::Break(session)
        }
    }
}

fn updated<P>(mut session: Session<P>, input: Input<P::Action, P::Item>) -> ControlFlow<Session<P>, Session<P>>
where P: Program {
    match session.program.update(input, &session.effects) {
        ControlFlow::Continue(()) => ControlFlow::Continue(session),
        ControlFlow::Break(outcome) => {
            session.failures.extend(outcome.err());
            ControlFlow::Break(session)
        }
    }
}

#[cfg(unix)]
fn suspended<P>(mut session: Session<P>) -> ControlFlow<Session<P>, Session<P>>
where P: Program {
    let resumed = released::<P>(&session.held).and_then(|()| {
        (try bikeshed io::Result<()> {
            kill_current_process_group(Signal::TSTP)?;
            entered::<P>()?;
            session.terminal.clear()?
        })
        .map_err(NEVec::new)
    });
    ended(session, Operation::Suspend, resumed)
}

fn leveled<P>(session: Session<P>, step: fn(&LevelFilter) -> LevelFilter) -> ControlFlow<Session<P>, Session<P>>
where P: Program {
    let level = step(&session.level);
    reloaded(&session.filter, level);
    ControlFlow::Continue(Session { level, logs: session.logs.set_default_display_level(level), ..session })
}

fn keyed<P>(session: Session<P>, event: Event) -> ControlFlow<Session<P>, Session<P>>
where P: Program {
    let paged = |current: Session<P>, page: TuiWidgetEvent| {
        current.logs.transition(page);
        ControlFlow::Continue(current)
    };
    let bound = event.as_key_event().filter(|key| !key.is_release()).and_then(|key| {
        let pressed = keymap::normalized(key);
        keymap::resolved(session.program.keys()).find(|row| keymap::active(&row.action, session.pane) && KeyEvent::from(&**row) == pressed).map(|row| row.into_owned().action)
    });
    match bound {
        Some(Action::Quit) => ControlFlow::Break(session),
        #[cfg(unix)]
        Some(Action::Suspend) => suspended(session),
        Some(Action::Toggle(pane)) => ControlFlow::Continue(Session { pane: (session.pane != Some(pane)).then_some(pane), ..session }),
        Some(Action::Older) => paged(session, TuiWidgetEvent::PrevPageKey),
        Some(Action::Newer) => paged(session, TuiWidgetEvent::NextPageKey),
        Some(Action::Newest) => paged(session, TuiWidgetEvent::EscapeKey),
        Some(Action::Verbose) => leveled(session, LevelFilter::increment_severity),
        Some(Action::Quiet) => leveled(session, LevelFilter::decrement_severity),
        Some(Action::Program(action)) => updated(session, Input::Action(action)),
        None => updated(session, Input::Event(event)),
    }
}

pub(crate) fn batched<P>(
    session: Session<P>,
    inputs: impl IntoIterator<Item = Result<Input<P::Action, P::Item>, Either<io::Error, P::Error>>>,
    styles: &Styles,
) -> ControlFlow<Session<P>, Session<P>>
where
    P: Program,
{
    let mut folded = inputs.into_iter().try_fold(session, |mut session, input| match input {
        Ok(Input::Event(event)) => keyed(session, event),
        Ok(input @ (Input::Action(_) | Input::Item(_) | Input::Tick(_) | Input::End)) => updated(session, input),
        Err(Either::Left(source)) => ended(session, Operation::Read, Err(NEVec::new(source))),
        Err(Either::Right(failure)) => {
            session.failures.push(failure);
            ControlFlow::Continue(session)
        }
    })?;
    tui_logger::move_events();
    let shown = folded.terminal.draw(|frame| view::drawn(frame, &mut folded.program, folded.pane, &folded.logs, styles)).map(|_frame| ()).map_err(NEVec::new);
    ended(folded, Operation::Draw, shown)
}
