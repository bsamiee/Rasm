---
name: dotnet-document
description: "Use when adding or reviewing XML doc comments on a C# member, or when a build fails on a compiler or Roslynator doc-comment diagnostic."
---

# [DOTNET_DOCUMENT]

Covers XML documentation comments (`///`) on C# members: the tag and cross-reference syntax, the summary standard, the opening phrase and tag set for each member kind, and the diagnostics the build raises on them. `GenerateDocumentationFile` is on in every project, member documentation is optional (`CS1591`, `RCS1140`, `RCS1141`, `RCS1142`, and `RCS1181` are off), and every member that has a doc comment follows these patterns.

[REFERENCES]:
- [01]-[REVIEW](references/review.md): Severity classes, the procedure, the factual, example, and quality checks, and the closing report

## [01]-[PRINCIPLES]

- Write doc comments on the members a caller uses without opening the source, never on trivial accessors with no side effect
- Name types and members with `<see cref="..."/>` and parameters with `<paramref name="..."/>`, the compiler does not resolve a plain-text name and a rename leaves it unchanged
- State what the member does, not how the body does it, a sentence that names a private field, a loop, or a called helper describes the implementation and goes
- Put the reason for a marshaling choice or a platform branch in `<remarks>`, the code shows the branch without its reason
- Document every `DllImport` or `LibraryImport` method with the C function it calls and its marshaling behavior

## [02]-[SYNTAX]

### [02.1]-[TAGS]

Each tag has one use, and an element with no content goes (`RCS1228` fails the build on an empty element):

| [INDEX] | [TAG]                      | [USAGE]                                                                                             |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `<summary>`                | One-sentence description, required on every documented member                                       |
|  [02]   | `<param name="x">`         | Parameter description, every parameter or none (`CS1573`), in declaration order (`RCS1232`)         |
|  [03]   | `<typeparam name="T">`     | Type parameter description, every type parameter or none (`CS1712`)                                 |
|  [04]   | `<returns>`                | Return value description                                                                            |
|  [05]   | `<value>`                  | Property value description                                                                          |
|  [06]   | `<remarks>`                | Usage facts the other tags do not state, one `<para>` per topic                                     |
|  [07]   | `<para>`                   | Paragraph inside `<summary>`, `<remarks>`, or `<returns>`, required past one paragraph (`RCS1226`)  |
|  [08]   | `<list type="bullet">`     | List inside `<remarks>`, one `<item>` per entry, `type` is `bullet`, `number`, or `table`           |
|  [09]   | `<c language="csharp">`    | Inline code, `<code>` for more than one line (`RCS1247`), `language` required unless one keyword    |
|  [10]   | `<code language="csharp">` | Preformatted block on its own lines inside `<example>`, `language` required (`MA0219`, `MA0218`)    |
|  [11]   | `<example>`                | Usage example, one sentence and one `<code>` block                                                  |
|  [12]   | `<exception cref="...">`   | Exception the member throws, the text states the condition                                          |
|  [13]   | `<inheritdoc/>`            | Copies the base or interface documentation into the XML file, tags on the member stay               |

`<inheritdoc cref="..."/>` copies from the named member, `path` filters the copied tags with an XPath expression, and Visual Studio inherits documentation for an override or implementation in the IDE only, the XML file the compiler writes holds nothing without the tag.

### [02.2]-[CROSS_REFERENCES]

Cross-references name types, members, parameters, and keywords through these tags:
- `cref` names a type or member, the compiler resolves it through the `using` directives (`CS1574` when unresolved, `CS1584` when the syntax is wrong, `CS1580` when it names a type parameter), a generic target takes the form `List{T}`, and link text sits between `<see cref="...">` and `</see>`
- `<see href="...">` names a URL, `cref` produces no link for a URL
- `<see langword="...">` names a keyword (`true`, `false`, `null`), `<c>true</c>` and backticks fail `MA0154`
- `<paramref name="..."/>` names a parameter and `<typeparamref name="T"/>` names a type parameter
- `<seealso cref="..."/>` lists a related member and cannot sit inside `<summary>`

