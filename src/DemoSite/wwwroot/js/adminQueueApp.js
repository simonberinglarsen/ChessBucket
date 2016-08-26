(function (angular) {
    var app = angular.module('adminQueueApp', []);
    app.controller('adminQueueCtrl', ['$scope', '$http', '$window', function ($scope, $http, $window) {
        $scope.viewmodel = {};
        $scope.search = function () {
            $scope.isLoading = true;
            $http({ method: 'Get', url: '/Admin/SearchQueue', params: { 'page': 1 } })
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
        $scope.search();
    }]);

})(window.angular);