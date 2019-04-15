using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BangazonWorkForceManagement.Models;
using BangazonWorkForceManagement.Models.ViewModels;
using BangazonWorkForceManagement.Models.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkForceManagement.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Employees
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.FirstName AS EmployeeFirstName,
                                               e.Id AS EmployeeId,
                                               e.IsSupervisor AS IsSupervisor,
	                                           e.LastName AS EmployeeLastName,
                                               d.Budget AS DepartmentBudget,
	                                           d.Name AS DepartmentName,
                                               d.Id as DepartmentId
                                        FROM Employee e
                                        JOIN Department AS d on d.Id = e.DepartmentId";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("EmployeeFirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("EmployeeLastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Department = new Department
                            {  
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                                Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget"))
                            }                        
                        };

                        employees.Add(employee);
                    }
                    reader.Close();
                    return View(employees);
                }
            }
        }

        // GET: Employees/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" 
										SELECT e.FirstName AS EmployeeFirstName,
                                               e.Id AS EmployeeId,
                                               e.IsSupervisor AS IsSupervisor,
	                                           e.LastName AS EmployeeLastName,
                                               d.Budget AS DepartmentBudget,
	                                           d.Name AS DepartmentName,
                                               d.Id as DepartmentId,
											   c.Make as ComputerMake,
                                               c.Id as ComputerId,
											   c.PurchaseDate as ComputerPurchaseDate,
											   c.DecomissionDate as ComputerDecomissionDate,
											   c.Manufacturer as ComputerManufacturer,
                                               tp.Id as TrainingProgramId,
											   tp.Name as TrainingProgramName,
											   tp.StartDate as TrainingProgramStartDate,
											   tp.EndDate as TrainingProgramEndDate,
											   tp.MaxAttendees as TrainingProgramMaxAtendees
                                        FROM Employee e
                                        JOIN Department AS d on d.Id = e.DepartmentId
										LEFT JOIN ComputerEmployee AS ce on ce.EmployeeId = e.Id
                                                  AND ce.UnAssignDate IS NULL
										LEFT JOIN Computer AS c on c.Id = ce.ComputerId
										LEFT JOIN EmployeeTraining AS et on et.EmployeeId = e.Id
										LEFT JOIN TrainingProgram AS tp on tp.Id = et.TrainingProgramId
										WHERE e.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    EmployeeDetailViewModel employee = null;
                    while (reader.Read())
                    {
                        if (employee == null)
                        {
                            employee = new EmployeeDetailViewModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("EmployeeFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("EmployeeLastName")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Department = new Department
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                                    Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget"))
                                },
                            };
                            if (! reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                            {
                                employee.Computer = new Computer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                    PurchaseDate = reader.GetDateTime(reader.GetOrdinal("ComputerPurchaseDate")),
                                    DecommissionDate = reader.IsDBNull(reader.GetOrdinal("ComputerDecomissionDate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("ComputerDecomissionDate")),
                                    Make = reader.GetString(reader.GetOrdinal("ComputerMake")),
                                    Manufacturer = reader.GetString(reader.GetOrdinal("ComputerManufacturer"))
                                };
                            }
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {

                            if (!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")))
                            {
                                if (!employee.TrainingProgramList.Exists(x => x.Id == reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))))
                                {
                                    employee.TrainingProgramList.Add(
                                new TrainingProgram
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                    Name = reader.GetString(reader.GetOrdinal("TrainingProgramName")),
                                    StartDate = reader.GetDateTime(reader.GetOrdinal("TrainingProgramStartDate")),
                                    EndDate = reader.GetDateTime(reader.GetOrdinal("TrainingProgramEndDate")),
                                    MaxAttendees = reader.GetInt32(reader.GetOrdinal("TrainingProgramMaxAtendees"))
                                });
                                }

                            }
                        }
                    };
                    reader.Close();
                    return View(employee);
                }
            }
        }
        

        // GET: Employees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id,
                                               e.FirstName,
                                               e.LastName,
                                               e.IsSupervisor,
                                               e.DepartmentId,
                                               etp.TrainingProgramId,
                                               ce.ComputerId
                                          FROM Employee e 
                                               LEFT JOIN EmployeeTraining etp on e.Id = etp.EmployeeId
                                               LEFT JOIN ComputerEmployee ce on e.Id = ce.EmployeeId
                                                         AND ce.UnassignDate IS NULL
                                         WHERE e.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    var reader = cmd.ExecuteReader();

                    EmployeeEditViewModel viewModel = null;
                    while (reader.Read())
                    {
                        if (viewModel == null)
                        {
                            var employeeId = reader.GetInt32(reader.GetOrdinal("Id"));

                            viewModel = new EmployeeEditViewModel(
                                employeeId, _config.GetConnectionString("DefaultConnection"));

                            viewModel.Employee = new Employee
                            {
                                Id = employeeId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                            {
                                viewModel.Employee.CurrentComputerId = 
                                    reader.GetInt32(reader.GetOrdinal("ComputerId"));
                            }
                            else
                            {
                                viewModel.Employee.CurrentComputerId = 0;
                            }
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")))
                        {
                            viewModel.SelectedTrainingProgramIds.Add(
                                reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))
                            );
                        }
                    }
                    reader.Close();

                    if (viewModel == null)
                    {
                        return NotFound();
                    }

                    return View(viewModel);
                }
            }
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeEditViewModel viewModel)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Transaction????
                    //  https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/local-transactions
                    using (var transaction = conn.BeginTransaction())
                    {
                        cmd.Transaction = transaction;
                        try
                        {
                            // Employee
                            cmd.CommandText = @"UPDATE Employee 
                                                   SET FirstName = @FirstName,
                                                       LastName = @LastName,
                                                       IsSupervisor = @IsSupervisor,
                                                       DepartmentId = @DepartmentId
                                                 WHERE id = @EmployeeId";

                            cmd.Parameters.Add(new SqlParameter("@FirstName", viewModel.Employee.FirstName));
                            cmd.Parameters.Add(new SqlParameter("@LAstName", viewModel.Employee.LastName));
                            cmd.Parameters.Add(new SqlParameter("@IsSupervisor", viewModel.Employee.IsSuperVisor));
                            cmd.Parameters.Add(new SqlParameter("@DepartmentId", viewModel.Employee.DepartmentId));
                            cmd.Parameters.Add(new SqlParameter("@EmployeeId", id));
                            cmd.ExecuteNonQuery();

                            cmd.Parameters.Clear();

                            // Computer
                            cmd.CommandText = @"SELECT ComputerId 
                                                  FROM ComputerEmployee 
                                                 WHERE EmployeeId = @EmployeeId
                                                       AND UnassignDate IS NULL";
                            cmd.Parameters.Add(new SqlParameter("@EmployeeId", id));
                            var reader = cmd.ExecuteReader();

                            // If they don't have a computer, we use 0 as a default id value
                            //  so...if we got some data, use it, otherwise 0
                            var currentComputerId = reader.Read()
                                ? reader.GetInt32(reader.GetOrdinal("ComputerId"))
                                : 0;
                            reader.Close();
                            cmd.Parameters.Clear();

                            // Did their computer change?
                            if (currentComputerId != viewModel.Employee.CurrentComputerId)
                            {

                                cmd.CommandText = @"UPDATE ComputerEmployee
                                                       SET UnassignDate = SYSDATETIME()
                                                     WHERE EmployeeId = @EmployeeId AND UnassignDate IS NULL;";

                                // Do they have a new computer or did they just have their old one unassigned.
                                if (viewModel.Employee.CurrentComputerId != 0)
                                {
                                    cmd.CommandText += @" INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate)
                                                               VALUES (@EmployeeId, @ComputerId, SYSDATETIME());";
                                }

                                cmd.Parameters.Add(new SqlParameter("@EmployeeId", id));
                                cmd.Parameters.Add(new SqlParameter("@ComputerId", viewModel.Employee.CurrentComputerId));
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                            }


                            // Training Programs

                            // Delete everything, then rebuild...

                            cmd.CommandText = "DELETE FROM EmployeeTraining WHERE EmployeeId = @EmployeeId";
                            cmd.Parameters.Add(new SqlParameter("@EmployeeId", id));
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                                     VALUES (@EmployeeId, @TrainingProgramId)";
                            foreach (var trainingProgramId in viewModel.SelectedTrainingProgramIds)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("@EmployeeId", id));
                                cmd.Parameters.Add(new SqlParameter("@TrainingProgramId", trainingProgramId));
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return RedirectToAction(nameof(Index));
                        }
                        catch
                        {
                            transaction.Rollback();
                            return RedirectToAction(nameof(Edit), new { id = id });
                        }
                    }
                }
            }
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


    }
}