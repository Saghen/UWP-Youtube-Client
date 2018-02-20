using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Models.MediaStreams;

namespace YTApp.Classes
{
    static class Constants
    {
        static public readonly string ClientID = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com";
        static public readonly string ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm";
        static public readonly string APIKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0";
        static public Google.Apis.Auth.OAuth2.Responses.TokenResponse Token;

        static public MainPage MainPageRef;

        static public Video activeVideo = new Video();
        static public string activeVideoID = "";

        static public Channel activeChannel = new Channel();
        static public string activeChannelID = "";

        static public MediaStreamInfoSet videoInfo;
    }
}
