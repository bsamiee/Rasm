# [VALIDATION]

`Validation<FAIL, A>` is an applicative type for computations that should return a successful value or collect multiple independent failures. Its key advantage over short-circuiting validation is that callers can see every applicable error at once.

## [01]-[LIFTING]

For an applicative functor `F`, `Apply` invokes a wrapped function with a wrapped argument:

```csharp
K<F, B> Apply<A, B>(K<F, Func<A, B>> mf, K<F, A> ma)
```

This is the lifted equivalent of calling `f(a)`. `Map` lifts a regular function and applies it to an `F<A>`; `Apply` expects the function to be lifted already. `Pure` lifts an ordinary value into the applicative structure. These operations let a computation stay in the lifted space instead of repeatedly unpacking and rebuilding values.

In C#, `Pure(value)` first creates `Pure<A>` and `Fail(error)` creates `Fail<E>`. Implicit conversion supplies the eventual concrete type when the return context is known:

```csharp
static Validation<Error, int> CharToDigit(char ch) =>
    ch is >= '0' and <= '9'
        ? Pure(ch - '0')
        : Fail(Error.New($"expected a digit, but got: {ch}"));
```

When type inference cannot resolve these intermediate types, construct the result directly with `Validation<Error, int>.Success(...)` or `.Fail(...)`. Core language-ext types also provide these conversions and `SelectMany` overloads for LINQ expressions.

## [02]-[VALIDATOR_COMPOSITION]

A card-details model has three validated components:

```csharp
public record CardNumber(Seq<int> Number);
public record Expiry(int Month, int Year);
public record CVV(int Number);
public record CreditCardDetails(CardNumber CardNumber, Expiry Expiry, CVV CVV)
{
    public static CreditCardDetails Make(
        CardNumber cardNo,
        Expiry expiry,
        CVV cvv) =>
        new(cardNo, expiry, cvv);
}
```

The records keep the code short, but their public construction does not enforce the represented invariants. Classes can restrict construction when that matters.

### [02.1]-[DIGITS_AND_LENGTH]

Traversing the characters with `CharToDigit` both converts valid characters and collects every invalid-character error:

```csharp
static Validation<Error, Iterable<int>> ValidateAllDigits(string value) =>
    value.AsIterable()
         .Traverse(CharToDigit)
         .As();

static Validation<Error, int> ValidateInt(string value) =>
    ValidateAllDigits(value).Map(_ => int.Parse(value));
```

For example, validating `"xy123"` reports failures for both `x` and `y`, while `"123"` succeeds with the digits `[1, 2, 3]`. The `int` conversion is kept separate because a card number needs the digit sequence rather than one integer.

Length can be validated generically for a foldable value:

```csharp
static Validation<Error, K<F, A>> ValidateLength<F, A>(K<F, A> fa, int length)
    where F : Foldable<F> =>
    fa.Count() == length
        ? Pure(fa)
        : Fail(Error.New(
            $"expected length to be {length}, but got: {fa.Count()}"));

static Validation<Error, string> ValidateLength(string value, int length) =>
    ValidateLength(value.AsIterable(), length).Map(_ => value);
```

### [02.2]-[CVV]

A CVV must contain only digits and have length three. Neither check consumes the other, so they combine applicatively:

```csharp
static Validation<Error, CVV> ValidateCVV(string cvv) =>
    fun<int, string, CVV>((code, _) => new CVV(code))
       .Map(ValidateInt(cvv))
       .Apply(ValidateLength(cvv, 3))
       .As();
```

The recurring applicative shape is a lifted constructor followed by `Map` and one or more `Apply` calls. For `"xy123"`, the result contains the two digit errors and the length error rather than stopping after the first problem.

### [02.3]-[EXPIRY]

The intended expiry rules are two numeric parts separated by a backslash, slash, hyphen, or space: a month from 1 through 12 and a four-digit year from the current year through the next ten years. `Expiry` implements addition and comparison so the month/year pair can participate in a range from the current month through the same month ten years later:

