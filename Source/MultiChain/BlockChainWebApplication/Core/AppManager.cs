using BlockChainWebApplication.Core.Entities.SAPI;
using BlockChainWebApplication.Core.Utilities;
using MultiChainLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BlockChainWebApplication.Models;
using BlockChainWebApplication.Core.Entities.Common;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using TuesPechkin;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.Web.Hosting;

namespace BlockChainWebApplication.Core
{
    public class AppManager
    {

        #region User

        protected static DatabaseEntities db
        {
            get
            {
                return DatabaseContextManager.Current;
            }
        }

        #region Session

        public static LoggedInUser User
        {
            get
            {
                return HttpContext.Current.Session["Blockchain_User"] as LoggedInUser;
            }
        }

        #endregion

        public static async Task<LoggedInUser> AuthenticateUserAsync(string username, string Password)
        {
            try
            {
                string _authString = Rijndael.Encrypt(string.Concat(username, Password));

                var _user = await (from u in db.Users
                                   where (u.Username.Equals(username) || u.Email.Equals(username)) && (u.AuthString.Equals(_authString)) && u.IsDataActive
                                   select new
                                   {
                                       u.Email,
                                       u.ID,
                                       u.Phone,
                                       u.BCHash,
                                       u.Gender,
                                       u.IsSuperAdmin,
                                       u.NickName,
                                       u.Username,
                                       u.IsSearchable,
                                   }).FirstOrDefaultAsync();

                if (_user != null)
                {
                    return new LoggedInUser
                    {
                        NickName = _user.NickName,
                        ID = _user.ID,
                        Gender = _user.Gender,
                        Email = _user.Email,
                        BCHash = _user.BCHash,
                        Phone = _user.Phone,
                        Username = _user.Username,
                        IsSuperAdmin = _user.IsSuperAdmin,
                        IsSearchable = _user.IsSearchable,
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return null;
            }
        }

        public static LoggedInUser AuthenticateUser(string Email, string Password)
        {
            try
            {
                string _authString = Rijndael.Encrypt(string.Concat(Email, Password));

                var _user = (from u in db.Users
                             where (u.Username.Equals(Email) || u.Email.Equals(Email)) && (u.AuthString.Equals(_authString)) && u.IsDataActive
                             select new
                             {
                                 u.NickName,
                                 u.Phone,
                                 u.ID,
                                 u.Email,
                                 u.BCHash,
                                 u.Gender,
                                 u.IsSuperAdmin,
                                 u.Username,
                                 u.IsSearchable,
                             }).FirstOrDefault();

                if (_user != null)
                {
                    return new LoggedInUser
                    {
                        NickName = _user.NickName,
                        ID = _user.ID,
                        Gender = _user.Gender,
                        Email = _user.Email,
                        BCHash = _user.BCHash,
                        Phone = _user.Phone,
                        Username = _user.Username,
                        IsSuperAdmin = _user.IsSuperAdmin,
                        IsSearchable = _user.IsSearchable,
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return null;
            }
        }

        #endregion

        #region BlockChain

        protected static MultiChainClient Client = new MultiChainClient("45.63.97.114", 4350, false, "multichainrpc", "EQTBzc6H3idrNB2Z8uxySGCj2uxNLKeMJK5tc4iWM8ti", "seratio_chain");

        public static async Task<string> CreateAddress()
        {
            JsonRpcResponse<string> _response = await Client.GetNewAddressAsync();

            await Client.GrantAsync(new string[] { _response.Result }, BlockchainPermissions.Send);
            await Client.GrantAsync(new string[] { _response.Result }, BlockchainPermissions.Receive);

            return Rijndael.Encrypt(_response.Result);
        }

        #endregion

        #region SAPI

        public static async Task<SAPIResponse> GetPV(string email)
        {
            using (HttpClient _client = new HttpClient())
            {
                string _responseString = await _client.GetStringAsync($"http://seratio.com/api?cmd=get&username=api@api.com&password=Zbj8Cm9gmNv3Ez56&email={email}");

                if (!string.IsNullOrEmpty(_responseString))
                {
                    return JsonConvert.DeserializeObject<SAPIResponse>(_responseString);
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<SAPIResponse> CalculatePV(Data data)
        {
            using (HttpClient _client = new HttpClient())
            {
                string _data = JsonConvert.SerializeObject(data);
                byte[] _buffer = Encoding.UTF8.GetBytes(_data);
                ByteArrayContent _byteContent = new ByteArrayContent(_buffer);

                _byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage _response = await _client.PostAsync($"http://seratio.com/api?cmd=store&username=api@api.com&password=Zbj8Cm9gmNv3Ez56", _byteContent);
                string _responseString = await _response.Content.ReadAsStringAsync();

                if (_response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<SAPIResponse>(_responseString);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        public static double CurrentGoldPricePerGram { get; set; }

        public static bool IsPVScoreUpdatorRunning = false;

        public static string AppURL
        {
            get
            {
                return $"http://{HttpContext.Current.Request.Url.Authority}/";
            }
        }

        private static IConverter PDFConverter = new ThreadSafeConverter(new RemotingToolset<PdfToolset>(new Win32EmbeddedDeployment(new TempFolderDeployment())));

        public static async Task<string> SavePDF(string url, string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string _path = path;
            Uri _url = new Uri(url);

            PDFConverter.Error += Converter_Error;

            var _document = new HtmlToPdfDocument
            {
                GlobalSettings =                    {
                        ProduceOutline = true,
                        PaperSize = PaperKind.A4,
                        Orientation = GlobalSettings.PaperOrientation.Portrait,
                        Margins = {
                    Top = 0,
                    Left = 0.00,
                    Right = 2.00,
                    Bottom = 0
                }
                    }
            };

            var _objectSettings = new ObjectSettings();
            _objectSettings.PageUrl = url;

            _document.Objects.Add(_objectSettings);

            byte[] _data = PDFConverter.Convert(_document);

            File.WriteAllBytes(path, _data);

            await Task.Delay(1000);
            return Path.GetFileName(_path);
        }

        private static void Converter_Error(object sender, TuesPechkin.ErrorEventArgs e)
        {

        }

        public static void SendEmail(string subject, string to, string content)
        {
            MailMessage _msg = new MailMessage();

            _msg.To.Add(to);
            _msg.From = new MailAddress("blockchain@seratio.com", "Seratio Blockchain");
            _msg.Subject = subject;

            string _template = File.ReadAllText(HostingEnvironment.MapPath("~/Views/Default/Template.html"));

            content = _template.Replace("{mail_body}", content);

            _msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(content, null, MediaTypeNames.Text.Html));

            using (SmtpClient _client = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587)))
            {
                _client.Credentials = new NetworkCredential("username", "1 New Credential");
                _client.Send(_msg);
            }
        }

        public static string FormatJson(object json)
        {
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        public static int GetAllWaitingDownloadRequests()
        {
            try
            {
                return db.Requests.Count(r => r.IsDataActive && (r.SenderID == User.ID) && !r.HasDownloaded && r.Status == "Approved");
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return 0;
            }
        }

        public static int GetAllRejectedRequests()
        {
            try
            {
                return db.Requests.Count(r => r.IsDataActive && (r.SenderID == User.ID) && !r.HasDownloaded && r.HasRejected);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return 0;
            }
        }

        public static int GetAllPendingRequests()
        {
            try
            {
                return db.Requests.Count(r => r.IsDataActive && (r.Recipient == User.Email || r.Recipient == User.NickName) && r.Status == "Pending");
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return 0;
            }
        }

        public static string GetNameByBCHash(string address)
        {
            if (address == "17XoaF4NcEyYWEkYeS8LQnTwBRZgBz587MFGXu")
            {
                return "Seratio Platform";
            }

            string _hash = Rijndael.Encrypt(address);

            var _user = db.Users.FirstOrDefault(u => u.BCHash == _hash);

            if (_user != null)
            {
                return string.IsNullOrEmpty(_user.NickName) ? "@" + _user.Username : _user.NickName;
            }

            return address;
        }

        public static int GetAllPendingAssetsRequests()
        {
            try
            {
                if (AppManager.User.IsSuperAdmin)
                {
                    return db.AssestsRequests.Count(r => r.Status == "Pending");

                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return 0;
            }
        }

        public static int GetAllRespondedAssestsRequests()
        {
            try
            {
                return db.AssestsRequests.Count(r => (r.HasApproved || r.HasRejected) && r.UserID == AppManager.User.ID);

            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return 0;
            }
        }

        public static TransactionMetaData GetTransactionMetaData(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<TransactionMetaData>(value);
            }
            catch
            {
                return null;
            }
        }

        public static void Prepare()
        {
            if (!Directory.Exists(HostingEnvironment.MapPath("/errors")))
            {
                Directory.CreateDirectory(HostingEnvironment.MapPath("/errors"));
            }

            if (!Directory.Exists(HostingEnvironment.MapPath("/Print")))
            {
                Directory.CreateDirectory(HostingEnvironment.MapPath("/Print"));
            }
        }
    }
}