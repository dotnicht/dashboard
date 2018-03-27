namespace InvestorDashboard.Api.Models.AccountViewModels
{
    public class RegisterViewModel : UserViewModel
    {
        public string ClickId { get; set; }
        public string ReferralCode { get; set; }
        public string Password { get; set; }
        public string ReCaptchaToken{ get; set; }
    }
}
