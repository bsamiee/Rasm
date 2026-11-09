import { Array, Config, Effect, FileSystem, Option, Path, Redacted, Schema, Stream } from 'effect';
import { Base64, Xml, Zip } from 'effect/encoding';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SIGNING = ['ZXP_CERTIFICATE', 'ZXP_CERTIFICATE_PASSWORD'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Uxp = Schema.fromJsonString(Schema.Struct({ manifestVersion: Schema.Finite, id: Schema.String, host: Schema.Struct({ app: Schema.String }) })).annotate({
    description: 'UXP manifest.json naming one host, the form one .ccx carries',
});
const Cep = Schema.Struct({ ExtensionBundleId: Schema.String }).annotate({ description: 'ExtensionManifest attributes of a CEP CSXS/manifest.xml' });

// --- [TABLES] --------------------------------------------------------------------------

const PACKAGES = {
    uxp: {
        manifest: 'manifest.json',
        archive: '.ccx',
        variables: [],
        description: 'Packs the built UXP plugin into <id>_<host>.ccx, a zip with manifest.json at its root',
        named: (text: string) => Effect.succeed(Option.map(Schema.decodeOption(Uxp)(text), ({ id, host }) => `${id}_${host.app}.ccx`)),
        pack: Effect.fnUntraced(function* (folder: string, file: string) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const names = yield* Effect.filter((yield* fs.readDirectory(folder, { recursive: true })).toSorted(), (name) => Effect.map(fs.stat(path.join(folder, name)), ({ type }) => type === 'File'));
            yield* Stream.fromIterable(names).pipe(
                Stream.map((name) => Zip.fileStream(name.split(path.sep).join('/'), fs.stream(path.join(folder, name)))),
                Stream.pipeThroughChannel(Zip.encode()),
                Stream.run(fs.sink(file)),
            );
            return true;
        }),
    },
    zxpsigncmd: {
        manifest: 'CSXS/manifest.xml',
        archive: '.zxp',
        variables: SIGNING,
        description: 'Signs the built CEP extension into <ExtensionBundleId>.zxp with ZXPSignCmd, in the release configuration',
        named: (text: string) =>
            Stream.make(text).pipe(
                Stream.pipeThroughChannel(Xml.decodeString()),
                Stream.pipeThroughChannel(Xml.elements(['ExtensionManifest'])),
                Stream.runHead,
                Effect.map(Option.flatMap(({ attributes }) => Option.map(Schema.decodeUnknownOption(Cep)(attributes), ({ ExtensionBundleId }) => `${ExtensionBundleId}.zxp`))),
                Effect.orElseSucceed(Option.none<string>),
            ),
        pack: Effect.fnUntraced(function* (folder: string, file: string, timestampAuthority?: string) {
            const [certificate, password] = yield* Effect.all([Config.Redacted(SIGNING[0]), Config.Redacted(SIGNING[1])]);
            const p12 = yield* Effect.fromResult(Base64.decode(Redacted.value(certificate)));
            const handle = yield* ChildProcess.make('ZXPSignCmd', ['-sign', folder, file, '/dev/fd/3', Redacted.value(password), ...Array.flatMap(Option.toArray(Option.fromUndefinedOr(timestampAuthority)), (url) => ['-tsa', url])], {
                extendEnv: true,
                stdin: 'ignore',
                stdout: 'inherit',
                stderr: 'inherit',
                additionalFds: { fd3: { type: 'input', stream: Stream.make(p12) } },
            });
            return (yield* handle.exitCode) === 0;
        }, Effect.scoped),
    },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { PACKAGES };
