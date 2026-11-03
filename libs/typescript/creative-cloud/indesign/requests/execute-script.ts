import { ExecuteScript } from '@rasm/automation/requests';
import { Uxp } from '@rasm/uxp/runtime';
import { Effect, flow, Schema } from 'effect';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { commit } from '#/dom/transactions';

// --- [OPERATIONS] ----------------------------------------------------------------------

const executeScript = Effect.fn('executeScript')(function* (payload: ExecuteScript) {
    const { exported } = yield* Model;
    const given = { ...Object.fromEntries(exported), resolve: flow(Schema.decodeUnknownEffect(Specifier.Path), Effect.flatMap(Model.resolve), Effect.runSyncWith(yield* Effect.context<Model>())) };
    const called = yield* Uxp.evaluate(given, payload.code);
    const result = yield* commit(payload.label, called);
    return yield* Model.read(ExecuteScript.Result, payload.label, () => ({ result }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { executeScript };
