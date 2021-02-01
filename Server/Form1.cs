using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class devServer : Form
    {
        public devServer()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseSocket();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string newMess = "Server: " + txtMessage.Text;
            lsvMessage.Items.Add(new ListViewItem() { Text = newMess });
            foreach (Socket item in listClient)
            {
                Send(item);
            }
            txtMessage.Clear();
        }
        IPEndPoint IP;
        Socket server;
        List<Socket> listClient;
        void CreateServer()
        {
            listClient = new List<Socket>();
            
            // IP: địa chỉ của server
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            txtIPServer.Text = GetLocalIPv4(NetworkInterfaceType.Wireless80211); // get theo Internet hoặc Wifi - ở đây set Wifi
            if (string.IsNullOrEmpty(txtIPServer.Text))
            {
                txtIPServer.Text = GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }
            server.Bind(IP);

            Thread listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        listClient.Add(client);

                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            });
            listen.IsBackground = true;
            listen.Start();
        }
        void CloseSocket()
        {
            server.Close();
        }
        void Send(Socket client)
        {
            if (txtMessage.Text != string.Empty)
            {
                string newMess = "Server: " + txtMessage.Text;
                client.Send(Serialize(newMess));
            }
        }
        void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);
                    AddMessage(message);
                }
            }
            catch
            {
                listClient.Remove(client);
                client.Close();
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

        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        private void btnCreateServer_Click(object sender, EventArgs e)
        {
            CreateServer();
        }
    }
}
