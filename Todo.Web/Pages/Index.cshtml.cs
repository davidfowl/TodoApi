using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly TodoApiClient _client;
        public IndexModel(TodoApiClient client)
        {
            _client = client;
        }

        public string? TodoToken { get; set; }

        public JsonArray Todos { get; set; }

        public async Task OnGet()
        {
            if (User.Identity is ClaimsIdentity { IsAuthenticated: true } identity)
            {
                var idToken = (await HttpContext.GetTokenAsync("id"))!;
                TodoToken = await _client.GetTokenAsync(idToken);

                Todos = await _client.GetTodosAsync(TodoToken);
            }
        }
    }
}