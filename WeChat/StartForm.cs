using OpenWeChat;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WeChat
{
    public partial class StartForm : Form
    {
        #region DLL引用

        public void INIWrite(string key, string value, string section = "WeChat")
        {
            DLL.WriteINI(section, key, value, PATH);
        }

        public string INIRead(string key, string section = "WeChat")
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);
            DLL.GetINI(section, key, "", temp, 255, PATH);
            return temp.ToString();
        }

        #endregion DLL引用

        #region INIT

        private SystemDLL DLL = new SystemDLL();
        private string CmdPath = string.Empty;
        private string PATH = string.Empty;
        ShowLock showLock = null;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams paras = base.CreateParams;
                paras.ExStyle |= 0x02000000;
                return paras;
            }
        }

        #endregion INIT

        public StartForm()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CmdPath = Environment.ExpandEnvironmentVariables("%systemdrive%") + @"\Windows\System32\cmd.exe";

            PATH = Environment.CurrentDirectory + "\\wechatconfig.ini";
            if (File.Exists(PATH))
            {
                try
                {
                    toolStripComboBox1.SelectedIndex = Convert.ToInt32(INIRead("OpenCount")) - 1;
                    if (INIRead("Width") != string.Empty && INIRead("Height") != string.Empty)
                    {
                        try
                        {
                            Size = new Size(Convert.ToInt32(INIRead("Width")), Convert.ToInt32(INIRead("Height")));
                        }
                        catch (Exception)
                        {
                            INIWrite("Width", "1000");
                            INIWrite("Height", "600");
                        }
                    }
                }
                catch (Exception)
                {
                    File.Delete(PATH);
                }
            }
        }

        /// <summary>
        /// 菜单启动
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

            ShowLogin login = new ShowLogin();
            if (login.ShowDialog() == DialogResult.Yes)
            {
                Process[] process = Process.GetProcessesByName("WeChat");
                if (process.Length > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        DLL.SetParentWindow(process[i].MainWindowHandle, tabControl1.TabPages[i].Handle);
                        DLL.SetShowWindow(process[i].MainWindowHandle, (int)ProcessWindowStyle.Maximized);
                    }
                }
            }
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

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 设置锁定密码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showLock == null)
            {
                showLock = new ShowLock(Statc.setting);
                if (showLock.ShowDialog() == DialogResult.OK)
                {
                    INIWrite("Password", showLock.Tag.ToString());

                    showLock.Dispose();
                    showLock = null;
                }
            }
            
        }

        /// <summary>
        /// 菜单锁定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 锁定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(PATH))
            {
                if (INIRead("Password") != string.Empty)
                {
                    panel1.Visible = false;

                    if (showLock == null)
                    {
                        showLock = new ShowLock(Statc.Lock, INIRead("Password"));
                        showLock.StartPosition = FormStartPosition.CenterScreen;
                        showLock.Show(this);
                        new Thread(() =>
                        {
                            while (true)
                            {
                                if (showLock.DialogResult == DialogResult.OK)
                                {
                                    Invoke(new MethodInvoker(delegate
                                    {
                                        panel1.Visible = true;
                                    }));

                                    showLock.Dispose();
                                    showLock = null;
                                    break;
                                }
                                Thread.Sleep(1000);
                            }
                        }).Start();
                    }
                }
                else
                {
                    MessageBox.Show("还未设置锁定密码");
                    设置锁定密码ToolStripMenuItem_Click(null, null);
                }
            }
        }

        /// <summary>
        /// 禁用关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                this.Dispose();
                this.Close();
                Environment.Exit(0);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// task click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;

                if (showLock != null)
                {
                    showLock.WindowState = FormWindowState.Normal;
                    showLock.Focus();
                }
            }
            else
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        /// <summary>
        /// SIZE SAVE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartForm_SizeChanged(object sender, EventArgs e)
        {
            INIWrite("Width", Width.ToString());
            INIWrite("Height", Height.ToString());
        }

        /// <summary>
        /// MIn FORM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartForm_MinimumSizeChanged(object sender, EventArgs e)
        {
            if (showLock != null)
            {
                showLock.WindowState = FormWindowState.Minimized;
            }
        }
    }

    public class SystemDLL
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

        public void SetParentWindow(IntPtr hwnd, IntPtr Parent)
        {
            SetParent(hwnd, Parent);
        }

        public void SetShowWindow(IntPtr hwnd, int nCmdShow)
        {
            ShowWindow(hwnd, nCmdShow);
        }

        public void WriteINI(string s, string key, string val, string file)
        {
            WritePrivateProfileString(s, key, val, file);
        }

        public void GetINI(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath)
        {
            GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }
    }
}