(function (angular) {
    var app = angular.module('gameSearchApp', []);
    app.controller('gameSearchCtrl', ['$scope', '$http', '$window', function ($scope, $http, $window) {
        $scope.page = 0;
        $scope.searchText = '';
        $scope.isLoading = false;
        $scope.search = function () {
            $scope.isLoading = true;
            $http({ method: 'Get', url: '/Game/SearchGames', params: { 'searchText': $scope.searchText, 'page': $scope.page } })
            .success(function (data) {
                $scope.viewmodel = data;
                $scope.page = $scope.viewmodel.CurrentPage;
                $scope.isLoading = false;
            })
            .error(function (errorData) {
                var k = 8;
                $scope.isLoading = false;
            });
        }
        $scope.prev = function () {
            $scope.page--;
            $scope.search();

        }
        $scope.next = function () {
            $scope.page++;
            $scope.search();
        }
        $scope.selectGame = function (gameId) {
            $window.location.href = '/game/show/' + gameId;
        }
        $scope.search();
    }]);

})(window.angular);