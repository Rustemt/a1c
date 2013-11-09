
var app = angular.module('main', ['ngResource']);
 
app.controller('GroupsCtrl', function ($scope, $http, $location) {

    $scope.save = function () {
        var payload = { id: $scope.id, content: $scope.yourName };
        $http.post('/tickets/postsave', payload)
            .success(function (data, status) {
                $scope.status = status;
                $scope.data = data;
                $scope.result = data;
            
                window.location = $location.path();
            }).
        error(function (data, status) {
            $scope.data = data || "Request failed";
            $scope.status = status;
        });
    };
});