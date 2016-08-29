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
        public string Black { get; set; }
        public string Date { get; set; }
        public string Event { get; set; }
        public int GameId { get; set; }
        public string Result { get; set; }
        public string Round { get; set; }
        public string Site { get; set; }
        public string White { get; set; }
        public string AnalysisState { get; set; }
        public string WhiteElo { get; set; }
        public string BlackElo { get; set; }
    }
}
