(function (angular) {
    var app = angular.module('gameUploadApp', []);
    app.controller('gameUploadCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.pgnText = '';
        $scope.uploadSuccess = false;
        $scope.uploadGame = function () {
            $scope.isLoading = true;
            $scope.uploadSuccess = false;
            $http({ method: 'Post', url: '/Game/ParseGame', data: { 'pgnText': $scope.pgnText } })
            .success(function (data) {
                $scope.viewmodel = data;
                $scope.isLoading = false;
                $scope.uploadSuccess = !($scope.viewmodel.Errors);
            })
            .error(function (errorData) {
                $scope.isLoading = false;
            });
        }
    }]);
})(window.angular);