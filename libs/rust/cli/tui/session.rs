use core::num::NonZero;
use core::ops::ControlFlow;
use core::{iter, mem};
use std::collections::VecDeque;
use std::error::Report;
use std::io::{self, Write};
use std::sync::Arc;
use std::sync::nonpoison::Mutex;

use clap::builder::Styles;
use crossterm::cursor::Show;
use crossterm::event::{DisableBracketedPaste, DisableFocusChange, DisableMouseCapture, EnableBracketedPaste, EnableFocusChange, EnableMouseCapture, Event, KeyEvent};
#[cfg(unix)]
use crossterm::event::{KeyboardEnhancementFlags, PopKeyboardEnhancementFlags, PushKeyboardEnhancementFlags};
use crossterm::terminal::{self, EnterAlternateScreen, LeaveAlternateScreen};
use crossterm::{execute, queue};
use futures::future::Either;
use itertools::Itertools as _;
use nonempty_collections::NEVec;
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use rasm_command::telemetry::Handle;
use ratatui::DefaultTerminal;
use ratatui::layout::{Constraint, Layout};
use ratatui::text::{Line, Span, Text};
use ratatui::widgets::{Block, Cell, Row, Table};
use ratatui_core::style::Style;
#[cfg(unix)]
use rustix::process::{Signal, kill_current_process_group};
use tracing_log::AsTrace as _;
use tracing_subscriber::filter::Directive;
use tui_logger::{LevelFilter, TuiLoggerWidget, TuiWidgetEvent, TuiWidgetState};
use validated::Validated;

use crate::keymap::{self, Action, Pane};
use crate::{Effects, Input, Mode, Operation, Program, ScreenError};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Held {
    pub(crate) lines: Mutex<Option<VecDeque<Box<[u8]>>>>,
    pub(crate) depth: NonZero<usize>,
}

impl Write for &Held {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> { self.written(buf.len(), Write::write, buf) }

    fn write_all(&mut self, buf: &[u8]) -> io::Result<()> {
        match buf {
            [] => Ok(()),
            [_, ..] => self.written((), Write::write_all, buf),
        }
    }

    fn flush(&mut self) -> io::Result<()> { Ok(()) }
}

