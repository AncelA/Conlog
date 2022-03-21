using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Conlog
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StartServer();
        }

        public async void StartServer()
        {
            // Establish the local endpoint for the socket. Dns.GetHostName returns the name of the host running the application.
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            // Creation TCP/IP Socket using Socket Class Constructor.
            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // Using Bind() method we associate a network address to the Server Socket All client that will connect to this Server Socket must know this network Address.
                listener.Bind(localEndPoint);

                // Using Listen() method we create the Client list that will want to connect to Server.
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting connection ... ");

                    // Suspend while waiting for incoming connection Using Accept() method the server will accept connection of client.
                    Socket clientSocket = await listener.AcceptAsync();

                    // Data buffer set up.
                    byte[] bytes = new Byte[1024];
                    string data = null;

                    while (true)
                    {
                        // receive data from a client in the form of bytes.
                        int numByte = clientSocket.Receive(bytes);

                        // The bytes are converted into a string.
                        data += Encoding.ASCII.GetString(bytes,0, numByte);

                        // A check done for the end of file tag indicating no rows were returned.
                        if(data.IndexOf("<EOF>") > -1)
                        {
                            ProcessData("");
                            break;
                        }

                        // The string representation of the data that was returned is sent to the ProcessData method.
                        ProcessData(data);

                        // The string data is searched for the end of the resultset which results in the break being hit.
                        if (data.IndexOf("}]") > -1)
                            break;
                    }

                    // Close client Socket using the Close() method. After closing, we can use the closed Socket for a new Client Connection.
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ProcessData(string data)
        {
            // If the string data is empty, the dataGridView is cleared. If the data is not empty, the data is bound to the dataGridView.
            if(data != "")
            {
                var list = JsonConvert.DeserializeObject<List<Customer>>(data);
                dataGridView1.DataSource = list;
            }
            else
                dataGridView1.DataSource = null;
        }
    }
}
