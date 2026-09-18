// --- [IMPORTS] -------------------------------------------------------------------------

import { type BridgeError, exited } from '@rasm/creative-cloud-server/errors';
import { discover } from '@rasm/creative-cloud-server/hosts';
import { doJavascript, read, reply } from '@rasm/creative-cloud-server/osascript';
import { deploy, type Executor, main, POLL } from '@rasm/creative-cloud-server/uxp';
import { HOSTS, PROBE_MS } from '@rasm/creative-cloud-server/values';
import { Array, type Config, type Crypto, Effect, Option, type Path, type PlatformError, Predicate, Schema, String } from 'effect';
import { Command } from 'effect/unstable/cli';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import { bridge } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.photoshop;
const _LOADED = `${bridge.manifest.name} (Loaded)`;

// --- [DEPLOY] --------------------------------------------------------------------------

const _loaded = read(_HOST.id, _HOST.bundleId, PROBE_MS, doJavascript('app.systemInformation'), Option.none()).pipe(
    Effect.retry({ while: Predicate.some([Predicate.isTagged('hostNotRunning'), Predicate.isTagged('hostUnresponsive')]), schedule: POLL }),
    Effect.repeat({ until: String.includes(_LOADED), schedule: POLL }),
    Effect.map((report) => ({ loaded: Array.findFirst(String.linesIterator(report), String.includes(_LOADED)) })),
);

const _shown = (
    execute: Executor,
): Effect.Effect<
    { readonly panel: { readonly views: number; readonly shown: boolean }; readonly scope: Schema.Json },
    BridgeError | PlatformError.PlatformError | Schema.SchemaError | Config.ConfigError,
    ChildProcessSpawner.ChildProcessSpawner | Path.Path | Crypto.Crypto
> =>
    Effect.gen(function* () {
        const { executable } = yield* discover(_HOST);
        const views = yield* Effect.flatMap(execute('return document.querySelectorAll("p").length;'), Schema.decodeUnknownEffect(Schema.Int));
        const clicked = yield* Effect.when(
            Effect.mapError(
                reply(
                    ChildProcess.make('osascript', [
                        '-e',
                        `tell application "System Events" to tell process "${executable}" to click menu item "${bridge.panel.label.default}" of menu 1 of menu item "${bridge.manifest.name}" of menu "Plugins" of menu bar 1`,
                    ]),
                ),
                exited(_HOST.id),
            ),
            Effect.succeed(views === 0),
        );
        const scope = yield* execute(
            [
                'return {',
                '    TextEncoder: typeof TextEncoder,',
                '    TextDecoder: typeof TextDecoder,',
                '    hrtime: typeof process.hrtime.bigint === "function" ? typeof process.hrtime.bigint() : "undefined",',
                '    queueMicrotask: typeof queueMicrotask,',
                '    status: document.querySelector("p").textContent,',
                '    plugin: await require("photoshop").core.getPluginInfo(),',
                '};',
            ].join('\n'),
        );
        return { panel: { views, shown: Option.isSome(clicked) }, scope };
    });

// --- [ENTRY] ---------------------------------------------------------------------------

main(Command.make('automation').pipe(Command.withSubcommands([Command.make('deploy', {}, () => deploy(_HOST, bridge, import.meta.dirname, _loaded, _shown))])), bridge.manifest.version);
