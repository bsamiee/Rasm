import type { Typeset } from '@rasm/automation/typesetting';
import { Effect, Option } from 'effect';
import { HostSettings } from '#/documents/settings';
import { commit } from '#/dom/transactions';

// --- [OPERATIONS] ----------------------------------------------------------------------

const typeset = Effect.fn('typeset')(function* (payload: Typeset) {
    const target = yield* HostSettings.target(payload.target);
    yield* Effect.transposeOption(Option.map(payload.settings, (given) => commit(payload.label, HostSettings.write(target, given))));
    return yield* HostSettings.read(target);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typeset };
