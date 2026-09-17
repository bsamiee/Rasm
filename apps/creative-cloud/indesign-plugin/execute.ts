// --- [IMPORTS] -------------------------------------------------------------------------

import { app, ScriptLanguage, UndoModes } from 'adobe:indesign';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Execute } from '@rasm/creative-cloud-server/frames';
import { Effect, Option } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _AsyncFunction: new (code: string) => () => Promise<unknown> = Object.getPrototypeOf(async () => undefined).constructor;

// --- [REJECTIONS] ----------------------------------------------------------------------

const _thrown = (cause: unknown): HostRejection =>
    HostRejection.cases.scriptThrew.make(
        cause instanceof Error
            ? { name: cause.name, message: cause.message, stack: Option.fromNullishOr(cause.stack), line: Option.none(), fileName: Option.none(), number: Option.none() }
            : { name: 'Error', message: String(cause), stack: Option.none(), line: Option.none(), fileName: Option.none(), number: Option.none() },
    );

// --- [RUN] -----------------------------------------------------------------------------

const execute = ({ code, undoName }: Execute): Effect.Effect<unknown, HostRejection> =>
    Option.match(undoName, {
        onNone: () => Effect.tryPromise({ try: () => new _AsyncFunction(code)(), catch: _thrown }),
        onSome: (name) => Effect.try({ try: () => app.doScript(`(function () {\n${code}\n})()`, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name), catch: _thrown }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
