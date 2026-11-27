import { homedir } from 'node:os';
import dialogs from '@rasm/acrobat/host?inline';
import { HostScript as Acrobat } from '@rasm/acrobat/script';
import type { Target } from '@rasm/application-tools/handle/target';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Instance } from '@rasm/applications/server/instance';
import { Application } from '@rasm/creative-cloud-desktop/application';
import { Completion } from '@rasm/extendscript/completion';
import host from '@rasm/extendscript/host?inline';
import { lower, originalLine } from '@rasm/extendscript/node';
import { HostScript as Illustrator } from '@rasm/illustrator/script';
import { HostScript as InDesign } from '@rasm/indesign/script';
import { Link } from '@rasm/mcp/link';
import { Line, Report } from '@rasm/mcp/report';
import { ToolCall } from '@rasm/mcp/toolkit';
import { HostError } from '@rasm/object-model/model';
import { Evaluation, EvaluationRpcs } from '@rasm/object-model/rpcs';
import { Unmatched } from '@rasm/search/unmatched';
import { HOSTS, PluginsInfo, UxpManifest } from '@rasm/uxp/manifest';
import { Array, Cache, Data, Duration, Effect, Exit, FileSystem, Function, Match, Option, Order, Path, Predicate, pipe, Record, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import type { RpcGroup } from 'effect/rpc';
import shim from 'es5-shim?inline';
import sham from 'es5-shim/es5-sham.js?inline';
import { Applications } from '#/applications';
import { Declarations } from '#/declarations';

// --- [TYPES] ---------------------------------------------------------------------------

type Route = Data.TaggedEnum<{
    Linked: object;
    Unlinked: object;
    Uxp: { readonly parameter: string; readonly script: typeof InDesign };
    Acrobat: object;
    Script: { readonly around: string };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const UXP: Record.ReadonlyRecord<string, typeof InDesign> = { [HOSTS.ID.product]: InDesign };
const EXTENDSCRIPT: Record.ReadonlyRecord<string, typeof Illustrator> = { [HOSTS.AI.product]: Illustrator };
const AROUND = String(<A>(_: unknown, body: () => A): A => body());

// --- [MODELS] --------------------------------------------------------------------------

const Route = Data.taggedEnum<Route>();
const Reply = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Completion)));
const ResultJson = Schema.toCodecJson(Evaluation.Result);

// --- [OPERATIONS] ----------------------------------------------------------------------

const bodyLine = (line: number) => Option.liftPredicate(line - 1, (at) => at > 0);
const reline = (line: (at: number) => Option.Option<number>) => (error: HostError) => HostError.fields.reason.matchOrElse(error.reason, { Thrown: ({ line: at }) => HostError.thrown(error.subject, Option.flatMap(at, line))(error.cause) }, () => error);

// --- [COMPOSITION] ---------------------------------------------------------------------

