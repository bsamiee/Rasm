# Agent Reports

Each first-wave agent writes one Markdown report here and also returns a concise final summary in chat.

Required report shape:

```markdown
# <surface> context-poison audit

## Scope read

## Method

## Findings

### <finding id>: <short title>
- Severity: critical | high | medium | low | note
- Confidence: evidence-backed | likely | uncertain
- Location: `<absolute path>:<line or section>`
- Evidence:
- Why it may poison context:
- Suggested disposition:

## Clean or intentionally scoped areas

## Gaps and follow-up reads
```

