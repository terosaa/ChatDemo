// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBus.SubscriptionContext
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.ServiceBus.Messaging;

namespace Microsoft.AspNet.SignalR.ServiceBus
{
  internal class SubscriptionContext
  {
    public string TopicPath { get; private set; }

    public string Name { get; private set; }

    public MessageReceiver Receiver { get; private set; }

    public SubscriptionContext(string topicPath, string subName, MessageReceiver receiver)
    {
      this.TopicPath = topicPath;
      this.Name = subName;
      this.Receiver = receiver;
    }
  }
}
