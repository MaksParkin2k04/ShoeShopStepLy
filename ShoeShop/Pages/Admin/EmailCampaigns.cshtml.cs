using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;
using System.Net;
using System.Net.Mail;

namespace ShoeShop.Pages.Admin;

public class EmailCampaignsModel : PageModel
{
    private readonly ApplicationContext _context;
    private readonly IConfiguration _configuration;

    public EmailCampaignsModel(ApplicationContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public List<EmailCampaign> Campaigns { get; set; } = new();
    public int TotalCustomers { get; set; }
    public int SentCampaigns { get; set; }
    public int DraftCampaigns { get; set; }

    public async Task OnGetAsync()
    {
        Campaigns = await _context.EmailCampaigns
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        TotalCustomers = await _context.Users.CountAsync();
        SentCampaigns = Campaigns.Count(c => c.Status == "Sent");
        DraftCampaigns = Campaigns.Count(c => c.Status == "Draft");
    }

    public async Task<IActionResult> OnPostCreateAsync(string subject, string body)
    {
        var campaign = new EmailCampaign
        {
            Id = Guid.NewGuid(),
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.Now,
            RecipientCount = await _context.Users.CountAsync(),
            Status = "Draft"
        };

        _context.EmailCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Черновик рассылки создан";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendAsync(Guid campaignId)
    {
        var campaign = await _context.EmailCampaigns.FindAsync(campaignId);
        if (campaign == null)
        {
            TempData["Error"] = "Рассылка не найдена";
            return RedirectToPage();
        }

        var users = await _context.Users.Where(u => !string.IsNullOrEmpty(u.Email)).ToListAsync();
        
        if (users.Count == 0)
        {
            TempData["Error"] = "Нет пользователей с email для отправки";
            return RedirectToPage();
        }

        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"] ?? "StepLy";

        int sentCount = 0;
        string lastError = "";

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            foreach (var user in users)
            {
                try
                {
                    var message = new MailMessage
                    {
                        From = new MailAddress(fromEmail, fromName),
                        Subject = campaign.Subject,
                        Body = campaign.Body,
                        IsBodyHtml = true
                    };
                    message.To.Add(user.Email);

                    await client.SendMailAsync(message);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                }
            }

            campaign.Status = "Sent";
            campaign.SentAt = DateTime.Now;
            campaign.RecipientCount = sentCount;
            await _context.SaveChangesAsync();

            var msg = $"Рассылка отправлена {sentCount} из {users.Count} получателям";
            if (!string.IsNullOrEmpty(lastError)) msg += $". Последняя ошибка: {lastError}";
            TempData["Success"] = msg;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка отправки: {ex.Message}";
        }

        return RedirectToPage();
    }
}
