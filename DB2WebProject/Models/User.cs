using System.ComponentModel.DataAnnotations;

namespace DB2WebProject.Models
{
    public class User
    {
        [Required(ErrorMessage = "Не указано имя")]
		[StringLength(50, ErrorMessage = "Длина имени должна быть не менее 3 символов и не более 50")]
		[RegularExpression(@"[A-Za-z0-9_\-.]{3,}",
						   ErrorMessage = "Имя может содержать только латинские буквы, цифры и символы \"_\", \"-\" и \".\"")]
		[Display(Name = "Имя пользователя")]
        public string? username { get; set; }

		[Required(ErrorMessage = "Не указана электронная почта")]
		[EmailAddress(ErrorMessage = "Не верно введена электронная почта")]
		[Display(Name = "Электронная почта")]
		public string? email { get; set; }

		[Required(ErrorMessage = "Не указан пароль")]
		[StringLength(30, MinimumLength = 6, ErrorMessage = "Длина пароля должна быть не менее 6 символов и не более 30")]
		[RegularExpression(@"(?=.*[a-z])(?=.*\d)[A-Za-z\d]{6,}", ErrorMessage = "Пароль должен содержать хотя бы 1 букву и цифру")]
		[Display(Name = "Пароль")]
		public string? password { get; set; }

		[Required(ErrorMessage = "Не указан пароль")]
		[Compare("password", ErrorMessage = "Пароли не совпадают")]
		[Display(Name = "Повторите пароль")]
		public string? passwordCheck { get; set; }

		public User() { }
		public User(string username, string email, string password, string passwordCheck) 
        {
            this.username = username;
            this.email = email;
            this.password = password;
			this.passwordCheck = passwordCheck;
        }
    }
}
