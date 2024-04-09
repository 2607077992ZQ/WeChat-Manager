using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WeChat
{
    public partial class StartForm : Form
    {
        #region systemDLL

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        #endregion systemDLL

        #region INIT

        private static string CmdPath = @"C:\Windows\System32\cmd.exe";
        private string PATH = string.Empty;
        private Thread thread;

        #endregion INIT

        public StartForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PATH = Environment.CurrentDirectory + "\\wechatconfig.ini";
            if (File.Exists(PATH))
            {
                try
                {
                    toolStripComboBox1.SelectedIndex = Convert.ToInt32(INIRead("OpenCount")) - 1;
                }
                catch (Exception)
                {
                    File.Delete(PATH);
                }
            }
        }

        #region DLL引用

        public void INIWrite(string key, string value, string section = "WeChat")
        {
            WritePrivateProfileString(section, key, value, PATH);
        }

        public string INIRead(string key, string section = "WeChat")
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, PATH);
            return temp.ToString();
        }

        #endregion

        /// <summary>
        /// start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (File.Exists(PATH))
            {
                if (INIRead("OpenWeChatPath") != string.Empty && INIRead("OpenCount") != string.Empty)
                {
                    if (!INIRead("OpenWeChatPath").Contains("WeChat"))
                    {
                        toolStripMenuItem2_Click(null, null);
                    }

                    try
                    {
                        int count = 0;
                        count = Convert.ToInt32(INIRead("OpenCount"));

                        Process[] process = Process.GetProcessesByName("WeChat");
                        foreach (var item in process)
                        {
                            //kill wechat
                            item.Kill();
                        }

                        StartWeChat(INIRead("OpenWeChatPath"), count);

                    }
                    catch (Exception)
                    {
                        INIWrite("OpenCount", "1");
                    }
                }
            }
            else
            {
                MessageBox.Show("还未设置启动环境");
            }

        }

        /// <summary>
        /// start
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        private void StartWeChat(string path, int count)
        {
            tabControl1.TabPages.Clear();
            for (int i = 0; i < count; i++)
            {
                tabControl1.TabPages.Add(new TabPage
                {
                    Text = $"账号{i + 1}"
                });
            }

            thread = new Thread(() =>
            {
                string cmd = "start \"\" ";
                for (int i = 0; i < count; i++)
                {
                    cmd += $"\"{path}\"&";
                }
                cmd += "exit";

                using (Process p = new Process())
                {
                    p.StartInfo.FileName = CmdPath;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();

                    p.StandardInput.WriteLine(cmd);
                    p.StandardInput.AutoFlush = true;

                    //string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    p.Close();
                }

                if (MessageBox.Show("请先全部登录\r\n完成后在点击\"是\"", "info", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process[] process = Process.GetProcessesByName("WeChat");
                    for (int i = 0; i < count; i++)
                    {
                        Invoke(new MethodInvoker(delegate
                        {
                            SetParent(process[i].MainWindowHandle, tabControl1.TabPages[i].Handle);
                            ShowWindow(process[i].MainWindowHandle, (int)ProcessWindowStyle.Maximized);
                        }));
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// wechat path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Files (*.exe)|*.exe"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                bool flag = true;
                if (File.Exists(PATH))
                {
                    flag = false;
                }

                if (path.Contains("WeChat"))
                {
                    INIWrite("OpenWeChatPath", path);
                    if (flag) INIWrite("OpenCount", "1");
                }
            }
        }

        /// <summary>
        /// count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            INIWrite("OpenCount", (toolStripComboBox1.SelectedIndex + 1).ToString());
        }
    }
}