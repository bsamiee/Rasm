import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { createRegistry, type DescMessage, type DescService, equals, fromBinary, fromJsonString, toBinary, toJsonString } from '@bufbuild/protobuf';
import { FileSystem, Path } from '@effect/platform';
import { HlcSchema } from '@rasm/contracts/rasm/contracts/clock/v1/hlc_pb';
import { Array, Context, Data, Effect, Match, Option, Schema } from 'effect';
import { xxhash128 } from 'hash-wasm';

// --- [TYPES] ---------------------------------------------------------------------------

type Specimen = Schema.Schema.Type<typeof _Specimen>;
type Expected = Schema.Schema.Type<typeof _Expected>;
type Definition = Schema.Schema.Type<typeof _Definition>;
type Actor = Schema.Schema.Type<typeof _Actor>;
type ContractCase = Schema.Schema.Type<typeof _Case>;
type Manifest = Schema.Schema.Type<typeof _Manifest>;
type ProofTarget = Manifest | ContractCase;
type Facts = Schema.Schema.Type<typeof _Facts>;
type Module = Readonly<Record<string, unknown>>;
type FingerprintAlgorithm = Schema.Schema.Type<typeof _Fingerprint>['algorithm'];
type Oracle = Schema.Schema.Type<typeof _Oracle>;
type ProofKind = keyof typeof _COLUMN;
type ProofColumn = (typeof _COLUMN)[ProofKind];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MANIFEST = 'manifest.json';
const _READ = { readUnknownFields: true } as const;
const _JSON_READ = { ignoreUnknownFields: false } as const;
const _PARSE = { errors: 'all', onExcessProperty: 'error' } as const;
const _ANCHORS = ['typescript:', 'tests/typescript/'] as const;
const _COLUMN = {
    custody: 'custody',
    'external-digest': 'externalDigest',
    'publisher-digest': 'publisherDigest',
    'semantic-conformance': 'semanticConformance',
    'semantic-roundtrip': 'semanticRoundtrip',
    'value-parity': 'valueParity',
} as const satisfies Record<Oracle | 'custody', string>;

// --- [MODELS] --------------------------------------------------------------------------

const _Rows = <A, I, R>(schema: Schema.Schema<A, I, R>) => Schema.optionalWith(Schema.Array(schema), { default: () => [] });
const _Hex128 = Schema.String.pipe(Schema.pattern(/^[0-9a-f]{32}$/));
const _Sha256 = Schema.String.pipe(Schema.pattern(/^[0-9a-f]{64}$/));
const _Fingerprint = Schema.Union(
    Schema.Struct({ algorithm: Schema.Literal('xxh128'), value: _Hex128 }),
    Schema.Struct({ algorithm: Schema.Literal('sha256'), value: _Sha256 }),
);
const _Distribution = Schema.Union(
    Schema.Struct({
        kind: Schema.Literal('typescript-json-module'),
        path: Schema.NonEmptyString,
        symbol: Schema.NonEmptyString,
    }),
    Schema.Struct({
        kind: Schema.Literal('python-package-resource'),
        path: Schema.NonEmptyString,
        package: Schema.Literal('rasm.contracts'),
    }),
);
const _Asset = {
    path: Schema.NonEmptyString,
    bytes: Schema.Int.pipe(Schema.filter((bytes) => bytes >= 0)),
    fingerprint: _Fingerprint,
} as const;
const _Specimen = Schema.Struct({
    role: Schema.Literal('specimen'),
    ..._Asset,
    minter: Schema.optionalWith(Schema.String, { default: () => '' }),
    distributions: _Rows(_Distribution),
});
const _Expected = Schema.Struct({
    role: Schema.Literal('expected'),
    ..._Asset,
    factsFormat: Schema.Literal('backend-generation-v1', 'content-digest-v1', 'hdf5-facts-v1', 'hlc-value-v1', 'matrix-market-facts-v1'),
});
const _Vector = Schema.Struct({
    specimens: Schema.NonEmptyArray(_Specimen),
    expected: Schema.optionalWith(_Expected, { as: 'Option', exact: true, nullable: true }),
});

const _ActorBase = {
    anchor: Schema.NonEmptyString,
    coordinate: Schema.NonEmptyString,
    binding: Schema.Literal('generated', 'package', 'proof'),
    supports: _Rows(
        Schema.Struct({
            kind: Schema.Literal('message', 'service', 'method'),
            fqn: Schema.NonEmptyString,
        }),
    ),
} as const;
const _MessageActor = Schema.Struct({ direction: Schema.Literal('message'), ..._ActorBase, roots: _Rows(Schema.NonEmptyString) });
const _RpcActor = (direction: 'client-request' | 'client-response' | 'server-request' | 'server-response') =>
    Schema.Struct({ direction: Schema.Literal(direction), ..._ActorBase, method: Schema.NonEmptyString });
