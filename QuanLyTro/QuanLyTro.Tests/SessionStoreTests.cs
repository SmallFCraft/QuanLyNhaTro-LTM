using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Security;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class SessionStoreTests
{
    [TestMethod]
    public void SessionStore_CreatesUniqueTokens()
    {
        var store = new SessionStore();
        var t1 = store.Create(1);
        var t2 = store.Create(1);

        Assert.AreNotEqual(t1, t2);
        Assert.IsTrue(store.TryGetUser(t1, out var u1));
        Assert.AreEqual(1, u1);
    }

    [TestMethod]
    public void SessionStore_RejectsInvalidOrRemovedTokens()
    {
        var store = new SessionStore();
        var t = store.Create(42);

        Assert.IsFalse(store.TryGetUser("not-a-token", out _));
        Assert.IsFalse(store.TryGetUser("", out _));

        store.Remove(t);
        Assert.IsFalse(store.TryGetUser(t, out _));
    }
}
