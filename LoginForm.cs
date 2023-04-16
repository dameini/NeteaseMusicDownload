using System;
using System.Threading.Tasks;
using Sunny.UI;
using NeteaseMusicDownloadWinForm.Utility;
using System.Threading;

namespace NeteaseMusicDownloadWinForm
{
    public partial class LoginForm : UIForm
    {
        //完成登录委托
        public delegate Task FinishLogin();
        //完成登录事件
        public event FinishLogin FinishLoginEvent;
        public LoginForm()
        {
            InitializeComponent();
        }
        //关闭窗口
        private void CloseLoginForm(object sender, EventArgs e)
        {
            this.Close();
        }
        //刷新二维码按钮
        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await RefreshQrCode();
            await MonitorQrCode();
        }
        private async void LoginForm_Load(object sender, EventArgs e)
        {
                await RefreshQrCode();
                await MonitorQrCode();
        }
        //刷新二维码
        private async Task RefreshQrCode()
        {
            //第一步生成二维码
            string uniKey = null;
            //异步执行，防止界面卡死
            
            await Task.Run(async () =>
            {
                //网易云有时候会返回null，尝试获取10次，10次都获取不了就显示加载二维码失败
                int i = 0;
                while (uniKey == null & i < 10)
                {
                    uniKey = await Login.GetLoginKey();
                    i++;
                }
            });
            if (uniKey != null)
            {
                string qrCodeContent = "http://music.163.com/login?codekey=" + uniKey;
                uiImageButton1.Image = Login.GenerateQrCode(qrCodeContent, 400, 400);
                refreshButton.Text = "等待扫码";
            }
            else
            {
                refreshButton.Text = "获取二维码失败";
            }

        }
        //监控二维码状态
        private async Task MonitorQrCode()
        {
            //获取不到二维码就没必要继续执行啦
            if (refreshButton.Text == "获取二维码失败")
            {
                return;
            }
            //第二步监控二维码的状态
            string qrCodeStatus = "等待扫码";
            //异步执行，防止界面卡死
            await Task.Run(async () =>
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    //判断是否登录成功或者二维码失效
                    if (refreshButton.Text == "授权登陆成功")
                    {
                        break; ;
                    }
                    if (refreshButton.Text == "二维码不存在或已过期")
                    {
                        break;
                    }
                    qrCodeStatus = await Login.CheckQrCodeStatus();
                    //null，是因为有时候网易云会返回null，json就会解析失败，然后我设置返回null
                    if (qrCodeStatus == null)
                    {
                        continue;
                    }
                    //控件没有创建不能调用invoke，不然会报错
                    if (!refreshButton.Created)
                    {
                        break;
                    }
                    //修改控件内容
                    refreshButton.Invoke(new EventHandler(delegate
                    {
                        refreshButton.Text = qrCodeStatus;
                    }));
                }
            });
            //登录成功关闭子窗口，登录失败（二维码失效）refreshButton启用
            if (refreshButton.Text == "授权登陆成功")
            {
                //保存cookie
                Login.SaveLoginCookie();
                //执行登录成功事件
                await FinishLoginEvent();
                //关闭窗口
                this.Close();
            }
            else
            {
                //没有登录成功，刷新二维码按钮可用
                refreshButton.Enabled = true;
            }
        }
    }
}
