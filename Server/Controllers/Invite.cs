using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
    [Route("[controller]")]
    public class Invite : Controller
    {
        private readonly IHubContext<Hubs.SignalR> _hubContext;

        public Invite(IHubContext<Hubs.SignalR> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet("{code}")]
        public IActionResult Info(string code)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var invites = Program.db.Table<InvitesContainer.Invites>();
                InvitesContainer.Invites invite = invites.ToList().Find(x => x.Code == code);
                if (!String.IsNullOrEmpty(invite?.Code)) {
                    InvitesContainerSuccess.Invites inviteSuccess = new InvitesContainerSuccess.Invites();
                    inviteSuccess.Success = true;
                    inviteSuccess.Code = invite.Code;
                    inviteSuccess.Group = invite.Group;
                    return Ok(inviteSuccess);
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 2 },
                        { "error", "Invalid information provided." }
                    });
                }
            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 3 },
                        { "error", "Invalid token." }
                    });
            }
        }

        [HttpPost("join/{code}")]
        public IActionResult Join(string code)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                List<string> userGroups = JsonConvert.DeserializeObject<List<string>>(user.Groups);
                var invites = Program.db.Table<InvitesContainer.Invites>();
                string id = invites.ToList().Find(x => x.Code == code)?.Group;
                if (!String.IsNullOrEmpty(id)) { 
                    if (!userGroups.Contains(id))
                    {
                        userGroups.Add(id);
                        user.Groups = JsonConvert.SerializeObject(userGroups);
                        Program.db.Update(user);
                        var groups = Program.db.Table<GroupsContainer.Groups>();
                        GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == id);
                        List<string> members = JsonConvert.DeserializeObject<List<string>>(group.Members);
                        members.Add(user.Id);
                        group.Members = JsonConvert.SerializeObject(members);
                        Program.db.Update(group);

                        GroupsContainerSuccess.Groups groupSuccess = new GroupsContainerSuccess.Groups();
                        groupSuccess.Success = true;
                        groupSuccess.Id = group.Id;
                        groupSuccess.Name = group.Name;
                        groupSuccess.Owner = group.Owner;
                        List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);
                        List<GroupsContainerSuccess.Chat> chats2 = new List<GroupsContainerSuccess.Chat>();
                        chats.ForEach(x => {
                            GroupsContainerSuccess.Chat chat = new GroupsContainerSuccess.Chat();
                            chat.Id = x.Id;
                            chat.Name = x.Name;
                            chats2.Add(chat);
                        });
                        groupSuccess.Chats = chats2;
                        groupSuccess.Members = JsonConvert.DeserializeObject<List<string>>(group.Members);

                        WebsocketObject6Container.Groups websocketObject = new WebsocketObject6Container.Groups();
                        websocketObject.Id = group.Id;
                        websocketObject.Name = group.Name;
                        _hubContext.Clients.Group(user.Id).SendAsync("JoinedGroup", websocketObject);

                        return Ok(groupSuccess);
                    }
                    else
                    {
                        return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "You already are in this group." }
                    });
                    }
            } else
            {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 2 },
                        { "error", "Invalid information provided." }
                    });
                }
            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 3 },
                        { "error", "Invalid token." }
                    });
            }
        }
    }
}
