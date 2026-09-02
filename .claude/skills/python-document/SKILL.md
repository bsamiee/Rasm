---
name: python-document
description: "Use when ruff reports a D or DOC violation, or when a public Python module, class, or function needs a Google-style docstring written or converted."
---

# [PYTHON_DOCUMENT]

Covers Google-style docstrings on Python modules, classes, functions, and methods: which symbols get one, the summary and section layout, and the patterns for classes, generators, async functions, overloads, properties, and examples.

- Use Google style for all docstrings
- Use plain-text section headers (`Args:`, `Returns:`) with indented descriptions, not Sphinx `:param:` or NumPy-style underlines

## [01]-[SCOPE]

What gets a docstring:
- Every public module, class, function, and method (`ruff` rules `D100` to `D107`)
- `__init__` (`D107`), with the constructor parameters under `Args:`
- Private functions and methods (`_name`) with non-obvious logic, as a one-liner, not one-liner helpers that do what their name says, a private multi-line docstring follows the same section rules (`DOC201` has no visibility filter)
- Dunder methods (`__repr__`, `__str__`, `__eq__`) only when they have surprising behavior
- Module docstrings at the top of the file, one or two sentences on the module's purpose

What not to document:
- Parameters obvious from name and type (`self`, `cls`)
- Boilerplate docstrings that say nothing (`"""Does the thing."""` on `do_thing()`)

Existing work:
- Leave an accurate existing docstring alone, or improve its wording without changing meaning
- Convert a docstring in another convention (Sphinx, NumPy) to Google style and preserve its content

## [02]-[FORMAT]

Formatting rules:
- Summary line: descriptive style in the third person ("Fetches rows", not "Fetch rows" or "This function fetches"), ends with a period, and fits on one line without wrapping
- Blank line between summary and extended description, and between extended description and first section
- Section headers end with a colon and have no blank line before the first entry
- Indentation: section content 4 spaces from the section header, continuation lines within an entry 4 more (8 total)
- One blank line between sections

### [02.1]-[SECTIONS]

Use these sections in this order and omit any that do not apply:

| [INDEX] | [SECTION]                 | [WHEN_TO_USE]                                                                                              |
| :-----: | :------------------------ | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `Args:`                   | Function/method has parameters (excluding `self`/`cls`), every parameter once present (`D417`)             |
|  [02]   | `Returns:`                | Function returns a value the summary does not state (`DOC201`, a summary opening with "Returns" is exempt) |
|  [03]   | `Yields:`                 | Function is a generator (use instead of `Returns`)                                                         |
|  [04]   | `Raises:`                 | Function raises exceptions directly (`DOC501`, `DOC502`)                                                   |
|  [05]   | `Note:` or `Notes:`       | Important caveats, side effects, or non-obvious behavior                                                   |
|  [06]   | `Example:` or `Examples:` | Usage a reader misuses without one                                                                         |
|  [07]   | `Attributes:`             | Class-level attributes of regular classes (dataclasses and Pydantic models use inline)                     |

### [02.2]-[ONE_LINERS]

When the function is simple and needs no `Args`/`Returns` sections, use a one-liner:

```python
def is_valid(self) -> bool:
    """Checks whether the configuration passes all validation rules."""
```

Rules for one-liners:
- Opening `"""` and closing `"""` on the same line
- No blank line after the one-liner before the code body

When a docstring needs any section (`Args`, `Returns`) or its summary with description exceeds one line, switch to multi-line.

## [03]-[TYPE_ANNOTATIONS]

When the code has type hints, do not repeat types in the docstring:
- The docstring adds meaning the type hint cannot convey
- Never restate the function signature in prose

The same function documented both ways:

```python
# BAD: type info is duplicated
def connect(host: str, port: int, timeout: float = 30.0) -> Connection:
    """Establishes a connection to the remote server.

    Args:
        host (str): The hostname or IP address of the server.
        port (int): The TCP port number to connect to.
        timeout (float): The timeout in seconds. Defaults to 30.0.

    Returns:
        Connection: An open connection to the server.
    """

# GOOD: types are in the signature, docstring adds meaning
def connect(host: str, port: int, timeout: float = 30.0) -> Connection:
    """Establishes a connection to the remote server.

    Args:
        host: Hostname or IP address.
        port: TCP port number.
        timeout: Seconds to wait before giving up.

    Returns:
        An open connection ready for queries.
    """
```

## [04]-[FUNCTIONS]

Basic structure:

```python
def function(arg1, arg2, *, keyword_only=None):
    """Summary line, one sentence in the third person that fits on one line.

    Extended description if needed. Can be multiple paragraphs.

    Args:
        arg1: Description of arg1. Continue on the next line with a 4-space hanging indent if it wraps.
        arg2: Description of arg2.
        keyword_only: Description. Mention the default behavior when None is passed if it's non-obvious.

    Returns:
        Description of the return value. If the function returns a complex structure, describe its shape.

    Raises:
        ValueError: When arg1 is negative.
        ConnectionError: If the remote service is unreachable.
    """
```

### [04.1]-[RETURNS]

Describe the shape of tuple and dict returns, and state when the function returns `None`:

