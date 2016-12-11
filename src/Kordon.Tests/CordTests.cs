using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kordon.Tests
{
    public class CordTests
    {
        [Fact]
        public void Build()
        {
            var cord = new Cord<string>();
        }

        [Fact]
        public void CallingRaiseWithoutRegisterationDoesNotThrow()
        {
            var cord = new Cord<string>();

            cord.Raise("Hello World!");
        }

        [Fact]
        public void CallingRegisterDoesNotThrow()
        {
            var cord = new Cord<string>();
            var handler = new Action<string>((a) => { });

            cord.Register(handler);
        }

        [Fact]
        public void CallingRegisterMultipleTimesDoesNotThrow()
        {
            var cord = new Cord<string>();
            var handler = new Action<string>(a => { });

            cord.Register(handler);
            cord.Register(handler);
        }

        [Fact]
        public void CallingUnregisterWithoutRegistrationDoesNotThrow()
        {
            var cord = new Cord<string>();
            var handler = new Action<string>(a => { });

            cord.Unregister(handler);
        }

        [Fact]
        public void CallingUnregisterWithNullDoesNotThrow()
        {
            var cord = new Cord<string>();

            cord.Unregister(null);
        }

        [Fact]
        public void CallingRegisterWithNullDoesNotThrow()
        {
            var cord = new Cord<string>();

            cord.Register(null);
        }

        [Fact]
        public void CallingRaiseAfterRegisteringNullHandlerDoesNotThrow()
        {
            var cord = new Cord<string>();

            cord.Register(null);
            cord.Raise("test");
        }

        [Fact]
        public void CallingRaiseDoesNotBlock()
        {
            var executed = false;
            var delay = TimeSpan.FromMilliseconds(100);
            var cord = new Cord<string>();
            var handler = new Action<string>(a =>
            {
                Thread.Sleep(delay);
                executed = true;
            });
            cord.Register(handler);

            cord.Raise("Helo World!");
            Assert.False(executed);
        }

        [Fact]
        public void HandlerIsExecutedSerially()
        {
            var arguments = new List<int>();
            for (var i = 0; i < 100; i++)
            {
                arguments.Add(i);
            }

            var handlerIndex = 0;
            var match = true;
            var cord = new Cord<int>();

            var handler = new Action<int>(a =>
            {
                if (a != arguments[handlerIndex])
                {
                    match = false;
                }

                handlerIndex++;
            });

            cord.Register(handler);

            foreach (var arg in arguments)
            {
                cord.Raise(arg);
            }

            Task.Delay(100).Wait();

            Assert.Equal(arguments.Count, handlerIndex);
            Assert.True(match);
        }

        [Fact]
        public void AllHandlersAreExecutedAsync()
        {
            var executionCount = 0;
            var executionSync = new object();
            var delay = TimeSpan.FromMilliseconds(100);
            var cord = new Cord<string>();
            var handler1 = new Action<string>(a =>
             {
                 lock (executionSync)
                 {
                     executionCount++;
                 }

                 Task.Delay(delay).Wait();
             });

            var handler2 = new Action<string>(a =>
             {
                 lock (executionSync)
                 {
                     executionCount++;
                 }

                 Task.Delay((int)delay.TotalMilliseconds * 2).Wait();
             });

            var handler3 = new Action<string>(a =>
             {
                 lock (executionSync)
                 {
                     executionCount++;
                 }

                 Task.Delay((int)delay.TotalMilliseconds * 3).Wait();
             });

            cord.Register(handler1);
            cord.Register(handler2);
            cord.Register(handler3);

            cord.Raise("test");
            Task.Delay((int)delay.TotalMilliseconds * 4).Wait();

            Assert.Equal(3, executionCount);
        }

        [Fact]
        public void RaiseMultipleTimesCallsHandlersInOrder()
        {
            throw new NotImplementedException();
        }
    }
}
