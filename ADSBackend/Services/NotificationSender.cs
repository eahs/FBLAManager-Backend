using ADSBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources;
using OneSignal.RestAPIv3.Client.Resources.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
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

                    var bconfig = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true)
                        .Build();

                    try
                    {
                        var pendingNotifications = await context.BoardPost
                            .Where(n => n.PostTime < DateTime.Now)
                            .Where(n => n.Status == "pending")
                            .ToListAsync();

                        if (pendingNotifications.Count > 0)
                        {
                            string apiKey = bconfig["OneSignalAPIKey"];

                            var client = new OneSignalClient(apiKey); // Use your Api Key

                            foreach (var notification in pendingNotifications)
                            {
                                var options = new NotificationCreateOptions
                                {
                                    AppId = new Guid("1c3e4393-0690-49b2-8e35-1281c2172bef"),   // Use your AppId
                                    IncludedSegments = new string[] { "Subscribed Users" }.ToList()
                                };
                                options.Headings.Add(LanguageCodes.English, notification.Title);
                                options.Contents.Add(LanguageCodes.English, notification.Message);

                                var result = client.Notifications.Create(options);

                                notification.Status = "sent";
                            }

                            context.UpdateRange(pendingNotifications);
                            await context.SaveChangesAsync();
                        }
                    }
                    catch(Exception e)
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