const scripts = Effect.fnUntraced(function* (links: Pick<Effect.Success<ReturnType<typeof Link.listen<RpcGroup.Rpcs<typeof EvaluationRpcs>>>>, 'peers' | 'changes' | 'call'>, identifier: string) {
    const [server, applications, declarations, files, path] = yield* Effect.all([ApplicationServer, Applications, Declarations, FileSystem.FileSystem, Path.Path]);
    const dictionaries = yield* Cache.makeWith(({ url }: Application) => server.dictionary({ url }), { capacity: Number.POSITIVE_INFINITY, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) });
    const readPlugin = Schema.decodeUnknownOption(Schema.Struct({ pluginId: Schema.TemplateLiteral([`${identifier}.`, Schema.String]), status: Schema.Literal('enabled'), hostMinVersion: Schema.String }));
    const route = Effect.fnUntraced(function* (application: Application) {
        const app = Array.findFirst(Struct.keys(HOSTS), (key) => HOSTS[key].pluginsInfo && HOSTS[key].product === application.product);
        const registries = Array.cartesianWith(Option.toArray(app), ['/', homedir()], (key, root) => path.join(root, 'Library', 'Application Support', 'Adobe', ...PluginsInfo.file(key)));
        const listed = yield* Effect.forEach(registries, (file) => files.readFileString(file).pipe(Effect.flatMap(Schema.decodeEffect(PluginsInfo)), Effect.map(Struct.get('plugins')), Effect.catchReason('PlatformError', 'NotFound', Function.constant(Effect.succeed([])))), { concurrency: 'unbounded' });
        const plugged = listed
            .flat()
            .flatMap((row) => Option.toArray(readPlugin(row)))
            .some(({ hostMinVersion }) => Order.isGreaterThanOrEqualTo(Application.VersionOrder)(application.version, hostMinVersion));
        const uxp = Record.get(UXP, application.product);
        const stated = yield* Effect.when(Cache.get(dictionaries, application), Effect.succeed(Option.isSome(uxp) || !plugged));
        const command = Option.flatMap(stated, ({ script }) => script);
        const parameters = Option.toArray(command).flatMap(({ parameters: held }) => held);
        const commands = Option.toArray(stated).flatMap(({ command: held }) => held.map(Struct.get('name')));
        const around = Option.match(Record.get(EXTENDSCRIPT, application.product), { onNone: () => AROUND, onSome: (script) => script.around });
        const scripted = Option.flatMap(stated, ({ suite }) => Option.as(command, Array.contains(suite.map(Struct.get('code')), Acrobat.suite) ? Route.Acrobat() : Route.Script({ around })));
        const found = pipe(
            Option.match(uxp, {
                onNone: Option.none,
                onSome: (script) =>
                    Option.map(
                        Array.findFirst(parameters, ({ enumerators }) => Array.contains(enumerators.map(Struct.get('code')), script.language)),
                        ({ name }) => Route.Uxp({ parameter: name, script }),
                    ),
            }),
            Option.orElse(() => (plugged ? Option.some(Route.Unlinked()) : Option.none())),
            Option.orElse(() => scripted),
        );
        return yield* Effect.fromOption(found, () => Unmatched.of({ subject: `linked plugin or script command in the dictionary of ${application.name}`, candidates: commands }));
    });
    return {
        run: Effect.fnUntraced(function* ({ row, live }: Effect.Success<typeof Target.current>, code: string) {
            const [application, peers, { returnEarly }] = yield* Effect.all([applications.find(row), links.peers, ToolCall]);
            const serving = Predicate.and(Link.runsIn(application), Link.serves(EvaluationRpcs));
            const found = yield* peers.some(serving) ? Effect.succeed(Route.Linked()) : route(application);
            const { program, engine, offset } = Match.value(found).pipe(
                Match.tag('Uxp', ({ script }) => ({ program: Evaluation.expression(code, false), engine: { targets: UxpManifest.engine }, offset: script.offset })),
                Match.tag('Acrobat', () => ({ program: Acrobat.program(Evaluation.expression(code, false)), engine: { targets: Acrobat.engine }, offset: Acrobat.offset })),
                Match.tag('Script', ({ around }) => ({ program: `var body = ${Evaluation.expression(code, false)};\nvar around = ${around};`, engine: lower.extendscript, offset: 1 })),
                Match.orElse(() => ({ program: Evaluation.expression(code, true), engine: { targets: UxpManifest.engine }, offset: 0 })),
            );
            yield* declarations.check(application, program, bodyLine);
            const { code: lowered, map } = yield* Effect.catchTag(lower(program, engine, 'typescript'), 'HostError', reline(bodyLine));
            const original = yield* originalLine(map);
            const label = Evaluation.formatLabel(code);
            const execute = Effect.fnUntraced(function* (given: Record.ReadonlyRecord<string, Schema.Json>) {
                const { reply, written } = yield* server.execute({ instance: live.instance, arguments: given });
                const completion = yield* Effect.mapError(Schema.decodeUnknownEffect(Reply)(reply), HostError.refused(`${application.name} script reply`));
                return { ...(yield* Completion.decode(ResultJson, label)(completion)), written };
            });
            const linked = Effect.flatMap(links.call(serving, `linked plugin of ${application.name}`), (client) => client('evaluate', { code: lowered, label }));
            const exited = SubscriptionRef.changes(server.running).pipe(
                Stream.filter((rows) => !Record.has(Instance.running(rows), live.instance)),
                Stream.runHead,
                Effect.andThen(HostError.of({ _tag: 'Unavailable' }, `linked Atelier plugin running UXP scripts in ${application.name}`)()),
            );
            const sent: Effect.Effect<Effect.Success<typeof linked | ReturnType<typeof execute>>, HostError | Error> = Route.$match(found, {
                Linked: () => linked,
                Unlinked: () =>
                    new Report({ lines: [new Line({ head: [application.name, 'plugin'], value: 'unlinked', facts: ['script pending'] })] }).pipe(
                        Report.text,
                        Effect.succeed,
                        Stream.succeed,
                        returnEarly,
                        Effect.andThen(links.changes.pipe(Stream.filter(Array.some(serving)), Stream.runHead)),
                        Effect.raceFirst(exited),
                        Effect.andThen(linked),
                    ),
                Uxp: ({ parameter, script }) => execute({ direct: script.wrap(lowered, host, label), [parameter]: { _tag: 'enumerator', name: script.language } }),
                Acrobat: () => execute({ direct: Acrobat.wrap(lowered, host, dialogs) }),
                Script: () =>
                    execute({
                        direct: `(function (self) {\n${lowered}\nif (typeof Object.keys !== 'function') {\n${shim}\n${sham}\n}\n${host}\nreturn encodeURIComponent(host.reply(function () { return around(self, function () { return { result: body.call(self) }; }, host); }));\n})(this)`,
                    }),
            });
            return yield* Effect.catchTag(
                sent,
                'HostError',
                reline((line) => Option.flatMap(original(line - offset), bodyLine)),
            );
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { scripts };
