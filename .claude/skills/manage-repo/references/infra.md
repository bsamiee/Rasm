# [INFRA]

## [01]-[PROGRAM]

- Every resource outside the tree and every repository setting is a typed row of one program
- Program reads credentials from the environment the secret store injects around the command
- Rows are `as const satisfies` their provider's argument type
- Row table is the whole declaration
- Existing resources are adopted through import on first run
- Missing resources are created
- Loose YAML, JSON, or shell files defining a resource are rows placed outside the program, move them into the program

## [02]-[WORKFLOWS]

- Workflows hold their trigger, permissions, and jobs
- Each job runs the setup step and task graph commands
- Setup step installs the tool manager, restores package caches, and installs dependencies
- CI runs target `check` on the root and the affected projects
- Format job proves the writers changed nothing
- One fan-in job with `needs` over every job and `if: always()` is the status check the merge rule names
- Skipped jobs report success to a required check, the fan-in fails on a `needs.<job>.result` of `skipped` or `failure`
- Reusable workflows, matrices, and composite actions arrive with the second job that needs them
- Hosted runs prove workflows
- Local runners with their images and daemon are second hosts, delete them

## [03]-[SECRETS]

- Secrets reach a workflow through the runner environment
- Store token is the one repository secret
- Non-secret values a workflow reads are variable rows of the program
- Secrets in a file, checked-in tokens, and hooks restoring secret text are leaks, remove them

## [04]-[TIMING]

- Release environments, deployment policies, tag rulesets, registry logins, and attestations join with the first library release
- Preview, plan, and dry-run variants of the apply target are duplicates, delete them, the apply target prints its own change summary
