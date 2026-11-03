import { homedir } from 'node:os';
import { Effect, FileSystem, Path } from 'effect';
import { HarnessError } from '#/error';
import type { Rule, Text } from '#/report';
import type { Instance } from '#/session';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOME = homedir();
const REPORTER = 'com.adobe.crashreporter';
const ENTRY = /\n(?=\[)/u;
const ERROR = /^\[[^\]]*\]\[[^\]]*\]\[Error\]/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const errors: Text = (contents) =>
    contents
        .split(ENTRY)
        .filter((entry) => ERROR.test(entry))
        .join('\n');
const uxp = (folder: string): Rule => ({ kind: 'host', pattern: `Library/Logs/Adobe/${folder}/UXPLogs_*.log`, root: HOME, text: errors });
const crashes = Effect.fn('crashes')(function* (instance: Instance) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const markers = `Library/Logs/${REPORTER}/v1/{crashdata,Archive}/*/.session_${instance.pid}.marker`;
    const sessions = yield* Effect.mapError(fs.glob(markers, { root: HOME }), HarnessError.refused(markers));
    return sessions.map((marker): Rule => ({ kind: 'crash', pattern: `${path.dirname(marker)}/crmaccrashlog_*.{fqlog,log}`, root: HOME }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { crashes, HOME, REPORTER, uxp };
