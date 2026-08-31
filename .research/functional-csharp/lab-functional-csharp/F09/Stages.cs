namespace Lab.F09;

internal static class Stages {
    public static Option<int> Inspect(int id) =>
        from initial in Some(id)
        from first in TransformationOne(initial)
        from second in TransformationTwo(first)
        from result in TransformationThree(second)
        select result;

    private static Option<int> TransformationOne(int value) => value > 0 ? Some(value * 2) : Option<int>.None;

    private static Option<int> TransformationTwo(int value) => value < 100 ? Some(value + 1) : Option<int>.None;

    private static Option<int> TransformationThree(int value) => value % 2 == 1 ? Some(value) : Option<int>.None;
}
