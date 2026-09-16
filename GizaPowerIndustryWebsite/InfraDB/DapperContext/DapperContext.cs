using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GizaPowerIndustryWebsite.InfraDB.DapperContext
{
    public class DapperDBContext
    {
        private readonly string _connectionString;

        public DapperDBContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        { 

            // استخدم Connection String من الـ Session أو من appsettings.json
            _connectionString =   configuration.GetConnectionString("GizaPowerConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured");
            }
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

}

