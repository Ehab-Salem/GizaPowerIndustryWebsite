using Dapper;
using GizaPowerIndustryWebsite.InfraDB.DapperContext;
using GizaPowerIndustryWebsite.Models;
using GizaPowerIndustryWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WInfraDB.DisposableFunction;

namespace GizaPowerIndustryWebsite.Controllers
{
    public class PagesController : Controller
    {
        private readonly DapperDBContext _connections;
        public PagesController(DapperDBContext dapperDB)
        {
            _connections= dapperDB;
        }

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult About()
        {
            return View ();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult CSR ()
        {
            return View();
        }

        [ActionName("Sustainability")]
        public IActionResult Sustainability()
        {
            return View();
        }

       // [ActionName("Sustainable-Spply-Chain")]
        public IActionResult sustainableSupplyChain()
        {
            return View();
        }

        [ActionName("Quality-Assurance")]
        public IActionResult qualityAssurance()
        {
            return  View ();
        }

        public IActionResult QHSE() 
        { 
            return View();
        }

        public IActionResult Media()
        {
            return View();
        }

        public IActionResult News() 
        { 
         return View ();
        }

       
        public IActionResult NewsDetails() 
        { 
            return View();
        }

       
        public IActionResult JobDetails()
        {
            return View();
        }
        private record VacancyHeaderForApplyRow(int Id, string JobTitle, int DepartmentId, string Department);

        [HttpGet]
        [Route("Careers/Apply/{id:int}")]
        public async Task<IActionResult> ApplyJob(int id)
        {
            using IDbConnection connection = _connections.CreateConnection();

            var job = await connection.QuerySingleOrDefaultAsync<VacancyHeaderForApplyRow>(@"
        SELECT v.Id, v.JobTitle, v.DepartmentId, d.Name AS Department
        FROM  [vacancies] v
        JOIN  [Department] d ON d.Id = v.DepartmentId
        WHERE v.Id = @Id AND v.IsActive = 1;",
                new { Id = id });

            if (job == null)
            {
                return NotFound();
            }

            var model = new VacancyApplicationViewModel
            {
                VacancyId = job.Id,
                JobTitle = job.JobTitle,
                DepartmentId = job.DepartmentId,
                DepartmentName = job.Department
            };

            return View(model);
        }

        // True when the request was sent by the apply-sheet's fetch() call
        // on the Job Details page, so we can answer with JSON instead of a
        // redirect/HTML view.
        private bool IsAjaxRequest()
            => string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        [HttpPost]
        [Route("Careers/Apply/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyJob(int id, VacancyApplicationViewModel model)
        {
            bool isAjax = IsAjaxRequest();

            if (id != model.VacancyId)
            {
                if (isAjax)
                {
                    return BadRequest(new
                    {
                        success = false,
                        errors = new Dictionary<string, string[]>
                        {
                            [""] = new[] { "This application doesn't match the job you're viewing. Please refresh the page and try again." }
                        }
                    });
                }
                return BadRequest();
            }

            // The job's department is looked up here, from VacancyId, rather
            // than trusted from a posted field — the form only sends
            // VacancyId, never a DepartmentId/JobTitle/DepartmentName.
            using IDbConnection lookupConnection = _connections.CreateConnection();
            var job = await lookupConnection.QuerySingleOrDefaultAsync<VacancyHeaderForApplyRow>(@"
        SELECT v.Id, v.JobTitle, v.DepartmentId, d.Name AS Department
        FROM  [vacancies] v
        JOIN  [Department] d ON d.Id = v.DepartmentId
        WHERE v.Id = @Id AND v.IsActive = 1;",
                new { Id = id });

            if (job == null)
            {
                if (isAjax)
                {
                    return NotFound(new
                    {
                        success = false,
                        errors = new Dictionary<string, string[]>
                        {
                            [""] = new[] { "This job is no longer available." }
                        }
                    });
                }
                return NotFound();
            }

            model.JobTitle = job.JobTitle;
            model.DepartmentId = job.DepartmentId;
            model.DepartmentName = job.Department;

            // File-specific validation (data annotations can't check extension/size well)
            if (model.CvFile == null || model.CvFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.CvFile), "Please attach your CV.");
            }
            else
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(model.CvFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.CvFile), "Only PDF or Word documents are allowed.");
                }
                else if (model.CvFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.CvFile), "File size must not exceed 5 MB.");
                }
            }

            // DepartmentId/JobTitle/DepartmentName are set server-side above,
            // so ModelState errors from binding those (they're no longer
            // posted) should be cleared rather than blocking submission.
            ModelState.Remove(nameof(model.DepartmentId));
            ModelState.Remove(nameof(model.JobTitle));
            ModelState.Remove(nameof(model.DepartmentName));

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                    return BadRequest(new { success = false, errors });
                }

                return View(model);
            }

            byte[] cvBytes;
            using (var memoryStream = new MemoryStream())
            {
                await model.CvFile.CopyToAsync(memoryStream);
                cvBytes = memoryStream.ToArray();
            }

            using IDbConnection connection = _connections.CreateConnection();

            await connection.ExecuteAsync(@"
        INSERT INTO VacanciesCvs
            (VacancyId, DepartmentId, Name, Address, Phone, Email, Experience, ExpectedSalary, CurrentSalary, Cv, CvFileName)
        VALUES
            (@VacancyId, @DepartmentId, @Name, @Address, @Phone, @Email, @Experience, @ExpectedSalary, @CurrentSalary, @Cv, @CvFileName);",
                new
                {
                    model.VacancyId,
                    model.DepartmentId,
                    model.Name,
                    model.Address,
                    model.Phone,
                    model.Email,
                    model.Experience,
                    model.ExpectedSalary,
                    model.CurrentSalary,
                    Cv = cvBytes,
                    CvFileName = model.CvFile.FileName
                });

            var successMessage = $"Thanks {model.Name}, your application for \"{model.JobTitle}\" has been received.";

            if (isAjax)
            {
                return Json(new { success = true, message = successMessage });
            }

            TempData["ApplySuccess"] = successMessage;
            return RedirectToAction("JobDetails", new { id });
        }

        private async Task RepopulateJobHeaderAsync(VacancyApplicationViewModel model, int id)
        {
            using IDbConnection connection = _connections.CreateConnection();
            var job = await connection.QuerySingleOrDefaultAsync<VacancyHeaderForApplyRow>(@"
        SELECT v.Id, v.JobTitle, v.DepartmentId, d.Name AS Department
        FROM  [vacancies] v
        JOIN  [Department] d ON d.Id = v.DepartmentId
        WHERE v.Id = @Id;", new { Id = id });

            if (job != null)
            {
                model.JobTitle = job.JobTitle;
                model.DepartmentName = job.Department;
            }
        }
        [HttpGet]
        [Route("Careers")]
        public async Task<IActionResult> Careers()
        {
            using IDbConnection connection = _connections.CreateConnection();
            if (connection.State != ConnectionState.Open) connection.Open();

            // Only show department tabs for departments that actually have an
            // active opening — an empty tab is worse than no tab.
            var departmentRows = await connection.QueryAsync<string>($@"
                SELECT DISTINCT d.Name
                FROM  [Department] d
                INNER JOIN  [vacancies] v ON v.DepartmentId = d.Id
                WHERE v.IsActive = 1
                ORDER BY d.Name;");

            var vacancyRows = await connection.QueryAsync<VacancyCardRow>($@"
                SELECT v.Id, v.JobTitle, v.JobSummary, d.Name AS Department, v.Location,
                       et.Name AS EmploymentType, st.Name AS SeniorityLevel
                FROM  [vacancies] v
                JOIN  [Department] d ON d.Id = v.DepartmentId
                JOIN  [EmployementType] et ON et.Id = v.EmploymentTypeId
                JOIN  [SeniorityType] st ON st.Id = v.SeniorityLevelId
                WHERE v.IsActive = 1
                ORDER BY v.CreateDate DESC;");

            var model = new CareersViewModel
            {
                Departments = departmentRows
                    .Select(name => new DepartmentTabViewModel { Name = name, Slug = Slugify(name) })
                    .ToList(),
                Vacancies = vacancyRows
                    .Select(v => new VacancyCardViewModel
                    {
                        Id = v.Id,
                        JobTitle = v.JobTitle,
                        JobSummary = v.JobSummary,
                        Department = v.Department,
                        DepartmentSlug = Slugify(v.Department),
                        Location = v.Location,
                        EmploymentType = v.EmploymentType,
                        SeniorityLevel = v.SeniorityLevel
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        [Route("Careers/JobDetails/{id:int}")]
        public async Task<IActionResult> JobDetails(int id)
        {
            using IDbConnection connection = _connections.CreateConnection();
            if (connection.State != ConnectionState.Open) connection.Open();

            var header = await connection.QuerySingleOrDefaultAsync<VacancyDetailsRow>($@"
                SELECT v.Id, v.JobTitle, v.JobSummary, v.Location, v.DepartmentId,
                       d.Name AS Department, et.Name AS EmploymentType, st.Name AS SeniorityLevel
                FROM [vacancies] v
                JOIN  [Department] d ON d.Id = v.DepartmentId
                JOIN  [EmployementType] et ON et.Id = v.EmploymentTypeId
                JOIN  [SeniorityType] st ON st.Id = v.SeniorityLevelId
                WHERE v.Id = @Id AND v.IsActive = 1;",
                new { Id = id });

            if (header == null)
            {
                return NotFound();
            }

            var responsibilities = await connection.QueryAsync<string>($@"
                SELECT ResponsibilitiesTitle
                FROM  [vacanciesResponsibilities]
                WHERE vacanciesId = @Id
                ORDER BY ResponsibilitiesSerial;", new { Id = id });

            var requirements = await connection.QueryAsync<string>($@"
                SELECT RequirementsTitle
                FROM  [vacanciesRequirements]
                WHERE vacanciesId = @Id
                ORDER BY RequirementsSerial;", new { Id = id });

            var skills = await connection.QueryAsync<string>($@"
                SELECT SkillsTitle
                FROM  [vacanciesSkills]
                WHERE vacanciesId = @Id
                ORDER BY SkillsSerial;", new { Id = id });

            var languages = await connection.QueryAsync<VacancyLanguageDisplay>($@"
                SELECT vl.LanguageTitle AS Language, ll.Name AS Level
                FROM  [vacanciesLanguages] vl
                JOIN  [LanguageLevels] ll ON ll.Id = vl.LanguageLevelID
                WHERE vl.vacanciesId = @Id
                ORDER BY vl.LanguagesSerial;", new { Id = id });

            var related = await connection.QueryAsync<RelatedJobViewModel>($@"
                SELECT TOP 4 v.Id, v.JobTitle, v.Location, et.Name AS EmploymentType
                FROM  [vacancies] v
                JOIN  [EmployementType] et ON et.Id = v.EmploymentTypeId
                WHERE v.IsActive = 1 AND v.Id <> @Id AND v.DepartmentId = @DepartmentId
                ORDER BY v.CreateDate DESC;",
                new { Id = id, header.DepartmentId });

            var model = new VacancyDetailsViewModel
            {
                Id = header.Id,
                JobTitle = header.JobTitle,
                JobSummary = header.JobSummary,
                Department = header.Department,
                Location = header.Location,
                EmploymentType = header.EmploymentType,
                SeniorityLevel = header.SeniorityLevel,
                Responsibilities = responsibilities.ToList(),
                Requirements = requirements.ToList(),
                Skills = skills.ToList(),
                Languages = languages.ToList(),
                RelatedJobs = related.ToList()
            };

            return View(model);
        }

        private static string Slugify(string name)
        {
            var slug = name.ToLowerInvariant().Replace("&", "");
            slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
            return slug.Trim('-');
        }

        private record VacancyCardRow(int Id, string JobTitle, string JobSummary, string Department, string Location, string EmploymentType, string SeniorityLevel);
        private record VacancyDetailsRow(int Id, string JobTitle, string JobSummary, string Location, int DepartmentId,
            string Department, string EmploymentType, string SeniorityLevel);


        public IActionResult Description()
        {
            return View();
        }

        public IActionResult Partners()
        {
            return View();
        }
        //========================================== EPC Projects ==========================================
        public IActionResult Projects()
        {
            return View();
        }
        ///========================================= Cables and Wires ==========================================
        public IActionResult Products()
        {
            return View();
        }
        public IActionResult CablesWires()
        {
            return View();
        }
        public IActionResult CableDetails()
        {
            return View();
        }
        //==================================== Low Voltage ============================================
        public IActionResult LowVoltage()
        {
            return View();
        }
        public IActionResult LowVoltageFirstTable(string Short, string img)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM LowVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sql, new { ProductName = Short });
            // 2. Get ALL technical rows for this product short code
            string sqlTechnical = "SELECT * FROM LowVoltageSingleCoreNonSheathed WHERE productShort = @productShort";
            var technicalRows = connection.Query<TechnicalRowModel>(sqlTechnical, new { productShort = Short }).ToList();
            var groups = technicalRows
                .GroupBy(r => (r.ProductHeader ?? "").Trim() + " (" + (r.Type ?? "").Trim() + ")")
                .ToList();
            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = groups;          
            return View(product);
        }

        public IActionResult LowVoltageSecondTable(string Short, string img)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM LowVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sql, new { ProductName = Short });
            // 2. Get all technical rows for this product short code
            string sqlTech = "SELECT * FROM LowVoltageSingleCoreSheathed WHERE ProductShort = @ProductShort";
            var allRows = connection.Query<TechnicalRowModel>(sqlTech, new { ProductShort = Short }).ToList();

            // Group by conductor type
            var groups = allRows.GroupBy(r => r.CoductorType).ToList();

            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = groups;
            return View(product);
        }

        public IActionResult LowVoltageThirdTable(string Short, string img)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM LowVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sql, new { ProductName = Short });
            // 2. Get all technical rows for this product short code
            string sqlTech = "SELECT * FROM LowVoltageSingleCoreSheathed WHERE ProductShort = @ProductShort";
            var allRows = connection.Query<TechnicalRowModel>(sqlTech, new { ProductShort = Short }).ToList();

            // Group by conductor type
            var groups = allRows.GroupBy(r => r.CoductorType).ToList();

            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = groups;
            return View(product);
        }

        public IActionResult LowVoltageFourthTable(string Short, string img)
        {
            using var connection = _connections.CreateConnection();
            //remove the  Multi CORES from the short
            var ShortDesc = Short.Replace("Multi CORES", "").Trim();
            // 1. Product description
            string sqlDesc = "SELECT * FROM LowVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sqlDesc, new { ProductName = Short });

            // 2. Technical rows – group by Type (core count) instead of conductor type
            string sqlTech = "SELECT * FROM LowVoltageMultiCores WHERE ShortDescraption = @ShortDesc";
            var allRows = connection.Query<MultiCoreRowModel>(sqlTech, new { ShortDesc = ShortDesc }).ToList();

            // ⚠️ Group by Type (e.g., "Two Core", "Three Core")
            var groups = allRows.GroupBy(r => r.Type).ToList();

            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = groups;
            return View(product);
        }
        public IActionResult LowVoltageFiveTable(string Short, string img)
        {
            using var connection = _connections.CreateConnection();
            //remove the  Multi CORES from the short
            var ShortDesc = Short.Replace("Multi CORES", "").Trim();
            // 1. Product description
            string sqlDesc = "SELECT * FROM LowVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sqlDesc, new { ProductName = Short });

            // 2. Technical rows – group by Type (core count) instead of conductor type
            string sqlTech = "SELECT * FROM LowVoltageMultiCores WHERE ShortDescraption = @ShortDesc";
            var allRows = connection.Query<MultiCoreRowModel>(sqlTech, new { ShortDesc = ShortDesc }).ToList();

            // ⚠️ Group by Type (e.g., "Two Core", "Three Core")
            var groups = allRows.GroupBy(r => r.Type).ToList();

            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = groups;
            return View(product);
        }
        //==================================== Low Voltage ============================================
        public IActionResult MediumVoltage()
        {
            return View();
        }
        public IActionResult MediumVoltageFirstTable(string Short,string img,string img2)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM MediumVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<MediumVoltageModel>(sql, new { ProductName = Short });
            // 2. Technical rows – choose table based on product short
            string sqlTech = "";
            bool isSingleCore = Short.Contains("1 CORE");  // simple check
            if (isSingleCore)
                sqlTech = "SELECT * FROM MedimVoltageSingleCore WHERE ProductShort = @ProductShort ";
            else
                sqlTech = "SELECT * FROM MedimVoltageMultiCore WHERE ProductShort = @ProductShort ";

            var allRows = connection.Query<MediumTechincalModel>(sqlTech, new { ProductShort = Short }).ToList();
            var groups = allRows.GroupBy(r => r.CoductorType).ToList();

            ViewBag.ProductImage = img;
            ViewBag.ProductImage2= img2;
            ViewBag.TechnicalGroups = groups;

            return View(product);
        } 
        public IActionResult MediumVoltageSecondTable(string Short, string img, string img2)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM MediumVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<MediumVoltageModel>(sql, new { ProductName = Short });
            // 2. Technical rows – choose table based on product short
            string sqlTech = "";
            bool isSingleCore = Short.Contains("1 CORE");  // simple check
            if (isSingleCore)
                sqlTech = "SELECT * FROM MedimVoltageSingleCore WHERE ProductShort = @ProductShort";
            else
                sqlTech = "SELECT * FROM MedimVoltageMultiCore WHERE ProductShort = @ProductShort";

            var allRows = connection.Query<MediumTechincalModel>(sqlTech, new { ProductShort = Short }).ToList();
            var groups = allRows.GroupBy(r => r.CoductorType).ToList();

            ViewBag.ProductImage = img;
            ViewBag.ProductImage2 = img2;
            ViewBag.TechnicalGroups = groups;

            return View(product);
        }
        public IActionResult MediumVoltageThirdTable(string Short, string img, string img2)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM MediumVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<MediumVoltageModel>(sql, new { ProductName = Short });
            // 2. Technical rows – choose table based on product short
            string sqlTech = "";
            bool isSingleCore = Short.Contains("1 CORE");  // simple check
            if (isSingleCore)
                sqlTech = "SELECT * FROM MedimVoltageSingleCore WHERE ProductShort = @ProductShort ";
            else
                sqlTech = "SELECT * FROM MedimVoltageMultiCore WHERE ProductShort = @ProductShort ";

            var allRows = connection.Query<MediumTechincalModel>(sqlTech, new { ProductShort = Short }).ToList();
            var groups = allRows.GroupBy(r => r.CoductorType).ToList();

            ViewBag.ProductImage = img;
            ViewBag.ProductImage2 = img2;
            ViewBag.TechnicalGroups = groups;

            return View(product);
        }
        public IActionResult MediumVoltageFourthTable(string Short, string img, string img2)
        {
            using var connection = _connections.CreateConnection();
            string sql = "SELECT * FROM MediumVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<MediumVoltageModel>(sql, new { ProductName = Short });
            // 2. Technical rows – choose table based on product short
            string sqlTech = "";
            bool isSingleCore = Short.Contains("1 CORE");  // simple check
            if (isSingleCore)
                sqlTech = "SELECT * FROM MedimVoltageSingleCore WHERE ProductShort = @ProductShort ";
            else
                sqlTech = "SELECT * FROM MedimVoltageMultiCore WHERE ProductShort = @ProductShort ";

            var allRows = connection.Query<MediumTechincalModel>(sqlTech, new { ProductShort = Short }).ToList();
            var groups = allRows.GroupBy(r => r.CoductorType).ToList();

            ViewBag.ProductImage = img;
            ViewBag.ProductImage2 = img2;
            ViewBag.TechnicalGroups = groups;

            return View(product);
        }
        //====================================== HighVoltage =========================================
        public IActionResult HighVoltage()
        {
            return View();
        }
        public IActionResult HighVoltageDetailsFirstTable(string Short,string img)
        {
            using var connection = _connections.CreateConnection();
            //remove the  Multi CORES from the short
            var ShortDesc = Short.Replace("Multi CORES", "").Trim();
            // 1. Product description
            string sqlDesc = "SELECT * FROM HighVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sqlDesc, new { ProductName = Short });

            // 2. Technical rows – group by Type (core count) instead of conductor type
            string sqlTech = "SELECT * FROM HighVoltage WHERE ProductShort = @ProductShort";
            var allRows = connection.Query<HighVoltageModel>(sqlTech, new { ProductShort = Short }).ToList();


            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = allRows;
            return View(product);
        }
        public IActionResult HighVoltageDetailsSecondTable(string Short, string img)
        {
            using var connection = _connections.CreateConnection();
            //remove the  Multi CORES from the short
            var ShortDesc = Short.Replace("Multi CORES", "").Trim();
            // 1. Product description
            string sqlDesc = "SELECT * FROM HighVoltageDescription WHERE ProductName = @ProductName";
            var product = connection.QueryFirstOrDefault<LowVoltageModel>(sqlDesc, new { ProductName = Short });

            // 2. Technical rows – group by Type (core count) instead of conductor type
            string sqlTech = "SELECT * FROM HighVoltage WHERE ProductShort = @ProductShort";
            var allRows = connection.Query<HighVoltageModel>(sqlTech, new { ProductShort = Short }).ToList();


            ViewBag.ProductImage = img;
            ViewBag.TechnicalGroups = allRows;
            return View(product);
        }
        //====================================== HighVoltage =========================================

        //========================================= Conductors ==========================================
        public IActionResult OverHead()
        {
            return View();
        }
        public async Task<IActionResult> AAACAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where  ProductName='AAAC' ORDER BY CAST(Area AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> AACAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where  ProductName='AAC' ORDER BY CAST(Area AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> ABCAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where  ProductName='ABC' ORDER BY CAST(Area AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> ACSRAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where ProductName='ACSR' ORDER BY TRY_CAST(Area AS DECIMAL(10,2)) ASC,TRY_CAST(Areacmil AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> ACSRAWAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where ProductName='ACSR / AW' ORDER BY TRY_CAST(Area AS DECIMAL(10,2)) ASC,TRY_CAST(Areacmil AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> AACSRAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where ProductName='AACSR' ORDER BY TRY_CAST(Area AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> BareSoftAsync(string name, string img)
        {
            using var connection = _connections.CreateConnection();
            ViewBag.ProductName = name;
            ViewBag.ProductImage = img;
            var data = await connection.QueryAsync<BareModel>("select * from OverHead where StanderName ='60228' ORDER BY CAST(Area AS DECIMAL(10,2)) ASC");
            ViewBag.Data = data;
            return View();
        }
        public async Task<IActionResult> BareHardAsync()
        {
            using var connection = _connections.CreateConnection();

            var data = await connection.QueryAsync<BareModel>("select * from OverHead where StanderName in('DIN 48201-1' , 'BS 7884') ORDER BY CAST(Area AS DECIMAL(10,2)) ASC;");
            ViewBag.Data = data;

            return View();
        }
        //========================================= Meters ==========================================
        public IActionResult Meters() 
        {
            return View();
        }
        public IActionResult SinglePhaseMeters()
        {
            return View();
        }
        public IActionResult ThreePhaseMeters()
        {
            return View();
        }
        //========================================== Single Phase Meters ==========================================
        public IActionResult SinglePhasePrePaidConnected(string Name,string Img)
        {

            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        public IActionResult SinglePhasePrePaidEnergy(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        public IActionResult SinglePhaseSmart(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        //========================================== Three Phase Meters ==========================================
        public IActionResult ThreePhasePrePaidConnected(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }

        public IActionResult ThreePhaseSmartMedium(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        public IActionResult ThreePhaseSmartLow(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        public IActionResult ThreePhaseSmart(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        public IActionResult ThreePhaseSmartSTS(string Name, string Img)
        {
            ViewBag.Name = Name;
            ViewBag.Img = Img;
            return View();
        }
        public IActionResult MeterDetails()
        {
            return View();
        }
        //========================================= Lighting ==========================================
        public IActionResult Lighting()
        {
            return View();
        }
        // =========================== Aura Products ===========================
        public IActionResult Aura()
        {
            return View();
        }
        public IActionResult Led()
        {
            return View();
        }
        public IActionResult LedProducts()
        {
            return View();
        }
        public IActionResult Spot()
        {
            return View();
        }
        public IActionResult SpotProducts()
        {
            return View();
        }
        public IActionResult AuraTubes()
        {
            return View();
        }
        public IActionResult AuraTubesProducts()
        {
            return View();
        }
        // =========================== Polarix Products ===========================
        public IActionResult Polarix()
        {
            return View();
        }
        // =========================== Indoor Products ===========================
        public IActionResult IndoorLuminires()
        {
            return View();
        }
        public IActionResult FixedAlum()
        {
            return View();
        }
        public IActionResult FixedAlumProducts()
        {
            return View();
        }
        public IActionResult HighBay()
        {
            return View();
        }
        public IActionResult HighBayProdcuts()
        {
            return View();
        }
        public IActionResult LedPanels()
        {
            return View();
        }
        public IActionResult LedPanelsProducts()
        {
            return View();
        }
        public IActionResult MovableAlum()
        {
            return View();
        }
        public IActionResult MovableAlumProducts()
        {
            return View();
        }
        public IActionResult TriProof()
        {
            return View();
        }
        public IActionResult TriProofProducts()
        {
            return View();
        }
        // =========================== Outdoor Products ===========================
        public IActionResult OutdoorLuminires()
        {
            return View();
        }
        public IActionResult Sports()
        {
            return View();
        }
        public IActionResult SportsProducts()
        {
            return View();
        }
        public IActionResult Architectural()
        {
            return View();
        }
        public IActionResult ArchitecturalProducts()
        {
            return View();
        }
        public IActionResult Road()
        {
            return View();
        }
        public IActionResult RoadProducts()
        {
            return View();
        }
        public IActionResult Residential()
        {
            return View();
        }
        public IActionResult ResidentialProducts()
        {
            return View();
        }
        public IActionResult Spikes()
        {
            return View();
        }
        public IActionResult SpikesProducts()
        {
            return View();
        }
        public IActionResult Bollard()
        {
            return View();
        }
        public IActionResult BollardProducts()
        {
            return View();
        }
        public IActionResult Recreational()
        {
            return View();
        }
        public IActionResult RecreationalProducts()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Supplaychain()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/api/metals")]
        public async Task<IActionResult> GetMetalsPrices([FromServices] IMetalsService metalsService)
        {
            var data = await metalsService.GetRatesAsync();
            return Ok(data);
        }
    }
}
