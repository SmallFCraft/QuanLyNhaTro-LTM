using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public class SanityTests
{
    [TestMethod]
    public void Test_DefaultPort()
    {
        Assert.AreEqual(8888, Shared.SharedConstants.DefaultPort);
    }
}
