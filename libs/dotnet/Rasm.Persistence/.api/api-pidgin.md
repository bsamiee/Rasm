# [RASM_API_PIDGIN]

`Pidgin` is an allocation-light parser-combinator library: `Parser<TToken, T>` is the immutable parser value, `Parser` and `Parser<TToken>` are the primitive and combinator statics, `ParserExtensions` carries every input shape a parse runs against, and `Result<TToken, T>` is the success-or-`ParseError` verdict a caller folds without a `catch`. `Rasm.Persistence` binds it as the CESQL grammar behind the subscription `sql` filter dialect — the expression language a delivery filter compiles once at subscription admission and evaluates per event.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the parser value, its statics, and the verdict

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :--------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `Parser<TToken, T>`          | abstract class  | the immutable parser value and every instance combinator           |
|  [02]   | `Parser<TToken>`             | static class    | token-generic primitives — `Token`, `Return`, `Fail`, `End`, `Any` |
|  [03]   | `Parser`                     | static class    | char primitives, `Map`, `OneOf`, `Try`, `Rec`, and the numerics    |
|  [04]   | `ParserExtensions`           | static class    | `Parse`/`ParseOrThrow` over every admitted input shape             |
|  [05]   | `Result<TToken, T>`          | class           | `Success` beside `Value` or `Error`, with a `Match` fold           |
|  [06]   | `ParseError<TToken>`         | class           | `Unexpected`, `Expected`, offset, position, and a rendered message |
|  [07]   | `Maybe<T>`                   | readonly struct | the optional a `.Optional()` parser yields                         |
|  [08]   | `Expected<TToken>`           | readonly struct | one expectation an error carries                                   |
|  [09]   | `SourcePos`/`SourcePosDelta` | readonly struct | line-and-column position and its delta algebra                     |
|  [10]   | `ParseException<TToken>`     | class           | what `ParseOrThrow` raises; `: ParseException`                     |
|  [11]   | `Unit`                       | sealed class    | the value a `Skip*`/`IgnoreResult` parser yields                   |

