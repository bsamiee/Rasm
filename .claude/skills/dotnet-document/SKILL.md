---
name: dotnet-document
description: "Use when adding or reviewing XML doc comments on a C# member, or when a build fails on a compiler or Roslynator doc-comment diagnostic."
---

# [DOTNET_DOCUMENT]

Covers XML documentation comments (`///`) on C# members: the Microsoft conventions for summaries, parameters, return values, and remarks, the tag and cross-reference syntax, and the phrasing for each member kind. Member documentation is optional (`CS1591` is off), and a member that gets a doc comment follows these patterns.

[REFERENCES]:
- [01]-[REVIEW](references/review.md): Severity taxonomy, source-first review procedure, factual, example, and quality checks, and the closing report

## [01]-[PRINCIPLES]

Follow Microsoft Documentation Conventions:
- Write XML doc comments (`///`) on the members the code documentation standard selects, never on trivial getters and setters without side effects
- Use `<see cref="..."/>` for cross-references, not plain text names
- Refer to parameters in descriptions with `<paramref name="..."/>`
- Document what, not how, the code shows the implementation
- Document every `DllImport` P/Invoke method with the C function it calls and its marshaling behavior

C# documentation must be lean and precise. It assumes developers understand modern FP/expression, OOP patterns, async/await, and standard .NET idioms and documents the parts specific to the architecture:
- Threading constraints — why something must run on a specific thread
- Native interop details — why the code uses a specific marshalling approach
- Platform differences — why code branches by platform

The same summary in both forms:

```csharp
// BAD: Overly verbose
/// <summary>
/// This method is responsible for handling the event that occurs when
/// the user taps on the screen. It takes the tap coordinates and
/// determines which grid cell was tapped, then sends the appropriate
/// command to the native library through the interop layer.
/// </summary>

// GOOD: Concise and technical
/// <summary>Handles a screen tap by translating touch coordinates to grid coordinates and dispatching the corresponding command to the native library</summary>
```

## [02]-[SYNTAX]

Every documented member uses the same tag layout:

```csharp
/// <summary>Brief description of the member</summary>
/// <param name="paramName">Description of the parameter</param>
/// <returns>Description of the return value</returns>
/// <remarks>Additional context, edge cases, or threading considerations</remarks>
```

### [02.1]-[TAGS]

Each tag has one use, and an element with no content is omitted (`RCS1228` fails the build on an empty element):

| [INDEX] | [TAG]                    | [USAGE]                                                             |
| :-----: | :----------------------- | :------------------------------------------------------------------ |
|  [01]   | `<summary>`              | Brief description (required)                                        |
|  [02]   | `<param name="x">`       | Parameter description, every parameter or none (`CS1573`)           |
|  [03]   | `<typeparam name="T">`   | Type parameter description, every type parameter or none (`CS1712`) |
|  [04]   | `<returns>`              | Return value description                                            |
|  [05]   | `<value>`                | Property value description                                          |
|  [06]   | `<remarks>`              | Extended details, one `<para>` per topic                            |
|  [07]   | `<list type="bullet">`   | Parallel items inside `<remarks>`                                   |
|  [08]   | `<exception cref="...">` | Exception the member throws                                         |
|  [09]   | `<inheritdoc/>`          | Inherits the base or interface documentation into the XML file      |

### [02.2]-[CROSS_REFERENCES]

Cross-references name types, members, parameters, and keywords through these tags:
- `cref` names a code element and the compiler resolves it (`CS1574`), a generic target is written as `List{T}`
- `<see href="...">` names a URL
- `<see langword>` names `true`, `false`, and `null`, never backticks

```xml
<see cref="Drawing.Canvas" />            <!-- Type -->
<see cref="Drawing.Canvas.DrawRect" />   <!-- Method -->
<see cref="Drawing.Paint.Color" />       <!-- Property -->
<see cref="Drawing.Colors.Red" />        <!-- Field -->
<paramref name="paint" />                    <!-- Parameter in same method -->

<!-- Keywords -->
<see langword="true" />
<see langword="false" />
<see langword="null" />
```

