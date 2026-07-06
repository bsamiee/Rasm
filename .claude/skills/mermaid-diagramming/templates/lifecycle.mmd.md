# [LIFECYCLE]

Draw a stateful owner: the states it holds and the guarded transitions between them. Use `stateDiagram-v2` with 5-7 states, `[*]` entry and exit, guard labels on every transition, and one composite state expanding its internal substates. `stateDiagram-v2` does not support ELK — drop `layout: elk`; `look: neo` still applies.

```mermaid
---
config:
  look: neo
  theme: base
---
stateDiagram-v2
    accTitle: Owner lifecycle
    accDescr: A stateful owner moving from idle through a composite active state into draining and closed, with guarded transitions.
    [*] --> Idle
    Idle --> Active: start
    state Active {
        [*] --> Serving
        Serving --> Paused: backpressure
        Paused --> Serving: resume
    }
    Active --> Draining: stop [inflight > 0]
    Active --> Closed: stop [inflight == 0]
    Draining --> Closed: flushed
    Closed --> [*]
```
