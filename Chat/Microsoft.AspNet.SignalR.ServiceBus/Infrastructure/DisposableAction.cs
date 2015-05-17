// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.Infrastructure.DisposableAction
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using System;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
  internal class DisposableAction : IDisposable
  {
    public static readonly DisposableAction Empty = new DisposableAction((Action) (() => {}));
    private Action<object> _action;
    private readonly object _state;

    public DisposableAction(Action action)
      : this((Action<object>) (state => ((Action) state)()), (object) action)
    {
    }

    public DisposableAction(Action<object> action, object state)
    {
      this._action = action;
      this._state = state;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      Interlocked.Exchange<Action<object>>(ref this._action, (Action<object>) (state => {}))(this._state);
    }

    public void Dispose()
    {
      this.Dispose(true);
    }
  }
}
