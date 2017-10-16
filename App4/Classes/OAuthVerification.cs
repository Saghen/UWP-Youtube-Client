using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace YTApp.Classes
{
    class OAuthVerification
    {
        const string clientID = "957928808020-1sc8669d9eeiaf7dfut2re5t21el2gcl.apps.googleusercontent.com";
        const string redirectURI = "viewer.youtube:/oauth2redirect";
        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        public void Start()
        {
            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = code_verifier;

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            Console.WriteLine("Opening authorization request URI: " + authorizationRequest);

            // Opens the Authorization URI in the browser.
            var success = Windows.System.Launcher.LaunchUriAsync(new Uri(authorizationRequest));
        }


        /// <summary>
        /// Processes the OAuth 2.0 Authorization Response
        /// </summary>
        /// <param name="e"></param>
        /*protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Uri)
            {
                // Gets URI from navigation parameters.
                Uri authorizationResponse = (Uri)e.Parameter;
                string queryString = authorizationResponse.Query;
                Console.WriteLine("MainPage received authorizationResponse: " + authorizationResponse);

                // Parses URI params into a dictionary
                // ref: http://stackoverflow.com/a/11957114/72176
                Dictionary<string, string> queryStringParams =
                        queryString.Substring(1).Split('&')
                             .ToDictionary(c => c.Split('=')[0],
                                           c => Uri.UnescapeDataString(c.Split('=')[1]));

                if (queryStringParams.ContainsKey("error"))
                {
                    Console.WriteLine(String.Format("OAuth authorization error: {0}.", queryStringParams["error"]));
                    return;
                }

                if (!queryStringParams.ContainsKey("code")
                    || !queryStringParams.ContainsKey("state"))
                {
                    Console.WriteLine("Malformed authorization response. " + queryString);
                    return;
                }

                // Gets the Authorization code & state
                string code = queryStringParams["code"];
                string incoming_state = queryStringParams["state"];

                // Retrieves the expected 'state' value from local settings (saved when the request was made).
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string expected_state = (String)localSettings.Values["state"];

                // Compares the receieved state to the expected value, to ensure that
                // this app made the request which resulted in authorization
                if (incoming_state != expected_state)
                {
                    Console.WriteLine(String.Format("Received request with invalid state ({0})", incoming_state));
                    return;
                }

                // Resets expected state value to avoid a replay attack.
                localSettings.Values["state"] = null;

                // Authorization Code is now ready to use!
                Console.WriteLine(Environment.NewLine + "Authorization code: " + code);

                string code_verifier = (String)localSettings.Values["code_verifier"];
                performCodeExchangeAsync(code, code_verifier);
            }
            else
            {
                Console.WriteLine(e.Parameter);
            }
        }*/

        async void performCodeExchangeAsync(string code, string code_verifier)
        {
            // Builds the Token request
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier
                );
            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);

            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                //Console.WriteLine("Authorization code exchange failed.");
                return;
            }

            // Sets the Authentication header of our HTTP client using the acquired access token.
            JsonObject tokens = JsonObject.Parse(responseString);
            string accessToken = tokens.GetNamedString("access_token");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Makes a call to the Userinfo endpoint, and prints the results.
            Console.WriteLine("Making API Call to Userinfo...");
            HttpResponseMessage userinfoResponse = client.GetAsync(userInfoEndpoint).Result;
            string userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
            Console.WriteLine(userinfoResponseContent);
        }

        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="Console.WriteLine">string to be appended</param>

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. Console.WriteLine will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            IBuffer buffer = CryptographicBuffer.GenerateRandom(length);
            return base64urlencodeNoPadding(buffer);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static IBuffer sha256(string inputStirng)
        {
            HashAlgorithmProvider sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(inputStirng, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(IBuffer buffer)
        {
            string base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}
