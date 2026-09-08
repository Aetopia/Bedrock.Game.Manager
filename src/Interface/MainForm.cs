using System;
using System.Reflection;
using System.Windows.Forms;
using Bedrock.Game.Manager.Store;

namespace Bedrock.Game.Manager.Interface;

sealed class MainForm : Form
{
    static readonly ComboBox _comboBox = new()
    {
        Sorted = true,
        Enabled = false,
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    static readonly Panel _panel = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
    };

    readonly TableLayoutPanel _tableLayoutPanel = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
    };

    readonly ProgressBar _progressBar = new()
    {
        Height = 1,
        Margin = new(),
        Dock = DockStyle.Bottom,
        MarqueeAnimationSpeed = 1,
        Style = ProgressBarStyle.Marquee
    };

    internal MainForm()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString(3);
        
        if (assembly.GetManifestResourceStream("Application.ico") is { } stream)
            using (stream) Icon = new(stream);

        Text = $"Bedrock Game Manager ~ {version}";

        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new(960, 540);

        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(_tableLayoutPanel);

        _tableLayoutPanel.RowStyles.Add(new() { SizeType = SizeType.AutoSize, Height = 0 });
        _tableLayoutPanel.RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });
        _tableLayoutPanel.RowStyles.Add(new() { SizeType = SizeType.AutoSize, Height = 0 });

        _tableLayoutPanel.Controls.Add(_comboBox, 0, 0);
        _tableLayoutPanel.Controls.Add(_panel, 0, 1);
        _tableLayoutPanel.Controls.Add(_progressBar, 0, 2);

        _comboBox.SelectedIndexChanged += OnSelectedIndexChanged;
        Load += OnLoad;
    }

    void OnSelectedIndexChanged(object? sender, EventArgs args)
    {
        var index = _comboBox.SelectedIndex;
        var control = (Control?)_comboBox.Items[index];

        _panel.Controls.Clear();
        _panel.Controls.Add(control);
    }

    async void OnLoad(object? sender, EventArgs args)
    {
        Load -= OnLoad;

        var gamingServices = ProductInfo.GetAsync(ProductId.GamingServices);
        var minecraftGames = ProductInfo.GetAsync(ProductId.s_minecraftGames);

        await foreach (var minecraftGame in minecraftGames)
        {
            var empty = _comboBox.Items.Count is 0;
            ProductPage page = new(this, minecraftGame, await gamingServices);

            _comboBox.Items.Add(page);

            if (empty)
            {
                _comboBox.SelectedIndex = 0;
                _comboBox.Enabled = true;
            }
        }

        _progressBar.Visible = false;
    }
}