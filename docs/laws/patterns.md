# [PATTERNS]

Pattern law binding two or more language branches. A row admitted here binds every branch it names; a single-branch law routes to that branch's stack doctrine instead.

## [01]-[ROWS]

| [INDEX] | [LAW]                                                                                                           | [BINDS]             |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------------ |
|  [01]   | a derived artifact keys on the content hash of its source; cache validity is key equality, never path or mtime  | C#, Python, tooling |
|  [02]   | root and anchor discovery walks upward to a sentinel file, never a fixed `parents[N]` depth or a cwd assumption | Python, C#, tooling |
|  [03]   | an operational rail returns one typed envelope; failure rides the envelope, never sentinel values in data rows  | all branches        |

The C# wire spells the content key `XxHash128`.
