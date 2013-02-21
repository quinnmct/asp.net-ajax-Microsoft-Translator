﻿using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Threading;

public partial class Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {                                                     //this may or may not be sensitive information
        AdmAuthentication admAuth = new AdmAuthentication("vernaculatetranslate", "LqLdkBdQ+I/DasFZ6EKBRKvlxmaTlBQPcWKV4Srs3eQ=");
        AdmAccessToken token = admAuth.GetAccessToken();

        Response.Write(string.Format(@"
                <script type=""text/javascript"">
                    window.accessToken = ""{0}"";
                </script>", token.access_token));

    }

    public string getCurrentLang()
    {
        Console.WriteLine(langChoice.SelectedIndex.ToString());
        return "es";
    }
}

class Program
{
    static void Main(string[] args)
    {
        AdmAccessToken admToken;
        string headerValue;
        //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
        //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
        AdmAuthentication admAuth = new AdmAuthentication("clientID", "client secret");
        try
        {
            admToken = admAuth.GetAccessToken();
            // Create a header with the access_token property of the returned token
            headerValue = "Bearer " + admToken.access_token;
            DetectMethod(headerValue);
        }
        catch (WebException e)
        {
            ProcessWebException(e);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    private static void DetectMethod(string authToken)
    {
        Console.WriteLine("Enter Text to detect language:");
        string textToDetect = Console.ReadLine();
        //Keep appId parameter blank as we are sending access token in authorization header.
        string uri = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + textToDetect;
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
        httpWebRequest.Headers.Add("Authorization", authToken);
        WebResponse response = null;
        try
        {
            response = httpWebRequest.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                string languageDetected = (string)dcs.ReadObject(stream);
                Console.WriteLine(string.Format("Language detected:{0}", languageDetected));
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        catch
        {
            throw;
        }
        finally
        {
            if (response != null)
            {
                response.Close();
                response = null;
            }
        }
    }
    private static void ProcessWebException(WebException e)
    {
        Console.WriteLine("{0}", e.ToString());
        // Obtain detailed error information
        string strResponse = string.Empty;
        using (HttpWebResponse response = (HttpWebResponse)e.Response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                {
                    strResponse = sr.ReadToEnd();
                }
            }
        }
        Console.WriteLine("Http status code={0}, error message={1}", e.Status, strResponse);
    }
}
[DataContract]
public class AdmAccessToken
{
    [DataMember]
    public string access_token { get; set; }
    [DataMember]
    public string token_type { get; set; }
    [DataMember]
    public string expires_in { get; set; }
    [DataMember]
    public string scope { get; set; }
}

public class AdmAuthentication
{
    public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
    private string clientId;
    private string clientSecret;
    private string request;
    private AdmAccessToken token;
    private Timer accessTokenRenewer;

    //Access token expires every 10 minutes. Renew it every 9 minutes only.
    private const int RefreshTokenDuration = 9;

    public AdmAuthentication(string clientId, string clientSecret)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        //If clientid or client secret has special characters, encode before sending request
        this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
        this.token = HttpPost(DatamarketAccessUri, this.request);
        //renew the token every specfied minutes
        accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback), this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
    }

    public AdmAccessToken GetAccessToken()
    {
        return this.token;
    }


    private void RenewAccessToken()
    {
        AdmAccessToken newAccessToken = HttpPost(DatamarketAccessUri, this.request);
        //swap the new token with old one
        //Note: the swap is thread unsafe
        this.token = newAccessToken;
        Console.WriteLine(string.Format("Renewed token for user: {0} is: {1}", this.clientId, this.token.access_token));
    }

    private void OnTokenExpiredCallback(object stateInfo)
    {
        try
        {
            RenewAccessToken();
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
        }
        finally
        {
            try
            {
                accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
            }
        }
    }


    private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
    {
        //Prepare OAuth request 
        WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
        webRequest.ContentType = "application/x-www-form-urlencoded";
        webRequest.Method = "POST";
        byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
        webRequest.ContentLength = bytes.Length;
        using (Stream outputStream = webRequest.GetRequestStream())
        {
            outputStream.Write(bytes, 0, bytes.Length);
        }
        using (WebResponse webResponse = webRequest.GetResponse())
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
            //Get deserialized object from JSON stream
            AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
            return token;
        }
    }
}