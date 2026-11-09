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
        description: 'Packs the built UXP plugin into <project>.ccx, a zip with manifest.json at its root',
        accepts: (text: string) => Effect.succeed(Option.isSome(Schema.decodeOption(Uxp)(text))),
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
        description: 'Signs a copy of the built CEP extension into <project>.zxp with ZXPSignCmd, in the release configuration',
        accepts: (text: string) =>
            Stream.make(text).pipe(
                Stream.pipeThroughChannel(Xml.decodeString()),
                Stream.pipeThroughChannel(Xml.elements(['ExtensionManifest'])),
                Stream.runHead,
                Effect.map(Option.exists(({ attributes }) => Schema.is(Cep)(attributes))),
                Effect.orElseSucceed(() => false),
            ),
        pack: Effect.fnUntraced(function* (folder: string, file: string, timestampAuthority?: string) {
            const [fs, path, certificate, password] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.Redacted(SIGNING[0]), Config.Redacted(SIGNING[1])]);
            const p12 = yield* Effect.fromResult(Base64.decode(Redacted.value(certificate)));
            const signed = path.join(yield* fs.makeTempDirectoryScoped(), path.basename(folder));
            yield* fs.copy(folder, signed);
            const handle = yield* ChildProcess.make('ZXPSignCmd', ['-sign', signed, file, '/dev/fd/3', Redacted.value(password), ...Array.flatMap(Option.toArray(Option.fromUndefinedOr(timestampAuthority)), (url) => ['-tsa', url])], {
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
