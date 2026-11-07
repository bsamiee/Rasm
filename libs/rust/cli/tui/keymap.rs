use core::iter;
use std::borrow::Cow;

use crossterm::event::{KeyCode, KeyEvent, KeyModifiers};
use itertools::Itertools as _;
use serde::{Deserialize, Serialize};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize, derive_more::Display)]
#[serde(rename_all = "lowercase")]
#[display(rename_all = "lowercase")]
pub enum Pane {
    Logs,
    Keys,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize, derive_more::Display)]
#[serde(rename_all = "kebab-case")]
#[display(rename_all = "kebab-case")]
pub enum Action<K> {
    Quit,
    #[cfg(unix)]
    Suspend,
    Older,
    Newer,
    Newest,
    Verbose,
    Quiet,
    #[serde(untagged)]
    Toggle(Pane),
    #[serde(untagged)]
    Program(K),
}

#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
pub struct Binding<K> {
    pub code: KeyCode,
    #[serde(default = "KeyModifiers::empty")]
    pub modifiers: KeyModifiers,
    pub action: Action<K>,
}

impl<K> From<&Binding<K>> for KeyEvent {
    fn from(value: &Binding<K>) -> Self { normalized(Self::new(value.code, value.modifiers)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub(crate) fn normalized(key: KeyEvent) -> KeyEvent {
    match key.code {
        KeyCode::Char(typed) if key.modifiers.contains(KeyModifiers::SHIFT) =>
            KeyEvent::new(KeyCode::Char(typed.to_uppercase().collect_array().map_or(typed, |[upper]| upper)), key.modifiers.difference(KeyModifiers::SHIFT)),
        KeyCode::BackTab => KeyEvent::new(key.code, key.modifiers.difference(KeyModifiers::SHIFT)),
        _ => KeyEvent::new(key.code, key.modifiers),
    }
}

pub(crate) fn resolved<K>(program: &[Binding<K>]) -> impl Iterator<Item = Cow<'_, Binding<K>>>
where K: Clone {
    iter::chain(
        program.iter().map(Cow::Borrowed),
        [
            Binding { code: KeyCode::Char('c'), modifiers: KeyModifiers::CONTROL, action: Action::Quit },
            #[cfg(unix)]
            Binding { code: KeyCode::Char('z'), modifiers: KeyModifiers::CONTROL, action: Action::Suspend },
            Binding { code: KeyCode::F(1), modifiers: KeyModifiers::NONE, action: Action::Toggle(Pane::Keys) },
            Binding { code: KeyCode::F(2), modifiers: KeyModifiers::NONE, action: Action::Toggle(Pane::Logs) },
            Binding { code: KeyCode::PageUp, modifiers: KeyModifiers::SHIFT, action: Action::Older },
            Binding { code: KeyCode::PageDown, modifiers: KeyModifiers::SHIFT, action: Action::Newer },
            Binding { code: KeyCode::End, modifiers: KeyModifiers::SHIFT, action: Action::Newest },
            Binding { code: KeyCode::Up, modifiers: KeyModifiers::SHIFT, action: Action::Verbose },
            Binding { code: KeyCode::Down, modifiers: KeyModifiers::SHIFT, action: Action::Quiet },
        ]
        .map(Cow::Owned),
    )
}

#[must_use]
pub(crate) fn active<K>(action: &Action<K>, shown: Option<Pane>) -> bool {
    match action {
        Action::Older | Action::Newer | Action::Newest | Action::Verbose | Action::Quiet => shown == Some(Pane::Logs),
        #[cfg(unix)]
        Action::Suspend => true,
        Action::Quit | Action::Toggle(_) | Action::Program(_) => true,
    }
}
