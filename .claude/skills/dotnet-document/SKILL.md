---
name: dotnet-document
description: "Use when adding or reviewing XML doc comments on a C# member, or a build fails on a compiler or Roslynator doc-comment diagnostic."
---

# [DOTNET_DOCUMENT]

Covers XML documentation comments (`///`) on C# members, from the tag syntax to the diagnostics the build raises on them. `GenerateDocumentationFile` is on in every project, member documentation is optional (`CS1591`, `RCS1140`, `RCS1141`, `RCS1142`, and `RCS1181` are off), every member that has a doc comment follows the patterns.

[REFERENCES]:
- [01]-[REVIEW](references/review.md): The review of existing doc comments against their source, with severity classes and the closing report

## [01]-[PRINCIPLES]

- A doc comment sits on a member a caller uses without opening the source
- Types and members are named with `<see cref="..."/>` and parameters with `<paramref name="..."/>`, the compiler does not resolve a plain-text name
- The text states what the member does, a sentence that names a private field, a loop, or a called helper describes the implementation
- The reason for a marshaling choice or a platform branch sits in `<remarks>`
- Every `DllImport` or `LibraryImport` method documents the C function it calls and its marshaling behavior

## [02]-[SYNTAX]

### [02.1]-[TAGS]

Each tag has one use, `RCS1228` fails the build on an empty element:

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

`<inheritdoc cref="..."/>` copies from the named member, `path` filters the copied tags with an XPath expression. Visual Studio inherits documentation for an override or implementation in the IDE alone, the XML file the compiler writes holds nothing without the tag.

### [02.2]-[CROSS_REFERENCES]

- `cref` names a type or member, the compiler resolves it through the `using` directives (`CS1574` when unresolved, `CS1584` when the syntax is wrong, `CS1580` when it names a type parameter), a generic target takes the form `List{T}`, link text sits between `<see cref="...">` and `</see>`
- `<see href="...">` names a URL
- `<see langword="...">` names a keyword (`true`, `false`, `null`), `<c>true</c>` and backticks fail `MA0154`
- `<paramref name="..."/>` names a parameter, `<typeparamref name="T"/>` a type parameter
- `<seealso cref="..."/>` lists a related member and cannot sit inside `<summary>`

```xml
<see cref="Drawing.Canvas" />
<see cref="Drawing.Canvas.DrawRect" />
<see cref="Drawing.Paint.Color" />
<see cref="Seq{T}" />
<paramref name="paint" />
<typeparamref name="T" />
<see langword="null" />
```

### [02.3]-[ESCAPING]

An unescaped character inside documentation text fails `CS1570`:

| [INDEX] | [CHARACTER] | [ESCAPE] |
| :-----: | :---------- | :------- |
|  [01]   | `<`         | `&lt;`   |
|  [02]   | `>`         | `&gt;`   |
|  [03]   | `&`         | `&amp;`  |

## [03]-[SUMMARIES]

Every documented member has a `<summary>` on one line (`RCS1253` with `roslynator_doc_comment_summary_style = single_line`), one sentence in the third person that states what the member does, returns, or represents, with no trailing period:
- Open with a present-tense verb, except the exception class ("The exception that is thrown when"), the enum member (a noun phrase), and the abstract or virtual member ("When overridden in a derived class,")
- Name the behavior, `String.Format` reads "Replaces each format item in a specified string with the string representation of a specified object"
- Restate no part of the signature, except the type name in a constructor or `Dispose` summary
- Overloads take one general summary broad enough for every overload, each overload a summary that names what its parameters add

## [04]-[MEMBERS]

Each member kind has its own opening phrase and tag set. Parameter, return, and value descriptions are noun phrases that open with an article and omit the data type, a boolean parameter reads `true to ...; otherwise, false`, a boolean return or property value reads `true if ...; otherwise, false`.

### [04.1]-[CLASSES_AND_STRUCTS]

