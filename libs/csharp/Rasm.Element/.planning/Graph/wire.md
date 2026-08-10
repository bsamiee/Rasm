# [ELEMENT_WIRE]

`ElementWire` owns the proto-first `rasm.element.v1` graph crossing. `ElementGraphWire` and `GraphDeltaWire` mirror closed seam unions; `WireCodec` owns per-case transcription; `Encode` lowers valid values; `DecodeGraph` and `DecodeDelta` re-admit hostile input on `Fin<T>`.

Content keys cross verbatim — `NodeId` as X32 text, `UInt128` as big-endian bytes — and every `NodeWire` carries the authoritative id-inclusive address minted under the active header tolerance. Decode reuses value admissions, graphs enter through `GraphDelta.AdmitOnto`, deltas prove `IsNormalForm` first, and `WireLimits` owns parse budgets and address verification.

`RedactionScope` clears encoded fields and carries the manifest, so unstable-node addresses remain evidence yet serve no OCC. Measures carry SI magnitude, quantity token, and dimension exponents. `GraphCrossing` composes the kernel message-envelope owner and admits the Protobuf event format over the wire body.

## [01]-[INDEX]

- [02]-[WIRE_CODEC]: the `rasm.element.v1` messages, `WireCodec` Mapperly transcription and key codecs, `ElementWire` encode/decode boundary, `WireLimits`, and the key, depth, and evolution laws.
- [03]-[EGRESS_REDACTION]: the `rasm.element` sensitivity taxonomy over the wire's classified columns, the `ClassifiedColumn` roster carrying each column group's `FieldMask` and identity verdict, and `RedactionScope` — the presence-clearing egress policy and its `RedactionManifestWire` receipt.
- [04]-[EVENT_ENVELOPE]: the `GraphEventType` closed crossing vocabulary over the kernel grammar and `GraphCrossing` — the mint composing `Rasm/Domain/event#ENVELOPE_MINT`, the Protobuf-format frame pair, the content-key `subject`, and the handling grade the egress scope derives.

## [02]-[WIRE_CODEC]

- Owner: the corpus-homed `rasm/element/v1/element.proto` `rasm.element.v1` contract — the language-neutral message roster `Grpc.Tools` compiles for C# (`GrpcServices=None`, message codegen only) and `buf`/`protoc-gen-es` + `grpcio-tools` compile for the TypeScript/Python peers, every compiler reading the one corpus root so the descriptor names this file identically at all three; `WireCodec` the `[Mapper]` static transcription family owning every per-case seam↔wire field mapping; `ElementWire` the boundary owner railing decode onto `Fin<T>`; `WireLimits` the parameterized decode-budget policy record.
- Cases: every closed seam union crosses as a `oneof` mirroring its cases 1:1 — `NodeWire` the eight `Node` payloads, `RelationshipWire` the six edge kinds, `PropertyValueWire` the recursive fourteen-case value family, `MaterialUsageWire` the explicit none/layer/profile usage family, `MaterialCompositionWire` the four composition arms, and `MaterialPropertySetWire` the engineering-property family. Generated keyed owners cross by key; absence is field presence, never a numeric or unset-oneof sentinel.
- Entry: `Encode(ElementGraph, scope)` mints every node address under `graph.Header.Tolerance` before applying scoped redaction.
- Entry: `Encode(GraphDelta, basis)` mints added and revised addresses from the explicit active header basis.
- Entry: A delta reheader supplies revision tolerance; the basis supplies each revised node's before-address tolerance.
- Entry: `DecodeGraph` and `DecodeDelta` re-admit values, structure, and carried-address verification on `Fin<T>`.
- Auto: `WireCodec` combines Mapperly's explicit member diagnostics with generated union/protobuf case dispatch. Decode re-mints a `MeasureValue` through `OfSi`, re-admits its `MeasureBand`, re-admits material-usage direction/cardinal tokens, and recursively re-admits every `PropertyValue`; no generated-code `Get` throw is part of the boundary contract. `ToPropertySet` keeps ELEVEN per-case bodies where every sibling decode collapses to a row table, because the arms share no generative structure to derive from: each names a distinct wire message, a distinct factory arity, and a distinct accumulating slot set, so a case-keyed row table carries the same eleven bodies behind eleven distinct closure types and trades the generated `PropertySetOneofCase` switch's compile-time exhaustiveness for a lookup miss — the switch IS the table here.
- Receipt: `ElementGraphWire` carries each node's authoritative address beside its payload and active header.
- Receipt: `GraphDeltaWire` carries authoritative addresses for added nodes and both sides of every revision.
- Receipt: Verified decode checks carried addresses outside `redaction.unstable_node_ids` and rails drift as `AddressUnstable`.
- Packages: Google.Protobuf (`IMessage<T>`/`MessageParser<T>`/`CodedInputStream.CreateWithLimits`/`ByteString`/`RepeatedField<T>`/`MessageExtensions` write family), Grpc.Tools (the `<Protobuf>` MSBuild item, `GrpcServices=None`, `PrivateAssets=all` — build-only, never a runtime surface), Riok.Mapperly (`[Mapper]`/`[UserMapping]`/`[MappingTarget]`/`[MapProperty]`/`[MapperIgnoreSource]`/`[UseStaticMapper]` and `MappingConversionType` policy over the Thinktecture `Create`/`Value` key codecs), NodaTime.Serialization.Protobuf (`NodaExtensions`/`ProtobufExtensions` registered WHOLESALE through `[UseStaticMapper]`, so `ToTimestamp`/`ToInstant`/`ToProtobufDuration`/`ToNodaDuration` cross with no per-member row), LanguageExt.Core (`Fin`/`Seq`/`Option` and the accumulating `Traverse` over `Validation<Error,_>` the admission folds collapse to `Fin` at their gate), Thinktecture.Runtime.Extensions (the generated total `Switch` encode dispatch).
- Auto: `WireLimits.Default` carries its two budgets as DECLARED POLICY VALUES, each naming the axis it bounds — the size ceiling bounds the WHOLE-SNAPSHOT transfer axis (an `ElementGraph` crosses in one message; there is no chunked graph transport), the recursion ceiling the one recursive family (`PropertyValue.List`/`Table`; every other message is flat) and it sits under protobuf's own default recursion limit of 100 so the seam refuses before the parser does; both are tuned numbers a deployment re-declares through `Of`, and a payload past either is refused by policy rather than by construction.
- Growth: a new node/edge/value case is one `oneof` arm and one `WireCodec` case mapping; a new payload column is one append-only numbered field; a new peer runtime is one codegen lane over the same `.proto`; a new decode budget is one `WireLimits` column carrying the axis it bounds.
- Boundary: Peers retain `NodeWire.content_address`; they never re-mint it from a decoded payload.
- Boundary: `content_address` is the 16-byte big-endian `ContentAddress.Of(node, activeTolerance)` value.
- Boundary: A manifest-listed unstable node retains the source address as evidence but cannot use it for edit OCC.
- Boundary: Recursive values parse under `WireLimits`, decoded values re-cross owner gates, and descriptors evolve append-only.

