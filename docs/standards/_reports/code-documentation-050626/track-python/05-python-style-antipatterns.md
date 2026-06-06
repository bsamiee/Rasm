# [PYTHON_05_STYLE_ANTIPATTERNS]

This report studies Python docstring style anti-patterns for `docs/standards/reference/code-documentation.md`. It does not edit active standards in-place; it records source-backed findings, candidate changes, no-change confirmations, and rejected patterns for a later standards pass.

## [1][SCOPE]

Assigned focus: `PYTHON 5`, Python docstring anti-patterns and style: type echo, `Raises:` misuse, property and attribute docs, module and package docs, deprecations, warnings, and examples.

Assigned output: `docs/standards/_reports/code-documentation-050626/track-python/05-python-style-antipatterns.md`.

Active standards read fully:
- `docs/standards/reference/code-documentation.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/style-guide.md`
- `docs/standards/information-structure.md`
- `docs/standards/proof.md`
- `docs/standards/formatting.md`

Repo instruction context checked:
- `CLAUDE.md`
- root `AGENTS.md`
- `~/.codex/memories/MEMORY.md` entries for the June 4-5 `docs/standards` rebuild, `_reports/` handling, active-corpus boundaries, and docs-only validation.

Current workspace note: `docs/standards/reference/code-documentation.md` was already modified in the worktree before this report. This report leaves that active file untouched.

## [2][CURRENT_STANDARD_SNAPSHOT]

The active Python capsule already has the right governing stance:
- Google docstrings are the public Python style.
- PEP 257 supplies the docstring substrate.
- Annotations, strict type checkers, and generated-reference tooling own type shape.
- `Args:`, `Returns:`, `Yields:`, `Receives:`, `Raises:`, `Attributes:`, `Warns:`, and `Examples:` are semantic sections, not mandatory headers.
- Type echo, blanket parameter documentation, broad `Raises:`, missing-annotation compensation, schema prose duplication, and mixed section dialects are rejected.

The main gap is operational sharpness. The capsule names anti-patterns, but a later edit can make each one mechanically testable so agents can decide whether to keep, rewrite, route, or delete a docstring section.

## [3][SOURCES_CHECKED]

[LOCAL_REPO]:
- `docs/standards/reference/code-documentation.md`: active code-comment and Python capsule doctrine.
- `docs/standards/README.md`: document-type routing, placement, split/link, and lifecycle.
- `docs/standards/AGENTS.md`: local standards-folder overlay, read scope, owner routing, and forbidden process metadata.
- `docs/standards/style-guide.md`: prose shape, examples, code-safe Markdown, and final proofing.
- `docs/standards/information-structure.md`: tables, definition blocks, examples, and record shape.
- `docs/standards/proof.md`: source freshness, evidence fields, and docs-only gate selection.
- ``: salience, artifact separation, generated mirrors, and provider-proof discipline.
- `docs/standards/formatting.md`: heading idiom, table surface, section markers, and whitespace.
- `CLAUDE.md` and root `AGENTS.md`: repo-level docs routing and the instruction to avoid static, test, or bridge proof claims for docs-only work.

[PRIMARY_OR_STABLE_EXTERNAL]:
- Python 3.14.5 documentation, `warnings`: `warnings.warn`, warning filters, default `DeprecationWarning` visibility, `skip_file_prefixes`, and `warnings.deprecated`. Source: <https://docs.python.org/3.14/library/warnings.html>.
- Python 3.14.5 documentation, `doctest`: interactive examples in docstrings are executable examples when doctest is used. Source: <https://docs.python.org/3.14/library/doctest.html>.
- Python 3.14.5 documentation, `property`: `@property` sets the property docstring from the getter and property docstrings are writable. Source: <https://docs.python.org/3.14/library/functions.html#property>.
- Python 3.14.5 documentation, `pydoc`: docstrings are a runtime and local documentation surface. Source: <https://docs.python.org/3.14/library/pydoc.html>.
- PEP 257, `Docstring Conventions`: active stable docstring convention; docstrings are first-statement object docs, attribute docstrings are extractable by tools, one-line docstrings must not repeat signatures, module and package docstrings summarize exported surfaces, and function docstrings document arguments, return values, side effects, exceptions, and call restrictions when applicable. Source: <https://peps.python.org/pep-0257/>.
- Google Python Style Guide, `Comments and Docstrings`: current stable style source for Google-style docstrings, module docstrings, function sections, property wording, attributes, `Raises:`, and comments. Source: <https://google.github.io/styleguide/pyguide.html>.
- Sphinx `sphinx.ext.napoleon`: supported Google and NumPy docstring sections, including `Args`, `Attributes`, `Examples`, `Raises`, `Warns`, `Yields`, and section aliases. Source: <https://www.sphinx-doc.org/en/master/usage/extensions/napoleon.html>.
- Griffe docstring recommendations: Griffe supports Google-style parsing, module/class/function/property/attribute/type-alias docstrings, `Receives:`, `Warns:`, and examples parsed as Python-console blocks in `Examples:`. Source: <https://mkdocstrings.github.io/griffe/guide/users/recommendations/docstrings/>.
- PEP 702: historical standards-track basis for `warnings.deprecated`; current canonical behavior now lives in Python docs and the typing specs. Source: <https://peps.python.org/pep-0702/>.

