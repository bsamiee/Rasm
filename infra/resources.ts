// Typed declarations of the GitHub repository settings and the Doppler project that program.ts registers

// --- [IMPORTS] -------------------------------------------------------------------------

import type { ActionsSecretArgs, RepositoryArgs } from '@pulumi/github';
import type { EnvironmentArgs, ProjectArgs, ServiceTokenArgs } from '@pulumiverse/doppler';

// --- [TYPES] ---------------------------------------------------------------------------

// An imported row takes the `import` resource option under the --import flag on its first up
type _Imported = { readonly imported: boolean };

// --- [DOPPLER] -------------------------------------------------------------------------

// Import ids: the project by name, an environment by `project.slug`, a branch config by `project.environment.name`
const _project = {
    name: 'rasm',
    description: 'Rasm repo and service secrets',
    imported: true,
} as const satisfies ProjectArgs & _Imported;

const _environments = [
    { slug: 'dev', name: 'Development', imported: true },
    { slug: 'prd', name: 'Production', imported: true },
] as const satisfies ReadonlyArray<Omit<EnvironmentArgs, 'project'> & _Imported>;

type _EnvironmentSlug = (typeof _environments)[number]['slug'];

// Doppler names a branch config `<environment>_<suffix>`, and every root config takes the slug of its environment
const _branchConfigs = [{ environment: 'dev', name: 'dev_repo', imported: true }] as const satisfies ReadonlyArray<
    { [E in _EnvironmentSlug]: { readonly environment: E; readonly name: `${E}_${string}` } }[_EnvironmentSlug] & _Imported
>;

type _ConfigName = _EnvironmentSlug | (typeof _branchConfigs)[number]['name'];

// Service tokens are created by the program, and a read token takes the `-readonly` suffix
const _serviceTokens = [{ config: 'dev_repo', name: 'rasm-ci-readonly', access: 'read' }] as const satisfies ReadonlyArray<
    Omit<ServiceTokenArgs, 'project'> & { readonly config: _ConfigName } & (
            | { readonly name: `${string}-readonly`; readonly access: 'read' }
            | { readonly access: 'read/write' }
        )
>;

type _ServiceTokenName = (typeof _serviceTokens)[number]['name'];

// --- [GITHUB] --------------------------------------------------------------------------

const _owner = 'bsamiee';

// Every live setting is an input, `visibility` stays computed, and the deprecated inputs stay unset
const _repository = {
    name: 'Rasm',
    imported: true,
    settings: {
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
    },
} as const satisfies { readonly name: string; readonly settings: Omit<RepositoryArgs, 'name'> } & _Imported;

// An Actions secret holds the key of a declared service token, and the value passes through no person and no file
const _actionsSecrets = [{ secretName: 'DOPPLER_TOKEN', serviceToken: 'rasm-ci-readonly' }] as const satisfies ReadonlyArray<
    Omit<ActionsSecretArgs, 'repository' | 'value'> & { readonly serviceToken: _ServiceTokenName }
>;

// --- [RESOURCES] -----------------------------------------------------------------------

const Resources = {
    project: _project,
    environments: _environments,
    branchConfigs: _branchConfigs,
    serviceTokens: _serviceTokens,
    owner: _owner,
    repository: _repository,
    actionsSecrets: _actionsSecrets,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Resources };
