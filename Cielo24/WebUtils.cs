using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using NLog;
using Newtonsoft.Json;

namespace Cielo24
{
    public class WebUtils
    {
        public static readonly TimeSpan BasicTimeout = new TimeSpan(TimeSpan.TicksPerSecond * 60);    // 60 seconds
        public static readonly TimeSpan DownloadTimeout = new TimeSpan(TimeSpan.TicksPerMinute * 5);  // 5 minutes
        // Made public to allow users to redirect the logger output
        public static Logger Logger = LogManager.GetLogger("WebUtils");

        /* A synchronous method that performs an HTTP request returning data received from the sever as a string */

        public string HttpRequest(Uri uri, HttpMethod method, TimeSpan timeout, Dictionary<string, string> headers = null)
        {
            return HttpRequest(uri.AbsoluteUri, method, timeout, headers);
        }

        /* A synchronous method that performs an HTTP request returning data received from the sever as a string */
        public string HttpRequest(string uri, HttpMethod method, TimeSpan timeout, Dictionary<string, string> headers = null)
        {
            Logger.Info("Uri: " + uri);
            var request = (HttpWebRequest) WebRequest.Create(uri);
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
        public string HttpRequest(string url, HttpMethod method, TimeSpan timeout, string requestBody)
        {
            var s = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            return UploadData(url, s, "password");
        }

        /* Uploads data in the body of HTTP request */
        public string UploadData(string uri, Stream inputStream, string contentType)
        {
            Debug.WriteLine("Uri: " + uri);
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Method = HttpMethod.Post.ToString();
            request.ContentType = contentType;
            request.AllowWriteStreamBuffering = false;
            request.ContentLength = inputStream.Length;

            var asyncRequest = request.BeginGetRequestStream(null, null);
            asyncRequest.AsyncWaitHandle.WaitOne(BasicTimeout); // Wait untill stream is opened
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
        private static string ReadResponse(WebRequest request, IAsyncResult asyncResponse)
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
                ErrorResponse response;
                if (Utils.TryDeserialize(errorJson, out response))
                    throw new EnumWebException(response.ErrorType, response.ErrorMessage, error);
                else
                    throw new EnumWebException(ErrorType.UnhandledError, "", error);
            }
        }
    }

    public class ErrorResponse
    {
        public ErrorType ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum HttpMethod { Get, Post, Delete, Put }

    public class EnumWebException : WebException
    {
        public ErrorType ErrorType { get; }

        public EnumWebException(ErrorType errType, string message, Exception inner)
            : base(errType + ": " + message, inner)
        {
            ErrorType = errType;
        }
    }
}