```xml
<see cref="Drawing.Canvas" />            <!-- Type -->
<see cref="Drawing.Canvas.DrawRect" />   <!-- Method -->
<see cref="Drawing.Paint.Color" />       <!-- Property -->
<see cref="Drawing.Colors.Red" />        <!-- Field -->
<see cref="Seq{T}" />                    <!-- Generic type -->
<paramref name="paint" />                <!-- Parameter of the same member -->
<typeparamref name="T" />                <!-- Type parameter of the same member -->

<!-- Keywords -->
<see langword="true" />
<see langword="false" />
<see langword="null" />
```

### [02.3]-[ESCAPING]

Escape these characters inside documentation text, an unescaped one fails `CS1570`:

| [INDEX] | [CHARACTER] | [ESCAPE] |
| :-----: | :---------- | :------- |
|  [01]   | `<`         | `&lt;`   |
|  [02]   | `>`         | `&gt;`   |
|  [03]   | `&`         | `&amp;`  |

## [03]-[SUMMARIES]

Every documented member has a `<summary>` on one line (`RCS1253` with `roslynator_doc_comment_summary_style = single_line`), and its text is one sentence in the third person that states what the member does, returns, or represents, with no filler, no hedge, and no trailing period:
- Open with a present-tense verb, except the exception class ("The exception that is thrown when"), the enum member (a noun phrase), and the abstract or virtual member ("When overridden in a derived class,")
- Name the behavior, not the member name: `String.Format` reads "Replaces each format item in a specified string with the string representation of a specified object", not "Formats a string"
- Restate no part of the signature: no parameter name, no member name, and no type name, except the type name in a constructor or `Dispose` summary
- Give overloads one general summary broad enough for every overload, and each overload a summary that names what its parameters add

## [04]-[MEMBERS]

Each member kind has its own opening phrase and tag set. Parameter, return, and value descriptions are noun phrases that open with an article and omit the data type, a boolean parameter reads "true to ...; otherwise, false", and a boolean return or property value reads "true if ...; otherwise, false".

### [04.1]-[CLASSES_AND_STRUCTS]

- Class and struct summaries state what the type holds, does, or represents, an interface opens with "Defines", "Provides", or "Exposes", an abstract base class reads "Defines the core behavior of X and provides a base for Y", and an exception class reads "The exception that is thrown when ..."
- `<remarks>` on a type holds usage facts the summary does not state, one `<para>` per topic, and a type with none has no `<remarks>`

The class summary, and for a type that owns a native resource the `<remarks>` that state how to create an instance (constructor or factory) and that the caller disposes it, with the `using` form in an `<example>`:

```xml
<summary>Holds the style and color information about how to draw geometries, text and bitmaps</summary>
<remarks><para>Instances come from the constructor, and the caller disposes each one after its last draw call</para></remarks>
<example>
Draws a rectangle with a paint the caller owns:
<code language="csharp">
using var paint = new Paint { Color = Colors.Red };
canvas.DrawRect(rect, paint);
</code>
</example>
```

### [04.2]-[CONSTRUCTORS]

- Open with the exact .NET phrase "Initializes a new instance of the `<see cref>` class", "struct" for a value type, and "Called from constructors in derived classes to initialize the `<see cref>` class" for an abstract class
- Keep the full phrase, a shortened form ("Initializes a new `<see cref>` from a packed value") drops it, and the overload text follows "class" or "struct"

Constructor summaries by kind:

```xml
<!-- Class constructor -->
<summary>Initializes a new instance of the <see cref="Drawing.Paint" /> class</summary>

<!-- With parameters, the text after the phrase names what this overload adds -->
<summary>Initializes a new instance of the <see cref="Drawing.Bitmap" /> class with the specified dimensions</summary>
<param name="width">The width of the bitmap, in pixels</param>
<param name="height">The height of the bitmap, in pixels</param>

<!-- Struct constructor -->
<summary>Initializes a new instance of the <see cref="Drawing.Point" /> struct</summary>

<!-- Abstract class constructor -->
<summary>Called from constructors in derived classes to initialize the <see cref="Drawing.NativeObject" /> class</summary>
```

