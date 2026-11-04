---
name: python-document
description: "Use when writing or reviewing Python docstrings or fixing ruff D or DOC findings, covering scope and Google-style syntax"
---

# [PYTHON_DOCUMENT]

Covers Google-style docstrings on Python modules, classes, functions, and methods.

Ruff settings in `pyproject.toml` fire every `D` rule the convention keeps and, under `preview`, `D420`, `D421`, and every `DOC` rule:

```!
yq -p toml -o toml '{"tool": {"ruff": (.tool.ruff | {"preview": .preview, "format": (.format | pick(["docstring-code-format"])), "lint": (.lint | pick(["select", "pycodestyle", "pydocstyle", "pydoclint"]))})}}' "${CLAUDE_PROJECT_DIR}/pyproject.toml"
```

## [01]-[SCOPE]

Members that take a docstring:
- Every public module, package, class, nested class, method, and function (`D100` to `D104`, `D106`)
- `__init__` (`D107`), with the constructor parameters under `Args:`
- Magic methods (`__repr__`, `__eq__`) as a one-liner, `D105` fires on every undocumented one
- Private functions and methods (`_name`) with logic the name does not state, as a one-liner
- Multi-line private docstrings keep `Returns:`, `Yields:`, and `Raises:`, `DOC` rules have no visibility filter
- Module docstrings at file top, one sentence on module contents
- `*args` and `**kwargs` under `Args:` (`D417`)

Exemptions:
- Methods decorated with `typing.override` unless the override changes the base contract, `D102` skips them
- `self` and `cls` under `Args:`

Accurate existing docstrings stay. Docstrings in another convention (Sphinx `:param:` fields, NumPy underlines) convert to Google sections with their content preserved.

## [02]-[FORMAT]

- Summary line: one sentence stating what the member does or returns, descriptive (`Fetches rows`) or imperative, one mood per file (`D401` off)
- Summary sits on first line after opening quotes (`D212`) and ends with a period (`D415`)
- `ruff format` never wraps docstring text, the summary line stays within the `E501` limit
- Blank lines: one after the summary (`D205`), one before each section (`D411`) and between sections (`D410`), no empty section (`D414`)
- Section headers: capitalized (`D405`), ending with a colon (`D416`), followed by the first entry with no blank line between (`D412`)
- Indentation: section entries 4 spaces from the header and continuation lines 4 more, body aligned with opening quotes (`D207`, `D208`)

### [02.1]-[SECTIONS]

`D420` orders `Args:`, then `Returns:` or `Yields:`, then `Raises:`, and leaves other sections unordered:

| [INDEX] | [SECTION]                 | [WHEN]                                                                                                  |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Args:`                   | Function has parameters, each one listed with a description (`D417`)                                    |
|  [02]   | `Returns:`                | Function returns a value (`DOC201`, a "Returns" summary satisfies it), none without `return` (`DOC202`) |
|  [03]   | `Yields:`                 | Function contains `yield` (`DOC402`), replaces `Returns:`, a `Yields:` without `yield` fails `DOC403`   |
|  [04]   | `Raises:`                 | Body contains `raise` (`DOC501`)                                                                        |
|  [05]   | `Note:` or `Notes:`       | Side effects, mutation of an argument, or a caveat the signature cannot show                            |
|  [06]   | `Example:` or `Examples:` | Usage a reader misuses without one                                                                      |
|  [07]   | `Attributes:`             | Attributes set in `__init__`, not properties                                                            |

### [02.2]-[ONE_LINERS]

Docstrings that need no section are one-liners: opening and closing quotes on one line (`D200`), no blank line before (`D201`) or after (`D202`), `DOC` rules skip them (`ignore-one-line-docstrings`):

```python
def is_valid(self) -> bool:
    """Checks whether the configuration passes every validation rule."""
```

Docstrings that need a section or a description are multi-line, with closing quotes on their own line (`D209`).

## [03]-[TYPE_ANNOTATIONS]

Docstrings add meaning a type hint cannot state, with no `(str)` after an argument name, no type before `Returns:` text, and no signature in prose (`D402`):

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

### [04.1]-[RETURNS]

Tuple and dict returns describe their shape, functions that can return `None` state when:

```text
Returns:
    The tuple (matched_items, unmatched_items), where each element is a list of strings.

Returns:
    The dict mapping user IDs to profile data, where each value is a dict with keys "name", "email", and "role".

Returns:
    The parsed configuration, or None when the file does not exist.
```

Functions returning `expression.Result` state the `Ok` value and each fault reason, in the one-line summary or under `Returns:`.

### [04.2]-[RAISES]

`Raises:` lists each exception a `raise` statement in the body throws (`DOC501`, `NotImplementedError` excluded), `DOC502` reports an entry no `raise` throws:

```text
Raises:
    ValueError: If the input is not a valid ISO 8601 date string.
    FileNotFoundError: If the template path does not exist.
```

### [04.3]-[GENERATORS]

`Yields:` describes the item `next()` returns:

```python
def read_chunks(path: Path, size: int = 8192) -> Iterator[bytes]:
    """Reads a file in fixed-size chunks.

    Yields:
        Chunks of at most `size` bytes.
    """
```

### [04.4]-[ASYNC]

Async function docstrings omit the `async` keyword.

### [04.5]-[OVERLOADS]

Implementations hold one docstring covering every `@overload` signature, `D418` reports a docstring on an `@overload` signature:

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

Decorators document their effect on the wrapped function. Decorators that replace the function copy `__doc__` with `functools.wraps`.

## [05]-[CLASSES]

Class docstrings open with what an instance represents (`"""The address of a shop."""`), exception classes state what the error represents.

### [05.1]-[FIELDS]

Field classes (`msgspec.Struct`, attrs classes, Pydantic models, TypedDicts) document fields by name and type alone with no `Attributes:` entry, ast-grep rule `no-attribute-docstring` reports a string after an assignment or type alias:

```python
class SearchResult(msgspec.Struct, frozen=True):
    """Search result with a relevance score."""

    url: str
    title: str
    score: float
    snippet: str | None = None
```

### [05.2]-[ABSTRACT_BASE_CLASSES]

Abstract methods and stub functions (`pass`, `...`, `raise NotImplementedError`) state the interface contract in `Returns:` and `Raises:`, `DOC` rules skip both.

### [05.3]-[PROPERTIES]

Property docstrings are a noun phrase with no leading verb (`D421`) and no `Args:` or `Returns:` (`DOC201` exempts `@property` and `functools.cached_property`). Setters document like methods:

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

- Code blocks indent 4 spaces within the section, a blank line separates a block from the label before it and the entry after it
- Each example gets a label when the section holds more than one
- `docstring-code-format` makes `ruff format` reformat `>>>` doctest lines, Markdown fences, and rST `::` blocks that parse
- Examples are indented plain blocks, `ruff format` leaves them as written