```proto signature
// rasm/element/v1/element.proto — the rasm.element.v1 graph wire. Field numbers are append-only; removal reserves.
// Content keys cross verbatim: NodeId as the X32 string, UInt128 as 16-byte big-endian bytes.
syntax = "proto3";

package rasm.element.v1;

import "google/protobuf/duration.proto";
import "google/protobuf/empty.proto";
import "google/protobuf/timestamp.proto";

option csharp_namespace = "Rasm.Element.Wire";

// --- [GRAPH_ENVELOPES] ---
message ElementGraphWire {
  HeaderWire header = 1;
  repeated NodeWire nodes = 2;
  repeated RelationshipWire edges = 3;
  optional RedactionManifestWire redaction = 4;  // present only on a scoped egress
}

// The scoped-egress receipt: the policy identity, the owner-qualified paths the policy cleared, and the ids of the
// nodes whose cleared columns fold into canonical bytes. A cleared column WITHOUT explicit presence reads as its
// proto3 default, so this declared roster — never the message — is the presence record a consumer separates a cleared
// column from an authored default by, which is why no redactor token stands in for the cleared value.
message RedactionManifestWire {
  string policy = 1;
  repeated string cleared_paths = 2;
  repeated string unstable_node_ids = 3;
}

message GraphDeltaWire {
  repeated NodeWire added_nodes = 1;
  repeated string removed_node_ids = 2;
  repeated NodeRevisionWire revised_nodes = 3;
  repeated RelationshipWire added_edges = 4;
  repeated RelationshipWire removed_edges = 5;
  optional HeaderWire header = 6;
}

message NodeRevisionWire {
  NodeWire before = 1;
  NodeWire after = 2;
}

message HeaderWire {
  string schema = 1;                       // ReleaseVersion key
  string view = 2;                         // ModelView key
  GeoReferenceWire geo_reference = 3;
  double tolerance = 4;
  google.protobuf.Timestamp at = 5;        // Instant via NodaTime ToTimestamp/ToInstant
  StepHeaderWire step = 6;
  map<string, string> unit_scheme = 7;     // UnitScheme: QuantityType token -> registry unit-enum member name; empty = SI
}

// --- [NODE_WIRE] ---
message NodeWire {
  string id = 1;                           // NodeId X32, verbatim — never re-derived by a peer
  oneof payload {
    ObjectWire object = 2;
    MaterialWire material = 3;
    PropertySetWire property_set = 4;
    QuantitySetWire quantity_set = 5;
    AssessmentWire assessment = 6;
    AppearanceWire appearance = 7;
    CoverageWire coverage = 8;
    ObservationWire observation = 9;
  }
  bytes content_address = 10;              // ContentAddress.Of(node, active header tolerance), 16-byte BE
}

message ObjectWire {
  string kind = 1;                         // ObjectKind key: occurrence | type
  optional string external_id = 2;         // the Bim-stored IFC GlobalId, re-emitted at Emit
  ClassificationWire classification = 3;
  repeated ClassificationWire classifications = 4;
  string predefined_type = 5;
  string name = 6;
  string tag = 7;
  map<string, bytes> representations = 8;  // RepresentationIdentifier -> 16-byte BE content key
  optional OwnerHistoryWire history = 9;
  SchemaSpanWire span = 10;
  optional string object_type = 11;        // the USERDEFINED label pairing with predefined_type; absent otherwise
  optional PlacementWire placement = 12;
}

// The PlacementTransform frame flattened to its nine ordered doubles — the Location origin, the Axis local-Z, and the
// RefDirection local-X. It crosses so a peer reads a rigid move off the node it moved; it stays OUT of the canonical
// bytes at its owner, which is what keeps the Rasm.Bim diff's Moved discriminant alive across the crossing.
message PlacementWire {
  double location_x = 1;
  double location_y = 2;
  double location_z = 3;
  double axis_x = 4;
  double axis_y = 5;
  double axis_z = 6;
  double ref_direction_x = 7;
  double ref_direction_y = 8;
  double ref_direction_z = 9;
}

message ClassificationWire {
  string system = 1;
  string code = 2;
  string edition = 3;
  optional string source = 4;
  optional string edition_date = 5;        // LocalDate, ISO-8601 token
  optional string title = 6;
}

message StepHeaderWire {
  repeated string descriptions = 1;
  string name = 2;
  google.protobuf.Timestamp time_stamp = 3;
  repeated string authors = 4;
  repeated string organizations = 5;
  string preprocessor = 6;
  string originating_system = 7;
  repeated string schema = 8;
}

message OwnerHistoryWire {
  string owning_user = 1;
  string owning_application = 2;
  google.protobuf.Timestamp created = 3;
  optional google.protobuf.Timestamp modified = 4;
  string change_action = 5;
  string state = 6;
}

message SchemaSpanWire {
  string introduced_in = 1;                // ReleaseVersion key
  optional string removed_in = 2;
}

message AppearanceWire {
  bytes appearance_key = 1;                // 16-byte BE — AppearanceSummary.Of mints, peers reproduce
  double base_color_r = 2;
  double base_color_g = 3;
  double base_color_b = 4;
  double metallic = 5;
  double roughness = 6;
  double opacity = 7;
  bool transmissive = 8;
}

// --- [VALUE_WIRE] ---
// The RECURSIVE fourteen-case PropertyValue mirror — hostile nesting is bounded by the decoder's
// CodedInputStream.CreateWithLimits recursion budget, never a seam re-check.
message PropertyValueWire {
  oneof value {
    string text = 1;
    MeasureValueWire measure = 2;
    bool boolean = 3;
    LogicalWire logical = 4;
    EnumeratedWire enumerated = 5;
    ReferenceWire reference = 6;
    BoundedWire bounded = 7;
    ListWire list = 8;
    TableWire table = 9;
    ComplexWire complex = 10;
    TemporalWire temporal = 11;
    bytes integer = 12;
    double number = 13;
    bytes binary = 14;
  }
}

message LogicalWire { optional bool value = 1; }                       // absent = UNKNOWN
// TYPED members (the IfcValue enumeration domain) — the Of admission closes them to the scalar arms at decode.
message EnumeratedWire { repeated PropertyValueWire selected = 1; repeated PropertyValueWire allowed = 2; }
// The TemporalValue arms as ISO-8601 tokens (the seam Iso() canon); the epoch stamp rides the Timestamp adapter.
message TemporalWire {
  oneof value {
    string date = 1;
    string moment = 2;
    string time = 3;
    string span = 4;
    google.protobuf.Timestamp stamp = 5;
  }
}
message ReferenceWire { string target_id = 1; optional string usage_name = 2; }
message BoundedWire {
  optional MeasureValueWire lower = 1;
  optional MeasureValueWire upper = 2;
  optional MeasureValueWire set_point = 3;
}
message ListWire { repeated PropertyValueWire values = 1; }
message TableWire { repeated TableRowWire rows = 1; string interpolation = 2; }
message TableRowWire { PropertyValueWire defining = 1; PropertyValueWire defined = 2; }
message ComplexWire { string usage_name = 1; map<string, PropertyValueWire> properties = 2; }

// SI-coerced identity columns ONLY — the exact columns CanonicalWriter.Measure hashes; the registry
// CanonicalUnit re-mints at decode through MeasureValue.OfSi, never a {value, unit:string} crossing.
message MeasureValueWire {
  string quantity_type = 1;
  double si = 2;
  sint32 dim_length = 3;
  sint32 dim_mass = 4;
  sint32 dim_time = 5;
  sint32 dim_current = 6;
  sint32 dim_temperature = 7;
  sint32 dim_amount = 8;
  sint32 dim_luminous_intensity = 9;
  optional MeasureBandWire uncertainty = 10;
}

message MeasureBandWire {
  string kind = 1;                         // UncertaintyKind key
  double lower_si = 2;
  double upper_si = 3;
  optional double standard_deviation_si = 4;
  optional double coverage_factor = 5;
}

// --- [BAG_WIRE] ---
message PropertySetWire {
  string set_name = 1;
  map<string, PropertyValueWire> values = 2;
  string inheritance = 3;                  // InheritanceMode key
  sint32 source_rank = 4;                  // PropertySource int key (10/20/30/40)
}

message QuantitySetWire {
  string set_name = 1;
  map<string, MeasureValueWire> values = 2;
  string inheritance = 3;
  sint32 source_rank = 4;
  map<string, GroupIdentityWire> groups = 5;  // dot-path prefix -> grouping identity; empty = an ungrouped bag
}

// The three grouping columns are proto3 `optional`, so an unstated qualifier crosses as field ABSENCE and an
// explicitly empty spelling crosses as a present empty string — the seam Option distinction the canonical bytes
// already presence-prefix. PropertySetWire carries no counterpart: a property bag nests through PropertyValueWire.
message GroupIdentityWire {
  optional string discrimination = 1;
  optional string quality = 2;
  optional string usage = 3;
}

// --- [EDGE_WIRE] ---
message RelationshipWire {
  oneof edge {
    ComposeWire compose = 1;
    AssignWire assign = 2;
    AssociateWire associate = 3;
    ConnectWire connect = 4;
    VoidWire void = 5;
    GenericWire generic = 6;
  }
}

message ComposeWire { string whole_id = 1; string part_id = 2; string sub_kind = 3; optional sint32 ordinal = 4; }
message AssignWire { string subject_id = 1; string definition_id = 2; string sub_kind = 3; }
message AssociateWire { string subject_id = 1; string resource_id = 2; MaterialUsageWire usage = 3; }
message ConnectWire { string from_id = 1; string to_id = 2; string sub_kind = 3; optional string realizing_id = 4; optional bytes interface_key = 5; }
message VoidWire { string host_id = 1; string feature_id = 2; string sub_kind = 3; }
message GenericWire {
  string wire_name = 1;
  string relating_id = 2;
  string related_id = 3;
  map<string, PropertyValueWire> attributes = 4;
  repeated RelationshipParticipantWire participants = 5;
}
message RelationshipParticipantWire { string node_id = 1; string role = 2; optional sint32 ordinal = 3; }

// MaterialUsage.None is an explicit arm; an unset oneof is malformed foreign input.
message MaterialUsageWire {
  oneof usage {
    LayerSetUsageWire layer_set = 1;
    ProfileSetUsageWire profile_set = 2;
    google.protobuf.Empty none = 3;
  }
}
message LayerSetUsageWire {
  string direction = 1;
  string sense = 2;
  optional MeasureValueWire offset_from_reference_line = 3;
  optional MeasureValueWire reference_extent = 4;
}
message ProfileSetUsageWire {
  optional sint32 cardinal_point = 1;
  optional MeasureValueWire reference_extent = 2;
}

// --- [MATERIAL_WIRE] ---
message MaterialWire {
  string material_key = 1;
  MaterialCompositionWire composition = 2;
  repeated MaterialPropertySetWire property_sets = 3;
}

message MaterialCompositionWire {
  oneof composition {
    SingleWire single = 1;
    LayerSetWire layer_set = 2;
    ProfileSetWire profile_set = 3;
    ConstituentSetWire constituent_set = 4;
  }
}
message SingleWire { string material_key = 1; }
message LayerSetWire { repeated MaterialLayerWire layers = 1; }
// The IFC per-row columns ride explicit presence: an unset priority is UNSET (never a numeric sentinel) and an unset
// ventilated flag is the IfcLogical UNKNOWN the seam Option<bool> carries, so absence and FALSE never alias.
message MaterialLayerWire {
  string material_key = 1;
  MeasureValueWire thickness = 2;
  string layer_name = 3;
  optional int32 priority = 4;
  string category = 5;
  optional bool ventilated = 6;
}
// Field 1 and 2 are reserved because the seam DERIVES the primary material and profile off the rows: a primary scalar
// beside row zero double-stores what row zero already carries. Composite is the set-level built-up outline.
message ProfileSetWire {
  reserved 1, 2;
  optional SectionPropertiesWire section = 3;
  repeated MaterialProfileWire profiles = 4;
  optional ProfileRefWire composite = 5;
}
message MaterialProfileWire {
  string material_key = 1;
  ProfileRefWire profile = 2;
  optional int32 priority = 3;
  string category = 4;
  repeated MeasureValueWire offsets = 5;
}
message ProfileRefWire { string standard = 1; string designation = 2; bytes content_key = 3; }
message ConstituentSetWire { repeated MaterialConstituentWire constituents = 1; }
message MaterialConstituentWire { string material_key = 1; string category = 2; double fraction = 3; string part_name = 4; }

message SectionPropertiesWire {
  MeasureValueWire area = 1;
  MeasureValueWire iyy = 2;
  MeasureValueWire izz = 3;
  MeasureValueWire j = 4;
  MeasureValueWire iw = 5;
  MeasureValueWire wely = 6;
  MeasureValueWire welz = 7;
  MeasureValueWire wply = 8;
  MeasureValueWire wplz = 9;
  MeasureValueWire av_y = 10;
  MeasureValueWire av_z = 11;
  MeasureValueWire radius_of_gyration_major = 12;
  MeasureValueWire radius_of_gyration_minor = 13;
  MeasureValueWire depth = 14;
  MeasureValueWire width = 15;
  MeasureValueWire heated_perimeter = 16;
  MeasureValueWire axis_distance = 17;
  MeasureValueWire shear_centre_y = 18;
  MeasureValueWire shear_centre_z = 19;
  double monosymmetry_factor = 20;
}

// Eleven engineering-property arms, evidence on the envelope (the base-class column) — each arm mirrors
// its Composition/material case columns 1:1; smart-enum columns cross as key tokens.
message MaterialPropertySetWire {
  PropertyEvidenceWire evidence = 1;
  oneof property_set {
    MechanicalWire mechanical = 2;
    OrthotropicWire orthotropic = 3;
    ThermalWire thermal = 4;
    AcousticWire acoustic = 5;
    FireWire fire = 6;
    EnvironmentalWire environmental = 7;
    CostWire cost = 8;
    DampingWire damping = 9;
    HygrothermalWire hygrothermal = 10;
    DurabilityWire durability = 11;
    OpticalWire optical = 12;
  }
}
message PropertyEvidenceWire { string source = 1; string reference = 2; optional string valid_until = 3; }
message MechanicalWire {
  MeasureValueWire density = 1;
  MeasureValueWire youngs_modulus = 2;
  MeasureValueWire yield_strength = 3;
  MeasureValueWire ultimate_strength = 4;
  double poissons_ratio = 5;
  double thermal_expansion_per_k = 6;
}
message OrthotropicWire {
  MeasureValueWire density = 1;
  MeasureValueWire e1_parallel = 2;
  MeasureValueWire e2_perpendicular = 3;
  MeasureValueWire shear_modulus = 4;
  MeasureValueWire strength1_parallel = 5;
  MeasureValueWire strength2_perpendicular = 6;
  double thermal_expansion_per_k = 7;
}
message ThermalWire {
  MeasureValueWire conductivity = 1;
  MeasureValueWire specific_heat = 2;
  MeasureValueWire u_value = 3;
  double vapour_resistance_factor = 4;
}
message AcousticWire {
  repeated double absorption_spectrum = 1;
  repeated double sound_reduction_index_db = 2;
  optional double dynamic_stiffness_mn_per_m3 = 3;
  optional double flow_resistivity_pa_s_per_m2 = 4;
  optional double loss_factor = 5;
}
message FireWire {
  string reaction = 1;                     // FireRating key
  string smoke = 2;                        // SmokeClass key
  string droplets = 3;                     // DropletClass key
  FireResistanceWire resistance = 4;
}
// Each criterion is OPTIONAL: an unmeasured leg (an ACI 216.1 equivalent-thickness derivation measures insulation
// alone) crosses ABSENT, never as a zero a peer reads as a failing rating.
message FireResistanceWire { optional sint32 load_bearing_minutes = 1; optional sint32 integrity_minutes = 2; optional sint32 insulation_minutes = 3; }
message EnvironmentalWire {
  string basis = 1;                        // MeasurementBasis key
  repeated double impacts = 2;
  double recycled_content = 3;
  double end_of_life_recovery = 4;
}
message CostWire {
  string basis = 1;
  string currency = 2;                     // ISO-4217 token
  double supply_per_unit = 3;
  double install_per_unit = 4;
  double lifecycle_per_unit = 5;
}
message DampingWire { double damping_ratio = 1; optional RayleighWire rayleigh = 2; }
message RayleighWire { double alpha_per_s = 1; double beta_s = 2; }
message HygrothermalWire {
  double porosity = 1;
  MeasureValueWire water_content_80rh = 2;
  MeasureValueWire free_water_saturation = 3;
  optional double water_absorption_kg_per_m2_sqrt_s = 4;
  optional SampledCurveWire sorption_isotherm = 5;
  optional SampledCurveWire liquid_transport = 6;
  optional SampledCurveWire moisture_conductivity = 7;
}
message SampledCurveWire { repeated double axis = 1; repeated double values = 2; }
message DurabilityWire {
  double carbonation_rate_mm_per_sqrt_year = 1;
  MeasureValueWire chloride_diffusion = 2;
  double ageing_exponent = 3;
}
message OpticalWire {
  double visible_transmittance = 1;
  double visible_reflectance_front = 2;
  double visible_reflectance_back = 3;
  double solar_transmittance = 4;
  double solar_reflectance_front = 5;
  double solar_reflectance_back = 6;
  double thermal_ir_transmittance = 7;
  double thermal_ir_emissivity_front = 8;
  double thermal_ir_emissivity_back = 9;
}

// --- [ASSESSMENT_WIRE] ---
message AssessmentWire {
  string discipline = 1;                   // Discipline key, re-admitted through Discipline.Parse
  string route = 2;                        // AnalysisRoute token
  bytes input_key = 3;                     // 16-byte BE
  string outcome = 4;                      // AssessmentOutcome key
  map<string, PropertyValueWire> results = 5;
  optional DiagnosticWire diagnostic = 6;
  optional bytes result_blob = 7;
  ProvenanceWire provenance = 8;
  repeated string depends_on_ids = 9;
}
message DiagnosticWire { string phase = 1; string kind = 2; string message = 3; optional sint32 code = 4; }
message ProvenanceWire {
  string author = 1;
  string tool = 2;
  string version = 3;
  google.protobuf.Timestamp at = 4;
  google.protobuf.Duration elapsed = 5;
  optional google.protobuf.Timestamp window_start = 6;
  optional google.protobuf.Timestamp window_end = 7;
  optional string correlation = 8;         // Guid canonical text
  sint32 attempt = 9;
}

// --- [OBSERVATION_WIRE] ---
// The measured-series crossing: the stream identity, the extent, and the by-reference chunk run — the sample BYTES
// never cross, exactly as a coverage raster never does. window_start/window_end are the flattened NodaTime Interval
// (both bounded by seam admission, so neither column is optional).
message ObservationWire {
  string sensor = 1;                       // SensorId — the deployment identity
  string aspect = 2;                       // PropertyName — the observed aspect of the element
  string observed = 3;                     // QuantityType token
  sint32 dim_length = 4;                   // the Dimension signature, flattened as MeasureValueWire carries it
  sint32 dim_mass = 5;
  sint32 dim_time = 6;
  sint32 dim_current = 7;
  sint32 dim_temperature = 8;
  sint32 dim_amount = 9;
  sint32 dim_luminous_intensity = 10;
  string canonical_unit = 11;
  string sampling = 12;                    // SamplingKind key
  optional google.protobuf.Duration cadence = 13;  // unset = event-driven, never a zero sentinel
  google.protobuf.Timestamp window_start = 14;
  google.protobuf.Timestamp window_end = 15;
  repeated ObservationChunkWire chunks = 16;
  SeriesStatisticsWire statistics = 17;
  SensorProvenanceWire provenance = 18;
}
message ObservationChunkWire {
  google.protobuf.Timestamp window_start = 1;
  google.protobuf.Timestamp window_end = 2;
  bytes series_key = 3;                    // 16-byte BE — the samples ride the object store, never the wire
  sint32 sample_count = 4;
}
message SensorProvenanceWire {
  string manufacturer = 1;
  string model = 2;
  string serial = 3;
  optional string calibrated_at = 4;       // LocalDate, ISO-8601 token
  optional MeasureBandWire tolerance = 5;  // the ZERO-CENTRED instrument band Value shifts onto each sample
}
message SeriesStatisticsWire {
  map<string, sint32> census = 1;          // ObservationGrade key -> count
  google.protobuf.Duration span = 2;
  optional MeasureValueWire minimum = 3;
  optional MeasureValueWire maximum = 4;
  optional MeasureValueWire mean = 5;
  optional MeasureValueWire total = 6;
}

// --- [COVERAGE_WIRE] ---
message CoverageWire {
  reserved 3;                              // retired six-coefficient GridDescriptorWire — the placement is the kernel lattice
  string kind = 1;                         // CoverageKind key
  bytes raster_key = 2;                    // 16-byte BE — the blob rides the object store, never the wire
  repeated CoverageBandWire bands = 4;
  GeoReferenceWire crs = 5;
  repeated OverviewLevelWire overviews = 6; // the Coarsen chain in decimation order — position IS the level ordinal
  sint32 base_block_x = 7;
  sint32 base_block_y = 8;
  repeated TimeSliceWire slices = 9;
  CellLatticeWire grid = 10;
}
message TimeSliceWire { google.protobuf.Timestamp at = 1; bytes raster_key = 2; }
// The kernel CellLattice placement: the TWELVE index-to-world affine coefficients in row-major order (the fourth
// matrix row is the invariant [0 0 0 1] and never crosses) plus the three-axis census and the cell budget the
// decoder re-admits through CellLattice.Of. A level's lattice crosses whole rather than as a decimation factor,
// so a peer runtime reconstructs the exact affine the content key was taken over.
message CellLatticeWire {
  repeated double affine = 1;              // exactly 12 — m00..m03, m10..m13, m20..m23
  sint32 columns = 2;
  sint32 rows = 3;
  sint32 layers = 4;
  sint64 ceiling = 5;
}
message CoverageBandWire {
  reserved 3;                              // retired RasterSampleType string key — storage is the kernel ChannelDtype roster
  sint32 index = 1;
  string name = 2;
  sint32 sample_type = 12;                 // kernel ChannelDtype key
  string role = 4;                         // BandRole key
  optional double no_data = 5;
  string units = 6;
  double offset = 7;
  double scale = 8;
  optional double range_min = 9;
  optional double range_max = 10;
  repeated ColorBinWire palette = 11;
}
// The legend colour crosses as its display-byte quadruple — the same ToRgb projection the content key takes, so the
// wire and the key agree by construction — and the decoder re-admits through PerceptualColor.OfRgb.
message ColorBinWire { sint32 index = 1; uint32 r = 2; uint32 g = 3; uint32 b = 4; uint32 a = 5; string category = 6; }
message OverviewLevelWire {
  reserved 1, 2, 3;                        // retired width/height/cell_size — the level carries its own lattice
  bytes raster_key = 4;
  sint32 block_x = 5;
  sint32 block_y = 6;
  CellLatticeWire grid = 7;
}

// --- [GEOREFERENCE_WIRE] ---
message GeoReferenceWire {
  double eastings = 1;
  double northings = 2;
  double orthogonal_height = 3;
  double x_axis_abscissa = 4;
  double x_axis_ordinate = 5;
  double scale_x = 6;
  double scale_y = 7;
  double scale_z = 8;
  string geodetic_datum = 9;
  string vertical_datum = 10;               // VerticalCrs.Name — blank with no vertical_epsg is the absent vertical frame
  optional ProjectedCrsWire crs = 11;
  optional double epoch = 12;
  optional sint32 vertical_epsg = 13;       // VerticalCrs.Epsg — the vertical authority code a survey/GIS ingest carries
}
message ProjectedCrsWire {
  string name = 1;
  optional sint32 epsg = 2;
  optional string wkt = 3;
  optional string map_projection = 4;
  optional string map_zone = 5;
  string resolution = 6;                   // CrsResolution key
}
```

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Numerics;
using System.Globalization;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Compliance.Classification;
using NodaTime.Serialization.Protobuf;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Rasm.Element.Wire;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
// The kernel lattice-axis count and the seam's physical 7-vector both spell Dimension; the alias names the kernel
// reading so the enclosing-namespace Dimension stays the bare SI signature.
using LatticeAxis = Rasm.Numerics.Dimension;

namespace Rasm.Element.Graph;

// Csproj codegen item this contract realizes; ProtoRoot pins the corpus root so the descriptor name reads
// rasm/element/v1/element.proto, the one spelling the frozen snapshot and both peer minters carry:
//   <Protobuf Include="../../../tests/contracts/rasm/element/v1/element.proto"
//             ProtoRoot="../../../tests/contracts" GrpcServices="None" />

// --- [MODELS] -----------------------------------------------------------------------------
// WireLimits owns size, recursion, and address-verification policy. Parse calls contain no budget literal: both
// defaults are DECLARED POLICY VALUES naming the axis they bound, and a deployment re-declares either through Of.
public sealed record WireLimits {
 // Bounds the WHOLE-SNAPSHOT transfer axis: an ElementGraph crosses in ONE message (there is no chunked graph
 // transport), so the budget clears the largest snapshot the seam admits and refuses beyond it.
 private const int SnapshotSizeCeiling = 512 << 20;

 // Bounds the ONE recursive family — PropertyValue.List/Table nest, every other message is flat — with headroom for
 // the envelope frames above it, and sits UNDER protobuf's own default recursion limit of 100 so a hostile nesting
 // depth is refused by this seam's declared budget rather than by the parser's.
 private const int NestedValueDepthCeiling = 96;

 private WireLimits(int sizeLimit, int recursionLimit, bool verifyAddresses) =>
  (SizeLimit, RecursionLimit, VerifyAddresses) = (sizeLimit, recursionLimit, verifyAddresses);

 public int SizeLimit { get; }
 public int RecursionLimit { get; }
 internal bool VerifyAddresses { get; init; }

 public static readonly WireLimits Default = new(SnapshotSizeCeiling, NestedValueDepthCeiling, verifyAddresses: false);
 public static readonly WireLimits Verified = Default.WithAddressVerification();

 public static Fin<WireLimits> Of(int sizeLimit, int recursionLimit, Op key) =>
  sizeLimit > 0 && recursionLimit > 0
   ? Fin.Succ(new WireLimits(sizeLimit, recursionLimit, verifyAddresses: false))
   : ElementFault.ValueRejected(key, $"<wire-limits-invalid:{sizeLimit}:{recursionLimit}>");

 public WireLimits WithAddressVerification() => this with { VerifyAddresses = true };
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Mapperly transcription family: source-generated per-case field mapping, key codecs hand-owned as
// [UserMapping] statics so identity NEVER re-derives — Mapperly transcribes shape, the seam owns identity.
// Encode case dispatch is the union's generated total Switch; decode dispatch is the generated PayloadCase/
// ValueCase closed enum ([MapDerivedType] is the class-hierarchy rail; a oneof envelope has no case base).
// RequiredMappingStrategy.Both proves BOTH sides complete — but source-side completeness is compiler-proved only while
// no [MapPropertyFromSource] reader lands here: one whole-source reader suppresses RMG020 for EVERY source member of
// that mapping, touched or not, so a reader-bearing mapping demotes its [MapperIgnoreSource] roster from compiler proof
// to authored inventory. Target-side RMG012 is unaffected.
// The two NodaTime.Serialization.Protobuf static mappers register the whole ToTimestamp/ToInstant/ToProtobufDuration/
// ToNodaDuration/ToDate/ToLocalDate family, so every plain temporal crossing generates with NO per-member codec row;
// the hand bodies below keep their explicit calls because each encodes a CHOICE — an Interval flattened to a bounded
// column pair, an Option presence write, an ISO pattern the wire fixes — not a plain conversion.
[Mapper(
 EnabledConversions = MappingConversionType.Constructor | MappingConversionType.ImplicitCast | MappingConversionType.Enumerable | MappingConversionType.Dictionary,
 RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.NodaExtensions))]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.ProtobufExtensions))]
internal static partial class WireCodec {
 // --- [KEY_CODECS] — verbatim crossings, never re-minted
 [UserMapping] internal static string ToWire(NodeId id) => id.Value;
 [UserMapping] internal static NodeId ToNodeId(string wire) => NodeId.Create(wire);
 [UserMapping] internal static string ToWire(MaterialId id) => id.Value;
 [UserMapping] internal static ByteString ToWire(UInt128 key) {
  Span<byte> be = stackalloc byte[16];
  BinaryPrimitives.WriteUInt128BigEndian(be, key);           // the persisted XxHash128 canonical form
  return ByteString.CopyFrom(be);
 }
 [UserMapping] internal static UInt128 ToKey(ByteString wire) => BinaryPrimitives.ReadUInt128BigEndian(wire.Span);

