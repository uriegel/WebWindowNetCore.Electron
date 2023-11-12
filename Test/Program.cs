using WebWindowNetCore;

var sseEventSource = WebView.CreateEventSource<Event>();
StartEvents(sseEventSource.Send);

WebView
    .Create()
    .InitialBounds(600, 800)
    .ResourceIcon("icon")
    .Title("WebView Test")
    .QueryString("?theme=windows")
    .SaveBounds()
    .OnFilesDrop((id, move, paths) => 
    {
        var arg = paths;
    })
    //.DebugUrl("http://localhost:3000")
    //.Url($"file://{Directory.GetCurrentDirectory()}/webroot/index.html")
    .ConfigureHttp(http => http
        .ResourceWebroot("webroot", "/web")
        .UseSse("sse/test", sseEventSource)
        .Build())
#if DEBUG            
    .DebuggingEnabled()
#endif            
    .Build()
    .Run("de.uriegel.WebWindowTest");    

void StartEvents(Action<Event> onChanged)   
{
    var counter = 0;
    new Thread(_ =>
        {
            while (true)
            {
                Thread.Sleep(5000);
                onChanged(new($"Ein Event {counter++}"));
            }
        })
        {
            IsBackground = true
        }.Start();   
}

record Event(string Content);