using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SmartEventTicketing.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly SmartEventTicketing.Data.ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            SmartEventTicketing.Data.ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public List<SmartEventTicketing.Models.EventCategory> AllCategories { get; set; } = new List<SmartEventTicketing.Models.EventCategory>();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            public DateTime DateOfBirth { get; set; }

            [StringLength(250)]
            [Display(Name = "Address")]
            public string? Address { get; set; }

            [Display(Name = "Event Preferences")]
            public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            AllCategories = await _context.EventCategories.OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Assign Member role
                    await _userManager.AddToRoleAsync(user, "Member");

                    // Create the Member record linked to this Identity user
                    var member = new SmartEventTicketing.Models.Member
                    {
                        UserId = await _userManager.GetUserIdAsync(user),
                        FullName = Input.FullName,
                        DateOfBirth = Input.DateOfBirth,
                        Address = Input.Address,
                        RegistrationDate = DateTime.Now
                    };
                    _context.Members.Add(member);
                    await _context.SaveChangesAsync();

                    // Save event preferences
                    if (Input.SelectedCategoryIds != null && Input.SelectedCategoryIds.Any())
                    {
                        foreach (var categoryId in Input.SelectedCategoryIds)
                        {
                            _context.MemberPreferences.Add(new SmartEventTicketing.Models.MemberPreference
                            {
                                MemberId = member.MemberId,
                                CategoryId = categoryId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Sign in immediately (email confirmation is disabled in Program.cs)
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed — reload categories so the form still works
            AllCategories = await _context.EventCategories.OrderBy(c => c.CategoryName).ToListAsync();
            return Page();
        }
    }
}
