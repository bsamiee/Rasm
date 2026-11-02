use core::iter;

use clap::builder::Styles;
use itertools::Itertools as _;
use ratatui::{
    Frame,
    layout::{Constraint, Layout},
    text::{Line, Span, Text},
    widgets::{Block, Cell, Row, Table},
};
use ratatui_core::style::Style;
use tui_logger::{TuiLoggerWidget, TuiWidgetState};

use crate::{
    Program,
    keymap::{self, Binding},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, derive_more::Display)]
pub(crate) enum Pane {
    Logs,
    Keys,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn drawn<P>(frame: &mut Frame<'_>, program: &mut P, pane: Option<Pane>, logs: &TuiWidgetState, styles: &Styles)
where P: Program {
    let rows = keymap::resolved(program.keys());
    let label = |row: &Binding<P::Action>| if row.modifiers.is_empty() { row.code.to_string() } else { format!("{}+{}", row.modifiers, row.code) };
    let toned = |active: bool, style: &anstyle::Style| if active { *style } else { *styles.get_placeholder() };
    let framed = |shown: Pane| Block::bordered().title(Line::styled(shown.to_string(), *styles.get_header()));
    let gap = Span::raw("  ");
    let width = usize::from(frame.area().width);
    let footer =
        rows.iter()
            .filter(|row| keymap::active(&row.action, pane))
            .map(|row| Line::from_iter([Span::styled(label(row), *styles.get_literal()), Span::raw(" "), Span::styled(row.action.to_string(), *styles.get_context())]))
            .coalesce(|line, hint| {
                if line.width().saturating_add(gap.width()).saturating_add(hint.width()) <= width { Ok(iter::chain(line, iter::once(gap.clone())).chain(hint).collect()) } else { Err((line, hint)) }
            })
            .collect::<Text<'_>>();
    let [body, lower, bottom] =
        Layout::vertical([Constraint::Fill(2), pane.map_or(Constraint::Length(0), |_shown| Constraint::Fill(1)), Constraint::Length(u16::try_from(footer.height()).unwrap_or(u16::MAX))])
            .areas(frame.area());
    program.view(frame, body);
    match pane {
        Some(shown @ Pane::Logs) => frame.render_widget(
            TuiLoggerWidget::default()
                .block(framed(shown))
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
                rows.iter()
                    .map(|row| (row, keymap::active(&row.action, pane)))
                    .map(|(row, active)| Row::new([Cell::from(label(row)).style(toned(active, styles.get_literal())), Cell::from(row.action.to_string()).style(toned(active, styles.get_context()))])),
                [Constraint::Fill(1), Constraint::Fill(3)],
            )
            .block(framed(shown)),
            lower,
        ),
        None => {}
    }
    frame.render_widget(footer, bottom);
}
