import type { ExecutorContext } from '@nx/devkit';
import { ActionsRepositoryPermissions, ActionsSecret, IssueLabels, Repository, RepositoryDependabotSecurityUpdates, RepositoryDeployKey, RepositoryEnvironment, RepositoryRuleset, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { PrivateKey } from '@pulumi/tls';
import { Cause, Effect, FileSystem, Path, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ description: Schema.String, repository: Schema.Struct({ url: Schema.String }) }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = (context: ExecutorContext) => ({
    projectName: 'rasm-infra',
    stackName: 'rasm',
    program: Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [{ description, repository: origin }, pattern] = yield* Effect.all([
            Effect.flatMap(fs.readFileString(path.join(context.root, 'package.json')), Schema.decodeEffect(Manifest)),
            Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern).pipe(Effect.mapError(() => new Cause.NoSuchElementError('nx.json states no release.releaseTag.pattern for the release tag ruleset'))),
        ]);
        const name = path.basename(origin.url, '.git');
        const { name: repository, fullName } = new Repository(
            name,
            {
                name,
                description,
                visibility: 'public',
                hasIssues: true,
                allowMergeCommit: false,
                allowUpdateBranch: true,
                deleteBranchOnMerge: true,
                squashMergeCommitTitle: 'PR_TITLE',
                squashMergeCommitMessage: 'PR_BODY',
                securityAndAnalysis: { secretScanning: { status: 'enabled' }, secretScanningPushProtection: { status: 'enabled' } },
            },
            { protect: true },
        );
        const assets = new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true });
        const assetsKey = new PrivateKey(`${name}-assets-deploy`, { algorithm: 'ED25519' });
        return {
            repository: fullName,
            assets: assets.fullName,
            assetsDeployKey: new RepositoryDeployKey(`${name}-assets-deploy`, { repository: assets.name, title: `${name} CI`, key: assetsKey.publicKeyOpenssh, readOnly: true }).title,
            assetsSecret: new ActionsSecret(`${name}-assets-deploy-key`, { repository, secretName: 'ASSETS_DEPLOY_KEY', value: assetsKey.privateKeyOpenssh }).secretName,
            securityUpdates: new RepositoryDependabotSecurityUpdates(`${name}-security-updates`, { repository: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository }).repository, enabled: false }).enabled,
            allowedActions: new ActionsRepositoryPermissions(`${name}-actions-permissions`, { repository, allowedActions: 'all' }).allowedActions,
            workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'read' }).defaultWorkflowPermissions,
            environments: ['release', 'deploy'].map((environment) => new RepositoryEnvironment(`${name}-${environment}`, { repository, environment }).environment),
            labels: new IssueLabels(`${name}-labels`, {
                repository,
                labels: [
                    { name: 'bug', color: 'd73a4a', description: 'Broken behavior' },
                    { name: 'enhancement', color: 'a2eeef', description: 'Feature request' },
                ],
            }).labels,
            releaseTags: new RepositoryRuleset(`${name}-release-tags`, {
                repository,
                name: 'release tags',
                target: 'tag',
                enforcement: 'active',
                conditions: { refName: { includes: [`refs/tags/${pattern.replaceAll(/\{\w+\}/gu, '**')}`], excludes: [] } },
                rules: { update: true, deletion: true, nonFastForward: true },
            }).rulesetId,
        };
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { repositoryStack };
