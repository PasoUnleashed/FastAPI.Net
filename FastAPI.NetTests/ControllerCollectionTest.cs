using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FastAPI.Net;
using System.Collections.Generic;
using System.Linq;

namespace FastAPI.NetTests
{
    [TestClass]
    public class ControllerCollectionTest
    {
        [TestMethod]
        public void PathsTest()
        {
            List<string> paths = new List<string>()
            {
                "api/ABC/b/get",
                "api/ABC/get",
                "api/ABC/post",
                "api/D/delete",
                "api/ABC/hnak/hina/get"

            };
            ControllerCollection collection = new ControllerCollection(AppDomain.CurrentDomain);
            Assert.AreEqual(paths.Count, collection.Handlers.Count);
            foreach (var i in collection.Handlers)
            {
                Console.WriteLine(i.Path);
                Assert.IsTrue(paths.Any((j) => j == i.Path));
            }
        }
    }

    [Route("/api/ABC")]
    public class ABCController:Controller
    {
        public void A() { }
        [Route("./b")]
        public void B()
        {

        }

        [HttpPost]
        public void C()
        {

        }
        [HttpDelete]
        [Route("/api/D")]
        public void D()
        {

        }
        [Route("./hnak/hina")]
        public void E()
        {

        }
    }

}
