# [PYTHON_01_DOCSTRINGS_GENERATORS_RESEARCH]

Focus: Python 3.15/current docstring standards, PEP 257, Google docstring guidance, Ruff docstring gates, Griffe and mkdocstrings generator boundaries, and generated-reference implications for `docs/standards/reference/code-documentation.md`.

Scope: research report only. No active standards were edited. The worktree already had modified and untracked `docs/standards` files; this report leaves them untouched.

## [1][CHECKS]

Local source reads:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/style-guide.md`
- `pyproject.toml`
- adjacent temporary input: `.reports/code-documentation-050626/track-general/02-general-generation.md`

Local searches:
- `fd -a '^CLAUDE\\.md$|^AGENTS\\.md$' /Users/bardiasamiee/Documents/99.Github/Rasm`
- `rg -n "docs/standards|code-documentation|docstring|mkdocstrings|Griffe|PEP 257|Google" /Users/bardiasamiee/.codex/memories/MEMORY.md`
- `rg -n "mkdocstrings|griffe|docstring|pydocstyle|ruff|pydantic|msgspec|beartype|python" pyproject.toml docs tools -g '*.toml' -g '*.md' -g '*.py'`
- `fd -a 'mkdocs|docs|pyproject|ruff|ty|pydocstyle' . -t f`
- `git status --short -- .reports/code-documentation-050626 docs/standards/reference/code-documentation.md docs/standards/style-guide.md docs/standards/README.md docs/standards/AGENTS.md`

Local configured truth:
- `pyproject.toml` sets `requires-python = ">=3.14"`, `tool.ty.environment.python-version = "3.14"`, `tool.mypy.python_version = "3.14"`, and `tool.ruff.target-version = "py314"`.
- `pyproject.toml` selects all Ruff lint rules, enables preview, configures Ruff pydocstyle with `convention = "google"`, and sets Ruff pydoclint `ignore-one-line-docstrings = false`.
- No local manifest or file-discovery hit proves configured `mkdocs`, `mkdocstrings`, or `griffe` generated Python API documentation in this checkout.

Current primary sources checked on 2026-06-05:
- PEP 257: https://peps.python.org/pep-0257/
- Python 3.15 `annotationlib`: https://docs.python.org/3.15/library/annotationlib.html
- Python 3.15 `inspect`: https://docs.python.org/3.15/library/inspect.html
- Google Python Style Guide, comments and docstrings: https://google.github.io/styleguide/pyguide.html#38-comments-and-docstrings
- Ruff settings, `lint.pydocstyle.convention`: https://docs.astral.sh/ruff/settings/#lint_pydocstyle_convention
- Ruff `signature-in-docstring`: https://docs.astral.sh/ruff/rules/signature-in-docstring/
- Ruff `docstring-missing-returns`: https://docs.astral.sh/ruff/rules/docstring-missing-returns/
- mkdocstrings-python overview: https://mkdocstrings.github.io/python/
- mkdocstrings-python docstring options: https://mkdocstrings.github.io/python/usage/configuration/docstrings/
- Griffe docstring parsers: https://mkdocstrings.github.io/griffe/reference/docstrings/
- Griffe official extensions: https://mkdocstrings.github.io/griffe/extensions/official/
- Griffe TypingDoc extension: https://mkdocstrings.github.io/griffe-typingdoc/
- PEP 727: https://peps.python.org/pep-0727/
- PEP 702: https://peps.python.org/pep-0702/

## [2][FINDINGS]

[F1][PYTHON_3_15_IS_UPSTREAM_CURRENT_NOT_REPO_TARGET]

The checked Python docs identify the upstream documentation as `3.15.0b2`, and `annotationlib` is present as a 3.14-added module with 3.15 documentation. The repo Python tooling still targets 3.14 in `pyproject.toml`, so the active standard can discuss Python 3.15 as the standards baseline only as a forward/upstream language target. It should not claim that Ruff, Ty, mypy, or local Python execution currently target 3.15.

Impact: keep the Python capsule's 3.15 horizon if that is the intentional docs baseline, but add or preserve local-proof wording when discussing configured tooling.

[F2][PEP_257_IS_SUBSTRATE_NOT_GOOGLE_STYLE]

PEP 257 is active informational guidance. It defines what docstrings are, where they attach, triple-double-quote convention, one-line and multi-line layout, summary-line behavior, indentation trimming, and the rule against restating a Python function signature in the docstring. It explicitly avoids defining markup syntax inside docstrings.

Impact: the active standard is right to treat PEP 257 as the substrate. Do not route `Args:`, `Returns:`, `Raises:`, `Yields:`, or generator-specific sections to PEP 257 itself.

[F3][GOOGLE_STYLE_IS_THE_REPO_DOCSTRING_DIALECT]

Google's current style guide says functions include methods, generators, and properties, and it requires a docstring for public API, nontrivial size, or non-obvious logic. It says a docstring should let the caller write the call without reading the body, should describe calling syntax and semantics, should document side effects relevant to use, and should avoid implementation detail unless that detail changes caller behavior.

Impact: keep Google docstrings as Rasm's Python dialect because local Ruff pydocstyle already selects `google`. The Rasm standard can still be stricter than Google by documenting only semantic obligations that declarations omit, but the examples should use Google section names and indentation.

[F4][TYPES_BELONG_TO_ANNOTATIONS_FIRST]

Google style says `Args:` descriptions include required types only when code lacks a corresponding type annotation, and `Returns:` includes type information only when the annotation does not provide it. PEP 257 and Ruff's `D402` rule also push against signature restatement. Python 3.15 `inspect.signature` and `annotationlib` reinforce the split because signatures and annotations are machine-readable shape sources, while docstrings are semantic text.

Impact: keep "annotations own type shape, docstrings own semantics." Strengthen any Python wording that could allow repeated parameter or return types inside Google sections when annotations already carry the shape.

[F5][RAISES_SHOULD_EXPOSE_SUPPORTED_EXCEPTIONS_ONLY]

Google's exception example intentionally omits a `ValueError` raised for API misuse while documenting the supported `ConnectionError` outcome. This matches the active Rasm rule that `Raises:` documents intentionally exposed exceptions, not every internal contract guard or validation branch.

Impact: keep the narrow `Raises:` rule. Do not let pydoclint completeness language become a requirement to document all native exceptions.

[F6][RUFF_PROVES_STYLE_GATE_NOT_GENERATOR_OUTPUT]

Ruff supports selecting `google`, `numpy`, or `pep257` conventions for pydocstyle, and Rasm selects `google`. Ruff pydoclint preview rules can check mismatches such as missing returns sections, but they are lint gates over source text, not generated reference output. Local per-file ignores also exempt several current Python surfaces from D101, D102, and D103.

Impact: treat Ruff as source-comment lint proof only. Do not cite Ruff as evidence that a generated API reference exists.

[F7][GRIFFE_GOOGLE_SUPPORT_IS_BROADER_THAN_GOOGLE_GUIDANCE]

Griffe supports `google`, `numpy`, `sphinx`, and `auto` docstring parsers, parses Google-style sections into structured data, and makes no markup assumption. Its Google parser supports aliases such as `Parameters`, `Args`, `Arguments`, and `Params`, and it supports sections including `Attributes`, `Parameters`, `Raises`, `Warns`, `Yields`, `Receives`, and `Returns`.

Impact: if Rasm says "Google docstrings," use canonical Google names such as `Args:` and `Returns:` in examples. Treat `Receives:` as a Griffe/mkdocstrings generator-profile detail for public `Generator[..., SendType, ...]` contracts, not as a plain Google style guide rule.

[F8][MKDOCSTRINGS_USES_GRIFFE_AND_ANNOTATIONS]

mkdocstrings-python states that Griffe collects the object tree and docstrings, and that Griffe collects type annotations for mkdocstrings to display parameter and return types. Its docstring options default to `docstring_style: "google"` and expose controls for docstring parser options, section rendering, `merge_init_into_class`, cross-reference behavior, and section visibility.

Impact: mkdocstrings and Griffe are valid adoptable Python generated-reference profiles. Since the repo does not prove local adoption, active standards should say "preferred generated-reference profile when generated Python API documentation is adopted," not "current generated rail."

[F9][PEP_727_IS_NOT_A_CURRENT_STANDARD]

PEP 727 proposed `typing.Doc` for documentation in `Annotated` metadata, but its status is `Withdrawn`. Griffe has an official `typing-doc` extension, and Griffe TypingDoc supports `annotated-doc` originally based on PEP 727, but that is extension behavior rather than current Python language standard behavior.

Impact: do not make `typing.Doc`, `typing_extensions.Doc`, `annotated-doc`, or Griffe TypingDoc a default Rasm source-comment rule. Mention it only as an optional generator extension if the repo adopts it explicitly.

[F10][PEP_702_IS_LIFECYCLE_NOT_GENERAL_DOCSTRING_CONTENT]

PEP 702 is final and defines a way to mark deprecations using the type system. Griffe also has an official `warnings-deprecated` extension for PEP 702. Rasm's repo policy currently bans deprecation markers in Python through Ruff banned APIs and prefers greenfield refactoring over lifecycle preservation.

Impact: keep PEP 702-style deprecation in lifecycle/support-contract language only. For Rasm Python source, local repo policy currently rejects deprecation decorators as preservation devices.

## [3][RECOMMENDATIONS]

[ADD]

- Add a Python local-proof caveat if the active standard keeps `Python 3.15` in the capsule: current upstream docs checked at `3.15.0b2`; current repo tooling targets Python 3.14.
- Add one sentence to the Python capsule or generated-reference boundary: `Receives:` is valid only when the configured Python generator parses it and the public generator accepts `send()` values.
- Add a generated-profile caveat: mkdocstrings and Griffe are preferred when adopted, but Ruff Google pydocstyle is the only Python docstring tooling proved locally by this report.

[CHANGE]

- Prefer `Args:` in Python examples and capsule wording when the intent is Google style. Allow `Parameters:` only as a Griffe-supported alias or when a configured generator profile standardizes it.
- Tighten `Warns:` language so it applies only when warnings are part of a supported caller-visible contract. Rasm Python policy currently bans deprecation and workaround warning flows.
- Phrase annotation truth through `annotationlib`, `inspect.signature`, and strict type checkers rather than through older forward-reference lore.

[REMOVE]

- Remove any implication that `typing.Doc` or PEP 727 is an accepted Python standard.
- Remove any wording that presents Griffe or mkdocstrings output as current Rasm generated documentation until a manifest, generated page, or command route proves adoption.
- Remove any Python docstring example that repeats obvious parameter names, declared types, return carrier names, or validation branches already carried by source.

## [4][NO_CHANGE_CONFIRMATIONS]

- Keep Google docstrings as the Rasm Python docstring dialect.
- Keep PEP 257 as substrate, not the section syntax owner.
- Keep the core rule that source comments document caller-visible semantics the declaration cannot express.
- Keep the rule against type echo in `Args:` and `Returns:`.
- Keep the rule that `Result[T, E]`, `Option[T]`, and expression-style effect builders document success and failure semantics in `Returns:`, not fake `Raises:` entries.
- Keep mkdocstrings and Griffe as the preferred profile when generated Python API documentation is adopted.
- Keep generated Python API reference out of active claims until repository tooling proves a generator.

## [5][PROPOSED_PYTHON_CAPSULE_SHAPE]

Use this shape if a future standards edit revises the Python capsule:

```text template
Toolchain: Google docstrings for public Python modules, classes, functions, methods, properties, protocols, and package entrypoints. PEP 257 supplies docstring placement and layout. Annotations, signatures, strict type checkers, and annotation introspection own machine shape. Griffe and mkdocstrings are the preferred generated-reference profile only when the repo adopts generated Python API docs.

