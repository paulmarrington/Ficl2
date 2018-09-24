// Copyright 2013-2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System;
  using System.Collections;

  /// <a href=""></a>
  public class Ficl1 {
    public Ficl() {
      context.oStack       = newList(64);
      context.iStack       = newLongList(64);
      context.compiling    = newWordList(64);
      context.runningWords = newWordList(32);
      context.secondStack  = newList(32);
      context.ctlStack     = newIntList(32);
      context.uploadStack  = newList(32);
      context.writer       = stringBuffer();
      context.errors       = stringBuffer();
      context.recording    = null;
      init();
    }

    /** compile and run source */
    public bool run(string sourceToCompile) {
      try {
        if (sourceToCompile == null || sourceToCompile.Length == 0) return true;
        context.abort         = false;
        context.source        = sourceToCompile;
        context.sourceLength  = strlen(context.source);
        context.sourcePointer = 0;
        clearErrors();
        bool more = true;
        do {
          if (context.ctlsp == 0) start_compiling("FICL.run");
          more = compile();
          if (context.ctlsp == 1) runWordList(end_compiling());
        } while (more);
        if (context.isp < 0) { abort("Error running: " + sourceToCompile); } else if (context.debugging) {
          stackDump("Debugging");
        }
        return !context.abort;
      } catch (Exception Exception) {
        error(Exception);
        resetContext();
        throw new Exception();
      } finally { output(context.errors); }
    }

    private bool compile() {
      string name;
      while ((name = getSourceWord()) != null) {
        context.isCompileMode = true;
        CompiledWord word = (CompiledWord) getStore(name);
        if (word != null) {
          if (word.type == TYPE_IMMEDIATE) ((Runnable) word.data).run();
          else context.compiling[context.cp++] = word;
        } else if (!compileLiteral(name)) {
          word = createWord(name, TYPE_WORD_LIST, nan, new CompiledWord[0]);
          putStore(name, word);
          context.compiling[context.cp++] = word;
        }
        context.isCompileMode = false;
        if (context.abort) return false;
        if (context.cp > 32 && context.ctlsp == 1) return true;
        if (context.cp > 48) return true;
      }
      return false;
    }

    public delegate void Runnable();

    public void extend(string name, Runnable actor) {
      storeWord(name, TYPE_RUNNABLE, 0, actor);
    }

    /** Extend FICL with a new word. */
    public void immediate(string name, Runnable actor) {
      storeWord(name, TYPE_IMMEDIATE, 0, actor);
    }

    public long popInt() {
      return context.iStack[--context.isp & 63];
    }

    public Object pop() {
      long popped = context.iStack[--context.isp & 63];
      if (popped == nan) { return context.oStack[--context.osp & 63]; }
      return newInteger(popped);
    }

    public void push(Object data) {
      context.oStack[context.osp++ & 63] = data;
      context.iStack[context.isp++ & 63] = nan;
    }

    public void pushInt(long data) {
      context.iStack[context.isp++ & 63] = data;
    }

    public void abort(string msg) {
      error(msg);
      resetContext();
      context.abort = true;
    }

    public Object get(string name) {
      CompiledWord word = (CompiledWord) getStore(name);
      if (word == null) return null;
      switch (word.type) {
        case TYPE_DATA: return word.data;
        case TYPE_INT:  return newInteger(word.integer);
        default:        return null;
      }
    }

    public long getInt(string name) {
      CompiledWord word = (CompiledWord) getStore(name);
      if (word != null && word.type == TYPE_INT) return word.integer;
      return nan;
    }

    public void put(string name, Object value) {
      Object old = get(name);
      if (value == null || !value.Equals(old)) storeWord(name, TYPE_DATA, 0, value);
    }

    public void putInt(string name, long value) {
      if (getInt(name) != value) storeWord(name, TYPE_INT, value, null);
    }

//   public void remove(string name) { deleteStore(name); }
//   public bool trigger_word_on_update(string trigger, string data) {
//    CompiledWord triggerWord = (CompiledWord) getStore(trigger);
//    CompiledWord dataWord = (CompiledWord) getStore(data);
//    if(triggerWord == null || dataWord == null) return false;
//    dataWord.trigger = triggerWord;
//    return true;
//  }
    public void runWord(CompiledWord word) {
      if (context.debugging) dumpWord(word);
      if (empty(word)) return;
      context.runningWords[context.rwsp++ & 31] = context.currentWord = word;
      switch (word.type) {
        case TYPE_DATA:
          context.oStack[context.osp++ & 63] = word.data;
          context.iStack[context.isp++ & 63] = nan;
          break;
        case TYPE_INT:
          context.iStack[context.isp++ & 63] = word.integer;
          break;
        case TYPE_RUNNABLE:
          ((Runnable) word.data).run();
          break;
        case TYPE_WORD_LIST:
          runWordList((CompiledWord[]) word.data);
          break;
      }
      context.rwsp--;
    }

    public void runWord(string name) {
      CompiledWord word = (CompiledWord) getStore(name);
      if (word != null) runWord(word);
    }

    public class Persist : FICL_Persistence {
      Hashtable store = new Hashtable();

      public void location(string dir) { }

      public string upload(string name, string contents) {
        store.put(name, contents);
        return "";
      }

      public string load(string name) {
        string contents = (string) store.get(name);
        return (contents == null) ? "" : contents;
      }
    }

    private FICL_Persistence persist = new Persist();

    // as in ficl.setPersistence(new FICL_File(baseDir));
    public void setPersistence(FICL_Persistence persistance) {
      persist = persistance;
    }

    ////////////////////////////////////////////////////////
    public class Context {
      public bool           abort,         isCompileMode, debugging;
      public stringBuffer   writer,        errors,        recording;
      public string         source,        compilingWord, lastDefinition;
      public int            sourcePointer, sourceLength,  jumpBy;
      public CompiledWord   currentWord;
      public CompiledWord[] compiling, runningWords;
      public Object[]       oStack,    secondStack, uploadStack;
      public int[]          ctlStack;
      public long[]         iStack;
      public int            isp, osp, cp, rwsp, ssp, ulsp, ctlsp;
      public string ref;
    }

    public Context context = newContext();

    private void resetContext() {
      context.isp = context.osp = context.rwsp = context.ssp =
        context.cp = context.jumpBy = context.ulsp = context.ctlsp = 0;
      context.abort = context.isCompileMode = false;
    }

    private class CompiledWord {
      int                   type;
      long                  integer;
      string                name;
      Object                data;
      CompiledWord[]        trigger;
      Hashtable             triggers = new Hashtable();
      public CompiledWord[] last_trigger;
    }

    private static int TYPE_RUNNABLE  = 1;
    private static int TYPE_IMMEDIATE = 2;
    private static int TYPE_WORD_LIST = 4;
    private static int TYPE_DATA      = 8;
    private static int TYPE_INT       = 16;

    private bool copyWord(CompiledWord to, int type, long val, Object data) {
      if (to.type != type || to.integer != val || !cmp(to.data, data)) {
        to.type    = type;
        to.integer = val;
        to.data    = data;
        return true;
      }
      return false;
    }

    private bool cmp(Object left, Object right) {
      if (left == null) { return right == null; }
      return left.Equals(right);
    }

    CompiledWord[] buildWordList() {
      CompiledWord[] words = new CompiledWord[context.cp];
      System.arraycopy(context.compiling, 0, words, 0, context.cp);
      context.cp = 0;
      return words;
    }

    private void runWordList(CompiledWord[] words) {
      int runPointer = 0;
      int end        = words.Length;
      while (runPointer < end) {
        if (context.abort) break;
        runWord(words[runPointer++]);
        if (context.jumpBy != 0) runPointer += context.jumpBy;
        context.jumpBy = 0;
      }
    }

    protected string getSourceWord() {
      do {
        if (context.sourcePointer >= context.sourceLength) return null;
      } while (isSpace(getNextSourceChar()));
      int first = context.sourcePointer - 1;
      do {
        if (context.sourcePointer == context.sourceLength) {
          context.sourcePointer = context.sourceLength + 1;
          break;
        }
      } while (!isSpace(getNextSourceChar()));

      string word = substring(context.source, first, context.sourcePointer - 1);
      if (context.debugging) output(" [[" + word + "]]\n");
      if (context.recording != null) context.recording.append(word).append(' ');
      return word;
    }

    private string getSourceText(char chr) {
      int start = context.sourcePointer;
      context.sourcePointer = context.source.indexOf(chr, context.sourcePointer);
      string text;
      if (context.sourcePointer == -1) {
        context.sourcePointer = context.sourceLength;
        text                  = context.source.substring(start);
      } else { text = context.source.substring(start, context.sourcePointer++); }
      if (context.recording != null) context.recording.append(text).append(chr).append(' ');
      return text;
    }

    private bool isSpace(char c) {
      return (c == ' ')  || (c == '\t') ||
             (c == '\r') || (c == '\n');
    }

    ////////////////////////////////////////////////////////
    private bool compileLiteral(string name) {
      int  sign = -1, idx = 0, len = name.Length();
      long num  = 0;
      if (name.charAt(0) == '-') { sign = idx = 1; }
      long decimal = -10000;
      while (idx < len) {
        int chr = name.charAt(idx++);
        if (chr == '.') { decimal = 1; } else {
          int digit = '0' - chr;
          if (digit > 0 || digit < -9) return false;
          num     =  (num * 10) + digit;
          decimal *= 10;
        }
      }
      if (decimal < 0) {
        context.compiling[context.cp++ & 63] =
          createNewWord("int", TYPE_INT, sign * num, null);
      } else {
        context.compiling[context.cp++ & 63] = createNewWord(
          "data",
          TYPE_DATA, 0, newDouble((sign * num) / (double) decimal));
      }
      return true;
    }

    protected void compileRunnable(Runnable runnable) {
      context.compiling[context.cp++ & 63] =
        createNewWord("run", TYPE_RUNNABLE, 0, runnable);
    }

    private CompiledWord
      createWord(string name, int type, long integer, Object data) {
      CompiledWord word      = (CompiledWord) getStore(name);
      if (word == null) word = newWord();
      return fillWord(word, name, type, integer, data);
    }

    private CompiledWord
      createNewWord(string name, int type, long integer, Object data) {
      return fillWord(newWord(), name, type, integer, data);
    }

    private CompiledWord fillWord(
      CompiledWord word, string name,
      int          type, long   integer, Object data) {
      word.type    = type;
      word.name    = name;
      word.data    = data;
      word.integer = integer;
      return word;
    }

    ////////////////////////////////////////////////////////
    private void storeWord(
      string name,
      int    type, long integer, Object data) {
      CompiledWord word    = (CompiledWord) getStore(name);
      bool         changed = true;
      if (word != null) { changed = copyWord(word, type, integer, data); } else {
        word = createWord(name, type, integer, data);
        putStore(name, word);
      }
      if (word.trigger      != null && changed) runWordList(word.trigger);
      if (word.last_trigger != null && changed) runWordList(word.last_trigger);
    }

    ////////////////////////////////////////////////////////
    private void start_compiling(string name) {
      context.secondStack[context.ssp++ & 31] = context.compilingWord;
      context.compilingWord                   = name;
      context.secondStack[context.ssp++ & 31] = context.compiling;
      context.ctlStack[context.ctlsp++  & 31] = context.cp;
      context.compiling                       = newWordList(64);
      context.cp                              = 0;
    }

    private CompiledWord[] end_compiling() {
      CompiledWord[] wordList = buildWordList();
      context.compiling = (CompiledWord[])
        context.secondStack[--context.ssp & 31];
      context.cp             = context.ctlStack[--context.ctlsp & 31];
      context.lastDefinition = context.compilingWord;
      context.compilingWord = (string)
        context.secondStack[--context.ssp & 31];
      return wordList;
    }

    private Runnable end_compile_word = () => { storeWord(context.compilingWord, TYPE_WORD_LIST, 0, end_compiling()); };
    private Runnable end_compile_trigger = () => {
      CompiledWord[] words  = end_compiling();
      CompiledWord   target = (CompiledWord) getStore(context.lastDefinition);
      if (empty(target)) {
        putInt(context.lastDefinition, 0);
        target = (CompiledWord) getStore(context.lastDefinition);
      }
      if (context.ref.Equals("[last]")) {
        target.last_trigger = words;
        return;
      }
      CompiledWord trigger = (CompiledWord) target.triggers.get(context.ref);
      if (empty(trigger)) {
        trigger = createNewWord(
          "on-update:" + context.lastDefinition,
          TYPE_WORD_LIST, 0, words);
        target.triggers.put(context.ref, trigger);
      }
      trigger.data = words;
      int         length = 0;
      Enumeration e      = target.triggers.elements();
      while (e.hasMoreElements()) {
        CompiledWord word = (CompiledWord) e.nextElement();
        length += ((CompiledWord[]) word.data).Length;
      }
      target.trigger = new CompiledWord[length];
      length         = 0;
      e              = target.triggers.elements();
      while (e.hasMoreElements()) {
        CompiledWord word = (CompiledWord) e.nextElement();
        words = ((CompiledWord[]) word.data);
        System.arraycopy(words, 0, target.trigger, length, words.Length);
        length += words.Length;
      }
    };

    ////////////////////////////////////////////////////////
    private void init() {
      immediate(
        ":", () => {
          start_compiling(getSourceWord());
          context.secondStack[context.ssp++ & 31] = end_compile_word;
        });
      immediate(
        ":on-update", () => {
          start_compiling(context.ref = getSourceWord());
          context.secondStack[context.ssp++ & 31] = end_compile_trigger;
        });
      immediate(
        "ref:", () => {
          context.ref = getSourceWord();
        });
      immediate(
        ";", () => {
          Runnable ender = (Runnable) context.secondStack[--context.ssp & 31];
          if (ender != null) ender.run();
        });

      immediate("(", () => { getSourceText(')'); });
      immediate(
        "\"",
        () => {
          context.compiling[context.cp++ & 63] =
            createNewWord(
              "\"",
              TYPE_DATA, 0, getSourceText('"'));
        });
      immediate(
        "'",
        () => {
          context.compiling[context.cp++ & 63] =
            createNewWord(
              "'",
              TYPE_DATA, 0, getSourceText('\''));
        });
      extend(
        ".",
        () => {
          long data = context.iStack[--context.isp & 63];
          output(" ");
          if (data == nan) { output(context.oStack[--context.osp & 63]); } else { outputInteger(data); }
        });
      extend(
        "and",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++] = a & b;
        });
      extend(
        ">",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++] = a > b ? 1 : 0;
        });
      extend(
        "<",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++] = a < b ? 1 : 0;
        });
      extend(
        ">=",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++] = a >= b ? 1 : 0;
        });
      extend(
        "<=",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++] = a <= b ? 1 : 0;
        });
      extend(
        "+",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++ & 63] = a + b;
        });
      immediate(
        "begin",
        () => {
          context.ctlStack[context.ctlsp++ & 31] = context.cp;
          context.ctlStack[context.ctlsp++ & 31] = 0;
        });
      immediate(
        "leave",
        () => {
          context.ctlStack[context.ctlsp++ & 31] = context.cp;
          context.compiling[context.cp++ & 63] = createNewWord(
            "leave", TYPE_RUNNABLE, 0,
            () => { context.jumpBy = (int) context.currentWord.integer; });
        });
      immediate(
        "again",
        () => {
          int cp, to = context.cp                                                                              + 1;
          while ((cp = context.ctlStack[--context.ctlsp & 31]) != 0) { context.compiling[cp].integer = to - cp - 1; }
          cp = context.ctlStack[--context.ctlsp & 31];
          int start = cp - context.cp - 1;
          context.compiling[context.cp++ & 63] =
            createNewWord(
              "again", TYPE_RUNNABLE, 0,
              (object) (() => { context.jumpBy = start; }));
        });
      immediate(
        "set:",
        () => {
          string name = getSourceWord();
          if (getStore(name) == null) storeWord(name, TYPE_INT, 0, null);
          context.compiling[context.cp++ & 63] =
            createNewWord(
              "set:", TYPE_RUNNABLE, 0,
              () => {
                long value = context.iStack[--context.isp & 63];
                if (value != nan) { storeWord(name, TYPE_INT, value, null); } else {
                  Object object = context.oStack[--context.osp & 63];
                  storeWord(name, TYPE_DATA, 0, object);
                }
              });
        });
      extend(
        "dec",
        () => { context.iStack[(context.isp - 1) & 63] -= 1; });
      immediate(
        ".d",
        () => { context.debugging = !context.debugging; });
      extend(
        ".s",
        () => { stackDump(context.writer); }
      );
      extend(
        "/",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++ & 63] = a / b;
        });
      extend(
        "drop",
        () => {
          long a = context.iStack[--context.isp & 63];
          if (a == nan) --context.osp;
        });
      extend(
        "dup",
        () => {
          long a = context.iStack[(context.isp - 1) & 63];
          if (a == nan) {
            Object ao = context.oStack[(context.osp - 1) & 63];
            context.oStack[context.osp++ & 63] = ao;
          }
          context.iStack[context.isp++ & 63] = a;
        });
      extend(
        "swap", // ( a b -- b a )
        () => {
          long b = context.iStack[(context.isp - 1) & 63];
          long a = context.iStack[(context.isp - 2) & 63];
          if (b == nan && a == nan) {
            Object bo = context.oStack[(context.osp - 1) & 63];
            context.oStack[(context.osp   - 1) & 63] =
              context.oStack[(context.osp - 2) & 63];
            context.oStack[(context.osp - 2) & 63] = bo;
          }
          context.iStack[(context.isp - 1) & 63] = a;
          context.iStack[(context.isp - 2) & 63] = b;
        });
      extend(
        "over", // ( a b -- a b a )
        () => {
          long a = context.iStack[(context.isp - 2) & 63];
          if (a == nan) {
            context.oStack[context.osp++       & 63] =
              context.oStack[(context.osp - 2) & 63];
          }
          context.iStack[context.isp++ & 63] = a;
        });
      extend(
        "=",
        () => {
          long b      = context.iStack[--context.isp & 63];
          long a      = context.iStack[--context.isp & 63];
          long equals = 0;
          if (a == b) {
            if (a == nan) {
              Object bo = context.oStack[--context.osp & 63];
              Object ao = context.oStack[--context.osp & 63];
              context.iStack[context.isp++ & 63] = ao.Equals(bo) ? 1 : 0;
              return;
            }
            equals = 1;
          }
          context.iStack[context.isp++ & 63] = equals;
          if (a == nan) --context.osp;
          if (b == nan) --context.osp;
        });
      immediate(
        "if",
        () => {
          context.ctlStack[context.ctlsp++ & 31] = context.cp;
          context.compiling[context.cp++ & 63] =
            createNewWord(
              "if", TYPE_RUNNABLE, 0,
              () => {
                long a                     = context.iStack[--context.isp & 63];
                if (a == 0) context.jumpBy = (int) context.currentWord.integer;
              });
        });
      immediate(
        "else",
        () => {
          int jumpFrom = context.ctlStack[--context.ctlsp & 31];
          context.compiling[jumpFrom].integer    = context.cp - jumpFrom;
          context.ctlStack[context.ctlsp++ & 31] = context.cp;
          context.compiling[context.cp++ & 63] =
            createNewWord(
              "else", TYPE_RUNNABLE, 0,
              () => { context.jumpBy = (int) context.currentWord.integer; });
        });
      immediate(
        "then",
        () => {
          int jumpFrom = context.ctlStack[--context.ctlsp & 31];
          context.compiling[jumpFrom].integer = context.cp - jumpFrom - 1;
        });
      extend(
        "inc",
        () => { context.iStack[(context.isp - 1) & 63] += 1; });
      extend(
        "-",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++ & 63] = a - b;
        });
      extend(
        "*",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++ & 63] = a * b;
        });
      extend(
        "not",
        () => {
          int isp = (context.isp - 1) & 63;
          context.iStack[isp] = (context.iStack[isp] == 0) ? 1 : 0;
        });
      extend(
        "or",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++ & 63] = a | b;
        });
      extend(
        "return",
        () => { context.jumpBy = 100000; });
      immediate(
        ":upload",
        () => {
          context.uploadStack[context.ulsp++ & 31] = getSourceWord();
          context.recording                        = new stringBuffer();
        });
      immediate(
        ";upload",
        () => {
          string name = (string) context.uploadStack[--context.ulsp & 31];
          context.recording.setLength(context.recording.Length() - 8);
          persist.upload(name, context.recording.tostring());
          context.recording = null;
        });
      immediate(
        "load:",
        () => {
          FICL.this.run(persist.load(getSourceWord()));
        });
      immediate(
        "[",
        () => {
          context.compiling[context.cp++ & 63] = createNewWord(
            "[", TYPE_RUNNABLE, 0,
            () => { context.ctlStack[context.ctlsp++ & 31] = context.isp; });
        });
      immediate(
        "]",
        () => {
          string       name   = getSourceWord();
          CompiledWord action = (CompiledWord) getStore(name);
          context.compiling[context.cp++ & 63] = createNewWord(
            "]" + name, TYPE_RUNNABLE, 0,
            () => {
              int start = context.ctlStack[--context.ctlsp & 31];
              if (start >= context.isp) return; // empty array
              start += 1;                       // leave first on stack

              int end = context.isp;
              int obs = 0;
              for (int i = start; i < end; i++) {
                if (context.iStack[i] == nan) obs++;
              }
              context.isp =  start;
              context.osp -= obs;
              Object[] obl = new Object[obs];
              System.arraycopy(context.oStack, context.osp, obl, 0, obs);
              obs = 0;
              for (int i = start; i < end; i++) {
                if (context.iStack[i] != nan) { pushInt(context.iStack[i]); } else { push(obl[obs++]); }
                ((Runnable) action.data).run();
              }
            });
        });
      extend(
        "\"\"",
        () => { // concat 2 strings
          string s2 = pop().tostring();
          string s1 = pop().tostring();
          push(s1 + s2);
        });
      immediate(
        "remove-word",
        () => { deleteStore(getSourceWord()); });
      extend(
        "<",
        () => {
          long b = context.iStack[--context.isp & 63];
          long a = context.iStack[--context.isp & 63];
          context.iStack[context.isp++ & 63] = a < b ? 1 : 0;
        });
    }

    private bool empty(CompiledWord word) {
      return (word == null) || (word.type == 0) ||
             (word.type == TYPE_WORD_LIST && ((CompiledWord[]) word.data).Length == 0);
    }

    ////////////////////////////////////////////////////////
    private long nan = Long.MAX_VALUE;

    private Context newContext() {
      return new Context();
    }

    private Object getStore(string key) {
      return store.get(key);
    }

    private void putStore(string key, Object value) {
      store.put(key, value);
    }

    private void deleteStore(string name) {
      store.remove(name);
    }

    private Hashtable store = new Hashtable();

    private CompiledWord newWord() {
      return new CompiledWord();
    }

    private Double newDouble(double value) {
      return new Double(value);
    }

    private void clearErrors() {
      context.errors = new stringBuffer();
    }

    protected void output(Object item) {
      context.writer.append(item.tostring());
    }

    protected void outputInteger(long integer) {
      context.writer.append(Long.tostring(integer));
    }

    public string tostring() {
      string result = context.writer.tostring();
      context.writer = stringBuffer();
      return result;
    }

    private void error(Exception Exception) {
//    stringWriter sw = new stringWriter();
//    Exception.printStackTrace(new PrintWriter(sw));
//    error(sw.tostring());
      error(Exception.getMessage());
    }

    private void error(string msg) {
      context.errors.append("\n========\n");
      if (context.isCompileMode) {
        context.errors.append("compile: ").append(context.compilingWord)
               .append(",");
      } else {
        context.errors.append("run: ");
        for (int i = 0; i < (context.rwsp & 31); i++) {
          CompiledWord word = context.runningWords[i];
          string       name = (word == null) ? "null" : word.name;
          context.errors.append(name).append("->");
        }
      }
      context.errors.append(msg);
      stackDump(context.errors);
      context.errors.append("\n========\n");
    }

    private void stackDump(string msg) {
      context.writer.append("\n%%%%%%%%%%\n" + msg + "\n%%%%%%%%%%");
      stackDump(context.writer);
      context.writer.append("%%%%%%%%%%\n%%%%%%%%%%\n");
    }

    private void stackDump(stringBuffer out) {
      out.append("\n");
      int osp = 0;
      if (context.isp > 0) {
        for (int i = 0; i < (context.isp & 63); i++) {
          out.append("\n").append(i).append(':');
          if (context.iStack[i] == nan) {
            out.append(context.oStack[osp++]);
          } else {
            out.append(context.iStack[i]).append(" ");
          }
        }
        out.append("\n======");
      }
      out.append("\nsp=").append(context.isp).append(", ctl sp=").append(context.ctlsp).append(", 2nd sp=")
                         .append(context.ssp).append(", upload sp=").append(context.ulsp).append("\n");
    }

    private void dumpWord(CompiledWord word) {
      if (word == null) {
        context.writer.append("{{**null**}}\n");
        return;
      }
      context.writer.append(" {{").append(word.name).append("=");
      switch (word.type) {
        case TYPE_DATA:
          context.writer.append(word.data.tostring());
          break;
        case TYPE_INT:
          context.writer.append(word.integer);
          break;
        case TYPE_RUNNABLE:
          context.writer.append("run");
          break;
        case TYPE_WORD_LIST:
          context.writer.append("list(").append(((CompiledWord[]) word.data).Length).append(")");
          break;
      }
      context.writer.append("/").append(context.isp).append("}}\n");
    }

    private char getNextSourceChar() {
      return context.source.charAt(context.sourcePointer++);
    }

    private Object[] newList(int items) {
      return new Object[items];
    }

    private CompiledWord[] newWordList(int items) {
      return new CompiledWord[items];
    }

    private int[] newIntList(int items) {
      return new int[items];
    }

    private long[] newLongList(int items) {
      return new long[items];
    }

    private Long newInteger(long value) {
      return new Long(value);
    }

    private stringBuffer stringBuffer() {
      return new stringBuffer();
    }

    private int strlen(string text) {
      return text.Length();
    }

    private string substring(string text, int from, int to) {
      return text.substring(from, to);
    }
  }
}
