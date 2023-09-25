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
                    // Проверяем время последнего входа
                    var currentTime = DateTime.UtcNow;
                    var timeSinceLastLogin = currentTime - existingUser.LastLoginTime;

                    if (timeSinceLastLogin.TotalMinutes <= 10)
                    {
                        // Если прошло менее 10 минут, отправляем сообщение о перезаходе
                        Console.WriteLine($"++ {name} reloaded");
                    }
                    else
                    {
                        // Если прошло более 10 минут, отправляем сообщение о входе
                        Console.WriteLine($"++ {name} logged in");
                    }

                    // Обновляем время последнего входа
                    existingUser.LastLoginTime = currentTime;
                    Clients.CallerState.UserName = name;
                    Clients.Others.ParticipantLogin(existingUser);
                    return new List<User>(ChatClients.Values);
                }
                else
                {
                    // Пользователь не существует, создаем новую запись
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



        public void Logout()
        {
            var name = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(name))
            {
                User client = new User();
                ChatClients.TryRemove(name, out client);
                Clients.Others.ParticipantLogout(name);
                Console.WriteLine($"-- {name} logged out");
            }
        }

        public void BroadcastTextMessage(string message)
        {
            var name = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(message))
            {
                Clients.Others.BroadcastTextMessage(name, message);
            }
        }

        public void BroadcastImageMessage(byte[] img)
        {
            var name = Clients.CallerState.UserName;
            if (img != null)
            {
                Clients.Others.BroadcastPictureMessage(name, img);
            }
        }

        public void UnicastTextMessage(string recepient, string message)
        {
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                !string.IsNullOrEmpty(message) && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);
                Clients.Client(client.ID).UnicastTextMessage(sender, message);
            }
        }

        public void UnicastImageMessage(string recepient, byte[] img)
        {
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                img != null && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);
                Clients.Client(client.ID).UnicastPictureMessage(sender, img);
            }
        }

        public void Typing(string recepient)
        {
            if (string.IsNullOrEmpty(recepient)) return;
            var sender = Clients.CallerState.UserName;
            User client = new User();
            ChatClients.TryGetValue(recepient, out client);
            Clients.Client(client.ID).ParticipantTyping(sender);
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
