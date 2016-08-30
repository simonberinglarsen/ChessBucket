(function (angular) {
    var app = angular.module('gameShowApp', []);
    app.controller('gameShowCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.boardHeader = '-';
        $scope.viewmodel = {};
        $scope.currentMove = {};
        $scope.halfMove = 0;
        $scope.isLoading = true;
        $scope.progressStyleBlack = { width: '50%' };
        $scope.progressStyleWhite = { width: '50%' };
        $scope.positionValue = 0;
        $scope.analysisMode = false;
        $http({ method: 'Get', url: '/Game/LoadGame', params: { 'id': globalViewModel.gameId} })
        .success(function (data) {
            $scope.viewmodel = data;
            $scope.showMove($scope.halfMove);
            $scope.isLoading = false;
        })
        .error(function (errorData) {
            var k = 8;
            $scope.isLoading = false;
        });
        $scope.showPrincipalVariation = function (halfMove) {
            $scope.variationHalfMove = halfMove;
            var currentVariationMove = $scope.bestMove.PrincipalVariation[halfMove];
            var fen = currentVariationMove.Fen;
            board.position(fen);
        }
        $scope.nextVariationMove = function () {
            if ($scope.variationHalfMove === $scope.bestMove.PrincipalVariation.length - 1)
                return;
            $scope.variationHalfMove++;
            $scope.showPrincipalVariation($scope.variationHalfMove);
        }
        $scope.prevVariationMove = function () {
            if ($scope.variationHalfMove <= 0) {
                $scope.showMove($scope.halfMove);
                return;
            }
            $scope.variationHalfMove--;
            $scope.showPrincipalVariation($scope.variationHalfMove);
        }
        $scope.showMove = function (halfMove) {
            $scope.variationHalfMove = -1;
            $scope.halfMove = halfMove;
            var evalMove = $scope.viewmodel.AnalyzedMoves[halfMove];
            var actualMove = evalMove.ActualMove || evalMove.BestMove;
            $scope.bestMove = evalMove.BestMove;
            $scope.actualMove = actualMove;
            $scope.positionValue = halfMove % 2 === 1 ? -actualMove.Value : actualMove.Value;
            var styleValue = $scope.positionValue;
            if (styleValue < -400)
                styleValue = -400;
            else if (styleValue > 400)
                styleValue = 400;

            styleValue += 400;

            $scope.progressStyleWhite.width = styleValue * 0.125 + '%';
            $scope.progressStyleBlack.width = (100 - styleValue * 0.125) + '%';

            var fen = actualMove.Fen;
            board.position(fen);
            $scope.boardHeader = evalMove.Description;
            $scope.analysisHeader = 'fine move. Computer suggests this plan:';
            if (evalMove.Category === 1)
                $scope.analysisHeader = 'Missed oppertunity:';
            else if (evalMove.Category === 2)
                $scope.analysisHeader = 'Text move (' + $scope.actualMove.MoveSan + ') was a blunder. Better was:';
            else if (evalMove.Category === 3)
                $scope.analysisHeader = 'Blunder exploited! good move.';
        }
        $scope.setAnalysisMode = function (enable) {
            $scope.analysisMode = enable;
        }
        $scope.prevMove = function () {
            if ($scope.halfMove === 0)
                return;
            $scope.halfMove--;
            $scope.showMove($scope.halfMove);
        }
        $scope.nextMove = function () {
            if ($scope.halfMove === $scope.viewmodel.AnalyzedMoves.length - 1)
                return;
            $scope.halfMove++;
            $scope.showMove($scope.halfMove);
        }

    }]);

})(window.angular);