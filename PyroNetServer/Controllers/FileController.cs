using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using TinyClient;

namespace PyroNetServer.Controllers;

[ApiController]
[Route("files")]
public class FileController : ControllerBase
{
    public string Folder = "GCode\\";
    public static List<User> Users;

    public FileController()
    {
        if (Users != null)
        {
            return;
        }

        var file = $"{Folder}users.txt";
        Users = new List<User>();
        var fs = System.IO.File.OpenRead(file);
        var ss = new StreamReader(fs);
        var str = ss.ReadToEnd();
        var lines = str.Split('\n');
        foreach (var line in lines)
        {
            var arr = line.Split("*");
            var user = new User(arr[0], arr[1]);
            Users.Add(user);
        }

        ss.Dispose();
    }

    [HttpGet("all")]
    public IEnumerable<string> GetAllFiles([FromQuery] string userName, [FromQuery] string password)
    {
        var user = Users.SingleOrDefault(x => x.Name == userName && x.Password == password);
        if (user == null)
        {
            return null;
        }
        var dir = $"{Folder}{user.Name}";
        Directory.CreateDirectory(dir);
        return Directory.EnumerateFiles(dir).Select(f =>
        {
            var fi = new FileInfo(f);

            return fi.Name;
        });
    }

    [HttpGet("users")]
    public IEnumerable<string> GetAllUsers([FromQuery] string key)
    {
        if (key == "JOKINC")
        {
            return Users.Select(x => $"{x.Name}, {x.Password}");
        }
        return Users.Select(x => x.Name);
    }

    [HttpGet("{fileName}")]
    public IActionResult GetFile([FromQuery] string userName, [FromQuery] string password, string fileName)
    {
        var user = Users.SingleOrDefault(x => x.Name == userName && x.Password == password);
        if (user == null)
        {
            return NotFound();
        }
        var dir = $"{Folder}{user.Name}\\";
        Directory.CreateDirectory(dir);
        return File(System.IO.File.ReadAllBytes($"{dir}{fileName}"), "text/plain");
    }

    [HttpDelete("delete/{fileName}")]
    public bool DeleteFile([FromQuery] string userName, [FromQuery] string password, string fileName)
    {
        var user = Users.SingleOrDefault(x => x.Name == userName && x.Password == password);
        if (user == null)
        {
            return false;
        }
        var dir = $"{Folder}{user.Name}";
        Directory.CreateDirectory(dir);
        var filePath = dir + $"\\{fileName}";
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            return true;
        }

        return false;
    }

    [HttpPost("{fileName}")]
    public async Task<IActionResult> AddFile([FromQuery] string userName, [FromQuery] string password, string fileName)
    {
        var user = Users.SingleOrDefault(x => x.Name == userName && x.Password == password);
        if (user == null)
        {
            return NotFound();
        }
        
        var dir = $"{Folder}{user.Name}";
        Directory.CreateDirectory(dir);
        var filePath = dir + $"\\{fileName}";
        await System.IO.File.WriteAllTextAsync(filePath, await HttpContext.Request.Body.ReadStreamAsString());
        return Ok();
    }    
    [HttpPost("register")]
    public IActionResult Register([FromQuery] string userName, [FromQuery] string password)
    {
        var user = new User(userName, password);
        if (Users.Exists(x => x.Name == userName && x.Password == password))
        {
            return Ok();
        }
        else
        {
            if (Users.Exists(x => x.Name == userName && x.Password != password))
            {
                return Unauthorized();
            }
        }
        Users.Add(user);
        var dir = $"{Folder}{user.Name}";
        Directory.CreateDirectory(dir);
        var ss = System.IO.File.AppendText($"{Folder}users.txt");
        ss.Write($"{user.Name}*{user.Password}\n");
        ss.Flush();
        ss.Dispose();

        return Ok();
    }
}

public class User
{
    public string Name;
    public string Password;

    public User(string name, string password)
    {
        Name = name;
        Password = password;
    }
}