const _Actor = Schema.Union(
    _MessageActor,
    _RpcActor('client-request'),
    _RpcActor('client-response'),
    _RpcActor('server-request'),
    _RpcActor('server-response'),
);
const _Authority = Schema.Union(
    Schema.Struct({ kind: Schema.Literal('application') }),
    Schema.Struct({ kind: Schema.Literal('domain'), producer: _Actor }),
    Schema.Struct({
        kind: Schema.Literal('infrastructure'),
        minters: Schema.Array(_Actor).pipe(Schema.filter((minters) => minters.length >= 2 || 'infrastructure authority requires two minters')),
    }),
    Schema.Struct({ kind: Schema.Literal('publisher') }),
);
const _Oracle = Schema.Literal('semantic-conformance', 'semantic-roundtrip', 'value-parity', 'external-digest', 'publisher-digest');
const _Readiness = Schema.Union(
    Schema.Struct({ kind: Schema.Literal('blocked'), blockers: Schema.NonEmptyArray(Schema.NonEmptyString) }),
    Schema.Struct({ kind: Schema.Literal('verified'), oracle: _Oracle, vectors: Schema.NonEmptyArray(_Vector) }),
);

const _Framing = Schema.Literal('proto-binary', 'proto-json', 'canonical-frame');
const _Proto = Schema.Struct({ kind: Schema.Literal('proto'), message: Schema.NonEmptyString, framing: _Framing });
const _CloudEvent = Schema.Struct({
    kind: Schema.Literal('cloudevent'),
    message: Schema.Literal('io.cloudevents.v1.CloudEvent'),
    framing: Schema.Literal('proto-binary'),
    type: Schema.NonEmptyString,
});
const _Schema = Schema.Struct({
    kind: Schema.Literal('schema'),
    path: Schema.NonEmptyString,
    framing: Schema.Literal('framed-binary', 'proto-json', 'canonical-json', 'msgpack', 'proto-binary', 'container'),
    derivedFrom: Schema.NonEmptyString,
});
const _Publisher = Schema.Struct({
    kind: Schema.Literal('publisher'),
    format: Schema.NonEmptyString,
    source: Schema.NonEmptyString,
    origin: Schema.Struct({
        repository: Schema.NonEmptyString,
        commit: Schema.String.pipe(Schema.pattern(/^[0-9a-f]{40}$/)),
        upstreamPath: Schema.NonEmptyString,
        license: Schema.Struct({
            spdx: Schema.Literal('Apache-2.0'),
            path: Schema.NonEmptyString,
            sha256: _Sha256,
        }),
    }),
});
const _Law = Schema.Struct({
    kind: Schema.Literal('law'),
    anchor: Schema.NonEmptyString,
    format: Schema.Literal('binary', 'hdf5', 'json', 'text'),
});
const _Definition = Schema.Union(_CloudEvent, _Proto, _Schema, _Publisher, _Law);
const _Case = Schema.Struct({
    id: Schema.NonEmptyString,
    definition: _Definition,
    authority: _Authority,
    readiness: _Readiness,
    consumers: _Rows(_Actor),
});
const _Entry = Schema.Struct({ id: Schema.NonEmptyString, law: Schema.NonEmptyString, cases: Schema.NonEmptyArray(_Case) });
const _Manifest = Schema.Struct({ version: Schema.Literal(2), entries: Schema.NonEmptyArray(_Entry) });

