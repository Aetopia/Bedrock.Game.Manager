using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bedrock.Game.Manager.Deploy;
using Bedrock.Game.Manager.Store;
using static System.Drawing.FontStyle;

namespace Bedrock.Game.Manager.Interface;

sealed class ProductPage : TableLayoutPanel
{
    readonly PictureBox _pictureBox = new()
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.CenterImage
    };

    readonly Button _installButton = new()
    {
        Text = "🡇",
        AutoSize = true,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.TopCenter,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
    };

    readonly Button _cancelButton = new()
    {
        Text = "✖",
        Margin = new(),
        AutoSize = true,
        Enabled = false,
        Dock = DockStyle.Fill,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
    };

    readonly Label _statusLabel = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        Anchor = AnchorStyles.None,
    };

    readonly ProgressBar _progressBar = new()
    {
        Margin = new(),
        Dock = DockStyle.Fill,
    };

    readonly TableLayoutPanel _tableLayoutPanel = new()
    {
        Visible = false,
        AutoSize = true,
        Dock = DockStyle.Fill,
        AutoSizeMode = AutoSizeMode.GrowAndShrink
    };

    readonly ProductInfo _minecraftGame;
    readonly ProductInfo _gamingServices;
    readonly Progress<(string, int)> _progress;

    InstallRequest<Progress<(string, int)>>? _request;

    const int ERROR_INSTALL_ALREADY_RUNNING = 0x652;
    static readonly string s_message = new Win32Exception(ERROR_INSTALL_ALREADY_RUNNING).Message;

    internal ProductPage(Form form, ProductInfo minecraftGame, ProductInfo gamingServices)
    {
        _progress = new(OnProgress);
        _minecraftGame = minecraftGame;
        _gamingServices = gamingServices;

        var cancelButtonFont = _cancelButton.Font;
        var installButtonFont = _installButton.Font;

        _cancelButton.Font = new(cancelButtonFont.FontFamily, cancelButtonFont.Size * 1.75F, Bold);
        _installButton.Font = new(installButtonFont.FontFamily, installButtonFont.Size * 1.75F, Underline | Bold);

        AutoSize = true;
        Dock = DockStyle.Fill;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });
        RowStyles.Add(new() { SizeType = SizeType.AutoSize, Height = 0 });

        Text = _minecraftGame.Title;

        _tableLayoutPanel.ColumnStyles.Add(new() { SizeType = SizeType.Percent, Width = 100 });
        _tableLayoutPanel.ColumnStyles.Add(new() { SizeType = SizeType.AutoSize, Width = 0 });
        _tableLayoutPanel.ColumnStyles.Add(new() { SizeType = SizeType.AutoSize, Width = 0 });

        _tableLayoutPanel.Controls.Add(_progressBar, 0, 0);
        _tableLayoutPanel.Controls.Add(_cancelButton, 1, 0);

        Controls.Add(_pictureBox, 0, 0);
        Controls.Add(_statusLabel, 0, 1);

        Controls.Add(_installButton, 0, 2);
        Controls.Add(_tableLayoutPanel, 0, 2);

        if (_minecraftGame.Image is { } image)
            _pictureBox.LoadAsync($"{image.Uri}");

        _cancelButton.Click += OnCancel;
        _installButton.Click += OnInstall;

        form.FormClosing += OnFormClosing;
    }

    bool IsInstalling
    {
        get;
        set
        {
            field = value;

            _statusLabel.Text = null;
            _installButton.Visible = !value;
            _tableLayoutPanel.Visible = value;

            _progressBar.Value = 0;
            _cancelButton.Enabled = false;
        }
    }

    void OnProgress((string, int) args)
    {
        if (_progressBar.Value != args.Item2)
        {
            _progressBar.Value = args.Item2;
            _statusLabel.Text = $"{args.Item1} ~ {args.Item2}%";
        }
    }

    async void OnInstall(object? sender, EventArgs args)
    {
        IsInstalling = true; try
        {
            foreach (var product in (IEnumerable<ProductInfo>)[_gamingServices, _minecraftGame])
            {
                IsInstalling = true; try
                {
                    _statusLabel.Text = $"{product.Title} ~ ?";
                    _request = await InstallManager.InstallAsync(product, _progress);
 
                    if (_request is null)
                        continue;

                    _cancelButton.Enabled = true;

                    if (!await _request.Task)
                        break;
                }
                finally
                {
                    IsInstalling = true;
                    _request = null;
                }
            }
        }
        finally { IsInstalling = false; }
    }

    void OnCancel(object? sender, EventArgs args)
    {
        _request?.Cancel();
    }

    void OnFormClosing(object? sender, FormClosingEventArgs args)
    {
        if (!args.Cancel && IsInstalling)
        {
            args.Cancel = true;
            MessageBox.Show(s_message, _minecraftGame.Title);
        }
    }

    public override string ToString() => _minecraftGame.Title;
}