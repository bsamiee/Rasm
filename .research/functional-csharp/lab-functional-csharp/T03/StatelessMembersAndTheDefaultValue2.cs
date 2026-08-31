namespace Lab.T03;

internal readonly record struct Absent;

[Union<Absent, int>(T1IsStateless = true, DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember)]
internal readonly partial struct MaybeInt : IDisallowDefaultValue {
    public static MaybeInt None => new Absent();
}
