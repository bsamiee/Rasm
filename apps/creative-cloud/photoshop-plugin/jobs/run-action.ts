// --- [IMPORTS] -------------------------------------------------------------------------

import { type Action, app, core } from 'adobe:photoshop';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Played, RunAction } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Option } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Request = (typeof RunAction)['Type'];

// --- [ACTIONS] -------------------------------------------------------------------------

const _found = ({ set, action }: Request): Option.Option<Action> =>
    Option.flatMap(
        Array.findFirst(app.actionTree, (candidate) => candidate.name === set),
        (candidate) => Array.findFirst(candidate.actions, (member) => member.name === action),
    );

const _played = (found: Action, name: string): Promise<void> =>
    app.documents.length === 0 ? core.executeAsModal(() => found.play(), { commandName: name }) : app.activeDocument.suspendHistory(() => found.play(), name);

// --- [HANDLER] -------------------------------------------------------------------------

const runAction: Handler = handler(RunAction, Played, (request) =>
    Effect.gen(function* () {
        const found = yield* Effect.fromOption(() => HostRejection.cases.actionNotFound.make(request))(_found(request));
        const played = `${request.set} › ${request.action}`;
        yield* Effect.tryPromise({ try: () => _played(found, played), catch: thrown });
        return { played };
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { runAction };