- A class or struct summary states what the type holds, does, or represents, an interface opens with "Defines", "Provides", or "Exposes", an abstract base class reads "Defines the core behavior of X and provides a base for Y", an exception class reads "The exception that is thrown when ..."
- `<remarks>` on a type holds usage facts the summary does not state, one `<para>` per topic

A type that owns a native resource states in `<remarks>` how to create an instance and that the caller disposes it, with the `using` form in an `<example>`:

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

The summary opens with the exact .NET phrase "Initializes a new instance of the `<see cref>` class", "struct" for a value type, and "Called from constructors in derived classes to initialize the `<see cref>` class" for an abstract class. The overload text follows "class" or "struct":

```xml
<summary>Initializes a new instance of the <see cref="Drawing.Paint" /> class</summary>

<summary>Initializes a new instance of the <see cref="Drawing.Bitmap" /> class with the specified dimensions</summary>
<param name="width">The width of the bitmap, in pixels</param>
<param name="height">The height of the bitmap, in pixels</param>

<summary>Initializes a new instance of the <see cref="Drawing.Point" /> struct</summary>

<summary>Called from constructors in derived classes to initialize the <see cref="Drawing.NativeObject" /> class</summary>
```

### [04.3]-[PROPERTIES]

The accessor list decides the opening verb:
- `{ get; set; }` opens with "Gets or sets", `{ get; }` with "Gets", `{ get; init; }` with "Gets or initializes"
- A boolean property opens with "Gets a value that indicates whether" or "Gets or sets a value that indicates whether"
- A struct property with a `set` accessor is settable
- `<value>` names the value with its unit ("The width, in pixels"), a default is a second sentence "The default is X" when the source shows the initializer

The summary and value of `public Color Color { get; set; }`, `public int Width { get; }`, `public bool Antialias { get; set; } = true;`, and a read-only boolean:

```xml
<summary>Gets or sets the color</summary>
<value>The color value</value>

<summary>Gets the width of the bitmap</summary>
<value>The width, in pixels</value>

<summary>Gets or sets a value that indicates whether anti-aliasing is enabled</summary>
<value><see langword="true" /> if anti-aliasing is enabled; otherwise, <see langword="false" />. The default is <see langword="true" /></value>

<summary>Gets a value that indicates whether the path is empty</summary>
<value><see langword="true" /> if the path contains no lines or curves; otherwise, <see langword="false" /></value>
```

### [04.4]-[METHODS]

A general method, a task-returning method, a factory, a try pattern, an abstract member, `Dispose()`, and `Dispose(bool)`:

```xml
<summary>Draws a rectangle using the specified paint</summary>
<param name="rect">The rectangle to draw</param>
<param name="paint">The paint to use</param>

<summary>Asynchronously encodes the image to the specified format</summary>
<returns>A task object that, when awaited, produces the encoded bytes</returns>

<summary>Creates a new image from encoded data</summary>
<param name="data">The encoded image data</param>
<returns>A new image, or <see langword="null" /> if the data is invalid</returns>

<summary>Attempts to parse the color from a string</summary>
<param name="value">The string to parse</param>
<param name="color">When this method returns, contains the parsed color if the parsing succeeded. This parameter is treated as uninitialized</param>
<returns><see langword="true" /> if the parsing succeeded; otherwise, <see langword="false" /></returns>

<summary>When overridden in a derived class, reads the next byte from the stream</summary>

<summary>Releases the resources used by the current instance of the <see cref="Drawing.Paint" /> class</summary>

<summary>Called by the <see cref="Drawing.Paint.Dispose" /> and <see cref="System.Object.Finalize" /> methods to release the managed and unmanaged resources used by the current instance of the <see cref="Drawing.Paint" /> class</summary>
<param name="disposing"><see langword="true" /> to release managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources</param>
```

### [04.5]-[EVENTS]

An event summary opens with "Occurs when", the `On` method that raises the event reads "Raises the `<see cref>` event", an `EventArgs` class reads "Provides data for the `<see cref>` event":