 // --- [CASE_TRANSCRIPTIONS] — Mapperly generates the flat-column width per case; every union-valued member
 // rides an explicit envelope fold below, every Option/tuple-flatten/token crossing rides an explicit
 // [UserMapping] carrier codec (conditional set on encode; the hand decode below reads the generated Has*/null
 // presence pairs), and [MapProperty] pins every seam→wire name seam so the generator never silently skips a member.
 // Envelope owns Id (NodeWire.id), so the payload mappings exclude it — RequiredMappingStrategy.Both would
 // otherwise fault the intentionally-unmapped source member. AllClassifications is the node's own COMPUTED union of
 // the primary and secondary columns, both of which already cross on their own fields, so mapping it would double-store
 // the primary on the wire and fork the two spellings the moment one side is edited.
 [MapperIgnoreSource(nameof(Node.Object.Id))]
 [MapperIgnoreSource(nameof(Node.Object.AllClassifications))]
 internal static partial ObjectWire ToWire(Node.Object node);
 [MapperIgnoreSource(nameof(Node.Material.Id))]
 [MapProperty(nameof(Node.Material.Properties), nameof(MaterialWire.PropertySets))]
 internal static partial MaterialWire ToWire(Node.Material node);
 // The property bag's Groups is empty by construction (its nesting is the PropertyValue.Complex case) and
 // PropertySetWire declares no counterpart, so the source member is ignored EXPLICITLY — RequiredMappingStrategy.Both
 // faults an unmapped source member, and that fault is the signal a group-bearing property bag would owe a wire field.
 [MapperIgnoreSource(nameof(PropertyBag.Groups))]
 [MapProperty(nameof(PropertyBag.Source), nameof(PropertySetWire.SourceRank))]
 internal static partial PropertySetWire ToWire(PropertyBag bag);
 [MapProperty(nameof(QuantityBag.Source), nameof(QuantitySetWire.SourceRank))]
 internal static partial QuantitySetWire ToWire(QuantityBag bag);
 [MapProperty(nameof(AssessmentPayload.DependsOn), nameof(AssessmentWire.DependsOnIds))]
 internal static partial AssessmentWire ToWire(AssessmentPayload payload);
 internal static partial AppearanceWire ToWire(AppearanceSummary summary);
 internal static partial CoverageWire ToWire(CoverageGrid grid);
 [MapProperty(nameof(Relationship.Assign.Subject), nameof(AssignWire.SubjectId))]
 [MapProperty(nameof(Relationship.Assign.Definition), nameof(AssignWire.DefinitionId))]
 internal static partial AssignWire ToWire(Relationship.Assign edge);
 [MapProperty(nameof(Relationship.Associate.Subject), nameof(AssociateWire.SubjectId))]
 [MapProperty(nameof(Relationship.Associate.Resource), nameof(AssociateWire.ResourceId))]
 internal static partial AssociateWire ToWire(Relationship.Associate edge);
 [MapProperty(nameof(Relationship.Connect.From), nameof(ConnectWire.FromId))]
 [MapProperty(nameof(Relationship.Connect.To), nameof(ConnectWire.ToId))]
 [MapProperty(nameof(Relationship.Connect.Realizing), nameof(ConnectWire.RealizingId))]
 [MapProperty(nameof(Relationship.Connect.Interface), nameof(ConnectWire.InterfaceKey))]
 internal static partial ConnectWire ToWire(Relationship.Connect edge);
 [MapProperty(nameof(Relationship.Void.Host), nameof(VoidWire.HostId))]
 [MapProperty(nameof(Relationship.Void.Feature), nameof(VoidWire.FeatureId))]
 internal static partial VoidWire ToWire(Relationship.Void edge);
 [MapProperty(nameof(Header.Reference), nameof(HeaderWire.GeoReference))]
 [MapProperty(nameof(Header.Units), nameof(HeaderWire.UnitScheme))]
 internal static partial HeaderWire ToWire(Header header);
 internal static partial StepHeaderWire ToWire(StepHeader step);
 internal static partial SectionPropertiesWire ToWire(SectionProperties section);
 internal static partial MechanicalWire ToWire(MaterialPropertySet.Mechanical set);
 internal static partial OrthotropicWire ToWire(MaterialPropertySet.Orthotropic set);
 internal static partial ThermalWire ToWire(MaterialPropertySet.Thermal set);
 internal static partial FireWire ToWire(MaterialPropertySet.Fire set);
 internal static partial EnvironmentalWire ToWire(MaterialPropertySet.Environmental set);
 internal static partial CostWire ToWire(MaterialPropertySet.Cost set);
 internal static partial DurabilityWire ToWire(MaterialPropertySet.Durability set);
 internal static partial OpticalWire ToWire(MaterialPropertySet.Optical set);
 internal static partial TimeSliceWire ToWire(TimeSlice slice);

 // Existing-target carrier codecs for the get-only MapField members — the Mapperly update pattern the bag,
 // attribute, and results map transcriptions compose; keys cross as the PropertyName string, values recurse.
 [UserMapping] internal static void ToWire(Map<PropertyName, PropertyValue> values, [MappingTarget] MapField<string, PropertyValueWire> wire) { foreach (var (n, v) in values) { wire[n.Value] = ToWire(v); } }
 [UserMapping] internal static void ToWire(Map<PropertyName, MeasureValue> values, [MappingTarget] MapField<string, MeasureValueWire> wire) { foreach (var (n, m) in values) { wire[n.Value] = ToWire(m); } }
 // The group run keys on the dot-path prefix string (not a PropertyName), and each Option column writes CONDITIONALLY
 // so an unstated qualifier leaves its proto3 optional unset rather than crossing as an empty spelling.
 [UserMapping] internal static void ToWire(Map<string, GroupIdentity> groups, [MappingTarget] MapField<string, GroupIdentityWire> wire) { foreach (var (prefix, group) in groups) { GroupIdentityWire row = new(); group.Discrimination.IfSome(d => row.Discrimination = d); group.Quality.IfSome(q => row.Quality = q); group.Usage.IfSome(u => row.Usage = u); wire[prefix] = row; } }
 [UserMapping] internal static void ToWire(UnitScheme scheme, [MappingTarget] MapField<string, string> wire) { foreach (var (quantity, unit) in scheme.Display) { wire[quantity] = unit; } }

 [UserMapping] internal static ClassificationWire ToWire(Classification c) {
  ClassificationWire w = new() { System = c.System, Code = c.Code, Edition = c.Edition };
  c.Source.IfSome(s => w.Source = s);
  c.EditionDate.IfSome(d => w.EditionDate = NodaTime.Text.LocalDatePattern.Iso.Format(d));
  c.Title.IfSome(t => w.Title = t);
  return w;
 }

 // --- [CARRIER_CODECS] — the Option/tuple-flatten crossings Mapperly cannot bridge alone: an absent Option leaves
 // Proto3 field stays unset when a nullable return skips assignment; a flatten writes its column pair explicitly.
 [UserMapping] internal static string? ToWire(Option<string> value) => value.Match<string?>(static s => s, static () => null);
 // proto3 explicit presence over a scalar is a NULLABLE mapping target, so an absent criterion or count crosses
 // UNSET rather than as a zero a peer reads as a measured value.
 [UserMapping] internal static int? ToWire(Option<int> value) => value.Match<int?>(static v => v, static () => null);
 [UserMapping] internal static string? ToWire(Option<NodeId> id) => id.Match<string?>(static i => i.Value, static () => null);
 [UserMapping] internal static ByteString? ToWire(Option<UInt128> key) => key.Match<ByteString?>(static k => ToWire(k), static () => null);
 // The absence carrier over the registered NodaExtensions conversion — the presence decision is this seam's, the
 // conversion the static mapper's, so an absent instant leaves its proto3 optional unset.
 [UserMapping] internal static Timestamp? ToWire(Option<NodaTime.Instant> at) => at.Match<Timestamp?>(static i => i.ToTimestamp(), static () => null);

 [UserMapping] internal static void ToWire(RepresentationContentHash representations, [MappingTarget] MapField<string, ByteString> wire) { foreach (var (id, hash) in representations.ByIdentifier) { wire[id] = ToWire(hash); } }

 [UserMapping] internal static SchemaSpanWire ToWire(SchemaSpan span) {
  SchemaSpanWire w = new() { IntroducedIn = span.IntroducedIn.Key };
  span.RemovedIn.IfSome(r => w.RemovedIn = r.Key);
  return w;
 }

 // Every column is a plain crossing once the static temporal mappers and the Option<Instant> carrier are registered,
 // so the audit row generates whole and only its ABSENCE decision stays hand-owned.
 internal static partial OwnerHistoryWire ToWire(OwnerHistory history);
 [UserMapping] internal static OwnerHistoryWire? ToWire(Option<OwnerHistory> history) =>
  history.Match<OwnerHistoryWire?>(static h => ToWire(h), static () => null);

 // The placement frame flattens to nine ordered columns — a triple-flatten Mapperly bridges only through nine per-column
 // path rows, so the flatten is owned here beside the GeoReference precedent, and the frame's ABSENCE rides the same
 // nullable-return carrier every optional message crossing takes.
 [UserMapping] internal static PlacementWire ToWire(PlacementTransform placement) => new() {
  LocationX = placement.Location.X, LocationY = placement.Location.Y, LocationZ = placement.Location.Z,
  AxisX = placement.Axis.X, AxisY = placement.Axis.Y, AxisZ = placement.Axis.Z,
  RefDirectionX = placement.RefDirection.X, RefDirectionY = placement.RefDirection.Y, RefDirectionZ = placement.RefDirection.Z,
 };
 [UserMapping] internal static PlacementWire? ToWire(Option<PlacementTransform> placement) =>
  placement.Match<PlacementWire?>(static p => ToWire(p), static () => null);

 // Wire epsg/resolution columns are peer-informative derivations; blank ProjectedCrs strings stay unset.
 [UserMapping] internal static GeoReferenceWire ToWire(GeoReference geo) {
  GeoReferenceWire w = new() {
   Eastings = geo.Eastings, Northings = geo.Northings, OrthogonalHeight = geo.OrthogonalHeight,
   XAxisAbscissa = geo.XAxisAbscissa, XAxisOrdinate = geo.XAxisOrdinate,
   ScaleX = geo.ScaleX, ScaleY = geo.ScaleY, ScaleZ = geo.ScaleZ,
   GeodeticDatum = geo.GeodeticDatum,
  };
  geo.Vertical.IfSome(v => {
   w.VerticalDatum = v.Name;
   v.Epsg.IfSome(e => w.VerticalEpsg = e);
  });
  geo.Crs.IfSome(c => {
   ProjectedCrsWire p = new() { Name = c.Name, Resolution = c.Resolution.Key };
   c.Epsg.IfSome(e => p.Epsg = e);
   if (c.Wkt.Length > 0) { p.Wkt = c.Wkt; }
   if (c.MapProjection.Length > 0) { p.MapProjection = c.MapProjection; }
   if (c.MapZone.Length > 0) { p.MapZone = c.MapZone; }
   w.Crs = p;
  });
  geo.Epoch.IfSome(epoch => w.Epoch = epoch);
  return w;
 }

 [UserMapping] internal static ComposeWire ToWire(Relationship.Compose edge) {
  ComposeWire wire = new() { WholeId = edge.Whole.Value, PartId = edge.Part.Value, SubKind = edge.SubKind.Key };
  edge.Ordinal.IfSome(ordinal => wire.Ordinal = ordinal);
  return wire;
 }

 [UserMapping] internal static GenericWire ToWire(Relationship.Generic edge) {
  GenericWire wire = new() { WireName = edge.WireName, RelatingId = edge.Source.Value, RelatedId = edge.Target.Value };
  ToWire(edge.Attributes, wire.Attributes);
  wire.Participants.AddRange(edge.Participants.Map(participant => {
   RelationshipParticipantWire row = new() { NodeId = participant.Node.Value, Role = participant.Role };
   participant.Ordinal.IfSome(ordinal => row.Ordinal = ordinal);
   return row;
  }));
  return wire;
 }

 [UserMapping] internal static CoverageBandWire ToWire(CoverageBand band) {
  CoverageBandWire w = new() { Index = band.Index, Name = band.Name, SampleType = band.SampleType.Key, Role = band.Role.Key, Units = band.Units, Offset = band.Offset, Scale = band.Scale };
  band.NoData.IfSome(v => w.NoData = v);
  band.Range.IfSome(r => { w.RangeMin = r.Min; w.RangeMax = r.Max; });
  // The legend colour crosses through the SAME ToRgb quantizer CanonicalBytes takes, so the wire quadruple and the
  // content key are one projection — a second quantization here would let two runtimes agree on the key and disagree
  // on the swatch. The decoder re-admits through PerceptualColor.OfRgb, never a stored perceptual triple, because the
  // display quadruple is the only form both the key and every host palette surface already speak. Both calls stay
  // CONDITION-FREE for the same reason coverage#COVERAGE_NODE CanonicalBytes does: the kernel seats a viewing
  // condition on appearance-case payloads and never on ToRgb, and a gamut or observer argument admitted at either
  // end alone splits the wire from the key it is defined to agree with.
  w.Palette.AddRange(band.Palette.Map(static c => {
   (byte r, byte g, byte b, byte a) = c.Colour.ToRgb();
   return new ColorBinWire { Index = c.Index, R = r, G = g, B = b, A = a, Category = c.Category };
  }));
  return w;
 }

 // The kernel placement crosses as its twelve index-to-world coefficients plus the census and ceiling the decoder
 // re-admits with — the fourth matrix row is the invariant [0 0 0 1] and carries no information, so twelve IS the
 // whole affine and a thirteenth column would be a value the receiver already knows.
 [UserMapping] internal static CellLatticeWire ToWire(CellLattice lattice) {
  CellLatticeWire w = new() { Columns = lattice.Columns.Value, Rows = lattice.Rows.Value, Layers = lattice.Layers.Value, Ceiling = lattice.Ceiling };
  w.Affine.AddRange(lattice.Affine);
  return w;
 }

 [UserMapping] internal static ProvenanceWire ToWire(Provenance p) {
  ProvenanceWire w = new() { Author = p.Author, Tool = p.Tool, Version = p.Version, At = p.At.ToTimestamp(), Elapsed = p.Elapsed.ToProtobufDuration(), Attempt = p.Attempt };
  p.Window.IfSome(i => { w.WindowStart = i.Start.ToTimestamp(); w.WindowEnd = i.End.ToTimestamp(); });
  // `CorrelationId` carries the kernel's own `ISpanFormattable` "D" render, so the wire text and the
  // `Guid.TryParse` decode below stay one round-trippable spelling.
  p.Correlation.IfSome(c => w.Correlation = c.ToString("D", CultureInfo.InvariantCulture));
  return w;
 }

 [UserMapping] internal static DiagnosticWire? ToWire(Option<Diagnostic> diagnostic) => diagnostic.Match<DiagnosticWire?>(
  static d => { DiagnosticWire w = new() { Phase = d.Phase.Key, Kind = d.Kind.Key, Message = d.Message }; d.Code.IfSome(c => w.Code = c); return w; },
  static () => null);

 [UserMapping] internal static PropertyEvidenceWire ToWire(PropertyEvidence evidence) {
  PropertyEvidenceWire w = new() { Source = evidence.Source, Reference = evidence.Reference };
  evidence.ValidUntil.IfSome(d => w.ValidUntil = NodaTime.Text.LocalDatePattern.Iso.Format(d));
  return w;
 }

 // MeasureValue crosses as its identity columns; the wire NEVER carries CanonicalUnit — decode re-mints it
 // through the OfSi registry resolve and re-attaches the band, so wire and canon agree by construction.
 // Encode stays the total [UserMapping]; DECODE is Fin — the OfSi finite gate below.
 [UserMapping] internal static MeasureValueWire ToWire(MeasureValue m) {
  MeasureValueWire w = new() {
   QuantityType = m.Type.Value, Si = m.Si,
   DimLength = m.Dimension.Length, DimMass = m.Dimension.Mass, DimTime = m.Dimension.Time,
   DimCurrent = m.Dimension.Current, DimTemperature = m.Dimension.Temperature,
   DimAmount = m.Dimension.Amount, DimLuminousIntensity = m.Dimension.LuminousIntensity,
  };
  m.Uncertainty.IfSome(b => w.Uncertainty = ToWire(b));
  return w;
 }
 // Fin-railed decode through the OWNER's OfSi finite gate — a hostile NaN/∞ scalar rails ValueRejected exactly as
 // an in-process SI-native mint does, never a decoder-local finite check; the keyless interior fault re-keys here.
 internal static Fin<MeasureValue> ToMeasure(MeasureValueWire w, Op key) =>
  MeasureValue.OfSi(
    QuantityType.Create(w.QuantityType),
    Dimension.Create(w.DimLength, w.DimMass, w.DimTime, w.DimCurrent, w.DimTemperature, w.DimAmount, w.DimLuminousIntensity),
    w.Si)
   .MapFail(_ => (Error)ElementFault.ValueRejected(key, $"<wire-measure-non-finite:{w.QuantityType}>"))
   .Bind(m => w.Uncertainty is null
    ? Fin.Succ(m)
    : ToBand(w.Uncertainty, key).Bind(band => m.WithUncertainty(band, key)));
 [UserMapping] internal static MeasureBandWire ToWire(MeasureBand band) {
  MeasureBandWire w = new() { Kind = band.Kind.Key, LowerSi = band.LowerSi, UpperSi = band.UpperSi };
  band.StandardDeviationSi.IfSome(sd => w.StandardDeviationSi = sd);
  band.CoverageFactor.IfSome(k => w.CoverageFactor = k);
  return w;
 }
 [UserMapping] internal static Fin<MeasureBand> ToBand(MeasureBandWire w, Op key) =>
  UncertaintyKind.TryGet(w.Kind, out UncertaintyKind? kind) && kind is { } row
   ? MeasureBand.Admit(
      row, w.LowerSi, w.UpperSi,
      Opt(w.HasStandardDeviationSi, w.StandardDeviationSi), Opt(w.HasCoverageFactor, w.CoverageFactor), key)
   : ElementFault.ValueRejected(key, $"<wire-uncertainty-kind:{w.Kind}>");

