import { parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { Array, Cause, Effect, flow, Option, Predicate, Schema } from 'effect';
import { Model } from './model.ts';
import { documents, resolve } from './references.ts';
import { Mutation } from './transactions.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [MODELS] --------------------------------------------------------------------------

class Script extends Mutation.extend<Script>('Script')(
    { code: Schema.String.annotate({ description: 'JavaScript function body' }) },
    {
        description: [
            'Runs native UXP JavaScript in InDesign with app, referenced native enumerations and DOM classes, and resolve(specifier) in scope.',
            'A synchronous body shares one undo step, rolled back on a throw. Top-level await makes the body asynchronous: a rejection keeps earlier writes, and writes after the first await fall outside the undo step and the preflight summary. Group later changes in app.doScript(fn, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name).',
            'Return values encode enumerators by name, objects as {type, specifier, name, valid}, and collections as {type, specifier, length}. Inspect class names for native fields and method signatures.',
        ].join('\n'),
    },
) {
    static readonly Result = Mutation.summarized(Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) }));

    override get label(): string {
        return `Run script: ${Option.getOrElse(
            Array.findFirst(this.code.split(LINE_BREAK), (line) => line.trim() !== ''),
            () => '(empty)',
        ).trim()}`;
    }

    run() {
        const { code } = this;
        const lines = code.split(LINE_BREAK);
        const failed = (position: Option.Option<number>) => (cause: unknown) => new Cause.UnknownError(cause, Option.getOrUndefined(Option.map(position, (at) => [`line ${at}`, ...Option.toArray(Array.get(lines, at - 1))].join(': '))));
        const commit = this.commitSettling.bind(this);
        return Effect.gen(function* () {
            const { exported } = yield* Model;
            const tree = yield* Effect.try({
                try: () => parse(code, { ecmaVersion: 2024, allowReturnOutsideFunction: true, allowAwaitOutsideFunction: true }),
                catch: (cause) => failed(Option.map(Schema.decodeUnknownOption(Schema.Struct({ loc: Schema.Struct({ line: Schema.Int }) }))(cause), ({ loc }) => loc.line))(cause),
            });
            const used = new Set<string>();
            let awaiting = false;
            fullAncestor(tree, (node, _state, ancestors) => {
                if (node.type === 'Identifier') {
                    used.add(node.name);
                }
                awaiting ||= (node.type === 'AwaitExpression' || (node.type === 'ForOfStatement' && node.await)) && !ancestors.some((above) => above.type === 'FunctionDeclaration' || above.type === 'FunctionExpression' || above.type === 'ArrowFunctionExpression');
            });
            const given = { ...Object.fromEntries(exported.filter(([name]) => used.has(name))), resolve: flow(resolve, Effect.runSyncWith(yield* Effect.context<Model>())) };
            const text = `{\n${code}\n}`;
            const header = 3;
            const compiled = yield* Effect.try({
                // biome-ignore lint/nursery/noImpliedEval: Script compiles caller-supplied source to run in InDesign
                try: () => new Function(...Object.keys(given), awaiting ? `return (async () => ${text})();` : text),
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
            return yield* commit(
                Script.Result,
                documents,
                () => Effect.try({ try: (): unknown => Reflect.apply(compiled, undefined, Object.values(given)), catch: thrown }),
                (value) => Effect.map(Effect.tryPromise({ try: () => Promise.resolve(value), catch: thrown }), (result) => ({ result })),
            );
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Script };
