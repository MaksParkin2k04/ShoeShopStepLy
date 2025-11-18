using System.Net;
using System.Text;

public class WebServer
{
    private readonly HttpListener _listener;
    private readonly string _wwwroot;

    public WebServer(string prefix, string wwwroot)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _wwwroot = wwwroot;
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"🌐 Web server started at {_listener.Prefixes.First()}");

        while (_listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequest(context));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Web server error: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            var path = request.Url?.AbsolutePath ?? "/";
            if (path == "/") path = "/miniapp.html";

            var filePath = Path.Combine(_wwwroot, path.TrimStart('/'));

            if (File.Exists(filePath))
            {
                var content = await File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(Path.GetExtension(filePath));

                response.ContentType = contentType;
                response.ContentLength64 = content.Length;
                response.Headers.Add("Access-Control-Allow-Origin", "*");

                await response.OutputStream.WriteAsync(content);
            }
            else
            {
                response.StatusCode = 404;
                var notFound = Encoding.UTF8.GetBytes("404 Not Found");
                await response.OutputStream.WriteAsync(notFound);
            }

            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Request error: {ex.Message}");
        }
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            _ => "text/plain"
        };
    }

    public void Stop()
    {
        _listener?.Stop();
    }
}