 // One envelope fold per union uses generated total Switch; a new case breaks compilation.
 internal static NodeWire ToWire(Node node, double tolerance) {
  NodeWire wire = node.Switch<NodeWire>(
   @object: o => new() { Id = o.Id.Value, Object = ToWire(o) },
   material: m => new() { Id = m.Id.Value, Material = ToWire(m) },
   propertySet: p => new() { Id = p.Id.Value, PropertySet = ToWire(p.Bag) },
   quantitySet: q => new() { Id = q.Id.Value, QuantitySet = ToWire(q.Bag) },
   assessment: a => new() { Id = a.Id.Value, Assessment = ToWire(a.Payload) },
   appearance: a => new() { Id = a.Id.Value, Appearance = ToWire(a.Summary) },
   coverage: c => new() { Id = c.Id.Value, Coverage = ToWire(c.Grid) },
   observation: o => new() { Id = o.Id.Value, Observation = ToWire(o.Series) });
  wire.ContentAddress = ToWire(ContentAddress.Of(node, tolerance).Value);
  return wire;
 }

 // Hand-owned like ToWire(GeoReference): the Interval flattens to a bounded column PAIR and the census map keys on a
 // generated row, neither a shape Mapperly bridges. Both window ends are bounded by seam admission, so the columns
 // are unconditional and no presence flag stands in for an unbounded side.
 [UserMapping] internal static ObservationWire ToWire(ObservationSeries series) {
  ObservationWire w = new() {
   Sensor = series.Sensor.Value, Aspect = series.Aspect.Value, Observed = series.Observed.Value,
   DimLength = series.Signature.Length, DimMass = series.Signature.Mass, DimTime = series.Signature.Time,
   DimCurrent = series.Signature.Current, DimTemperature = series.Signature.Temperature,
   DimAmount = series.Signature.Amount, DimLuminousIntensity = series.Signature.LuminousIntensity,
   CanonicalUnit = series.CanonicalUnit, Sampling = series.Sampling.Key,
   WindowStart = series.Window.Start.ToTimestamp(), WindowEnd = series.Window.End.ToTimestamp(),
   Statistics = ToWire(series.Statistics), Provenance = ToWire(series.Provenance),
  };
  series.Cadence.IfSome(cadence => w.Cadence = cadence.ToProtobufDuration());
  w.Chunks.AddRange(series.Chunks.Map(static chunk => new ObservationChunkWire {
   WindowStart = chunk.Window.Start.ToTimestamp(), WindowEnd = chunk.Window.End.ToTimestamp(),
   SeriesKey = ToWire(chunk.SeriesKey), SampleCount = chunk.SampleCount,
  }));
  return w;
 }

 [UserMapping] internal static SensorProvenanceWire ToWire(SensorProvenance provenance) {
  SensorProvenanceWire w = new() { Manufacturer = provenance.Manufacturer, Model = provenance.Model, Serial = provenance.Serial };
  provenance.CalibratedAt.IfSome(date => w.CalibratedAt = NodaTime.Text.LocalDatePattern.Iso.Format(date));
  provenance.Tolerance.IfSome(band => w.Tolerance = ToWire(band));
  return w;
 }

 [UserMapping] internal static SeriesStatisticsWire ToWire(SeriesStatistics statistics) {
  SeriesStatisticsWire w = new() { Span = statistics.Span.ToProtobufDuration() };
  foreach ((ObservationGrade grade, int count) in statistics.Census) { w.Census[grade.Key] = count; }
  statistics.Minimum.IfSome(measure => w.Minimum = ToWire(measure));
  statistics.Maximum.IfSome(measure => w.Maximum = ToWire(measure));
  statistics.Mean.IfSome(measure => w.Mean = ToWire(measure));
  statistics.Total.IfSome(measure => w.Total = ToWire(measure));
  return w;
 }

 internal static RelationshipWire ToWire(Relationship edge) => edge.Switch<RelationshipWire>(
  compose: e => new() { Compose = ToWire(e) },
  assign: e => new() { Assign = ToWire(e) },
  associate: e => new() { Associate = ToWire(e) },
  connect: e => new() { Connect = ToWire(e) },
  @void: e => new() { Void = ToWire(e) },
  generic: e => new() { Generic = ToWire(e) });

 internal static PropertyValueWire ToWire(PropertyValue value) => value.Switch<PropertyValueWire>(
  text: v => new() { Text = v.Value },
  measure: v => new() { Measure = ToWire(v.Value) },
  boolean: v => new() { Boolean = v.Value },
  logical: v => { LogicalWire l = new(); v.Value.IfSome(b => l.Value = b); return new() { Logical = l }; },
  enumerated: v => { EnumeratedWire e = new(); e.Selected.AddRange(v.Selected.Map(ToWire)); e.Allowed.AddRange(v.Allowed.Map(ToWire)); return new() { Enumerated = e }; },
  reference: v => { ReferenceWire r = new() { TargetId = v.Target.Value }; v.UsageName.IfSome(u => r.UsageName = u); return new() { Reference = r }; },
  bounded: v => { BoundedWire b = new(); v.Lower.IfSome(m => b.Lower = ToWire(m)); v.Upper.IfSome(m => b.Upper = ToWire(m)); v.SetPoint.IfSome(m => b.SetPoint = ToWire(m)); return new() { Bounded = b }; },
  list: v => { ListWire l = new(); l.Values.AddRange(v.Values.Map(ToWire)); return new() { List = l }; },
  table: v => { TableWire t = new() { Interpolation = v.Interp.Key }; t.Rows.AddRange(v.Rows.Map(r => new TableRowWire { Defining = ToWire(r.Defining), Defined = ToWire(r.Defined) })); return new() { Table = t }; },
  complex: v => { ComplexWire c = new() { UsageName = v.UsageName }; foreach (var (n, inner) in v.Properties) { c.Properties[n.Value] = ToWire(inner); } return new() { Complex = c }; },
  temporal: v => new() { Temporal = v.Value.Switch<TemporalWire>(
   date: static t => new() { Date = NodaTime.Text.LocalDatePattern.Iso.Format(t.Value) },
   moment: static t => new() { Moment = NodaTime.Text.LocalDateTimePattern.ExtendedIso.Format(t.Value) },
   time: static t => new() { Time = NodaTime.Text.LocalTimePattern.ExtendedIso.Format(t.Value) },
   span: static t => new() { Span = NodaTime.Text.PeriodPattern.Roundtrip.Format(t.Value) },
   stamp: static t => new() { Stamp = t.Value.ToTimestamp() }) },
  integer: static v => new() { Integer = ByteString.CopyFrom(v.Value.ToByteArray(isUnsigned: false, isBigEndian: true)) },
  number: static v => new() { Number = v.Value },
  binary: static v => new() { Binary = ByteString.CopyFrom(v.Value.ToArray()) });

 internal static MaterialUsageWire ToWire(MaterialUsage usage) => usage.Switch<MaterialUsageWire>(
  none: static _ => new() { None = new Google.Protobuf.WellKnownTypes.Empty() },
  layerSet: u => { LayerSetUsageWire wire = new() { Direction = u.Direction.Key, Sense = u.Sense.Key }; u.OffsetFromReferenceLine.IfSome(value => wire.OffsetFromReferenceLine = ToWire(value)); u.ReferenceExtent.IfSome(value => wire.ReferenceExtent = ToWire(value)); return new() { LayerSet = wire }; },
  profileSet: u => { ProfileSetUsageWire wire = new(); u.CardinalPoint.IfSome(value => wire.CardinalPoint = value.Key); u.ReferenceExtent.IfSome(value => wire.ReferenceExtent = ToWire(value)); return new() { ProfileSet = wire }; });

 // Every optional row column writes through explicit protobuf presence — an IfSome assignment, never a defaulted zero or
 // false that a decoder cannot distinguish from an author's real value.
 internal static MaterialCompositionWire ToWire(MaterialComposition composition) => composition.Switch<MaterialCompositionWire>(
  single: c => new() { Single = new SingleWire { MaterialKey = c.Material.Value } },
  layerSet: c => { LayerSetWire w = new(); w.Layers.AddRange(c.Layers.Map(static l => ToWire(l))); return new() { LayerSet = w }; },
  profileSet: c => { ProfileSetWire w = new(); w.Profiles.AddRange(c.Profiles.Map(static p => ToWire(p))); c.Composite.IfSome(r => w.Composite = ToWire(r)); c.Section.IfSome(s => w.Section = ToWire(s)); return new() { ProfileSet = w }; },
  constituentSet: c => { ConstituentSetWire w = new(); w.Constituents.AddRange(c.Constituents.Map(static x => new MaterialConstituentWire { MaterialKey = x.Material.Value, Category = x.Category, Fraction = x.Fraction, PartName = x.PartName })); return new() { ConstituentSet = w }; });

 internal static MaterialLayerWire ToWire(MaterialLayer layer) {
  MaterialLayerWire w = new() { MaterialKey = layer.Material.Value, Thickness = ToWire(layer.Thickness), LayerName = layer.LayerName, Category = layer.Category };
  layer.Priority.IfSome(p => w.Priority = p);
  layer.Ventilated.IfSome(v => w.Ventilated = v);
  return w;
 }

 internal static MaterialProfileWire ToWire(MaterialProfile profile) {
  MaterialProfileWire w = new() { MaterialKey = profile.Material.Value, Profile = ToWire(profile.Profile), Category = profile.Category };
  profile.Priority.IfSome(p => w.Priority = p);
  w.Offsets.AddRange(profile.Offsets.Map(static o => ToWire(o)));
  return w;
 }

 // ONE ProfileRef projection serves the row and the set-level composite — a second inline construction is the fork
 // that lets one leg drop the content key the Rehydrate gate re-checks.
 internal static ProfileRefWire ToWire(ProfileRef profile) =>
  new() { Standard = profile.Standard, Designation = profile.Designation, ContentKey = ToWire(profile.ContentKey) };

 // Evidence rides the envelope (the base-class column), each arm its generated flat mapping; the Acoustic/Damping/
 // Hygrothermal arms carry Option columns, so their bodies are owned here beside the fold.
 internal static MaterialPropertySetWire ToWire(MaterialPropertySet set) => set.Switch<MaterialPropertySetWire>(
  mechanical: x => new() { Evidence = ToWire(x.Evidence), Mechanical = ToWire(x) },
  orthotropic: x => new() { Evidence = ToWire(x.Evidence), Orthotropic = ToWire(x) },
  thermal: x => new() { Evidence = ToWire(x.Evidence), Thermal = ToWire(x) },
  acoustic: x => new() { Evidence = ToWire(x.Evidence), Acoustic = ToWire(x) },
  fire: x => new() { Evidence = ToWire(x.Evidence), Fire = ToWire(x) },
  environmental: x => new() { Evidence = ToWire(x.Evidence), Environmental = ToWire(x) },
  cost: x => new() { Evidence = ToWire(x.Evidence), Cost = ToWire(x) },
  damping: x => new() { Evidence = ToWire(x.Evidence), Damping = ToWire(x) },
  hygrothermal: x => new() { Evidence = ToWire(x.Evidence), Hygrothermal = ToWire(x) },
  durability: x => new() { Evidence = ToWire(x.Evidence), Durability = ToWire(x) },
  optical: x => new() { Evidence = ToWire(x.Evidence), Optical = ToWire(x) });

 [UserMapping] internal static AcousticWire ToWire(MaterialPropertySet.Acoustic set) {
  AcousticWire w = new();
  w.AbsorptionSpectrum.AddRange(set.AbsorptionSpectrum);
  w.SoundReductionIndexDb.AddRange(set.SoundReductionIndexDb);
  set.DynamicStiffnessMNPerM3.IfSome(v => w.DynamicStiffnessMnPerM3 = v);
  set.FlowResistivityPaSPerM2.IfSome(v => w.FlowResistivityPaSPerM2 = v);
  set.LossFactor.IfSome(v => w.LossFactor = v);
  return w;
 }
 [UserMapping] internal static DampingWire ToWire(MaterialPropertySet.Damping set) {
  DampingWire w = new() { DampingRatio = set.DampingRatio };
  set.Rayleigh.IfSome(r => w.Rayleigh = new RayleighWire { AlphaPerS = r.AlphaPerS, BetaS = r.BetaS });
  return w;
 }
 [UserMapping] internal static HygrothermalWire ToWire(MaterialPropertySet.Hygrothermal set) {
  HygrothermalWire w = new() { Porosity = set.Porosity, WaterContent80Rh = ToWire(set.WaterContent80Rh), FreeWaterSaturation = ToWire(set.FreeWaterSaturation) };
  set.WaterAbsorptionKgPerM2SqrtS.IfSome(v => w.WaterAbsorptionKgPerM2SqrtS = v);
  set.SorptionIsotherm.IfSome(curve => w.SorptionIsotherm = ToWire(curve));
  set.LiquidTransport.IfSome(curve => w.LiquidTransport = ToWire(curve));
  set.MoistureConductivity.IfSome(curve => w.MoistureConductivity = ToWire(curve));
  return w;
 }

 [UserMapping] internal static SampledCurveWire ToWire(SampledCurve curve) {
  SampledCurveWire wire = new();
  wire.Axis.AddRange(curve.Axis);
  wire.Values.AddRange(curve.Values);
  return wire;
 }

 // --- [DECODE_DISPATCH] — the generated closed PayloadCase/EdgeCase/ValueCase/UsageCase enums own decode
 // dispatch (an unset case rails ValueRejected, a new oneof arm surfaces as an unhandled enum member); every
 // value re-crosses the SAME seam gates an in-process author does — admitted, never trusted raw.
 internal static Fin<Node> ToNode(NodeWire w, Op key) {
  NodeId id = NodeId.Create(w.Id);                                     // verbatim — never re-derived
  return w.PayloadCase switch {
   NodeWire.PayloadOneofCase.Object => ToObject(id, w.Object, key),
   NodeWire.PayloadOneofCase.Material => ToMaterial(id, w.Material, key),
   NodeWire.PayloadOneofCase.PropertySet => ToBag(w.PropertySet, key).Map(bag => (Node)new Node.PropertySet(id, bag)),
   NodeWire.PayloadOneofCase.QuantitySet => ToBag(w.QuantitySet, key).Map(bag => (Node)new Node.QuantitySet(id, bag)),
   NodeWire.PayloadOneofCase.Assessment => ToAssessment(w.Assessment, key).Map(payload => (Node)new Node.Assessment(id, payload)),
   NodeWire.PayloadOneofCase.Appearance => AppearanceSummary.Rehydrate(
    ToKey(w.Appearance.AppearanceKey), w.Appearance.BaseColorR, w.Appearance.BaseColorG, w.Appearance.BaseColorB,
    w.Appearance.Metallic, w.Appearance.Roughness, w.Appearance.Opacity, w.Appearance.Transmissive, key)
    .Map(summary => (Node)new Node.Appearance(id, summary)),
   NodeWire.PayloadOneofCase.Coverage => ToCoverage(w.Coverage, key).Map(grid => (Node)new Node.Coverage(id, grid)),
   NodeWire.PayloadOneofCase.Observation => ToObservation(w.Observation, key).Map(series => (Node)new Node.Observation(id, series)),
   _ => ElementFault.ValueRejected(key, "<wire-node-payload-none>"),
  };
 }

 internal static Fin<Relationship> ToEdge(RelationshipWire w, Op key) => w.EdgeCase switch {
  RelationshipWire.EdgeOneofCase.Compose => Row(ComposeKind.TryGet(w.Compose.SubKind, out ComposeKind? ck), ck, w.Compose.SubKind, key)
   .Map(k => (Relationship)new Relationship.Compose(
    NodeId.Create(w.Compose.WholeId), NodeId.Create(w.Compose.PartId), k,
    w.Compose.HasOrdinal ? Some(w.Compose.Ordinal) : None)),
  RelationshipWire.EdgeOneofCase.Assign => Row(AssignKind.TryGet(w.Assign.SubKind, out AssignKind? ak), ak, w.Assign.SubKind, key)
   .Map(k => (Relationship)new Relationship.Assign(NodeId.Create(w.Assign.SubjectId), NodeId.Create(w.Assign.DefinitionId), k)),
  RelationshipWire.EdgeOneofCase.Associate => ToUsage(w.Associate.Usage, key)
   .Map(u => (Relationship)new Relationship.Associate(NodeId.Create(w.Associate.SubjectId), NodeId.Create(w.Associate.ResourceId), u)),
  RelationshipWire.EdgeOneofCase.Connect => Row(ConnectKind.TryGet(w.Connect.SubKind, out ConnectKind? nk), nk, w.Connect.SubKind, key)
   .Map(k => (Relationship)new Relationship.Connect(NodeId.Create(w.Connect.FromId), NodeId.Create(w.Connect.ToId), k,
    w.Connect.HasRealizingId ? Some(NodeId.Create(w.Connect.RealizingId)) : None,
    w.Connect.HasInterfaceKey ? Some(ToKey(w.Connect.InterfaceKey)) : None)),
  RelationshipWire.EdgeOneofCase.Void => Row(VoidKind.TryGet(w.Void.SubKind, out VoidKind? vk), vk, w.Void.SubKind, key)
   .Map(k => (Relationship)new Relationship.Void(NodeId.Create(w.Void.HostId), NodeId.Create(w.Void.FeatureId), k)),
  RelationshipWire.EdgeOneofCase.Generic => ToValueMap(w.Generic.Attributes, key)
   .Map(attributes => (Relationship)new Relationship.Generic(
    w.Generic.WireName, NodeId.Create(w.Generic.RelatingId), NodeId.Create(w.Generic.RelatedId), attributes,
    toSeq(w.Generic.Participants).Map(participant => new RelationshipParticipant(
     NodeId.Create(participant.NodeId), participant.Role, participant.HasOrdinal ? Some(participant.Ordinal) : None)))),
  _ => ElementFault.ValueRejected(key, "<wire-edge-none>"),
 };

