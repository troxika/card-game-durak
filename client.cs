using System.IO;
using System.Net.Sockets;

namespace WinFormsApp7
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;
        private string serverIP = "127.0.0.1";
        private int port = 27015;
        
        public Form1()
        {
            InitializeComponent();
        }
        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                client.Connect(serverIP, port);
                stream = client.GetStream();
                isConnected = true;
                MessageBox.Show("Подключено успешно");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ConnectToServer();
        }
    }
}