const _Field = Schema.Struct({
    kind: Schema.Literal('field'),
    path: Schema.Literal('/field'),
    dtype: Schema.Literal('float32'),
    shape: Schema.Tuple(Schema.Int, Schema.Int),
    chunks: Schema.Tuple(Schema.Int, Schema.Int),
    compression: Schema.Struct({ kind: Schema.Literal('gzip'), level: Schema.Int, shuffle: Schema.Boolean }),
    values: Schema.Tuple(Schema.Tuple(Schema.Number, Schema.Number), Schema.Tuple(Schema.Number, Schema.Number)),
    rootAttributes: Schema.Struct({
        bits: Schema.Int,
        bound: Schema.Number,
        formatKey: Schema.NonEmptyString,
        maxResidual: Schema.Number,
        residence: Schema.Literal('exact'),
    }),
    virtual: Schema.Struct({
        dims: Schema.Tuple(Schema.NonEmptyString, Schema.NonEmptyString),
        shape: Schema.Tuple(Schema.Int, Schema.Int),
        dtype: Schema.Literal('float32'),
        chunks: Schema.NonEmptyArray(Schema.Struct({ key: Schema.NonEmptyString, offset: Schema.Int, length: Schema.Int })),
    }),
});
const _Graduation = Schema.Struct({
    kind: Schema.Literal('graduation'),
    path: Schema.Literal('/bands'),
    evidenceKey: _Hex128,
    climate: Schema.Struct({
        kind: Schema.Literal('categorical'),
        categories: Schema.NonEmptyArray(Schema.NonEmptyString),
        mass: Schema.NonEmptyArray(Schema.Number),
    }),
    temperature: Schema.Struct({
        kind: Schema.Literal('numeric'),
        edges: Schema.NonEmptyArray(Schema.Number),
        mass: Schema.NonEmptyArray(Schema.Number),
    }),
});
const _Sparse = Schema.Struct({
    kind: Schema.Literal('sparse'),
    path: Schema.Literal('/A'),
    attributes: Schema.Struct({
        fill: Schema.Int,
        format: Schema.Literal('csc'),
        frobenius: Schema.Number,
        kind: Schema.Literal('lu'),
        ordering: Schema.Int,
        shape: Schema.Tuple(Schema.Int, Schema.Int),
        symmetric: Schema.Boolean,
    }),
    indices: Schema.NonEmptyArray(Schema.Int),
    indptr: Schema.NonEmptyArray(Schema.Int),
    permutation: Schema.NonEmptyArray(Schema.Int),
    values: Schema.NonEmptyArray(Schema.Number),
});
const _Waveform = Schema.Struct({
    kind: Schema.Literal('waveform'),
    path: Schema.Literal('/waveform'),
    dtype: Schema.Literal('float32'),
    shape: Schema.Tuple(Schema.Int, Schema.Int),
    chunks: Schema.Tuple(Schema.Int, Schema.Int),
    compression: Schema.Struct({ kind: Schema.Literal('gzip'), level: Schema.Int, shuffle: Schema.Boolean }),
    sampleRate: Schema.Number,
    values: Schema.Array(Schema.Array(Schema.Number)),
});
const _MatrixMarket = Schema.Struct({
    kind: Schema.Literal('matrix-market'),
    shape: Schema.Tuple(Schema.Int, Schema.Int),
    entries: Schema.Array(
        Schema.Struct({
            row: Schema.Int,
            column: Schema.Int,
            value: Schema.Number,
        }),
    ),
});
const _Content = Schema.Struct({
    kind: Schema.Literal('content-digest'),
    algorithm: Schema.Literal('xxh128'),
    seed: Schema.Literal(0),
    value: _Hex128,
});
const _BackendGeneration = Schema.Struct({
    kind: Schema.Literal('backend-generation'),
    contract: Schema.NonEmptyString,
    artifactKeys: Schema.Array(Schema.NonEmptyString),
    capabilityKeys: Schema.Array(Schema.NonEmptyString),
    preimageBytes: Schema.Int.pipe(Schema.filter((bytes) => bytes > 0)),
    preimageXxh128: _Hex128,
    preimageHex: Schema.String.pipe(Schema.pattern(/^(?:[0-9a-f]{2})+$/)),
});
const _Hlc = Schema.Struct({
    kind: Schema.Literal('hlc-value'),
    physical: Schema.String.pipe(Schema.pattern(/^[0-9]+$/)),
    logical: Schema.String.pipe(Schema.pattern(/^[0-9]+$/)),
});
const _Hdf = Schema.Union(_Field, _Graduation, _Sparse, _Waveform);
const _Facts = Schema.Union(_BackendGeneration, _Content, _Hdf, _Hlc, _MatrixMarket);

const _decode = Schema.decodeUnknown(Schema.parseJson(_Manifest), _PARSE);
const _decodeJson = Schema.decodeUnknown(Schema.parseJson(Schema.Unknown), _PARSE);
const _factDecoders: Readonly<Record<Expected['factsFormat'], (input: string) => Effect.Effect<Facts, unknown>>> = {
    'backend-generation-v1': Schema.decodeUnknown(Schema.parseJson(_BackendGeneration), _PARSE),
    'content-digest-v1': Schema.decodeUnknown(Schema.parseJson(_Content), _PARSE),
    'hdf5-facts-v1': Schema.decodeUnknown(Schema.parseJson(_Hdf), _PARSE),
    'hlc-value-v1': Schema.decodeUnknown(Schema.parseJson(_Hlc), _PARSE),
    'matrix-market-facts-v1': Schema.decodeUnknown(Schema.parseJson(_MatrixMarket), _PARSE),
};
const _sameContent = Schema.equivalence(_Content);
const _sameHlc = Schema.equivalence(_Hlc);

