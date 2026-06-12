# [BEDROCK] snapshot-codecs

## codec axis

- Three codec rows close the axis: text, binary, and raw pass-through.
- The text row is System.Text.Json with the generated-type converter factory registered once on the options value — per-type converters are derived, never hand-written.
- The binary row is the MessagePack contract surface; the raw row carries pre-encoded bytes that must not be re-framed — double-framing a raw payload destroys its original identity.
- Codec, compression, and hash identity are orthogonal axes; a profile is one row from each, and a new profile is row selection, never code.
- All three rows speak `IBufferWriter<byte>` on encode and `ReadOnlySequence<byte>`/`ReadOnlyMemory<byte>` on decode — the binary codec's overload set (`Serialize<T>(IBufferWriter<byte>, …)`, `Deserialize<T>(in ReadOnlySequence<byte>, …)`, stream and async forms) sets the I/O shape the other rows conform to, so codec choice never changes buffer ownership or the write path.
- `MessagePackSerializer.ConvertToJson` is the diagnostic egress for binary payloads — a debug projection row, never an alternate storage format.
- The typeless lane is a rejected row for durable artifacts: payloads that carry type names are a deserialization gadget surface and couple stored bytes to assembly identity (`OmitAssemblyVersion`/`AllowAssemblyVersionMismatch` exist precisely because that coupling breaks) — durable contracts are closed and typed, never discovered from the payload.
- The old-spec compatibility knob is likewise a rejected row — emitting a superseded wire dialect for a hypothetical legacy reader is compatibility theater in a system whose readers are all compiled from the same contracts.
- The allocating byte-array serialize overload is the convenience form; the snapshot lane writes through the buffer-writer overload into the seal pipeline — per-artifact heap arrays at snapshot scale are the rejected allocation profile.
- In-codec compression (`MessagePackSerializerOptions.WithCompression`) collapses the compression axis into the codec row. The chooser: header-owned external compression when the artifact must be verifiable and decompressible without the codec; codec-internal when payloads live and die inside the binary lane.
- Stacking both compression forms is the rejected form — LZ4-over-LZ4 spends CPU for near-zero ratio and doubles the failure surface on decode.

## contract and formatter derivation

- `[MessagePackObject]` with integer `[Key(n)]` members encodes as an array; the integer key sequence IS the wire schema.
- Appended keys are forward-compatible: older payloads leave new members at defaults; older readers skip trailing entries.
- Removed or reused keys silently re-type old payloads — keys are append-only and a deleted member's key is retired forever, never reassigned.
- Key gaps materialize as nil slots occupying array positions: sparse keys buy nothing and cost bytes per record — keys stay dense, and density plus append-only together make the contract auditable from the attribute list alone.
- String-key (map) mode — per type, or `UseMapMode` on the generated resolver for the whole closure — trades payload size for member-rename tolerance and partial readability; the snapshot default is integer keys, because the sealed header's contract stamp already owns evolution and makes map-mode self-description redundant weight.
- `[Union(tag, type)]` on the family root closes polymorphism with integer tags; tags are append-only under the same retirement law as keys — a reused tag re-types history, the wire-corruption class with no detection at decode time.
- `[SerializationConstructor]` pins the admission constructor when several exist; `[IgnoreMember]` excludes state that is derivable or process-local — both are contract declarations, not conveniences.
- `[GeneratedMessagePackResolver]` on a partial class derives the whole formatter closure at compile time — AOT-true, reflection-free; the formatter family for a module is one attribute on one partial type.
- The analyzer gates contract drift at build: unkeyed members, colliding formatters, invalid contract shapes, and missing union declarations are compile diagnostics — contract correctness is a build artifact, never a first-restore discovery.
- Generated domain owners (keyed value objects, smart-enum vocabularies, keyed unions) cross the binary codec through the dedicated formatter resolver (`ThinktectureMessageFormatterResolver.Instance`; constructor flag skips types carrying their own formatter attribute) so they serialize as bare key values.
- The wrapper-object encoding of a keyed owner is the rejected wire shape — it doubles payload size and leaks the owner's internal structure into the contract.
- The text codec mirrors the same admission: one converter factory on the options' converter list serves every generated owner, with constructor gates for attributed-type skip and span-parse admission.
- Per-type converter registration is the rejected spelling on both codecs — the factory derives what enumeration would hand-maintain, and the two lists would drift the day they coexist.
- Custom formatter kernels for irregular shapes drop to `MessagePackWriter`/`MessagePackReader` primitives (`WriteArrayHeader`/`WriteMapHeader`/`ReadArrayHeader`/`ReadMapHeader`, extension headers), the writer threaded by ref — the named kernel exemption inside one formatter type, never leaked into call sites.
- Application extension codes ride `ExtensionHeader` in the app-reserved non-negative range; the library reserves its own codes (compression envelopes live there) — colliding with reserved codes corrupts transparently-decompressed reads.
- Nil-slot semantics: in array mode a written nil and a retired-key gap are byte-identical — a reader cannot distinguish "author wrote absent" from "author never had the member", so nullable domain semantics never ride key gaps; absence is an explicit option-shaped member.
- Every serializer entry point threads a cancellation token — large snapshot encode and decode are cooperatively cancellable at buffer boundaries, so artifact writes participate in drain without a kill switch.

