# [RASM_API_LANGUAGEEXT_PARSEC]

`LanguageExt.Parsec` mints every parser as a delegate: `Parser<T>` maps a `PString` to a `ParserResult<T>` tagged consumed-or-empty beside OK-or-error, and `Parser<I, O>` runs that same algebra over a `PString<I>` token array. `Prim` and `Char` own the combinator vocabulary, `Expr.buildExpressionParser` folds an ordered precedence table, and `Token.makeTokenParser` derives a whole lexeme roster from one `GenLanguageDef`.

`ParserResult<T>` folds to `Option<T>` and `Either<string, T>` in place, so a refusal lands in a LanguageExt carrier with no `catch` at the boundary.

## [01]-[PARSER_CORE]

[PARSER_CORE_TYPE_SCOPE]: the `char`-input parser delegate, its state carrier, and the two-tag verdict, under `LanguageExt.Parsec`; `ParserExtensions` sits in the global namespace

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `Parser<T>`                     | delegate      | `PString -> ParserResult<T>`, the whole parser value      |
|  [02]   | `PString`                       | class         | text, cursor, `Pos`, `DefPos`, side, and user state       |
|  [03]   | `Pos`                           | class         | `Line` and `Column` under a total order                   |
|  [04]   | `Sidedness`                     | enum          | `Onside` and `Offside`, the layout arm                    |
|  [05]   | `ParserResult<T>`               | class         | `Tag` beside `Reply`, the consumption verdict             |
|  [06]   | `ResultTag`                     | enum          | `Consumed` and `Empty`                                    |
|  [07]   | `Reply<T>`                      | class         | `Tag`, `Result`, `State`, `Error`                         |
|  [08]   | `ReplyTag`                      | enum          | `OK` and `Error`                                          |
|  [09]   | `ParserError`                   | class         | tag, position, message, expectations, inner cause         |
|  [10]   | `ParserErrorTag`                | enum          | `Unknown`, `SysUnexpect`, `Unexpect`, `Expect`, `Message` |
|  [11]   | `ParserException`               | class         | the `Exception` this surface raises                       |
|  [12]   | `ParserResult`                  | static class  | verdict constructors over a `Reply<T>`                    |
|  [13]   | `Reply`                         | static class  | reply constructors for both arms                          |
|  [14]   | `Common`                        | static class  | reference-position access and error merge                 |
|  [15]   | `ParserExtensions`              | static class  | `Parse`, `label`, and the LINQ operators                  |
|  [16]   | `StringAndCollectionExtensions` | static class  | `ToPString` lifts an input shape into parser state        |

- `Pos` and `ParserError`: both carry `Equals`, `CompareTo`, the six comparison operators, and a static `Compare<R>(lhs, rhs, EQ, GT, LT)` three-way fold.
- `Reply<T>.Result` and `.State`: nullable and populated on the `OK` arm alone; read them behind `ParserResult<T>.Match`.

