import { stderr, stdout } from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { ActionsRepositoryPermissions, Repository, type RepositoryArgs, RepositoryDependabotSecurityUpdates, RepositoryRuleset, RepositoryVulnerabilityAlerts } from '@pulumi/github';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { BranchConfig, type BranchConfigArgs, Environment, Project, type ProjectArgs } from '@pulumiverse/doppler';
import { Array, Config, Effect, Equal, FileSystem, Path, Record, Schema, Stdio } from 'effect';
import { parse } from 'yaml';

// --- [MODELS] --------------------------------------------------------------------------

const _Steps = Schema.Array(
    Schema.Struct({ uses: Schema.optionalKey(Schema.Union([Schema.TemplateLiteral(['./', Schema.String]), Schema.TemplateLiteralParser([Schema.String, '@', Schema.String])])) }),
);
const _Actions = Schema.Struct({
    workflows: Schema.Array(
        Schema.Struct({
            jobs: Schema.Record(Schema.String, Schema.Struct({ name: Schema.optionalKey(Schema.String), needs: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)), steps: _Steps })),
        }),
    ),
    actions: Schema.Array(Schema.Struct({ runs: Schema.Struct({ steps: _Steps }) })),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const _program = ({ workflows, actions }: typeof _Actions.Type): Record.ReadonlyRecord<string, unknown> => {
    const patternsAlloweds = [...workflows.flatMap(({ jobs }) => Record.values(jobs).flatMap(({ steps }) => steps)), ...actions.flatMap(({ runs }) => runs.steps)].flatMap(({ uses }) =>
        typeof uses === 'object' ? [`${uses[0]}@*`] : [],
    );
    const requiredChecks = workflows.flatMap(({ jobs }) =>
        Record.toEntries(jobs).flatMap(([id, { name, needs }]) => (needs !== undefined && Equal.equals(Array.difference(Record.keys(jobs), needs), [id]) ? [{ context: name ?? id }] : [])),
    );
    const projectArgs = { name: 'rasm', description: 'Repository and service secrets' } as const satisfies ProjectArgs;
    const repositoryArgs = {
        name: 'Rasm',
        description: 'Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.',
        allowAutoMerge: true,
        allowMergeCommit: false,
        allowUpdateBranch: true,
        deleteBranchOnMerge: true,
        squashMergeCommitTitle: 'PR_TITLE',
        squashMergeCommitMessage: 'PR_BODY',
        hasIssues: true,
        securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
    } as const satisfies RepositoryArgs;
    const project = new Project(projectArgs.name, projectArgs);
    const environments = Record.map({ dev: 'Development', prd: 'Production' } as const, (name, slug) => new Environment(slug, { project: project.name, slug, name }).slug);
    const branchArgs = { name: 'dev_repo', project: project.name, environment: environments.dev } as const satisfies BranchConfigArgs;
    const repository = new Repository(repositoryArgs.name, repositoryArgs, { protect: true });
    const vulnerabilityAlerts = new RepositoryVulnerabilityAlerts(`${repositoryArgs.name}-vulnerability-alerts`, { repository: repository.name });
    return {
        repository: repository.fullName,
        configs: [...Record.values(environments), new BranchConfig(branchArgs.name, branchArgs).name],
        securityUpdates: new RepositoryDependabotSecurityUpdates(`${repositoryArgs.name}-security-updates`, { repository: vulnerabilityAlerts.repository, enabled: true }).enabled,
        ruleset: new RepositoryRuleset(`${repositoryArgs.name}-main`, {
            repository: repository.name,
            name: 'main',
            target: 'branch',
            enforcement: 'active',
            conditions: { refName: { includes: ['~DEFAULT_BRANCH'], excludes: [] } },
            rules: { deletion: true, nonFastForward: true, requiredStatusChecks: { requiredChecks } },
            bypassActors: [{ actorType: 'RepositoryRole', actorId: 5, bypassMode: 'always' }],
        }).name,
        allowedActions: new ActionsRepositoryPermissions(`${repositoryArgs.name}-actions-permissions`, {
            repository: repository.name,
            allowedActions: 'selected',
            allowedActionsConfig: { githubOwnedAllowed: false, patternsAlloweds },
        }).allowedActions,
    };
};

Effect.gen(function* () {
    const [fs, path, stdio, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Stdio.Stdio, Config.String('PULUMI_HOME')]);
    const [operation] = yield* stdio.args.pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Literals(['up', 'refresh'])]))));
    const workDir = path.join(home, 'work');
    yield* fs.makeDirectory(workDir, { recursive: true });
    const github = path.join(import.meta.dirname, '..', '.github');
    const actions = yield* Effect.all(
        Record.map(
            { workflows: 'workflows/*', actions: 'actions/*/action.yml' },
            Effect.fnUntraced(function* (pattern: string) {
                const files = yield* fs.glob(pattern, { root: github });
                return yield* Effect.forEach(files, (file) => Effect.map(fs.readFileString(path.join(github, file)), parse), { concurrency: 'unbounded' });
            }),
        ),
        { concurrency: 'unbounded' },
    ).pipe(Effect.flatMap(Schema.decodeUnknownEffect(_Actions)));
    yield* Effect.tryPromise(async (signal) =>
        (await LocalWorkspace.createOrSelectStack({ stackName: 'rasm', projectName: 'rasm-infra', program: async () => _program(actions) }, { workDir }))[operation]({
            onOutput: (text) => stdout.write(text),
            onError: (text) => stderr.write(text),
            signal,
        }),
    );
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
