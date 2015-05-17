// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBus.ServiceBusConnection
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.ServiceBus
{
  internal class ServiceBusConnection : IDisposable
  {
    private static readonly TimeSpan ErrorBackOffAmount = TimeSpan.FromSeconds(5.0);
    private static readonly TimeSpan DefaultReadTimeout = TimeSpan.FromSeconds(60.0);
    private static readonly TimeSpan ErrorReadTimeout = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10.0);
    private const int DefaultReceiveBatchSize = 1000;
    private readonly TimeSpan _backoffTime;
    private readonly TimeSpan _idleSubscriptionTimeout;
    private readonly NamespaceManager _namespaceManager;
    private readonly MessagingFactory _factory;
    private readonly ServiceBusScaleoutConfiguration _configuration;
    private readonly string _connectionString;
    private readonly TraceSource _trace;

    public ServiceBusConnection(ServiceBusScaleoutConfiguration configuration, TraceSource traceSource)
    {
      this._trace = traceSource;
      this._connectionString = configuration.BuildConnectionString();
      try
      {
        this._namespaceManager = NamespaceManager.CreateFromConnectionString(this._connectionString);
        this._factory = MessagingFactory.CreateFromConnectionString(this._connectionString);
        if (configuration.RetryPolicy != null)
          this._factory.RetryPolicy = configuration.RetryPolicy;
        else
          this._factory.RetryPolicy = RetryPolicy.Default;
      }
      catch (ConfigurationErrorsException ex)
      {
        TraceSourceExtensions.TraceError(this._trace, "The configured Service Bus connection string contains an invalid property. Check the exception details for more information.");
        throw;
      }
      this._backoffTime = configuration.BackoffTime;
      this._idleSubscriptionTimeout = configuration.IdleSubscriptionTimeout;
      this._configuration = configuration;
    }

    public void Subscribe(ServiceBusConnectionContext connectionContext)
    {
      if (connectionContext == null)
        throw new ArgumentNullException("connectionContext");
      this._trace.TraceInformation("Subscribing to {0} topic(s) in the service bus...", (object) connectionContext.TopicNames.Count);
      connectionContext.NamespaceManager = this._namespaceManager;
      for (int topicIndex = 0; topicIndex < connectionContext.TopicNames.Count; ++topicIndex)
        this.Retry((Action) (() => this.CreateTopic(connectionContext, topicIndex)));
      this._trace.TraceInformation("Subscription to {0} topics in the service bus Topic service completed successfully.", (object) connectionContext.TopicNames.Count);
    }

    private void CreateTopic(ServiceBusConnectionContext connectionContext, int topicIndex)
    {
      lock (connectionContext.TopicClientsLock)
      {
        if (connectionContext.IsDisposed)
          return;
        string local_0 = connectionContext.TopicNames[topicIndex];
        if (!this._namespaceManager.TopicExists(local_0))
        {
          try
          {
            this._trace.TraceInformation("Creating a new topic {0} in the service bus...", (object) local_0);
            this._namespaceManager.CreateTopic(local_0);
            this._trace.TraceInformation("Creation of a new topic {0} in the service bus completed successfully.", (object) local_0);
          }
          catch (MessagingEntityAlreadyExistsException exception_0)
          {
            this._trace.TraceInformation("Creation of a new topic {0} threw an MessagingEntityAlreadyExistsException.", (object) local_0);
          }
        }
        TopicClient local_1 = TopicClient.CreateFromConnectionString(this._connectionString, local_0);
        if (this._configuration.RetryPolicy != null)
          local_1.RetryPolicy = this._configuration.RetryPolicy;
        else
          local_1.RetryPolicy = RetryPolicy.Default;
        connectionContext.SetTopicClients(local_1, topicIndex);
        this._trace.TraceInformation("Creation of a new topic client {0} completed successfully.", (object) local_0);
      }
      this.CreateSubscription(connectionContext, topicIndex);
    }

    private void CreateSubscription(ServiceBusConnectionContext connectionContext, int topicIndex)
    {
      lock (connectionContext.SubscriptionsLock)
      {
        if (connectionContext.IsDisposed)
          return;
        string local_0 = connectionContext.TopicNames[topicIndex];
        string local_1 = Guid.NewGuid().ToString();
        try
        {
          this._namespaceManager.CreateSubscription(new SubscriptionDescription(local_0, local_1)
          {
            AutoDeleteOnIdle = this._idleSubscriptionTimeout
          });
          this._trace.TraceInformation("Creation of a new subscription {0} for topic {1} in the service bus completed successfully.", (object) local_1, (object) local_0);
        }
        catch (MessagingEntityAlreadyExistsException exception_0)
        {
          this._trace.TraceInformation("Creation of a new subscription {0} for topic {1} threw an MessagingEntityAlreadyExistsException.", (object) local_1, (object) local_0);
        }
        string local_3 = SubscriptionClient.FormatSubscriptionPath(local_0, local_1);
        MessageReceiver local_4 = this._factory.CreateMessageReceiver(local_3, ReceiveMode.ReceiveAndDelete);
        this._trace.TraceInformation("Creation of a message receive for subscription entity path {0} in the service bus completed successfully.", (object) local_3);
        connectionContext.SetSubscriptionContext(new SubscriptionContext(local_0, local_1, local_4), topicIndex);
        this.ProcessMessages(new ServiceBusConnection.ReceiverContext(topicIndex, local_4, connectionContext));
        connectionContext.OpenStream(topicIndex);
      }
    }

    private void Retry(Action action)
    {
      string format = "Failed to create service bus subscription or topic : {0}";
      while (true)
      {
        try
        {
          action();
          break;
        }
        catch (UnauthorizedAccessException ex)
        {
          TraceSourceExtensions.TraceError(this._trace, format, (object) ex.Message);
          break;
        }
        catch (MessagingException ex)
        {
          TraceSourceExtensions.TraceError(this._trace, format, (object) ex.Message);
          if (!ex.IsTransient)
            break;
          Thread.Sleep(ServiceBusConnection.RetryDelay);
        }
        catch (Exception ex)
        {
          TraceSourceExtensions.TraceError(this._trace, format, (object) ex.Message);
          Thread.Sleep(ServiceBusConnection.RetryDelay);
        }
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this._factory == null)
        return;
      this._factory.Close();
    }

    public void Dispose()
    {
      this.Dispose(true);
    }

    private void ProcessMessages(ServiceBusConnection.ReceiverContext receiverContext)
    {
      while (true)
      {
        try
        {
          IAsyncResult asyncResult;
          do
          {
            asyncResult = receiverContext.Receiver.BeginReceiveBatch(1000, receiverContext.ReceiveTimeout, (AsyncCallback) (ar =>
            {
              if (ar.CompletedSynchronously)
                return;
              ServiceBusConnection.ReceiverContext receiverContext1 = (ServiceBusConnection.ReceiverContext) ar.AsyncState;
              if (!this.ContinueReceiving(ar, receiverContext1))
                return;
              this.ProcessMessages(receiverContext1);
            }), (object) receiverContext);
          }
          while (asyncResult.CompletedSynchronously && this.ContinueReceiving(asyncResult, receiverContext));
          break;
        }
        catch (OperationCanceledException ex)
        {
          TraceSourceExtensions.TraceError(this._trace, "OperationCanceledException was thrown in trying to receive the message from the service bus.");
          break;
        }
        catch (Exception ex)
        {
          TraceSourceExtensions.TraceError(this._trace, ex.Message);
          receiverContext.OnError(ex);
          Thread.Sleep(ServiceBusConnection.RetryDelay);
        }
      }
    }

    private bool ContinueReceiving(IAsyncResult asyncResult, ServiceBusConnection.ReceiverContext receiverContext)
    {
      bool flag = true;
      TimeSpan timeOut = this._backoffTime;
      try
      {
        IEnumerable<BrokeredMessage> messages = receiverContext.Receiver.EndReceiveBatch(asyncResult);
        receiverContext.OnMessage(messages);
        receiverContext.ReceiveTimeout = ServiceBusConnection.DefaultReadTimeout;
      }
      catch (ServerBusyException ex)
      {
        receiverContext.OnError((Exception) ex);
        flag = false;
      }
      catch (OperationCanceledException ex)
      {
        TraceSourceExtensions.TraceError(this._trace, "Receiving messages from the service bus threw an OperationCanceledException, most likely due to a closed channel.");
        return false;
      }
      catch (MessagingEntityNotFoundException ex)
      {
        TaskAsyncHelper.Catch<Task>(receiverContext.Receiver.CloseAsync(), (TraceSource) null);
        receiverContext.OnError((Exception) ex);
        TaskAsyncHelper.Then(TaskAsyncHelper.Delay(ServiceBusConnection.RetryDelay), (Action) (() => this.Retry((Action) (() => this.CreateSubscription(receiverContext.ConnectionContext, receiverContext.TopicIndex)))));
        return false;
      }
      catch (Exception ex)
      {
        receiverContext.OnError(ex);
        flag = false;
        timeOut = ServiceBusConnection.ErrorBackOffAmount;
        receiverContext.ReceiveTimeout = ServiceBusConnection.ErrorReadTimeout;
      }
      if (flag)
        return true;
      TaskAsyncHelper.Then<ServiceBusConnection.ReceiverContext>(TaskAsyncHelper.Delay(timeOut), (Action<ServiceBusConnection.ReceiverContext>) (ctx => this.ProcessMessages(ctx)), receiverContext);
      return false;
    }

    private class ReceiverContext
    {
      public const int ReceiveBatchSize = 1000;
      public readonly MessageReceiver Receiver;
      public readonly ServiceBusConnectionContext ConnectionContext;

      public int TopicIndex { get; private set; }

      public TimeSpan ReceiveTimeout { get; set; }

      public ReceiverContext(int topicIndex, MessageReceiver receiver, ServiceBusConnectionContext connectionContext)
      {
        this.TopicIndex = topicIndex;
        this.Receiver = receiver;
        this.ReceiveTimeout = ServiceBusConnection.DefaultReadTimeout;
        this.ConnectionContext = connectionContext;
      }

      public void OnError(Exception ex)
      {
        this.ConnectionContext.ErrorHandler(this.TopicIndex, ex);
      }

      public void OnMessage(IEnumerable<BrokeredMessage> messages)
      {
        this.ConnectionContext.Handler(this.TopicIndex, messages);
      }
    }
  }
}
