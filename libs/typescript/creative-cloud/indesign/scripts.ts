import { parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { Array, Cause, Effect, flow, Option, Predicate, Schema } from 'effect';
import type { Host } from './host.ts';
import { Classes } from './objects/classes.ts';
import { resolve } from './objects/specifiers.ts';
import { Mutation, transacted } from './objects/transactions.ts';
import { Reports } from './preflight.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [MODELS] --------------------------------------------------------------------------

class Script extends Mutation.extend<Script>('Script')(
    { code: Schema.String.annotate({ description: 'JavaScript function body' }) },
    {
        description: [
            'Runs native UXP JavaScript in InDesign with app, referenced native enumerations and DOM classes, and resolve(specifier) in scope.',
            'A synchronous body shares one undo step, rolled back on a throw. Top-level await makes the body asynchronous: a rejection keeps earlier writes, and writes after the first await fall outside the undo step and the preflight summary. Group later changes in app.doScript(fn, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name).',
            'Return values encode enumerators by name, objects as {type, specifier}, and collections as {type, length}. Inspect class names for native fields and method signatures.',
        ].join('\n'),
    },
) {
    static readonly Result = Mutation.Result({ result: Schema.OptionFromOptionalKey(Schema.Json) });

    override get label(): string {
        return `Run script: ${Option.getOrElse(
            Array.findFirst(this.code.split(LINE_BREAK), (line) => line.trim() !== ''),
            () => '(empty)',
        ).trim()}`;
    }

    run() {
        const { code, label } = this;
        const lines = code.split(LINE_BREAK);
        const failed = (position: Option.Option<number>) => (cause: unknown) => new Cause.UnknownError(cause, Option.getOrUndefined(Option.map(position, (at) => [`line ${at}`, ...Option.toArray(Array.get(lines, at - 1))].join(': '))));
        return Effect.gen(function* () {
            const { exported, encoded } = yield* Classes;
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
            const given = { ...Object.fromEntries(exported.filter(([name]) => used.has(name) && !(name in globalThis))), resolve: flow(resolve, Effect.runSyncWith(yield* Effect.context<Classes | Host>())) };
            const text = `{\n${code}\n}`;
            const compiled = yield* Effect.try({
                // biome-ignore lint/nursery/noImpliedEval: Script compiles caller-supplied source to run in InDesign
                try: () => new Function(...Object.keys(given), awaiting ? `return (async () => ${text})();` : text),
                catch: failed(Option.none()),
            });
            const header = compiled.toString().slice(0, compiled.toString().indexOf(text)).split('\n').length;
            const thrown = (cause: unknown) => {
                const prepare = Error.prepareStackTrace;
                Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
                try {
                    return failed(Option.map(Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber), (at) => at - header))(cause);
                } finally {
                    Error.prepareStackTrace = prepare;
                }
            };
            const { value: settled, touched } = yield* transacted(
                { label },
                Effect.try({
                    try: () => {
                        const value: unknown = Reflect.apply(compiled, undefined, Object.values(given));
                        return Predicate.isPromiseLike(value) ? value : encoded(value, true);
                    },
                    catch: thrown,
                }),
            );
            const json = Predicate.isPromiseLike(settled) ? yield* Effect.flatMap(Effect.tryPromise({ try: () => Promise.resolve(settled), catch: thrown }), (value) => Effect.try({ try: () => encoded(value, true), catch: thrown })) : settled;
            const result = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(json), Schema.decodeEffect(Schema.fromJsonString(Schema.Json)))).pipe(Effect.orDie);
            return { result, preflight: yield* Effect.flatMap(Reports, ({ summarized }) => summarized(touched)) };
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Script };
