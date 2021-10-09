using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FastAPI.NetTests
{
    [TestClass]
    public class PathBinderTests
    {
        [TestMethod]
        public void BasicPathingTest()
        {
            Dictionary<string, int> binding = new Dictionary<string, int>();
            binding.Add("/a/b/a", 1);
            binding.Add("/a/b/b", 2);
            binding.Add("/a/b/c", 3);
            binding.Add("/a/{id}/c", 4);
            binding.Add("/a/a", 5);
            binding.Add("/", 6);
            var tree = new FastAPI.Net.PathParsing.PathBinder<int>(binding);
            
            foreach(var i in binding)
            {
                Console.WriteLine($"Testing {i.Key}");
                Assert.IsTrue(tree.TryGet(i.Key, out var x));
               
                Assert.AreEqual(i.Value, x.Result);
                Console.WriteLine($"True {x.Result}");
                
            }

        }
        [TestMethod]
        public void ArgumentTest()
        {
            Random r = new Random();
            Dictionary<string, int> binding = new Dictionary<string, int>();
            binding.Add("/a/{id}/a", 1);
            binding.Add("/c/{id}/a", 2);
            binding.Add("/a/{id}/c", 3);
            binding.Add("/a/{id}/{name}", 4);
            binding.Add("/a/{id}/{name}/delete", 5);
            binding.Add("/a/{name}/delete/{id}", 6);
            //binding.Add("/a", 6);

            //binding.Add("/{id}", 7);


            var tree = new FastAPI.Net.PathParsing.PathBinder<int>(binding);
            var names = new string[] { "ab", "cd", "ed", "sa", "ds" };
            foreach (var i in binding)
            {

                Console.WriteLine($"Testing {i.Key}");
                string name = names[r.Next(names.Length)];
                string id = r.Next(100).ToString();
                Assert.IsTrue(tree.TryGet(i.Key.Replace("{id}", id).Replace("{name}", name), out var x));
                
                Assert.AreEqual(i.Value, x.Result);
                if (x.Args.ContainsKey("{name}"))
                {
                    Assert.AreEqual(name, x.Args["{name}"]);
                }
                if (i.Key.Contains("{id}"))
                {
                    Assert.AreEqual(id, x.Args["{id}"]);
                }
                Console.WriteLine($"True {x.Result}");
                foreach(var a in x.Args)
                {
                    Console.WriteLine($"{a.Key}:{a.Value}");
                }
            }

        }
    }
}