[PARSER_CORE_ENTRY_SCOPE]: running a parser and folding its verdict

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Prim.parse(Parser<T>, string)`                                | static   | run over raw text; a `PString` overload rides |
|  [02]   | `parser.Parse(string)`                                         | static   | the extension spelling of the same run        |
|  [03]   | `value.ToPString()`                                            | static   | lift text into state seated at `Pos.Zero`     |
|  [04]   | `PString.Zero`                                                 | static   | the empty state every lift refines            |
|  [05]   | `result.ToOption() -> Option<T>`                               | instance | drop the diagnostic, keep presence            |
|  [06]   | `result.ToEither() -> Either<string, T>`                       | instance | rendered error text on `Left`                 |
|  [07]   | `result.ToEither<ERROR>(Func<string, ERROR>)`                  | instance | map that text onto a caller's error owner     |
|  [08]   | `result.Match(ConsumedOK, ConsumedError, EmptyOK, EmptyError)` | instance | total fold across both tags                   |
|  [09]   | `result.Select(Func<T, U>)`                                    | instance | project the parsed value                      |
|  [10]   | `result.Project(S, Func<S, T, U>)`                             | instance | project against a carried seed                |
|  [11]   | `result.IsFaulted`                                             | property | error test on the inner `Reply`               |
|  [12]   | `result.SetEndIndex(int)`                                      | instance | narrow the consumed window                    |
|  [13]   | `ParserResult.ConsumedOK(T, PString)`                          | static   | mint a consuming success                      |
|  [14]   | `ParserResult.EmptyOK(T, PString, ParserError?)`               | static   | mint a non-consuming success                  |
|  [15]   | `ParserResult.ConsumedError<T>(ParserError)`                   | static   | mint a consuming failure                      |
|  [16]   | `ParserResult.EmptyError<T>(ParserError)`                      | static   | mint a non-consuming failure                  |
|  [17]   | `Reply.OK(T, PString, ParserError?)`                           | static   | mint the success arm directly                 |
|  [18]   | `Reply.Error<T>(ParserError)`                                  | static   | mint the failure arm directly                 |

- `ParserResult<T>.Match`: four narrower overloads ride the same name, keyed on whether the caller splits `Empty` from `Consumed`, error from result, or neither.
- `ParserResult<T>.ToEither`: renders `ToString()` on the faulted arm, so the `Left` carries position and expectation text already formatted.

[PARSER_CORE_ENTRY_SCOPE]: composition, labelling, state, and error construction

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `parser.label(string)`                                 | static   | replace the expectation a failure reports        |
|  [02]   | `parser.Map(Func<T, U>)`                               | static   | project the parsed value                         |
|  [03]   | `parser.Select(Func<T, U>)`                            | static   | the query-syntax projection                      |
|  [04]   | `parser.Bind(Func<T, Parser<U>>)`                      | static   | sequence one parser onto the next                |
|  [05]   | `parser.SelectMany(Func<T, Parser<U>>, Func<T, U, V>)` | static   | the LINQ query-syntax seat                       |
|  [06]   | `parser.Where(Func<T, bool>)`                          | static   | fail the parse on a predicate; `Filter` twins it |
|  [07]   | `parser.Flatten()`                                     | static   | collapse `Parser<Parser<A>>`                     |
|  [08]   | `parser.Flatten(Func<string>)`                         | static   | fail a `Parser<Option<T>>` on `None`             |
|  [09]   | `parser.Flatten(Func<L, string>)`                      | static   | fail a `Parser<Either<L, R>>` from its `Left`    |
|  [10]   | `Prim.getPos`                                          | static   | the current `Pos`                                |
|  [11]   | `Prim.getIndex`                                        | static   | the current index into the source                |
|  [12]   | `Prim.setState(T)`                                     | static   | write the `PString.UserState` slot               |
|  [13]   | `Prim.getState<T>()`                                   | static   | read that slot back typed                        |
|  [14]   | `Common.getDefPos`                                     | static   | the layout reference position                    |
|  [15]   | `Common.setDefPos(Pos, Parser<T>)`                     | static   | run a parser against a stated reference          |
|  [16]   | `Common.onside(Pos, Pos)`                              | static   | the layout admission test                        |
|  [17]   | `Common.mergeError(ParserError?, ParserError?)`        | static   | fold two errors on position order                |
|  [18]   | `Common.mergeErrorReply(ParserError, Reply<T>)`        | static   | fold an error into a standing reply              |
|  [19]   | `ParserError.Expect(Pos, string, string)`              | static   | mint an expectation failure                      |
|  [20]   | `ParserError.Unexpect(Pos, string)`                    | static   | mint an unexpected-input failure                 |
|  [21]   | `ParserError.Message(Pos, string)`                     | static   | mint a free-text failure                         |
|  [22]   | `error.ToStringNoPosition()`                           | instance | render the message without its position          |
|  [23]   | `input.SetPos(Pos)`                                    | instance | reseat the cursor position                       |
|  [24]   | `input.SetIndex(int)`                                  | instance | reseat the index                                 |
|  [25]   | `input.SetUserState(object?)`                          | instance | seat the user-state slot                         |
|  [26]   | `input.SetSide(Sidedness)`                             | instance | seat the layout arm                              |

- `ParserError.SysUnexpect(Pos, string)` and `ParserError.Unknown(Pos)`: the two remaining tag constructors; `Unknown` seeds a merge with no standing error.
- `PString`: every `Set*` member returns a fresh state, so a combinator threads the cursor by value.

## [02]-[COMBINATORS]

[COMBINATORS_TYPE_SCOPE]: the modules owning the `char`-input combinator vocabulary, all under `LanguageExt.Parsec`

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Prim`   | static class  | control, repetition, separation, chaining, projection |
|  [02]   | `Char`   | static class  | character terminals and character classes             |
|  [03]   | `Indent` | static class  | layout gating against `PString.DefPos`                |

