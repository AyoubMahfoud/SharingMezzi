using Microsoft.AspNetCore.Mvc;
using SharingMezzi.Web.Services;

namespace SharingMezzi.Web.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly IAuthService _authService;

        public NavbarViewComponent(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isAuthenticated = _authService.IsAuthenticated();
            var currentUser = isAuthenticated ? await _authService.GetCurrentUserAsync() : null;

            var model = new NavbarViewModel
            {
                IsAuthenticated = isAuthenticated,
                CurrentUser = currentUser
            };

            return View(model);
        }
    }

    public class NavbarViewModel
    {
        public bool IsAuthenticated { get; set; }
        public Models.User? CurrentUser { get; set; }
    }
}