## [4][FINDINGS]

### [4.1][TYPE_ECHO]

Type echo is the highest-value Python anti-pattern to keep explicit. PEP 257 rejects one-line docstrings that repeat a function signature because introspection can recover that shape. Google narrows the same rule for typed Python: `Args:` descriptions include type information only when the code lacks a corresponding annotation, and `Returns:` includes type information only when annotations do not provide it.

Candidate stronger rule:
- If an annotation carries the type, the docstring states only semantic delta: unit, range, accepted domain, trusted boundary, ownership, default meaning, side effect, or absence semantics.
- If an annotation is missing by necessity, the docstring may name the type shape only as a temporary surface fact and should also name the proof gap or annotation blocker when the repo expects annotations.
- If a section says only `foo: str`, `Returns: list[str]`, `Result[T, E]`, or `None`, delete it or move the semantic failure or success meaning into the section.

Accepted: `path: Repository-relative path; absolute paths are rejected before filesystem access.`
Rejected: `path: str.`
Reason: the accepted text names the caller obligation that `str` cannot carry.

Confidence: high. This is directly supported by PEP 257, Google Python style, and the current Rasm source-comment doctrine.

### [4.2][RAISES_MISUSE]

`Raises:` should name only exceptions relevant to the public interface. Google explicitly rejects documenting exceptions raised by violating the API contract because that would make out-of-contract behavior part of the interface. Rasm's result-oriented doctrine adds a second boundary: typed rails returned as data belong in `Returns:`, not `Raises:`.

Candidate stronger rule:
- Use `Raises:` only for actual exposed exceptions that a valid caller may need to catch.
- Do not document validation failures returned as `Result`, `Option`, accumulated validation, status objects, or other typed data in `Raises:`.
- Do not document `ValueError`, `TypeError`, `AssertionError`, or parser errors caused only by violating preconditions already stated in `Args:`.
- Do not put warnings in `Raises:`; use `Warns:` only when emitted warnings are part of the public surface.
- For `ExceptionGroup`, document it only when grouped exceptions are part of the supported boundary, not when it is an internal concurrency implementation detail.

Accepted: `Raises: OSError: when the configured cache directory cannot be opened.`
Rejected: `Raises: ValueError: when mode is not one of the documented modes.`
Reason: the accepted exception can occur for a valid caller; the rejected entry turns contract violation into API.

Confidence: high. This is directly supported by Google Python style and current Rasm rail-vs-throwing guidance.

### [4.3][PROPERTY_DOCS]

Property docstrings should read like attribute documentation, not function documentation. Google says a `@property` data descriptor should use the style of an attribute or function argument, and the Python `property` docs show the getter docstring becoming the property docstring. Google also says property behavior should match ordinary attribute-access expectations: cheap, straightforward, and unsurprising.

Candidate stronger rule:
- A property docstring states the value meaning, unit, absence semantics, cache/laziness contract, mutation effect, or security/data-exposure boundary when the property exposes one.
- It does not start with `Returns`, does not describe getter mechanics, and does not document `self`.
- If property access is expensive, mutating, blocking, or surprising, prefer a method surface or document the caller-visible boundary explicitly when the property is already a public contract.
- Setter and deleter behavior is documented only when the public property contract includes mutation validation, invalidation, resource release, or externally visible side effects.

Accepted: `"""Canonical project root after symlink normalization."""`
Rejected: `"""Returns the project root string."""`
Reason: the accepted property doc names the value contract; the rejected doc echoes getter shape and return type.

Confidence: high. This is supported by Python `property`, Google properties, and Google docstring sections.

