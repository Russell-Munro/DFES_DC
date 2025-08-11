using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using UDC.Common;
using UDC.Common.Data;
using UDC.Common.Database.Data.Models.Database;
using UDC.Common.Database.Data;

namespace DataConnectorUI.Services
{
    public class AuthSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<AuthSessionService> _logger;
       // private readonly IAppSettings AppSettings;

        private const string CookieKey = "st";

        public AuthSessionService(
            IHttpContextAccessor httpContextAccessor,
            DatabaseContext dbContext,
            ILogger<AuthSessionService> logger
            //IAppSettings appSettings
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _logger = logger;
           // AppSettings = appSettings;
        }

        private string GetUISessionToken()
        {
            var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
            if (cookies != null && cookies.ContainsKey(CookieKey))
            {
                var value = cookies[CookieKey];
                if (!string.IsNullOrEmpty(value))
                {
                    return GeneralHelpers.parseString(value);
                }
            }
            return string.Empty;
        }

        private void SetUISessionToken(string value, DateTime expires)
        {
            var options = new CookieOptions
            {
                IsEssential = true,
                HttpOnly = true,
                Path = "/",
                Expires = expires,
                Secure = false, //dev setting only! change to true for production with HTTPS
                SameSite = SameSiteMode.Lax //dev only, change to None for production with HTTPS
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieKey, value, options);
        }

        public UIUser Authenticate(string remoteToken, int clientTZOffsetMins)
        {
            UIUser retVal = null;
            var objRemoteToken = ParseRemoteToken(remoteToken);

            if (objRemoteToken != null)
            {
                DateTime dtTokenDate = GeneralHelpers.parseDate(objRemoteToken["TokenDate"].ToString());
                int remoteTokenExpiryMins = GeneralHelpers.parseInt32(AppSettings.GetValue("RemoteTokenExpiryMinutes"));

                if (dtTokenDate != DateTime.MinValue && DateTime.UtcNow.Subtract(dtTokenDate).TotalMinutes <= remoteTokenExpiryMins)
                {
                    string sessionID = GetCurrentSessionID();

                    if (!string.IsNullOrEmpty(sessionID) && GeneralHelpers.parseGUID(sessionID) != Guid.Empty)
                    {
                        retVal = _dbContext.UIUsers.FirstOrDefault(obj => obj.sessionID == sessionID);
                    }
                    if (retVal == null)
                    {
                        sessionID = Guid.NewGuid().ToString();
                        retVal = new UIUser();
                        _dbContext.UIUsers.Add(retVal);
                    }

                    retVal.sessionID = sessionID;
                    retVal.remoteUserId = GeneralHelpers.parseString(objRemoteToken["UserId"]);
                    retVal.Username = GeneralHelpers.parseString(objRemoteToken["Username"]);
                    retVal.Email = GeneralHelpers.parseString(objRemoteToken["Email"]);
                    retVal.IsConnectionAdmin = GeneralHelpers.parseBool(objRemoteToken["IsConnectionAdmin"]);
                    retVal.IsSyncManager = GeneralHelpers.parseBool(objRemoteToken["IsSyncManager"]);
                    retVal.ReferringApplication = GeneralHelpers.parseString(objRemoteToken["ReferringApplication"]);
                    retVal.ReferringApplicationURL = GeneralHelpers.parseString(objRemoteToken["ReferringApplicationURL"]);
                    retVal.LastAccessed = DateTime.UtcNow;

                    if (clientTZOffsetMins != 0)
                    {
                        retVal.TZOffsetMins = clientTZOffsetMins;
                    }

                    _dbContext.SaveChanges();

                    string cryptoKey = AppSettings.GetValue("LocalKey");
                    int sessionLengthMins = GeneralHelpers.parseInt32(AppSettings.GetValue("UISessionLengthMinutes"));
                    string sessionToken = Cryptor.Encrypt(sessionID, cryptoKey);
                    int tzOffset = 0;

                    if (retVal.TZOffsetMins != 0)
                    {
                        tzOffset = ((retVal.TZOffsetMins * -1) / 60);
                    }

                    SetUISessionToken(sessionToken, DateTime.UtcNow.AddHours(tzOffset).AddMinutes(sessionLengthMins));
                }
            }

            return retVal;
        }

        public bool IsAuthenticated()
        {
            bool retVal = false;
            var objCurrUser = GetCurrentUser();

            if (objCurrUser != null)
            {
                long sessionTimeoutMins = GeneralHelpers.parseInt64(AppSettings.GetValue("UISessionLengthMinutes"));

                if (DateTime.UtcNow.Subtract(objCurrUser.LastAccessed).TotalMinutes <= sessionTimeoutMins)
                {
                    retVal = true;

                    _dbContext.Attach(objCurrUser);
                    objCurrUser.LastAccessed = DateTime.UtcNow;
                    _dbContext.SaveChanges();
                }
            }
            ClearExpiredSessions();

            return retVal;
        }

        public string GetCurrentSessionID()
        {
            string sessionToken = GetUISessionToken();
            return ParseSessionToken(sessionToken);
        }

        public UIUser GetCurrentUser()
        {
            UIUser retVal = null;
            string sessionID = GetCurrentSessionID();

            if (!string.IsNullOrEmpty(sessionID) && GeneralHelpers.parseGUID(sessionID) != Guid.Empty)
            {
                retVal = _dbContext.UIUsers.FirstOrDefault(obj => obj.sessionID == sessionID);
            }

            return retVal;
        }

        private string ParseSessionToken(string token)
        {
            string retVal = "";
            string cryptoKey = AppSettings.GetValue("LocalKey");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(cryptoKey))
            {
                try
                {
                    retVal = Cryptor.Decrypt(token, cryptoKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ParseSessionToken failed");
                }
            }

            return retVal;
        }

        private Dictionary<string, object> ParseRemoteToken(string token)
        {
            Dictionary<string, object> retVal = null;
            string cryptoKey = AppSettings.GetValue("SharedKey");
            string decrypted = "";

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(cryptoKey))
            {
                try
                {
                    decrypted = Cryptor.Decrypt(token, cryptoKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ParseRemoteToken failed");
                }
                if (!string.IsNullOrEmpty(decrypted))
                {
                    retVal = JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
                }
            }

            return retVal;
        }

        private void ClearExpiredSessions()
        {
            long sessionTimeoutMins = GeneralHelpers.parseInt64(AppSettings.GetValue("UISessionLengthMinutes"));

            DateTime cutoffTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(sessionTimeoutMins));

            _dbContext.UIUsers.RemoveRange(
                _dbContext.UIUsers.Where(obj => EF.Functions.DateDiffMinute(obj.LastAccessed, DateTime.UtcNow) >= sessionTimeoutMins)
                    .ToList()
            );

            _dbContext.SaveChanges();
        }
    }

    //public interface IAppSettings
    //{
    //    string GetValue(string key);
    //}
}