```xml
<summary>Occurs when the surface needs to be repainted</summary>
<summary>Raises the <see cref="Drawing.Surface.Paint" /> event</summary>
<summary>Provides data for the <see cref="Drawing.Surface.Paint" /> event</summary>
```

### [04.6]-[ENUMS]

The type summary opens with "Specifies" or "Describes", a member summary is a noun phrase or a sentence with no opening verb, a mask member reads "A mask used to retrieve X", a `[Flags]` enum states which members combine and which stand alone:

```xml
<summary>Specifies the blend mode for drawing operations</summary>

<summary>The source pixel in place of the destination pixel</summary>
<summary>A blend of the source and destination pixels</summary>
```

### [04.7]-[PARAMETERS]

- Every parameter or none (`CS1573`), in declaration order (`RCS1232`)
- The description opens with an article and states the unit, the valid range, and the default, `<see langword="null" />` for a nullable parameter
- An enum parameter reads "One of the enumeration values that specifies ...", a `[Flags]` parameter "A bitwise combination of the enumeration values that specifies ..."
- An `out` parameter reads "When this method returns, contains ... This parameter is treated as uninitialized", a `ref` parameter ends with ", passed by reference", an indexer integer reads "The zero-based index of ..."

```xml
<param name="rect">The rectangle to draw</param>
<param name="bufferSize">The size of the buffer, in bytes. This value must be greater than zero. The default size is 4096</param>
<param name="antialias"><see langword="true" /> to enable anti-aliasing; otherwise, <see langword="false" /></param>
<param name="blendMode">One of the enumeration values that specifies the blend mode</param>
<param name="flags">A bitwise combination of the enumeration values that specifies the options</param>
<param name="result">When this method returns, contains the parsed value if the parsing succeeded. This parameter is treated as uninitialized</param>
```

### [04.8]-[RETURN_VALUES]

- A LanguageExt return names the success value and each failure case: `Option<A>` reads "The X, or none when ...", `Fin<A>` reads "The X, or a `<see cref>` error when ...", `Validation<Error, A>` reads "The X, or every error from ...", `Either<L, R>` reads "The L when ..., or the R when ..."
- `Task` and `ValueTask` returns read "A task object that, when awaited, produces ...", an enum "One of the enumeration values that indicates ...", a `[Flags]` enum "A bitwise combination of the enumeration values that ..."

```xml
<returns>A new image</returns>
<returns>A new image, or <see langword="null" /> if the data is invalid</returns>
<returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" /></returns>
<returns>One of the enumeration values that indicates the result</returns>
<returns>The person with the specified name, or none when no person has that name</returns>
<returns>The validated age, or an <see cref="InvalidAge" /> error when the value is negative or at least 120</returns>
<returns>The registered person, or every error from the name and age checks</returns>
```

### [04.9]-[EXCEPTIONS]

- Each exception the member throws takes `<exception cref>`, a failure returned as data belongs in `<returns>`
- The condition reads as if "if" preceded it, in the present tense, with `<paramref>` for the parameter, separate conditions of one exception with "-or-", a condition of an exception stored in the returned task ends with "This exception is stored into the returned task"
- `InvalidOperationException` takes the instance state that rejects the call as the condition ("The path is empty"), a call after `Dispose` takes `ObjectDisposedException`

```xml
<exception cref="System.ArgumentNullException"><paramref name="paint" /> is <see langword="null" /></exception>
<exception cref="System.ArgumentOutOfRangeException"><paramref name="width" /> is less than zero</exception>
<exception cref="System.InvalidOperationException">The path is empty</exception>
<exception cref="System.ObjectDisposedException">The surface is disposed</exception>
```

## [05]-[THREADING]

A thread constraint sits in `<remarks>` and names the thread, lock, or monitor: the thread a member must run on and whether an awaited continuation resumes on it, any `lock()` or `Monitor.TryEnter` requirement the caller satisfies.
