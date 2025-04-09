using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

class Program
{
    private static readonly string token = "7403287268:AAGAR7kFsl6-pUVIlXJNzo1tvilettPhBrU";
    private static readonly long adminId = 7502335499; 
    private static ITelegramBotClient botClient = new TelegramBotClient(token);

    static async Task Main()
    {
        using var cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"✅ Bot ishlayapti: {me.FirstName}");

        Console.ReadLine();
        cts.Cancel();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } messageText) return;

        long userId = message.Chat.Id;

        // ✅ Agar foydalanuvchi yozsa -> Admin (sizga) yuboramiz
        if (userId != adminId)
        {
            string forwardMessage = $"📩 *Yangi xabar*:\n👤 *Foydalanuvchi:* {message.Chat.FirstName} (ID: {userId})\n\n📜 *Xabar:* {messageText}";

            await bot.SendTextMessageAsync(
                chatId: adminId,
                text: forwardMessage,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );

            Console.WriteLine($"📩 Yangi xabar foydalanuvchidan: {messageText} (ID: {userId})");
        }
        // ✅ Agar admin reply bersa -> Foydalanuvchiga qaytaramiz
        else if (message.ReplyToMessage != null)
        {
            string originalText = message.ReplyToMessage.Text;
            long targetUserId = ExtractUserId(originalText);

            if (targetUserId != 0)
            {
                await bot.SendTextMessageAsync(
                    chatId: targetUserId,
                    text: $"📩 *Admindan javob:*\n\n{messageText}",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"✅ Admin xabari foydalanuvchiga yuborildi: {messageText} (ID: {targetUserId})");
            }
            else
            {
                Console.WriteLine($"❌ Xatolik: Foydalanuvchi ID'si olinmadi! Xabarning asl formati:\n{originalText}");
            }
        }
    }

    // ✅ Reply qilingan xabardan foydalanuvchi ID'sini ajratish
    static long ExtractUserId(string messageText)
    {
        try
        {
            Console.WriteLine($"🔍 ID ajratish uchun xabar: {messageText}");

            // ✅ ID formatiga mos bo'lgan raqamlarni ajratib olish
            Match match = Regex.Match(messageText, @"ID:\s*(\d+)");
            if (match.Success && long.TryParse(match.Groups[1].Value, out long userId))
            {
                Console.WriteLine($"✅ Foydalanuvchi ID aniqlandi: {userId}");
                return userId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ID ajratishda xatolik: {ex.Message}");
        }
        return 0;
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Xatolik: {exception.Message}");
        return Task.CompletedTask;
    }
}
