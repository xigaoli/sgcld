using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace sgcld
{
    
    public partial class Form1 : Form
    {

        #region DllImport
        //设置钩子  
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //抽掉钩子  
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll")]
        //调用下一个钩子  
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);
        [DllImport("User32.dll")]
        public static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, Int32 dwExtraInfo);
        #endregion
        public Form1()
        {
            InitializeComponent();

            Rectangle r = Screen.GetWorkingArea(this);
            this.Location = new Point(r.Right - this.Width, r.Bottom - this.Height);
            //这里是调用：单击按钮2之后 开启钩子监视
            Hook_Start();

            //全局aero
            //如果启用Aero
            if (DwmIsCompositionEnabled())
            {
                Margins m = new Margins();
                DwmExtendFrameIntoClientArea(this.Handle, ref m); //开启全窗体透明效果
            }
        }
        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        static extern bool DwmIsCompositionEnabled(); //Dll 导入 DwmApi

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (DwmIsCompositionEnabled())
            {
                e.Graphics.Clear(Color.Black); //将窗体用黑色填充（Dwm 会把黑色视为透明区域）
            }
        }

        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        static int hHook = 0;
        public const int WH_KEYBOARD_LL = 13;
        HookProc KeyBoardHookProcedure;
        //键盘Hook结构函数  
        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }//函数声明

        #region 自定义事件
        public void Hook_Start()
        {
            // 安装键盘钩子  
            if (hHook == 0)
            {
                KeyBoardHookProcedure = new HookProc(KeyBoardHookProc);
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                          KeyBoardHookProcedure,
                        GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

                //如果设置钩子失败.  
                if (hHook == 0)
                {
                    Hook_Clear();
                    //throw new Exception("设置Hook失败!");  
                }
            }
        }

        //取消钩子事件  
        public void Hook_Clear()
        {
            bool retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
            //如果去掉钩子失败.  

        }

        //这里个函数里边写的是你需要拦截的内容 检测到按键之后要进行的操作
        public int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyBoardHookStruct kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));

                //这里开始判断按键 然后进行操作
                if (kbh.vkCode == (int)Keys.Q && (int)Control.ModifierKeys == (int)Keys.Alt)  //截获alt+Q
                {
                    if (this.Visible == false)
                    {
                        this.Visible = true;
                        Thread.Sleep(10);
                    }
                    else
                    {
                        this.Visible = false;
                        Thread.Sleep(10);
                    }
                    return 1;
                }
                if (kbh.vkCode == (int)Keys.R && (int)Control.ModifierKeys == (int)Keys.Alt)  //截获alt+R
                {
                    this.Close();
                    return 1;
                }


            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        #endregion



    }
}
