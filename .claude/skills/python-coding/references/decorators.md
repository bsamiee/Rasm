# [DECORATORS]

Covers behavior derived from a declaration at import, from wrappers and decorator factories to per-step callables, class hooks, registries, and decorator order.

## [01]-[WRAPPERS]

Wrappers translate what a function returns while `**P` keeps every parameter it takes. Factories are one top-level function taking configuration before the body, and `@partial(<factory>, <configuration>)` applies it on the `def` with no inner function returning a decorator. `Fault`, `Faults`, `Outcome`, and `Reason` are the package faults:

```python
def reported[**P, T](hold: Callable[[Faults], None], body: Callable[P, Outcome[T]], /) -> Callable[P, Option[T]]:
    @wraps(body)
    def translated(*args: P.args, **kwargs: P.kwargs) -> Option[T]:
        return body(*args, **kwargs).map_error(hold).to_option()

    return translated


def logged(faults: Faults, /) -> None:
    sys.stderr.writelines(f"{fault.message}\n" for fault in faults)


@partial(reported, logged)
def halved(value: int, /) -> Outcome[int]:
    return Ok(value).filter_with(lambda number: number % 2 == 0, partial(Fault.of, Reason.OUTSIDE, "value", member="an even number")).map(lambda number: number // 2)
```

`halved(3)` writes `value accepts an even number, not 3` and returns `Nothing`, and `halved("x")` fails mypy and ty against the body's own parameter:
- `wraps` copies `__name__`, `__qualname__`, `__doc__`, `__wrapped__`, and `__type_params__`
- Hosts reading `Option` or an exit code get one wrapper per vocabulary, every body returns `Outcome`
- Wrappers hold no state between calls, a sink the wrapper writes to is a dependency its factory takes

## [02]-[DERIVED_CALLABLES]

Callables built per declaration take their identity from it. `update_wrapper(partial(<runner>, step), step)` binds one shared runner to each step and copies the step's name and docstring onto the result:

```python
def _exited(step: Callable[[], Outcome[None]], /) -> int:
    match (outcome := step()).tag:
        case "ok":
            return 0
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 1


COMMANDS = tuple(update_wrapper(partial(_exited, step), step) for step in (build, clean))
```

`build` and `clean` are documented functions returning `Outcome[None]`, and a host registering commands reads each as `build` or `clean` with its docstring as help:
- Names and docstrings written onto a closure after its `def` restate what `update_wrapper` copies
- `update_wrapper` returns a typed wrapper, and `__name__` on each command reads under both checkers
- Members a factory builds per owner or row take their key from the owner's key, with no `__name__` rewrite

## [03]-[CLASS_HOOKS]

Class declarations derive through a base hook when each entry is a type with class attributes. `ClassVar` declarations on `Rule` type every attribute its hook writes, a missing docstring raises at class build, and each subclass states its docstring and pattern alone:

```python
class Rule:
    code: ClassVar[str]
    summary: ClassVar[str]
    pattern: ClassVar[re.Pattern[str]]

    @override
    def __init_subclass__(cls) -> None:
        super().__init_subclass__()
        if cls.__doc__ is None:
            raise TypeError(cls.__qualname__)
        cls.code = "-".join(word.lower() for word in re.findall(r"[A-Z][a-z\d]*", cls.__name__))
        cls.summary = inspect.cleandoc(cls.__doc__)

    def hit(self, line: str, /) -> bool:
        return self.pattern.search(line) is not None


class TrailingSpace(Rule):
    """Line ends in whitespace."""

    pattern = re.compile(r"\s$")


class HardTab(Rule):
    """Line holds a tab."""

    pattern = re.compile(r"\t")


RULES: frozendict[str, type[Rule]] = frozendict({rule.code: rule for rule in Rule.__subclasses__()})
```

`RULES` holds `trailing-space` and `hard-tab` once both class statements run:
- Subclasses of a subclass run the same hook, `class TrailingTab(TrailingSpace)` derives `trailing-tab`
- `__subclasses__()` lists direct subclasses in definition order, `TrailingTab` stays out of `RULES`
- `__set_name__(self, owner, name)` hands a descriptor the attribute name it sits under, a key derived there needs no string argument
- `__init_subclass__` overrides on a `msgspec.Struct` base hide the class keywords its stub declares

## [04]-[REGISTRIES]

Registries derive from declarations a module holds, and each new entry is one declaration. Union aliases list their cases, and `singledispatch` answers which implementation a type reaches:

```python
type Shape = Circle | Rect

KINDS: frozendict[str, type[Shape]] = frozendict({case.__name__: case for case in get_args(Shape.__value__)})


@singledispatch
def rendered(value: object, /) -> str:
    return repr(value)


@rendered.register
def _(value: Decimal, /) -> str:
    return f"{value:.2f}"


def registered(kind: type, /) -> bool:
    return rendered.dispatch(kind) is not rendered.dispatch(object)
```

`Circle` and `Rect` are record cases, and adding a case to `Shape` adds its row to `KINDS`. `registered(Decimal)` reads a capability from the registration itself, and no flag restates it:
- Declarations below a table stay out of it
- `singledispatch` follows MRO and ABC registration, a union annotation registers each member, and `frozendict` dispatches as `Mapping`
- Implementations in another module register when their module imports, and the dispatcher's module imports none of them

## [05]-[ORDER]

Decorators apply bottom to top, each wrapping the result of the one below it:
- `@catch(exception=E)` sits above a scope decorator (`@np.errstate`, a lock), the scope runs inside each converted call
- `@staticmethod`, `@classmethod`, and `@property` sit on top, every decorator below them sees the plain function
- `@classmethod` over `@functools.cache` keys the cache by class and arguments, each subclass caching its own values
- `@contextmanager` and `@asynccontextmanager` sit closest to the generator they convert
