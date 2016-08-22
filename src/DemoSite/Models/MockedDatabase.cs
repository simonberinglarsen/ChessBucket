using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace DemoSite.Models
{
    public class MockedDatabase
    {

        private static MockedDatabase instance = new MockedDatabase();
        private MockedDatabase()
        {
        }

        public static MockedDatabase Instance
        {
            get
            {
                return instance;
            }
        }


        public void Load()
        {
            MockedDatabase x = JsonConvert.DeserializeObject<MockedDatabase>(File.ReadAllText("jsonblob.txt"));
            Games = x.Games;

        }

        public void Save()
        {
            string x = JsonConvert.SerializeObject(this);
            File.WriteAllText("jsonblob.txt", x);
        }

        public List<GameData> Games { get; set; }
    }

  
}
