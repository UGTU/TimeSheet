var app = angular.module('ngApp', ['ngRoute']);

app.config(function($routeProvider, $locationProvider) {
    //configure the routing rules here
    $routeProvider.when('/:year/:month', {
        //url: "/Register/IndexNew",
        controller: 'PagesCtrl'
    });
    //routing DOESN'T work without html5Mode
    //$locationProvider.html5Mode(true);
});

app.controller('PagesCtrl', function($rootScope, $scope, $routeParams, $route, $http) {

    //=================================================================

    $scope.jsonDateToDate = function (date) {
        var myDate = new Date(date.match(/\d+/)[0] * 1);
        return myDate.toString('dd.MM');
    };

    $scope.timeSheetViewLink = function (ts) {
        var link = '@Url.Action("Show", "TsShow", new { id = "replace" })';
        return link.replace('replace', ts.IdTimeSheet);
    };

    $scope.departmentLink = function (dep) {
        var link = '@Url.Action("TimeSheetList", "Main", new { id = "replace" })';
        return link.replace('replace', dep.IdDepartment);
    };

    $scope.adFakeTs = function (dep) {
        var link = '@Html.Raw(Url.Action("AddFakeTimeSheet", "Register", new {idDep="replace", date = currentDate}))';
        link = link.replace('replace', dep.IdDepartment);
        $http.post(link).success(
            function (result) {
                if (result.Result) {
                    console.log('add fake ts succes');
                    $scope.loadData();
                } else {
                    console.log("Ошибка при созлании табеля");
                }
            }
        );
    }

    $scope.dellFakeTs = function (ts) {
        var link = '@Html.Raw(Url.Action("DellFakeTimeSheet", "Register", new {id="replace"}))';
        link = link.replace('replace', ts.IdTimeSheet);
        console.log(link);
        $http.post(link).success(
            function (result) {
                if (result.Result) {
                    console.log('dell fake ts succes');
                    $scope.loadData();
                } else {
                    console.log("Ошибка при удалении табеля");
                }
            }
        );
    }

    //=================================================================



    $scope.printDate = function (date) {
        console.log(date.format('L'));
    }

    $scope.loadData = function (year, mounth) {
        var url = '/Register/GetData1?year=' + year + '&mounth=' + (mounth + 1);
        $scope.departments = null;
        console.log(url);
        $http.get(url).success(function (data) {
            $scope.departments = data.deps;
            $scope.dateString = data.dateString;
            //angular.forEach($scope.departments, function (dep) {
            //    var tsList = [];
            //    angular.forEach(data.ts, function (ts) {
            //        if (dep.IdDepartment == ts.Department.IdDepartment)
            //            tsList.push(ts);
            //    });
            //    dep.timesheets = tsList;
            //});
            //console.log(data);
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