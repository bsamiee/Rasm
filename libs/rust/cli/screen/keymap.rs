use core::iter;

use crossterm::event::{KeyCode, KeyEvent, KeyModifiers};
use serde::{Deserialize, Serialize};

use crate::view::Pane;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize, derive_more::Display)]
#[serde(rename_all = "kebab-case")]
#[display(rename_all = "kebab-case")]
pub enum Action<K> {
    Quit,
    #[cfg(unix)]
    Suspend,
    Logs,
    Keys,
    Older,
    Newer,
    Newest,
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
    fn from(value: &Binding<K>) -> Self { Self::new(value.code, value.modifiers) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn resolved<K>(own: &[Binding<K>]) -> Vec<Binding<K>>
where K: Clone {
    iter::chain(
        own.iter().cloned(),
        [
            Binding { code: KeyCode::Char('c'), modifiers: KeyModifiers::CONTROL, action: Action::Quit },
            #[cfg(unix)]
            Binding { code: KeyCode::Char('z'), modifiers: KeyModifiers::CONTROL, action: Action::Suspend },
            Binding { code: KeyCode::F(1), modifiers: KeyModifiers::NONE, action: Action::Keys },
            Binding { code: KeyCode::F(2), modifiers: KeyModifiers::NONE, action: Action::Logs },
            Binding { code: KeyCode::PageUp, modifiers: KeyModifiers::SHIFT, action: Action::Older },
            Binding { code: KeyCode::PageDown, modifiers: KeyModifiers::SHIFT, action: Action::Newer },
            Binding { code: KeyCode::End, modifiers: KeyModifiers::SHIFT, action: Action::Newest },
        ],
    )
    .fold(Vec::new(), |mut kept: Vec<Binding<K>>, row| {
        if kept.iter().all(|earlier| KeyEvent::from(earlier) != KeyEvent::from(&row)) {
            kept.push(row);
        }
        kept
    })
}

#[must_use]
pub(crate) fn active<K>(action: &Action<K>, shown: Option<Pane>) -> bool {
    match action {
        Action::Older | Action::Newer | Action::Newest => shown == Some(Pane::Logs),
        #[cfg(unix)]
        Action::Suspend => true,
        Action::Quit | Action::Logs | Action::Keys | Action::Program(_) => true,
    }
}
