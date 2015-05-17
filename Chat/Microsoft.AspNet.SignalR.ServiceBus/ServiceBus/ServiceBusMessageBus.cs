// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBus.ServiceBusMessageBus
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.ServiceBus
{
  /// <summary>
  /// Uses Windows Azure Service Bus topics to scale-out SignalR applications in web farms.
  /// 
  /// </summary>
  public class ServiceBusMessageBus : ScaleoutMessageBus
  {
    private const string SignalRTopicPrefix = "SIGNALR_TOPIC";
    private ServiceBusConnectionContext _connectionContext;
    private TraceSource _trace;
    private readonly ServiceBusConnection _connection;
    private readonly string[] _topics;

    protected override int StreamCount
    {
      get
      {
        return this._topics.Length;
      }
    }

    public ServiceBusMessageBus(IDependencyResolver resolver, ServiceBusScaleoutConfiguration configuration)
      : base(resolver, (ScaleoutConfiguration) configuration)
    {
      if (configuration == null)
        throw new ArgumentNullException("configuration");
      this._trace = DependencyResolverExtensions.Resolve<ITraceManager>(resolver)["SignalR." + typeof (ServiceBusMessageBus).Name];
      this._connection = new ServiceBusConnection(configuration, this._trace);
      this._topics = Enumerable.ToArray<string>(Enumerable.Select<int, string>(Enumerable.Range(0, configuration.TopicCount), (Func<int, string>) (topicIndex => string.Concat(new object[4]
      {
        (object) "SIGNALR_TOPIC_",
        (object) configuration.TopicPrefix,
        (object) "_",
        (object) topicIndex
      }))));
      this._connectionContext = new ServiceBusConnectionContext(configuration, (IList<string>) this._topics, this._trace, new Action<int, IEnumerable<BrokeredMessage>>(this.OnMessage), new Action<int, Exception>(((ScaleoutMessageBus) this).OnError), new Action<int>(((ScaleoutMessageBus) this).Open));
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.Subscribe));
    }

    protected override Task Send(int streamIndex, IList<Message> messages)
    {
      Stream stream = ServiceBusMessage.ToStream(messages);
      this.TraceMessages(messages, "Sending");
      return this._connectionContext.Publish(streamIndex, stream);
    }

    private void OnMessage(int topicIndex, IEnumerable<BrokeredMessage> messages)
    {
      if (!Enumerable.Any<BrokeredMessage>(messages))
        this.Open(topicIndex);
      foreach (BrokeredMessage brokeredMessage in messages)
      {
        using (brokeredMessage)
        {
          ScaleoutMessage message = ServiceBusMessage.FromBrokeredMessage(brokeredMessage);
          this.TraceMessages(message.Messages, "Receiving");
          this.OnReceived(topicIndex, (ulong) brokeredMessage.EnqueuedSequenceNumber, message);
        }
      }
    }

    private void Subscribe(object state)
    {
      this._connection.Subscribe(this._connectionContext);
    }

    private void TraceMessages(IList<Message> messages, string messageType)
    {
      if (!this._trace.Switch.ShouldTrace(TraceEventType.Verbose))
        return;
      foreach (Message message in (IEnumerable<Message>) messages)
        TraceSourceExtensions.TraceVerbose(this._trace, "{0} {1} bytes over Service Bus: {2}", (object) messageType, (object) message.Value.Array.Length, (object) message.GetString());
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      if (this._connectionContext != null)
        this._connectionContext.Dispose();
      if (this._connection == null)
        return;
      this._connection.Dispose();
    }
  }
}
