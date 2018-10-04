namespace Askowl.Ficl {
  using System;

  /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
  public class FiclStackEntry {
    /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
    public interface Item : Cache.Boxed {
      // ReSharper disable MissingXmlDoc

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

      Item   Not();
      bool   True();
      string ToString();

      // ReSharper restore MissingXmlDoc
    }

    // ReSharper disable InconsistentNaming
    private static int nz(int    number)      => number == 0 ? 0 : 1;
    private static int nz(long   number)      => number == 0 ? 0 : 1;
    private static int nz(float  number)      => Compare.AlmostEqual(number, 0) ? 0 : 1;
    private static int nz(double number)      => Compare.AlmostEqual(number, 0) ? 0 : 1;
    private static int lt(int    a, int    b) => a < b ? 0 : 1;
    private static int lt(long   a, long   b) => a < b ? 0 : 1;
    private static int lt(float  a, float  b) => a < b ? 0 : 1;
    private static int lt(double a, double b) => a < b ? 0 : 1;
    private static int eq(int    a, int    b) => a == b ? 1 : 0;
    private static int eq(long   a, long   b) => a == b ? 1 : 0;
    private static int eq(float  a, float  b) => Compare.AlmostEqual(a, b) ? 1 : 0;
    private static int eq(double a, double b) => Compare.AlmostEqual(a, b) ? 1 : 0;

    private static int eq(object      a, int    b) => a.Equals(b) ? 1 : 0;
    private static int eq(object      a, long   b) => a.Equals(b) ? 1 : 0;
    private static int eq(object      a, float  b) => a.Equals(b) ? 1 : 0;
    private static int eq(object      a, double b) => a.Equals(b) ? 1 : 0;
    private static int lt(IComparable a, int    b) => a.CompareTo(b) < 0 ? 0 : 1;
    private static int lt(IComparable a, long   b) => a.CompareTo(b) < 0 ? 0 : 1;
    private static int lt(IComparable a, float  b) => a.CompareTo(b) < 0 ? 0 : 1;
    private static int lt(IComparable a, double b) => a.CompareTo(b) < 0 ? 0 : 1;

    // ReSharper restore InconsistentNaming

    /// <a href=""></a> <inheritdoc cref="Cache.Boxed{T}" />
    public class Item<T> : Cache.Boxed<T>, Item {
      // ReSharper disable MissingXmlDoc
      internal new static Item New(T      item) => Cache.Boxed<T>.New(item) as Item;
      internal static     Item Clone(Item item) => Clone(item as Cache.Boxed<T>) as Item;
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

      public virtual Item Not() => Integer.New(0);

      public virtual bool True() => false;

      public override string ToString() => Value.ToString();
      // ReSharper restore MissingXmlDoc
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Object : Item<object> { // ReSharper disable MissingXmlDoc
      public override bool True() => !string.IsNullOrWhiteSpace(Value.ToString());

      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value.ToString() + number.Value);
          case Long number:    return New(Value.ToString() + number.Value);
          case Float number:   return New(Value.ToString() + number.Value);
          case Double number:  return New(Value.ToString() + number.Value);
          default:             return base.Plus(other);
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
        if (Value is IComparable value) {
          switch (other) {
            case Integer number: return New(lt(value, number.Value));
            case Long number:    return New(lt(value, number.Value));
            case Float number:   return New(lt(value, number.Value));
            case Double number:  return New(lt(value, number.Value));
          }
        }
        return base.LessThan(other);
      }

      public override Item GreaterThan(Item other) {
        if (Value is IComparable value) {
          switch (other) {
            case Integer number: return New(nz(lt(value, number.Value) | eq(value, number.Value)));
            case Long number:    return New(nz(lt(value, number.Value) | eq(value, number.Value)));
            case Float number:   return New(nz(lt(value, number.Value) | eq(value, number.Value)));
            case Double number:  return New(nz(lt(value, number.Value) | eq(value, number.Value)));
          }
        }
        return base.GreaterThan(other);
      }

      public override Item LessThanOrEquals(Item other) {
        if (Value is IComparable value) {
          switch (other) {
            case Integer number: return New(lt(value, number.Value) | eq(value, number.Value));
            case Long number:    return New(lt(value, number.Value) | eq(value, number.Value));
            case Float number:   return New(lt(value, number.Value) | eq(value, number.Value));
            case Double number:  return New(lt(value, number.Value) | eq(value, number.Value));
          }
        }
        return base.LessThanOrEquals(other);
      }

      public override Item GreaterThanOrEquals(Item other) {
        if (Value is IComparable value) {
          switch (other) {
            case Integer number: return New(nz(lt(value, number.Value)));
            case Long number:    return New(nz(lt(value, number.Value)));
            case Float number:   return New(nz(lt(value, number.Value)));
            case Double number:  return New(nz(lt(value, number.Value)));
          }
        }
        return base.GreaterThanOrEquals(other);
      }
      // ReSharper restore MissingXmlDoc
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Integer : Item<int> {
      // ReSharper disable MissingXmlDoc
      public override bool True() => Value != 0;

      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value                   + number.Value);
          case Long number:    return Long.New(Value              + number.Value);
          case Float number:   return Float.New(Value             + number.Value);
          case Double number:  return Double.New(Value            + number.Value);
          case Object obj:     return Object.New(Value.ToString() + obj.Value);
          default:             return base.Plus(other);
        }
      }

      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value        - number.Value);
          case Long number:    return Long.New(Value   - number.Value);
          case Float number:   return Float.New(Value  - number.Value);
          case Double number:  return Double.New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value        * number.Value);
          case Long number:    return Long.New(Value   * number.Value);
          case Float number:   return Float.New(Value  * number.Value);
          case Double number:  return Double.New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

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
          // ReSharper disable once PatternAlwaysOfType
          case Cache.Boxed _: return New(nz(Value));
          default:            return base.Not();
        }
      }
      // ReSharper restore MissingXmlDoc
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Long : Item<long> {
      // ReSharper disable MissingXmlDoc
      public override bool True() => Value != 0;

      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value                   + number.Value);
          case Long number:    return New(Value                   + number.Value);
          case Float number:   return Float.New(Value             + number.Value);
          case Double number:  return Double.New(Value            + number.Value);
          case Object obj:     return Object.New(Value.ToString() + obj.Value);
          default:             return base.Plus(other);
        }
      }

      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value        - number.Value);
          case Long number:    return New(Value        - number.Value);
          case Float number:   return Float.New(Value  - number.Value);
          case Double number:  return Double.New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value        * number.Value);
          case Long number:    return New(Value        * number.Value);
          case Float number:   return Float.New(Value  * number.Value);
          case Double number:  return Double.New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value        / number.Value);
          case Long number:    return New(Value        / number.Value);
          case Float number:   return Float.New(Value  / number.Value);
          case Double number:  return Double.New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }

      public override Item And(Item other) {
        switch (other) {
          case Integer number: return New(Value     & number.Value);
          case Long number:    return New(Value     & number.Value);
          case Float number:   return New(nz(Value) & nz(number.Value));
          case Double number:  return New(nz(Value) & nz(number.Value));
          default:             return base.And(other);
        }
      }

      public override Item Or(Item other) {
        switch (other) {
          case Integer number: return New(Value             | number.Value);
          case Long number:    return New(Value             | number.Value);
          case Float number:   return Integer.New(nz(Value) | nz(number.Value));
          case Double number:  return Integer.New(nz(Value) | nz(number.Value));
          default:             return base.Or(other);
        }
      }

      public override Item Equals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(eq(Value, (long) number.Value));
          case Long number:    return Integer.New(eq(Value, number.Value));
          case Float number:   return Integer.New(eq(Value, number.Value));
          case Double number:  return Integer.New(eq(Value, number.Value));
          default:             return base.Equals(other);
        }
      }

      public override Item LessThan(Item other) {
        switch (other) {
          case Integer number: return Integer.New(lt(Value, (long) number.Value));
          case Long number:    return Integer.New(lt(Value, number.Value));
          case Float number:   return Integer.New(lt(Value, number.Value));
          case Double number:  return Integer.New(lt(Value, number.Value));
          default:             return base.LessThan(other);
        }
      }

      public override Item GreaterThan(Item other) {
        switch (other) {
          case Integer number: return Integer.New(nz(lt(Value, (long) number.Value) | eq(Value, (long) number.Value)));
          case Long number:    return Integer.New(nz(lt(Value, number.Value)        | eq(Value, number.Value)));
          case Float number:   return Integer.New(nz(lt(Value, number.Value)        | eq(Value, number.Value)));
          case Double number:  return Integer.New(nz(lt(Value, number.Value)        | eq(Value, number.Value)));
          default:             return base.GreaterThan(other);
        }
      }

      public override Item LessThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(lt(Value, (long) number.Value) | eq(Value, (long) number.Value));
          case Long number:    return Integer.New(lt(Value, number.Value)        | eq(Value, number.Value));
          case Float number:   return Integer.New(lt(Value, number.Value)        | eq(Value, number.Value));
          case Double number:  return Integer.New(lt(Value, number.Value)        | eq(Value, number.Value));
          default:             return base.LessThanOrEquals(other);
        }
      }

      public override Item GreaterThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(nz(lt(Value, (long) number.Value)));
          case Long number:    return Integer.New(nz(lt(Value, number.Value)));
          case Float number:   return Integer.New(nz(lt(Value, number.Value)));
          case Double number:  return Integer.New(nz(lt(Value, number.Value)));
          default:             return base.GreaterThanOrEquals(other);
        }
      }

      public override Item Not() {
        switch (this) {
          // ReSharper disable once PatternAlwaysOfType
          case Cache.Boxed _: return Integer.New(nz(Value));
          default:            return base.Not();
        }
      }
      // ReSharper restore MissingXmlDoc
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Float : Item<float> {
      // ReSharper disable MissingXmlDoc
      public override bool True() => nz(Value) != 0;

      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value                   + number.Value);
          case Long number:    return New(Value                   + number.Value);
          case Float number:   return New(Value                   + number.Value);
          case Double number:  return Double.New(Value            + number.Value);
          case Object obj:     return Object.New(Value.ToString() + obj.Value);
          default:             return base.Plus(other);
        }
      }

      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value        - number.Value);
          case Long number:    return New(Value        - number.Value);
          case Float number:   return New(Value        - number.Value);
          case Double number:  return Double.New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value        * number.Value);
          case Long number:    return New(Value        * number.Value);
          case Float number:   return New(Value        * number.Value);
          case Double number:  return Double.New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value        / number.Value);
          case Long number:    return New(Value        / number.Value);
          case Float number:   return New(Value        / number.Value);
          case Double number:  return Double.New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }

      public override Item And(Item other) {
        switch (other) {
          case Integer number: return New(nz(Value) & number.Value);
          case Long number:    return New(nz(Value) & number.Value);
          case Float number:   return New(nz(Value) & nz(number.Value));
          case Double number:  return New(nz(Value) & nz(number.Value));
          default:             return base.And(other);
        }
      }

      public override Item Or(Item other) {
        switch (other) {
          case Integer number: return Integer.New(nz(Value) | number.Value);
          case Long number:    return Integer.New(nz(Value) | nz(number.Value));
          case Float number:   return Integer.New(nz(Value) | nz(number.Value));
          case Double number:  return Integer.New(nz(Value) | nz(number.Value));
          default:             return base.Or(other);
        }
      }

      public override Item Equals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(eq(Value, (float) number.Value));
          case Long number:    return Integer.New(eq(Value, (float) number.Value));
          case Float number:   return Integer.New(eq(Value, number.Value));
          case Double number:  return Integer.New(eq(Value, number.Value));
          default:             return base.Equals(other);
        }
      }

      public override Item LessThan(Item other) {
        switch (other) {
          case Integer number: return Integer.New(lt(Value, (float) number.Value));
          case Long number:    return Integer.New(lt(Value, (float) number.Value));
          case Float number:   return Integer.New(lt(Value, number.Value));
          case Double number:  return Integer.New(lt(Value, number.Value));
          default:             return base.LessThan(other);
        }
      }

      public override Item GreaterThan(Item other) {
        switch (other) {
          case Integer number:
            return Integer.New(nz(lt(Value, (float) number.Value) | eq(Value, (float) number.Value)));
          case Long number:   return Integer.New(nz(lt(Value, (float) number.Value) | eq(Value, (float) number.Value)));
          case Float number:  return Integer.New(nz(lt(Value, number.Value)         | eq(Value, number.Value)));
          case Double number: return Integer.New(nz(lt(Value, number.Value)         | eq(Value, number.Value)));
          default:            return base.GreaterThan(other);
        }
      }

      public override Item LessThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(lt(Value, (float) number.Value) | eq(Value, (float) number.Value));
          case Long number:    return Integer.New(lt(Value, (float) number.Value) | eq(Value, (float) number.Value));
          case Float number:   return Integer.New(lt(Value, number.Value)         | eq(Value, number.Value));
          case Double number:  return Integer.New(lt(Value, number.Value)         | eq(Value, number.Value));
          default:             return base.LessThanOrEquals(other);
        }
      }

      public override Item GreaterThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(nz(lt(Value, (float) number.Value)));
          case Long number:    return Integer.New(nz(lt(Value, (float) number.Value)));
          case Float number:   return Integer.New(nz(lt(Value, number.Value)));
          case Double number:  return Integer.New(nz(lt(Value, number.Value)));
          default:             return base.GreaterThanOrEquals(other);
        }
      }

      public override Item Not() {
        switch (this) {
          // ReSharper disable once PatternAlwaysOfType
          case Cache.Boxed _: return Integer.New(nz(Value));
          default:            return base.Not();
        }
      }
      // ReSharper restore MissingXmlDoc
    }

    /// <a href=""></a><inheritdoc cref="Cache.Boxed{T}" />
    public class Double : Item<double> {
      // ReSharper disable MissingXmlDoc
      public override bool True() => nz(Value) != 0;

      public override Item Plus(Item other) {
        switch (other) {
          case Integer number: return New(Value                   + number.Value);
          case Long number:    return New(Value                   + number.Value);
          case Float number:   return New(Value                   + number.Value);
          case Double number:  return New(Value                   + number.Value);
          case Object obj:     return Object.New(Value.ToString() + obj.Value);
          default:             return base.Plus(other);
        }
      }

      public override Item Minus(Item other) {
        switch (other) {
          case Integer number: return New(Value - number.Value);
          case Long number:    return New(Value - number.Value);
          case Float number:   return New(Value - number.Value);
          case Double number:  return New(Value - number.Value);
          default:             return base.Minus(other);
        }
      }

      public override Item Times(Item other) {
        switch (other) {
          case Integer number: return New(Value * number.Value);
          case Long number:    return New(Value * number.Value);
          case Float number:   return New(Value * number.Value);
          case Double number:  return New(Value * number.Value);
          default:             return base.Times(other);
        }
      }

      public override Item Divide(Item other) {
        switch (other) {
          case Integer number: return New(Value / number.Value);
          case Long number:    return New(Value / number.Value);
          case Float number:   return New(Value / number.Value);
          case Double number:  return New(Value / number.Value);
          default:             return base.Divide(other);
        }
      }

      public override Item And(Item other) {
        switch (other) {
          case Integer number: return New(nz(Value) & number.Value);
          case Long number:    return New(nz(Value) & number.Value);
          case Float number:   return New(nz(Value) & nz(number.Value));
          case Double number:  return New(nz(Value) & nz(number.Value));
          default:             return base.And(other);
        }
      }

      public override Item Or(Item other) {
        switch (other) {
          case Integer number: return Integer.New(nz(Value) | number.Value);
          case Long number:    return Integer.New(nz(Value) | nz(number.Value));
          case Float number:   return Integer.New(nz(Value) | nz(number.Value));
          case Double number:  return Integer.New(nz(Value) | nz(number.Value));
          default:             return base.Or(other);
        }
      }

      public override Item Equals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(eq(Value, (double) number.Value));
          case Long number:    return Integer.New(eq(Value, (double) number.Value));
          case Float number:   return Integer.New(eq(Value, (double) number.Value));
          case Double number:  return Integer.New(eq(Value, number.Value));
          default:             return base.Equals(other);
        }
      }

      public override Item LessThan(Item other) {
        switch (other) {
          case Integer number: return Integer.New(lt(Value, (double) number.Value));
          case Long number:    return Integer.New(lt(Value, (double) number.Value));
          case Float number:   return Integer.New(lt(Value, (double) number.Value));
          case Double number:  return Integer.New(lt(Value, number.Value));
          default:             return base.LessThan(other);
        }
      }

      public override Item GreaterThan(Item other) {
        switch (other) {
          case Integer number:
            return Integer.New(nz(lt(Value, (double) number.Value) | eq(Value, (double) number.Value)));
          case Long number: return Integer.New(nz(lt(Value, (double) number.Value) | eq(Value, (double) number.Value)));
          case Float number:
            return Integer.New(nz(lt(Value, (double) number.Value) | eq(Value, (double) number.Value)));
          case Double number: return Integer.New(nz(lt(Value, number.Value) | eq(Value, number.Value)));
          default:            return base.GreaterThan(other);
        }
      }

      public override Item LessThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(lt(Value, (double) number.Value) | eq(Value, (double) number.Value));
          case Long number:    return Integer.New(lt(Value, (double) number.Value) | eq(Value, (double) number.Value));
          case Float number:   return Integer.New(lt(Value, (double) number.Value) | eq(Value, (double) number.Value));
          case Double number:  return Integer.New(lt(Value, number.Value)          | eq(Value, number.Value));
          default:             return base.LessThanOrEquals(other);
        }
      }

      public override Item GreaterThanOrEquals(Item other) {
        switch (other) {
          case Integer number: return Integer.New(nz(lt(Value, (double) number.Value)));
          case Long number:    return Integer.New(nz(lt(Value, (double) number.Value)));
          case Float number:   return Integer.New(nz(lt(Value, (double) number.Value)));
          case Double number:  return Integer.New(nz(lt(Value, number.Value)));
          default:             return base.GreaterThanOrEquals(other);
        }
      }

      public override Item Not() {
        switch (this) {
          // ReSharper disable once PatternAlwaysOfType
          case Cache.Boxed _: return Integer.New(nz(Value));
          default:            return base.Not();
        }
      }
      // ReSharper restore MissingXmlDoc
    }
  }
}