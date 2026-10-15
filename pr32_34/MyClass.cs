using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pr32_34
{
    static class MyClass
    {
        //static public string connectionStr = @"host=localhost;uid=root;pwd=root;database=pr32";
        static public string folderResources = Directory.GetCurrentDirectory() + @"\Resources\";

        static public string login, password;
        static public string id, fio, idrole, role;

        static public string selectedProduct;
        static public string deletedPhoto = "";

        static public Dictionary<string, int> Mypr37;

        static public Image OpenImage(string path)
        {
            Image img;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            img = Image.FromStream(fs);
            fs.Close();

            return img;
        }
    }
}
