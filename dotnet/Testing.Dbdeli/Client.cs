namespace Datagrove.Testing.Dbdeli;
using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
// a reply is :number:json
// an error is $:number:json

public class Rpc
{
    public string? method { get; set; }
    public Int64? id { get; set; }
    public dynamic? @params { get; set; }

    public JsonElement? result { get; set; }
    public RpcError? error { get; set; }

    public Rpc()
    {

    }
    public Rpc(string message, Int64 id, dynamic @params)
    {
        this.method = message;
        this.id = id;
        this.@params = @params;
    }
}
public class RpcError
{
    Int64? code { get; set; }
    string? message { get; set; }
}
public class Reply<T>
{
    public T? result { get; set; }
    public string? error { get; set; }
}
public class Reserve
{
    public string sku { get; set; }
    public string description { get; set; }
    public Reserve(string sku, string description)
    {
        this.sku = sku;
        this.description = description;
    }
}
public class Release
{
    public string sku { get; set; }
    public Int64 Ticket { get; set; }
    public Release(string sku, Int64 tag)
    {
        this.sku = sku;
        this.Ticket = tag;
    }
}

public class Lease : IAsyncDisposable
{
    public DbdeliClient client;
    public Release tag;
    public Lease(DbdeliClient client, Release tag)
    {
        this.client = client;
        this.tag = tag;
    }
    public async ValueTask DisposeAsync()
    {
        Console.Write($"Release {tag}");
        await client.send("release", 42, tag);
        try {
        await client.client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        } catch(Exception){}
    }
}

public class DbdeliClient
{
    public ClientWebSocket client;
    DbdeliClient(ClientWebSocket client)
    {
        this.client = client;
    }
    static public async Task<DbdeliClient> connect(Uri url)
    {

        var cl = new ClientWebSocket();
        await cl.ConnectAsync(url, CancellationToken.None);
        return new DbdeliClient(cl);
    }
    public async ValueTask send(string message, Int64 id, dynamic json)
    {
        var b = JsonSerializer.SerializeToUtf8Bytes(new Rpc(message, id, json));
        var x = System.Text.Encoding.UTF8.GetString(b);
        await client.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public async ValueTask<Rpc> recv()
    {
        ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);

        WebSocketReceiveResult? result = null;
        while (true)
        {
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await client.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);
                var r = JsonSerializer.Deserialize<Rpc>(ms.ToArray());
                if (r != null)
                {
                    return r;
                }
            }
        }
    }


    public async ValueTask<Lease> reserve(string sku, string description)
    {
        await send("reserve", 42, new Reserve(sku, description));
        while (true)
        {
            var r = await recv();
            if (r.method == null && r.id == 42)
            {
                var json = r.result?.GetRawText();
                var ln = JsonSerializer.Deserialize<Int64>(json ?? "0");
                Console.Write($"lease ({json}) => {ln}");
                return new Lease(this, new Release(sku, ln));
            }
        }
    }

}