[COMBINATORS_ENTRY_SCOPE]: control flow and alternation (`Prim`)

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :--------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `Prim.result(T)`                         | static  | succeed with a value, consuming nothing          |
|  [02]   | `Prim.zero<T>()`                         | static  | fail carrying no message                         |
|  [03]   | `Prim.failure<T>(string)`                | static  | fail carrying a message                          |
|  [04]   | `Prim.unexpected<T>(string)`             | static  | fail as unexpected input; a `Pos` overload rides |
|  [05]   | `Prim.either(Parser<T>, Parser<T>)`      | static  | take the right branch on an empty left failure   |
|  [06]   | `Prim.choice(params Parser<T>[])`        | static  | first branch that succeeds; `Seq` twins it       |
|  [07]   | `Prim.attempt(Parser<T>)`                | static  | backtrack a failure that already consumed        |
|  [08]   | `Prim.lookAhead(Parser<T>)`              | static  | match and rewind                                 |
|  [09]   | `Prim.notFollowedBy(Parser<T>, string?)` | static  | succeed only where the inner parser fails        |
|  [10]   | `Prim.lazyp(Func<Parser<T>>)`            | static  | defer construction, the recursion seat           |
|  [11]   | `Prim.eof`                               | static  | assert end of input                              |
|  [12]   | `Prim.unitp`                             | static  | the `Unit`-yielding parser                       |

