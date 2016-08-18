(function (angular) {
    var app = angular.module('boardApp', []);
    app.controller('testCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.boardHeader = '-';
        $scope.viewmodel = {};
        $scope.currentMove = {};
        $scope.halfMove = 0;
        $scope.isLoading = true;


        $http({ method: 'Get', url: '/Board/RandomGame', params: {} })
        .success(function (data) {
            $scope.viewmodel = data;
                $scope.showMove($scope.halfMove);
            $scope.isLoading = false;
        })
        .error(function (errorData) {
            var k = 8;
            $scope.isLoading = false;
        });

        $scope.showMove = function (halfMove) {
            $scope.halfMove = halfMove;
            var evalMove = $scope.viewmodel.AnalyzedMoves[halfMove];
            $scope.currentMove = evalMove;
            var actualMove = evalMove.AllMoves[evalMove.ActualMoveIndex];
            var fen = actualMove.Fen;
            board.position(fen);
            $scope.boardHeader = evalMove.Description;
            if (evalMove.Category === 1)
                $scope.boardHeader += '  (Missed Oppertunity)';
            else if (evalMove.Category === 2)
                $scope.boardHeader += '  (Blunder)';
            else if (evalMove.Category === 3)
                $scope.boardHeader += '  (Good Move)';
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