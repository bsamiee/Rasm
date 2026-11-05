import { readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { findMatchingProjects, readNxJsonFromDisk } from '@nx/devkit/internal';
import { ActionsRepositoryPermissions, IssueLabel, type IssueLabelArgs, Release, Repository, RepositoryDependabotSecurityUpdates, RepositoryRuleset, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { Effect, FileSystem, Path, Record } from 'effect';
import { runCommand } from '#/eng/typescript/run-command';

// --- [COMPOSITION] ---------------------------------------------------------------------

const repositoryStack = {
    projectName: 'rasm-infra',
    select: 'selectStack' as const,
    stackName: 'rasm',
    program: Effect.gen(function* () {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [url, readme, graph, nx] = yield* Effect.all([runCommand('git', ['remote', 'get-url', 'origin'], { cwd: workspaceRoot }), fs.readFileString(path.join(workspaceRoot, 'README.md')), Effect.try(readCachedProjectGraph), Effect.try(readNxJsonFromDisk)], { concurrency: 'unbounded' });
        const [pattern, paragraph] = yield* Effect.all([Effect.fromNullishOr(nx.release?.releaseTag?.pattern), Effect.fromNullishOr(readme.split('\n\n').find((block) => !block.startsWith('#')))]);
        const feeds = findMatchingProjects(['tag:output:appcast', 'tag:output:extension'], graph.nodes);
        const name = path.basename(url.trim(), '.git');
        const { name: repository, fullName } = new Repository(
            name,
            {
                name,
                description: paragraph.replaceAll('\n', ' ').trim(),
                visibility: 'public',
                topics: [],
                hasIssues: true,
                webCommitSignoffRequired: false,
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
            assets: new Repository(`${name}-Assets`, { name: `${name}-Assets`, description: `Binary assets ${name} holds as a private submodule`, visibility: 'private', topics: [], hasIssues: false, allowMergeCommit: false, deleteBranchOnMerge: true }, { protect: true }).fullName,
            securityUpdates: new RepositoryDependabotSecurityUpdates(`${name}-security-updates`, { repository: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository }).repository, enabled: false }).enabled,
            allowedActions: new ActionsRepositoryPermissions(`${name}-actions-permissions`, { repository, allowedActions: 'all' }).allowedActions,
            workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'read', canApprovePullRequestReviews: false }).defaultWorkflowPermissions,
            labels: Record.map(
                { bug: { color: 'd73a4a', description: 'Broken behavior' }, enhancement: { color: 'a2eeef', description: 'Feature request' } } satisfies Record.ReadonlyRecord<string, Omit<IssueLabelArgs, 'name' | 'repository'>>,
                (args, label) => new IssueLabel(`${name}-${label}`, { ...args, name: label, repository }).name,
            ),
            releaseTags: new RepositoryRuleset(`${name}-release-tags`, {
                repository,
                name: 'release tags',
                target: 'tag',
                enforcement: 'active',
                conditions: { refName: { includes: [`refs/tags/${pattern.replaceAll(/\{\w+\}/gu, '*')}`], excludes: [] } },
                rules: { update: true, deletion: true, nonFastForward: true },
            }).rulesetId,
            updates: Record.fromIterableWith(feeds, (project) => [project, new Release(`${name}-${project}-updates`, { repository, tagName: `${project}-updates`, name: `${project} updates`, draft: false }, { protect: true }).htmlUrl]),
        };
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { repositoryStack };
