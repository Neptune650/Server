using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using IdGen;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLite;

public class UsersContainer
{
    public class Users
    {
        [PrimaryKey]
        public string Token { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public string Username { get; set; }
        public string Usernumber { get; set; }
        public string Groups { get; set; }
    }
}

public class GroupsContainer
{
    public class Groups
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Chats { get; set; }
    }
}

public class InvitesContainer
{
    public class Invites
    {
        [PrimaryKey]
        public string Code { get; set; }
        public string Group { get; set; }
    }
}

public class InvitesContainerSuccess
{
    public class Invites
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Group { get; set; }
    }
}

public class WebsocketObjectContainer
{

    public class Message
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public nPI2UserContainer.Users Author { get; set; }
    }
}

public class WebsocketObject2Container
{

    public class Message
    {
        public string Id { get; set; }
        public string Content { get; set; }
    }
}

public class WebsocketObject3Container
{

    public class Message
    {
        public string Id { get; set; }
    }
}

public class WebsocketObject4Container
{

    public class Chat
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

public class WebsocketObject5Container
{

    public class Chat
    {
        public string Id { get; set; }
    }
}

public class WebsocketObject6Container
{
    public class Chat
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Groups
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

public class WebsocketObject7Container
{

    public class Groups
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
    }
}

public class WebsocketObject8Container
{

    public class Groups
    {
        public string Id { get; set; }
    }
}

public class GroupsContainerObjectified
{
        public class Author
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Usernumber { get; set; }
        }

        public class Message2
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public Author Author { get; set; }
    }

    public class Message
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
    }

    public class Chat
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Message> Messages { get; set; }
    }

    public class Groups
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public List<Chat> Chats { get; set; }
        public List<string> Members { get; set; }
    }
}

public class GroupsContainerSuccess
{
    public class Author
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Usernumber { get; set; }
    }

    public class MessageSuccess
    {
        public bool Success { get; set; }
        public GroupsContainerObjectified.Message Message { get; set; }
    }


    public class MessageSuccess2
    {
        public bool Success { get; set; }
        public GroupsContainerObjectified.Message2 Message { get; set; }
    }

    public class MessagesSuccess
    {
        public bool Success { get; set; }
        public List<GroupsContainerObjectified.Message2> Messages { get; set; }
    }

    public class Chat
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ChatSuccess
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Groups
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public List<Chat> Chats { get; set; }
        public List<string> Members { get; set; }
    }
}

public class nPIUserContainer
{
    public class Users
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Usernumber { get; set; }
        public List<GroupsContainerObjectified.Groups> Groups { get; set; }
    }
}

public class nPI2UserContainer
{

    public class Users
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Usernumber { get; set; }
    }

    public class UsersSuccess
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public string Usernumber { get; set; }
    }
}

namespace Server
{
    public class Program
    {
        public static string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Harmony.db");
        public static SQLiteConnection db = new SQLiteConnection(databasePath);
        public static IdGenerator generator = new IdGenerator(0);

        public static void Main(string[] args)
        {
            db.CreateTable<UsersContainer.Users>();
            db.CreateTable<GroupsContainer.Groups>();
            db.CreateTable<InvitesContainer.Invites>();

                CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
