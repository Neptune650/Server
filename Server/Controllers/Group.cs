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
    [Route("[controller]")]
    public class Group : Controller
    {

        private readonly IHubContext<Hubs.SignalR> _hubContext;

        public Group(IHubContext<Hubs.SignalR> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet("{id}")]
        public IActionResult Info(string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == id);
                if (!String.IsNullOrEmpty(group?.Id))
                {
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
                    return Ok(groupSuccess);
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
        public IActionResult Create(string id)
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
                if (name.Length < 31 && !String.IsNullOrEmpty(name))
                {
                    GroupsContainerObjectified.Groups groupObjectified = new GroupsContainerObjectified.Groups();
                    groupObjectified.Id = Program.generator.CreateId().ToString();
                    groupObjectified.Name = name;
                    groupObjectified.Owner = user.Id;
                    GroupsContainerObjectified.Chat chatObjectified = new GroupsContainerObjectified.Chat();
                    chatObjectified.Id = Program.generator.CreateId().ToString();
                    chatObjectified.Name = "general";
                    chatObjectified.Messages = new List<GroupsContainerObjectified.Message>();
                    List<GroupsContainerObjectified.Chat> chats = new List<GroupsContainerObjectified.Chat>();
                    chats.Add(chatObjectified);
                    groupObjectified.Chats = chats;
                    List<string> members = new List<string>();
                    members.Add(groupObjectified.Owner);
                    groupObjectified.Members = members;

                    GroupsContainer.Groups group = new GroupsContainer.Groups();
                    group.Id = groupObjectified.Id;
                    group.Name = groupObjectified.Name;
                    group.Owner = groupObjectified.Owner;
                    group.Chats = JsonConvert.SerializeObject(groupObjectified.Chats);
                    Program.db.Insert(group);

                    List<string> groups = JsonConvert.DeserializeObject<List<string>>(user.Groups);
                    groups.Add(groupObjectified.Id);
                    user.Groups = JsonConvert.SerializeObject(groups);
                    Program.db.Update(user);

                    WebsocketObject6Container.Groups websocketObject = new WebsocketObject6Container.Groups();
                    websocketObject.Id = group.Id;
                    websocketObject.Name = group.Name;
                    _hubContext.Clients.Group(user.Id).SendAsync("JoinedGroup", websocketObject);

                    GroupsContainerSuccess.Groups groupSuccess = new GroupsContainerSuccess.Groups();
                    groupSuccess.Success = true;
                    groupSuccess.Id = group.Id;
                    groupSuccess.Name = group.Name;
                    groupSuccess.Owner = group.Owner;
                    List<GroupsContainerSuccess.Chat> chats2 = new List<GroupsContainerSuccess.Chat>();
                    chats.ForEach(x => {
                        GroupsContainerSuccess.Chat chat = new GroupsContainerSuccess.Chat();
                        chat.Id = x.Id;
                        chat.Name = x.Name;
                        chats2.Add(chat);
                    });
                    groupSuccess.Chats = chats2;

                    return Ok(groupSuccess);
                }
                else
                {
                    return BadRequest(new Dictionary<string, object>{
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

        [HttpPatch("{id}")]
        public IActionResult Edit(string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            Microsoft.Extensions.Primitives.StringValues namePre;
            Request.Headers.TryGetValue("Name", out namePre);
            string name = namePre.ToString();
            Microsoft.Extensions.Primitives.StringValues ownerPre;
            Request.Headers.TryGetValue("Owner", out ownerPre);
            string owner = ownerPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == id);

                if (user.Id == group.Owner)
                {
                        if (!String.IsNullOrEmpty(group?.Id) && !String.IsNullOrEmpty(name) ? name.Length < 31 : true && !String.IsNullOrEmpty(owner) ? !String.IsNullOrEmpty(users.ToList().Find(x => x.Id == owner)?.Id) : true)
                        {
                            group.Name = name.Length < 31 ? name : group.Name;
                            group.Owner = !String.IsNullOrEmpty(users.ToList().Find(x => x.Id == owner)?.Id) ? owner : group.Owner;
                            Program.db.Update(group);
                            GroupsContainerSuccess.Groups groupSuccess = new GroupsContainerSuccess.Groups();
                            groupSuccess.Success = true;
                            groupSuccess.Id = group.Id;
                            groupSuccess.Name = group.Name;
                            groupSuccess.Owner = group.Owner;
                            groupSuccess.Chats = JsonConvert.DeserializeObject<List<GroupsContainerSuccess.Chat>>(group.Chats);

                        WebsocketObject7Container.Groups websocketObject = new WebsocketObject7Container.Groups();
                        websocketObject.Id = group.Id;
                        websocketObject.Name = group.Name;
                        websocketObject.Owner = group.Owner;
                        _hubContext.Clients.Group(group.Id).SendAsync("EditedGroup", websocketObject);

                        return Ok(groupSuccess);
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
                        { "errorCode", 2 },
                        { "error", "You don't own this group." }
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

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                var groups = Program.db.Table<GroupsContainer.Groups>();
                GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == id);

                if (user.Id == group?.Owner)
                {
                    Program.db.Delete<GroupsContainer.Groups>(id);
                    GroupsContainerObjectified.Groups groupObjectified = new GroupsContainerObjectified.Groups();
                    groupObjectified.Id = group.Id;
                    groupObjectified.Name = group.Name;
                    groupObjectified.Owner = group.Owner;
                    groupObjectified.Chats = JsonConvert.DeserializeObject<List<GroupsContainerObjectified.Chat>>(group.Chats);

                    WebsocketObject8Container.Groups websocketObject = new WebsocketObject8Container.Groups();
                    websocketObject.Id = group.Id;
                    _hubContext.Clients.Group(group.Id).SendAsync("DeletedGroup", websocketObject);
                    return Ok(groupObjectified);
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 2 },
                        { "error", "You don't own this group." }
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

        [HttpPost("{id}/invite")]
        public IActionResult Invite(string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                if (JsonConvert.DeserializeObject<List<string>>(user.Groups).Contains(id))
                {
                    var invites = Program.db.Table<InvitesContainer.Invites>();
                    string invite = GenerateInvite(invites);
                    InvitesContainer.Invites inviteObject = new InvitesContainer.Invites();
                    inviteObject.Code = invite;
                    inviteObject.Group = id;
                    Program.db.Insert(inviteObject);
                    InvitesContainerSuccess.Invites inviteSuccess = new InvitesContainerSuccess.Invites();
                    inviteSuccess.Success = true;
                    inviteSuccess.Code = inviteObject.Code;
                    inviteSuccess.Group = inviteObject.Group;
                    return Ok(inviteSuccess);
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "You aren't in this group." }
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

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveAsync(string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                List<string> userGroups = JsonConvert.DeserializeObject<List<string>>(user.Groups);
                if (userGroups.Contains(id))
                {
                    userGroups.Remove(id);
                    user.Groups = JsonConvert.SerializeObject(userGroups);
                    Program.db.Update(user);
                    var groups = Program.db.Table<GroupsContainer.Groups>();
                    GroupsContainer.Groups group = groups.ToList().Find(x => x.Id == id);
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

                    WebsocketObject8Container.Groups websocketObject = new WebsocketObject8Container.Groups();
                    websocketObject.Id = group.Id;
                    foreach (string groupMember in Hubs.SignalR.usersDictionary.Values) {
                        await _hubContext.Groups.RemoveFromGroupAsync(groupMember, group.Id);
                    }
                    await _hubContext.Clients.Group(user.Id).SendAsync("LeftGroup", websocketObject);

                    return Ok(groupSuccess);
                }
                else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "success", false },
                        { "errorCode", 1 },
                        { "error", "You aren't in this group." }
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

        string GenerateInvite(SQLite.TableQuery<InvitesContainer.Invites> invites)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string invite = new string(Enumerable.Range(1, 6).Select(_ => chars[new Random().Next(chars.Length)]).ToArray());
            if (invites.Any(x => x.Code == invite))
            {
                GenerateInvite(invites);
            }
            return invite;
        }

        }
    }