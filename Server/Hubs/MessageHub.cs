using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    public class MessageHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var users = Program.db.Table<UsersContainer.Users>();
            string token = Context.GetHttpContext().Request.Query["access_token"];
            if (String.IsNullOrEmpty(users.ToList().Find(x => x.Token == token)?.Id))
            {
                Context.Abort();
            }
            await base.OnConnectedAsync();
        }

        public async Task Login(string user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, user);
        }

        public async Task ChangeToGroup(string oldGroup, string newGroup)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldGroup);
            await Groups.AddToGroupAsync(Context.ConnectionId, newGroup);
        }

        public async Task ChangeToChannel(string oldChannel, string newChannel)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldChannel);
            await Groups.AddToGroupAsync(Context.ConnectionId, newChannel);
        }
    }
}