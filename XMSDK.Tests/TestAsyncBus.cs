using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using XMSDK.Framework.EventBus;

namespace XMSDK.Tests
{
    [TestFixture]
    public class TestAsyncBus
    {
        private static async Task<T> WaitWithTimeout<T>(Task<T> task, int timeoutMs = 2000)
        {
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            if (completed != task) throw new TimeoutException("Task timed out");
            return await task;
        }

        private static async Task WaitWithTimeout(Task task, int timeoutMs = 2000)
        {
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
            if (completed != task) throw new TimeoutException("Task timed out");
            await task;
        }

        [Test]
        public async Task TestSimplePublishAsync()
        {
            using (var bus = new AsyncBus())
            {
                var tcs = new TaskCompletionSource<string>();

                bus.SubscribeAsync<string>("string", async context =>
                {
                    tcs.TrySetResult(context.Payload);
                    await Task.CompletedTask;
                });

                var handled = await bus.PublishAsync("Hello EventBus", "string");
                Assert.AreEqual(1, handled, "应当有一个处理器被调用");

                var result = await WaitWithTimeout(tcs.Task);
                Assert.AreEqual("Hello EventBus", result);
            }
        }

        [Test]
        public async Task TestWildcardAndRegexMatching()
        {
            using (var bus = new AsyncBus())
            {
                var tcsWildcard = new TaskCompletionSource<bool>();
                var tcsRegex = new TaskCompletionSource<bool>();

                bus.SubscribeAsync<string>("str*", async ctx =>
                {
                    tcsWildcard.TrySetResult(true);
                    await Task.CompletedTask;
                });

                bus.SubscribeAsync<string>("^str\\w+$", async ctx =>
                {
                    tcsRegex.TrySetResult(true);
                    await Task.CompletedTask;
                }, true);

                var handled = await bus.PublishAsync("payload", "string");
                // 两个订阅都会匹配
                Assert.AreEqual(2, handled);

                Assert.IsTrue(await WaitWithTimeout(tcsWildcard.Task));
                Assert.IsTrue(await WaitWithTimeout(tcsRegex.Task));
            }
        }

        [Test]
        public async Task TestHandlerOrder()
        {
            using (var bus = new AsyncBus())
            {
                var order = new List<int>();

                bus.SubscribeAsync<string>(async ctx =>
                {
                    order.Add(1);
                    await Task.Delay(1);
                });
                bus.SubscribeAsync<string>(async ctx =>
                {
                    order.Add(2);
                    await Task.Delay(1);
                });
                bus.SubscribeAsync<string>(async ctx =>
                {
                    order.Add(3);
                    await Task.Delay(1);
                });

                var handled = await bus.PublishAsync("o");
                Assert.AreEqual(3, handled);

                // 异步总线不会按照顺序执行，而且所有的一起执行
                // Assert.AreEqual(new List<int> { 1, 2, 3 }, order);
            }
        }

        [Test]
        public async Task TestUnsubscribe()
        {
            using (var bus = new AsyncBus())
            {
                var called = false;
                var id = bus.SubscribeAsync<string>(async ctx =>
                {
                    called = true;
                    await Task.CompletedTask;
                });

                var unsubResult = bus.Unsubscribe(id);
                Assert.IsTrue(unsubResult);

                var handled = await bus.PublishAsync("x", "");
                Assert.AreEqual(0, handled);
                Assert.IsFalse(called);
            }
        }

        // 测试多态：基类订阅与派生类订阅
        private class BasePayload { public string Txt { get; set; } }
        private class DerivedPayload : BasePayload { public int Value { get; set; } }

        [Test]
        public async Task TestPolymorphicDispatch()
        {
            using (var bus = new AsyncBus())
            {
                var baseTcs = new TaskCompletionSource<BasePayload>();
                var derivedTcs = new TaskCompletionSource<DerivedPayload>();

                bus.SubscribeAsync<BasePayload>(async ctx =>
                {
                    baseTcs.TrySetResult(ctx.Payload);
                    await Task.CompletedTask;
                });
                bus.SubscribeAsync<DerivedPayload>(async ctx =>
                {
                    derivedTcs.TrySetResult(ctx.Payload);
                    await Task.CompletedTask;
                });

                var payload = new DerivedPayload { Txt = "hi", Value = 42 };
                var handled = await bus.PublishAsync(payload);

                // 现在只会调用 Derived 的处理器，不会调用 Base 的处理器（精确类型匹配）
                Assert.AreEqual(1, handled);

                // 只有派生类订阅会被触发
                var derivedResult = await WaitWithTimeout(derivedTcs.Task);
                Assert.AreEqual(42, derivedResult.Value);
                Assert.AreEqual("hi", derivedResult.Txt);

                // 基类订阅不会被触发
                Assert.IsFalse(baseTcs.Task.IsCompleted);
            }
        }
    }
}
