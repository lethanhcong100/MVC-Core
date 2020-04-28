using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Xuatxu { get; set; }
        public string Loaihang { get; set; }
    }

    public class Account
    {
        public string token { get; set; }
        public IList<TodoItem> todoitem { get; set; }
    }
}
