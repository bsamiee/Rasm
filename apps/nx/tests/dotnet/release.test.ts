import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { pairs, placed, publishing, released, resolved } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect, Layer, Schema } from 'effect';
import { files, MSBUILD, members, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;
const MANIFESTS = ['src/Core/Fixture.Core.csproj', 'src/Middle/Fixture.Middle.csproj'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Found = Schema.fromJsonString(Schema.Struct({ searchResult: Schema.Array(Schema.Struct({ packages: Schema.Array(Schema.Struct({ id: Schema.String, version: Schema.String })) })) })).annotate({
    description: 'JSON dotnet package search --exact-match --format json prints: per source every version of the package, oldest first',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const packable = Effect.map(members, (stated) => stated.filter((project) => project.packable));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-dotnet release of MSBuild projects versioned together', (methods) => {
    const NEXT = '0.2.0';

    methods.effect(
        'gives every project MSBuild marks packable the release tag',
        Effect.fnUntraced(function* () {
            const [stated, { tagged }] = yield* Effect.all([packable, placed]);
            assert.includeMembers(
                tagged,
                stated.map(({ root }) => root),
                `[${README.tagged}; ${MSBUILD.kinds}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'reads the current version of each packable project from its project file',
        Effect.fnUntraced(function* () {
            const [stated, shown] = yield* Effect.all([packable, released('version', NEXT, '--dry-run')]);
            assert.strictEqual(shown.code, 0, shown.output);
            assert.deepStrictEqual(
                Array.difference(
                    stated.map(({ manifest, version }) => `Resolved the current version as ${version} from manifest: ${manifest}`),
                    resolved(shown.output),
                ),
                [],
                `[${NX.disk}; ${MSBUILD.version}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'writes the new version where MSBuild evaluates it',
        Effect.fnUntraced(function* () {
            const versioned = yield* released('version', NEXT);
            assert.strictEqual(versioned.code, 0, versioned.output);
            const stated = yield* packable;
            assert.isNotEmpty(stated, `[${MSBUILD.kinds}]`);
            assert.deepStrictEqual(
                stated.filter(({ version }) => version !== NEXT).map(({ name, version }) => `${name} ${version}`),
                [],
                `[${NX.written}; ${MSBUILD.version}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'changes nothing in a project file but the version text, comments and layout kept',
        Effect.fnUntraced(function* () {
            const { text } = yield* Consumer;
            assert.deepStrictEqual(
                yield* Effect.forEach(MANIFESTS, text),
                MANIFESTS.map((manifest) => files[manifest].replace('<Version>0.1.0</Version>', `<Version>${NEXT}</Version>`)),
                `[${NX.layout}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'gives every packable project a publish target waiting on the publish of the packable projects it references, and none to a project MSBuild marks not packable',
        Effect.fnUntraced(function* () {
            const [stated, planned] = yield* Effect.all([packable, publishing]);
            assert.sameMembers(
                planned.folders,
                stated.map(({ root }) => root),
                `[${NX.publish}; ${MSBUILD.kinds}]`,
            );
            assert.sameMembers(planned.pairs, pairs(stated), `[${NX.publish}; ${MSBUILD.references}]`);
        }),
        bounds.test,
    );
});

it.layer(
    Layer.provideMerge(
        Consumer.layer({
            resolved: import.meta.url,
            settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version: { currentVersionResolver: 'registry' } } },
            ignored: ['bin', 'obj'],
            files: {
                'src/Json/Newtonsoft.Json.csproj': '<Project Sdk="Microsoft.NET.Sdk">\n    <PropertyGroup>\n        <TargetFramework>net10.0</TargetFramework>\n        <Version>0.0.1</Version>\n    </PropertyGroup>\n</Project>\n',
                'src/Json/Empty.cs': 'namespace Newtonsoft.Json;\n',
            },
        }),
        Layer.provideMerge(Packed.layer(plugin.name), layer),
    ),
    { timeout: bounds.built, excludeTestServices: true },
)('@rasm/nx-dotnet release of a package nuget.org holds', (methods) => {
    methods.effect(
        'reads the current version from the source dotnet searches',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const found = yield* called('dotnet', ['package', 'search', 'Newtonsoft.Json', '--exact-match', '--format', 'json']);
            const { searchResult } = yield* Schema.decodeEffect(Found)(found.stdout);
            const stable = searchResult.flatMap(({ packages }) => packages).filter(({ version }) => !version.includes('-'));
            const latest = yield* Effect.orDie(Effect.fromOption(Array.last(stable)));
            const shown = yield* released('version', 'patch', '--dry-run');
            assert.include(shown.output, `Resolved the current version as ${latest.version} from the remote registry`, `[${NX.registry}; ${MSBUILD.search}]`);
        }),
        bounds.test,
    );
});
