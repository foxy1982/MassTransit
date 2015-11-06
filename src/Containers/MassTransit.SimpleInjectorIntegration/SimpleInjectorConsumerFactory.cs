﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.SimpleInjectorIntegration
{
    using System.Threading.Tasks;
    using Pipeline;
    using SimpleInjector;
    using SimpleInjector.Extensions.ExecutionContextScoping;
    using Util;


    public class SimpleInjectorConsumerFactory<TConsumer> :
        IConsumerFactory<TConsumer>
        where TConsumer : class
    {
        readonly Container _container;

        public SimpleInjectorConsumerFactory(Container container)
        {
            _container = container;
        }

        async Task IConsumerFactory<TConsumer>.Send<T>(ConsumeContext<T> context, IPipe<ConsumerConsumeContext<TConsumer, T>> next)
        {
            using (var scope = _container.BeginExecutionContextScope())
            {
                var consumer = _container.GetInstance<TConsumer>();
                if (consumer == null)
                {
                    throw new ConsumerException(
                        string.Format("Unable to resolve consumer type '{0}'.", TypeMetadataCache<TConsumer>.ShortName));
                }

                await next.Send(context.PushConsumer(consumer));
            }
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            context.CreateConsumerFactoryScope<TConsumer>("simpleinjector");
        }
    }
}