// --- [OPERATIONS] ----------------------------------------------------------------------

fn rest_pattern_workspace_field(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `..` covers `Window` fields the pattern leaves unnamed
      --> tools/dylint/examples/rest_pattern_workspace_field.rs:37:9
       |
    37 |     let Window { start, .. } = window;
       |         ^^^^^^^^^^^^^^^^--^^
       |                         |
       |                         help: name each field `..` covers as `field: _`: `end: _, label: _`
       |
       = note: `#[warn(rest_pattern_workspace_field)]` on by default
    applicability MachineApplicable: name each field `..` covers as `field: _`

    warning: `..` covers `Drag` fields the pattern leaves unnamed
      --> tools/dylint/examples/rest_pattern_workspace_field.rs:48:9
       |
    48 |         Gesture::Drag { from, .. } => from,
       |         ^^^^^^^^^^^^^^^^^^^^^^--^^
       |                               |
       |                               help: name each field `..` covers as `field: _`: `to: _`
    applicability MachineApplicable: name each field `..` covers as `field: _`

    warning: `..` covers `Pinch` fields the pattern leaves unnamed
      --> tools/dylint/examples/rest_pattern_workspace_field.rs:49:9
       |
    49 |         Gesture::Pinch(first, ..) => first,
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: write `_` in place of `..` for each field it covers

    warning: `..` covers `Token` fields the pattern leaves unnamed
      --> tools/dylint/examples/rest_pattern_workspace_field.rs:63:9
       |
    63 |     let sealed::Token { id, .. } = token;
       |         ^^^^^^^^^^^^^^^^^^^^-^^^
       |                             |
       |                             help: name each field `..` covers as `field: _`: `scope: _,`
    applicability MachineApplicable: name each field `..` covers as `field: _`
    ");
}

fn wildcard_workspace_enum_match_arm(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `matches!` sends each `RenderError` variant it leaves unnamed to `false`
       --> tools/dylint/examples/untyped_error.rs:393:34
        |
    393 | ...r| matches!(error, RenderError::Format(_) | RenderError::Narrow(_) | RenderError::Abandoned(_) | RenderError::Parse(_) | RenderError::Never(_)), |...
        |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`
        = note: `#[warn(wildcard_workspace_enum_match_arm)]` on by default

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:25:9
       |
    25 |         _ => 1,
       |         ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`
       = note: `#[warn(wildcard_workspace_enum_match_arm)]` on by default

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:32:9
       |
    32 |         other => other,
       |         ^^^^^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:47:10
       |
    47 |         &_ => 5,
       |          ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:69:9
       |
    69 |         _ => 1,
       |         ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:76:33
       |
    76 |         Command::Send { signal: _ } => 1,
       |                                 ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:98:9
       |
    98 |         _ => 1,
       |         ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:102:44
        |
    102 | const fn if_let(signal: Signal) -> u8 { if let Signal::Start = signal { 0 } else { 1 } }
        |                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `let .. else` sends each `Signal` variant its pattern leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:117:10
        |
    117 |     let (Signal::Pause | Signal::Stop) = signal else { return None };
        |          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `matches!` sends each `Signal` variant it leaves unnamed to `false`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:123:47
        |
    123 | const fn tested_set(signal: Signal) -> bool { matches!(signal, Signal::Start | Signal::Resume) }
        |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:125:52
        |
    125 | const fn tested_branch(signal: Signal) -> u8 { if !matches!(signal, Signal::Stop) { 0 } else { 1 } }
        |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:127:40
        |
    127 | fn compared(signal: Signal) -> u8 { if signal == Signal::Resume { 0 } else { 1 } }
        |                                        ^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:139:14
        |
    139 |         Some(_) => 1,
        |              ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:147:10
        |
    147 |         (_, _) => 1,
        |          ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:147:13
        |
    147 |         (_, _) => 1,
        |             ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:162:12
        |
    162 |         Ok(_) | Err(_) => 1,
        |            ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:170:13
        |
    166 | macro_rules! classified {
    ...
    170 |             _ => 1,
        |             ^
    ...
    175 | const fn expanded(signal: Signal) -> u8 { classified!(signal) }
        |                                           ------------------- in this macro invocation
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:177:51
        |
    177 | fn listed(left: Signal, right: Signal) -> u8 { if [left, right].contains(&Signal::Stop) { 0 } else { 1 } }
        |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:179:51
        |
    179 | fn either(left: Signal, right: Signal) -> u8 { if left == Signal::Stop || right == Signal::Pause { 0 } else { 1 } }
        |                                                   ^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:181:58
        |
    181 | const fn chained(signal: Signal, ready: bool) -> u8 { if let Signal::Start = signal && ready { 0 } else { 1 } }
        |                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:183:50
        |
    183 | fn queued(signals: &VecDeque<Signal>) -> u8 { if signals.contains(&Signal::Resume) { 0 } else { 1 } }
        |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(rest_pattern_workspace_field, wildcard_workspace_enum_match_arm);
