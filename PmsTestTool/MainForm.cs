using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace PmsTestTool
{
    public partial class MainForm : Form
    {
        TcpListener tcpListener;
        TcpClient tcpClient;
        private byte[] STX = new byte[] { 0x02 };
        private byte[] ETX = new byte[] { 0x03 };

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //this.Icon = new Icon("Resources/pke.ico");
            //this.Icon = Properties.Resources.pke;
            backgroundWorker1.RunWorkerAsync();
        }

        private void dataEkleListBox(string data)
        {
            Invoke((Action)(() =>
            {
                listBox1.Items.Insert(0, data);
            }));

        }

        private void HandleClient()
        {
            try
            {
                dataEkleListBox("New Connection.");
                NetworkStream stream = tcpClient.GetStream();
                StreamReader sr = new StreamReader(stream);

                string okunan = "";

                while (sr.Peek() > 0)
                {
                oku1:
                    char[] buf = new char[1];
                    int readchars = sr.Read(buf, 0, 1);
                    okunan += buf[0];
                    if (Convert.ToInt16(buf[0]) == 3)
                    {
                        dataEkleListBox("Inc -->" + okunan);
                        Console.WriteLine(okunan);
                        if (okunan.Contains("LS|"))
                        {
                            sendToPBX("LS|");
                            sendToPBX("LD|PKE.pms.v1");
                        }
                        if (okunan.Contains("LA|"))
                        {
                            sendToPBX("LA|");
                        }

                        okunan = null;
                        goto oku1;
                    }
                    goto oku1;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                tcpClient.Close();
                tcpListener.Stop();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), 60600);
                tcpListener.Start();
                while (true)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        tcpListener.Stop();
                        e.Cancel = true;
                        return;
                    }
                    tcpClient = new TcpClient();
                    tcpClient = tcpListener.AcceptTcpClient();
                    HandleClient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                tcpClient.Close();
                tcpListener.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string data = "LA|";
            sendToPBX(data);
            
            data = "";
            sendToPBX(data);
        }

        private void sendToPBX(string data)
        {
            if (tcpClient != null)
            {
                if (tcpClient.Connected)
                {
                    NetworkStream stream = tcpClient.GetStream();

                    byte[] bytes = Encoding.ASCII.GetBytes(data);

                    IEnumerable<byte> rv = STX.Concat(bytes).Concat(ETX);

                    stream.Write(rv.ToArray(), 0, rv.Count());

                    dataEkleListBox("Out +++" + Encoding.ASCII.GetString(rv.ToArray()));
                    stream.Flush();
                }
            }
        }

        private string pmsDateTime(DateTime dat,bool wake)
        {
            string date = dat.ToString("yyMMdd");
            string time = dat.ToString("HHmmss");
            if (wake)
            {
                time = dat.ToString("HHmm00");
            }

            return DateTime.Now.ToString("|DA" + date + "|TI" + time + "|");
        }

        private void giBtn_Click(object sender, EventArgs e)
        {
            //<STX>GI|RN202|GTMr.|GFDavid|GNMorrissey|G#6799|GLEA|GSN|<ETX>

            string data =
                "GI|RN" + roomTbox.Text +
                "|GT" + titleTbox.Text +
                "|GF" + nameTbox.Text.Split(' ')[0] +
                "|GN" + nameTbox.Text.Split(' ')[1] +
                "|G#9" + roomTbox.Text +
                "|GL" + langTbox.Text +
                "|GSN|";

            sendToPBX(data);
        }

        private void goBtn_Click(object sender, EventArgs e)
        {
            //<STX>GO|RN201|G#2126|GSN|<ETX>
            //<STX>GO|RN201|<ETX>

            string data =
                "GO|RN" + roomTbox.Text + "|";
            sendToPBX(data);
        }

        private void spRsBtn_Click(object sender, EventArgs e)
        {
            //<STX>RE|RN202|CS1|<ETX>
            string data =
                "RE|RN" + roomTbox.Text + "|RS" + rsNum.Value.ToString() + "|";
            sendToPBX(data);
        }

        private void spCsBtn_Click(object sender, EventArgs e)
        {
            //<STX>RE|RN202|CS1|<ETX>
            string data =
                "RE|RN" + roomTbox.Text + "|CS" + csNum.Value.ToString() + "|";
            sendToPBX(data);
        }

        private void dndYesBtn_Click(object sender, EventArgs e)
        {
            //<STX>RE|RN201|DNY|<ETX>
            string data =
                "RE|RN" + roomTbox.Text + "|DNY|";
            sendToPBX(data);
        }

        private void dndNoBtn_Click(object sender, EventArgs e)
        {
            //<STX>RE|RN201|DNY|<ETX>
            string data = "RE|RN" + roomTbox.Text + "|DNN|";
            sendToPBX(data);
        }

        private void wakSetBtn_Click(object sender, EventArgs e)
        {
            //<STX>WR|RN202|DA231031|TI104600|<ETX>
            string data = "WR|RN" + roomTbox.Text + pmsDateTime(wakDP.Value,true);
            sendToPBX(data);
        }

        private void wakClrBtn_Click(object sender, EventArgs e)
        {
            //<STX>WC|RN202|DA231031|TI104600|<ETX>
            string data = "WC|RN" + roomTbox.Text + pmsDateTime(wakDP.Value, true);
            sendToPBX(data);
        }

        private void msgBtn_Click(object sender, EventArgs e)
        {
            //XL|RN111|MT3
            string data = "RE|RN" + roomTbox.Text + "|MLY|G#9"+roomTbox.Text+"|";
            sendToPBX(data);
        }
    }
}
