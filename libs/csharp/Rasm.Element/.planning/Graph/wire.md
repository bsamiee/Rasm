# [ELEMENT_WIRE]

The ONE wire-codec owner: a proto-first, content-key-preserving graph wire — the `rasm.element.v1` contract (`Graph/element.proto`, `csharp_namespace Rasm.Element.Wire`) whose `oneof`-backed envelopes mirror the closed seam unions 1:1, the `WireCodec` Mapperly-generated per-case transcription family, and the `ElementWire` boundary owner with infallible `Encode` lowering and `Fin<T>`-railed `DecodeGraph`/`DecodeDelta` re-admission. Content keys cross VERBATIM — a `NodeId` as its X32 string, a `UInt128` content hash as a 16-byte big-endian `bytes` field (the persisted `XxHash128` canonical form the TypeScript `h128` boundary normalizes to and the Python `ContentKey` flips from its little-endian memory) — so a peer REPRODUCES the seam key over the count-prefixed injective `CanonicalBytes` canon and NEVER re-mints, re-derives, or re-hashes an identity. The codec COMPOSES the sibling owners brief-frozen: `Node.ToCanonicalBytes`, `PropertyValue.Of`, `MeasureValue.OfSi`, `CardinalPoint.Of`, `Classification.Of`, and `Discipline.Parse` are the admission gates every decoded value re-crosses, AND the decoded aggregate re-crosses the graph's own STRUCTURAL owners — `DecodeGraph` routes the whole transcription through `GraphDelta.AdmitOnto` over a `Genesis` root so `LegalLink` runs per decoded edge, and `DecodeDelta` gates the `IsNormalForm` shape and hands back a delta whose only sanctioned application is `AdmitOnto` — so a hostile payload lands on the SAME `ElementFault` rail an in-process author does: `ValueRejected` on a parse failure, a `PayloadCase` miss, or a value the seam admission refuses; `NodeAbsent`/`RelationshipInvalid`/`DeltaConflict` on a dangling, illegal, duplicate, or non-normal-form structure; `AddressUnstable` from the optional `ContentAddress.Verify` rehydrate integrity gate. The decode leg parses under `CodedInputStream.CreateWithLimits` — THE depth/size gate `Properties/property#PROPERTY_VALUE` defers hostile `PropertyValueWire` nesting to, a parameterized `WireLimits` policy record carrying the budgets. The baked `Element` NEVER crosses (a derived `Bake` fold, never a wire record); the `ElementGraphWire` snapshot and the `GraphDeltaWire` change record are the two crossings, the delta envelope forward-arming the `DELTA_CRDT` streaming lane, and each crossing publishes under the `GraphEventEnvelope` metadata whose subject is the crossing's own content key. Measures cross SI-coerced — the `QuantityType` token, the SI magnitude, and the seven `Dimension` exponents, never a `{value, unit:string}` shape — and the decode re-mints the registry `CanonicalUnit` through `MeasureValue.OfSi`, so the wire carries exactly the identity columns `CanonicalWriter.Measure` hashes. The contract's evolution law is the `typescript:core/interchange/contract` `FileDescriptorSet` drift gate (`Identical`/`Additive`/`Breaking`); the float-bearing `IfcMaterialLayer` golden vector anchors the three-runtime round-trip.

## [01]-[INDEX]

- [02]-[WIRE_CODEC]: the `rasm.element.v1` proto contract (`ElementGraphWire`/`GraphDeltaWire`/`HeaderWire`/`NodeWire`/`RelationshipWire`/`PropertyValueWire`/`MeasureValueWire`/`MaterialUsageWire` and the typed payload wires down to the engineering-property leaf messages), the `WireCodec` Mapperly per-case transcription mapper with its verbatim key codecs, the `ElementWire` boundary owner (`Encode`/`DecodeGraph`/`DecodeDelta` under the `WireLimits` policy), and the key-verbatim / depth-gate / contract-evolution laws.
- [03]-[EVENT_ENVELOPE]: the `GraphEventType` closed event-token vocabulary and the `GraphEventEnvelope` CloudEvents-aligned crossing metadata — content-key subject dedup, the `Attributes`/`Admit` transport-neutral dual, and the W3C trace slots an app-tier propagator fills.

## [02]-[WIRE_CODEC]

