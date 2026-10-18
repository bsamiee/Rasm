import { NodeServices } from '@effect/platform-node';
import { ActionsRepositoryPermissions, IssueLabel, type IssueLabelArgs, Repository, RepositoryDependabotSecurityUpdates, RepositoryVulnerabilityAlerts, WorkflowRepositoryPermissions } from '@pulumi/github';
import { Effect, Path, Record } from 'effect';
import { stdout } from '../eng/typescript/process.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const stack = { projectName: 'rasm-infra', stackName: 'rasm' };

const program = Effect.fnUntraced(
    function* () {
        const [path, url] = yield* Effect.all([Path.Path, stdout('git', ['remote', 'get-url', 'origin'], { cwd: import.meta.dirname })]);
        const name = path.basename(url.trim(), '.git');
        const { name: repository, fullName } = new Repository(
            name,
            {
                name,
                description: 'Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.',
                visibility: 'public',
                topics: [],
                hasIssues: true,
                webCommitSignoffRequired: false,
                allowAutoMerge: true,
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
            securityUpdates: new RepositoryDependabotSecurityUpdates(`${name}-security-updates`, { repository: new RepositoryVulnerabilityAlerts(`${name}-vulnerability-alerts`, { repository }).repository, enabled: true }).enabled,
            allowedActions: new ActionsRepositoryPermissions(`${name}-actions-permissions`, { repository, allowedActions: 'all' }).allowedActions,
            workflowPermissions: new WorkflowRepositoryPermissions(`${name}-workflow-permissions`, { repository, defaultWorkflowPermissions: 'read', canApprovePullRequestReviews: false }).defaultWorkflowPermissions,
            labels: Record.map(
                { bug: { color: 'd73a4a', description: "Something isn't working" }, enhancement: { color: 'a2eeef', description: 'New feature or request' } } satisfies Record.ReadonlyRecord<string, Omit<IssueLabelArgs, 'name' | 'repository'>>,
                (args, label) => new IssueLabel(`${name}-${label}`, { ...args, name: label, repository }).name,
            ),
        };
    },
    Effect.provide(NodeServices.layer),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { program, stack };
