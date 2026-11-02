import { ExecuteScript, HostError } from '@rasm/automation/requests';
import { Uxp } from '@rasm/uxp/runtime';
import { parse } from 'acorn';
import { full } from 'acorn-walk';
import { Effect, flow, Schema } from 'effect';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { commit } from '#/dom/transactions';
import { read } from '#/dom/value';

// --- [OPERATIONS] ----------------------------------------------------------------------

const executeScript = Effect.fn('executeScript')(function* (payload: ExecuteScript) {
    const { exported } = yield* Model;
    const tree = yield* Effect.try({ try: () => parse(payload.code, { ecmaVersion: 'latest' }), catch: HostError.refused(payload.label) });
    const used = new Set<string>();
    full(tree, (node) => {
        if (node.type === 'Identifier') {
            used.add(node.name);
        }
    });
    const given = { ...Object.fromEntries(exported.filter(([name]) => used.has(name))), resolve: flow(Schema.decodeUnknownEffect(Specifier.Path), Effect.flatMap(Specifier.resolve), Effect.runSyncWith(yield* Effect.context<Model>())) };
    const called = yield* Uxp.evaluate(given, payload.code);
    const result = yield* commit(payload.label, called);
    return yield* read(ExecuteScript.Result, payload.label, () => ({ result }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { executeScript };
