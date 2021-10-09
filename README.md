# FastAPI.Net
FastAPI is a standalone API server library. Has a very simmilliar structure to asp.net mvc api webapp. With very little third party library dependencies.


# Example
```c#
class Program
{
    static void Main(string[] args)
    {
        var config = new APIConfig();
        config.UseHttps(false);
        var api = new API(config, "localhost", 8080);
        api.Start();
        while (true) ;
    }
}
[Route("/")]
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
    [Route("./a")]
    public void path()
    {
        AuthCheck();
    }
    [Route("./people/{id}")]
    public Person person(int id)
    {
        AuthCheck();
        return new Person(id, "aaa", DateTime.Now);
    }
    [HttpDelete]
    public Person delete(Person p)
    {
        AuthCheck();
        return p;
    }
    public void g()
    {
        AuthCheck();
        Console.WriteLine("base get in controller1");
    }
}
public class Auth : FastAPI.Net.Authentication.AuthenticationIdentity
{
    public Auth(string pass)
    {
        Console.WriteLine($"Password:{pass}");
    }
    public override bool HasMinimalAccess()
    {
        throw new NotImplementedException();
    }
}
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
