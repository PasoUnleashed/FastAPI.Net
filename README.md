# FastAPI.Net
FastAPI is a standalone API server library. Has a very simmilliar structure to asp.net mvc api webapp. With very little third party library dependencies.

# Install
![Nuget](https://img.shields.io/nuget/v/FastAPI.Net)

# Todo
* Minimal Access Check
* Edge cases for parameters
* ~~ResponseObject with status code/message~~
* Debugging and Logging
* Accounting

# Example
```c#
class Program
{
    static void Main(string[] args)
    {
        // Create configuration
        var config = new ServerConfig().UseHttps(false).VerboseExceptions(true);             
        // Create server
        var api = new Server(config, "localhost", 8080);
        // Start the server
        api.Start();
        // Rest of the appliaction.
        while (true) ;
    }
}
[Route("/api")] //http://localhost:8080/api
public class Controller1:Controller
{

    void AuthCheck()
    {
        if (this.Identity != null)
        {
            Console.WriteLine("Found Identity");
        }
        else
        {
            Console.WriteLine("No Identity");
        }
    }
    [HttpPut] //http://localhost:8080/api/AddPerson/1
    [Route("./AddPerson/")]
    public void AddPerson(Person p)
    {
        Console.WriteLine($"Adding person with id {p.ID}");
    }
    [Route("./{id}")] //http://localhost:8080/api/4120
    public Person GetPerson(int id)
    {
        Console.WriteLine($"Returning person with id {id}");
        return new Person(id, "somePerson", DateTime.Now);
    }
    static Random r = new Random();
    [HttpGet]
    [Route("./response")]
    public HttpResponse Random()
    {
        var rg = r.NextDouble();
        if (rg> 0.666)
        {
            throw new Exception("unhandled error");
        }else if (rg > 0.333)
        {
            return HttpResponse.CreateError(500, new Exception("internal error"));
        }
        else
        {
            return HttpResponse.CreateSuccess("Success");
        }
        
    }
}
/// <summary>
/// Our authentication class
/// </summary>
public class Auth : FastAPI.Net.Authentication.AuthenticationIdentity
{
    // The identity will be created if the request header contains the constructor's parameters.
    public Auth(string username)
    {
        Console.WriteLine($"username:{username}");
    }
    // Everyone get's basic access in this implementation
    public override bool HasMinimalAccess()
    {
        return true;
    }
}

```


