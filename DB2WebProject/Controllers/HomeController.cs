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
using System.Text.RegularExpressions;

namespace DB2WebProject.Controllers
{
	public class HomeController : Controller
	{
		private static IndexViewModel model;

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

		private async Task<Dictionary<string, int>> checkAdjective(Dictionary<string, int> wordsToFreq, int count)
		{
			var extraOptions = new PuppeteerExtra();
			extraOptions.Use(new StealthPlugin());
			LaunchOptions options = new LaunchOptions()
			{
				Headless = true,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
			};
			IBrowser browser = await extraOptions.LaunchAsync(options);
			IPage page = await browser.NewPageAsync();

			var goFunc = page.GoToAsync("https://morphologyonline.ru/");
			await goFunc;
			while(!goFunc.IsCompleted) { }

			var submitFunction = (string s) => "async () => { \r\n" +
				$"	document.querySelectorAll('input[type=\"text\"]')[0].value = \"{s}\";\r\n" +
				"	document.querySelectorAll('button[type=\"submit\"]')[0].click();\r\n}";

			var checkFunction = "async () => { let arr = document.querySelectorAll('li');\r\n" +
				"	for (let i = 0; i < arr.length; i++)\r\n" +
				"		if(arr[i].textContent.includes('имя прилагательное'))\r\n" +
				"			return true; }";

			List<string> strings = new List<string>(wordsToFreq.Select(s => s.Key).ToList());
			Dictionary<string, int> topWords = new Dictionary<string, int>();
			for (int i = 0; i < wordsToFreq.Count; i++)
			{
				if (topWords.Count >= count)
					break;
				await page.EvaluateFunctionAsync(submitFunction(strings[i]));
				await page.WaitForNavigationAsync();
				if(page.EvaluateFunctionAsync<bool>(checkFunction).Result)
					topWords.Add(strings[i], wordsToFreq[strings[i]]);
				await page.GoBackAsync();
			}

			return topWords;
		}

		private Dictionary<string, int> parseReviews(in List<string> reviews)
		{
			Dictionary<string, int> wordsToFreq = new Dictionary<string, int>();
			List<string> words = new List<string>();

			foreach (string review in reviews)
			{
				string[] delimiters = new string[]{ " ", ",", ".", "?", "!", ";", ":", "\"", "\'", "(", ")", "\n", "\r"};
				string[] splitWords = review.Split(delimiters, options: StringSplitOptions.TrimEntries);
				words.AddRange(splitWords);
			}
			
			for (int i = 0; i < words.Count; i++)
			{
				if (!Regex.IsMatch(words[i], "^[А-Яа-я]+$"))
				{
					words.RemoveAt(i);
					i--;
				}
				else
					words[i] = String.Concat(words[i][0].ToString().ToUpper(), words[i].Substring(1));
			}
			
			foreach (string word in words) 
			{
				if (wordsToFreq.ContainsKey(word))
					wordsToFreq[word]++;
				else wordsToFreq.Add(word, 1);
			}

			return wordsToFreq.OrderBy(x => -x.Value).ToDictionary(x => x.Key, x => x.Value);
		}

        [HttpGet]
		public IActionResult Index()
		{
			if (model == null)
				model = new IndexViewModel(false);

			if (TempData["loggedIn"] is not null)
			{
				model.loggedIn = (bool)TempData["loggedIn"];
				model.username = (string)TempData["user"];
			}

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

			Dictionary<string, int> dic = parseReviews(reviews);
			model.wordsToFreq = checkAdjective(dic, Math.Min(10, dic.Count)).Result;

			TempData["productURL"] = link;
			model.uriToProduct = uri;

			return View("Index", model);
        }

		[HttpPost]
		public IActionResult saveToProfile(string productName, string userComment)
		{
			StringBuilder summary = new StringBuilder();
			foreach (string s in model.wordsToFreq.Keys)
				summary.Append(s + ", ");
			summary.Remove(summary.Length - 2, 2);
			NpgsqlDataReader reader = DBConnection.getConnection().Select(
				$"SELECT * FROM users.product_summary " +
				$"WHERE username = '{model.username}' AND link = '{model.uriToProduct}'");
			if (reader.Read())
			{
				reader.Close();
				DBConnection.getConnection().Update(
				$"UPDATE users.product_summary " +
				$"SET product_name = '{productName}', summary = '{summary}', " +
				$"user_comment = '{userComment}' " +
				$"WHERE username = '{model.username}' AND link = '{model.uriToProduct}'");
			}
			else DBConnection.getConnection().Insert($"INSERT INTO " +
				$"users.product_summary(link, product_name, summary, user_comment, username, website) " +
				$"VALUES ('{model.uriToProduct}', '{productName}', '{summary}', '{userComment}', " +
				$"'{model.username}', '{model.uriToProduct.Host}')");

			TempData["saved"] = true;
			return View("Index", model);
		}

		[HttpGet]
		public IActionResult profile()
		{
			TempData["username"] = model.username;
			return Redirect("~/Profile/Profile");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}