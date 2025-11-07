using Microsoft.AspNetCore.SignalR;

namespace ShoppingBasket.Api.Hubs;

public class CatalogHub : Hub
{
    public const string CatalogChanged = "catalog:changed";
}
