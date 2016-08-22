(function (angular) {
    var app = angular.module('gameUploadApp', []);
    app.controller('gameUploadCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.pgnText = '';
        $scope.uploadGame = function () {
            $http({ method: 'Get', url: '/Game/ParseGame', params: { 'pgnText': $scope.pgnText } })
            .success(function (data) {
                $scope.viewmodel = data;
                $scope.showMove($scope.halfMove);
                $scope.isLoading = false;
            })
            .error(function (errorData) {
                var k = 8;
                $scope.isLoading = false;
            });
        }
    }]);
})(window.angular);