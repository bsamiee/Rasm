// --- [IMPORTS] -------------------------------------------------------------------------

import { collections, enumerations } from '@rasm/creative-cloud-server/indesign';
import { Results } from '@rasm/creative-cloud-server/indesign/jobs';
import { doScript, read } from '@rasm/creative-cloud-server/osascript';
import { absent, sorted } from '@rasm/creative-cloud-server/sdef';
import { deploy, main, POLL, probed, protocol, server, until } from '@rasm/creative-cloud-server/uxp';
import { HOSTS, PROBE_MS } from '@rasm/creative-cloud-server/values';
import { Array, Console, Effect, flow, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Command } from 'effect/unstable/cli';
import { bridge } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.indesign;

// --- [DEPLOY] --------------------------------------------------------------------------

const _docked = read(
    _HOST.id,
    _HOST.bundleId,
    PROBE_MS,
    `${doScript(`var label = ${JSON.stringify(bridge.panel.label.default)}; var docked = app.panels.itemByName(label).isValid; if (!docked) app.menuActions.itemByName(label).invoke(); docked`)} language javascript`,
    Option.none(),
).pipe(
    Effect.retry({ while: Predicate.some([Predicate.isTagged('hostNotRunning'), Predicate.isTagged('hostUnresponsive')]), schedule: POLL }),
    Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Boolean))),
    Effect.map((docked) => ({ docked })),
);

const _deploy = deploy(_HOST, bridge, import.meta.dirname, _docked, (execute) =>
    execute('return { TextEncoder: typeof TextEncoder, TextDecoder: typeof TextDecoder, queueMicrotask: typeof queueMicrotask, hrtimeBigint: typeof process.hrtime.bigint };'),
);

// --- [RECONCILE] -----------------------------------------------------------------------

const _reconcile = Effect.gen(function* () {
    const mcp = yield* server(_HOST, bridge.manifest);
    yield* until(mcp.health, probed);
    const listed = yield* mcp.call('indesign_list_enums', {}, Results.fields.listEnums);
    const registered = Record.map(Record.fromIterableBy(listed.enums, Struct.get('name')), (row) => sorted(Array.map(row.constants, Struct.get('name'))));
    const tabled = Record.map(enumerations, flow(Record.keys, sorted));
    const unvalued = Array.map(
        Array.filter(
            Array.flatMap(listed.enums, (row) => Array.map(row.constants, (constant) => ({ enumeration: row.name, ...constant }))),
            ({ enumeration, name, value }) => !Option.exists(value, (code) => Option.contains(Option.flatMap(Record.get(enumerations, enumeration), Record.get(name)), code)),
        ),
        ({ enumeration, name }) => `${enumeration}.${name}`,
    );
    const report = {
        undeclared: { enumerations: Array.difference(Record.keys(registered), Record.keys(tabled)), constants: absent(registered, tabled) },
        unregistered: absent(tabled, registered),
        unvalued,
        generated: Record.size(tabled),
        runtime: listed.enums.length,
        constants: Number.sumAll(Array.map(listed.enums, (row) => row.constants.length)),
        collections: { generated: Record.size(collections), unconfirmed: Array.difference(Record.values(collections), listed.functions) },
    };
    yield* Console.log(JSON.stringify(report, null, 4));
    return yield* report.undeclared.enumerations.length === 0 && Record.isEmptyRecord(report.undeclared.constants) && unvalued.length === 0
        ? Console.log('list_enums equals the generated table over every runtime enumeration, constant, and value')
        : Effect.fail({ _tag: 'unreconciled' as const, ...Struct.pick(report, ['undeclared', 'unvalued']) });
});

// --- [ENTRY] ---------------------------------------------------------------------------

main(
    Command.make('automation').pipe(Command.withSubcommands([Command.make('deploy', {}, () => _deploy), Command.make('reconcile', {}, () => Effect.provide(Effect.scoped(_reconcile), protocol))])),
    bridge.manifest.version,
);
