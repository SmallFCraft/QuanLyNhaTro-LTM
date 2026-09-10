using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Security;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PasswordHasherTests
{
    [TestMethod]
    public void PasswordHash_VerifiesCorrectPasswordOnly()
    {
        var hash = PasswordHasher.Hash("MatKhau!123");
        Assert.IsTrue(PasswordHasher.Verify("MatKhau!123", hash));
        Assert.IsFalse(PasswordHasher.Verify("sai", hash));
        Assert.AreNotEqual(hash, PasswordHasher.Hash("MatKhau!123"));
    }

    [TestMethod]
    public void PasswordHash_RejectsBlankPassword()
    {
        Assert.ThrowsException<ArgumentException>(() => PasswordHasher.Hash(""));
        Assert.ThrowsException<ArgumentException>(() => PasswordHasher.Hash("   "));
    }

    [TestMethod]
    public void PasswordHash_VerifyHandlesGarbageHash()
    {
        Assert.IsFalse(PasswordHasher.Verify("anything", "not-a-hash"));
        Assert.IsFalse(PasswordHasher.Verify("anything", ""));
    }
}
