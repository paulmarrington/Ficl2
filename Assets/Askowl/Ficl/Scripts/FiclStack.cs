namespace Askowl.Ficl {
  /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
  public class FiclFifo : Fifo<FiclStackEntry.Item> {
    internal void Push(int    number) => Push(FiclStackEntry.Integer.New(number));
    internal void Push(long   number) => Push(FiclStackEntry.Long.New(number));
    internal void Push(float  number) => Push(FiclStackEntry.Double.New(number));
    internal void Push(double number) => Push(FiclStackEntry.Double.New(number));
    internal void Push(object text)   => Push(FiclStackEntry.Object.New(text));

    /// <a href=""></a> <inheritdoc />
    public override FiclStackEntry.Item Pop() => base.Pop().Release();

    /// <a href=""></a>
    public void Add() => Push(Swap().Pop().Plus(Pop()));

    private void Add(int other) => Push(Pop().Plus(FiclStackEntry.Integer.New(other)));

    /// <a href=""></a>
    public void Subtract() => Push(Swap().Pop().Minus(Pop()));

    /// <a href=""></a>
    public void Multiply() => Push(Swap().Pop().Times(Pop()));

    /// <a href=""></a>
    public void Divide() => Push(Swap().Pop().Divide(Pop()));

    /// <a href=""></a>
    public void Increment() => Add(1);

    /// <a href=""></a>
    public void Decrement() => Add(-1);

    /// <a href=""></a>
    public void And() => Push(Swap().Pop().And(Pop()));

    /// <a href=""></a>
    public void Or() => Push(Swap().Pop().Or(Pop()));

    /// <a href=""></a>
    public void Equals() => Push(Swap().Pop().Equals(Pop()));

    /// <a href=""></a>
    public void LessThan() => Push(Swap().Pop().LessThan(Pop()));

    /// <a href=""></a>
    public void GreaterThan() => Push(Swap().Pop().GreaterThan(Pop()));

    /// <a href=""></a>
    public void LessThanOrEqual() => Push(Swap().Pop().LessThanOrEquals(Pop()));

    /// <a href=""></a>
    public void GreaterThanOrEqual() => Push(Swap().Pop().GreaterThanOrEquals(Pop()));

    /// <a href=""></a>
    public void Not() => Push(Swap().Pop().Not());

    /// <a href=""></a>
    public void Drop() { Pop(); }

    /// <a href=""></a>
    public void Dup() { Push(Top); }

    /// <a href=""></a>
    public void Swaps() => Swap();

    /// <a href=""></a>
    public void Over() { Push(Next); }

    /// <a href=""></a>
    public void OnUpdate() { }

    /// <a href=""></a>
    public void Ref() { }
  }
}