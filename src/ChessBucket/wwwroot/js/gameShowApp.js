(function (angular) {
    var app = angular.module('gameShowApp', ['ui.bootstrap']);
    app.controller('gameShowCtrl', ['$scope', '$http', function ($scope, $http) {
        $scope.currentPosition = '';
        $scope.boardHeader = '-';
        $scope.viewmodel = {};
        $scope.currentMove = {};
        $scope.halfMove = 0;
        $scope.isLoading = true;
        $scope.progressStyleBlack = { width: '50%' };
        $scope.progressStyleWhite = { width: '50%' };
        $scope.positionValue = 0;
        $scope.selectedTab = 'Moves';
        $scope.tag = '';
        $scope.allTags = [];
        $http({ method: 'Get', url: '/Game/LoadGame', params: { 'id': globalViewModel.gameId } })
        .success(function (data) {
            $scope.viewmodel = data;
            $scope.showMove($scope.halfMove);
            $scope.isLoading = false;
        })
        .error(function (errorData) {
            var k = 8;
            $scope.isLoading = false;
        });
        $http({ method: 'Get', url: '/Game/GetTags', params: { 'gameId': globalViewModel.gameId } })
        .success(function (data) {
            $scope.allTags = data;
            $scope.tagStorageStatus = 'tags loaded';
        })
        .error(function (errorData) {
            $scope.tagStorageStatus = 'failed to load tags';
        });
        $scope.showPrincipalVariation = function (halfMove) {
            $scope.variationHalfMove = halfMove;
            var currentVariationMove = $scope.bestMove.PrincipalVariation[halfMove];
            var fen = currentVariationMove.Fen;
            $scope.setBoardFen(fen);
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
            $scope.setBoardFen(fen);
            $scope.boardHeader = evalMove.Description;
            $scope.analysisHeader = 'fine move. Computer suggests this plan:';
            if (evalMove.Category === 1)
                $scope.analysisHeader = 'Missed oppertunity:';
            else if (evalMove.Category === 2)
                $scope.analysisHeader = 'Text move (' + $scope.actualMove.MoveSan + ') was a blunder. Better was:';
            else if (evalMove.Category === 3)
                $scope.analysisHeader = 'Blunder exploited! good move.';
        }

        $scope.setBoardFen = function(fen) {
            board.position(fen);
            $scope.currentPosition = fen;
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
        $scope.addTag = function (tagName) {
            $scope.allTags.push(tagName);
            $scope.tagName = '';
            $scope.updateTags();
        }

        $scope.removeTag = function (index) {
            $scope.allTags.splice(index, 1);
            $scope.updateTags();
        }

        $scope.updateTags = function () {
            $scope.tagStorageStatus = 'updating tags';
            var data = { "gameId": globalViewModel.gameId, "tags": $scope.allTags };
            $http.post('/Game/UpdateTags', data, { headers: { 'Content-Type': 'application/json' } })
            .success(function (data) {
                $scope.tagStorageStatus = 'tag-update succeeded';
            })
            .error(function (errorData) {
                $scope.tagStorageStatus = 'tag-update failed';
            });
        }

        $scope.getTags = function (viewValue) {
            return $http.get('/Game/GetAllTags', { params: { 'filter': viewValue } }).then(function (response) {
                return response.data;
            });
        };

        $scope.modelOptions = {
            debounce: {
                default: 500,
                blur: 250
            },
            getterSetter: true
        };

    }]);



})(window.angular);