using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using PyroNetServer.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq.Indexing;
using TinyClient;

namespace PyroNetServer.Controllers;

[ApiController]
[Route("update")]
public class UpdateController : ControllerBase
{
    public IDocumentStore DocumentStore = DocumentStoreHolder.Store;
    
    [HttpGet]
    public async Task<Stream> Download([FromQuery] string id)
    {
        var session = DocumentStore.OpenAsyncSession();
        var result = await session.Advanced.Attachments.GetAsync($"update-{id}", "Pyro.IO.dll");
        session.Dispose();
        return result.Stream;
    }

    [HttpPost("stats")]
    public async Task RefreshUserStats([FromQuery] string username, [FromQuery] string versionId)
    {
        var session = DocumentStore.OpenAsyncSession();
        if (await session.Advanced.ExistsAsync(username))
        {
            var user = await session.LoadAsync<PreloaderUser>(username);
            user.LastChangedVersion = DateTime.Now;
            user.VersionId = versionId;
        }
        else
        {
            await session.StoreAsync(new PreloaderUser(username, DateTime.Now, versionId), username);
        }

        await session.SaveChangesAsync();
        session.Dispose();
    }
    [HttpPost]
    public async Task Upload([FromQuery] int id)
    {
        var session = DocumentStore.OpenAsyncSession();
        var pmodel = new PyroModel(id);
        var name = "Pyro.IO.dll";
        var bytes = await HttpContext.Request.Body.ReadStreamAsBytes();
        if (await CheckIfUploadIsNewest(id))
        {
            if (!await session.Advanced.ExistsAsync("update-latest"))
            {
                await session.StoreAsync(new PyroModel(-1)
                {
                    Id = "update-latest"
                });
            }
            session.Advanced.Attachments.Store("update-latest", name, new MemoryStream(bytes));

        }

        if (!await session.Advanced.ExistsAsync(pmodel.Id))
        {
            await session.StoreAsync(pmodel, pmodel.Id);
        }
        session.Advanced.Attachments.Store(pmodel.Id, name, new MemoryStream(bytes));
        await session.SaveChangesAsync();
        session.Dispose();
    }
    
    public async Task<bool> CheckIfUploadIsNewest(int id)
    {
        var session = DocumentStore.OpenAsyncSession();
        var query = session.Advanced.AsyncRawQuery<Integer>("from \"PyroModels\" as model select model.Version");
        var documents = await query.ToArrayAsync();
        session.Dispose();
        return documents.All(x => x < id);
    } 
}