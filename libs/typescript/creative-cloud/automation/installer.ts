import process from 'node:process';
import { Automation } from '@rasm/applications/node';
import { Array, Config, Effect, FileSystem, Option, Path, Predicate, Record, Schema, Stream, String } from 'effect';
import { ChildProcess } from 'effect/process';
import { HostError } from '#/host-error';

// --- [TYPES] ---------------------------------------------------------------------------

type Host = keyof (typeof PLATFORMS)['darwin']['hosts'];

interface Plugin {
    readonly id: string;
    readonly name: string;
    readonly host: { readonly app: Host; readonly minVersion: string };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const AGENT = 'Adobe/Adobe Desktop Common/RemoteComponents/UPI/UnifiedPluginInstallerAgent';
const NO_HOST_RELEASE = /(?<![\w-])-411(?!\w)/u;

// --- [MODELS] --------------------------------------------------------------------------

const Registry = Schema.fromJsonString(Schema.Struct({ plugins: Schema.Array(Schema.Struct({ pluginId: Schema.String, name: Schema.String, versionString: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const output = Effect.fnUntraced(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit' });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return { text, code };
}, Effect.scoped);
const PLATFORMS = {
    darwin: {
        agent: `/Library/Application Support/${AGENT}/UnifiedPluginInstallerAgent.app/Contents/MacOS/UnifiedPluginInstallerAgent`,
        flag: '--',
        home: 'HOME',
        support: 'Library/Application Support/Adobe',
        hosts: { ID: ['com.adobe.InDesign'], PS: ['com.adobe.Photoshop'], premierepro: ['com.adobe.PremierePro', 'com.adobe.PremiereProBeta.27'], ame: ['com.adobe.ame.application', 'com.adobe.amebeta.application.27'], aftereffects: ['com.adobe.AfterEffects', 'com.adobe.AfterEffectsBeta.application'] },
        running: (identifiers: readonly string[], server: ChildProcess.Command) =>
            Effect.gen(function* () {
                const { instances } = yield* Automation;
                const rows = yield* Effect.fromOption(yield* Stream.runHead(instances({ identifiers })));
                return rows.filter(({ state }) => !Predicate.hasProperty(state, 'exited')).map(({ pid }) => `${pid}`);
            }).pipe(Effect.provide(Automation.layer(server))),
    },
    win32: {
        agent: `C:/Program Files/Common Files/${AGENT}/UnifiedPluginInstallerAgent.exe`,
        flag: '/',
        home: 'APPDATA',
        support: 'Adobe',
        hosts: { ID: ['InDesign.exe'], PS: ['Photoshop.exe'], premierepro: ['Adobe Premiere Pro.exe'], ame: ['Adobe Media Encoder.exe'], aftereffects: ['AfterFX.exe'] },
        running: (names: readonly string[]) =>
            Effect.map(
                Effect.forEach(names, (name) => output('tasklist', ['/fi', `imagename eq ${name}`, '/fo', 'csv', '/nh'])),
                Array.flatMap(({ text }) => Array.fromIterable(String.linesIterator(text)).filter(String.startsWith('"'))),
            ),
    },
} as const;
const platform = Effect.map(Schema.decodeUnknownEffect(Schema.Literals(Record.keys(PLATFORMS)))(process.platform), (held) => PLATFORMS[held]);
const adobe = Effect.gen(function* () {
    const [{ home, support }, path] = yield* Effect.all([platform, Path.Path]);
    return path.join(yield* Config.String(home), support);
});
const extension = (id: string) => Effect.map(Effect.all([Path.Path, adobe]), ([path, folder]) => path.join(folder, 'CEP', 'extensions', id));
const hosts = (app: Host) => Effect.map(platform, ({ hosts: names }): readonly string[] => names[app]);
const running = (app: Host, server: ChildProcess.Command) => Effect.flatMap(platform, ({ hosts: names, running: read }) => read(names[app], server));
const registered = Effect.fnUntraced(function* ({ id, host: { app } }: Pick<Plugin, 'id' | 'host'>) {
    const [fs, path, folder] = yield* Effect.all([FileSystem.FileSystem, Path.Path, adobe]);
    const file = path.join(folder, 'UXP', 'PluginsInfo', 'v1', `${app}.json`);
    const { plugins } = (yield* fs.exists(file)) ? yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Registry)) : { plugins: [] };
    return Array.findFirst(plugins, ({ pluginId }) => pluginId === id);
});
const replaced = Effect.fnUntraced(function* (plugin: Plugin, archive: string) {
    const [fs, { agent, flag }, held, superseded] = yield* Effect.all([FileSystem.FileSystem, platform, registered(plugin), extension(plugin.id)]);
    const refused = HostError.refused(`${agent} over ${plugin.name}`);
    yield* Option.match(held, {
        onNone: () => Effect.void,
        onSome: ({ name }) => Effect.flatMap(output(agent, [`${flag}remove`, name]), (removal) => (removal.code === 0 ? Effect.void : Effect.fail(refused(removal.text)))),
    });
    const { text } = yield* output(agent, [`${flag}install`, archive]);
    yield* text.includes('Installation Successful') ? Effect.void : Effect.fail(NO_HOST_RELEASE.test(text) ? HostError.refused(`No installed release of ${plugin.host.app} ${plugin.host.minVersion} or later accepting ${plugin.name}`)(text) : refused(text.trim()));
    yield* fs.remove(superseded, { recursive: true, force: true });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension, hosts, registered, replaced, running };
