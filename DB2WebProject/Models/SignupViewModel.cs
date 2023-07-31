namespace DB2WebProject.Models
{
	public class SignupViewModel
	{
		public User user { get; set; } = new User();
		public bool signupMode { get; set; } = false;

		public SignupViewModel() { }

		public SignupViewModel(User user)
		{
			this.user = user;
		}
	}
}
