namespace Lab.T03;

[Union<TypeParamRef1, string>]
internal readonly partial struct Result<T> {
    public static implicit operator Result<T>(T value) => CreateT(value);
}
