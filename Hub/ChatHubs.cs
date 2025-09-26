using ChatHub.Api.Model;
using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Api.Hub
{
    public class ChatHubs : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connections;
        public ChatHubs(IDictionary<string, UserRoomConnection> connections)
        {
            _connections = connections;
        }
        public async Task JoinRoom(UserRoomConnection userRoomConnection)
        { 
        await Groups.AddToGroupAsync(Context.ConnectionId, userRoomConnection.Room!);
            _connections[Context.ConnectionId] = userRoomConnection;
            await Clients.Group(userRoomConnection.Room!)
                .SendAsync("ReceiveMessage", "Lets Program Bot",$"{userRoomConnection.User} has joined the room {userRoomConnection.Room}",DateTime.Now);
            await SendConnectedUser(userRoomConnection.Room!);
        }
        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var userRoomConnection))
            {
                await Clients.Group(userRoomConnection.Room!)
                    .SendAsync("ReceiveMessage", $"{userRoomConnection.User}",message,DateTime.Now);
            }
        }
        public Task SendConnectedUser(string room)
        {
            var users = _connections.Values.
                Where(x => x.Room == room)
                .Select(x => x.User);
            return Clients.Group(room)
                .SendAsync("ConnectedUser", users);
        }
        public override  Task OnDisconnectedAsync(Exception? exp)
        {
            if (!_connections.TryGetValue(Context.ConnectionId, out UserRoomConnection roomConnection))
            {
                //_connections.Remove(Context.ConnectionId);
                //await Clients.Group(userRoomConnection.Room!)
                //    .SendAsync("ReceiveMessage", $"{userRoomConnection.User} has left the room {userRoomConnection.Room}");
                //await sendConnection(userRoomConnection.Room!);
                return base.OnDisconnectedAsync(exp);
            }
            Clients.Group(roomConnection.Room!).SendAsync
                ("ReceiveMessage","Lets Program bot", $"{roomConnection.User} has left the room {roomConnection.Room}",DateTime.Now);
            SendConnectedUser(roomConnection.Room!);
            return base.OnDisconnectedAsync(exp);
        }
    }
}