 // Build the tree raw off the closed ValueCase, then ONE PropertyValue.Of at the envelope — Of recurses the
 // composites itself, so the structural admission runs exactly once over the whole decoded value.
 internal static Fin<PropertyValue> ToValue(PropertyValueWire w, Op key) => RawValue(w, key).Bind(v => PropertyValue.Of(v, key));

 static Fin<PropertyValue> RawValue(PropertyValueWire w, Op key) => w.ValueCase switch {
  PropertyValueWire.ValueOneofCase.Text => Fin.Succ((PropertyValue)new PropertyValue.Text(w.Text)),
  PropertyValueWire.ValueOneofCase.Measure => ToMeasure(w.Measure, key).Map(static m => (PropertyValue)new PropertyValue.Measure(m)),
  PropertyValueWire.ValueOneofCase.Boolean => Fin.Succ((PropertyValue)new PropertyValue.Boolean(w.Boolean)),
  PropertyValueWire.ValueOneofCase.Logical => Fin.Succ((PropertyValue)new PropertyValue.Logical(w.Logical.HasValue ? Some(w.Logical.Value) : None)),
  PropertyValueWire.ValueOneofCase.Enumerated => toSeq(w.Enumerated.Selected).TraverseM(v => RawValue(v, key)).As().Bind(selected =>
   toSeq(w.Enumerated.Allowed).TraverseM(v => RawValue(v, key)).As().Map(allowed => (PropertyValue)new PropertyValue.Enumerated(selected, allowed))),
  PropertyValueWire.ValueOneofCase.Reference => Fin.Succ((PropertyValue)new PropertyValue.Reference(NodeId.Create(w.Reference.TargetId), w.Reference.HasUsageName ? Some(w.Reference.UsageName) : None)),
  PropertyValueWire.ValueOneofCase.Bounded =>
   (OptMeasure(w.Bounded.Lower, key), OptMeasure(w.Bounded.Upper, key), OptMeasure(w.Bounded.SetPoint, key))
    .Apply(static (lower, upper, setPoint) => (PropertyValue)new PropertyValue.Bounded(lower, upper, setPoint)).As(),
  PropertyValueWire.ValueOneofCase.List => toSeq(w.List.Values).TraverseM(v => RawValue(v, key)).As().Map(vs => (PropertyValue)new PropertyValue.List(vs)),
  PropertyValueWire.ValueOneofCase.Table => Row(Interpolation.TryGet(w.Table.Interpolation, out Interpolation? rule), rule, w.Table.Interpolation, key)
   .Bind(interp => toSeq(w.Table.Rows).TraverseM(r => RawValue(r.Defining, key).Bind(d => RawValue(r.Defined, key).Map(x => (Defining: d, Defined: x)))).As()
    .Map(rows => (PropertyValue)new PropertyValue.Table(rows, interp))),
  PropertyValueWire.ValueOneofCase.Complex => toSeq(w.Complex.Properties).TraverseM(p => RawValue(p.Value, key).Map(v => (Name: PropertyName.Create(p.Key), Value: v))).As()
   .Map(pairs => (PropertyValue)new PropertyValue.Complex(w.Complex.UsageName, pairs.Fold(Map<PropertyName, PropertyValue>(), static (m, p) => m.Add(p.Name, p.Value)))),
  PropertyValueWire.ValueOneofCase.Temporal => ToTemporal(w.Temporal, key).Map(static t => (PropertyValue)new PropertyValue.Temporal(t)),
  PropertyValueWire.ValueOneofCase.Integer => Fin.Succ((PropertyValue)new PropertyValue.Integer(new BigInteger(w.Integer.Span, isUnsigned: false, isBigEndian: true))),
  PropertyValueWire.ValueOneofCase.Number => Fin.Succ((PropertyValue)new PropertyValue.Number(w.Number)),
  PropertyValueWire.ValueOneofCase.Binary => Fin.Succ((PropertyValue)new PropertyValue.Binary(toSeq(w.Binary.ToByteArray()))),
  _ => ElementFault.ValueRejected(key, "<wire-value-none>"),
 };

 // TemporalValue arms re-admit through NodaTime ISO patterns (the seam Iso() canon reversed); a malformed
 // token rails ValueRejected, the epoch stamp rides the Timestamp adapter untouched.
 static Fin<TemporalValue> ToTemporal(TemporalWire w, Op key) => w.ValueCase switch {
  TemporalWire.ValueOneofCase.Date => Iso(NodaTime.Text.LocalDatePattern.Iso, w.Date, key).Map(static v => (TemporalValue)new TemporalValue.Date(v)),
  TemporalWire.ValueOneofCase.Moment => Iso(NodaTime.Text.LocalDateTimePattern.ExtendedIso, w.Moment, key).Map(static v => (TemporalValue)new TemporalValue.Moment(v)),
  TemporalWire.ValueOneofCase.Time => Iso(NodaTime.Text.LocalTimePattern.ExtendedIso, w.Time, key).Map(static v => (TemporalValue)new TemporalValue.Time(v)),
  TemporalWire.ValueOneofCase.Span => Iso(NodaTime.Text.PeriodPattern.Roundtrip, w.Span, key).Map(static v => (TemporalValue)new TemporalValue.Span(v)),
  TemporalWire.ValueOneofCase.Stamp => Fin.Succ((TemporalValue)new TemporalValue.Stamp(w.Stamp.ToInstant())),
  _ => ElementFault.ValueRejected(key, "<wire-temporal-none>"),
 };

 static Fin<T> Iso<T>(NodaTime.Text.IPattern<T> pattern, string token, Op key) =>
  pattern.Parse(token) is { Success: true } parsed ? Fin.Succ(parsed.Value) : ElementFault.ValueRejected(key, $"<wire-temporal:{token}>");

 internal static Fin<MaterialUsage> ToUsage(MaterialUsageWire? w, Op key) => w?.UsageCase switch {
  MaterialUsageWire.UsageOneofCase.None => Fin.Succ((MaterialUsage)new MaterialUsage.None()),
  MaterialUsageWire.UsageOneofCase.LayerSet =>
   from direction in Row(LayerSetDirection.TryGet(w.LayerSet.Direction, out LayerSetDirection? direction), direction, w.LayerSet.Direction, key)
   from sense in Row(DirectionSense.TryGet(w.LayerSet.Sense, out DirectionSense? sense), sense, w.LayerSet.Sense, key)
   from offset in OptMeasure(w.LayerSet.OffsetFromReferenceLine, key)
   from extent in OptMeasure(w.LayerSet.ReferenceExtent, key)
   from usage in MaterialUsage.LayerSet.Of(direction, sense, offset, extent, key)
   select usage,
  MaterialUsageWire.UsageOneofCase.ProfileSet =>
   from extent in OptMeasure(w.ProfileSet.ReferenceExtent, key)
   from usage in MaterialUsage.ProfileSet.Of(w.ProfileSet.HasCardinalPoint ? Some(w.ProfileSet.CardinalPoint) : None, extent, key)
   select usage,
  null => ElementFault.ValueRejected(key, "<wire-usage-unset>"),
  _ => ElementFault.ValueRejected(key, "<wire-usage-unknown>"),
 };

 internal static Fin<Header> ToHeader(HeaderWire w, Op key) =>
  !ReleaseVersion.TryGet(w.Schema, out ReleaseVersion? schema) ? ElementFault.ValueRejected(key, $"<wire-schema:{w.Schema}>")
  : !ModelView.TryGet(w.View, out ModelView? view) ? ElementFault.ValueRejected(key, $"<wire-view:{w.View}>")
  : ToGeoReference(w.GeoReference, key).Map(geo => new Header(schema!, view!, geo, w.Tolerance, w.At.ToInstant(),
    new StepHeader(toSeq(w.Step.Descriptions), w.Step.Name, w.Step.TimeStamp.ToInstant(), toSeq(w.Step.Authors),
     toSeq(w.Step.Organizations), w.Step.Preprocessor, w.Step.OriginatingSystem, toSeq(w.Step.Schema)),
    new UnitScheme(w.UnitScheme.Aggregate(Map<string, string>(), static (m, p) => m.Add(p.Key, p.Value)))));

 // --- [DECODE_PAYLOADS] — per-payload re-admission over the verified seam factories.
 static Fin<Node> ToObject(NodeId id, ObjectWire w, Op key) =>
  !ObjectKind.TryGet(w.Kind, out ObjectKind? kind) ? ElementFault.ValueRejected(key, $"<wire-object-kind:{w.Kind}>")
  : ToClassification(w.Classification, key).Bind(primary =>
    toSeq(w.Classifications).TraverseM(c => ToClassification(c, key)).As().Bind(secondary =>
     ToSpan(w.Span, key).Map(span => (Node)new Node.Object(
      id, kind!, w.HasExternalId ? Some(w.ExternalId) : None, primary, PredefinedType.Create(w.PredefinedType),
      w.HasObjectType ? Some(w.ObjectType) : None, w.Name, w.Tag,
      new RepresentationContentHash(w.Representations.Aggregate(Map<string, UInt128>(), static (m, p) => m.Add(p.Key, ToKey(p.Value)))),
      w.History is null ? None : Some(new OwnerHistory(w.History.OwningUser, w.History.OwningApplication, w.History.Created.ToInstant(),
       w.History.Modified is null ? None : Some(w.History.Modified.ToInstant()), w.History.ChangeAction, w.History.State)),
      span, secondary, ToPlacement(w.Placement)))));

 // The frame re-admits through its own kernel factory: the nine columns are free reals under no seam gate (a placement
 // carries no tolerance and no invariant — the canonical-bytes exclusion at its owner is what makes it free), so
 // message presence answers the whole decision and no rail is owed. The bare Vector3 is the enclosing namespace's
 // seam coordinate, never the System.Numerics carrier the prelude also has in scope.
 static Option<PlacementTransform> ToPlacement(PlacementWire? w) =>
  w is null
   ? None
   : Some(PlacementTransform.Create(
      new Vector3(w.LocationX, w.LocationY, w.LocationZ),
      new Vector3(w.AxisX, w.AxisY, w.AxisZ),
      new Vector3(w.RefDirectionX, w.RefDirectionY, w.RefDirectionZ)));

 static Fin<Node> ToMaterial(NodeId id, MaterialWire w, Op key) =>
  ToComposition(w.Composition, key).Bind(composition =>
   toSeq(w.PropertySets).TraverseM(p => ToPropertySet(p, key)).As().Map(sets =>
    (Node)new Node.Material(id, MaterialId.Of(w.MaterialKey), composition, sets)));

 // Every arm re-enters the seam Of* admission (the row-count, thickness, priority-range, offset-arity, and normalization
 // gates hold for hostile wire bytes exactly as for an in-process author), and each optional row column reads through the
 // generated Has* presence probe — a defaulted zero priority or false ventilation never forges an author's value. The
 // ProfileSet arm admits the rows FIRST and stamps the baked section afterwards through WithSection, so the private-ctor
 // case is never constructed directly and the head-row derivation stays total.
 static Fin<MaterialComposition> ToComposition(MaterialCompositionWire w, Op key) => w.CompositionCase switch {
  MaterialCompositionWire.CompositionOneofCase.Single => Fin.Succ(MaterialComposition.OfSingle(MaterialId.Of(w.Single.MaterialKey))),
  MaterialCompositionWire.CompositionOneofCase.LayerSet =>
   toSeq(w.LayerSet.Layers).TraverseM(l => ToMeasure(l.Thickness, key).Map(t => new MaterialLayer(
     MaterialId.Of(l.MaterialKey), t, l.LayerName,
     l.HasPriority ? Some(l.Priority) : Option<int>.None, l.Category,
     l.HasVentilated ? Some(l.Ventilated) : Option<bool>.None))).As()
    .Bind(layers => MaterialComposition.OfLayerSet(layers, key)),
  MaterialCompositionWire.CompositionOneofCase.ProfileSet =>
   from profiles in toSeq(w.ProfileSet.Profiles).TraverseM(p => ToProfile(p, key)).As()
   from composite in w.ProfileSet.Composite is null ? Fin.Succ(Option<ProfileRef>.None) : ToProfileRef(w.ProfileSet.Composite, key).Map(Some)
   from admitted in MaterialComposition.OfProfileSet(profiles, key, composite)
   from section in w.ProfileSet.Section is null ? Fin.Succ(Option<SectionProperties>.None) : ToSection(w.ProfileSet.Section, key).Map(Some)
   select section.Match(Some: admitted.WithSection, None: () => admitted),
  MaterialCompositionWire.CompositionOneofCase.ConstituentSet => MaterialComposition.OfConstituentSet(
   toSeq(w.ConstituentSet.Constituents).Map(c => new MaterialConstituent(MaterialId.Of(c.MaterialKey), c.Category, c.Fraction, c.PartName)), key),
  _ => ElementFault.ValueRejected(key, "<wire-composition-none>"),
 };

 // One compound-profile row: every offset re-crosses the MeasureValue finite gate beside the row's own ProfileRef admission.
 static Fin<MaterialProfile> ToProfile(MaterialProfileWire w, Op key) =>
  from profile in ToProfileRef(w.Profile, key)
  from offsets in toSeq(w.Offsets).TraverseM(o => ToMeasure(o, key)).As()
  select new MaterialProfile(MaterialId.Of(w.MaterialKey), profile, w.HasPriority ? Some(w.Priority) : Option<int>.None, w.Category, offsets);

 // ONE ProfileRef admission serves the row and the set-level composite: Rehydrate re-derives the content key off the
 // normalized (standard, designation) and rails when a persisted key disagrees, so no wire leg trusts a carried digest.
 static Fin<ProfileRef> ToProfileRef(ProfileRefWire w, Op key) =>
  ProfileRef.Rehydrate(w.Standard, w.Designation, ToKey(w.ContentKey), key);

 // ONE column table owns the section's measured run: each row pairs the wire slot's own name with its accessor, and
 // ROW POSITION is simultaneously the traversal order, the frozen SectionPropertiesWire field order, and the ctor
 // position — so a slot moves once and both directions follow. The slot name is load-bearing on the rail: a
 // non-finite column names ITSELF rather than reporting the quantity token nineteen columns share. The positional
 // rebuild survives because a C# constructor takes no splat; the table's own order is what pins it, and the arity is
 // proved by the table rather than restated.
 static readonly (string Slot, Func<SectionPropertiesWire, MeasureValueWire> Read)[] SectionColumns = [
  ("area", static w => w.Area), ("iyy", static w => w.Iyy), ("izz", static w => w.Izz), ("j", static w => w.J),
  ("iw", static w => w.Iw), ("wely", static w => w.Wely), ("welz", static w => w.Welz), ("wply", static w => w.Wply),
  ("wplz", static w => w.Wplz), ("av-y", static w => w.AvY), ("av-z", static w => w.AvZ),
  ("radius-of-gyration-major", static w => w.RadiusOfGyrationMajor), ("radius-of-gyration-minor", static w => w.RadiusOfGyrationMinor),
  ("depth", static w => w.Depth), ("width", static w => w.Width), ("heated-perimeter", static w => w.HeatedPerimeter),
  ("axis-distance", static w => w.AxisDistance), ("shear-centre-y", static w => w.ShearCentreY), ("shear-centre-z", static w => w.ShearCentreZ)];

