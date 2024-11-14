using System;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using IdentityService.Models;
using IdentityService.Pages.Account.Register;
using System.Security.Claims;
using IdentityModel;

namespace IdentityService.Pages.Register;

[SecurityHeaders]
[AllowAnonymous]    
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    // Get access to the user manager...
    public Index(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public RegisterViewModel Input { get; set; }

    [BindProperty]
    public bool RegisterUserSuccess { get; set; }

    /// <summary>
    /// Handles the GET request to display the registration page.
    /// Initializes the Input model with the provided return URL.
    /// </summary>
    /// <param name="returnUrl">The URL to redirect to after registration.</param>
    /// <returns>A PageResult representing the registration page.</returns>
    public IActionResult OnGet(string returnUrl)
    {
        //Bind the property from the model
        this.Input = new RegisterViewModel { ReturnUrl = returnUrl };

        return Page();
    }

    /// <summary>
    /// This is an ASP.NET Core Razor Pages handler method that handles the POST request when a user submits a registration form.
    /// It creates a new user account if the form data is valid, sets the user's username, email, and full name, 
    /// and redirects to the same page with a success indicator if the creation is successful.
    /// </summary>
    /// <returns>An IActionResult</returns>
    /// 
    public async Task<IActionResult> OnPost()
    {
        if (this.Input.Button != "register") return Redirect("~/");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, Input.FullName)
                });

                this.RegisterUserSuccess = true;
            }
        }

        return Page();
    }
}

