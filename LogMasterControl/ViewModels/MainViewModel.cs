using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.DryIoc;
using System.Threading;
using System.IO;
using System.Windows.Markup;

namespace LogMasterControl.ViewModels
{
    internal class MainViewModel : BindableBase
    {
		private ObservableCollection<string> messageList;

		public ObservableCollection<string> MessageList
        {
			get { return messageList; }
			set { messageList = value; }
		}

		public MainViewModel()
		{
            this.messageList = new ObservableCollection<string>();
            this.listener = new TcpListener(IPAddress.Any, 8100);
            Task.Run(StartListening);
        }

        private TcpListener listener;

        public void StartListening()
        {
            listener.Start();
            Console.WriteLine($"Server started on port {8100}.");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient(); // 等待客户端连接
                Console.WriteLine("Client connected.");

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
                clientThread.Start(client);
            }
        }

        private void HandleClientCommunication(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            using (NetworkStream stream = client.GetStream())
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                try
                {
                    while (client.Connected)
                    {
                        //等待接收从站的消息
                        string receiveMessage = reader.ReadString();
                        PrismApplication.Current.Dispatcher.Invoke(() =>
                        {
                            this.messageList.Add(receiveMessage);
                        });

                        //发送消息给从站
                        writer.Write("主控已接收到消息");
                    }
                }
                catch (Exception e)
                {
                    PrismApplication.Current.Dispatcher.Invoke(() =>
                    {
                        this.messageList.Add(e.Message);
                    });
                }
                finally
                {
                    client.Close();
                }
            }
        }

        //private Socket listener;

        //private async void StartListening()
        //{
        //    listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    listener.Bind(new IPEndPoint(IPAddress.Any, 8001));
        //    listener.Listen(10);

        //    while (true)
        //    {
        //        Socket handler = await listener.AcceptAsync();
        //        await Task.Run(() => ProcessClient(handler));
        //    }
        //}

        //private void ProcessClient(Socket handler)
        //{
        //    byte[] buffer = new byte[1024];
        //    while (true)
        //    {
        //        int bytesRead = handler.Receive(buffer);
        //        if (bytesRead <= 0)
        //        {
        //            break;
        //        }
        //        var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //        // 处理数据
        //        PrismApplication.Current.Dispatcher.Invoke(() =>
        //        {
        //            this.messageList.Add(data);
        //        });
        //    }
        //    handler.Shutdown(SocketShutdown.Both);
        //    handler.Close();
        //}
    }
}
