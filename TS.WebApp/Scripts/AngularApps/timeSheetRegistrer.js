var app = angular.module('ngApp', ['ngRoute']);

app.config(function($routeProvider, $locationProvider) {
    //configure the routing rules here
    $routeProvider.when('/:year/:month', {
        controller: 'PagesCtrl'
    });
    //routing DOESN'T work without html5Mode
    //$locationProvider.html5Mode(true);
});

app.controller('PagesCtrl', function($rootScope, $scope, $routeParams, $route, $http) {

    $scope.timeSheetViewLink = function (ts) {
        return '/TsShow/Show/' + ts.IdTimeSheet;
    };

    $scope.departmentLink = function (dep) {
        return '/Main/TimeSheetList/' + dep.IdDepartment;
    };

    $scope.adFakeTs = function (dep) {
        var link = '/Register/AddFakeTimeSheet?idDep=' + dep.IdDepartment + '&year=' + $scope.nMounth.year() + '&mounth=' + $scope.nMounth.month();
        console.log(link);
        $http.post(link).success(
            function (result) {
                if (result.Result) {
                    console.log('add fake ts succes');
                    dep.timesheets.push(result.ts);
                } else {
                    console.log("Ошибка при созлании табеля");
                    alert("Ошибка при созлании табеля");
                }
            }
        );
    }

    $scope.dellFakeTs = function (dep) {
        var ts = dep.timesheets[0];
        var link = '/Register/DellFakeTimeSheet?id=' + ts.IdTimeSheet;
        console.log(link);
        $http.post(link).success(
            function (result) {
                if (result.Result) {
                    dep.timesheets.splice(0, 1);
                    console.log('dell fake ts succes');
                } else {
                    console.log("Ошибка при удалении табеля");
                    alert("Ошибка при удалении табеля");
                }
            }
        );
    }

    $scope.printDate = function (date) {
        console.log(date.format('L'));
    }

    $scope.loadData = function (year, mounth) {
        var url = '/Register/GetData?year=' + year + '&mounth=' + (mounth + 1);
        $scope.departments = null;
        console.log(url);
        $http.get(url).success(function (data) {
            $scope.departments = data.deps;
            $scope.dateString = data.dateString;
            console.log('data loaded');
        });
    };

    $scope.setDate = function (date) {
        $scope.date = date;
        $scope.printDate($scope.date);

        $scope.nMounth = date.clone().add('months', 1);
        $scope.pMounth = date.clone().add('months', -1);

        $scope.nextMounth = "/#/" + $scope.nMounth.year() + "/" + ($scope.nMounth.month()+1);
        $scope.previousMounth = "/#/" + $scope.pMounth.year() + "/" + ($scope.pMounth.month() + 1);

        $scope.loadData($scope.date.year(), $scope.date.month());
    }

    //======    Controller init     =======================

    $scope.setDate(moment());

    //If you want to use URL attributes before the website is loaded
    $rootScope.$on('$routeChangeSuccess', function () {
        $scope.setDate(moment([$routeParams.year, $routeParams.month - 1, 1]));
    });
});