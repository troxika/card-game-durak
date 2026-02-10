using System.Net.Sockets;
using System.Windows.Forms;

namespace Client_winforms
{
    public partial class Form1 : Form
    {

        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;

        private bool isConnected = false;
        private string serverIP = "46.226.106.127";
        private int port = 36257;
        //private string serverIP = "127.0.0.1";
        //private int port = 27015;



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

        private void button2_Click(object sender, EventArgs e)
        {
            using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (isConnected && stream != null)
            {
                try
                {
                    string message = textBox1.Text;
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    label1.Text = "Отправлено: " + message;
                    stream.Write(data, 0, data.Length);
                    textBox1.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка отправки: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("...");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

}