### [4.4][ATTRIBUTE_DOCS]

Attribute documentation has two valid shapes that should not be mixed accidentally. PEP 257 recognizes attribute docstrings after simple assignments as extractable by tools, and Griffe documents module, class, and instance attribute docstrings placed immediately after assignments. Google instead documents public class attributes in an `Attributes:` section, excluding properties.

Candidate stronger rule:
- Use per-attribute docstrings when the configured generated-reference profile extracts them and when the attribute itself is the public surface.
- Use a class `Attributes:` section when documenting a compact set of public instance attributes and the generator/profile expects Google-style class sections.
- Do not duplicate the same attribute in both a per-assignment docstring and a class `Attributes:` section unless one route is generated and the other is intentionally omitted.
- Do not document private attributes, storage fields, or schema fields in source docstrings when annotations, dataclass/Pydantic/msgspec metadata, or generated schema owns the field shape.

Accepted: `timeout_seconds: request deadline in seconds; zero disables retries.`
Rejected: `timeout_seconds: float.`
Reason: the accepted text adds unit and sentinel meaning; the rejected text repeats annotation shape.

Confidence: medium-high. The source evidence is strong, but the final rule should reflect whichever generator profile the repo actually adopts.

### [4.5][MODULE_PACKAGE_DOCS]

Module and package docstrings should describe contents and usage at the public entrypoint, not become local READMEs, command catalogs, architecture pages, or task guides. PEP 257 says all modules normally have docstrings, exported functions/classes should have docstrings, and a package may be documented in the `__init__.py` docstring. Google says files should start with a docstring describing contents and usage, while test module docstrings are needed only when they add information.

Candidate stronger rule:
- Public package entrypoints use module/package docstrings to state the import surface, purpose, and primary usage semantics.
- Ordinary internal modules may omit docstrings when source names, annotations, and package docs already carry the public contract.
- Test module docstrings appear only for unusual setup, external dependencies, execution details, or fixture contracts that a test reader cannot infer.
- Do not put command catalogs, full package maps, generated API lists, architecture invariants, runbook triggers, or tutorial steps in module docstrings; route them to README, generated API, reference, architecture, runbook, how-to, or tutorial.

Accepted: `"""Public quality-tool command entrypoint for static, test, bridge, and API rails."""`
Rejected: a module docstring that lists every function in the file and repeats all CLI flags.
Reason: the accepted summary identifies the public entrypoint; the rejected catalog belongs to generated API or reference docs.

Confidence: high for route-away. The only nuance is that PEP 257's "all modules should normally have docstrings" is broader than Rasm's anti-spam doctrine; the local standard should keep the public/caller-visible threshold.

### [4.6][DEPRECATIONS]

Deprecation should be marker-backed when it is a public Python support contract. Python 3.14 documents `warnings.deprecated` as a decorator for classes, functions, and overloads; runtime warnings may be emitted, static type checkers generate diagnostics, and the message is saved in `__deprecated__`. PEP 702 is now historical and points readers to current docs and typing specs.

Candidate stronger rule:
- Use `warnings.deprecated` when a public Python class, function, method, or overload remains callable but external callers must migrate.
- The deprecation message names replacement path, behavior delta, and removal or migration condition when known.
- A docstring may explain migration-relevant behavior, but it should not be the only deprecation signal when static or runtime consumers need the warning.
- Internal greenfield stale surfaces should be deleted or replaced, not preserved with docstring-only `Deprecated:` prose.
- If `category=None` is used to disable runtime warnings, the support reason should be explicit because static checker behavior is unaffected.

Accepted: `@deprecated("Use load_plan() after resolving the project root.")`
Rejected: `"""Deprecated."""`
Reason: the accepted marker reaches tooling and names a replacement; the rejected docstring has no support route.

Confidence: high. This aligns with Python 3.14 docs, PEP 702, `code-documentation.md` lifecycle rules, and the repo greenfield policy.

### [4.7][WARNINGS]

Warnings are not exceptions, and public warnings need a distinct docstring route. Python warnings can be ignored, shown, or converted to errors by filters. `DeprecationWarning` is ignored by default outside `__main__`, and Python recommends making ignored warnings visible during dependency-update testing. Griffe explicitly supports documenting warnings via `Warns:`.