```text
Returns:
    A tuple of (matched_items, unmatched_items) where each element is a list of strings.

Returns:
    A dict mapping user IDs to their profile data, where each value is a dict with keys "name", "email", and "role".

Returns:
    The parsed configuration, or None if the file doesn't exist.
```

A `Result` return describes the success value and each failure variant under `Returns:`, never under `Raises:`.

### [04.2]-[RAISES]

- Document only exceptions the function raises directly (`DOC501`), not every exception from called code, and nothing the body does not raise (`DOC502`)

Raises format:

```text
Raises:
    ValueError: If the input is not a valid ISO 8601 date string.
    FileNotFoundError: If the template path doesn't exist.
```

### [04.3]-[GENERATORS]

- Use `Yields:` instead of `Returns:`

A generator docstring:

```python
def read_chunks(path: Path, size: int = 8192):
    """Reads a file in fixed-size chunks.

    Yields:
        Bytes chunks of at most `size` bytes.
    """
```

### [04.4]-[ASYNC]

- Document identically to sync functions
- The `async` keyword in the signature is sufficient, do not add "This is an async function" to the docstring unless the async behavior is important

### [04.5]-[OVERLOADS]

- Document the primary overload
- For `@typing.overload`, put a docstring that covers all signatures on the implementation (non-decorated) function

The docstring on the implementation:

```python
@overload
def parse(data: str) -> dict: ...
@overload
def parse(data: bytes) -> dict: ...

def parse(data):
    """Parses input data into a structured dict.

    Accepts either a JSON string or raw bytes (decoded as UTF-8).

    Args:
        data: JSON content as a string or bytes.

    Returns:
        Parsed dictionary.
    """
```

### [04.6]-[DECORATORS]

- Document the decorator's effect on the wrapped function, not the wrapper's internals
- When the decorator modifies the function's signature or return type, add a `Note:` section

## [05]-[CLASSES]

Regular classes:
- For attributes set in `__init__`, use `Attributes:` in the class docstring
- Document `__init__` parameters in `__init__`'s own docstring

A regular class with `Attributes:` and an `__init__` docstring:

```python
class HTTPClient:
    """HTTP client with connection pooling and retry logic.

    Manages a pool of persistent connections and automatically retries failed requests with exponential backoff.

    Attributes:
        base_url: The root URL all requests are relative to.
        timeout: Default timeout for requests in seconds.
        max_retries: Maximum retry attempts for failed requests.
    """

    def __init__(self, base_url: str, timeout: float = 30.0):
        """Initializes the HTTP client.

        Args:
            base_url: Root URL (e.g., "https://api.example.com").
            timeout: Default request timeout in seconds.
        """
        self.base_url = base_url
        self.timeout = timeout
        self.max_retries = 3
```

### [05.1]-[INLINE_ATTRIBUTES]

Dataclasses, Pydantic models, TypedDicts, NamedTuples:
- Inline attribute docstrings, a bare string literal on the line after the attribute
- The only field documentation that tools (Sphinx, pdoc) pick up

Inline attribute docstrings on a dataclass, a Pydantic model, and a TypedDict:

```python
@dataclass
class SearchResult:
    """A single search result with relevance scoring."""

    url: str
    """The canonical URL of the result."""

    title: str
    """Page title, already HTML-unescaped."""

    score: float
    """Relevance score between 0.0 and 1.0."""

    snippet: str | None = None
    """Extracted text snippet, if available."""

class UserCreate(BaseModel):
    """Schema for creating a new user account."""

    email: EmailStr
    """Must be a valid, unique email address."""

    password: str = Field(..., min_length=8)
    """Minimum 8 characters. Stored as bcrypt hash."""

    display_name: str = Field(..., max_length=100)
    """Publicly visible name."""

class Coordinates(TypedDict):
    """Geographic coordinate pair."""

    lat: float
    """Latitude in decimal degrees (-90 to 90)."""

    lng: float
    """Longitude in decimal degrees (-180 to 180)."""
```

### [05.2]-[ABSTRACT_BASE_CLASSES]

- Document the interface contract in the abstract method's docstring
- Concrete implementations can reference the base class doc or add implementation-specific details

### [05.3]-[PROPERTIES]

Document properties like attributes, a noun phrase for what the property evaluates to, with no `Args` and no leading verb (`D421`):

```python
@property
def is_expired(self) -> bool:
    """Whether the token has passed its expiration time."""

@name.setter
def name(self, value: str) -> None:
    """Sets the display name.

    Raises:
        ValueError: If the name is empty or exceeds 100 characters.
    """
```

## [06]-[EXAMPLES]

When a reader misuses the call shape without one, use `Examples:`.

Rules:
- Indent code blocks by 4 spaces within the section
- Blank line before and after each code block
- With more than one example, label each one
- Examples must be syntactically valid Python
- Write `doctest`-style examples (`>>>`) only in a project that runs doctests

A docstring with labeled examples:

```python
def retry(max_attempts: int = 3, backoff: float = 1.0):
    """Decorator that retries a function on exception.

    Args:
        max_attempts: Total attempts before giving up.
        backoff: Seconds to wait between attempts (doubles each retry).

    Examples:
        Basic usage with defaults:

            @retry()
            def fetch_data():
                ...

        Custom retry configuration:

            @retry(max_attempts=5, backoff=2.0)
            def fragile_operation():
                ...
    """
```
