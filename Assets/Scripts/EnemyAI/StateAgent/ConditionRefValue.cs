using System;

public enum Predicate
{
    Less,
    LessOrEqual,
    Equal,
    GreaterOrEqual,
    Greater
}

[Serializable]
public class RefValue<T> where T : struct
{
    public T value;

    public static implicit operator T(RefValue<T> refValue) { return refValue.value; }
}

public abstract class Condition
{
    public abstract bool IsTrue();
}

public class Condition<T> : Condition where T : struct, IComparable<T>
{
    private readonly RefValue<T> refValue;
    private readonly Predicate predicate;
    private readonly T value;

    public Condition(RefValue<T> refValue, Predicate predicate, T value)
    {
        this.refValue = refValue;
        this.predicate = predicate;
        this.value = value;
    }

    public override bool IsTrue()
    {
        switch (predicate)
        {
            case Predicate.Less:
                return refValue.value.CompareTo(value) < 0;
            case Predicate.LessOrEqual:
                return refValue.value.CompareTo(value) <= 0;
            case Predicate.Equal:
                return refValue.value.CompareTo(value) == 0;
            case Predicate.GreaterOrEqual:
                return refValue.value.CompareTo(value) >= 0;
            case Predicate.Greater:
                return refValue.value.CompareTo(value) > 0;
            default:
                return false;
        }
    }
}