### [04.3]-[PROPERTIES]

The accessor list decides the opening verb, read it and never infer the verb from the property's purpose:
- `{ get; set; }` opens with "Gets or sets", `{ get; }` with "Gets", and `{ get; init; }` with "Gets or initializes"
- Boolean properties open with "Gets a value that indicates whether" or "Gets or sets a value that indicates whether"
- Struct properties with a `set` accessor are settable, value semantics do not make them read-only
- `<value>` names the value with its unit ("The width, in pixels"), a default is a second sentence "The default is X" only when the source shows the initializer, and the sentence "This property is read-only" goes

Property summaries and values by accessor:

```xml
<!-- Read/write, signature public Color Color { get; set; } -->
<summary>Gets or sets the color</summary>
<value>The color value</value>

<!-- Read-only, signature public int Width { get; } -->
<summary>Gets the width of the bitmap</summary>
<value>The width, in pixels</value>

<!-- Boolean read/write, signature public bool Antialias { get; set; } = true; -->
<summary>Gets or sets a value that indicates whether anti-aliasing is enabled</summary>
<value><see langword="true" /> if anti-aliasing is enabled; otherwise, <see langword="false" />. The default is <see langword="true" /></value>

<!-- Boolean read-only -->
<summary>Gets a value that indicates whether the path is empty</summary>
<value><see langword="true" /> if the path contains no lines or curves; otherwise, <see langword="false" /></value>
```

### [04.4]-[METHODS]

Each method pattern has its own opening phrase:

```xml
<!-- General method -->
<summary>Draws a rectangle using the specified paint</summary>
<param name="rect">The rectangle to draw</param>
<param name="paint">The paint to use</param>

<!-- Task-returning method -->
<summary>Asynchronously encodes the image to the specified format</summary>
<returns>A task object that, when awaited, produces the encoded bytes</returns>

<!-- Factory method -->
<summary>Creates a new image from encoded data</summary>
<param name="data">The encoded image data</param>
<returns>A new image, or <see langword="null" /> if the data is invalid</returns>

<!-- Try pattern -->
<summary>Attempts to parse the color from a string</summary>
<param name="value">The string to parse</param>
<param name="color">When this method returns, contains the parsed color if the parsing succeeded. This parameter is treated as uninitialized</param>
<returns><see langword="true" /> if the parsing succeeded; otherwise, <see langword="false" /></returns>

<!-- Abstract or virtual member of an abstract class -->
<summary>When overridden in a derived class, reads the next byte from the stream</summary>

<!-- Dispose() -->
<summary>Releases the resources used by the current instance of the <see cref="Drawing.Paint" /> class</summary>

<!-- Dispose(Boolean) -->
<summary>Called by the <see cref="Drawing.Paint.Dispose" /> and <see cref="System.Object.Finalize" /> methods to release the managed and unmanaged resources used by the current instance of the <see cref="Drawing.Paint" /> class</summary>
<param name="disposing"><see langword="true" /> to release managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources</param>
```

### [04.5]-[EVENTS]

Event summaries open with "Occurs when", the `On` method that raises the event reads "Raises the `<see cref>` event", and an `EventArgs` class reads "Provides data for the `<see cref>` event":

```xml
<summary>Occurs when the surface needs to be repainted</summary>
<summary>Raises the <see cref="Drawing.Surface.Paint" /> event</summary>
<summary>Provides data for the <see cref="Drawing.Surface.Paint" /> event</summary>
```

### [04.6]-[ENUMS]

- The type summary opens with "Specifies" or "Describes", a member summary is a noun phrase or a sentence with no opening verb, and a mask member reads "A mask used to retrieve X"
- `[Flags]` enums state which members combine and which stand alone

Enum type and member summaries:

