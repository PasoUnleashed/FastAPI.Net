using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net.Authentication
{
    
    /// <summary>
    /// An authentication identity. When implemented will be filled and associated with the request and present in the controller when the handler is called for any checks to be performed
    /// </summary>
    /// <remarks>
    /// any parameters defiend in the constructor will be retrieved from the url params.
    /// </remarks>
    /// <example>
    /// 
    /// public class USernameAndPassword:AuthenticationIdentity{
    ///     bool isAuth;
    ///     public UsernameAndPassword(string username,string password){
    ///         //do auth
    ///     }
    ///     public bool HasMinimalAccess(){
    ///         return isAuth;
    ///     }
    /// }
    /// </example>
    public abstract class AuthenticationIdentity
    {
        public abstract bool HasMinimalAccess();
                                                                                                                                                                                                                                        
    }



    /// <summary>
    /// This class parses authentication identities from header. It finds defined identities and tries to match data from the request headers to a defined type
    /// </summary>
    public class AuthenticationIdentityFactory
    {
        AppDomain domain;
        List<Type> authTypes;
        /// <summary>
        /// Initializes the IdentityFactory
        /// </summary>
        /// <param name="domain">the domain where the Authentication identity types are defined</param>
        public AuthenticationIdentityFactory(AppDomain domain)
        {
            this.domain = domain;
            this.authTypes = new List<Type>();
            Console.WriteLine("Initializing auth factory");
            foreach (var i in domain.GetAssemblies())
            {
                authTypes = authTypes.Concat(i.GetTypes().Where((j) => typeof(Authentication.AuthenticationIdentity).IsAssignableFrom(j)&&!j.IsAbstract)).ToList();
            }
            foreach(var i in authTypes)
            {
                Console.WriteLine($"Found auth identity {i.Name}");
            }
        }
        /// <summary>
        /// Attempt to create an identity from request headers.
        /// </summary>
        /// <param name="headers">a name/value collection containing the headers of a request</param>
        /// <returns></returns>
        public AuthenticationIdentity GetIdentity(NameValueCollection headers)
        {
            foreach(var i in authTypes)
            {
                foreach(var j in i.GetConstructors())
                {
                    if (j.GetParameters().All((p) => headers.AllKeys.Contains(p.Name))){
                        var vals = new object[j.GetParameters().Count()];
                        var param = j.GetParameters();
                        for (int p = 0; p < vals.Count(); p++)
                        {
                            vals[p] = headers[param[p].Name];
                        }
                        Console.WriteLine("Found auth");
                        return (AuthenticationIdentity)j.Invoke(vals);
                    }
                }
            }
            return null;
        }
    }
    
    
}
