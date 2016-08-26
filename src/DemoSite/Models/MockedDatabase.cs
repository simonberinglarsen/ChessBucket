using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace DemoSite.Models
{
    public class MockedDatabase2
    {

        private static MockedDatabase2 instance = new MockedDatabase2();
        private MockedDatabase2()
        {
        }

        public static MockedDatabase2 Instance
        {
            get
            {
                return instance;
            }
        }


        public void Load()
        {
            MockedDatabase2 x = JsonConvert.DeserializeObject<MockedDatabase2>(File.ReadAllText("jsonblob.txt"));
            Games = x.Games;

        }

        public void Save()
        {
            string x = JsonConvert.SerializeObject(this);
            File.WriteAllText("jsonblob.txt", x);
        }

        public List<Game> Games { get; set; }
    }

  
}
