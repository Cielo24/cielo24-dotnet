using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using NLog;

namespace Cielo24
{
    public class WebUtils
    {
        public static readonly TimeSpan BASIC_TIMEOUT = new TimeSpan(TimeSpan.TicksPerSecond * 60);    // 60 seconds
        public static readonly TimeSpan DOWNLOAD_TIMEOUT = new TimeSpan(TimeSpan.TicksPerMinute * 5);  // 5 minutes
        // Made public to allow users to redirect the logger output
        public static Logger logger = LogManager.GetLogger("WebUtils");

        /* A synchronous method that performs an HTTP request returning data received from the sever as a string */
        public string HttpRequest(Uri uri, HttpMethod method, TimeSpan timeout, Dictionary<string, string> headers = null)
        {
            logger.Info("Uri: " + uri);
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = method.ToString();
            foreach (var pair in headers ?? new Dictionary<string, string>())
            {
                request.Headers[pair.Key] = pair.Value;
            }

            var asyncResult = request.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(timeout); // Wait untill response is received, then proceed
            if (asyncResult.IsCompleted)
            {
                return ReadResponse(request, asyncResult);
            }
            throw new TimeoutException("The HTTP session has timed out.");
        }

        /* Used exclusively by UpdatePassword method */
        public string HttpRequest(Uri uri, HttpMethod method, TimeSpan timeout, string requestBody)
        {
            var s = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            return UploadData(uri, s, "password");
        }

        /* Uploads data in the body of HTTP request */
        public string UploadData(Uri uri, Stream inputStream, string contentType)
        {
            Debug.WriteLine("Uri: " + uri);
            var request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = HttpMethod.POST.ToString();
            request.ContentType = contentType;
            request.AllowWriteStreamBuffering = false;
            request.ContentLength = inputStream.Length;

            var asyncRequest = request.BeginGetRequestStream(null, null);
            asyncRequest.AsyncWaitHandle.WaitOne(BASIC_TIMEOUT); // Wait untill stream is opened
            if (asyncRequest.IsCompleted)
            {
                try
                {
                    var webStream = request.EndGetRequestStream(asyncRequest);
                    inputStream.CopyTo(webStream);
                    inputStream.Dispose();
                    webStream.Flush();
                    webStream.Dispose();
                }
                catch (WebException err)
                {
                    throw new WebException("Unknown error: could not upload data.", err);
                }
            }
            else
            {
                throw new WebException("Timeout error: could not open stream for data uploading.");
            }

            var asyncResponse = request.BeginGetResponse(null, null);
            asyncResponse.AsyncWaitHandle.WaitOne();
            if (asyncResponse.IsCompleted)
            {
                return ReadResponse(request, asyncResponse);
            }
            throw new TimeoutException("The HTTP session has timed out.");
        }

        /* Helper method */
        private string ReadResponse(HttpWebRequest request, IAsyncResult asyncResponse)
        {
            try
            {
                var response = (HttpWebResponse)request.EndGetResponse(asyncResponse);
                var stream = response.GetResponseStream();
                var streamReader = new StreamReader(stream);
                var serverResponse = streamReader.ReadToEnd();
                stream.Dispose();
                return serverResponse;
            }
            catch (WebException error) // Catch (400) Bad Request error
            {
                var errorStream = error.Response.GetResponseStream();
                var streamReader = new StreamReader(errorStream);
                var errorJson = streamReader.ReadToEnd();
                var responseDict = Utils.Deserialize<Dictionary<string, string>>(errorJson);
                throw new EnumWebException(responseDict["ErrorType"], responseDict["ErrorComment"]);
            }
        }
    }

    public enum HttpMethod { GET, POST, DELETE, PUT }

    public class EnumWebException : WebException
    {
        private string errorType;
        public string ErrorType { get { return errorType; } }

        public EnumWebException(string errType, string message)
            : base(errType + ": " + message)
        {
            errorType = errType;
        }
    }
}