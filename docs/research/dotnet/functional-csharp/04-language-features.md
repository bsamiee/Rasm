# [LANGUAGE_FEATURES]

## [03]-[EXPLICIT_ALTERNATIVES]

Discriminator flags with fields meaningful only for one flag value permit invalid combinations. Model the alternatives as distinct variants. `[Union]` on an abstract partial record with nested sealed record cases defines a closed set of cases, and the generated `Switch` takes one arm per case. `Option<A>` is a readonly struct, `Fin<A>` is an abstract class, `Either<L, R>` and `Validation<Error, A>` are record classes, and each supports `Match`.

### [03.1]-[ACTIVE_PATTERNS]

Active patterns run a custom function during pattern matching and extract a value on success. This F# example parses a date:

```fsharp
let (|IsDateTime|_|) (input: string) =
    let success, value = DateTime.TryParse input
    if success then Some value else None

let tryParseDateTime input =
    match input with
    | IsDateTime value -> Some value
    | _ -> None
```

`IsDateTime` both decides whether the case matches and supplies the parsed `DateTime` to the result expression.

