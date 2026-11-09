import { Effect, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Inspected = Schema.fromJsonString(
    Schema.Tuple([
        Schema.Struct({
            Config: Schema.Struct({ Env: Schema.Array(Schema.String) }),
            NetworkSettings: Schema.Struct({ Ports: Schema.Record(Schema.String, Schema.NullOr(Schema.Array(Schema.Struct({ HostIp: Schema.String, HostPort: Schema.String })))) }),
        }),
    ]),
).annotate({ description: 'docker container inspect: the environment a container runs with and the host addresses each container port is published on' });

// --- [ERRORS] --------------------------------------------------------------------------

class ServiceError extends Schema.TaggedError<ServiceError, { readonly brand: unique symbol }>()('ServiceError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const docker = Effect.fnUntraced(function* (args: readonly string[]) {
    const handle = yield* ChildProcess.make('docker', args, { extendEnv: true, stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout.trim() : yield* ServiceError.make({ subject: ['docker', ...args].join(' '), detail: `exited ${code}: ${stderr.trim()}` });
}, Effect.scoped);

const service = Effect.fn('service')(function* (project: string, name: string) {
    const subject = `${project}/${name}`;
    const container = yield* docker(['compose', '--project-name', project, 'ps', '--quiet', name]).pipe(
        Effect.filterOrElse(
            (id) => id !== '',
            () => ServiceError.make({ subject, detail: `not running; start the ${project} Compose project` }),
        ),
    );
    const [{ Config, NetworkSettings }] = yield* Effect.flatMap(docker(['container', 'inspect', container]), Schema.decodeEffect(Inspected));
    const published = Object.values(NetworkSettings.Ports).flatMap((bindings) => (bindings ?? []).map(({ HostIp, HostPort }) => `${HostIp}:${HostPort}`));
    const [address, ...others] = published;
    return address === undefined || others.length > 0
        ? yield* ServiceError.make({ subject, detail: `publishes host addresses [${published.join(', ')}] where one address names the service` })
        : { address, environment: Record.fromIterableWith(Config.Env, (entry) => [entry.slice(0, entry.indexOf('=')), entry.slice(entry.indexOf('=') + 1)]) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ServiceError, service };
