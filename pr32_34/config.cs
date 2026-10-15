using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace pr32_34
{
    public class config
    {
        public string con()
        {
            string h = ConfigurationManager.AppSettings["host"].ToString();
            string u = ConfigurationManager.AppSettings["uid"].ToString();
            string p = ConfigurationManager.AppSettings["pwd"].ToString();
            return $"host={h};uid={u};pwd={p}";
        }
    }
}
