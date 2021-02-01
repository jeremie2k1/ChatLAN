using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class devClient : Form
    {
        public devClient()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string newMess = Name + ": " + txtMessage.Text;
            lsvMessage.Items.Add(new ListViewItem() { Text = newMess });
            Send();
            txtMessage.Clear();
        }
        IPEndPoint IP;
        Socket client;
        string Name;
        void Connect()
        {
            // IP: địa chỉ của server
            IP = new IPEndPoint(IPAddress.Parse(txtServerIP.Text), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(IP);
                MessageBox.Show("Kết nối thành công!");
            }
            catch
            {
                MessageBox.Show("Lỗi kết nối tới server!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }
        void CloseSocket()
        {
            client.Close();
        }
        void Send()
        {
            if (txtMessage.Text != string.Empty)
            {
                string newMess = Name + ": " + txtMessage.Text;
                client.Send(Serialize(newMess));
            }
        }
        
        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 100000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);
                    AddMessage(message);
                }
            }
            catch
            {
                CloseSocket();
            }
            
            
        }
        void AddMessage(string mess)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = mess });
        }
        byte[] Serialize(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, obj);
            return ms.ToArray();
        }
        object Deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf1 = new BinaryFormatter();
            //ms.Position = 0;
            return bf1.Deserialize(ms);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseSocket();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void btnConfirmName_Click(object sender, EventArgs e)
        {
            Name = txtName.Text;
            MessageBox.Show("Đăng ký tên thành công!");
        }
    }
}
