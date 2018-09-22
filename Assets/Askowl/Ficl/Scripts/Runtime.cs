// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Ficl {
  using System;
  using System.Collections.Generic;

  /// <a href=""></a>
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Runtime : IDisposable {
    #region Code Storage
    internal static          Runtime Instance => Cache<Runtime>.Instance;
    internal static readonly Runtime Core = Instance;

    internal class ActorDictionary : Dictionary<string, Action> {
      public bool Find(string key) => ContainsKey(key);
    }

    internal ActorDictionary Immediate = new ActorDictionary();
    internal ActorDictionary Word      = new ActorDictionary();

    internal readonly Askowl.Stack<Action> Code = Askowl.Stack<Action>.Instance;

    /// <a href=""></a>
    /// <inheritdoc />
    public void Dispose() {
      Code.Dispose();
      Immediate.Clear();
      Word.Clear();
      Cache<Runtime>.Dispose(this);
    }

    internal void Execute(int start, int end) { } //#TBD#//
    #endregion

    internal readonly FiclStack Items = FiclStack.Instance as FiclStack;

    #region Compiled Words
    private Runtime() {
      var immediate = Core.Immediate;
      var word      = Core.Word;

      #region Arithmetic
      word["+"]   = Items.Add;
      word["-"]   = Items.Subtract;
      word["*"]   = Items.Multiply;
      word["/"]   = Items.Divide;
      word["dec"] = Items.Decrement;
      word["inc"] = Items.Increment;
      #endregion

      #region Boolean Operators
      word["and"] = () => { };
      word["not"] = () => { };
      word["or"]  = () => { };
      #endregion

      #region Comparators
      word[">"]  = () => { };
      word["<"]  = () => { };
      word[">="] = () => { };
      word["<="] = () => { };
      word["="]  = () => { };
      #endregion

      #region Debugging
      word[".d"] = () => { };
      word[".s"] = () => { };
      #endregion

      #region Define New Words
      immediate[":"]           = () => { };
      word["return"]           = () => { };
      immediate[";"]           = () => { };
      word["return"]           = () => { };
      immediate["remove-word"] = () => { };
      #endregion

      #region Flow Control
      immediate["begin"] = () => { };
      immediate["leave"] = () => { };
      immediate["again"] = () => { };
      immediate["if"]    = () => { };
      immediate["else"]  = () => { };
      immediate["then"]  = () => { };
      #endregion

      #region I/O
      word["."] = () => { };
      #endregion

      #region Persistence
      immediate[":upload"] = () => { };
      immediate[";upload"] = () => { };
      immediate["load:"]   = () => { };
      #endregion

      #region Stack Control
      word["drop"] = () => { };
      word["dup"]  = () => { };
      word["swap"] = () => { };
      word["over"] = () => { };
      #endregion

      #region Structures
      immediate["["]  = () => { };
      immediate["]"]  = () => { };
      immediate["\""] = () => { };
      immediate["'"]  = () => { };
      word["\"\""]    = () => { };
      immediate["("]  = () => { };
      #endregion

      #region Variables
      immediate["set:"]       = () => { };
      immediate[":on-update"] = () => { };
      immediate["ref:"]       = () => { };
      #endregion
    }
    #endregion
  }
}