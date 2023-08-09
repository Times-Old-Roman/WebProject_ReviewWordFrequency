using DB2WebProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using DB2WebProject.DBTools;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DB2WebProject.Controllers
{
    public class SignupController : Controller
    {
		DBConnection connection { get; set; } = DBConnection.getConnection();
		SignupViewModel model = new SignupViewModel();

		private bool SignupUsernameCheck(string username)
		{
			NpgsqlDataReader reader = connection.Select($"SELECT username FROM users.user " +
				$"WHERE username = '{username}'");
			bool userExists = reader.Read();
			reader.Close();
			return userExists;
		}

		private bool LogIn(string username, string password)
		{
			NpgsqlDataReader reader = connection.Select($"SELECT password FROM users.user " +
				$"WHERE username = '{username}'");
			bool userExists = reader.Read();
			bool correctPassword = userExists ? reader.GetString(0) == password : false;
			reader.Close();
			return userExists & correctPassword;
		}

		[HttpGet]
		public IActionResult Signup()
        {
            return View(model);
        }

		[HttpPost]
		public IActionResult SignupNoAccount()
		{
			TempData["signup"] = true;
			return View("Signup", model);
		}

        [HttpPost]
        public IActionResult SignupSubmit(User user)
        {
			model.user.username = user.username;
			model.user.email = user.email;
			model.user.password = user.password;

			if(TempData["signup"] is null ? false : (bool)TempData["signup"])
			{
				TempData["usernameExists"] = SignupUsernameCheck(user.username);
				if ((bool)TempData["usernameExists"])
				{
					ModelState.AddModelError("username", "Такое имя пользователя занято");
					return View("Signup", model);
				}
				try
				{
					connection.Insert($"INSERT INTO users.user (username, email, password)" +
					$"VALUES ('{user.username}', '{user.email}', '{user.password}')");
				}
				catch (PostgresException ex)
				{
					RedirectPermanent("~/Shared/Error");
					throw ex;
				}
				TempData["loggedIn"] = true;
				TempData["user"] = user.username;
				return LocalRedirect("~/Home/Index");
			}

			bool correctInfo = LogIn(user.username, user.password);
			if (correctInfo)
			{
				TempData["loggedIn"] = true;
				TempData["user"] = user.username;
				return RedirectToAction("Index", "Home");
			}
			ModelState.AddModelError("username", "Имя пользователя или пароль введены неверно");
			return View("Signup", model);
        }
    }
}
