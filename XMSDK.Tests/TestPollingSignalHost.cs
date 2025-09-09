using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using XMSDK.Framework.Communication;
using XMSDK.Framework.Communication.Signal;

namespace XMSDK.Tests;

internal sealed class TestHost : PollingSignalHost
{
    public readonly List<(string addr, object oldV, object newV)> Changes = new();

    public void RegisterAll()
    {
        RegisterBool("b1", _ => Task.FromResult(false));
        RegisterByte("by1", _ => Task.FromResult((byte)0));
        RegisterShort("s1", _ => Task.FromResult((short)0));
        RegisterInt("i1", _ => Task.FromResult(0));
        RegisterLong("l1", _ => Task.FromResult(0L));
        RegisterFloat("f1", _ => Task.FromResult(0f));
        RegisterDouble("d1", _ => Task.FromResult(0d));
    }

    protected override void OnValueChanged(string address, object oldValue, object newValue)
    {
        lock (Changes)
        {
            Changes.Add((address, oldValue, newValue));
        }
    }
}

[TestFixture]
public class TestPollingSignalHost
{
    [Test]
    public async Task SetAndCache_Works_For_All_Types_V2()
    {
        var host = new TestHost();
        host.RegisterAll();

        // bool
        Assert.IsFalse(host.TryGetBool("b1", out var b));
        var ok = await host.SetBoolAsync("b1", true);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetBool("b1", out b) && b);

        // byte
        Assert.IsFalse(host.TryGetByte("by1", out var byv));
        ok = await host.SetByteAsync("by1", 7);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetByte("by1", out byv) && byv == 7);

        // short
        Assert.IsFalse(host.TryGetShort("s1", out var sv));
        ok = await host.SetShortAsync("s1", 12);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetShort("s1", out sv) && sv == 12);

        // int
        Assert.IsFalse(host.TryGetInt("i1", out var iv));
        ok = await host.SetIntAsync("i1", 34);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetInt("i1", out iv) && iv == 34);

        // long
        Assert.IsFalse(host.TryGetLong("l1", out var lv));
        ok = await host.SetLongAsync("l1", 56L);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetLong("l1", out lv) && lv == 56L);

        // float
        Assert.IsFalse(host.TryGetFloat("f1", out var fv));
        ok = await host.SetFloatAsync("f1", 1.5f);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetFloat("f1", out fv) && Math.Abs(fv - 1.5f) < 1e-6);

        // double
        Assert.IsFalse(host.TryGetDouble("d1", out var dv));
        ok = await host.SetDoubleAsync("d1", 2.5d);
        Assert.IsTrue(ok);
        Assert.IsTrue(host.TryGetDouble("d1", out dv) && Math.Abs(dv - 2.5d) < 1e-9);

        // events
        Assert.That(host.Changes.Count, Is.GreaterThanOrEqualTo(7));
    }

    [Test]
    public void TypeMismatch_Throws_V2()
    {
        var host = new TestHost();
        host.RegisterAll();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await host.SetIntAsync("s1", 1));
    }
}
