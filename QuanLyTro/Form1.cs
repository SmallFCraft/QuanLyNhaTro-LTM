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
                await _bridge.DispatchAsync(rawJson, PostToPage);
            }
        };

        var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot", "auth", "index.html");
        webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
    }

    private Task PostToPage(string msg)
    {
        // Form/webView có thể đã bị huỷ khi phản hồi TCP về muộn → bỏ qua im lặng,
        // không để exception trên luồng nền giết tiến trình.
        try
        {
            if (IsDisposed || Disposing || !IsHandleCreated || webView.IsDisposed) return Task.CompletedTask;
            webView.Invoke(() =>
            {
                if (webView.IsDisposed || webView.CoreWebView2 is null) return;
                webView.CoreWebView2.PostWebMessageAsString(msg);
            });
        }
        catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
        {
            // Teardown đua với phản hồi — chấp nhận rơi gói tin.
        }

        return Task.CompletedTask;
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        try
        {
            BeginInvoke(() =>
            {
                if (IsDisposed || webView.IsDisposed) return;
                _ = webView.CoreWebView2?.ExecuteScriptAsync("alert('Mất kết nối máy chủ TCP. Vui lòng thử lại.');");
            });
        }
        catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
        {
            // Cửa sổ đang đóng — không cần thông báo.
        }
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
