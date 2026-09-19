// --- [IMPORTS] -------------------------------------------------------------------------

import type { BridgeError } from '@rasm/creative-cloud-server/errors';
import type { Bundle } from '@rasm/creative-cloud-server/hosts';
import { collections, enumerations } from '@rasm/creative-cloud-server/indesign';
import { Results } from '@rasm/creative-cloud-server/indesign/jobs';
import { doScript, read } from '@rasm/creative-cloud-server/osascript';
import { absent, sorted } from '@rasm/creative-cloud-server/sdef';
import { deploy, main, POLL, probed, server, until } from '@rasm/creative-cloud-server/uxp';
import { HOSTS, TIMEOUT_CEILING_MS } from '@rasm/creative-cloud-server/values';
import { Array, Console, Effect, flow, Number, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Command } from 'effect/unstable/cli';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import { bridge } from './uxp.config.ts';

// --- [DEPLOY] --------------------------------------------------------------------------

const _shown = (bundle: (typeof Bundle)['Type']): Effect.Effect<{ readonly visible: boolean }, BridgeError | Schema.SchemaError, ChildProcessSpawner.ChildProcessSpawner> =>
    read(
        HOSTS.indesign.id,
        bundle.bundlePath,
        TIMEOUT_CEILING_MS,
        `${doScript(`var panel = app.panels.itemByName(${JSON.stringify(bridge.panel.label.default)}); panel.visible = true; panel.visible`)} language javascript`,
        Option.none(),
    ).pipe(
        Effect.retry({ while: Predicate.some([Predicate.isTagged('hostNotRunning'), Predicate.isTagged('hostUnresponsive')]), schedule: POLL }),
        Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Boolean))),
        Effect.map((visible) => ({ visible })),
    );

const _deploy = deploy(HOSTS.indesign, bridge, import.meta.dirname, Option.some(_shown));

// --- [RECONCILE] -----------------------------------------------------------------------

const _reconcile = Effect.gen(function* () {
    const mcp = yield* server(HOSTS.indesign, bridge.manifest);
    yield* until(mcp.health, probed);
    const { result } = yield* mcp.call('indesign_list_enums', {}, Schema.Struct({ result: Results.fields.listEnums }));
    const registered = Record.map(Record.fromIterableBy(result.enums, Struct.get('name')), (row) => sorted(Array.map(row.constants, Struct.get('name'))));
    const tabled = Record.map(enumerations, flow(Record.keys<string, number>, sorted));
    const unvalued = Array.map(
        Array.filter(
            Array.flatMap(result.enums, (row) => Array.map(row.constants, (constant) => ({ enumeration: row.name, ...constant }))),
            ({ enumeration, name, value }) =>
                !Option.exists(value, (code) => Option.contains(Option.flatMap(Record.get<string, Readonly<Record<string, number>>>(enumerations, enumeration), Record.get(name)), code)),
        ),
        ({ enumeration, name }) => `${enumeration}.${name}`,
    );
    const report = {
        undeclared: { enumerations: Array.difference(Record.keys(registered), Record.keys(tabled)), constants: absent(registered, tabled) },
        unregistered: absent(tabled, registered),
        unvalued,
        generated: Record.size(tabled),
        runtime: result.enums.length,
        constants: Number.sumAll(Array.map(result.enums, (row) => row.constants.length)),
        collections: { generated: Record.size(collections), unconfirmed: Array.difference(Record.values(collections), result.functions) },
    };
    yield* Console.log(JSON.stringify(report, null, 4));
    return yield* report.undeclared.enumerations.length === 0 && Record.isEmptyRecord(report.undeclared.constants) && unvalued.length === 0
        ? Console.log('The generated table covers every independently discovered native enumeration and canonical constant')
        : Effect.fail({ _tag: 'unreconciled' as const, ...Struct.pick(report, ['undeclared', 'unvalued']) });
});

// --- [ENTRY] ---------------------------------------------------------------------------

main(Command.make('automation').pipe(Command.withSubcommands([Command.make('deploy', {}, () => _deploy), Command.make('reconcile', {}, () => Effect.scoped(_reconcile))])), bridge.manifest.version);