## resolver and options law

- `CompositeResolver.Create(formatters, resolvers)` resolves first-match-wins and caches the result per closed generic type — resolver composition happens exactly once at boot.
- Mutating the chain after first use is ineffective by design, which makes the resolver order a boot-time declaration, not runtime state — late formatter registration is unrepresentable, not merely discouraged.
- Resolution order law: explicit formatter instances → generated-domain resolver → generated-contract resolver → standard fallback; a type resolvable by two rows resolves to the first, so specificity decreases monotonically down the chain.
- `MessagePackSerializerOptions` is immutable; every `With*` returns a copy. One frozen options value per store profile is the law — options built at call sites fork codec policy invisibly and are the rejected form.
- The restore lane always reads under the untrusted security profile (`WithSecurity`): hash-collision-resistant map decoding plus the object-graph depth ceiling (default 500, `WithMaximumObjectGraphDepth`) guard adversarial and corrupted payloads.
- A restored blob's provenance is unprovable regardless of writer — trusted-read of persisted bytes is rejected even for bytes the same process wrote, because the bytes crossed a rest boundary.
- The write lane keeps the default trusted profile — the security cost is read-side only, so hardening restore costs writes nothing.
- `CompressionMinLength` (default 64) skips compression for payloads below the threshold even when the compression row is set — observed encoding can differ from requested policy, which is why receipts read the payload, never the policy.
- Decompression is extension-header-driven: a reader with any compression setting decodes both compressed and plain payloads — the compression row is write-side policy only, so changing it never breaks the installed base.
- Multi-document streams need no custom framing inside the binary lane: `MessagePackStreamReader.ReadAsync` yields one complete `ReadOnlySequence<byte>` per message, `ReadArrayAsync` streams array elements, and `RemainingBytes`/`DiscardBufferedData` handle resync — the append-stream and log-segment reader is package-native.
- Framing is invariant under compression: a compressed payload is still one well-formed message (the envelope is an extension value), so stream readers frame compressed and plain documents identically — segment files may freely mix both.
- Buffer economics are options-borne: the sequence pool and the contiguous-memory hint ride the options value, so large-snapshot memory behavior is profile policy, not call-site tuning.
- One shared sequence pool across profiles is the default; a dedicated pool is justified only by a measured isolation need — per-profile pools multiply steady-state memory for symmetry's sake.

## compression rows

