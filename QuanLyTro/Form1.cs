using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using QuanLyTro.Network;

namespace QuanLyTro;

public partial class Form1 : Form
{
    public static TcpClientService Client { get; } = new();
    private readonly WebMessageBridge _bridge;

    public Form1()
    {
        InitializeComponent();
        _bridge = new WebMessageBridge(Client);
        Load += Form1_Load;
        Client.Disconnected += OnClientDisconnected;
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        var (host, port) = ReadServerEndpoint();
        try
        {
            if (!Client.IsConnected)
            {
                await Client.ConnectAsync(host, port);
            }
        }
        catch
        {
            // Cho phép mở UI, JS sẽ báo khi bấm login
        }

        await InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        await webView.EnsureCoreWebView2Async();
        webView.CoreWebView2.WebMessageReceived += async (_, args) =>
        {
            var rawJson = args.TryGetWebMessageAsString();
            if (!string.IsNullOrEmpty(rawJson))
            {
                await _bridge.DispatchAsync(rawJson, msg =>
                {
                    webView.Invoke(() => webView.CoreWebView2.PostWebMessageAsString(msg));
                    return Task.CompletedTask;
                });
            }
        };

        var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot", "index.html");
        webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        BeginInvoke(() =>
        {
            _ = webView.CoreWebView2?.ExecuteScriptAsync("alert('Mất kết nối máy chủ TCP. Vui lòng thử lại.');");
        });
    }

    private static (string Host, int Port) ReadServerEndpoint()
    {
        var host = ConfigurationManager.AppSettings["ServerHost"] ?? "127.0.0.1";
        var port = int.TryParse(ConfigurationManager.AppSettings["ServerPort"], out var p) ? p : 8888;
        return (host, port);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Client.Disconnected -= OnClientDisconnected;
        Client.Dispose();
        base.OnFormClosed(e);
    }
}
