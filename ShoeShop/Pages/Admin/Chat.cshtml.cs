using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Pages.Admin
{
    [Authorize(Roles = "Admin,Manager,Consultant")]
    public class ChatModel : PageModel
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ChatModel> _logger;

        public ChatModel(ApplicationContext context, ILogger<ChatModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<ChatSession> ChatSessions { get; set; } = new();
        public List<ChatMessage> Messages { get; set; } = new();
        public string? SelectedUserId { get; set; }
        public string? SelectedUserName { get; set; }

        public async Task OnGetAsync(string? userId = null)
        {
            // Получаем все уникальные сессии чатов
            ChatSessions = await _context.ChatMessages
                .GroupBy(m => m.UserId)
                .Select(g => new ChatSession
                {
                    UserId = g.Key,
                    UserName = g.First().UserName,
                    LastMessage = g.OrderByDescending(m => m.CreatedAt).First().Message,
                    LastMessageTime = g.Max(m => m.CreatedAt),
                    MessageCount = g.Count(),
                    UnreadCount = g.Count(m => !m.IsAnswered && string.IsNullOrEmpty(m.Response))
                })
                .OrderByDescending(s => s.LastMessageTime)
                .ToListAsync();

            // Если выбран пользователь, загружаем его сообщения
            if (!string.IsNullOrEmpty(userId))
            {
                SelectedUserId = userId;
                var firstMessage = await _context.ChatMessages
                    .Where(m => m.UserId == userId)
                    .FirstOrDefaultAsync();
                
                SelectedUserName = firstMessage?.UserName ?? "Неизвестный пользователь";

                Messages = await _context.ChatMessages
                    .Where(m => m.UserId == userId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostSendResponseAsync(string userId, string response)
        {
            try
            {
                // Получаем имя пользователя
                var userMessage = await _context.ChatMessages
                    .Where(m => m.UserId == userId)
                    .FirstOrDefaultAsync();
                
                var userName = userMessage?.UserName ?? "Гость";
                
                // Создаем новое сообщение от администратора
                var adminMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    UserName = userName,
                    Message = response,
                    CreatedAt = DateTime.Now,
                    IsAnswered = true,
                    IsAutoResponse = false,
                    Response = null,
                    RespondedBy = User.Identity?.Name ?? "Администратор",
                    RespondedAt = DateTime.Now,
                    IsClosed = false
                };

                _context.ChatMessages.Add(adminMessage);
                
                // Отмечаем все неотвеченные сообщения как отвеченные
                var unreadMessages = await _context.ChatMessages
                    .Where(m => m.UserId == userId && !m.IsAnswered)
                    .ToListAsync();
                    
                foreach (var msg in unreadMessages)
                {
                    msg.IsAnswered = true;
                }

                await _context.SaveChangesAsync();

                return RedirectToPage(new { userId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending response: {ex.Message}");
                return RedirectToPage(new { userId = userId });
            }
        }

        public async Task<IActionResult> OnPostCloseChatAsync(string userId)
        {
            try
            {
                var messages = await _context.ChatMessages
                    .Where(m => m.UserId == userId)
                    .ToListAsync();

                foreach (var message in messages)
                {
                    message.IsClosed = true;
                }

                await _context.SaveChangesAsync();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error closing chat: {ex.Message}");
                return RedirectToPage();
            }
        }
    }

    public class ChatSession
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public DateTime LastMessageTime { get; set; }
        public int MessageCount { get; set; }
        public int UnreadCount { get; set; }
    }
}