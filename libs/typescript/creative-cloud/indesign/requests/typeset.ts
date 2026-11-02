import type { Typeset } from '@rasm/automation/typesetting';
import { Effect, Option } from 'effect';
import { HostSettings } from '#/documents/settings';
import { commit } from '#/dom/transactions';

// --- [OPERATIONS] ----------------------------------------------------------------------

const typeset = Effect.fn('typeset')(function* (payload: Typeset) {
    const target = yield* HostSettings.target(payload.target);
    return yield* Option.match(payload.settings, {
        onNone: () => HostSettings.read(target),
        onSome: (given) => commit(payload.label, Effect.andThen(HostSettings.write(target, given), HostSettings.read(target))),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { typeset };