[COMBINATORS_ENTRY_SCOPE]: repetition, separation, and chaining (`Prim`)

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `Prim.many(Parser<T>)`                             | static  | zero or more into `Seq<T>`                 |
|  [02]   | `Prim.many1(Parser<T>)`                            | static  | one or more into `Seq<T>`                  |
|  [03]   | `Prim.manyn(Parser<T>, int)`                       | static  | exactly `n` applications                   |
|  [04]   | `Prim.manyn0(Parser<T>, int)`                      | static  | zero up to `n`                             |
|  [05]   | `Prim.manyn1(Parser<T>, int)`                      | static  | one up to `n`                              |
|  [06]   | `Prim.count(int, Parser<T>)`                       | static  | a counted run                              |
|  [07]   | `Prim.skipMany(Parser<T>)`                         | static  | discard zero or more                       |
|  [08]   | `Prim.skipMany1(Parser<T>)`                        | static  | discard one or more                        |
|  [09]   | `Prim.manyUntil(Parser<T>, Parser<U>)`             | static  | repeat until a terminator matches          |
|  [10]   | `Prim.sepBy(Parser<T>, Parser<S>)`                 | static  | separated, possibly empty                  |
|  [11]   | `Prim.sepBy1(Parser<T>, Parser<S>)`                | static  | separated, one minimum                     |
|  [12]   | `Prim.sepEndBy(Parser<T>, Parser<S>)`              | static  | separated with an optional trailer         |
|  [13]   | `Prim.sepEndBy1(Parser<T>, Parser<S>)`             | static  | the same, one minimum                      |
|  [14]   | `Prim.endBy(Parser<T>, Parser<S>)`                 | static  | every element terminated                   |
|  [15]   | `Prim.endBy1(Parser<T>, Parser<S>)`                | static  | the same, one minimum                      |
|  [16]   | `Prim.between(Parser<L>, Parser<R>, Parser<T>)`    | static  | bracket an inner parser                    |
|  [17]   | `Prim.chain(params Parser<T>[])`                   | static  | run in order into `Seq<T>`; `Seq` twins it |
|  [18]   | `Prim.cons(Parser<T>, Parser<Seq<T>>)`             | static  | prepend one element to a parsed sequence   |
|  [19]   | `Prim.flatten(Parser<Seq<Seq<T>>>)`                | static  | concatenate nested sequences               |
|  [20]   | `Prim.chainl(Parser<T>, Parser<Func<T, T, T>>, T)` | static  | left fold with a seed                      |
|  [21]   | `Prim.chainr(Parser<T>, Parser<Func<T, T, T>>, T)` | static  | right fold with a seed                     |
|  [22]   | `Prim.chainl1(Parser<T>, Parser<Func<T, T, T>>)`   | static  | left fold, one term minimum                |
|  [23]   | `Prim.chainr1(Parser<T>, Parser<Func<T, T, T>>)`   | static  | right fold, one term minimum               |
|  [24]   | `Prim.optional(Parser<T>) -> Parser<Option<T>>`    | static  | absence as an option                       |
|  [25]   | `Prim.optionOrElse(T, Parser<T>)`                  | static  | a stated value on an empty failure         |
|  [26]   | `Prim.optionalSeq(Parser<T>)`                      | static  | absence as an empty `Seq<T>`               |
|  [27]   | `Prim.optionalList(Parser<T>)`                     | static  | absence as an empty `Lst<T>`               |
|  [28]   | `Prim.optionalArray(Parser<T>)`                    | static  | absence as an empty array                  |

[COMBINATORS_ENTRY_SCOPE]: value projection (`Prim`)

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                           |
| :-----: | :--------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `Prim.asString(Parser<Seq<char>>)`       | static  | join parsed characters into a `string` |
|  [02]   | `Prim.asInteger(Parser<Seq<char>>, int)` | static  | parse an `int` on a stated base        |
|  [03]   | `Prim.asDouble(Parser<Seq<char>>)`       | static  | parse a `double`                       |
|  [04]   | `Prim.asFloat(Parser<Seq<char>>)`        | static  | parse a `float`                        |

- `Prim.asInteger`, `asDouble`, `asFloat`: each yields `Parser<Option<A>>`, so a matched-but-unconvertible run reads as `None` rather than a parse failure; `asInteger` also carries a base-free overload.

[COMBINATORS_ENTRY_SCOPE]: character terminals and classes (`Char`)

| [INDEX] | [SURFACE]                        | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `Char.ch(char)`                  | static  | one literal character                        |
|  [02]   | `Char.ch<EQ>(char)`              | static  | the same under an `Eq<char>` witness         |
|  [03]   | `Char.satisfy(Func<char, bool>)` | static  | one character passing a predicate            |
|  [04]   | `Char.oneOf(string)`             | static  | membership; a `params char[]` overload rides |
|  [05]   | `Char.noneOf(string)`            | static  | exclusion; a `params char[]` overload rides  |
|  [06]   | `Char.str(string)`               | static  | a literal run                                |
|  [07]   | `Char.str<EQ>(string)`           | static  | that run under an `Eq<char>` witness         |
|  [08]   | `Char.anyChar`                   | static  | any single character                         |
|  [09]   | `Char.space`                     | static  | one whitespace character                     |
|  [10]   | `Char.spaces`                    | static  | skip zero or more whitespace                 |
|  [11]   | `Char.tab`                       | static  | a tab                                        |
|  [12]   | `Char.control`                   | static  | a control character                          |
|  [13]   | `Char.CR`                        | static  | a carriage return                            |
|  [14]   | `Char.LF`                        | static  | a line feed                                  |
|  [15]   | `Char.CRLF`                      | static  | the pair, returning `\n`                     |
|  [16]   | `Char.endOfLine`                 | static  | either terminator, returning `\n`            |
|  [17]   | `Char.digit`                     | static  | a decimal digit                              |
|  [18]   | `Char.octDigit`                  | static  | an octal digit                               |
|  [19]   | `Char.hexDigit`                  | static  | a hexadecimal digit                          |
|  [20]   | `Char.letter`                    | static  | a letter                                     |
|  [21]   | `Char.alphaNum`                  | static  | a letter or digit                            |
|  [22]   | `Char.lower`                     | static  | a lower-case letter                          |
|  [23]   | `Char.upper`                     | static  | an upper-case letter                         |
|  [24]   | `Char.punctuation`               | static  | a punctuation character                      |
|  [25]   | `Char.separator`                 | static  | a Unicode separator                          |
|  [26]   | `Char.symbolchar`                | static  | a Unicode symbol character                   |

