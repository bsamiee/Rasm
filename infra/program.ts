// --- [IMPORTS] -------------------------------------------------------------------------

import {
    ActionsRepositoryPermissions,
    type ActionsRepositoryPermissionsArgs,
    Repository,
    type RepositoryArgs,
    RepositoryRuleset,
    type RepositoryRulesetArgs,
    RepositoryVulnerabilityAlerts,
    type RepositoryVulnerabilityAlertsArgs,
} from '@pulumi/github';
import type { CustomResourceOptions } from '@pulumi/pulumi';
import { BranchConfig, Environment, Project, type ProjectArgs } from '@pulumiverse/doppler';
import { Effect, Record } from 'effect';

// --- [DOPPLER] -------------------------------------------------------------------------

const _PROJECT = { name: 'rasm', description: 'Repository and service secrets' } as const satisfies ProjectArgs;

const _ENVIRONMENTS = { dev: 'Development', prd: 'Production' } as const;

const _BRANCH_CONFIGS = { dev_repo: 'dev' } as const satisfies Record<`${keyof typeof _ENVIRONMENTS}_${string}`, keyof typeof _ENVIRONMENTS>;

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

const _VULNERABILITY_ALERTS = { enabled: true } as const satisfies Omit<RepositoryVulnerabilityAlertsArgs, 'repository'>;

const _RULESETS = {
    main: {
        target: 'branch',
        enforcement: 'active',
        conditions: { refName: { includes: ['~DEFAULT_BRANCH'], excludes: [] } },
        rules: {
            deletion: true,
            nonFastForward: true,
            requiredStatusChecks: { requiredChecks: [{ context: 'required' }], strictRequiredStatusChecksPolicy: false },
        },
        bypassActors: [{ actorType: 'RepositoryRole', actorId: 5, bypassMode: 'always' }],
    },
} as const satisfies Record<string, Omit<RepositoryRulesetArgs, 'repository' | 'name'>>;

const _ACTIONS_PERMISSIONS = {
    allowedActions: 'selected',
    allowedActionsConfig: {
        githubOwnedAllowed: true,
        patternsAlloweds: ['nrwl/nx-set-shas@*', 'jdx/mise-action@*'],
    },
} as const satisfies Omit<ActionsRepositoryPermissionsArgs, 'repository'>;

// --- [PROGRAM] -------------------------------------------------------------------------

const program = (adopt: boolean): Effect.Effect<Record<string, unknown>> =>
    Effect.sync(() => {
        const adoption = (id: string): CustomResourceOptions => (adopt ? { import: id } : {});
        const project = new Project(_PROJECT.name, _PROJECT, adoption(_PROJECT.name));
        const environments = Record.map(
            _ENVIRONMENTS,
            (name, slug) => new Environment(slug, { project: project.name, slug, name }, adoption(`${_PROJECT.name}.${slug}`)).slug,
        );
        const branchConfigs = Record.map(
            _BRANCH_CONFIGS,
            (environment, name) =>
                new BranchConfig(
                    name,
                    { project: project.name, environment: environments[environment], name },
                    adoption(`${_PROJECT.name}.${environment}.${name}`),
                ).name,
        );
        const configs = { ...environments, ...branchConfigs };
        const repository = new Repository(_REPOSITORY.name, _REPOSITORY, { protect: true, ...adoption(_REPOSITORY.name) });
        const vulnerabilityAlerts = new RepositoryVulnerabilityAlerts(`${_REPOSITORY.name}-vulnerability-alerts`, {
            repository: repository.name,
            ..._VULNERABILITY_ALERTS,
        });
        const rulesets = Record.map(
            _RULESETS,
            (row, name) => new RepositoryRuleset(`${_REPOSITORY.name}-${name}`, { repository: repository.name, name, ...row }),
        );
        const actionsPermissions = new ActionsRepositoryPermissions(`${_REPOSITORY.name}-actions-permissions`, {
            repository: repository.name,
            ..._ACTIONS_PERMISSIONS,
        });
        return {
            repository: repository.fullName,
            configs: Record.keys(configs),
            vulnerabilityAlerts: vulnerabilityAlerts.enabled,
            rulesets: Record.keys(rulesets),
            allowedActions: actionsPermissions.allowedActions,
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { program };
