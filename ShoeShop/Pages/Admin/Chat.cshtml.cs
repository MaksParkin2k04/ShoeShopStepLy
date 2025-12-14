using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeShop.Models;
using System.Text.Json;
using ShoeShop.Data;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin,Consultant")]
    public class ChatModel : PageModel
    {
        private readonly ApplicationContext _context;
        
        public ChatModel(ApplicationContext context)
        {
            _context = context;
        }
        
        public List<ChatMessage> UnreadMessages { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                UnreadMessages = _context.ChatMessages
                    .Where(m => !m.IsAutoResponse && !m.IsClosed)
                    .GroupBy(m => m.UserId)
                    .Where(g => g.Any(msg => !msg.IsAnswered))
                    .Select(g => g.OrderByDescending(m => m.CreatedAt).First(msg => !msg.IsAnswered))
                    .ToList();
            }
            catch
            {
                UnreadMessages = new List<ChatMessage>();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var json = await new StreamReader(Request.Body).ReadToEndAsync();
                if (string.IsNullOrEmpty(json))
                {
                    return new JsonResult(new { success = false, error = "Пустой запрос" });
                }
                
                var data = JsonSerializer.Deserialize<JsonElement>(json);
            
            if (data.TryGetProperty("action", out var actionElement) && actionElement.GetString() == "close")
            {
                return await CloseChat(data);
            }
            
            var messageId = data.GetProperty("messageId").GetString();
            var response = data.GetProperty("response").GetString();
            
            try
            {
                var message = _context.ChatMessages
                    .FirstOrDefault(m => m.Id.ToString() == messageId);
                
                if (message != null)
                {
                    message.Response = response;
                    message.RespondedBy = "Консультант StepLy";
                    message.RespondedAt = DateTime.Now;
                    message.IsAnswered = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
            
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
        
        private async Task<IActionResult> CloseChat(JsonElement data)
        {
            try
            {
                var userId = data.GetProperty("userId").GetString();
                var messages = _context.ChatMessages
                    .Where(m => m.UserId == userId)
                    .ToList();
                
                foreach (var message in messages)
                {
                    message.IsClosed = true;
                }
                
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}