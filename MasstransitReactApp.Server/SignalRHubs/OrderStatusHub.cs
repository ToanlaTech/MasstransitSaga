using Microsoft.AspNetCore.SignalR;

namespace MasstransitReactApp.Server.SignalRHubs
{
    public class OrderStatusHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            // Thêm client vào một group riêng theo orderId
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            // Xóa client khỏi group khi họ không muốn nhận cập nhật nữa
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        }
    }
}