Candidate stronger rule:
- Use `Warns:` only for warnings a valid caller may observe, filter, escalate, or test against.
- The entry names the warning category and the condition that emits it.
- Do not use `Warns:` for internal diagnostics, logging, traces, or warnings emitted only by dependency internals unless the wrapper intentionally exposes them.
- If the warning is a deprecation warning for the symbol itself, prefer `warnings.deprecated` as the primary signal and use `Warns:` only when the call also emits a behavior-specific warning.
- When wrapper depth is caller-visible, source comments may mention that warning attribution is intentionally shifted by `stacklevel` or `skip_file_prefixes`.

Accepted: `Warns: ResourceWarning: when a live handle is abandoned without closing.`
Rejected: `Raises: ResourceWarning: if cleanup is skipped.`
Reason: warning filters, not exception handling, control the observable channel unless filters convert it to an error.

Confidence: high. This is supported by Python warnings docs and Griffe Google-style recommendations.

### [4.8][EXAMPLES]

Examples in Python docstrings are a contract surface when doctest or the generated-reference parser consumes them. Python `doctest` searches text that looks like interactive Python sessions and executes it to verify exact output. Griffe parses `>>>` and `...` blocks as code blocks inside plural `Examples:` sections. Google allows module usage examples and function examples when they clarify use.

Candidate stronger rule:
- Add `Examples:` only when it prevents a likely semantic misuse, demonstrates non-obvious lifecycle/failure handling, or provides a minimal public call shape that prose cannot express.
- Keep doctest-shaped examples deterministic, side-effect bounded, and exact if the repo may run doctest.
- Prefer explicit fenced `pycon` examples when the renderer needs a stable code-block route.
- Do not use examples as mini tutorials, command transcripts, generated output dumps, broad happy paths, or unverified tests.
- Do not hide required semantics only in an example; the docstring prose still owns the contract.

Accepted: an example showing that `require_all_keys=False` omits missing keys from the returned mapping.
Rejected: an example that only calls `load_config(path)` with no visible semantic distinction.
Reason: the accepted example prevents a caller mistake; the rejected example repeats invocability.

Confidence: high. This aligns with doctest, Griffe, Google, and the local examples rule.

### [4.9][SECTION_DIALECT]

Mixed section dialects are a real parser risk. Sphinx Napoleon supports many aliases, but Rasm's active capsule says Google docstrings. Allowing every Sphinx/Napoleon alias would make generated parsing looser and harder to review.

Candidate stronger rule:
- Use one Google-style dialect per docstring.
- Prefer the canonical section names used by the active capsule: `Args:`, `Returns:`, `Yields:`, `Receives:`, `Raises:`, `Attributes:`, `Warns:`, and `Examples:`.
- Do not mix NumPy-style underlined sections, Sphinx field lists, Napoleon aliases, Markdown headings, and Google sections inside one docstring.
- Treat `Parameters:`, `Return:`, `Warnings:`, and alias forms as parser compatibility facts, not preferred authoring style, unless a configured generator requires them.

Confidence: medium-high. Sphinx supports many aliases, but local consistency and generated-reference stability favor a narrower authoring set.

### [4.10][DOCSTRING_OR_COMMENT]

Python docstrings and comments serve different readers. Google says caller-relevant function use belongs in the docstring, while subtle implementation details that are not relevant to the caller belong as comments near the code. The active Rasm standard already reserves inline comments for non-obvious rationale.

Candidate stronger rule:
- Put public caller contract in the docstring.
- Put implementation rationale near the code path it explains.
- Route architecture, task, operation, and generated-reference facts away from source comments.
- Delete comments that narrate Python syntax or repeat the next line.

Confidence: high. This aligns with Google, PEP 257, and local standards.

## [5][ADD_RECOMMENDATIONS]

Add a compact Python anti-pattern decision record under the Python capsule:

```text template
Python docstring section: `<summary, Args, Returns, Yields, Receives, Raises, Attributes, Warns, Examples>`
Machine truth already present: `<annotation, signature, property, schema metadata, warning/deprecated marker, generated parser, or omit>`
Semantic delta: `<unit, range, ownership, absence, side effect, failure, warning, lifecycle, example distinction, or omit>`
Reject when: `<type echo, carrier echo, contract-violation Raises, docstring-only deprecation, warning in Raises, dialect mix, or tutorial route>`
Route-away: `<generated API, README, reference, architecture, support matrix, runbook, how-to, tutorial, or omit>`
```

Add a Python `Raises/Warns split` rule:
- `Raises:` is for valid-caller exceptions.
- `Warns:` is for emitted warning categories that callers may filter, escalate, or test.
- Returned failure data belongs in `Returns:`.

