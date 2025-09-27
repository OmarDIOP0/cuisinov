using Microsoft.AspNetCore.SignalR;

namespace CantineFront.Hubs
{
    public class ChatCommandHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {


            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
