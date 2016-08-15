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
      
        $scope.showMove = function (halfMove) {
            $scope.halfMove = halfMove;
            var evalMove = $scope.viewmodel.EvaluatedMoves[halfMove];
            $scope.currentMove = evalMove;
            board.position(evalMove.AfterFen);
            $scope.boardHeader = evalMove.Move;
        }
        $scope.prevMove = function () {
            if ($scope.halfMove === 0)
                return;
            $scope.halfMove--;
            $scope.showMove($scope.halfMove);
        }
        $scope.nextMove = function () {
            if ($scope.halfMove === $scope.viewmodel.EvaluatedMoves.length-1)
                return;
            $scope.halfMove++;
            $scope.showMove($scope.halfMove);
        }

    }]);

})(window.angular);