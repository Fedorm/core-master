using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace RemObjects.InternetPack
{
    class PassiveServer : SimpleServer
    {
        public PassiveServer(IPAddress address, int portFrom, int portTo)
            : base()
        {
            this.Binding = new PassiveServerBinding(address,portFrom, portTo);
        }
    }

    public class PassiveServerBinding : ServerBinding
    {
        private int portFrom;
        private int portTo;
        private IPAddress address;

        public PassiveServerBinding(IPAddress address, int portFrom, int portTo)
            :base()
        {
            this.Address = address;
            this.portFrom = portFrom;
            this.portTo = portTo;
        }

        public override void BindUnthreaded()
        {
            for (int i = portFrom; i <= portTo; i++)
            {
                try
                {
                    this.EndPoint = new IPEndPoint(this.Address, i);
                    this.ListeningSocket = new Socket(this.AddressFamily, this.SocketType, this.Protocol);
                    if (!this.EnableNagle)
                        this.ListeningSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
                    this.ListeningSocket.Bind(this.EndPoint);
                    break;
                }
                catch (SocketException)
                {
                    if (i == portTo)
                        throw;
                }
            }

        }
    }
}