class CorpusFault extends Data.TaggedError('CorpusFault')<{
    readonly reason: 'unreadable' | 'malformed' | 'unregistered' | 'drift';
    readonly detail: string;
}> {}

class CorpusRoot extends Context.Reference<CorpusRoot>()('rasm-testkit/CorpusRoot', {
    defaultValue: (): string => fileURLToPath(new URL('../../contracts', import.meta.url)),
}) {}

class Proof extends Schema.Class<Proof>('Proof')({
    assets: Schema.Int,
    blocked: Schema.Int,
    custody: Schema.Int,
    externalDigest: Schema.Int,
    publisherDigest: Schema.Int,
    semanticConformance: Schema.Int,
    semanticRoundtrip: Schema.Int,
    valueParity: Schema.Int,
}) {
    get verified(): number {
        return Array.reduce(Object.values(_COLUMN), 0, (total: number, column: ProofColumn) => total + this[column]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isDescriptor = (value: unknown): value is DescMessage | DescService =>
    typeof value === 'object' &&
    value !== null &&
    'kind' in value &&
    (value.kind === 'message' || value.kind === 'service') &&
    'typeName' in value &&
    typeof value.typeName === 'string';

// Module-relative glob (vite-root independent); keys renormalize to estate-root form so registry
// distribution rows ('/libs/...') resolve under any project root.
const _modules = Object.fromEntries(
    Object.entries(import.meta.glob<Module>('../../../libs/typescript/contracts/gen/**/*.ts', { eager: true })).map(
        ([key, module]) => [key.replace(/^(?:\.\.\/)+/, '/'), module] as const,
    ),
);
const _descriptors = Array.flatMap(Object.values(_modules), (module) => Array.filter(Object.values(module), _isDescriptor));
const _registry = createRegistry(..._descriptors);

const _digesters = {
    sha256: (bytes: Uint8Array): Effect.Effect<string, CorpusFault> =>
        Effect.try({
            try: () => createHash('sha256').update(bytes).digest('hex'),
            catch: (fault) => new CorpusFault({ reason: 'drift', detail: String(fault) }),
        }),
    xxh128: (bytes: Uint8Array): Effect.Effect<string, CorpusFault> =>
        Effect.tryPromise({
            try: () => xxhash128(bytes),
            catch: (fault) => new CorpusFault({ reason: 'drift', detail: String(fault) }),
        }),
} as const satisfies Record<FingerprintAlgorithm, (bytes: Uint8Array) => Effect.Effect<string, CorpusFault>>;

const _relativePath = (value: string): boolean => {
    const parts = value.split('/');
    return (
        value.length > 0 &&
        !value.startsWith('/') &&
        !value.includes('\\') &&
        Array.every(parts, (part) => part !== '' && part !== '.' && part !== '..')
    );
};

const _readPath = (assetPath: string): Effect.Effect<Uint8Array, CorpusFault, FileSystem.FileSystem | Path.Path> =>
    _relativePath(assetPath)
        ? Effect.gen(function* () {
              const fs = yield* FileSystem.FileSystem;
              const path = yield* Path.Path;
              const root = yield* CorpusRoot;
              return yield* Effect.mapError(
                  fs.readFile(path.join(root, assetPath)),
                  (fault) => new CorpusFault({ reason: 'unreadable', detail: `${assetPath}: ${fault.message}` }),
              );
          })
        : Effect.fail(new CorpusFault({ reason: 'unregistered', detail: `${assetPath}: path is not normalized corpus-relative` }));

const _read = (asset: Specimen | Expected): Effect.Effect<Uint8Array, CorpusFault, FileSystem.FileSystem | Path.Path> => _readPath(asset.path);

const _distribution = (asset: Specimen | Expected, raw: Uint8Array): Effect.Effect<void, CorpusFault> =>
    Match.value(asset).pipe(
        Match.discriminator('role')('expected', () => Effect.void),
        Match.discriminator('role')('specimen', (specimen) =>
            Effect.forEach(specimen.distributions, (distribution) =>
                Match.value(distribution).pipe(
                    Match.discriminator('kind')('python-package-resource', () => Effect.void),
                    Match.discriminator('kind')('typescript-json-module', (held) =>
                        Effect.gen(function* () {
                            const module = _modules[`/${held.path}`];
                            const source = yield* Effect.mapError(
                                _decodeJson(new TextDecoder().decode(raw)),
                                (fault) => new CorpusFault({ reason: 'drift', detail: `${asset.path}: ${fault.message}` }),
                            );
                            if (module === undefined || !(held.symbol in module) || JSON.stringify(module[held.symbol]) !== JSON.stringify(source)) {
                                return yield* Effect.fail(
                                    new CorpusFault({
                                        reason: 'drift',
                                        detail: `${held.path}#${held.symbol}: distributed value differs from ${asset.path}`,
                                    }),
                                );
                            }
                        }),
                    ),
                    Match.exhaustive,
                ),
            ).pipe(Effect.asVoid),
        ),
        Match.exhaustive,
    );

const _verified = (asset: Specimen | Expected): Effect.Effect<Uint8Array, CorpusFault, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const raw = yield* _read(asset);
        const digest = yield* _digesters[asset.fingerprint.algorithm](raw);
        if (raw.length !== asset.bytes || digest !== asset.fingerprint.value) {
            return yield* Effect.fail(
                new CorpusFault({
                    reason: 'drift',
                    detail: `${asset.path}: expected ${asset.bytes}/${asset.fingerprint.value}, got ${raw.length}/${digest}`,
                }),
            );
        }
        yield* _distribution(asset, raw);
        return raw;
    });

const _publisher = (
    contract: ContractCase,
    specimens: ReadonlyArray<Specimen>,
): Effect.Effect<void, CorpusFault, FileSystem.FileSystem | Path.Path> => {
    const definition = contract.definition;
    return definition.kind === 'publisher' &&
        Array.every(
            specimens,
            (asset) =>
                asset.fingerprint.algorithm === 'sha256' && (asset.path === definition.source || asset.path.startsWith(`${definition.source}/`)),
        )
        ? Effect.gen(function* () {
              const license = definition.origin.license;
              const raw = yield* _readPath(license.path);
              const digest = yield* _digesters.sha256(raw);
              if (digest !== license.sha256) {
                  return yield* Effect.fail(
                      new CorpusFault({ reason: 'drift', detail: `${license.path}: expected ${license.sha256}, got ${digest}` }),
                  );
              }
          })
        : Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: publisher digest lacks source-contained SHA-256 custody` }));
};

const _actors = (contract: ContractCase): ReadonlyArray<Actor> => {
    const authority = Match.value(contract.authority).pipe(
        Match.discriminator('kind')('application', () => []),
        Match.discriminator('kind')('domain', (held) => [held.producer]),
        Match.discriminator('kind')('infrastructure', (held) => held.minters),
        Match.discriminator('kind')('publisher', () => []),
        Match.exhaustive,
    );
    return [...authority, ...contract.consumers];
};

const _anchored = (actor: Actor): boolean => Array.some(_ANCHORS, (prefix) => actor.anchor.startsWith(prefix));

const _method = (fqn: string): boolean => {
    const boundary = fqn.lastIndexOf('.');
    if (boundary < 1) {
        return false;
    }
    const service = _registry.getService(fqn.slice(0, boundary));
    return service !== undefined && Array.some(service.methods, (method) => `${service.typeName}.${method.name}` === fqn);
};

const _absent = (contract: ContractCase, subject: string, fqn: string): CorpusFault =>
    new CorpusFault({ reason: 'unregistered', detail: `${contract.id}: generated TypeScript ${subject} ${fqn} is absent` });

const _messageFaults = (contract: ContractCase, actor: Actor): ReadonlyArray<CorpusFault> => {
    const definition = contract.definition;
    const roots = [
        ...(definition.kind === 'proto' || definition.kind === 'cloudevent' ? [definition.message] : []),
        ...(definition.kind === 'publisher' && actor.direction === 'message' ? actor.roots : []),
        ...Array.filterMap(actor.supports, (support) => (support.kind === 'message' ? Option.some(support.fqn) : Option.none())),
    ];
    return Array.filterMap(roots, (fqn) => {
        const descriptor = _registry.getMessage(fqn);
        if (descriptor === undefined) {
            return Option.some(_absent(contract, 'message', fqn));
        }
        return definition.kind === 'publisher' && !definition.source.endsWith(`${descriptor.file.name}.proto`)
            ? Option.some(
                  new CorpusFault({
                      reason: 'drift',
                      detail: `${contract.id}: publisher source ${definition.source} does not own ${fqn} from ${descriptor.file.name}`,
                  }),
              )
            : Option.none();
    });
};

const _supportFaults = (contract: ContractCase, actor: Actor): ReadonlyArray<CorpusFault> =>
    Array.filterMap(actor.supports, (support) =>
        (support.kind === 'service' ? _registry.getService(support.fqn) !== undefined : support.kind !== 'method' || _method(support.fqn))
            ? Option.none()
            : Option.some(_absent(contract, support.kind, support.fqn)),
    );

const _actorFaults = (contract: ContractCase, actor: Actor): ReadonlyArray<CorpusFault> => {
    const definition = contract.definition;
    if (definition.kind === 'law' || definition.kind === 'schema') {
        return [
            new CorpusFault({
                reason: 'drift',
                detail: `${contract.id}: ${actor.anchor} claims generated custody for a non-generated definition`,
            }),
        ];
    }
    return [
        ..._messageFaults(contract, actor),
        ...(actor.direction !== 'message' && !_method(actor.method) ? [_absent(contract, 'method', actor.method)] : []),
        ..._supportFaults(contract, actor),
    ];
};

const _bindings = (contract: ContractCase): Effect.Effect<void, CorpusFault> =>
    Option.match(
        Array.head(
            Array.flatMap(_actors(contract), (actor) => (actor.binding === 'generated' && _anchored(actor) ? _actorFaults(contract, actor) : [])),
        ),
        { onNone: () => Effect.void, onSome: Effect.fail },
    );

const _message = (definition: Definition): Option.Option<readonly [string, 'proto-binary' | 'proto-json' | 'canonical-frame']> =>
    Match.value(definition).pipe(
        Match.discriminator('kind')('proto', (held) => Option.some([held.message, held.framing] as const)),
        Match.discriminator('kind')('cloudevent', (held) => Option.some([held.message, held.framing] as const)),
        Match.discriminator('kind')('law', () => Option.none()),
        Match.discriminator('kind')('publisher', () => Option.none()),
        Match.discriminator('kind')('schema', () => Option.none()),
        Match.exhaustive,
    );

const _typescript = (contract: ContractCase): boolean => Array.some(_actors(contract), _anchored);

const _sameBytes = (first: Uint8Array, second: Uint8Array): boolean =>
    first.length === second.length && first.every((value, index) => value === second[index]);

const _roundtrip = (contract: ContractCase, specimen: Specimen, raw: Uint8Array): Effect.Effect<void, CorpusFault> =>
    Option.match(_message(contract.definition), {
        onNone: () => Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: semantic round-trip has no generated descriptor` })),
        onSome: ([typeName, framing]) =>
            Option.match(Option.fromNullable(_registry.getMessage(typeName)), {
                onNone: () => Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: generated descriptor ${typeName} is absent` })),
                onSome: (descriptor) =>
                    Effect.flatMap(
                        Effect.try({
                            try: () =>
                                Match.value(framing).pipe(
                                    Match.when('proto-binary', () => {
                                        const decoded = fromBinary(descriptor, raw, _READ);
                                        const normalized = toBinary(descriptor, decoded, { writeUnknownFields: false });
                                        return (
                                            _sameBytes(normalized, toBinary(descriptor, decoded, { writeUnknownFields: true })) &&
                                            equals(descriptor, decoded, fromBinary(descriptor, normalized, _READ))
                                        );
                                    }),
                                    Match.when('proto-json', () => {
                                        const decoded = fromJsonString(descriptor, new TextDecoder().decode(raw), _JSON_READ);
                                        return equals(descriptor, decoded, fromJsonString(descriptor, toJsonString(descriptor, decoded), _JSON_READ));
                                    }),
                                    Match.when('canonical-frame', () => false),
                                    Match.exhaustive,
                                ),
                            catch: (fault) => new CorpusFault({ reason: 'drift', detail: `${specimen.path}: ${String(fault)}` }),
                        }),
                        (same) =>
                            same
                                ? Effect.void
                                : Effect.fail(
                                      new CorpusFault({
                                          reason: 'drift',
                                          detail: `${specimen.path}: generated descriptor round-trip changed its semantic value`,
                                      }),
                                  ),
                    ),
            }),
    });

const _expected = (asset: Expected, raw: Uint8Array): Effect.Effect<Facts, CorpusFault> =>
    Effect.mapError(
        _factDecoders[asset.factsFormat](new TextDecoder().decode(raw)),
        (fault) => new CorpusFault({ reason: 'drift', detail: `${asset.path}: ${String(fault)}` }),
    );

const _provenance = (contract: ContractCase, specimens: ReadonlyArray<Specimen>): Effect.Effect<void, CorpusFault> => {
    if (contract.authority.kind !== 'infrastructure') {
        return Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: value parity has no infrastructure minters` }));
    }
    const declared = Array.map(contract.authority.minters, (actor) => `${actor.anchor}@${actor.coordinate}`);
    const observed = Array.map(specimens, (specimen) => specimen.minter);
    const exact =
        observed.length === declared.length &&
        new Set(observed).size === observed.length &&
        new Set(declared).size === declared.length &&
        Array.every(declared, (minter) => observed.includes(minter));
    return exact
        ? Effect.void
        : Effect.fail(
              new CorpusFault({
                  reason: 'drift',
                  detail: `${contract.id}: value-parity specimens do not carry exactly one declared minter`,
              }),
          );
};