- `Char`: this module name collides with `System.Char` under a global `using LanguageExt.Parsec`, so a file reaching both qualifies one of them.

[COMBINATORS_ENTRY_SCOPE]: layout gating (`Indent`)

| [INDEX] | [SURFACE]                          | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :--------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `Indent.indented(int, Parser<T>) ` | static  | run only past a stated offset from the reference |
|  [02]   | `Indent.indented(Parser<T>)`       | static  | run at or past the reference column              |
|  [03]   | `Indent.indented1(Parser<T>)`      | static  | run one column or more past it                   |
|  [04]   | `Indent.indented2(Parser<T>)`      | static  | run two columns or more past it                  |
|  [05]   | `Indent.indented4(Parser<T>)`      | static  | run four columns or more past it                 |

## [03]-[EXPRESSION]

[EXPRESSION_TYPE_SCOPE]: the precedence-table row algebra and the fold that consumes it

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `Operator<A>`  | abstract class | one precedence-table row carrying its `OperatorTag` |
|  [02]   | `InfixOp<A>`   | class          | a binary row holding `Assoc` and its operator       |
|  [03]   | `PrefixOp<A>`  | class          | a prefix row holding its operator                   |
|  [04]   | `PostfixOp<A>` | class          | a postfix row holding its operator                  |
|  [05]   | `Assoc`        | enum           | `None`, `Left`, `Right`                             |
|  [06]   | `OperatorTag`  | enum           | `Infix`, `Prefix`, `Postfix`                        |
|  [07]   | `Operator`     | static class   | the row constructors for both parser families       |
|  [08]   | `Expr`         | static class   | the table-and-term fold                             |

[EXPRESSION_ENTRY_SCOPE]: building an operator-precedence parser

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `Operator.Infix(Assoc, Parser<Func<A, A, A>>)`           | static   | one binary row at a stated associativity         |
|  [02]   | `Operator.Prefix(Parser<Func<A, A>>)`                    | static   | one prefix row                                   |
|  [03]   | `Operator.Postfix(Parser<Func<A, A>>)`                   | static   | one postfix row                                  |
|  [04]   | `Expr.buildExpressionParser(Operator<T>[][], Parser<T>)` | static   | fold the table and a term parser into one parser |
|  [05]   | `op.Tag`                                                 | property | the row's discriminant                           |
|  [06]   | `op.SplitOp(state)`                                      | instance | partition the row into the five operator lanes   |

- `Expr.buildExpressionParser`: outer array index is precedence, highest first; every row inside one inner array binds at equal precedence.
- `Operator<A>.SplitOp`: takes and returns the same five-`Seq` tuple — right-associative, left-associative, non-associative, prefix, postfix — which the fold accumulates per precedence level.

## [04]-[LEXER]

