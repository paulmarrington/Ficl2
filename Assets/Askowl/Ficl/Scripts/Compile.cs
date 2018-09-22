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

      Action Add(Action actor) => runtime.Code.Push(actor);

      Action Run(Action actor) {
        actor();
        return actor;
      }

      Action CompileWord(string name) {
        switch (name) {
          case var k when runtime.Immediate.Find(k):        return Run(runtime.Immediate[k]);
          case var k when runtime.Word.Find(k):             return Add(runtime.Word[k]);
          case var n when long.TryParse(n, out long i):     return Add(() => runtime.Items.Push(i));
          case var n when double.TryParse(n, out double d): return Add(() => runtime.Items.Push(d));
          default:                                          return Add(() => runtime.Execute(0, 0));
        }
      }

      for (string name = NextWord; name != null; name = NextWord) CompileWord(name);

      return runtime;
    }

    private string  source;
    private int     location, length;
    private bool    debugging;
    private Runtime runtime;

    private readonly Log.EventRecorder log = Log.Events(action: "FICL Compile");

    private string NextWord {
      get {
        do {
          if (location >= length) return null;
        } while (Char.IsWhiteSpace(source[location++]));

        int first = location - 1;
        do {
          if (location == length) {
            location = length + 1;
            break;
          }
        } while (!Char.IsWhiteSpace(source[location++]));

        var word = source.Substring(first, location - first - 1);
        if (debugging) log($" [[{word}]]\n");
        return word;
      }
    }
  }
}