function Department($scope, $http) {
    $http.get('@Html.Raw(@Url.Action("GetDepartment", "Admin", new { idTimeSheet = @Model }))').success(function (data) {
        $scope.Departments = data;
        $scope.DepIdList = [];
        $scope.SetCurrentDepartment(null);
        $scope.CurrentDepartment = null;
        $('#deparmentsLoad').remove();
    });

    $scope.SetCurrentDepartment = function (idManagеDepartment) {
        if ($scope.DepIdList.indexOf(idManagеDepartment) === -1) {
            $scope.DepIdList.push(idManagеDepartment);
        }
        var deps = [];
        $scope.CurrentDepartment = null;
        angular.forEach($scope.Departments, function (dep) {
            if (dep.IdManagerDepartment == idManagеDepartment)
                deps.push(dep);
            if (dep.IdDepartment == idManagеDepartment) {
                $scope.CurrentDepartment = dep;
            }
        });
        $scope.CerrentDepartments = deps;
        $scope.SelectDepartment(deps[0]);
    };

    $scope.Search = function () {
        if ($scope.query != '') {
            $scope.CerrentDepartments = $scope.Departments;
        } else
            $scope.SetCurrentDepartment(null);
    };

    $scope.Back = function () {
        $scope.DepIdList.splice($scope.DepIdList.length - 1, 1);
        $scope.SetCurrentDepartment($scope.DepIdList[$scope.DepIdList.length - 1]);
    };

    $scope.BackHide = function () {
        return $scope.CurrentDepartment == null ? true : false;
    };

    $scope.SelectDepartment = function (department) {
        $scope.ClearCss();
        department.css = "active";
        $scope.SelectedDepartment = department;
        console.log(department.IdDepartment);
        $scope.GetApprovers(department.IdDepartment);
    };

    $scope.ClearCss = function () {
        angular.forEach($scope.Departments, function (dep) {
            dep.css = "";
        });
    };

    $scope.GetApprovers = function (idDepartment) {
        var link = '@Html.Raw(@Url.Action("GetApproverForDepartment", "Admin", new { id = "replaceId" }))';
        link = link.replace("replaceId", idDepartment);
        $http.get(link).success(function (data) {
            $scope.sdEmployees = data.DepartmetEmployees;
            $scope.EmployeesClone();
        });
    };

    $scope.EmployeesClone = function () {
        angular.forEach($scope.sdEmployees, function (sdEmployee) {
            if (sdEmployee.EmployeeLogin == null) {
                sdEmployee.EmployeeLogin = '';
                // sdEmployee.isEdit = '0';
            }
        });
        $scope.clEmployees = angular.copy($scope.sdEmployees);
    };

    $scope.IsEmployeesEquals = function (sdEmployee, clEmployee) {
        if (sdEmployee.IdEmployee == clEmployee.IdEmployee && sdEmployee.EmployeeLogin == clEmployee.EmployeeLogin)
            return true;
        return false;
        //return angular.equals(sdEmployee, clEmployee);
    };

    $scope.IsSaveDisable = function () {
        var result = true;
        angular.forEach($scope.sdEmployees, function (sdEmployee) {
            angular.forEach($scope.clEmployees, function (clEmployee) {
                if (sdEmployee.IdEmployee == clEmployee.IdEmployee) {
                    if (!$scope.IsEmployeesEquals(sdEmployee, clEmployee)) {
                        result = false;
                    }
                }
            });
        });
        return result;
    };

    $scope.GetState = function (empl) {
        var r = '';
        angular.forEach($scope.clEmployees, function (clEmployee) {
            if (empl.IdEmployee == clEmployee.IdEmployee) {
                if (!$scope.IsEmployeesEquals(empl, clEmployee)) {
                    r = 'glyphicon glyphicon-floppy-remove';
                }
            }
        });
        return r;
    };




    $scope.InitApprover = function (approver, employees) {
        if (approver == null) return null;
        var empl;
        angular.forEach(employees, function (employee) {
            if (approver.IdEmployee == employee.IdEmployee) {
                empl = employee;
                return;
            }
        });
        return empl;
    };

    $scope.DepartmentApproveHide = function () {
        if ($scope.sdEmployees == null || $scope.sdEmployees.length == 0)
            return true;
        return false;
    };

    $scope.GetEmployeeName = function (employee) {
        return employee.Surname + ' ' + employee.Name + ' ' + employee.Patronymic;
    };


    $scope.CanSetAsCurrentDepartment = function (idDepartment) {
        var result = true;
        angular.forEach($scope.Departments, function (dep) {
            if (dep.IdManagerDepartment == idDepartment)
                result = false;
        });
        return result;
    };

    $scope.SaveApprover = function (approver) {
        var link = '@Html.Raw(@Url.Action("SaveEmployeeLogin", "Admin", new { idEmployee = "idEmployeeReplace", employeeLogin = "employeeLoginReplace" }))';
        link = link.replace("idEmployeeReplace", approver.IdEmployee).replace("employeeLoginReplace", approver.EmployeeLogin);
        console.log(link);
        $http.get(link).success(function (data) {
            if (data.result) {
                console.log('approve save success');
                angular.forEach($scope.clEmployees, function (sdEmployee) {
                    if (sdEmployee.IdEmployee == approver.IdEmployee) {
                        sdEmployee.EmployeeLogin = approver.EmployeeLogin;
                    }
                });
            }
            else
                alert('При сохранении возникли проблемы, изменения не сохранены');
        });
    };

    $scope.Save = function () {
        angular.forEach($scope.sdEmployees, function (sdEmployee) {
            angular.forEach($scope.clEmployees, function (clEmployee) {
                if (sdEmployee.IdEmployee == clEmployee.IdEmployee) {
                    if (!$scope.IsEmployeesEquals(sdEmployee, clEmployee)) {
                        $scope.SaveApprover(sdEmployee);
                    }
                }
            });
        });
    };


};