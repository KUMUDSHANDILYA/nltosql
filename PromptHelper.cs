public static class PromptHelper
{
    public static string Schema = @"
CREATE TABLE Departments (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100)
);

CREATE TABLE Employees (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100),
    Title NVARCHAR(100),
    DepartmentID INT FOREIGN KEY REFERENCES Departments(ID)
);

CREATE TABLE Salaries (
    ID INT PRIMARY KEY,
    EmployeeID INT FOREIGN KEY REFERENCES Employees(ID),
    Salary DECIMAL(10, 2),
    Date DATE
);

CREATE TABLE Projects (
    ID INT PRIMARY KEY,
    Name NVARCHAR(100),
    Budget DECIMAL(10,2)
);

CREATE TABLE EmployeeProjects (
    EmployeeID INT FOREIGN KEY REFERENCES Employees(ID),
    ProjectID INT FOREIGN KEY REFERENCES Projects(ID)
);
";
}