Comment owns:
- one-line summary that states behavior without name echo;
- extended summary for invariant, lifecycle, resource, cancellation, concurrency, security, data-exposure, schema, or interop semantics;
- `Args:` entries only for caller obligation, unit, ownership, accepted semantic range, trust boundary, or context requirement;
- `Returns:` entries for success payload, typed failure rail, effect boundary, resource ownership, or terminal behavior;
- `Yields:` for public generator output and ordering;
- `Receives:` only when a configured Griffe or mkdocstrings profile parses generator `send()` contracts;
- `Raises:`, `Attributes:`, `Warns:`, and `Examples:` only for supported exceptions, public attribute meaning, caller-visible warnings, or non-obvious call shape.
```

This shape preserves the active standard's semantics while separating Google style, PEP 257, local linting, and generator-profile behavior.

## [6][CONFIDENCE]

High for PEP 257, Google style, annotation/source-comment split, Ruff Google pydocstyle configuration, and the lack of local mkdocstrings or Griffe generation proof.

Medium for Python 3.15-specific wording because the checked upstream docs are current `3.15.0b2`, while the repo's configured Python target remains 3.14.

Medium for Griffe section support because the current docs clearly list supported sections, but actual rendered output depends on future mkdocstrings configuration.

## [7][PROOF_GAPS]

- No active standards were edited, so no active-corpus Markdown link or anchor validation was required.
- No Python lint, type, test, or generated-reference rail was run because this was report-only research.
- No mkdocstrings build was run because no local mkdocs or mkdocstrings adoption route was found.
- External sources were checked on 2026-06-05; re-check Python 3.15 and generator docs before editing active standards.

## [8][TRANSCRIPT_SUMMARY]

Commands run from `/Users/bardiasamiee/Documents/99.Github/Rasm`:
- memory registry search for prior `docs/standards` context;
- `fd`, `rg`, `wc`, `sed`, and `nl -ba` reads for required repo files and local configuration;
- `rg` and `fd` searches for Python docstring tooling and generator adoption;
- Context7 lookups for mkdocstrings-python and Griffe;
- Exa search for Python 3.15 docstring and annotation generator sources;
- web primary-source checks for Python, PEPs, Google style, Ruff, Griffe, and mkdocstrings;
- path-limited `git status --short` before writing.

Write action:
- Created `.reports/code-documentation-050626/track-python/01-python-docstrings-generators.md` only.
