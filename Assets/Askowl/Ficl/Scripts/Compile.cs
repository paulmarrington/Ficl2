// Copyright 2013-2018+ (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Ficl {
  using System;

  /// <a href=""></a>
  public class Compile {
    /// <a href=""></a>
    public Runtime Source(string sourceText) {
      source    = sourceText;
      location  = 0;
      length    = source.Length;
      debugging = false;
      runtime   = Runtime.Instance;

      Action add(Action actor) => runtime.Code.Push(actor);

      Action run(Action actor) {
        actor();
        // in case the immediate call needs a name for the word/reference/etc
        if (runtime.NeedsTextTo != null) runtime.ConsumeText(NextString);
        return actor;
      }

      // ReSharper disable once UnusedLocalFunctionReturnValue
      Action compileWord(string name) {
        switch (name) {
          case var k when runtime.Contexts.Immediate(k):    return run(runtime.Contexts.Actor);
          case var k when runtime.Contexts.Compiled(k):     return add(runtime.Contexts.Actor);
          case var n when long.TryParse(n, out long i):     return add(() => runtime.FiclFifo.Push(i));
          case var n when double.TryParse(n, out double d): return add(() => runtime.FiclFifo.Push(d));
          default:                                          return add(() => runtime.Execute(0, 0));
        }
      }

      for (string name = NextWord; name != null; name = NextWord) compileWord(name);

      return runtime;
    }

    private string  source;
    private int     location, length;
    private bool    debugging;
    private Runtime runtime;

    private readonly Log.EventRecorder log = Log.Events(action: "FICL Compile");

    private string NextString {
      get {
        if (char.IsWhiteSpace(runtime.NeedsTextTo[0])) return NextWord;
        int first = location + 1;
        int end   = source.IndexOf(runtime.NeedsTextTo, first + 1, StringComparison.Ordinal);
        if (end == -1) {
          location = source.Length;
          return source.Substring(first);
        } else {
          location = end + runtime.NeedsTextTo.Length;
          return source.Substring(first, end - location);
        }
      }
    }
    private string NextWord {
      get {
        do {
          if (location >= length) return null;
        } while (char.IsWhiteSpace(source[location++]));

        int first = location - 1;
        do {
          if (location == length) {
            location = length + 1;
            break;
          }
        } while (!Char.IsWhiteSpace(source[location++]));

        string word = source.Substring(first, location - first - 1);
        if (debugging) log($" [[{word}]]\n");
        return word;
      }
    }
  }
}