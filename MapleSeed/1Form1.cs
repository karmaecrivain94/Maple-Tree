﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MaryJane
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Toolbelt.Form1 = this;
        }

        private static List<string> Library { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            MinimumSize = MaximumSize = Size;
            Text += Toolbelt.Version;

            Database.Initialize();

            GetLibrary();

            status.Text = Settings.Instance.TitleDirectory;
        }

        private void GetLibrary()
        {
            Library = new List<string>(Directory.GetDirectories(Toolbelt.Settings.TitleDirectory));

            foreach (var item in Library)
                if (!string.IsNullOrEmpty(item))
                    ListBoxAddItem(new FileInfo(item).Name);
        }

        private void ListBoxAddItem(object obj)
        {
            if (listBox1.InvokeRequired)
                listBox1.BeginInvoke(new Action(() => { listBox1.Items.Add(obj); }));
            else
                listBox1.Items.Add(obj);
        }

        public void UpdateProgress(int percentage, long recvd, long toRecv)
        {
            if (Toolbelt.Form1 != null)
            {
                var pg = Toolbelt.Form1.progressBar;
                pg.Invoke(new Action(() => pg.Value = percentage));
            }

            var received = Toolbelt.SizeSuffix(recvd);
            var toReceive = Toolbelt.SizeSuffix(toRecv);
            SetStatus($"{percentage}% | {received} / {toReceive}");
        }

        public void AppendLog(string msg, Color color = default(Color))
        {
            msg += '\n';
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new Action(() =>
                {
                    richTextBox1.AppendText(msg);
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                richTextBox1.AppendText(msg);
                richTextBox1.ScrollToCaret();
            }
        }

        public void SetStatus(string msg, Color color = default(Color))
        {
            msg += '\n';
            if (status.InvokeRequired)
            {
                status.BeginInvoke(new Action(() =>
                {
                    status.Text = msg;
                    status.ForeColor = color;
                }));
            }
            else
            {
                status.Text = msg;
                status.ForeColor = color;
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string rpx = null, gamePath = null;
            var item = listBox1.SelectedItem as string;

            string[] files = {};

            if (item != null)
                gamePath = Path.Combine(Toolbelt.Settings.TitleDirectory, item);

            if (gamePath != null)
                files = Directory.GetFiles(gamePath, "*.rpx", SearchOption.AllDirectories);

            if (files.Length > 0)
                rpx = files[0];

            var cemuPath = Path.Combine(Settings.Instance.CemuDirectory, "cemu.exe");
            if (File.Exists(cemuPath) && File.Exists(rpx))
            {
                Toolbelt.RunCemu(cemuPath, rpx);
            }
            else
            {
                SetStatus("Could not find a valid .rpx");
            }
        }

        private void fullTitle_CheckedChanged(object sender, EventArgs e)
        {
            updateBtn.Text = fullTitle.Checked ? "Download" : "Update";
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            string fullPath = null, item = listBox1.SelectedItem as string;

            if (item != null)
                fullPath = Path.Combine(Toolbelt.Settings.TitleDirectory, item);

            if (Toolbelt.Database != null)
            {
                var title = Database.Find(item);
                Toolbelt.Database.UpdateGame(title.TitleID, fullPath);
            }

            listBox1.Enabled = false;
        }

        private void fullScreen_CheckedChanged(object sender, EventArgs e)
        {
            Toolbelt.Settings.FullScreenMode = fullScreen.Checked;
        }
    }
}