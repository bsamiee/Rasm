import { parse } from 'acorn';
import { full } from 'acorn-walk';
import { Array, Cause, Effect, flow, Option, Predicate, Schema } from 'effect';
import { read } from '#/dom/encoding';
import { resolve } from '#/dom/location';
import { Model } from '#/dom/model';
import { Mutation } from '#/dom/transactions';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [MODELS] --------------------------------------------------------------------------

class ExecuteScript extends Mutation.extend<ExecuteScript>('ExecuteScript')(
    { code: Schema.String.annotate({ description: 'JavaScript function body' }) },
    {
        description: [
            'Scope holds app, referenced enumerations and DOM classes, and resolve(specifier)',
            'Body runs synchronously in one undo step, a throw rolls it back and awaited work runs after the answer. Inspect reads File fields as paths',
            'Results encode enumerators by name, objects as {type, specifier, name, valid}, collections as {type, specifier, length}. kind lists class members',
        ].join('\n'),
    },
) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });

    override get label(): string {
        return `Run ${Option.getOrElse(
            Array.findFirst(this.code.split(LINE_BREAK), (line) => line.trim() !== ''),
            () => 'empty script',
        ).trim()}`;
    }

    run() {
        const { code } = this;
        const lines = code.split(LINE_BREAK);
        const failed = (position: Option.Option<number>) => (cause: unknown) => new Cause.UnknownError(cause, Option.getOrUndefined(Option.map(position, (at) => [`line ${at}`, ...Option.toArray(Array.get(lines, at - 1))].join(': '))));
        const commit = this.commit.bind(this);
        const { label } = this;
        return Effect.gen(function* () {
            const { exported } = yield* Model;
            const tree = yield* Effect.try({
                try: () => parse(code, { ecmaVersion: 2024, allowReturnOutsideFunction: true }),
                catch: (cause) => failed(Option.map(Schema.decodeUnknownOption(Schema.Struct({ loc: Schema.Struct({ line: Schema.Int }) }))(cause), ({ loc }) => loc.line))(cause),
            });
            const used = new Set<string>();
            full(tree, (node) => {
                if (node.type === 'Identifier') {
                    used.add(node.name);
                }
            });
            const given = { ...Object.fromEntries(exported.filter(([name]) => used.has(name))), resolve: flow(resolve, Effect.runSyncWith(yield* Effect.context<Model>())) };
            const text = `{\n${code}\n}`;
            const header = 3;
            const compiled = yield* Effect.try({
                // biome-ignore lint/nursery/noImpliedEval: ExecuteScript compiles caller-supplied source to run in InDesign
                try: () => new Function(...Object.keys(given), text),
                catch: failed(Option.none()),
            });
            const thrown = (cause: unknown) => {
                const prepare = Error.prepareStackTrace;
                Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
                try {
                    return failed(Option.map(Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber), (at) => at - header))(cause);
                } finally {
                    Error.prepareStackTrace = prepare;
                }
            };
            const result = yield* commit(Effect.try({ try: (): unknown => Reflect.apply(compiled, undefined, Object.values(given)), catch: thrown }));
            return yield* read(ExecuteScript.Result, label, () => ({ result }));
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecuteScript };
