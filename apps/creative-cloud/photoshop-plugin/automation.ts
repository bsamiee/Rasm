// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { bundle, info } from '@rasm/creative-cloud-server/hosts';
import { doJavascript, read, reply } from '@rasm/creative-cloud-server/osascript';
import { attached, declared, install, POLL, probed, protocol, relaunch, server, until } from '@rasm/creative-cloud-server/uxp';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import { Array, Cause, Clock, Console, Effect, flow, Option, Predicate, Record, Schema, String } from 'effect';
import { Command } from 'effect/unstable/cli';
import { ChildProcess } from 'effect/unstable/process';
import { bridge } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.photoshop;
const _REPORT_MS = 5000;
const _LOADED = `${bridge.manifest.name} (Loaded)`;
const _VIEWS = 'return document.querySelectorAll("p").length;';
const _SCOPE = [
    'return {',
    '    TextEncoder: typeof TextEncoder,',
    '    TextDecoder: typeof TextDecoder,',
    '    hrtime: typeof process.hrtime.bigint === "function" ? typeof process.hrtime.bigint() : "undefined",',
    '    queueMicrotask: typeof queueMicrotask,',
    '    status: document.querySelector("p").textContent,',
    '    plugin: await require("photoshop").core.getPluginInfo(),',
    '};',
].join('\n');
const _TYPINGS = { uxp: '@adobe-uxp-types/uxp', photoshop: '@adobe-uxp-types/photoshop' } as const;

// --- [ERRORS] --------------------------------------------------------------------------

const NotLoaded = Schema.TaggedStruct('notLoaded', { report: Schema.String });

// --- [GENERATE] ------------------------------------------------------------------------

const _generate = Effect.flatMap(
    Effect.all(Record.map(_TYPINGS, (name, module) => declared(new URL(import.meta.resolve(`${name}/package.json`)), module, import.meta.dirname))),
    flow(JSON.stringify, Console.log),
);

// --- [DEPLOY] --------------------------------------------------------------------------

const _show = (executable: string): ChildProcess.StandardCommand =>
    ChildProcess.make('osascript', [
        '-e',
        `tell application "System Events" to tell process "${executable}" to click menu item "${bridge.panel.label.default}" of menu 1 of menu item "${bridge.manifest.name}" of menu "Plugins" of menu bar 1`,
    ]);

const _deploy = Effect.gen(function* () {
    const { executable } = yield* info(yield* bundle(_HOST));
    const placed = yield* install(bridge.manifest, import.meta.dirname);
    const mcp = yield* server(_HOST, bridge.manifest);
    const launchedAt = yield* relaunch(_HOST, 'quit');
    const report = yield* read(_HOST.id, _HOST.bundleId, _REPORT_MS, doJavascript('app.systemInformation'), Option.none()).pipe(
        Effect.filterOrFail(String.includes(_LOADED), (text) => NotLoaded.make({ report: text })),
        Effect.retry({ while: Predicate.or(Predicate.or(Predicate.isTagged('hostNotRunning'), Predicate.isTagged('hostUnresponsive')), Predicate.isTagged('notLoaded')), schedule: POLL }),
    );
    const loadedAt = yield* Clock.currentTimeMillis;
    const link = yield* until(mcp.health, attached);
    const attachedAt = yield* Clock.currentTimeMillis;
    const probe = yield* until(mcp.health, probed);
    const probedAt = yield* Clock.currentTimeMillis;
    const views = yield* Effect.flatMap(mcp.execute(_VIEWS), Schema.decodeUnknownEffect(Schema.Int));
    yield* Effect.when(Effect.orDie(reply(_show(executable))), Effect.succeed(views === 0));
    const scope = yield* mcp.execute(_SCOPE);
    yield* Console.log(
        JSON.stringify(
            {
                ...placed,
                loaded: Array.findFirst(String.linesIterator(report), String.includes(_LOADED)),
                loadedAfterLaunchMs: loadedAt - launchedAt,
                attachedAfterLaunchMs: attachedAt - launchedAt,
                probedAfterAttachMs: probedAt - attachedAt,
                link: link.link,
                probe: probe.probe,
                panel: { views, shown: views === 0 },
                scope,
            },
            null,
            4,
        ),
    );
});

// --- [ENTRY] ---------------------------------------------------------------------------

Command.run(
    Command.make('automation').pipe(Command.withSubcommands([Command.make('generate', {}, () => _generate), Command.make('deploy', {}, () => Effect.provide(Effect.scoped(_deploy), protocol))])),
    { version: bridge.manifest.version },
).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
