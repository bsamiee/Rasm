---
name: python-document
description: "Use when ruff reports a D or DOC violation, or a public Python module, class, or function needs a Google-style docstring written or converted."
---

# [PYTHON_DOCUMENT]

Covers Google-style docstrings on Python modules, classes, functions, and methods, from which symbols get one to the example layout.

`ruff check` enforces `convention = "google"` with `preview = true`: every `D` rule the convention keeps, `D420`, `D421`, and every `DOC` rule except the ignored `DOC502` fire, section headers are `Name:` lines with no underline (`D416`).

## [01]-[SCOPE]

What gets a docstring:
- Every public module, package, class, nested class, method, and function (`D100` to `D104`, `D106`), files under `tests/**` skip `D101` to `D103` through per-file ignores and need the module docstring
- `__init__` (`D107`), with the constructor parameters under `Args:`
- Magic methods (`__repr__`, `__eq__`) as a one-liner, `D105` fires on every undocumented one
- Private functions and methods (`_name`) with logic the name does not state, as a one-liner the `DOC` rules skip (`ignore-one-line-docstrings = true`), a multi-line private docstring has `Returns:`, `Yields:`, and `Raises:` because the `DOC` rules have no visibility filter
- Module docstrings at the top of the file, one sentence on the module contents

Exemptions:
- Methods decorated with `typing.override` unless the override changes the base contract, `D102` exempts them and `ignore-decorators` lists the decorator
- `self`, `cls`, `*args`, and `**kwargs` under `Args:`, `ignore-var-parameters = true` exempts `*args` and `**kwargs` from `D417`
- A summary that repeats the name, the summary states the effect, return, or side effect the name and signature leave out

An accurate existing docstring stays. A docstring in another convention (Sphinx `:param:` fields, NumPy underlines) converts to Google sections with its content preserved.

## [02]-[FORMAT]

- Summary line: one sentence in the third person that states what the member does or returns (`Fetches rows`), on the first physical line after the opening quotes (`D212`), ending with a period (`D415`). `D402` rejects a signature restated in the summary. `ruff format` never wraps docstring text, so the line stays within the 300-column `E501` limit. Google accepts the imperative and `D401` is off under `google`, so a file keeps one style
- Blank lines: one between the summary and the description (`D205`), one before each section (`D411`), one between sections (`D410`), no empty section (`D414`)
- Section headers: capitalized (`D405`), ending with a colon (`D416`), followed by the first entry with no blank line between (`D412`)
- Indentation: section entries 4 spaces from the header and continuation lines 4 more, one width per file, the docstring body aligns with the opening quotes (`D207`, `D208`)

### [02.1]-[SECTIONS]

`D420` orders `Args:`, then `Returns:` or `Yields:`, then `Raises:`, and leaves the other sections unordered:

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

A docstring that needs no section is a one-liner: opening and closing quotes on one line (`D200`), no blank line before it (`D201`) or after it (`D202`), the `DOC` rules skip it (`ignore-one-line-docstrings = true`):

```python
def is_valid(self) -> bool:
    """Checks whether the configuration passes every validation rule."""
```

A docstring that needs a section or a description is multi-line, with the closing quotes on their own line (`D209`).

## [03]-[TYPE_ANNOTATIONS]

Type hints state the types, the docstring adds the meaning a hint cannot: no `(str)` after an argument name, no type before the `Returns:` text, no signature in prose (`D402`):

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

A tuple or dict return describes its shape, a function that can return `None` states when:

```text
Returns:
    The tuple (matched_items, unmatched_items), where each element is a list of strings.

Returns:
    The dict mapping user IDs to profile data, where each value is a dict with keys "name", "email", and "role".

Returns:
    The parsed configuration, or None when the file does not exist.
```

A function returning `expression.Result` describes the `Ok` value and each `Error` variant under `Returns:`.

### [04.2]-[RAISES]

Every exception the body raises with `raise` is listed (`DOC501`, `NotImplementedError` excluded) with the propagated exceptions a caller handles, `DOC502` is ignored and a listed exception the body does not raise passes:

```text
Raises:
    ValueError: If the input is not a valid ISO 8601 date string.
    FileNotFoundError: If the template path does not exist.
```

### [04.3]-[GENERATORS]

`Yields:` replaces `Returns:` (`DOC402` on a missing one) and describes the item `next()` returns:

```python
def read_chunks(path: Path, size: int = 8192) -> Iterator[bytes]:
    """Reads a file in fixed-size chunks.

    Yields:
        Chunks of at most `size` bytes.
    """
```

### [04.4]-[ASYNC]

An async function follows the same rules, the docstring does not repeat the `async` keyword.

### [04.5]-[OVERLOADS]

`@overload` signatures have no docstring, the implementation has one that covers every signature, `ignore-decorators` lists `typing.overload`, the `D` rules check the implementation alone:

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

A decorator documents its effect on the wrapped function. A decorator that replaces the function copies `__doc__` with `functools.wraps`, autodoc reads `__doc__` from the imported object.

## [05]-[CLASSES]

A class docstring opens with what an instance represents (`"""The address of a shop."""`), an exception class states what the error represents:

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
- The class docstring has no `Attributes:` entry for the fields

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

An abstract method documents the interface contract, the `DOC` rules skip abstract methods and stub functions (`pass`, `...`, `raise NotImplementedError`), their `Returns:` and `Raises:` sections state the contract.

### [05.3]-[PROPERTIES]

A property documents like an attribute: a noun phrase for the value with no leading verb (`D421`), no `Args:`, no `Returns:` (`DOC201` exempts properties, `property-decorators` adds `pydantic.computed_field` to `@property` and `functools.cached_property`), a setter documents like a method:

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

- A code block indents 4 spaces within the section, a blank line separates a block from the label before it and the entry after it
- Each example gets a label when the section holds more than one
- `ruff format` reformats `>>>` doctest lines, Markdown fences, and rST `::` blocks under `docstring-code-format = true` and skips a block that does not parse, an indented plain block stays as written
- Examples are indented plain blocks, `addopts` has no `--doctest-modules`, nothing runs a doctest

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
