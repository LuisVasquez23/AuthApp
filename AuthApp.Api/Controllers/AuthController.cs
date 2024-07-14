using AuthApp.Api.DTOs;
using AuthApp.Api.Services.JWT;
using AuthApp.Api.Services.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;

namespace AuthApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMailService _mailService;

        // Constructor
        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMailService mailService,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> SignUp(RegisterDTO registerDTO)
        {
            // check user has been registered already
            var exist = await _userManager.FindByEmailAsync(registerDTO.Email);
            // If the user exists return a BadRequest status as a result
            if (exist != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = $"{exist.Email} has been registered already!" });
            }

            // Generate a new IdentityUser object
            // Set EmailConfirmed to false and when email confirmed, it changes to the true
            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                EmailConfirmed = false
            };

            // Create a new user
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            // If the creation process is not succeeded return a BadRequest status
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Registeration failed" });
            }

            // Check if users has not been registered with role user, then the user will be added to the User role 
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Generate token
            var token = HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));

            // Generate Confirmation link
            var confirmationLink = new StringBuilder($"https://localhost:7290/api/Auth/confirmEmail?token={token}&userId={user.Id}");

            // Send Email *
            var status = _mailService.Send(user.Email, "Email Confirmation", confirmationLink.ToString(), false);
            // If the status was true, the email has been sent
            if (status)
            {
                return StatusCode(StatusCodes.Status201Created, new { message = "Confirmation link has been sent to your email address" });
            }

            // Otherwise, return a BadRequest as a result
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> SignIn(LoginDTO loginDTO)
        {
            // Check Password is correct
            var signInResult = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, false, false);

            if (signInResult.Succeeded)
            {
                // Check email confirmed
                var user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user.EmailConfirmed)
                {
                    // We will implement the other sections like JWTToken generator and email confirmation 
                    // Generate JWT token *
                    // Furthermore, IdentityServer is another option that is useful to generate JWT token
                    var token = _jwtTokenGenerator.GenerateToken(user);

                    // Return token and email as data to the result of the signin endpoint
                    return StatusCode(StatusCodes.Status200OK, new { email = loginDTO.Email, token = token });
                }
            }
            // The password or email is incorrect and return a BadRequest as a result of signin endpoint
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            // Find user
            var user = await _userManager.FindByIdAsync(userId);

            // If there is not a user or token is empty return NotFound as a result of ConfirmEmail endpoint
            if (user is null || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            if (user.EmailConfirmed)
            {
                return StatusCode(StatusCodes.Status201Created, new { message = "You already confirm your account" });
            }

            // check token and confirm
            var result = await _userManager.ConfirmEmailAsync(user, token);
            // If email confirmation process has been succeeded, generate a new token and pass it to the result
            if (result.Succeeded)
            {
                var Jwttoken = _jwtTokenGenerator.GenerateToken(user);
                return StatusCode(StatusCodes.Status200OK, new { email = user.Email, token = Jwttoken });
            }
            // otherwise, return Forbidden status as a result
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        [HttpGet("ResendConfirmationLink")]
        public async Task<IActionResult> ResendConfirmationLink(string email)
        {
            // Check email exists in the database
            var user = await _userManager.FindByEmailAsync(email);
            //If there is a user, execute the if block
            if (user is not null)
            {

                if (user.EmailConfirmed)
                {
                    return StatusCode(StatusCodes.Status201Created, new { message = "You already confirm your account" });
                }

                // Generate token
                var token = HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));

                // Generate Confirmation link
                var confirmationLink = new StringBuilder($"https://localhost:7290/api/Auth/confirmEmail?token={token}&userId={user.Id}");

                // Send Email 
                var status = _mailService.Send(user.Email, "Email Confirmation", confirmationLink.ToString(), false);
                // If the status was true, the email has been sent
                if (status)
                {
                    return StatusCode(StatusCodes.Status201Created, new { message = "Confirmation link has been sent to your email address" });
                }

            }
            // Otherwise, return a BadRequest as a result
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpGet("ResetPasswordLink")]
        public async Task<IActionResult> ResetPasswordLink(string email)
        {
            // check user exist
            var user = await _userManager.FindByEmailAsync(email);

            // If there is a user, execute the if block
            if (user is not null || user.EmailConfirmed != false)
            {
                // Generate token
                var token = HttpUtility.UrlEncode(await _userManager.GeneratePasswordResetTokenAsync(user));

                // Generate Reset Password Link
                var passwordResetLink = new StringBuilder($"https://localhost:7290/api/Auth/ResetPassword?token={token}&userId={user.Id}");
                // Send Email 
                var status = _mailService.Send(user.Email, "Reset Password", passwordResetLink.ToString(), false);
                // If the status was true, the email has been sent
                if (status)
                {
                    return StatusCode(StatusCodes.Status201Created, new { message = "Reset password link has been sent to your email address" });
                }
            }
            // Otherwise, return a BadRequest as a result
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string token, string userId, ResetPasswordDTO passwordDTO)
        {
            // check exist user
            var user = await _userManager.FindByIdAsync(userId);
            // If there is a user, execute the if block
            if (user is not null)
            {
                // Reset password based on the token that is comming through the url
                var result = await _userManager.ResetPasswordAsync(user, token, passwordDTO.Password);
                // If the result is succeeded then exucute the if block
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status201Created, new { message = "Your password has been updated." });
                }
            }
            // Otherwise, return a BadRequest as a result
            return StatusCode(StatusCodes.Status400BadRequest);
        }
    }
}
