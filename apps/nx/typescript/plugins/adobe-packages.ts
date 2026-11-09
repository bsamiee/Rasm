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

// --- [ERRORS] --------------------------------------------------------------------------

class TypeScriptError extends Schema.TaggedError<TypeScriptError, { readonly brand: unique symbol }>()('TypeScriptError', { message: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {}

// --- [TABLES] --------------------------------------------------------------------------

const PACKAGES = {
    uxp: {
        manifest: 'manifest.json',
        archive: '.ccx',
        variables: [],
        description: 'Packs the UXP plugin a build wrote, else its source folder, into <project>.ccx, a zip with manifest.json at its root',
        accepts: (text: string) => Effect.succeed(Option.isSome(Schema.decodeOption(Uxp)(text))),
        pack: Effect.fnUntraced(function* (folder: string, names: readonly string[], file: string) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
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
        description: 'Signs a copy of the CEP extension a build wrote, else its source folder, into <project>.zxp with ZXPSignCmd, in the release configuration',
        accepts: (text: string) =>
            Stream.make(text).pipe(
                Stream.pipeThroughChannel(Xml.decodeString()),
                Stream.pipeThroughChannel(Xml.elements(['ExtensionManifest'])),
                Stream.runHead,
                Effect.map(Option.exists(({ attributes }) => Schema.is(Cep)(attributes))),
                Effect.orElseSucceed(() => false),
            ),
        pack: Effect.fnUntraced(function* (folder: string, names: readonly string[], file: string, timestampAuthority?: string) {
            const [fs, path, certificate, password] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.Redacted(SIGNING[0]), Config.Redacted(SIGNING[1])]);
            const p12 = yield* Effect.fromResult(Base64.decode(Redacted.value(certificate)));
            const signed = path.join(yield* fs.makeTempDirectoryScoped(), path.basename(folder));
            yield* Effect.forEach(names, (name) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(signed, name)), { recursive: true }), fs.copyFile(path.join(folder, name), path.join(signed, name))), { discard: true });
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

export { PACKAGES, TypeScriptError };
