using Hedonist.Business;
using Hedonist.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hedonist.WebApi.Controllers {
    public class ReportController : Controller {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult Index() {
            return View();
        }

        [HttpGet]
        [Route("PurchasedGifts")]
        // GET: ReportController/PurchasedGifts/AccessString
        public async Task<ActionResult> PurchasedGiftsAsync(string accessString) {
            //TODO:move AccessKey to database or settings
            const string AccessKey = "385D145D-4B16-4306-915D-6738D04BC9B4";
            try {
                if (accessString == AccessKey) {
                    logger.Debug($"IN PurchasedGifts(accessString={accessString})");
                    string reportString = await new QuizEngine().GetPurchasedReportSCVStringAsync();
                    var bytes = Encoding.UTF8.GetBytes(reportString);
                    var result = Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
                    string fileName = $"Гедонист отчет " + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + ".csv";
                    return File(result, "text/csv", fileName);

                } else {
                    logger.Debug($"OUT PurchasedGifts(accessString={accessString}) return Status404NotFound");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception ex) {
                logger.Error(ex, $"PurchasedGifts(accessString={accessString}) EXCEPTION");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("LeftGifts")]
        // GET: ReportController/PurchasedGifts/AccessString
        public async Task<ActionResult> LeftGiftsAsync(string accessString) {
            //TODO:move AccessKey to database or settings
            const string AccessKey = "7241853D-D9DE-4DFF-BEC2-B1FEAA8DFE59";
            try {
                if (accessString == AccessKey) {
                    logger.Debug($"IN LeftGiftsAsync(accessString={accessString})");
                    string reportString = await new QuizEngine().GetLeftGiftsSCVStringAsync();
                    var bytes = Encoding.UTF8.GetBytes(reportString);
                    var result = Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
                    string fileName = $"Гедонист остатки " + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + ".csv";
                    return File(result, "text/csv", fileName);

                }
                else {
                    logger.Debug($"OUT LeftGiftsAsync(accessString={accessString}) return Status404NotFound");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception ex) {
                logger.Error(ex, $"LeftGiftsAsync(accessString={accessString}) EXCEPTION");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }

}
