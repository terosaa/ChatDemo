// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBus.ServiceBusMessage
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNet.SignalR.ServiceBus
{
  public static class ServiceBusMessage
  {
    public static Stream ToStream(IList<Message> messages)
    {
      if (messages == null)
        throw new ArgumentNullException("messages");
      return (Stream) new MemoryStream(new ScaleoutMessage(messages).ToBytes());
    }

    public static ScaleoutMessage FromBrokeredMessage(BrokeredMessage brokeredMessage)
    {
      if (brokeredMessage == null)
        throw new ArgumentNullException("brokeredMessage");
      using (MemoryStream memoryStream = new MemoryStream())
      {
        brokeredMessage.GetBody<Stream>().CopyTo((Stream) memoryStream);
        return ScaleoutMessage.FromBytes(memoryStream.ToArray());
      }
    }
  }
}
