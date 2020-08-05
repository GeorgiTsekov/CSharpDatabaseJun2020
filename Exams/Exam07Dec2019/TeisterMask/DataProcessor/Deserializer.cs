namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using System.Text;
    using XMLHelper;
    using TeisterMask.DataProcessor.ImportDto;
    using TeisterMask.Data.Models;
    using System.Globalization;
    using TeisterMask.Data.Models.Enums;
    using Castle.Core.Internal;
    using Newtonsoft.Json;
    using System.Linq;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var sb = new StringBuilder();

            const string rootElement = "Projects";

            var dtoResult = XMLConverter.Deserializer<ProjectImportDto>(xmlString, rootElement);

            List<Project> projects = new List<Project>();

            foreach (var dto in dtoResult)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!IsDateValid(dto.OpenDate, "dd/MM/yyyy"))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime projectOpenDate = DateTimeFormated(dto.OpenDate, "dd/MM/yyyy");
                DateTime? projectDueDate;

                if (!dto.DueDate.IsNullOrEmpty())
                {
                    //if I do receive DueDate in XML
                    if (!IsDateValid(dto.DueDate, "dd/MM/yyyy"))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    projectDueDate = DateTimeFormated(dto.DueDate, "dd/MM/yyyy");
                }
                else
                {
                    //if I do not receive DueDate in XML
                    projectDueDate = null;
                }

                Project project = new Project
                {
                    Name = dto.Name,
                    OpenDate = projectOpenDate,
                    DueDate = projectDueDate,
                };

                foreach (var taskDto in dto.Tasks)
                {
                    if (!IsValid(taskDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (!IsDateValid(taskDto.OpenDate, "dd/MM/yyyy") 
                        || !IsDateValid(taskDto.DueDate, "dd/MM/yyyy"))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime taskOpenDate = DateTimeFormated(taskDto.OpenDate, "dd/MM/yyyy");
                    DateTime taskDueDate = DateTimeFormated(taskDto.DueDate, "dd/MM/yyyy");

                    if (taskOpenDate < project.OpenDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (projectDueDate.HasValue)
                    {
                        if (taskDueDate > projectDueDate.Value)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }
                    }

                    Task task = new Task
                    {
                        Name = taskDto.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = (ExecutionType)taskDto.ExecutionType,
                        LabelType = (LabelType)taskDto.LabelType,
                    };

                    project.Tasks.Add(task);
                }

                projects.Add(project);
                sb.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));
            }

            context.Projects.AddRange(projects);
            context.SaveChanges();
            return sb.ToString().TrimEnd();

            //foreach (var dto in dtoResult)
            //{
            //    var tasks = new List<Task>();

            //    if (IsValid(dto))
            //    {
            //        Project project;

            //        if (!dto.DueDate.IsNullOrEmpty())
            //        {
            //            project = new Project
            //            {
            //                Name = dto.Name,
            //                OpenDate = DateTime.ParseExact(dto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            //                DueDate = DateTime.ParseExact(dto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            //            };

            //            context.Projects.Add(project);
            //        }
            //        else
            //        {
            //            project = new Project
            //            {
            //                Name = dto.Name,
            //                OpenDate = DateTime.ParseExact(dto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            //            };

            //            context.Projects.Add(project);
            //        }

            //        context.SaveChanges();

            //        foreach (var dtoTask in dto.Tasks)
            //        {
            //            if (IsValid(dtoTask))
            //            {
            //                var task = new Task
            //                {
            //                    Name = dtoTask.Name,
            //                    OpenDate = DateTime.ParseExact(dtoTask.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            //                    DueDate = DateTime.ParseExact(dtoTask.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            //                    ExecutionType = (ExecutionType)dtoTask.ExecutionType,
            //                    LabelType = (LabelType)dtoTask.LabelType,
            //                    Project = project,
            //                };

            //                if (task.OpenDate.Ticks < project.OpenDate.Ticks)
            //                {
            //                    sb.AppendLine(ErrorMessage);
            //                    continue;
            //                }

            //                if (project.DueDate.HasValue)
            //                {
            //                    var dueDate = (DateTime)project.DueDate;

            //                    if (task.DueDate.Ticks > dueDate.Ticks)
            //                    {
            //                        sb.AppendLine(ErrorMessage);
            //                        continue;
            //                    }

            //                    tasks.Add(task);
            //                }
            //                else
            //                {
            //                    tasks.Add(task);
            //                }
            //            }
            //            else
            //            {
            //                sb.AppendLine(ErrorMessage);
            //            }
            //        }

            //        context.Tasks.AddRange(tasks);
            //        context.SaveChanges();

            //        sb.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, tasks.Count));
            //    }
            //    else
            //    {
            //        sb.AppendLine(ErrorMessage);
            //    }
            //}

            //return sb.ToString().TrimEnd();
        }

        public static bool IsDateValid(string date, string format)
        {
            DateTime currentDate;
            bool isCurrentDateValid = DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate);

            return isCurrentDateValid;
        }

        public static DateTime DateTimeFormated(string date, string format)
        {
            DateTime currentDate;
            bool isCurrentDateValid = DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentDate);

            return currentDate;
        }

        public static bool IsUsernameValid(string username)
        {
            foreach (var ch in username)
            {
                if (!Char.IsLetterOrDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var employeeDtos = JsonConvert.DeserializeObject<EmployeeDto[]>(jsonString);

            List<Employee> employees = new List<Employee>();

            foreach (var dto in employeeDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!IsUsernameValid(dto.Username))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Employee employee = new Employee
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    Phone = dto.Phone,
                };

                foreach (int taskId in dto.Tasks.Distinct())
                {
                    Task task = context
                        .Tasks
                        .FirstOrDefault(t => t.Id == taskId);

                    if (task == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    employee.EmployeesTasks.Add(new EmployeeTask()
                    {
                        Employee = employee,
                        Task = task,
                    });
                }

                employees.Add(employee);

                sb.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username, employee.EmployeesTasks.Count));
            }

            context.Employees.AddRange(employees);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

            //foreach (var dto in employeeDtos)
            //{
            //    if (IsValid(dto))
            //    {
            //        var employee = new Employee
            //        {
            //            Username = dto.Username,
            //            Email = dto.Email,
            //            Phone = dto.Phone,
            //        };

            //        context.Employees.Add(employee);

            //        context.SaveChanges();

            //        List<EmployeeTask> employeeTasks = new List<EmployeeTask>();

            //        foreach (var taskId in dto.Tasks.Distinct())
            //        {
            //            var employeeTask = context.Tasks
            //                    .FirstOrDefault(t => t.Id == taskId);

            //            if (employeeTask != null)
            //            {
            //                employeeTasks.Add(new EmployeeTask
            //                {
            //                    TaskId = taskId,
            //                    EmployeeId = employee.Id,
            //                    Employee = employee,
            //                });
            //            }
            //            else
            //            {
            //                sb.AppendLine(ErrorMessage);
            //            }
            //        }

            //        context.EmployeesTasks.AddRange(employeeTasks);

            //        context.SaveChanges();

            //        sb.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username, employeeTasks.Count));
            //    }
            //    else
            //    {
            //        sb.AppendLine(ErrorMessage);
            //    }
            //}
            //context.SaveChanges();

            //return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}