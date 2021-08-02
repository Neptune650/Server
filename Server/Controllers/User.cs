using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{

    [Route("api/[controller]")]
    public class User : Controller
    {
        [HttpGet]
        public IActionResult InfoMe()
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            nPIUserContainer.Users user = new nPIUserContainer.Users();
            UsersContainer.Users userInside = users.ToList().Find(x => x.Token == token);
                if(userInside?.Token == token)
                {
                    user.Success = true;
                    user.Id = userInside.Id;
                    user.Email = userInside.Email;
                    user.Username = userInside.Username;
                    user.Usernumber = userInside.Usernumber;
                    user.Groups = JsonConvert.DeserializeObject<List<string>>(userInside.Groups);
                return Ok(user);

            } else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid token." }
                    });
            }
         }

        [HttpGet("{id}")]
        public IActionResult Info(string id)
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            if (!String.IsNullOrEmpty(users.ToList().Find(x => x.Token == token)?.Id?.ToString()))
            {
                UsersContainer.Users userInside = users.ToList().Find(x => x.Id == id);
                nPI2UserContainer.UsersSuccess user = new nPI2UserContainer.UsersSuccess();
                if (!String.IsNullOrEmpty(userInside?.Id))
                {
                    user.Success = true;
                    user.Id = userInside.Id;
                    user.Username = userInside.Username;
                    user.Usernumber = userInside.Usernumber;
                    return Ok(user);
                } else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid information provided." }
                    });
                }

            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid token." }
                    });
            }
        }

        [HttpPatch]
        public IActionResult Edit()
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            Microsoft.Extensions.Primitives.StringValues emailPre;
            Request.Headers.TryGetValue("Email", out emailPre);
            string email = emailPre.ToString();
            Microsoft.Extensions.Primitives.StringValues passwordPre;
            Request.Headers.TryGetValue("Password", out passwordPre);
            string password = passwordPre.ToString();
            Microsoft.Extensions.Primitives.StringValues usernamePre;
            Request.Headers.TryGetValue("Username", out usernamePre);
            string username = usernamePre.ToString();
            Microsoft.Extensions.Primitives.StringValues usernumberPre;
            Request.Headers.TryGetValue("Usernumber", out usernumberPre);
            string usernumber = usernumberPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users user = users.ToList().Find(x => x.Token == token);
            if (!String.IsNullOrEmpty(user?.Id))
            {
                    if (isEmail(!String.IsNullOrEmpty(email) ? email : user.Email) && (!String.IsNullOrEmpty(username) ? username : user.Username).Length < 31 && int.TryParse(!String.IsNullOrEmpty(usernumber) ? usernumber : user.Usernumber, out _) && (!String.IsNullOrEmpty(usernumber) ? usernumber : user.Usernumber).Length == 4 && (!String.IsNullOrEmpty(usernumber) ? usernumber : user.Usernumber) != "0000" && !String.IsNullOrEmpty(!String.IsNullOrEmpty(password) ? password : Encoding.Default.GetString(user.Password)))
                    {
                        user.Email = isEmail(user.Email) ? email : user.Email;
                    if (!String.IsNullOrEmpty(password))
                    {
                        var argon2 = new Argon2id(Encoding.Default.GetBytes(password));
                        argon2.DegreeOfParallelism = 4;
                        argon2.MemorySize = 1024;
                        argon2.Iterations = 40;
                        argon2.Salt = user.Salt;
                        argon2.AssociatedData = new byte[] { };
                        user.Password = argon2.GetBytes(128);
                        user.Token = "User " + (Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id)) + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString())) + "." + Convert.ToBase64String(new HMACSHA256().ComputeHash(new byte[] { 0 }))).Replace("=", "");
                    }
                        user.Username = !String.IsNullOrEmpty(username) && username.Length < 31 ? username : user.Username;
                        user.Usernumber = int.TryParse(!String.IsNullOrEmpty(usernumber) ? usernumber : user.Usernumber, out _) && usernumber.Length == 4 && usernumber != "0000" ? usernumber : user.Usernumber;
                    if (user.Token == token)
                    {
                        Program.db.Update(user);
                    }
                    else
                    {
                        Program.db.Insert(user);
                        Program.db.Delete<UsersContainer.Users>(token);
                    }
                        nPIUserContainer.Users nPIUser = new nPIUserContainer.Users();
                        nPIUser.Success = true;
                        nPIUser.Id = user.Id;
                        nPIUser.Email = user.Email;
                        nPIUser.Username = user.Username;
                        nPIUser.Usernumber = user.Usernumber;
                        nPIUser.Groups = JsonConvert.DeserializeObject<List<string>>(user.Groups);
                        return Ok(nPIUser);
                    } else
                    {
                        return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid information provided." }
                    });
                    }
            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid token." }
                    });
            }
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            Microsoft.Extensions.Primitives.StringValues tokenPre;
            Request.Headers.TryGetValue("Authorization", out tokenPre);
            string token = tokenPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            UsersContainer.Users userInside = users.ToList().Find(x => x.Token == token);
            nPIUserContainer.Users user = new nPIUserContainer.Users();
            if (!String.IsNullOrEmpty(userInside?.Id))
            {
                user.Success = true;
                user.Id = userInside.Id;
                user.Email = userInside.Email;
                user.Username = userInside.Username;
                user.Usernumber = userInside.Usernumber;
                user.Groups = JsonConvert.DeserializeObject<List<string>>(userInside.Groups);
                Program.db.Delete<UsersContainer.Users>(token);
                return Ok(user);
            }
            else
            {
                return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid token." }
                    });
            }
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            Microsoft.Extensions.Primitives.StringValues emailPre;
            Request.Headers.TryGetValue("Email", out emailPre);
            string email = emailPre.ToString();
            Microsoft.Extensions.Primitives.StringValues passwordPre;
            Request.Headers.TryGetValue("Password", out passwordPre);
            string password = passwordPre.ToString();
            var users = Program.db.Table<UsersContainer.Users>();
            string token = "";
            UsersContainer.Users user = users.ToList().Find(x => x.Email == email);
                var argon2 = new Argon2id(Encoding.Default.GetBytes(password));
                argon2.DegreeOfParallelism = 4;
                argon2.MemorySize = 1024;
                argon2.Iterations = 40;
                argon2.Salt = user.Salt;
                argon2.AssociatedData = new byte[] { };
                if (user?.Email == email && BitConverter.ToString(user?.Password) == BitConverter.ToString(argon2.GetBytes(128)))
                {
                    token = user.Token;
                }
                if (!String.IsNullOrEmpty(token))
                {
                    return Ok(new Dictionary<string, object>{
                        { "Success", true },
                        { "Token", token }
                    });
                } else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid information provided." }
                    });
                }
        }

        [HttpPost("register")]
        public IActionResult Register()
        {
            Microsoft.Extensions.Primitives.StringValues emailPre;
            Request.Headers.TryGetValue("Email", out emailPre);
            string email = emailPre.ToString();
            Microsoft.Extensions.Primitives.StringValues usernamePre;
            Request.Headers.TryGetValue("Username", out usernamePre);
            string username = usernamePre.ToString();
            Microsoft.Extensions.Primitives.StringValues usernumberPre;
            Request.Headers.TryGetValue("Usernumber", out usernumberPre);
            string usernumber = usernumberPre.ToString();
            Microsoft.Extensions.Primitives.StringValues passwordPre;
            Request.Headers.TryGetValue("Password", out passwordPre);
            string password = passwordPre.ToString();
            if(isEmail(email) && username.Length < 31 && int.TryParse(usernumber, out _) && usernumber.Length == 4 && usernumber != "0000" && !String.IsNullOrEmpty(password))
            {
                var users = Program.db.Table<UsersContainer.Users>();
                if (String.IsNullOrEmpty(users.ToList().Find(x => x.Email == email)?.Id?.ToString()) && String.IsNullOrEmpty(users.ToList().FindAll(x => x.Username == username).Find(x => x.Usernumber == usernumber)?.Id?.ToString())) {
                    UsersContainer.Users user = new UsersContainer.Users();
                    user.Id = Program.generator.CreateId().ToString();
                    user.Token = "User " + (Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id)) + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString())) + "." + Convert.ToBase64String(new HMACSHA256().ComputeHash(new byte[] { 0 }))).Replace("=", "");
                    user.Email = email;
                    user.Username = username;
                    user.Usernumber = usernumber;
                    var argon2 = new Argon2id(Encoding.Default.GetBytes(password));
                    argon2.DegreeOfParallelism = 4;
                    argon2.MemorySize = 1024;
                    argon2.Iterations = 40;
                    Byte[] salt = new byte[16];
                    new Random().NextBytes(salt);
                    user.Salt = salt;
                    argon2.Salt = salt;
                    argon2.AssociatedData = new byte[] { };
                    user.Password = argon2.GetBytes(128);
                    user.Groups = "[]";
                    Program.db.Insert(user);
                    return Ok(new Dictionary<string, object>{
                        { "Success", true },
                        { "Token", user.Token }
                    });
                } else
                {
                    return Unauthorized(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Information already used." }
                    });
                }
            } else
            {
                return BadRequest(new Dictionary<string, object>{
                        { "Success", false },
                        { "ErrorCode", 1 },
                        { "Error", "Invalid information provided." }
                    });
            }
        }

        bool isEmail(string email)
        {
            try
            {
                System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
}
            }
        }
    }
