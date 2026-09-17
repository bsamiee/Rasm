// --- [IMPORTS] -------------------------------------------------------------------------

import { type Action, app, core } from 'adobe:photoshop';
import { active, type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Played, RunAction } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Option } from 'effect';

// --- [ACTIONS] -------------------------------------------------------------------------

const _found = ({ set, action }: (typeof RunAction)['Type']): Option.Option<Action> =>
    Option.flatMap(
        Array.findFirst(app.actionTree, (candidate) => candidate.name === set),
        (candidate) => Array.findFirst(candidate.actions, (member) => member.name === action),
    );

const _played = (found: Action, name: string): Promise<void> =>
    Option.match(active(app), { onNone: () => core.executeAsModal(() => found.play(), { commandName: name }), onSome: (open) => open.suspendHistory(() => found.play(), name) });

// --- [HANDLER] -------------------------------------------------------------------------

const runAction: Handler = handler(RunAction, Played, (request) =>
    Effect.gen(function* () {
        const found = yield* Effect.fromOption(_found(request), () => HostRejection.cases.actionNotFound.make(request));
        const played = `${request.set} › ${request.action}`;
        yield* Effect.tryPromise({ try: () => _played(found, played), catch: thrown });
        return { played };
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { runAction };
