// Copyright 2010 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.

using System;
using System.Net;

namespace Microsoft.Synchronization.ClientServices
{
    /// <summary>
    /// Base class that will handle the processing of a CacheRequest
    /// </summary>
	public abstract class CacheRequestHandler
    {
        SerializationFormat _serializationFormat;
        Uri _baseUri;
        string _scopeName;

        protected SerializationFormat SerializationFormat
        {
            get { return _serializationFormat; }
        }

        protected string ScopeName
        {
            get { return _scopeName; }
        }

        protected Uri BaseUri
        {
            get { return _baseUri; }
        }

        protected CacheRequestHandler(Uri baseUri, SerializationFormat format, string scopeName)
        {
            this._baseUri = baseUri;
            this._serializationFormat = format;
            this._scopeName = scopeName;
        }

        /// <summary>
        /// Method that will contain the actual implementation of the cache request processing.
        /// </summary>
        /// <param name="request">CacheRequest object</param>
        /// <param name="state">User state object</param>
        public abstract void ProcessCacheRequestAsync(CacheRequest request, object state);

        /// <summary>
        /// Event handler that callers can hook to get notified when the request has been processed.
        /// </summary>
        public event EventHandler<ProcessCacheRequestCompletedEventArgs> ProcessCacheRequestCompleted;

        protected void OnProcessCacheRequestCompleted(ProcessCacheRequestCompletedEventArgs args)
        {
            if (this.ProcessCacheRequestCompleted != null)
            {
                this.ProcessCacheRequestCompleted(this, args);
            }
        }
    }
}
