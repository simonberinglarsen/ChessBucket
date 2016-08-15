(function (angular) {
    var app = angular.module('boardApp', []);
    app.controller('testCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.boardHeader = '-';
        $scope.viewmodel = {};
        $scope.currentMove = {};
        $scope.halfMove = 1;

        
        $http({ method: 'Get', url: '/Board/RandomGame', params: {} })
        .success(function (data) {
            $scope.viewmodel = data;
        })
        .error(function (errorData) {
            var k = 8;
        });
        $scope.showMoveByNumberAndColor = function (moveNumber, isWhite) {
            $scope.showMove(moveNumber * 2 - isWhite);
        }
        $scope.showMove = function (halfMove) {
            $scope.halfMove = halfMove;
            var moveNumber = Math.floor((halfMove + 1) / 2);
            var isWhite = Boolean(1 - (halfMove + 1) % 2);
            var blackMoveText = '';
            var evalMove = $scope.viewmodel.EvaluatedMoves[moveNumber - 1];
            var move;
            if (isWhite)
                move = evalMove.White;
            else
                move = evalMove.Black;
            $scope.currentMove = move;
            board.position(move.AfterFen);
            if (!isWhite)
                blackMoveText = '- ';
            $scope.boardHeader = moveNumber + '. ' + blackMoveText + move.Move + '(' + move.AfterCentiPawns + ')';
        }
        $scope.prevMove = function () {
            if ($scope.halfMove === 1)
                return;
            $scope.halfMove--;
            $scope.showMove($scope.halfMove);
        }
        $scope.nextMove = function () {
            if ($scope.halfMove === $scope.viewmodel.EvaluatedMoves.length*2)
                return;
            $scope.halfMove++;
            $scope.showMove($scope.halfMove);
        }

    }]);

})(window.angular);