```csharp
public record Expiry(int Month, int Year) :
    IAdditionOperators<Expiry, Expiry, Expiry>,
    IComparisonOperators<Expiry, Expiry, bool>
{
    public static Expiry operator +(Expiry left, Expiry right)
    {
        var month = left.Month + right.Month;
        var year = left.Year + right.Year;
        while (month > 12)
        {
            month -= 12;
            year++;
        }
        return new Expiry(month, year);
    }

    public static bool operator >(Expiry left, Expiry right) =>
        left.Year > right.Year ||
        left.Year == right.Year && left.Month > right.Month;
    public static bool operator >=(Expiry left, Expiry right) =>
        left.Year > right.Year ||
        left.Year == right.Year && left.Month >= right.Month;
    public static bool operator <(Expiry left, Expiry right) =>
        left.Year < right.Year ||
        left.Year == right.Year && left.Month < right.Month;
    public static bool operator <=(Expiry left, Expiry right) =>
        left.Year < right.Year ||
        left.Year == right.Year && left.Month <= right.Month;

    public static Expiry Now
    {
        get
        {
            var now = DateTime.Now;
            return new Expiry(now.Month, now.Year);
        }
    }
    public static Range<Expiry> NextTenYears =>
        LanguageExt.Range.fromMinMax(
            Now,
            Now + new Expiry(0, 10),
            new Expiry(1, 0));
}

static Validation<Error, A> ValidateInRange<A>(A value, Range<A> range)
    where A : IAdditionOperators<A, A, A>, IComparisonOperators<A, A, bool> =>
    range.InRange(value)
        ? Pure(value)
        : Fail(Error.New(
            $"expected value in range of {range.From} to {range.To}, " +
            $"but got: {value}"));
```

The two numeric parses produce the same success type, so `&` combines their values into a `Seq<int>` and accumulates both failures. Range checking consumes the parsed pair, so the LINQ expression sequences it after parsing:

```csharp
static Validation<Error, Expiry> ValidateExpiryDate(string expiryDate) =>
    expiryDate.Split(['\\', '/', '-', ' ']) switch
    {
        [var month, var year] =>
            from parts in ValidateInt(month) & ValidateInt(year)
            let expiry = new Expiry(parts[0], parts[1])
            from _ in ValidateInRange(expiry, Expiry.NextTenYears)
            select expiry,
        _ => Fail(Error.New(
            $"expected expiry-date in the format: MM/YYYY, " +
            $"but got: {expiryDate}"))
};
```

The code validates both parts as integers and then checks the combined month/year value against the date range. It does not actually enforce the stated two-character month or four-character year shapes, nor does it run a separate `1..12` month check.

For validations with the same success type, `&` requires all operands to succeed and collects their successful values; their failures are accumulated. `|` succeeds when either operand succeeds and combines errors only when both fail.

### [02.4]-[CARD_NUMBER]

The number must be all digits, contain 16 characters, and satisfy the Luhn checksum. Digit and length checks are independent, but checksum validation depends on their successful digit sequence:

```csharp
static Validation<Error, CardNumber> ValidateCardNumber(string cardNo) =>
    (ValidateAllDigits(cardNo), ValidateLength(cardNo, 16))
        .Apply((digits, _) => digits.ToSeq())
        .Bind(ValidateLuhn)
        .Map(digits => new CardNumber(digits))
        .As();
```

Tuple-based `Apply` is an alternative to an explicit `Map`/`Apply` chain and can avoid writing the lambda's full type signature. If C# does not coerce a tuple operand to the
required higher-kinded interface, the explicit chain is the fallback. `Bind` is used only at the dependency boundary: the Luhn check runs after the shape checks succeed.

## [03]-[COMPLETE_RESULT]

The three component validators are independent, so the final constructor is lifted and applied to each result:

```csharp
public static Validation<Error, CreditCardDetails> Validate(
    string cardNo,
    string expiryDate,
    string cvv) =>
    fun<CardNumber, Expiry, CVV, CreditCardDetails>(CreditCardDetails.Make)
       .Map(ValidateCardNumber(cardNo))
       .Apply(ValidateExpiryDate(expiryDate))
       .Apply(ValidateCVV(cvv))
       .As();
```

If all three succeed, the result is `CreditCardDetails`. If several fields are invalid, their errors are returned together.

## [04]-[FAILURE_CONTEXT]

Character-level failures are precise but lack field context. `MapFail` can replace or wrap them where that context is known:

```csharp
static Validation<Error, CVV> ValidateCVV(string cvv) =>
    fun<int, string, CVV>((code, _) => new CVV(code))
       .Map(ValidateInt(cvv)
           .MapFail(_ => Error.New("CVV code should be a number")))
       .Apply(ValidateLength(cvv, 3)
           .MapFail(_ => Error.New("CVV code should be 3 digits in length")))
       .As();
```

An outer error can retain the original details as an inner error:

```csharp
validation.MapFail(error => Error.New("card number not valid", error));
```

This supports concise messages for callers without discarding diagnostic detail.

## [05]-[ACCUMULATION_SEMANTICS]

`Validation`'s `Apply` handles the four meaningful combinations directly:
- successful function and successful argument: invoke the function;
- successful function and failed argument: return the argument failure;
- failed function and successful argument: return the function failure;
- failed function and failed argument: return `Fail(e1 + e2)`.

Therefore the `FAIL` type must be a `Monoid`: it needs an associative combination and an empty value. The built-in `Error` type supplies this behavior. A bespoke failure type must also be monoidal for `Validation` to accumulate its values applicatively.
