// Pulumi resources for repository settings and Doppler configuration, and no workflow reads a Doppler secret

// --- [IMPORTS] -------------------------------------------------------------------------

import {
    ActionsRepositoryPermissions,
    type ActionsRepositoryPermissionsArgs,
    ActionsVariable,
    Repository,
    type RepositoryArgs,
    RepositoryEnvironment,
    type RepositoryEnvironmentArgs,
    RepositoryEnvironmentDeploymentPolicy,
    type RepositoryEnvironmentDeploymentPolicyArgs,
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

// Doppler names a branch config <environment>_<suffix>
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
    // Public repositories hold advanced security on and take no advancedSecurity block
    securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
} as const satisfies RepositoryArgs;

// Dependabot security updates stay off because the upgrade target moves every dependency to its newest release
const _VULNERABILITY_ALERTS = { enabled: true } as const satisfies Omit<RepositoryVulnerabilityAlertsArgs, 'repository'>;

const _RULESETS = {
    // The admin role bypass keeps the owner's direct pushes open while pull requests wait for the required check
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
    // Tag creation stays open for nx release under the workflow token
    releases: {
        target: 'tag',
        enforcement: 'active',
        conditions: { refName: { includes: ['refs/tags/*@*'], excludes: [] } },
        rules: { deletion: true, update: true, nonFastForward: true },
    },
} as const satisfies Record<string, Omit<RepositoryRulesetArgs, 'repository' | 'name'>>;

// Each environment takes custom branch policies and the one policy naming the branch its jobs deploy from
const _DEPLOYMENT_ENVIRONMENTS = {
    release: { deploymentBranchPolicy: { protectedBranches: false, customBranchPolicies: true }, policy: { branchPattern: 'main' } },
} as const satisfies Record<
    string,
    Omit<RepositoryEnvironmentArgs, 'repository' | 'environment'> & {
        policy: Omit<RepositoryEnvironmentDeploymentPolicyArgs, 'repository' | 'environment'>;
    }
>;

// The allow list names every action outside GitHub's own that the workflows use, the supply-chain control in place of digest pins
const _ACTIONS_PERMISSIONS = {
    allowedActions: 'selected',
    allowedActionsConfig: {
        githubOwnedAllowed: true,
        patternsAlloweds: ['nrwl/nx-set-shas@*', 'jdx/mise-action@*', 'NuGet/login@*'],
    },
} as const satisfies Omit<ActionsRepositoryPermissionsArgs, 'repository'>;

// Variables the workflows read as vars.<NAME>, the entry reads each value from the environment under the same name
const ACTIONS_VARIABLES = ['NUGET_USER'] as const;

// --- [PROGRAM] -------------------------------------------------------------------------

// Default providers read DOPPLER_TOKEN and GITHUB_TOKEN, and GitHub detects the owner from its token
const program = (adopt: boolean, variables: Record<string, string>): Effect.Effect<Record<string, unknown>> =>
    Effect.sync(() => {
        // Import existing projects, environments, configs, and repositories, and create every other row
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
        const actionsVariables = Record.map(
            variables,
            (value, variableName) => new ActionsVariable(`${_REPOSITORY.name}-${variableName}`, { repository: repository.name, variableName, value }),
        );
        const vulnerabilityAlerts = new RepositoryVulnerabilityAlerts(`${_REPOSITORY.name}-vulnerability-alerts`, {
            repository: repository.name,
            ..._VULNERABILITY_ALERTS,
        });
        const rulesets = Record.map(
            _RULESETS,
            (row, name) => new RepositoryRuleset(`${_REPOSITORY.name}-${name}`, { repository: repository.name, name, ...row }),
        );
        const deploymentEnvironments = Record.map(
            _DEPLOYMENT_ENVIRONMENTS,
            ({ policy, ...row }, environment) =>
                new RepositoryEnvironmentDeploymentPolicy(`${_REPOSITORY.name}-${environment}`, {
                    repository: repository.name,
                    environment: new RepositoryEnvironment(`${_REPOSITORY.name}-${environment}`, { repository: repository.name, environment, ...row })
                        .environment,
                    ...policy,
                }),
        );
        const actionsPermissions = new ActionsRepositoryPermissions(`${_REPOSITORY.name}-actions-permissions`, {
            repository: repository.name,
            ..._ACTIONS_PERMISSIONS,
        });
        return {
            repository: repository.fullName,
            configs: Record.keys(configs),
            actionsVariables: Record.keys(actionsVariables),
            vulnerabilityAlerts: vulnerabilityAlerts.enabled,
            rulesets: Record.keys(rulesets),
            deploymentEnvironments: Record.keys(deploymentEnvironments),
            allowedActions: actionsPermissions.allowedActions,
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { ACTIONS_VARIABLES, program };
