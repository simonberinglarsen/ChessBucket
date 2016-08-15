(function (angular) {
    var app = angular.module('movegenApp', []);
    app.controller('testCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.helloWorld = 'hej';
        $http({ method: 'Get', url: '/Board/RandomGame', params: { id: 12 } })
        .success(function (data) {
            var k = 8;
        })
        .error(function (errorData) {
            var k = 8;
        });
    }]);

})(window.angular);