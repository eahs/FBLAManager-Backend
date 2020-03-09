using ADSBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ADSBackend.Services
{
    public class NotificationSender : HostedService
    {
        private readonly IServiceProvider _provider;

        public NotificationSender(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                using (IServiceScope scope = _provider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var config = scope.ServiceProvider.GetRequiredService<Configuration>();

                    try
                    {
                        var pendingNotifications = await context.BoardPost
                            .Where(n => n.PostTime < DateTime.Now)
                            .Where(n => n.Status == "pending")
                            .ToListAsync();

                        if (pendingNotifications.Count > 0)
                        {
                            foreach (var notification in pendingNotifications)
                            {
                                // TODO: Create the message to send
                                
                                // TODO: Send it

                                notification.Status = "sent";
                            }

                            context.UpdateRange(pendingNotifications);
                            await context.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        // if the DB is not currently connected, wait a second and try again
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                }

                var task = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                try
                {
                    await task;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        /*
        private string ConvertToPlainText(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(System.Web.HttpUtility.HtmlDecode(str));
            return doc.DocumentNode.InnerText;
        }
        */

        private string Truncate(string str, int length)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Length <= length ? str : str.Substring(0, length);
        }
    }
}