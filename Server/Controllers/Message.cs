﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
    [Route("group/{gid}/chat/{cid}/[controller]")]
    public class Message : Controller
    {
        private readonly IHubContext<Hubs.SignalR> _hubContext;

        public Message(IHubContext<Hubs.SignalR> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet("{mid?}")]
        public IActionResult Info(string gid, string cid, string mid)
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
                if(!String.IsNullOrEmpty(chat?.Id)) { 
                if (String.IsNullOrEmpty(mid))
                {
                    GroupsContainerSuccess.MessagesSuccess messagesSuccess = new GroupsContainerSuccess.MessagesSuccess();
                    messagesSuccess.Success = true;
                        List<GroupsContainerObjectified.Message2> realMessages = new List <GroupsContainerObjectified.Message2>();
                        chat.Messages.ForEach(message => {
                            GroupsContainerObjectified.Message2 realMessage = new GroupsContainerObjectified.Message2();
                            realMessage.Id = message.Id;
                            realMessage.Content = message.Content;
                            realMessage.Author = new GroupsContainerObjectified.Author();
                            realMessage.Author.Id = message.Author;
                            realMessage.Author.Username = users.ToList().Find(x => x.Id == realMessage.Author.Id)?.Username ?? "Deleted User";
                            realMessage.Author.Usernumber = users.ToList().Find(x => x.Id == realMessage.Author.Id)?.Usernumber ?? "0000";
                            realMessages.Add(realMessage);
                        });
                    messagesSuccess.Messages = realMessages;
                    return Ok(messagesSuccess);
                } else
                {
                        GroupsContainerObjectified.Message message = chat.Messages.Find(x => x.Id == mid);
                    if (!String.IsNullOrEmpty(message?.Id))
                        {
                            GroupsContainerSuccess.MessageSuccess2 messageSuccess = new GroupsContainerSuccess.MessageSuccess2();
                            messageSuccess.Success = true;
                            GroupsContainerObjectified.Message2 realMessage = new GroupsContainerObjectified.Message2();
                            realMessage.Id = message.Id;
                            realMessage.Content = message.Content;
                            realMessage.Author = new GroupsContainerObjectified.Author();
                            realMessage.Author.Id = message.Author;
                            realMessage.Author.Username = users.ToList().Find(x => x.Id == realMessage.Author.Id)?.Username ?? "Deleted User";
                            realMessage.Author.Usernumber = users.ToList().Find(x => x.Id == realMessage.Author.Id)?.Usernumber ?? "0000";
                            messageSuccess.Message = realMessage;
                            return Ok(messageSuccess);
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

        [HttpPost]
        public IActionResult Send(string gid, string cid)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            Microsoft.Extensions.Primitives.StringValues messagePre;
            Request.Headers.TryGetValue("Message", out messagePre);
            string message = messagePre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == gid);
                GroupsContainerObjectified.Chat chat = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group?.Chats ?? "[{}]")?.Find(x => x.Id == cid);
                if (!String.IsNullOrEmpty(chat?.Id) && !String.IsNullOrEmpty(message) && message.Length < 1000)
                {
                    List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);
                    GroupsContainerObjectified.Message messageObject = new GroupsContainerObjectified.Message();
                    messageObject.Id = Program.generator.CreateId().ToString();
                    messageObject.Content = message;
                    messageObject.Author = user.Id;
                    chat.Messages.Add(messageObject);
                    chats[chats.FindIndex(x => x.Id == cid)].Messages = chat.Messages;
                    group.Chats = JsonConvert.SerializeObject(chats);
                    Program.db.Update(group);
                    GroupsContainerSuccess.MessageSuccess messageSuccess = new GroupsContainerSuccess.MessageSuccess();
                    messageSuccess.Success = true;
                    messageSuccess.Message = messageObject;
                    WebsocketObjectContainer.Message websocketObject = new WebsocketObjectContainer.Message();
                    websocketObject.Id = messageObject.Id;
                    websocketObject.Content = messageObject.Content;
                    nPI2UserContainer.Users nPIUser = new nPI2UserContainer.Users();
                    nPIUser.Id = user.Id;
                    nPIUser.Username = user.Username;
                    nPIUser.Usernumber = user.Usernumber;
                    websocketObject.Author = nPIUser;
                    _hubContext.Clients.Group(chat.Id).SendAsync("NewMessage", websocketObject);
                    return Ok(messageSuccess);
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

        [HttpPatch("{mid}")]
        public IActionResult Edit(string gid, string cid, string mid)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            Microsoft.Extensions.Primitives.StringValues messagePre;
            Request.Headers.TryGetValue("Message", out messagePre);
            string message = messagePre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == gid);
                GroupsContainerObjectified.Chat chat = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group?.Chats ?? "[{}]")?.Find(x => x.Id == cid);
                GroupsContainerObjectified.Message messageObject = chat.Messages.Find(x => x.Id == mid);
                if (!String.IsNullOrEmpty(message) && !String.IsNullOrEmpty(chat?.Id) && !String.IsNullOrEmpty(messageObject?.Id) && message.Length < 1000)
                {
                    List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);
                    GroupsContainerObjectified.Message messageThingy = chats[chats.FindIndex(x => x.Id == cid)].Messages[chats[chats.FindIndex(x => x.Id == cid)].Messages.FindIndex(x => x.Id == mid)];
                    messageThingy.Content = message;
                    group.Chats = JsonConvert.SerializeObject(chats);
                    Program.db.Update(group);
                    GroupsContainerSuccess.MessageSuccess messageSuccess = new GroupsContainerSuccess.MessageSuccess();
                    messageSuccess.Success = true;
                    messageSuccess.Message = messageThingy;
                    WebsocketObject2Container.Message websocketObject = new WebsocketObject2Container.Message();
                    websocketObject.Id = messageObject.Id;
                    websocketObject.Content = messageObject.Content;
                    _hubContext.Clients.Group(chat.Id).SendAsync("EditedMessage", websocketObject);
                    return Ok(messageSuccess);
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

        [HttpDelete("{mid}")]
        public IActionResult Delete(string gid, string cid, string mid)
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
                GroupsContainerObjectified.Message messageObject = chat.Messages.Find(x => x.Id == mid);
                if (!String.IsNullOrEmpty(chat?.Id) && !String.IsNullOrEmpty(messageObject?.Id))
                {
                    List<GroupsContainerObjectified.Chat> chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);
                    chats[chats.FindIndex(x => x.Id == cid)].Messages.RemoveAll(x => x.Id == mid);
                    group.Chats = JsonConvert.SerializeObject(chats);
                    Program.db.Update(group);
                    GroupsContainerSuccess.MessageSuccess messageSuccess = new GroupsContainerSuccess.MessageSuccess();
                    messageSuccess.Success = true;
                    messageSuccess.Message = messageObject;
                    WebsocketObject2Container.Message websocketObject = new WebsocketObject2Container.Message();
                    websocketObject.Id = messageObject.Id;
                    _hubContext.Clients.Group(chat.Id).SendAsync("DeletedMessage", websocketObject);
                    return Ok(messageSuccess);
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

    }
}