[LEXER_TYPE_SCOPE]: the language definition, its derived lexeme rosters, and the seeds

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :---------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `GenLanguageDef`  | class         | comment forms, identifier and operator shapes, reserveds |
|  [02]   | `GenTokenParser`  | class         | the lexeme roster derived from one definition            |
|  [03]   | `GenTokenParser2` | class         | that roster returning value beside span and index        |
|  [04]   | `Language`        | static class  | the definition seeds                                     |
|  [05]   | `Token`           | static class  | derives `GenTokenParser`                                 |
|  [06]   | `Token2`          | static class  | derives `GenTokenParser2`                                |

[LEXER_ENTRY_SCOPE]: declaring a language and deriving its lexer

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :--------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `GenLanguageDef.Empty`                   | static   | the zero definition every seed refines           |
|  [02]   | `def.With(CommentStart: …, …)`           | instance | named-argument refinement of any field           |
|  [03]   | `Language.HaskellStyle`                  | static   | block, nested, and line comments with `'`-idents |
|  [04]   | `Language.JavaStyle`                     | static   | C-family comment forms and operator characters   |
|  [05]   | `Language.Haskell98Def`                  | static   | `HaskellStyle` plus the Haskell reserved rosters |
|  [06]   | `Token.makeTokenParser(GenLanguageDef)`  | static   | derive the whole lexeme roster                   |
|  [07]   | `Token2.makeTokenParser(GenLanguageDef)` | static   | derive it carrying begin and end position        |

- `GenLanguageDef`: `CommentStart`, `CommentEnd`, `CommentLine`, `NestedComments`, `IdentStart`, `IdentLetter`, `OpStart`, `OpLetter`, `ReservedNames`, `ReservedOpNames`, `CaseSensitive` are public readonly fields, and `With` is the only refinement seat.
- `Language.HaskellStyle` and `JavaStyle`: both carry empty reserved rosters, so a grammar states its own keywords through `With`.

[LEXER_ENTRY_SCOPE]: the derived lexeme roster (`GenTokenParser`)

| [INDEX] | [SURFACE]                    | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :--------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `lexer.Identifier`           | property | an identifier that is no reserved name                  |
|  [02]   | `lexer.Reserved(string)`     | property | one stated keyword, not a prefix of a longer identifier |
|  [03]   | `lexer.Operator`             | property | an operator that is no reserved operator                |
|  [04]   | `lexer.ReservedOp(string)`   | property | one stated operator symbol                              |
|  [05]   | `lexer.CharLiteral`          | property | a character literal with escapes resolved               |
|  [06]   | `lexer.StringLiteral`        | property | a string literal with escapes resolved                  |
|  [07]   | `lexer.Natural`              | property | an unsigned integer in any admitted base                |
|  [08]   | `lexer.Integer`              | property | a signed integer                                        |
|  [09]   | `lexer.Float`                | property | a floating literal                                      |
|  [10]   | `lexer.NaturalOrFloat`       | property | either, as `Either<int, double>`                        |
|  [11]   | `lexer.Decimal`              | property | a base-ten literal                                      |
|  [12]   | `lexer.Hexadecimal`          | property | a base-sixteen literal past its prefix                  |
|  [13]   | `lexer.Octal`                | property | a base-eight literal past its prefix                    |
|  [14]   | `lexer.Symbol(string)`       | property | a literal run followed by whitespace                    |
|  [15]   | `lexer.WhiteSpace`           | property | whitespace and comments under the definition            |
|  [16]   | `lexer.Semi`                 | property | a semicolon lexeme                                      |
|  [17]   | `lexer.Comma`                | property | a comma lexeme                                          |
|  [18]   | `lexer.Colon`                | property | a colon lexeme                                          |
|  [19]   | `lexer.Dot`                  | property | a dot lexeme                                            |
|  [20]   | `lexer.Lexeme(Parser<T>)`    | instance | run a parser and eat trailing whitespace                |
|  [21]   | `lexer.Parens(Parser<T>)`    | instance | bracket in round brackets                               |
|  [22]   | `lexer.Braces(Parser<T>)`    | instance | bracket in curly brackets                               |
|  [23]   | `lexer.Angles(Parser<T>)`    | instance | bracket in angle brackets                               |
|  [24]   | `lexer.Brackets(Parser<T>)`  | instance | bracket in square brackets                              |
|  [25]   | `lexer.CommaSep(Parser<T>)`  | instance | comma-separated, possibly empty                         |
|  [26]   | `lexer.CommaSep1(Parser<T>)` | instance | comma-separated, one minimum                            |
|  [27]   | `lexer.SemiSep(Parser<T>)`   | instance | semicolon-separated, possibly empty                     |
|  [28]   | `lexer.SemiSep1(Parser<T>)`  | instance | semicolon-separated, one minimum                        |

