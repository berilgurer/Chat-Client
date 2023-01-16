using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace inLabEx
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
            string IP = textBox_ip.Text;

            int portNum;
            if ((Int32.TryParse(textBox_port.Text, out portNum)) || (IP != "10.61.1.42"))
            {
                try
                {
                    clientSocket.Connect(IP, portNum);
                    connected = true;
                    logs.AppendText("Connection established...\n");

                    Thread receiveThread = new Thread(Receive);
                    receiveThread.Start();

                }
                catch
                {
                    logs.AppendText("Problem occured while connecting...\n");
                }
            }
            else
            {
                logs.AppendText("Problem occured while connecting...\n");
            }
        }

        private void Receive()
        {
            string name = textBox_name.Text;
            while (connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);

                    string incomingToken = Encoding.Default.GetString(buffer);
                    //incomingToken = incomingToken.Substring(0, incomingToken.IndexOf("\0"));
                    int outToken;
                    Int32.TryParse(incomingToken, out outToken);
                    

                    logs.AppendText("Server: " + outToken + "\n");
                           
                    int sum = 0;
                    for (int i = 0; i< name.Length; i++)             
                    {
                        if(name[i] != ' ')
                        {
                            int ch = (int)name[i];
                            sum += ch;
                        }                  
                    }
                    int finalResult;
                    finalResult = outToken * sum;
                    string newResult = finalResult.ToString() + " " + name;
                    logs.AppendText("Message sent:" + newResult + "\n");

                    Byte[] bufferSend = Encoding.Default.GetBytes(newResult);
                    clientSocket.Send(bufferSend);

                    Byte[] buffer2 = new Byte[64];
                    clientSocket.Receive(buffer2);

                    string incomingToken2 = Encoding.Default.GetString(buffer2);
                    incomingToken = incomingToken.Substring(0, incomingToken.IndexOf("\0"));                

                    logs.AppendText("Server: " + incomingToken2 + "\n");

                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("The server has disconnected\n");
                        button_connect.Enabled = true;        
                    }

                    
                }
                clientSocket.Close();
                connected = false;
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}
