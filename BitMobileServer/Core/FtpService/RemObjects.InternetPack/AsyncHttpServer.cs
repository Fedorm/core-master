/*---------------------------------------------------------------------------
  RemObjects Internet Pack for .NET - Core Library
  (c)opyright RemObjects Software, LLC. 2003-2012. All rights reserved.

  Using this code requires a valid license of the RemObjects Internet Pack
  which can be obtained at http://www.remobjects.com?ip.
---------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace RemObjects.InternetPack.Http
{
#if DESIGN
    [System.Drawing.ToolboxBitmap(typeof(RemObjects.InternetPack.Server), "Glyphs.HttpServer.bmp")]
#endif
    public class AsyncHttpServer : AsyncServer
    {
        public AsyncHttpServer()
        {
            this.DefaultPort = 80;
            this.KeepAlive = true;
            this.ServerName = sServerName;
            this.MaxPostSize = 4194304;
        }

#if FULLFRAMEWORK
        public AsyncHttpServer(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }
#endif

        public override Type GetWorkerClass()
        {
            return typeof(AsyncHttpWorker);
        }

        #region Properties
        [DefaultValue(true), Category("Server")]
        public Boolean KeepAlive
        {
            get
            {
                return fKeepAlive;
            }
            set
            {
                fKeepAlive = value;
            }
        }
        private Boolean fKeepAlive;

        [DefaultValue(true), Category("Server")]
        public Boolean ValidateRequests
        {
            get
            {
                return fValidateRequests;
            }
            set
            {
                fValidateRequests = value;
            }
        }
        private Boolean fValidateRequests;

        [DefaultValue(sServerName), Category("Server")]
        public String ServerName
        {
            get
            {
                return fServerName;
            }
            set
            {
                fServerName = value;
            }
        }
        private String fServerName;

        public const String sServerName = "Internet Pack HTTP Server";

        [DefaultValue(4194304), Category("Server")]
        public Int32 MaxPostSize
        {
            get
            {
                return fMaxPostSize;
            }
            set
            {
                fMaxPostSize = value;
            }
        }
        private Int32 fMaxPostSize;
        #endregion

        public event OnAsyncHttpRequestHandler OnBeforeHaveData;
        public event OnAsyncHttpRequestHandler OnHttpRequest;
        public event OnAsyncHttpRequestHandler OnHttpResponseSent;
        public event OnAsyncHttpRequestHandler OnHttpResponseFailed;

        internal protected virtual void TriggerOnBeforeHaveData(Connection connection, AsyncHttpContext context)
        {
            if (this.OnBeforeHaveData != null)
            {
                OnAsyncHttpRequestArgs lEventArgs = new OnAsyncHttpRequestArgs(connection, context);
                this.OnBeforeHaveData(this, lEventArgs);
            }
        }

        internal protected virtual void TriggerOnHttpRequest(Connection connection, AsyncHttpContext context)
        {
            if (this.OnHttpRequest != null)
            {
                OnAsyncHttpRequestArgs lEventArgs = new OnAsyncHttpRequestArgs(connection, context);
                this.OnHttpRequest(this, lEventArgs);
            }
        }

        internal protected virtual void TriggerOnHttpResponseSent(Connection connection, AsyncHttpContext context)
        {
            if (this.OnHttpResponseSent != null)
            {
                OnAsyncHttpRequestArgs lEventArgs = new OnAsyncHttpRequestArgs(connection, context);
                this.OnHttpResponseSent(this, lEventArgs);
            }
        }

        internal protected virtual void TriggerOnHttpResponseFailed(Connection connection, AsyncHttpContext context)
        {
            if (this.OnHttpResponseFailed != null)
            {
                OnAsyncHttpRequestArgs lEventArgs = new OnAsyncHttpRequestArgs(connection, context);
                this.OnHttpResponseFailed(this, lEventArgs);
            }
        }

        protected internal virtual AsyncHttpContext NewContext(AsyncHttpWorker worker)
        {
            return new AsyncHttpContext(worker);
        }
    }

    public delegate void OnAsyncHttpRequestHandler(Object sender, OnAsyncHttpRequestArgs e);

    public class OnAsyncHttpRequestArgs : ConnectionEventArgs
    {
        public OnAsyncHttpRequestArgs(Connection connection, AsyncHttpContext context)
            : base(connection)
        {
            this.fContext = context;
        }

        public AsyncHttpContext Context
        {
            get
            {
                return this.fContext;
            }
        }
        private readonly AsyncHttpContext fContext;
    }

    public class AsyncHttpRequest : HttpRequestResponse
    {
        protected override Boolean Client
        {
            get
            {
                return false;
            }
        }

        protected override Boolean Server
        {
            get
            {
                return true;
            }
        }

        public Byte[] ContentBytes
        {
            get
            {
                return this.fContentBytes;
            }
            set
            {
                this.fContentBytes = value;
            }
        }
        private Byte[] fContentBytes;
    }

    public class AsyncHttpContext
    {
        public AsyncHttpContext(AsyncHttpWorker worker)
        {
            this.fWorker = worker;
            this.fCurrentRequest = new AsyncHttpRequest();
            this.fCurrentResponse = new HttpServerResponse();
        }

        private readonly AsyncHttpWorker fWorker;

        #region Properties
        public Connection Connection
        {
            get
            {
                return this.fWorker.DataConnection;
            }
        }

        public AsyncHttpRequest CurrentRequest
        {
            get
            {
                return this.fCurrentRequest;
            }
        }
        private readonly AsyncHttpRequest fCurrentRequest;

        public HttpServerResponse CurrentResponse
        {
            get
            {
                return this.fCurrentResponse;
            }
        }
        private readonly HttpServerResponse fCurrentResponse;

        public Boolean ResponseSent
        {
            get
            {
                return this.fResponseSent;
            }
            set
            {
                this.fResponseSent = value;
            }
        }
        private Boolean fResponseSent;

        public Object UserData
        {
            get
            {
                return this.fUserData;
            }
            set
            {
                this.fUserData = value;
            }
        }
        private Object fUserData;
        #endregion

        public void SendResponse()
        {
            if (this.ResponseSent)
                return;

            this.fResponseSent = true;
            this.CurrentResponse.FinalizeHeader();
            this.fWorker.SendResponse();
        }
    }

    public class AsyncHttpWorker : AsyncWorker
    {
        private AsyncHttpContext fContext;

        protected override void DoSetup()
        {
            this.DataConnection.MaxLineLength = 8096;
            this.DataConnection.MaxLineLengthEnabled = true;
            this.DataConnection.AsyncDisconnect += new EventHandler(cbDisconnect);
            this.fContext = ((AsyncHttpServer)Owner).NewContext(this);
            try
            {
                this.DataConnection.BeginReadLine(new AsyncCallback(HeaderFirstLineCallback), null);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void cbDisconnect(Object sender, EventArgs e)
        {
            this.Done();
        }

        #region header reading
        private void HeaderFirstLineCallback(IAsyncResult ar)
        {
            fContext.ResponseSent = false;

            String lFirst;
            try
            {
                lFirst = this.DataConnection.EndReadLine(ar);
            }
            catch (ConnectionClosedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                this.fContext.CurrentRequest.Header.FirstHeader = lFirst;
                this.fContext.CurrentRequest.Header.ParseFirstLine();
            }
            catch (HttpHeaderException)
            {
                this.SendInvalidRequest();
                return;
            }
            catch (UrlParserException)
            {
                this.SendInvalidRequest();
                return;
            }

            try
            {
                this.DataConnection.BeginReadLine(new AsyncCallback(HeaderLinesCallback), null);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void HeaderLinesCallback(IAsyncResult ar)
        {
            String lHeaderLine = null;
            try
            {
                lHeaderLine = this.DataConnection.EndReadLine(ar);
            }
            catch (ConnectionClosedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            // HTTP Request Type is already known
            String lHttpMethod = this.fContext.CurrentRequest.Header.RequestType;
            Boolean lRequireBody = (lHttpMethod == "POST") || (lHttpMethod == "PUT") || (lHttpMethod == "MERGE");

            Boolean lHaveData = true;
            while (lHaveData)
            {
                lHaveData = false;
                if (lHeaderLine == "")
                {
                    // we've got the last line. Process it
                    if (lRequireBody)
                    {
                        Int64 lContentLength;
#if FULLFRAMEWORK
                        if (!Int64.TryParse(fContext.CurrentRequest.Header.GetHeaderValue("Content-Length"), out lContentLength))
#else
                        if (!LongHelper.TryParse(fContext.CurrentRequest.Header.GetHeaderValue("Content-Length"), out lContentLength))
#endif
                            lContentLength = 0;

                        if (lContentLength > ((AsyncHttpServer)this.Owner).MaxPostSize)
                        {
                            this.SendInvalidRequest(new Exception("Content-Length too large"));
                            return;
                        }

                        try
                        {
                            ((AsyncHttpServer)this.Owner).TriggerOnBeforeHaveData(this.DataConnection, this.fContext);
                        }
                        catch (Exception ex)
                        {
                            this.SendInvalidRequest(ex);
                            return;
                        }

                        if (this.fContext.ResponseSent)
                            return; // already triggered the required functions.

                        try
                        {
                            Byte[] lData = new Byte[(Int32)lContentLength];
                            DataConnection.BeginRead(lData, 0, (Int32)lContentLength, new AsyncCallback(WantBodyCallback), lData);
                        }
                        catch (SocketException)
                        {
                        }
                        catch (ObjectDisposedException)
                        {
                        }

                        return;
                    }
                    else
                    {
                        try
                        {
                            ((AsyncHttpServer)Owner).TriggerOnHttpRequest(DataConnection, fContext);
                            return;
                        }
                        catch (Exception ex)
                        {
                            this.SendInvalidRequest(ex);
                            return;
                        }
                    }
                }

                if (fContext.CurrentRequest.Header.Count >= fContext.CurrentRequest.Header.MaxHeaderLines && fContext.CurrentRequest.Header.MaxHeaderLinesEnabled)
                {
                    SendInvalidRequest();
                    return;
                }

                Int32 lPosition = lHeaderLine.IndexOf(":");
                if (lPosition == -1)
                {
                    SendInvalidRequest();
                    return;
                }

                String lName = lHeaderLine.Substring(0, lPosition);
                String lValue = null;
                if (lHeaderLine.Length > lPosition + 1)
                    lValue = lHeaderLine.Substring(lPosition + 2);

                fContext.CurrentRequest.Header.SetHeaderValue(lName, lValue);
                lHeaderLine = DataConnection.BufferReadLine();
                if (lHeaderLine != null)
                {
                    lHaveData = true;
                    continue;
                }

                try
                {
                    DataConnection.BeginReadLine(new AsyncCallback(HeaderLinesCallback), null);
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private void WantBodyCallback(IAsyncResult ar)
        {
            try
            {
                this.DataConnection.EndRead(ar);
            }
            catch (ConnectionClosedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            this.fContext.CurrentRequest.ContentBytes = (Byte[])ar.AsyncState;

            try
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpRequest(this.DataConnection, this.fContext);
            }
            catch (Exception ex)
            {
                this.SendInvalidRequest(ex);
                return;
            }
        }
        #endregion

        private void SendInvalidRequest(Exception ex)
        {
            this.fContext.CurrentResponse.Header.SetHeaderValue("Server", ((AsyncHttpServer)Owner).ServerName);

            if (ex != null)
                this.fContext.CurrentResponse.SendError(500, "Server Error while processing HTTP request.", ex);
            else
                this.fContext.CurrentResponse.SendError(500, "Server Error while processing HTTP request.");

            if (this.fContext.CurrentResponse.ContentBytes == null)
            {
                if (this.fContext.CurrentResponse.ContentString != null)
                    this.fContext.CurrentResponse.ContentBytes = this.fContext.CurrentResponse.Encoding.GetBytes(fContext.CurrentResponse.ContentString);
                else
                    this.fContext.CurrentResponse.ContentBytes = new Byte[0];
            }

            this.fContext.CurrentResponse.FinalizeHeader();
            Byte[] lHeader = Encoding.ASCII.GetBytes(this.fContext.CurrentResponse.Header.ToString());
            Byte[] lData = new Byte[fContext.CurrentResponse.ContentBytes.Length + lHeader.Length];

            Array.Copy(lHeader, 0, lData, 0, lHeader.Length);
            Array.Copy(fContext.CurrentResponse.ContentBytes, 0, lData, lHeader.Length, fContext.CurrentResponse.ContentBytes.Length);

            this.DataConnection.BeginWrite(lData, 0, lData.Length, new AsyncCallback(InvalidRequestCallback), null);
        }

        private void SendInvalidRequest()
        {
            this.SendInvalidRequest(null);
        }

        private void InvalidRequestCallback(IAsyncResult ar)
        {
            try
            {
                this.DataConnection.EndWrite(ar);
                this.DataConnection.TriggerAsyncDisconnect();
                this.DataConnection.Disconnect();
            }
            catch (ConnectionClosedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        public override void Done()
        {
            base.Done();
        }

        private Int32 fBodyOffset;
        private Byte[] fBodyBuffer;

        public void SendResponse()
        {
            try
            {
                fContext.CurrentResponse.KeepAlive = ((AsyncHttpServer)AsyncOwner).KeepAlive;
                Byte[] lHeader = Encoding.ASCII.GetBytes(fContext.CurrentResponse.Header.ToString());

                if (lHeader.Length >= 4096 || fContext.CurrentResponse.ContentSource == ContentSource.ContentNone)
                {
                    fBodyOffset = 0;

                    switch (fContext.CurrentResponse.ContentSource)
                    {
                        case ContentSource.ContentBytes:
                            DataConnection.BeginWrite(lHeader, 0, fBodyOffset, new AsyncCallback(ResponseBodyCallback), fContext.CurrentResponse.ContentBytes);
                            break;

                        case ContentSource.ContentString:
                            Byte[] lBuffer = fContext.CurrentResponse.Encoding.GetBytes(fContext.CurrentResponse.ContentString);
                            DataConnection.BeginWrite(lHeader, 0, fBodyOffset, new AsyncCallback(ResponseBodyCallback), lBuffer);
                            break;

                        case ContentSource.ContentStream:
                            fContext.CurrentResponse.ContentStream.Position = 0;
                            DataConnection.BeginWrite(lHeader, 0, fBodyOffset, new AsyncCallback(ResponseBodyCallback), fContext.CurrentResponse.ContentStream);
                            break;

                        default:
                            DataConnection.BeginWrite(lHeader, 0, fBodyOffset, new AsyncCallback(ResponseBodyCallback), null);
                            break;
                    }
                }
                else
                {
                    if (fBodyBuffer == null)
                        fBodyBuffer = new Byte[4096];

                    Array.Copy(lHeader, 0, fBodyBuffer, 0, lHeader.Length);
                    fBodyOffset = fBodyBuffer.Length - lHeader.Length;

                    switch (fContext.CurrentResponse.ContentSource)
                    {
                        case ContentSource.ContentBytes:
                            if (fBodyOffset > fContext.CurrentResponse.ContentBytes.Length)
                                fBodyOffset = fContext.CurrentResponse.ContentBytes.Length;
                            Array.Copy(fContext.CurrentResponse.ContentBytes, 0, fBodyBuffer, lHeader.Length, fBodyOffset);
                            DataConnection.BeginWrite(fBodyBuffer, 0, fBodyOffset + lHeader.Length, new AsyncCallback(ResponseBodyCallback), fContext.CurrentResponse.ContentBytes);
                            break;

                        case ContentSource.ContentString:
                            Byte[] lBuffer = fContext.CurrentResponse.Encoding.GetBytes(fContext.CurrentResponse.ContentString);
                            if (fBodyOffset > lBuffer.Length)
                                fBodyOffset = lBuffer.Length;
                            Array.Copy(lBuffer, 0, fBodyBuffer, lHeader.Length, fBodyOffset);
                            DataConnection.BeginWrite(fBodyBuffer, 0, fBodyOffset + lHeader.Length, new AsyncCallback(ResponseBodyCallback), lBuffer);
                            break;

                        case ContentSource.ContentStream:
                            fContext.CurrentResponse.ContentStream.Position = 0;
                            fBodyOffset = fContext.CurrentResponse.ContentStream.Read(fBodyBuffer, lHeader.Length, fBodyOffset);
                            DataConnection.BeginWrite(fBodyBuffer, 0, fBodyOffset + lHeader.Length, new AsyncCallback(ResponseBodyCallback), fContext.CurrentResponse.ContentStream);
                            break;

                        default:
                            DataConnection.BeginWrite(lHeader, 0, fBodyOffset, new AsyncCallback(ResponseBodyCallback), null);
                            break;
                    }
                }
            }
            catch (ConnectionClosedException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }
            catch (SocketException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }
            catch (ObjectDisposedException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }
        }

        private void ResponseBodyCallback(IAsyncResult ar)
        {
            try
            {
                DataConnection.EndWrite(ar);

                if (ar.AsyncState is Stream)
                {
                    Stream lData = (Stream)ar.AsyncState;
                    Int32 lLen = lData.Read(fBodyBuffer, 0, fBodyBuffer.Length);
                    if (lLen != 0)
                    {
                        DataConnection.BeginWrite(fBodyBuffer, 0, lLen, new AsyncCallback(ResponseBodyCallback), lData);
                        return;
                    }
                }
                else if (ar.AsyncState is Byte[])
                {
                    Byte[] lData = (Byte[])ar.AsyncState;
                    Int32 lLen = fBodyBuffer.Length;
                    if (fBodyOffset + lLen > lData.Length)
                        lLen = lData.Length - fBodyOffset;
                    if (lLen != 0)
                    {
                        Array.Copy(lData, fBodyOffset, fBodyBuffer, 0, lLen);
                        fBodyOffset += lLen;
                        DataConnection.BeginWrite(fBodyBuffer, 0, lLen, new AsyncCallback(ResponseBodyCallback), lData);
                        return;
                    }
                }
            }
            catch (ConnectionClosedException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }
            catch (SocketException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }
            catch (ObjectDisposedException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }

            AsyncHttpContext lOldContext = fContext;

            fContext = ((AsyncHttpServer)Owner).NewContext(this);
            try
            {
                DataConnection.BeginReadLine(new AsyncCallback(HeaderFirstLineCallback), null);
            }
            catch (SocketException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }

            catch (ObjectDisposedException)
            {
                ((AsyncHttpServer)Owner).TriggerOnHttpResponseFailed(DataConnection, fContext);
                return;
            }
            ((AsyncHttpServer)Owner).TriggerOnHttpResponseSent(DataConnection, lOldContext);

            return;
        }
    }
}
