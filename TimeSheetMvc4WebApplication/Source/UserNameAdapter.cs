using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.Source
{
    public class UserNameAdapter
    {
        public static string Adapt(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            if (username == "ALEXEY-PC\\Alexey") username = "atipunin@ugtu.net";
            if (username.ToLower().StartsWith(@"ugtu\".ToLower()))
            {
                username =
                    string.Format("{0}@{1}.net", username.Substring(5, username.Length - 5),
                        username.Substring(0, 4)).ToLower();
            }
            return username;
        }
    }
}