const _parity = (
    contract: ContractCase,
    specimens: ReadonlyArray<readonly [Specimen, Uint8Array]>,
    facts: Facts,
): Effect.Effect<ProofKind, CorpusFault> =>
    Match.value(facts).pipe(
        Match.discriminator('kind')('backend-generation', () => Effect.succeed('custody' as const)),
        Match.discriminator('kind')('content-digest', (expected) =>
            Effect.as(
                Effect.forEach(specimens, ([specimen, raw]) =>
                    Effect.flatMap(_digesters.xxh128(raw), (value) =>
                        _sameContent({ kind: 'content-digest', algorithm: 'xxh128', seed: 0, value }, expected)
                            ? Effect.void
                            : Effect.fail(
                                  new CorpusFault({ reason: 'drift', detail: `${specimen.path}: content digest differs from expected facts` }),
                              ),
                    ),
                ),
                'value-parity' as const,
            ),
        ),
        Match.discriminator('kind')('hlc-value', (expected) =>
            contract.definition.kind === 'proto' && contract.definition.message === HlcSchema.typeName
                ? Effect.as(
                      Effect.forEach(specimens, ([specimen, raw]) =>
                          Effect.flatMap(
                              Effect.try({
                                  try: () => fromBinary(HlcSchema, raw, _READ),
                                  catch: (fault) => new CorpusFault({ reason: 'drift', detail: `${specimen.path}: ${String(fault)}` }),
                              }),
                              (decoded) =>
                                  _sameHlc(
                                      { kind: 'hlc-value', physical: decoded.physical.toString(), logical: decoded.logical.toString() },
                                      expected,
                                  )
                                      ? Effect.void
                                      : Effect.fail(
                                            new CorpusFault({ reason: 'drift', detail: `${specimen.path}: HLC value differs from expected facts` }),
                                        ),
                          ),
                      ),
                      'value-parity' as const,
                  )
                : Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: HLC facts do not bind ${HlcSchema.typeName}` })),
        ),
        Match.discriminator('kind')('field', () => Effect.succeed('custody' as const)),
        Match.discriminator('kind')('graduation', () => Effect.succeed('custody' as const)),
        Match.discriminator('kind')('sparse', () => Effect.succeed('custody' as const)),
        Match.discriminator('kind')('waveform', () => Effect.succeed('custody' as const)),
        Match.discriminator('kind')('matrix-market', () => Effect.succeed('custody' as const)),
        Match.exhaustive,
    );

const _vector = (
    contract: ContractCase,
    oracle: Oracle,
    vector: Schema.Schema.Type<typeof _Vector>,
): Effect.Effect<ProofKind, CorpusFault, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const specimens = yield* Effect.forEach(vector.specimens, (asset) => Effect.map(_verified(asset), (raw) => [asset, raw] as const));
        const expected = yield* Option.match(vector.expected, {
            onNone: () => Effect.succeed(Option.none<readonly [Expected, Uint8Array]>()),
            onSome: (asset) => Effect.map(_verified(asset), (raw) => Option.some([asset, raw] as const)),
        });
        return yield* Match.value(oracle).pipe(
            Match.when('external-digest', () => Effect.succeed('external-digest' as const)),
            Match.when('publisher-digest', () =>
                Effect.as(
                    _publisher(
                        contract,
                        Array.map(specimens, ([asset]) => asset),
                    ),
                    'publisher-digest' as const,
                ),
            ),
            Match.when('semantic-roundtrip', () =>
                _typescript(contract)
                    ? Effect.as(
                          Effect.forEach(specimens, ([asset, raw]) => _roundtrip(contract, asset, raw)),
                          'semantic-roundtrip' as const,
                      )
                    : Effect.succeed('custody' as const),
            ),
            Match.when('semantic-conformance', () =>
                Option.match(expected, {
                    onNone: () => Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: expected facts are absent` })),
                    onSome: ([asset, raw]) =>
                        Effect.flatMap(_expected(asset, raw), (facts) =>
                            facts.kind === 'field' || facts.kind === 'graduation' || facts.kind === 'sparse' || facts.kind === 'waveform'
                                ? Effect.succeed('custody' as const)
                                : _parity(contract, specimens, facts).pipe(Effect.as('semantic-conformance' as const)),
                        ),
                }),
            ),
            Match.when('value-parity', () =>
                Option.match(expected, {
                    onNone: () => Effect.fail(new CorpusFault({ reason: 'drift', detail: `${contract.id}: expected facts are absent` })),
                    onSome: ([asset, raw]) =>
                        Effect.zipRight(
                            _provenance(
                                contract,
                                Array.map(specimens, ([specimen]) => specimen),
                            ),
                            Effect.flatMap(_expected(asset, raw), (facts) => _parity(contract, specimens, facts)),
                        ),
                }),
            ),
            Match.exhaustive,
        );
    });

