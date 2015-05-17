// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.ServiceBusScaleoutConfiguration
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.ServiceBus;
using System;

namespace Microsoft.AspNet.SignalR
{
  /// <summary>
  /// Settings for the Service Bus scale-out message bus implementation.
  /// 
  /// </summary>
  public class ServiceBusScaleoutConfiguration : ScaleoutConfiguration
  {
    private int _topicCount;

    /// <summary>
    /// The Service Bus connection string to use.
    /// 
    /// </summary>
    public string ConnectionString { get; private set; }

    /// <summary>
    /// The topic prefix to use. Typically represents the app name.
    ///             This must be consistent between all nodes in the web farm.
    /// 
    /// </summary>
    public string TopicPrefix { get; private set; }

    /// <summary>
    /// The number of topics to send messages over. Using more topics reduces contention and may increase throughput.
    ///             This must be consistent between all nodes in the web farm.
    ///             Defaults to 5.
    /// 
    /// </summary>
    public int TopicCount
    {
      get
      {
        return this._topicCount;
      }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value");
        this._topicCount = value;
      }
    }

    /// <summary>
    /// Gets or sets the message’s time to live value. This is the duration after
    ///             which the message expires, starting from when the message is sent to the
    ///             Service Bus. Messages older than their TimeToLive value will expire and no
    ///             longer be retained in the message store. Subscribers will be unable to receive
    ///             expired messages.
    /// 
    /// </summary>
    public TimeSpan TimeToLive { get; set; }

    /// <summary>
    /// Specifies the time duration after which an idle subscription is deleted
    /// 
    /// </summary>
    public TimeSpan IdleSubscriptionTimeout { get; set; }

    /// <summary>
    /// Specifies the delay before we try again after an error
    /// 
    /// </summary>
    public TimeSpan BackoffTime { get; set; }

    /// <summary>
    /// Gets or Sets the operation timeout for all Service Bus operations
    /// 
    /// </summary>
    public TimeSpan? OperationTimeout { get; set; }

    /// <summary>
    /// Gets or Sets the maximum message size (in bytes) that can be sent or received
    ///             Default value is set to 256KB which is the maximum recommended size for Service Bus operations
    /// 
    /// </summary>
    public int MaximumMessageSize { get; set; }

    /// <summary>
    /// Gets or sets the retry policy for service bus
    ///             Default value is RetryExponential.Default
    /// 
    /// </summary>
    public RetryPolicy RetryPolicy { get; set; }

    public ServiceBusScaleoutConfiguration(string connectionString, string topicPrefix)
    {
      if (string.IsNullOrEmpty(connectionString))
        throw new ArgumentNullException("connectionString");
      if (string.IsNullOrEmpty(topicPrefix))
        throw new ArgumentNullException("topicPrefix");
      this.IdleSubscriptionTimeout = TimeSpan.FromHours(1.0);
      this.ConnectionString = connectionString;
      this.TopicPrefix = topicPrefix;
      this.TopicCount = 5;
      this.BackoffTime = TimeSpan.FromSeconds(20.0);
      this.TimeToLive = TimeSpan.FromMinutes(1.0);
      this.MaximumMessageSize = 262144;
      this.OperationTimeout = new TimeSpan?();
    }

    /// <summary>
    /// Returns Service Bus connection string to use.
    /// 
    /// </summary>
    public string BuildConnectionString()
    {
      if (!this.OperationTimeout.HasValue)
        return this.ConnectionString;
      return new ServiceBusConnectionStringBuilder(this.ConnectionString)
      {
        OperationTimeout = this.OperationTimeout.Value
      }.ToString();
    }
  }
}
