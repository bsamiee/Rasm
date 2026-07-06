# [<scope-token>_CAPABILITIES]

<one-sentence-charter-of-the-capability-surface-this-atlas-covers>

[MATURITY]: `[GENESIS]` `[BUILT]` `[HARDENED]` `[COMMODITY]`

## [01]-[ENTRIES]

- [<id>]-[<MATURITY>]: <capability-what-the-system-must-be-able-to-do>
  - Owner: <the-one-surface-that-carries-it>
  - Edges: <depends-on-and-consumed-by-at-unit-grain>
  - Importance: <the-goal-or-direction-it-serves>
  - Gaps: <admitted-capability-no-owner-exploits-or-the-missing-slice>

Binding: a capability names an ability independent of its implementation — the `Owner` field carries the implementation fact, so renames touch one field. `Edges` stay at unit grain; member-level detail belongs to the owner's own page. `Gaps` is the load-bearing field for elicitation: an empty gaps field asserts the capability is fully exploited, and that assertion is checked, not assumed. Maturity advances by editing the leader token alone.
