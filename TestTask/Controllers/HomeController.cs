using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
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
                Session[_userStatSessionKey] = new List<UserUrlData>(); //initialize storage to keep user data. It will be used to form stats after.

            return View();
        }

        [HttpGet]
        public ActionResult Statistics()
        {
            return View(Session[_userStatSessionKey]);
        }

        [HttpGet]
        public ActionResult Chart(string userUrl)
        {
            Session[_urlDetailsSessionKey] = userUrl;
            return View();
        }

        [HttpPost]
        public ActionResult GetChartDataAjax()
        {
            string userUrl = Session[_urlDetailsSessionKey] as string;

            if (!userUrl.IsNullOrWhiteSpace())
            {
                Models.Chart chart = new Models.Chart();
                chart.series = new List<Series>();
                List<UserUrlData> userUrlsList = Session[_userStatSessionKey] as List<UserUrlData>;

                var suitableUrlsByUserUrl = userUrlsList.Where(elem => elem.UserUrl == userUrl);

                chart.categories = suitableUrlsByUserUrl
                    .Select(elem => elem.Date.ToShortDateString())
                    .Distinct()
                    .ToArray();

                var allUniqueCodes = suitableUrlsByUserUrl.Select(elem => elem.StatusCode).Distinct();
                List<int> tempList = new List<int>();
                foreach (var code in allUniqueCodes) //in all unique codes...
                {
                    Series series = new Series
                    {
                        name = code.ToString()
                    };

                    foreach (var date in chart.categories) //...by all unique dates...
                    {
                        tempList.Add(
                            suitableUrlsByUserUrl           //...form the right series according to js chart API
                            .Where(elem => elem.Date.ToShortDateString() == date)
                            .Where(elem => elem.StatusCode == code)
                            .Count()
                            );
                    }

                    series.data = tempList.ToArray();
                    chart.series.Add(series);
                    tempList.Clear();
                }


                return Json(chart, JsonRequestBehavior.AllowGet);
            }
            return Content("Error"); 
        }


        [HttpPost]
        public ActionResult GetUserDataFromUrlAjax(string userUrl)
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
                    statusCode = exceptionResponse != null ? (int)exceptionResponse.StatusCode : 403;
                }
            }
            else
            {
                title = "Некорректный формат url.";
            }

            List<UserUrlData> userStatList = Session[_userStatSessionKey] as List<UserUrlData>;

            userStatList.Add(new UserUrlData
            {
                Date = DateTime.Now,
                UserUrl = userUrl,
                Title = title,
                StatusCode = statusCode
            });

            return new JsonResult() { Data = new { userUrl, title, statusCode } };
        }
    }
}