using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryFirstTests;

namespace QueryFirst.QueryObj.Helper.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var ctx = new CodeGenerationContext(new ConfigResolver(new FakeConfigFileReader()),
                new AdoSchemaFetcher());
            var maker = new WrapperClassMaker()
            {
               CodeGenerationContext = ctx
            };
            var code = maker.TransformText();
        }
    }
}