- In-codec rows: the block-array form chunks the payload into independently compressed blocks — streaming-decompressible, avoids large contiguous buffers, the default binary-lane row.
- The single-block in-codec form is marginally smaller but requires the full buffer at decode — admissible only for small bounded payloads, and the size bound is the class's declared budget, not a guess.
- External block row: `LZ4Codec.Encode(source, target, level)` returns the compressed length or −1 when the target cannot hold the result.
- Incompressible input is a normal outcome, not an error; the fallback is store-raw-with-flag, and the header's observed-compression row records which branch ran.
- `LZ4Codec.MaximumOutputSize(length)` bounds the encode buffer (slightly above input size) — allocate once against the bound, slice to the returned length.
- `Decode` demands the exact uncompressed length as its target and returns −1 on mismatch — the uncompressed length is mandatory header data, not optional metadata; `PartialDecode` exists for prefix reads when only a head slice is needed.
- Level rows: `L00_FAST` is the interactive lane; `L03_HC`–`L09_HC` and `L10_OPT`/`L11_OPT`/`L12_MAX` are archive lanes where encode cost is paid once and decode cost is flat across levels.
- Level is archival policy data recorded in the receipt, never a hardcoded argument — flat decode cost means the level decision is purely a write-side budget call, revisable per class without touching readers.
- `LZ4Pickler.Pickle/Unpickle` is a self-describing, version-tagged envelope with typed corruption errors (unknown version, output-size mismatch, decode-count mismatch, unexpected data length) and `IBufferWriter` overloads for zero-copy composition.
- `UnpickledSize` peeks the original length without decoding — buffer sizing before decompression, free.
- The pickle envelope is library-proprietary — the admitted surface carries no interoperable cross-tool frame, so external interchange framing is owned by the sealed header, and the pickle row serves internal artifacts only.
- Dictionary-seeded decode (`Decode` with a dictionary span) supports delta-against-known-prefix schemes — the advanced row for many-small-similar payloads where a shared dictionary amortizes entropy.

## hash identity rows

- `XxHash3` (64-bit) is the identity default — fastest, streaming-capable, value-projecting (`HashToUInt64`, `GetCurrentHashAsUInt64`) so registry keys stay primitive.
- `XxHash128` (`HashToUInt128`, `GetCurrentHashAsUInt128`) is the content-address row where manifests aggregate many artifacts and collision headroom is structural, not probabilistic comfort.
- `Crc32`/`Crc64` exist to satisfy foreign formats that specify them; the 32-bit xxHash generation is superseded everywhere a format does not pin it — interop rows, never identity defaults.
- Non-cryptographic law: identity, dedup, and corruption detection only — tamper evidence is a different rail and a hash row never claims it.
- The seed parameter is domain separation: one seed per artifact class makes cross-class hash equality impossible by construction — content keys from different namespaces share storage without aliasing, and a content-addressed registry needs no class column in its key.
- The streaming lifecycle (`Append(span)`, `Append(Stream)`, `AppendAsync`, `GetCurrentHash`, `GetHashAndReset`) hashes during the write pass — identity costs zero extra reads.
- `GetHashAndReset` chains multi-artifact runs on one instance — one allocation per run, not per artifact.
- `Clone()` forks mid-stream state: hash a shared prefix once, branch per variant — the amortization for families of artifacts sharing a common head.
- Non-throwing forms (`TryGetCurrentHash`, `TryGetHashAndReset`, `TryHash`) plus `HashLengthInBytes` serve fixed-buffer header writers without exception control flow.
- Hash-domain row: the header records whether the content hash covers stored bytes (verify-without-decode) or plaintext (verify-after-decode).
- Stored-bytes is the default domain because it orders verification strictly before any decoder touches unproven input; the plaintext domain is admitted only when an external consumer must verify decoded content.
- Boot-time asset identity: a manifest of content-hash receipts per shipped asset, verified at boot via streaming append; a mismatch re-materializes the asset and emits a divergence receipt rather than failing boot.
- The identity manifest is itself a sealed artifact verified first — root-of-trust ordering: the verifier of everything is the one thing verified by the rejection ladder alone.
- Runtime re-verification and hot-reload lanes are debug-gated — production identity is checked once at the boot checkpoint, and a production code path that re-hashes assets per use has converted an integrity check into a steady-state tax.
- `HashLengthInBytes` drives header field sizing mechanically — the header layout for a new hash row derives from the algorithm instance, never from a parallel constant.

## sealed header

