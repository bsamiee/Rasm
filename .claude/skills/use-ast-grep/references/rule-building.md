# [RULE_BUILDING]

Each rule derives from one correction and the contracts deciding where the correction applies.

## [01]-[EVIDENCE]

Read original code and its consumers before selecting a matcher:
1. Before and after forms with the original diff, or conforming and violating forms of a stated policy
2. Language semantics, configured grammar, resolved package declarations, and implementation deciding the correction
3. Configured checkers, a rule leaves out forms a linter or type-aware checker reports and keeps an arm for a defect that checker's fix writes

## [02]-[APPLICABILITY]

Syntax and its contract establish a diagnostic, ast-grep reads no type, flow, or binding:
- Type resolution, control and data flow, constant propagation, ownership, and lifetime claims take semantic tooling
- Repeated text establishes spelling alone, an import or declaration establishes meaning, a call shape enters a rule as a callee test
- Unused bound values prove no independence between operations, earlier success can establish a later precondition
- Policy rules name the restriction without claiming the supported API or syntax is defective
- Coverage follows the grammar's accepted forms and the API contract, a form absent from current code keeps its arm, counts decide nothing
- Lists a declaration states stay in that declaration, a rule requires consumers to read the owner
- `files:` names the folders a rule's cause covers, a package rule's the folders of projects referencing the package

## [03]-[CONSTRUCTION]

Matched constructs identify the violation and contain the required edit, related syntax supplies evidence without enlarging them:
- Statement removal selects the whole statement through a pattern ending in its terminator
- `kind` with `has` on a field states a role a pattern states by position, it survives modifiers and wrappers a pattern misses
- Single edits correcting every dependent use report the shared declaration once, independent corrections report each use
- Labels pinpoint captured causes without changing match or replacement ranges
- Modifier and wrapper differences that change the correction's applicability take their own arm

## [04]-[REWRITES]

Fix templates own every token the parser would have kept:
- Enumerate matched tokens, comments, wrappers, and call shapes before writing templates, each child the template omits leaves the source
- Templates placing `$A` under a tighter operator parenthesize it or match only leaf kinds
- Evaluation count, order, timing, short-circuiting, and argument binding stay outside the edit unless the correction names them
- Types, null behavior, collation, and numeric association stay across expression refactors
- Rules omit `fix` when types, evaluation, binding, precedence, or comments decide the replacement
- Messages name the violation with its captures, `note` states the required form and deciding contract

## [05]-[PROOF]

Rules prove themselves on one scan of a scratch file of violating forms and near misses, then the project at zero findings:

```bash
ast-grep scan --inline-rules "$(cat <rule>)" --json=stream --stdin < <scratch>
ast-grep scan --filter '^<id>$' <path>
```

- Rules reading an injected document run over a host file under `scan`
- Rules exiting 0 with no finding can hold a never-matching kind intersection or a parse-error pattern beside `kind`, `--debug-query=pattern` shows what compiled

`ast-grep test` holds cases under each `testConfigs` `testDir`, one YAML document per rule:

```yaml
id: <rule id>
valid: [<code that reports nothing>]
invalid: [<code that reports>]
```

- `invalid` cases compare against `__snapshots__/<id>-snapshot.yml`, a missing snapshot fails the run until `-U` writes it
- `--skip-snapshot-tests` tests whether each case reports, `--include-off` runs `severity: off` rules, `-f` takes a regex over ids
