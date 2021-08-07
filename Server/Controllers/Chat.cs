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
    [Route("api/group/{gid}/[controller]")]
    public class Chat : Controller
    {
        private readonly IHubContext<Hubs.SignalR> _hubContext;

        public Chat(IHubContext<Hubs.SignalR> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet("{cid}")]
        public IActionResult Info(string gid, string cid)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == gid);
                GroupsContainerObjectified.Chat chat = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group?.Chats ?? "[{}]")?.Find(x => x.Id == cid);
                if (!String.IsNullOrEmpty(chat?.Id))
                {
                    GroupsContainerSuccess.ChatSuccess chatSuccess = new GroupsContainerSuccess.ChatSuccess();
                    chatSuccess.Success = true;
                    chatSuccess.Id = chat.Id;
                    chatSuccess.Name = chat.Name;
                    return Ok(chatSuccess);
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid information provided." }
                    });
                }
            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid token." }
                    });
            }
        }

        [HttpPost]
        public IActionResult Create(string gid, string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            Microsoft.Extensions.Primitives.StringValues namePre;
            Request.Headers.TryGetValue("Name", out namePre);
            string name = namePre.ToString();
            var groups = Program.db.Table<GroupsContainer.Groups>();
            GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == gid);
            if (!String.IsNullOrEmpty(group?.Id))
            {
                if (name.Length < 31 && !String.IsNullOrEmpty(name))
                {
                    GroupsContainerObjectified.Chat chat = new GroupsContainerObjectified.Chat();
                    chat.Id = Program.generator.CreateId().ToString();
                    chat.Name = name;
                    chat.Messages = new List<GroupsContainerObjectified.Message>();

                    List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);

                    chats.Add(chat);

                    group.Chats = JsonConvert.SerializeObject(chats);
                    Program.db.Update(group);

                    GroupsContainerSuccess.ChatSuccess chatSuccess = new GroupsContainerSuccess.ChatSuccess();
                    chatSuccess.Success = true;
                    chatSuccess.Id = chat.Id;
                    chatSuccess.Name = chat.Name;

                    WebsocketObject4Container.Chat websocketObject = new WebsocketObject4Container.Chat();
                    websocketObject.Id = chat.Id;
                    websocketObject.Name = chat.Name;
                    _hubContext.Clients.Group(group.Id).SendAsync("NewChat", websocketObject);

                    return Ok(chatSuccess);
                }
                else
                {
                    return BadRequest(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid information provided." }
                    });
                }
            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid token." }
                    });
            }
        }

        [HttpPatch("{cid}")]
        public IActionResult Edit(string gid, string cid)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            Microsoft.Extensions.Primitives.StringValues namePre;
            Request.Headers.TryGetValue("Name", out namePre);
            string name = namePre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == gid);

                if (user.Id == group.Owner)
                {
                    List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);
                    GroupsContainerObjectified.Chat chat = chats.Find(x => x.Id == cid);
                    if (!String.IsNullOrEmpty(chat?.Id) && !String.IsNullOrEmpty(name) ? name.Length < 31 : false)
                    {
                        chats[chats.FindIndex(x => x.Id == cid)].Name = name;
                        group.Chats = JsonConvert.SerializeObject(chats);
                        Program.db.Update(group);
                        GroupsContainerSuccess.ChatSuccess chatSuccess = new GroupsContainerSuccess.ChatSuccess();
                        chatSuccess.Success = true;
                        chatSuccess.Id = chat.Id;
                        chatSuccess.Name = chat.Name;

                        WebsocketObject4Container.Chat websocketObject = new WebsocketObject4Container.Chat();

                        websocketObject.Id = chat.Id;
                        websocketObject.Name = chat.Name;
                        _hubContext.Clients.Group(group.Id).SendAsync("EditedChat", websocketObject);

                        return Ok(chatSuccess);
                    }
                    else
                    {
                        return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid information provided." }
                    });
                    }
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "You don't own this group." }
                    });
                }

            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid token." }
                    });
            }
        }

        [HttpDelete("{cid}")]
        public IActionResult Delete(string gid, string cid)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == gid);

                if (user.Id == group.Owner)
                {
                    List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);
                    GroupsContainerObjectified.Chat chat = chats.Find(x => x.Id == cid);
                    if (!String.IsNullOrEmpty(chat?.Id))
                    {
                        chats.RemoveAll(x => x.Id == cid);
                        group.Chats = JsonConvert.SerializeObject(chats);
                        Program.db.Update(group);
                        GroupsContainerSuccess.ChatSuccess chatSuccess = new GroupsContainerSuccess.ChatSuccess();
                        chatSuccess.Success = true;
                        chatSuccess.Id = chat.Id;
                        chatSuccess.Name = chat.Name;

                        WebsocketObject5Container.Chat websocketObject = new WebsocketObject5Container.Chat();
                        websocketObject.Id = chat.Id;
                        _hubContext.Clients.Group(group.Id).SendAsync("DeletedChat", websocketObject);

                        return Ok(chatSuccess);
                    }
                    else
                    {
                        return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid information provided." }
                    });
                    }
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "You don't own this group." }
                    });
                }

            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "Invalid token." }
                    });
            }
        }
    }
}
