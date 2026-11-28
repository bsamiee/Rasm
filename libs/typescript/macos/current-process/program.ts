import { extname, join, resolve } from 'node:path';
import process from 'node:process';
import { Darwin } from '@rasm/darwin';
import { Array, Cause, ConfigProvider, Context, Duration, Effect, FileSystem, flow, Layer, Option, Predicate, Record, Schema } from 'effect';
import { Host } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Info = Schema.Struct({ CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String });
const Agent = Schema.Struct({
    BundleProgram: Schema.optionalKey(Schema.String),
    ExitTimeOut: Schema.optionalKey(Schema.Int),
    Sockets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ SockNodeName: Schema.String, SockServiceName: Schema.Int }))),
});

// --- [SERVICES] ------------------------------------------------------------------------

class Program extends Context.Service<Program>()('@rasm/current-process/program', {
    make: Effect.gen(function* () {
        const [darwin, files] = yield* Effect.all([Darwin, FileSystem.FileSystem]);
        const location = resolve(process.execPath, '..', '..', '..');
        const { info: declared, executable } = yield* darwin.bundle(location).pipe(Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`${location} bundle executable`))));
        const info = yield* Schema.decodeUnknownEffect(Info)(declared);
        const agents = join(location, 'Contents', 'Library', 'LaunchAgents');
        const agent = yield* files.readDirectory(agents).pipe(
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())),
            Effect.map(Array.filter((file) => extname(file) === '.plist')),
            Effect.flatMap(Effect.forEach((file) => files.readFile(join(agents, file)).pipe(Effect.flatMap(darwin.propertyList), Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`${join(agents, file)} property list`))), Effect.flatMap(Schema.decodeUnknownEffect(Agent))))),
            Effect.map(Array.findFirst(({ BundleProgram }) => BundleProgram !== undefined && join(location, BundleProgram) === process.execPath)),
        );
        const sockets = Option.flatMapNullishOr(agent, ({ Sockets }) => Sockets);
        const address = yield* sockets.pipe(
            Option.flatMap(flow(Record.values, Array.head)),
            Option.map(({ SockNodeName, SockServiceName }) => Effect.fromResult(Host.hostPortFromInput({ host: SockNodeName, port: SockServiceName }))),
            Effect.transposeOption,
        );
        const descriptors = yield* sockets.pipe(
            Option.map((declaring) => Effect.forEach(Record.keys(declaring), darwin.activateSocket)),
            Effect.transposeOption,
            Effect.map(Option.flatMap((answers) => Option.map(Option.all(answers), Array.flatten))),
        );
        const share = agent.pipe(
            Option.flatMapNullishOr(({ ExitTimeOut }) => ExitTimeOut),
            Option.filter((seconds) => seconds > 0),
            Option.map((seconds) => Duration.seconds(seconds / 2)),
        );
        const preferences = ConfigProvider.make((path) => {
            const [keys, indices] = Array.span(path, Predicate.isString);
            return darwin.preference(info.CFBundleIdentifier, keys.join('.')).pipe(Effect.flatMap((found) => found.pipe(Option.getOrUndefined, ConfigProvider.fromUnknown).load(indices)));
        });
        return { identifier: info.CFBundleIdentifier, version: info.CFBundleShortVersionString, executable, address, descriptors, share, preferences };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make).pipe(Layer.provide(Darwin.layer));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Program };
