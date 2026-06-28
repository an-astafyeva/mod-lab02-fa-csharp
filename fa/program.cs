using System;
using System.Collections.Generic;

namespace fans
{
  public class State
  {
    public string Name = string.Empty;
    public Dictionary<char, State> Transitions = new();
    public bool IsAcceptState;
  }

  // FA1: язык 1*0?1* — строка из единиц с не более чем одним нулём
  // Состояния: q0(start,acc) -1-> q0, -0-> q1; q1(acc) -1-> q1, -0-> dead; dead -любой-> dead
  public class FA1
  {
    public bool? Run(IEnumerable<char> s)
    {
      var dead = new State { Name = "dead", IsAcceptState = false };
      dead.Transitions['0'] = dead;
      dead.Transitions['1'] = dead;

      var q1 = new State { Name = "q1", IsAcceptState = true };   // после единственного нуля
      q1.Transitions['0'] = dead;
      q1.Transitions['1'] = q1;

      var q0 = new State { Name = "q0", IsAcceptState = true };   // начало / блок единиц
      q0.Transitions['0'] = q1;
      q0.Transitions['1'] = q0;

      var current = q0;
      foreach (var c in s)
      {
        if (!current.Transitions.TryGetValue(c, out var next))
          return null;
        current = next;
      }
      return current.IsAcceptState;
    }
  }

  // FA2: язык 0*1*0* | 1*0*1* — не более одной смены типа символа
  // Реализуем как два параллельных автомата (A: 0*1*0*, B: 1*0*1*), принимаем если хоть один принял
  public class FA2
  {
    private bool RunSingle(IEnumerable<char> s, bool startWithZero)
    {
      // startWithZero=true:  0*1*0*  (состояния: z0->o->z1->dead)
      // startWithZero=false: 1*0*1*  (состояния: o0->z->o1->dead)
      char first = startWithZero ? '0' : '1';
      char second = startWithZero ? '1' : '0';

      var dead = new State { Name = "dead", IsAcceptState = false };
      dead.Transitions[first]  = dead;
      dead.Transitions[second] = dead;

      var s2 = new State { Name = "s2", IsAcceptState = true };
      s2.Transitions[first]  = s2;
      s2.Transitions[second] = dead;

      var s1 = new State { Name = "s1", IsAcceptState = true };
      s1.Transitions[first]  = s2;
      s1.Transitions[second] = s1;

      var s0 = new State { Name = "s0", IsAcceptState = true };
      s0.Transitions[first]  = s0;
      s0.Transitions[second] = s1;

      var current = s0;
      foreach (var c in s)
      {
        if (!current.Transitions.TryGetValue(c, out var next))
          return false;
        current = next;
      }
      return current.IsAcceptState;
    }

    public bool? Run(IEnumerable<char> s)
    {
      // нужно пройти строку дважды — материализуем
      var list = new List<char>(s);
      return RunSingle(list, true) || RunSingle(list, false);
    }
  }

  // FA3: язык (00|11)* — строка из пар одинаковых символов
  // Состояния: q0(start,acc), q1(ждём 0), q2(ждём 1), dead
  public class FA3
  {
    public bool? Run(IEnumerable<char> s)
    {
      var dead = new State { Name = "dead", IsAcceptState = false };
      dead.Transitions['0'] = dead;
      dead.Transitions['1'] = dead;

      var q2 = new State { Name = "q2", IsAcceptState = false };  // получили 1, ждём ещё 1

      var q1 = new State { Name = "q1", IsAcceptState = false };  // получили 0, ждём ещё 0

      var q0 = new State { Name = "q0", IsAcceptState = true };   // начало / чётная позиция
      q0.Transitions['0'] = q1;
      q0.Transitions['1'] = q2;

      q1.Transitions['0'] = q0;
      q1.Transitions['1'] = dead;

      q2.Transitions['0'] = dead;
      q2.Transitions['1'] = q0;

      var current = q0;
      foreach (var c in s)
      {
        if (!current.Transitions.TryGetValue(c, out var next))
          return null;
        current = next;
      }
      return current.IsAcceptState;
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      string s = "01111";
      FA1 fa1 = new FA1();
      Console.WriteLine($"FA1(\"{s}\") = {fa1.Run(s)}");
      FA2 fa2 = new FA2();
      Console.WriteLine($"FA2(\"{s}\") = {fa2.Run(s)}");
      FA3 fa3 = new FA3();
      Console.WriteLine($"FA3(\"{s}\") = {fa3.Run(s)}");
    }
  }
}
