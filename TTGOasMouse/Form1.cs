using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Gma.System.MouseKeyHook;

namespace TTGOasMouse {

    public partial class Form1 : Form {
        public Thread readThread;
        public String ReadSerialVal;
        static bool _continue = false;
        public static SerialPort _serialPort = new SerialPort();
        
        public String touched_temp = "0";
        public int pos_x = 0, pos_y = 0, m_x, m_y;
        public int a, b;

        public int total_x, total_y;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);

        private IKeyboardMouseEvents m_Events;

        public Form1() {
            InitializeComponent();
            //control = richTextBox1;

        }

        private void Subscribe(IKeyboardMouseEvents events) {
            m_Events = events;
            m_Events.MouseMove += M_Events_MouseMove;
        }

        private void Unsubscribe() {
            if (m_Events == null) return;
            m_Events.MouseMove -= M_Events_MouseMove;
            m_Events.Dispose();
            m_Events = null;
        }

        private void M_Events_MouseMove(object sender, MouseEventArgs e) {
            m_x = e.X;
            m_y = e.Y;
        }


        private void label2_Click(object sender, EventArgs e) {

        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e) {
            comboBox1.Items.Clear();
            foreach (string s in SerialPort.GetPortNames()) {
                comboBox1.Items.Add(s);
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());

        }

        private void button1_Click(object sender, EventArgs e) {

            if (!_continue) {

                StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;



                // Allow the user to set the appropriate properties.
                _serialPort.PortName = comboBox1.Text;
                _serialPort.BaudRate = int.Parse(textBox1.Text);

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                try {
                    _serialPort.Open();
                    button1.BackColor = Color.Red;
                    _continue = true;
                    button1.Text = "Close";
                    readThread = new Thread(Read);
                    readThread.Start();
                } catch (Exception eee) {
                    MessageBox.Show(eee.Message);
                }
            }  else {
                button1.BackColor = Color.OliveDrab;
                button1.Text = "Begin";
                //readThread.Abort();
                _serialPort.Close();
                _continue = false;
                
            }

        }

        private void UpdateLabel(string newText) {
            if ((newText.ToLower()).Contains("left clicked!")) {
                richTextBox1.AppendText("L_Click!\n");
                VirtualMouse.LeftClick();
            } else if ((newText.ToLower()).Contains("right clicked!")) {
                richTextBox1.AppendText("R_Click!\n");
                VirtualMouse.RightClick();
            } else if ((newText.ToLower()).Contains("close")) {
                richTextBox1.AppendText("Exit\n");
            } else {
                
                string x = newText.Split(',')[0];
                string y = newText.Split(',')[1];
                string touched = newText.Split(',')[2];
                

                int a = int.Parse(x) - 120;
                int b = int.Parse(y) - 120;

                if (touched.Contains("1") && touched_temp.Contains("0")) {
                    
                    
                    pos_x = m_x;
                    pos_y = m_y;


                    total_x = pos_x + a;
                    total_y = pos_y + b;

                    richTextBox1.AppendText("X: " + a + "   Y: " + b + "   T: " + touched+ "\n");
                    VirtualMouse.MoveTo(pos_x + a, pos_y + b);

                } else if(touched.Contains("1") && touched_temp.Contains("1")) {

                    richTextBox1.AppendText("X: " + a + "   Y: " + b + "   T: " + touched);
                    VirtualMouse.MoveTo(pos_x + a, pos_y + b);

                } else {

                    richTextBox1.AppendText("X: " + a + "   Y: " + b + "   T: " + touched);
                    VirtualMouse.MoveTo(pos_x + a, pos_y + b);
                    
                }
                touched_temp = touched;

            }
            richTextBox1.ScrollToCaret();
        }

        public delegate void ShowSerialData(String r);
        private void Read() {
            while (_continue) {
                try {

                    ReadSerialVal = _serialPort.ReadLine();
                    if (richTextBox1.InvokeRequired) {
                        ShowSerialData ssd = UpdateLabel;
                        Invoke(ssd, ReadSerialVal);
                    } else {
                        richTextBox1.AppendText(ReadSerialVal);
                        richTextBox1.ScrollToCaret();
                    }

                } catch (TimeoutException) {

                } catch (Exception ex) {
                    break;
                } 
            }
        }
   

        private void richTextBox1_TextChanged(object sender, EventArgs e) {

        }

        private void pictureBox3_Click(object sender, EventArgs e) {
            Process.Start("https://www.youtube.com/tnowroz");
        }

        private void pictureBox2_Click(object sender, EventArgs e) {
            Process.Start("https://github.com/TNeutron");
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            Process.Start("https://www.linkedin.com/in/tnowroz/"); 
        }

        private void pictureBox4_Click(object sender, EventArgs e) {
            this.Close();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Unsubscribe();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void panel2_MouseDown(object sender, MouseEventArgs e) {

            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

    }

    }

    public static class VirtualMouse {

        
        [DllImport("user32.dll")]

        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        /*
                [DllImport("user32.dll")]
                static extern bool GetCursorPos(out POINT lpPoint);


                public static int ShowMousePosition() {
                    POINT point;
                    if (GetCursorPos(out point) && point.X != _x && point.Y != _y) {


                        int x_post = point.X;
                        return x_post;
                    }
                    return 0;

                }*/

        public static float Remap(float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static void Move(int xDelta, int yDelta) {
            mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }
        public static void MoveTo(int x, int y) {
            float min = 0;
            float max = 65535;

            int mappedX = (int)Remap(x, 0.0f, 1920.0f, min, max);
            int mappedY = (int)Remap(y, 0.0f, 1080.0f, min, max);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, mappedX, mappedY, 0, 0);
        }
        public static void LeftClick() {
            mouse_event(MOUSEEVENTF_LEFTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }

        public static void LeftDown() {
            mouse_event(MOUSEEVENTF_LEFTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }

        public static void LeftUp() {
            mouse_event(MOUSEEVENTF_LEFTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }

        public static void RightClick() {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }

        public static void RightDown() {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }

        public static void RightUp() {
            mouse_event(MOUSEEVENTF_RIGHTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }
    }





}