[BRACKET_SEPARATOR_MATRIX]: `ParensCommaSep` `ParensCommaSep1` `BracesCommaSep` `BracesCommaSep1` `AnglesCommaSep` `AnglesCommaSep1` `BracketsCommaSep` `BracketsCommaSep1` `ParensSemiSep` `ParensSemiSep1` `BracesSemiSep` `BracesSemiSep1` `AnglesSemiSep` `AnglesSemiSep1` `BracketsSemiSep` `BracketsSemiSep1`

- `GenTokenParser2`: carries the same roster with `SepBy` and `SepBy1` beside it, every member returning `Parser<(A Value, Pos BeginPos, Pos EndPos, int BeginIndex, int EndIndex)>`; its bracket members take an already-spanned inner parser.

## [05]-[TOKEN_STREAM]

[TOKEN_STREAM_TYPE_SCOPE]: the token-array parser family, under `LanguageExt.Parsec`; `ParserIOExtensions` sits in the global namespace

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `Parser<I, O>`       | delegate       | `PString<I> -> ParserResult<I, O>`                    |
|  [02]   | `PString<T>`         | class          | token array, cursor, user state, `TokenPos` projector |
|  [03]   | `ParserResult<I, O>` | class          | the consumption verdict over a token stream           |
|  [04]   | `Reply<I, O>`        | class          | the success-or-error arm over a token stream          |
|  [05]   | `Operator<I, O>`     | abstract class | one precedence-table row over `Parser<I, O>`          |
|  [06]   | `InfixOp<I, O>`      | class          | a binary row holding `Assoc` and its operator         |
|  [07]   | `PrefixOp<I, O>`     | class          | a prefix row                                          |
|  [08]   | `PostfixOp<I, O>`    | class          | a postfix row                                         |
|  [09]   | `PrimIO`             | static class   | the combinator vocabulary over token input            |
|  [10]   | `ItemIO`             | static class   | token terminals                                       |
|  [11]   | `ExprIO`             | static class   | the table-and-term fold over token input              |
|  [12]   | `IndentIO`           | static class   | layout gating over token input                        |
|  [13]   | `ParserResultIO`     | static class   | verdict constructors taking a `TokenPos` projector    |
|  [14]   | `ParserIOExtensions` | static class   | `Parse`, `label`, and the LINQ operators              |

- `PrimIO`: carries `Prim`'s combinator roster typed `Parser<I, O>`; `manyn`, `manyn0`, `manyn1`, `cons`, `flatten`, and the labelled `notFollowedBy` stay `Prim`-only, and `Prim`'s parser fields spell as generic methods here.

[TOKEN_STREAM_ENTRY_SCOPE]: admitting a token stream, running it, and folding the verdict

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `value.ToPString(Func<T, Pos>)`                              | static   | lift a `Seq<T>` or `IEnumerable<T>` to state |
|  [02]   | `PString<T>.Zero(Func<T, Pos>)`                              | static   | the empty token state                        |
|  [03]   | `PrimIO.parse(Parser<I, O>, Seq<I>, Func<I, Pos>)`           | static   | run over a token sequence                    |
|  [04]   | `parser.Parse(Seq<I>, Func<I, Pos>)`                         | static   | the extension spelling of that run           |
|  [05]   | `input.Cast<U>()`                                            | instance | narrow the token type where `U : T`          |
|  [06]   | `result.ToOption() -> Option<O>`                             | instance | drop the diagnostic, keep presence           |
|  [07]   | `result.ToEither<ERROR>(Func<string, ERROR>)`                | instance | map the rendered text onto an error owner    |
|  [08]   | `ParserResultIO.EmptyError<I, O>(ParserError, Func<I, Pos>)` | static   | mint a failure carrying the projector        |

