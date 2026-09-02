---
name: python-document
description: "Use when ruff reports a D or DOC violation, or when a public Python module, class, or function needs a Google-style docstring written or converted."
---

# [PYTHON_DOCUMENT]

Covers Google-style docstrings on Python modules, classes, functions, and methods: which symbols get one, the summary and section layout, and the patterns for classes, generators, async functions, overloads, properties, and examples.

- `ruff check` enforces `convention = "google"` with `preview = true`: every `D` rule the convention keeps, `D420`, `D421`, and every `DOC` rule except the ignored `DOC502` fire, and section headers are `Name:` lines with no underline (`D416`)

## [01]-[SCOPE]

What gets a docstring:
- Every public module, package, class, nested class, method, and function (`D100` to `D104`, `D106`), files under `tests/**` skip `D101` to `D103` through per-file ignores and still need the module docstring
- `__init__` (`D107`), with the constructor parameters under `Args:`
- Magic methods (`__repr__`, `__eq__`), as a one-liner, `D105` fires on every undocumented one
- Private functions and methods (`_name`) with logic the name does not state, as a one-liner the `DOC` rules skip (`ignore-one-line-docstrings = true`), a multi-line private docstring has `Returns:`, `Yields:`, and `Raises:` because the `DOC` rules have no visibility filter
- Module docstrings at the top of the file, one sentence on the module contents

What not to document:
- Methods decorated with `typing.override` unless the override changes the base contract, `D102` exempts them and `ignore-decorators` lists the decorator
- `self`, `cls`, `*args`, and `**kwargs` under `Args:`, `ignore-var-parameters = true` exempts `*args` and `**kwargs` from `D417`
- Summaries that repeat the name (`"""Runs the job."""` on `run_job()`), the summary states the effect, return, or side effect the name and signature leave out

Existing work:
- Leave an accurate existing docstring alone, or improve its wording without changing meaning
- Convert a docstring in another convention (Sphinx `:param:` fields, NumPy underlines) to Google sections and preserve its content

## [02]-[FORMAT]

Formatting rules:
- Summary line: one sentence in the third person that states what the member does or returns (`Fetches rows`, not `Fetch rows` or `This function fetches`), with no filler, no hedge, and no restatement of the signature (`D402`), on the first physical line after the opening quotes (`D212`), ending with a period (`D415`), within the 300-column `E501` limit because `ruff format` never wraps docstring text, and in one style per file because Google also accepts the imperative and `D401` is off under `google`
- Blank lines: one between the summary and the description (`D205`), one before each section (`D411`), one between sections (`D410`), and no empty section (`D414`)
- Section headers: capitalized (`D405`), ending with a colon (`D416`), and followed by the first entry with no blank line between (`D412`)
- Indentation: section entries 4 spaces from the header and continuation lines 4 more (8 total), Google accepts 2 or 4 with one width per file, and the docstring body aligns with the opening quotes (`D207`, `D208`)

### [02.1]-[SECTIONS]

Use these sections and omit any that do not apply, `D420` orders `Args:`, then `Returns:` or `Yields:`, then `Raises:` and leaves the other sections unordered:

| [INDEX] | [SECTION]                 | [WHEN_TO_USE]                                                                                              |
| :-----: | :------------------------ | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `Args:`                   | Function has parameters, each one listed with a description (`D417`)                                       |
|  [02]   | `Returns:`                | Function returns a value (`DOC201`, a "Returns" summary satisfies it), none without `return` (`DOC202`)    |
|  [03]   | `Yields:`                 | Function contains `yield` (`DOC402`), replaces `Returns:`, a `Yields:` without `yield` fails `DOC403`      |
|  [04]   | `Raises:`                 | Body contains `raise` (`DOC501`), propagated exceptions a caller handles are allowed (`DOC502` is ignored) |
|  [05]   | `Note:` or `Notes:`       | Side effects, mutation of an argument, or a caveat the signature cannot show                               |
|  [06]   | `Example:` or `Examples:` | Usage a reader misuses without one                                                                         |
|  [07]   | `Attributes:`             | Attributes set in `__init__`, not properties, dataclass and model fields use inline docstrings             |

### [02.2]-[ONE_LINERS]

When the docstring needs no section, use a one-liner: opening and closing quotes on one line (`D200`), no blank line before it (`D201`) or after it (`D202`), and the `DOC` rules skip it (`ignore-one-line-docstrings = true`):

```python
def is_valid(self) -> bool:
    """Checks whether the configuration passes every validation rule."""
```

Docstrings that need a section or a description are multi-line, with the closing quotes on their own line (`D209`).

## [03]-[TYPE_ANNOTATIONS]

Type hints state the types, the docstring adds the meaning a hint cannot: no `(str)` after an argument name, no type before the `Returns:` text, and no signature in prose (`D402`):

```python
def connect(host: str, port: int, timeout: float = 30.0) -> Connection:
    """Opens a connection to the remote server.

    Args:
        host: Hostname or IP address.
        port: TCP port number.
        timeout: Seconds to wait before the attempt fails.

    Returns:
        The open connection ready for queries.
    """
```

## [04]-[FUNCTIONS]

The full layout, with a wrapped `Args:` entry on a hanging indent:

```python
def load_config(path: Path, *, strict: bool = False) -> Config:
    """Loads a configuration file and applies the environment overrides.

    Environment variables override the file values after parsing.

    Args:
        path: File to parse, a `.json` suffix selects the JSON parser and any other suffix selects the TOML parser.
        strict: Reject unknown keys instead of ignoring them.

    Returns:
        The merged configuration.

    Raises:
        ValueError: If the file contains an unknown key and `strict` is set.
    """
```

### [04.1]-[RETURNS]

