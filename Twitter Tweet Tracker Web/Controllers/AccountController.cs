using System;
using System.Web;
using System.Configuration;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using Twitter_Tweet_Tracker_Web.Models;
using Twitter_Tweet_Tracker_Web.Models.Twitter;

namespace Twitter_Tweet_Tracker_Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private string oAuthToken = string.Empty;
        private string oAuthSecret = string.Empty;

        [HttpPost]
        public ActionResult Login()
        {
            var dev_env = Environment.GetEnvironmentVariables()["DEV_ENV"];
            var isLocalEnv = dev_env?.Equals("1") ?? false;
            var client = new RestClient("https://api.twitter.com")
            {
                Authenticator = new OAuth1Authenticator()
                {
                    CallbackUrl = isLocalEnv ? "https://localhost:44313/account/logincallback" : "",
                    ConsumerKey = ConfigurationManager.AppSettings["twitterAPIKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["twitterAPISecret"],
                    SignatureMethod = OAuthSignatureMethod.HmacSha1
                }
            };

            try
            {
                var request = new RestRequest("oauth/request_token");
                var response = client.Execute(request);
                var processTokenResult = ProcessTokenResponse(response.Content);
                if (processTokenResult != HttpStatusCode.OK)
                {
                    ViewBag.Errors = $"oAuth response error";
                    return RedirectToAction("Index", "Home");
                }

                return Redirect($"https://api.twitter.com/oauth/authorize?oauth_token={oAuthToken}");


            }
            catch (Exception e)
            {
                ViewBag.Errors = $"oAuth response error {e.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LoginCallback(string oauth_token, string oauth_verifier)
        {

            var client = new RestClient("https://api.twitter.com");

            try
            {
                var request = new RestRequest("oauth/access_token");
                request.AddParameter("oauth_token", oauth_token);
                request.AddParameter("oauth_verifier", oauth_verifier);
                var response = client.Execute(request, Method.Post);
                var user = new TTTUser();
                var processTokenResult = ProcessTokenResponse(response.Content, ref user);
                if (processTokenResult != HttpStatusCode.OK)
                {
                    return RedirectToAction("Index", "Home");
                }

                var _twitterUser = GetTwitterUser(user.user_id, this.oAuthToken, oAuthSecret);
                if (_twitterUser != null)
                {
                    user.name = _twitterUser.data?.name;
                    user.profileImage = _twitterUser.data?.profile_image_url;
                    if (_twitterUser.data?.public_metrics != null)
                    {
                        user.followers_count = _twitterUser.data.public_metrics.followers_count;
                        user.following_count = _twitterUser.data.public_metrics.following_count;
                        user.tweet_count = _twitterUser.data.public_metrics.tweet_count;
                    }
                    
                }
                ViewBag.user = user;
                return RedirectToAction("Index", "Home", new RouteValueDictionary(user));
            }
            catch (Exception e)
            {
                ViewBag.Errors = $"oAuth response error {e.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public static TwitterUser GetTwitterUser (string userId, string oAuthToken, string oAuthSecret)
        {
            var client = new RestClient("https://api.twitter.com/2")
            {
                Authenticator = new OAuth1Authenticator()
                {
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ConsumerKey = ConfigurationManager.AppSettings["twitterAPIKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["twitterAPISecret"],
                    TokenSecret = oAuthSecret,
                    Token = oAuthToken,
                    Type = OAuthType.ProtectedResource

                },
            };

            try
            {
                var request = new RestRequest($"users/{userId}");
                request.AddParameter("user.fields", "profile_image_url");
                var response = client.Execute(request);
                var data = response.Content;
                if (!string.IsNullOrWhiteSpace(data))
                {
                    var twitterUser = JsonConvert.DeserializeObject<TwitterUser>(response.Content);
                    return twitterUser;
                }
                
            }
            catch (Exception e)
            {
                
            }
            return null;

        }
        #region helpers

        public HttpStatusCode ProcessTokenResponse (string response, ref TTTUser user)
        {
            try
            {
                var items = response.Split('&');
                foreach (var item in items)
                {
                    var keyValue = item.Split('=');
                    if (keyValue[0] == "oauth_token")
                    {
                        oAuthToken = keyValue[1];
                        if (user != null)
                            user.oAuthToken = keyValue[1];
                    }
                    if (keyValue[0] == "oauth_token_secret")
                    {
                        oAuthSecret = keyValue[1];
                        if (user != null)
                            user.oAuthSecret = keyValue[1];
                    }

                    if (user != null)
                    {
                        if (keyValue[0] == "user_id")
                            user.user_id = keyValue[1];
                        if (keyValue[0] == "screen_name")
                            user.username = keyValue[1];
                    }
                   
                }

                return HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                ViewBag.Errors = $"oAuth response error {e.Message}";
                return HttpStatusCode.InternalServerError;

            }
        }

        public HttpStatusCode ProcessTokenResponse(string response)
        {
            TTTUser _user = null;
            return ProcessTokenResponse(response, ref _user);
        }
        
        #endregion
    }
}