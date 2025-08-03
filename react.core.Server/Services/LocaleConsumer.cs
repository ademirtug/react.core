using duoword.admin.Server.Data;
using duoword.admin.Server.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace duoword.admin.Server.Services
{
    //public class LocaleConsumer : BackgroundService
    //{

    //    string queueName = "locale-translation";
    //    RabbitMQService mqService;
    //    IServiceScopeFactory scopeFactory;
    //    IChatClient AI;

    //    public LocaleConsumer(RabbitMQService service, IServiceScopeFactory factory, IChatClient chatClient)
    //    {
    //        mqService = service;
    //        scopeFactory = factory;
    //        AI = chatClient;
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        var channel = await mqService.GetChannelAsync();
    //        await channel.QueueDeclareAsync(queue: queueName,
    //                             durable: true,
    //                             exclusive: false,
    //                             autoDelete: false,
    //                             arguments: null);

    //        await channel.ExchangeDeclareAsync("dead_letter_exchange", "direct");
    //        await channel.QueueDeclareAsync("dead_letter_queue", durable: true, exclusive: false, autoDelete: false);
    //        await channel.QueueBindAsync("dead_letter_queue", "dead_letter_exchange", routingKey: "");

    //        var consumer = new AsyncEventingBasicConsumer(channel);
    //        consumer.ReceivedAsync += Consumer_ReceivedAsync;


    //        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

    //        await Task.CompletedTask;
    //    }

    //    private async Task<Task> Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs ea)
    //    {
    //        try
    //        {
    //            using var scope = scopeFactory.CreateScope();
    //            var locales = scope.ServiceProvider.GetRequiredService<IRepository<Locale>>();

    //            Locale? en = locales.Where(l => l.LanguageCode == "en-US").Include(l => l.Entries).FirstOrDefault();
    //            if (en == null)
    //                throw new Exception("English locale not found");

    //            int localeId = int.TryParse(Encoding.UTF8.GetString(ea.Body.ToArray()), out int parsedId) ? parsedId : -1;
    //            if (localeId == -1)
    //                throw new Exception("Invalid locale ID received");

    //            Locale? locale = locales.Where(l => l.Id == localeId).Include(l => l.Entries).FirstOrDefault();
    //            if (locale == null)
    //                throw new Exception($"Locale with ID {localeId} not found");

    //            // **Find missing and extra entries**
    //            var missing = en.Entries.Where(e => !locale.Entries.Any(l => l.Name == e.Name)).ToList();
    //            var extra = locale.Entries.Where(l => !en.Entries.Any(e => e.Name == l.Name)).ToList();

    //            // **Remove extra entries**
    //            if (extra.Any())
    //            {
    //                locale.Entries.RemoveAll(e => extra.Contains(e));
    //                locales.SaveChanges();
    //            }

    //            // **Translate and add missing entries**
    //            foreach (var batch in missing.Chunk(50))
    //            {
    //                var messageContent = $"You are a professional translator tasked with translating a mobile application's user interface into the language {locale.LanguageCode} and provide only json output.\n\n";
    //                messageContent += JsonSerializer.Serialize(batch, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    //                var response = await AI.GetResponseAsync(messageContent);
    //                var translated = JsonSerializer.Deserialize<List<LocaleEntry>>(
    //                    response?.Message?.Text?.Replace("```json", "").Replace("```", "") ?? "[]",
    //                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    //                if (translated != null && translated.Any())
    //                {
    //                    locale.Entries.AddRange(translated);
    //                    locales.SaveChanges();
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var channel = ((AsyncEventingBasicConsumer)sender).Channel;
    //            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
    //            return Task.CompletedTask;
    //        }

    //        return Task.CompletedTask;
    //    }

    //}
}