- `PString<T>` and `ParserResultIO`: every construction takes the `Func<I, Pos>` token-position projector, so a token carries its own source position and the stream never re-scans text for one.

[TOKEN_STREAM_ENTRY_SCOPE]: token terminals and the token-only combinators

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `ItemIO.item(A)`                                                 | static  | one token equal to the stated value           |
|  [02]   | `ItemIO.satisfy(Func<A, bool>)`                                  | static  | one token passing a predicate                 |
|  [03]   | `ItemIO.oneOf(Seq<A>)`                                           | static  | membership in a token set                     |
|  [04]   | `ItemIO.noneOf(Seq<A>)`                                          | static  | exclusion from a token set                    |
|  [05]   | `ItemIO.anyItem<A>()`                                            | static  | any single token                              |
|  [06]   | `ItemIO.str(Seq<A>)`                                             | static  | a token run in order                          |
|  [07]   | `PrimIO.children(Parser<I, Seq<I>>, Parser<I, O>)`               | static  | descend into a token's own children           |
|  [08]   | `PrimIO.asString<I, O>(Parser<I, O>)`                            | static  | render a parsed token payload as `string`     |
|  [09]   | `PrimIO.asInteger(Parser<I, string>, int)`                       | static  | parse an `int` from a parsed string           |
|  [10]   | `PrimIO.asDouble(Parser<I, string>)`                             | static  | parse a `double` from a parsed string         |
|  [11]   | `PrimIO.asFloat(Parser<I, string>)`                              | static  | parse a `float` from a parsed string          |
|  [12]   | `ExprIO.buildExpressionParser(Operator<I, O>[][], Parser<I, O>)` | static  | fold table and term over token input          |
|  [13]   | `IndentIO.indented(int, Parser<TOKEN, A>)`                       | static  | run past a stated offset; `1`/`2`/`4` twin it |
|  [14]   | `Operator.Infix(Assoc, Parser<I, Func<O, O, O>>)`                | static  | one binary row over token input               |

- `PrimIO.children`: seats a tree-shaped token stream — a parent token yields its child sequence and the inner parser runs against that window.

## [06]-[PIPES]

[PIPES_TYPE_SCOPE]: the streaming adapter, under `LanguageExt`

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------ | :------------ | :-------------------------------------------- |
|  [01]   | `ParsecPipes` | static class  | parser state and parser runs as `Pipe` stages |

[PIPES_ENTRY_SCOPE]: lifting input into a pipeline and running a parser inside it

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------ | :------ | :----------------------------------------------------- |
|  [01]   | `ParsecPipes.toParserStringT<M>()`                | static  | map streamed text to `PString`                         |
|  [02]   | `ParsecPipes.toTokenStringT<M, A>(Func<A, Pos>?)` | static  | map streamed token arrays to `PString<A>`              |
|  [03]   | `ParsecPipes.toParserString<RT>()`                | static  | the runtime-bound text stage                           |
|  [04]   | `ParsecPipes.toTokenString<RT, A>(Func<A, Pos>?)` | static  | the runtime-bound token stage                          |
|  [05]   | `parser.ToPipeT<M, OUT>()`                        | static  | run a parser as a `PipeT` stage under `M : MonadIO<M>` |
|  [06]   | `parser.ToPipe<RT, OUT>()`                        | static  | run it as a runtime-bound `Pipe` stage                 |

- `ParsecPipes.ToPipeT` and `ToPipe`: the `Parser<OUT>` arms fail the stage with `Errors.ParseError` carrying the rendered text; the `Parser<IN, OUT>` arms yield nothing on a failed parse and await the next input.
