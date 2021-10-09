# FastAPI.Net
FastAPI is a standalone API server library. Has a very simmilliar structure to asp.net mvc api webapp. With very little third party library dependencies.

# Install
```
dotnet add package FastAPI.Net --version 1.0.3
```

# Todo
* Minimal Access Check
* Edge cases for parameters
* ResponseObject with status code/message

# Example
```c#
class Program
{
    static void Main(string[] args)
    {
        // Create configuration
        var config = new APIConfig().UseHttps(false);             
        // Create server
        var api = new API(config, "localhost", 8080);
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
    [HttpPut] //http://localhost:8080/api/AddPerson
    [Route("./AddPerson")]
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
}
/// Our authentication identity class
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

// Testing data class
public class Person
{
    public int ID { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public Person() { }
    public Person(int id,string name,DateTime birthday)
    {
        this.ID = id;
        Birthday = birthday;
        Name = name;
    }
}

```


