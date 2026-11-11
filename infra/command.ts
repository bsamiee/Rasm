import { Completed } from '@rasm/process';
import { Effect, Schema } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = (command: string, args: readonly string[], env: Readonly<Record<string, string>> = {}) => Completed.output(ChildProcess.make(command, args, { env, extendEnv: true, stdin: 'ignore' }));
const deployed = Effect.flatMap(run('doppler', ['secrets', 'download', '--no-file', '--format', 'json', '--config', 'deploy']), Schema.decodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.String))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { deployed, run };
