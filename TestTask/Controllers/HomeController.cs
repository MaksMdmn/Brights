using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TestTask.DTOs;
using TestTask.Models;

namespace TestTask.Controllers
{
    public class HomeController : Controller
    {
        readonly char[] _separator = Environment.NewLine.ToCharArray();
        readonly string _userStatSessionKey = "userstat";
        readonly string _urlDetailsSessionKey = "urldetails";

        public ActionResult Index()
        {
            if (Session[_userStatSessionKey] == null)
                Session[_userStatSessionKey] = new List<UserUrlData>(); //initialize storage to keep user data. It will be used to make stats after.

            return View();
        }

        [HttpGet]
        public ActionResult Statistics()
        {
            return View(Session[_userStatSessionKey]);
        }

        [HttpGet]
        public ActionResult Chart(string userUrl) //TODO still working on additional task...
        {
            Session[_urlDetailsSessionKey] = userUrl;
            return View();
        }

        [HttpPost]
        public JsonResult GetChartDataAjax() //TODO still working on additional task...
        {
            string userUrl = Session[_urlDetailsSessionKey] as string;
            if (!userUrl.IsNullOrWhiteSpace())
            {
                var userList = Session[_userStatSessionKey] as List<UserUrlData>;

                var correctUrls = userList.Where(elem => elem.UserUrl == userUrl);
                var uniqueCodes = correctUrls.Select(elem => elem.StatusCode).Distinct();



                return new JsonResult();

            }
            return new JsonResult();
        }


        [HttpPost]
        public JsonResult GetUserDataFromUrlAjax(string userUrl)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            StreamReader reader;
            string html = string.Empty;
            string title = string.Empty;
            int statusCode = 0;
            Uri url;
            bool IsUrlCorrect;

            IsUrlCorrect = Uri.TryCreate(userUrl, UriKind.Absolute, out url)
                && (url.Scheme == Uri.UriSchemeHttp
                    || url.Scheme == Uri.UriSchemeHttps);

            if (IsUrlCorrect)
            {
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                    using (response = (HttpWebResponse)request.GetResponse())
                    using (reader = new StreamReader(response.GetResponseStream()))
                    {

                        html = reader.ReadToEnd();
                        title = Regex.Match(
                           html,
                           @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                           RegexOptions.IgnoreCase)
                           .Groups["Title"].Value;
                        statusCode = (int)response.StatusCode;
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse exceptionResponse = (HttpWebResponse)ex.Response;
                    title = "Не удалось перейти по данному url";
                    statusCode = exceptionResponse != null ? (int)exceptionResponse.StatusCode : 0;
                }
            }
            else
            {
                title = "Некорректный формат url.";
            }

            AddDataToSession(userUrl, title, statusCode);

            return new JsonResult() { Data = new { userUrl, title, statusCode } };
        }

        private void AddDataToSession(string userUrl, string title, int statusCode)
        {
            List<UserUrlData> userStatList = Session[_userStatSessionKey] as List<UserUrlData>;

            userStatList.Add(new UserUrlData
            {
                Date = DateTime.Now,
                UserUrl = userUrl,
                Title = title,
                StatusCode = statusCode
            });
        }
    }
}