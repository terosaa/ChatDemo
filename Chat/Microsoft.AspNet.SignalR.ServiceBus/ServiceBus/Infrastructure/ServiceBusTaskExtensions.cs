// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBus.Infrastructure.ServiceBusTaskExtensions
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.ServiceBus.Infrastructure
{
  public static class ServiceBusTaskExtensions
  {
    public static Task SendAsync(this TopicClient client, BrokeredMessage message)
    {
      return Task.Factory.FromAsync((Func<AsyncCallback, object, IAsyncResult>) ((cb, state) => client.BeginSend((BrokeredMessage) state, cb, (object) null)), new Action<IAsyncResult>(client.EndSend), (object) message);
    }
  }
}
