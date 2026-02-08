using System.Net.Sockets;

namespace Client_winforms
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;
        private string serverIP = "46.226.106.127";
        private int port = 34987;

        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                client.Connect(serverIP, port);
                stream = client.GetStream();
                isConnected = true;
                label1.Text = "Подключено успешно";
                //MessageBox.Show("Подключено успешно");
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Ошибка подключения: " + ex.Message);
                label1.Text = "Ошибка подключения: " + ex.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