- The header is a fixed-width little-endian prologue outside any codec: magic, header version, codec row id, compression row id (observed, not requested), hash row id plus domain, contract/schema stamp, uncompressed length, stored length, content hash, store-epoch token.
- Fixed layout means rejection happens by offset reads before any parsing library touches the payload — the header is the artifact's entire trust boundary.
- Fixed-width fields are deliberate over a compact encoding: the header is read by offset under failure conditions, and a variable-length header re-introduces a parser exactly where the design removes one.
- The header carries its own small checksum so header corruption is distinguishable from payload corruption.
- Two corruptions, two recovery routes: header loss is terminal for the copy; payload loss may have a sibling replica or a salvageable twin — one receipt naming the wrong tier sends recovery down the wrong route.
- Self-description is total: a sealed artifact is restorable by a process knowing only the header layout — codec, compression, length, identity, and epoch all travel with the bytes, so no registry lookup gates restore.
- The single-pass seal: write a placeholder header, stream the payload while the hash appends, finalize with `GetHashAndReset`, seek to offset zero, write the final header, flush-to-disk, rename.
- One pass, no re-read — and the artifact is invalid (zeroed magic) at every instant before the final header lands, so partially written artifacts self-reject without any tombstone or lock file.

## atomic write protocol

- The temp file is a sibling in the SAME directory — cross-volume moves degrade to copy-plus-delete and lose atomicity.
- The commit sequence is write → `FileStream.Flush(flushToDisk: true)` → `File.Move(temp, final, overwrite: true)` (a rename at the platform layer), or `File.Replace` when an automatic backup of the incumbent is wanted.
- Directory-entry durability (syncing the directory itself) is unreachable from managed code — the named boundary residual: rename ordering after a crash is filesystem-dependent, accepted and documented, never papered over with a pseudo-sync.
- Orphan-temp sweep at open: temps follow a deterministic suffix scheme carrying the writing epoch, so the sweep distinguishes the current era's in-flight writes from dead eras at a glance.
- The sweep deletes temps that fail header verification and treats temps passing the full rejection ladder as receipted recovery candidates — a complete-but-unclaimed artifact (crash between flush and rename) is salvageable precisely because the sealed header makes completeness provable.
- Rejection-ladder receipts from sweeps and restores land on the boot fact stream — corruption discovered at open is operational evidence with the same routing as any other fault, never a log line.
- Sidecar deletion law: when the rename replaces a store that owns companion files (journal/shared-memory sidecars), the fenced window deletes the incumbent's sidecar set before the new payload lands — a fresh store paired with a stale sidecar is a corruption mode, not a recoverable state.
- The fenced order is fixed: fence connections → delete sidecar set → rename payload → epoch bump → reopen — each step receipted, and a crash between steps leaves a state the open ritual classifies unambiguously.
- Write-once discipline: a sealed artifact is never mutated in place — amendment is a new artifact plus identity change; in-place patching invalidates the hash, the header, and every manifest line that cites the content key.
- The flush row has two spellings: a single flush-to-disk before rename (large artifacts, one barrier), or write-through file options paying the barrier per write (many small artifacts, no trailing flush) — a policy value on the protocol, chosen per artifact class.
- `File.Replace` retains the displaced incumbent as a named backup in the same atomic step — the row for stores whose rollback plan is "previous artifact", deleting the backup only after the new artifact's first successful verification.

## typed restore rejections

- The rejection ladder is ordered so each tier is verifiable before the next runs, and restore never best-efforts past any tier:
- Tier 1 — magic/identity mismatch: not this system's artifact; nothing further is knowable.
- Tier 2 — header version unknown: written by a future surface; the artifact is evidence of deployment skew, not of corruption.
- Tier 3 — header checksum failure: header corruption, terminal for this copy.
- Tier 4 — stored-length mismatch: truncation, with byte counts in the receipt — distinct from hash failure because it names how much is missing.
- Tier 5 — content-hash mismatch: payload corruption with intact framing.
- Tier 6 — codec or compression row unknown: capability gap; the artifact is intact but unreadable by this process.
- Tier 7 — contract stamp newer than the compiled reader: the typed newer-schema rejection.
- Tier 8 — decode fault under the untrusted profile: contract drift hiding behind a matching stamp; the receipt pairs the codec error with the stamp.
- Tiers 1–5 run on header and raw bytes with zero decoding; tiers 6–8 are the only ones engaging codec machinery — the ladder's order is its threat model: corrupted input is rejected before any parser with attack surface runs.
- Every rejection is a typed receipt naming tier, artifact identity, and the evidence pair the tier compared — restore failures are routable facts, not exceptions.
- Tier-6 capability gaps are deployment evidence, not data errors: an artifact rejected for an unknown codec row by one process and restored by a newer sibling localizes the rollout fault without touching the artifact.

