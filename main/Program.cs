using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace FakelordApp
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll")]
        static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll")]
        static extern bool SwapBuffers(IntPtr hdc);

        [DllImport("opengl32.dll")]
        static extern IntPtr wglCreateContext(IntPtr hdc);

        [DllImport("opengl32.dll")]
        static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [DllImport("opengl32.dll")]
        static extern void glClearColor(float red, float green, float blue, float alpha);

        [DllImport("opengl32.dll")]
        static extern void glClear(uint mask);

        [DllImport("opengl32.dll")]
        static extern void glBegin(uint mode);

        [DllImport("opengl32.dll")]
        static extern void glEnd();

        [DllImport("opengl32.dll")]
        static extern void glColor3f(float red, float green, float blue);

        [DllImport("opengl32.dll")]
        static extern void glVertex3f(float x, float y, float z);

        [DllImport("opengl32.dll")]
        static extern void glRotatef(float angle, float x, float y, float z);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string? lpModuleName);

        delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct WNDCLASS
        {
            public uint style;
            public WndProcDelegate lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
        }

        private static WndProcDelegate? staticWndProc;

        static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            const uint WM_DESTROY = 0x0002;
            if (msg == WM_DESTROY)
            {
                PostQuitMessage(0);
                return IntPtr.Zero;
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        static void Main(string[] args)
        {
            string currentExePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            string currentExeName = Path.GetFileName(currentExePath);

            // Eğer csgo.exe olarak çalışmıyorsa kendini csgo.exe yapıp başlatır
            if (!currentExeName.Equals("csgo.exe", StringComparison.OrdinalIgnoreCase))
            {
                string targetDir = AppDomain.CurrentDomain.BaseDirectory;
                string csgoExePath = Path.Combine(targetDir, "csgo.exe");

                File.Copy(currentExePath, csgoExePath, true);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] 'csgo.exe' oluşturuldu ve gerçek 3D OpenGL penceresi başlatılıyor...");
                Console.ResetColor();

                Process.Start(new ProcessStartInfo
                {
                    FileName = csgoExePath,
                    UseShellExecute = true
                });
                return;
            }

            // --- BURASI ARTIK GERÇEK csgo.exe OLARAK ÇALIŞIYOR ---
            RunCSGOWindow();
        }

        static void RunCSGOWindow()
        {
            // SADECE standart boş Win32 penceresi (3D YOK, OpenGL YOK, SwapBuffers YOK)
            string className = "SimpleTestWindow";
            string windowTitle = "";

            staticWndProc = CustomWndProc;

            WNDCLASS wc = new WNDCLASS
            {
                lpfnWndProc = staticWndProc,
                hInstance = GetModuleHandle(null),
                lpszClassName = className
            };

            RegisterClass(ref wc);

            // PENCEREYİ TAMAMEN GİZLİ (SW_HIDE = 0) BAŞLATIYORUZ
            IntPtr hwnd = CreateWindowEx(
                0x00000080, // WS_EX_TOOLWINDOW (Görev çubuğunda bile görünmez!)
                className, windowTitle, 0, // 0 = WS_OVERLAPPED (görünürlük bayrağı yok)
                0, 0, 0, 0,
                IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);

            ShowWindow(hwnd, 0); // SW_HIDE = 0 (Ekranda kesinlikle görünmez!)


            MSG msg;
            bool running = true;
            const uint WM_QUIT = 0x0012;
            const uint PM_REMOVE = 0x0001;

            // Sadece standart Windows mesaj döngüsü
            while (running)
            {
                while (PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    if (msg.message == WM_QUIT)
                    {
                        running = false;
                        break;
                    }
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }

                Thread.Sleep(50);
            }
        }
    }
}
