using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoSite.Models.GameViewModels
{
    public class GameSearchViewModel
    {
        public int CurrentPage { get; set; }
        public string SearchKeyword { get; set; }
        public List<SearchResultViewModel> SearchResults { get; set; }
    }

    public class SearchResultViewModel
    {
        public int GameId { get; set; }
        public int Year { get; set; }
        public string Tournament { get; set; }
        public string Round { get; set; }
        public string Black { get; set; }
        public int BlackRating { get; set; }
        public string White { get; set; }
        public int WhiteRating { get; set; }
        public string Result { get; set; }
    }
}