- Owner: the `Graph/element.proto` `rasm.element.v1` contract — the language-neutral message roster `Grpc.Tools` compiles for C# (`GrpcServices=None`, message codegen only) and `buf`/`protoc-gen-es` + `grpcio-tools` compile for the TypeScript/Python peers; `WireCodec` the `[Mapper]` static transcription family owning every per-case seam↔wire field mapping; `ElementWire` the boundary owner railing decode onto `Fin<T>`; `WireLimits` the parameterized decode-budget policy record.
- Cases: every closed seam union crosses as a `oneof` mirroring its cases 1:1 — `NodeWire` the seven `Node` payloads, `RelationshipWire` the six edge kinds, `PropertyValueWire` the recursive fourteen-case value family, `MaterialUsageWire` the explicit none/layer/profile usage family, `MaterialCompositionWire` the four composition arms, and `MaterialPropertySetWire` the engineering-property family. Generated keyed owners cross by key; absence is field presence, never a numeric or unset-oneof sentinel.
- Entry: `ElementWire.Encode(ElementGraph)` and `Encode(GraphDelta)` are the infallible total lowerings of already-valid graphs onto `ElementGraphWire`/`GraphDeltaWire` (the wire message IS the byte surface — a consumer composes the `Google.Protobuf` write family `WriteTo(IBufferWriter<byte>)`/`ToByteArray`/`WriteDelimitedTo` directly, never a forwarding byte wrapper); `DecodeGraph(Stream, WireLimits, Op)` and `DecodeDelta(Stream, WireLimits, Op)` are the one `Fin<T>`-railed decode leg per wire kind (the `GeometrySource` typed-leg precedent — the discriminant is the return TYPE, never a `Get`/`GetById` arity family), parsing through `MessageParser<T>.ParseFrom(CodedInputStream.CreateWithLimits(stream, limits.SizeLimit, limits.RecursionLimit))`, re-admitting every value through the seam gates, and re-crossing the STRUCTURAL owners — `DecodeGraph` admits the transcription as a `Genesis`-rooted delta through `GraphDelta.AdmitOnto` (`LegalLink` per decoded edge, a duplicate wire node id railed before admission), `DecodeDelta` gates `GraphDelta.IsNormalForm` and returns a delta applied ONLY through `AdmitOnto`, never `ReplayOnto`.
- Auto: `WireCodec` combines Mapperly's explicit member diagnostics with generated union/protobuf case dispatch. Decode re-mints a `MeasureValue` through `OfSi`, re-admits its `MeasureBand`, re-admits material-usage direction/cardinal tokens, and recursively re-admits every `PropertyValue`; no generated-code `Get` throw is part of the boundary contract.
- Receipt: an `ElementGraphWire` is the snapshot a peer decodes into its own graph mirror WITHOUT re-deriving an identity — ids and content keys verbatim, the decoded transcription re-admitted as a `Genesis`-rooted delta through `GraphDelta.AdmitOnto` so `LegalLink` runs per decoded edge; a `GraphDeltaWire` is the change record a streaming consumer folds (the `DELTA_CRDT` convergence substrate's wire form); the optional `ContentAddress.Verify` sweep is the rehydrate integrity verdict a content-keyed consumer reads before trusting a crossed id, railing `ElementFault.AddressUnstable` per drifted node.
- Packages: Google.Protobuf (`IMessage<T>`/`MessageParser<T>`/`CodedInputStream.CreateWithLimits`/`ByteString`/`RepeatedField<T>`/`MessageExtensions` write family), Grpc.Tools (the `<Protobuf>` MSBuild item, `GrpcServices=None`, `PrivateAssets=all` — build-only, never a runtime surface), Riok.Mapperly (`[Mapper]`/`[UserMapping]`/`[MappingTarget]`/`[MapProperty]` + `MappingConversionType` policy over the Thinktecture `Create`/`Value` key codecs), NodaTime.Serialization.Protobuf (`Instant.ToTimestamp()`/`Timestamp.ToInstant()`, `Duration` dual), LanguageExt.Core (`Fin`/`Seq`/`Option` + the `Traverse` admission folds), Thinktecture.Runtime.Extensions (the generated total `Switch` encode dispatch).
- Growth: a new node/edge/value case is one `oneof` arm plus one `WireCodec` case mapping (the descriptor gate classifies the addition `Additive`); a new payload column is one numbered proto field (never a renumber — field numbers are append-only, removal reserves); a new peer runtime is one codegen lane over the SAME `.proto`, never a second contract; a new decode budget is one `WireLimits` column, never a hardcoded literal in the parse call.
- Boundary: ids and content keys cross verbatim; peers re-mint nothing. Persisted `UInt128` keys use 16-byte big-endian form, distinct from the canonical writer's little-endian hash input. The derived `Element` never crosses, recursive values parse under admitted `WireLimits`, every decoded value re-crosses its owner gate, and descriptor evolution rejects renumbers, incompatible type changes, and unreserved removals.

```proto signature
// Graph/element.proto — the rasm.element.v1 graph wire. Field numbers are append-only; removal reserves.
// Content keys cross verbatim: NodeId as the X32 string, UInt128 as 16-byte big-endian bytes.
syntax = "proto3";
package rasm.element.v1;
option csharp_namespace = "Rasm.Element.Wire";

import "google/protobuf/timestamp.proto";
import "google/protobuf/duration.proto";
import "google/protobuf/empty.proto";

// --- [GRAPH_ENVELOPES] ---
message ElementGraphWire {
  HeaderWire header = 1;
  repeated NodeWire nodes = 2;
  repeated RelationshipWire edges = 3;
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
  }
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
message MaterialLayerWire { string material_key = 1; MeasureValueWire thickness = 2; string layer_name = 3; }
message ProfileSetWire { string material_key = 1; ProfileRefWire profile = 2; optional SectionPropertiesWire section = 3; }
message ProfileRefWire { string standard = 1; string designation = 2; bytes content_key = 3; }
message ConstituentSetWire { repeated MaterialConstituentWire constituents = 1; }
message MaterialConstituentWire { string material_key = 1; string category = 2; double fraction = 3; }

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
message FireResistanceWire { sint32 load_bearing_minutes = 1; sint32 integrity_minutes = 2; sint32 insulation_minutes = 3; }
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

// --- [COVERAGE_WIRE] ---
message CoverageWire {
  string kind = 1;                         // CoverageKind key
  bytes raster_key = 2;                    // 16-byte BE — the blob rides the object store, never the wire
  GridDescriptorWire grid = 3;
  repeated CoverageBandWire bands = 4;
  GeoReferenceWire crs = 5;
  repeated OverviewLevelWire overviews = 6;
  sint32 base_block_x = 7;
  sint32 base_block_y = 8;
  repeated TimeSliceWire slices = 9;
}
message TimeSliceWire { google.protobuf.Timestamp at = 1; bytes raster_key = 2; }
message GridDescriptorWire {
  double origin_x = 1;
  double cell_size_x = 2;
  double row_rotation = 3;
  double origin_y = 4;
  double column_rotation = 5;
  double cell_size_y = 6;
  sint32 width = 7;
  sint32 height = 8;
}
message CoverageBandWire {
  sint32 index = 1;
  string name = 2;
  string sample_type = 3;                  // RasterSampleType key
  string role = 4;                         // BandRole key
  optional double no_data = 5;
  string units = 6;
  double offset = 7;
  double scale = 8;
  optional double range_min = 9;
  optional double range_max = 10;
  repeated ColorBinWire palette = 11;
}
message ColorBinWire { sint32 index = 1; uint32 r = 2; uint32 g = 3; uint32 b = 4; uint32 a = 5; string category = 6; }
message OverviewLevelWire {
  sint32 width = 1;
  sint32 height = 2;
  double cell_size = 3;
  bytes raster_key = 4;
  sint32 block_x = 5;
  sint32 block_y = 6;
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
  string vertical_datum = 10;
  optional ProjectedCrsWire crs = 11;
  optional double epoch = 12;
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
using Google.Protobuf;
using Google.Protobuf.Collections;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.Domain;
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

namespace Rasm.Element.Graph;

// The csproj codegen item this contract realizes as (signature-locked here until the .proto lands as source):
//   <Protobuf Include="Graph/element.proto" GrpcServices="None" />

// --- [MODELS] -----------------------------------------------------------------------------
// The parameterized decode-budget policy — size cap, recursion cap (the PropertyValueWire/ComplexWire hostile-
// nesting gate property.md defers here), and the rehydrate integrity sweep toggle. Never a hardcoded literal
// at a parse call; a consumer tightens by policy value, not by a sibling decode entry.
public sealed record WireLimits {
 private WireLimits(int sizeLimit, int recursionLimit, bool verifyAddresses) =>
  (SizeLimit, RecursionLimit, VerifyAddresses) = (sizeLimit, recursionLimit, verifyAddresses);

 public int SizeLimit { get; }
 public int RecursionLimit { get; }
 public bool VerifyAddresses { get; init; }

 public static readonly WireLimits Default = new(sizeLimit: 512 << 20, recursionLimit: 96, verifyAddresses: false);
 public static readonly WireLimits Verified = Default with { VerifyAddresses = true };

 public static Fin<WireLimits> Of(int sizeLimit, int recursionLimit, bool verifyAddresses, Op key) =>
  sizeLimit > 0 && recursionLimit > 0
   ? Fin.Succ(new WireLimits(sizeLimit, recursionLimit, verifyAddresses))
   : ElementFault.ValueRejected(key, $"<wire-limits-invalid:{sizeLimit}:{recursionLimit}>");
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The Mapperly transcription family: source-generated per-case field mapping, key codecs hand-owned as
// [UserMapping] statics so identity NEVER re-derives — Mapperly transcribes shape, the seam owns identity.
// Encode case dispatch is the union's generated total Switch; decode dispatch is the generated PayloadCase/
// ValueCase closed enum ([MapDerivedType] is the class-hierarchy rail; a oneof envelope has no case base).
[Mapper(
 EnabledConversions = MappingConversionType.Constructor | MappingConversionType.ImplicitCast | MappingConversionType.Enumerable | MappingConversionType.Dictionary,
 RequiredMappingStrategy = RequiredMappingStrategy.Both)]
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
 [UserMapping] internal static Google.Protobuf.WellKnownTypes.Timestamp ToWire(NodaTime.Instant at) => at.ToTimestamp();
 [UserMapping] internal static NodaTime.Instant ToInstant(Google.Protobuf.WellKnownTypes.Timestamp wire) => wire.ToInstant();

 // --- [CASE_TRANSCRIPTIONS] — Mapperly generates the flat-column width per case; every union-valued member
 // rides an explicit envelope fold below, every Option/tuple-flatten/token crossing rides an explicit
 // [UserMapping] carrier codec (conditional set on encode; the hand decode below reads the generated Has*/null
 // presence pairs), and [MapProperty] pins every seam→wire name seam so the generator never silently skips a member.
 // The envelope owns Id (NodeWire.id), so the payload mappings exclude it — RequiredMappingStrategy.Both would
 // otherwise fault the intentionally-unmapped source member.
 [MapperIgnoreSource(nameof(Node.Object.Id))]
 internal static partial ObjectWire ToWire(Node.Object node);
 [MapperIgnoreSource(nameof(Node.Material.Id))]
 [MapProperty(nameof(Node.Material.Properties), nameof(MaterialWire.PropertySets))]
 internal static partial MaterialWire ToWire(Node.Material node);
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
 [UserMapping] internal static void ToWire(UnitScheme scheme, [MappingTarget] MapField<string, string> wire) { foreach (var (quantity, unit) in scheme.Display) { wire[quantity] = unit; } }

 [UserMapping] internal static ClassificationWire ToWire(Classification c) {
  ClassificationWire w = new() { System = c.System, Code = c.Code, Edition = c.Edition };
  c.Source.IfSome(s => w.Source = s);
  c.EditionDate.IfSome(d => w.EditionDate = NodaTime.Text.LocalDatePattern.Iso.Format(d));
  c.Title.IfSome(t => w.Title = t);
  return w;
 }

 // --- [CARRIER_CODECS] — the Option/tuple-flatten crossings Mapperly cannot bridge alone: an absent Option leaves
 // the proto3 field UNSET (a nullable return skips the assignment), a flatten writes its column pair explicitly.
 [UserMapping] internal static string? ToWire(Option<string> value) => value.MatchUnsafe(static s => s, static () => (string?)null);
 [UserMapping] internal static string? ToWire(Option<NodeId> id) => id.MatchUnsafe(static i => i.Value, static () => (string?)null);
 [UserMapping] internal static ByteString? ToWire(Option<UInt128> key) => key.MatchUnsafe(static k => ToWire(k), static () => (ByteString?)null);

 [UserMapping] internal static void ToWire(RepresentationContentHash representations, [MappingTarget] MapField<string, ByteString> wire) { foreach (var (id, hash) in representations.ByIdentifier) { wire[id] = ToWire(hash); } }

 [UserMapping] internal static SchemaSpanWire ToWire(SchemaSpan span) {
  SchemaSpanWire w = new() { IntroducedIn = span.IntroducedIn.Key };
  span.RemovedIn.IfSome(r => w.RemovedIn = r.Key);
  return w;
 }

 [UserMapping] internal static OwnerHistoryWire? ToWire(Option<OwnerHistory> history) => history.MatchUnsafe(
  static h => {
   OwnerHistoryWire w = new() { OwningUser = h.OwningUser, OwningApplication = h.OwningApplication, Created = h.Created.ToTimestamp(), ChangeAction = h.ChangeAction, State = h.State };
   h.Modified.IfSome(m => w.Modified = m.ToTimestamp());
   return w;
  },
  static () => (OwnerHistoryWire?)null);

 // The wire's epsg/resolution columns are peer-informative derivations; blank ProjectedCrs strings stay UNSET.
 [UserMapping] internal static GeoReferenceWire ToWire(GeoReference geo) {
  GeoReferenceWire w = new() {
   Eastings = geo.Eastings, Northings = geo.Northings, OrthogonalHeight = geo.OrthogonalHeight,
   XAxisAbscissa = geo.XAxisAbscissa, XAxisOrdinate = geo.XAxisOrdinate,
   ScaleX = geo.ScaleX, ScaleY = geo.ScaleY, ScaleZ = geo.ScaleZ,
   GeodeticDatum = geo.GeodeticDatum, VerticalDatum = geo.VerticalDatum,
  };
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
  w.Palette.AddRange(band.Palette.Map(static c => new ColorBinWire { Index = c.Index, R = c.R, G = c.G, B = c.B, A = c.A, Category = c.Category }));
  return w;
 }

 [UserMapping] internal static ProvenanceWire ToWire(Provenance p) {
  ProvenanceWire w = new() { Author = p.Author, Tool = p.Tool, Version = p.Version, At = p.At.ToTimestamp(), Elapsed = p.Elapsed.ToProtobufDuration(), Attempt = p.Attempt };
  p.Window.IfSome(i => { w.WindowStart = i.Start.ToTimestamp(); w.WindowEnd = i.End.ToTimestamp(); });
  p.Correlation.IfSome(c => w.Correlation = c.ToString("D"));
  return w;
 }

 [UserMapping] internal static DiagnosticWire? ToWire(Option<Diagnostic> diagnostic) => diagnostic.MatchUnsafe(
  static d => { DiagnosticWire w = new() { Phase = d.Phase.Key, Kind = d.Kind.Key, Message = d.Message }; d.Code.IfSome(c => w.Code = c); return w; },
  static () => (DiagnosticWire?)null);

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

 // The ONE envelope fold per union — the generated total Switch, a new case a compile break, never a default arm.
 internal static NodeWire ToWire(Node node) => node.Switch<NodeWire>(
  @object: o => new() { Id = o.Id.Value, Object = ToWire(o) },
  material: m => new() { Id = m.Id.Value, Material = ToWire(m) },
  propertySet: p => new() { Id = p.Id.Value, PropertySet = ToWire(p.Bag) },
  quantitySet: q => new() { Id = q.Id.Value, QuantitySet = ToWire(q.Bag) },
  assessment: a => new() { Id = a.Id.Value, Assessment = ToWire(a.Payload) },
  appearance: a => new() { Id = a.Id.Value, Appearance = ToWire(a.Summary) },
  coverage: c => new() { Id = c.Id.Value, Coverage = ToWire(c.Grid) });

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

 internal static MaterialCompositionWire ToWire(MaterialComposition composition) => composition.Switch<MaterialCompositionWire>(
  single: c => new() { Single = new SingleWire { MaterialKey = c.Material.Value } },
  layerSet: c => { LayerSetWire w = new(); w.Layers.AddRange(c.Layers.Map(static l => new MaterialLayerWire { MaterialKey = l.Material.Value, Thickness = ToWire(l.Thickness), LayerName = l.LayerName })); return new() { LayerSet = w }; },
  profileSet: c => { ProfileSetWire w = new() { MaterialKey = c.Material.Value, Profile = new ProfileRefWire { Standard = c.Profile.Standard, Designation = c.Profile.Designation, ContentKey = ToWire(c.Profile.ContentKey) } }; c.Section.IfSome(s => w.Section = ToWire(s)); return new() { ProfileSet = w }; },
  constituentSet: c => { ConstituentSetWire w = new(); w.Constituents.AddRange(c.Constituents.Map(static x => new MaterialConstituentWire { MaterialKey = x.Material.Value, Category = x.Category, Fraction = x.Fraction })); return new() { ConstituentSet = w }; });

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

 // The TemporalValue arms re-admit through the NodaTime ISO patterns (the seam Iso() canon reversed) — a malformed
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
      w.Name, w.Tag,
      new RepresentationContentHash(w.Representations.Aggregate(Map<string, UInt128>(), static (m, p) => m.Add(p.Key, ToKey(p.Value)))),
      w.History is null ? None : Some(new OwnerHistory(w.History.OwningUser, w.History.OwningApplication, w.History.Created.ToInstant(),
       w.History.Modified is null ? None : Some(w.History.Modified.ToInstant()), w.History.ChangeAction, w.History.State)),
      span, secondary))));

 static Fin<Node> ToMaterial(NodeId id, MaterialWire w, Op key) =>
  ToComposition(w.Composition, key).Bind(composition =>
   toSeq(w.PropertySets).TraverseM(p => ToPropertySet(p, key)).As().Map(sets =>
    (Node)new Node.Material(id, MaterialId.Of(w.MaterialKey), composition, sets)));

 static Fin<MaterialComposition> ToComposition(MaterialCompositionWire w, Op key) => w.CompositionCase switch {
  MaterialCompositionWire.CompositionOneofCase.Single => Fin.Succ(MaterialComposition.OfSingle(MaterialId.Of(w.Single.MaterialKey))),
  MaterialCompositionWire.CompositionOneofCase.LayerSet =>
   toSeq(w.LayerSet.Layers).TraverseM(l => ToMeasure(l.Thickness, key).Map(t => new MaterialLayer(MaterialId.Of(l.MaterialKey), t, l.LayerName))).As()
    .Bind(layers => MaterialComposition.OfLayerSet(layers, key)),
  MaterialCompositionWire.CompositionOneofCase.ProfileSet =>
   (w.ProfileSet.Section is null ? Fin.Succ(Option<SectionProperties>.None) : ToSection(w.ProfileSet.Section, key).Map(Some))
    .Bind(section => ProfileRef.Rehydrate(w.ProfileSet.Profile.Standard, w.ProfileSet.Profile.Designation, ToKey(w.ProfileSet.Profile.ContentKey), key)
     .Map(profile => (MaterialComposition)new MaterialComposition.ProfileSet(MaterialId.Of(w.ProfileSet.MaterialKey), profile, section))),
  MaterialCompositionWire.CompositionOneofCase.ConstituentSet => MaterialComposition.OfConstituentSet(
   toSeq(w.ConstituentSet.Constituents).Map(c => new MaterialConstituent(MaterialId.Of(c.MaterialKey), c.Category, c.Fraction)), key),
  _ => ElementFault.ValueRejected(key, "<wire-composition-none>"),
 };

 // SectionProperties decode is hand-owned Fin — nineteen measure columns re-cross the OfSi finite gate, which a
 // Mapperly partial cannot thread — the traversal order mirroring the wire field order exactly (positional by the
 // frozen SectionPropertiesWire layout, the count locked by the ctor arity).
 static Fin<SectionProperties> ToSection(SectionPropertiesWire w, Op key) =>
  toSeq<MeasureValueWire>([w.Area, w.Iyy, w.Izz, w.J, w.Iw, w.Wely, w.Welz, w.Wply, w.Wplz, w.AvY, w.AvZ,
    w.RadiusOfGyrationMajor, w.RadiusOfGyrationMinor, w.Depth, w.Width, w.HeatedPerimeter, w.AxisDistance, w.ShearCentreY, w.ShearCentreZ])
   .TraverseM(m => ToMeasure(m, key)).As()
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
       FireResistance.Of(w.Fire.Resistance.LoadBearingMinutes, w.Fire.Resistance.IntegrityMinutes, w.Fire.Resistance.InsulationMinutes, key)
        .Map(resistance => MaterialPropertySet.OfFire(reaction, smoke, droplets, resistance, evidence))))),
    MaterialPropertySetWire.PropertySetOneofCase.Environmental => MeasurementBasis.Parse(w.Environmental.Basis, key).Bind(basis =>
     MaterialPropertySet.OfEnvironmental(basis, [.. w.Environmental.Impacts], w.Environmental.RecycledContent, w.Environmental.EndOfLifeRecovery, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Cost => MeasurementBasis.Parse(w.Cost.Basis, key).Bind(basis =>
     Currency.Parse(w.Cost.Currency, key).Bind(currency =>
      MaterialPropertySet.OfCost(currency, basis, w.Cost.SupplyPerUnit, w.Cost.InstallPerUnit, w.Cost.LifecyclePerUnit, key, evidence))),
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

 static Fin<CoverageGrid> ToCoverage(CoverageWire w, Op key) =>
  from kind in Row(CoverageKind.TryGet(w.Kind, out CoverageKind? row), row, w.Kind, key)
  from crs in ToGeoReference(w.Crs, key)
  from bands in toSeq(w.Bands).TraverseM(band => ToBand(band, key)).As()
  from coverage in CoverageGrid.Of(
   kind, ToKey(w.RasterKey),
   new GridDescriptor(w.Grid.OriginX, w.Grid.CellSizeX, w.Grid.RowRotation, w.Grid.OriginY, w.Grid.ColumnRotation, w.Grid.CellSizeY, w.Grid.Width, w.Grid.Height),
   bands, crs, key,
   toSeq(w.Overviews).Map(overview => new OverviewLevel(overview.Width, overview.Height, overview.CellSize, ToKey(overview.RasterKey), overview.BlockX, overview.BlockY)),
   toSeq(w.Slices).Map(slice => new TimeSlice(slice.At.ToInstant(), ToKey(slice.RasterKey))),
   w.BaseBlockX, w.BaseBlockY)
  select coverage;

 static Fin<CoverageBand> ToBand(CoverageBandWire w, Op key) =>
  Row(RasterSampleType.TryGet(w.SampleType, out RasterSampleType? st), st, w.SampleType, key).Bind(sampleType =>
   Row(BandRole.TryGet(w.Role, out BandRole? br), br, w.Role, key).Bind(role =>
    w.HasRangeMin != w.HasRangeMax ? ElementFault.ValueRejected(key, "<wire-band-range-half-open>")
    : !w.Palette.All(static p => (p.R | p.G | p.B | p.A) <= 255u) ? ElementFault.ValueRejected(key, "<wire-band-palette-channel-overflow>")
    : Fin.Succ(new CoverageBand(w.Index, w.Name, sampleType, role, Opt(w.HasNoData, w.NoData), w.Units, w.Offset, w.Scale,
       w.HasRangeMin ? Some((w.RangeMin, w.RangeMax)) : None,
       toSeq(w.Palette).Map(static p => new ColorBin(p.Index, (byte)p.R, (byte)p.G, (byte)p.B, (byte)p.A, p.Category))))));

 // A seam GeoReference is Identity (no CRS) or Admit-resolved (Some CRS) — the wire mirrors the closed pair: an
 // absent crs decodes ONLY to the exact Identity tuple (junk columns rail), a present crs re-admits in full; the
 // wire's derived epsg/resolution columns are peer-informative — the seam re-derives both through Admit.
 static Fin<GeoReference> ToGeoReference(GeoReferenceWire w, Op key) => GeoReference.Admit(
  w.Eastings, w.Northings, w.OrthogonalHeight,
  w.XAxisAbscissa, w.XAxisOrdinate, w.ScaleX, w.ScaleY, w.ScaleZ,
  w.GeodeticDatum, w.VerticalDatum,
  w.Crs?.Name ?? "", w.Crs?.Wkt ?? "", w.Crs?.MapProjection ?? "", w.Crs?.MapZone ?? "", key,
  Opt(w.HasEpoch, w.Epoch));

 static Fin<Option<Diagnostic>> ToDiagnostic(DiagnosticWire? w, Op key) =>
  w is null ? Fin.Succ(Option<Diagnostic>.None)
  : Row(SolvePhase.TryGet(w.Phase, out SolvePhase? sp), sp, w.Phase, key).Bind(phase =>
    Row(FailureKind.TryGet(w.Kind, out FailureKind? fk), fk, w.Kind, key).Bind(kind =>
     Diagnostic.Of(phase, kind, w.Message, key, w.HasCode ? Some(w.Code) : None).Map(Some)));

 // Message fields carry presence by nullness (proto3 message presence); the window is both-or-neither.
 static Fin<Provenance> ToProvenance(ProvenanceWire w, Op key) {
  Guid correlation = default;
  return (w.WindowStart is null) != (w.WindowEnd is null)
   ? ElementFault.ValueRejected(key, "<wire-provenance-window-half-open>")
   : w.HasCorrelation && !Guid.TryParse(w.Correlation, out correlation)
    ? ElementFault.ValueRejected(key, $"<wire-provenance-correlation:{w.Correlation}>")
    : Fin.Succ(new Provenance(w.Author, w.Tool, w.Version, w.At.ToInstant(), w.Elapsed.ToNodaDuration(),
       w.WindowStart is null || w.WindowEnd is null ? None : Some(new NodaTime.Interval(w.WindowStart.ToInstant(), w.WindowEnd.ToInstant())),
       w.HasCorrelation ? Some(correlation) : None, w.Attempt));
 }

 static Fin<PropertyBag> ToBag(PropertySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   ToValueMap(w.Values, key).Map(values => new PropertyBag(w.SetName, values, axes.Mode, axes.Rank)));

 static Fin<QuantityBag> ToBag(QuantitySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   toSeq(w.Values).TraverseM(p => ToMeasure(p.Value, key).Map(m => (Name: PropertyName.Create(p.Key), Value: m))).As()
    .Map(pairs => new QuantityBag(w.SetName, pairs.Fold(Map<PropertyName, MeasureValue>(), static (m, p) => m.Add(p.Name, p.Value)), axes.Mode, axes.Rank)));

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
// The boundary owner: infallible Encode (a valid graph lowers totally), Fin-railed Decode (one typed leg per
// wire kind — the GeometrySource precedent). The wire message IS the byte surface: a consumer composes the
// Google.Protobuf write family (WriteTo(IBufferWriter<byte>) / ToByteArray / WriteDelimitedTo) on the returned
// envelope directly — a forwarding byte wrapper here is the deleted form.
public static class ElementWire {
 public static ElementGraphWire Encode(ElementGraph graph) {
  ElementGraphWire wire = new() { Header = WireCodec.ToWire(graph.Header) };
  wire.Nodes.AddRange(toSeq(graph.Nodes.Values).Map(WireCodec.ToWire));
  wire.Edges.AddRange(graph.Edges.Select(WireCodec.ToWire));
  return wire;
 }

 public static GraphDeltaWire Encode(GraphDelta delta) {
  GraphDeltaWire wire = new();
  wire.AddedNodes.AddRange(delta.AddedNodes.Map(WireCodec.ToWire));
  wire.RemovedNodeIds.AddRange(delta.RemovedNodes.Map(static id => id.Value));
  wire.RevisedNodes.AddRange(delta.RevisedNodes.Map(static r => new NodeRevisionWire { Before = WireCodec.ToWire(r.Before), After = WireCodec.ToWire(r.After) }));
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
 // foreign edges is the deleted form (the wire is not a validated producer). Duplicate node ids rail DeltaConflict
 // before admission (two same-id wire nodes would otherwise coalesce silently through the PutNode upsert). Then
 // optionally sweep ContentAddress.Verify (AddressUnstable per drifted id). The protobuf parse fault is a BOUNDARY
 // exception (InvalidProtocolBufferException) caught ONCE here and lowered to ValueRejected — never a leaked throw.
 public static Fin<ElementGraph> DecodeGraph(Stream payload, WireLimits limits, Op key) =>
  Parse(ElementGraphWire.Parser, payload, limits, key).Bind(wire => Funnel(key, () =>
   WireCodec.ToHeader(wire.Header, key).Bind(header =>
    toSeq(wire.Nodes).TraverseM(n => WireCodec.ToNode(n, key)).As().Bind(nodes =>
     toSeq(wire.Edges).TraverseM(e => WireCodec.ToEdge(e, key)).As().Bind(edges =>
      nodes.Map(static n => n.Id).ToHashSet().Count != nodes.Count
       ? ElementFault.DeltaConflict(key, "<wire-node-duplicate-id>")
       : edges.Fold(nodes.Fold(GraphDelta.Empty.Reheader(header), static (delta, node) => delta.Put(node)), static (delta, edge) => delta.Link(edge))
          .AdmitOnto(ElementGraph.Genesis(header), key)
          .Bind(step => limits.VerifyAddresses ? ContentAddress.Verify(step.Graph, key).ToFin().Map(_ => step.Graph) : Fin.Succ(step.Graph)))))));

 // The decoded delta re-crosses the IsNormalForm shape gate (a double-entry id or edge rails DeltaConflict — the
 // unique-per-id normal form Merge produces is an OBLIGATION on a foreign transcription, never assumed), and its
 // ONLY sanctioned application is AdmitOnto — ReplayOnto trusts a delta the seam's own algebra produced, which a
 // wire payload is not, so the structural edge law runs when the foreign delta lands on a graph.
 public static Fin<GraphDelta> DecodeDelta(Stream payload, WireLimits limits, Op key) =>
  Parse(GraphDeltaWire.Parser, payload, limits, key).Bind(wire => Funnel(key, () =>
   toSeq(wire.AddedNodes).TraverseM(n => WireCodec.ToNode(n, key)).As().Bind(added =>
    toSeq(wire.RevisedNodes).TraverseM(r => WireCodec.ToNode(r.Before, key).Bind(b => WireCodec.ToNode(r.After, key).Map(a => (b, a)))).As().Bind(revised =>
     toSeq(wire.AddedEdges).TraverseM(e => WireCodec.ToEdge(e, key)).As().Bind(addedEdges =>
      toSeq(wire.RemovedEdges).TraverseM(e => WireCodec.ToEdge(e, key)).As().Bind(removedEdges =>
       (wire.Header is null ? Fin.Succ(Option<Header>.None) : WireCodec.ToHeader(wire.Header, key).Map(Some))
        .Map(header => new GraphDelta(
         added, toSeq(wire.RemovedNodeIds).Map(NodeId.Create), revised.Map(static p => (p.b, p.a)),
         addedEdges, removedEdges, header))
        .Bind(delta => delta.IsNormalForm ? Fin.Succ(delta) : ElementFault.DeltaConflict(key, "<wire-delta-not-normal-form>"))))))));

 // Residual-throw funnel over protobuf/generated mapping code; typed inner faults pass untouched.
 static Fin<T> Funnel<T>(Op key, Func<Fin<T>> decode) =>
  key.Catch(decode).MapFail(e => e.IsExceptional ? (Error)ElementFault.ValueRejected(key, $"<wire-decode-throw:{e.Message}>") : e);

 static Fin<T> Parse<T>(MessageParser<T> parser, Stream payload, WireLimits limits, Op key) where T : class, IMessage<T> {
  try { return Fin.Succ(parser.ParseFrom(CodedInputStream.CreateWithLimits(payload, limits.SizeLimit, limits.RecursionLimit))); }
  catch (InvalidProtocolBufferException fault) { return ElementFault.ValueRejected(key, $"<wire-parse:{fault.Message}>"); }
 }
}
```

## [03]-[EVENT_ENVELOPE]

- Owner: `GraphEventType` the closed `[SmartEnum<string>]` event-token vocabulary (`rasm.element.graph.v1` the snapshot crossing, `rasm.element.graphdelta.v1` the change crossing — the version suffix rides the token); `GraphEventEnvelope` the CloudEvents-aligned crossing metadata record: token, source identity, subject `ContentAddress`, occurrence `NodaTime.Instant`, and the W3C `traceparent`/`tracestate` slots as data fields an app-tier propagator fills, the seam referencing no OTel type.
- Entry: `For(ElementGraph, source, at, key)` and `For(GraphDelta, tolerance, source, at, key)` are the one polymorphic mint — the subject derives through `ContentAddress.OfGraph` for a snapshot and the `GraphDelta.ToCanonicalBytes` address for a delta, the SAME projections the `Rasm.Persistence` event dedup and the `Projection/observe#HOOK_RAIL` facts key on; `Attributes()` projects the canonical attribute rows any carrier writes as transport headers; `Admit(attributes, key)` is the consumer-side inverse — the token re-admits through the vocabulary lookup, the subject through the `ContentAddress` X32 factory, unknown attributes pass untouched, and a missing required attribute or malformed slot rails `ElementFault.ValueRejected`.
- Auto: `Id` derives from the subject, so a re-published crossing shares its event identity by construction and consumer dedup is one subject compare; the traceparent slot admits only the W3C shape (2-hex version, 32-hex trace id, 16-hex span id, 2-hex flags, lowercase, dash-delimited) while tracestate crosses opaque — vendor-list truth belongs to the propagator.
- Receipt: the envelope IS the broker-lane metadata — a Kafka/NATS/MQTT/CloudEvents lane at the peer tier publishes a crossing as envelope attributes with the protobuf body, and a streaming consumer folds length-prefixed bodies (`MessageExtensions.WriteLengthPrefixedTo(IBufferWriter<byte>)` into a pooled sink, `WriteDelimitedTo` the stream-shaped sibling) one envelope per frame, deduped by `Subject`.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `TryGet`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Map`), NodaTime (`Instant` + `InstantPattern.ExtendedIso` the `time` canon), Google.Protobuf (`MessageExtensions.WriteLengthPrefixedTo` the buffer-writer frame).
- Growth: a `Breaking` descriptor dial mints a sibling token row, so old consumers keep decoding their own token; a new attribute is one envelope column with its `Attributes`/`Admit` rows; a new broker lane is one carrier adapter at the peer tier, never a seam member.
- Boundary: the envelope carries identity and trace METADATA alone — payload bytes ride the wire message, classified columns ride the redaction ruling, and the seam never fills a trace slot, because a self-minted traceparent forks the app tier's propagation truth; `WireKind` stays the in-process decode-kind dimension while `GraphEventType` names the crossing on the transport — two vocabularies, two consumers; attribute names stay canonical (`id`/`source`/`type`/`subject`/`time`/`traceparent`/`tracestate`) and the binding prefix (`ce-`, `ce_`) is the carrier adapter's.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// Closed event-token vocabulary every broker-published crossing rides — the version suffix is part of the
// token, so a Breaking descriptor dial mints a sibling row and old consumers keep decoding their own token.
[SmartEnum<string>]
public sealed partial class GraphEventType {
 public static readonly GraphEventType Graph = new("rasm.element.graph.v1");
 public static readonly GraphEventType Delta = new("rasm.element.graphdelta.v1");
}

// CloudEvents-aligned crossing metadata — id/source/type/subject/time plus the W3C trace slots as DATA (an
// app-tier propagator fills them; the seam mints none). Subject IS the dedup key: OfGraph for a snapshot, the
// ToCanonicalBytes address for a delta — the same projections Persistence dedup and the observe facts read.
public sealed record GraphEventEnvelope {
 private GraphEventEnvelope(GraphEventType type, string source, ContentAddress subject, NodaTime.Instant at,
  Option<string> traceParent, Option<string> traceState) =>
  (Type, Source, Subject, At, TraceParent, TraceState) = (type, source, subject, at, traceParent, traceState);

 public GraphEventType Type { get; }
 public string Source { get; }
 public ContentAddress Subject { get; }
 public NodaTime.Instant At { get; }
 public Option<string> TraceParent { get; }
 public Option<string> TraceState { get; }

 // Event identity derives from the subject, so a re-published crossing dedups by construction.
 public string Id => Subject.ToValue();

 public static Fin<GraphEventEnvelope> For(ElementGraph graph, string source, NodaTime.Instant at, Op key,
  Option<string> traceParent = default, Option<string> traceState = default) =>
  Of(GraphEventType.Graph, source, ContentAddress.OfGraph(graph), at, traceParent, traceState, key);

 public static Fin<GraphEventEnvelope> For(GraphDelta delta, double tolerance, string source, NodaTime.Instant at, Op key,
  Option<string> traceParent = default, Option<string> traceState = default) =>
  Of(GraphEventType.Delta, source, ContentAddress.Of(delta.ToCanonicalBytes(tolerance).Span), at, traceParent, traceState, key);

 // Canonical attribute rows a carrier writes as transport headers; the binding prefix (ce-, ce_) is the lane's.
 public Seq<(string Key, string Value)> Attributes() {
  Seq<(string Key, string Value)> rows =
   [("id", Id), ("source", Source), ("type", Type.Key), ("subject", Subject.ToValue()),
    ("time", NodaTime.Text.InstantPattern.ExtendedIso.Format(At))];
  rows = TraceParent.Match(Some: held => rows.Add(("traceparent", held)), None: () => rows);
  return TraceState.Match(Some: held => rows.Add(("tracestate", held)), None: () => rows);
 }

 // Consumer-side inverse over one attribute frame: unknown keys pass untouched (outer-wrapper tolerance), the
 // token and subject re-admit through their owners, and a missing required attribute rails typed.
 public static Fin<GraphEventEnvelope> Admit(Seq<(string Key, string Value)> attributes, Op key) {
  Map<string, string> frame = attributes.Fold(Map<string, string>(), static (held, row) => held.AddOrUpdate(row.Key, row.Value));
  return Required(frame, "type", key).Bind(token =>
   Required(frame, "source", key).Bind(source =>
    Required(frame, "subject", key).Bind(subject =>
     Required(frame, "time", key).Bind(time =>
      !GraphEventType.TryGet(token, out GraphEventType? kind) ? ElementFault.ValueRejected(key, $"<event-type-unknown:{token}>")
      : ContentAddress.Validate(subject, null, out ContentAddress? address) is not null ? ElementFault.ValueRejected(key, $"<event-subject-malformed:{subject}>")
      : NodaTime.Text.InstantPattern.ExtendedIso.Parse(time) is { Success: true } at
       ? Of(kind!, source, address!, at.Value, frame.Find("traceparent"), frame.Find("tracestate"), key)
       : ElementFault.ValueRejected(key, $"<event-time-malformed:{time}>")))));
 }

 static Fin<GraphEventEnvelope> Of(GraphEventType type, string source, ContentAddress subject, NodaTime.Instant at,
  Option<string> traceParent, Option<string> traceState, Op key) =>
  string.IsNullOrWhiteSpace(source) ? ElementFault.ValueRejected(key, "<event-source-blank>")
  : traceParent.Exists(static held => !TraceParentShaped(held)) ? ElementFault.ValueRejected(key, "<event-traceparent-malformed>")
  : Fin.Succ(new GraphEventEnvelope(type, source.Trim(), subject, at, traceParent, traceState));

 static Fin<string> Required(Map<string, string> frame, string attribute, Op key) =>
  frame.Find(attribute).ToFin(ElementFault.ValueRejected(key, $"<event-attribute-absent:{attribute}>"));

 // W3C traceparent: 2-hex version "-" 32-hex trace id "-" 16-hex span id "-" 2-hex flags, lowercase.
 static bool TraceParentShaped(string value) =>
  value is { Length: 55 } && value[2] == '-' && value[35] == '-' && value[52] == '-'
   && value.Index().All(static slot => slot.Index is 2 or 35 or 52 || char.IsAsciiHexDigitLower(slot.Item));
}
```

## [04]-[IMPLEMENTATION_LAW]

- [KEY_VERBATIM_LAW]: the wire carries every identity VERBATIM — `NodeId` as the X32 string, `UInt128` content keys (`RepresentationContentHash` entries, `AppearanceKey`, `RasterKey`, `InputKey`, `ProfileRef.ContentKey`, `ResultBlob`) as 16-byte BIG-ENDIAN `bytes` fields, the persisted `System.IO.Hashing.XxHash128` canonical form the TypeScript `hash-wasm` `h128` boundary normalizes its little-endian digest to and the Python `ContentKey.memory` (`to_bytes(16, "little")`) flips from — one canonical crossing form, the LE/BE normalize owned once per peer at its decode boundary. A peer REPRODUCES a key over the count-prefixed injective `CanonicalBytes` canon (the `Projection/address#CANONICAL_WRITER` count-prefix law) and never re-mints: until the queued `PY_WIRE_ALIGNMENT` canonical-writer mirrors land, transcription-not-recomputation is the only legal peer posture, and a peer-side re-derived id, a second digest function, or a little-endian key crossing is the named drift defect. The big-endian WIRE key form is DISTINCT from the little-endian `CanonicalWriter.U128` write — the latter is the interior HASHING canon (bytes fed to the hasher), the former the persisted KEY form (bytes the hasher emits), and conflating them forks every cross-runtime key.
- [CODEC_DIVISION]: three generators, one lane each — `Grpc.Tools` (`GrpcServices=None`, `PrivateAssets=all`) emits the `rasm.element.v1` message classes at build; Mapperly emits every per-case seam↔wire field transcription (compile-time, zero reflection, the `[UserMapping]` key codecs pinning identity crossings); the Thinktecture generated total `Switch` owns encode case dispatch and the protobuf generated `PayloadCase`/`ValueCase`/`EdgeCase` closed enums own decode dispatch. `[MapDerivedType]` is the class-hierarchy rail — a `oneof` envelope's case messages share no base type, so the envelope folds dispatch through the seam's own generated `Switch` (a new union case is a compile break at exactly one site per family) while Mapperly owns the field-by-field transcription the protobuf runtime does not; a hand-rolled per-field assignment body beside a Mapperly-generatable mapping, or a runtime/reflection mapper, is the deleted form.
- [ADMISSION_AND_DEPTH_GATE]: `DecodeGraph`/`DecodeDelta` parse under the admitted positive `WireLimits` size and recursion budgets. Every decoded value re-crosses its owner gate — including `PropertyValue.Of`, `MeasureValue.OfSi`, `MeasureBand.Admit`, `MaterialUsage.LayerSet.Of`/`ProfileSet.Of`, `Classification.Of`, and `Discipline.Parse` — before the aggregate re-crosses `AdmitOnto` or `IsNormalForm`. Unset oneofs, unknown keyed rows, invalid bands, duplicate ids, and illegal graph structure therefore share the same typed fault rail as in-process authors.
- [EVENT_ENVELOPE]: every broker-published crossing rides one `GraphEventEnvelope` whose subject is the crossing's own content key — `ContentAddress.OfGraph` for a snapshot, the `GraphDelta.ToCanonicalBytes` address for a delta — so consumer dedup is one subject compare and no event identity re-mints; attributes cross under canonical names with the binding prefix owned by the carrier adapter, trace slots cross as data the app-tier propagator fills, and the streaming lane frames one envelope per length-prefixed body (`WriteLengthPrefixedTo` into a buffer writer, `WriteDelimitedTo` onto a stream).
- [CONTRACT_EVOLUTION]: `Graph/element.proto` is the descriptor source the `typescript:core/interchange/contract` gate diffs — the emitted `FileDescriptorSet` classifies every contract change `Identical`/`Additive`/`Breaking`, so a new `oneof` arm or appended field is `Additive` (peers decode old payloads unchanged) and a renumber, type change, or field removal without a `reserved` claim is `Breaking` (the gate refuses the dial). Field numbers are append-only; a removed field reserves its number and name. The golden float-bearing `IfcMaterialLayer` parity vector (`MATERIAL_LAYER_GOLDEN`, a `design_pin` until the counted-bag pin lands) anchors the three-runtime round-trip: one `MaterialLayerWire` crossing whose decoded `MeasureValueWire` reproduces the C# content key byte-for-byte over the counted canonical layout.
