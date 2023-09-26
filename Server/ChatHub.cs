using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ChatHub : Microsoft.AspNet.SignalR.Hub<IClient>
    {
        private static ConcurrentDictionary<string, User> ChatClients = new ConcurrentDictionary<string, User>();

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                Clients.Others.ParticipantDisconnection(userName);
                Console.WriteLine($"-- {userName} disconnected");
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                Clients.Others.ParticipantReconnection(userName);
                Console.WriteLine($"== {userName} reconnected");
            }
            return base.OnReconnected();
        }

        public List<User> Login(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                User existingUser;
                if (ChatClients.TryGetValue(name, out existingUser))
                {
                    var currentTime = DateTime.UtcNow;
                    var timeSinceLastLogin = currentTime - existingUser.LastLoginTime;

                    if (timeSinceLastLogin.TotalMinutes <= 10)
                    {
                        Console.WriteLine($"++ {name} reloaded");
                    }
                    else
                    {
                        Console.WriteLine($"++ {name} logged in");
                    }
                    existingUser.LastLoginTime = currentTime;
                    Clients.CallerState.UserName = name;
                    Clients.Others.ParticipantLogin(existingUser);
                    return new List<User>(ChatClients.Values);
                }
                else
                {
                    Console.WriteLine($"++ {name} logged in");
                    var newUser = new User { Name = name, ID = Context.ConnectionId, LastLoginTime = DateTime.UtcNow };
                    ChatClients[name] = newUser;
                    Clients.CallerState.UserName = name;
                    Clients.Others.ParticipantLogin(newUser);
                    return new List<User>(ChatClients.Values);
                }
            }
            return null;
        }

        public void RecordUserAction(string action)
        {
            var userName = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(action))
            {
                Console.WriteLine($"{userName} clicked {action}");
            }
        }
    }
}
