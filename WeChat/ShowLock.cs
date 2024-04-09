using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenWeChat
{
    public partial class ShowLock : Form
    {
        private Statc fromstatc;
        private string password = null;

        /// <summary>
        /// 窗口锁
        /// </summary>
        /// <param name="from"></param>
        public ShowLock(Statc from, string pwd = "")
        {
            InitializeComponent();
            fromstatc = from;
            password = pwd;
        }

        private void ShowLock_Load(object sender, EventArgs e)
        {
            switch (fromstatc)
            {
                case Statc.setting:
                    label2.Visible = true;
                    txtpwds.Visible = true;
                    button2.Visible = true;
                    pwderr.Visible = false;
                    break;
                case Statc.Lock:
                    label2.Visible = false;
                    txtpwds.Visible = false;
                    button2.Visible = false;
                    pwderr.Visible = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (fromstatc)
            {
                case Statc.setting:
                    if (PasswordCheck() == true)
                    {
                        this.Tag = MD5Encrypt32(txtpwd.Text);
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    break;
                case Statc.Lock:
                    if (MD5Encrypt32(txtpwd.Text) == password)
                    {
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        txtpwd.Text = string.Empty;
                        pwderr.Visible = true;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 密码效验
        /// </summary>
        /// <returns></returns>
        private bool PasswordCheck()
        {
            if (txtpwd.Text != string.Empty && txtpwds.Text != string.Empty)
            {
                if (txtpwd.Text == txtpwds.Text)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("密码不一致");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("密码不能为空");
                return false;
            }
            
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public string MD5Encrypt32(string pwd)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string tmp = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(pwd)));
            return tmp.Replace("-", "");
        }
    }

    public enum Statc
    {
        setting,
        Lock
    }
}
