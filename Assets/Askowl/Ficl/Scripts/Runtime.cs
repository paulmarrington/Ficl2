// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Ficl {
  using System;
  using System.Collections.Generic;
  using System.Text;

  /// <a href=""></a>
  // ReSharper disable once ClassNeverInstantiated.Global
  public class Runtime : IDisposable {
    private Log.EventRecorder log = Log.Events("Ficl");
    private bool              debugging;

    #region Code Storage
    internal static Runtime Instance => Cache<Runtime>.Instance;

    internal class ActorDictionary : Dictionary<string, Action> {
      public bool Find(string key) => ContainsKey(key);
    }

    internal class ContextFifo : Fifo<ActorDictionary[]> {
      public Action Actor;

      private bool Find(string key, int mapIndex) {
        for (var i = 0; i < Count; i++) {
          ActorDictionary dictionary = this[i]?[mapIndex];
          if (dictionary?.TryGetValue(key, out Actor) == true) return true;
        }
        return false;
      }

      public bool Immediate(string key) => Find(key, 0);
      public bool Compiled(string  key) => Find(key, 1);
    }

    private void ResetContext() {
      if (Contexts.Count == 0) {
        Contexts.Push(new[] { Core[0], Core[1] });
      } else {
        Contexts.Count = 1; // Back to only Core
      }
      // add base dictionary - searched before namespaces and before core
      Contexts.Push(new[] { new ActorDictionary(), new ActorDictionary() });
    }

    private  ActorDictionary[]                   Core      = { new ActorDictionary(), new ActorDictionary() };
    internal Dictionary<string, ActorDictionary> Context   = new Dictionary<string, ActorDictionary>();
    internal Dictionary<string, ActorDictionary> Immediate = new Dictionary<string, ActorDictionary>();

    internal readonly ContextFifo  Contexts = new ContextFifo();
    internal readonly Fifo<Action> Code     = Fifo<Action>.Instance;

    // ReSharper disable once UnusedMember.Local
    private void DeactivateItem() { ResetContext(); }

    /// <a href=""></a>
    /// <inheritdoc />
    public void Dispose() {
      Code.Dispose();
      Context.Clear();
      Immediate.Clear();
      Contexts.Dispose();
      Cache<Runtime>.Dispose(this);
    }

    internal void Execute(int start, int end) { } //#TBD#//
    #endregion

    #region Runtime Operations
    /// <a href=""></a>
    public void Debugging() { debugging = !debugging; }

    /// <a href=""></a>
    public void StackDump() {
      builder.Clear();
      for (var i = 0; i < FiclFifo.Count; i++) {
        builder.Append($"\n{i}: {FiclFifo[i]}");
      }
      log(builder.ToString());
    }

    private StringBuilder builder = new StringBuilder();
    #endregion

    #region Compile-time Operations
    private Fifo<int>    CompilingStart = Fifo<int>.Instance;
    private Fifo<string> CompilingName  = Fifo<string>.Instance;
    private Fifo<int>    JumpList       = Fifo<int>.Instance;
    private int          codePointer;
    private bool         returning;

    private void CompilingStarter() {
      CompilingStart.Push(Code.Count);
      JumpList.Push(0);
    }

    private void CompilingStarter(string name) {
      CompilingName.Push(name);
      CompilingStarter();
    }

    private void Set() {
      RequiresText(
        name => {
          FiclStackEntry.Item item = FiclFifo.Pop();         // take from stack
          Contexts.Top[1][name] = () => FiclFifo.Push(item); // and save as word
        });
    }

    private void Begin() { CompilingStarter("Begin"); }

    private void Leave() {
      int jumpTo = JumpList.Count - 1;
      Code.Push(() => codePointer = JumpList[jumpTo]); // yep, uses the heap
    }

    private void End() {
      JumpList.Top = Code.Count;
      CompilingStart.Pop();
      CompilingName.Pop();
    }

    private void Again() {
      JumpList.Push(CompilingStart.Top);
      Leave();         // so we jump back to the previous begin
      JumpList.Swap(); // Put the jumpList for leaves to the top
      End();           // point any Leave commands
      JumpList.Swap(); // then return jumpList order
    }

    private void Colon() {
      Leave(); // so we will jump over compiled words (to be run when called)
      CompilingStarter();
      RequiresText((name) => CompilingName.Push(name));
    }

    private void Semicolon() {
      int first = CompilingStart.Pop(), last = Code.Count - 1;

      void wordListActor() {
        returning = false;
        for (int i = first; (i <= last) && !returning; i++) Code[i]();
        returning = false;
      }

      Contexts.Top[1][CompilingName.Pop()] = wordListActor;
      End();
    }

    private void Return() { Code.Push(() => returning = true); }

    private void If() {
      CompilingStarter("If");
      Code.Push(
        () => {
          if (!FiclFifo.Pop().True()) codePointer++; // skip leave
        });
      Leave(); // this gets hit if the if statement is false
    }

    private void Else() {
      int jumpTo = JumpList.Count;                     // leave the if true block
      Code.Push(() => codePointer = JumpList[jumpTo]); // yep, uses the heap
      End();                                           // end of the if true block
      Begin();                                         // start of the if false block
    }

    internal void RequiresText(Action<string> useWordActor, string terminator = " ") {
      useText     = useWordActor;
      NeedsTextTo = terminator;
    }

    private  Action<string> useText;
    internal string         NeedsTextTo;

    internal void ConsumeText(string text) {
      NeedsTextTo = null;
      useText(text);
    }
    #endregion

    #region Lists
    private void ListStart()   { fifoAnchors.Push(FiclFifo.Count); }
    private void ListEnd()     { RequiresText(ProcessList); }
    private void DoubleQuote() { RequiresText((text) => FiclFifo.Push(text), "\""); }
    private void SingleQuote() { RequiresText((text) => FiclFifo.Push(text), "'"); }
    private void OpenBracket() { RequiresText((text) => { },                 ")"); }

    private void ProcessList(string word) {
      if (Contexts.Compiled(word)) {
        var actor = Contexts.Actor;
        for (int bottom = fifoAnchors.Pop(); FiclFifo.Count > bottom;) actor();
      } else {
        FiclFifo.Count = fifoAnchors.Pop();
      }
    }
    #endregion

    internal readonly FiclFifo  FiclFifo    = FiclFifo.Instance as FiclFifo;
    private           Fifo<int> fifoAnchors = Fifo<int>.Instance;

    private Runtime() {
      ResetContext();
      ActorDictionary immediate = Core[0];
      ActorDictionary word      = Core[1];

      #region Arithmetic
      word["+"]   = FiclFifo.Add;
      word["-"]   = FiclFifo.Subtract;
      word["*"]   = FiclFifo.Multiply;
      word["/"]   = FiclFifo.Divide;
      word["dec"] = FiclFifo.Decrement;
      word["inc"] = FiclFifo.Increment;
      #endregion

      #region Boolean Operators
      word["and"] = FiclFifo.And;
      word["not"] = FiclFifo.Not;
      word["or"]  = FiclFifo.Or;
      #endregion

      #region Comparators
      word[">"]  = FiclFifo.GreaterThan;
      word["<"]  = FiclFifo.LessThan;
      word[">="] = FiclFifo.GreaterThanOrEqual;
      word["<="] = FiclFifo.LessThanOrEqual;
      word["="]  = FiclFifo.Equals;
      #endregion

      #region Debugging
      word[".d"] = Debugging;
      word[".s"] = StackDump;
      #endregion

      #region Define New Words
      immediate[":"] = Colon;
      word["return"] = Return;
      immediate[";"] = Semicolon;
      #endregion

      #region Flow Control
      immediate["begin"] = Begin;
      immediate["leave"] = Leave;
      immediate["again"] = Again;
      immediate["end"]   = End;
      immediate["if"]    = If;
      immediate["else"]  = Else;
      immediate["then"]  = End;
      #endregion

      #region I/O
      word["."] = () => log(FiclFifo.Pop().ToString());
      #endregion

      #region Stack Control
      word["drop"] = FiclFifo.Drop;
      word["dup"]  = FiclFifo.Dup;
      word["swap"] = FiclFifo.Swaps;
      word["over"] = FiclFifo.Over;
      #endregion

      #region Arrays, strings, comments and other objects
      word["["]       = ListStart;
      immediate["]:"] = ListEnd;
      immediate["\""] = DoubleQuote;
      immediate["'"]  = SingleQuote;
      immediate["("]  = OpenBracket;
      #endregion

      #region Variables
      immediate["set:"]       = Set;
      immediate[":on-update"] = FiclFifo.OnUpdate;
      immediate["ref:"]       = FiclFifo.Ref;
      #endregion
    }
  }
}