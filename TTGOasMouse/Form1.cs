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


namespace TTGOasMouse {

    public partial class Form1 : Form {
        public Thread readThread;
        public String ReadSerialVal;
        static bool _continue = false;
        public static SerialPort _serialPort = new SerialPort();


        public Form1() {
            InitializeComponent();
            //control = richTextBox1;
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
            VirtualMouse.MoveTo(1000, 0);

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
                    button1.Text = "Close Port";
                    readThread = new Thread(Read);
                    readThread.Start();
                } catch (Exception eee) {
                    MessageBox.Show(eee.Message);
                }
            }  else {
                button1.BackColor = Color.Yellow;
                button1.Text = "Open Port";
                //readThread.Abort();
                _serialPort.Close();
                _continue = false;
                
            }

        }

        private void UpdateLabel(string newText) {
            if ((newText.ToLower()).Contains("clicked")) {
                richTextBox1.AppendText("Click!\n");
            } else {
                String x = newText.Split(',')[0];
                String y = newText.Split(',')[1];
                richTextBox1.AppendText("X: " + x + "   Y: " + y);
                VirtualMouse.MoveTo(Int16.Parse(x), Int16.Parse(y));
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