```xml
<!-- Type -->
<summary>Specifies the blend mode for drawing operations</summary>

<!-- Members -->
<summary>The source pixel in place of the destination pixel</summary>
<summary>A blend of the source and destination pixels</summary>
```

### [04.7]-[PARAMETERS]

- Cover every parameter or none (`CS1573`), in declaration order (`RCS1232`)
- Open with an article, state the unit, the valid range, and the default in the description, and write `<see langword="null" />` for a nullable parameter, not `default`
- Write "One of the enumeration values that specifies ..." for an enum parameter and "A bitwise combination of the enumeration values that specifies ..." for a `[Flags]` parameter
- Write "When this method returns, contains ... This parameter is treated as uninitialized" for an `out` parameter, ", passed by reference" at the end of a `ref` parameter, and "The zero-based index of ..." for an indexer integer

Parameter descriptions by kind:

```xml
<!-- Class/struct parameter -->
<param name="rect">The rectangle to draw</param>

<!-- Primitive with unit, range, and default -->
<param name="bufferSize">The size of the buffer, in bytes. This value must be greater than zero. The default size is 4096</param>

<!-- Boolean parameter -->
<param name="antialias"><see langword="true" /> to enable anti-aliasing; otherwise, <see langword="false" /></param>

<!-- Enum parameter -->
<param name="blendMode">One of the enumeration values that specifies the blend mode</param>

<!-- Flag enum parameter -->
<param name="flags">A bitwise combination of the enumeration values that specifies the options</param>

<!-- out parameter -->
<param name="result">When this method returns, contains the parsed value if the parsing succeeded. This parameter is treated as uninitialized</param>
```

### [04.8]-[RETURN_VALUES]

- LanguageExt returns name the success value and each failure case, never the result type: `Option<A>` reads "The X, or none when ...", `Fin<A>` reads "The X, or a `<see cref>` error when ...", `Validation<Error, A>` reads "The X, or every error from ...", and `Either<L, R>` reads "The L when ..., or the R when ..."
- `Task` and `ValueTask` returns read "A task object that, when awaited, produces ...", enums "One of the enumeration values that indicates ...", and `[Flags]` enums "A bitwise combination of the enumeration values that ..."

Return descriptions by kind:

```xml
<!-- Object -->
<returns>A new image</returns>

<!-- Nullable -->
<returns>A new image, or <see langword="null" /> if the data is invalid</returns>

<!-- Boolean -->
<returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" /></returns>

<!-- Enum -->
<returns>One of the enumeration values that indicates the result</returns>

<!-- Option<Person> -->
<returns>The person with the specified name, or none when no person has that name</returns>

<!-- Fin<Age> -->
<returns>The validated age, or an <see cref="InvalidAge" /> error when the value is negative or at least 120</returns>

<!-- Validation<Error, Person> -->
<returns>The registered person, or every error from the name and age checks</returns>
```

### [04.9]-[EXCEPTIONS]

- Document each exception the member throws with `<exception cref>`, a failure returned as data belongs in `<returns>`
- State the condition as if "if" preceded it, in the present tense, with `<paramref>` for the parameter, separate conditions of one exception with "-or-", and end a condition of an exception stored in the returned task with "This exception is stored into the returned task"
- Document `InvalidOperationException` with the instance state that rejects the call as the condition ("The path is empty"), and a call after `Dispose` with `ObjectDisposedException`

Exception descriptions:

```xml
<exception cref="System.ArgumentNullException"><paramref name="paint" /> is <see langword="null" /></exception>
<exception cref="System.ArgumentOutOfRangeException"><paramref name="width" /> is less than zero</exception>
<exception cref="System.InvalidOperationException">The path is empty</exception>
<exception cref="System.ObjectDisposedException">The surface is disposed</exception>
```

## [05]-[THREADING]

Thread constraints go in `<remarks>` and name the thread, lock, or monitor:
- State the thread a member must run on (the UI thread) and whether an awaited continuation resumes on it
- Detail any `lock()` or `Monitor.TryEnter` requirement the caller must satisfy
