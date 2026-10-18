import { parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { Array, Effect, flow, Option, Predicate, Schema } from 'effect';
import { encoded, exported, type Model, resolve, transacted } from './document/objects.ts';
import { InDesignError, type Operations } from './models.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const execute = Effect.fn('execute')(function* ({ run: { code }, description }: typeof Operations.execute.payload.Type) {
    const lines = code.split(LINE_BREAK);
    const failed = (position: Option.Option<number>) => InDesignError.refused(Option.map(position, (at) => [`line ${at}`, ...Option.toArray(Array.get(lines, at - 1))].join(': ')));
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
    const given = { ...Object.fromEntries(exported.filter(([name]) => used.has(name) && !(name in globalThis))), resolve: flow(resolve, Effect.runSyncWith(yield* Effect.context<Model>())) };
    const text = `{\n${code}\n}`;
    const compiled = yield* Effect.try({
        // biome-ignore lint/nursery/noImpliedEval: Script compiles caller-supplied source to run in InDesign
        try: () => new Function(...Object.keys(given), awaiting ? `return (async () => ${text})();` : text),
        catch: failed(Option.none()),
    });
    const header = compiled.toString().slice(0, compiled.toString().indexOf(text)).split('\n').length;
    const thrown = (cause: unknown): InDesignError => {
        const prepare = Error.prepareStackTrace;
        Error.prepareStackTrace = (_error, frames) => frames.find((frame) => frame.isEval())?.getLineNumber();
        try {
            return failed(Option.map(Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber), (at) => at - header))(cause);
        } finally {
            Error.prepareStackTrace = prepare;
        }
    };
    const settled = yield* transacted(
        description,
        Effect.try({
            try: () => {
                const value: unknown = Reflect.apply(compiled, undefined, Object.values(given));
                return Predicate.isPromiseLike(value) ? value : encoded(value, true);
            },
            catch: thrown,
        }),
    );
    const json = Predicate.isPromiseLike(settled) ? yield* Effect.flatMap(Effect.tryPromise({ try: () => Promise.resolve(settled), catch: thrown }), (value) => Effect.try({ try: () => encoded(value, true), catch: thrown })) : settled;
    return { result: yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(json), Schema.decodeEffect(Schema.fromJsonString(Schema.Json)))).pipe(Effect.orDie) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
