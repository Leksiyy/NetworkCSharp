namespace ASP;

public class Program
{
    public static void Main(string[] args)
    {
        int id = 1;


        List<Person> users = new List<Person>
        {
            new() { Id = id++, Name = "Tom", Age = 37 },
            new() { Id = id++, Name = "Bob", Age = 41 },
            new() { Id = id++, Name = "Sam", Age = 24 }
        };


        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();


        app.MapGet("/api/users", () => users);


        app.MapGet("/api/users/{id}", (int id) =>
        {
            Person? user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Results.NotFound(new { message = "Пользователь не найден" });
            }

            return Results.Json(user);
        });


        app.MapDelete("/api/users/{id}", (int id) =>
        {
            Person? user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Results.NotFound(new { message = "Пользователь не найден" });
            }


            users.Remove(user);
            return Results.Json(user);
        });


        app.MapPost("/api/users", (Person user) =>
        {
            user.Id = id++;
            users.Add(user);
            return user;
        });


        app.MapPut("/api/users", (Person userData) =>
        {
            var user = users.FirstOrDefault(u => u.Id == userData.Id);
            if (user == null)
            {
                return Results.NotFound(new { message = "Пользователь не найден" });
            }


            user.Age = userData.Age;
            user.Name = userData.Name;
            return Results.Json(user);
        });


        app.Run();
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }
    
}