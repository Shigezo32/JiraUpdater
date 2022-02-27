using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace JiraUpdater;

public class Program
{
    private const string KeyName = "課題キー";

    private static string _userName = "";
    private static string _userPassword = "";

    static async Task Main(string[] args)
    {
        var files = System.Environment.GetCommandLineArgs();

        if (files.Length == 1)
        {
            Console.WriteLine("CSV ファイルをドロップしてください。");
            var path = Console.ReadLine();

            foreach (var item in CreateBody(path))
            {
                Console.WriteLine(item);
            }
        }
    }

    private static IEnumerable<(string key, string body)> CreateBody(string path)
    {
        var lines = File.ReadLines(path);

        var headers = CreateHeaders(lines.First());

        foreach (var row in lines.Skip(1))
        {
            var jObject = new JObject();
            var key = "";

            var items = row.Split(',');
            for (int i = 0; i < items.Length; i++)
            {
                var header = ConvertPhysicalName(headers[i]);
                if (string.IsNullOrWhiteSpace(header)) { continue; }
                if (header == KeyName)
                {
                    key = header;
                    continue;
                }
                jObject.Add(header, items[i]);
            }

            if (!string.IsNullOrWhiteSpace(key) && jObject.Count > 0)
            {
                yield return (key, jObject.ToString());
            }
        }

    }

    private static Dictionary<int, string> CreateHeaders(string header)
    {
        var headers = header.Split(',');

        var map = new Dictionary<int, string>();
        for (int i = 0; i < headers.Length; i++)
        {
            map.Add(i, headers[i]);
        }

        return map;
    }

    private static string ConvertPhysicalName(string logicalName)
    {
        return logicalName switch
        {
            "課題キー" => "課題キー",
            "bbb" => "123",
            "bsbb" => "1",
            "d" => "3",
            _ => null,
        };
    }

    private async Task UpdateIssueAsync(string key, string body)
    {
        var url = $"https://aaa/{key}";
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _userName, _userPassword))));
        request.Content = new StringContent(body, Encoding.UTF8, @"application/json");

        var client = new HttpClient();

        Console.WriteLine($"{key} を更新");
        Console.WriteLine(body);

        var response = await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            Console.WriteLine("成功");
        }
    }
}





