---
description: Standard for source-level public symbol documentation
---

# Code documentation standards

Code documentation is symbol-level reference. It explains intent, constraints,
effect channels, and public contracts that names and types cannot express.

## Use when

Use this standard for:

- public visible types and members;
- public functions and methods;
- public error, fault, or result variants;
- public effect channels;
- non-obvious invariants;
- lifecycle, resource, or concurrency obligations that affect callers;
- examples where API misuse is likely.

Do not document obvious accessors, private implementation details, or prose that
repeats the type signature.

## External basis

Use the language documentation format that the toolchain understands:

- C#: XML documentation comments own tag names, compiler validation, `cref`
  resolution, and generated XML output.
- TypeScript: TSDoc owns exported API comment syntax, tags, links, and
  parser-compatible Markdown.
- Python: PEP 257 owns docstring placement; Google-style sections apply only
  when the project uses that dialect.

This standard decides what semantic information source-level comments must
carry.

## C# XML comments

Use these tags when relevant:

- `<summary>` for purpose.
- `<typeparam>` for generic type-parameter meaning or constraints.
- `<param>` for parameter meaning, origin, units, or caller obligations.
- `<returns>` for values, effects, or typed failure channels.
- `<value>` for property meaning when not obvious.
- `<remarks>` for invariants, examples, lifecycle, or domain constraints.
- `<exception>` only for exceptions actually thrown.
- `<see>` and `<seealso>` with resolvable `cref` for code references.
- `<inheritdoc>` only when inherited documentation is still correct.

Use `<paramref>` for parameter references, `<c>` for inline code, and `<code>`
for multiline examples. Use `<include>` only when the external XML file is
maintained with the source member.

## TypeScript comments

Use TSDoc comments for exported APIs. Use these tags when relevant:

- summary section for brief API purpose;
- `@remarks` for longer invariants, lifecycle, examples, or constraints;
- `@typeParam` for generic type-parameter meaning or constraints;
- `@param` for parameter meaning, origin, units, or caller obligations;
- `@returns` for values, effects, or typed failure channels;
- `@throws` only for exceptions actually thrown;
- `{@link ...}` and `@see` for references that resolve;
- `@inheritDoc` only when inherited documentation is still correct.

Do not use JSDoc type-expression syntax to duplicate TypeScript types. The
signature owns types; TSDoc owns semantics not visible from the signature.

## Python docstrings

Use PEP 257 docstrings for public modules, packages, classes, functions, and
methods. Use triple double-quoted strings. Start with a concise summary line;
add more detail only when the summary cannot carry the public contract.

Use Google-style sections when a public object needs structured details:

- `Args:`
- `Returns:`
- `Yields:`
- `Raises:`
- `Attributes:`

Do not repeat Python type annotations unless the public contract needs
information the annotation cannot express, such as units, ranges, shape
constraints, or protocol obligations.

## Effect and failure channels

Effect-returning and result-returning APIs must document caller-visible
behavior without flattening the carrier:

- success value and observable side effect, when present;
- failure channel, fault variant, or accumulated validation meaning;
- cancellation, retry, resource, clock, IO, or runtime-context requirements;
- where native exceptions are converted into typed failures;
- terminal boundaries where effects are executed or collapsed.

Document thrown exceptions only when the API actually throws.

## Cross-references and inline comments

Cross-references must resolve through the relevant documentation tool or source
compiler:

- C# code references use `cref`.
- TypeScript code references use TSDoc `{@link ...}` or `@see`.
- Python references use the configured documentation generator syntax only when
  generated docs resolve it.

Inline comments explain why a non-obvious choice exists. They do not narrate
the next line, preserve commented-out code, duplicate names, or carry release
history.

## Boundaries

- API docs own generated or contract-backed API reference.
- Code documentation owns source-level public symbol comments.
- Style guide owns prose mechanics inside comments.
- Reference docs own curated lookup facts outside source.

## Review checklist

- [ ] Public contracts have useful documentation.
- [ ] Comments add intent or constraints not visible from types.
- [ ] Effect and failure channels are accurate.
- [ ] Exceptions are documented only when thrown.
- [ ] Language comments follow the format the toolchain understands.
- [ ] Cross-references resolve to real symbols or docs.
