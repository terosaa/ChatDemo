// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBus.ServiceBusConnectionContext
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.ServiceBus
{
  public class ServiceBusConnectionContext : IDisposable
  {
    private readonly ServiceBusScaleoutConfiguration _configuration;
    private readonly SubscriptionContext[] _subscriptions;
    private readonly TopicClient[] _topicClients;
    private readonly TraceSource _trace;

    public object SubscriptionsLock { get; private set; }

    public object TopicClientsLock { get; private set; }

    public IList<string> TopicNames { get; private set; }

    public Action<int, IEnumerable<BrokeredMessage>> Handler { get; private set; }

    public Action<int, Exception> ErrorHandler { get; private set; }

    public Action<int> OpenStream { get; private set; }

    public bool IsDisposed { get; private set; }

    public NamespaceManager NamespaceManager { get; set; }

    public ServiceBusConnectionContext(ServiceBusScaleoutConfiguration configuration, IList<string> topicNames, TraceSource traceSource, Action<int, IEnumerable<BrokeredMessage>> handler, Action<int, Exception> errorHandler, Action<int> openStream)
    {
      if (topicNames == null)
        throw new ArgumentNullException("topicNames");
      this._configuration = configuration;
      this._subscriptions = new SubscriptionContext[topicNames.Count];
      this._topicClients = new TopicClient[topicNames.Count];
      this._trace = traceSource;
      this.TopicNames = topicNames;
      this.Handler = handler;
      this.ErrorHandler = errorHandler;
      this.OpenStream = openStream;
      this.TopicClientsLock = new object();
      this.SubscriptionsLock = new object();
    }

    public Task Publish(int topicIndex, Stream stream)
    {
      if (this.IsDisposed)
        return TaskAsyncHelper.Empty;
      BrokeredMessage message = new BrokeredMessage(stream, true)
      {
        TimeToLive = this._configuration.TimeToLive
      };
      if (message.Size > (long) this._configuration.MaximumMessageSize)
        TraceSourceExtensions.TraceWarning(this._trace, "Message size {0}KB exceeds the maximum size limit of {1}KB : {2}", (object) (message.Size / 1024L), (object) (this._configuration.MaximumMessageSize / 1024), (object) message);
      return this._topicClients[topicIndex].SendAsync(message);
    }

    internal void SetSubscriptionContext(SubscriptionContext subscriptionContext, int topicIndex)
    {
      lock (this.SubscriptionsLock)
      {
        if (this.IsDisposed)
          return;
        this._subscriptions[topicIndex] = subscriptionContext;
      }
    }

    internal void SetTopicClients(TopicClient topicClient, int topicIndex)
    {
      lock (this.TopicClientsLock)
      {
        if (this.IsDisposed)
          return;
        this._topicClients[topicIndex] = topicClient;
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this.IsDisposed)
        return;
      lock (this.TopicClientsLock)
      {
        lock (this.SubscriptionsLock)
        {
          for (int local_0 = 0; local_0 < this.TopicNames.Count; ++local_0)
          {
            TopicClient local_1 = this._topicClients[local_0];
            if (local_1 != null)
              local_1.Close();
            SubscriptionContext local_2 = this._subscriptions[local_0];
            if (local_2 != null)
            {
              local_2.Receiver.Close();
              this.NamespaceManager.DeleteSubscription(local_2.TopicPath, local_2.Name);
            }
          }
          this.IsDisposed = true;
        }
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
    }
  }
}
