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

            bus.Subscribe<string>((context) => { received.Add(context.Payload); });

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
            bus.Subscribe<string>("exact", (ctx) => received.Add("e:" + ctx.Payload));
            // 通配符匹配
            bus.Subscribe<string>("pre*", (ctx) => received.Add("w:" + ctx.Payload), false);
            // 正则匹配
            bus.Subscribe<string>("^r\\d+$", (ctx) => received.Add("r:" + ctx.Payload), true);

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
            bus.Subscribe<string>((ctx) => { seq.Add("first"); ctx.StopSubscription(); });
            // 第二个订阅者保持订阅
            bus.Subscribe<string>((ctx) => { seq.Add("second"); });

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

            var id1 = bus.Subscribe<string>((ctx) => { received += 1; });
            var id2 = bus.Subscribe<string>((ctx) => { received += 10; });

            Assert.IsTrue(bus.Unsubscribe(id1));
            bus.Publish("a");
            Assert.AreEqual(10, received);

            var removed = bus.UnsubscribeAll<string>();
            Assert.AreEqual(1, removed); // id2 removed

            received = 0;
            bus.Publish("b");
            Assert.AreEqual(0, received);
        }

        class BaseMsg { public string Text { get; set; } }
        class DerivedMsg : BaseMsg { }

        [Test]
        public void TypeHierarchy_Matching_BaseClassOrInterface()
        {
            var bus = new DirectBus();
            BaseMsg baseReceived = null;
            DerivedMsg derivedReceived = null;

            // 订阅基类
            bus.Subscribe<BaseMsg>((ctx) => { baseReceived = ctx.Payload; });
            // 订阅派生类
            bus.Subscribe<DerivedMsg>((ctx) => { derivedReceived = ctx.Payload; });

            // 发布派生类，应被两个订阅者匹配到（基类和派生类）
            bus.Publish(new DerivedMsg { Text = "ok" });

            Assert.IsNotNull(baseReceived);
            Assert.IsNotNull(derivedReceived);
            Assert.AreEqual("ok", baseReceived.Text);
            Assert.AreEqual("ok", derivedReceived.Text);
        }

        [Test]
        public void HandlerException_DoesNotPrevent_Others()
        {
            var bus = new DirectBus();
            var called = new List<string>();

            bus.Subscribe<string>((ctx) => throw new InvalidOperationException("boom"));
            bus.Subscribe<string>((ctx) => { called.Add("ok"); });

            // 虽然第一个处理器抛异常，第二个仍应被调用
            var count = bus.Publish("v");
            Assert.AreEqual(1, count);
            CollectionAssert.AreEqual(new[] { "ok" }, called);
        }
    }
}

