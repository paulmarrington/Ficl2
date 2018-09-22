namespace Askowl.Ficl {
  /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
  public class FiclStack : Stack<FiclStack.Item> {
    internal        void Push(int    number) => Push(Integer.New(number));
    internal        void Push(long   number) => Push(Long.New(number));
    internal        void Push(float  number) => Push(Double.New(number));
    internal        void Push(double number) => Push(Double.New(number));
    public override Item Pop()               => base.Pop().Release();

    public void Add()          => Push(Swap().Pop().Plus(Pop()));
    public void Add(int other) => Push(Pop().Plus(Integer.New(1)));
    public void Subtract()     => Push(Swap().Pop().Minus(Pop()));
    public void Multiply()     => Push(Swap().Pop().Times(Pop()));
    public void Divide()       => Push(Swap().Pop().Divide(Pop()));
    public void Increment()    => Add(1);
    public void Decrement()    => Add(-1);

    public void And() => Push(Swap().Pop().And(Pop()));

    /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
    public interface Item : Cache.Boxed {
      Item Release();
      Item Plus(Item   other);
      Item Minus(Item  other);
      Item Times(Item  other);
      Item Divide(Item other);

      Item And(Item                 other);
      Item Or(Item                  other);
      Item Equals(Item              other);
      Item LessThan(Item            other);
      Item GreaterThan(Item         other);
      Item LessThanOrEquals(Item    other);
      Item GreaterThanOrEquals(Item other);
      Item Not();
    }

    private static int nz(int    number)      => number == 0 ? 0 : 1;
    private static int nz(long   number)      => number == 0 ? 0 : 1;
    private static int nz(float  number)      => Compare.AlmostEqual(number, 0) ? 0 : 1;
    private static int nz(double number)      => Compare.AlmostEqual(number, 0) ? 0 : 1;
    private static int eq(int    a, int    b) => a == b ? 0 : 1;
    private static int eq(long   a, long   b) => a == b ? 0 : 1;
    private static int eq(float  a, float  b) => Compare.AlmostEqual(a, b) ? 0 : 1;
    private static int eq(double a, double b) => Compare.AlmostEqual(a, b) ? 0 : 1;
    private static int lt(int    a, int    b) => a < b ? 0 : 1;
    private static int lt(long   a, long   b) => a < b ? 0 : 1;
    private static int lt(float  a, float  b) => a < b ? 0 : 1;
    private static int lt(double a, double b) => a < b ? 0 : 1;

    /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
    public class Item<T> : Cache.Boxed<T>, Item {
      internal new static Item New(T      item) => Cache.Boxed<T>.New(item) as Item;
      internal new static Item Clone(Item item) => Clone(item as Cache.Boxed<T>) as Item;
      public              Item Release()        => Recycle() as Item;

      public virtual Item Plus(Item   other) => Clone(this);
      public virtual Item Minus(Item  other) => Clone(this);
      public virtual Item Times(Item  other) => Clone(this);
      public virtual Item Divide(Item other) => Clone(this);

      public virtual Item And(Item                 other) => Integer.New(0);
      public virtual Item Or(Item                  other) => Integer.New(0);
      public virtual Item Equals(Item              other) => Integer.New(0);
      public virtual Item LessThan(Item            other) => Integer.New(0);
      public virtual Item GreaterThan(Item         other) => Integer.New(0);
      public virtual Item LessThanOrEquals(Item    other) => Integer.New(0);
      public virtual Item GreaterThanOrEquals(Item other) => Integer.New(0);
      public virtual Item Not()                           => Integer.New(0);
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Integer : Item<int> {
      /// <a href=""></a> <inheritdoc />
      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value        + number.Value);
          case Long number:    return Long.New(Value   + number.Value);
          case Float number:   return Float.New(Value  + number.Value);
          case Double number:  return Double.New(Value + number.Value);
          default:             return base.Plus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value        - number.Value);
          case Long number:    return Long.New(Value   - number.Value);
          case Float number:   return Float.New(Value  - number.Value);
          case Double number:  return Double.New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value        * number.Value);
          case Long number:    return Long.New(Value   * number.Value);
          case Float number:   return Float.New(Value  * number.Value);
          case Double number:  return Double.New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value        / number.Value);
          case Long number:    return Long.New(Value   / number.Value);
          case Float number:   return Float.New(Value  / number.Value);
          case Double number:  return Double.New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }

      public override Item And(Item other) {
        switch (other) {
          case Integer number: return New(Value      & number.Value);
          case Long number:    return Long.New(Value & number.Value);
          case Float number:   return New(nz(Value)  & nz(number.Value));
          case Double number:  return New(nz(Value)  & nz(number.Value));
          default:             return base.And(other);
        }
      }

      public override Item Or(Item other) {
        switch (other) {
          case Integer number: return New(Value            | number.Value);
          case Long number:    return Long.New(Value       | number.Value);
          case Float number:   return Float.New(nz(Value)  | nz(number.Value));
          case Double number:  return Double.New(nz(Value) | nz(number.Value));
          default:             return base.Or(other);
        }
      }

      public override Item Equals(Item other) {
        switch (other) {
          case Integer number: return New(eq(Value, number.Value));
          case Long number:    return New(eq(Value, number.Value));
          case Float number:   return New(eq(Value, number.Value));
          case Double number:  return New(eq(Value, number.Value));
          default:             return base.Equals(other);
        }
      }

      public override Item LessThan(Item other) {
        switch (other) {
          case Integer number: return New(lt(Value, number.Value));
          case Long number:    return New(lt(Value, number.Value));
          case Float number:   return New(lt(Value, number.Value));
          case Double number:  return New(lt(Value, number.Value));
          default:             return base.LessThan(other);
        }
      }

      public override Item GreaterThan(Item other) {
        switch (other) {
          case Integer number: return New(nz(lt(Value, number.Value) | eq(Value, number.Value)));
          case Long number:    return New(nz(lt(Value, number.Value) | eq(Value, number.Value)));
          case Float number:   return New(nz(lt(Value, number.Value) | eq(Value, number.Value)));
          case Double number:  return New(nz(lt(Value, number.Value) | eq(Value, number.Value)));
          default:             return base.GreaterThan(other);
        }
      }

      public override Item LessThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return New(lt(Value, number.Value) | eq(Value, number.Value));
          case Long number:    return New(lt(Value, number.Value) | eq(Value, number.Value));
          case Float number:   return New(lt(Value, number.Value) | eq(Value, number.Value));
          case Double number:  return New(lt(Value, number.Value) | eq(Value, number.Value));
          default:             return base.LessThanOrEquals(other);
        }
      }

      public override Item GreaterThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return New(nz(lt(Value, number.Value)));
          case Long number:    return New(nz(lt(Value, number.Value)));
          case Float number:   return New(nz(lt(Value, number.Value)));
          case Double number:  return New(nz(lt(Value, number.Value)));
          default:             return base.GreaterThanOrEquals(other);
        }
      }


      public override Item Not() {
        switch (this) {
          case Cache.Boxed _: return New(nz(Value));
          default:             return base.Not();
        }
      }
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Long : Item<long> {
      /// <a href=""></a> <inheritdoc />
      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value        + number.Value);
          case Long number:    return New(Value        + number.Value);
          case Float number:   return Float.New(Value  + number.Value);
          case Double number:  return Double.New(Value + number.Value);
          default:             return base.Plus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value        - number.Value);
          case Long number:    return New(Value        - number.Value);
          case Float number:   return Float.New(Value  - number.Value);
          case Double number:  return Double.New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value        * number.Value);
          case Long number:    return New(Value        * number.Value);
          case Float number:   return Float.New(Value  * number.Value);
          case Double number:  return Double.New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value        / number.Value);
          case Long number:    return New(Value        / number.Value);
          case Float number:   return Float.New(Value  / number.Value);
          case Double number:  return Double.New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Float : Item<float> {
      /// <a href=""></a> <inheritdoc />
      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value        + number.Value);
          case Long number:    return New(Value        + number.Value);
          case Float number:   return New(Value        + number.Value);
          case Double number:  return Double.New(Value + number.Value);
          default:             return base.Plus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value        - number.Value);
          case Long number:    return New(Value        - number.Value);
          case Float number:   return New(Value        - number.Value);
          case Double number:  return Double.New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value        * number.Value);
          case Long number:    return New(Value        * number.Value);
          case Float number:   return New(Value        * number.Value);
          case Double number:  return Double.New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value        / number.Value);
          case Long number:    return New(Value        / number.Value);
          case Float number:   return New(Value        / number.Value);
          case Double number:  return Double.New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Double : Item<double> {
      /// <a href=""></a> <inheritdoc />
      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value + number.Value);
          case Long number:    return New(Value + number.Value);
          case Float number:   return New(Value + number.Value);
          case Double number:  return New(Value + number.Value);
          default:             return base.Plus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value - number.Value);
          case Long number:    return New(Value - number.Value);
          case Float number:   return New(Value - number.Value);
          case Double number:  return New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value * number.Value);
          case Long number:    return New(Value * number.Value);
          case Float number:   return New(Value * number.Value);
          case Double number:  return New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      /// <a href=""></a> <inheritdoc />
      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value / number.Value);
          case Long number:    return New(Value / number.Value);
          case Float number:   return New(Value / number.Value);
          case Double number:  return New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }
    }
  }
}