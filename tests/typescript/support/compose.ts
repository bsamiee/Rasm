import { basename, dirname } from 'node:path';
import { Array, Effect, Formatter, Function, Predicate, Result, Schema, SchemaTransformation } from 'effect';
import { DockerComposeEnvironment, getContainerRuntimeClient } from 'testcontainers';

// --- [MODELS] --------------------------------------------------------------------------

const Environment = Schema.String.pipe(Schema.decodeTo(Schema.Record(Schema.String, Schema.String), SchemaTransformation.splitKeyValue({ separator: '\0' }))).annotate({
    description: 'container environment, its `KEY=VALUE` entries joined by NUL, which no entry holds',
});

// --- [ERRORS] --------------------------------------------------------------------------

class ServiceError extends Schema.TaggedError<ServiceError, { readonly brand: unique symbol }>()('ServiceError', {
    reason: Schema.TaggedUnion({ Unreachable: {}, Unstarted: {}, Absent: { running: Schema.Int }, Unpublished: { port: Schema.Int, published: Schema.Array(Schema.Int) } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: ServiceError['reason'], subject: string) => (cause?: unknown) => ServiceError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const reason = ServiceError.fields.reason.match(this.reason, {
            Unreachable: () => 'container runtime refused the call',
            Unstarted: () => 'services did not start',
            Absent: ({ running }) => `${running} containers run where one names the service`,
            Unpublished: ({ port, published }) => `publishes host ports [${published.join(', ')}] for container port ${port} where one names the service`,
        });
        return [`${this.subject}: ${reason}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const service = Effect.fn('service')(function* (project: string, name: string, port: number) {
    const subject = `${project}/${name}`;
    const unreachable = ServiceError.of({ _tag: 'Unreachable' }, subject);
    const client = yield* Effect.tryPromise({ try: getContainerRuntimeClient, catch: unreachable });
    const [container] = yield* Effect.tryPromise({
        try: () => client.container.dockerode.listContainers({ filters: { label: [`com.docker.compose.project=${project}`, `com.docker.compose.service=${name}`] } }),
        catch: unreachable,
    }).pipe(Effect.filterOrFail(Predicate.isTupleOf(1), ({ length }) => ServiceError.of({ _tag: 'Absent', running: length }, subject)()));
    const published = Array.dedupe(Array.filterMap(container.Ports, ({ PrivatePort, PublicPort, Type }) => (PrivatePort === port && Type === 'tcp' && PublicPort !== undefined ? Result.succeed(PublicPort) : Result.failVoid)));
    const [bound] = yield* Effect.fromResult(Result.liftPredicate(published, Predicate.isTupleOf(1), (ports) => ServiceError.of({ _tag: 'Unpublished', port, published: ports }, subject)()));
    const { Config } = yield* Effect.tryPromise({ try: () => client.container.inspect(client.container.getById(container.Id)), catch: unreachable });
    return { address: `${client.info.containerRuntime.host}:${bound}`, environment: yield* Effect.orDie(Schema.decodeEffect(Environment)(Config.Env.join('\0'))) };
});

const environment = (file: string, services: Array.NonEmptyReadonlyArray<string>, configure: (environment: DockerComposeEnvironment) => DockerComposeEnvironment = Function.identity) =>
    Effect.acquireDisposable(
        Effect.tryPromise({
            try: () => configure(new DockerComposeEnvironment(dirname(file), basename(file)).withEnvironment({ COMPOSE_PROFILES: '*' })).up([...services]),
            catch: ServiceError.of({ _tag: 'Unstarted' }, `${file} ${services.join(' ')}`),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { environment, ServiceError, service };
