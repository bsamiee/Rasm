// --- [IMPORTS] -------------------------------------------------------------------------

import { Console, Effect, FileSystem, flow, Option, Path, Record, Schema } from 'effect';
import { Argument, Command, Flag } from 'effect/unstable/cli';
import { HOUSE, write } from './acrobat/actions.ts';
import { Bundle, Channel, discover, discoverOn, Hosts, installed } from './hosts.ts';
import { generate as illustrator } from './illustrator/generate.ts';
import { generate as indesign } from './indesign/generate.ts';
import { Jobs, processId } from './jobs.ts';
import manifest from './package.json' with { type: 'json' };
import { layer as bridge } from './socket.ts';
import { InstallError, main } from './uxp.ts';
import { HOSTS, HostId } from './values.ts';

// --- [COMMANDS] ------------------------------------------------------------------------

const _deploy = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const resolved = yield* Hosts;
    const queues = yield* Jobs;
    const acrobat = yield* installed('acrobat', resolved.acrobat);
    yield* Effect.flatMap(processId(queues.acrobat), Option.match({ onNone: () => Effect.void, onSome: (pid) => Effect.fail(InstallError.cases.hostRunning.make({ host: acrobat.id, pid })) }));
    yield* fs.remove(path.join(acrobat.supportFolder, 'DC', 'JavaScripts'), { recursive: true, force: true });
    yield* Effect.forEach(Record.values(HOUSE), (action) => Effect.flatMap(write(acrobat, action), Console.log), { discard: true });
}).pipe(Effect.scoped, Effect.provide(bridge([])));

const _version = Command.make('version', { host: Argument.Literals('host', HostId.literals), channel: Flag.optional(Flag.Literals('channel', Channel.literals)) }, ({ host, channel }) =>
    Effect.flatMap(
        Option.match(channel, { onNone: () => discover(HOSTS[host]), onSome: (wanted) => discoverOn(HOSTS[host], Option.some(wanted)) }),
        flow(Schema.encodeSync(Schema.fromJsonString(Bundle)), Console.log),
    ),
);

// --- [ENTRY] ---------------------------------------------------------------------------

main(
    Command.make('automation').pipe(
        Command.withSubcommands([
            Command.make('deploy', {}, () => _deploy),
            Command.make('generate-illustrator', {}, () => Effect.provide(illustrator(), bridge([]))),
            Command.make('generate-indesign', {}, () => Effect.provide(indesign, bridge([]))),
            _version,
        ]),
    ),
    manifest.version,
);
