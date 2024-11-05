using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

List<InitialsMessage> initialsList = new List<InitialsMessage>(); // נעדכן את הרשימה לאובייקטים של הודעות
List<WebSocket> webSocketClients = new List<WebSocket>();

var app = builder.Build();

app.UseWebSockets();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/html/initials.html");
    await context.Response.CompleteAsync();
});
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        webSocketClients.Add(webSocket);

        // שלח את רשימת ראשי התיבות הקיימת ללקוח החדש
        var initialMessage = JsonSerializer.Serialize(initialsList);
        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(initialMessage)), WebSocketMessageType.Text, true, CancellationToken.None);

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var data = JsonSerializer.Deserialize<InitialsMessage>(message);

                    // הוסף את השם וראשי התיבות לרשימה אם תקין
                    if (data != null && !string.IsNullOrEmpty(data.Initials) && !string.IsNullOrEmpty(data.Name))
                    {
                        initialsList.Add(data);
                    }

                    // שלח לכל הלקוחות את הרשימה המעודכנת
                    var updatedMessage = JsonSerializer.Serialize(initialsList);
                    foreach (var client in webSocketClients)
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(updatedMessage)), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            webSocketClients.Remove(webSocket);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
});

app.UseStaticFiles();
app.Run();

public class InitialsMessage
{
    public string Initials { get; set; }
    public string Name { get; set; }
}
