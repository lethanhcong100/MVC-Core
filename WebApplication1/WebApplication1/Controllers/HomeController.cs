using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication1.Models;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StackExchange.Redis.IDatabase _database;

        public HomeController(ILogger<HomeController> logger, StackExchange.Redis.IDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        public IActionResult TrangChu()
        {
            // Để load trang login
            return View("Login");
        }


        string Baseurl = "https://localhost:44371/";
        public async Task<IActionResult> Create(TodoItem todoItem, string token)
        {
            // Để add thêm item
            object data = new
            {
                id = todoItem.Id,
                name = todoItem.Name,
                xuatxu = todoItem.Xuatxu,
                loaihang = todoItem.Loaihang
            };

            var payload = JsonConvert.SerializeObject(data);
            HttpContent c = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(Baseurl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage Res = await client.PostAsync("api/todoitems", c);
            // Sau khi add thì gọi về Index để load danh sách item
            // TempData["token"] = token;
            HttpContext.Session.SetString("token", token);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("token");
            // Để load danh sách item
            // Sẽ gọi đến Controller Action API để get danh sách item trả ra trang danh sách sản phẩm
            List<TodoItem> todoItem = new List<TodoItem>();
            Account account = new Account();
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(Baseurl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            HttpResponseMessage Res = await client.GetAsync("api/todoitems");

            if (Res.IsSuccessStatusCode)
            {
                using (HttpContent content = Res.Content)
                {
                    var nhan = await content.ReadAsStringAsync();
                    account = JsonConvert.DeserializeObject<Account>(nhan);
                }
            }
            ViewBag.Token = account.token;
            HttpContext.Session.Remove("token");
            HttpContext.Session.SetString("token", account.token);
            return View(account.todoitem);
        }

        public async Task<IActionResult> Delete(int id, string token)
        {
            // Để delete item
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(Baseurl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage Res = await client.DeleteAsync("api/todoitems/" + id);
            HttpContext.Session.SetString("token", token);
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> Login(User user)
        {
            // Sau khi nhấn nút đăng nhập thành công thì sẽ gọi đến Action Index kèm token để select danh sách item
            // Nếu đăng nhập fail thì return trang chủ login để đăng nhập lại
            object data = new
            {
                username = user.Username,
                password = user.Password
            };
            var payload = JsonConvert.SerializeObject(data);
            HttpContent c = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(Baseurl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage Res = await client.PostAsync("users/authenticate", c);
            if (Res.IsSuccessStatusCode)
            {
                using (HttpContent content = Res.Content)
                {
                    var nhan = await content.ReadAsStringAsync();
                    user = JsonConvert.DeserializeObject<User>(nhan);              
                    HttpContext.Session.SetString("token", user.Token);
                    return RedirectToAction("Index", "Home");
                }
            }
            else
                return RedirectToAction("TrangChu", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
