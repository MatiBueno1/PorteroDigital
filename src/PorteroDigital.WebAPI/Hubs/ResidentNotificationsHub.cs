using Microsoft.AspNetCore.SignalR;

namespace PorteroDigital.WebAPI.Hubs;

public sealed class ResidentNotificationsHub : Hub
{
    public Task JoinHouseGroup(Guid houseId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetHouseGroup(houseId));
    }

    public static string GetHouseGroup(Guid houseId)
    {
        return $"house:{houseId:N}";
    }
}
