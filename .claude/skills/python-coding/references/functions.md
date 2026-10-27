# [FUNCTIONS]

Covers functions as values, from decorators that read a declaration at import to pipelines of unary steps.

## [01]-[DECORATORS]

`Command` holds a decorated function and types each call through its parameters `P` and return `R`, and `Body` adds the `__name__` its key reads. `@Command` over a module-level `def build_all(target: str, *, clean: bool = False) -> int` keeps its signature and derives key `build-all`:

```python
class Body[**P, R](Protocol):
    __name__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> R: ...


class Command[**P, R](msgspec.Struct, frozen=True):
    __wrapped__: Body[P, R]

    @property
    def key(self) -> str:
        return self.__wrapped__.__name__.replace("_", "-")

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> R:
        return self.__wrapped__(*args, **kwargs)


@Command
def build_all(target: str, *, clean: bool = False) -> int:
    return 0 if clean else len(target)
```

- Calls through a record check against `P`, and `build_all(1)` or a keyword for a positional-only parameter fails mypy and ty
- Records over one function compare and hash equal (`Command(build_all.__wrapped__) == build_all`), a table keyed by record holds each function once

Class declarations derive through a base hook when each entry is a type with class attributes. `ClassVar` declarations on `Rule` type every attribute its hook writes, and each subclass states its docstring and pattern alone:

```python
class Rule:
    code: ClassVar[str]
    pattern: ClassVar[re.Pattern[str]]

    @override
    def __init_subclass__(cls) -> None:
        super().__init_subclass__()
        if cls.__doc__ is None:
            raise TypeError(cls.__qualname__)
        cls.code = "-".join(word.lower() for word in re.findall(r"[A-Z][a-z\d]*", cls.__name__))

    def hit(self, line: str, /) -> bool:
        return self.pattern.search(line) is not None


class TrailingSpace(Rule):
    """Line ends in whitespace."""

    pattern = re.compile(r"\s$")


class HardTab(Rule):
    """Line holds a tab."""

    pattern = re.compile(r"\t")
```

- `TrailingSpace.code` is `trailing-space` once its class statement runs, checkers type it through the base `ClassVar[str]`
- Subclasses of a subclass run the same hook, `class TrailingTab(TrailingSpace)` derives `trailing-tab`

## [02]-[REGISTRIES]

Registries derive from declarations a module holds, each new entry is one declaration, and module state is one frozen table built at import. Function registries read module namespace for their decorator's record type, class registries read their base's direct subclasses:

```python
@Command
def remove(target: str, /) -> int:
    return len(target)


COMMANDS: frozendict[str, Command[..., int]] = frozendict({entry.key: entry for entry in globals().values() if isinstance(entry, Command)})
RULES: frozendict[str, type[Rule]] = frozendict({rule.code: rule for rule in Rule.__subclasses__()})
```

- Tables sit after the last declaration they read, a declaration below a table stays out of it
- `COMMANDS` reads every `Command` its namespace binds, a command imported by name joins it
- `isinstance` narrows each namespace value to its record type, and declared table types need no cast
- `__subclasses__()` lists direct subclasses in definition order, `TrailingTab` stays out of `RULES`

Select the form by the call site:

| [INDEX] | [CALL_SITE]                            | [FORM]                                                                                      |
| :-----: | :------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | Lookup table built once                | `frozendict` keyed by object answering lookup (enum member, final leaf type, tuple of both) |
|  [02]   | Implementations other modules register | `functools.singledispatch`, dispatch follows MRO and ABC registration                       |
|  [03]   | Short-circuiting boolean               | `any` or `all` over a generator                                                             |
|  [04]   | Threshold decision                     | `bisect` over sorted breaks indexes a label tuple                                           |

`RATES` answers per enum member, `rate` finds its member through `bisect`, and `rendered` dispatches on the first parameter each `register` implementation annotates:

```python
class Tier(StrEnum):
    DEPLETED = "depleted"
    LOW = "low"
    HEALTHY = "healthy"


RATES: frozendict[Tier, float] = frozendict({Tier.DEPLETED: 0.5, Tier.LOW: 1.0, Tier.HEALTHY: 2.0})


def rate(volts: float, /) -> float:
    return RATES[(Tier.DEPLETED, Tier.LOW, Tier.HEALTHY)[bisect((3.2, 3.6), volts)]]


def drained(readings: tuple[float, ...], /) -> bool:
    return any(volts < 3.2 for volts in readings)


@singledispatch
def rendered(value: object, /) -> str:
    return repr(value)


@rendered.register
def _(value: Decimal, /) -> str:
    return f"{value:.2f}"
```

- Tables keyed by a tuple of members hold every combination, `TABLE[kind, tier]` reads one, and a missing pair raises `KeyError` as a defect
- Dict displays keyed by subclasses of one `msgspec.Struct` base fail mypy, an annotated tuple of pairs passes both checkers
- Implementations in another module register when their module imports, and the dispatcher's module imports none of them

## [03]-[SPECIALIZATION]

Handlers take each dependency as the function they call, and composition binds it once through `partial`. `sized` reads bytes through `Reader` and checks a limit, production binds `disk`, and a probe binds `fixed` over chosen content. `Fault`, `Outcome`, and `Reason` are the package faults:

```python
type Reader = Callable[[Path], Outcome[bytes]]


def disk(path: Path, /) -> Outcome[bytes]:
    return catch(path.read_bytes, exception=OSError)().map_error(lambda error: (Fault(reason=Reason.UNREADABLE, subject=str(path), cause=error),))


def sized(read: Reader, limit: int, path: Path, /) -> Outcome[int]:
    return read(path).map(len).filter_with(lambda size: size <= limit, lambda size: (Fault(reason=Reason.OUTSIDE, subject=str(path), member=f"at most {limit} bytes", cause=str(size)),))


def fixed(content: bytes, _path: Path, /) -> Outcome[bytes]:
    return Ok(content)


handled: Callable[[Path], Outcome[int]] = partial(sized, disk, 4096)
probe: Callable[[Path], Outcome[int]] = partial(sized, partial(fixed, b"abc"), 2)
```

- Both checkers check arguments a `partial` binds, and mypy accepts the `partial[R]` it infers in any `Callable` slot returning `R`
- `partial` values keep their remaining parameters under ty, and a slot with other parameters fails

## [04]-[PIPELINES]

Pipelines name a composition of unary steps once and apply it where each value enters. `slug` normalizes text left to right, and `tier` pipes text through `slug` into a raising enum lookup that `catch` converts:

```python
slug: Callable[[str], str] = compose(str.strip, str.casefold, partial(re.compile(r"[\s_]+").sub, "-"))


def tier(text: str, /) -> Outcome[Tier]:
    return pipe(text, slug, catch(Tier, exception=ValueError)).map_error(lambda error: (Fault(reason=Reason.INVALID, subject=repr(text), cause=error),))
```

- Unbound methods (`str.strip`, `str.casefold`) are unary steps over their subject
- Steps fixing a later argument bind a method of the fixed object, `partial(pattern.sub, "-")` leaves the text parameter open
- `catch(Tier, exception=ValueError)` keeps `Tier` as its value type
- `compose` takes at most nine steps and `pipe` sixteen, a longer call matches no overload, and a long chain splits into named compositions
- Composition is associative and `compose()` is the identity, regrouping steps into named parts keeps behavior
- `Result.pipe` and `Option.pipe` apply functions to the container, `map` and `bind` apply them to its value
