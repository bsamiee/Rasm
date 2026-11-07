import { matchesGlob } from 'node:path';
import type { Tree } from '@nx/devkit';
import { Effect, Match, Predicate, Schema } from 'effect';
import { VersionActions } from 'nx/release';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE_LABEL = /\d+(?:\.\d+){2,3}-/u;
const PRERELEASE_BUMPS: Readonly<Record<string, string>> = { major: 'premajor', minor: 'preminor', patch: 'prepatch' };

// --- [MODELS] --------------------------------------------------------------------------

const LockEntry = Schema.Union([
    Schema.Struct({ type: Schema.Literal('Direct'), resolved: Schema.String }),
    Schema.Struct({ type: Schema.Literal('Project'), dependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }),
    Schema.Struct({ type: Schema.Literals(['Transitive', 'CentralTransitive']) }),
]);
const NuGetLock = Schema.fromJsonString(Schema.Struct({ dependencies: Schema.Record(Schema.String, Schema.Record(Schema.String, LockEntry)) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const published = Match.type<typeof LockEntry.Type>().pipe(Match.discriminatorsExhaustive('type')({ Direct: ({ resolved }) => [resolved], Project: ({ dependencies }) => Object.values(dependencies), Transitive: () => [], CentralTransitive: () => [] }));

// --- [COMPOSITION] ---------------------------------------------------------------------

class TagVersionActions extends VersionActions {
    override readonly validManifestFilenames = null;
    held = false;

    override init(tree: Tree) {
        const { root } = this.projectGraphNode.data;
        const locks = tree.children(root).filter((file) => matchesGlob(file, 'packages*.lock.json'));
        return Effect.runPromise(Effect.forEach(locks, (file) => Schema.decodeEffect(NuGetLock)(tree.read(`${root}/${file}`, 'utf-8') ?? ''))).then((decoded) => {
            const entries = decoded.flatMap(({ dependencies }) => Object.values(dependencies).flatMap(Object.values));
            this.held = entries.flatMap(published).some((version) => RELEASE_LABEL.test(version));
            return super.init(tree);
        });
    }

    override calculateNewVersion(currentVersion: string | null, newVersionInput: string, newVersionInputReason: string, newVersionInputReasonData: Record<string, unknown>, preid: string) {
        const [input, label] = Match.value({ declared: this.finalConfigForProject.versionActionsOptions?.preid, held: this.held }).pipe(
            Match.when({ declared: Predicate.isString, held: true }, ({ declared }) => [PRERELEASE_BUMPS[newVersionInput] ?? newVersionInput, declared] as const),
            Match.when({ declared: Predicate.isString }, () => [newVersionInput === 'prerelease' ? 'patch' : newVersionInput, preid] as const),
            Match.orElse(() => [newVersionInput, preid] as const),
        );
        return super.calculateNewVersion(currentVersion, input, newVersionInputReason, newVersionInputReasonData, label);
    }

    override readCurrentVersionFromSourceManifest() {
        return Promise.resolve({ currentVersion: '0.0.0', manifestPath: '' });
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion() {
        return Promise.resolve([]);
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default TagVersionActions;