 // Nineteen measure columns re-cross the OfSi finite gate, which a Mapperly partial cannot thread, and they accumulate:
 // a datasheet with three bad columns names all three, matching the owning admission's own accumulating shape.
 static Fin<SectionProperties> ToSection(SectionPropertiesWire w, Op key) =>
  toSeq(SectionColumns)
   .Traverse(column => Present(column.Read(w), $"section.{column.Slot}", key)
    .Bind(cell => ToMeasure(cell, key))
    .MapFail(_ => (Error)ElementFault.ValueRejected(key, $"<wire-section-column:{column.Slot}>"))
    .ToValidation())
   .As().ToFin()
   .Map(m => new SectionProperties(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15], m[16], m[17], m[18], w.MonosymmetryFactor));

 // Every arm re-enters the canonical MaterialPropertySet.Of* admission rail — the decoder NEVER constructs a case
 // directly, so the physical bounds, finite gates, matrix arity, and cross-field refinements the owner declares hold
 // for hostile wire bytes exactly as for an in-process author; the raw-double columns pass through verbatim and the
 // measured columns re-cross as admitted MeasureValues (or their SI scalars where the owner mints the type itself).
 static Fin<MaterialPropertySet> ToPropertySet(MaterialPropertySetWire w, Op key) =>
  ToDate(w.Evidence.HasValidUntil, w.Evidence.ValidUntil, key).Bind(validUntil => {
   PropertyEvidence evidence = new(w.Evidence.Source, w.Evidence.Reference, validUntil);
   return w.PropertySetCase switch {
    MaterialPropertySetWire.PropertySetOneofCase.Mechanical =>
     (ToMeasure(w.Mechanical.Density, key), ToMeasure(w.Mechanical.YoungsModulus, key), ToMeasure(w.Mechanical.YieldStrength, key), ToMeasure(w.Mechanical.UltimateStrength, key))
      .Apply(static (density, youngs, yield, ultimate) => (density, youngs, yield, ultimate)).As()
      .Bind(t => MaterialPropertySet.OfMechanical(t.density, t.youngs, t.yield, t.ultimate, w.Mechanical.PoissonsRatio, w.Mechanical.ThermalExpansionPerK, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Orthotropic =>
     (ToMeasure(w.Orthotropic.Density, key), ToMeasure(w.Orthotropic.E1Parallel, key), ToMeasure(w.Orthotropic.E2Perpendicular, key), ToMeasure(w.Orthotropic.ShearModulus, key), ToMeasure(w.Orthotropic.Strength1Parallel, key), ToMeasure(w.Orthotropic.Strength2Perpendicular, key))
      .Apply(static (density, e1, e2, shear, f1, f2) => (density, e1, e2, shear, f1, f2)).As()
      .Bind(t => MaterialPropertySet.OfOrthotropic(t.density, t.e1, t.e2, t.shear, t.f1, t.f2, w.Orthotropic.ThermalExpansionPerK, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Thermal =>
     (ToMeasure(w.Thermal.Conductivity, key), ToMeasure(w.Thermal.SpecificHeat, key), ToMeasure(w.Thermal.UValue, key))
      .Apply(static (conductivity, specificHeat, uValue) => (conductivity, specificHeat, uValue)).As()
      .Bind(t => MaterialPropertySet.OfThermal(t.conductivity, t.specificHeat, t.uValue, w.Thermal.VapourResistanceFactor, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Acoustic => Acoustic.Of(
     w.Acoustic.AbsorptionSpectrum.ToArray(), w.Acoustic.SoundReductionIndexDb.ToArray(), key,
     Opt(w.Acoustic.HasDynamicStiffnessMnPerM3, w.Acoustic.DynamicStiffnessMnPerM3), Opt(w.Acoustic.HasFlowResistivityPaSPerM2, w.Acoustic.FlowResistivityPaSPerM2), Opt(w.Acoustic.HasLossFactor, w.Acoustic.LossFactor))
     .Map(spectrum => MaterialPropertySet.OfAcoustic(spectrum, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Fire => FireRating.Parse(w.Fire.Reaction, key).Bind(reaction =>
     Row(SmokeClass.TryGet(w.Fire.Smoke, out SmokeClass? sc), sc, w.Fire.Smoke, key).Bind(smoke =>
      Row(DropletClass.TryGet(w.Fire.Droplets, out DropletClass? dc), dc, w.Fire.Droplets, key).Bind(droplets =>
       FireResistance.Of(
        Opt(w.Fire.Resistance.HasLoadBearingMinutes, w.Fire.Resistance.LoadBearingMinutes),
        Opt(w.Fire.Resistance.HasIntegrityMinutes, w.Fire.Resistance.IntegrityMinutes),
        Opt(w.Fire.Resistance.HasInsulationMinutes, w.Fire.Resistance.InsulationMinutes), key)
        .Map(resistance => MaterialPropertySet.OfFire(reaction, smoke, droplets, resistance, evidence))))),
    MaterialPropertySetWire.PropertySetOneofCase.Environmental => MeasurementBasis.Parse(w.Environmental.Basis, key).Bind(basis =>
     MaterialPropertySet.OfEnvironmental(basis, [.. w.Environmental.Impacts], w.Environmental.RecycledContent, w.Environmental.EndOfLifeRecovery, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Cost => MeasurementBasis.Parse(w.Cost.Basis, key).Bind(basis =>
     Currency.Parse(w.Cost.Currency, key).Bind(currency =>
      MaterialPropertySet.OfCost(basis, currency, w.Cost.SupplyPerUnit, w.Cost.InstallPerUnit, w.Cost.LifecyclePerUnit, key, evidence))),
    MaterialPropertySetWire.PropertySetOneofCase.Damping => MaterialPropertySet.OfDamping(
     w.Damping.DampingRatio, w.Damping.Rayleigh is null ? None : Some((w.Damping.Rayleigh.AlphaPerS, w.Damping.Rayleigh.BetaS)), key, evidence),
    MaterialPropertySetWire.PropertySetOneofCase.Hygrothermal =>
     (ToMeasure(w.Hygrothermal.WaterContent80Rh, key), ToMeasure(w.Hygrothermal.FreeWaterSaturation, key),
      OptCurve(w.Hygrothermal.SorptionIsotherm, key), OptCurve(w.Hygrothermal.LiquidTransport, key), OptCurve(w.Hygrothermal.MoistureConductivity, key))
      .Apply(static (waterContent, saturation, sorption, liquid, conductivity) => (waterContent, saturation, sorption, liquid, conductivity)).As()
      .Bind(t => MaterialPropertySet.OfHygrothermal(w.Hygrothermal.Porosity, t.waterContent.Si, t.saturation.Si,
       Opt(w.Hygrothermal.HasWaterAbsorptionKgPerM2SqrtS, w.Hygrothermal.WaterAbsorptionKgPerM2SqrtS), key, evidence, t.sorption, t.liquid, t.conductivity)),
    MaterialPropertySetWire.PropertySetOneofCase.Durability =>
     ToMeasure(w.Durability.ChlorideDiffusion, key).Bind(chloride => MaterialPropertySet.OfDurability(
      w.Durability.CarbonationRateMmPerSqrtYear, chloride.Si, w.Durability.AgeingExponent, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Optical => MaterialPropertySet.OfOptical(
     w.Optical.VisibleTransmittance, w.Optical.VisibleReflectanceFront, w.Optical.VisibleReflectanceBack, w.Optical.SolarTransmittance, w.Optical.SolarReflectanceFront, w.Optical.SolarReflectanceBack, w.Optical.ThermalIrTransmittance, w.Optical.ThermalIrEmissivityFront, w.Optical.ThermalIrEmissivityBack, key, evidence),
    _ => ElementFault.ValueRejected(key, "<wire-material-property-none>"),
   };
  });

 static Fin<AssessmentPayload> ToAssessment(AssessmentWire w, Op key) =>
  from discipline in Discipline.Parse(w.Discipline, key)
  from route in AnalysisRoute.Of(w.Route, key)
  from outcome in Row(AssessmentOutcome.TryGet(w.Outcome, out AssessmentOutcome? state), state, w.Outcome, key)
  from results in ToValueMap(w.Results, key)
  from diagnostic in ToDiagnostic(w.Diagnostic, key)
  from provenance in ToProvenance(w.Provenance, key)
  from payload in AssessmentPayload.Rehydrate(
   discipline, route, ToKey(w.InputKey), outcome, results, diagnostic,
   w.HasResultBlob ? Some(ToKey(w.ResultBlob)) : None, provenance, key,
   toSeq(w.DependsOnIds).Map(NodeId.Create))
  select payload;

 // ToObservation decodes the measured series: every token re-crosses its generated row gate, every required message
 // column and every flattened window rebuilds through the presence-and-order gate the BOUNDED NodaTime Interval both
 // seam ends require, and the whole run re-enters through Rehydrate — so the advancing-chunk, bracketing-window, and
 // census-coherence invariants re-prove against hostile input rather than riding the producer's word, and an unset
 // statistics or provenance message names itself on the rail instead of dereferencing inside the residual funnel.
 // Sample bytes stay in the object store; only content keys cross.
 static Fin<ObservationSeries> ToObservation(ObservationWire w, Op key) =>
  from sensor in SensorId.Of(w.Sensor, key)
  from sampling in Row(SamplingKind.TryGet(w.Sampling, out SamplingKind? kind), kind, w.Sampling, key)
  from window in ToInterval(w.WindowStart, w.WindowEnd, "observation.window", key)
  from chunks in toSeq(w.Chunks).TraverseM(chunk =>
   ToInterval(chunk.WindowStart, chunk.WindowEnd, "observation.chunk.window", key)
    .Map(span => new ObservationChunk(span, ToKey(chunk.SeriesKey), chunk.SampleCount))).As()
  from statistics in ToStatistics(w.Statistics, key)
  from provenance in ToSensorProvenance(w.Provenance, key)
  from series in ObservationSeries.Rehydrate(
   sensor, PropertyName.Create(w.Aspect), QuantityType.Create(w.Observed),
   Dimension.Create(w.DimLength, w.DimMass, w.DimTime, w.DimCurrent, w.DimTemperature, w.DimAmount, w.DimLuminousIntensity),
   w.CanonicalUnit, sampling,
   w.Cadence is null ? Option<NodaTime.Duration>.None : Some(w.Cadence.ToNodaDuration()),
   window, chunks, statistics, provenance, key)
  select series;

 // Census keys re-cross the generated ObservationGrade gate, so an unknown grade rails rather than silently dropping
 // a bucket the completeness ratio then over-counts against; the summary message and its span column admit before
 // either read, since an absent summary is a decode refusal rather than an empty one.
 static Fin<SeriesStatistics> ToStatistics(SeriesStatisticsWire? w, Op key) =>
  from summary in Present(w, "observation.statistics", key)
  from span in Present(summary.Span, "observation.statistics.span", key)
  from census in toSeq(summary.Census).TraverseM(entry =>
   Row(ObservationGrade.TryGet(entry.Key, out ObservationGrade? grade), grade, entry.Key, key)
    .Map(row => (Grade: row, entry.Value))).As()
  from minimum in OptMeasure(summary.Minimum, key)
  from maximum in OptMeasure(summary.Maximum, key)
  from mean in OptMeasure(summary.Mean, key)
  from total in OptMeasure(summary.Total, key)
  select SeriesStatistics.Of(
   census.Fold(Map<ObservationGrade, int>(), static (map, entry) => map.AddOrUpdate(entry.Grade, entry.Value)),
   span.ToNodaDuration(), minimum, maximum, mean, total);

 static Fin<SensorProvenance> ToSensorProvenance(SensorProvenanceWire? w, Op key) =>
  from audit in Present(w, "observation.provenance", key)
  from calibrated in ToDate(audit.HasCalibratedAt, audit.CalibratedAt, key)
  from tolerance in audit.Tolerance is null ? Fin.Succ(Option<MeasureBand>.None) : ToBand(audit.Tolerance, key).Map(Some)
  select new SensorProvenance(audit.Manufacturer, audit.Model, audit.Serial, calibrated, tolerance);

 static Fin<CoverageGrid> ToCoverage(CoverageWire w, Op key) =>
  from kind in Row(CoverageKind.TryGet(w.Kind, out CoverageKind? row), row, w.Kind, key)
  from crs in ToGeoReference(w.Crs, key)
  from bands in toSeq(w.Bands).TraverseM(band => ToBand(band, key)).As()
  from grid in ToLattice(w.Grid, key)
  from overviews in toSeq(w.Overviews).TraverseM(overview =>
   ToLattice(overview.Grid, key).Map(lattice => new OverviewLevel(lattice, ToKey(overview.RasterKey), overview.BlockX, overview.BlockY))).As()
  from coverage in CoverageGrid.Of(
   kind, ToKey(w.RasterKey), grid, bands, crs, key,
   overviews,
   toSeq(w.Slices).Map(slice => new TimeSlice(slice.At.ToInstant(), ToKey(slice.RasterKey))),
   w.BaseBlockX, w.BaseBlockY)
  select coverage;

 // The placement RE-ADMITS through the kernel's own gate rather than crossing as trusted state: a wire whose affine
 // is non-invertible or whose census breaches the ceiling rails here, so a foreign encoder cannot hand this runtime
 // a lattice its own CellLattice.Of would refuse. The arity gate is the wire's, because a repeated field carries no
 // fixed length and a short affine would otherwise index past its own array; the census crosses the SAME rail through
 // AcceptValidated, because the generated Create THROWS on a non-positive axis and a foreign encoder owns that int.
 static Fin<CellLattice> ToLattice(CellLatticeWire? w, Op key) =>
  w is { Affine.Count: 12 } wire
   ? from columns in key.AcceptValidated<LatticeAxis>(candidate: wire.Columns)
     from rows in key.AcceptValidated<LatticeAxis>(candidate: wire.Rows)
     from layers in key.AcceptValidated<LatticeAxis>(candidate: wire.Layers)
     from lattice in CellLattice.Of([.. wire.Affine], columns, rows, layers, wire.Ceiling, key)
     select lattice
   : ElementFault.ValueRejected(key, $"<wire-lattice-affine-arity:{w?.Affine.Count ?? 0}>");

 static Fin<CoverageBand> ToBand(CoverageBandWire w, Op key) =>
  Row(ChannelDtype.TryGet(w.SampleType, out ChannelDtype? st), st, w.SampleType, key).Bind(sampleType =>
   Row(BandRole.TryGet(w.Role, out BandRole? br), br, w.Role, key).Bind(role =>
    w.HasRangeMin != w.HasRangeMax ? ElementFault.ValueRejected(key, "<wire-band-range-half-open>")
    : !w.Palette.All(static p => (p.R | p.G | p.B | p.A) <= 255u) ? ElementFault.ValueRejected(key, "<wire-band-palette-channel-overflow>")
    : toSeq(w.Palette).TraverseM(bin => PerceptualColor
       .OfRgb((byte)bin.R, (byte)bin.G, (byte)bin.B, alpha: bin.A / 255.0, key: key)
       .Map(colour => new ColorBin(bin.Index, colour, bin.Category))).As()
      .Map(palette => new CoverageBand(w.Index, w.Name, sampleType, role, Opt(w.HasNoData, w.NoData), w.Units, w.Offset, w.Scale,
       w.HasRangeMin ? Some((w.RangeMin, w.RangeMax)) : None, palette))));

 // A seam GeoReference is Identity (no CRS) or Admit-resolved (Some CRS) — the wire mirrors the closed pair: an
 // absent crs decodes ONLY to the exact Identity tuple (junk columns rail), a present crs re-admits in full; the
 // wire's derived epsg/resolution columns are peer-informative — the seam re-derives both through Admit.
 static Fin<GeoReference> ToGeoReference(GeoReferenceWire w, Op key) => GeoReference.Admit(
  w.Eastings, w.Northings, w.OrthogonalHeight,
  w.XAxisAbscissa, w.XAxisOrdinate, w.ScaleX, w.ScaleY, w.ScaleZ,
  w.GeodeticDatum, w.VerticalDatum,
  w.Crs?.Name ?? "", w.Crs?.Wkt ?? "", w.Crs?.MapProjection ?? "", w.Crs?.MapZone ?? "", key,
  Opt(w.HasEpoch, w.Epoch), Opt(w.HasVerticalEpsg, w.VerticalEpsg));

 static Fin<Option<Diagnostic>> ToDiagnostic(DiagnosticWire? w, Op key) =>
  w is null ? Fin.Succ(Option<Diagnostic>.None)
  : Row(SolvePhase.TryGet(w.Phase, out SolvePhase? sp), sp, w.Phase, key).Bind(phase =>
    Row(FailureKind.TryGet(w.Kind, out FailureKind? fk), fk, w.Kind, key).Bind(kind =>
     Diagnostic.Of(phase, kind, w.Message, key, w.HasCode ? Some(w.Code) : None).Map(Some)));

 // Message fields carry presence by nullness (proto3 message presence); the window is both-or-neither, and the
 // present pair rebuilds through the shared window gate so a reversed pair rails here rather than throwing inside
 // the NodaTime constructor. The instant and the elapsed span are required columns and admit by name.
 static Fin<Provenance> ToProvenance(ProvenanceWire w, Op key) {
  Guid correlation = default;
  return (w.WindowStart is null) != (w.WindowEnd is null)
   ? ElementFault.ValueRejected(key, "<wire-provenance-window-half-open>")
   : w.HasCorrelation && !Guid.TryParse(w.Correlation, out correlation)
    ? ElementFault.ValueRejected(key, $"<wire-provenance-correlation:{w.Correlation}>")
    : (from at in Present(w.At, "provenance.at", key)
       from elapsed in Present(w.Elapsed, "provenance.elapsed", key)
       from window in w.WindowStart is null
        ? Fin.Succ(Option<NodaTime.Interval>.None)
        : ToInterval(w.WindowStart, w.WindowEnd, "provenance.window", key).Map(Some)
       select new Provenance(w.Author, w.Tool, w.Version, at.ToInstant(), elapsed.ToNodaDuration(), window,
        w.HasCorrelation ? Some(CorrelationId.Create(correlation)) : None, w.Attempt));
 }

 static Fin<PropertyBag> ToBag(PropertySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   ToValueMap(w.Values, key).Map(values => new PropertyBag(w.SetName, values, axes.Mode, axes.Rank)));

 static Fin<QuantityBag> ToBag(QuantitySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   toSeq(w.Values).TraverseM(p => ToMeasure(p.Value, key).Map(m => (Name: PropertyName.Create(p.Key), Value: m))).As()
    .Map(pairs => new QuantityBag(w.SetName, pairs.Fold(Map<PropertyName, MeasureValue>(), static (m, p) => m.Add(p.Name, p.Value)), axes.Mode, axes.Rank, ToGroups(w.Groups))));

 // The group run re-admits TOTAL: the three columns are free grouping text under no seam gate, so absence is the
 // whole decision each Has* presence pair answers and no rail is owed. A prefix naming no value row is admitted —
 // an authored group whose members a partial crossing omitted is data, not a malformed payload.
 static Map<string, GroupIdentity> ToGroups(IEnumerable<KeyValuePair<string, GroupIdentityWire>> entries) =>
  toSeq(entries).Fold(Map<string, GroupIdentity>(), static (map, entry) => map.Add(entry.Key, new GroupIdentity(
   entry.Value.HasDiscrimination ? Some(entry.Value.Discrimination) : None,
   entry.Value.HasQuality ? Some(entry.Value.Quality) : None,
   entry.Value.HasUsage ? Some(entry.Value.Usage) : None)));

 static Fin<(InheritanceMode Mode, PropertySource Rank)> BagAxes(string inheritance, int sourceRank, Op key) =>
  !InheritanceMode.TryGet(inheritance, out InheritanceMode? mode) ? ElementFault.ValueRejected(key, $"<wire-inheritance:{inheritance}>")
  : PropertySource.TryGet(sourceRank, out PropertySource? source) ? Fin.Succ((mode!, source!))
  : ElementFault.ValueRejected(key, $"<wire-source-rank:{sourceRank}>");

 static Fin<Map<PropertyName, PropertyValue>> ToValueMap(IEnumerable<KeyValuePair<string, PropertyValueWire>> entries, Op key) =>
  toSeq(entries).TraverseM(p => ToValue(p.Value, key).Map(v => (Name: PropertyName.Create(p.Key), Value: v))).As()
   .Map(pairs => pairs.Fold(Map<PropertyName, PropertyValue>(), static (m, p) => m.Add(p.Name, p.Value)));

 static Fin<Classification> ToClassification(ClassificationWire w, Op key) =>
  ToDate(w.HasEditionDate, w.EditionDate, key).Bind(editionDate =>
   Classification.Of(w.System, w.Code, key, w.Edition,
    source: w.HasSource ? Some(w.Source) : None, editionDate: editionDate, title: w.HasTitle ? Some(w.Title) : None));

 static Fin<SchemaSpan> ToSpan(SchemaSpanWire w, Op key) =>
  !ReleaseVersion.TryGet(w.IntroducedIn, out ReleaseVersion? introduced) ? ElementFault.ValueRejected(key, $"<wire-span-introduced:{w.IntroducedIn}>")
  : !w.HasRemovedIn ? Fin.Succ(new SchemaSpan(introduced!, None))
  : ReleaseVersion.TryGet(w.RemovedIn, out ReleaseVersion? removed) ? Fin.Succ(new SchemaSpan(introduced!, Some(removed!)))
  : ElementFault.ValueRejected(key, $"<wire-span-removed:{w.RemovedIn}>");

 static Fin<Option<NodaTime.LocalDate>> ToDate(bool present, string iso, Op key) =>
  !present ? Fin.Succ(Option<NodaTime.LocalDate>.None)
  : NodaTime.Text.LocalDatePattern.Iso.Parse(iso) is { Success: true } parsed ? Fin.Succ(Some(parsed.Value))
  : ElementFault.ValueRejected(key, $"<wire-date:{iso}>");

 // Proto3 carries MESSAGE presence as nullness, so a column the schema declares non-optional still arrives unset
 // from a hostile producer and the residual funnel would report its dereference as an opaque throw. Present names
 // the missing column on the rail instead, and ToInterval pairs it with the ORDER proof the flattened window needs:
 // the NodaTime two-Instant constructor throws on a reversed pair and would fire before any seam gate reads it.
 static Fin<T> Present<T>(T? w, string column, Op key) where T : class =>
  w is not null ? Fin.Succ(w) : ElementFault.ValueRejected(key, $"<wire-message-absent:{column}>");

 static Fin<NodaTime.Interval> ToInterval(
  Google.Protobuf.WellKnownTypes.Timestamp? start, Google.Protobuf.WellKnownTypes.Timestamp? end, string column, Op key) =>
  from opened in Present(start, $"{column}.start", key)
  from closed in Present(end, $"{column}.end", key)
  from window in opened.ToInstant() <= closed.ToInstant()
   ? Fin.Succ(new NodaTime.Interval(opened.ToInstant(), closed.ToInstant()))
   : ElementFault.ValueRejected(key, $"<wire-window-reversed:{column}>")
  select window;

 static Fin<Option<MeasureValue>> OptMeasure(MeasureValueWire? w, Op key) =>
  w is null ? Fin.Succ(Option<MeasureValue>.None) : ToMeasure(w, key).Map(Some);

 static Fin<Option<SampledCurve>> OptCurve(SampledCurveWire? w, Op key) =>
  w is null ? Fin.Succ(Option<SampledCurve>.None) : SampledCurve.Of(w.Axis.ToArray(), w.Values.ToArray(), key).Map(Some);

 static Option<double> Opt(bool present, double value) => present ? Some(value) : None;

 // Untrusted wire token -> generated SmartEnum row: the generated TryGet composed once, the miss railed.
 static Fin<T> Row<T>(bool found, T? row, string token, Op key) where T : class =>
  found && row is not null ? Fin.Succ(row) : ElementFault.ValueRejected(key, $"<wire-token:{typeof(T).Name}:{token}>");
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// ElementWire boundary: infallible Encode and Fin-railed Decode with one typed leg per wire kind.
// Wire messages are the byte surface; consumers compose the
// Google.Protobuf write family (WriteTo(IBufferWriter<byte>) / ToByteArray / WriteDelimitedTo) on the returned
// envelope directly — a forwarding byte wrapper here is the deleted form.
public static class ElementWire {
 // ONE graph encode carries the egress policy: an absent scope folds through RedactionScope.None, whose empty column
 // roster is the identity of Apply, so the unredacted and the scoped crossing run the same path and no sibling
 // EncodeRedacted forks it. Clearing runs on the JUST-ENCODED message, never on a caller's value.
 public static ElementGraphWire Encode(ElementGraph graph, Option<RedactionScope> scope = default) {
  ElementGraphWire wire = new() { Header = WireCodec.ToWire(graph.Header) };
  wire.Nodes.AddRange(toSeq(graph.Nodes.Values).Map(node => WireCodec.ToWire(node, graph.Header.Tolerance)));
  wire.Edges.AddRange(graph.Edges.Select(WireCodec.ToWire));
  return scope.IfNone(RedactionScope.None).Apply(wire);
 }

 public static NodeWire Encode(Node node, double tolerance) => WireCodec.ToWire(node, tolerance);

 public static GraphDeltaWire Encode(GraphDelta delta, Header basis) {
  Header revision = delta.Header.IfNone(basis);
  GraphDeltaWire wire = new();
  wire.AddedNodes.AddRange(delta.AddedNodes.Map(node => WireCodec.ToWire(node, revision.Tolerance)));
  wire.RemovedNodeIds.AddRange(delta.RemovedNodes.Map(static id => id.Value));
  wire.RevisedNodes.AddRange(delta.RevisedNodes.Map(r => new NodeRevisionWire {
   Before = WireCodec.ToWire(r.Before, basis.Tolerance),
   After = WireCodec.ToWire(r.After, revision.Tolerance),
  }));
  wire.AddedEdges.AddRange(delta.AddedEdges.Map(WireCodec.ToWire));
  wire.RemovedEdges.AddRange(delta.RemovedEdges.Map(WireCodec.ToWire));
  delta.Header.IfSome(h => wire.Header = WireCodec.ToWire(h));
  return wire;
 }

 // Parse under the explicit-limits reader (the ONE hostile-payload depth/size gate), re-admit every node, edge,
 // and header VALUE through the seam gates, then route the whole transcription through the graph's own STRUCTURAL
 // admission: the decoded snapshot enters as a Genesis-rooted GraphDelta through AdmitOnto, so LegalLink runs per
 // decoded edge — an absent endpoint rails NodeAbsent, an illegal endpoint-kind pair RelationshipInvalid, a
 // duplicate link DeltaConflict — exactly as the in-process Link path; a decoder-trusted ElementGraph.Of over
 // foreign edges is the deleted form (the wire is not a validated producer). Then optionally sweep the address
 // complement of the crossing's redaction manifest. The protobuf parse fault is a BOUNDARY exception
 // (InvalidProtocolBufferException) caught ONCE here and lowered to ValueRejected — never a leaked throw.
 //
 // The duplicate-id gate runs FIRST, over the RAW wire ids: two same-id wire nodes coalesce silently through the
 // PutNode upsert, so the conflict must rail before anything trusts the transcription — and a string scan is one
 // pass over the cheapest column on the message, where gating after ToNode pays every payload admission on a
 // hostile duplicate-stuffed graph before rejecting it.
 //
 // Node and edge admission ACCUMULATE: the two runs are independent of each other and each element is independent
 // within its run, so the applicative Traverse over Validation reports every malformed node AND every malformed edge
 // of a hostile payload in ONE failure, collapsing to Fin once at the structural gate — a first-failure TraverseM
 // would make a four-hundred-defect payload a four-hundred-round conversation.
 public static Fin<ElementGraph> DecodeGraph(Stream payload, WireLimits limits, Op key) =>
  Parse(ElementGraphWire.Parser, payload, limits, key).Bind(wire => Funnel(key, () =>
   toSeq(wire.Nodes).Map(static n => n.Id).Distinct().Count != wire.Nodes.Count
    ? ElementFault.DeltaConflict(key, "<wire-node-duplicate-id>")
    : WireCodec.ToHeader(wire.Header, key).Bind(header =>
       (toSeq(wire.Nodes).Traverse(n => AdmitNode(
          n, header.Tolerance,
          limits.VerifyAddresses && (wire.Redaction is null || !wire.Redaction.UnstableNodeIds.Contains(n.Id)), key)
         .ToValidation()).As(),
        toSeq(wire.Edges).Traverse(e => WireCodec.ToEdge(e, key).ToValidation()).As())
        .Apply(static (nodes, edges) => (Nodes: nodes, Edges: edges)).As().ToFin()
        .Bind(admitted =>
         admitted.Edges.Fold(admitted.Nodes.Fold(GraphDelta.Empty.Reheader(header), static (delta, node) => delta.Put(node)), static (delta, edge) => delta.Link(edge))
          .AdmitOnto(ElementGraph.Genesis(header), key)
          .Map(static step => step.Graph))))));

 // Verified decode checks the authoritative carried address and the node's own content-derived identity. A redaction
 // manifest suppresses this leg for the ids it declares unstable; consumers retain that roster as OCC ineligibility.
 static Fin<Node> AdmitNode(NodeWire wire, double tolerance, bool verify, Op key) =>
  WireCodec.ToNode(wire, key).Bind(node =>
   !verify ? Fin.Succ(node)
   : wire.ContentAddress.Length != 16
    ? ElementFault.AddressUnstable(key, $"<wire-content-address-width:{wire.Id}:{wire.ContentAddress.Length}>")
    : WireCodec.ToKey(wire.ContentAddress) != ContentAddress.Of(node, tolerance).Value
     ? ElementFault.AddressUnstable(key, $"<wire-content-address-mismatch:{wire.Id}>")
     : ContentAddress.Verify(node, tolerance, key).Map(_ => node));

 // Decoded deltas re-cross the IsNormalForm shape gate (a double-entry id or edge rails DeltaConflict — the
 // unique-per-id normal form Merge produces is an OBLIGATION on a foreign transcription, never assumed), and its
 // ONLY sanctioned application is AdmitOnto — ReplayOnto trusts a delta the seam's own algebra produced, which a
 // wire payload is not, so the structural edge law runs when the foreign delta lands on a graph.
 // Its four node and edge sections are independent runs over independent elements, so they admit through the SAME
 // accumulating Traverse the snapshot leg takes and join applicatively — one failure carrying every defect across all
 // four sections — before the delta shape gate runs.
 public static Fin<GraphDelta> DecodeDelta(Stream payload, Header basis, WireLimits limits, Op key) =>
  Parse(GraphDeltaWire.Parser, payload, limits, key).Bind(wire => Funnel(key, () =>
   (wire.Header is null ? Fin.Succ(Option<Header>.None) : WireCodec.ToHeader(wire.Header, key).Map(Some))
   .Bind(header => {
    Header revision = header.IfNone(basis);
    return (toSeq(wire.AddedNodes).Traverse(n => AdmitNode(n, revision.Tolerance, limits.VerifyAddresses, key).ToValidation()).As(),
     toSeq(wire.RevisedNodes).Traverse(r => AdmitNode(r.Before, basis.Tolerance, limits.VerifyAddresses, key)
      .Bind(b => AdmitNode(r.After, revision.Tolerance, limits.VerifyAddresses, key).Map(a => (Before: b, After: a)))
      .ToValidation()).As(),
     toSeq(wire.AddedEdges).Traverse(e => WireCodec.ToEdge(e, key).ToValidation()).As(),
     toSeq(wire.RemovedEdges).Traverse(e => WireCodec.ToEdge(e, key).ToValidation()).As())
     .Apply(static (added, revised, addedEdges, removedEdges) => (added, revised, addedEdges, removedEdges)).As().ToFin()
     .Map(sections => new GraphDelta(
      sections.added, toSeq(wire.RemovedNodeIds).Map(NodeId.Create), sections.revised,
      sections.addedEdges, sections.removedEdges, header))
     .Bind(delta => delta.IsNormalForm ? Fin.Succ(delta) : ElementFault.DeltaConflict(key, "<wire-delta-not-normal-form>"));
   }))));

 // Residual-throw funnel over protobuf/generated mapping code; typed inner faults pass untouched.
 static Fin<T> Funnel<T>(Op key, Func<Fin<T>> decode) =>
  key.Catch(decode).MapFail(e => e.IsExceptional ? (Error)ElementFault.ValueRejected(key, $"<wire-decode-throw:{e.Message}>") : e);

 static Fin<T> Parse<T>(MessageParser<T> parser, Stream payload, WireLimits limits, Op key) where T : class, IMessage<T> =>
  key.Catch(() => Fin.Succ(parser.ParseFrom(CodedInputStream.CreateWithLimits(payload, limits.SizeLimit, limits.RecursionLimit))))
   .MapFail(error => error.IsExceptional
    ? (Error)ElementFault.ValueRejected(key, $"<wire-parse:{error.Message}>")
    : error);
}
```

## [03]-[EGRESS_REDACTION]

- Owner: `ElementClassification` the `rasm.element` taxonomy's two `DataClassification` keys; `ClassifiedColumn` the `[SmartEnum<string>]` roster — one row per classified column group, carrying its owning `MessageDescriptor`, the `FieldMask` over that owner, its `DataClassificationSet`, and its identity verdict; `RedactionScope` the `WireLimits`-sibling egress policy record.
- Cases: `Commercial` claims the `CostWire` columns, the `EnvironmentalWire` impact matrix, and `PropertyEvidenceWire` — evidence rides the `MaterialPropertySetWire` ENVELOPE, so one row reaches all eleven property cases. `Personal` claims the `ObjectWire` audit row, `StepHeaderWire.authors`/`organizations`, `ProvenanceWire.author`/`correlation`, and `SensorProvenanceWire.serial`.
- Law: identity splits the roster on the canonical-bytes preimage its OWNERS already fix, never on a re-derivation here — `Composition/material#MATERIAL_PROPERTY` `CaseBytes` folds the evidence envelope and the `Cost`/`Environmental` columns into every `Node.Material` key, so those three rows are IDENTITY-BEARING and clearing one re-keys its node; `Graph/element#NODE_MODEL` `WriteObject` excludes `OwnerHistory` and `Projection/address#CONTENT_ADDRESS` `OfGraph` excludes the `StepHeader`/`Provenance` provenance slots, so those four rows are IDENTITY-INERT and clearing one moves no key and owes no manifest row.
- Entry: `RedactionScope.Of(policy, DataClassificationSet, key)` claims every row whose own set the request contains — `DataClassificationSet` keys on WHOLE-set equality, so containment reads as the union fixing the request — and rails a blank policy or a request claiming no row; `scope.Apply(wire)` is the whole egress effect and `RedactionScope.None` its identity element.
- Auto: protobuf's own path grammar admits NO segment past a repeated field, so only the singular header spine validates root-relative against `ElementGraphWire` and every column reached through `nodes` declares its mask against its OWNING descriptor, the clearing walk carrying the traversal the mask cannot express; the walk descends singular and repeated message fields alone, which is total over the roster because a classified column is declared on a typed payload message and a map value is a generated map-entry.
- Receipt: `RedactionManifestWire` is the crossing's egress receipt — policy identity, the owner-qualified path roster, and the node ids the clearing re-keyed. A cleared column WITHOUT explicit presence (`FieldDescriptor.HasPresence` is false for every scalar in the roster) reads as its proto3 default, so the DECLARED roster — never the message — is the presence record separating a cleared column from an authored default.
- Packages: Microsoft.Extensions.Compliance.Abstractions (`DataClassification(taxonomy, value)`/`DataClassificationSet`/`Union` — the contract assembly ALONE, so this seam mints classification keys and resolves no `Redactor`), Google.Protobuf (`FieldMask.FromString<T>`/`Normalize`/`Paths`, `MessageDescriptor.Fields.InFieldNumberOrder`/`FindFieldByName`, `FieldDescriptor.IsMap`/`IsRepeated`/`FieldType`/`Accessor`, `IFieldAccessor.Clear`/`GetValue`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `Items` roster), LanguageExt.Core (`Fin`/`Seq`/`Option`).
- Growth: a new classified column is one `ClassifiedColumn` row naming its owner, its paths, its classification, and its identity verdict; a new sensitivity class is one `DataClassification` key on the taxonomy; a new policy is one `Of` call — never a per-policy scope type, never a second walk, and never a redactor token substituted for a cleared value.
- Boundary: the mechanism is PRESENCE CLEARING on the encoded message and nothing else — no `Redactor` resolves, no HMAC pseudonym crosses, and no re-derived identity space mints, so a redacted crossing PRESERVES its source content keys and a partner reference off the source model still resolves; the policy touches the wire message alone and never an `ElementGraph`, so an in-process consumer of the same graph is unaffected; and a redacted crossing is a DISTINCT byte stream from its unredacted twin, so the `Graph/corpus` parity vectors are forged unredacted and a redacted stream is never a parity input.
- Boundary: `unstable_node_ids` makes the retained source `content_address` unusable as an edit OCC precondition.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The seam's own compliance taxonomy. The two keys are minted as ordinary (taxonomy, value) pairs off the CONTRACT
// assembly, so an app-tier redactor registration and this wire policy select rows from ONE vocabulary while this page
// stays free of the redaction runtime — the classification is the SELECTOR here and the cleared field path is the effect.
public static class ElementClassification {
 public const string Taxonomy = "rasm.element";

 public static readonly DataClassification Commercial = new(Taxonomy, "commercial");
 public static readonly DataClassification Personal = new(Taxonomy, "personal");
}

// --- [TABLES] -----------------------------------------------------------------------------
// One row per classified column GROUP. Owner is the message the columns live on; Mask is the FieldMask over that
// owner, so the roster's path algebra never has to express a segment past a repeated field — the walk owns traversal
// and the mask owns selection. IdentityBearing is read off the owners' canonical-bytes preimages, never re-derived.
[SmartEnum<string>]
public sealed partial class ClassifiedColumn {
 public static readonly ClassifiedColumn Cost = new("cost",
  Owned<CostWire>(), Declared<CostWire>("basis,currency,supply_per_unit,install_per_unit,lifecycle_per_unit"),
  ElementClassification.Commercial, identityBearing: true);

 public static readonly ClassifiedColumn Environmental = new("environmental",
  Owned<EnvironmentalWire>(), Declared<EnvironmentalWire>("basis,impacts,recycled_content,end_of_life_recovery"),
  ElementClassification.Commercial, identityBearing: true);

 // Evidence rides the MaterialPropertySetWire envelope, so ONE row reaches every one of the eleven property cases.
 public static readonly ClassifiedColumn PropertyEvidence = new("property-evidence",
  Owned<PropertyEvidenceWire>(), Declared<PropertyEvidenceWire>("source,reference,valid_until"),
  ElementClassification.Commercial, identityBearing: true);

 // The audit row clears WHOLE off its owner rather than column by column: history carries explicit message presence,
 // so the cleared crossing is absence-honest with no manifest row owed.
 public static readonly ClassifiedColumn OwnerHistory = new("owner-history",
  Owned<ObjectWire>(), Declared<ObjectWire>("history"), ElementClassification.Personal, identityBearing: false);

 public static readonly ClassifiedColumn StepAuthorship = new("step-authorship",
  Owned<StepHeaderWire>(), Declared<StepHeaderWire>("authors,organizations"),
  ElementClassification.Personal, identityBearing: false);

 public static readonly ClassifiedColumn ComputeProvenance = new("compute-provenance",
  Owned<ProvenanceWire>(), Declared<ProvenanceWire>("author,correlation"),
  ElementClassification.Personal, identityBearing: false);

 public static readonly ClassifiedColumn SensorSerial = new("sensor-serial",
  Owned<SensorProvenanceWire>(), Declared<SensorProvenanceWire>("serial"),
  ElementClassification.Personal, identityBearing: false);

 public MessageDescriptor Owner { get; }
 public FieldMask Mask { get; }
 public DataClassificationSet Classes { get; }
 public bool IdentityBearing { get; }

 // Clearing IS the redaction: the mask's own path resolves its descriptor row and IFieldAccessor.Clear writes the
 // proto3 default — dropping a presence-bearing message whole, zeroing a presence-less scalar, emptying a repeated
 // run. Answering the identity verdict is what lets the sweep decide a node's manifest row without a second probe.
 // The foreach is the protobuf boundary's own mutable shape, the named statement exemption on this page.
 public bool Clear(IMessage message) {
  foreach (string path in Mask.Paths) { Owner.FindFieldByName(path).Accessor.Clear(message); }
  return IdentityBearing;
 }

 // Roster rows are declaration-total: FromString<T> parses AND proves every path against the owning message's own
 // grammar, Normalize sorts, dedupes, and prunes subpaths, so a mistyped column is a construction defect at roster
 // materialization rather than a clear that silently reaches nothing. Owned reads the descriptor off the published
 // IMessage.Descriptor reflection entry, so the roster names no generated static.
 static FieldMask Declared<T>(string paths) where T : IMessage<T> => FieldMask.FromString<T>(paths).Normalize();
 static MessageDescriptor Owned<T>() where T : IMessage<T>, new() => new T().Descriptor;
}

// --- [MODELS] -----------------------------------------------------------------------------
// The egress policy record beside WireLimits: a policy identity plus the rows one requested classification set claims.
// None carries an EMPTY roster and is the identity of Apply, so an unscoped encode and a scoped one are one code path.
public sealed record RedactionScope {
 private RedactionScope(string policy, Seq<ClassifiedColumn> columns) => (Policy, Columns) = (policy, columns);

 public string Policy { get; }
 public Seq<ClassifiedColumn> Columns { get; }

 public static readonly RedactionScope None = new("", Seq<ClassifiedColumn>());

 // A row is claimed when the request CONTAINS its classification set; DataClassificationSet keys on whole-set
 // equality, so containment reads as the union fixing the request rather than as a member scan. A policy claiming no
 // row rails instead of crossing an unredacted stream under a redaction label.
 public static Fin<RedactionScope> Of(string policy, DataClassificationSet classes, Op key) =>
  string.IsNullOrWhiteSpace(policy)
   ? ElementFault.ValueRejected(key, "<redaction-policy-blank>")
   : toSeq(ClassifiedColumn.Items).Filter(column => classes.Union(column.Classes).Equals(classes)) is { IsEmpty: false } claimed
    ? Fin.Succ(new RedactionScope(policy.Trim(), claimed))
    : ElementFault.ValueRejected(key, $"<redaction-scope-claims-nothing:{policy}>");

 // --- [OPERATIONS]
 // The whole egress effect: clear every claimed column reachable from the encoded message, then stamp the manifest.
 // The header spine's rows are identity-inert by the roster's own law, so its sweep yields no node id; each node's
 // sweep contributes its id exactly when an identity-bearing row was reached, which is the roster the verifying
 // decode admits as declared-unstable. Message mutation is the protobuf boundary's shape — the statement exemption —
 // and the empty-roster arm returns the message untouched, which is what makes None a true identity.
 public ElementGraphWire Apply(ElementGraphWire wire) {
  if (Columns.IsEmpty) { return wire; }
  _ = Sweep(wire.Header);
  Seq<string> unstable = toSeq(wire.Nodes).Fold(Seq<string>(), (roster, node) => Sweep(node) ? roster.Add(node.Id) : roster);
  RedactionManifestWire manifest = new() { Policy = Policy };
  manifest.ClearedPaths.AddRange(Columns.Bind(column => toSeq(column.Mask.Paths).Map(path => $"{column.Owner.Name}.{path}")));
  manifest.UnstableNodeIds.AddRange(unstable);
  wire.Redaction = manifest;
  return wire;
 }

 // ONE descriptor walk per message root, ANSWERING whether the walk reached an identity-bearing row: clear what this
 // message owns, then descend. The folds are strict and every join is the NON-short-circuiting `|`, because clearing
 // is an EFFECT — `||` would skip the descent the moment a row on this message already moved identity, leaving the
 // subtree uncleared while still reporting the verdict. The one short-circuit that IS wanted is the owner match,
 // which must not clear a message the row does not own. A new classified column is one roster row, no new traversal.
 bool Sweep(IMessage message) =>
  Columns.Fold(false, (moved, column) =>
   (ReferenceEquals(column.Owner, message.Descriptor) && column.Clear(message)) | moved)
   | Nested(message).Fold(false, (moved, child) => Sweep(child) | moved);

 // Singular and repeated message fields are the whole descent: a classified column is declared on a typed payload
 // message, and a map field's value is a generated map-entry no roster row can own. An unset oneof arm reads null
 // through the accessor and drops out, so a node walks only its own case.
 static Seq<IMessage> Nested(IMessage message) =>
  toSeq(message.Descriptor.Fields.InFieldNumberOrder())
   .Filter(static field => field.FieldType is FieldType.Message && !field.IsMap)
   .Bind(field => field.Accessor.GetValue(message) switch {
    IMessage single => Seq(single),
    System.Collections.IEnumerable run => toSeq(run.Cast<IMessage>()),
    _ => Seq<IMessage>(),
   });
}
```

## [04]-[EVENT_ENVELOPE]

- Owner: `GraphEventType` the closed crossing vocabulary, each row carrying the `Rasm/Domain/event#EVENT_GRAMMAR` `EventType` its facts announce and the `EventSource` naming the producing capability; `GraphCrossing` the seam's composition of the kernel envelope owner — one mint, one Protobuf-framed encode, one decode, and the handling grade an egress scope derives.
- Entry: `GraphCrossing.Mint(crossing, subject, operation, at, body, ports, key)` composes `EventEnvelope.Mint` and returns its `Fin<CloudEvent>`; `Frame(envelope, key)` composes `EventEnvelope.Encode(EventFormat.Protobuf, …)` and `Admit(frame, key)` composes `EventEnvelope.Decode`, so the crossing owns which format it admits and the kernel owns every codec.
- Auto: `id` carries the PRODUCING RAIL's operation identity and `subject` the content key, so `(source, id)` is the uniqueness composite a dedup reads and two rails announcing one snapshot stay two events. `subject` renders through `EventKey.Render` — the kernel's ONE envelope content-key spelling — never `ContentAddress.ToValue()`, whose upper-case X32 is this seam's own protobuf and `NodeId` spelling and puts a second rendering of one key on one wire.
- Auto: `datacontenttype` DERIVES from the encoded message's own descriptor — `application/protobuf` carrying the `messageType` parameter off `IMessage.Descriptor.FullName` — so a consumer selects its parser from the attribute rather than from the topic it arrived on, and a renamed wire message moves the attribute with it. `dataschema` is the composing rail's registry binding and arrives as a value, because this seam runs no registry.
- Auto: `dataclassification` DERIVES from the egress scope through `#EGRESS_REDACTION`'s own roster — a scope claiming every `ClassifiedColumn` row grades `internal`, and every lesser scope (`RedactionScope.None` included) grades `restricted`, whose `DataGrade.Redact` column states the redaction route is still owed. A crossing therefore cannot announce a handling class its cleared-column roster contradicts.
- Receipt: the envelope IS the broker-lane metadata — the protobuf body is `Data` and the frame's `ContentType` is what a binding stamps — and a streaming consumer folds length-prefixed bodies (`MessageExtensions.WriteLengthPrefixedTo(IBufferWriter<byte>)` into a pooled sink, `WriteDelimitedTo` the stream-shaped sibling) one frame per crossing, deduped on `(source, id)`.
- Packages: Rasm (`Rasm.Domain` `EventEnvelope.Mint`/`.Encode`/`.Decode`, `EventMint`, `EventType`/`EventSource`/`EventKey`, `EventExtension`/`EventRoster`, `EventFormat.Protobuf`, `EventFrame`, `DataGrade`, `TraceCarrier`), CloudNative.CloudEvents (`CloudEvent` — the envelope value crossing this seam's signatures), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `TryGet`), LanguageExt.Core (`Fin`/`Option`/`Seq`), NodaTime (`Instant`), Google.Protobuf (`IMessage.Descriptor`, `MessageExtensions.ToByteArray`/`WriteLengthPrefixedTo`).
- Growth: a new crossing is one `GraphEventType` row carrying its own `EventType`, so a `Breaking` descriptor dial moves that row's major and old consumers keep matching their own; a new envelope dimension is one `EventExtension` row at the kernel owner and one `Extensions` entry here; a new broker lane is one binding row at its consuming owner, never a seam member.
- Boundary: the envelope carries metadata alone and the protobuf message is the body; bindings, content mode, prefixes, `dataref` residence, and delivery guarantees seat at the consuming owner. The creation-time trace arrives as a `TraceCarrier` VALUE the composing rail captured — this seam neither reads `Activity.Current` nor formats a `traceparent`, because the kernel mint owns the stamp and the propagator owns the format. `WireKind` stays the in-process decode dimension `Projection/observe` tags facts with; `GraphEventType` stays the transport crossing vocabulary.

| [INDEX] | [CROSSING]  | [TYPE]                            | [SOURCE]                | [BODY]              |
| :-----: | :---------- | :-------------------------------- | :---------------------- | :------------------ |
|  [01]   | `snapshot`  | `rasm.element.graph.frozen.v1`    | `rasm:element/snapshot` | `ElementGraphWire`  |
|  [02]   | `delta`     | `rasm.element.delta.appended.v1`  | `rasm:element/delta`    | `GraphDeltaWire`    |

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Net.Mime;
using CloudNative.CloudEvents;
using Google.Protobuf;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Wire;

namespace Rasm.Element.Graph;

// --- [TYPES] ----------------------------------------------------------------------------------
// Closed crossing vocabulary. Each row carries the kernel `EventType` its facts announce rather than a literal
// token: `Of` assembles the four grammar segments, so a row cannot spell a type the estate grammar refuses and a
// major move is one argument on the row that owns it. `Source` names the producing CAPABILITY under the same
// grammar, so no host, deployment, or topic can enter the identity a consumer keys its subscription on.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GraphEventType {
 private const string Domain = "element";

 public static readonly GraphEventType Snapshot = new("snapshot", subject: "graph", fact: "frozen");
 public static readonly GraphEventType Delta = new("delta", subject: "delta", fact: "appended");

 private GraphEventType(string key, string subject, string fact) : this(key) =>
  (Type, Source) = (EventType.Of(Domain, subject, fact, major: 1), EventSource.Of(Domain, capability: key));

 public EventType Type { get; }

 public EventSource Source { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------------
// The composing rail's own contributions, so this seam never invents one: `Operation` is the producing rail's
// operation identity that `id` carries (a content digest there would make two rails announcing one snapshot into
// one event and drop the second), `Schema` the registry binding a serdes arrow resolved, `Trace` the creation-time
// pair the rail captured, and `Extensions` whatever rostered rows the rail adds. Every slot is a value, so this
// page reads no ambient clock, no ambient activity, and no registry.
public readonly record struct CrossingPorts(
 string Operation,
 Option<Uri> Schema,
 TraceCarrier Trace,
 Seq<(EventExtension Row, object Value)> Extensions);

// --- [OPERATIONS] -------------------------------------------------------------------------------
public static class GraphCrossing {
 // ONE mint: the kernel owner funnels construction, every rostered write, and `Validate()` through its own rail,
 // so this seam composes an admitted request and never touches a `CloudEvent` slot. `subject` and the handling
 // grade are the two rows this seam derives; every other value arrives admitted.
 public static Fin<CloudEvent> Mint(GraphEventType crossing, ContentAddress subject, Instant at,
   IMessage body, CrossingPorts ports, Op key, Option<RedactionScope> scope = default) =>
  EventEnvelope.Mint(
   new EventMint(
    Type: crossing.Type,
    Source: crossing.Source,
    Id: ports.Operation,
    Subject: Some(EventKey.Render(subject.Value)),
    Time: at,
    DataSchema: ports.Schema,
    DataContentType: Some(ContentType(body)),
    Data: body.ToByteArray(),
    Trace: ports.Trace,
    Extensions: ports.Extensions.Add((EventExtension.DataClassification, Grade(scope).Key))),
   key);

 // Structured self-contained frame: the kernel encode chooses the framing and hands back the carrier a binding
 // stamps, so a lane that carries one message whole needs no second encoder and a batch is the same call at a
 // higher arity. A binary-mode lane instead ships `Data` beside the binding's own attribute headers and reads
 // `datacontenttype` for its parser — one envelope, two placements, zero re-packs.
 public static Fin<EventFrame> Frame(Op key, params ReadOnlySpan<CloudEvent> envelopes) =>
  EventEnvelope.Encode(EventFormat.Protobuf, key, envelopes);

 public static Fin<Seq<CloudEvent>> Admit(EventFrame frame, Op key) => EventEnvelope.Decode(frame, key);

 // The handling class the crossing announces is the SCOPE's own answer: a scope claiming every classified row
 // ships a body whose commercial and personal columns are already cleared, and every lesser scope still carries
 // them, so `DataGrade.Redact` reads true exactly while the redaction route is owed. Deriving the grade forecloses
 // a crossing that labels itself clean while its manifest lists nothing.
 public static DataGrade Grade(Option<RedactionScope> scope) =>
  scope.Map(static claimed => claimed.Columns.Count == ClassifiedColumn.Items.Count).IfNone(false)
   ? DataGrade.Internal
   : DataGrade.Restricted;

 // Content type DERIVES from the message's own descriptor, so a consumer selects its parser from the attribute
 // rather than from the topic, and a renamed wire message moves the declaration with it.
 static string ContentType(IMessage body) =>
  new ContentType("application/protobuf") { Parameters = { ["messageType"] = body.Descriptor.FullName } }.ToString();
}
```

## [05]-[IMPLEMENTATION_LAW]

- [KEY_VERBATIM_LAW]: wire identities cross verbatim. `NodeId` uses X32 text; `UInt128` keys use big-endian fields while `CanonicalWriter.U128` remains little-endian hash input. Each peer normalizes once at decode and never substitutes a second digest.
- [NODE_OCC_ADDRESS]: `content_address` mints under the active header tolerance; delta encode requires its basis header.
- [CODEC_DIVISION]: `Grpc.Tools` emits messages, Mapperly emits field transcription, Thinktecture `Switch` owns seam-case encode dispatch, and protobuf case enums own decode dispatch. Reflection and parallel hand-written mappings are forbidden.
- [ADMISSION_AND_DEPTH_GATE]: `DecodeGraph` and `DecodeDelta` parse under positive `WireLimits`. Every decoded value re-crosses its owner gate before the aggregate reaches `AdmitOnto` or `IsNormalForm`. Duplicate node ids rail on a raw-id scan before value admission. Unset cases, unknown rows, invalid values, and illegal structure share the in-process typed rail.
- [EVENT_ENVELOPE]: `GraphCrossing` composes the kernel envelope owner whole — one mint, one Protobuf-framed encode, one decode — with `id` the composing rail's operation identity, `subject` the content key under the kernel `EventKey` spelling, `datacontenttype` derived from the body descriptor, and `dataclassification` from the egress scope; binding prefixes, content mode, and `dataref` residence own at the consuming binding; Protobuf streaming rides `WriteLengthPrefixedTo`/`WriteDelimitedTo`.
- [EGRESS_REDACTION]: a scoped crossing clears classified field paths on the encoded message and carries its `RedactionManifestWire`. Source content keys survive — no key re-derives over redacted bytes — and the verifying decode admits exactly the manifest-named nodes as declared-unstable while a drifted node outside that roster still faults `AddressUnstable`. A redacted crossing is a DISTINCT byte stream from its unredacted twin, so parity vectors are forged and compared unredacted and a redaction policy never enters a parity gate.
- [CONTRACT_EVOLUTION]: `rasm/element/v1/element.proto` is the descriptor source. Appended fields and new `oneof` arms are additive; renumbers, incompatible type changes, and unreserved removals are breaking. Whole-graph parity literals remain governed by `Graph/corpus`'s terminal research route until exact addresses exist.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
