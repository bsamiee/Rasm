namespace Rasm.Vectors;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Vector {
    public static Fin<TOut> Project<TOut>(VectorIntent intent, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(intent).ToFin(op.InvalidInput())
               from model in Optional(context).ToFin(op.MissingContext())
               from result in active.Project<TOut>(context: model, op: op)
               select result;
    }
}
