using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PyroNetServer.Databases;
using PyroNetServerIntermediateLibrary;

namespace PyroNetServer.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    public readonly SessionDatabase Database;
    public readonly Random Random;

    public SessionController(SessionDatabase database, Random random)
    {
        Database = database;
        Random = random;
    }
    [HttpGet("all/{key:int?}")]
    public IEnumerable<Session> GetAllSessions(int key = 0)
    {
        foreach (var session in Database.Sessions.Values)
        {
            if (session.GetTimeSinceUpdate() > TimeSpan.FromMinutes(2))
            {
                Database.Sessions.Remove(session.Name);
            }
            else
            {
                if (key == 420)
                {
                    session.UpdateTimeOnly();
                    yield return session;
                    continue;
                }
                var s = session.Copy();
                s.UpdateTimeOnly();
                s.Password = null;
                yield return s;
            }
        }
    }
    
    [HttpGet("random")]
    public Session CreateRandomSession([FromQuery] string userName)
    {
        var key = Math.Abs((userName.GetHashCode() * Random.NextDouble()).GetHashCode());
        Session session = new Session();
        session.Owner = userName;
        session.Name = new string((key >> 2).ToString().Select(x => (char) (x + 30)).ToArray());
        session.Password = new string((key >> 5).ToString().Select(x => (char) (x + 30)).ToArray());
        session.TimeSinceUpdate = Stopwatch.StartNew();
        
        if (Database.Sessions.TryAdd(session.Name, session))
        {
            return session;
        }

        throw new BadHttpRequestException("Session already exists!");
    }
    
    [HttpGet]
    public Session GetExistingSession([FromQuery] string id, [FromQuery] string password)
    {
        var exists = Database.Sessions.TryGetValue(id, out var session);
        if (!exists)
        {
            throw new BadHttpRequestException("Session does not exist!");
        }
        
        var validPassword = session != null && session.Password == password;
        if (!validPassword)
        {
            throw new BadHttpRequestException("Password mismatch!");
        }

        if (session is null)
        {
            throw new BadHttpRequestException("Bad error, session is null!");
        }
        
        session.Update();
        return session;
    }
    
    [HttpPost("new/{session}")]
    public void PostSession(Session session)
    {
        session.TimeSinceUpdate = Stopwatch.StartNew();
        if (!Database.Sessions.TryAdd(session.Name, session))
        {
            throw new BadHttpRequestException("Session with the same name already exists!");
        }
    }
    
    [HttpPut("update")]
    public void Update([FromQuery] string id, [FromQuery] string password)
    {
        var session = GetExistingSession(id, password);
        session.Update();
    }

    [HttpPut("update/users")]
    public void UpdateUsers([FromQuery] string id, [FromQuery] string password, [FromQuery] string userName)
    {
        var session = GetExistingSession(id, password);
        session.Update();
        if (session.Users.Contains(userName))
        {
            throw new BadHttpRequestException($"Session already contained user '{userName}'!");
        }

        session.Users.Add(userName);
    }
}