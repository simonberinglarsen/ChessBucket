(function (angular) {
    var app = angular.module('openingApp', ['ui.bootstrap']);
    app.controller('openingCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.line = [];
        $scope.nodeId = 0;
        $scope.currentFen = 'rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1';
        $scope.viewmodel = {};
        $scope.selectNode = function (parentId, move) {
            return $http.get('/Opening/TransitionsByNodeId', { params: { 'parentId': parentId } }).then(function (response) {
                $scope.nodeId = parentId;
                $scope.viewmodel = response.data;
                if ($scope.viewmodel.transitions[0].Parent === null) {
                    $scope.currentFen = 'rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1';
                } else {
                    $scope.currentFen = $scope.viewmodel.transitions[0].Parent.Fen;
                }
                $scope.setBoardFen($scope.currentFen);
                $scope.line.push({
                    'San': move,
                    'Id': parentId
                });
            });
        };
        $scope.setBoardFen = function (fen) {
            board.position(fen);
            $scope.currentPosition = fen;
        }
        $scope.revertLine = function (index) {
            var id = $scope.line[index].Id;
            var san = $scope.line[index].San;
            $scope.line.splice(index, $scope.line.length - index);
            $scope.selectNode(id, san);
        }
        $scope.selectNode(0);
    }]);
})(window.angular);