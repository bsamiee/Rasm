import { readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { findMatchingProjects, readNxJsonFromDisk } from '@nx/devkit/internal';
import { ActionsRepositoryPermissions, ActionsSecret, IssueLabels, Release, Repository, RepositoryDependabotSecurityUpdates, RepositoryRuleset, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { secret } from '@pulumi/pulumi';
import { Config, Effect, FileSystem, Path, Record, Redacted } from 'effect';
import { Package } from '#/eng/typescript/manifest';

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = {
    projectName: 'rasm-infra',
    stackName: 'rasm',
    program: Effect.gen(function* () {
        const secretName = 'SIXLABORSLICENSEKEY';
        const [fs, path, license] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.Redacted(secretName)]);
        const [manifest, readme, graph, nx] = yield* Effect.all([Package.read(workspaceRoot), fs.readFileString(path.join(workspaceRoot, 'README.md')), Effect.try(readCachedProjectGraph), Effect.try(readNxJsonFromDisk)], { concurrency: 'unbounded' });
        const [url, pattern, paragraph] = yield* Effect.all([Effect.fromNullishOr(manifest.repository), Effect.fromNullishOr(nx.release?.releaseTag?.pattern), Effect.fromNullishOr(readme.split('\n\n').find((block) => !block.startsWith('#')))]);
        const feeds = findMatchingProjects(['tag:output:appcast', 'tag:output:extension'], graph.nodes);
        const name = path.basename(url, '.git');
        const { name: repository, fullName } = new Repository(
            name,
            {
                name,
                description: paragraph.replaceAll('\n', ' ').trim(),
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
        return {
            repository: fullName,
            assets: new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true }).fullName,
            securityUpdates: new RepositoryDependabotSecurityUpdates(`${name}-security-updates`, { repository: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository }).repository, enabled: false }).enabled,
            allowedActions: new ActionsRepositoryPermissions(`${name}-actions-permissions`, { repository, allowedActions: 'all' }).allowedActions,
            workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'read' }).defaultWorkflowPermissions,
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
                conditions: { refName: { includes: [`refs/tags/${pattern.replaceAll(/\{\w+\}/gu, '*')}`], excludes: [] } },
                rules: { update: true, deletion: true, nonFastForward: true },
            }).rulesetId,
            license: new ActionsSecret(`${name}-sixlabors-license`, { repository, secretName, value: secret(Redacted.value(license)) }).secretName,
            updates: Record.fromIterableWith(feeds, (feed) => [feed, new Release(`${name}-${feed}-updates`, { repository, tagName: `${feed}-updates`, name: `${feed} updates`, draft: false }, { protect: true }).htmlUrl]),
        };
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { repositoryStack };