impl Held {
    fn written<T>(&self, buffered: T, write: impl FnOnce(&mut io::StderrLock<'static>, &[u8]) -> io::Result<T>, buf: &[u8]) -> io::Result<T> {
        let mut stderr = io::stderr().lock();
        self.lines.with_mut(|lines| {
            let Some(lines) = lines else { return write(&mut stderr, buf) };
            if lines.len() == self.depth.get() {
                lines.pop_front();
            }
            lines.push_back(Box::from(buf));
            Ok(buffered)
        })
    }
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
    pub(crate) held: Arc<Held>,
    pub(crate) failures: Vec<P::Error>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn entered(modes: &[Mode], held: &Held) -> io::Result<()> {
    let mut out = io::stdout().lock();
    let _stderr = io::stderr().lock();
    let mut lines = held.lines.lock();
    terminal::enable_raw_mode()?;
    *lines = Some(VecDeque::new());
    queue!(out, EnterAlternateScreen, EnableBracketedPaste)?;
    #[cfg(unix)]
    queue!(out, PushKeyboardEnhancementFlags(KeyboardEnhancementFlags::DISAMBIGUATE_ESCAPE_CODES | KeyboardEnhancementFlags::REPORT_ALTERNATE_KEYS))?;
    modes.iter().try_for_each(|mode| match mode {
        Mode::Mouse => queue!(out, EnableMouseCapture),
        Mode::Focus => queue!(out, EnableFocusChange),
    })?;
    out.flush()
}

fn released(modes: &[Mode], held: &Held) -> Result<(), NEVec<io::Error>> {
    let mut out = io::stdout().lock();
    let mut stderr = io::stderr().lock();
    let mut lines = held.lines.lock();
    let Some(backlog) = lines.take() else { return Ok(()) };
    let escaped = try bikeshed io::Result<()> {
        #[cfg(unix)]
        queue!(out, PopKeyboardEnhancementFlags)?;
        modes.iter().try_for_each(|mode| match mode {
            Mode::Mouse => queue!(out, DisableMouseCapture),
            Mode::Focus => queue!(out, DisableFocusChange),
        })?;
        execute!(out, DisableBracketedPaste, Show, LeaveAlternateScreen)?
    };
    let cooked = terminal::disable_raw_mode();
    let delivered = backlog.iter().try_for_each(|line| stderr.write_all(line));
    Validated::from(cooked).map3(Validated::from(escaped), Validated::from(delivered), |(), (), ()| ()).ok()
}

pub(crate) fn failed(operation: Operation) -> impl Fn(io::Error) -> ScreenError { move |source| ScreenError::Terminal { operation, source } }

pub(crate) fn left(modes: &[Mode], held: &Held) -> impl Iterator<Item = ScreenError> { released(modes, held).err().into_iter().flatten().map(failed(Operation::Leave)) }

pub(crate) fn reloaded(filter: &Handle, level: LevelFilter) {
    if let Err(error) = filter.modify(|filter| *filter = mem::take(filter).add_directive(Directive::from(level.as_trace()))) {
        tracing::warn!({ EXCEPTION_MESSAGE } = %Report::new(error), "tracing filter reload failed");
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
    let ended = |mut session: Session<P>, operation: Operation, outcome: Result<(), NEVec<io::Error>>| match outcome {
        Ok(()) => ControlFlow::Continue(session),
        Err(sources) => {
            session.failures.extend(sources.into_iter().map(failed(operation)).map(P::Error::from));
            ControlFlow::Break(session)
        }
    };
    let updated = |mut session: Session<P>, input: Input<P::Action, P::Item>| match session.program.update(input, &session.effects) {
        ControlFlow::Continue(()) => ControlFlow::Continue(session),
        ControlFlow::Break(outcome) => {
            session.failures.extend(outcome.err());
            ControlFlow::Break(session)
        }
    };
    let keyed = |session: Session<P>, event: Event| {
        #[cfg(unix)]
        let suspended = |mut session: Session<P>| {
            let resumed = released(P::MODES, &session.held).and_then(|()| {
                (try bikeshed io::Result<()> {
                    kill_current_process_group(Signal::TSTP)?;
                    entered(P::MODES, &session.held)?;
                    session.terminal.clear()?
                })
                .map_err(NEVec::new)
            });
            ended(session, Operation::Suspend, resumed)
        };
        let leveled = |session: Session<P>, step: fn(&LevelFilter) -> LevelFilter| {
            let level = step(&session.level);
            reloaded(&session.filter, level);
            ControlFlow::Continue(Session { level, logs: session.logs.set_default_display_level(level), ..session })
        };
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
    };
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
    let shown = folded
        .terminal
        .draw(|frame| {
            let (program, pane, logs) = (&mut folded.program, folded.pane, &folded.logs);
            let rows = keymap::resolved(program.keys())
                .map(|row| {
                    let active = keymap::active(&row.action, pane);
                    (row, active)
                })
                .filter(|(_, active)| *active || pane == Some(Pane::Keys))
                .unique_by(|(row, active)| (KeyEvent::from(&**row), *active))
                .map(|(row, active)| (if row.modifiers.is_empty() { row.code.to_string() } else { format!("{}+{}", row.modifiers, row.code) }, row.action.to_string(), active))
                .collect::<Vec<_>>();
            let toned = |active: bool, style: &anstyle::Style| if active { *style } else { *styles.get_placeholder() };
            let bordered = |shown: Pane| Block::bordered().title(Line::styled(shown.to_string(), *styles.get_header()));
            let gap = Span::raw("  ");
            let width = usize::from(frame.area().width);
            let footer = rows
                .iter()
                .filter(|(_, _, active)| *active)
                .map(|(key, action, _)| Line::from_iter([Span::styled(key.as_str(), *styles.get_literal()), Span::raw(" "), Span::styled(action.as_str(), *styles.get_context())]))
                .map(|line| (line.width(), line))
                .coalesce(|(occupied, mut line), (next, hint)| {
                    let joined = occupied.saturating_add(gap.width()).saturating_add(next);
                    if joined <= width {
                        line.extend(iter::once(gap.clone()).chain(hint));
                        Ok((joined, line))
                    } else {
                        Err(((occupied, line), (next, hint)))
                    }
                })
                .map(|(_, line)| line)
                .collect::<Text<'_>>();
            let [body, lower, bottom] =
                Layout::vertical([Constraint::Fill(2), pane.map_or(Constraint::Length(0), |_shown| Constraint::Fill(1)), Constraint::Length(footer.height().saturating_cast())]).areas(frame.area());
            program.view(frame, body, styles);
            match pane {
                Some(shown @ Pane::Logs) => frame.render_widget(
                    TuiLoggerWidget::default()
                        .block(bordered(shown))
                        .style_error(Style::from(*styles.get_error()))
                        .style_warn(Style::from(*styles.get_invalid()))
                        .style_info(Style::from(*styles.get_valid()))
                        .style_debug(Style::from(*styles.get_context()))
                        .style_trace(Style::from(*styles.get_placeholder()))
                        .state(logs),
                    lower,
                ),
                Some(shown @ Pane::Keys) => frame.render_widget(
                    Table::new(
                        rows.iter().map(|(key, action, active)| {
                            Row::new([Cell::from(key.as_str()).style(toned(*active, styles.get_literal())), Cell::from(action.as_str()).style(toned(*active, styles.get_context()))])
                        }),
                        [Constraint::Fill(1), Constraint::Fill(3)],
                    )
                    .block(bordered(shown)),
                    lower,
                ),
                None => {}
            }
            frame.render_widget(footer, bottom);
        })
        .map(|_frame| ())
        .map_err(NEVec::new);
    ended(folded, Operation::Draw, shown)
}
