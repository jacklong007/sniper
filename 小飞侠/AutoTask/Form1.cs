using CsharpHttpHelper;
using CsharpHttpHelper.CsharpHttpHelper.Helper;
using mshtml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;


namespace AutoTask
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> cookies;
        string currentCookieKey;
        DateTime completeDate = DateTime.MinValue;
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;          
            LoadConfig();
            StartTask();
        }

        void LoadConfig()
        {
            string path = Environment.CurrentDirectory + @"\UserConfig.ini";
            cookies = INIHelper.GetAllKeyValues("Cookies", path);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        void GetTaskPoint()
        {
            HttpHelper http = new HttpHelper();
            var cookie = cookies[currentCookieKey];
            var token = getCookie(cookie, "cvl_csrf_token");
            string[] aliArr = new string[] { "https://www.douyu.com/japi/carnival/nc/actTask/takePrize_POST_2020CFPLS16_2watch10", "https://www.douyu.com/japi/carnival/nc/actTask/takePrize_POST_2020CFPLS16_danmu"};


            for (int i = 0; i < aliArr.Length; i++)
            {
                var items = aliArr[i].Split('_');

                http.GetHtml(new HttpItem()
                {
                    URL = items[0],
                    Method = "POST",
                    Postdata = String.Format(items[2],token),
                    Cookie= cookie
                });
                Thread.Sleep(1000 * 3);
            }


        }


        public string getCookie(string cookiesString, string cookieName)
        {
            
            return Regex.Match(cookiesString, "(^| )" + cookieName + "=([^;]*)(;|$)").Value;

        }


        void StartTask()
        {
            var th1 = new Thread(new ThreadStart(() =>
              {

                  while (true)
                  {


                      if (!(DateTime.Now.Hour == 0 && DateTime.Now.Hour < 1) || completeDate == DateTime.MinValue)
                      {
                          return;
                      }
                      if (completeDate == DateTime.Now.Date)
                          return;

                      //设置cookie，刷新页面，过几分钟发弹幕，一小时后领取积分，继续下一个
                      foreach (var keyvalue in cookies)
                      {
                          ClearCookie();
                          var cookie = keyvalue.Value;
                          currentCookieKey = keyvalue.Key;
                          InternetSetCookie("https://www.douyu.com/", null, cookie);

                          webBrowser1.Refresh();
                          webBrowser2.Refresh();
                          Thread.Sleep(5 * 1000);

                          this.Invoke(new Action(() =>
                          {
                              var doc = webBrowser1.Document.DomDocument as IHTMLDocument2;
                              var win = doc.parentWindow as IHTMLWindow2;

                              string jscode = @" function sendComment(){for(var i=1;i<=5;i++){var span1=i*1000;setTimeout(function (){document.querySelector('.ChatSend-txt').value='白鲨';document.querySelector('.ChatSend-button').click()},span1);} setTimeout(function(){var eles=document.querySelectorAll('.wmTaskV3GiftBtn-btn'),for(var index in eles){eles[index].click();}},1000*60*63) };sendComment();";

                              HtmlElement head = webBrowser1.Document.GetElementsByTagName("head")[0];
                              //创建script标签
                              HtmlElement scriptEl = webBrowser1.Document.CreateElement("script");
                              IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
                              //给script标签加js内容
                              element.text = jscode;
                              //将script标签添加到head标签中
                              head.AppendChild(scriptEl);
                              //执行js代码
                              webBrowser1.Document.InvokeScript("sendComment");

                              //win.execScript(jscode, "javascript");

                          }));

                        
                          //发弹幕

                          //领积分
                          Thread.Sleep(1000 * 30);//60 * 70
                          GetTaskPoint();

                          completeDate = DateTime.Now.Date;
                      }
                      Thread.Sleep(10 * 1000);
                  }


              }));
            th1.Start();
        }

        void ClearCookie()
        {

            if (webBrowser1.InvokeRequired)
            {

                Invoke(new Action<string>(s =>
                {

                    HtmlDocument document = webBrowser1.Document;
                    document.ExecCommand("ClearAuthenticationCache", false, null);
                    webBrowser2.Document.ExecCommand("ClearAuthenticationCache", false, null);

                }),"");


            }

        }

        void SetCookies(string url, string cookieStr)
        {
            foreach (string c in cookieStr.Split(';'))
            {
                string[] item = c.Split('=');
                if (item.Length == 2)
                {
                    InternetSetCookie(url, null, new Cookie(HttpUtility.UrlEncode(item[0]).Replace("+", ""), HttpUtility.UrlEncode(item[1])).ToString());
                }
            }

        }


        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

    }
}
