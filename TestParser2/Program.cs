using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TestParser2
{
    class Program
    {
        private static string token { get; set; } = "";//Токен бота
        private static long adminId { get; set; } = 1;//id пользователя, которому бот будет отсылать результаты
        private static string loginLk { get; set; } = ""; //Логин от личного кабинета
        private static string passwordLk { get; set; } = ""; //Пароль от личного кабинета


        private static TelegramBotClient client;
        static void Main(string[] args)
        {

            //Переменные, для отслеживания изменений в расписании.
            string NewString = "";
            string curent = "";
            ////
            var date = DateTime.Now.ToString("mm.ss");
            while (true)
            {
                date = DateTime.Now.ToString("mm.ss");
                if (date == "30.01" || date == "00.01") //Каждые 30 минут делает запрос
                {
                    NewString = GetSchedule().Result;
                    if (NewString != curent)
                    {
                        curent = NewString;

                        //бот ТГ
                        client = new TelegramBotClient(token);
                        client.SendTextMessageAsync(
                                        chatId: adminId,
                                        text: $"{curent}");
                    }
                }
            }
        }

        public static async Task<string> GetSchedule()
        {
            HttpClient h = new HttpClient();

            //Указываем параметры для браузера
            var values = new Dictionary<string, string>
            {
                { "LoginForm[username]", loginLk },
                { "LoginForm[password]", passwordLk },
                { "LoginForm[rememberMe]", "1"},
                { "login-button:", ""}
            };
            h.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            h.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,ru;q=0.8");
            h.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            h.DefaultRequestHeaders.Add("Connection", "keep-alive");
            h.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            h.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            h.DefaultRequestHeaders.Add("Origin", "https://lk.samgk.ru");
            h.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Mobile Safari/537.36");
            h.DefaultRequestHeaders.Add("Referer", "https://lk.samgk.ru/");
            h.DefaultRequestHeaders.Add("Host", "lk.samgk.ru");
            h.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-1");

            //Открываем страницу https://lk.samgk.ru/ с указанными параметрами 
            var content = new FormUrlEncodedContent(values);
            var response = await h.PostAsync("https://lk.samgk.ru/", content);

            //Открывает страницу с расписанием
            response = await h.GetAsync("https://lk.samgk.ru/user/account/schedule");
            var responseString = await response.Content.ReadAsStringAsync();

            //Получаем расписание на следующий день
            var web = new HtmlAgilityPack.HtmlDocument();
            web.LoadHtml(responseString);

            //Дата расписания
            var date = web.DocumentNode.SelectSingleNode("//div[@class='wrap']/div[@id='app']/div[@class='account-wrap']/div/div[@class='right-panel']//section[@id='schedule']/div[@class='frame no-mobile']/ul/li[2]/div[@class='sheduleTitle']").InnerText;
            //Само расписание,
            var s = web.DocumentNode.SelectSingleNode("//div[@class='wrap']/div[@id='app']/div[@class='account-wrap']/div/div[@class='right-panel']//section[@id='schedule']/div[@class='frame no-mobile']/ul/li[2]/div[@class='lesson-list']");
            
            if (s != null)
            {
                //Преобразуем страницу в список
                var html = s
                .SelectNodes(".//div[@class='lesson-item']");

                var subject = new List<string>();
                foreach (var item in html) 
                {
                    //Console.WriteLine(Regex.Replace(item.InnerText, @"\s+", " "));
                    subject.Add(Regex.Replace(item.InnerText, @"\s+", " "));
                }

                var result = $"{date}\n";
                foreach(var item in subject)
                {
                    result += $"\n{item}";
                }
                return result;
            }
            else
            {
                //Console.WriteLine(s.InnerText.Trim());
                return s.InnerText.Trim();
            }
        }
    }
}
