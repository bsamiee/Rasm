using Rasm.Grasshopper.UI;
using Rasm.TestKit;

namespace Rasm.Grasshopper.Tests.UI;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class SubscriptionGens {
    public static readonly Op Key = Op.Of(name: "subscription-test");
    public static readonly Gen<int> Tag = Gen.Int[0, 1000];
    public static Subscription Of(int tag, IList<int> log) => Subscription.Atom(detach: () => log.Add(item: tag));
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SubscriptionMonoidLaws {
    [Fact]
    public void LeftIdentity() =>
        Spec.ForAll(SubscriptionGens.Tag, static t => {
            List<int> log = [];
            (Subscription.Empty | SubscriptionGens.Of(tag: t, log: log)).Dispose();
            Assert.Equal<int>(expected: [t], actual: log);
        });
    [Fact]
    public void RightIdentity() =>
        Spec.ForAll(SubscriptionGens.Tag, static t => {
            List<int> log = [];
            (SubscriptionGens.Of(tag: t, log: log) | Subscription.Empty).Dispose();
            Assert.Equal<int>(expected: [t], actual: log);
        });
    [Fact]
    public void Associativity() =>
        Spec.ForAll(SubscriptionGens.Tag.Select(SubscriptionGens.Tag, SubscriptionGens.Tag), static tuple => {
            int a = tuple.Item1, b = tuple.Item2, c = tuple.Item3;
            List<int> leftLog = [];
            Subscription leftGrouped = SubscriptionGens.Of(tag: a, log: leftLog) | SubscriptionGens.Of(tag: b, log: leftLog) | SubscriptionGens.Of(tag: c, log: leftLog);
            leftGrouped.Dispose();
            List<int> rightLog = [];
            Subscription rightGrouped = SubscriptionGens.Of(tag: a, log: rightLog) | (SubscriptionGens.Of(tag: b, log: rightLog) | SubscriptionGens.Of(tag: c, log: rightLog));
            rightGrouped.Dispose();
            Assert.Equal(expected: leftLog, actual: rightLog);
        });
    [Fact]
    public void CompositeWithCompositeFlattensLifo() {
        List<int> log = [];
        Subscription left = SubscriptionGens.Of(tag: 1, log: log) | SubscriptionGens.Of(tag: 2, log: log);
        Subscription right = SubscriptionGens.Of(tag: 3, log: log) | SubscriptionGens.Of(tag: 4, log: log);
        (left | right).Dispose();
        Assert.Equal<int>(expected: [4, 3, 2, 1], actual: log);
    }
}

public sealed class SubscriptionLifoLaws {
    [Fact]
    public void GeneratedStackMatchesListModel() =>
        Spec.ForAll(SubscriptionGens.Tag.Array[1, 16], static tags => {
            List<int> actual = [];
            Subscription.Composite(members: toSeq(tags.Select(tag => SubscriptionGens.Of(tag: tag, log: actual)))).Dispose();
            Assert.Equal(expected: [.. tags.Reverse()], actual: [.. actual]);
        });
    [Fact]
    public void PairDisposesRightThenLeft() =>
        Spec.ForAll(SubscriptionGens.Tag.Select(SubscriptionGens.Tag), static tuple => {
            int a = tuple.Item1, b = tuple.Item2;
            List<int> log = [];
            (SubscriptionGens.Of(tag: a, log: log) | SubscriptionGens.Of(tag: b, log: log)).Dispose();
            Assert.Equal<int>(expected: [b, a], actual: log);
        });
    [Fact]
    public void CompositeFactoryAcceptsFifoStoresLifo() =>
        Spec.ForAll(SubscriptionGens.Tag.Select(SubscriptionGens.Tag, SubscriptionGens.Tag), static tuple => {
            int a = tuple.Item1, b = tuple.Item2, c = tuple.Item3;
            List<int> log = [];
            Subscription.Composite(members: Seq(
                SubscriptionGens.Of(tag: a, log: log),
                SubscriptionGens.Of(tag: b, log: log),
                SubscriptionGens.Of(tag: c, log: log))).Dispose();
            Assert.Equal<int>(expected: [c, b, a], actual: log);
        });
    [Fact]
    public void EmptyDisposeIsNoOp() {
        List<int> log = [];
        Subscription.Empty.Dispose();
        Assert.Empty(collection: log);
    }
    [Fact]
    public void CompositeNormalizesZeroMembersToEmpty() =>
        Assert.Same(expected: Subscription.Empty, actual: Subscription.Composite(members: Seq<Subscription>()));
    [Fact]
    public void CompositeNormalizesSingletonToAtom() {
        Subscription atom = Subscription.Atom(detach: static () => { });
        Assert.Same(expected: atom, actual: Subscription.Composite(members: Seq(atom)));
    }
    [Fact]
    public void AtomDisposeIsNonIdempotentAndRunsDetachEachTime() {
        int count = 0;
        Subscription atom = Subscription.Atom(detach: () => count++);
        atom.Dispose();
        atom.Dispose();
        Assert.Equal(expected: 2, actual: count);
    }
    [Fact]
    public void DetachOnceAtomRunsAtMostOnce() {
        int count = 0;
        Subscription atom = Subscription.Atom(detach: () => count++, detachOnce: true);
        atom.Dispose();
        atom.Dispose();
        Assert.Equal(expected: 1, actual: count);
    }
}

public sealed class SubscriptionBindLaws {
    [Fact]
    public void AttachRunsOnceAtBindDetachRunsOnceAtDispose() {
        int attaches = 0;
        int detaches = 0;
        Fin<Subscription> result = Subscription.Bind(attach: () => attaches++, detach: () => detaches++);
        Spec.Succ(result: result, then: sub => {
            Assert.Equal(expected: 1, actual: attaches);
            Assert.Equal(expected: 0, actual: detaches);
            sub.Dispose();
            Assert.Equal(expected: 1, actual: detaches);
        });
    }
    [Fact]
    public void SuccessfulBindDetachFaultDoesNotEscapeDisposeRail() {
        int attaches = 0;
        Fin<Subscription> result = Subscription.Bind(attach: () => attaches++, detach: static () => throw new InvalidOperationException(message: "detach-boom"));
        Spec.Succ(result: result, then: sub => {
            sub.Dispose();
            Assert.Equal(expected: 1, actual: attaches);
        });
    }
    [Fact]
    public void MarshalToUiIsClassifiedStaticallyAndExecutedByBridgeScenario() {
        Subscription.AtomCase atom = Assert.IsType<Subscription.AtomCase>(@object: Subscription.Atom(detach: static () => { }, marshalToUi: true));
        Assert.True(condition: atom.MarshalToUi);
    }
    [Fact]
    public void AttachThrowsTriggersRollbackDetach() {
        int rollbacks = 0;
        Fin<Subscription> result = Subscription.Bind(
            attach: static () => throw new InvalidOperationException(message: "attach-boom"),
            detach: () => rollbacks++);
        // FailCategory binds to the stable Fault.Category contract; message-text drift would not regress the test.
        Spec.FailCategory(result: result, category: "Mutation");
        Assert.Equal(expected: 1, actual: rollbacks);
    }
    [Fact]
    public void DoubleFaultProducesManyErrorsCarryingBothCauses() =>
        // Spec.FailMany factors out the Fail + IsType<ManyErrors> + count + per-substring check so any future
        // Error.+ rail (paint hook failures, document mutation rollbacks) reuses the same template.
        Spec.FailMany(
            result: Subscription.Bind(
                attach: static () => throw new InvalidOperationException(message: "attach-boom"),
                detach: static () => throw new InvalidOperationException(message: "rollback-boom")),
            expectedCount: 2,
            expectedSubstrings: ["attach-boom", "rollback-boom"]);
}
