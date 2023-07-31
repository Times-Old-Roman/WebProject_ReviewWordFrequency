using DB2WebProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PuppeteerSharp;
using PuppeteerExtraSharp;
using System.Runtime.CompilerServices;
using System.Collections;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using System.Text;
using DB2WebProject.DBTools;
using Npgsql;
using System.Text.Json.Nodes;
using PuppeteerExtraSharp.Plugins.ExtraStealth;

namespace DB2WebProject.Controllers
{
	public class HomeController : Controller
	{
		IndexViewModel model = new IndexViewModel(false);

		private async Task<string[]> getData(string website, string link)
		{
			NpgsqlDataReader reader = DBConnection.getConnection().Select(
				$"SELECT parse_script, args FROM users.website WHERE name = '{website}'");
			reader.Read();
			string parseScript = reader.GetString(0);
			string[] args = reader.GetString(1).Split("\\,");
			args[0] = args[0].Remove(0,1);
			args[args.Length - 1] = args[args.Length - 1].Remove(args[args.Length - 1].Length - 1);
			reader.Close();

			var extraOptions = new PuppeteerExtra();
			extraOptions.Use(new StealthPlugin());
			LaunchOptions options = new LaunchOptions()
			{
				Headless = true,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
			};
			IBrowser browser = await extraOptions.LaunchAsync(options);
			IPage page = await browser.NewPageAsync();
			
			var goFunc = page.GoToAsync(link);
			await goFunc;
			while (!goFunc.IsCompleted) { }
			Thread.Sleep(3000);

			List<string> res = new List<string>();
			if (args[0] == "infiniteScroll" || args[0] == "clickScroll")
			{
				Task<string[]> evaluation = page.EvaluateFunctionAsync<string[]>(parseScript);
				string[] result = evaluation.Result;
				while(!evaluation.IsCompleted) { }
				res.AddRange(result);
			}
			if (args[0] == "pages")
			{
				string[] newPageAvailible;
				do
				{
					newPageAvailible = (await page.EvaluateFunctionAsync<string[]>(args[1]));
					res.AddRange(await page.EvaluateFunctionAsync<string[]>(parseScript));
					if (newPageAvailible[0] == "true")
					{
						await page.GoToAsync(newPageAvailible[1]);
						Thread.Sleep(200);
					}
				} while (newPageAvailible[0] == "true");
			}
			if(website == "yandex_market")
			{
				for (int i = 0; i < res.Count; i++)
				{
					if(res[i].IndexOf("Опыт использования:") != -1)
						res[i] = res[i].Remove(res[i].IndexOf("Опыт использования:"), "Опыт использования:".Length);
					if (res[i].IndexOf("Достоинства:") != -1)
						res[i] = res[i].Remove(res[i].IndexOf("Достоинства:"), "Достоинства:".Length);
					if (res[i].IndexOf("Недостатки:") != -1)
						res[i] = res[i].Remove(res[i].IndexOf("Недостатки:"), "Недостатки:".Length);
					if (res[i].IndexOf("Комментарий:") != -1)
						res[i] = res[i].Remove(res[i].IndexOf("Комментарий:"), "Комментарий:".Length);
				}
			}
			await page.CloseAsync();
			await browser.CloseAsync();
			browser.Dispose();
			return res.ToArray();
		}

		private Dictionary<string, int> parseReviews(in List<string> reviews)
		{
			Dictionary<string, int> wordsToFreq = new Dictionary<string, int>();
			foreach (string review in reviews)
			{
				string[] delimiters = new string[]{ " ", ",", ".", "?", "!", ";", ":", "\"", "\'", "(", ")", "\n", "\r"};
				string[] words = review.Split(delimiters, options: StringSplitOptions.TrimEntries);
				foreach (string word in words) 
				{
					if (wordsToFreq.ContainsKey(word))
						wordsToFreq[word]++;
					else wordsToFreq.Add(word, 1);
				}
			}
			foreach (string word in wordsToFreq.Keys)
				if (word.Any(Char.IsDigit))
					wordsToFreq.Remove(word);
			wordsToFreq.Remove("");
			return wordsToFreq.OrderBy(x => -x.Value).ToDictionary(x => x.Key, x => x.Value);
		}

        [HttpGet]
		public IActionResult Index()
		{
			if (TempData["loggedIn"] is not null)
				model.loggedIn = (bool)TempData["loggedIn"];

			return View(model);
		}

		[HttpGet]
        public IActionResult Signup()
        {
            return Redirect("~/Signup/Signup");
        }

        [HttpPost]
        public ActionResult productLinkSubmit(Uri uri)
        {
			string link = uri.ToString();
			string website = uri.Host.ToString();
			
			List<string> reviews = new List<string>();
			Task<string[]> getDataProcess = getData(website, link);
			string[] reviewsGet = getDataProcess.Result;

			foreach (string review in reviewsGet)
				reviews.Add(review);

			model.wordsToFreq = parseReviews(reviews);

			return View("Index", model);
        }

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}