import { stderr, stdout } from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { ActionsRepositoryPermissions, Repository, type RepositoryArgs, RepositoryDependabotSecurityUpdates, RepositoryRuleset, RepositoryVulnerabilityAlerts } from '@pulumi/github';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { BranchConfig, type BranchConfigArgs, Environment, Project, type ProjectArgs, Secret, type SecretArgs } from '@pulumiverse/doppler';
import { Array, Config, Effect, Equal, FileSystem, Formatter, Path, Record, Redacted, Schema, Stdio } from 'effect';
import { parse } from 'yaml';

// --- [MODELS] --------------------------------------------------------------------------

const _Workflows = Schema.Array(
    Schema.Struct({
        jobs: Schema.Record(Schema.String, Schema.Struct({ name: Schema.optionalKey(Schema.String), needs: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)) })),
    }),
);
const _Operation = Schema.Literals(['up', 'refresh']);
const _Secrets = Config.all((['DOPPLER_TOKEN', 'GITHUB_TOKEN', 'PULUMI_ACCESS_TOKEN'] as const).map((name) => Config.map(Config.Redacted(name), (value) => ({ name, value }))));

// --- [ERRORS] --------------------------------------------------------------------------

const _Reason = Schema.TaggedUnion({
    Unselected: { cause: Schema.Defect() },
    Failed: { operation: _Operation, cause: Schema.Defect() },
});
class InfraError extends Schema.TaggedError<InfraError, { readonly brand: unique symbol }>()('InfraError', { reason: _Reason }) {
    override get message(): string {
        return _Reason.match(this.reason, {
            Unselected: ({ cause }) => `cannot create or select the Pulumi stack: ${Formatter.format(cause)}`,
            Failed: ({ operation, cause }) => `pulumi ${operation} failed: ${Formatter.format(cause)}`,
        });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const _program = (workflows: typeof _Workflows.Type, secrets: Config.Success<typeof _Secrets>): Record.ReadonlyRecord<string, unknown> => {
    const requiredChecks = workflows.flatMap(({ jobs }) => Record.toEntries(jobs).flatMap(([id, { name, needs }]) => (needs !== undefined && Equal.equals(Array.difference(Record.keys(jobs), needs), [id]) ? [{ context: name ?? id }] : [])));
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
    const configs = { ...environments, [branchArgs.name]: new BranchConfig(branchArgs.name, branchArgs).name };
    const repository = new Repository(repositoryArgs.name, repositoryArgs, { protect: true });
    const vulnerabilityAlerts = new RepositoryVulnerabilityAlerts(`${repositoryArgs.name}-vulnerability-alerts`, { repository: repository.name });
    return {
        repository: repository.fullName,
        configs: Record.values(configs),
        secrets: secrets.map(({ name, value }) => new Secret(`${projectArgs.name}.${branchArgs.name}.${name}`, { project: project.name, config: configs[branchArgs.name], name, value: Redacted.value(value) } satisfies SecretArgs).name),
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
            allowedActions: 'all',
        }).allowedActions,
    };
};

const infra = Effect.fn('infra')(function* () {
    const [fs, path, stdio, home, secrets] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Stdio.Stdio, Config.String('PULUMI_HOME'), _Secrets]);
    const [operation] = yield* stdio.args.pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([_Operation]))));
    const workDir = path.join(home, 'work');
    yield* fs.makeDirectory(workDir, { recursive: true });
    const workflowsDirectory = path.join(import.meta.dirname, '..', '.github', 'workflows');
    const workflows = yield* fs.glob('*', { root: workflowsDirectory }).pipe(
        Effect.flatMap((files) => Effect.forEach(files, (file) => Effect.map(fs.readFileString(path.join(workflowsDirectory, file)), parse), { concurrency: 'unbounded' })),
        Effect.flatMap(Schema.decodeUnknownEffect(_Workflows)),
    );
    const stack = yield* Effect.tryPromise({
        try: () => LocalWorkspace.createOrSelectStack({ stackName: 'rasm', projectName: 'rasm-infra', program: () => Promise.resolve(_program(workflows, secrets)) } satisfies InlineProgramArgs, { workDir }),
        catch: (cause) => InfraError.make({ reason: { _tag: 'Unselected', cause } }),
    });
    yield* Effect.tryPromise({
        try: (signal) => stack[operation]({ onOutput: (text) => stdout.write(text), onError: (text) => stderr.write(text), signal }),
        catch: (cause) => InfraError.make({ reason: { _tag: 'Failed', operation, cause } }),
    });
});

infra().pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