### [02.3]-[ESCAPING]

Escape these characters inside documentation text:

| [INDEX] | [CHARACTER] | [ESCAPE] |
| :-----: | :---------- | :------- |
|  [01]   | `<`         | `&lt;`   |
|  [02]   | `>`         | `&gt;`   |
|  [03]   | `&`         | `&amp;`  |

## [03]-[SUMMARIES]

Every documented member requires a `<summary>` tag, and its text follows these patterns:
- One line, one sentence, no trailing period (`RCS1253` with `roslynator_doc_comment_summary_style = single_line`)
- Begin with a present-tense, third-person verb
- Do not repeat the member name, provide meaningful context
- Use language-neutral text (no C#/VB-specific terms)
- Avoid parameter names and self-referential names

## [04]-[MEMBERS]

Each member kind has its own opening phrase and tag set, and parameter and return descriptions are noun phrases without the data type.

### [04.1]-[CLASSES_AND_STRUCTS]

- A class summary states what the type holds or does
- Include `<remarks>` only for a class with non-obvious architectural significance

The class summary:

```xml
<summary>Holds the style and color information about how to draw geometries, text and bitmaps</summary>
```

Types that wrap native resources (`IDisposable`) must have remarks that cover:
- What the type does
- How to create instances (constructor vs factory)
- Disposal pattern — always show `using` in examples
- Threading constraints if any

### [04.2]-[CONSTRUCTORS]

- Always open with the exact .NET phrase "Initializes a new instance of the `<see cref>` class"
- Use "struct" instead of "class" for value types, the declaration shows the type kind
- Shortened forms ("Initializes a new `<see cref>` from a packed value") violate the guideline

Constructor summaries by kind:

```xml
<!-- Class constructor -->
<summary>Initializes a new instance of the <see cref="Drawing.Paint" /> class</summary>

<!-- With parameters - describe what makes this overload different -->
<summary>Initializes a new instance of the <see cref="Drawing.Bitmap" /> class with the specified dimensions</summary>
<param name="width">The width of the bitmap</param>
<param name="height">The height of the bitmap</param>

<!-- Struct constructor (note "struct", not "class") -->
<summary>Initializes a new instance of the <see cref="Drawing.Point" /> struct</summary>

<!-- Abstract class constructor -->
<summary>Called from constructors in derived classes to initialize the <see cref="Drawing.NativeObject" /> class</summary>

<!-- BAD: omits "instance of the ... struct" -->
<summary>Initializes a new <see cref="Drawing.Color" /> from a packed BGRA value.</summary>
<!-- GOOD: full phrase with "struct" -->
<summary>Initializes a new instance of the <see cref="Drawing.Color" /> struct from a packed BGRA value</summary>
```

### [04.3]-[PROPERTIES]

The declaration's accessor decides the opening verb, read it and do not infer the verb from the property's purpose:
- `{ get; set; }` → "Gets or sets"
- `{ get; }` → "Gets"
- Struct properties are a trap: many look read-only but are settable
- Boolean properties open with "Gets a value indicating whether"
- `<value>` describes a state, like a return value, and reads "true if", never "true to"

Property summaries and values by accessor:

```xml
<!-- Read/write — signature: public Color Color { get; set; } -->
<summary>Gets or sets the color</summary>
<value>The color value</value>

<!-- Read-only — signature: public int Width { get; } (do NOT say "This property is read-only") -->
<summary>Gets the width of the bitmap</summary>
<value>The width in pixels</value>

<!-- Boolean read/write -->
<summary>Gets or sets a value indicating whether anti-aliasing is enabled</summary>
<value><see langword="true" /> if anti-aliasing is enabled; otherwise, <see langword="false" /></value>

<!-- Boolean read-only -->
<summary>Gets a value indicating whether the path is empty</summary>
<value><see langword="true" /> if the path contains no lines or curves; otherwise, <see langword="false" /></value>

<!-- BAD: signature is { get; set; } but summary says only "Gets" -->
<summary>Gets the variation axis minimum value.</summary>   <!-- should be "Gets or sets" -->
```

### [04.4]-[METHODS]

Each method pattern has its own phrasing:

```xml
<!-- General method -->
<summary>Draws a rectangle using the specified paint</summary>
<param name="rect">The rectangle to draw</param>
<param name="paint">The paint to use</param>

<!-- Async method -->
<summary>Asynchronously encodes the image to the specified format</summary>

<!-- Factory method -->
<summary>Creates a new image from encoded data</summary>
<param name="data">The encoded image data</param>
<returns>A new image, or <see langword="null" /> if the data is invalid</returns>

<!-- Try pattern -->
<summary>Attempts to parse the color from a string</summary>
<param name="value">The string to parse</param>
<param name="color">When this method returns, contains the parsed color if successful. This parameter is treated as uninitialized</param>
<returns><see langword="true" /> if the parsing succeeded; otherwise, <see langword="false" /></returns>

<!-- Dispose() -->
<summary>Releases the resources used by the current instance of the <see cref="Drawing.Paint" /> class</summary>

<!-- Dispose(Boolean) -->
<summary>Called by the <see cref="Drawing.Paint.Dispose" /> and <see cref="System.Object.Finalize" /> methods to release the managed and unmanaged resources used by the current instance of the <see cref="Drawing.Paint" /> class</summary>
```

Async methods:
- Document what the method awaits and whether it runs on a background thread

### [04.5]-[EVENTS]

Event summaries open with "Occurs when":

```xml
<summary>Occurs when the surface needs to be repainted</summary>
```

### [04.6]-[ENUMS]

- Document the enum and non-obvious members
- For `[Flags]`, document combination semantics

Enum type and member summaries:

```xml
<!-- Type -->
<summary>Specifies the blend mode for drawing operations</summary>

<!-- Members - no verb needed -->
<summary>Source pixels replace destination pixels</summary>
<summary>Source and destination pixels are blended</summary>
```

### [04.7]-[PARAMETERS]

- Cover every parameter or none (`CS1573`), in declaration order (`RCS1232`)
- Begin with an article (The, A, An)
- Write "true to..." for booleans, not "true if..."
- Write `<see langword="null" />` for a nullable parameter, not `default`

Parameter descriptions by kind:

```xml
<!-- Class/struct parameter -->
<param name="rect">The rectangle to draw</param>

<!-- Boolean parameter: "true to...", NOT "true if..." -->
<param name="antialias"><see langword="true" /> to enable anti-aliasing; otherwise, <see langword="false" /></param>

<!-- Enum parameter -->
<param name="blendMode">One of the enumeration values that specifies the blend mode</param>

<!-- Flag enum parameter -->
<param name="flags">A bitwise combination of the enumeration values that specifies the options</param>

<!-- out parameter -->
<param name="result">When this method returns, contains the parsed value if successful. This parameter is treated as uninitialized</param>
```

### [04.8]-[RETURN_VALUES]

- Write `<returns>` when the summary does not open with "Returns"
- For a result type, state the success value and each failure case, never the carrier name
- Write "true if..." for booleans, not "true to..."

Return descriptions by kind:

```xml
<!-- Object -->
<returns>A new image</returns>

<!-- Nullable -->
<returns>A new image, or <see langword="null" /> if the data is invalid</returns>

<!-- Boolean: "true if...", NOT "true to..." -->
<returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" /></returns>

<!-- Enum -->
<returns>One of the enumeration values that indicates the result</returns>
```

### [04.9]-[EXCEPTIONS]

- Document explicitly thrown exceptions with `<exception>` tags, a failure returned as data belongs in `<returns>`
- Document `InvalidOperationException` for state preconditions

Exception descriptions:

```xml
<exception cref="System.ArgumentNullException"><paramref name="paint" /> is <see langword="null" /></exception>
<exception cref="System.ArgumentOutOfRangeException"><paramref name="width" /> is less than zero</exception>
```

## [05]-[THREADING]

Thread safety requirements go in `<remarks>` and name the thread, lock, or monitor involved:
- State when a method must run on the UI thread
- Detail any `lock()` or `Monitor.TryEnter` requirements
