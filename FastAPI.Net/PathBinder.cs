using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FastAPI.Net.PathParsing { 
    /// <summary>
    /// Binder class that binds a string to an object of type T, is capable of collecting arugments from query paths. And returning them in the result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PathBinder<T>
    {
        PathTree<T> tree;
        public PathBinder(IEnumerable<KeyValuePair<string,T>> bindings)
        {
            tree = PathTree<T>.Create(bindings);
        }
        /// <summary>
        /// Query the pathbinder
        /// </summary>
        /// <param name="path">the query path</param>
        /// <param name="res">the result object</param>
        /// <returns>Does the query exist</returns>
        public bool TryGet(string path,out PathBindingResult res)
        {
            var g = tree.Get(path);
            if (g.HasResult)
            {
                res = new PathBindingResult() { Args = g.Args, Result = g.Result };
                return true;
            }
            else
            {
                res = null;
                return false;
            }
        }
        /// <summary>
        /// The result of a path query
        /// </summary>
        public class PathBindingResult
        {
            T result;
            Dictionary<string, string> args;

            /// <summary>
            /// The result of querying this path
            /// </summary>
            public T Result { get => result; set => result = value; }
            /// <summary>
            /// The arguments contained in the query path
            /// </summary>
            public Dictionary<string, string> Args { get => args; set => args = value; }
        }
    }
    
   
    /// <summary>
    /// A fast datastructure to bind paths to datastructures, with the ability to parse path arguments
    /// </summary>
    /// <typeparam name="T"> The type paths are bound to</typeparam>
    internal class PathTree<T>
    {
        // Every root (path ending)
        Dictionary<string, PathTreeNode> roots;
       
        /// <summary>
        /// Create a new tree from a set of path bindings
        /// </summary>
        /// <param name="bindings"> full path or relative paths (relative to controller) bound to a structure</param>
        /// <returns></returns>
        public static PathTree<T> Create(IEnumerable<KeyValuePair<string,T>> bindings)
        {
            var paths = bindings.Aggregate(new List<BoundPathObject>(), (l, i) =>{ return l.Concat(new BoundPathObject[] { new BoundPathObject(i.Key,i.Value) }).ToList(); });
            return new PathTree<T>() { roots = GetRoots(paths) };

        }
        /// <summary>
        /// Construct a tree
        /// </summary>
        /// <param name="paths"> a list of all path objects </param>
        /// <returns>The roots of the tree </returns>
        static Dictionary<string, PathTreeNode> GetRoots(List<BoundPathObject> paths)
        {
            Dictionary<string, PathTreeNode> roots=new Dictionary<string, PathTreeNode>();
            foreach(var i in paths)
            {
                
                if (!roots.ContainsKey(i[i.Length - 1]))
                {
                    roots.Add(i[i.Length-1], GetNodeFor(i[i.Length - 1]));
                    
                }
                AddPathToNode(i, roots[i[i.Length - 1]], i.Length - 2);
            }
            return roots;

            
            
        }
        /// <summary>
        /// Get the node that should be created for this position in teh path
        /// </summary>
        /// <param name="pos">The string value of a path position /`position0`/`position1`/.../`position N-1`</param>
        /// <returns></returns>
        static PathTreeNode GetNodeFor(string pos)
        {
            // pos is arg?
            if (IsArg(pos))
            {
                return(new ArgumentTreeNode());
            }
            else
            {
               return(new StaticTreeNode(pos));
            }
        }
        /// <summary>
        /// Add full path to the appropriate root
        /// </summary>
        /// <param name="o">the path object</param>
        /// <param name="n">the node to add path[index] under</param>
        /// <param name="index">the index of the position to be added to the node n</param>
        static void AddPathToNode(BoundPathObject o,PathTreeNode n,int index)
        {
            
            if (index >= 0)
            {

                var pos = o[index];
                var node = GetNodeFor(o[index]);
                if (node is ArgumentTreeNode)
                {
                    if (!n.Children.Any((i) => i is ArgumentTreeNode))
                    {
                        n.Children.Add(new ArgumentTreeNode());
                        node = n.Children.Last();
                    }
                    else
                    {
                        node = n.Children.First((i) => i is ArgumentTreeNode);
                    }
                }
                else if (!n.Children.Any((i) => i.Val == o[index]))
                {
                    n.Children.Add(node);

                }
                else
                {

                    node = n.Children.First((i) => i.Val == o[index]);
                }
                AddPathToNode(o, node, index - 1);
            } 
            //if full path was added to the root node. append the result to the end of the path
            else if (index == -1)
            {
                // if there's a result associated to this path throw an exception
                if(n.Children.Any((i)=>i is ResultNode))
                {
                    throw new Exception($"Multiple results for 1 path found {(n.Children.First((i)=>i is ResultNode) as ResultNode).Result.Path} and {o.Path}");
                }
                else
                {
                    n.Children.Add(new ResultNode(o));
                }
            }
        }
        /// <summary>
        /// get the binding associated with the path. Priotrizes static paths over args ones. e.g /data/a > /data/{name}
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Path result (containing args collected and the structure bound to this path)</returns>
        internal PathResult Get(string path)
        {
            var obj = new BoundPathObject(path,default(T));
            var whr = roots.Values.Where((i) => i is ArgumentTreeNode).ToList();
            if (roots.TryGetValue(obj[obj.Length-1],out var node))
            {
                
                return Get(obj, node, obj.Length-1, new (string, int)[] { });
                
            }else if (whr.Count() > 0)
            {
                foreach (var i in whr)
                {
                    var r = Get(obj, i, obj.Length - 1, new (string, int)[] { });
                    if (r.HasResult)
                    {
                        return r;
                    }
                }
            }
            return new PathResult();
        }
        /// <summary>
        /// Compare position to a node, if it passes move to the next node. other wise break. if a result node is reached return it.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <param name="i"></param>
        /// <param name="args"></param>
        /// <returns>Path result (containing args collected and the structure bound to this path)</returns>
        PathResult Get(BoundPathObject path,PathTreeNode node,int i,(string,int)[] args)
        {
            
            
            if (i >= 0)
            {
                if ((path[i])!=node.Val&&node is ArgumentTreeNode)
                {
                    
                        args = Enumerable.Concat(args, new (string, int)[] { (path[i], i) }).ToArray();
                        
                    
                }else if (node.Val != path[i])
                {
                    return new PathResult();
                }
                var res = new Dictionary<PathTreeNode, PathResult>();
                foreach(var child in node.Children)
                {
                    var g = Get(path, child, i - 1, args);
                    if (g.HasResult)
                    {
                        res.Add(child, g);
                    }
                }
                if (res.Count > 0)
                {
                    var x = res.ToList();
                    x.Sort((a, b) => ((a.Key is StaticTreeNode ? 0 : 1).CompareTo((b.Key is StaticTreeNode ? 0 : 1))));
                    return x.First().Value;
                }
                return new PathResult();
            }
            else
            {
                if (node is ResultNode f)
                {
                    return new PathResult(args.Aggregate(new Dictionary<string, string>(), (d, ar) => { d.Add(f.Result[ar.Item2],path[ar.Item2]); return d; }),(f as ResultNode).Result.Result);
                }
            }
            return new PathResult();
        }
        /// <summary>
        /// is this position in the path an argument
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        static bool IsArg(string pos)
        {
            return pos.StartsWith("{") && pos.EndsWith("}");
        }
        /// <summary>
        /// A node in a path tree, representing an argument position, a static path position, or a result.
        /// </summary>
        abstract class PathTreeNode
        {

            List<PathTreeNode> children=new List<PathTreeNode>();
            PathTreeNode parent;
            string val;
            public PathTreeNode(string val)
            {
                this.val = val;
            }

            public string Val { get => val; set => val = value; }
            /// <summary>
            /// Children nodes
            /// </summary>
            internal List<PathTreeNode> Children { get => children; set => children = value; }
            internal PathTreeNode Parent { get => parent; set => parent = value; }
        }
        /// <summary>
        /// A position in a path that is an argument eg /{id}
        /// </summary>
        class ArgumentTreeNode : PathTreeNode
        {
            public ArgumentTreeNode() : base("arg")
            {
            }

            
        }

        /// <summary>
        /// a position in the path that is static e.g /employees
        /// </summary>
        class StaticTreeNode : PathTreeNode
        {
            string name;

            public StaticTreeNode(string val) : base(val)
            {
            }

             
        }
        /// <summary>
        /// the object of type T this path is bound to
        /// </summary>
        class ResultNode : PathTreeNode
        {
            BoundPathObject result;

            public ResultNode(BoundPathObject result) : base("result")
            {
                this.result = result;
            }

            public BoundPathObject Result { get => result;}

           
        }
        /// <summary>
        /// 
        /// </summary>
        internal class PathResult
        {
            T result;
            bool hasResult;
            Dictionary<string, string> args;
            public PathResult()
            {

            }
            public PathResult(Dictionary<string, string> args, T result)
            {
                this.result = result;
                this.args = args;
                hasResult = true;
            }
            /// <summary>
            /// structure this path is bound to
            /// </summary>
            public T Result { get => result; }
            /// <summary>
            /// Does this path have a result?
            /// </summary>
            public bool HasResult { get => hasResult; }
            /// <summary>
            /// Arguments collected from path
            /// </summary>
            public Dictionary<string, string> Args { get => args; }
        }
        /// <summary>
        /// An object parsing a path into a more manipulable form
        /// </summary>
        class PathObject
        {
            string path;
            string[] pos;
            public PathObject(string path)
            {
                this.path = path;
                if (this.path == "")
                {
                    this.path = " ";
                }

                path = path.Trim();
                path = path.Trim('/');
                path = path.Replace("//", "/");
                path = path.Replace("\\", "/");
                path = path.TrimEnd('/');
                pos = path.Split('/').Where((i) => i.Trim() != "").ToArray();
                pos = new string[] { " " }.Concat(pos).ToArray();
            }
            /// <summary>
            /// Get a position
            /// </summary>
            /// <param name="p">position</param>
            /// <returns></returns>
            public string this[int p] {
                get => pos[p];
            }
            /// <summary>
            /// How many positions in the path
            /// </summary>
            public int Length { get => pos.Length; }
            public string Path { get => path; set => path = value; }
        }
        /// <summary>
        /// represnts a path schema. containing the bound structure and the raw identifiers
        /// </summary>
        class BoundPathObject : PathObject
        {
            T result;
            public BoundPathObject(string path, T result) : base(path)
            {
                this.result = result;
            }
            /// <summary>
            /// the result this path is bound to
            /// </summary>
            public T Result { get => result; set => result = value; }
        }
    }
   
}
