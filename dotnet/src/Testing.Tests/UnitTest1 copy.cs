namespace Datagrove.Testing.Tests;
using Datagrove.Testing.Pepinillo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
    }
}



[TestClass]
[TestCategory("unit")]
public class PepinTest
{
    string rootDir()
    {
        return Directory.GetCurrentDirectory() + "/../../../../../template/pepinillo";
    }
    [TestMethod]
    public async Task TestInit()
    {
        await Pepin.build(rootDir());
    }
}
