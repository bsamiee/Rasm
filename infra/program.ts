import { ActionsRepositoryPermissions, Repository, type RepositoryArgs, RepositoryDependabotSecurityUpdates, RepositoryRuleset, RepositoryVulnerabilityAlerts } from '@pulumi/github';
import type { CustomResourceOptions } from '@pulumi/pulumi';
import { BranchConfig, Environment, Project, type ProjectArgs } from '@pulumiverse/doppler';
import { Array, Equal, Record, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Steps = Schema.Array(
    Schema.Struct({ uses: Schema.optionalKey(Schema.Union([Schema.TemplateLiteral(['./', Schema.String]), Schema.TemplateLiteralParser([Schema.String, '@', Schema.String])])) }),
);
const Text = Schema.optionalKey(Schema.String);
const GitHubOwned = Schema.TemplateLiteral([Schema.Literals(['actions', 'github']), '/', Schema.String]);
const Job = Schema.Struct({ name: Text, needs: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)), if: Text, steps: Steps });
const Actions = Schema.Struct({
    workflows: Schema.Array(Schema.Struct({ jobs: Schema.Record(Schema.String, Job) })),
    actions: Schema.Array(Schema.Struct({ runs: Schema.Struct({ steps: Steps }) })),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const program = (imports: boolean, github: typeof Actions.Type): Record.ReadonlyRecord<string, unknown> => {
    const uses = [...github.workflows.flatMap(({ jobs }) => Record.values(jobs).flatMap(({ steps }) => steps)), ...github.actions.flatMap(({ runs }) => runs.steps)].flatMap((step) =>
        step.uses === undefined || typeof step.uses === 'string' ? [] : [step.uses[0]],
    );
    const patternsAlloweds = Array.dedupe(uses.flatMap((action) => (Schema.is(GitHubOwned)(action) ? [] : [`${action}@*`])));
    const requiredChecks = github.workflows.flatMap(({ jobs }) =>
        Record.toEntries(jobs).flatMap(([id, { name, needs, if: condition }]) =>
            condition === 'always()' && needs !== undefined && Equal.equals(Array.difference(Record.keys(jobs), needs), [id]) ? [{ context: name ?? id }] : [],
        ),
    );
    const projectArgs = { name: 'rasm', description: 'Repository and service secrets' } as const satisfies ProjectArgs;
    const repositoryArgs = {
        name: 'Rasm',
        description: 'AEC/design-geometry workspace',
        archiveOnDestroy: true,
        allowAutoMerge: true,
        allowMergeCommit: false,
        allowUpdateBranch: true,
        deleteBranchOnMerge: true,
        squashMergeCommitTitle: 'PR_TITLE',
        squashMergeCommitMessage: 'PR_BODY',
        hasIssues: true,
        securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
    } as const satisfies RepositoryArgs;
    const options = (id: string): CustomResourceOptions => (imports ? { import: id } : {});
    const project = new Project(projectArgs.name, projectArgs, options(projectArgs.name));
    const environments = Record.map(
        { dev: 'Development', prd: 'Production' } as const,
        (name, slug) => new Environment(slug, { project: project.name, slug, name }, options(`${projectArgs.name}.${slug}`)).slug,
    );
    const branches = { dev: ['repo'] } as const satisfies Partial<Record<keyof typeof environments, readonly string[]>>;
    const repository = new Repository(repositoryArgs.name, repositoryArgs, { protect: true, ...options(repositoryArgs.name) });
    const vulnerabilityAlerts = new RepositoryVulnerabilityAlerts(`${repositoryArgs.name}-vulnerability-alerts`, { repository: repository.name });
    return {
        repository: repository.fullName,
        configs: [
            ...Record.values(environments),
            ...Record.toEntries(branches).flatMap(([environment, suffixes]) =>
                suffixes.map(
                    (suffix) =>
                        new BranchConfig(
                            `${environment}_${suffix}`,
                            { project: project.name, environment: environments[environment], name: `${environment}_${suffix}` },
                            options(`${projectArgs.name}.${environment}.${environment}_${suffix}`),
                        ).name,
                ),
            ),
        ],
        vulnerabilityAlerts: vulnerabilityAlerts.enabled,
        securityUpdates: new RepositoryDependabotSecurityUpdates(`${repositoryArgs.name}-security-updates`, { repository: vulnerabilityAlerts.repository, enabled: true }).enabled,
        rulesets: [
            new RepositoryRuleset(`${repositoryArgs.name}-main`, {
                repository: repository.name,
                name: 'main',
                target: 'branch',
                enforcement: 'active',
                conditions: { refName: { includes: ['~DEFAULT_BRANCH'], excludes: [] } },
                rules: { deletion: true, nonFastForward: true, requiredStatusChecks: { requiredChecks } },
                bypassActors: [{ actorType: 'RepositoryRole', actorId: 5, bypassMode: 'always' }],
            }).name,
        ],
        allowedActions: new ActionsRepositoryPermissions(`${repositoryArgs.name}-actions-permissions`, {
            repository: repository.name,
            allowedActions: 'selected',
            allowedActionsConfig: { githubOwnedAllowed: true, patternsAlloweds },
        }).allowedActions,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Actions, program };
