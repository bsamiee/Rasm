use std::io;

use anstream::{AutoStream, ColorChoice};
use anstyle::{Ansi256Color, Color, Effects, Style};
use clap::builder::Styles;
use comfy_table::{Attribute, Cell};
use inquire::ui::{self, Attributes, ErrorMessageRenderConfig, RenderConfig, StyleSheet};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn painted<C>(color: Color, indexed: impl FnOnce(u8) -> C, rgb: impl FnOnce(u8, u8, u8) -> C) -> C {
    match color {
        Color::Ansi(color) => indexed(Ansi256Color::from(color).index()),
        Color::Ansi256(color) => indexed(color.index()),
        Color::Rgb(color) => rgb(color.r(), color.g(), color.b()),
    }
}

pub(crate) fn prompts(styles: &Styles) -> RenderConfig<'static> {
    let sheet = |style: &Style| {
        let colored = |color| painted(color, ui::Color::AnsiValue, ui::Color::rgb);
        StyleSheet {
            fg: style.get_fg_color().map(colored),
            bg: style.get_bg_color().map(colored),
            att: [(Effects::BOLD, Attributes::BOLD), (Effects::ITALIC, Attributes::ITALIC)]
                .into_iter()
                .filter_map(|(effect, attribute)| style.get_effects().contains(effect).then_some(attribute))
                .collect(),
        }
    };
    match AutoStream::choice(&io::stderr()) {
        ColorChoice::Never => RenderConfig::empty(),
        ColorChoice::Auto | ColorChoice::AlwaysAnsi | ColorChoice::Always => {
            let base = RenderConfig::default_colored();
            let [literal, valid, context, error] = [styles.get_literal(), styles.get_valid(), styles.get_context(), styles.get_error()].map(sheet);
            RenderConfig {
                prompt_prefix: base.prompt_prefix.with_style_sheet(literal),
                answered_prompt_prefix: base.answered_prompt_prefix.with_style_sheet(valid),
                prompt: sheet(styles.get_header()),
                default_value: context,
                placeholder: sheet(styles.get_placeholder()),
                help_message: context,
                answer: valid,
                canceled_prompt_indicator: base.canceled_prompt_indicator.with_style_sheet(sheet(styles.get_invalid())),
                error_message: ErrorMessageRenderConfig { prefix: base.error_message.prefix.with_style_sheet(error), message: error, ..base.error_message },
                highlighted_option_prefix: base.highlighted_option_prefix.with_style_sheet(literal),
                selected_checkbox: base.selected_checkbox.with_style_sheet(valid),
                selected_option: Some(literal),
                editor_prompt: context,
                ..base
            }
        }
    }
}

pub(crate) fn styled(cell: Cell, style: &Style) -> Cell {
    let colored = |color| painted(color, comfy_table::Color::AnsiValue, |r, g, b| comfy_table::Color::Rgb { r, g, b });
    let cell = cell.add_attributes(
        [
            (Effects::BOLD, Attribute::Bold),
            (Effects::DIMMED, Attribute::Dim),
            (Effects::ITALIC, Attribute::Italic),
            (Effects::UNDERLINE, Attribute::Underlined),
            (Effects::DOUBLE_UNDERLINE, Attribute::DoubleUnderlined),
            (Effects::CURLY_UNDERLINE, Attribute::Undercurled),
            (Effects::DOTTED_UNDERLINE, Attribute::Underdotted),
            (Effects::DASHED_UNDERLINE, Attribute::Underdashed),
            (Effects::BLINK, Attribute::SlowBlink),
            (Effects::INVERT, Attribute::Reverse),
            (Effects::HIDDEN, Attribute::Hidden),
            (Effects::STRIKETHROUGH, Attribute::CrossedOut),
        ]
        .into_iter()
        .filter_map(|(effect, attribute)| style.get_effects().contains(effect).then_some(attribute))
        .collect(),
    );
    let cell = style.get_fg_color().map(colored).into_iter().fold(cell, Cell::fg);
    style.get_bg_color().map(colored).into_iter().fold(cell, Cell::bg)
}
