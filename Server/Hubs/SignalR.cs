using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    public class SignalR : Hub
    {
        public static Dictionary<string, string> usersDictionary = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var users = Program.db.Table<UsersContainer.Users>();
            string token = Context.GetHttpContext().Request.Query["access_token"];
            if (String.IsNullOrEmpty(users.ToList().Find(x => x.Token == token)?.Id))
            {
                Context.Abort();
            }
            else
            {
                usersDictionary[token] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, users.ToList().Find(x => x.Token == token).Id);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (usersDictionary.ContainsKey(Context.GetHttpContext().Request.Query["access_token"].ToString() ?? ""))
            {
                usersDictionary.Remove(Context.GetHttpContext().Request.Query["access_token"]);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ChangeToGroup(string oldGroup, string newGroup)
        {
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == Context.GetHttpContext().Request.Query["access_token"]);
            if (!String.IsNullOrEmpty(oldGroup))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldGroup);
            }
            if (user.Groups.Contains(newGroup))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, newGroup);
            }
        }

        public async Task ChangeToChat(string actualGroup, string oldChat, string newChat)
        {
            if(!String.IsNullOrEmpty(oldChat))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldChat);
            }
            var groups = Program.db.Table<GroupsContainer.Groups>();
            GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == actualGroup);
            if (group.Chats.Contains(newChat))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, newChat);
            }
        }
    }
}