const _cases = (target: ProofTarget): ReadonlyArray<ContractCase> =>
    'entries' in target ? Array.flatMap(target.entries, (entry) => entry.cases) : [target];

const _prove = (target: ProofTarget): Effect.Effect<Proof, CorpusFault, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const cases = _cases(target);
        yield* Effect.forEach(cases, _bindings, { concurrency: 'unbounded', discard: true });
        const kinds = yield* Effect.forEach(
            cases,
            (contract): Effect.Effect<ReadonlyArray<ProofKind>, CorpusFault, FileSystem.FileSystem | Path.Path> => {
                const readiness = contract.readiness;
                return readiness.kind === 'verified'
                    ? Effect.forEach(readiness.vectors, (vector) => _vector(contract, readiness.oracle, vector))
                    : Effect.succeed([] as ReadonlyArray<ProofKind>);
            },
            { concurrency: 'unbounded' },
        ).pipe(Effect.map((nested) => Array.flatMap(nested, (held) => held)));
        const counted = Array.groupBy(kinds, (kind) => _COLUMN[kind]);
        const count = (column: ProofColumn): number => counted[column]?.length ?? 0;
        const assets = Array.reduce(
            cases,
            0,
            (total, contract) =>
                total +
                (contract.readiness.kind === 'verified'
                    ? Array.reduce(
                          contract.readiness.vectors,
                          0,
                          (held, vector) => held + vector.specimens.length + Option.match(vector.expected, { onNone: () => 0, onSome: () => 1 }),
                      )
                    : 0),
        );
        return new Proof({
            assets,
            blocked: Array.filter(cases, (contract) => contract.readiness.kind === 'blocked').length,
            custody: count('custody'),
            externalDigest: count('externalDigest'),
            publisherDigest: count('publisherDigest'),
            semanticConformance: count('semanticConformance'),
            semanticRoundtrip: count('semanticRoundtrip'),
            valueParity: count('valueParity'),
        });
    });

const Corpus = {
    Manifest: _Manifest,
    manifest: Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const root = yield* CorpusRoot;
        const raw = yield* Effect.mapError(
            fs.readFileString(path.join(root, _MANIFEST)),
            (fault) => new CorpusFault({ reason: 'unreadable', detail: fault.message }),
        );
        return yield* Effect.mapError(_decode(raw), (fault) => new CorpusFault({ reason: 'malformed', detail: fault.message }));
    }),
    resolve: (manifest: Manifest, coordinate: { readonly entry: string; readonly case: string }): Effect.Effect<ContractCase, CorpusFault> =>
        Effect.mapError(
            Option.flatMap(
                Array.findFirst(manifest.entries, (entry) => entry.id === coordinate.entry),
                (entry) => Array.findFirst(entry.cases, (held) => held.id === coordinate.case),
            ),
            () => new CorpusFault({ reason: 'unregistered', detail: `${coordinate.entry}/${coordinate.case}` }),
        ),
    load: _read,
    prove: _prove,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Corpus, CorpusFault, CorpusRoot, Proof };
