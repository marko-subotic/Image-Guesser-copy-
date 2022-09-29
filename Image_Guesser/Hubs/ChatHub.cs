using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Image_Guesser.Data;

namespace Image_Guesser.Hubs
{

    //user class with fields for various data, important part is isHost and hostGame
    //this is the game that should be sent out to all other users to play their game
    //with, but to stay on the same game as everyone else
    public class User
    {
        private string connectionId;
        private string userName;
        private bool isReady;
        private bool isHost;
        private Game hostGame;
        private int score;

        private int timer;

        public User(string connectionId, string userName)
        {

            this.connectionId = connectionId;
            this.userName = userName;
            isReady = false;
            isHost = false;
            hostGame = null;

            score = 0;

            timer = 30;

        }

        public void isReadyTrue()
        {
            isReady = true;
        }

        public void isReadyFalse()
        {
            isReady = false;
        }

        public string getConnectionId()
        {
            return connectionId;
        }

        public string getUserName()
        {
            return userName;
        }

        public bool getIsReady()
        {
            return isReady;
        }
        
        public int getScore()
        {
            return score;
        }

        public int getTimer()
        {
            return timer;
        }

        public void setScore(int score)
        {
            this.score = score;
        }
        public bool getIsHost()
        {
            return isHost;
        }

        public void setIsHost()
        {
            if(!isHost)
            {
                isHost = true;
                hostGame = new Game();
            }
        }
        public Game getHostGame()
        {
            return hostGame;
        }

        public void setTimer(int input)
        {
            timer = input;
        }
    }

    public class ChatHub : Hub
    {
        // the key is group code, the ArrayList contains all users in the group
        private static Dictionary<String, ArrayList> userStorage = new Dictionary<String, ArrayList>();
        public static Dictionary<String, int> roomTimers = new Dictionary<String, int>();
        public async Task SendMessage(string user, string message, string groupName)
        {
            if (userStorage.ContainsKey(groupName) && !message.Equals(""))
            {
                Console.WriteLine("sending message rn");

                await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
            }
        }

        public async Task SendScore(string user, int score, string groupName)
        {
            if (userStorage.ContainsKey(groupName))
            {
                Console.WriteLine("sending score rn");
                searchUsers(groupName, user).setScore(score);
                await SendMessage(user, "sending scores", groupName);
            }
        }

        public async Task<bool> AddToGroup(string groupName, string userName)
        {
            if (!userStorage.ContainsKey(groupName))
            {
                userStorage.Add(groupName, new ArrayList());
                roomTimers.Add(groupName, 30);
                User host = new User(Context.ConnectionId, userName);
                host.setIsHost();
                userStorage.GetValueOrDefault(groupName).Add(host);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                Console.WriteLine(userStorage.GetValueOrDefault(Context.ConnectionId));
                await Clients.Group(groupName).SendAsync("ReceiveMessage", groupName, $"{groupName} has been created.");
                return true;
            }
            else
            {
                ArrayList work = userStorage.GetValueOrDefault(groupName);
                if (!work.Contains(Context.ConnectionId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    await Clients.Group(groupName).SendAsync("ReceiveMessage", Context.ConnectionId, $"{userName} has joined the group {groupName}.");
                    work.Add(new User(Context.ConnectionId, userName));
                }
                return true;
            }

        }
        public async Task<bool> CheckGroup(string groupName)
        {
            
            if (string.IsNullOrEmpty(groupName) || !userStorage.ContainsKey(groupName))
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public async Task CreateGroup(string groupName, string userName)
        {
            userStorage.Add(groupName, new ArrayList());
            userStorage.GetValueOrDefault(groupName).Add(new User(Context.ConnectionId, userName));
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine(userStorage.GetValueOrDefault(Context.ConnectionId));
            await Clients.Group(groupName).SendAsync("ReceiveMessage", groupName, $"{groupName} has been created.");

        }

        public async Task RemoveFromGroup(String groupName, String userName)
        {
            if (userStorage.ContainsKey(groupName))
            {
                userStorage.GetValueOrDefault(groupName).Remove(searchUsers(groupName, userName));
                await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
        }

        public static List<String> getUsernameList(String groupName)
        {
            List<String> temp = new List<String>();
            if (userStorage.ContainsKey(groupName))
            {

                foreach (User user in userStorage.GetValueOrDefault(groupName))
                {
                    temp.Add(user.getUserName());
                }
                return temp;
            }
            return null;
        }
        public static List<int> getScoresList(String groupName)
        {
            
            List<int> temp = new List<int>();
            if (userStorage.ContainsKey(groupName))
            {

                foreach (User user in userStorage.GetValueOrDefault(groupName))
                {
                    temp.Add(user.getScore());
                }
                return temp;
            }
            return null;
        }

        public static List<User> getUserList(String groupName)
        {
            List<User> temp = new List<User>();
            if (userStorage.ContainsKey(groupName))
            {

                userStorage.GetValueOrDefault(groupName);

            }
            return null;
        }
        public void ChangeStatusTrue(string groupName, string userInput)
        {
            Console.WriteLine("begging");
            searchUsers(groupName, userInput).isReadyTrue();
        }
        public void ChangeStatusFalse(string groupName, string userInput)
        {
            searchUsers(groupName, userInput).isReadyFalse();
        }


        private User searchUsers(String groupName, String userName)
        {
            ArrayList temp = userStorage.GetValueOrDefault(groupName);
            foreach (User user in temp)
            {
                if (user.getUserName().Equals(userName))
                {
                    return user;
                }
            }
            return null;
        }

        public static User getGameHost(String groupName)
        {
            ArrayList temp = userStorage.GetValueOrDefault(groupName);
            foreach (User user in temp)
            {
                if (user.getIsHost())
                {
                    return user;
                }
            }
            return null;
        }
        public static Game getHostGame(String groupName)
        {
            ArrayList temp = userStorage.GetValueOrDefault(groupName);
            foreach (User user in temp)
            {
                if (user.getIsHost())
                {
                    return user.getHostGame();
                }
            }
            return null;
        }

        public bool allUsersReady(String groupName)
        {
            ArrayList temp = userStorage.GetValueOrDefault(groupName);
            foreach (User user in temp)
            {
                if (!user.getIsReady())
                {
                    return false;
                }
            }
            return true;
        }

        public void SetNewImage(String groupName)
        {
            getHostGame(groupName).makeNewImage();
        }
    }
}
