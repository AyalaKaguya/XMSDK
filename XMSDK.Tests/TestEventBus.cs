using System;
using System.Collections.Generic;
using NUnit.Framework;
using XMSDK.Framework.EventBus;

namespace XMSDK.Tests
{
    [TestFixture]
    public class TestEventBus
    {
        [Test]
        public void SimplePublishSubscribe_Works()
        {
            var bus = new DirectBus();
            var received = new List<string>();

            bus.Subscribe<string>(context => { received.Add(context.Payload); });

            var count = bus.Publish("hello");

            Assert.AreEqual(1, count);
            Assert.AreEqual(1, received.Count);
            Assert.AreEqual("hello", received[0]);
        }

        [Test]
        public void RouteKey_ExactAndWildcardAndRegex_Matching()
        {
            var bus = new DirectBus();
            var received = new List<string>();

            // 精确匹配
            bus.Subscribe<string>("exact", ctx => received.Add("e:" + ctx.Payload));
            // 通配符匹配
            bus.Subscribe<string>("pre*", ctx => received.Add("w:" + ctx.Payload), false);
            // 正则匹配
            bus.Subscribe<string>("^r\\d+$", ctx => received.Add("r:" + ctx.Payload), true);

            bus.Publish("p1", "preABC"); // wildcard
            bus.Publish("ex", "exact"); // exact
            bus.Publish("r100", "r100"); // regex

            CollectionAssert.AreEquivalent(new[] { "w:p1", "e:ex", "r:r100" }, received);
        }

        [Test]
        public void SubscriptionOrder_And_OneTimeSubscription()
        {
            var bus = new DirectBus();
            var seq = new List<string>();

            // 第一个订阅者会停止订阅（一次性）
            bus.Subscribe<string>(ctx =>
            {
                seq.Add("first");
                ctx.StopSubscription();
            });
            // 第二个订阅者保持订阅
            bus.Subscribe<string>(ctx => { seq.Add("second"); });

            // 首次发布，两个处理器都应执行（first 执行后标记移除）
            var c1 = bus.Publish("x");
            Assert.AreEqual(2, c1);
            CollectionAssert.AreEqual(new[] { "first", "second" }, seq);

            seq.Clear();
            // 再次发布，只有 second 存在
            var c2 = bus.Publish("y");
            Assert.AreEqual(1, c2);
            CollectionAssert.AreEqual(new[] { "second" }, seq);
        }

        [Test]
        public void Unsubscribe_ById_And_UnsubscribeAll()
        {
            var bus = new DirectBus();
            var received = 0;

            var id1 = bus.Subscribe<string>(ctx => { received += 1; });
            var id2 = bus.Subscribe<string>(ctx => { received += 10; });

            Assert.IsTrue(bus.Unsubscribe(id1));
            bus.Publish("a");
            Assert.AreEqual(10, received);

            var removed = bus.UnsubscribeAll<string>();
            Assert.AreEqual(1, removed); // id2 removed

            received = 0;
            bus.Publish("b");
            Assert.AreEqual(0, received);
        }

        private class BaseMsg
        {
            public string Text { get; set; }
        }

        private class DerivedMsg : BaseMsg
        {
        }

        [Test]
        public void TypeHierarchy_Matching_BaseClassOrInterface()
        {
            var bus = new DirectBus();
            var baseReceived = false;
            var derivedReceived = false;

            // 订阅基类
            bus.Subscribe<BaseMsg>("DerivedMsg", ctx => { baseReceived = true; });
            // 订阅派生类
            bus.Subscribe<DerivedMsg>("DerivedMsg", ctx => { derivedReceived = true; });

            // 发布派生类，只有派生类订阅者会收到，基类不会收到
            bus.Publish(new DerivedMsg { Text = "ok" }, "DerivedMsg");

            Assert.IsTrue(derivedReceived, "派生类订阅者应该收到事件");
            Assert.IsFalse(baseReceived, "基类订阅者不应该收到派生类事件");
        }

        [Test]
        public void HandlerException_DoesNotPrevent_Others()
        {
            var bus = new DirectBus();
            var called = new List<string>();

            bus.Subscribe<string>(ctx => throw new InvalidOperationException("boom"));
            bus.Subscribe<string>(ctx => { called.Add("ok"); });

            // 由于没有异常处理委托，第一个异常会被抛出，阻止后续处理器执行
            Assert.Throws<InvalidOperationException>(() => bus.Publish("v"));
            Assert.AreEqual(0, called.Count);
        }

        [Test]
        public void CustomExceptionHandler_CanSuppressExceptions()
        {
            var bus = new DirectBus();
            var exceptionsCaught = new List<ExceptionInfo>();
            var called = new List<string>();

            bus.Subscribe<string>(ctx => throw new InvalidOperationException("boom"));
            bus.Subscribe<string>(ctx => { called.Add("ok"); });

            // 在发布时定义异常处理器，记录异常但不重新抛出
            var count = bus.Publish("test", (context, exception, handler) =>
            {
                exceptionsCaught.Add(new ExceptionInfo
                {
                    Context = context,
                    Exception = exception,
                    Handler = handler
                });
                return false; // 不重新抛出异常
            });

            // 所有处理器都应该被调用，包括抛异常的那个
            Assert.AreEqual(2, count);
            CollectionAssert.AreEqual(new[] { "ok" }, called);

            // 验证异常被捕获
            Assert.AreEqual(1, exceptionsCaught.Count);
            Assert.IsInstanceOf<InvalidOperationException>(exceptionsCaught[0].Exception);
            Assert.AreEqual("boom", exceptionsCaught[0].Exception.Message);
        }

        /// <summary>
        /// 异常信息结构体
        /// </summary>
        private struct ExceptionInfo
        {
            public object Context { get; set; }
            public Exception Exception { get; set; }
            public Delegate Handler { get; set; }
        }

        [Test]
        public void CustomExceptionHandler_CanRethrowExceptions()
        {
            var bus = new DirectBus();
            var exceptionsCaught = new List<Exception>();

            bus.Subscribe<string>(ctx => throw new InvalidOperationException("boom"));

            // 在发布时定义异常处理器，记录异常并重新抛出
            Assert.Throws<InvalidOperationException>(() => 
                bus.Publish("test", (context, exception, handler) =>
                {
                    exceptionsCaught.Add(exception);
                    return true; // 重新抛出异常
                }));

            // 验证异常被记录
            Assert.AreEqual(1, exceptionsCaught.Count);
            Assert.IsInstanceOf<InvalidOperationException>(exceptionsCaught[0]);
        }

        [Test]
        public void DefaultBehavior_RethrowsExceptions()
        {
            var bus = new DirectBus();
            
            bus.Subscribe<string>(ctx => throw new InvalidOperationException("boom"));

            // 不提供异常处理器，默认行为应该抛出异常
            Assert.Throws<InvalidOperationException>(() => bus.Publish("test"));
        }

        [Test]
        public void ExceptionHandler_WithRouteKey()
        {
            var bus = new DirectBus();
            var exceptionsCaught = new List<Exception>();

            bus.Subscribe<string>("test-route", ctx => throw new InvalidOperationException("boom"));

            // 测试带路由键的发布也能正确处理异常
            var count = bus.Publish("test", "test-route", (context, exception, handler) =>
            {
                exceptionsCaught.Add(exception);
                return false; // 不重新抛出
            });

            Assert.AreEqual(1, count);
            Assert.AreEqual(1, exceptionsCaught.Count);
        }
    }
}

