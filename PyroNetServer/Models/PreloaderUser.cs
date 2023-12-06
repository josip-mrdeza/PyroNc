namespace PyroNetServer.Models;

public class PreloaderUser
{
    public string Username { get; set; }
    public DateTime LastChangedVersion { get; set; }
    public string VersionId { get; set; }

    public PreloaderUser(string username, DateTime lastChangedVersion, string versionId)
    {
        Username = username;
        LastChangedVersion = lastChangedVersion;
        VersionId = versionId;
    }
}