[PUBLIC_TYPE_SCOPE]: the operator-precedence builder and the configuration seat

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :--------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Expression.ExpressionParser`            | static class  | `Build` folds a term parser and a precedence table into one |
|  [02]   | `Expression.Operator`                    | static class  | one row per fixity and associativity                        |
|  [03]   | `Expression.OperatorTableRow<TToken, T>` | sealed class  | one precedence level; `And` merges, `Empty` seeds           |
|  [04]   | `Expression.BinaryOperatorType`          | enum          | `NonAssociative`/`LeftAssociative`/`RightAssociative`       |
|  [05]   | `Expression.UnaryOperatorType`           | enum          | `Prefix`/`Postfix`                                          |
|  [06]   | `Configuration.IConfiguration<TToken>`   | interface     | array-pool provider and source-position calculator          |
|  [07]   | `Configuration.Configuration`            | static class  | the default configurations and their overrides              |
|  [08]   | `TokenStreams.ITokenStream<TToken>`      | interface     | the pull-shaped input a `Parse` overload takes              |

- [01]-[PARSER_COMBINATORS]: instance members carry the whole combinator algebra — `Map`/`Select`/`Bind`/`SelectMany`, so LINQ query syntax composes parsers directly, beside `Then`/`Before`/`Between`, `Or`, `Many`/`AtLeastOnce`/`Repeat`/`Until`/`ManyThen`, the `Separated` family in its four terminator shapes, `Optional`, `Assert`/`Where`, `Labelled`, `RecoverWith`, `Cast`/`OfType`, and the `Skip*` and `Trace*` families.
- [01]-[SPAN_PROJECTION]: `MapWithInput` and `Slice` both take a `ReadOnlySpanFunc`, so a matched span projects without materializing.
- [03]-[CHAR_PRIMITIVES]: `Char`/`CIChar`/`String`/`CIString`/`OneOf`/`CIOneOf`/`AnyCharExcept`, the character classes (`Digit`, `Letter`, `LetterOrDigit`, `Lowercase`, `Uppercase`, `Punctuation`, `Symbol`, `Separator`, `Whitespace`/`Whitespaces`/`WhitespaceString`/`SkipWhitespaces`, `EndOfLine`), the numerics (`Num`/`DecimalNum`/`LongNum`/`OctalNum`/`HexNum`/`Real`, with `Int`/`UnsignedInt`/`Long`/`UnsignedLong` on an explicit base), and the `Enum<TEnum>`/`CIEnum<TEnum>` pair.
- [03]-[CONTROL]: `Try` (backtracks a consumed failure), `Lookahead` (matches without consuming), `Not`, `Rec` in its three shapes (a `Func`, a self-referencing `Func`, or a `Lazy`) for a recursive grammar, `OneOf` over parsers, and `Map` in arities up to eight.
- [04]-[PARSE_SHAPES]: `Parse`/`ParseOrThrow` bind `string`, `TextReader`, `IList<T>`, `IReadOnlyList<T>` (through the distinct `ParseReadOnlyList` name), `IEnumerable<T>`, `IEnumerator<T>`, `T[]`, `ReadOnlySpan<T>`, `Stream` (for `byte` tokens), `ITokenStream<T>`, and a `ref ParseState<T>`; each takes an optional `IConfiguration<TToken>`.
- [05]-[RESULT_FOLD]: `Success`, `Value`, `Error`, `GetValueOrDefault` in three arities, `Match(success, failure)`, and the `Select`/`SelectMany`/`Or`/`Cast` projections — so a verdict folds onto a caller's own result without an exception.
- [06]-[ERROR_DETAIL]: `EOF`, `Unexpected`, `Expected`, `ErrorOffset`/`ErrorOffsetLong`, `ErrorPos`/`ErrorPosDelta`, `Message`, and `RenderErrorMessage(SourcePos?)` — the whole diagnostic a refusing admission reports.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar construction and evaluation

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Parser.String(str)` / `CIString(str)` / `Char(c)` / `OneOf(...)`  | static   | terminal matchers                                   |
|  [02]   | `Parser<TToken>.Token(predicate)` / `.Return(v)` / `.Fail(msg)`    | static   | token-generic primitives                            |
|  [03]   | `Parser.Try(parser)` / `Lookahead(parser)` / `Not(parser)`         | static   | backtracking and negative lookahead                 |
|  [04]   | `Parser.Rec(() => parser)`                                         | static   | the recursion seat a self-referential grammar needs |
|  [05]   | `parser.Or(other)` / `.Many()` / `.Separated(sep)` / `.Optional()` | instance | the combinator algebra                              |
|  [06]   | `parser.Labelled(label)`                                           | instance | names the expectation an error reports              |
|  [07]   | `parser.MapWithInput(selector)` / `.Slice(selector)`               | instance | projects the matched span without materializing it  |
|  [08]   | `from x in p1 from y in p2 select f(x, y)`                         | query    | `SelectMany` composition in LINQ syntax             |
|  [09]   | `Operator.InfixL(op)` / `InfixR` / `InfixN` / `Prefix` / `Postfix` | static   | one precedence-table row per fixity                 |
|  [10]   | `ExpressionParser.Build(term, table)`                              | static   | folds term and table into one expression parser     |
|  [11]   | `parser.Parse(input[, configuration])`                             | instance | `Result<TToken, T>` — the admitted result           |
|  [12]   | `parser.ParseOrThrow(input[, configuration])`                      | instance | raises `ParseException<TToken>`                     |
|  [13]   | `result.Match(success, failure)`                                   | instance | the fold onto a caller's own result                 |
|  [14]   | `error.RenderErrorMessage([initialSourcePos])`                     | instance | the diagnostic a refusing admission reports         |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Parsers are immutable VALUES, so a grammar builds once as `static readonly` fields and every evaluation reuses them; constructing a parser per input rebuilds the whole expression graph on every call.
- `Or` does NOT backtrack a branch that already consumed input — a left branch that consumed and then failed fails the alternation. `Try` is the explicit backtrack, so a grammar whose alternatives share a prefix either factors the prefix out or wraps the branch.
- `Rec` is the only way a grammar references itself, because a static-field grammar cannot reference a field whose initializer has not run; the `Func` and `Lazy` shapes both defer the read to first parse.
- `Parse` returns a verdict and `ParseOrThrow` raises, so a boundary admitting untrusted text takes `Parse` and folds `Result.Match` onto its own result; the throwing form serves text this solution itself rendered.
- `ExpressionParser.Build` folds a term parser and an ordered precedence table into one parser, so an operator grammar is a TABLE of rows rather than a hand-written recursive-descent ladder; `OperatorTableRow.And` merges rows at one precedence level and `Empty` seeds a fold.
- `MapWithInput` and `Slice` take a `ReadOnlySpanFunc`, so a projection reads the matched input span directly and a grammar that needs the source text of a match never re-slices the input at its own boundary.
- `ParseError` carries the offset, the source position, the unexpected token, and the expectation set, so a refusing admission reports WHERE and WHAT rather than a bare failure; `Labelled` is what makes the expectation set legible.
- This package targets `net7.0` alone and ships no `net10.0` asset, so the reference binds that asset — pure IL with no native or analyzer surface, which is why the target gap costs nothing at compile or run time.

[STACKING]:
- `LanguageExt.Core`(`api-languageext.md`): `Result<TToken, T>.Match` folds onto `Fin<T>` at the admitting boundary, so a CESQL grammar failure is a typed refusal carrying the rendered parse error and never an exception in domain logic.
- `Version/egress` consumer anchor: a subscription's `sql` filter expression parses ONCE at subscription admission and an unparseable expression refuses the subscription there — never a delivery — so the compiled parser value and the compiled expression both outlive every event they filter.

[LOCAL_ADMISSION]:
- Grammars are `static readonly` parser values built once; a parser constructed inside an evaluation is the rejected form.
- Untrusted expression text crosses `Parse` and folds through `Match`; `ParseOrThrow` never appears at an admission boundary.
- Operator grammars ride the precedence table and `ExpressionParser.Build`; a hand-written recursive-descent ladder over mutable state beside it is the deleted form.
- Every terminal carries a `Labelled` name, so the expectation set a refusal reports names the grammar's own vocabulary rather than raw character classes.
