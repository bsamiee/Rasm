# [REWRITING]

Source supplied at run time is parsed once, changed by splicing its original text at node positions, and compiled through the `Function` constructor with its scope as named parameters. A splice keeps every character it does not replace, with comments, spacing, and each line number a throw reports.

## [01]-[SPLICES]

One pass routes each computed member read through a function its caller supplies, and every other construct compiles as written:

```ts
import { type AnyNode, parse, type Token } from 'acorn';
import { ancestor } from 'acorn-walk';
import MagicString from 'magic-string';

class SourceError extends Schema.TaggedError<SourceError, { readonly brand: unique symbol }>()('SourceError', {
    line: Schema.OptionFromOptionalKey(Schema.Int),
    cause: Schema.Defect(),
}) {}

const Located = Schema.Struct({ loc: Schema.Struct({ line: Schema.Int }) });
const compiled = (
    code: string,
    scope: Readonly<Record<string, unknown>>,
    read: (target: unknown, key: PropertyKey) => unknown,
): Result.Result<{ readonly run: () => unknown; readonly header: number }, SourceError> =>
    Result.try({
        try: () => {
            const [text, tokens] = [new MagicString(code), Array.empty<Token>()];
            const needsReference = (node: AnyNode, parent: AnyNode | undefined, grandparent: AnyNode | undefined): boolean =>
                (parent?.type === 'AssignmentExpression' && parent.left === node) ||
                (parent?.type === 'UpdateExpression' && parent.argument === node) ||
                (parent?.type === 'UnaryExpression' && parent.operator === 'delete') ||
                ((parent?.type === 'CallExpression' || parent?.type === 'NewExpression') && parent.callee === node) ||
                (parent?.type === 'TaggedTemplateExpression' && parent.tag === node) ||
                ((parent?.type === 'ForInStatement' || parent?.type === 'ForOfStatement') && parent.left === node) ||
                (parent?.type === 'AssignmentPattern' && parent.left === node) ||
                parent?.type === 'ArrayPattern' ||
                parent?.type === 'RestElement' ||
                grandparent?.type === 'ObjectPattern';
            ancestor(parse(code, { ecmaVersion: 2024, allowReturnOutsideFunction: true, onToken: tokens }), {
                MemberExpression: (node, _state, ancestors) => {
                    const [, parent, grandparent] = Array.reverse(ancestors);
                    const open = Array.findFirst(tokens, (token) => token.start >= node.object.end && token.type.label === '[');
                    const routed = node.computed && !node.optional && node.object.type !== 'Super' && !needsReference(node, parent, grandparent);
                    if (routed && Option.isSome(open)) {
                        text.prependRight(node.start, '__read(')
                            .overwrite(open.value.start, open.value.end, ',')
                            .overwrite(node.end - 1, node.end, ')');
                    }
                },
            });
            const [bound, body] = [{ __read: read, ...scope }, text.toString()];
            // biome-ignore lint/nursery/noImpliedEval: <host> compiles caller-supplied source
            const run = new Function(...Object.keys(bound), body);
            const source = run.toString();
            return { run: (): unknown => Reflect.apply(run, undefined, Object.values(bound)), header: source.slice(0, source.indexOf(body)).split('\n').length - 1 };
        },
        catch: (cause) => SourceError.make({ line: Option.map(Schema.decodeUnknownOption(Located)(cause), ({ loc }) => loc.line), cause }),
    });
```

Each splice replaces one bracket token or inserts text. `needsReference` keeps assignment targets, call and `new` callees, and binding patterns as written, since `__read` returns a value. `parse` under `ecmaVersion: 2024` rejects syntax V8 in UXP lacks, with its line, before compilation. `new Function` stands under one rule-specific `biome-ignore`, `Reflect.apply` calls the untyped `Function` it returns, and `header` is the line count that constructor writes ahead of its body.

- Scope entries are the names the source references, passed as values, and no source text is built from a value
- Bodies with a top-level `await` compile as the return of an async arrow function around the body and run outside a synchronous unit of work
- Supplied TypeScript passes through `stripTypeScriptTypes(<code>, { mode: 'strip' })` of `node:module`, which keeps every position, before reaching the host

## [02]-[THROWN_LINES]

Throws from compiled source report their line inside the frame V8 marks as evaluated code, readable only while a stack is prepared:

```ts
const line = (cause: unknown, header: number): Option.Option<number> => {
    const prepare = Error.prepareStackTrace;
    Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
    try {
        return Option.map(Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber), (at) => at - header);
    } finally {
        Error.prepareStackTrace = prepare;
    }
};
```

`line` returns the frame line less `header`. V8 prepares a stack at its first `stack` read, and `line` runs inside the `catch` that first sees a throw, before a host primitive rethrows it as a new `Error`.
