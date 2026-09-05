// Inline Pulumi program that declares the Doppler project and the GitHub repository settings, registers them, and adopts the live ones under --import

// --- [IMPORTS] -------------------------------------------------------------------------

import { ActionsSecret, Repository, type RepositoryArgs } from '@pulumi/github';
import type { CustomResourceOptions } from '@pulumi/pulumi';
import { BranchConfig, Environment, Project, type ProjectArgs, ServiceToken } from '@pulumiverse/doppler';
import { Effect, Option, Record } from 'effect';

// --- [DOPPLER] -------------------------------------------------------------------------

const _PROJECT = { name: 'rasm', description: 'Repository and service secrets' } as const satisfies ProjectArgs;

const _ENVIRONMENTS = { dev: 'Development', prd: 'Production' } as const;

// Doppler names a branch config <environment>_<suffix>
const _BRANCH_CONFIGS = { dev_repo: 'dev' } as const satisfies Record<`${keyof typeof _ENVIRONMENTS}_${string}`, keyof typeof _ENVIRONMENTS>;

const _SERVICE_TOKENS = { 'rasm-ci-readonly': { config: 'dev_repo', access: 'read' } } as const satisfies Record<
    string,
    { readonly config: keyof typeof _ENVIRONMENTS | keyof typeof _BRANCH_CONFIGS; readonly access: 'read' | 'read/write' }
>;

// --- [GITHUB] --------------------------------------------------------------------------

const _REPOSITORY = {
    name: 'Rasm',
    description: 'AEC/design-geometry workspace',
    archived: false,
    archiveOnDestroy: true,
    allowAutoMerge: true,
    allowMergeCommit: false,
    allowRebaseMerge: true,
    allowSquashMerge: true,
    allowUpdateBranch: true,
    deleteBranchOnMerge: true,
    mergeCommitTitle: 'MERGE_MESSAGE',
    mergeCommitMessage: 'PR_TITLE',
    squashMergeCommitTitle: 'PR_TITLE',
    squashMergeCommitMessage: 'PR_BODY',
    hasIssues: true,
    hasProjects: false,
    hasWiki: false,
    hasDiscussions: false,
    webCommitSignoffRequired: false,
} as const satisfies RepositoryArgs;

const _ACTIONS_SECRETS = { DOPPLER_TOKEN: 'rasm-ci-readonly' } as const satisfies Record<string, keyof typeof _SERVICE_TOKENS>;

// --- [PROGRAM] -------------------------------------------------------------------------

// The default providers read DOPPLER_TOKEN and GITHUB_TOKEN from the environment, and the GitHub provider detects the owner from its token
const program = (adopt: boolean): Effect.Effect<Record<string, unknown>> =>
    Effect.sync(() => {
        // Every row with a live counterpart adopts it under --import in place of creating one, tokens and secrets are created
        const adoption = (id: string): CustomResourceOptions => Record.getSomes({ import: Option.liftPredicate(id, () => adopt) });
        const project = new Project(_PROJECT.name, _PROJECT, adoption(_PROJECT.name));
        const environments = Record.map(
            _ENVIRONMENTS,
            (name, slug) => new Environment(slug, { project: project.name, slug, name }, adoption(`${_PROJECT.name}.${slug}`)),
        );
        const branchConfigs = Record.map(
            _BRANCH_CONFIGS,
            (environment, name) =>
                new BranchConfig(
                    name,
                    { project: project.name, environment: environments[environment].slug, name },
                    adoption(`${_PROJECT.name}.${environment}.${name}`),
                ),
        );
        const configs = { ...Record.map(environments, (environment) => environment.slug), ...Record.map(branchConfigs, (config) => config.name) };
        const serviceTokens = Record.map(
            _SERVICE_TOKENS,
            (row, name) =>
                new ServiceToken(`${row.config}-${name}`, { project: project.name, config: configs[row.config], name, access: row.access }),
        );
        const repository = new Repository(_REPOSITORY.name, _REPOSITORY, { protect: true, ...adoption(_REPOSITORY.name) });
        const actionsSecrets = Record.map(
            _ACTIONS_SECRETS,
            (token, secretName) =>
                new ActionsSecret(`${_REPOSITORY.name}-${secretName}`, { repository: repository.name, secretName, value: serviceTokens[token].key }),
        );
        return {
            repository: repository.fullName,
            configs: Record.keys(configs),
            serviceTokens: Record.keys(serviceTokens),
            actionsSecrets: Record.keys(actionsSecrets),
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { program };
