// --- [IMPORTS] -------------------------------------------------------------------------

import { ActionsRepositoryPermissions, Repository, type RepositoryArgs, RepositoryDependabotSecurityUpdates, RepositoryRuleset, RepositoryVulnerabilityAlerts } from '@pulumi/github';
import type { CustomResourceOptions } from '@pulumi/pulumi';
import { BranchConfig, Environment, Project, type ProjectArgs } from '@pulumiverse/doppler';
import { Record } from 'effect';

// --- [DOPPLER] -------------------------------------------------------------------------

const _PROJECT = { name: 'rasm', description: 'Repository and service secrets' } as const satisfies ProjectArgs;

// --- [GITHUB] --------------------------------------------------------------------------

const _REPOSITORY = {
    name: 'Rasm',
    description: 'AEC/design-geometry workspace',
    visibility: 'public',
    archived: false,
    archiveOnDestroy: true,
    allowAutoMerge: true,
    allowMergeCommit: false,
    allowRebaseMerge: true,
    allowSquashMerge: true,
    allowUpdateBranch: true,
    deleteBranchOnMerge: true,
    squashMergeCommitTitle: 'PR_TITLE',
    squashMergeCommitMessage: 'PR_BODY',
    hasIssues: true,
    hasProjects: false,
    hasWiki: false,
    hasDiscussions: false,
    webCommitSignoffRequired: false,
    securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
} as const satisfies RepositoryArgs;

// --- [PROGRAM] -------------------------------------------------------------------------

const program = (adopt: boolean): Record.ReadonlyRecord<string, unknown> => {
    const adoption = (id: string): CustomResourceOptions => (adopt ? { import: id } : {});
    const project = new Project(_PROJECT.name, _PROJECT, adoption(_PROJECT.name));
    const environments = Record.map(
        { dev: 'Development', prd: 'Production' } as const,
        (name, slug) => new Environment(slug, { project: project.name, slug, name }, adoption(`${_PROJECT.name}.${slug}`)).slug,
    );
    const repository = new Repository(_REPOSITORY.name, _REPOSITORY, { protect: true, ...adoption(_REPOSITORY.name) });
    const vulnerabilityAlerts = new RepositoryVulnerabilityAlerts(`${_REPOSITORY.name}-vulnerability-alerts`, { repository: repository.name, enabled: true });
    return {
        repository: repository.fullName,
        configs: Record.keys({
            ...environments,
            ...Record.map(
                { dev_repo: 'dev' } as const satisfies { [K in `${keyof typeof environments}_${string}`]: K extends `${infer E}_${string}` ? E : never },
                (environment, name) => new BranchConfig(name, { project: project.name, environment: environments[environment], name }, adoption(`${_PROJECT.name}.${environment}.${name}`)).name,
            ),
        }),
        vulnerabilityAlerts: vulnerabilityAlerts.enabled,
        securityUpdates: new RepositoryDependabotSecurityUpdates(`${_REPOSITORY.name}-security-updates`, { repository: repository.name, enabled: true }, { dependsOn: vulnerabilityAlerts }).enabled,
        rulesets: [
            new RepositoryRuleset(`${_REPOSITORY.name}-main`, {
                repository: repository.name,
                name: 'main',
                target: 'branch',
                enforcement: 'active',
                conditions: { refName: { includes: ['~DEFAULT_BRANCH'], excludes: [] } },
                rules: { deletion: true, nonFastForward: true, requiredStatusChecks: { requiredChecks: [{ context: 'required' }], strictRequiredStatusChecksPolicy: false } },
                bypassActors: [{ actorType: 'RepositoryRole', actorId: 5, bypassMode: 'always' }],
            }).name,
        ],
        allowedActions: new ActionsRepositoryPermissions(`${_REPOSITORY.name}-actions-permissions`, {
            repository: repository.name,
            allowedActions: 'selected',
            allowedActionsConfig: { githubOwnedAllowed: true, patternsAlloweds: ['nrwl/nx-set-shas@*', 'jdx/mise-action@*'] },
        }).allowedActions,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { program };
