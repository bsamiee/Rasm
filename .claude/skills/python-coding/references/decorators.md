# [DECORATORS]

Covers decorators and dispatch registries.

## [01]-[WRAPPERS]

Package decorators apply each change around a call they cover, and a factory applies one no package covers. `caught` over `np.errstate` returns what the scope raises as faults naming the body:

```python
def caught[**P, T](exception: type[Exception], /, *, reason: StrEnum = Reason.FAILED) -> Callable[[Callable[P, T]], Callable[P, Outcome[T]]]:
    def decorator(body: Callable[P, T], /) -> Callable[P, Outcome[T]]:
        run = catch(body, exception=exception)

        @wraps(body)
        def call(*args: P.args, **kwargs: P.kwargs) -> Outcome[T]:
            return run(*args, **kwargs).map_error(partial(Fault.of, reason, call.__qualname__))

        return call

    return decorator


@caught(FloatingPointError)
@np.errstate(all="raise")
def normalized(values: NDArray[np.float64], /) -> NDArray[np.float64]:
    return values / np.linalg.norm(values)
```

`normalized(np.zeros(3))` fails with `normalized failed. invalid value encountered in divide`, and `normalized` keeps the body's parameter types:
- `P` and `T` in a factory's return type alone bind once per decorated function
- `Callable` declares no `__qualname__`, `call.__qualname__` names the subject, and a method fails as `<Owner>.<method>`
- `catch` passes a `Result` its body returns through unchanged while typing it nested, a body under `caught` returns a plain value
- Scopes around a call are `@contextmanager` and `@asynccontextmanager` generators applied as `@<scope>()`
- Each call enters a fresh scope at its first step and leaves it when a decorated generator exhausts or a coroutine returns
- `np.errstate` scopes a plain `def` alone, and a generator or `async def` body enters it through `with`
- `@classmethod` over `@cache` keys each entry by class, and registration decorators sit outermost

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
- Union annotations register each member, and an annotation naming no class (`Mapping[str, object]`, a `type` alias) passes its class or the alias's `__value__` to `register`
- `dispatch(kind)` follows MRO and ABC registration, and `frozendict` reaches the `Mapping` implementation
- `rendered.dispatch(kind) is not rendered.dispatch(object)` reads a capability from the registrations
- Implementations in another module register when an eager import loads it, and the dispatcher's module imports none of them