## divergent

### codec-axis-formatters — the codec row as a complete capability record

- Each codec row carries encode thunk (buffer-writer-driven), decode thunk (sequence-driven), contract-stamp derivation, untrusted-read profile, and diagnostic projection — five capabilities per row, consumed by every artifact class.
- The profile table is codec × compression × hash with every cell reachable from rows alone — three codecs, four compression rows, two hash widths yield twenty-four profiles and zero per-profile code.
- Contract evolution lives in three places that must agree — attribute keys (wire truth), the generated resolver (compile truth), the header stamp (artifact truth) — and deriving the stamp from the compiled contract makes stamp/contract divergence unrepresentable rather than tested-for; the analyzer becomes the single enforcement point for all three.
- Rejected forms the axis forecloses: per-type serializer choice (the class row chooses), runtime resolver mutation (frozen at boot), handwritten formatters for generated owners, typeless payloads as schema insurance, and map-mode as evolution insurance (the header already owns evolution).
- The nil-slot edge feeds the axis: because a reader cannot distinguish a written nil from a retired-key slot, optional-member semantics ride explicit option-shaped members, never key gaps — a contract rule the analyzer cannot see and the codec row must state.

### lz4-hash-header — the header as the only cross-surface contract

- Internal codec compression, external block compression, and raw all normalize under the header's (codec, compression-observed, lengths, hash, domain) tuple — one verification path regardless of how the bytes were produced.
- The header records what WAS done, never what was requested: the minimum-length skip and the incompressible −1 fallback both make policy diverge from outcome, so receipts echoing policy instead of outcome are wrong by construction.
- Identity composes with compression directionally: hashing stored bytes means the compression row participates in identity — recompressing changes the key — so dedup-relevant artifact classes pin their compression row in the class definition; a class that lets the level float loses dedup across writers, silently and permanently.
- Seeded identity partitions the hash space per artifact class, which is what lets one flat content-addressed store serve every class without key prefixes; the seed table is one more frozen policy row, and a seed change is identity migration — epoch-gated, never casual.
- The header version is a one-way ratchet: readers admit versions at or below their compiled ceiling, writers emit exactly their compiled version — version skew across a process fleet resolves at tier 2 of the rejection ladder with deployment evidence, never with a best-effort parse of an unknown layout.

### atomic-write-restore — one reversible choreography

- The writer's commit point is the rename, the restorer's commit point is the epoch bump, and everything before either commit point is repeatable garbage by construction.
- The crash matrix is total: before flush → orphan temp (swept); after flush before rename → complete orphan (salvage candidate via the rejection ladder, receipted either way); after rename before epoch bump → file new while epoch says old, and the open-ritual epoch reconcile re-runs verification; after epoch bump → committed.
- Salvage is the non-obvious capability: the sealed header converts the worst crash window from data loss into a receipted recovery decision — an orphan passing the full ladder is an admissible artifact whose only defect is an unclaimed name.
- Restore composes the same primitives in reverse — verify (full ladder) → materialize as temp → sidecar-fenced rename → epoch bump → reopen — so write and restore share one protocol vocabulary and one receipt taxonomy; a restore is auditable as the same typed steps a write emits, the only asymmetry being who supplies the bytes.
- Flush-strength is a policy row on the protocol, not a constant: flush-once-before-rename for large sealed artifacts, write-through for many small artifacts where per-write latency beats a trailing flush — both rows end at the same rename commit point and neither changes the crash matrix.
