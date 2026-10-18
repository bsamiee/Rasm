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
       --> tools/dylint/examples/untyped_error.rs:386:34
        |
    386 | ...r| matches!(error, RenderError::Format(_) | RenderError::Narrow(_) | RenderError::Abandoned(_) | RenderError::Parse(_) | RenderError::Never(_)), |...
        |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`
        = note: `#[warn(wildcard_workspace_enum_match_arm)]` on by default

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:24:9
       |
    24 |         _ => 1,
       |         ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`
       = note: `#[warn(wildcard_workspace_enum_match_arm)]` on by default

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:31:9
       |
    31 |         other => other,
       |         ^^^^^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:46:10
       |
    46 |         &_ => 5,
       |          ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:68:9
       |
    68 |         _ => 1,
       |         ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:75:33
       |
    75 |         Command::Send { signal: _ } => 1,
       |                                 ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
      --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:97:9
       |
    97 |         _ => 1,
       |         ^
       |
       = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:101:44
        |
    101 | const fn if_let(signal: Signal) -> u8 { if let Signal::Start = signal { 0 } else { 1 } }
        |                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `let .. else` sends each `Signal` variant its pattern leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:116:10
        |
    116 |     let (Signal::Pause | Signal::Stop) = signal else { return None };
        |          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `matches!` sends each `Signal` variant it leaves unnamed to `false`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:122:47
        |
    122 | const fn tested_set(signal: Signal) -> bool { matches!(signal, Signal::Start | Signal::Resume) }
        |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:124:52
        |
    124 | const fn tested_branch(signal: Signal) -> u8 { if !matches!(signal, Signal::Stop) { 0 } else { 1 } }
        |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: `if` over `Signal` sends each variant its condition leaves unnamed to `else`
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:126:40
        |
    126 | fn compared(signal: Signal) -> u8 { if signal == Signal::Resume { 0 } else { 1 } }
        |                                        ^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:138:14
        |
    138 |         Some(_) => 1,
        |              ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:146:10
        |
    146 |         (_, _) => 1,
        |          ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:146:13
        |
    146 |         (_, _) => 1,
        |             ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:161:12
        |
    161 |         Ok(_) | Err(_) => 1,
        |            ^
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`

    warning: catch-all pattern covers each `Signal` variant the other arms leave unnamed
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:169:13
        |
    165 | macro_rules! classified {
    ...
    169 |             _ => 1,
        |             ^
    ...
    174 | const fn expanded(signal: Signal) -> u8 { classified!(signal) }
        |                                           ------------------- in this macro invocation
        |
        = help: name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(rest_pattern_workspace_field, wildcard_workspace_enum_match_arm);
