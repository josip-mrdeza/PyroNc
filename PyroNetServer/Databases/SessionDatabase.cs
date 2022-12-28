using PyroNetServerIntermediateLibrary;

namespace PyroNetServer.Databases;

public class SessionDatabase
{
    public Dictionary<string, Session> Sessions = new Dictionary<string, Session>();
}