Describe the shape of tuple and dict returns, and state when the function returns `None`:

```text
Returns:
    The tuple (matched_items, unmatched_items), where each element is a list of strings.

Returns:
    The dict mapping user IDs to profile data, where each value is a dict with keys "name", "email", and "role".

Returns:
    The parsed configuration, or None when the file does not exist.
```

Functions returning `expression.Result` describe the `Ok` value and each `Error` variant under `Returns:`, never under `Raises:`.

### [04.2]-[RAISES]

- List every exception the body raises with `raise` (`DOC501`, `NotImplementedError` excluded) and the propagated exceptions a caller handles, `DOC502` is ignored and a listed exception the body does not raise passes

Raises format:

```text
Raises:
    ValueError: If the input is not a valid ISO 8601 date string.
    FileNotFoundError: If the template path does not exist.
```

### [04.3]-[GENERATORS]

- `Yields:` replaces `Returns:` (`DOC402` on a missing one) and describes the item `next()` returns, not the generator object

The generator docstring:

```python
def read_chunks(path: Path, size: int = 8192) -> Iterator[bytes]:
    """Reads a file in fixed-size chunks.

    Yields:
        Chunks of at most `size` bytes.
    """
```

### [04.4]-[ASYNC]

- Async functions follow the same rules, the docstring does not repeat the `async` keyword

### [04.5]-[OVERLOADS]

- `@overload` signatures have no docstring and the implementation has one that covers every signature, `ignore-decorators` lists `typing.overload`, the `D` rules skip the overloads (`D418` never fires) and check the implementation alone

The docstring on the implementation:

```python
@overload
def parse(data: str) -> dict[str, object]: ...
@overload
def parse(data: bytes) -> dict[str, object]: ...


def parse(data: str | bytes) -> dict[str, object]:
    """Parses JSON input into a mapping.

    Accepts a JSON string or UTF-8 bytes.

    Args:
        data: JSON content as a string or bytes.

    Returns:
        The parsed mapping.
    """
```

### [04.6]-[DECORATORS]

- Document the effect of the decorator on the wrapped function, not the wrapper internals
- In a decorator that replaces the function, copy `__doc__` with `functools.wraps`, autodoc reads `__doc__` from the imported object and a wrapper without it hides the docstring

## [05]-[CLASSES]

Class docstrings open with what an instance represents (`"""The address of a shop."""`, not `"""Class that describes a shop address."""`), and an exception class states what the error represents, not when it occurs.

Regular class with `Attributes:` and an `__init__` docstring:

```python
class HTTPClient:
    """HTTP client with a connection pool and retries.

    Keeps persistent connections and retries failed requests with exponential backoff.

    Attributes:
        base_url: Root URL for every request path.
        timeout: Default request timeout in seconds.
        max_retries: Retry attempts per failed request.
    """

    def __init__(self, base_url: str, timeout: float = 30.0) -> None:
        """Initializes the client.

        Args:
            base_url: Root URL without a trailing slash.
            timeout: Default request timeout in seconds.
        """
        self.base_url = base_url
        self.timeout = timeout
        self.max_retries = 3
```

### [05.1]-[INLINE_ATTRIBUTES]

Dataclasses, Pydantic models, TypedDicts, and NamedTuples:
- Each field gets a string literal on the line after the field, autodoc reads that literal (or a `#:` comment before the field) as the attribute docstring
- The class docstring has no `Attributes:` entry for those fields, one field is documented in one place

Inline attribute docstrings on a dataclass:

```python
@dataclass
class SearchResult:
    """Search result with a relevance score."""

    url: str
    """The canonical URL of the result."""

    title: str
    """Page title with HTML entities decoded."""

    score: float
    """Relevance score between 0.0 and 1.0."""

    snippet: str | None = None
    """Extracted text snippet, None when the source has no text."""
```

### [05.2]-[ABSTRACT_BASE_CLASSES]

- Abstract methods document the interface contract, the `DOC` rules skip abstract methods and stub functions (`pass`, `...`, `raise NotImplementedError`), their `Returns:` and `Raises:` sections state the contract with no body to check

### [05.3]-[PROPERTIES]

Document properties like attributes: a noun phrase for the value with no leading verb (`D421`), no `Args:`, and no `Returns:` (`DOC201` exempts properties, and `property-decorators` adds `pydantic.computed_field` to `@property` and `functools.cached_property`), a setter documents like a method:

```python
@property
def is_expired(self) -> bool:
    """Whether the token expired."""

@name.setter
def name(self, value: str) -> None:
    """Sets the display name.

    Raises:
        ValueError: If the name is empty or exceeds 100 characters.
    """
```

## [06]-[EXAMPLES]

Rules:
- Code blocks indent 4 spaces within the section, and a blank line separates a block from the label before it and the entry after it
- Each example gets a label when the section holds more than one
- `ruff format` reformats `>>>` doctest lines, Markdown fences, and rST `::` blocks under `docstring-code-format = true` and skips a block that does not parse, an indented plain block stays as written
- Examples are indented plain blocks, not `>>>` lines: `addopts` has no `--doctest-modules` and `testpaths` is `tests`, nothing runs a doctest and no check detects a stale one

The docstring with labeled examples:

```python
def retry[**P, R](max_attempts: int = 3, backoff: float = 1.0) -> Callable[[Callable[P, R]], Callable[P, R]]:
    """Retries the decorated function after an exception.

    Args:
        max_attempts: Total attempts before the last exception propagates.
        backoff: Seconds between attempts, doubled after each failure.

    Examples:
        Defaults:

            @retry()
            def fetch_data():
                ...

        Explicit limits:

            @retry(max_attempts=5, backoff=2.0)
            def fragile_operation():
                ...
    """
```
