namespace DPMS_WebAPI.ViewModels
{
	public class RegisterVM
	{
		public string UserName { get; set; }
		public string FullName { get; set; }
		public DateTime Dob { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string ReTypePassword { get; set; }
	}
}
