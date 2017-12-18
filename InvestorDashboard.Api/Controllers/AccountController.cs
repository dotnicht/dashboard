using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using InvestorDashboard.Api.Models;
using InvestorDashboard.Api.Models.AccountViewModels;
using InvestorDashboard.Api.Models.ManageViewModels;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private const string GetUserByIdActionName = "GetUserById";
        private const string GetRoleByIdActionName = "GetRoleById";

        private readonly IAuthorizationService _authorizationService;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AccountController(
          ApplicationDbContext context,
          UserManager<ApplicationUser> userManager,
          IAuthorizationService authorizationService,
          SignInManager<ApplicationUser> signInManager,
          ILogger<AccountController> logger,
          IOptions<IdentityOptions> identityOptions,
          IMessageService messageService,
          IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _identityOptions = identityOptions;
            _authorizationService = authorizationService;
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet("users/me")]
        [Produces(typeof(UserViewModel))]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                ApplicationUser appUser = await _userManager.FindByNameAsync(this.User.Identity.Name);

                if (appUser == null)
                    return NotFound(this.User.Identity.Name);

                var userVM = _mapper.Map<UserViewModel>(appUser);
                userVM.Roles = new string[0];

                if (userVM != null)
                    return Ok(userVM);
                else
                    return NotFound(appUser.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = ex.Message
                });
            }
        }


        [HttpPut("users/me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserEditViewModel user)
        {
            ApplicationUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (user == null)
                    return BadRequest($"{nameof(user)} cannot be null");


                if (appUser == null)
                    return NotFound(appUser.Id);
                _mapper.Map<UserViewModel, ApplicationUser>(user, appUser);

                var result = await _userManager.UpdateAsync(appUser);

                return Ok();

            }

            return BadRequest(ModelState);
        }

        [HttpPost("forgot_password"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = "user_not_exist"
                    });
                }
                if (!(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = "user_not_confirmed"
                    });
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = System.Web.HttpUtility.UrlEncode(code);

                var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/account/reset_password?email={System.Web.HttpUtility.UrlEncode(user.Email)}&code={code}";

                await _messageService.SendPasswordResetMessage(model.Email, $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return Ok();
                //return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            // return View(model);
            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.ServerError
            });
        }

        [HttpGet("reset_password"), Produces("application/json")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string code)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1)
            };

            Response.Cookies.Append("reset_token", code, options);
            Response.Cookies.Append("reset_email", email, options);

            return RedirectPermanent("/reset_password");
        }

        [HttpPost("reset_password"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Empty;
                foreach (var i in ModelState.Values)
                {
                    foreach (var e in i.Errors)
                    {
                        errors += $"{e.ErrorMessage}";
                    }
                }
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = errors
                });
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError
                });
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                Response.Cookies.Delete("reset_token");
                Response.Cookies.Delete("reset_email");

                return Ok();
            }
            else
            {
                var errors = string.Empty;

                foreach (var e in result.Errors)
                {
                    errors += $"{e.Description}";
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = errors
                });
            }

        }

        [HttpPost("change_password"), Produces("application/json")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                    });
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);
                if (!changePasswordResult.Succeeded)
                {
                    var errors = string.Empty;
                    foreach (var e in changePasswordResult.Errors)
                    {
                        errors += $"{e.Description}";
                    }
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = errors
                    });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok();
            }
            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.ServerError
            });
        }
    }
}
