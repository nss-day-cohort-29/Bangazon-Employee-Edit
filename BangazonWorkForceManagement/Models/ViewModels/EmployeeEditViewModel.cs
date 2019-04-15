using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BangazonWorkForceManagement.Models.ViewModels
{
    public class EmployeeEditViewModel
    {
        private readonly string _connectionString;

        // This constructor is used when ASP.NET creates an instance of this class 
        //  to be passed as a parameter to the "POST" Edit method.
        public EmployeeEditViewModel() { }

        //  inside the "GET" Edit method.
        /// <summary>
        /// This constructor is used when WE create an instance of this class
        ///  inside the "GET" Edit method.
        /// </summary>
        /// <param name="employeeId">The id of the employee being edited</param>
        /// <param name="connectionString">The connection string to our SQL Database</param>
        public EmployeeEditViewModel(int employeeId, string connectionString)
        {
            _connectionString = connectionString;

            AllDepartments = GetAllDepartments();
            AvailableTrainingPrograms = GetAvailableFutureTrainingPrograms(employeeId);
            AvailableComputers = GetAvailableComputersForEmployee(employeeId);
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public Employee Employee { get; set; }

        public List<int> SelectedTrainingProgramIds { get; set; } = new List<int>();
        public List<TrainingProgram> AvailableTrainingPrograms { get; set; }
        public List<SelectListItem> AvailableTrainingProgramOptions
        {
            get
            {
                if (AvailableTrainingPrograms == null)
                {
                    return null;
                }

                return AvailableTrainingPrograms.Select(tp =>
                    new SelectListItem(tp.Name, tp.Id.ToString())
                ).ToList();
            }
        }

        public List<Department> AllDepartments { get; set; }
        public List<SelectListItem> AllDepartmentOptions
        {
            get
            {
                if (AllDepartments == null)
                {
                    return null;
                }

                return AllDepartments.Select(d =>
                    new SelectListItem(d.Name, d.Id.ToString())
                ).ToList();
            }
        }

        public List<Computer> AvailableComputers { get; set; }
        public List<SelectListItem> AvalableComputerOptions 
        {
            get
            {
                if (AvailableComputers == null)
                {
                    return null;
                }

                var options = AvailableComputers.Select(c =>
                    new SelectListItem($"{c.Manufacturer} {c.Make}", c.Id.ToString())
                ).ToList();

                // Insert a blank record for unassigning a computer without assigning a new computer.
                options.Insert(0, new SelectListItem("--No Computer--", "0"));

                return options;
            }
        }

        /// <summary>
        ///  Get all the departments in the database
        ///   Used to populate the Department select box on the Edit view
        /// </summary>
        /// <returns>All the Departments</returns>
        private List<Department> GetAllDepartments()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Budget FROM Department";
                    var reader = cmd.ExecuteReader();

                    var departments = new List<Department>();
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                        });
                    }
                    reader.Close();
                    return departments;
                }
            }
        }

        /// <summary>
        ///  Gets all the Training Programs that this employee MIGHT be enrolled in.
        /// </summary>
        /// <param name="employeId"></param>
        /// <returns>
        ///  Training programs that meet the following criteria:
        ///   In the future
        ///    AND
        ///   Is NOT full (enrollment has not exceeded MaxAttendees)
        ///    OR
        ///   In which the given Employee is ALREADY enrolled
        /// </returns>
        private List<TrainingProgram> GetAvailableFutureTrainingPrograms(int employeId)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DISTINCT tp.Id, tp.Name, tp.MaxAttendees, tp.StartDate, tp.EndDate
                                          FROM TrainingProgram tp
                                               LEFT JOIN (SELECT TrainingProgramId, COUNT(*) AS AttendeeCount
                                                            FROM EmployeeTraining
                                                        GROUP BY TrainingProgramId) tpCount
                                                      ON tp.id = tpCount.TrainingProgramId
                                               LEFT JOIN EmployeeTraining et ON tp.id = et.TrainingProgramId
                                         WHERE tp.StartDate > SYSDATETIME()  -- In the future
                                               AND (tpCount.AttendeeCount IS NULL  -- No one has enrolled
                                                    OR tpCount.AttendeeCount < tp.MaxAttendees  -- Has room for another another attendee
                                                    OR et.EmployeeId = @EmployeeId)  -- Is already being attended by this employee";
                    cmd.Parameters.Add(new SqlParameter("@EmployeeId", employeId));
                    var reader = cmd.ExecuteReader();

                    var trainingPrograms = new List<TrainingProgram>();
                    while (reader.Read())
                    {
                        trainingPrograms.Add(new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
                        });
                    }
                    reader.Close();
                    return trainingPrograms;
                }
            }
        }

        /// <summary>
        ///  Get all the computers that this Employee MAY be assigned.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>
        ///  Computers that meet the following criteria:
        ///   NOT Decommissioned
        ///    AND
        ///   NOT currently assigned to another employee
        ///    OR
        ///   Never assigned to anyone
        ///    OR
        ///   Currently assigned to this employee
        /// </returns>
        private List<Computer> GetAvailableComputersForEmployee(int employeeId)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, 
                                               c.Manufacturer, 
                                               c.Make,
                                               c.PurchaseDate, 
                                               c.DecomissionDate
                                          FROM Computer c LEFT JOIN ComputerEmployee ce ON c.Id = ce.ComputerId
                                          WHERE c.DecomissionDate IS NULL -- must still be in use
                                                AND (ce.EmployeeId IS NULL -- never been assigned
                                                     OR (ce.EmployeeId = @EmployeeId AND ce.UnassignDate IS NULL) -- This employee's current computer
                                                     OR (ce.ComputerId NOT IN (SELECT ComputerId FROM ComputerEmployee WHERE UnassignDate IS NULL))) -- Not currently assigned to someone else";
                    cmd.Parameters.Add(new SqlParameter("@EmployeeId", employeeId));
                    var reader = cmd.ExecuteReader();

                    var computers = new List<Computer>();
                    while (reader.Read())
                    {
                        computers.Add(new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                        });
                    }
                    reader.Close();
                    return computers;
                }
            }
        }
    }
}
