# [DECORATORS]

Covers behavior decorators derive from a declaration at import, from package decorator stacks and wrapper factories to dispatch registries.

## [01]-[WRAPPERS]

Package decorators state each change around a call, and a stack applies them bottom to top. `np.errstate` scopes every call it decorates, and `@catch` above it converts what the scope raises:

```python
@catch(exception=FloatingPointError)
@np.errstate(all="raise")
def normalized(values: NDArray[np.float64], /) -> NDArray[np.float64]:
    return values / np.linalg.norm(values)
```

`normalized(np.zeros(3))` returns an `Error` holding `FloatingPointError("invalid value encountered in divide")`:
- `np.errstate` and a `@contextmanager` scope are context decorators, each keeps the body's type and enters a fresh scope per call
- `@contextmanager` and `@asynccontextmanager` sit closest to their generator, `@classmethod` over `@cache` keys each entry by class
- Host registration (`@app.command`) sits above every wrapper and reads the body's signature through `__wrapped__`
- Wrappers no package states are a curried factory `def <name>[**P, T, U](<configuration>, /) -> Callable[[Callable[P, T]], Callable[P, U]]`
- `P` held by the factory's return type alone binds once per decorated function under both checkers
- Factories return a `wraps(body)` closure over `*args: P.args, **kwargs: P.kwargs`, and a sink it writes to is a factory parameter

## [02]-[REGISTRIES]

Dispatch other modules extend is `functools.singledispatch`, and `register` reads each implementation's class from its first parameter annotation:

```python
@singledispatch
def rendered(value: object, /) -> str:
    return repr(value)


@rendered.register
def _(value: Decimal | Fraction, /) -> str:
    return f"{value:.2f}"


@rendered.register(Mapping)
def _(value: Mapping[str, object], /) -> str:
    return ", ".join(f"{key}={rendered(item)}" for key, item in value.items())
```

`rendered(frozendict(rate=Fraction(1, 3), count=2))` returns `rate=0.33, count=2`:
- Union annotations register each member, a parameterized annotation takes its class as the `register` argument
- `dispatch(kind)` follows MRO and ABC registration, and `frozendict` reaches the `Mapping` implementation
- `rendered.dispatch(kind) is not rendered.dispatch(object)` reads a capability from the registrations
- Implementations in another module register when their module imports, and the dispatcher's module imports none of them
