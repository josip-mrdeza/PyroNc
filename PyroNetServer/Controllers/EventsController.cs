using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace PyroNetServer.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    public static Dictionary<string, List<Stream>> Writers = new Dictionary<string, List<Stream>>();

    [HttpGet]
    public async Task Id([FromQuery] string id)
    {
        if (!Writers.ContainsKey(id))
        {
            Writers.Add(id, new List<Stream>());
        }
        Writers[id].Add(Response.Body);
        Response.HttpContext.RequestAborted.Register(() =>
        {
            Writers[id].Remove(Response.Body);
        });
        Response.ContentType = "application/octet-stream";
        await Response.BodyWriter.WriteAsync(new byte[]{0,0,0,0,0}, CancellationToken.None);
        await Response.BodyWriter.FlushAsync();
        Response.HttpContext.RequestAborted.WaitHandle.WaitOne();
    }

    [HttpPost("invoke")]
    public async Task InvokeEvent([FromQuery] string id, [FromQuery] string sequence)
    {
        var buffer = Encoding.UTF8.GetBytes(sequence);
        Writers.TryGetValue(id, out var writers);
        var count = writers == null ? 0 : writers.Count(x => x.CanWrite);
        Console.WriteLine($"Writers: {count}");
        await HttpContext.Response.WriteAsync($"Id matches {count} writers.");
        foreach (var pipeWriter in writers)
        {
            try
            {
                var c = HttpContext;
                var req = c.Request;
                var len = req.ContentLength;
                await pipeWriter.WriteAsync(new byte[]{1});
                if (len is not null)
                {
                    await pipeWriter.WriteAsync(BitConverter.GetBytes((int)len));
                    await req.Body.CopyToAsync(pipeWriter!);
                }
                await pipeWriter.WriteAsync(buffer);
                await pipeWriter.FlushAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    [HttpPost("invoke/basic")]
    public async Task InvokeEventBasic([FromQuery] string id, [FromQuery] string sequence)
    {
        var buffer = Encoding.UTF8.GetBytes(sequence);
        var writers = Writers[id];
        Console.WriteLine($"Writers: {writers.Count}");
        foreach (var pipeWriter in writers)
        {
            try
            {
                await pipeWriter.WriteAsync(new byte[]{0});
                await pipeWriter.WriteAsync(buffer);
                await pipeWriter.FlushAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}