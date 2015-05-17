// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNet.SignalR.DependencyResolverExtensions
// Assembly: Microsoft.AspNet.SignalR.ServiceBus, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: E5BA2856-2F92-432B-90C3-DC54650F367A
// Assembly location: C:\SignalRChat2 (1)\SignalRChat\packages\Microsoft.AspNet.SignalR.ServiceBus.2.2.0\lib\net45\Microsoft.AspNet.SignalR.ServiceBus.dll

using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.ServiceBus;
using System;

namespace Microsoft.AspNet.SignalR
{
  public static class DependencyResolverExtensions
  {
    /// <summary>
    /// Use Windows Azure Service Bus as the messaging backplane for scaling out of ASP.NET SignalR applications in a web farm.
    /// 
    /// </summary>
    /// <param name="resolver">The dependency resolver.</param><param name="connectionString">The Service Bus connection string to use.</param><param name="topicPrefix">The topic prefix to use. Typically represents the app name. This must be consistent between all nodes in the web farm.</param>
    /// <returns>
    /// The dependency resolver
    /// </returns>
    /// 
    /// <remarks>
    /// Note: Only Windows Azure Service Bus is supported. Service Bus for Windows Server (on-premise) is not supported.
    /// </remarks>
    public static IDependencyResolver UseServiceBus(this IDependencyResolver resolver, string connectionString, string topicPrefix)
    {
      ServiceBusScaleoutConfiguration configuration = new ServiceBusScaleoutConfiguration(connectionString, topicPrefix);
      return DependencyResolverExtensions.UseServiceBus(resolver, configuration);
    }

    /// <summary>
    /// Use Windows Azure Service Bus as the messaging backplane for scaling out of ASP.NET SignalR applications in a web farm.
    /// 
    /// </summary>
    /// <param name="resolver">The dependency resolver.</param><param name="configuration">The Service Bus scale-out configuration options.</param>
    /// <returns>
    /// The dependency resolver
    /// </returns>
    /// 
    /// <remarks>
    /// Note: Only Windows Azure Service Bus is supported. Service Bus for Windows Server (on-premise) is not supported.
    /// </remarks>
    public static IDependencyResolver UseServiceBus(this IDependencyResolver resolver, ServiceBusScaleoutConfiguration configuration)
    {
      ServiceBusMessageBus bus = new ServiceBusMessageBus(resolver, configuration);
      resolver.Register(typeof (IMessageBus), (Func<object>) (() => (object) bus));
      return resolver;
    }
  }
}
