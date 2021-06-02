using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Server.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Display(Name = "邮件地址")] public string Email { get; set; }
        [Display(Name = "用户名")] public string Username { get; set; }
        [Display(Name = "选手ID（南京大学学工号）")] public string ContestantId { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(20, ErrorMessage = "The {0} must be at most {1} characters long.")]
            [Display(Name = "选手名称")]
            public string ContestantName { get; set; }
        }

        private void LoadUser(ApplicationUser user)
        {
            Email = user.Email;
            Username = user.UserName;
            ContestantId = user.ContestantId;

            Input = new InputModel
            {
                ContestantName = user.ContestantName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            LoadUser(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                LoadUser(user);
                return Page();
            }

            var contestantName = user.ContestantName;
            if (Input.ContestantName != contestantName)
            {
                user.ContestantName = Input.ContestantName;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}