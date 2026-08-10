# [PY_ARTIFACTS_API_LATEX2MATHML]

`latex2mathml` converts a LaTeX math expression to presentation MathML in pure Python — the front-end `ziamath` drives for its `Latex`/`Text` paths. Artifacts composes it at two seams: `ziamath` consumes the conversion internally, and `typography/math#FORMULA` composes `commands.FUNCTIONS` directly — the module-global operator registry whose tuple identity makes the per-render snapshot-and-restore real.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `latex2mathml`
- package: `latex2mathml` (MIT)
- module: `latex2mathml`
- owner: `artifacts`
- rail: figure (behind `ziamath`); operator-registry custody (direct)
- depends: none (pure Python, stdlib `xml.etree` egress)
- entry points: library and a `latex2mathml` CLI; the design composes the in-process API
- capability: LaTeX -> presentation-MathML conversion (string or `ET.Element`), a typed LaTeX-grammar exception family, and the module-global operator vocabulary `\DeclareMathOperator`-style registration rebinds

## [02]-[CONVERSION]

[ENTRYPOINT_SCOPE]: LaTeX -> MathML egress

`convert` answers a standalone MathML string; `convert_to_element` answers the same content as a live `ET.Element` for a consumer composing one XML tree without a reparse. `display` selects `inline` (the default) versus `block` layout intent, `xmlns` stamps the MathML namespace, and `parent` grafts the result into a caller-owned tree.

| [INDEX] | [MEMBER]                                                      | [KIND]    | [ROLE]                                                 |
| :-----: | :------------------------------------------------------------ | :-------- | :----------------------------------------------------- |
|  [01]   | `converter.convert(latex, xmlns, display, parent)`            | serialize | LaTeX -> MathML `str`                                  |
|  [02]   | `converter.convert_to_element(latex, xmlns, display, parent)` | compose   | LaTeX -> MathML `ET.Element`                           |
|  [03]   | `walker.walk(data, display, macros) -> list[Node]`            | parse     | tokenized LaTeX -> node list, the pre-XML intermediate |
|  [04]   | `tokenizer.tokenize(...)`                                     | parse     | raw LaTeX token stream the walker consumes             |

[FAULT_SCOPE]: the typed grammar-exception family

Malformed LaTeX raises one of twelve grammar exceptions (`DenominatorNotFoundError`, `DoubleSubscriptsError`, `DoubleSuperscriptsError`, `ExtraLeftOrMissingRightError`, `InvalidAlignmentError`, `InvalidStyleForGenfracError`, `InvalidWidthError`, `LimitsMustFollowMathOperatorError`, `MissingEndError`, `MissingSuperScriptOrSubscriptError`, `NoAvailableTokensError`, `NumeratorNotFoundError`) from `latex2mathml.exceptions` — a boundary converts them to the fault rail at the seam that parses caller-supplied LaTeX; through `ziamath` they surface inside the render call and ride that owner's fence.

## [03]-[OPERATOR_REGISTRY]

[REGISTRY_SCOPE]: `commands.FUNCTIONS` — the composed member

`commands.FUNCTIONS` is the module-global TUPLE of upright-operator names the parser treats as `\DeclareMathOperator` functions. `ziamath.declareoperator` extends it by REBINDING — measured at source, `FUNCTIONS = FUNCTIONS + (name,)` — never an in-place append, so a captured binding is a true immutable snapshot and a `finally` rebind is a real restore. Its length is import-order-dependent (ziamath's own import-time declarations extend it), so a consumer reads the live binding per render and never records a count.

| [INDEX] | [MEMBER]                            | [KIND]   | [ROLE]                                                                      |
| :-----: | :---------------------------------- | :------- | :-------------------------------------------------------------------------- |
|  [01]   | `commands.FUNCTIONS: tuple[str, …]` | registry | upright-operator vocabulary; rebound, never mutated — snapshot/restore-safe |
|  [02]   | `commands.LIMIT: tuple[str, …]`     | registry | operators taking limits-style scripts (read-only companion roster)          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `convert` (string) and `convert_to_element` (`ET.Element`, optionally grafted through `parent=`) spell one conversion egress — never a per-format converter family; `walker.walk`/`tokenizer.tokenize` are the pre-XML intermediates a grammar-analysis consumer reads, not a second egress.
- `commands.FUNCTIONS` is process-global state: a per-render operator vocabulary is a snapshot of the current binding, the render, then a `finally` rebind of the snapshot — the tuple identity is the load-bearing fact, since the same discipline over a list aliases the mutated object and restores nothing. Every render captures the baseline earlier `declareoperator` calls (ziamath's import-time set included) left, never a baseline frozen at module init.
- grammar failure is a typed exception family, not a `None` return — the twelve `exceptions` members convert at the parse boundary; a bare `except Exception` over a conversion masks a grammar bug as input failure.

[STACKING]:
- `ziamath`(`.api/ziamath.md`): the render trio's `Latex`/`Text` paths call this conversion internally and `declareoperator` rebinds this registry — the artifacts formula rail reaches LaTeX through `ziamath`, and this package's direct composition is the registry custody alone (`typography/math#FORMULA` `_laid` brackets `config.svg2` and the `FUNCTIONS` snapshot under one `RLock` with a `finally` restore).
- `document/model`(`.planning/document/model.md`): a MathML-bearing document node consumes `convert_to_element` output grafted via `parent=` where the equation must join an existing `xml.etree` tree without the ziamath layout pass — the raw-MathML lane beside the rendered-SVG lane.
- runtime rails: caller-supplied LaTeX converts under the boundary `Result` rail with the twelve grammar exceptions as the named `except` arms; registry mutation runs serialized on the `to_thread` lane inside the owning lock, matching the `ziamath` global-config discipline.

[RAIL_LAW]:
- Package: `latex2mathml`
- Owns: LaTeX -> presentation-MathML conversion (string and `ET.Element`), the LaTeX token/walk intermediates, the typed grammar-exception family, and the `commands.FUNCTIONS`/`commands.LIMIT` operator vocabulary custody
- Accept: the `ziamath` front-end dependency; direct `commands.FUNCTIONS` snapshot-and-restore at the one formula owner; `convert_to_element` for raw-MathML document nodes joining a caller tree
- Reject: MathML -> SVG typesetting where `ziamath` owns layout; a second LaTeX parser or hand-rolled operator table where `FUNCTIONS` is the registry; an in-place mutation of `FUNCTIONS` where rebind is the contract; recording the registry length where import order owns it; a raised grammar exception crossing the async edge where the boundary rail owns failure