Add a Python `property and attribute docs` rule:
- Property docstrings read like attribute value contracts.
- Class `Attributes:` sections and per-assignment attribute docstrings are alternate generated-reference shapes; avoid duplicating one fact in both.

Add a Python `deprecation marker` rule:
- `warnings.deprecated` is the primary marker for public symbol deprecation when static or runtime consumers need the warning.
- Docstring prose can explain migration semantics but must not be the only deprecation mechanism for a public support contract.

Add a Python `examples` rule:
- Examples exist to prevent semantic misuse or prove non-obvious call shape.
- Doctest-shaped examples must be deterministic and exact when doctest may consume them.

## [6][CHANGE_RECOMMENDATIONS]

Change the Python capsule's `Reject` list from a broad phrase list into grouped anti-patterns:

[MACHINE_SHAPE_ECHO]:
- Type echo in `Args:` or `Returns:`.
- Signature echo in summary lines.
- Carrier echo such as `Result[T, E]` without success and failure meaning.
- Property docs that say `Returns <type>`.

[CHANNEL_MISROUTING]:
- `Raises:` for typed rails, validation data, or precondition violations.
- `Raises:` for warnings.
- `Warns:` for internal diagnostics or dependency warnings not intentionally exposed.
- Docstring-only deprecation where tooling needs `warnings.deprecated`.

[SURFACE_DUPLICATION]:
- Attribute facts duplicated between assignment docstrings, class `Attributes:`, schema metadata, and generated field descriptions.
- Module docstrings that become README, command catalog, architecture, runbook, how-to, or tutorial bodies.
- Examples that become task guides or unverified transcripts.

[PARSER_DIALECT_DRIFT]:
- NumPy/Sphinx/Google section mixing inside one Google docstring.
- Alias section names treated as preferred style when they are only parser compatibility.
- Markdown headings inside docstrings where Google section headers should carry structure.

## [7][REMOVE_RECOMMENDATIONS]

Remove any future wording that implies:
- every parameter in a public function needs an `Args:` entry even when name, annotation, and summary are enough;
- every return annotation needs a `Returns:` section;
- every possible `ValueError` or `TypeError` should appear in `Raises:`;
- properties should be documented as functions;
- warnings are a subtype of exceptions for docstring routing;
- `Deprecated:` prose in a docstring is equivalent to `warnings.deprecated`;
- module docstrings should hand-maintain generated package catalogs;
- examples are required for ordinary invocability.

## [8][NO_CHANGE_CONFIRMATIONS]

Keep these active-standard positions:
- Google docstrings remain the Python source-comment style.
- PEP 257 remains the docstring substrate.
- Annotations, strict type checkers, and generated-reference tooling own type shape.
- Docstrings carry caller-visible semantics, not private implementation narration.
- `Raises:` documents intentionally exposed exceptions only.
- `Warns:` is valid for public warning behavior.
- Attribute, property, schema, and generated-reference facts should not be duplicated in prose.
- Lifecycle signals apply only to external support contracts; greenfield internal stale surfaces should be deleted or replaced.
- Examples stay conditional and misuse-preventing.

## [9][SOURCE_LIST]

- Python 3.14.5 `warnings`: <https://docs.python.org/3.14/library/warnings.html>
- Python 3.14.5 `doctest`: <https://docs.python.org/3.14/library/doctest.html>
- Python 3.14.5 `property`: <https://docs.python.org/3.14/library/functions.html#property>
- Python 3.14.5 `pydoc`: <https://docs.python.org/3.14/library/pydoc.html>
- PEP 257: <https://peps.python.org/pep-0257/>
- PEP 702: <https://peps.python.org/pep-0702/>
- Google Python Style Guide: <https://google.github.io/styleguide/pyguide.html>
- Sphinx Napoleon: <https://www.sphinx-doc.org/en/master/usage/extensions/napoleon.html>
- Griffe docstring recommendations: <https://mkdocstrings.github.io/griffe/guide/users/recommendations/docstrings/>

## [10][VALIDATION_NOTES]

- Report file added only at `docs/standards/_reports/code-documentation-050626/track-python/05-python-style-antipatterns.md`.
- Active standards were not edited by this worker.
- External sources were checked on 2026-06-05.
- Docs-only validation should be limited to Markdown diff hygiene for this report path unless a later editor changes active standards.
