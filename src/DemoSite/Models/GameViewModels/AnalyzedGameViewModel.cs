using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoSite.Models.GameViewModels
{
    public class AnalyzedGameViewModel
    {
       
        public AnalyzedGameViewModel(Game gd)
        {
            this.AnalysisState = gd.AnalysisState.ToString();
            this.AnalysisInfo = gd.AnalysisInfo;
            this.Event = gd.Event;
            this.Site = gd.Site;
            this.Date = gd.Date;
            this.Round = gd.Round;
            this.White = gd.White;
            this.Black = gd.Black;
            this.WhiteElo = gd.WhiteElo;
            this.BlackElo = gd.BlackElo;
            this.Result = gd.Result;
        }

        public string BlackElo { get; set; }

        public string WhiteElo { get; set; }

        public string AnalysisState { get; set; }
        public string AnalysisInfo { get; set; }
        //The name of the tournament or match event.
        public string Event { get; set; }
        //The location of the event. This is in "City, Region COUNTRY" format, where COUNTRY is the three-letter International Olympic Committee code for the country. An example is "New York City, NY USA".
        public string Site { get; set; }
        //The starting date of the game, in YYYY.MM.DD form. "??" are used for unknown values.
        public string Date { get; set; }
        //The playing round ordinal of the game within the event.
        public string Round { get; set; }
        //The player of the white pieces, in "last name, first name" format.
        public string White { get; set; }
        //The player of the black pieces, same format as White.
        public string Black { get; set; }
        //The result of the game.This can only have four possible values: "1-0" (White won), "0-1" (Black won), "1/2-1/2" (Draw), or "*" (other, e.g., the game is ongoing).
        public string Result { get; set; }
        public List<AnalyzedMoveViewModel> AnalyzedMoves { get; set; }
    }
    public class AnalyzedMoveViewModel
    {
        public bool IsWhite { get; set; }
        public string Description { get; set; }
        public EvaluatedMoveViewModel BestMove { get; set; }
        public EvaluatedMoveViewModel ActualMove { get; set; }
        public int Category { get; set; }

        internal static AnalyzedMoveViewModel MapFrom(AnalyzedMove from, Board b)
        {
            AnalyzedMoveViewModel to = new AnalyzedMoveViewModel()
            {
                ActualMove = EvaluatedMoveViewModel.MapFrom(from.ActualMove, b),
                BestMove = EvaluatedMoveViewModel.MapFrom(from.BestMove, b),
                Category = from.Category,
                IsWhite = from.IsWhite
            };
            return to;
        }
    }

    public class EvaluatedMoveViewModel
    {
        public string Fen { get; set; }
        public string MoveSan { get; set; }
        public string MoveLan { get; set; }
        public int Value { get; set; }
        public int DeltaToBest { get; set; }
        public EvaluatedMoveViewModel[] PrincipalVariation { get; set; }

        internal static EvaluatedMoveViewModel MapFrom(EvaluatedMove from, Board b)
        {
            if (from == null) return null;
            string beforeFen = b.DumpFen();
            EvaluatedMoveViewModel to = new EvaluatedMoveViewModel() {
                DeltaToBest = from.DeltaToBest,
                MoveLan = from.MoveLan,
                MoveSan = from.MoveSan,
                Value = from.Value
            };
            if(from.PrincipalVariation != null)
            {
                List<EvaluatedMoveViewModel> pv = new List<EvaluatedMoveViewModel>();
                foreach(var pvMove in from.PrincipalVariation)
                {
                    pv.Add(EvaluatedMoveViewModel.MapFrom(pvMove, b));
                    b.DoMove(Move.FromLan(pvMove.MoveLan));
                }
                to.PrincipalVariation = pv.ToArray();
                b.Setup(beforeFen);
            }
            b.DoMove(Move.FromLan(from.MoveLan));
            to.Fen = b.DumpFen();
            b.Setup(beforeFen);
            return to;
        }
    }
}
