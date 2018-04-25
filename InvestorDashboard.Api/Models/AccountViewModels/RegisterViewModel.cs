namespace InvestorDashboard.Api.Models.AccountViewModels
{
    public class RegisterViewModel : UserViewModel
    {
        public string ClickId { get; set; }
        public string Referral { get; set; }
        public string UtmSource { get; set; }
        public string StartUrl { get; set; }
        public string Password { get; set; }
        public string ReCaptchaToken{ get; set; }
    }
}
