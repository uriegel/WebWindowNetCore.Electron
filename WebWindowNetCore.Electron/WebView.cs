using System.Diagnostics;
using System.Text.Json;
using WebWindowNetCore.Base;
using WebWindowNetCore.Data;

namespace WebWindowNetCore;

public class WebView : WebWindowNetCore.Base.WebView
{
    public static WebViewBuilder Create()
        => new WebViewBuilder();

    public override int Run(string gtkId = "")
    {
        var electron = new Process()
        {
            StartInfo = new()
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
#if Linux                
                FileName = "electron",
#else
                FileName = "electron.cmd", 
#endif
                CreateNoWindow = true,
                Arguments = Path.Combine(Directory.GetCurrentDirectory(), "../resources/electron/main.js"),
        },
            EnableRaisingEvents = true
        };

        electron.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        electron.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);
        electron.StartInfo.Environment.Add("StartInfo", JsonSerializer.Serialize(new StartInfo(settings!.Title), JsonDefault.Value));
        electron.Start();
        electron.BeginOutputReadLine();
        electron.BeginErrorReadLine();
        electron.EnableRaisingEvents = true;
        electron.WaitForExit();
        return electron.ExitCode;
    }

    internal WebView(WebViewBuilder builder)
        => settings = builder.Data;

    WebViewSettings? settings;

    bool saveBounds;
}

record StartInfo(
    string Title
);